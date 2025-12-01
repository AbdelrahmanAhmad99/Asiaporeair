 using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.DataSeeding.DataSeedingDTOs;
using Infrastructure.Data.DataSeeding.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Seeder for the Regular Users table and the Primary Identity Entity (AppUser).
    /// Responsibilities: Creating user accounts, assigning 'User' roles, and linking them to the Frequent Flyer program.
    /// </summary>
    public class UserSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<UserSeeder> _logger;
        private const string JsonFileName = "User.json";
        private const string RoleName = "User";  

        public UserSeeder(
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ILogger<UserSeeder> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }
         
        public async Task SeedUsersAsync()
        {

            const string TableName = "user";  
            if (await _context.Users.AnyAsync()) return;

            // 1.Reset the counter to ensure it starts from 1 (if the user's table uses automatic numbering)
            await JsonDataSeederHelper.ResetIdentityCounterAsync(_context, TableName);

            // verification: Do not feed if there are already users in the required role.
            if ((await _userManager.GetUsersInRoleAsync(RoleName)).Any())
            {
                _logger.LogInformation("User seeding skipped: Role '{RoleName}' already has existing users.", RoleName);
                return;
            }

            try
            {
                _logger.LogInformation("Starting User and AppUser seeding for role '{RoleName}'...", RoleName);

               // 1. Read data from a JSON file using Helper 
               // AppUserSeedDto is used because it contains all the required Identity fields
                var seedDtos = await JsonDataSeederHelper.ReadAndDeserializeJsonAsync<AppUserSeedDto>(
                    JsonFileName, _logger);

                if (!seedDtos.Any())
                {
                    _logger.LogWarning("No data found in {JsonFileName}. Skipping insertion.", JsonFileName);
                    return;
                }

                // 2. Fetch FrequentFlyer IDs in order for binding 
                // The first 50 records(or the available number) will be retrieved
                var frequentFlyerRecords = await _context.FrequentFlyers
                    .Where(ff => ff.IsDeleted == false)
                    .OrderBy(ff => ff.FlyerId)
                    .Select(ff => new { ff.FlyerId, ff.Level })
                    .Take(50)// We only take the first 50 records for linking
                    .ToListAsync();

                var seededUserCount = 0;
                var frequentFlyerIndex = 0;

                // 3. Creating users and creating the corresponding User entity
                foreach (var dto in seedDtos)
                { 
                    var appUser = new AppUser
                    {
                        UserName = dto.UserName,
                        Email = dto.Email,
                        EmailConfirmed = true,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        DateOfBirth = dto.DateOfBirth,
                        Address = dto.Address,
                        PhoneNumber = dto.PhoneNumber,
                        UserType = UserType.User,  
                        DateCreated = DateTime.UtcNow, 
                    };

                    var result = await _userManager.CreateAsync(appUser, dto.Password);

                    if (result.Succeeded)
                    {
                        // B. Assigning the role of 'User'
                        await _userManager.AddToRoleAsync(appUser, RoleName);

                        // c. Creating a specialized User (client) entity
                        var userEntity = new User
                        {
                            AppUserId = appUser.Id,
                            IsDeleted = false
                        };

                        // D. Professional Linking: Linking the first 50 users to the first 50 records in FrequentFlyer
                        if (frequentFlyerIndex < frequentFlyerRecords.Count)
                        {
                            //Setting the foreign key and level
                            userEntity.FrequentFlyerId = frequentFlyerRecords[frequentFlyerIndex].FlyerId;
                            userEntity.KrisFlyerTier = frequentFlyerRecords[frequentFlyerIndex].Level;
                            frequentFlyerIndex++;
                        }

                        _context.Users.Add(userEntity);
                        seededUserCount++;
                    }
                    else
                    {
                        _logger.LogError("Error creating AppUser '{UserName}': {Errors}",
                            dto.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // 4.Saving specialized User entities in the database
                await _context.SaveChangesAsync();

                _logger.LogInformation("User Seeding completed: Successfully seeded {Count} '{RoleName}' users.", seededUserCount, RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR during User seeding process.");
                throw;
            }
        }
    }
}