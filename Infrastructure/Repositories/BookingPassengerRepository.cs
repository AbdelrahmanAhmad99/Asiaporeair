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
    public class BookingPassengerRepository : GenericRepository<BookingPassenger>, IBookingPassengerRepository
    {
        public BookingPassengerRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<BookingPassenger?> GetActiveByIdAsync(int bookingId, int passengerId)
        {
            // Use FindAsync for composite key lookup, then check IsDeleted
            var bookingPassenger = await _dbSet.FindAsync(bookingId, passengerId);
            return (bookingPassenger != null && !bookingPassenger.IsDeleted) ? bookingPassenger : null;
        }
         
        public async Task AddMultipleAsync(IEnumerable<BookingPassenger> bookingPassengers)
        {
            await _dbSet.AddRangeAsync(bookingPassengers);
        }
        public async Task<IEnumerable<BookingPassenger>> GetByBookingAsync(int bookingId)
        {
            return await _dbSet
                .Include(bp => bp.Passenger)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u.FrequentFlyer)
                .Include(bp => bp.SeatAssignment)
                    .ThenInclude(s => s.CabinClass)
                .Where(bp => bp.BookingId == bookingId && !bp.IsDeleted && !bp.Passenger.IsDeleted)
                .OrderBy(bp => bp.Passenger.LastName)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<BookingPassenger>> GetByPassengerAsync(int passengerId)
        {
            return await _dbSet
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.FlightInstance)
                        .ThenInclude(fi => fi.Schedule)
                .Where(bp => bp.PassengerId == passengerId && !bp.IsDeleted && !bp.Booking.IsDeleted)
                .OrderBy(bp => bp.Booking.BookingTime)
                .ToListAsync();
        }
         
        public async Task<BookingPassenger?> GetWithDetailsAsync(int bookingId, int passengerId)
        {
            return await _dbSet
                .Include(bp => bp.Booking)
                .Include(bp => bp.Passenger)
                .Include(bp => bp.SeatAssignment)
                    .ThenInclude(s => s.CabinClass)
                .Where(bp => bp.BookingId == bookingId && bp.PassengerId == passengerId && !bp.IsDeleted && !bp.Passenger.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<bool> UpdateSeatAssignmentAsync(int bookingId, int passengerId, string? seatId)
        {
            var bookingPassenger = await _dbSet.FindAsync(bookingId, passengerId);
            if (bookingPassenger == null || bookingPassenger.IsDeleted)
            {
                return false; // Not found or deleted
            }

            // Validate seatId exists if not null (optional, could be done in service layer)
            if (seatId != null && !await _context.Seats.AnyAsync(s => s.SeatId == seatId && !s.IsDeleted))
            {
                return false; // Seat to assign doesn't exist or is deleted
            }

            bookingPassenger.SeatAssignmentId = seatId;
            Update(bookingPassenger); // Mark as modified
            // SaveChangesAsync called by UnitOfWork
            return true;
        }
         
        public async Task<IEnumerable<BookingPassenger>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<BookingPassenger>> GetAllActiveAsync()
        {
            return await _dbSet.Where(bp => !bp.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsAsync(int bookingId, int passengerId)
        {
            return await _dbSet.AnyAsync(bp => bp.BookingId == bookingId && bp.PassengerId == passengerId && !bp.IsDeleted);
        }
         
        public async Task<int> GetPassengerCountForBookingAsync(int bookingId)
        {
            return await _dbSet.CountAsync(bp => bp.BookingId == bookingId && !bp.IsDeleted);
        }
         
        public async Task<int> GetPassengerCountForFlightAsync(int flightInstanceId)
        {
            // Counts passengers by linking through the Booking table
            return await _dbSet
                .Include(bp => bp.Booking)
                .CountAsync(bp =>
                    bp.Booking.FlightInstanceId == flightInstanceId &&
                    !bp.IsDeleted &&
                    !bp.Booking.IsDeleted);
        }
         
        public async Task<int> GetPassengerCountForCabinAsync(int flightInstanceId, int cabinClassId)
        {
            // Counts passengers by linking through Booking (for flight) and Seat (for cabin)
            return await _dbSet
                .Include(bp => bp.Booking)
                .Include(bp => bp.SeatAssignment)
                .CountAsync(bp =>
                    bp.Booking.FlightInstanceId == flightInstanceId &&
                    bp.SeatAssignment.CabinClassId == cabinClassId && // Check cabin class via seat
                    !bp.IsDeleted &&
                    !bp.Booking.IsDeleted);
        }
         
        public override async Task<IEnumerable<BookingPassenger>> GetAllAsync()
        {
            return await _dbSet.Where(bp => !bp.IsDeleted).ToListAsync();
        }

        // Retrieves assignments for a specific flight, including SeatAssignment.
        public async Task<IEnumerable<BookingPassenger>> GetAssignmentsByFlightAsync(int flightInstanceId)
        {
            return await _dbSet
                .Include(bp => bp.Booking) // Need booking to filter by flight instance
                .Include(bp => bp.SeatAssignment) // Include the seat link
                .Where(bp => bp.Booking.FlightInstanceId == flightInstanceId && !bp.IsDeleted)
                .ToListAsync();
        }

        // Retrieves a specific assignment based on the flight and the assigned seat.
        public async Task<BookingPassenger?> GetAssignmentByFlightAndSeatAsync(int flightInstanceId, string seatId)
        {
            return await _dbSet
                .Include(bp => bp.Booking) // Need booking to filter by flight instance
                .Where(bp => bp.Booking.FlightInstanceId == flightInstanceId &&
                             bp.SeatAssignmentId == seatId &&
                             !bp.IsDeleted)
                .FirstOrDefaultAsync();
        }

        // Retrieves assignments for a specific booking, including Passenger, SeatAssignment, and CabinClass.
        public async Task<IEnumerable<BookingPassenger>> GetAssignmentsByBookingWithDetailsAsync(int bookingId)
        {
            return await _dbSet
                .Include(bp => bp.Passenger)  
                .Include(bp => bp.SeatAssignment)  
                    .ThenInclude(s => s.CabinClass)  
                .Where(bp => bp.BookingId == bookingId && !bp.IsDeleted)
                .OrderBy(bp => bp.Passenger.LastName)  
                .ToListAsync();
        }

        public async Task<bool> IsSeatAssignedOnFlightAsync(int flightInstanceId, string seatId)
        {
            // Check if ANY booking for this flight has this seat assigned (and is not deleted)
            return await _context.BookingPassengers
                .Include(bp => bp.Booking)
                .AnyAsync(bp =>
                    bp.Booking.FlightInstanceId == flightInstanceId &&
                    bp.SeatAssignmentId == seatId &&
                    !bp.IsDeleted &&
                    !bp.Booking.IsDeleted
                );
        }

    }
}