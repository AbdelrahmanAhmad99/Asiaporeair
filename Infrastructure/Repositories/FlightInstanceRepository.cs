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
    public class FlightInstanceRepository : GenericRepository<FlightInstance>, IFlightInstanceRepository
    {
        public FlightInstanceRepository(ApplicationDbContext context) : base(context) { }
         
        public async Task<FlightInstance?> GetActiveByIdAsync(int instanceId)
        {
            return await _dbSet
                .Where(fi => fi.InstanceId == instanceId && !fi.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<FlightInstance>> SearchAsync(Expression<Func<FlightInstance, bool>> filter)
        {
            return await _dbSet
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.OriginAirport)
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.DestinationAirport)
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.AircraftType)
                .Include(fi => fi.Schedule.Airline) // Include Airline
                .Include(fi => fi.Aircraft)  
                    .ThenInclude(a => a.AircraftType)
                .Where(fi => !fi.IsDeleted) // Ensure active instances
                .Where(filter)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all future active flight instances. Includes core details.
        /// </summary>
        public async Task<IEnumerable<FlightInstance>> GetAllFutureAsync()
        {
            return await _dbSet
                .Where(fi => fi.ScheduledDeparture > DateTime.UtcNow && !fi.IsDeleted) // Use UtcNow for consistency
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.OriginAirport) // Include Airports
                 .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.DestinationAirport)
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.AircraftType)
                .Include(fi => fi.Schedule.Airline) // Include Airline
                .OrderBy(fi => fi.ScheduledDeparture)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves an active flight instance with comprehensive details.
        /// </summary>
        public async Task<FlightInstance?> GetWithDetailsAsync(int id)
        {
            return await _dbSet 
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.OriginAirport)
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.DestinationAirport)
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.AircraftType)
                .Include(fi => fi.Schedule.Airline) // Include Airline via Schedule
                .Include(fi => fi.Aircraft) // Include assigned Aircraft
                     .ThenInclude(a => a.AircraftType) // Include type of assigned aircraft  
                .Where(fi => fi.InstanceId == id && !fi.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves active flight instances scheduled within a specific date range.
        /// </summary>
        public async Task<IEnumerable<FlightInstance>> GetByScheduledDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Where(fi => fi.ScheduledDeparture >= startDate.Date &&
                             fi.ScheduledDeparture < exclusiveEndDate &&
                             !fi.IsDeleted)
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.AircraftType)  
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)        
                        .ThenInclude(r => r.OriginAirport)  
                .Include(fi => fi.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.DestinationAirport)  
                .Include(fi => fi.Schedule.Airline)
                 .Include(fi => fi.Aircraft) // Include assigned Aircraft
                     .ThenInclude(a => a.AircraftType) // Include type of assigned aircraft
                .OrderBy(fi => fi.ScheduledDeparture)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves active flight instances matching a specific flight number within a date range.
        /// </summary>
        public async Task<IEnumerable<FlightInstance>> FindByFlightNumberAndDateRangeAsync(string flightNumber, DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            var upperFlightNumber = flightNumber.ToUpper();
            return await _dbSet
                .Include(fi => fi.Schedule)
                .Where(fi => fi.Schedule.FlightNo.ToUpper() == upperFlightNumber &&
                             fi.ScheduledDeparture >= startDate.Date &&
                             fi.ScheduledDeparture < exclusiveEndDate &&
                             !fi.IsDeleted)
                .Include(fi => fi.Schedule.Route.OriginAirport)
                .Include(fi => fi.Schedule.Route.DestinationAirport)
                .Include(fi => fi.Schedule.Airline)
                .OrderBy(fi => fi.ScheduledDeparture)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves active flight instances based on their current operational status.
        /// </summary>
        public async Task<IEnumerable<FlightInstance>> GetByStatusAsync(string status)
        {
            var upperStatus = status.ToUpper();
            return await _dbSet
                .Where(fi => fi.Status.ToUpper() == upperStatus && !fi.IsDeleted)
                .Include(fi => fi.Schedule.Route.OriginAirport)
                .Include(fi => fi.Schedule.Route.DestinationAirport)
                .Include(fi => fi.Schedule.Airline)
                .OrderBy(fi => fi.ScheduledDeparture)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves an active flight instance by ID, including its assigned Flight Crew and Crew Member details.
        /// </summary>
        public async Task<FlightInstance?> GetWithCrewAsync(int instanceId)
        {
            return await _dbSet
                .Include(fi => fi.FlightCrews)
                    .ThenInclude(fc => fc.CrewMember)
                        .ThenInclude(cm => cm.Employee) // Load Employee details via CrewMember
                             .ThenInclude(e => e.AppUser) // Optionally load AppUser if needed for name
                .Where(fi => fi.InstanceId == instanceId && !fi.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all flight instances, including those marked as soft-deleted.
        /// </summary>
        public async Task<IEnumerable<FlightInstance>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }

        /// <summary>
        /// Retrieves all active (not soft-deleted) flight instances.
        /// </summary>
        public async Task<IEnumerable<FlightInstance>> GetAllActiveAsync()
        {
            return await _dbSet.Where(fi => !fi.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// Checks if a flight instance exists for a specific schedule on a given departure date/time.
        /// </summary>
        public async Task<bool> ExistsByScheduleAndTimeAsync(int scheduleId, DateTime scheduledDepartureTime)
        {
            // Use tolerance if needed, e.g., comparing only up to seconds
            return await _dbSet.AnyAsync(fi => fi.ScheduleId == scheduleId && fi.ScheduledDeparture == scheduledDepartureTime);
        }

        /// <summary>
        /// Updates the status and actual departure/arrival times for a flight instance.
        /// </summary>
        public async Task<bool> UpdateFlightStatusAsync(int instanceId, string newStatus, DateTime? actualDepartureTime, DateTime? actualArrivalTime)
        {
            var instance = await _dbSet.FindAsync(instanceId);
            if (instance == null)
            {
                return false; // Instance not found
            }

            instance.Status = newStatus;
            if (actualDepartureTime.HasValue)
            {
                instance.ActualDeparture = actualDepartureTime.Value;
            }
            if (actualArrivalTime.HasValue)
            {
                instance.ActualArrival = actualArrivalTime.Value;
            }

            Update(instance); // Mark as modified
            // SaveChangesAsync() called by UnitOfWork
            return true;
        }

        // Added: New function to satisfy FlightOperationsService
        public async Task<FlightInstance?> GetConflictingFlightAsync(string tailNumber, DateTime departure, DateTime arrival, int? instanceIdToExclude)
        {
            // Add a buffer time for turnaround (e.g., 2 hours)
            var buffer = TimeSpan.FromHours(2);
            var requiredStartTime = departure - buffer;
            var requiredEndTime = arrival + buffer;

            var query = _dbSet
                .Include(fi => fi.Schedule) // Include schedule for logging
                .Where(fi => fi.AircraftId == tailNumber &&
                             !fi.IsDeleted &&
                             fi.Status != "Cancelled" &&
                             // Check for overlap: (StartA <= EndB) and (EndA >= StartB)
                             fi.ScheduledDeparture < requiredEndTime &&
                             fi.ScheduledArrival > requiredStartTime);

            if (instanceIdToExclude.HasValue)
            {
                query = query.Where(fi => fi.InstanceId != instanceIdToExclude.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Overrides base GetAllAsync to ensure only active instances are returned by default.
        /// </summary>
        public override async Task<IEnumerable<FlightInstance>> GetAllAsync()
        {
            return await _dbSet.Where(fi => !fi.IsDeleted).ToListAsync();
        }

        // --- Add this implementation ---
        // Gets the total capacity and number of booked seats for a flight instance.
        public async Task<(int TotalCapacity, int BookedSeats)> GetSeatCountsAsync(int instanceId)
        {
            var flightInstance = await _dbSet
                .Include(fi => fi.Aircraft.AircraftType) // Need AircraftType for MaxSeats
                .Include(fi => fi.Aircraft.Configurations) // Fallback capacity
                .Where(fi => fi.InstanceId == instanceId && !fi.IsDeleted)
                .Select(fi => new {
                    // Get capacity primarily from AircraftType, fallback to Config
                    Capacity = fi.Aircraft.AircraftType.MaxSeats  ?? 0
                })
                .FirstOrDefaultAsync();

            if (flightInstance == null)
            {
                return (0, 0); // Flight not found
            }

            // Count booked passengers for this flight instance
            int bookedSeats = await _context.BookingPassengers
                                            .Include(bp => bp.Booking)
                                            .CountAsync(bp => bp.Booking.FlightInstanceId == instanceId && !bp.IsDeleted);

            return (flightInstance.Capacity, bookedSeats);
        }

    }
}