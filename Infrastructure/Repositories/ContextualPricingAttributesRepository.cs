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
    public class ContextualPricingAttributesRepository : GenericRepository<ContextualPricingAttributes>, IContextualPricingAttributesRepository
    {
        public ContextualPricingAttributesRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<ContextualPricingAttributes?> GetActiveByIdAsync(int attributeId)
        {
            var attributes = await _dbSet.FindAsync(attributeId);
            return (attributes != null && !attributes.IsDeleted) ? attributes : null;
        }
         
        public async Task<ContextualPricingAttributes?> GetByTimeUntilDepartureAsync(int daysUntilDeparture)
        {
            // Order by TimeUntilDeparture, find the first one >= the target days
            return await _dbSet
                .Where(a => !a.IsDeleted && a.TimeUntilDeparture.HasValue)
                .OrderBy(a => a.TimeUntilDeparture)
                .FirstOrDefaultAsync(a => a.TimeUntilDeparture >= daysUntilDeparture);
        }
         
        public async Task<ContextualPricingAttributes?> GetByLengthOfStayAsync(int lengthOfStayDays)
        {
            // Similar logic as TimeUntilDeparture
            return await _dbSet
                .Where(a => !a.IsDeleted && a.LengthOfStay.HasValue)
                .OrderBy(a => a.LengthOfStay)
                .FirstOrDefaultAsync(a => a.LengthOfStay >= lengthOfStayDays);
            
        }
         
        public async Task<IEnumerable<ContextualPricingAttributes>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<ContextualPricingAttributes>> GetAllActiveAsync()
        {
            return await _dbSet.Where(a => !a.IsDeleted).ToListAsync();
        }
         
       
        public async Task<bool> ExistsByIdAsync(int attributeId)
        {
            return await _dbSet.AnyAsync(a => a.AttributeId == attributeId);
        }
         
        public async Task<ContextualPricingAttributes?> GetForPricingAsync(int attributeId)  
        {
            // Same as GetActiveByIdAsync essentially, but kept for compatibility
            return await _dbSet
                         .Where(a => a.AttributeId == attributeId && !a.IsDeleted)
                         .FirstOrDefaultAsync();
        }
         
        public override async Task<IEnumerable<ContextualPricingAttributes>> GetAllAsync()
        {
            return await _dbSet.Where(a => !a.IsDeleted).ToListAsync();
        }
    }
}