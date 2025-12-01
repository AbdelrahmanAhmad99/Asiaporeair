using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
 
namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the FlightLegDef entity.
    /// Responsibility: Reads flight leg definitions from a JSON file and seeds them into the database.
    /// It ensures ID consistency and respects foreign key constraints.
    /// </summary>
    public class FlightLegDefSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightLegDefSeeder> _logger;
        private const string JsonFileName = "FlightLegDef.json";
        private const string TableName = "flight_leg_def"; // Column name from Creation SingaporeDb.sql for IDENTITY reset

        public FlightLegDefSeeder(
            ApplicationDbContext context,
            ILogger<FlightLegDefSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the FlightLegDef data seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting FlightLegDef Seeding process...");

            try
            {
                // 1. Check if the table already has data to prevent duplicate seeding.
                if (await _context.Set<FlightLegDef>().AnyAsync())
                {
                    _logger.LogInformation("FlightLegDef table already contains data. Skipping seeding.");
                    return;
                }

                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 2. Read and Deserialize the JSON data using the helper method.
                var legDefsDto = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<FlightLegDefSeedDto>(JsonFileName, _logger);

                if (legDefsDto == null || !legDefsDto.Any())
                {
                    _logger.LogWarning("No data found in {JsonFileName}. Seeding skipped.", JsonFileName);
                    return;
                }

                _logger.LogInformation("Successfully deserialized {Count} records from {JsonFileName}.", legDefsDto.Count, JsonFileName);

                // 3. Convert DTOs to Entity objects.
                var legDefs = new List<FlightLegDef>();
                foreach (var dto in legDefsDto)
                {
                    legDefs.Add(new FlightLegDef
                    {
                        //LegDefId = dto.LegDefId,
                        ScheduleId = dto.ScheduleId,
                        SegmentNumber = dto.SegmentNumber,
                        DepartureAirportId = dto.DepartureAirportId,
                        ArrivalAirportId = dto.ArrivalAirportId,
                        IsDeleted = dto.IsDeleted
                    });
                }

                // 4. Add the entities to the DbContext.
                await _context.Set<FlightLegDef>().AddRangeAsync(legDefs);

                // 5. Save changes to the database.
                await _context.SaveChangesAsync();
                 

                _logger.LogInformation("FlightLegDef Seeding completed: Successfully seeded {Count} records.", legDefs.Count );
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during FlightLegDef seeding process from {JsonFileName}.", JsonFileName);
                throw;
            }
        }
    }
}