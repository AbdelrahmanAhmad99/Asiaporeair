using Domain.Entities;
using Infrastructure.Data.DataSeeding.Helpers;
using Infrastructure.Data.DataSeeding.SeedDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Seeds the FlightSchedule reference data into the database.
    /// This process ensures that flight schedules are consistently initialized.
    /// </summary>
    public class FlightScheduleSeeder
    { 
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightScheduleSeeder> _logger;

        private const string JsonFileName = "FlightSchedule.json";
        private const string TableName = "flight_schedule";
        public FlightScheduleSeeder(ApplicationDbContext context, ILogger<FlightScheduleSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Reads FlightSchedule data from JSON, resets the table identity, and seeds the database.
        /// </summary>
        public async Task SeedAsync()
        {
            if (await _context.FlightSchedules.AnyAsync())
            {
                _logger.LogInformation("FlightSchedule table already contains data. Seeding skipped.");
                return;
            }

            _logger.LogInformation("Starting FlightSchedule data seeding from {FileName}...", JsonFileName);

            try
            {
                 
                // 1. Read and Deserialize JSON Data
                var scheduleDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<FlightScheduleSeedDto>(JsonFileName, _logger);

                if (!scheduleDtos.Any())
                {
                    _logger.LogWarning("No FlightSchedule data found in {FileName}. Seeding aborted.", JsonFileName);
                    return;
                }

                // 2. Reset IDENTITY counter for SQL Server compatibility
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 3. Map DTOs to Entities
                var flightSchedules = scheduleDtos.Select(dto => new FlightSchedule
                {
                    //ScheduleId = dto.ScheduleId,
                    FlightNo = dto.FlightNo,
                    RouteId = dto.RouteId,
                    AirlineId = dto.AirlineId,
                    AircraftTypeId = dto.AircraftTypeId,
                    DepartureTimeScheduled = dto.DepartureTimeScheduled,
                    ArrivalTimeScheduled = dto.ArrivalTimeScheduled,
                    DaysOfWeek = dto.DaysOfWeek,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 4. Add to Context and Save
                await _context.FlightSchedules.AddRangeAsync(flightSchedules);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} FlightSchedule records.", flightSchedules.Count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding FlightSchedule data.");
                // Re-throw the exception to halt the seeding process if critical data fails
                throw;
            }
        }
    }
}