using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for FlightSchedule entity, extending the generic repository.
    /// Provides methods for querying planned flight schedules, crucial for flight search,
    /// booking availability, and schedule management in the airport system.
    /// </summary>
    public interface IFlightScheduleRepository : IGenericRepository<FlightSchedule>
    {
        /// <summary>
        /// Retrieves an active flight schedule by its unique ID.
        /// </summary>
        /// <param name="scheduleId">The primary key ID of the schedule.</param>
        /// <returns>The FlightSchedule entity if found and active; otherwise, null.</returns>
        Task<FlightSchedule?> GetActiveByIdAsync(int scheduleId);

        /// <summary>
        /// Finds active flight schedules matching a specific flight number (case-insensitive).
        /// </summary>
        /// <param name="flightNumber">The flight number (e.g., "SQ319").</param>
        /// <returns>An enumerable collection of active FlightSchedule entities matching the number.</returns>
        Task<IEnumerable<FlightSchedule>> FindByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Retrieves all active flight schedules for a specific route.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>An enumerable collection of active FlightSchedule entities for the route.</returns>
        Task<IEnumerable<FlightSchedule>> GetByRouteAsync(int routeId);

        /// <summary>
        /// Retrieves all active flight schedules operated by a specific airline.
        /// </summary>
        /// <param name="airlineIataCode">The 2-letter IATA code of the airline.</param>
        /// <returns>An enumerable collection of active FlightSchedule entities operated by the airline.</returns>
        Task<IEnumerable<FlightSchedule>> GetByAirlineAsync(string airlineIataCode);

        /// <summary>
        /// Retrieves active flight schedules planned for a specific date range.
        /// Considers departure times.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active FlightSchedule entities within the date range.</returns>
        Task<IEnumerable<FlightSchedule>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves active flight schedules based on origin, destination, and departure date.
        /// This is a common query for the booking system's flight search.
        /// </summary>
        /// <param name="originIataCode">Origin airport IATA code.</param>
        /// <param name="destinationIataCode">Destination airport IATA code.</param>
        /// <param name="departureDate">The specific date of departure.</param>
        /// <returns>An enumerable collection of matching active FlightSchedule entities.</returns>
        Task<IEnumerable<FlightSchedule>> FindSchedulesAsync(string originIataCode, string destinationIataCode, DateTime departureDate);

        /// <summary>
        /// Retrieves all flight schedules, including those marked as soft-deleted.
        /// For administrative review or historical data access.
        /// </summary>
        /// <returns>An enumerable collection of all FlightSchedule entities.</returns>
        Task<IEnumerable<FlightSchedule>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) flight schedules.
        /// </summary>
        /// <returns>An enumerable collection of active FlightSchedule entities.</returns>
        Task<IEnumerable<FlightSchedule>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active flight schedule by ID, including its associated Route, Airline, and AircraftType details (eager loading).
        /// </summary>
        /// <param name="scheduleId">The ID of the schedule.</param>
        /// <returns>The FlightSchedule entity with related details loaded, if found and active; otherwise, null.</returns>
        Task<FlightSchedule?> GetWithDetailsAsync(int scheduleId);

        /// <summary>
        /// Checks if a schedule with the specified flight number exists for a given date (active or soft-deleted).
        /// </summary>
        /// <param name="flightNumber">The flight number to check.</param>
        /// <param name="departureDate">The specific date of departure.</param>
        /// <returns>True if a schedule exists for the flight number on that date; otherwise, false.</returns>
        Task<bool> ExistsByFlightNumberAndDateAsync(string flightNumber, DateTime departureDate);
    }
}