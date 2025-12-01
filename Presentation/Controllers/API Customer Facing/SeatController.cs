using Application.DTOs.Seat;  
using Application.Models;
using Application.Services.Interfaces;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Errors;  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller manages seat maps and assignments.
    [ApiController]
    [Authorize] // All endpoints require auth by default
    [Produces("application/json")]
    [Route("api/v1/Seats")] // Base route for seats
    public class SeatController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly ILogger<SeatController> _logger;  

        public SeatController(ISeatService seatService, ILogger<SeatController> logger)
        {
            _seatService = seatService;
            _logger = logger;
        }
 

        // Retrieves the full seat map for a specific flight instance
        [HttpGet("flight/{flightInstanceId}/seatmap")]
        [AllowAnonymous] // Seat maps can be public
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetSeatMapForFlight([FromRoute] int flightInstanceId)
        {
            _logger.LogInformation("Retrieving seat map for FlightInstanceId {FlightId}.", flightInstanceId);
            try
            {
                // This method exists in SeatService.cs
                var result = await _seatService.GetSeatMapForFlightAsync(flightInstanceId);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Seat map retrieved successfully", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error retrieving seat map for FlightInstanceId {FlightId}", flightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Retrieves *only* the available seats for a flight
        [HttpGet("flight/available-seats")]
        [AllowAnonymous] // Useful for public search
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetAvailableSeats([FromQuery] SeatAvailabilityRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("Retrieving available seats for FlightInstanceId {FlightId}.", request.FlightInstanceId);
            try
            {
                // This method exists in SeatService.cs
                var result = await _seatService.GetAvailableSeatsAsync(request);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Available seats retrieved successfully", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error retrieving available seats for FlightInstanceId {FlightId}", request.FlightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Retrieves all current seat assignments for a *specific booking*
        [HttpGet("booking/{bookingId}/assignments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetSeatAssignmentsForBooking([FromRoute] int bookingId)
        {
            _logger.LogInformation("User {UserEmail} retrieving seat assignments for BookingId {BookingId}.", User.Identity?.Name, bookingId);
            try
            {
                // This method exists in SeatService.cs
                var result = await _seatService.GetSeatAssignmentsForBookingAsync(bookingId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Booking seat assignments retrieved successfully", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error retrieving seat assignments for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Assigns a *single* seat to a passenger
        [HttpPost("assign-seat")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> AssignSeat([FromBody] AssignSeatRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("User {UserEmail} assigning SeatId {SeatId} to PassengerId {PassengerId} on BookingId {BookingId}.",
                User.Identity?.Name, request.SeatId, request.PassengerId, request.BookingId);
            try
            {
                // This method exists in SeatService.cs
                var result = await _seatService.AssignSeatAsync(request, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Seat assigned successfully"));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
                }
                if (error.Contains("taken", StringComparison.OrdinalIgnoreCase) || error.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error assigning seat for BookingId {BookingId}", request.BookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Assigns *multiple* seats in one request
        [HttpPost("booking/{bookingId}/reserve-seats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> ReserveSeats([FromRoute] int bookingId, [FromBody] List<ReserveSeatDto> reservesDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("User {UserEmail} reserving {Count} seats for BookingId {BookingId}.",
                User.Identity?.Name, reservesDto.Count, bookingId);
            try
            {
                // This method exists in SeatService.cs
                // Note: This service method needs authorization logic added to it.
                var result = await _seatService.ReserveSeatsAsync(bookingId, reservesDto);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Seats reserved successfully", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error reserving seats for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Removes a seat assignment for a specific passenger
        [HttpDelete("booking/{bookingId}/passenger/{passengerId}/remove-seat")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> RemoveSeatAssignment([FromRoute] int bookingId, [FromRoute] int passengerId)
        {
            _logger.LogInformation("User {UserEmail} removing seat for PassengerId {PassengerId} on BookingId {BookingId}.",
                User.Identity?.Name, passengerId, bookingId);
            try
            {
                // This method exists in SeatService.cs
                var result = await _seatService.RemoveSeatAssignmentAsync(bookingId, passengerId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Seat assignment removed successfully"));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error removing seat for PassengerId {PassengerId} on BookingId {BookingId}", passengerId, bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
 
    }
}