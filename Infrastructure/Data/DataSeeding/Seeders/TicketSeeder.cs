using Domain.Entities;
using Domain.Enums;  
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
    /// Professional Seeder for the Ticket entity.
    /// Responsibility: Reads Ticket data from JSON, converts it to entities, and inserts it into the database.
    /// </summary>
    public class TicketSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<TicketSeeder> _logger;
        private const string JsonFileName = "Ticket.json";
        private const string TableName = "ticket"; // SQL table name for IDENTITY reset

        public TicketSeeder(ApplicationDbContext context, ILogger<TicketSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Ticket data seeding process, adhering to foreign key constraints.
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting Ticket Seeding process for table '{TableName}'.", TableName);

                // 1. Check if the table already has data
                if (await _context.Set<Ticket>().AnyAsync())
                {
                    _logger.LogInformation("Table '{TableName}' already contains data. Skipping seeding.", TableName);
                    return;
                }


                // Reset the Identity Counter using the helper method
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

                // 2. Read and deserialize data from JSON file using the helper method
                var dtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<TicketSeedDto>(JsonFileName, _logger);

                if (dtos == null || !dtos.Any())
                {
                    _logger.LogWarning("No data found in JSON file '{JsonFileName}'. Skipping seeding.", JsonFileName);
                    return;
                }

                // 3. Convert DTOs to Entity objects
                var entities = dtos.Select(dto => new Ticket
                {
                    TicketCode = dto.TicketCode,
                    IssueDate = dto.IssueDate,
                    // Convert string status from JSON to the enum type
                    Status = Enum.Parse<TicketStatus>(dto.Status, true),
                    PassengerId = dto.PassengerId,
                    BookingId = dto.BookingId,
                    FlightInstanceId = dto.FlightInstanceId,
                    SeatId = dto.SeatId, // SeatId can be null
                    FrequentFlyerId = dto.FrequentFlyerId, // FrequentFlyerId can be null
                    IsDeleted = dto.IsDeleted
                }).ToList();

                // 4. Insert entities into the database
                await _context.Set<Ticket>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                 

                _logger.LogInformation("Ticket Seeding completed: Successfully seeded {Count} records for table '{TableName}'.", entities.Count, TableName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Ticket seeding process for table '{TableName}'.", TableName);
                throw;
            }
        }
    }
}