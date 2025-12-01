using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for FlightLegDef entity, extending the generic repository.
    /// Provides methods for querying individual flight segments (legs) within a flight schedule.
    /// Important for displaying detailed itineraries and managing multi-stop flights.
    /// </summary>
    public interface IFlightLegDefRepository : IGenericRepository<FlightLegDef>
    {
        /// <summary>
        /// Retrieves an active flight leg definition by its unique ID.
        /// </summary>
        /// <param name="legDefId">The primary key ID of the flight leg definition.</param>
        /// <returns>The FlightLegDef entity if found and active; otherwise, null.</returns>
        Task<FlightLegDef?> GetActiveByIdAsync(int legDefId);

        /// <summary>
        /// Retrieves all active flight leg definitions associated with a specific flight schedule ID, ordered by segment number.
        /// </summary>
        /// <param name="scheduleId">The ID of the flight schedule.</param>
        /// <returns>An ordered enumerable collection of active FlightLegDef entities for the schedule.</returns>
        Task<IEnumerable<FlightLegDef>> GetByScheduleAsync(int scheduleId);

        /// <summary>
        /// Retrieves active flight leg definitions departing from a specific airport on a given date range (based on the associated schedule's departure time).
        /// Useful for airport departure management.
        /// </summary>
        /// <param name="departureAirportIataCode">The IATA code of the departure airport.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active FlightLegDef entities departing from the airport within the date range.</returns>
        Task<IEnumerable<FlightLegDef>> GetByDepartureAirportAndDateAsync(string departureAirportIataCode, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves active flight leg definitions arriving at a specific airport on a given date range (based on the associated schedule's arrival time).
        /// Useful for airport arrival management.
        /// </summary>
        /// <param name="arrivalAirportIataCode">The IATA code of the arrival airport.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active FlightLegDef entities arriving at the airport within the date range.</returns>
        Task<IEnumerable<FlightLegDef>> GetByArrivalAirportAndDateAsync(string arrivalAirportIataCode, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves all flight leg definitions, including those marked as soft-deleted.
        /// For administrative review or data history.
        /// </summary>
        /// <returns>An enumerable collection of all FlightLegDef entities.</returns>
        Task<IEnumerable<FlightLegDef>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) flight leg definitions.
        /// </summary>
        /// <returns>An enumerable collection of active FlightLegDef entities.</returns>
        Task<IEnumerable<FlightLegDef>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active flight leg definition by ID, including its associated Flight Schedule and Airport details (eager loading).
        /// </summary>
        /// <param name="legDefId">The ID of the flight leg definition.</param>
        /// <returns>The FlightLegDef entity with related details loaded, if found and active; otherwise, null.</returns>
        Task<FlightLegDef?> GetWithDetailsAsync(int legDefId);

        /// <summary>
        /// Checks if a specific leg segment number exists for a given flight schedule (active or soft-deleted).
        /// </summary>
        /// <param name="scheduleId">The ID of the flight schedule.</param>
        /// <param name="segmentNumber">The segment number to check.</param>
        /// <returns>True if the segment exists for the schedule; otherwise, false.</returns>
        Task<bool> ExistsByScheduleAndSegmentAsync(int scheduleId, int segmentNumber);
    }
}