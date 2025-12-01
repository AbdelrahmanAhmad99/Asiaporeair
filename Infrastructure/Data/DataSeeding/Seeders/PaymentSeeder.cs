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
    /// Professional Seeder for the Payment entity.
    /// Responsibility: Reads payment data from a JSON file and seeds the database.
    /// Ensures data integrity and uses the shared data seeder helper.
    /// </summary>
    public class PaymentSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<PaymentSeeder> _logger;
        private const string JsonFileName = "Payment.json";
        private const string TableName = "payment"; // Table name for IDENTITY reset

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentSeeder"/> class.
        /// </summary>
        public PaymentSeeder(ApplicationDbContext context, ILogger<PaymentSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the payment seeding process asynchronously.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting Payment seeding process from '{JsonFile}'...", JsonFileName);

            try
            {
                // 1. Check if the table already contains any records to prevent duplicate seeding.
                if (await _context.Set<Payment>().AnyAsync())
                {
                    _logger.LogInformation("Payment table already contains data. Skipping seeding.");
                    return;
                }

                // 2. Read and deserialize the JSON data using the professional helper method.
                // The DTO ensures accurate mapping of JSON fields.
                List<PaymentSeedDto> paymentDtos =
                    await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<PaymentSeedDto>(JsonFileName, _logger);

                if (!paymentDtos.Any())
                {
                    _logger.LogWarning("No data found in '{JsonFile}'. Payment seeding aborted.", JsonFileName);
                    return;
                }

                // 3. Reset the IDENTITY counter for the Payment table (must run before insertion if table was previously truncated/empty).
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                _logger.LogInformation("Successfully deserialized {Count} Payment records. Starting database insertion...", paymentDtos.Count);

                // 4. Transform DTOs to Entities and add them to the context.
                int seededCount = 0;
                foreach (var dto in paymentDtos)
                {
                    var paymentEntity = new Payment
                    {
                        // Note: PaymentId (PK) is managed by the database IDENTITY column.
                        BookingId = dto.BookingId,              
                        Amount = dto.Amount,                    
                        Method = dto.Method,                    
                        TransactionDateTime = dto.TransactionDateTime,
                        Status = dto.Status,                    // 'Success' for confirmed bookings
                        TransactionId = dto.TransactionId,      
                        IsDeleted = dto.IsDeleted
                    };

                    _context.Set<Payment>().Add(paymentEntity);
                    seededCount++;
                }

                // 5. Final save for all new Payment records.
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment Seeding completed: Successfully seeded {Count} records.", seededCount);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Payment seeding process. Review Payment.json and dependencies.");
                throw;
            }
        }
    }
}