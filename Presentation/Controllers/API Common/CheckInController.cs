using Application.DTOs.BoardingPass;  
using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Errors;  
using System;
using System.Collections.Generic;  
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller handles the check-in process (issuing boarding passes)
    // and gate operations (scanning boarding passes).
    [ApiController]
    [Route("api/v1/checkin")] // Use 'checkin' as the base route
    [Produces("application/json")]
    [Authorize] // All actions require auth unless specified
    public class CheckInController : ControllerBase
    {
        private readonly IBoardingPassService _boardingPassService;
        private readonly ILogger<CheckInController> _logger;

        public CheckInController(IBoardingPassService boardingPassService, ILogger<CheckInController> logger)
        {
            _boardingPassService = boardingPassService;
            _logger = logger;
        }

        #region --- Public API (Customer Online Check-In) ---
        // Endpoints used by the public SingaporeAir website for online check-in

        // POST: api/v1/checkin/generate
        // Customer action: Initiates online check-in and generates a boarding pass.
        [HttpPost("generate")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateBoardingPass([FromBody] GenerateBoardingPassRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _boardingPassService.GenerateBoardingPassAsync(request, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // Handle other validation errors (e.g., "Boarding pass already exists", "Seat assignment is required")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created with the new resource
                return CreatedAtAction(
                    nameof(GetBoardingPassById), // Points to the admin endpoint for retrieval
                    new { passId = result.Data.PassId },
                    new ApiResponse(StatusCodes.Status201Created, "Boarding pass generated successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception generating boarding pass for BookingId {BookingId}, PassengerId {PassengerId}.", request.BookingId, request.PassengerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/checkin/booking/{bookingId}/passenger/{passengerId}
        // Customer action: Retrieves an already-issued boarding pass.
        [HttpGet("booking/{bookingId:int}/passenger/{passengerId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBoardingPassForPassenger(int bookingId, int passengerId)
        {
            try
            {
                // Call the service method
                var result = await _boardingPassService.GetBoardingPassByBookingPassengerAsync(bookingId, passengerId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Boarding pass retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving boarding pass for BookingId {BookingId}, PassengerId {PassengerId}.", bookingId, passengerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Admin API (Airport Operations) ---
        // Endpoints used by the Airport Management System (Check-in counters, Gate Agents)

        // POST: api/v1/checkin/admin/gate-scan
        // Gate Agent action: Simulates scanning a boarding pass at the gate.
        [HttpPost("admin/gate-scan")]
        [Authorize(Roles = "Admin, SuperAdmin, GateAgent")] // Specific roles
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ScanBoardingPassAtGate([FromBody] GateScanRequestDto scanRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _boardingPassService.ScanBoardingPassAtGateAsync(scanRequest, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // Handle logical failures (e.g., "not valid for this flight", "already boarded")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Boarding successful.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during gate scan for PassId {PassId}, FlightId {FlightId}.", scanRequest.PassId, scanRequest.FlightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/checkin/admin/flight-manifest/{flightInstanceId:int}
        // Admin/Agent action: Retrieves all issued boarding passes for a specific flight (Gate Manifest).
        [HttpGet("admin/flight-manifest/{flightInstanceId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor, CheckInAgent, GateAgent")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlightManifest(int flightInstanceId)
        {
            try
            {
                // Call the service method
                var result = await _boardingPassService.GetBoardingPassesForFlightAsync(flightInstanceId);

                if (!result.IsSuccess)
                {
                    // This is likely an internal error
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight manifest retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving manifest for FlightInstanceId {FlightId}.", flightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/checkin/admin/search
        // Admin action: Searches all boarding passes in the system with filters.
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchBoardingPasses(
            [FromQuery] BoardingPassFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size limit

            try
            {
                // Call the search service method
                var result = await _boardingPassService.SearchBoardingPassesAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Create a structured data object for the response
                var paginatedData = new
                {
                    items = result.Data.Items,
                    pagination = new
                    {
                        totalCount = result.Data.TotalCount,
                        pageSize = result.Data.PageSize,
                        currentPage = result.Data.PageNumber,
                        totalPages = result.Data.TotalPages
                    }
                };

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Boarding pass search retrieved successfully.", paginatedData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during boarding pass search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/checkin/admin/pass/{passId:int}
        // Admin/Agent action: Retrieves a specific boarding pass by its internal ID.
        [HttpGet("admin/pass/{passId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor, CheckInAgent, GateAgent")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBoardingPassById(int passId)
        {
            try
            {
                // Call the service method
                var result = await _boardingPassService.GetBoardingPassByIdAsync(passId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Boarding pass retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving BoardingPassId {PassId}.", passId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/checkin/admin/void/{passId:int}
        // Admin/Agent action: Voids (soft-deletes) a boarding pass and reverts ticket status.
        [HttpDelete("admin/void/{passId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, CheckInAgent")] // As per service logic
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoidBoardingPass(int passId)
        {
            try
            {
                // Call the service method
                var result = await _boardingPassService.VoidBoardingPassAsync(passId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // e.g., "Cannot void... passenger has boarded"
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Boarding pass voided successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception voiding BoardingPassId {PassId}.", passId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}