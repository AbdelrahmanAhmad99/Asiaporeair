using Application.DTOs.FlightOperations;
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
using Application.DTOs.FlightSchedule;  

namespace Application.Services
{
    // Service implementation for managing real-time flight operations.
    public class FlightOperationsService : IFlightOperationsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FlightOperationsService> _logger;

        // Constructor for dependency injection
        public FlightOperationsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FlightOperationsService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Retrieves a single flight instance by its unique ID with full details.
        public async Task<ServiceResult<FlightInstanceDto>> GetInstanceByIdAsync(int instanceId)
        {
            try
            {
                var instance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(instanceId);
                if (instance == null)
                {
                    _logger.LogWarning("Flight Instance ID {InstanceId} not found.", instanceId);
                    return ServiceResult<FlightInstanceDto>.Failure($"Flight instance with ID {instanceId} not found.");
                }

                var dto = _mapper.Map<FlightInstanceDto>(instance);
                return ServiceResult<FlightInstanceDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight instance ID {InstanceId}.", instanceId);
                return ServiceResult<FlightInstanceDto>.Failure("An error occurred while retrieving the flight instance.");
            }
        }

        // Retrieves a lightweight list of flights for Flight Information Display Systems (FIDS).
        public async Task<ServiceResult<IEnumerable<FlightInstanceBriefDto>>> GetFlightInstancesForFidsAsync(string airportIataCode, string direction)
        {
            _logger.LogInformation("Fetching FIDS data for airport {AirportIataCode}, direction {Direction}.", airportIataCode, direction);
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                Expression<Func<FlightInstance, bool>> filter = fi =>
                    fi.ScheduledDeparture >= today && fi.ScheduledDeparture < tomorrow;

                if (direction.Equals("Departure", StringComparison.OrdinalIgnoreCase))
                {
                    filter = filter.And(fi => fi.Schedule.Route.OriginAirportId == airportIataCode);
                }
                else if (direction.Equals("Arrival", StringComparison.OrdinalIgnoreCase))
                {
                    filter = filter.And(fi => fi.Schedule.Route.DestinationAirportId == airportIataCode);
                }
                else
                {
                    return ServiceResult<IEnumerable<FlightInstanceBriefDto>>.Failure("Invalid direction. Must be 'Departure' or 'Arrival'.");
                }

                var instances = await _unitOfWork.FlightInstances.SearchAsync(filter);

                // Map to brief DTO
                var dtos = instances.Select(instance =>
                {
                    var dto = _mapper.Map<FlightInstanceBriefDto>(instance);
                    if (direction.Equals("Departure", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.ScheduledTime = instance.ScheduledDeparture; 
                        //dto.EstimatedTime = instance.ActualDeparture; 
                        // dto.Gate = instance.DepartureGate; 
                    }
                    else
                    {
                        dto.ScheduledTime = instance.ScheduledArrival; 
                        //dto.EstimatedTime = instance.ActualArrival; 
                        // dto.Gate = instance.ArrivalGate;
                    }
                    return dto;
                }).ToList();

                return ServiceResult<IEnumerable<FlightInstanceBriefDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FIDS data for {AirportIataCode}.", airportIataCode);
                return ServiceResult<IEnumerable<FlightInstanceBriefDto>>.Failure("An error occurred while retrieving FIDS data.");
            }
        }

