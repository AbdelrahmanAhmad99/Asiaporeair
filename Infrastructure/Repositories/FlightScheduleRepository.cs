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
    public class FlightScheduleRepository : GenericRepository<FlightSchedule>, IFlightScheduleRepository
    {
        public FlightScheduleRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<FlightSchedule?> GetActiveByIdAsync(int scheduleId)
        {
            return await _dbSet
                .Where(fs => fs.ScheduleId == scheduleId && !fs.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<FlightSchedule>> FindByFlightNumberAsync(string flightNumber)
        {
            var upperFlightNumber = flightNumber.ToUpper(); 
            return await _dbSet
                .Where(fs => fs.FlightNo.ToUpper() == upperFlightNumber && !fs.IsDeleted)
                .Include(fs => fs.Route) 
                .Include(fs => fs.Airline)
                .OrderBy(fs => fs.DepartureTimeScheduled)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all active flight schedules for a specific route.
        /// </summary> 
        public async Task<IEnumerable<FlightSchedule>> GetByRouteAsync(int routeId)
        {
            return await _dbSet
                .Where(fs => fs.RouteId == routeId && !fs.IsDeleted)
                .Include(fs => fs.Airline)
                .Include(fs => fs.AircraftType)
                .OrderBy(fs => fs.DepartureTimeScheduled)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<FlightSchedule>> GetByAirlineAsync(string airlineIataCode)
        {
            return await _dbSet
                .Where(fs => fs.AirlineId == airlineIataCode && !fs.IsDeleted)
                .Include(fs => fs.Route)
                .Include(fs => fs.AircraftType)
                .OrderBy(fs => fs.DepartureTimeScheduled)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves active flight schedules planned for a specific date range (based on departure time).
        /// </summary> 
        public async Task<IEnumerable<FlightSchedule>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure end date is exclusive for the range check
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Where(fs => fs.DepartureTimeScheduled >= startDate.Date &&
                             fs.DepartureTimeScheduled < exclusiveEndDate &&
                             !fs.IsDeleted)
                .Include(fs => fs.Route)
                .Include(fs => fs.Airline)
                .OrderBy(fs => fs.DepartureTimeScheduled)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves active flight schedules based on origin, destination, and departure date.
        /// </summary>
        public async Task<IEnumerable<FlightSchedule>> FindSchedulesAsync(string originIataCode, string destinationIataCode, DateTime departureDate)
        {
            return await _dbSet
                .Include(fs => fs.Route) // Needed for filtering
                .Include(fs => fs.Airline)
                .Include(fs => fs.AircraftType)
                .Where(fs => fs.Route.OriginAirportId == originIataCode &&
                             fs.Route.DestinationAirportId == destinationIataCode &&
                             fs.DepartureTimeScheduled.Date == departureDate.Date &&
                             !fs.IsDeleted)
                .OrderBy(fs => fs.DepartureTimeScheduled)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all flight schedules, including those marked as soft-deleted.
        /// </summary>
        public async Task<IEnumerable<FlightSchedule>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set
        }

        /// <summary>
        /// Retrieves all active (not soft-deleted) flight schedules.
        /// </summary>
        public async Task<IEnumerable<FlightSchedule>> GetAllActiveAsync()
        {
            return await _dbSet.Where(fs => !fs.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// Retrieves an active flight schedule by ID, including its associated Route, Airline, and AircraftType details.
        /// </summary>
        public async Task<FlightSchedule?> GetWithDetailsAsync(int scheduleId)
        {
            return await _dbSet
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.OriginAirport)
                .Include(fs => fs.Route)
                    .ThenInclude(r => r.DestinationAirport)
                .Include(fs => fs.Airline)
                .Include(fs => fs.AircraftType)
                .Where(fs => fs.ScheduleId == scheduleId && !fs.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Checks if a schedule with the specified flight number exists for a given date.
        /// </summary>
        public async Task<bool> ExistsByFlightNumberAndDateAsync(string flightNumber, DateTime departureDate)
        {
            var upperFlightNumber = flightNumber.ToUpper();
            return await _dbSet.AnyAsync(fs => fs.FlightNo.ToUpper() == upperFlightNumber &&
                                              fs.DepartureTimeScheduled.Date == departureDate.Date);
        }

        /// <summary>
        /// Overrides base GetAllAsync to ensure only active schedules are returned by default.
        /// </summary>
        public override async Task<IEnumerable<FlightSchedule>> GetAllAsync()
        {
            return await _dbSet.Where(fs => !fs.IsDeleted).ToListAsync();
        }
    }
}