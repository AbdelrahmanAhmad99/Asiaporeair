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
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<Payment?> GetActiveByIdAsync(int paymentId)
        {
            var payment = await _dbSet.FindAsync(paymentId);
            return (payment != null && !payment.IsDeleted) ? payment : null;
        }
            public async Task<IEnumerable<Payment>> GetByBookingAsync(int bookingId)  
        {
            return await _dbSet
                .Include(p => p.Booking) // Include booking details
                .Where(p => p.BookingId == bookingId && !p.IsDeleted)
                .ToListAsync();  
        }

         
        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Include(p => p.Booking) // Include booking for context
                    .ThenInclude(b => b.User.AppUser) // Include user for reporting
                .Where(p => p.TransactionDateTime >= startDate.Date &&
                             p.TransactionDateTime < exclusiveEndDate &&
                             !p.IsDeleted)
                .OrderByDescending(p => p.TransactionDateTime)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Payment>> GetByMethodAsync(string method)
        {
            var upperMethod = method.ToUpper();
            return await _dbSet
                .Include(p => p.Booking)
                .Where(p => p.Method.ToUpper() == upperMethod && !p.IsDeleted)
                .OrderByDescending(p => p.TransactionDateTime)
                .ToListAsync();
        }
         
        public async Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Where(p => p.TransactionDateTime >= startDate.Date &&
                             p.TransactionDateTime < exclusiveEndDate &&
                             !p.IsDeleted)
                .SumAsync(p => p.Amount); // Sum the Amount directly
        }
         
        public async Task<IEnumerable<Payment>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<Payment>> GetAllActiveAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsForBookingAsync(int bookingId)
        {
            return await _dbSet.AnyAsync(p => p.BookingId == bookingId && !p.IsDeleted);
        }
         
        public override async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }
         
        public async Task UpdateStatusAsync(int paymentId, string status)
        {
            var payment = await GetByIdAsync(paymentId);
            if (payment != null)
            {
                payment.Method = status; // Assuming 'Method' is used for status
                Update(payment);
            }
        }
 
        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            // Search for the payment where TransactionId matches and is not deleted
            return await _dbSet
                      .Include(p => p.Booking)
                      .FirstOrDefaultAsync(p => p.TransactionId == transactionId && !p.IsDeleted);
        }
    }
}
 