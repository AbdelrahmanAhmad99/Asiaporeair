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
    ///  Seeder for the AircraftConfig entity.
    /// Responsibility: Seeds initial aircraft configuration data by reading from a JSON file.
    /// </summary>
    public class AircraftConfigSeeder
    { 
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AircraftConfigSeeder> _logger;
        private const string JsonFileName = "AircraftConfig.json";
        private const string TableName = "aircraft_config"; // Matches the database table name

        public AircraftConfigSeeder(
            ApplicationDbContext context,
            ILogger<AircraftConfigSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }
         
        public async Task SeedAsync()
        {
            _logger.LogInformation("Attempting to seed {TableName} data.", TableName);

            try
            {
                // 1. Check if the table already contains data to avoid duplication
                if (await _context.Set<AircraftConfig>().AnyAsync())
                {
                    _logger.LogInformation("{TableName} already contains data. Skipping seeding.", TableName);
                    return;
                }

                // 2. Reset the IDENTITY counter before bulk insert
                // This step ensures the ConfigId starts correctly from 1, essential for consistent primary key generation.
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);


                // 3. Read and deserialize JSON data using the centralized helper method
                var configDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AircraftConfigSeedDto>(
                    JsonFileName, _logger);

                if (configDtos == null || !configDtos.Any())
                {
                    _logger.LogWarning("No data found or deserialization failed for {JsonFileName}. Seeding aborted.", JsonFileName);
                    return;
                }
 
                // 4. Map DTOs to Entities
                var configEntities = configDtos.Select(dto => new AircraftConfig
                {
                    // ConfigId is IDENTITY and is handled by the database
                    AircraftId = dto.AircraftId,
                    ConfigurationName = dto.ConfigurationName,
                    TotalSeatsCount = dto.TotalSeatsCount,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 5. Add to context and save
                await _context.Set<AircraftConfig>().AddRangeAsync(configEntities);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} records into {TableName}.", configEntities.Count, TableName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during {TableName} seeding process. Details: {Message}", TableName, ex.Message);
                throw;
            }
        }
    }
}