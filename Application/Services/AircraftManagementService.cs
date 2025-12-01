using Application.DTOs.Aircraft;
using Application.DTOs.Seat;
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
    // Service implementation for managing the aircraft fleet and configurations.
    public class AircraftManagementService : IAircraftManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AircraftManagementService> _logger;

        // Constructor for dependency injection
        public AircraftManagementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AircraftManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // --- Aircraft Methods ---

        // Retrieves a single active aircraft by its tail number.
        public async Task<ServiceResult<AircraftDto>> GetAircraftByTailNumberAsync(string tailNumber)
        {
            try
            {
                // Use the repository method that includes Airline and AircraftType
                var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(tailNumber); // Assuming this includes details
                if (aircraft == null)
                {
                    _logger.LogWarning("Active aircraft with TailNumber {TailNumber} not found.", tailNumber);
                    return ServiceResult<AircraftDto>.Failure($"Aircraft with tail number '{tailNumber}' not found or is inactive.");
                }

                var dto = _mapper.Map<AircraftDto>(aircraft);
                return ServiceResult<AircraftDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft with TailNumber {TailNumber}.", tailNumber);
                return ServiceResult<AircraftDto>.Failure("An error occurred while retrieving aircraft data.");
            }
        }

        // Retrieves a paginated list of active aircraft based on advanced filters.
        public async Task<ServiceResult<PaginatedResult<AircraftDto>>> GetAircraftPaginatedAsync(AircraftFilterDto filter, int pageNumber, int pageSize)
        {
            try
            {
                // Build filter expression
                Expression<Func<Aircraft, bool>> filterExpression = a => (filter.IncludeDeleted || !a.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.AirlineIataCode))
                    filterExpression = filterExpression.And(a => a.AirlineId == filter.AirlineIataCode);
                if (filter.AircraftTypeId.HasValue)
                    filterExpression = filterExpression.And(a => a.AircraftTypeId == filter.AircraftTypeId.Value);
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    var upperStatus = filter.Status.ToUpper();
                    filterExpression = filterExpression.And(a => a.Status.ToUpper() == upperStatus);
                }
                if (filter.MinFlightHours.HasValue)
                    filterExpression = filterExpression.And(a => a.TotalFlightHours >= filter.MinFlightHours.Value);
                if (filter.MaxFlightHours.HasValue)
                    filterExpression = filterExpression.And(a => a.TotalFlightHours <= filter.MaxFlightHours.Value);
                if (filter.AcquiredAfterDate.HasValue)
                    filterExpression = filterExpression.And(a => a.AcquisitionDate >= filter.AcquiredAfterDate.Value);

                // Fetch paged results
                var (aircraftList, totalCount) = await _unitOfWork.Aircrafts.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(a => a.AirlineId).ThenBy(a => a.TailNumber)
                );
                 

                // Manual hydration (less efficient but works if GetPagedAsync is simple)
                var dtos = new List<AircraftDto>();
                foreach (var aircraft in aircraftList)
                {
                    // Manually load if null (N+1 query issue, better to fix in Repo)
                    if (aircraft.Airline == null)
                        aircraft.Airline = await _unitOfWork.Airlines.GetByIataCodeAsync(aircraft.AirlineId);
                    if (aircraft.AircraftType == null)
                        aircraft.AircraftType = await _unitOfWork.AircraftTypes.GetActiveByIdAsync(aircraft.AircraftTypeId);

                    dtos.Add(_mapper.Map<AircraftDto>(aircraft));
                }

                var paginatedResult = new PaginatedResult<AircraftDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<AircraftDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated aircraft.");
                return ServiceResult<PaginatedResult<AircraftDto>>.Failure("An error occurred while retrieving aircraft.");
            }
        }

        // Retrieves full details for a single aircraft.
        public async Task<ServiceResult<AircraftDetailDto>> GetAircraftDetailsAsync(string tailNumber)
        {
            try
            {
                var aircraft = await _unitOfWork.Aircrafts.GetWithDetailsAsync(tailNumber);
                if (aircraft == null)
                {
                    _logger.LogWarning("Active aircraft details for TailNumber {TailNumber} not found.", tailNumber);
                    return ServiceResult<AircraftDetailDto>.Failure($"Aircraft details for '{tailNumber}' not found or is inactive.");
                }

                var dto = _mapper.Map<AircraftDetailDto>(aircraft);

                // Calculate seat counts for each cabin class
                foreach (var config in dto.Configurations)
                {
                    foreach (var cabin in config.CabinClasses)
                    {
                        // This mapping is now done in AutoMapper profile
                        // cabin.SeatCount = await _unitOfWork.Seats.CountAsync(s => s.CabinClassFk == cabin.CabinClassId && !s.IsDeleted);
                    }
                }

                return ServiceResult<AircraftDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving full details for aircraft {TailNumber}.", tailNumber);
                return ServiceResult<AircraftDetailDto>.Failure("An error occurred while retrieving aircraft details.");
            }
        }

        // Creates a new aircraft record.
        public async Task<ServiceResult<AircraftDto>> CreateAircraftAsync(CreateAircraftDto createDto)
        {
            _logger.LogInformation("Attempting to create aircraft with TailNumber {TailNumber}.", createDto.TailNumber);
            var tailNumberUpper = createDto.TailNumber.ToUpper();

            // Validation
            if (await _unitOfWork.Aircrafts.ExistsByTailNumberAsync(tailNumberUpper))
                return ServiceResult<AircraftDto>.Failure($"Aircraft with tail number '{tailNumberUpper}' already exists.");
            if (await _unitOfWork.Airlines.GetByIataCodeAsync(createDto.AirlineIataCode) == null)
                return ServiceResult<AircraftDto>.Failure($"Airline '{createDto.AirlineIataCode}' not found or is inactive.");
            if (await _unitOfWork.AircraftTypes.GetActiveByIdAsync(createDto.AircraftTypeId) == null)
                return ServiceResult<AircraftDto>.Failure($"Aircraft Type ID {createDto.AircraftTypeId} not found or is inactive.");

            try
            {
                var newAircraft = _mapper.Map<Aircraft>(createDto);

                await _unitOfWork.Aircrafts.AddAsync(newAircraft);
                await _unitOfWork.SaveChangesAsync();

                // Load related data for response DTO
                newAircraft.Airline = await _unitOfWork.Airlines.GetByIataCodeAsync(newAircraft.AirlineId);
                newAircraft.AircraftType = await _unitOfWork.AircraftTypes.GetActiveByIdAsync(newAircraft.AircraftTypeId);

                _logger.LogInformation("Successfully created aircraft with TailNumber {TailNumber}.", newAircraft.TailNumber);
                return ServiceResult<AircraftDto>.Success(_mapper.Map<AircraftDto>(newAircraft));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating aircraft {TailNumber}.", createDto.TailNumber);
                return ServiceResult<AircraftDto>.Failure($"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating aircraft {TailNumber}.", createDto.TailNumber);
                return ServiceResult<AircraftDto>.Failure("An unexpected error occurred.");
            }
        }

        // Updates an existing aircraft's core properties.
        public async Task<ServiceResult<AircraftDto>> UpdateAircraftAsync(string tailNumber, UpdateAircraftDto updateDto)
        {
            _logger.LogInformation("Attempting to update aircraft {TailNumber}.", tailNumber);

            // 1. Fetch the aircraft. The repository method (AircraftRepository.cs) already includes Airline and AircraftType.
            var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(tailNumber);

            if (aircraft == null)
                return ServiceResult<AircraftDto>.Failure($"Active aircraft '{tailNumber}' not found.");

            // 2. Validation: Check if the new Airline exists
            if (aircraft.AirlineId != updateDto.AirlineIataCode.ToUpperInvariant()) // Only re-validate if changing
            {
                if (await _unitOfWork.Airlines.GetByIataCodeAsync(updateDto.AirlineIataCode) == null)
                    return ServiceResult<AircraftDto>.Failure($"Airline '{updateDto.AirlineIataCode}' not found or is inactive.");
            }

            // 3. Validation: Check if the new Aircraft Type exists
            if (aircraft.AircraftTypeId != updateDto.AircraftTypeId) // Only re-validate if changing
            {
                if (await _unitOfWork.AircraftTypes.GetActiveByIdAsync(updateDto.AircraftTypeId) == null)
                    return ServiceResult<AircraftDto>.Failure($"Aircraft Type ID {updateDto.AircraftTypeId} not found or is inactive.");
            }

            try
            {
                // Check for changes before mapping/saving
                bool changed =
                    aircraft.AirlineId != updateDto.AirlineIataCode.ToUpperInvariant() ||
                    aircraft.AircraftTypeId != updateDto.AircraftTypeId ||
                    aircraft.AcquisitionDate != updateDto.AcquisitionDate;

                if (!changed)
                {
                    // If no changes, return the current DTO (Mapping is done in the next step to ensure it works)
                    var currentDto = _mapper.Map<AircraftDto>(aircraft);
                    return ServiceResult<AircraftDto>.Success(currentDto);
                }

                // Apply updates (AutoMapper handles mapping from DTO to entity)
                _mapper.Map(updateDto, aircraft);

                // Ensure Ids are uppercase, as mapping might miss it if logic is only in Create map
                aircraft.AirlineId = aircraft.AirlineId.ToUpperInvariant();

                _unitOfWork.Aircrafts.Update(aircraft);
                await _unitOfWork.SaveChangesAsync();

                // 4. Eager load updated navigation properties if they were changed
                // Since GetByTailNumberAsync already loads them, we just need to ensure the entity context
                // correctly reflects the new IDs (especially if the relationships were loaded before the update).
                // A simple re-fetch with navigation properties is the safest way to guarantee a correct DTO.
                var updatedAircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(tailNumber);

                // 5. Map the updated entity to the DTO
                var updatedDto = _mapper.Map<AircraftDto>(updatedAircraft);

                _logger.LogInformation("Successfully updated aircraft {TailNumber}.", tailNumber);

                // 6. Return success with the updated DTO
                return ServiceResult<AircraftDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating aircraft {TailNumber}.", tailNumber);
                return ServiceResult<AircraftDto>.Failure("An error occurred while updating the aircraft.");
            }
        }

        // Soft deletes an aircraft.
        public async Task<ServiceResult> DeleteAircraftAsync(string tailNumber)
        {
            var tailNumberUpper = tailNumber.ToUpperInvariant();
            _logger.LogInformation("Attempting to soft-delete aircraft {TailNumber}.", tailNumber);
            var aircraft = await _unitOfWork.Aircrafts.GetWithDetailsAsync(tailNumber);
            if (aircraft == null)
                return ServiceResult.Failure($"Active aircraft '{tailNumber}' not found.");

            // Check for active dependencies (Flight Instances)
            bool hasActiveInstances = await _unitOfWork.FlightInstances.AnyAsync(
                fi => fi.AircraftId == tailNumber &&
                      !fi.IsDeleted &&
                      fi.Status != "Arrived" && // Example of checking *operational* status
                      fi.Status != "Cancelled"
            );

            if (hasActiveInstances)
            {
                _logger.LogWarning("Failed to delete aircraft {TailNumber}: active flight instances exist.", tailNumber);
                return ServiceResult.Failure($"Cannot delete aircraft '{tailNumber}'. It is assigned to active or upcoming flights.");
            }

            // Check for ACTIVE Aircraft Configurations associated with this aircraft.
            // If a configuration exists and is not deleted, deletion should be blocked.
            bool hasActiveConfigs = await _unitOfWork.AircraftConfigs.AnyAsync(
                config => config.AircraftId == tailNumberUpper && !config.IsDeleted
            );

            if (hasActiveConfigs)
            {
                _logger.LogWarning("Deletion failed for aircraft {TailNumber}: active configurations exist.", tailNumberUpper);
                return ServiceResult.Failure(
                    $"Cannot delete aircraft '{tailNumberUpper}'. It has active flight configurations (AircraftConfig) linked to it. Please delete or soft-delete them first."
                );
            }

            try
            {
                  
                _unitOfWork.Aircrafts.SoftDelete(aircraft);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted aircraft {TailNumber}.", tailNumber);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting aircraft {TailNumber}.", tailNumber);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // Reactivates a soft-deleted aircraft.
        public async Task<ServiceResult> ReactivateAircraftAsync(string tailNumber)
        {
            _logger.LogInformation("Attempting to reactivate aircraft {TailNumber}.", tailNumber);
            var aircraft = await _unitOfWork.Aircrafts.GetByIdAsync(tailNumber); // Find by PK, regardless of IsDeleted

            if (aircraft == null)
                return ServiceResult.Failure($"Aircraft '{tailNumber}' not found.");
            if (!aircraft.IsDeleted)
                return ServiceResult.Failure($"Aircraft '{tailNumber}' is already active.");

            try
            {
                aircraft.IsDeleted = false;
                _unitOfWork.Aircrafts.Update(aircraft);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully reactivated aircraft {TailNumber}.", tailNumber);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating aircraft {TailNumber}.", tailNumber);
                return ServiceResult.Failure("An error occurred during reactivation.");
            }
        }

        // Updates the operational status of an aircraft.
        public async Task<ServiceResult> UpdateAircraftStatusAsync(string tailNumber, UpdateAircraftStatusDto dto)
        {
            _logger.LogInformation("Updating status for aircraft {TailNumber} to {Status}.", tailNumber, dto.Status);
            var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(tailNumber);
            if (aircraft == null)
                return ServiceResult.Failure($"Active aircraft '{tailNumber}' not found.");

            try
            {
                aircraft.Status = dto.Status;
                _unitOfWork.Aircrafts.Update(aircraft);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully updated status for aircraft {TailNumber}.", tailNumber);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for aircraft {TailNumber}.", tailNumber);
                return ServiceResult.Failure("An error occurred while updating aircraft status.");
            }
        }

        // Adds flight hours to an aircraft's total log.
        public async Task<ServiceResult<int?>> AddFlightHoursAsync(string tailNumber, AddFlightHoursDto dto)
        {
            _logger.LogInformation("Adding {Hours} flight hours to aircraft {TailNumber}.", dto.HoursToAdd, tailNumber);
            var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(tailNumber);
            if (aircraft == null)
                return ServiceResult<int?>.Failure($"Active aircraft '{tailNumber}' not found.");

            try
            {
                aircraft.TotalFlightHours = (aircraft.TotalFlightHours ?? 0) + dto.HoursToAdd;
                _unitOfWork.Aircrafts.Update(aircraft);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully added flight hours to {TailNumber}. New total: {TotalHours}", tailNumber, aircraft.TotalFlightHours);
                return ServiceResult<int?>.Success(aircraft.TotalFlightHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding flight hours for aircraft {TailNumber}.", tailNumber);
                return ServiceResult<int?>.Failure("An error occurred while adding flight hours.");
            }
        }


        // --- Aircraft Configuration Methods ---

        // Retrieves all active configurations for a specific aircraft.
        public async Task<ServiceResult<IEnumerable<AircraftConfigDto>>> GetConfigsForAircraftAsync(string tailNumber)
        {
            if (!await _unitOfWork.Aircrafts.ExistsByTailNumberAsync(tailNumber))
                return ServiceResult<IEnumerable<AircraftConfigDto>>.Failure($"Aircraft '{tailNumber}' not found.");

            var configs = await _unitOfWork.AircraftConfigs.GetByAircraftAsync(tailNumber);
            var dtos = _mapper.Map<IEnumerable<AircraftConfigDto>>(configs);
            return ServiceResult<IEnumerable<AircraftConfigDto>>.Success(dtos);
        }

        // Retrieves the details of a single configuration.
        public async Task<ServiceResult<AircraftConfigDto>> GetConfigDetailsAsync(int configId)
        {
            var config = await _unitOfWork.AircraftConfigs.GetWithCabinClassesAsync(configId);
            if (config == null)
                return ServiceResult<AircraftConfigDto>.Failure($"Configuration with ID {configId} not found or is inactive.");

            var dto = _mapper.Map<AircraftConfigDto>(config);
            return ServiceResult<AircraftConfigDto>.Success(dto);
        }

        // Creates a new configuration for an aircraft.
        public async Task<ServiceResult<AircraftConfigDto>> CreateAircraftConfigAsync(string tailNumber, CreateAircraftConfigDto createDto)
        {
            _logger.LogInformation("Adding new config '{ConfigName}' to aircraft {TailNumber}.", createDto.ConfigurationName, tailNumber);
            var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(tailNumber);
            if (aircraft == null)
                return ServiceResult<AircraftConfigDto>.Failure($"Active aircraft '{tailNumber}' not found.");

            // Check for duplicate config name on *this* aircraft
            if (await _unitOfWork.AircraftConfigs.ExistsByNameAsync(createDto.ConfigurationName, tailNumber))
                return ServiceResult<AircraftConfigDto>.Failure($"Configuration name '{createDto.ConfigurationName}' already exists for this aircraft.");

            try
            {
                var newConfig = _mapper.Map<AircraftConfig>(createDto);
                newConfig.AircraftId = tailNumber; // Set the foreign key

                await _unitOfWork.AircraftConfigs.AddAsync(newConfig);
                await _unitOfWork.SaveChangesAsync(); // Save to get the new ConfigId

                _logger.LogInformation("Successfully created config ID {ConfigId} for aircraft {TailNumber}.", newConfig.ConfigId, tailNumber);
                return ServiceResult<AircraftConfigDto>.Success(_mapper.Map<AircraftConfigDto>(newConfig));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating config '{ConfigName}' for aircraft {TailNumber}.", createDto.ConfigurationName, tailNumber);
                return ServiceResult<AircraftConfigDto>.Failure("An error occurred while creating the configuration.");
            }
        }

        // Updates an existing aircraft configuration's name.
        public async Task<ServiceResult<AircraftConfigDto>> UpdateAircraftConfigAsync(int configId, UpdateAircraftConfigDto updateDto)
        {
            _logger.LogInformation("Updating config ID {ConfigId}.", configId);
            // 1. Fetch the active configuration.
            var config = await _unitOfWork.AircraftConfigs.GetActiveByIdAsync(configId);
            if (config == null)
                return ServiceResult<AircraftConfigDto>.Failure($"Active configuration with ID {configId} not found.");

            // 2. Check for name conflict (excluding self)
            var upperConfigName = updateDto.ConfigurationName.ToUpper();
            if (await _unitOfWork.AircraftConfigs.AnyAsync(
                ac => ac.AircraftId == config.AircraftId && // Assuming configurations are unique per aircraft
                      ac.ConfigId != configId &&
                      ac.ConfigurationName.ToUpper() == upperConfigName))
            {
                return ServiceResult<AircraftConfigDto>.Failure($"Configuration name '{updateDto.ConfigurationName}' already exists for this aircraft.");
            }

            try
            {
                // 3. Apply updates using AutoMapper
                // This line updates properties on the 'config' entity from the 'updateDto'.
                _mapper.Map(updateDto, config);

                // 4. Check for changes (Optional, but good practice if not done by mapping)
                // Since we are using Map, we assume changes are intended. We rely on SaveChangesAsync to track and persist.

                _unitOfWork.AircraftConfigs.Update(config);
                await _unitOfWork.SaveChangesAsync();

                // 5. Map the updated entity to the DTO for the response.
                var updatedDto = _mapper.Map<AircraftConfigDto>(config);

                _logger.LogInformation("Successfully updated config ID {ConfigId}.", configId);
                // 6. Return success with the updated DTO.
                return ServiceResult<AircraftConfigDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating config ID {ConfigId}.", configId);
                return ServiceResult<AircraftConfigDto>.Failure("An error occurred while updating the configuration.");
            }
        }

        // Soft deletes an aircraft configuration.
        public async Task<ServiceResult> DeleteAircraftConfigAsync(int configId)
        {
            _logger.LogInformation("Attempting to soft-delete config ID {ConfigId}.", configId);
            var config = await _unitOfWork.AircraftConfigs.GetActiveByIdAsync(configId);
            if (config == null)
                return ServiceResult.Failure($"Active configuration with ID {configId} not found.");

            // Check for dependencies: Active Cabin Classes
            bool hasActiveCabins = await _unitOfWork.CabinClasses.AnyAsync(cc => cc.ConfigId == configId && !cc.IsDeleted);
            if (hasActiveCabins)
            {
                _logger.LogWarning("Failed to delete config {ConfigId}: active cabin classes exist.", configId);
                return ServiceResult.Failure($"Cannot delete configuration ID {configId}. It still has active cabin classes. Please delete cabin classes first.");
            }

            try
            {
                _unitOfWork.AircraftConfigs.SoftDelete(config);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted config ID {ConfigId}.", configId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting config ID {ConfigId}.", configId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // --- Cabin Class Methods ---

        // Retrieves all active cabin classes for a specific configuration.
        public async Task<ServiceResult<IEnumerable<CabinClassDto>>> GetCabinClassesForConfigAsync(int configId)
        {
            if (!await _unitOfWork.AircraftConfigs.AnyAsync(ac => ac.ConfigId == configId))
                return ServiceResult<IEnumerable<CabinClassDto>>.Failure($"Configuration ID {configId} not found.");

            var cabins = await _unitOfWork.CabinClasses.GetByConfigurationAsync(configId);
            var dtos = _mapper.Map<IEnumerable<CabinClassDto>>(cabins);
            return ServiceResult<IEnumerable<CabinClassDto>>.Success(dtos);
        }

        // Creates a new cabin class within a configuration.
        public async Task<ServiceResult<CabinClassDto>> CreateCabinClassAsync(CreateCabinClassDto createDto)
        {
            _logger.LogInformation("Adding new cabin class '{CabinName}' to config ID {ConfigId}.", createDto.Name, createDto.ConfigId);

            // Check if parent config exists
            var config = await _unitOfWork.AircraftConfigs.GetActiveByIdAsync(createDto.ConfigId);
            if (config == null)
                return ServiceResult<CabinClassDto>.Failure($"Active configuration with ID {createDto.ConfigId} not found.");

            // Check for duplicate cabin class name in *this* config
            if (await _unitOfWork.CabinClasses.ExistsByNameAsync(createDto.Name, createDto.ConfigId))
                return ServiceResult<CabinClassDto>.Failure($"Cabin class name '{createDto.Name}' already exists for this configuration.");

            try
            {
                var newCabin = _mapper.Map<CabinClass>(createDto);

                await _unitOfWork.CabinClasses.AddAsync(newCabin);
                await _unitOfWork.SaveChangesAsync(); // Save to get new CabinClassId

                _logger.LogInformation("Successfully created cabin class ID {CabinClassId} for config {ConfigId}.", newCabin.CabinClassId, newCabin.ConfigId);
                return ServiceResult<CabinClassDto>.Success(_mapper.Map<CabinClassDto>(newCabin));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cabin class '{CabinName}' for config {ConfigId}.", createDto.Name, createDto.ConfigId);
                return ServiceResult<CabinClassDto>.Failure("An error occurred while creating the cabin class.");
            }
        }

        // Soft deletes a cabin class.
        public async Task<ServiceResult> DeleteCabinClassAsync(int cabinClassId)
        {
            _logger.LogInformation("Attempting to soft-delete cabin class ID {CabinClassId}.", cabinClassId);
            var cabin = await _unitOfWork.CabinClasses.GetActiveByIdAsync(cabinClassId);
            if (cabin == null)
                return ServiceResult.Failure($"Active cabin class with ID {cabinClassId} not found.");

            // Check for dependencies: Active Seats
            bool hasActiveSeats = await _unitOfWork.Seats.AnyAsync(s => s.CabinClassId == cabinClassId && !s.IsDeleted);
            if (hasActiveSeats)
            {
                _logger.LogWarning("Failed to delete cabin class {CabinClassId}: active seats exist.", cabinClassId);
                return ServiceResult.Failure($"Cannot delete cabin class ID {cabinClassId} ({cabin.Name}). It still has active seats assigned. Please delete seats first.");
            }

            try
            {
                _unitOfWork.CabinClasses.SoftDelete(cabin);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted cabin class ID {CabinClassId}.", cabinClassId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting cabin class ID {CabinClassId}.", cabinClassId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // Retrieves all seats for a specific cabin class (useful for seat map display).
        public async Task<ServiceResult<IEnumerable<object>>> GetSeatMapForCabinClassAsync(int cabinClassId)
        {
            // This implementation depends on a SeatDto
            // Assuming you have a SeatDto in Application.DTOs.Seat
              
            // Placeholder since SeatDto is not defined in this context
            _logger.LogInformation("Retrieving seat map for cabin class ID {CabinClassId}.", cabinClassId);
            //var seats = await _unitOfWork.Seats.GetByAircraftConfigAsync(cabinClassId); // Reusing existing method, assuming GetByAircraftConfigAsync actually means GetByCabinClass
            // Call the correct repository method
            var seats = await _unitOfWork.Seats.GetByCabinClassAsync(cabinClassId);

            if (seats == null)
            {
                return ServiceResult<IEnumerable<object>>.Failure("No seats found for this cabin class.");
            }

            var seatMap = seats.Select(s => new { s.SeatId, s.SeatNumber, s.IsWindow, s.IsExitRow });
            return ServiceResult<IEnumerable<object>>.Success(seatMap.Cast<object>());
        }
         

        #region --- Seat Methods ---

        // Creates a new seat for a specific cabin class.
        public async Task<ServiceResult<SeatDto>> CreateSeatAsync(CreateSeatDto createDto)
        {
            _logger.LogInformation("Creating new seat {SeatNumber} for CabinClassId {CabinClassId}.", createDto.SeatNumber, createDto.CabinClassId);

            // 1. Get Cabin Class and its parent Config/Aircraft
            var cabinClass = await _unitOfWork.CabinClasses.GetActiveByIdAsync(createDto.CabinClassId);
            if (cabinClass == null)
                return ServiceResult<SeatDto>.Failure($"Cabin Class ID {createDto.CabinClassId} not found.");

            var config = await _unitOfWork.AircraftConfigs.GetActiveByIdAsync(cabinClass.ConfigId);
            if (config == null)
                return ServiceResult<SeatDto>.Failure($"Parent Config ID {cabinClass.ConfigId} not found.");

            var aircraftId = config.AircraftId;

            // 2. Generate Seat ID (e.g., "1A-9V-SWA")
            var seatId = $"{createDto.SeatNumber.ToUpper()}-{aircraftId}";

            // 3. Check for uniqueness
            if (await _unitOfWork.Seats.ExistsByIdAsync(seatId))
                return ServiceResult<SeatDto>.Failure($"Seat ID '{seatId}' (Number: {createDto.SeatNumber}) already exists on this aircraft.");

            try
            {
                var newSeat = _mapper.Map<Seat>(createDto);
                newSeat.SeatId = seatId;
                newSeat.AircraftId = aircraftId; // Set the FK
                newSeat.IsDeleted = false;

                await _unitOfWork.Seats.AddAsync(newSeat);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created Seat ID {SeatId}.", seatId);

                // Load navigation property for DTO
                newSeat.CabinClass = cabinClass;
                return ServiceResult<SeatDto>.Success(_mapper.Map<SeatDto>(newSeat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seat {SeatNumber} for {CabinClassId}.", createDto.SeatNumber, createDto.CabinClassId);
                return ServiceResult<SeatDto>.Failure("An error occurred while creating the seat.");
            }
        }

        // Updates an existing seat.
        public async Task<ServiceResult<SeatDto>> UpdateSeatAsync(string seatId, UpdateSeatDto updateDto)
        {
            _logger.LogInformation("Updating seat {SeatId}.", seatId);

            // Ensure the seat is fetched, including its CabinClass for subsequent validation and mapping.
            var seat = await _unitOfWork.Seats.GetWithCabinClassAsync(seatId);
            if (seat == null)
                return ServiceResult<SeatDto>.Failure($"Active seat with ID '{seatId}' not found.");

            // Validate new Cabin Class ID
            if (seat.CabinClassId != updateDto.CabinClassId)
            {
                var newCabin = await _unitOfWork.CabinClasses.GetActiveByIdAsync(updateDto.CabinClassId);
                if (newCabin == null)
                    return ServiceResult<SeatDto>.Failure($"New Cabin Class ID {updateDto.CabinClassId} not found.");

                // Check if the new cabin belongs to the same aircraft
                // Ensure both old and new Configs are fetched or correctly loaded.
                // We rely on GetWithCabinClassAsync loading the old CabinClass and its ConfigId.

                // Fetch the old config's aircraft ID if the CabinClass navigation property was loaded correctly.
                var oldConfig = await _unitOfWork.AircraftConfigs.GetActiveByIdAsync(seat.CabinClass.ConfigId);
                var newConfig = await _unitOfWork.AircraftConfigs.GetActiveByIdAsync(newCabin.ConfigId);

                if (oldConfig?.AircraftId != newConfig?.AircraftId)
                    return ServiceResult<SeatDto>.Failure("Cannot move seat to a cabin class on a different aircraft.");

                // Update the navigation property if the CabinClassId changed successfully
                seat.CabinClass = newCabin;
            }
            // If CabinClassId didn't change, ensure CabinClass is loaded for DTO mapping
            else if (seat.CabinClass == null)
            {
                // This is a safety check; GetWithCabinClassAsync should have loaded it.
                seat.CabinClass = await _unitOfWork.CabinClasses.GetActiveByIdAsync(seat.CabinClassId);
            }

            try
            {
                // Map updates from DTO to the entity
                _mapper.Map(updateDto, seat);

                // Note: The SeatNumber (which makes up the ID) cannot be changed via UpdateSeatDto.

                _unitOfWork.Seats.Update(seat);
                await _unitOfWork.SaveChangesAsync();

                // After successful save, map the updated entity to the DTO
                var updatedDto = _mapper.Map<SeatDto>(seat);

                _logger.LogInformation("Successfully updated seat {SeatId}.", seatId);
                // Return success with the updated DTO
                return ServiceResult<SeatDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating seat {SeatId}.", seatId);
                // Return failure with generic error message
                return ServiceResult<SeatDto>.Failure("An error occurred while updating the seat.");
            }
        }

        // Soft deletes a seat.
        public async Task<ServiceResult> DeleteSeatAsync(string seatId)
        {
            _logger.LogInformation("Attempting to soft-delete seat {SeatId}.", seatId);
            var seat = await _unitOfWork.Seats.GetWithCabinClassAsync(seatId); // Assuming GetActiveByIdAsync
            if (seat == null)
                return ServiceResult.Failure($"Active seat '{seatId}' not found.");

            // Check for dependencies: Active seat assignments
            bool hasAssignments = await _unitOfWork.BookingPassengers.AnyAsync(bp => bp.SeatAssignmentId == seatId && !bp.IsDeleted);
            if (hasAssignments)
            {
                _logger.LogWarning("Failed to delete seat {SeatId}: active assignments exist.", seatId);
                return ServiceResult.Failure($"Cannot delete seat {seatId} ({seat.SeatNumber}). It is currently assigned in one or more bookings.");
            }

            try
            {
                _unitOfWork.Seats.SoftDelete(seat);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted seat {SeatId}.", seatId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting seat {SeatId}.", seatId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        #endregion


    }



}