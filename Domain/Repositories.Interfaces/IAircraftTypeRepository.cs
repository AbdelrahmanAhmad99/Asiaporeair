using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for AircraftType entity, extending the generic repository.
    /// Provides specific methods for querying aircraft type data, used in flight scheduling,
    /// aircraft management, and potentially pricing logic.
    /// </summary>
    public interface IAircraftTypeRepository : IGenericRepository<AircraftType>
    {
        /// <summary>
        /// Retrieves an active aircraft type by its unique ID.
        /// </summary>
        /// <param name="typeId">The primary key ID of the aircraft type.</param>
        /// <returns>The AircraftType entity if found and active; otherwise, null.</returns>
        Task<AircraftType?> GetActiveByIdAsync(int typeId);

        /// <summary>
        /// Retrieves active aircraft types matching a specific model name (case-insensitive search).
        /// </summary>
        /// <param name="model">The model name or partial name to search for.</param>
        /// <returns>An enumerable collection of matching active AircraftType entities.</returns>
        Task<IEnumerable<AircraftType>> FindByModelAsync(string model);

        /// <summary>
        /// Retrieves active aircraft types produced by a specific manufacturer (case-insensitive search).
        /// </summary>
        /// <param name="manufacturer">The manufacturer name.</param>
        /// <returns>An enumerable collection of active AircraftType entities from the specified manufacturer.</returns>
        Task<IEnumerable<AircraftType>> GetByManufacturerAsync(string manufacturer);

        /// <summary>
        /// Retrieves active aircraft types based on specified criteria like range, seats, or capacity.
        /// Useful for the airport management system to filter types for route planning.
        /// </summary>
        /// <param name="minRangeKm">Minimum required range in kilometers (optional).</param>
        /// <param name="minSeats">Minimum required number of seats (optional).</param>
        /// <param name="minCargoCapacity">Minimum required cargo capacity (optional).</param>
        /// <returns>An enumerable collection of active AircraftType entities matching the criteria.</returns>
        Task<IEnumerable<AircraftType>> FindByCriteriaAsync(int? minRangeKm = null, int? minSeats = null, decimal? minCargoCapacity = null);

        /// <summary>
        /// Retrieves all aircraft types, including those marked as soft-deleted.
        /// Primarily for administrative views in the airport management system (e.g., auditing or reactivation).
        /// </summary>
        /// <returns>An enumerable collection of all AircraftType entities.</returns>
        Task<IEnumerable<AircraftType>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) aircraft types.
        /// Standard method for populating dropdowns or general display.
        /// </summary>
        /// <returns>An enumerable collection of active AircraftType entities.</returns>
        Task<IEnumerable<AircraftType>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active aircraft type by its ID, including its associated Aircraft fleet (eager loading).
        /// </summary>
        /// <param name="typeId">The ID of the aircraft type.</param>
        /// <returns>The AircraftType entity with its Aircraft collection loaded, if found and active; otherwise, null.</returns>
        Task<AircraftType?> GetWithAircraftAsync(int typeId);

        /// <summary>
        /// Checks if an aircraft type with the specified model exists (active or soft-deleted, case-insensitive).
        /// </summary>
        /// <param name="model">The model name to check.</param>
        /// <returns>True if an aircraft type with the given model exists; otherwise, false.</returns>
        Task<bool> ExistsByModelAsync(string model);

        /// <summary>
        /// Checks if an aircraft type from the specified manufacturer exists (active or soft-deleted, case-insensitive).
        /// </summary>
        /// <param name="manufacturer">The manufacturer name to check.</param>
        /// <returns>True if an aircraft type from the given manufacturer exists; otherwise, false.</returns>
        Task<bool> ExistsByManufacturerAsync(string manufacturer);
    }
}