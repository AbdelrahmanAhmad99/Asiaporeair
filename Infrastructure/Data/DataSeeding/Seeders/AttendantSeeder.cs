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
    /// Professional Seeder for the Attendant role.
    /// Responsibility: Creates core identity (AppUser), assigns the 'Attendant' role, 
    /// and links them to Employee, CrewMember, and Attendant tables in a single transaction-like process.
    /// </summary>
    public class AttendantSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<AttendantSeeder> _logger;
        private const string JsonFileName = "Attendant.json";
        private const string RoleName = "Attendant";
        private const string EmployeeTableName = "employee"; // Table name for IDENTITY reset

        public AttendantSeeder(
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ILogger<AttendantSeeder> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Attendant seeding process.
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                // 1. Check if the Attendant role exists and if any users with this role are already seeded.
                if (await _userManager.Users.AnyAsync(u => u.UserType == UserType.Attendant))
                {
                    _logger.LogInformation("Attendant Seeding skipped: Users with the '{RoleName}' role already exist.", RoleName);
                    return;
                }

                // 2. Reset IDENTITY counter for 'employee' table if it's empty (Ensures EmployeeId starts at 1)
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, EmployeeTableName);

                // 3. Read and Deserialize JSON Data
                var attendantDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AttendantSeedDto>(JsonFileName, _logger);

                int seededCount = 0;
                foreach (var dto in attendantDtos)
                {
                    // 4. Create the AppUser Record
                    var appUserEntity = new AppUser
                    {
                        UserName = dto.UserName,
                        Email = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        DateOfBirth = dto.DateOfBirth,
                        Address = dto.Address,
                        UserType = UserType.Attendant,  
                        ProfilePictureUrl = dto.ProfilePictureUrl,
                        EmailConfirmed = true  
                    };

                    var result = await _userManager.CreateAsync(appUserEntity, dto.Password);

                    if (result.Succeeded)
                    {
                        // Assign the Attendant role
                        await _userManager.AddToRoleAsync(appUserEntity, RoleName);

                        // 5. Create the Employee Record (Common to all staff)
                        var employeeEntity = new Employee
                        {
                            AppUserId = appUserEntity.Id,
                            DateOfHire = dto.DateOfHire,
                            Salary = dto.Salary,
                            ShiftPreferenceFk = 11,//dto.ShiftPreferenceFk,
                        };
                        _context.Set<Employee>().Add(employeeEntity);
                        // IMPORTANT: SaveChanges is required here to generate the EmployeeId (Identity value)
                        await _context.SaveChangesAsync();

                        // 6. Create the CrewMember Record (Common to all flight crew: Pilot/Attendant)
                        var crewMemberEntity = new CrewMember
                        {
                            EmployeeId = employeeEntity.EmployeeId, // PK/FK from Employee
                            CrewBaseAirportId = dto.CrewBaseAirportId,
                            Position = dto.Position
                        };
                        _context.Set<CrewMember>().Add(crewMemberEntity);

                        // 7. Create the Attendant Record (Specific role)
                        var attendantEntity = new Attendant
                        {
                            EmployeeId = employeeEntity.EmployeeId, // PK/FK from CrewMember/Employee
                            AppUserId = appUserEntity.Id // FK to AppUser
                            // Note: AddedById is nullable, so we omit it for initial seeding
                        };
                        _context.Set<Attendant>().Add(attendantEntity);

                        seededCount++;
                    }
                    else
                    {
                        _logger.LogError("Error creating AppUser '{UserName}': {Errors}",
                            dto.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // 8. Final save for CrewMember and Attendant records
                await _context.SaveChangesAsync();

                _logger.LogInformation("Attendant Seeding completed: Successfully seeded {Count} '{RoleName}' users.", seededCount, RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during Attendant seeding process.");
                throw;
            }
        }
    }
}