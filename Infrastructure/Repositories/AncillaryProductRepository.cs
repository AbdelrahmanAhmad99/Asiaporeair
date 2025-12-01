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
    public class AncillaryProductRepository : GenericRepository<AncillaryProduct>, IAncillaryProductRepository
    {
        public AncillaryProductRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<AncillaryProduct?> GetActiveByIdAsync(int productId)
        {
            var product = await _dbSet.FindAsync(productId);
            return (product != null && !product.IsDeleted) ? product : null;
        }
         
        public async Task<IEnumerable<AncillaryProduct>> GetAvailableAsync()  
        {
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }
         
        public async Task<IEnumerable<AncillaryProduct>> GetByCategoryAsync(string category)
        {
            var upperCategory = category.ToUpper();
            return await _dbSet
                .Where(p => p.Category != null && p.Category.ToUpper() == upperCategory && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<AncillaryProduct>> FindByNameAsync(string nameSubstring)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && EF.Functions.Like(p.Name, $"%{nameSubstring}%"))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<AncillaryProduct>> GetProductsSoldForBookingAsync(int bookingId)  
        {
            // Join AncillarySale with AncillaryProduct to get product details for a booking
            return await _context.AncillarySales
                .Where(s => s.BookingId == bookingId && !s.IsDeleted && !s.Product.IsDeleted) // Check IsDeleted on both
                .Include(s => s.Product)  
                .Select(s => s.Product)  
                .Distinct() // Get unique products sold
                .ToListAsync();
        }
         
        public async Task<IEnumerable<AncillaryProduct>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<AncillaryProduct>> GetAllActiveAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).Include(s => s.AncillarySales).ToListAsync();
        }
         
        public async Task<bool> ExistsByNameAsync(string name)
        {
             
            // Convert both the database column (p.Name) and the parameter (name) to upper case
            // This creates a case-insensitive comparison that EF Core can translate to SQL (UPPER() function).
            var upperName = name.ToUpper();
            return await _dbSet.AnyAsync(p => p.Name.ToUpper() == upperName);
        }
         
        public async Task<IEnumerable<AncillaryProduct>> GetByPriceRangeAsync(decimal minCost, decimal maxCost)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.BaseCost.HasValue && p.BaseCost >= minCost && p.BaseCost <= maxCost)
                .OrderBy(p => p.BaseCost)
                .ToListAsync();
        }
         
        public override async Task<IEnumerable<AncillaryProduct>> GetAllAsync()
        {
            
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<AncillaryProduct>> GetByBookingAsync(int bookingId)
        {
            return await _context.AncillarySales
                .Where(s => s.BookingId == bookingId)
                .Select(s => s.Product)
                .ToListAsync();
        }
    }
}
 