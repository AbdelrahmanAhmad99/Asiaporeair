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
    /// Professional Seeder responsible for populating the 'AircraftType' reference table.
    /// It uses a JSON file for data consistency and professional helper methods for file IO and identity reset.
    /// </summary>
    public class AircraftTypeSeeder
    { 
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AircraftTypeSeeder> _logger;
        private const string JsonFileName = "AircraftType.json";
        private const string TableName = "aircraft_type"; // The database table name for IDENTITY RESET

        public AircraftTypeSeeder(
            ApplicationDbContext context,
            ILogger<AircraftTypeSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the professional seeding process for AircraftType table.
        /// </summary>
        public async Task SeedAsync()
        {
            // check: Stop seeding if the table already contains data.
            if (await _context.AircraftTypes.AnyAsync())
            {
                _logger.LogInformation("Skipping AircraftType Seeding: Table '{TableName}' already contains data.", TableName);
                return;
            }


            await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

            _logger.LogInformation("Starting AircraftType Seeding from file: {FileName}...", JsonFileName);

            try
            {
                // 1. Read and Deserialize JSON data using the professional helper method
                var aircraftTypes = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AircraftTypeSeedDto>(
                    JsonFileName,
                    _logger);

                if (aircraftTypes == null || !aircraftTypes.Any())
                {
                    _logger.LogWarning("AircraftType Seeding skipped: Deserialized data from {FileName} is null or empty.", JsonFileName);
                    return;
                }

                // 2. Map DTOs to Entity objects (Manual mapping for control)
                var aircraftTypeEntities = aircraftTypes.Select(dto => new AircraftType
                {
                    Model = dto.Model,
                    Manufacturer = dto.Manufacturer,
                    RangeKm = dto.RangeKm,
                    MaxSeats = dto.MaxSeats, 
                    CargoCapacity = dto.CargoCapacity,
                    CruisingVelocity = dto.CruisingVelocity,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 3. Add entities to the context
                await _context.AircraftTypes.AddRangeAsync(aircraftTypeEntities);

                // 4. Reset Identity Counter (Critical for fresh seeding after truncation)
                // Uses the dedicated professional method from JsonDataSeederHelper
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 5. Save changes to the database
                await _context.SaveChangesAsync();

                _logger.LogInformation("AircraftType Seeding completed: Successfully seeded {Count} records into '{TableName}'.", aircraftTypeEntities.Count, TableName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during AircraftType seeding process from file {FileName}.", JsonFileName);
                throw;
            }
        }
    }
}