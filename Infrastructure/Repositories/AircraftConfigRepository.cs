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
    public class AircraftConfigRepository : GenericRepository<AircraftConfig>, IAircraftConfigRepository
    {
        public AircraftConfigRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<AircraftConfig?> GetActiveByIdAsync(int configId)
        {
            return await _dbSet
                .Where(ac => ac.ConfigId == configId && !ac.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AircraftConfig>> GetByAircraftAsync(string tailNumber)
        {
            return await _dbSet
                .Where(ac => ac.AircraftId == tailNumber && !ac.IsDeleted)
                .Include(ac => ac.Aircraft)
                .Include(ac => ac.CabinClasses.Where(cc => !cc.IsDeleted))
                    .ThenInclude(cc => cc.Seats.Where(s => !s.IsDeleted))  
                .OrderBy(ac => ac.ConfigurationName)
                .ToListAsync();
        }

        public async Task<AircraftConfig?> GetWithCabinClassesAsync(int configId)
        {
            return await _dbSet
                .Include(ac => ac.CabinClasses.Where(cc => !cc.IsDeleted))
                    .ThenInclude(cc => cc.Seats.Where(s => !s.IsDeleted))  
                .Where(ac => ac.ConfigId == configId && !ac.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<AircraftConfig?> GetByNameAndAircraftAsync(string configName, string tailNumber)
        {
            var upperConfigName = configName.ToUpper();
            return await _dbSet
                .Where(ac => ac.AircraftId == tailNumber &&
                             ac.ConfigurationName.ToUpper() == upperConfigName &&
                             !ac.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AircraftConfig>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set 
        }

         public async Task<IEnumerable<AircraftConfig>> GetAllActiveAsync()
        {
            return await _dbSet.Where(ac => !ac.IsDeleted).ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string configName, string tailNumber)
        {
            var upperConfigName = configName.ToUpper();
            return await _dbSet.AnyAsync(ac => ac.AircraftId == tailNumber &&
                                              ac.ConfigurationName.ToUpper() == upperConfigName);
        }

        public override async Task<IEnumerable<AircraftConfig>> GetAllAsync()
        {
            return await _dbSet.Where(ac => !ac.IsDeleted).ToListAsync();
        }
    }
}