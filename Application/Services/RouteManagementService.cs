using Application.DTOs.Route;
using Application.Models; // For ServiceResult & PaginatedResult
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;  

namespace Application.Services
{
    // Service implementation for managing flight Routes and Route Operators.
    public class RouteManagementService : IRouteManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RouteManagementService> _logger;

        // Constructor for dependency injection
        public RouteManagementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RouteManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // --- Route Methods ---

        // Retrieves a single active route by its ID, including airport details.
        public async Task<ServiceResult<RouteDto>> GetRouteByIdAsync(int routeId)
        {
            try
            {
                var route = await _unitOfWork.Routes.GetWithAirportsAsync(routeId);
                if (route == null)
                {
                    _logger.LogWarning("Route with ID {RouteId} not found or inactive.", routeId);
                    return ServiceResult<RouteDto>.Failure($"Route with ID {routeId} not found or is inactive.");
                }
                var dto = _mapper.Map<RouteDto>(route);
                return ServiceResult<RouteDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route with ID {RouteId}.", routeId);
                return ServiceResult<RouteDto>.Failure("An error occurred while retrieving the route.");
            }
        }

        // Retrieves all active routes originating from a specific airport.
        public async Task<ServiceResult<IEnumerable<RouteDto>>> GetActiveRoutesByOriginAsync(string originIataCode)
        {
            try
            {
                var routes = await _unitOfWork.Routes.GetByOriginAsync(originIataCode);
                var dtos = _mapper.Map<IEnumerable<RouteDto>>(routes);
                _logger.LogInformation("Retrieved {Count} routes originating from {OriginIataCode}.", dtos.Count(), originIataCode);
                return ServiceResult<IEnumerable<RouteDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes for origin airport {OriginIataCode}.", originIataCode);
                return ServiceResult<IEnumerable<RouteDto>>.Failure("An error occurred while retrieving routes.");
            }
        }

        // Retrieves all active routes arriving at a specific airport.
        public async Task<ServiceResult<IEnumerable<RouteDto>>> GetActiveRoutesByDestinationAsync(string destinationIataCode)
        {
            try
            {
                var routes = await _unitOfWork.Routes.GetByDestinationAsync(destinationIataCode);
                var dtos = _mapper.Map<IEnumerable<RouteDto>>(routes);
                _logger.LogInformation("Retrieved {Count} routes arriving at {DestinationIataCode}.", dtos.Count(), destinationIataCode);
                return ServiceResult<IEnumerable<RouteDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes for destination airport {DestinationIataCode}.", destinationIataCode);
                return ServiceResult<IEnumerable<RouteDto>>.Failure("An error occurred while retrieving routes.");
            }
        }

