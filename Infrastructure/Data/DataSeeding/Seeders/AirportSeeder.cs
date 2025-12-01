using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the Airport entity, responsible for populating static airport data.
    /// It ensures data integrity by using external JSON files and the centralized data helper.
    /// </summary>
    public class AirportSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<AirportSeeder> _logger;
        private const string JsonFileName = "Airport.json";
        private const string TableName = "Airport"; // Entity table name

        public AirportSeeder(
            ApplicationDbContext context,
            ILogger<AirportSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Airport data seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            // 1. Check if data already exists to prevent duplicate seeding
            if (await _context.Set<Airport>().AnyAsync())
            {
                _logger.LogInformation("{TableName} Seeding skipped: Data already exists.", TableName);
                return;
            }

            try
            {
                _logger.LogInformation("Starting {TableName} Seeding process...", TableName);

                // **Note on ResetIdentityCounterAsync:**
                // The Airport table uses a string (IataCode) as its Primary Key and is NOT an IDENTITY column.
                // Therefore, calling JsonDataSeederHelper.ResetIdentityCounterAsync is not applicable for this table.

                // 2. Read and Deserialize JSON data using the centralized helper
                List<AirportSeedDto> airportDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AirportSeedDto>(
                    JsonFileName,
                    _logger
                );

                if (airportDtos.Count == 0)
                {
                    _logger.LogWarning("{JsonFileName} file was read but contained no records. Seeding stopped.", JsonFileName);
                    return;
                }

                // 3. Map DTOs to Entity Model
                var airports = airportDtos.Select(dto => new Airport
                {
                    IataCode = dto.IataCode,
                    IcaoCode = dto.IcaoCode,
                    Name = dto.Name,
                    City = dto.City,
                    CountryId = dto.CountryId, // Foreign Key
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    Altitude = dto.Altitude,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 4. Add entities to the context for bulk insertion
                _context.Set<Airport>().AddRange(airports);

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