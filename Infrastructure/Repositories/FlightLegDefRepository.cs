using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data; // Assuming ApplicationDbContext is here
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{ 
    public class FlightLegDefRepository : GenericRepository<FlightLegDef>, IFlightLegDefRepository
    {
        public FlightLegDefRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<FlightLegDef?> GetActiveByIdAsync(int legDefId)
        {
            return await _dbSet
                .Where(fl => fl.LegDefId == legDefId && !fl.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<FlightLegDef>> GetByScheduleAsync(int scheduleId)
        {
            return await _dbSet
                .Where(fl => fl.ScheduleId == scheduleId && !fl.IsDeleted)
                .Include(fl => fl.DepartureAirport) // Include details for display
                .Include(fl => fl.ArrivalAirport)
                .OrderBy(fl => fl.SegmentNumber)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<FlightLegDef>> GetByDepartureAirportAndDateAsync(string departureAirportIataCode, DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Include(fl => fl.Schedule) // Needed to filter by schedule date
                .Where(fl => fl.DepartureAirportId == departureAirportIataCode &&
                             fl.Schedule.DepartureTimeScheduled >= startDate.Date &&
                             fl.Schedule.DepartureTimeScheduled < exclusiveEndDate &&
                             !fl.IsDeleted)
                .Include(fl => fl.ArrivalAirport)
                .Include(fl => fl.Schedule.Airline) // Include additional useful info
                .OrderBy(fl => fl.Schedule.DepartureTimeScheduled).ThenBy(fl => fl.SegmentNumber)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<FlightLegDef>> GetByArrivalAirportAndDateAsync(string arrivalAirportIataCode, DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Include(fl => fl.Schedule) // Needed to filter by schedule date
                .Where(fl => fl.ArrivalAirportId == arrivalAirportIataCode &&
                             fl.Schedule.ArrivalTimeScheduled >= startDate.Date && // Use arrival time here
                             fl.Schedule.ArrivalTimeScheduled < exclusiveEndDate &&
                             !fl.IsDeleted)
                .Include(fl => fl.DepartureAirport)
                .Include(fl => fl.Schedule.Airline)
                .OrderBy(fl => fl.Schedule.ArrivalTimeScheduled).ThenBy(fl => fl.SegmentNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all flight leg definitions, including those marked as soft-deleted.
        /// </summary>
        public async Task<IEnumerable<FlightLegDef>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }

        /// <summary>
        /// Retrieves all active (not soft-deleted) flight leg definitions.
        /// </summary>
        public async Task<IEnumerable<FlightLegDef>> GetAllActiveAsync()
        {
            return await _dbSet.Where(fl => !fl.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// Retrieves an active flight leg definition by ID, including its associated Flight Schedule and Airport details.
        /// </summary>
        public async Task<FlightLegDef?> GetWithDetailsAsync(int legDefId)
        {
            return await _dbSet
                .Include(fl => fl.Schedule)
                    .ThenInclude(s => s.Route)
                .Include(fl => fl.Schedule)
                    .ThenInclude(s => s.Airline)
                .Include(fl => fl.DepartureAirport)
                    .ThenInclude(a => a.Country)
                .Include(fl => fl.ArrivalAirport)
                    .ThenInclude(a => a.Country)
                .Where(fl => fl.LegDefId == legDefId && !fl.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Checks if a specific leg segment number exists for a given flight schedule.
        /// </summary>
        public async Task<bool> ExistsByScheduleAndSegmentAsync(int scheduleId, int segmentNumber)
        {
            return await _dbSet.AnyAsync(fl => fl.ScheduleId == scheduleId && fl.SegmentNumber == segmentNumber);
        }

        /// <summary>
        /// Overrides base GetAllAsync to ensure only active leg definitions are returned by default.
        /// </summary>
        public override async Task<IEnumerable<FlightLegDef>> GetAllAsync()
        {
            return await _dbSet.Where(fl => !fl.IsDeleted).ToListAsync();
        }
    }
}