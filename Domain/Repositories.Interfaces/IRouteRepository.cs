using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Route entity, extending the generic repository.
    /// Provides methods for querying flight routes between airports, essential for
    /// flight search, scheduling, and route management in the airport system.
    /// </summary>
    public interface IRouteRepository : IGenericRepository<Route>
    {
        /// <summary>
        /// Retrieves an active route by its unique ID.
        /// </summary>
        /// <param name="routeId">The primary key ID of the route.</param>
        /// <returns>The Route entity if found and active; otherwise, null.</returns>
        Task<Route?> GetActiveByIdAsync(int routeId);

        /// <summary>
        /// Finds active routes between a specific origin and destination airport.
        /// </summary>
        /// <param name="originIataCode">The 3-letter IATA code of the origin airport.</param>
        /// <param name="destinationIataCode">The 3-letter IATA code of the destination airport.</param>
        /// <returns>An enumerable collection of active Route entities matching the criteria.</returns>
        Task<IEnumerable<Route>> FindByOriginDestinationAsync(string originIataCode, string destinationIataCode);

        /// <summary>
        /// Retrieves all active routes originating from a specific airport.
        /// Useful for airport management departure boards or route analysis.
        /// </summary>
        /// <param name="originIataCode">The 3-letter IATA code of the origin airport.</param>
        /// <returns>An enumerable collection of active Route entities originating from the airport.</returns>
        Task<IEnumerable<Route>> GetByOriginAsync(string originIataCode);

        /// <summary>
        /// Retrieves all active routes arriving at a specific airport.
        /// Useful for airport management arrival boards or route analysis.
        /// </summary>
        /// <param name="destinationIataCode">The 3-letter IATA code of the destination airport.</param>
        /// <returns>An enumerable collection of active Route entities arriving at the airport.</returns>
        Task<IEnumerable<Route>> GetByDestinationAsync(string destinationIataCode);

        /// <summary>
        /// Retrieves all routes, including those marked as soft-deleted.
        /// For administrative review or auditing.
        /// </summary>
        /// <returns>An enumerable collection of all Route entities.</returns>
        Task<IEnumerable<Route>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) routes.
        /// </summary>
        /// <returns>An enumerable collection of active Route entities.</returns>
        Task<IEnumerable<Route>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active route by ID, including its Origin and Destination Airport details (eager loading).
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>The Route entity with associated Airports loaded, if found and active; otherwise, null.</returns>
        Task<Route?> GetWithAirportsAsync(int routeId);

        /// <summary>
        /// Retrieves an active route by ID, including the Airlines operating on this route via RouteOperator (eager loading).
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>The Route entity with associated RouteOperators and Airlines loaded, if found and active; otherwise, null.</returns>
        Task<Route?> GetWithOperatorsAsync(int routeId);

        /// <summary>
        /// Checks if a direct route exists between the specified origin and destination airports (active only).
        /// </summary>
        /// <param name="originIataCode">The 3-letter IATA code of the origin airport.</param>
        /// <param name="destinationIataCode">The 3-letter IATA code of the destination airport.</param>
        /// <returns>True if an active route exists; otherwise, false.</returns>
        Task<bool> ExistsBetweenAirportsAsync(string originIataCode, string destinationIataCode);
    }
}