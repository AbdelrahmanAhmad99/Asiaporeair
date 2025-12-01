 using Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices.FileService
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadsBaseDirectory;

        // Define allowed file types and max size for security
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long _maxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            // Files will be saved in 'wwwroot/uploads' so they can be served publicly
            _uploadsBaseDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(_uploadsBaseDirectory))
            {
                Directory.CreateDirectory(_uploadsBaseDirectory);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            // 1. Validation
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty.", nameof(file));
            }

            if (file.Length > _maxFileSizeInBytes)
            {
                throw new ArgumentException($"File size exceeds the limit of {_maxFileSizeInBytes / 1024 / 1024} MB.", nameof(file));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Invalid file type. Allowed types are: {string.Join(", ", _allowedExtensions)}", nameof(file));
            }

            // 2. Path Sanitization & Creation
            // Sanitize folderName to prevent path traversal attacks
            var sanitizedFolderName = Path.GetFileName(folderName);
            var targetDirectory = Path.Combine(_uploadsBaseDirectory, sanitizedFolderName);

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // 3. File Creation
            // Sanitize file name and create a unique name
            var sanitizedFileName = Path.GetFileNameWithoutExtension(file.FileName)
                                        .Replace(" ", "_");
            var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}";
            var absoluteFilePath = Path.Combine(targetDirectory, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Handle potential I/O errors
                throw new IOException($"Failed to save file: {ex.Message}", ex);
            }

            // 4. Return the relative URL path for use in <img> tags or APIs
            return $"/uploads/{sanitizedFolderName}/{uniqueFileName}";
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            try
            {
                // Sanitize the path to prevent deleting files outside the uploads directory
                var webPath = relativePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, webPath);

                // Security check: ensure the file is within the wwwroot
                if (!fullPath.StartsWith(_webHostEnvironment.WebRootPath))
                {
                    // Log this attempt
                    return;
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log the error (e.g., file in use, permissions)
                Console.WriteLine($"Error deleting file {relativePath}: {ex.Message}");
            }
        }
    }
}