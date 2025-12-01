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
    /// Professional Seeder for the Seat entity.
    /// Responsibility: Populates the seat table with detailed seating data loaded from a JSON file.
    /// Requires data from Aircraft and CabinClass to be present.
    /// </summary>
    public class SeatSeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<SeatSeeder> _logger;
        private const string JsonFileName = "Seat.json";
        private const string TableName = "seat"; // Table name for IDENTITY reset

        /// <summary>
        /// Initializes a new instance of the SeatSeeder class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger instance for professional logging.</param>
        public SeatSeeder(
            ApplicationDbContext context,
            ILogger<SeatSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Seat seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                // 1. Check if the table already contains data to prevent redundant seeding.
                if (await _context.Set<Seat>().AnyAsync())
                {
                    _logger.LogInformation("{TableName} table already contains data. Skipping seeding.", TableName);
                    return;
                }

                _logger.LogInformation("Starting {TableName} data seeding from JSON file: {FileName}", TableName, JsonFileName);

                // 2. Read and deserialize data from the JSON file using the professional helper.
                var dtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<SeatSeedDto>(
                    JsonFileName, _logger);

                if (!dtos.Any())
                {
                    _logger.LogWarning("No data found in {FileName}. Skipping {TableName} seeding.", JsonFileName, TableName);
                    return;
                }

                // PROFESSIONAL NOTE: The Seat table uses a string primary key (SeatId) 
                // and is not an IDENTITY column, so ResetIdentityCounterAsync is not applicable and is omitted. 

                // 3. Convert DTOs to Entity objects and add to the context.
                var entities = dtos.Select(dto => new Seat
                {
                    SeatId = dto.SeatId,
                    AircraftId = dto.AircraftId,
                    SeatNumber = dto.SeatNumber,
                    CabinClassId = dto.CabinClassId,
                    IsWindow = dto.IsWindow,
                    IsExitRow = dto.IsExitRow,
                    IsDeleted = dto.IsDeleted
                }).ToList();

                await _context.Set<Seat>().AddRangeAsync(entities);

                // 4. Save all changes to the database.
                await _context.SaveChangesAsync();

                _logger.LogInformation("Seat Seeding completed: Successfully seeded {Count} records into '{TableName}'.", entities.Count, TableName);
            }
            catch (System.Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during {TableName} seeding process from {FileName}. Ensure CabinClass and Aircraft tables are seeded first.", TableName, JsonFileName);
                throw;
            }
        }
    }
}