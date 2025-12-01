using Domain.Entities;
using Infrastructure.Data.DataSeeding.SeedDtos;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Dedicated seeder class for managing the data insertion of Aircraft entities.
    /// This process relies on the Airline and AircraftType tables being successfully seeded first.
    /// </summary>
    public class AircraftSeeder
    {
        private const string JsonFileName = "Aircraft.json";
        private const string TableName = "aircraft"; // The name of the table in the database
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AircraftSeeder> _logger;

        public AircraftSeeder(ApplicationDbContext context, ILogger<AircraftSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the initial list of Aircrafts into the database.
        /// </summary>
        public async Task SeedAsync()
        {
            // 1. Check if the table already contains data to prevent duplicate seeding
            if (await _context.Aircrafts.AnyAsync())
            {
                _logger.LogInformation("Aircraft table already contains data. Seeding skipped.");
                return;
            }

            try
            {
                _logger.LogInformation("Starting to seed {EntityName} data...", nameof(Aircraft));

                // NOTE: The primary key (tail_number) is a string and not an IDENTITY column.
                // ResetIdentityCounterAsync is not strictly necessary for this table. 

                // 2. Read and deserialize data from the JSON file
                var aircraftDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AircraftSeedDto>(JsonFileName, _logger);

                if (aircraftDtos == null || aircraftDtos.Count == 0)
                {
                    _logger.LogWarning("No {EntityName} data found in {FileName}. Seeding aborted.", nameof(Aircraft), JsonFileName);
                    return;
                }

                // 3. Map DTOs to Entity objects
                var aircrafts = new List<Aircraft>();
                foreach (var dto in aircraftDtos)
                {
                    aircrafts.Add(new Aircraft
                    {
                        TailNumber = dto.TailNumber,
                        AirlineId = dto.AirlineId,
                        AircraftTypeId = dto.AircraftTypeId,
                        TotalFlightHours = dto.TotalFlightHours,
                        AcquisitionDate = dto.AcquisitionDate,
                        Status = dto.Status,
                        IsDeleted = dto.IsDeleted
                    });
                }

                // 4. Add entities to the context and save changes
                await _context.Aircrafts.AddRangeAsync(aircrafts);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} {EntityName} records.", aircrafts.Count, nameof(Aircraft));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during {EntityName} seeding.", nameof(Aircraft));
                throw;
            }
        }
    }
}