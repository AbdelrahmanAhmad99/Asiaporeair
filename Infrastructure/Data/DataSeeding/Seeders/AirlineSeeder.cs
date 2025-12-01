using Domain.Entities;
using Infrastructure.Data.DataSeeding.SeedDtos;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Dedicated seeder class for managing the data insertion of Airline entities.
    /// This process relies on the Airport table being successfully seeded first.
    /// </summary>
    public class AirlineSeeder
    {
        private const string JsonFileName = "Airline.json";
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AirlineSeeder> _logger;

        public AirlineSeeder(ApplicationDbContext context, ILogger<AirlineSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the initial list of Airlines into the database.
        /// </summary>
        public async Task SeedAsync()
        {
            // 1. Check if the table already contains data to prevent duplicate seeding
            if (await _context.Airlines.AnyAsync())
            {
                _logger.LogInformation("Airline table already contains data. Seeding skipped.");
                return;
            }

            try
            {
                _logger.LogInformation("Starting to seed {EntityName} data...", nameof(Airline));

                // 2. Read and deserialize data from the JSON file
                var airlineDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AirlineSeedDto>(JsonFileName, _logger);

                if (airlineDtos == null || airlineDtos.Count == 0)
                {
                    _logger.LogWarning("No {EntityName} data found in {FileName}. Seeding aborted.", nameof(Airline), JsonFileName);
                    return;
                }

                // 3. Map DTOs to Entity objects
                var airlines = new List<Airline>();
                foreach (var dto in airlineDtos)
                {
                    airlines.Add(new Airline
                    {
                        IataCode = dto.IataCode,
                        Name = dto.Name,
                        Callsign = dto.Callsign,
                        OperatingRegion = dto.OperatingRegion,
                        BaseAirportId = dto.BaseAirportId,
                        IsDeleted = dto.IsDeleted
                    });
                }

                // 4. Add entities to the context and save changes
                await _context.Airlines.AddRangeAsync(airlines);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} {EntityName} records.", airlines.Count, nameof(Airline));
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "FATAL: {FileName} not found during seeding.", JsonFileName);
                throw; // Re-throw to halt the overall seeding process
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during {EntityName} seeding.", nameof(Airline));
                // Optional: Re-throw based on project policy
                throw;
            }
        }
    }
}