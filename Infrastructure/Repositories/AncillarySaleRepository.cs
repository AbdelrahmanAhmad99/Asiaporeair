using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;  
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{ 
    public class AncillarySaleRepository : GenericRepository<AncillarySale>, IAncillarySaleRepository
    {
        public AncillarySaleRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<AncillarySale?> GetActiveByIdAsync(int saleId)
        {
            var sale = await _dbSet.FindAsync(saleId);
            return (sale != null && !sale.IsDeleted) ? sale : null;
        }
         
        public async Task<IEnumerable<AncillarySale>> GetByBookingAsync(int bookingId)  
        {
            return await _dbSet
                .Include(s => s.Product)  
                .Where(s => s.BookingId == bookingId &&
                             !s.IsDeleted &&  
                             !s.Product.IsDeleted)  
                .OrderBy(s => s.Product.Name)  
                .ToListAsync();
        }
         
        public async Task<IEnumerable<AncillarySale>> GetByProductAsync(int productId)
        {
            return await _dbSet
                .Where(s => s.ProductId == productId && !s.IsDeleted)
                .Include(s => s.Booking)  
                .OrderBy(s => s.Booking.BookingTime)
                .ToListAsync();
        }
         
        public async Task<decimal> GetTotalRevenueForBookingAsync(int bookingId)
        {
            // Sum the PricePaid for non-deleted sales linked to the booking
            return await _dbSet
                .Where(s => s.BookingId == bookingId && !s.IsDeleted && s.PricePaid.HasValue)
                .SumAsync(s => s.PricePaid ?? 0); // Use ?? 0 to handle potential nulls safely
        }
         
        public async Task<decimal> GetTotalRevenueForFlightInstanceAsync(int flightInstanceId)
        {
            // Join AncillarySale with Booking to filter by FlightInstanceFk
            return await _dbSet
                .Include(s => s.Booking) // Include Booking to access FlightInstanceFk
                .Where(s => s.Booking.FlightInstanceId == flightInstanceId && !s.IsDeleted && s.PricePaid.HasValue)
                .SumAsync(s => s.PricePaid ?? 0);
        }
         
        public async Task<IEnumerable<AncillarySale>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<AncillarySale>> GetAllActiveAsync()
        {
            return await _dbSet.Where(s => !s.IsDeleted).Include(s => s.Product).ToListAsync();
        }
         
        public override async Task<IEnumerable<AncillarySale>> GetAllAsync()
        {
            return await _dbSet.Where(s => !s.IsDeleted).ToListAsync();
        }
    }
} 
