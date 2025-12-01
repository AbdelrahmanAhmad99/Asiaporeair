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
    public class BoardingPassRepository : GenericRepository<BoardingPass>, IBoardingPassRepository
    {
        public BoardingPassRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<BoardingPass?> GetActiveByIdAsync(int passId)
        {
            var pass = await _dbSet.FindAsync(passId);
            return (pass != null && !pass.IsDeleted) ? pass : null;
        }
         
        public async Task<BoardingPass?> GetByBookingPassengerAsync(int bookingId, int passengerId)
        {
            return await _dbSet
                .Include(bp => bp.BookingPassenger)  
                    .ThenInclude(bpass => bpass.Passenger)
                .ThenInclude(p => p.User)          
                    .ThenInclude(u => u.FrequentFlyer)
                .Include(bp => bp.Seat)  
                    .ThenInclude(s => s.CabinClass)  
                .Where(bp => bp.BookingPassengerBookingId == bookingId &&
                             bp.BookingPassengerPassengerId == passengerId &&
                             !bp.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<BoardingPass>> GetByBookingAsync(int bookingId)
        {
            return await _dbSet
                .Include(bp => bp.BookingPassenger.Passenger)  
                .Include(bp => bp.Seat)  
                .Where(bp => bp.BookingPassengerBookingId == bookingId && !bp.IsDeleted)
                .OrderBy(bp => bp.BookingPassenger.Passenger.LastName)  
                .ToListAsync();
        }
         
        public async Task<IEnumerable<BoardingPass>> GetByFlightInstanceAsync(int flightInstanceId)
        { 
            return await _dbSet
                .Include(bp => bp.BookingPassenger.Booking)  
                .Include(bp => bp.BookingPassenger.Passenger)  
                .Include(bp => bp.Seat)  
                .Where(bp => bp.BookingPassenger.Booking.FlightInstanceId == flightInstanceId && !bp.IsDeleted)
                .OrderBy(bp => bp.Seat.SeatNumber)  
                .ToListAsync();
        }
         
        public async Task<BoardingPass?> GetByFlightAndSeatAsync(int flightInstanceId, string seatId)
        {
            return await _dbSet
               .Include(bp => bp.BookingPassenger.Booking)  
               .Where(bp => bp.SeatId == seatId &&
                            bp.BookingPassenger.Booking.FlightInstanceId == flightInstanceId &&
                            !bp.IsDeleted)
               .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<BoardingPass>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<BoardingPass>> GetAllActiveAsync()
        {
            return await _dbSet.Where(bp => !bp.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsForBookingPassengerAsync(int bookingId, int passengerId)
        {
            return await _dbSet.AnyAsync(bp => bp.BookingPassengerBookingId == bookingId &&
                                               bp.BookingPassengerPassengerId == passengerId &&
                                               !bp.IsDeleted);
        }
         
        public async Task<bool> UpdateBoardingDetailsAsync(int passId, DateTime? boardingTime, bool? precheckStatus)
        {
            var pass = await _dbSet.FindAsync(passId);
            if (pass == null || pass.IsDeleted)
            {
                return false; // Not found or deleted
            }

            bool changed = false;
            if (boardingTime.HasValue)
            {
                pass.BoardingTime = boardingTime.Value;
                changed = true;
            }
            if (precheckStatus.HasValue)
            {
                pass.PrecheckStatus = precheckStatus.Value;
                changed = true;
            }

            if (changed)
            {
                Update(pass); // Mark as modified
                // SaveChangesAsync called by UnitOfWork
            }
            return true;
        }
         
        public override async Task<IEnumerable<BoardingPass>> GetAllAsync()
        {
            return await _dbSet.Where(bp => !bp.IsDeleted).ToListAsync();
        }
    }
}