using Application.DTOs.FlightSchedule;
using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Errors;  
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller manages flight schedule templates (not live instances).
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/schedules")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only
    public class FlightScheduleController : ControllerBase
    {
        private readonly IFlightSchedulingService _scheduleService;
        private readonly ILogger<FlightScheduleController> _logger;

        public FlightScheduleController(IFlightSchedulingService scheduleService, ILogger<FlightScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        #region --- Flight Schedule Endpoints ---

        // GET: api/v1/admin/schedules/search
        // Performs an advanced, paginated search for flight schedules.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchSchedules(
            [FromQuery] ScheduleFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                // Call the service method
                var result = await _scheduleService.SearchSchedulesAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Schedule search retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during schedule search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/schedules/{scheduleId:int}
        // Retrieves a single flight schedule by its ID.
        [HttpGet("{scheduleId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetScheduleById(int scheduleId)
        {
            try
            {
                // Call the service method
                var result = await _scheduleService.GetScheduleByIdAsync(scheduleId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight schedule retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving schedule ID {ScheduleId}.", scheduleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/schedules
        // Creates a new flight schedule template.
        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")] // Write access
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateFlightScheduleDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _scheduleService.CreateScheduleAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "already exists", "Route not found")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetScheduleById),
                    new { scheduleId = result.Data.ScheduleId },
                    new ApiResponse(StatusCodes.Status201Created, "Flight schedule created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating schedule {FlightNo}.", createDto.FlightNo);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/schedules/{scheduleId:int}
        // Updates an existing flight schedule template.
        [HttpPut("{scheduleId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Write access
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] CreateFlightScheduleDto updateDto)
        {
            // We reuse CreateFlightScheduleDto for updates as per the service signature
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method (which now returns the DTO)
                var result = await _scheduleService.UpdateScheduleAsync(scheduleId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle validation errors (e.g., "active flight instances exist", "already exists")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // MODIFIED: Return the updated DTO (result.Data) instead of null
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight schedule updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating schedule {ScheduleId}.", scheduleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/schedules/{scheduleId:int}
        // Soft-deletes a flight schedule (if no dependencies exist).
        [HttpDelete("{scheduleId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Destructive action
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSchedule(int scheduleId)
        {
            try
            {
                // Call the service method
                var result = await _scheduleService.DeleteScheduleAsync(scheduleId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle dependency error
                    if (result.Errors.Any(e => e.Contains("flight instances")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight schedule soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting schedule {ScheduleId}.", scheduleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Flight Leg Definition Endpoints ---

        // GET: api/v1/admin/schedules/{scheduleId:int}/legs
        // Retrieves all active flight legs (segments) for a specific schedule.
        [HttpGet("{scheduleId:int}/legs")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLegsBySchedule(int scheduleId)
        {
            try
            {
                // Call the service method
                var result = await _scheduleService.GetLegsByScheduleAsync(scheduleId);

                if (!result.IsSuccess)
                {
                    // This means the schedule itself wasn't found
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight legs retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving legs for schedule {ScheduleId}.", scheduleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/schedules/{scheduleId:int}/legs
        // Creates a new flight leg definition (segment) within a schedule.
        [HttpPost("{scheduleId:int}/legs")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Write access
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateFlightLeg(int scheduleId, [FromBody] CreateFlightLegDefDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _scheduleService.CreateFlightLegAsync(scheduleId, createDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle validation errors (e.g., "already exists", "Airport invalid")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // We don't have a "GetLegById" endpoint, so just return 201 with data
                return StatusCode(StatusCodes.Status201Created,
                    new ApiResponse(StatusCodes.Status201Created, "Flight leg created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating leg for schedule {ScheduleId}.", scheduleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }
         

        #endregion
    }
}