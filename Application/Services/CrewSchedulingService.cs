using Application.DTOs.CrewScheduling;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services
{
    // Service implementation for assigning crew members to flights.
    public class CrewSchedulingService : ICrewSchedulingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CrewSchedulingService> _logger; 

        // Define buffer time for crew availability checks (e.g., 2 hours before/after)
        private static readonly TimeSpan AssignmentBuffer = TimeSpan.FromHours(2);

        public CrewSchedulingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CrewSchedulingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Assigns one or more crew members to a flight instance.
        public async Task<ServiceResult> AssignCrewToFlightAsync(AssignCrewRequestDto request, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} attempting to assign crew to FlightInstanceId {FlightId}.", performingUser.Identity?.Name, request.FlightInstanceId);

            // Authorization: Check if the user has scheduling permissions (e.g., Admin, Supervisor)
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("Supervisor") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot assign crew.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied. Insufficient permissions for crew scheduling.");
            }

            // 1. Get Flight Instance details
            var flightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(request.FlightInstanceId);
            if (flightInstance == null)
            {
                return ServiceResult.Failure($"Flight Instance with ID {request.FlightInstanceId} not found.");
            }
            if (flightInstance.Schedule == null || flightInstance.Schedule.AircraftType == null)
            {
                return ServiceResult.Failure("Flight instance is missing schedule or aircraft type details.");
            }

            var assignmentsToAdd = new List<FlightCrew>();
            var validationErrors = new List<string>();

            // 2. Validate each requested assignment
            foreach (var assignmentDetail in request.Assignments)
            {
                // 2a. Check if crew member exists and is active
                var crewMember = await _unitOfWork.CrewMembers.GetWithEmployeeDetailsAsync(assignmentDetail.CrewMemberEmployeeId);
                if (crewMember == null || crewMember.Employee == null || crewMember.Employee.AppUser == null || crewMember.IsDeleted || crewMember.Employee.IsDeleted || crewMember.Employee.AppUser.IsDeleted)
                {
                    validationErrors.Add($"Crew member with Employee ID {assignmentDetail.CrewMemberEmployeeId} not found or is inactive.");
                    continue;
                }

                // 2b. Check if already assigned to this flight
                if (await _unitOfWork.FlightCrews.ExistsAssignmentAsync(request.FlightInstanceId, assignmentDetail.CrewMemberEmployeeId))
                {
                    _logger.LogWarning("Crew member {CrewId} already assigned to flight {FlightId}.", assignmentDetail.CrewMemberEmployeeId, request.FlightInstanceId);
                    continue; // Skip if already assigned
                }

                // 2c. Check for conflicting assignments (basic time overlap)
                var existingAssignments = await _unitOfWork.FlightCrews.GetAssignmentsForCrewMemberByDateRangeAsync(
                    assignmentDetail.CrewMemberEmployeeId,
                    flightInstance.ScheduledDeparture.Add(-AssignmentBuffer),
                    flightInstance.ScheduledArrival.Add(AssignmentBuffer)
                );
                if (existingAssignments.Any())
                {
                    validationErrors.Add($"Crew member {crewMember.Employee.AppUser.LastName} (ID: {crewMember.EmployeeId}) has a conflicting assignment.");
                    continue;
                }

                // 2d. Validate Qualifications (Pilot Type Rating)
                if (crewMember.Position.Equals("Pilot", StringComparison.OrdinalIgnoreCase))
                {
                    var pilotProfile = await _unitOfWork.Pilots.GetByEmployeeIdAsync(crewMember.EmployeeId);
                    if (pilotProfile == null || pilotProfile.AircraftTypeId != flightInstance.Schedule.AircraftTypeId)
                    {
                        validationErrors.Add($"Pilot {crewMember.Employee.AppUser.LastName} (ID: {crewMember.EmployeeId}) is not type-rated for Aircraft Type ID {flightInstance.Schedule.AircraftTypeId}.");
                        continue;
                    }
                }

                // 2e. Validate Certifications (Basic expiry check)
                var certifications = await _unitOfWork.Certifications.GetByCrewMemberAsync(crewMember.EmployeeId);
                if (certifications.Any(c => c.ExpiryDate.HasValue && c.ExpiryDate.Value < flightInstance.ScheduledDeparture.Date))
                {
                    validationErrors.Add($"Crew member {crewMember.Employee.AppUser.LastName} (ID: {crewMember.EmployeeId}) has expired certifications relevant to this flight date.");
                    continue;
                }

                // 3. If all checks pass, create the FlightCrew entity
                assignmentsToAdd.Add(new FlightCrew
                {
                    FlightInstanceId = request.FlightInstanceId,
                    CrewMemberId = assignmentDetail.CrewMemberEmployeeId,
                    Role = assignmentDetail.Role,
                    IsDeleted = false
                });
            }

            // If any validation errors occurred, return failure
            if (validationErrors.Any())
            {
                _logger.LogWarning("Crew assignment validation failed for Flight {FlightId}: {Errors}", request.FlightInstanceId, string.Join("; ", validationErrors));
                return ServiceResult.Failure(validationErrors);
            }

            // If no errors and assignments exist, add them
            if (!assignmentsToAdd.Any())
            {
                 
                return ServiceResult.Success(); // Non-generic Success() takes no arguments }
            }
                try
            {
                await _unitOfWork.FlightCrews.AddMultipleAssignmentsAsync(assignmentsToAdd);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully assigned {Count} crew members to FlightInstanceId {FlightId}.", assignmentsToAdd.Count, request.FlightInstanceId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred while assigning crew to FlightInstanceId {FlightId}.", request.FlightInstanceId);
                return ServiceResult.Failure($"An error occurred while saving assignments: {ex.Message}");
            }
        }

        // Removes a specific crew member assignment from a flight instance.
        public async Task<ServiceResult> RemoveCrewFromFlightAsync(int flightInstanceId, int crewMemberEmployeeId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} attempting to remove crew member {CrewId} from FlightInstanceId {FlightId}.", performingUser.Identity?.Name, crewMemberEmployeeId, flightInstanceId);

            // Authorization
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("Supervisor") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot remove crew assignments.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied. Insufficient permissions.");
            }

            try
            {
                var assignment = await _unitOfWork.FlightCrews.GetActiveByIdAsync(flightInstanceId, crewMemberEmployeeId);
                if (assignment == null)
                {
                    return ServiceResult.Failure("Crew assignment not found.");
                }

                // Use soft delete via the repository method
                await _unitOfWork.FlightCrews.RemoveMultipleAssignmentsAsync(new List<FlightCrew> { assignment });
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully removed crew member {CrewId} from FlightInstanceId {FlightId}.", crewMemberEmployeeId, flightInstanceId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing crew member {CrewId} from FlightInstanceId {FlightId}.", crewMemberEmployeeId, flightInstanceId);
                return ServiceResult.Failure($"An error occurred: {ex.Message}");
            }
        }

        // Retrieves the full crew roster for a specific flight instance.
        public async Task<ServiceResult<FlightRosterDto>> GetFlightRosterAsync(int flightInstanceId)
        {
            _logger.LogInformation("Retrieving crew roster for FlightInstanceId {FlightId}.", flightInstanceId);
            try
            {
                var flightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);
                if (flightInstance == null)
                {
                    return ServiceResult<FlightRosterDto>.Failure($"Flight Instance with ID {flightInstanceId} not found.");
                }

                var crewAssignments = await _unitOfWork.FlightCrews.GetCrewForFlightAsync(flightInstanceId);

                var rosterDto = new FlightRosterDto
                {
                    FlightInstanceId = flightInstance.InstanceId,
                    FlightNumber = flightInstance.Schedule.FlightNo,
                    OriginAirport = flightInstance.Schedule.Route.OriginAirport.IataCode,
                    DestinationAirport = flightInstance.Schedule.Route.DestinationAirport.IataCode,
                    ScheduledDeparture = flightInstance.ScheduledDeparture,
                    ScheduledArrival = flightInstance.ScheduledArrival,
                    AircraftType = flightInstance.Schedule.AircraftType.Model,
                    AssignedCrew = _mapper.Map<List<FlightCrewAssignmentDto>>(crewAssignments)
                };

                return ServiceResult<FlightRosterDto>.Success(rosterDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roster for FlightInstanceId {FlightId}.", flightInstanceId);
                return ServiceResult<FlightRosterDto>.Failure("An error occurred while retrieving the flight roster.");
            }
        }

        // Retrieves the flight schedule for a specific crew member within a date range.
        public async Task<ServiceResult<CrewScheduleDto>> GetCrewMemberScheduleAsync(CrewScheduleRequestDto request)
        {
            _logger.LogInformation("Retrieving schedule for Crew Member {CrewId} from {Start} to {End}.", request.CrewMemberEmployeeId, request.StartDate, request.EndDate);
            try
            {
                var crewMember = await _unitOfWork.CrewMembers.GetWithEmployeeDetailsAsync(request.CrewMemberEmployeeId);
                if (crewMember?.Employee?.AppUser == null)
                {
                    return ServiceResult<CrewScheduleDto>.Failure($"Crew member with Employee ID {request.CrewMemberEmployeeId} not found.");
                }

                var assignments = await _unitOfWork.FlightCrews.GetAssignmentsForCrewMemberByDateRangeAsync(
                    request.CrewMemberEmployeeId,
                    request.StartDate,
                    request.EndDate);

                var scheduleDto = new CrewScheduleDto
                {
                    CrewMemberEmployeeId = crewMember.EmployeeId,
                    CrewMemberName = $"{crewMember.Employee.AppUser.FirstName} {crewMember.Employee.AppUser.LastName}",
                    ScheduleStartDate = request.StartDate,
                    ScheduleEndDate = request.EndDate,
                    AssignedFlights = _mapper.Map<List<ScheduledFlightDto>>(assignments)
                };

                return ServiceResult<CrewScheduleDto>.Success(scheduleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule for Crew Member {CrewId}.", request.CrewMemberEmployeeId);
                return ServiceResult<CrewScheduleDto>.Failure("An error occurred while retrieving the crew member's schedule.");
            }
        }

        // Finds potentially available and qualified crew members for a given flight requirement.
        public async Task<ServiceResult<IEnumerable<CrewAvailabilityResponseDto>>> FindAvailableCrewAsync(CrewAvailabilityRequestDto request)
        {
            _logger.LogInformation("Finding available crew for flight between {Start} and {End}.", request.FlightDepartureTime, request.FlightArrivalTime);
            try
            {
                // 1. Get potentially suitable crew based on position and base (basic filter)
                var potentialCrew = await _unitOfWork.CrewMembers.FindAvailableCrewAsync(request.RequiredPosition, request.RequiredBaseAirportIata);

                var availableCrewDtos = new List<CrewAvailabilityResponseDto>();
                var requiredStartTime = request.FlightDepartureTime.Add(-AssignmentBuffer);
                var requiredEndTime = request.FlightArrivalTime.Add(AssignmentBuffer);

                // 2. Filter further based on conflicts, qualifications, certifications
                foreach (var crew in potentialCrew)
                {
                    if (crew.Employee?.AppUser == null) continue; // Skip if user data is missing

                    // 2a. Check for conflicting assignments
                    var conflicts = await _unitOfWork.FlightCrews.GetAssignmentsForCrewMemberByDateRangeAsync(
                        crew.EmployeeId, requiredStartTime, requiredEndTime);
                    if (conflicts.Any())
                    {
                        continue; // Skip if conflicting assignment found
                    }

                    var availabilityDto = _mapper.Map<CrewAvailabilityResponseDto>(crew);
                    availabilityDto.IsTypeRated = true; // Default
                    availabilityDto.HasValidCertification = true; // Default

                    // 2b. Check Pilot Type Rating
                    if (crew.Position.Equals("Pilot", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!request.RequiredAircraftTypeId.HasValue)
                        {
                            _logger.LogWarning("Aircraft Type ID is required when searching for available pilots.");
                            // Or skip this pilot if type rating check is mandatory
                            continue;
                        }
                        var pilotProfile = await _unitOfWork.Pilots.GetByEmployeeIdAsync(crew.EmployeeId);
                        if (pilotProfile == null || pilotProfile.AircraftTypeId != request.RequiredAircraftTypeId.Value)
                        {
                            availabilityDto.IsTypeRated = false;
                            continue; // Skip pilot if not type-rated
                        }
                    }

                    // 2c. Check Certifications (Basic expiry check)
                    var certifications = await _unitOfWork.Certifications.GetByCrewMemberAsync(crew.EmployeeId);
                    if (certifications.Any(c => c.ExpiryDate.HasValue && c.ExpiryDate.Value < request.FlightDepartureTime.Date))
                    {
                        availabilityDto.HasValidCertification = false;
                        continue; // Skip if certification expired before the flight date
                    }

                    availableCrewDtos.Add(availabilityDto);
                }

                _logger.LogInformation("Found {Count} potentially available crew members.", availableCrewDtos.Count);
                return ServiceResult<IEnumerable<CrewAvailabilityResponseDto>>.Success(availableCrewDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding available crew.");
                return ServiceResult<IEnumerable<CrewAvailabilityResponseDto>>.Failure("An error occurred while searching for available crew.");
            }
        }
    }
}