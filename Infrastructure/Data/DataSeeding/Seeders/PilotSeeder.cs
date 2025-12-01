using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder for the Pilot role, handling AppUser, Employee, CrewMember, and Pilot entity creation.
    /// Responsibility: Creates identity, assigns the 'Pilot' role, and links them across the four related tables.
    /// </summary>
    public class PilotSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<PilotSeeder> _logger;
        private const string JsonFileName = "Pilot.json";
        private const string RoleName = "Pilot";
        private const string EmployeeTableName = "employee"; // Table name for IDENTITY reset

        public PilotSeeder(
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ILogger<PilotSeeder> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes the Pilot seeding process asynchronously.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting {RoleName} seeding process from '{JsonFile}'...", RoleName, JsonFileName);

            try
            {
                // 1. Read and deserialize the JSON data using the professional helper method.
                List<PilotSeedDto> pilotDtos =
                    await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<PilotSeedDto>(JsonFileName, _logger);

                if (!pilotDtos.Any())
                {
                    _logger.LogWarning("No data found in '{JsonFile}'. {RoleName} seeding aborted.", JsonFileName, RoleName);
                    return;
                }

                // Optimization: Pre-check the number of existing Pilots to determine if seeding is needed.
                int existingPilotsCount = await _context.Set<Pilot>().CountAsync();
                if (existingPilotsCount >= pilotDtos.Count)
                {
                    _logger.LogInformation("{RoleName} table already contains enough data ({ExistingCount}). Skipping seeding.", RoleName, existingPilotsCount);
                    return;
                }

                // 2. Reset the IDENTITY counter for the Employee table before insertion (EmployeeId is used as FK in Pilot/CrewMember).
                // Assuming EmployeeId is an auto-increment column.
                await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, EmployeeTableName);


                _logger.LogInformation("Successfully deserialized {Count} {RoleName} records. Starting database insertion...", pilotDtos.Count, RoleName);

                int seededCount = 0;
                foreach (var dto in pilotDtos)
                {
                    // Check if a user with this email already exists to prevent identity conflicts.
                    if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    {
                        _logger.LogWarning("User with email '{Email}' already exists. Skipping.", dto.Email);
                        continue;
                    }

                    // 3. Create AppUser Identity Record (AspnetUsers)
                    var appUserEntity = new AppUser
                    {
                        UserName = dto.Email, // Often set to Email
                        Email = dto.Email,
                        EmailConfirmed = true,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        DateOfBirth = dto.DateOfBirth,
                        Address = dto.Address,
                        PhoneNumber = dto.PhoneNumber,
                        DateCreated = DateTime.UtcNow,
                        UserType = UserType.Pilot, 
                        ProfilePictureUrl= "/profiles/Photo.jpg",
                        IsDeleted = dto.IsDeleted
                    };

                    var result = await _userManager.CreateAsync(appUserEntity, dto.Password);

                    if (result.Succeeded)
                    {
                        // 4. Assign Role to the AppUser
                        await _userManager.AddToRoleAsync(appUserEntity, RoleName);

                        // 5. Create Employee Record (employee)
                        var employeeEntity = new Employee
                        {
                            AppUserId = appUserEntity.Id, // FK to AspNetUsers
                            DateOfHire = dto.DateOfHire,
                            Salary = dto.Salary,
                            ShiftPreferenceFk= 9 ,
                            IsDeleted = dto.IsDeleted
                        };
                        _context.Set<Employee>().Add(employeeEntity);
                        await _context.SaveChangesAsync(); // Save to get the generated EmployeeId

                        // 6. Create CrewMember Record (crew_member)
                        var crewMemberEntity = new CrewMember
                        {
                            EmployeeId = employeeEntity.EmployeeId,  
                            CrewBaseAirportId = dto.CrewBaseAirportId,  
                            Position = dto.Position, // "Pilot"
                            IsDeleted = dto.IsDeleted
                        };
                        _context.Set<CrewMember>().Add(crewMemberEntity);

                        // 7. Create Pilot Record (pilot)
                        var pilotEntity = new Pilot
                        {
                            EmployeeId = employeeEntity.EmployeeId, // PK/FK from Employee/CrewMember
                            AppUserId = appUserEntity.Id,  
                            LicenseNumber = dto.LicenseNumber,
                            TotalFlightHours = dto.TotalFlightHours,
                            AircraftTypeId = dto.AircraftTypeId,  
                            LastSimCheckDate = dto.LastSimCheckDate,
                            AddedById = null ,     //dto.AddedById,
                            IsDeleted = dto.IsDeleted
                        };
                        _context.Set<Pilot>().Add(pilotEntity);

                        await _context.SaveChangesAsync(); // Final save for CrewMember and Pilot
                        seededCount++;
                    }
                    else
                    {
                        _logger.LogError("Error creating AppUser '{Email}' for Pilot: {Errors}",
                            dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                _logger.LogInformation("{RoleName} Seeding completed: Successfully seeded {Count} records.", RoleName, seededCount);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during {RoleName} seeding process. Review {JsonFile} and dependencies.", RoleName, JsonFileName);
                throw;
            }
        }
    }
}