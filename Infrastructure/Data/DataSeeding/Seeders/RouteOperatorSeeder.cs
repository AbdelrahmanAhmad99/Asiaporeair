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
    /// Professional Seeder for the RouteOperator entity (Junction Table between Route and Airline).
    /// Responsibility: Seeds the possible route/airline combinations from a JSON file.
    /// </summary>
    public class RouteOperatorSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<RouteOperatorSeeder> _logger;
        private const string JsonFileName = "RouteOperator.json";
        private const string TableName = "RouteOperator"; // The actual table name in the database

        public RouteOperatorSeeder(ApplicationDbContext context, ILogger<RouteOperatorSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the RouteOperator data seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            // Check if data already exists to prevent duplicate entries
            if (await _context.Set<RouteOperator>().AnyAsync())
            {
                _logger.LogInformation("RouteOperator table already contains data. Skipping seeding.");
                return;
            }

            try
            { 

                // 1. Read and Deserialize JSON Data using the dedicated helper method
                _logger.LogInformation("Starting RouteOperator seeding process by reading {JsonFile}...", JsonFileName);
                var routeOperatorDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<RouteOperatorSeedDto>(
                    JsonFileName, _logger);

                if (routeOperatorDtos == null || !routeOperatorDtos.Any())
                {
                    _logger.LogWarning("No data found in {JsonFileName}. RouteOperator seeding skipped.", JsonFileName);
                    return;
                }

                // 2. Map DTOs to Entities
                var routeOperatorEntities = routeOperatorDtos.Select(dto => new RouteOperator
                {
                    RouteId = dto.RouteId,
                    AirlineId = dto.AirlineId,
                    CodeshareStatus = dto.CodeshareStatus,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 3. Add entities to context and save
                await _context.Set<RouteOperator>().AddRangeAsync(routeOperatorEntities);
                await _context.SaveChangesAsync();
                 


                _logger.LogInformation("RouteOperator Seeding completed: Successfully seeded {Count} records.", routeOperatorEntities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during RouteOperator seeding process.");
                throw;
            }
        }
    }
}