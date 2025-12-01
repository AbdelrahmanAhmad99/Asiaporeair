using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Data.DataSeeding.Seeders
{
    /// <summary>
    /// Professional Seeder class responsible for ensuring all required Identity roles exist.
    /// Roles: SuperAdmin, Admin, Supervisor, Pilot, Attendant, User (Customer).
    /// </summary>
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleSeeder> _logger;

        // Define the roles required by the application
        private static readonly List<string> RequiredRoles = new List<string>
        {
            "SuperAdmin",
            "Admin",
            "Supervisor",
            "Pilot",
            "Attendant",
            "User" // Customer role for website booking
        };

        public RoleSeeder(RoleManager<IdentityRole> roleManager, ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the Identity roles into the database asynchronously.
        /// </summary>
        public async Task SeedRolesAsync()
        {
            _logger.LogInformation("Starting Role Seeding: Checking for required Identity roles...");

            foreach (var roleName in RequiredRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                    }
                    else
                    {
                        _logger.LogError("Error creating role '{RoleName}': {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    // For production, this is a successful check, but we log the attempt.
                    _logger.LogDebug("Role '{RoleName}' already exists. Skipping.", roleName);
                }
            }

            _logger.LogInformation("Role Seeding completed successfully.");
        }
    }
}