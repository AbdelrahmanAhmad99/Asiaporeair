using Application.DTOs.FlightOperations;
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
    // This controller manages live flight instances (Operations).
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/operations")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only on most
    public class FlightOperationsController : ControllerBase
    {
        private readonly IFlightOperationsService _flightOpsService;
        private readonly ILogger<FlightOperationsController> _logger;

        public FlightOperationsController(IFlightOperationsService flightOpsService, ILogger<FlightOperationsController> logger)
        {
            _flightOpsService = flightOpsService;
            _logger = logger;
        }

        #region --- Instance Read Endpoints ---

        // GET: api/v1/admin/operations/search
        // Performs an advanced, paginated search for flight instances.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchFlightInstances(
            [FromQuery] FlightInstanceFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                // Call the service method
                var result = await _flightOpsService.SearchFlightInstancesAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight instance search retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during flight instance search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/operations/{instanceId:int}
        // Retrieves full details for a single flight instance.
        [HttpGet("{instanceId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInstanceById(int instanceId)
        {
            try
            {
                // Call the service method
                var result = await _flightOpsService.GetInstanceByIdAsync(instanceId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight instance retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving instance ID {InstanceId}.", instanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/operations/by-date-range
        // Retrieves flight instances for a specific date range, optionally for one airport. 
        [HttpGet("by-date-range")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInstancesByDateRange( 
            [FromQuery][Required] DateTime startDate,
            [FromQuery][Required] DateTime endDate,
            [FromQuery] string? airportIataCode = null)
        {
            try
            {
                // Call the service method
                var result = await _flightOpsService.GetInstancesByDateRangeAsync(startDate, endDate, airportIataCode);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                       new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight instances retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving instances by date range.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/operations/by-aircraft/{tailNumber}
        // Retrieves flight instances for a specific aircraft in a date range.
        [HttpGet("by-aircraft/{tailNumber}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInstancesByAircraft( // <-- (THE FIX)
            string tailNumber,
            [FromQuery][Required] DateTime startDate,
            [FromQuery][Required] DateTime endDate)
        {
            try
            {
                // Call the service method
                var result = await _flightOpsService.GetInstancesByAircraftAsync(tailNumber, startDate, endDate);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                       new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight instances for aircraft retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving instances for aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Instance Write Endpoints ---

        // POST: api/v1/admin/operations/generate-from-schedules
        // Generates flight instances for a date range based on flight schedules.
        [HttpPost("generate-from-schedules")]
        [Authorize(Roles = "SuperAdmin")] // High-level operational task
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateInstancesFromSchedules([FromBody] GenerateInstancesRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _flightOpsService.GenerateInstancesFromSchedulesAsync(request);

                if (!result.IsSuccess)
                {
                    // Even if it fails, it might contain a partial report
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Generation failed or completed with errors.", result.Data ?? (object)result.Errors));
                }
 
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight instance generation completed.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception generating instances from schedules.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/operations/ad-hoc
        // Creates a single, non-scheduled (ad-hoc) flight instance.
        [HttpPost("ad-hoc")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAdHocFlightInstance([FromBody] CreateAdHocFlightInstanceDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _flightOpsService.CreateAdHocFlightInstanceAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "Route not found", "Aircraft not found")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return CreatedAtAction(
                    nameof(GetInstanceById),
                    new { instanceId = result.Data.InstanceId },
                    new ApiResponse(StatusCodes.Status201Created, "Ad-hoc flight instance created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating ad-hoc flight {FlightNo}.", createDto.FlightNo);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/operations/{instanceId:int}/status
        // Updates the operational status of a flight (e.g., Delayed, Cancelled).
        [HttpPut("{instanceId:int}/status")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFlightStatus(int instanceId, [FromBody] UpdateFlightStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _flightOpsService.UpdateFlightStatusAsync(instanceId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));  

                  
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }
 
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Flight status updated successfully.",
                    result.Data  
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating status for instance {InstanceId}.", instanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/operations/{instanceId:int}/times
        // Updates the actual departure/arrival times for a flight.
        [HttpPut("{instanceId:int}/times")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFlightTimes(int instanceId, [FromBody] UpdateFlightTimesDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _flightOpsService.UpdateFlightTimesAsync(instanceId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));  

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }
                 
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Flight times updated successfully.",
                    result.Data  
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating times for instance {InstanceId}.", instanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/operations/{instanceId:int}/assign-aircraft
        // Assigns a specific aircraft tail number to a flight instance.
        [HttpPut("{instanceId:int}/assign-aircraft")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignAircraftToInstance(int instanceId, [FromBody] AssignAircraftDto assignDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _flightOpsService.AssignAircraftToInstanceAsync(instanceId, assignDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle validation errors (e.g., "not operational", "conflicts with")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft assigned to flight successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception assigning aircraft to instance {InstanceId}.", instanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/operations/{instanceId:int}
        // Soft-deletes a flight instance (if no dependencies exist).
        [HttpDelete("{instanceId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Destructive action
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFlightInstance(int instanceId, [FromQuery][Required] string reason)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "A reason is required to delete a flight instance." } });
            }

            try
            {
                // Call the service method
                var result = await _flightOpsService.DeleteFlightInstanceAsync(instanceId, reason);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle dependency error
                    if (result.Errors.Any(e => e.Contains("bookings")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight instance soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting instance {InstanceId}.", instanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}