        // Performs an advanced, paginated search for flight instances.
        public async Task<ServiceResult<PaginatedResult<FlightInstanceDto>>> SearchFlightInstancesAsync(FlightInstanceFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching flight instances for page {PageNumber} with filter: {Filter}", pageNumber, filter.FlightNo);
            try
            {
                // Build the filter expression dynamically
                Expression<Func<FlightInstance, bool>> filterExpression = fi => (filter.IncludeDeleted || !fi.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.FlightNo))
                { 
                    var upperFlightNo = filter.FlightNo.ToUpper();
                    filterExpression = filterExpression.And(fi => fi.Schedule.FlightNo.ToUpper() == upperFlightNo);
                }
                if (!string.IsNullOrWhiteSpace(filter.AirlineIataCode))
                    filterExpression = filterExpression.And(fi => fi.Schedule.AirlineId == filter.AirlineIataCode);
                if (!string.IsNullOrWhiteSpace(filter.Status))
                { 
                    var upperStatus = filter.Status.ToUpper();
                    filterExpression = filterExpression.And(fi => fi.Status.ToUpper() == upperStatus);
                }
                if (!string.IsNullOrWhiteSpace(filter.AircraftTailNumber))
                    filterExpression = filterExpression.And(fi => fi.AircraftId == filter.AircraftTailNumber);

                if (filter.Date.HasValue)
                {
                    var date = filter.Date.Value.Date;
                    var nextDate = date.AddDays(1);
                    filterExpression = filterExpression.And(fi => fi.ScheduledDeparture >= date && fi.ScheduledDeparture < nextDate);
                }

                if (!string.IsNullOrWhiteSpace(filter.AirportIataCode))
                {
                    if (filter.Direction == "Departure")
                        filterExpression = filterExpression.And(fi => fi.Schedule.Route.OriginAirportId == filter.AirportIataCode);
                    else if (filter.Direction == "Arrival")
                        filterExpression = filterExpression.And(fi => fi.Schedule.Route.DestinationAirportId == filter.AirportIataCode);
                    else
                        filterExpression = filterExpression.And(fi => fi.Schedule.Route.OriginAirportId == filter.AirportIataCode || fi.Schedule.Route.DestinationAirportId == filter.AirportIataCode);
                }

                
                var (items, totalCount) = await _unitOfWork.FlightInstances.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(fs => fs.ScheduledDeparture),
                    includeProperties: "Schedule.Route.OriginAirport,Schedule.Route.DestinationAirport,Schedule.Airline,Schedule.AircraftType,Aircraft"
                );

