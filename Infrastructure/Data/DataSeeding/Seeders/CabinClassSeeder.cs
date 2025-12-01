using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the CabinClass entity.
    /// Responsibility: Populates the cabin_class table with data loaded from a JSON file.
    /// It ensures consistency by resetting the IDENTITY counter before seeding.
    /// </summary>
    public class CabinClassSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<CabinClassSeeder> _logger;
        private const string JsonFileName = "CabinClass.json";
        private const string TableName = "cabin_class"; // Table name for IDENTITY reset

        /// <summary>
        /// Initializes a new instance of the CabinClassSeeder class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger instance for professional logging.</param>
        public CabinClassSeeder(
            ApplicationDbContext context,
            ILogger<CabinClassSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the CabinClass seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                // 1. Check if the table already contains data to prevent redundant seeding.
                if (await _context.Set<CabinClass>().AnyAsync())
                {
                    _logger.LogInformation("{TableName} table already contains data. Skipping seeding.", TableName);
                    return;
                }

                _logger.LogInformation("Starting {TableName} data seeding from JSON file: {FileName}", TableName, JsonFileName);

                // 2. Read and deserialize data from the JSON file using the professional helper.
                var dtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<CabinClassSeedDto>(
                    JsonFileName, _logger);

                if (!dtos.Any())
                {
                    _logger.LogWarning("No data found in {FileName}. Skipping {TableName} seeding.", JsonFileName, TableName);
                    return;
                }

                // 3. Reset the IDENTITY counter for the table to ensure IDs start from 1.
                // This uses the professional helper method and is crucial for data consistency.
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 4. Convert DTOs to Entity objects and add to the context.
                var entities = dtos.Select(dto => new CabinClass
                {
                    // CabinClassId is IDENTITY, so it's not set here.
                    ConfigId = dto.ConfigId,
                    Name = dto.Name,
                    IsDeleted = dto.IsDeleted 
                }).ToList();

                await _context.Set<CabinClass>().AddRangeAsync(entities);

                // 5. Save all changes to the database.
                await _context.SaveChangesAsync();

                _logger.LogInformation("CabinClass Seeding completed: Successfully seeded {Count} records into '{TableName}'.", entities.Count, TableName);
            }
            catch (System.Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during {TableName} seeding process from {FileName}.", TableName, JsonFileName);
                throw;
            }
        }
    }
}