using Application.DTOs.AircraftType;
using Application.Models;
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
    // Service implementation for managing Aircraft Type data.
    public class AircraftTypeService : IAircraftTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AircraftTypeService> _logger;

        // Constructor injection
        public AircraftTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AircraftTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Retrieves all active aircraft types.
        public async Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAllActiveAircraftTypesAsync()
        {
            try
            {
                var types = await _unitOfWork.AircraftTypes.GetAllActiveAsync();
                var dtos = _mapper.Map<IEnumerable<AircraftTypeDto>>(types);
                _logger.LogInformation("Successfully retrieved all active aircraft types.");
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active aircraft types.");
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("An error occurred while retrieving aircraft types.");
            }
        }

        // Retrieves a specific active aircraft type by ID.
        public async Task<ServiceResult<AircraftTypeDto>> GetAircraftTypeByIdAsync(int typeId)
        {
            try
            {
                var type = await _unitOfWork.AircraftTypes.GetActiveByIdAsync(typeId);
                if (type == null)
                {
                    _logger.LogWarning("Aircraft type with ID {TypeId} not found or inactive.", typeId);
                    return ServiceResult<AircraftTypeDto>.Failure($"Aircraft type with ID {typeId} not found or is inactive.");
                }

                var dto = _mapper.Map<AircraftTypeDto>(type);
                _logger.LogInformation("Successfully retrieved aircraft type with ID {TypeId}.", typeId);
                return ServiceResult<AircraftTypeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft type with ID {TypeId}.", typeId);
                return ServiceResult<AircraftTypeDto>.Failure($"An error occurred while retrieving aircraft type {typeId}.");
            }
        }

        // Finds active aircraft types by model substring.
        public async Task<ServiceResult<IEnumerable<AircraftTypeDto>>> FindAircraftTypesByModelAsync(string modelSubstring)
        {
            if (string.IsNullOrWhiteSpace(modelSubstring))
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("Model search term cannot be empty.");

            try
            {
                var types = await _unitOfWork.AircraftTypes.FindByModelAsync(modelSubstring);
                var dtos = _mapper.Map<IEnumerable<AircraftTypeDto>>(types);
                _logger.LogInformation("Found {Count} aircraft types matching model '{Model}'.", dtos.Count(), modelSubstring);
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding aircraft types by model '{Model}'.", modelSubstring);
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("An error occurred during the search.");
            }
        }

        // Retrieves active aircraft types by manufacturer.
        public async Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAircraftTypesByManufacturerAsync(string manufacturer)
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("Manufacturer name cannot be empty.");

            try
            {
                var types = await _unitOfWork.AircraftTypes.GetByManufacturerAsync(manufacturer);
                var dtos = _mapper.Map<IEnumerable<AircraftTypeDto>>(types);
                _logger.LogInformation("Found {Count} aircraft types for manufacturer '{Manufacturer}'.", dtos.Count(), manufacturer);
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft types by manufacturer '{Manufacturer}'.", manufacturer);
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("An error occurred while retrieving types by manufacturer.");
            }
        }

        // Performs an advanced search with pagination.
        public async Task<ServiceResult<PaginatedResult<AircraftTypeDto>>> SearchAircraftTypesAsync(AircraftTypeFilterDto filter, int pageNumber, int pageSize)
        {
            try
            {
                // Build filter dynamically
                Expression<Func<AircraftType, bool>> filterExpression = at => (filter.IncludeDeleted || !at.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.ModelContains))
                    filterExpression = filterExpression.And(at => EF.Functions.Like(at.Model, $"%{filter.ModelContains}%"));
                 
                if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
                {
                    var upperManufacturer = filter.Manufacturer.ToUpper();
                    filterExpression = filterExpression.And(at => at.Manufacturer.ToUpper() == upperManufacturer);
                }

                if (filter.MinRangeKm.HasValue) filterExpression = filterExpression.And(at => at.RangeKm >= filter.MinRangeKm.Value);
                if (filter.MaxRangeKm.HasValue) filterExpression = filterExpression.And(at => at.RangeKm <= filter.MaxRangeKm.Value);
                if (filter.MinSeats.HasValue) filterExpression = filterExpression.And(at => at.MaxSeats >= filter.MinSeats.Value);
                if (filter.MaxSeats.HasValue) filterExpression = filterExpression.And(at => at.MaxSeats <= filter.MaxSeats.Value);
                if (filter.MinCargoCapacity.HasValue) filterExpression = filterExpression.And(at => at.CargoCapacity >= filter.MinCargoCapacity.Value);
                if (filter.MaxCargoCapacity.HasValue) filterExpression = filterExpression.And(at => at.CargoCapacity <= filter.MaxCargoCapacity.Value);


                var (types, totalCount) = await _unitOfWork.AircraftTypes.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(at => at.Manufacturer).ThenBy(at => at.Model)
                );

                var dtos = _mapper.Map<List<AircraftTypeDto>>(types);
                var paginatedResult = new PaginatedResult<AircraftTypeDto>(dtos, totalCount, pageNumber, pageSize);

                _logger.LogInformation("Searched aircraft types page {PageNumber} with filter. Found {Count} total items.", pageNumber, totalCount);
                return ServiceResult<PaginatedResult<AircraftTypeDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching aircraft types with filter on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<AircraftTypeDto>>.Failure("An error occurred during the aircraft type search.");
            }
        }


        // Creates a new aircraft type.
        public async Task<ServiceResult<AircraftTypeDto>> CreateAircraftTypeAsync(CreateAircraftTypeDto createDto)
        {
           
            var upperModel = createDto.Model.ToUpper();
            var upperMan = createDto.Manufacturer.ToUpper();

            // Check for uniqueness (Model + Manufacturer combination)
            if (await _unitOfWork.AircraftTypes.AnyAsync(at => at.Model.ToUpper() == upperModel &&
                                                               at.Manufacturer.ToUpper() == upperMan))
            {
                return ServiceResult<AircraftTypeDto>.Failure($"An aircraft type with model '{createDto.Model}' from manufacturer '{createDto.Manufacturer}' already exists.");
            }

            try
            {
                var newType = _mapper.Map<AircraftType>(createDto);
                newType.IsDeleted = false; // Ensure active

                await _unitOfWork.AircraftTypes.AddAsync(newType);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<AircraftTypeDto>(newType);
                _logger.LogInformation("Successfully created aircraft type ID {TypeId} ({Model} by {Manufacturer}).", newType.TypeId, newType.Model, newType.Manufacturer);
                return ServiceResult<AircraftTypeDto>.Success(dto);
            }
            catch (DbUpdateException ex) // Catch specific DB errors
            {
                _logger.LogError(ex, "Database error creating aircraft type '{Model}' by {Manufacturer}.", createDto.Model, createDto.Manufacturer);
                return ServiceResult<AircraftTypeDto>.Failure($"A database error occurred while creating the aircraft type: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating aircraft type '{Model}' by {Manufacturer}.", createDto.Model, createDto.Manufacturer);
                return ServiceResult<AircraftTypeDto>.Failure("An unexpected error occurred while creating the aircraft type.");
            }
        }

        // Updates an existing aircraft type.
        public async Task<ServiceResult<AircraftTypeDto>> UpdateAircraftTypeAsync(int typeId, UpdateAircraftTypeDto updateDto)
        {
            try
            {
                var type = await _unitOfWork.AircraftTypes.GetActiveByIdAsync(typeId);
                if (type == null)
                {
                    return ServiceResult<AircraftTypeDto>.Failure($"Active aircraft type with ID {typeId} not found.");
                }

                // We only proceed if Model or Manufacturer in the DTO is provided and different
                string upperModel = type.Model.ToUpper(); // Default to current model
                string upperMan = type.Manufacturer.ToUpper(); // Default to current manufacturer

                // Update local variables if new values are provided
                if (!string.IsNullOrWhiteSpace(updateDto.Model))
                    upperModel = updateDto.Model.ToUpper();
                if (!string.IsNullOrWhiteSpace(updateDto.Manufacturer))
                    upperMan = updateDto.Manufacturer.ToUpper();

                // Check if the updated Model/Manufacturer combination conflicts with another existing type
                if (await _unitOfWork.AircraftTypes.AnyAsync(at => at.TypeId != typeId && // Exclude self
                                                                   at.Model.ToUpper() == upperModel &&
                                                                   at.Manufacturer.ToUpper() == upperMan))
                {
                    return ServiceResult<AircraftTypeDto>.Failure($"Another aircraft type with model '{updateDto.Model}' from manufacturer '{updateDto.Manufacturer}' already exists.");
                }


                // Use AutoMapper to map updated fields from DTO to entity. 
                // The mapping profile should handle ignoring nulls in the DTO for PUT/PATCH behavior.
                _mapper.Map(updateDto, type);
                _unitOfWork.AircraftTypes.Update(type);
                await _unitOfWork.SaveChangesAsync();

                // Map the updated entity back to a DTO for the response.
                var updatedDto = _mapper.Map<AircraftTypeDto>(type);

                _logger.LogInformation("Successfully updated aircraft type ID {TypeId}.", typeId);
                // Return success with the updated DTO
                return ServiceResult<AircraftTypeDto>.Success(updatedDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error updating aircraft type ID {TypeId}. The record may have been modified by another user.", typeId);
                return ServiceResult<AircraftTypeDto>.Failure("The aircraft type record was modified by another user. Please reload and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating aircraft type ID {TypeId}.", typeId);
                return ServiceResult<AircraftTypeDto>.Failure("An error occurred while updating the aircraft type.");
            }
        }

        // Soft deletes an aircraft type.
        public async Task<ServiceResult> DeleteAircraftTypeAsync(int typeId)
        {
            try
            {
                var type = await _unitOfWork.AircraftTypes.GetActiveByIdAsync(typeId);
                if (type == null)
                {
                    return ServiceResult.Failure($"Active aircraft type with ID {typeId} not found.");
                }

                // Check for dependencies: Active aircraft, active schedules
                bool hasActiveAircraft = await _unitOfWork.Aircrafts.AnyAsync(a => a.AircraftTypeId == typeId && !a.IsDeleted);
                bool hasActiveSchedules = await _unitOfWork.FlightSchedules.AnyAsync(fs => fs.AircraftTypeId == typeId && !fs.IsDeleted);

                if (hasActiveAircraft || hasActiveSchedules)
                {
                    List<string> dependencies = new();
                    if (hasActiveAircraft) dependencies.Add("active aircraft");
                    if (hasActiveSchedules) dependencies.Add("active flight schedules");
                    return ServiceResult.Failure($"Cannot delete aircraft type ID {typeId} ({type.Model}) as it is currently used by: {string.Join(", ", dependencies)}.");
                }

                _unitOfWork.AircraftTypes.SoftDelete(type);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully soft-deleted aircraft type ID {TypeId}.", typeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting aircraft type ID {TypeId}.", typeId);
                return ServiceResult.Failure("An error occurred while deleting the aircraft type.");
            }
        }

        // Reactivates a soft-deleted aircraft type.
        public async Task<ServiceResult> ReactivateAircraftTypeAsync(int typeId)
        {
            try
            {
                // Fetch including deleted using GetByIdAsync
                var type = await _unitOfWork.AircraftTypes.GetByIdAsync(typeId);
                if (type == null)
                {
                    return ServiceResult.Failure($"Aircraft type with ID {typeId} not found.");
                }

                if (!type.IsDeleted)
                {
                    return ServiceResult.Failure($"Aircraft type ID {typeId} is already active.");
                }

                type.IsDeleted = false; // Reactivate
                _unitOfWork.AircraftTypes.Update(type);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully reactivated aircraft type ID {TypeId}.", typeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating aircraft type ID {TypeId}.", typeId);
                return ServiceResult.Failure("An error occurred while reactivating the aircraft type.");
            }
        }

        // Retrieves all types, including soft-deleted ones.
        public async Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAllAircraftTypesIncludingDeletedAsync()
        {
            try
            {
                var types = await _unitOfWork.AircraftTypes.GetAllIncludingDeletedAsync();
                var dtos = _mapper.Map<IEnumerable<AircraftTypeDto>>(types);
                _logger.LogInformation("Successfully retrieved all aircraft types (including deleted).");
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all aircraft types (including deleted).");
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("An error occurred while retrieving all aircraft types.");
            }
        }

        // Retrieves aircraft types suitable for a given route distance.
        public async Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAircraftTypesSuitableForRouteAsync(int routeDistanceKm)
        {
            if (routeDistanceKm <= 0)
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("Route distance must be positive.");

            try
            {
                // Find types where range_km is greater than or equal to the route distance
                var types = await _unitOfWork.AircraftTypes.FindByCriteriaAsync(minRangeKm: routeDistanceKm);
                var dtos = _mapper.Map<IEnumerable<AircraftTypeDto>>(types);
                _logger.LogInformation("Found {Count} aircraft types suitable for distance {Distance} km.", dtos.Count(), routeDistanceKm);
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding suitable aircraft types for distance {Distance} km.", routeDistanceKm);
                return ServiceResult<IEnumerable<AircraftTypeDto>>.Failure("An error occurred while finding suitable aircraft types.");
            }
        }
    }
}