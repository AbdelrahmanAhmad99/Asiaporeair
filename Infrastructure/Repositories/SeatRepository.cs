using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SeatRepository : GenericRepository<Seat>, ISeatRepository
    {
        public SeatRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<IEnumerable<Seat>> GetAvailableAsync(int flightInstanceId, int cabinClassId)
        {
            // Get IDs of seats already assigned to passengers on this specific flight instance
            var reservedSeatIds = await _context.BookingPassengers
                .Include(bp => bp.Booking)  
                .Where(bp => bp.Booking.FlightInstanceId == flightInstanceId && bp.SeatAssignmentId != null) // Check FK for flight instance
                .Select(bp => bp.SeatAssignmentId) // Select the assigned seat ID
                .Distinct() // Ensure uniqueness
                .ToListAsync();

            // Find seats in the specified cabin class that are NOT in the reserved list
            return await _dbSet
                .Include(s => s.CabinClass)  
                .Where(s => s.CabinClassId == cabinClassId &&  
                             !s.IsDeleted &&  
                             !reservedSeatIds.Contains(s.SeatId)) // Ensure seat is not reserved
                .OrderBy(s => s.SeatNumber)  
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Seat>> GetSeatsByAircraftAsync(string aircraftTailNumber)
        {
            return await _dbSet
                .Include(s => s.CabinClass) // Include cabin class info
                .Where(s => s.AircraftId == aircraftTailNumber && !s.IsDeleted)
                .OrderBy(s => s.CabinClassId).ThenBy(s => s.SeatNumber) // Order logically
                .ToListAsync();
        }
         
        public async Task<Seat?> GetWithCabinClassAsync(string seatId)
        {
            return await _dbSet
                .Include(s => s.CabinClass)
                .Where(s => s.SeatId == seatId && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }

        
        public async Task<IEnumerable<Seat>> GetByBookingAsync(int bookingId)
        {
            // Retrieve Seat entities based on the SeatAssignmentFk in BookingPassenger
            return await _context.BookingPassengers
                .Where(bp => bp.BookingId == bookingId && bp.SeatAssignmentId != null)  
                .Include(bp => bp.SeatAssignment) 
                    .ThenInclude(s => s.CabinClass)  
                .Select(bp => bp.SeatAssignment)  
                .Where(s => s != null && !s.IsDeleted)  
                .Distinct() // Avoid duplicates if multiple passengers somehow share a seat momentarily (shouldn't happen with UNIQUE constraint)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Seat>> GetByAircraftConfigAsync(int configId)
        {
            // Seats link to Aircraft, which links to Config. We need to go through CabinClass.
            return await _dbSet
                .Include(s => s.CabinClass)
                .Where(s => s.CabinClass.ConfigId == configId && !s.IsDeleted)
                .OrderBy(s => s.CabinClassId).ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Seat>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global query filters are set
        }
         
        public async Task<IEnumerable<Seat>> GetAllActiveAsync()
        {
            return await _dbSet.Where(s => !s.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsByIdAsync(string seatId)
        {
            return await _dbSet.AnyAsync(s => s.SeatId == seatId);
        }
         
        public override async Task<IEnumerable<Seat>> GetAllAsync()
        {
            return await _dbSet.Where(s => !s.IsDeleted).ToListAsync();
        }

 
        // Retrieves available seats for a specific flight and optional cabin class ID.
        public async Task<IEnumerable<Seat>> GetAvailableSeatsForFlightAsync(int flightInstanceId, string aircraftId, IEnumerable<string> reservedSeatIds, int? cabinClassId = null)
        {
            // Convert reservedSeatIds to HashSet for efficient lookup
            var reservedSeatIdSet = reservedSeatIds.ToHashSet();

            var query = _dbSet
                .Include(s => s.CabinClass) // Include cabin details
                .Where(s => s.AircraftId == aircraftId &&  
                             !s.IsDeleted &&              
                             !reservedSeatIdSet.Contains(s.SeatId)); // Exclude reserved seats

            if (cabinClassId.HasValue)
            {
                query = query.Where(s => s.CabinClassId == cabinClassId.Value);  
            }

            return await query
                .OrderBy(s => s.CabinClassId).ThenBy(s => s.SeatNumber) // Logical ordering
                .ToListAsync();
        }

         
    
        public async Task<IEnumerable<Seat>> GetByCabinClassAsync(int cabinClassId)
        {
            return await _dbSet
                .Where(s => s.CabinClassId == cabinClassId && !s.IsDeleted)
                .Include(s => s.CabinClass) // Include cabin class info
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task ReserveMultipleAsync(IEnumerable<string> seatIds, int bookingId)
        {
            // The actual reservation logic should be in the service layer
            // This repository method is for a simple bulk update/creation
            // A more robust approach would be to add entities to the BookingPassenger table.
        }

         
    }
}