using Application.DTOs.CrewScheduling;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for assigning crew members to flight instances and managing rosters.
    public interface ICrewSchedulingService
    {
        // Assigns one or more crew members to a flight instance.
        Task<ServiceResult> AssignCrewToFlightAsync(AssignCrewRequestDto request, ClaimsPrincipal performingUser);

        // Removes a specific crew member assignment from a flight instance.
        Task<ServiceResult> RemoveCrewFromFlightAsync(int flightInstanceId, int crewMemberEmployeeId, ClaimsPrincipal performingUser);

        // Retrieves the full crew roster for a specific flight instance.
        Task<ServiceResult<FlightRosterDto>> GetFlightRosterAsync(int flightInstanceId);

        // Retrieves the flight schedule for a specific crew member within a date range.
        Task<ServiceResult<CrewScheduleDto>> GetCrewMemberScheduleAsync(CrewScheduleRequestDto request);

        // Finds potentially available and qualified crew members for a given flight requirement.
        Task<ServiceResult<IEnumerable<CrewAvailabilityResponseDto>>> FindAvailableCrewAsync(CrewAvailabilityRequestDto request);
    }
}