        // Performs an advanced, paginated search for routes.
        public async Task<ServiceResult<PaginatedResult<RouteDto>>> SearchRoutesAsync(RouteFilterDto filter, int pageNumber, int pageSize)
        {
            try
            {
                // Base filter expression
                Expression<Func<Route, bool>> filterExpression = r => (filter.IncludeDeleted || !r.IsDeleted);

                // Add text-based filters
                if (!string.IsNullOrWhiteSpace(filter.OriginAirportIataCode))
                    filterExpression = filterExpression.And(r => r.OriginAirportId == filter.OriginAirportIataCode.ToUpper());
                if (!string.IsNullOrWhiteSpace(filter.DestinationAirportIataCode))
                    filterExpression = filterExpression.And(r => r.DestinationAirportId == filter.DestinationAirportIataCode.ToUpper());

                // Add distance filters
                if (filter.MinDistance.HasValue) filterExpression = filterExpression.And(r => r.DistanceKm >= filter.MinDistance.Value);
                if (filter.MaxDistance.HasValue) filterExpression = filterExpression.And(r => r.DistanceKm <= filter.MaxDistance.Value);

                // Add join-based filters (more complex)
                if (!string.IsNullOrWhiteSpace(filter.OriginCountryIsoCode))
                    filterExpression = filterExpression.And(r => r.OriginAirport.CountryId == filter.OriginCountryIsoCode.ToUpper());
                if (!string.IsNullOrWhiteSpace(filter.DestinationCountryIsoCode))
                    filterExpression = filterExpression.And(r => r.DestinationAirport.CountryId == filter.DestinationCountryIsoCode.ToUpper());
                if (!string.IsNullOrWhiteSpace(filter.OperatingAirlineIataCode))
                    filterExpression = filterExpression.And(r => r.RouteOperators.Any(ro => ro.AirlineId == filter.OperatingAirlineIataCode.ToUpper() && !ro.IsDeleted));

                // Fetch paged results
                var (routes, totalCount) = await _unitOfWork.Routes.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(r => r.OriginAirportId).ThenBy(r => r.DestinationAirportId)
                );

                // Manual hydration for DTO mapping (workaround for GetPagedAsync limitations)
                var dtos = new List<RouteDto>();
                foreach (var route in routes)
                {
                    if (route.OriginAirport == null)
                        route.OriginAirport = await _unitOfWork.Airports.GetByIataCodeAsync(route.OriginAirportId);
                    if (route.DestinationAirport == null)
                        route.DestinationAirport = await _unitOfWork.Airports.GetByIataCodeAsync(route.DestinationAirportId);

                    dtos.Add(_mapper.Map<RouteDto>(route));
                }

                var paginatedResult = new PaginatedResult<RouteDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<RouteDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching routes with filter on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<RouteDto>>.Failure("An error occurred during route search.");
            }
        }

        // Creates a new flight route.
        public async Task<ServiceResult<RouteDto>> CreateRouteAsync(CreateRouteDto createDto)
        {
            var originCode = createDto.OriginAirportIataCode.ToUpper();
            var destCode = createDto.DestinationAirportIataCode.ToUpper();

            // Validate: Same origin and destination
            if (originCode == destCode)
                return ServiceResult<RouteDto>.Failure("Origin and destination airports cannot be the same.");

            // Validate: Airports exist and are active
            var originAirport = await _unitOfWork.Airports.GetByIataCodeAsync(originCode);
            var destAirport = await _unitOfWork.Airports.GetByIataCodeAsync(destCode);

            if (originAirport == null)
                return ServiceResult<RouteDto>.Failure($"Origin airport '{originCode}' not found or is inactive.");
            if (destAirport == null)
                return ServiceResult<RouteDto>.Failure($"Destination airport '{destCode}' not found or is inactive.");

            // Validate: Route doesn't already exist (check active and inactive)
            if (await _unitOfWork.Routes.ExistsBetweenAirportsAsync(originCode, destCode))
            {
                var existing = await _unitOfWork.Routes.FindByOriginDestinationAsync(originCode, destCode);
                if (existing.Any(r => !r.IsDeleted))
                    return ServiceResult<RouteDto>.Failure($"An active route from {originCode} to {destCode} already exists.");
                else
                    return ServiceResult<RouteDto>.Failure($"A route from {originCode} to {destCode} already exists but is soft-deleted. Reactivate it instead.");
            }

            try
            {
                var newRoute = _mapper.Map<Route>(createDto);

                await _unitOfWork.Routes.AddAsync(newRoute);
                await _unitOfWork.SaveChangesAsync();

                // Assign airports for DTO mapping
                newRoute.OriginAirport = originAirport;
                newRoute.DestinationAirport = destAirport;

                _logger.LogInformation("Successfully created route ID {RouteId} from {Origin} to {Destination}.", newRoute.RouteId, originCode, destCode);
                return ServiceResult<RouteDto>.Success(_mapper.Map<RouteDto>(newRoute));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route from {Origin} to {Destination}.", originCode, destCode);
                return ServiceResult<RouteDto>.Failure("An unexpected error occurred while creating the route.");
            }
        }

        // Updates an existing route's details (distance only).
        public async Task<ServiceResult<RouteDto>> UpdateRouteAsync(int routeId, UpdateRouteDto updateDto)
        {
            _logger.LogInformation("Attempting to update route ID {RouteId}.", routeId);

            // Use GetWithAirportsAsync to ensure related objects (for full DTO mapping) are included
            var route = await _unitOfWork.Routes.GetWithAirportsAsync(routeId);

            if (route == null)
                return ServiceResult<RouteDto>.Failure($"Active route with ID {routeId} not found.");

            try
            {
                // 1. Map only the distance property
                _mapper.Map(updateDto, route); // Assumes mapping profile handles updating only DistanceKm

                _unitOfWork.Routes.Update(route);
                await _unitOfWork.SaveChangesAsync();

                // 2. Map the updated entity back to a DTO for the response.
                var updatedDto = _mapper.Map<RouteDto>(route);

                _logger.LogInformation("Successfully updated distance for route ID {RouteId}.", routeId);
                // 3. Return success with the updated DTO
                return ServiceResult<RouteDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route ID {RouteId}.", routeId);
                return ServiceResult<RouteDto>.Failure("An error occurred while updating the route.");
            }
        }

        // Soft deletes a route.
        public async Task<ServiceResult> DeleteRouteAsync(int routeId)
        {
            _logger.LogInformation("Attempting to soft-delete route ID {RouteId}.", routeId);
            var route = await _unitOfWork.Routes.GetActiveByIdAsync(routeId);
            if (route == null)
                return ServiceResult.Failure($"Active route with ID {routeId} not found.");

            // Check for dependencies: Active Flight Schedules
            bool hasActiveSchedules = await _unitOfWork.FlightSchedules.AnyAsync(fs => fs.RouteId == routeId && !fs.IsDeleted);
            if (hasActiveSchedules)
            {
                _logger.LogWarning("Failed to delete route {RouteId}: active flight schedules exist.", routeId);
                return ServiceResult.Failure($"Cannot delete route ID {routeId}. It is used by one or more active flight schedules. Please delete schedules first.");
            }

            // Also need to soft-delete all RouteOperators associated with this route
            var operators = await _unitOfWork.RouteOperators.GetOperatorsByRouteAsync(routeId);
            foreach (var op in operators)
            {
                _unitOfWork.RouteOperators.SoftDelete(op);
            }

            _unitOfWork.Routes.SoftDelete(route);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully soft-deleted route ID {RouteId} and its operators.", routeId);
            return ServiceResult.Success();
        }

        // Reactivates a soft-deleted route.
        public async Task<ServiceResult> ReactivateRouteAsync(int routeId)
        {
            _logger.LogInformation("Attempting to reactivate route ID {RouteId}.", routeId);
            var route = await _unitOfWork.Routes.GetByIdAsync(routeId); // Get by PK (finds deleted)
            if (route == null)
                return ServiceResult.Failure($"Route with ID {routeId} not found.");
            if (!route.IsDeleted)
                return ServiceResult.Failure($"Route ID {routeId} is already active.");

            try
            {
                route.IsDeleted = false;
                _unitOfWork.Routes.Update(route);

                // Also reactivate associated RouteOperators
                var operators = await _unitOfWork.RouteOperators.GetAllIncludingDeletedAsync();
                foreach (var op in operators.Where(o => o.RouteId == routeId && o.IsDeleted))
                {
                    op.IsDeleted = false;
                    _unitOfWork.RouteOperators.Update(op);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully reactivated route ID {RouteId} and its operators.", routeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating route ID {RouteId}.", routeId);
                return ServiceResult.Failure("An error occurred during reactivation.");
            }
        }

        // --- Route Operator Methods ---

        // Retrieves a route with its associated airlines.
        public async Task<ServiceResult<RouteDetailDto>> GetRouteWithOperatorsAsync(int routeId)
        {
            try
            {
                var route = await _unitOfWork.Routes.GetWithOperatorsAsync(routeId); // Repo method includes RouteOperators.Airline
                if (route == null)
                {
                    _logger.LogWarning("Route details with operators for ID {RouteId} not found or inactive.", routeId);
                    return ServiceResult<RouteDetailDto>.Failure($"Route with ID {routeId} not found or is inactive.");
                }

                // Need to load airports for the base DTO
                if (route.OriginAirport == null)
                    route.OriginAirport = await _unitOfWork.Airports.GetByIataCodeAsync(route.OriginAirportId);
                if (route.DestinationAirport == null)
                    route.DestinationAirport = await _unitOfWork.Airports.GetByIataCodeAsync(route.DestinationAirportId);

                var dto = _mapper.Map<RouteDetailDto>(route);
                return ServiceResult<RouteDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route with operators for ID {RouteId}.", routeId);
                return ServiceResult<RouteDetailDto>.Failure("An error occurred while retrieving route details.");
            }
        }

        // Assigns an airline to a route.
        public async Task<ServiceResult<RouteOperatorDto>> AssignOperatorToRouteAsync(AssignOperatorDto assignDto)
        {
            // Validate Route exists and is active
            var route = await _unitOfWork.Routes.GetActiveByIdAsync(assignDto.RouteId);
            if (route == null)
                return ServiceResult<RouteOperatorDto>.Failure($"Active route with ID {assignDto.RouteId} not found.");

            // Validate Airline exists and is active
            var airline = await _unitOfWork.Airlines.GetByIataCodeAsync(assignDto.AirlineIataCode.ToUpper());
            if (airline == null)
                return ServiceResult<RouteOperatorDto>.Failure($"Active airline with IATA code '{assignDto.AirlineIataCode}' not found.");

            // Check if assignment already exists (even if soft-deleted)
            var existingAssignment = await _unitOfWork.RouteOperators.GetByIdAsync(route.RouteId, airline.IataCode);
            if (existingAssignment != null)
            {
                if (!existingAssignment.IsDeleted)
                    return ServiceResult<RouteOperatorDto>.Failure($"Airline '{airline.Name}' is already assigned to this route.");

                // If soft-deleted, reactivate and update it
                existingAssignment.IsDeleted = false;
                existingAssignment.CodeshareStatus = assignDto.IsCodeshare;
                _unitOfWork.RouteOperators.Update(existingAssignment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Reactivated and updated operator {AirlineIata} on route {RouteId}.", airline.IataCode, route.RouteId);
                return ServiceResult<RouteOperatorDto>.Success(_mapper.Map<RouteOperatorDto>(existingAssignment));
            }

            // If no existing assignment, create new one
            try
            {
                var newOperator = _mapper.Map<RouteOperator>(assignDto);
                await _unitOfWork.RouteOperators.AddAsync(newOperator);
                await _unitOfWork.SaveChangesAsync();

                newOperator.Airline = airline; // Attach airline for DTO mapping
                _logger.LogInformation("Successfully assigned operator {AirlineIata} to route {RouteId}.", airline.IataCode, route.RouteId);
                return ServiceResult<RouteOperatorDto>.Success(_mapper.Map<RouteOperatorDto>(newOperator));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning operator {AirlineIata} to route {RouteId}.", airline.IataCode, route.RouteId);
                return ServiceResult<RouteOperatorDto>.Failure("An error occurred while assigning the operator.");
            }
        }

        // Updates the codeshare status for an airline on a route.
        public async Task<ServiceResult<RouteOperatorDto>> UpdateOperatorOnRouteAsync(int routeId, string airlineIataCode, UpdateOperatorDto updateDto)
        {
            var airlineCode = airlineIataCode.ToUpper();

            // We need to retrieve the operator including the Airline object for the DTO mapping
            var routeOperator = await _unitOfWork.RouteOperators.GetActiveByIdAsync(routeId, airlineCode);

            if (routeOperator == null)
                return ServiceResult<RouteOperatorDto>.Failure($"No active assignment found for airline '{airlineCode}' on route {routeId}.");

            try
            {
                // 1. Update the property manually (or use mapper for simple updates)
                routeOperator.CodeshareStatus = updateDto.IsCodeshare;

                _unitOfWork.RouteOperators.Update(routeOperator);
                await _unitOfWork.SaveChangesAsync();

                // 2. Map the updated entity back to a DTO for the response.
                var updatedDto = _mapper.Map<RouteOperatorDto>(routeOperator);

                _logger.LogInformation("Successfully updated codeshare status for {AirlineIata} on route {RouteId}.", airlineCode, routeId);
                // 3. Return success with the updated DTO
                return ServiceResult<RouteOperatorDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating codeshare status for {AirlineIata} on route {RouteId}.", airlineCode, routeId);
                return ServiceResult<RouteOperatorDto>.Failure("An error occurred while updating the operator.");
            }
        }

        // Removes an airline from a route (soft delete).
        public async Task<ServiceResult> RemoveOperatorFromRouteAsync(int routeId, string airlineIataCode)
        {
            var airlineCode = airlineIataCode.ToUpper();
            _logger.LogInformation("Attempting to soft-delete operator {AirlineIata} from route {RouteId}.", airlineCode, routeId);

            var routeOperator = await _unitOfWork.RouteOperators.GetActiveByIdAsync(routeId, airlineCode);
            if (routeOperator == null)
                return ServiceResult.Failure($"No active assignment found for airline '{airlineCode}' on route {routeId}.");

            // Check for dependencies: Active Flight Schedules using this *specific* operator
            bool hasActiveSchedules = await _unitOfWork.FlightSchedules.AnyAsync(
                fs => fs.RouteId == routeId &&
                      fs.AirlineId == airlineCode &&
                      !fs.IsDeleted
            );

            if (hasActiveSchedules)
            {
                _logger.LogWarning("Failed to remove operator {AirlineIata} from route {RouteId}: active schedules exist.", airlineCode, routeId);
                return ServiceResult.Failure($"Cannot remove airline '{airlineCode}' from route {routeId}. It is assigned to active flight schedules. Please delete schedules first.");
            }

            try
            {
                _unitOfWork.RouteOperators.SoftDelete(routeOperator);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully soft-deleted operator {AirlineIata} from route {RouteId}.", airlineCode, routeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting operator {AirlineIata} from route {RouteId}.", airlineCode, routeId);
                return ServiceResult.Failure("An error occurred during operator removal.");
            }
        }

         
    }
}