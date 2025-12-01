using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the SuperAdmin role, handling AppUser, Employee, and SuperAdmin entity creation.
    /// Responsibility: Creates core identity, assigns the 'SuperAdmin' role, and links them to Employee and SuperAdmin tables.
    /// </summary>
    public class SuperAdminSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context; 
        private readonly ILogger<SuperAdminSeeder> _logger;
        private const string JsonFileName = "SuperAdmin.json";
        private const string RoleName = "SuperAdmin";
        private const string EmployeeTableName = "employee"; // Table name for IDENTITY reset

        public SuperAdminSeeder(
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ILogger<SuperAdminSeeder> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the SuperAdmin data seeding process asynchronously.
        /// </summary>
        public async Task SeedAsync()
        {
            if (await _context.Set<SuperAdmin>().AnyAsync())
            {
                _logger.LogInformation("SuperAdmin seeding skipped: Records already exist.");
                return;
            }

            try
            {
                // 1. Reset Identity Counter for Employee table (Crucial for clean seeding)
                //await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, EmployeeTableName);

                // 2. Read and Deserialize JSON Data
                var superAdminDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<SuperAdminSeedDto>(
                    JsonFileName, _logger);

                int seededCount = 0;
                foreach (var dto in superAdminDtos)
                {
                    // Check if AppUser already exists
                    if (await _userManager.FindByNameAsync(dto.UserName) != null)
                    {
                        _logger.LogWarning("AppUser with UserName '{UserName}' already exists. Skipping.", dto.UserName);
                        continue;
                    }

                    // 3. Create the AppUser (Identity Core)
                    var appUserEntity = new AppUser
                    {
                        UserName = dto.UserName,
                        Email = dto.Email,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        DateOfBirth = dto.DateOfBirth,
                        Address = dto.Address,
                        UserType = UserType.SuperAdmin, // Set the discriminator
                        DateCreated = DateTime.UtcNow,
                        IsDeleted = false,
                        ProfilePictureUrl = dto.ProfilePictureUrl
                    };

                    var result = await _userManager.CreateAsync(appUserEntity, dto.Password);

                    if (result.Succeeded)
                    {
                        // 4. Assign the SuperAdmin Role
                        await _userManager.AddToRoleAsync(appUserEntity, RoleName);

                        // 5. Create the Employee Record
                        var employeeEntity = new Employee
                        {
                            AppUserId = appUserEntity.Id,  
                            DateOfHire = dto.DateOfHire ?? DateTime.UtcNow,
                            Salary = dto.Salary,
                            ShiftPreferenceFk = dto.ShiftPreferenceFk,
                            IsDeleted = false
                        };
                        _context.Set<Employee>().Add(employeeEntity);
                        // SaveChanges is required here to generate the EmployeeId (Identity value)
                        await _context.SaveChangesAsync();

                        // 6. Create the SuperAdmin Record
                        var superAdminEntity = new SuperAdmin
                        {
                            AppUserId = appUserEntity.Id,  
                            EmployeeId = employeeEntity.EmployeeId  
                        };
                        _context.Set<SuperAdmin>().Add(superAdminEntity);

                        seededCount++;
                    }
                    else
                    {
                        _logger.LogError("Error creating AppUser '{UserName}': {Errors}",
                            dto.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // 7. Final save for SuperAdmin records
                await _context.SaveChangesAsync();

                _logger.LogInformation("SuperAdmin Seeding completed: Successfully seeded {Count} '{RoleName}' users.", seededCount, RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during SuperAdmin seeding process.");
                throw;
            }
        }
    }
}