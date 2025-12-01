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
    /// Professional Seeder for the BookingPassenger junction entity.
    /// Responsibility: Seeds the associations between Bookings and Passengers, including seat assignments.
    /// This entity uses a Composite Key (BookingId, PassengerId) and does not have an IDENTITY column to reset.
    /// </summary>
    public class BookingPassengerSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<BookingPassengerSeeder> _logger;
        private const string JsonFileName = "BookingPassenger.json";
        private const string EntityName = "BookingPassenger";
        // The table name is typically "BookingPassenger" if not specified with [Table] in the entity.
        // We do not need a table name constant as there is no IDENTITY reset.

        public BookingPassengerSeeder(
            ApplicationDbContext context,
            ILogger<BookingPassengerSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>booking_passenger
        /// Executes the BookingPassenger data seeding process asynchronously.
        /// </summary>
        public async Task SeedAsync()
        {
            // The check is complex due to the composite key, but AnyAsync is sufficient.
            if (await _context.Set<BookingPassenger>().AnyAsync())
            {
                _logger.LogInformation("{EntityName} seeding skipped: Records already exist.", EntityName);
                return;
            }

            try
            {
                // 1. Read and Deserialize JSON Data
                var bookingPassengerDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<BookingPassengerSeedDto>(
                    JsonFileName, _logger);

                if (!bookingPassengerDtos.Any())
                {
                    _logger.LogWarning("No {EntityName} data found in {FileName}. Seeding skipped.", EntityName, JsonFileName);
                    return;
                }

                // 2. Prepare Entities
                var bookingPassengerEntities = bookingPassengerDtos.Select(dto => new BookingPassenger
                {
                    BookingId = dto.BookingId,
                    PassengerId = dto.PassengerId,
                    SeatAssignmentId = dto.SeatAssignmentFk,  
                    IsDeleted = false  
                }).ToList();

                // 3. Add to Context and Save
                await _context.Set<BookingPassenger>().AddRangeAsync(bookingPassengerEntities);
                await _context.SaveChangesAsync();

                _logger.LogInformation("{EntityName} Seeding completed: Successfully seeded {Count} records.",
                    EntityName, bookingPassengerEntities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during {EntityName} seeding process.", EntityName);
                throw;
            }
        }
    }
}
