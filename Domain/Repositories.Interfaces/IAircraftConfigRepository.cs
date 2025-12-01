using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for AircraftConfig entity, extending the generic repository.
    /// Provides methods for managing different seating configurations for specific aircraft.
    /// Crucial for seat mapping, booking availability, and aircraft management.
    /// </summary>
    public interface IAircraftConfigRepository : IGenericRepository<AircraftConfig>
    {
        /// <summary>
        /// Retrieves an active aircraft configuration by its unique ID.
        /// </summary>
        /// <param name="configId">The primary key ID of the configuration.</param>
        /// <returns>The AircraftConfig entity if found and active; otherwise, null.</returns>
        Task<AircraftConfig?> GetActiveByIdAsync(int configId);

        /// <summary>
        /// Retrieves all active configurations associated with a specific aircraft tail number.
        /// </summary>
        /// <param name="tailNumber">The unique tail number of the aircraft.</param>
        /// <returns>An enumerable collection of active AircraftConfig entities for the specified aircraft.</returns>
        Task<IEnumerable<AircraftConfig>> GetByAircraftAsync(string tailNumber);

        /// <summary>
        /// Retrieves an active aircraft configuration by its ID, including its associated Cabin Classes (eager loading).
        /// </summary>
        /// <param name="configId">The ID of the configuration.</param>
        /// <returns>The AircraftConfig entity with its CabinClasses collection loaded, if found and active; otherwise, null.</returns>
        Task<AircraftConfig?> GetWithCabinClassesAsync(int configId);

        /// <summary>
        /// Retrieves a specific active configuration by its name and associated aircraft tail number (case-insensitive name check).
        /// </summary>
        /// <param name="configName">The name of the configuration.</param>
        /// <param name="tailNumber">The unique tail number of the aircraft.</param>
        /// <returns>The matching active AircraftConfig entity if found; otherwise, null.</returns>
        Task<AircraftConfig?> GetByNameAndAircraftAsync(string configName, string tailNumber);

        /// <summary>
        /// Retrieves all aircraft configurations, including those marked as soft-deleted.
        /// Useful for administrative history or reactivation in the management system.
        /// </summary>
        /// <returns>An enumerable collection of all AircraftConfig entities.</returns>
        Task<IEnumerable<AircraftConfig>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) aircraft configurations.
        /// </summary>
        /// <returns>An enumerable collection of active AircraftConfig entities.</returns>
        Task<IEnumerable<AircraftConfig>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a configuration with the specified name already exists for a given aircraft (case-insensitive).
        /// Checks both active and soft-deleted records to prevent name clashes.
        /// </summary>
        /// <param name="configName">The configuration name to check.</param>
        /// <param name="tailNumber">The unique tail number of the aircraft.</param>
        /// <returns>True if a configuration with the name exists for the aircraft; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string configName, string tailNumber);
    }
}