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
    public class PassengerRepository : GenericRepository<Passenger>, IPassengerRepository
    {
        public PassengerRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<Passenger?> GetActiveByIdAsync(int passengerId)
        {
            var passenger = await _dbSet.FindAsync(passengerId);
            return (passenger != null && !passenger.IsDeleted) ? passenger : null;
        }

        /// <summary>
        /// Adds multiple *new* Passenger entities. Linking to booking is separate.
        /// </summary>
        public async Task AddMultiplePassengersAsync(IEnumerable<Passenger> passengers) // Renamed parameter for clarity
        {
            // This adds the Passenger records themselves.
            // Linking to Booking is done via BookingPassengerRepository.
            await _dbSet.AddRangeAsync(passengers);
        }

        /// <summary>
        /// Retrieves all active passengers associated with a specific booking ID.
        /// </summary>
        public async Task<IEnumerable<Passenger>> GetByBookingAsync(int bookingId) // Existing method retained
        {
            // Retrieve passengers via the BookingPassenger link table
            return await _context.BookingPassengers
                .Where(bp => bp.BookingId == bookingId && !bp.IsDeleted && !bp.Passenger.IsDeleted)  
                .Include(bp => bp.Passenger)  
                    .ThenInclude(p => p.User)  
                        .ThenInclude(u => u.FrequentFlyer) 
                .Select(bp => bp.Passenger)  
                .Distinct() // Ensure unique passengers if structure allows duplicates (it shouldn't)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves an active passenger by their ID, including details.
        /// </summary>
        public async Task<Passenger?> GetWithDetailsAsync(int id) // Existing method retained and validated
        {
            return await _dbSet
                .Include(p => p.User) // Include the related User entity
                    .ThenInclude(u => u.FrequentFlyer) // Include FrequentFlyer via User
                .Where(p => p.PassengerId == id && !p.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all active passengers associated with a specific User ID.
        /// </summary>
        public async Task<IEnumerable<Passenger>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                 .Include(p => p.User) // Include the related User entity
                    .ThenInclude(u => u.FrequentFlyer) // Include FrequentFlyer via User
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Finds active passengers matching a specific passport number.
        /// </summary>
        public async Task<IEnumerable<Passenger>> FindByPassportAsync(string passportNumber)
        {
            var lowerPassportNumber = passportNumber.ToLower(); 

            return await _dbSet
                .Include(p => p.User)
                    .ThenInclude(u => u.FrequentFlyer)
                .Where(p => p.PassportNumber != null &&
                             p.PassportNumber.ToLower() == lowerPassportNumber &&  
                             !p.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Finds active passengers matching first and/or last names (partial match).
        /// </summary>
        public async Task<IEnumerable<Passenger>> FindByNameAsync(string? firstName = null, string? lastName = null)
        {
            var query = _dbSet.Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(firstName))
            {
                query = query.Where(p => EF.Functions.Like(p.FirstName, $"%{firstName}%"));
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(p => EF.Functions.Like(p.LastName, $"%{lastName}%"));
            }

            return await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();
        }

        /// <summary>
        /// Retrieves all passengers, including those marked as soft-deleted.
        /// </summary>
        public async Task<IEnumerable<Passenger>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }

        /// <summary>
        /// Retrieves all active (not soft-deleted) passengers.
        /// </summary>
        public async Task<IEnumerable<Passenger>> GetAllActiveAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// Checks if a passenger with the specified passport number exists.
        /// </summary>
        public async Task<bool> ExistsByPassportAsync(string passportNumber)
        {
            return await _dbSet.AnyAsync(p => p.PassportNumber != null &&
                                              p.PassportNumber.Equals(passportNumber, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Overrides base GetAllAsync to ensure only active passengers are returned by default.
        /// </summary>
        public override async Task<IEnumerable<Passenger>> GetAllAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }

        public async Task AddMultipleAsync(IEnumerable<Passenger> passengers, int bookingId)
        { 
            await _context.Passengers.AddRangeAsync(passengers);
        }
    }
}
 