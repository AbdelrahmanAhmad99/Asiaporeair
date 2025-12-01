using Application.DTOs.FlightOperations;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for managing real-time flight operations (Flight Instances).
    public interface IFlightOperationsService
    {
        // Retrieves a single flight instance by its unique ID with full details.
        Task<ServiceResult<FlightInstanceDto>> GetInstanceByIdAsync(int instanceId);

        // Retrieves a lightweight list of flights for Flight Information Display Systems (FIDS).
        Task<ServiceResult<IEnumerable<FlightInstanceBriefDto>>> GetFlightInstancesForFidsAsync(string airportIataCode, string direction);

        // Performs an advanced, paginated search for flight instances.
        Task<ServiceResult<PaginatedResult<FlightInstanceDto>>> SearchFlightInstancesAsync(FlightInstanceFilterDto filter, int pageNumber, int pageSize);

        // Retrieves all flight instances for a specific date range (for reports or ops).
        Task<ServiceResult<IEnumerable<FlightInstanceDto>>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate, string? airportIataCode = null);

        // Retrieves flight instances assigned to a specific aircraft tail number.
        Task<ServiceResult<IEnumerable<FlightInstanceDto>>> GetInstancesByAircraftAsync(string tailNumber, DateTime startDate, DateTime endDate);

        // Generates flight instances for a date range based on existing FlightSchedules.
        Task<ServiceResult<GenerationReportDto>> GenerateInstancesFromSchedulesAsync(GenerateInstancesRequestDto request);

        // Creates a single, non-scheduled flight (e.g., charter, ferry flight).
        Task<ServiceResult<FlightInstanceDto>> CreateAdHocFlightInstanceAsync(CreateAdHocFlightInstanceDto createDto);

        // Updates the primary operational status of a flight (e.g., Delayed, Cancelled).
        Task<ServiceResult<FlightInstanceDto>> UpdateFlightStatusAsync(int instanceId, UpdateFlightStatusDto updateDto);

        // Updates the estimated or actual departure/arrival times for a flight.
        Task<ServiceResult<FlightInstanceDto>> UpdateFlightTimesAsync(int instanceId, UpdateFlightTimesDto updateDto);

        // Assigns a specific aircraft (by tail number) to a flight instance.
        Task<ServiceResult> AssignAircraftToInstanceAsync(int instanceId, AssignAircraftDto assignDto); 

        // Soft-deletes a flight instance (e.g., if generated in error).
        Task<ServiceResult> DeleteFlightInstanceAsync(int instanceId, string reason);

        // Retrieves summary data for the airport operational dashboard.
        Task<ServiceResult<OperationalDashboardDto>> GetOperationalDashboardAsync(string airportIataCode, DateTime date);
    }
}