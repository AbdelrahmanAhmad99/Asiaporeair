using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
 
namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the Passenger entity.
    /// Responsibility: Reads Passenger data from a JSON file and seeds them into the database.
    /// It ensures ID consistency and respects the foreign key to the User table.
    /// </summary>
    public class PassengerSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PassengerSeeder> _logger;
        private const string JsonFileName = "Passenger.json";
        private const string TableName = "passenger"; // Table name for IDENTITY reset

        public PassengerSeeder(
            ApplicationDbContext context,
            ILogger<PassengerSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Passenger data seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting Passenger Seeding process from {JsonFileName}...", JsonFileName);

            try
            {
                // 1. Check if the table already contains data
                if (await _context.Set<Passenger>().AnyAsync())
                {
                    _logger.LogInformation("Passenger table already contains data. Skipping seeding.");
                    return;
                }

                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);


                // 2. Read and Deserialize JSON data using the professional helper method
                // We use ReadAndDeserializeJsonAsync from the helper class
                var passengersDto = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<PassengerSeedDto>(JsonFileName, _logger);

                if (passengersDto == null || !passengersDto.Any())
                {
                    _logger.LogWarning("No data found in {JsonFileName}. Skipping Passenger seeding.", JsonFileName);
                    return;
                }

                // 3. Map DTOs to Entity objects
                var passengers = new List<Passenger>(); 
                foreach (var dto in passengersDto)
                {
                    passengers.Add(new Passenger
                    { 
                        UserId = dto.UserId,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        DateOfBirth = dto.DateOfBirth,
                        PassportNumber = dto.PassportNumber,
                        IsDeleted = dto.IsDeleted
                    });
                }

                // 4. Add the entities to the DbContext.
                await _context.Set<Passenger>().AddRangeAsync(passengers);

                // 5. Save changes to the database.
                await _context.SaveChangesAsync(); 

                _logger.LogInformation("Passenger Seeding completed: Successfully seeded {Count} records.", passengers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Passenger seeding process from {JsonFileName}.", JsonFileName);
                throw;
            }
        }
    }
}