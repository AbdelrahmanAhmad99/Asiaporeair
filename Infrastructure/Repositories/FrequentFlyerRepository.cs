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
    public class FrequentFlyerRepository : GenericRepository<FrequentFlyer>, IFrequentFlyerRepository
    {
        public FrequentFlyerRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<FrequentFlyer?> GetActiveByIdAsync(int flyerId)
        {
            var flyer = await _dbSet.FindAsync(flyerId);
            return (flyer != null && !flyer.IsDeleted) ? flyer : null;
        }
         
        public async Task<FrequentFlyer?> GetByCardNumberAsync(string cardNumber) // Existing method retained
        {
            return await _dbSet
                .Where(f => f.CardNumber == cardNumber && !f.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<FrequentFlyer?> GetByUserIdAsync(int userId)
        {
            // Find the User entity first, then navigate to the FrequentFlyer
            var user = await _context.Users
                                     .Include(u => u.FrequentFlyer) // Eager load
                                     .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

            return (user?.FrequentFlyer != null && !user.FrequentFlyer.IsDeleted) ? user.FrequentFlyer : null;
        }
         
        public async Task<IEnumerable<FrequentFlyer>> FindByLevelAsync(string level)
        {
            var upperLevel = level.ToUpper();
            return await _dbSet
                .Where(f => f.Level != null && f.Level.ToUpper() == upperLevel && !f.IsDeleted)
                .OrderBy(f => f.CardNumber)
                .ToListAsync();
        }
         
        public async Task<int?> UpdatePointsAsync(int flyerId, int pointsDelta)
        {
            var frequentFlyer = await _dbSet.FindAsync(flyerId);
            if (frequentFlyer != null && !frequentFlyer.IsDeleted)
            {
                frequentFlyer.AwardPoints = (frequentFlyer.AwardPoints ?? 0) + pointsDelta;
                Update(frequentFlyer); // Mark as modified
                // SaveChangesAsync is called by UnitOfWork
                return frequentFlyer.AwardPoints;
            }
            return null;
        }

        /// <summary>
        /// Updates points from a booking (implementation retained from your code).
        /// </summary>
        public async Task<int> UpdatePointsFromBookingAsync(int bookingId, int pointsDelta) // Existing method retained
        {
            // Find the user associated with the booking
            var booking = await _context.Bookings
                                       .Include(b => b.User) // Include the User navigation property
                                       .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking?.User?.FrequentFlyerId != null) // Use the FK from the User entity
            {
                var frequentFlyer = await _dbSet.FindAsync(booking.User.FrequentFlyerId);
                if (frequentFlyer != null && !frequentFlyer.IsDeleted)
                {
                    frequentFlyer.AwardPoints = (frequentFlyer.AwardPoints ?? 0) + pointsDelta;
                    Update(frequentFlyer);
                    // Let UnitOfWork handle SaveChangesAsync
                    return frequentFlyer.AwardPoints ?? 0;
                }
            }
            return 0; // Return 0 if no flyer found or booking has no associated user/flyer
        }


        /// <summary>
        /// Retrieves all frequent flyer accounts, including those marked as soft-deleted.
        /// </summary>
        public async Task<IEnumerable<FrequentFlyer>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }

        /// <summary>
        /// Retrieves all active (not soft-deleted) frequent flyer accounts.
        /// </summary>
        public async Task<IEnumerable<FrequentFlyer>> GetAllActiveAsync()
        {
            return await _dbSet.Where(f => !f.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// Checks if a frequent flyer account with the specified card number exists.
        /// </summary>
        public async Task<bool> ExistsByCardNumberAsync(string cardNumber)
        {
            return await _dbSet.AnyAsync(f => f.CardNumber == cardNumber);
        }

        /// <summary>
        /// Retrieves frequent flyer accounts with points above a certain threshold.
        /// </summary>
        public async Task<IEnumerable<FrequentFlyer>> GetMembersWithHighPointsAsync(int pointsThreshold)
        {
            return await _dbSet
                .Where(f => !f.IsDeleted && f.AwardPoints.HasValue && f.AwardPoints >= pointsThreshold)
                .OrderByDescending(f => f.AwardPoints)
                .ToListAsync();
        }

        /// <summary>
        /// Overrides base GetAllAsync to ensure only active accounts are returned by default.
        /// </summary>
        public override async Task<IEnumerable<FrequentFlyer>> GetAllAsync()
        {
            return await _dbSet.Where(f => !f.IsDeleted).ToListAsync();
        }
    }
}