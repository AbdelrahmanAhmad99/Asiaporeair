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
    public class CabinClassRepository : GenericRepository<CabinClass>, ICabinClassRepository
    {
        public CabinClassRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<CabinClass?> GetActiveByIdAsync(int cabinClassId)
        {
            return await _dbSet
                .Where(cc => cc.CabinClassId == cabinClassId && !cc.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<CabinClass>> GetByConfigurationAsync(int configId)
        {
            return await _dbSet
                 .Where(cc => cc.ConfigId == configId && !cc.IsDeleted)
                 .Include(cc => cc.AircraftConfig)
                 .Include(cc => cc.Seats.Where(s => !s.IsDeleted))  
                 .OrderBy(cc => cc.Name)
                 .ToListAsync();
        }
         
        public async Task<CabinClass?> GetByNameAndConfigAsync(string name, int configId)
        {
            var upperName = name.ToUpper();
            return await _dbSet
                .Where(cc => cc.ConfigId == configId &&
                             cc.Name.ToUpper() == upperName &&
                             !cc.IsDeleted)
                .FirstOrDefaultAsync();
        } 
        public async Task<IEnumerable<CabinClass>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); 
            // Or simply: return await _dbSet.ToListAsync();
        }
         
        public async Task<IEnumerable<CabinClass>> GetAllActiveAsync()
        {
            return await _dbSet.Where(cc => !cc.IsDeleted).ToListAsync();
        }
         
        public async Task<CabinClass?> GetWithSeatsAsync(int cabinClassId)
        {
            return await _dbSet
                .Include(cc => cc.Seats.Where(s => !s.IsDeleted))  
                .Where(cc => cc.CabinClassId == cabinClassId && !cc.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<bool> ExistsByNameAsync(string name, int configId)
        {
            var upperName = name.ToUpper();
            return await _dbSet.AnyAsync(cc => cc.ConfigId == configId &&
                                               cc.Name.ToUpper() == upperName);
        }
         
        public override async Task<IEnumerable<CabinClass>> GetAllAsync()
        {
            return await _dbSet.Where(cc => !cc.IsDeleted).ToListAsync();
        }
    }
}
