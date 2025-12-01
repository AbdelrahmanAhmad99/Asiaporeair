using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Seeder responsible for populating the 'country' reference table.
    /// This table is essential for defining airports and passenger nationalities.
    /// </summary>
    public class CountrySeeder
    {
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<CountrySeeder> _logger;
        private const string JsonFileName = "Country.json";
        private const string TableName = "country";

        public CountrySeeder(
            ApplicationDbContext context,
            ILogger<CountrySeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Country data seeding process.
        /// Reads from JSON, checks for existing data, and inserts new records.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting Country Seeding: Checking for existing data in '{TableName}' table...", TableName);

            // 1. Check if the table already contains data
            if (await _context.Countries.AnyAsync())
            {
                _logger.LogInformation("Country Seeding skipped: The '{TableName}' table already contains data.", TableName);
                return;
            }

            try
            {
                // **Important Note:** We do not call ResetIdentityCounterAsync here
                // because the primary key (IsoCode) is NOT an IDENTITY column. 

                // 2. Read and Deserialize JSON data
                var countryDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<CountrySeedDto>(JsonFileName, _logger);

                _logger.LogInformation("Successfully read {Count} country records from '{FileName}'.", countryDtos.Count, JsonFileName);

                // 3. Map DTOs to Entity and track for insertion
                var countryEntities = countryDtos.Select(dto => new Country
                {
                    IsoCode = dto.IsoCode,
                    Name = dto.Name,
                    Continent = dto.Continent,
                    IsDeleted = false  
                }).ToList();

                // 4. Add entities to the context
                await _context.Countries.AddRangeAsync(countryEntities);

                // 5. Save changes to the database
                await _context.SaveChangesAsync();

                _logger.LogInformation("Country Seeding completed: Successfully seeded {Count} records into '{TableName}'.", countryEntities.Count, TableName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Country seeding process for '{TableName}'.", TableName);
                throw;
            }
        }
    }
}