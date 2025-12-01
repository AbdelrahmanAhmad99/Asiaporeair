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
    /// Seeder for the Admin role, handling AppUser, Employee, and Admin entity creation.
    /// Responsibility: Creates core identity, assigns the 'Admin' role, and links them to Employee and Admin tables.
    /// </summary>
    public class AdminSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<AdminSeeder> _logger;
        private const string JsonFileName = "Admin.json";
        private const string RoleName = "Admin";
        private const string EmployeeTableName = "employee";  

        public AdminSeeder(
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ILogger<AdminSeeder> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        
        // Executes the Admin data seeding process asynchronously. 
        public async Task SeedAsync()
        {
            if (await _context.Set<Admin>().AnyAsync())
            {
                _logger.LogInformation("Admin seeding skipped: Records already exist.");
                return;
            }

            try
            {
                // Note: The SuperAdminSeeder already called ResetIdentityCounterAsync for 'employee'. 
                // We ensure it's called again here for redundancy or if SuperAdminSeeder is skipped.
                //await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, EmployeeTableName);

                // 2. Read and Deserialize JSON Data
                var adminDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AdminSeedDto>(
                    JsonFileName, _logger);

                int seededCount = 0;
                foreach (var dto in adminDtos)
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
                        UserType = UserType.Admin, // Set the discriminator
                        DateCreated = DateTime.UtcNow,
                        IsDeleted = false,
                        ProfilePictureUrl = dto.ProfilePictureUrl
                    };

                    var result = await _userManager.CreateAsync(appUserEntity, dto.Password);

                    if (result.Succeeded)
                    {
                        // 4. Assign the Admin Role
                        await _userManager.AddToRoleAsync(appUserEntity, RoleName);

                        // 5. Create the Employee Record
                        var employeeEntity = new Employee
                        {
                            AppUserId = appUserEntity.Id, // Link to AppUser
                            DateOfHire = dto.DateOfHire ?? DateTime.UtcNow,
                            Salary = dto.Salary,
                            ShiftPreferenceFk = dto.ShiftPreferenceFk,
                            IsDeleted = false
                        };

                        _context.Set<Employee>().Add(employeeEntity);

                        // SaveChanges is required here to generate the EmployeeId (Identity value)
                        await _context.SaveChangesAsync();

                        // 6. Create the Admin Record
                        var adminEntity = new Admin
                        {
                            AppUserId = appUserEntity.Id,  
                            EmployeeId = employeeEntity.EmployeeId,  
                            Department = dto.Department,  
                            AddedById = dto.AddedById  
                        };
                        _context.Set<Admin>().Add(adminEntity);

                        seededCount++;
                    }
                    else
                    {
                        _logger.LogError("Error creating AppUser '{UserName}': {Errors}",
                            dto.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // 7. Final save for Admin records
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin Seeding completed: Successfully seeded {Count} '{RoleName}' users.", seededCount, RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Admin seeding process.");
                throw;
            }
        }
    }
}