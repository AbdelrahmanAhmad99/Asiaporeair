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
    public class FareBasisCodeRepository : GenericRepository<FareBasisCode>, IFareBasisCodeRepository
    {
        public FareBasisCodeRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<FareBasisCode?> GetByCodeAsync(string code) // Existing method implementation
        {
            // Use FirstOrDefaultAsync with PK and IsDeleted check
            return await _dbSet
                .Where(f => f.Code == code && !f.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<FareBasisCode>> FindByDescriptionAsync(string descriptionSubstring)
        {
            // Using EF.Functions.Like for potentially better performance on large text fields
            return await _dbSet
                .Where(f => !f.IsDeleted && EF.Functions.Like(f.Description, $"%{descriptionSubstring}%"))
                .OrderBy(f => f.Code)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<FareBasisCode>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set
        }

        
        public async Task<IEnumerable<FareBasisCode>> GetAllActiveAsync()
        {
            return await _dbSet.Where(f => !f.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _dbSet.AnyAsync(f => f.Code == code);
        }
         
        public override async Task<IEnumerable<FareBasisCode>> GetAllAsync()
        {
            // Explicitly filter non-deleted items
            return await _dbSet.Where(f => !f.IsDeleted).ToListAsync();
        }
    }
}