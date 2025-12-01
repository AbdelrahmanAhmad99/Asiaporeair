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
    /// Professional Seeder for the FlightCrew entity (Many-to-Many join table).
    /// Responsibility: Associates CrewMembers (Pilots and Attendants) with specific FlightInstances.
    /// Note: This seeder assumes that CrewMember (Pilot/Attendant) and FlightInstance entities have already been seeded.
    /// </summary>
    public class FlightCrewSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightCrewSeeder> _logger;
        private const string JsonFileName = "FlightCrew.json";
        private const string TableName = "flight_crew"; // Assuming the table name is 'flight_crew'

        /// <summary>
        /// Initializes a new instance of the FlightCrewSeeder.
        /// </summary>
        public FlightCrewSeeder(ApplicationDbContext context, ILogger<FlightCrewSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the seeding process for FlightCrew records.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting FlightCrew seeding process...");

            try
            {
                // 1. Check if the table is already populated to prevent duplicate insertions
                if (await _context.Set<FlightCrew>().AnyAsync())
                {
                    _logger.LogInformation("FlightCrew table already contains data. Skipping seeding.");
                    return;
                }

                // Note: The FlightCrew table typically does not have an IDENTITY column,
                // so ResetIdentityCounterAsync is not needed for this join table.
                // It is kept here as a reminder but commented out based on the common structure of join tables. 

                // 2. Read and Deserialize JSON data using the professional helper method
                var crewDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<FlightCrewSeedDto>(
                    JsonFileName, _logger);

                // 3. Convert DTOs to Entity objects and track changes
                var flightCrewEntities = crewDtos.Select(dto => new FlightCrew
                {
                    FlightInstanceId = dto.FlightInstanceId,
                    CrewMemberId = dto.CrewMemberId,
                    Role = dto.Role,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 4. Add entities in bulk for performance
                await _context.AddRangeAsync(flightCrewEntities);
                await _context.SaveChangesAsync();

                _logger.LogInformation("FlightCrew Seeding completed: Successfully seeded {Count} records.", flightCrewEntities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during FlightCrew seeding process.");
                throw;
            }
        }
    }
}