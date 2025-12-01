using Application.DTOs.Route;
using Application.Models;  
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{ 
    public interface IRouteManagementService
    {
        // --- Route Methods ---

        /// <summary>
        /// Retrieves a single active route by its ID, including airport details.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>A ServiceResult containing the RouteDto, or a failure result.</returns>
        Task<ServiceResult<RouteDto>> GetRouteByIdAsync(int routeId);

        /// <summary>
        /// Retrieves all active routes originating from a specific airport.
        /// Used by booking site to show "Destinations from..."
        /// </summary>
        /// <param name="originIataCode">The 3-letter IATA code of the origin airport.</param>
        /// <returns>A ServiceResult containing a list of destination RouteDto objects.</returns>
        Task<ServiceResult<IEnumerable<RouteDto>>> GetActiveRoutesByOriginAsync(string originIataCode);

        /// <summary>
        /// Retrieves all active routes arriving at a specific airport.
        /// Used in management system for arrival logistics planning.
        /// </summary>
        /// <param name="destinationIataCode">The 3-letter IATA code of the destination airport.</param>
        /// <returns>A ServiceResult containing a list of origin RouteDto objects.</returns>
        Task<ServiceResult<IEnumerable<RouteDto>>> GetActiveRoutesByDestinationAsync(string destinationIataCode);

        /// <summary>
        /// Performs an advanced, paginated search for routes based on multiple filters. (Management System)
        /// </summary>
        /// <param name="filter">The filter criteria (origin, destination, airline, distance, etc.).</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <returns>A ServiceResult containing a paginated list of matching RouteDto objects.</returns>
        Task<ServiceResult<PaginatedResult<RouteDto>>> SearchRoutesAsync(RouteFilterDto filter, int pageNumber, int pageSize);

        /// <summary>
        /// Creates a new flight route after validating airports and uniqueness. (Management System)
        /// </summary>
        /// <param name="createDto">The data for the new route.</param>
        /// <returns>A ServiceResult containing the created RouteDto, or a failure result.</returns>
        Task<ServiceResult<RouteDto>> CreateRouteAsync(CreateRouteDto createDto);

        /// <summary>
        /// Updates an existing route's details (e.g., distance). (Management System)
        /// </summary>
        /// <param name="routeId">The ID of the route to update.</param>
        /// <param name="updateDto">The updated data for the route.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<RouteDto>> UpdateRouteAsync(int routeId, UpdateRouteDto updateDto);

        /// <summary>
        /// Soft deletes a route. Fails if the route is in use by active flight schedules. (Management System)
        /// </summary>
        /// <param name="routeId">The ID of the route to soft delete.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> DeleteRouteAsync(int routeId);

        /// <summary>
        /// Reactivates a soft-deleted route. (Management System)
        /// </summary>
        /// <param name="routeId">The ID of the route to reactivate.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> ReactivateRouteAsync(int routeId);

        // --- Route Operator Methods ---

        /// <summary>
        /// Retrieves a single route with a detailed list of all airlines operating on it (including codeshares).
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>A ServiceResult containing the RouteDetailDto with its list of operators.</returns>
        Task<ServiceResult<RouteDetailDto>> GetRouteWithOperatorsAsync(int routeId);

        /// <summary>
        /// Assigns an airline to an existing route (creates a RouteOperator link). (Management System)
        /// </summary>
        /// <param name="assignDto">The data for the new assignment.</param>
        /// <returns>A ServiceResult containing the created RouteOperatorDto.</returns>
        Task<ServiceResult<RouteOperatorDto>> AssignOperatorToRouteAsync(AssignOperatorDto assignDto);

        /// <summary>
        /// Updates the codeshare status for an airline on a specific route. (Management System)
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <param name="airlineIataCode">The IATA code of the airline.</param>
        /// <param name="updateDto">The data containing the new codeshare status.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<RouteOperatorDto>> UpdateOperatorOnRouteAsync(int routeId, string airlineIataCode, UpdateOperatorDto updateDto);

        /// <summary>
        /// Removes an airline from a route (soft deletes the RouteOperator link). (Management System)
        /// Fails if the operator is assigned to active flight schedules on this route.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <param name="airlineIataCode">The IATA code of the airline to remove.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> RemoveOperatorFromRouteAsync(int routeId, string airlineIataCode);
    }
}