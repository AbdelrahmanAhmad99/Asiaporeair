using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the AncillarySale entity.
    /// Responsibility: Reads ancillary sales data from a JSON file, resets the IDENTITY counter,
    /// and inserts the records into the database.
    /// </summary>
    public class AncillarySaleSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<AncillarySaleSeeder> _logger;
        private const string JsonFileName = "AncillarySale.json";
        private const string TableName = "ancillary_sale"; // The actual table name in the DB

        public AncillarySaleSeeder(
            ApplicationDbContext context,
            ILogger<AncillarySaleSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the seeding logic for AncillarySale data.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting AncillarySale Seeding process from {FileName}.", JsonFileName);

            try
            {
                // 1. Check if data already exists to prevent duplicate seeding
                if (await _context.Set<AncillarySale>().AnyAsync())
                {
                    _logger.LogInformation("AncillarySale table is not empty. Skipping seeding.");
                    return;
                }

                // 2. Read and deserialize data from the JSON file using the helper
                var salesDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AncillarySaleSeedDto>(JsonFileName, _logger);

                // 3. Reset the IDENTITY counter before insertion
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 4. Map DTOs to Entities
                var salesEntities = new List<AncillarySale>();
                foreach (var dto in salesDtos)
                {
                    salesEntities.Add(new AncillarySale
                    { 
                        BookingId = dto.BookingId,
                        ProductId = dto.ProductId,
                        SegmentId = dto.SegmentId, 
                        Quantity = dto.Quantity,
                        PricePaid = dto.PricePaid,
                        IsDeleted = dto.IsDeleted
                    });
                }

                // 5. Add all entities and save changes
                await _context.Set<AncillarySale>().AddRangeAsync(salesEntities);
                var seededCount = await _context.SaveChangesAsync();

                _logger.LogInformation("AncillarySale Seeding completed: Successfully seeded {Count} records.", seededCount);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during AncillarySale seeding process.");
                throw;
            }
        }
    }
}