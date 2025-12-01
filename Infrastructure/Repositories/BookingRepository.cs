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
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<Booking?> GetActiveByIdAsync(int bookingId)
        {
            var booking = await _dbSet.FindAsync(bookingId);
            return (booking != null && !booking.IsDeleted) ? booking : null;
        }
         
        public async Task<Booking?> GetByReferenceAsync(string bookingReference)
        {
            return await _dbSet
                .Where(b => b.BookingRef == bookingReference && !b.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<Booking?> GetWithDetailsAsync(int bookingId)
        {
            return await _dbSet
                .Include(b => b.User)
                    .ThenInclude(u => u.AppUser)
                .Include(b => b.User)
                    .ThenInclude(u => u.FrequentFlyer)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Passenger)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.SeatAssignment)
                        .ThenInclude(s => s.CabinClass)
                .Include(b => b.AncillarySales)
                    .ThenInclude(ans => ans.Product)
                .Include(b => b.Payments)
                .Include(b => b.FlightInstance)
                    .ThenInclude(fi => fi.Schedule)
                        .ThenInclude(s => s.Route)
                            .ThenInclude(r => r.OriginAirport)
                .Include(b => b.FlightInstance.Schedule.Airline)
                .Include(b => b.FlightInstance.Schedule.Route.DestinationAirport)
                .Include(b => b.FlightInstance.Aircraft)
                    .ThenInclude(a => a.AircraftType)
                .Include(b => b.FareBasisCode)
                .Where(b => b.BookingId == bookingId && !b.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<PaginatedBookingsResult> GetPaginatedByUserAsync(string userId, int pageNumber, int pageSize)  
        {
            // Query assumes User entity has AppUserId navigation property back to AppUser
            var query = _dbSet
                .Include(b => b.User)  
                .Include(b => b.FlightInstance.Schedule.Route.OriginAirport)  
                .Include(b => b.FlightInstance.Schedule.Route.DestinationAirport)
                .Where(b => b.User.AppUserId == userId && !b.IsDeleted);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.BookingTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedBookingsResult
            {
                TotalCount = totalCount,
                Bookings = bookings
            };
        }
         
        public async Task<IEnumerable<Booking>> GetByFlightInstanceAsync(int flightInstanceId)
        {
            return await _dbSet
                .Where(b => b.FlightInstanceId == flightInstanceId && !b.IsDeleted)
                .Include(b => b.User.AppUser) // Include user details
                .Include(b => b.User)
                   .ThenInclude(bp => bp.FrequentFlyer)
                .Include(b => b.BookingPassengers)
                   .ThenInclude(bp => bp.Passenger) // Include passengers
                         .ThenInclude(bp => bp.BookingPassengers)
                              .ThenInclude(bp => bp.SeatAssignment)
                                 .ThenInclude(s => s.CabinClass)
                .OrderBy(b => b.BookingTime)
                .ToListAsync();
        } 
        public async Task<IEnumerable<Booking>> FindByPassengerPassportAsync(string passportNumber)
        {
            // Query through the BookingPassengers link table
            return await _dbSet
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Passenger)
                .Where(b => !b.IsDeleted &&
                            b.BookingPassengers.Any(bp => bp.Passenger.PassportNumber != null &&
                                                          bp.Passenger.PassportNumber.Equals(passportNumber, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Where(b => b.BookingTime >= startDate.Date &&
                             b.BookingTime < exclusiveEndDate &&
                             !b.IsDeleted)
                .Include(b => b.FlightInstance)
                    .ThenInclude(fi => fi.Schedule)
                          .ThenInclude(s => s.Airline)  
                .Include(b => b.FlightInstance)
                      .ThenInclude(fi => fi.Schedule)
                          .ThenInclude(s => s.Route)
                .Include(b => b.User.AppUser)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Booking>> GetByPaymentStatusAsync(string status)
        {
            return await _dbSet
                .Where(b => b.PaymentStatus != null && b.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase) && !b.IsDeleted)
                .Include(b => b.User.AppUser)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();
        }
         
        public async Task<PaginatedBookingsResult> GetAllIncludingDeletedPaginatedAsync(int pageNumber, int pageSize)
        {
            var query = _dbSet.IgnoreQueryFilters() // Include soft-deleted
                              .Include(b => b.User.AppUser);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.BookingTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedBookingsResult
            {
                TotalCount = totalCount,
                Bookings = bookings
            };
        }
         
        public async Task<bool> UpdatePaymentStatusAsync(int bookingId, string newStatus)
        {
            var booking = await _dbSet.FindAsync(bookingId);
            if (booking == null)
            {
                return false;
            }
            booking.PaymentStatus = newStatus;
            Update(booking); // Mark as modified
            // SaveChangesAsync called by UnitOfWork
            return true;
        }
         
        public async Task<bool> ExistsByReferenceAsync(string bookingReference)
        {
            return await _dbSet.AnyAsync(b => b.BookingRef == bookingReference);
        }
         
        public override async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _dbSet.Where(b => !b.IsDeleted).ToListAsync();
        }


 
        // Retrieves active bookings for a specific User (passenger profile) ID.
        public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(b => b.UserId == userId && !b.IsDeleted)
                 .Include(b => b.FlightInstance.Schedule.Route.OriginAirport)  
                 .Include(b => b.FlightInstance.Schedule.Route.DestinationAirport)
                 .Include(b => b.FlightInstance.Schedule)  
                 .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Passenger)  
                 .Include(b => b.User)
                    .ThenInclude(b => b.FrequentFlyer)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();
        }
    }
}