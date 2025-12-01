using Application.DTOs.FlightSchedule;
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

namespace Application.Services
{
    // Service implementation for managing Flight Schedules and Leg Definitions.
    public class FlightSchedulingService : IFlightSchedulingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FlightSchedulingService> _logger;

        
        public FlightSchedulingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FlightSchedulingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Retrieves a single active flight schedule by its ID.
        public async Task<ServiceResult<FlightScheduleDto>> GetScheduleByIdAsync(int scheduleId)
        {
            try
            {
                // Retrieve schedule with all necessary details (Route, Airports, Airline, Type)
                var schedule = await _unitOfWork.FlightSchedules.GetWithDetailsAsync(scheduleId);
                if (schedule == null)
                {
                    _logger.LogWarning("Schedule ID {ScheduleId} not found or inactive.", scheduleId);
                    return ServiceResult<FlightScheduleDto>.Failure($"Flight schedule with ID {scheduleId} not found or is inactive.");
                }

                // Map to DTO
                var dto = _mapper.Map<FlightScheduleDto>(schedule);

                // Add route name for clarity
                dto.RouteName = $"{schedule.Route.OriginAirport.IataCode} - {schedule.Route.DestinationAirport.IataCode}";

                _logger.LogInformation("Successfully retrieved schedule ID {ScheduleId}.", scheduleId);
                return ServiceResult<FlightScheduleDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule ID {ScheduleId}.", scheduleId);
                return ServiceResult<FlightScheduleDto>.Failure("An error occurred while retrieving the flight schedule.");
            }
        }

        // Performs an advanced, paginated search for routes.
        public async Task<ServiceResult<PaginatedResult<FlightScheduleDto>>> SearchSchedulesAsync(ScheduleFilterDto filter, int pageNumber, int pageSize)
        {
            try
            {
                Expression<Func<FlightSchedule, bool>> filterExpression = fs => (filter.IncludeDeleted || !fs.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.FlightNo))
                { 
                    var upperFlightNo = filter.FlightNo.ToUpper();
                    filterExpression = filterExpression.And(fs => fs.FlightNo.ToUpper() == upperFlightNo);
                }

                if (!string.IsNullOrWhiteSpace(filter.AirlineIataCode))
                    filterExpression = filterExpression.And(fs => fs.AirlineId == filter.AirlineIataCode);

                if (!string.IsNullOrWhiteSpace(filter.OriginIataCode))
                    filterExpression = filterExpression.And(fs => fs.Route.OriginAirportId == filter.OriginIataCode);
                if (!string.IsNullOrWhiteSpace(filter.DestinationIataCode))
                    filterExpression = filterExpression.And(fs => fs.Route.DestinationAirportId == filter.DestinationIataCode);

                if (filter.DepartureDate.HasValue)
                    filterExpression = filterExpression.And(fs => fs.DepartureTimeScheduled.Date == filter.DepartureDate.Value.Date);

 
                var (items, totalCount) = await _unitOfWork.FlightSchedules.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(fs => fs.DepartureTimeScheduled),
                    includeProperties: "Route.OriginAirport,Route.DestinationAirport,Airline,AircraftType"
                );

                // Map items to DTOs
                var dtos = new List<FlightScheduleDto>();
                foreach (var item in items)
                {
                    var dto = _mapper.Map<FlightScheduleDto>(item);
                    // Manually map fields not covered by basic includes
                    if (item.Route?.OriginAirport != null && item.Route?.DestinationAirport != null)
                    {
                        dto.RouteName = $"{item.Route.OriginAirport.IataCode} - {item.Route.DestinationAirport.IataCode}";
                    }
                    dtos.Add(dto);
                } 

