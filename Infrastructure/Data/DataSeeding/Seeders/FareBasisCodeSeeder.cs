using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the FareBasisCode entity.
    /// Responsibility: Populates the static fare basis codes required for ticket pricing and rules.
    /// </summary>
    public class FareBasisCodeSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<FareBasisCodeSeeder> _logger;
        private const string JsonFileName = "FareBasisCode.json";
        private const string TableName = "fare_basis_code"; // Entity table name

        public FareBasisCodeSeeder(
            ApplicationDbContext context,
            ILogger<FareBasisCodeSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the FareBasisCode data seeding process.
        /// Reads from JSON, checks for existing data, and inserts new reference records.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting {TableName} Seeding: Checking for existing data...", TableName);

            try
            {
                // 1. Check if data already exists to prevent duplicate seeding
                if (await _context.Set<FareBasisCode>().AnyAsync())
                {
                    _logger.LogInformation("{TableName} Seeding skipped: Data already exists.", TableName);
                    return;
                }

                // FareBasisCode (Code) is NOT an IDENTITY column, so no need to call ResetIdentityCounterAsync.

                // 2. Read and Deserialize JSON data using the centralized helper
                var fareBasisDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<FareBasisCodeSeedDto>(
                    JsonFileName,
                    _logger
                );

                if (fareBasisDtos.Count == 0)
                {
                    _logger.LogWarning("{JsonFileName} file was read but contained no records. Seeding stopped.", JsonFileName);
                    return;
                }

                _logger.LogInformation("Successfully read {Count} records from '{FileName}'.", fareBasisDtos.Count, JsonFileName);


                // 3. Map DTOs to Entity Model
                var fareBasisEntities = fareBasisDtos.Select(dto => new FareBasisCode
                {
                    Code = dto.Code,
                    Description = dto.Description,
                    Rules = dto.Rules,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 4. Add entities to the context for bulk insertion
                await _context.Set<FareBasisCode>().AddRangeAsync(fareBasisEntities);

                // 5. Commit changes to the database
                int seededCount = await _context.SaveChangesAsync();

                _logger.LogInformation("{TableName} Seeding completed successfully: Seeded {Count} records.", TableName, seededCount);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during {TableName} seeding process.", TableName);
                throw;
            }
        }
    }
}