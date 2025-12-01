using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Aircraft entity, extending the generic repository.
    /// Provides methods for managing and querying the airline's fleet, crucial for
    /// flight scheduling, maintenance tracking, and overall fleet management.
    /// </summary>
    public interface IAircraftRepository : IGenericRepository<Aircraft>
    {
        /// <summary>
        /// Retrieves an active aircraft by its unique tail number (primary key).
        /// </summary>
        /// <param name="tailNumber">The unique tail number of the aircraft.</param>
        /// <returns>The Aircraft entity if found and active; otherwise, null.</returns>
        Task<Aircraft?> GetByTailNumberAsync(string tailNumber);

        /// <summary>
        /// Retrieves all active aircraft belonging to a specific airline.
        /// </summary>
        /// <param name="airlineIataCode">The 2-letter IATA code of the airline.</param>
        /// <returns>An enumerable collection of active Aircraft entities for the airline.</returns>
        Task<IEnumerable<Aircraft>> GetByAirlineAsync(string airlineIataCode);

        /// <summary>
        /// Retrieves all active aircraft of a specific type.
        /// </summary>
        /// <param name="aircraftTypeId">The ID of the aircraft type.</param>
        /// <returns>An enumerable collection of active Aircraft entities of the specified type.</returns>
        Task<IEnumerable<Aircraft>> GetByTypeAsync(int aircraftTypeId);

        /// <summary>
        /// Retrieves all active aircraft currently having a specific status (e.g., 'Active', 'Maintenance', 'Grounded').
        /// </summary>
        /// <param name="status">The status to filter by (case-insensitive).</param>
        /// <returns>An enumerable collection of active Aircraft entities with the specified status.</returns>
        Task<IEnumerable<Aircraft>> GetByStatusAsync(string status);

        /// <summary>
        /// Retrieves aircraft that might require maintenance based on total flight hours.
        /// (Example threshold: > 5000 hours). Useful for the management system.
        /// </summary>
        /// <param name="minFlightHoursThreshold">The minimum flight hours to check.</param>
        /// <returns>An enumerable collection of active Aircraft entities exceeding the threshold.</returns>
        Task<IEnumerable<Aircraft>> GetRequiringMaintenanceCheckAsync(int minFlightHoursThreshold = 5000);

        /// <summary>
        /// Retrieves all aircraft, including those marked as soft-deleted.
        /// For administrative purposes in the management system.
        /// </summary>
        /// <returns>An enumerable collection of all Aircraft entities.</returns>
        Task<IEnumerable<Aircraft>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) aircraft.
        /// Standard method for general fleet overview.
        /// </summary>
        /// <returns>An enumerable collection of active Aircraft entities.</returns>
        Task<IEnumerable<Aircraft>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active aircraft by its tail number, including detailed related information
        /// like Airline, AircraftType, Configurations, and Seats (eager loading).
        /// </summary>
        /// <param name="tailNumber">The unique tail number of the aircraft.</param>
        /// <returns>The Aircraft entity with its related details loaded, if found and active; otherwise, null.</returns>
        Task<Aircraft?> GetWithDetailsAsync(string tailNumber);

        /// <summary>
        /// Checks if an aircraft with the specified tail number exists (active or soft-deleted).
        /// </summary>
        /// <param name="tailNumber">The tail number to check.</param>
        /// <returns>True if an aircraft with the given tail number exists; otherwise, false.</returns>
        Task<bool> ExistsByTailNumberAsync(string tailNumber);

        /// <summary>
        /// Updates the status of a specific aircraft.
        /// </summary>
        /// <param name="tailNumber">The tail number of the aircraft to update.</param>
        /// <param name="newStatus">The new status string.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateStatusAsync(string tailNumber, string newStatus);

        /// <summary>
        /// Updates the total flight hours for a specific aircraft.
        /// </summary>
        /// <param name="tailNumber">The tail number of the aircraft.</param>
        /// <param name="additionalHours">The number of hours to add to the current total.</param>
        /// <returns>The new total flight hours if successful, null otherwise.</returns>
        Task<int?> AddFlightHoursAsync(string tailNumber, int additionalHours);
    }
}