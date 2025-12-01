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
    /// Professional Seeder for the ContextualPricingAttributes entity.
    /// Responsibility: Seeds the initial set of contextual attributes used by the dynamic pricing engine.
    /// </summary>
    public class ContextualPricingAttributesSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<ContextualPricingAttributesSeeder> _logger;
        private const string JsonFileName = "ContextualPricingAttributes.json";
        private const string TableName = "contextual_pricing_attributes";

        public ContextualPricingAttributesSeeder(ApplicationDbContext context, ILogger<ContextualPricingAttributesSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the ContextualPricingAttributes data seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            // Check if data already exists to prevent duplicate entries
            if (await _context.Set<ContextualPricingAttributes>().AnyAsync())
            {
                _logger.LogInformation("ContextualPricingAttributes table already contains data. Skipping seeding.");
                return;
            }

            try
            {

                // Reset Identity Counter (Required for tables with IDENTITY PK)
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);


                // 1. Read and Deserialize JSON Data using the dedicated helper method
                _logger.LogInformation("Starting ContextualPricingAttributes seeding process by reading {JsonFile}...", JsonFileName);
                var dtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<ContextualPricingAttributesSeedDto>(
                    JsonFileName, _logger);

                if (dtos == null || !dtos.Any())
                {
                    _logger.LogWarning("No data found in {JsonFileName}. ContextualPricingAttributes seeding skipped.", JsonFileName);
                    return;
                }

                // 2. Map DTOs to Entities
                var entities = dtos.Select(dto => new ContextualPricingAttributes
                {
                    TimeUntilDeparture = dto.TimeUntilDeparture,
                    LengthOfStay = dto.LengthOfStay,
                    CompetitorFares = dto.CompetitorFares,
                    WillingnessToPay = dto.WillingnessToPay,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 3. Add entities to context and save
                await _context.Set<ContextualPricingAttributes>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                 

                _logger.LogInformation("ContextualPricingAttributes Seeding completed: Successfully seeded {Count} records.", entities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during ContextualPricingAttributes seeding process.");
                throw;
            }
        }
    }
}