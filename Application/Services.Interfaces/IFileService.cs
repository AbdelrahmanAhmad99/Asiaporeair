using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Saves a file to a specified folder within the web root.
        /// </summary>
        /// <param name="file">The IFormFile to save.</param>
        /// <param name="folderName">The subfolder within 'wwwroot/uploads' (e.g., "ProfilePictures").</param>
        /// <returns>The relative URL path to the saved file (e.g., "/uploads/ProfilePictures/unique-name.jpg").</returns>
        /// <exception cref="ArgumentException">Thrown if file is null, empty, or invalid type/size.</exception>
        Task<string> SaveFileAsync(IFormFile file, string folderName);

        /// <summary>
        /// Deletes a file using its relative URL path.
        /// </summary>
        /// <param name="relativePath">The relative URL path (e.g., "/uploads/ProfilePictures/unique-name.jpg").</param>
        void DeleteFile(string relativePath);
    }
}