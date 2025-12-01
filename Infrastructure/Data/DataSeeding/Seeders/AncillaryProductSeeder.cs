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
    /// Professional Seeder responsible for populating the 'AncillaryProduct' reference table.
    /// Ancillary products (e.g., extra baggage, meal upgrades) are static offerings required before bookings.
    /// </summary>
    public class AncillaryProductSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<AncillaryProductSeeder> _logger;
        private const string JsonFileName = "AncillaryProduct.json";
        private const string TableName = "ancillary_product"; // SQL Table name for IDENTITY reset

        public AncillaryProductSeeder(
            ApplicationDbContext context,
            ILogger<AncillaryProductSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the AncillaryProduct data seeding process using JSON data.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting AncillaryProduct Seeding: Checking for existing data in '{TableName}' table...", TableName);

            // 1. Check if data already exists to prevent duplicate seeding
            if (await _context.Set<AncillaryProduct>().AnyAsync())
            {
                _logger.LogInformation("{TableName} Seeding skipped: Data already exists.", TableName);
                return;
            }

            try
            {
                // 2. Reset IDENTITY counter (Ensures ProductId starts from 1 if the table is empty)
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 3. Read and Deserialize JSON data using the centralized helper method
                var productDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AncillaryProductSeedDto>(JsonFileName, _logger);

                _logger.LogInformation("Successfully read {Count} ancillary product records from '{FileName}'.", productDtos.Count, JsonFileName);

                // 4. Map DTOs to Entity model
                var productEntities = productDtos.Select(dto => new AncillaryProduct
                {
                    Name = dto.Name,
                    Category = dto.Category,
                    BaseCost = dto.BaseCost,
                    UnitOfMeasure = dto.UnitOfMeasure,
                    IsDeleted = dto.IsDeleted // Should always be false for seeding
                }).ToList();

                // 5. Add entities to the context for bulk insertion
                await _context.Set<AncillaryProduct>().AddRangeAsync(productEntities);

                // 6. Save changes to the database
                int seededCount = await _context.SaveChangesAsync();

                _logger.LogInformation("AncillaryProduct Seeding completed: Successfully seeded {Count} records into '{TableName}'.", seededCount, TableName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during AncillaryProduct seeding process for '{TableName}'.", TableName);
                throw;
            }
        }
    }
}