                var dtos = _mapper.Map<List<FlightInstanceDto>>(items);
                var paginatedResult = new PaginatedResult<FlightInstanceDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<FlightInstanceDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flight instances on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<FlightInstanceDto>>.Failure("An error occurred during the instance search.");
            }
        }

        // Retrieves all flight instances for a specific date range (for reports or ops).
        public async Task<ServiceResult<IEnumerable<FlightInstanceDto>>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate, string? airportIataCode = null)
        {
            _logger.LogInformation("Getting instances from {StartDate} to {EndDate} for airport {AirportCode}.", startDate, endDate, airportIataCode);
            try
            {
                var instances = await _unitOfWork.FlightInstances.GetByScheduledDateRangeAsync(startDate, endDate);
          
                if (!string.IsNullOrWhiteSpace(airportIataCode))
                {
                    var upperairportIataCode = airportIataCode.ToUpper();
                    instances = instances.Where(fi =>
                        fi.Schedule.Route.OriginAirportId == upperairportIataCode ||
                        fi.Schedule.Route.DestinationAirportId == upperairportIataCode);
                }

                var dtos = _mapper.Map<IEnumerable<FlightInstanceDto>>(instances);
                return ServiceResult<IEnumerable<FlightInstanceDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instances by date range.");
                return ServiceResult<IEnumerable<FlightInstanceDto>>.Failure("An error occurred while retrieving flight instances.");
            }
        }

        // Retrieves flight instances assigned to a specific aircraft tail number.
        public async Task<ServiceResult<IEnumerable<FlightInstanceDto>>> GetInstancesByAircraftAsync(string tailNumber, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Getting instances for aircraft {TailNumber} from {StartDate} to {EndDate}.", tailNumber, startDate, endDate);
            try
            {
                var instances = await _unitOfWork.FlightInstances.GetByScheduledDateRangeAsync(startDate, endDate);
                var uppertailNumber = tailNumber.ToUpper();
                instances = instances.Where(fi => fi.AircraftId == uppertailNumber);

                var dtos = _mapper.Map<IEnumerable<FlightInstanceDto>>(instances);
                return ServiceResult<IEnumerable<FlightInstanceDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instances by aircraft {TailNumber}.", tailNumber);
                return ServiceResult<IEnumerable<FlightInstanceDto>>.Failure("An error occurred while retrieving flight instances.");
            }
        }

        // Generates flight instances for a date range based on existing FlightSchedules.

        public async Task<ServiceResult<GenerationReportDto>> GenerateInstancesFromSchedulesAsync(GenerateInstancesRequestDto request)
        {
            _logger.LogInformation("Starting generation of flight instances from {StartDate} to {EndDate} for airline {AirlineCode}.",
                request.StartDate, request.EndDate, request.AirlineIataCode ?? "ALL");

            var report = new GenerationReportDto();

            // ---  Prevent generating flights in the past ---
            var today = DateTime.UtcNow.Date;
            if (request.EndDate.Date < today)
            {
                return ServiceResult<GenerationReportDto>.Failure("Cannot generate flight instances for a date range that is entirely in the past.");
            }

            // Adjust start date if it's in the past to only generate from today onwards
            var generationStartDate = request.StartDate.Date < today ? today : request.StartDate.Date;

            if (request.StartDate > request.EndDate)
                return ServiceResult<GenerationReportDto>.Failure("Start date must be before end date.");

            int createdCount = 0;
            try
            {
                var allSchedules = string.IsNullOrWhiteSpace(request.AirlineIataCode)
                    ? await _unitOfWork.FlightSchedules.GetAllActiveAsync()
                    : await _unitOfWork.FlightSchedules.GetByAirlineAsync(request.AirlineIataCode);

                _logger.LogDebug("Found {Count} active schedules to process.", allSchedules.Count());

                for (var day = generationStartDate; day <= request.EndDate.Date; day = day.AddDays(1))
                {
                    var dayOfWeekBit = (byte)(1 << (int)day.DayOfWeek);

                    var schedulesForDay = allSchedules.Where(s =>
                        s.DaysOfWeek.HasValue && (s.DaysOfWeek.Value & dayOfWeekBit) != 0);

                    foreach (var schedule in schedulesForDay)
                    {
                        var scheduleTime = schedule.DepartureTimeScheduled.TimeOfDay;
                        var scheduledDeparture = day.Add(scheduleTime);
                        var arrivalTime = schedule.ArrivalTimeScheduled.TimeOfDay;
                        var scheduledArrival = arrivalTime < scheduleTime
                            ? day.AddDays(1).Add(arrivalTime)
                            : day.Add(arrivalTime);

                        // Check if instance already exists
                        bool instanceExists = await _unitOfWork.FlightInstances.ExistsByScheduleAndTimeAsync(schedule.ScheduleId, scheduledDeparture);
                        if (instanceExists)
                        {
                            report.Warnings.Add($"Skipping: Instance for {schedule.FlightNo} on {scheduledDeparture.ToShortDateString()} already exists.");
                            continue;
                        }

                        // ---  Professional Aircraft Assignment ---
                        // Attempt to find an available aircraft.
                        string? assignedAircraft = await FindAvailableAircraftAsync(schedule.AircraftTypeId, scheduledDeparture, scheduledArrival);

                        if (string.IsNullOrEmpty(assignedAircraft))
                        {
                            _logger.LogWarning("No available aircraft found for {FlightNo} (Type ID {AircraftTypeId}) on {Date}. Instance will be created as 'TBA' (To Be Assigned).",
                                schedule.FlightNo, schedule.AircraftTypeId, scheduledDeparture);

                            report.Warnings.Add($"Instance {schedule.FlightNo} on {scheduledDeparture.ToShortDateString()} created without an assigned aircraft (TBA).");

                            // We set AircraftId to NULL (assuming DB Schema is updated to be NULLABLE)
                        }

                        var newInstance = new FlightInstance
                        {
                            ScheduleId = schedule.ScheduleId,
                            ScheduledDeparture = scheduledDeparture,
                            ScheduledArrival = scheduledArrival,
                            Status = "Scheduled",
                            IsDeleted = false,
                            AircraftId = assignedAircraft // This will be NULL if no aircraft was found
                        };

                        // ---  FALLBACK LOGIC ---
                        // The dangerous fallback logic that assigned a random aircraft has been removed.

                        await _unitOfWork.FlightInstances.AddAsync(newInstance);
                        createdCount++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                report.InstancesCreated = createdCount;
                _logger.LogInformation("Successfully generated {Count} new flight instances.", createdCount);

                return ServiceResult<GenerationReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flight instances."); 

                report.Failures.Add($"An error occurred: {ex.Message}");
                return ServiceResult<GenerationReportDto>.Failure(report, "An error occurred during generation.");
            }
        }
         

        // Creates a single, non-scheduled flight (e.g., charter, ferry flight).
        public async Task<ServiceResult<FlightInstanceDto>> CreateAdHocFlightInstanceAsync(CreateAdHocFlightInstanceDto createDto)
        {
            _logger.LogInformation("Attempting to create ad-hoc flight {FlightNo}.", createDto.FlightNo);

            // 1. Validate dependencies
            if (await _unitOfWork.Routes.GetActiveByIdAsync(createDto.RouteId) == null)
                return ServiceResult<FlightInstanceDto>.Failure($"Route ID {createDto.RouteId} not found.");
            if (await _unitOfWork.Airlines.GetByIataCodeAsync(createDto.AirlineIataCode) == null)
                return ServiceResult<FlightInstanceDto>.Failure($"Airline {createDto.AirlineIataCode} not found.");

            var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(createDto.AircraftTailNumber);
            if (aircraft == null)
                return ServiceResult<FlightInstanceDto>.Failure($"Aircraft {createDto.AircraftTailNumber} not found.");

            // 2. Business logic validation
            if (createDto.ScheduledDeparture >= createDto.ScheduledArrival)
                return ServiceResult<FlightInstanceDto>.Failure("Departure must be before arrival.");

            try
            {
                // Ad-hoc flights require a "placeholder" schedule because schedule_fk is NOT NULL.
                var adHocSchedule = new FlightSchedule
                {
                    FlightNo = createDto.FlightNo,
                    RouteId = createDto.RouteId,
                    AirlineId = createDto.AirlineIataCode,
                    AircraftTypeId = aircraft.AircraftTypeId,
                    DepartureTimeScheduled = createDto.ScheduledDeparture,
                    ArrivalTimeScheduled = createDto.ScheduledArrival,
                    DaysOfWeek = 0, // Not recurring
                    IsDeleted = true // Hide from regular schedule searches
                };

                await _unitOfWork.FlightSchedules.AddAsync(adHocSchedule);
                await _unitOfWork.SaveChangesAsync(); // Save to get the new ScheduleId

                _logger.LogInformation("Created ad-hoc schedule ID {ScheduleId} for flight {FlightNo}.", adHocSchedule.ScheduleId, createDto.FlightNo);

                var newInstance = new FlightInstance
                {
                    ScheduleId = adHocSchedule.ScheduleId,
                    AircraftId = createDto.AircraftTailNumber,
                    ScheduledDeparture = createDto.ScheduledDeparture,
                    ScheduledArrival = createDto.ScheduledArrival,
                    Status = createDto.Status,
                    IsDeleted = false
                };

                await _unitOfWork.FlightInstances.AddAsync(newInstance);
                await _unitOfWork.SaveChangesAsync(); // Save to get new InstanceId

                _logger.LogInformation("Successfully created ad-hoc instance ID {InstanceId} for flight {FlightNo}.", newInstance.InstanceId, createDto.FlightNo);

                // Load details for DTO response
                var detailedInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(newInstance.InstanceId);
                var dto = _mapper.Map<FlightInstanceDto>(detailedInstance);
                return ServiceResult<FlightInstanceDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ad-hoc flight {FlightNo}.", createDto.FlightNo);
                return ServiceResult<FlightInstanceDto>.Failure("An error occurred while creating the ad-hoc flight.");
            }
        }

        // Updates the primary operational status of a flight.
        public async Task<ServiceResult<FlightInstanceDto>> UpdateFlightStatusAsync(int instanceId, UpdateFlightStatusDto updateDto)
        {
            _logger.LogInformation("Updating status for instance ID {InstanceId} to {Status}.", instanceId, updateDto.Status);

            // IMPORTANT: Use GetWithDetailsAsync to ensure AircraftId is loaded for validation.
            var instance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(instanceId);
            if (instance == null)
                return ServiceResult<FlightInstanceDto>.Failure($"Active flight instance ID {instanceId} not found.");

            // BUSINESS LOGIC CHECK: Cannot set status to Departed or Arrived without an assigned aircraft.
            // Assuming 'AircraftId' is the nullable foreign key (e.g., TailNumber)
            if ((updateDto.Status == "Departed" || updateDto.Status == "Arrived") && string.IsNullOrEmpty(instance.AircraftId))
            {
                _logger.LogWarning("Status update failed for instance {InstanceId}. Cannot set status to {Status} without an assigned aircraft.", instanceId, updateDto.Status);
                return ServiceResult<FlightInstanceDto>.Failure("Cannot set status to Departed or Arrived without an assigned aircraft.");
            }

            try
            {
                instance.Status = updateDto.Status;

                if (updateDto.Status == "Departed" && !instance.ActualDeparture.HasValue)
                    instance.ActualDeparture = DateTime.UtcNow;

                if (updateDto.Status == "Arrived" && !instance.ActualArrival.HasValue)
                    instance.ActualArrival = DateTime.UtcNow;

                // Note: The reason field from DTO is not used in the entity here, but logging is good practice.

                _unitOfWork.FlightInstances.Update(instance);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status for instance ID {InstanceId}.", instanceId);

                // Return the updated entity mapped to DTO
                var updatedDto = _mapper.Map<FlightInstanceDto>(instance);
                return ServiceResult<FlightInstanceDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for instance ID {InstanceId}.", instanceId);
                return ServiceResult<FlightInstanceDto>.Failure("An error occurred while updating the flight status.");
            }
        }

        // Updates the estimated or actual departure/arrival times for a flight.
        public async Task<ServiceResult<FlightInstanceDto>> UpdateFlightTimesAsync(int instanceId, UpdateFlightTimesDto updateDto)
        {
            _logger.LogInformation("Updating times for instance ID {InstanceId}.", instanceId);

            // 1. Fetch instance with details
            var instance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(instanceId);
            if (instance == null)
                return ServiceResult<FlightInstanceDto>.Failure($"Active flight instance ID {instanceId} not found.");

            // 2. BUSINESS LOGIC CHECK: Cannot record actual times without an assigned aircraft.
            // (This check was already correctly implemented in the previous step)
            if ((updateDto.ActualDeparture.HasValue || updateDto.ActualArrival.HasValue) && string.IsNullOrEmpty(instance.AircraftId))
            {
                _logger.LogWarning("Time update failed for instance {InstanceId}. Cannot record actual departure/arrival without an assigned aircraft.", instanceId);
                return ServiceResult<FlightInstanceDto>.Failure("Cannot record actual departure or arrival time without an assigned aircraft.");
            }

            // 3.LOGIC CHECK: Actual Departure vs. Scheduled Departure
            if (updateDto.ActualDeparture.HasValue)
            {
                // Allow a slight lead time (e.g., 24 hours) for pre-emptive recording, 
                // but prevent recording dates that are months or years ahead of the schedule.
                if (updateDto.ActualDeparture.Value < instance.ScheduledDeparture.AddDays(-30))
                {
                    return ServiceResult<FlightInstanceDto>.Failure("Actual Departure time is extremely early compared to the scheduled time and appears to be an error.");
                }
                
            }

            // 4. LOGIC CHECK: Actual Arrival vs. Actual Departure (Chronological Check)
            // If both times are being set in the current update, or if arrival is being set after departure was already set
            if (updateDto.ActualArrival.HasValue)
            {
                // Determine the effective departure time (new one if provided, otherwise the existing one)
                DateTime effectiveDeparture = updateDto.ActualDeparture ?? instance.ActualDeparture ?? DateTime.MinValue;

                if (effectiveDeparture != DateTime.MinValue && updateDto.ActualArrival.Value <= effectiveDeparture.AddMinutes(5)) // Allow 5 minutes minimum flight time
                {
                    return ServiceResult<FlightInstanceDto>.Failure("Actual Arrival time must be at least 5 minutes after Actual Departure time.");
                }
            }

            try
            {
                // 5. Update Entity and Save Changes
                instance.ActualDeparture = updateDto.ActualDeparture ?? instance.ActualDeparture;
                instance.ActualArrival = updateDto.ActualArrival ?? instance.ActualArrival;

                // Auto-update status (existing logic is fine)
                if (updateDto.ActualDeparture.HasValue && instance.Status != "Departed" && instance.Status != "Arrived")
                    instance.Status = "Departed";

                if (updateDto.ActualArrival.HasValue && instance.Status != "Arrived")
                    instance.Status = "Arrived";

                _unitOfWork.FlightInstances.Update(instance);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated times for instance ID {InstanceId}.", instanceId);

                // Return the updated entity mapped to DTO
                var updatedDto = _mapper.Map<FlightInstanceDto>(instance);
                return ServiceResult<FlightInstanceDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating times for instance ID {InstanceId}.", instanceId);
                return ServiceResult<FlightInstanceDto>.Failure("An error occurred while updating flight times.");
            }
        }

        // Assigns a specific aircraft (by tail number) to a flight instance.
        public async Task<ServiceResult> AssignAircraftToInstanceAsync(int instanceId, AssignAircraftDto assignDto)
        {
            _logger.LogInformation("Attempting to assign aircraft {TailNumber} to instance ID {InstanceId}.", assignDto.TailNumber, instanceId);

            var instance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(instanceId); // Need Schedule details
            if (instance == null)
                return ServiceResult.Failure($"Active flight instance ID {instanceId} not found.");

            var aircraft = await _unitOfWork.Aircrafts.GetByTailNumberAsync(assignDto.TailNumber);
            if (aircraft == null)
                return ServiceResult.Failure($"Aircraft {assignDto.TailNumber} not found or is inactive.");

            // 1. Validation: Aircraft Type
            if (aircraft.AircraftTypeId != instance.Schedule.AircraftTypeId)
            {
                _logger.LogWarning("Aircraft assignment failed for instance {InstanceId}. Scheduled type {ScheduledType}, attempted {AttemptedType}.",
                    instanceId, instance.Schedule.AircraftTypeId, aircraft.AircraftTypeId);
                return ServiceResult.Failure($"Aircraft {assignDto.TailNumber} is type ID {aircraft.AircraftTypeId}, but flight requires type ID {instance.Schedule.AircraftTypeId}.");
            }

            // 2. Validation: Aircraft Status
            if (aircraft.Status != "Active") // Assuming "Operational" is the ready status
            {
                _logger.LogWarning("Aircraft assignment failed for instance {InstanceId}. Aircraft {TailNumber} status is {Status}.",
                    instanceId, aircraft.TailNumber, aircraft.Status);
                return ServiceResult.Failure($"Aircraft {assignDto.TailNumber} is not operational (Status: {aircraft.Status}).");
            }

            // 3. Validation: Aircraft Availability (Check for conflicting flights)
            //  This method is now added to the repository
            var conflictingFlight = await _unitOfWork.FlightInstances.GetConflictingFlightAsync(
                assignDto.TailNumber,
                instance.ScheduledDeparture,
                instance.ScheduledArrival,
                instanceId // Exclude this instance itself from the check
            );

            if (conflictingFlight != null)
            {
                _logger.LogWarning("Aircraft assignment failed for instance {InstanceId}. Aircraft {TailNumber} is already assigned to instance {ConflictingId} at that time.",
                    instanceId, aircraft.TailNumber, conflictingFlight.InstanceId);
                return ServiceResult.Failure($"Aircraft {assignDto.TailNumber} is already assigned to flight {conflictingFlight.Schedule.FlightNo} (ID: {conflictingFlight.InstanceId}) which conflicts with this time.");
            }

            try
            {
                instance.AircraftId = assignDto.TailNumber;
                _unitOfWork.FlightInstances.Update(instance);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully assigned aircraft {TailNumber} to instance ID {InstanceId}.", assignDto.TailNumber, instanceId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning aircraft for instance ID {InstanceId}.", instanceId);
                return ServiceResult.Failure("An error occurred while assigning the aircraft.");
            }
        }
         

        // Soft-deletes a flight instance (e.g., if generated in error).
        public async Task<ServiceResult> DeleteFlightInstanceAsync(int instanceId, string reason)
        {
            _logger.LogInformation("Attempting to soft-delete instance ID {InstanceId} for reason: {Reason}", instanceId, reason);
            var instance = await _unitOfWork.FlightInstances.GetActiveByIdAsync(instanceId);
            if (instance == null)
                return ServiceResult.Failure($"Active flight instance ID {instanceId} not found.");

            // Business Logic: Check if flight has active bookings or tickets.
            if (await _unitOfWork.Bookings.AnyAsync(b => b.FlightInstanceId == instanceId && !b.IsDeleted))
            {
                _logger.LogWarning("Delete failed for instance {InstanceId}. Active bookings exist.", instanceId);
                return ServiceResult.Failure("Cannot delete flight instance: Active bookings are associated with this flight.");
            }

            // Business Logic: Cannot delete a flight that has already departed or arrived.
            if (instance.Status == "Departed" || instance.Status == "Arrived")
            {
                _logger.LogWarning("Delete failed for instance {InstanceId}. Flight is in status {Status}.", instanceId, instance.Status);
                return ServiceResult.Failure($"Cannot delete a flight that has already {instance.Status}.");
            }

            try
            {
                _unitOfWork.FlightInstances.SoftDelete(instance);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully soft-deleted instance ID {InstanceId}.", instanceId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting instance ID {InstanceId}.", instanceId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // Retrieves summary data for the airport operational dashboard.
        public async Task<ServiceResult<OperationalDashboardDto>> GetOperationalDashboardAsync(string airportIataCode, DateTime date)
        {


            _logger.LogInformation("Generating operational dashboard for {AirportIataCode} on {Date}.", airportIataCode, date);
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                var instances = await _unitOfWork.FlightInstances.GetByScheduledDateRangeAsync(startDate, endDate);
                var airportInstances = instances.Where(fi =>
                    fi.Schedule.Route.OriginAirportId == airportIataCode ||
                    fi.Schedule.Route.DestinationAirportId == airportIataCode).ToList();

                var departures = airportInstances.Where(fi => fi.Schedule.Route.OriginAirportId == airportIataCode).ToList();
                var arrivals = airportInstances.Where(fi => fi.Schedule.Route.DestinationAirportId == airportIataCode).ToList();

                var dashboard = new OperationalDashboardDto
                {
                    AirportIataCode = airportIataCode,
                    ForDate = startDate,
                    TotalDepartures = departures.Count,
                    TotalArrivals = arrivals.Count,

                    DeparturesOnTime = departures.Count(d => d.Status == "OnTime" || d.Status == "Departed"),
                    DeparturesDelayed = departures.Count(d => d.Status == "Delayed"),
                    DeparturesCancelled = departures.Count(d => d.Status == "Cancelled"),

                    ArrivalsOnTime = arrivals.Count(a => a.Status == "OnTime" || a.Status == "Arrived"),
                    ArrivalsDelayed = arrivals.Count(a => a.Status == "Delayed"),
                    ArrivalsCancelled = arrivals.Count(a => a.Status == "Cancelled"),

                    // Corrected: CountAsync is now available
                    AircraftOnGround = await _unitOfWork.Aircrafts.CountAsync(a =>
                        a.Airline.BaseAirportId == airportIataCode && a.Status == "Operational"),

                    // Get 5 flights that are delayed or need attention
                    UrgentFlights = _mapper.Map<List<FlightInstanceBriefDto>>(
                        departures.Where(d => d.Status == "Delayed")
                        .Concat(arrivals.Where(a => a.Status == "Delayed"))
                        .OrderBy(f => f.ScheduledDeparture)
                        .Take(5))
                };

                return ServiceResult<OperationalDashboardDto>.Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating operational dashboard for {AirportIataCode}.", airportIataCode);
                return ServiceResult<OperationalDashboardDto>.Failure("An error occurred while generating the dashboard.");
            }
        }

        // --- Private Helper Methods ---

        // Helper to find an available aircraft of a specific type for a flight.
        private async Task<string?> FindAvailableAircraftAsync(int aircraftTypeId, DateTime departure, DateTime arrival)
        {
            // 1. Get all "Operational" aircraft of the correct type
            // Corrected: FindAsync is now available
            var availableAircraft = await _unitOfWork.Aircrafts.FindAsync(
                a => a.AircraftTypeId == aircraftTypeId &&
                a.Status == "Operational" &&
                !a.IsDeleted
            );

            // 2. Check each one for availability
            foreach (var aircraft in availableAircraft)
            {
                // Corrected: GetConflictingFlightAsync is now available
                var conflictingFlight = await _unitOfWork.FlightInstances.GetConflictingFlightAsync(
                    aircraft.TailNumber, departure, arrival, null);

                if (conflictingFlight == null)
                {
                    // This aircraft is free
                    return aircraft.TailNumber;
                }
            }

            // No aircraft found
            return null;
        }
    }
}