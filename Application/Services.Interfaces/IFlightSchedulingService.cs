using Application.DTOs.FlightSchedule;
using Application.Models;  
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing Flight Schedules and Flight Leg Definitions.
    /// This is the core engine for defining future flight operations (timetables).
    /// </summary>
    public interface IFlightSchedulingService
    {
        // --- Schedule CRUD & Lookups ---

        /// <summary>
        /// Retrieves a single active flight schedule by its ID, including full route details.
        /// </summary>
        /// <param name="scheduleId">The ID of the flight schedule.</param>
        /// <returns>A ServiceResult containing the detailed FlightScheduleDto.</returns>
        Task<ServiceResult<FlightScheduleDto>> GetScheduleByIdAsync(int scheduleId);

        /// <summary>
        /// Retrieves a paginated list of flight schedules based on advanced filters.
        /// </summary>
        /// <param name="filter">The filter criteria (flight number, route, airline, date).</param>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Results per page.</param>
        /// <returns>A ServiceResult containing a paginated list of matching schedules.</returns>
        Task<ServiceResult<PaginatedResult<FlightScheduleDto>>> SearchSchedulesAsync(ScheduleFilterDto filter, int pageNumber, int pageSize);

        /// <summary>
        /// Creates a new flight schedule. Performs validation on foreign keys and time logic.
        /// </summary>
        /// <param name="createDto">The data for the new schedule.</param>
        /// <returns>A ServiceResult containing the created FlightScheduleDto.</returns>
        Task<ServiceResult<FlightScheduleDto>> CreateScheduleAsync(CreateFlightScheduleDto createDto);

        /// <summary>
        /// Updates core details (like scheduled times, aircraft type) of an existing flight schedule.
        /// </summary>
        /// <param name="scheduleId">The ID of the schedule to update.</param>
        /// <param name="updateDto">The updated data.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<FlightScheduleDto>> UpdateScheduleAsync(int scheduleId, CreateFlightScheduleDto updateDto); // Reusing Create DTO for convenience

        /// <summary>
        /// Soft deletes a flight schedule. Fails if active flight instances still exist.
        /// </summary>
        /// <param name="scheduleId">The ID of the schedule to soft delete.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> DeleteScheduleAsync(int scheduleId);

        // --- Leg Definition Methods ---

        /// <summary>
        /// Retrieves all active flight leg definitions (segments) for a given flight schedule, ordered by segment number.
        /// </summary>
        /// <param name="scheduleId">The ID of the flight schedule.</param>
        /// <returns>A ServiceResult containing a list of FlightLegDefDto objects.</returns>
        Task<ServiceResult<IEnumerable<FlightLegDefDto>>> GetLegsByScheduleAsync(int scheduleId);

        /// <summary>
        /// Creates a new flight leg definition (segment) within a schedule.
        /// </summary>
        /// <param name="scheduleId">The ID of the parent schedule.</param>
        /// <param name="createDto">The data for the new leg.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<FlightLegDefDto>> CreateFlightLegAsync(int scheduleId, CreateFlightLegDefDto createDto);
    }
}