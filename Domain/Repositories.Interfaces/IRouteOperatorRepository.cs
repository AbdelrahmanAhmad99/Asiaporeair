using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
     
    public interface IRouteOperatorRepository : IGenericRepository<RouteOperator>
    {
        /// <summary>
        /// Retrieves an active RouteOperator entry by its composite primary key (Route ID and Airline IATA code).
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <param name="airlineIataCode">The 2-letter IATA code of the airline.</param>
        /// <returns>The RouteOperator entity if found and active; otherwise, null.</returns>
        Task<RouteOperator?> GetActiveByIdAsync(int routeId, string airlineIataCode);

        /// <summary>
        /// Retrieves all active airlines operating on a specific route.
        /// Includes details of the Airline entity.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>An enumerable collection of active RouteOperator entities with Airline details for the specified route.</returns>
        Task<IEnumerable<RouteOperator>> GetOperatorsByRouteAsync(int routeId);

        /// <summary>
        /// Retrieves all active routes operated by a specific airline.
        /// Includes details of the Route entity.
        /// </summary>
        /// <param name="airlineIataCode">The 2-letter IATA code of the airline.</param>
        /// <returns>An enumerable collection of active RouteOperator entities with Route details for the specified airline.</returns>
        Task<IEnumerable<RouteOperator>> GetRoutesByOperatorAsync(string airlineIataCode);

        /// <summary>
        /// Retrieves all active codeshare partners for a specific airline on a specific route.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <param name="operatingAirlineIataCode">The IATA code of the airline actually operating the flight.</param>
        /// <returns>An enumerable collection of active RouteOperator entities representing codeshare partners on the route.</returns>
        Task<IEnumerable<RouteOperator>> GetCodesharePartnersAsync(int routeId, string operatingAirlineIataCode);

        /// <summary>
        /// Retrieves all RouteOperator entries, including those marked as soft-deleted.
        /// Useful for administrative history or auditing in the management system.
        /// </summary>
        /// <returns>An enumerable collection of all RouteOperator entities.</returns>
        Task<IEnumerable<RouteOperator>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) RouteOperator entries.
        /// </summary>
        /// <returns>An enumerable collection of active RouteOperator entities.</returns>
        Task<IEnumerable<RouteOperator>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a specific airline operates on a specific route (active only).
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <param name="airlineIataCode">The 2-letter IATA code of the airline.</param>
        /// <returns>True if the airline actively operates on the route; otherwise, false.</returns>
        Task<bool> ExistsAsync(int routeId, string airlineIataCode);
    }
}