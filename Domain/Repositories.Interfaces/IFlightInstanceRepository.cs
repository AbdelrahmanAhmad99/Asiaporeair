using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{ 
    public interface IFlightInstanceRepository : IGenericRepository<FlightInstance>
    {
        /// <summary>
        /// Retrieves an active flight instance by its unique ID.
        /// </summary>
        /// <param name="instanceId">The primary key ID of the flight instance.</param>
        /// <returns>The FlightInstance entity if found and active; otherwise, null.</returns>
        Task<FlightInstance?> GetActiveByIdAsync(int instanceId);

        /// <summary>
        /// Retrieves active flight instances scheduled within a specific date range (based on scheduled departure).
        /// Useful for airport operational dashboards.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active FlightInstance entities within the date range.</returns>
        Task<IEnumerable<FlightInstance>> GetByScheduledDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves active flight instances matching a specific flight number within a date range.
        /// </summary>
        /// <param name="flightNumber">The flight number (e.g., "SQ319").</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of matching active FlightInstance entities.</returns>
        Task<IEnumerable<FlightInstance>> FindByFlightNumberAndDateRangeAsync(string flightNumber, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves active flight instances based on their current operational status (e.g., 'Scheduled', 'Departed', 'Arrived', 'Delayed', 'Cancelled').
        /// Critical for the airport management system.
        /// </summary>
        /// <param name="status">The status string (case-insensitive).</param>
        /// <returns>An enumerable collection of active FlightInstance entities with the specified status.</returns>
        Task<IEnumerable<FlightInstance>> GetByStatusAsync(string status);

        /// <summary>
        /// Retrieves an active flight instance by ID, including its assigned Flight Crew and Crew Member details (eager loading).
        /// Essential for managing crew assignments in the airport system.
        /// </summary>
        /// <param name="instanceId">The ID of the flight instance.</param>
        /// <returns>The FlightInstance entity with FlightCrew and CrewMember details loaded, if found and active; otherwise, null.</returns>
        Task<FlightInstance?> GetWithCrewAsync(int instanceId);

        /// <summary>
        /// Retrieves all flight instances, including those marked as soft-deleted.
        /// For administrative review or historical data.
        /// </summary>
        /// <returns>An enumerable collection of all FlightInstance entities.</returns>
        Task<IEnumerable<FlightInstance>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) flight instances.
        /// </summary>
        /// <returns>An enumerable collection of active FlightInstance entities.</returns>
        Task<IEnumerable<FlightInstance>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a flight instance exists for a specific schedule on a given departure date/time (active or soft-deleted).
        /// </summary>
        /// <param name="scheduleId">The ID of the flight schedule.</param>
        /// <param name="scheduledDepartureTime">The exact scheduled departure timestamp.</param>
        /// <returns>True if an instance exists for that schedule and time; otherwise, false.</returns>
        Task<bool> ExistsByScheduleAndTimeAsync(int scheduleId, DateTime scheduledDepartureTime);

        /// <summary>
        /// Updates the status and actual departure/arrival times for a flight instance.
        /// Used by the airport operations system.
        /// </summary>
        /// <param name="instanceId">The ID of the flight instance to update.</param>
        /// <param name="newStatus">The new operational status.</param>
        /// <param name="actualDepartureTime">The actual departure time (optional).</param>
        /// <param name="actualArrivalTime">The actual arrival time (optional).</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateFlightStatusAsync(int instanceId, string newStatus, DateTime? actualDepartureTime, DateTime? actualArrivalTime);


        /// <summary>
        /// Searches for flight instances based on criteria like origin, destination, dates, and passenger counts.
        /// Supports filtering for availability based on multiple passengers.
        /// </summary>
        /// <param name="filter">Dynamic filter expression.</param>
        /// <returns>List of matching flight instances.</returns>
        Task<IEnumerable<FlightInstance>> SearchAsync(Expression<Func<FlightInstance, bool>> filter);

        /// <summary>
        /// Retrieves all future flight instances.
        /// </summary>
        /// <returns>List of future flight instances.</returns>
        Task<IEnumerable<FlightInstance>> GetAllFutureAsync();

        /// <summary>
        /// Retrieves a flight instance with details like aircraft, route, and available seats.
        /// </summary>
        /// <param name="id">Flight instance ID.</param>
        /// <returns>Flight instance with details.</returns>
        Task<FlightInstance?> GetWithDetailsAsync(int id);

        Task<FlightInstance?> GetConflictingFlightAsync(string tailNumber, DateTime departure, DateTime arrival, int? instanceIdToExclude);
 
        // Gets the total capacity and number of booked seats for a flight instance.
        Task<(int TotalCapacity, int BookedSeats)> GetSeatCountsAsync(int instanceId);

    }
}

 