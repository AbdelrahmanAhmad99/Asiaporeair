using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Airline entity, extending the generic repository.
    /// Provides specific methods for querying airline data, essential for flight scheduling,
    /// partnerships (codeshare), and airline management.
    /// </summary>
    public interface IAirlineRepository : IGenericRepository<Airline>
    {
        /// <summary>
        /// Retrieves an active airline by its unique IATA code.
        /// </summary>
        /// <param name="iataCode">The 2-letter IATA code.</param>
        /// <returns>The Airline entity if found and active; otherwise, null.</returns>
        Task<Airline?> GetByIataCodeAsync(string iataCode);

        /// <summary>
        /// Retrieves active airlines matching a specific name (case-insensitive).
        /// </summary>
        /// <param name="name">The name or partial name of the airline.</param>
        /// <returns>An enumerable collection of matching active Airline entities.</returns>
        Task<IEnumerable<Airline>> FindByNameAsync(string name);

        /// <summary>
        /// Retrieves all active airlines based at a specific airport.
        /// </summary>
        /// <param name="airportIataCode">The 3-letter IATA code of the base airport.</param>
        /// <returns>An enumerable collection of active Airline entities based at the airport.</returns>
        Task<IEnumerable<Airline>> GetByBaseAirportAsync(string airportIataCode);

        /// <summary>
        /// Retrieves all active airlines operating within a specific region (case-insensitive).
        /// </summary>
        /// <param name="region">The operating region.</param>
        /// <returns>An enumerable collection of active Airline entities in the region.</returns>
        Task<IEnumerable<Airline>> GetByOperatingRegionAsync(string region);

        /// <summary>
        /// Retrieves all airlines, including those marked as soft-deleted.
        /// For administrative purposes in the management system.
        /// </summary>
        /// <returns>An enumerable collection of all Airline entities.</returns>
        Task<IEnumerable<Airline>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) airlines.
        /// Standard method for general use.
        /// </summary>
        /// <returns>An enumerable collection of active Airline entities.</returns>
        Task<IEnumerable<Airline>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active airline by its IATA code, including its base Airport details (eager loading).
        /// </summary>
        /// <param name="iataCode">The 2-letter IATA code.</param>
        /// <returns>The Airline entity with its BaseAirport loaded, if found and active; otherwise, null.</returns>
        Task<Airline?> GetWithBaseAirportAsync(string iataCode);

        /// <summary>
        /// Retrieves an active airline by its IATA code, including its associated Aircraft fleet (eager loading).
        /// </summary>
        /// <param name="iataCode">The 2-letter IATA code.</param>
        /// <returns>The Airline entity with its Aircraft collection loaded, if found and active; otherwise, null.</returns>
        Task<Airline?> GetWithAircraftAsync(string iataCode);

        /// <summary>
        /// Checks if an airline with the specified IATA code exists (active or soft-deleted).
        /// </summary>
        /// <param name="iataCode">The 2-letter IATA code to check.</param>
        /// <returns>True if an airline with the given IATA code exists; otherwise, false.</returns>
        Task<bool> ExistsByIataCodeAsync(string iataCode);

        /// <summary>
        /// Checks if an airline with the specified name exists (active or soft-deleted, case-insensitive).
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if an airline with the given name exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);
    }
}