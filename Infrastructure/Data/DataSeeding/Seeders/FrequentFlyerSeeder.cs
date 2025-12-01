 using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;  
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the FrequentFlyer table.
    /// Responsibility: Reads frequent flyer data from a JSON file and inserts it into the database.
    /// </summary>
    public class FrequentFlyerSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FrequentFlyerSeeder> _logger;
        private const string JsonFileName = "FrequentFlyer.json";

        public FrequentFlyerSeeder(ApplicationDbContext context, ILogger<FrequentFlyerSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        //  method to reset Identity counter only if the table is empty and we want to start from 1.
        private async Task ResetIdentityCounterAsync()
        {
            // Check if the database is SQL Server (since DBCC is specific to it)
            if (_context.Database.IsSqlServer())
            {
                // Use the RESEED command. Setting the seed value to 0 means the next inserted row will get ID 1.
                await _context.Database.ExecuteSqlRawAsync(
                    "IF NOT EXISTS(SELECT 1 FROM frequent_flyer) DBCC CHECKIDENT ('frequent_flyer', RESEED, 0)");
            }
        }
        /// <summary>
        /// Executes the data seeding operation for the FrequentFlyer table.
        /// </summary>
        public async Task SeedAsync()
        {
            // Professional check: Do not re-seed if the table already contains data.
            if (await _context.FrequentFlyers.AnyAsync())
            {
                _logger.LogInformation("FrequentFlyer seeding skipped: Table already contains data.");
                return;
            }

            try
            {
                //await ResetIdentityCounterAsync();
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, "frequent_flyer");
                _logger.LogInformation("Starting FrequentFlyer data seeding from {JsonFileName}...", JsonFileName);

                // 1. Read and Deserialize data using the centralized Helper
                var seedDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<FrequentFlyerSeedDto>(
                    JsonFileName, _logger);

                if (!seedDtos.Any())
                {
                    _logger.LogWarning("No data found in {JsonFileName}. Skipping insertion.", JsonFileName);
                    return;
                }

                // 2. Convert DTOs to Entities
                var entitiesToSeed = seedDtos
                    .Select(dto => new FrequentFlyer
                    {
                        CardNumber = dto.CardNumber,
                        Level = dto.Level,
                        AwardPoints = dto.AwardPoints,
                        IsDeleted = false
                    })
                    // Ensure CardNumber is trimmed and not null if DTO allows null (good practice)
                    .Where(e => !string.IsNullOrEmpty(e.CardNumber))
                    .ToList();

                // 3. Add data and save changes
                await _context.FrequentFlyers.AddRangeAsync(entitiesToSeed);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} FrequentFlyer records.", entitiesToSeed.Count);
            }
            catch (Exception ex)
            {
                // The Helper method should handle most file errors, but we catch others here.
                _logger.LogError(ex, "A critical error occurred during FrequentFlyer seeding.");
                throw;
            }
        }
    }
}