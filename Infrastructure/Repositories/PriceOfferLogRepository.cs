using Application.DTOs.PriceOfferLog;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;  
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{ 
    public class PriceOfferLogRepository : GenericRepository<PriceOfferLog>, IPriceOfferLogRepository
    {
        public PriceOfferLogRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<PriceOfferLog?> GetActiveByIdAsync(int offerId)
        {
            var log = await _dbSet.FindAsync(offerId);
            return (log != null && !log.IsDeleted) ? log : null;
        }
         
        public async Task<IEnumerable<PriceOfferLog>> GetByTimestampRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.AddTicks(1);
            return await _dbSet
                .Include(log => log.Fare)
                .Include(log => log.Ancillary)
                .Include(log => log.ContextAttributes)
                .Where(log => log.Timestamp >= startDate &&
                              log.Timestamp < exclusiveEndDate &&
                              !log.IsDeleted)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<PriceOfferLog>> GetByFareCodeAsync(string fareCode)
        {
            return await _dbSet
                .Where(log => log.FareId == fareCode && !log.IsDeleted)
                .Include(log => log.ContextAttributes)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<PriceOfferLog>> GetByAncillaryProductAsync(int ancillaryProductId)
        {
            return await _dbSet
                .Where(log => log.AncillaryId == ancillaryProductId && !log.IsDeleted)
                .Include(log => log.ContextAttributes)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<PriceOfferLog>> GetByContextAttributesAsync(int contextAttributeId)
        {
            return await _dbSet
                .Where(log => log.ContextAttributesId == contextAttributeId && !log.IsDeleted)
                .Include(log => log.Fare)
                .Include(log => log.Ancillary)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<PriceOfferLog>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<PriceOfferLog>> GetAllActiveAsync()
        {
            return await _dbSet.Where(log => !log.IsDeleted).ToListAsync();
        }
         
        public async Task<PriceOfferLog?> GetLatestForFareAndContextAsync(string fareCode, int contextAttributeId)
        {
            return await _dbSet
                .Where(log => log.FareId == fareCode &&
                              log.ContextAttributesId == contextAttributeId &&
                              !log.IsDeleted)
                .OrderByDescending(log => log.Timestamp)
                .FirstOrDefaultAsync();
        }
         
        public async Task<PriceOfferLog?> GetLatestForAncillaryAndContextAsync(int ancillaryProductId, int contextAttributeId)
        {
            return await _dbSet
               .Where(log => log.AncillaryId == ancillaryProductId &&
                             log.ContextAttributesId == contextAttributeId &&
                             !log.IsDeleted)
               .OrderByDescending(log => log.Timestamp)
               .FirstOrDefaultAsync();
        }
         
        public override async Task<IEnumerable<PriceOfferLog>> GetAllAsync()
        {
            return await _dbSet.Where(log => !log.IsDeleted).ToListAsync();
        }



         

        // Retrieves a paginated list based on filter criteria using Expression.
        public async Task<(IEnumerable<PriceOfferLog> Items, int TotalCount)> GetPaginatedLogsAsync(
            Expression<Func<PriceOfferLog, bool>> filter, // Accept Expression
            int pageNumber,
            int pageSize)
        {
            // Start with a queryable, including related data
            var query = _dbSet
                .Include(log => log.Fare)
                .Include(log => log.Ancillary)
                .Include(log => log.ContextAttributes)
                .Where(filter); // Apply the filter expression directly

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(log => log.Timestamp) // Default order
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Calculates pricing analytics for a fare code. Returns a tuple.
        public async Task<(decimal AveragePrice, decimal MinPrice, decimal MaxPrice, int OfferCount)?> GetPricingAnalyticsForFareAsync(
            string fareCode,
            DateTime startDate,
            DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            var query = _dbSet
                .Where(log => log.FareId == fareCode &&
                              !log.IsDeleted &&
                              log.Timestamp >= startDate.Date &&
                              log.Timestamp < exclusiveEndDate);

            if (!await query.AnyAsync()) return null;

            // Use Select to project into an anonymous type or tuple before FirstOrDefaultAsync
            var stats = await query
                .GroupBy(log => 1) // Group by constant to aggregate all results
                .Select(g => new // Project into an anonymous type
                {
                    AveragePrice = g.Average(l => l.OfferPriceQuote),
                    MinPrice = g.Min(l => l.OfferPriceQuote),
                    MaxPrice = g.Max(l => l.OfferPriceQuote),
                    OfferCount = g.Count()
                })
                .FirstOrDefaultAsync();

            // Convert anonymous type to named tuple if stats is not null
            return stats != null ? (stats.AveragePrice, stats.MinPrice, stats.MaxPrice, stats.OfferCount) : null;
        }

        // Calculates pricing analytics for an ancillary product. Returns a tuple.
        public async Task<(decimal AveragePrice, decimal MinPrice, decimal MaxPrice, int OfferCount)?> GetPricingAnalyticsForAncillaryAsync(
            int ancillaryProductId,
            DateTime startDate,
            DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            var query = _dbSet
                .Where(log => log.AncillaryId == ancillaryProductId &&
                              !log.IsDeleted &&
                              log.Timestamp >= startDate.Date &&
                              log.Timestamp < exclusiveEndDate);

            if (!await query.AnyAsync()) return null;

            var stats = await query
                .GroupBy(log => 1)
                .Select(g => new
                {
                    AveragePrice = g.Average(l => l.OfferPriceQuote),
                    MinPrice = g.Min(l => l.OfferPriceQuote),
                    MaxPrice = g.Max(l => l.OfferPriceQuote),
                    OfferCount = g.Count()
                })
                .FirstOrDefaultAsync();

            return stats != null ? (stats.AveragePrice, stats.MinPrice, stats.MaxPrice, stats.OfferCount) : null;
        }
       
    }
}