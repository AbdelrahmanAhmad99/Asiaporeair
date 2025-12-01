using Domain.Entities;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the Certification entity.
    /// Responsibility: Inserts training and license records for CrewMembers (Pilots and Attendants).
    /// </summary>
    public class CertificationSeeder
    {
        private readonly ApplicationDbContext _context; 
        private readonly ILogger<CertificationSeeder> _logger;
        private const string JsonFileName = "Certification.json";
        private const string CertificationTableName = "certification"; // Table name for IDENTITY reset

        public CertificationSeeder(
            ApplicationDbContext context,
            ILogger<CertificationSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Certification seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                // 1. Check if the table already contains data to avoid duplication.
                if (await _context.Set<Certification>().AnyAsync())
                {
                    _logger.LogInformation("Certification Seeding skipped: The table '{TableName}' already contains data.", CertificationTableName);
                    return;
                }

                // 2. Reset IDENTITY counter for 'certification' table (Ensures CertId starts at 1)
                // This is crucial for IDENTITY columns.
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, CertificationTableName);

                // 3. Read and Deserialize JSON Data
                var certificationDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<CertificationSeedDto>(JsonFileName, _logger);

                var certifications = new List<Certification>();

                foreach (var dto in certificationDtos)
                {
                    // 4. Map DTO to Entity
                    var certificationEntity = new Certification
                    {
                        CrewMemberId = dto.CrewMemberId, // PK/FK from CrewMember/Employee
                        Type = dto.Type,
                        IssueDate = dto.IssueDate,
                        ExpiryDate = dto.ExpiryDate,
                        IsDeleted = dto.IsDeleted
                    };
                    certifications.Add(certificationEntity);
                }

                // 5. Add all entities and save changes
                await _context.Set<Certification>().AddRangeAsync(certifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Certification Seeding completed: Successfully seeded {Count} records.", certifications.Count);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Certification seeding process.");
                throw;
            }
        }
    }
}