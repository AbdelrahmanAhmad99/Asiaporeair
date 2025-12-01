using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;  
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{ 
    public class PilotRepository : GenericRepository<Pilot>, IPilotRepository
    {
        public PilotRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Retrieves an active Pilot profile by the linked Employee ID (which is the PK).
        public async Task<Pilot?> GetByEmployeeIdAsync(int employeeId)
        {
            // Pilot PK is EmployeeId, includes related details.
            return await _dbSet
                .Include(p => p.AppUser)
                .Include(p => p.CrewMember)
                    .ThenInclude(cm => cm.Employee)  
                .Include(p => p.TypeRating)  
                .Include(p => p.AddedBy)     
                .Where(p => p.EmployeeId == employeeId && !p.IsDeleted) // IsDeleted check on Pilot
                .FirstOrDefaultAsync();
        }

        // Retrieves an active Pilot profile by the linked AppUser ID.
        public async Task<Pilot?> GetByAppUserIdAsync(string appUserId)
        {
            return await _dbSet
               .Include(p => p.AppUser)
               .Include(p => p.CrewMember)
                   .ThenInclude(cm => cm.Employee)
               .Include(p => p.TypeRating)
               .Include(p => p.AddedBy)
               .Where(p => p.AppUserId == appUserId && !p.IsDeleted)
               .FirstOrDefaultAsync();
        }

        // Retrieves active Pilots who are type-rated for a specific aircraft type.
        public async Task<IEnumerable<Pilot>> GetPilotsByTypeRatingAsync(int aircraftTypeId)
        {
            return await _dbSet
                .Include(p => p.AppUser) // Include basic details
                .Include(p => p.CrewMember) // Include base airport
                .Where(p => p.AircraftTypeId == aircraftTypeId && !p.IsDeleted)
                .OrderBy(p => p.AppUser.LastName)
                .ToListAsync();
        }

        // Retrieves active Pilots based at a specific airport.
        public async Task<IEnumerable<Pilot>> GetPilotsByBaseAirportAsync(string airportIataCode)
        {
            return await _dbSet
                .Include(p => p.AppUser)
                .Include(p => p.CrewMember) // Needed for filtering base
                .Where(p => p.CrewMember.CrewBaseAirportId == airportIataCode && !p.IsDeleted)
                .OrderBy(p => p.AppUser.LastName)
                .ToListAsync();
        }

        // Finds active Pilots by license number.
        public async Task<Pilot?> FindByLicenseNumberAsync(string licenseNumber)
        {
            return await _dbSet
               .Include(p => p.AppUser)
               .Include(p => p.CrewMember)
               .Where(p => p.LicenseNumber.Equals(licenseNumber, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted)
               .FirstOrDefaultAsync();
        }

        // Retrieves all Pilot profiles, including soft-deleted ones.
        public async Task<IEnumerable<Pilot>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                        .Include(p => p.AppUser)
                        .Include(p => p.CrewMember)
                        .Include(p => p.TypeRating)
                        .OrderBy(p => p.AppUser.LastName)
                        .ToListAsync();
        }

        // Retrieves all active Pilot profiles with full details.
        public async Task<IEnumerable<Pilot>> GetAllActiveWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.AppUser)
                .Include(p => p.CrewMember)
                    .ThenInclude(cm => cm.Employee) // Needed? CrewMember PK is EmployeeId
                .Include(p => p.TypeRating)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.AppUser.LastName)
                .ToListAsync();
        }

        // Checks if an active Pilot record exists for a given Employee ID.
        public async Task<bool> ExistsByEmployeeIdAsync(int employeeId)
        {
            // Check based on PK and IsDeleted flag
            return await _dbSet.AnyAsync(p => p.EmployeeId == employeeId && !p.IsDeleted);
        }
         
    }
}