                var paginatedResult = new PaginatedResult<FlightScheduleDto>(dtos, totalCount, pageNumber, pageSize);
                _logger.LogInformation("Searched schedules page {PageNumber}. Found {Count} total items.", pageNumber, totalCount);
                return ServiceResult<PaginatedResult<FlightScheduleDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flight schedules on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<FlightScheduleDto>>.Failure("An error occurred during the search.");
            }
        }

        // Creates a new flight schedule.
        public async Task<ServiceResult<FlightScheduleDto>> CreateScheduleAsync(CreateFlightScheduleDto createDto)
        {
            _logger.LogInformation("Attempting to create schedule {FlightNo}.", createDto.FlightNo);

            // 1. Validate Foreign Keys
            if (await _unitOfWork.Routes.GetActiveByIdAsync(createDto.RouteId) == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Route ID {createDto.RouteId} not found or is inactive.");
            if (await _unitOfWork.Airlines.GetByIataCodeAsync(createDto.AirlineIataCode) == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Airline code '{createDto.AirlineIataCode}' not found or is inactive.");
            if (await _unitOfWork.AircraftTypes.GetActiveByIdAsync(createDto.AircraftTypeId) == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Aircraft Type ID {createDto.AircraftTypeId} not found or is inactive.");

            // 2. Validate Business Logic (Time constraints, uniqueness)
            if (createDto.DepartureTimeScheduled >= createDto.ArrivalTimeScheduled)
                return ServiceResult<FlightScheduleDto>.Failure("Departure time must be before arrival time.");

            // Check if a schedule with the exact flight number exists for the scheduled days/times (complex check)
            if (await _unitOfWork.FlightSchedules.ExistsByFlightNumberAndDateAsync(createDto.FlightNo, createDto.DepartureTimeScheduled))
                return ServiceResult<FlightScheduleDto>.Failure($"A schedule with flight number '{createDto.FlightNo}' already exists for this date/time.");

            try
            {
                var newSchedule = _mapper.Map<FlightSchedule>(createDto);

                await _unitOfWork.FlightSchedules.AddAsync(newSchedule);
                await _unitOfWork.SaveChangesAsync();

                // Load details for the DTO response
                var detailedSchedule = await _unitOfWork.FlightSchedules.GetWithDetailsAsync(newSchedule.ScheduleId);
                var dto = _mapper.Map<FlightScheduleDto>(detailedSchedule);
                 

                // Add route name for clarity
                dto.RouteName = $"{detailedSchedule.Route.OriginAirport.IataCode} - {detailedSchedule.Route.DestinationAirport.IataCode}";

                _logger.LogInformation("Successfully created schedule ID {ScheduleId} for flight {FlightNo}.", newSchedule.ScheduleId, newSchedule.FlightNo);
                return ServiceResult<FlightScheduleDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule {FlightNo}.", createDto.FlightNo);
                return ServiceResult<FlightScheduleDto>.Failure("An error occurred while creating the flight schedule.");
            }
        }

        // Updates core details of an existing flight schedule.
        public async Task<ServiceResult<FlightScheduleDto>> UpdateScheduleAsync(int scheduleId, CreateFlightScheduleDto updateDto)
        {
            _logger.LogInformation("Attempting to update schedule ID {ScheduleId}.", scheduleId);
            var schedule = await _unitOfWork.FlightSchedules.GetActiveByIdAsync(scheduleId);
            if (schedule == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Active schedule with ID {scheduleId} not found.");

            // 1. Validate Foreign Keys
            if (schedule.RouteId != updateDto.RouteId && await _unitOfWork.Routes.GetActiveByIdAsync(updateDto.RouteId) == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Route ID {updateDto.RouteId} not found or is inactive.");
            if (schedule.AirlineId != updateDto.AirlineIataCode && await _unitOfWork.Airlines.GetByIataCodeAsync(updateDto.AirlineIataCode) == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Airline code '{updateDto.AirlineIataCode}' not found or is inactive.");
            if (schedule.AircraftTypeId != updateDto.AircraftTypeId && await _unitOfWork.AircraftTypes.GetActiveByIdAsync(updateDto.AircraftTypeId) == null)
                return ServiceResult<FlightScheduleDto>.Failure($"Aircraft Type ID {updateDto.AircraftTypeId} not found or is inactive.");

            // 2. Business Logic Validation
            if (updateDto.DepartureTimeScheduled >= updateDto.ArrivalTimeScheduled)
                return ServiceResult<FlightScheduleDto>.Failure("Departure time must be before arrival time.");

            // 3.Flight Number Uniqueness Check (Requested Feature)
            // Check if the flight number/date combo is changing and if it conflicts with *another* schedule.
            if (schedule.FlightNo != updateDto.FlightNo || schedule.DepartureTimeScheduled.Date != updateDto.DepartureTimeScheduled.Date)
            {
                // Use a repository method or AnyAsync to check for conflicts, excluding the current scheduleId
                // Assuming ExistsByFlightNumberAndDateAsync(flightNo, date) exists as implied by CreateScheduleAsync
                // We need a check that excludes the current scheduleId.

                bool conflictExists = await _unitOfWork.FlightSchedules.AnyAsync(
                    fs => fs.ScheduleId != scheduleId && // <-- Exclude self
                          fs.FlightNo == updateDto.FlightNo &&
                          fs.DepartureTimeScheduled.Date == updateDto.DepartureTimeScheduled.Date &&
                          !fs.IsDeleted
                );

                if (conflictExists)
                {
                    return ServiceResult<FlightScheduleDto>.Failure($"A schedule with flight number '{updateDto.FlightNo}' already exists for this date/time on another schedule.");
                }
            }

            // 4. Check for Active Flight Instances (if core schedule properties change)
            bool isScheduleCoreChanging = schedule.RouteId != updateDto.RouteId || schedule.AircraftTypeId != updateDto.AircraftTypeId;
            if (isScheduleCoreChanging)
            {
                bool hasActiveInstances = await _unitOfWork.FlightInstances.AnyAsync(
                    fi => fi.ScheduleId == scheduleId && !fi.IsDeleted && fi.Status != "Arrived" && fi.Status != "Cancelled"
                );
                if (hasActiveInstances)
                    return ServiceResult<FlightScheduleDto>.Failure("Cannot change route or aircraft type; active/upcoming flight instances exist for this schedule.");
            }

            try
            {
                // Apply updates via Mapper
                _mapper.Map(updateDto, schedule);

                _unitOfWork.FlightSchedules.Update(schedule);
                await _unitOfWork.SaveChangesAsync();

                // 5. Re-fetch the detailed entity to include navigation properties for the DTO
                var detailedSchedule = await _unitOfWork.FlightSchedules.GetWithDetailsAsync(scheduleId);
                if (detailedSchedule == null)
                {
                    _logger.LogError("Failed to re-fetch updated schedule {ScheduleId} after update.", scheduleId);
                    return ServiceResult<FlightScheduleDto>.Failure("Schedule updated but could not be retrieved successfully.");
                }

                var dto = _mapper.Map<FlightScheduleDto>(detailedSchedule);

                // 6.Add RouteName (as seen in Create/Get methods)
                dto.RouteName = $"{detailedSchedule.Route.OriginAirport.IataCode} - {detailedSchedule.Route.DestinationAirport.IataCode}";

                _logger.LogInformation("Successfully updated schedule ID {ScheduleId}.", scheduleId);

                // 7.Return the updated DTO
                return ServiceResult<FlightScheduleDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule ID {ScheduleId}.", scheduleId);
                return ServiceResult<FlightScheduleDto>.Failure("An error occurred while updating the schedule.");
            }
        }

        // Soft deletes a flight schedule.
        public async Task<ServiceResult> DeleteScheduleAsync(int scheduleId)
        {
            _logger.LogInformation("Attempting to soft-delete schedule ID {ScheduleId}.", scheduleId);
            var schedule = await _unitOfWork.FlightSchedules.GetActiveByIdAsync(scheduleId);
            if (schedule == null)
                return ServiceResult.Failure($"Active schedule with ID {scheduleId} not found.");

            // Check for dependencies: Active Flight Instances
            bool hasActiveInstances = await _unitOfWork.FlightInstances.AnyAsync(
                fi => fi.ScheduleId == scheduleId && !fi.IsDeleted
            );

            if (hasActiveInstances)
            {
                _logger.LogWarning("Failed to delete schedule {ScheduleId}: active flight instances exist.", scheduleId);
                return ServiceResult.Failure($"Cannot delete schedule ID {scheduleId}. It is used by one or more active flight instances.");
            }

            try
            {
                // Also soft-delete associated leg definitions (if any)
                var legs = await _unitOfWork.FlightLegDefs.GetByScheduleAsync(scheduleId);
                foreach (var leg in legs)
                {
                    _unitOfWork.FlightLegDefs.SoftDelete(leg);
                }

                _unitOfWork.FlightSchedules.SoftDelete(schedule);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully soft-deleted schedule ID {ScheduleId} and its legs.", scheduleId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting schedule ID {ScheduleId}.", scheduleId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // --- Leg Definition Methods ---

        // Retrieves all active flight leg definitions (segments) for a given flight schedule.
        public async Task<ServiceResult<IEnumerable<FlightLegDefDto>>> GetLegsByScheduleAsync(int scheduleId)
        {
            var schedule = await _unitOfWork.FlightSchedules.GetActiveByIdAsync(scheduleId);
            if (schedule == null)
                return ServiceResult<IEnumerable<FlightLegDefDto>>.Failure($"Active schedule with ID {scheduleId} not found.");

            try
            {
                var legs = await _unitOfWork.FlightLegDefs.GetByScheduleAsync(scheduleId); // Repo loads airports
                var dtos = _mapper.Map<IEnumerable<FlightLegDefDto>>(legs);
                return ServiceResult<IEnumerable<FlightLegDefDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight legs for schedule ID {ScheduleId}.", scheduleId);
                return ServiceResult<IEnumerable<FlightLegDefDto>>.Failure("An error occurred while retrieving flight legs.");
            }
        }

        // Creates a new flight leg definition (segment) within a schedule.
        public async Task<ServiceResult<FlightLegDefDto>> CreateFlightLegAsync(int scheduleId, CreateFlightLegDefDto createDto)
        {
            _logger.LogInformation("Adding new leg to schedule ID {ScheduleId}. Segment: {Segment}", scheduleId, createDto.SegmentNumber);

            // 1. Validate Schedule existence
            var schedule = await _unitOfWork.FlightSchedules.GetActiveByIdAsync(scheduleId);
            if (schedule == null)
                return ServiceResult<FlightLegDefDto>.Failure($"Active schedule with ID {scheduleId} not found.");

            // 2. Validate Airports existence
            if (await _unitOfWork.Airports.GetByIataCodeAsync(createDto.DepartureAirportIataCode) == null ||
                await _unitOfWork.Airports.GetByIataCodeAsync(createDto.ArrivalAirportIataCode) == null)
                return ServiceResult<FlightLegDefDto>.Failure("One or both airport codes are invalid or inactive.");

            // 3. Validate Segment Number uniqueness for this schedule
            if (await _unitOfWork.FlightLegDefs.ExistsByScheduleAndSegmentAsync(scheduleId, createDto.SegmentNumber))
                return ServiceResult<FlightLegDefDto>.Failure($"Segment number {createDto.SegmentNumber} already exists for this schedule.");

            try
            {
                var newLeg = _mapper.Map<FlightLegDef>(createDto);
                newLeg.ScheduleId = scheduleId;

                await _unitOfWork.FlightLegDefs.AddAsync(newLeg);
                await _unitOfWork.SaveChangesAsync();

                var detailedLeg = await _unitOfWork.FlightLegDefs.GetWithDetailsAsync(newLeg.LegDefId);
                var dto = _mapper.Map<FlightLegDefDto>(detailedLeg);

                _logger.LogInformation("Successfully created leg ID {LegId} for schedule {ScheduleId}.", newLeg.LegDefId, scheduleId);
                return ServiceResult<FlightLegDefDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating flight leg for schedule ID {ScheduleId}.", scheduleId);
                return ServiceResult<FlightLegDefDto>.Failure("An error occurred while creating the flight leg.");
            }
        }
    }
}