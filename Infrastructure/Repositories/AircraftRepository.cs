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
     public class AircraftRepository : GenericRepository<Aircraft>, IAircraftRepository
    {
        public AircraftRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Aircraft?> GetByTailNumberAsync(string tailNumber)
        {
            // Use FirstOrDefaultAsync with the primary key and IsDeleted check
            return await _dbSet
                .Include(a => a.Airline)  
                .Include(a => a.AircraftType)  
                .Where(a => a.TailNumber == tailNumber && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Aircraft>> GetByAirlineAsync(string airlineIataCode)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.AirlineId == airlineIataCode)
                .Include(a => a.Airline)  
                .OrderBy(a => a.TailNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aircraft>> GetByTypeAsync(int aircraftTypeId)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.AircraftTypeId == aircraftTypeId)
                .Include(a => a.AircraftType)  
                .OrderBy(a => a.TailNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aircraft>> GetByStatusAsync(string status)
        {
            var upperStatus = status.ToUpper();
            return await _dbSet
                .Where(a => !a.IsDeleted && a.Status.ToUpper() == upperStatus)
                .OrderBy(a => a.TailNumber)
                .ToListAsync();
        }

         public async Task<IEnumerable<Aircraft>> GetRequiringMaintenanceCheckAsync(int minFlightHoursThreshold = 5000)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.TotalFlightHours.HasValue && a.TotalFlightHours >= minFlightHoursThreshold)
                .OrderByDescending(a => a.TotalFlightHours)
                .ToListAsync();
        }

       public async Task<IEnumerable<Aircraft>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set 
        }

         public async Task<IEnumerable<Aircraft>> GetAllActiveAsync()
        {
            return await _dbSet.Where(a => !a.IsDeleted).ToListAsync();
        }

        public async Task<Aircraft?> GetWithDetailsAsync(string tailNumber)
        {
            return await _dbSet
                .Include(a => a.Airline)
                .Include(a => a.AircraftType)
                     .ThenInclude(a => a.Aircrafts)
                         .ThenInclude(a => a.Configurations)
                         .ThenInclude(ac => ac.CabinClasses.Where(cc => !cc.IsDeleted))
                         .ThenInclude(cc => cc.Seats.Where(s => !s.IsDeleted))  
                .Where(a => a.TailNumber == tailNumber && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
         public async Task<bool> ExistsByTailNumberAsync(string tailNumber)
        {
            return await _dbSet.AnyAsync(a => a.TailNumber == tailNumber);
        }

         public async Task<bool> UpdateStatusAsync(string tailNumber, string newStatus)
        {
            var aircraft = await _dbSet.FindAsync(tailNumber);
            if (aircraft == null)
            {
                return false; // Aircraft not found
            }

            aircraft.Status = newStatus;
            Update(aircraft); // Mark entity as modified
            // Note: SaveChangesAsync() needs to be called by the UnitOfWork
            return true;
        }

        public async Task<int?> AddFlightHoursAsync(string tailNumber, int additionalHours)
        {
            var aircraft = await _dbSet.FindAsync(tailNumber);
            if (aircraft == null || additionalHours < 0)
            {
                return null;  
            }

            aircraft.TotalFlightHours = (aircraft.TotalFlightHours ?? 0) + additionalHours;
            Update(aircraft); // Mark entity as modified
            // Note: SaveChangesAsync() needs to be called by the UnitOfWork
            return aircraft.TotalFlightHours;
        }

         public override async Task<IEnumerable<Aircraft>> GetAllAsync()
        {
            return await _dbSet.Where(a => !a.IsDeleted).ToListAsync();
        }
    }
}