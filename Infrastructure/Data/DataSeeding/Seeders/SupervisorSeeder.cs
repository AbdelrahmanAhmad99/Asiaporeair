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
    /// Professional Seeder for the Supervisor role, handling AppUser, Employee, and Supervisor entity creation.
    /// Responsibility: Creates identity, assigns the 'Supervisor' role, and links them to Employee and Supervisor tables.
    /// </summary>
    public class SupervisorSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<SupervisorSeeder> _logger;
        private const string JsonFileName = "Supervisor.json";
        private const string RoleName = "Supervisor";
        private const string EmployeeTableName = "employee"; // Table name for IDENTITY reset

        public SupervisorSeeder(
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ILogger<SupervisorSeeder> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Supervisor data seeding process asynchronously.
        /// </summary>
        public async Task SeedAsync()
        {
            // Check if Supervisor records already exist to prevent duplicate seeding
            if (await _context.Set<Supervisor>().AnyAsync())
            {
                _logger.LogInformation("Supervisor seeding skipped: Records already exist.");
                return;
            }

            try
            {
                // 1. Reset IDENTITY counter for the base employee table (ensures sequential IDs starting after previous seeders)
                //await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, EmployeeTableName);

                // 2. Read and Deserialize JSON Data
                var supervisorDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<SupervisorSeedDto>(
                    JsonFileName, _logger);

                int seededCount = 0;
                foreach (var dto in supervisorDtos)
                {
                    // Check if AppUser already exists by UserName
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
                        UserType = UserType.Supervisor, 
                        DateCreated = DateTime.UtcNow,
                        IsDeleted = false,
                        ProfilePictureUrl = dto.ProfilePictureUrl
                    };

                    var result = await _userManager.CreateAsync(appUserEntity, dto.Password);

                    if (result.Succeeded)
                    {
                        // 4. Assign the Supervisor Role
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
                        // SaveChanges is crucial here to generate the EmployeeId (Identity value)
                        await _context.SaveChangesAsync();

                        // 6. Create the Supervisor Record
                        var supervisorEntity = new Supervisor
                        {
                            AppUserId = appUserEntity.Id,  
                            EmployeeId = employeeEntity.EmployeeId,  
                            ManagedArea = dto.ManagedArea,  
                            AddedById = dto.AddedById // FK to the user who added this supervisor (e.g., Admin/SuperAdmin)
                        };
                        _context.Set<Supervisor>().Add(supervisorEntity);

                        seededCount++;
                    }
                    else
                    {
                        _logger.LogError("Error creating AppUser '{UserName}': {Errors}",
                            dto.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // 7. Final save for Supervisor records
                await _context.SaveChangesAsync();

                _logger.LogInformation("Supervisor Seeding completed: Successfully seeded {Count} '{RoleName}' users.", seededCount, RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Supervisor seeding process.");
                throw;
            }
        }
    }
}