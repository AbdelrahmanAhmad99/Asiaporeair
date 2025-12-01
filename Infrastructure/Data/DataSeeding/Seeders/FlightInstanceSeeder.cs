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
    /// Professional Seeder for the FlightInstance entity.
    /// Responsibility: Reads flight instance data from JSON, maps it to the entity, and seeds the database.
    /// It ensures the IDENTITY counter is reset only if the table is empty.
    /// </summary>
    public class FlightInstanceSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<FlightInstanceSeeder> _logger;
        private const string JsonFileName = "FlightInstance.json";
        private const string TableName = "flight_instance"; // Table name in the database

        public FlightInstanceSeeder(
            ApplicationDbContext context,
            ILogger<FlightInstanceSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the FlightInstance data seeding process asynchronously.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting FlightInstance Seeding process from {JsonFile}...", JsonFileName);

            try
            {
                // 1. Check if the table already contains data. If so, skip seeding.
                if (await _context.Set<FlightInstance>().AnyAsync())
                {
                    _logger.LogInformation("FlightInstance table already contains data. Skipping seeding.");
                    return;
                }

                // 2. Reset the IDENTITY counter before seeding to ensure IDs start from 1.
                // This method ensures the reset happens only if the table is empty.
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 3. Read and deserialize data from the JSON file using the helper method.
                var dtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<FlightInstanceSeedDto>(JsonFileName, _logger);

                // 4. Map DTOs to the Entity and add to the context.
                var entities = dtos.Select(dto => new FlightInstance
                {
                    // No need to set InstanceId as it's an IDENTITY column
                    ScheduleId = dto.ScheduleId,
                    AircraftId = dto.AircraftId,
                    ScheduledDeparture = dto.ScheduledDeparture,
                    ActualDeparture = dto.ActualDeparture,
                    ScheduledArrival = dto.ScheduledArrival,
                    ActualArrival = dto.ActualArrival,
                    Status = dto.Status,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                await _context.Set<FlightInstance>().AddRangeAsync(entities);

                // 5. Save all changes to the database.
                await _context.SaveChangesAsync();

                _logger.LogInformation("FlightInstance Seeding completed: Successfully seeded {Count} records.", entities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FATAL ERROR during FlightInstance seeding process.");
                // It is professional to rethrow the exception to stop the application startup if seeding fails.
                throw;
            }
        }
    }
}