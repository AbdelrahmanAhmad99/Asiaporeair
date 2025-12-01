using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for CabinClass entity, extending the generic repository.
    /// Provides methods for querying cabin class details within specific aircraft configurations.
    /// Essential for seat selection, pricing tiers, and booking availability.
    /// </summary>
    public interface ICabinClassRepository : IGenericRepository<CabinClass>
    {
        /// <summary>
        /// Retrieves an active cabin class by its unique ID.
        /// </summary>
        /// <param name="cabinClassId">The primary key ID of the cabin class.</param>
        /// <returns>The CabinClass entity if found and active; otherwise, null.</returns>
        Task<CabinClass?> GetActiveByIdAsync(int cabinClassId);

        /// <summary>
        /// Retrieves all active cabin classes associated with a specific aircraft configuration ID.
        /// </summary>
        /// <param name="configId">The ID of the aircraft configuration.</param>
        /// <returns>An enumerable collection of active CabinClass entities for the specified configuration.</returns>
        Task<IEnumerable<CabinClass>> GetByConfigurationAsync(int configId);

        /// <summary>
        /// Retrieves a specific active cabin class by its name within a given aircraft configuration (case-insensitive name check).
        /// </summary>
        /// <param name="name">The name of the cabin class (e.g., 'Economy', 'Business').</param>
        /// <param name="configId">The ID of the aircraft configuration.</param>
        /// <returns>The matching active CabinClass entity if found; otherwise, null.</returns>
        Task<CabinClass?> GetByNameAndConfigAsync(string name, int configId);

        /// <summary>
        /// Retrieves all cabin classes, including those marked as soft-deleted.
        /// For administrative review or data auditing in the management system.
        /// </summary>
        /// <returns>An enumerable collection of all CabinClass entities.</returns>
        Task<IEnumerable<CabinClass>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) cabin classes across all configurations.
        /// </summary>
        /// <returns>An enumerable collection of active CabinClass entities.</returns>
        Task<IEnumerable<CabinClass>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active cabin class by its ID, including its associated Seats (eager loading).
        /// </summary>
        /// <param name="cabinClassId">The ID of the cabin class.</param>
        /// <returns>The CabinClass entity with its Seats collection loaded, if found and active; otherwise, null.</returns>
        Task<CabinClass?> GetWithSeatsAsync(int cabinClassId);

        /// <summary>
        /// Checks if a cabin class with the specified name exists within a given configuration (case-insensitive).
        /// Checks both active and soft-deleted records.
        /// </summary>
        /// <param name="name">The cabin class name to check.</param>
        /// <param name="configId">The ID of the aircraft configuration.</param>
        /// <returns>True if a cabin class with the name exists for the configuration; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name, int configId);
    }
}