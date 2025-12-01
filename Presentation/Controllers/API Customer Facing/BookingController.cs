using Application.DTOs.Booking;
using Application.DTOs.Passenger;
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
    // This controller handles customer-facing booking creation and management.
    // All endpoints require an authenticated user.
    [ApiController] 
    [Area("Public API")] // As per architecture doc 
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize] // All actions require a logged-in user
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPassengerService _passengerService;
        private readonly ILogger<BookingController> _logger;

        // Inject required services
        public BookingController(
            IBookingService bookingService,
            IPassengerService passengerService,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _passengerService = passengerService;
            _logger = logger;
        }

        

        // Creates a new flight booking
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createDto)
        {
            // 1. Model Validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = errors,
                    Message = "Validation failed for the booking request."
                });
            }

            // 2. Logging Context
            // Capture basic user info safely for logs
            var userName = User.Identity?.Name ?? "Unknown User";
            _logger.LogInformation("User '{User}' is attempting to create a booking for Flight ID {FlightId}.",
                userName, createDto.FlightInstanceId);

            try
            {
                // 3. Call Service
                // We pass the User Principal directly so the service can extract the claims securely.
                var result = await _bookingService.CreateBookingAsync(createDto, User);

                // 4. Handle Failure Scenarios Explicitly
                if (!result.IsSuccess)
                {
                    var errorMessage = result.Errors.FirstOrDefault() ?? "Booking creation failed.";

                    // Determine specific status codes based on the error message content
                    // (In a larger system, use specific Error Types or Enums from ServiceResult)

                    // Authentication/Identity issues
                    if (errorMessage.Contains("Authentication", StringComparison.OrdinalIgnoreCase) ||
                        errorMessage.Contains("not found for the current user", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Booking failed due to authentication issue: {Error}", errorMessage);
                        return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, errorMessage));
                    }

                    // Domain Logic issues (Seats, Flight Status, etc.)
                    if (errorMessage.Contains("Insufficient seats", StringComparison.OrdinalIgnoreCase) ||
                        errorMessage.Contains("status", StringComparison.OrdinalIgnoreCase) ||
                        errorMessage.Contains("departed", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Booking failed due to domain rule: {Error}", errorMessage);
                        // 400 Bad Request is appropriate for business rule violations by the client
                        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, errorMessage));
                    }

                    // Resource Not Found
                    if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, errorMessage));
                    }

                    // Default fallback for logic errors
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, errorMessage));
                }

                // 5. Handle Success Scenario
                if (result.Data != null)
                {
                    // Return 201 Created with the resulting Booking DTO wrapped in a standard response
                    // Ideally, add a 'Location' header pointing to the GetBookingDetails endpoint
                    return StatusCode(StatusCodes.Status201Created, new ApiResponse(
                        StatusCodes.Status201Created,
                        "Booking created successfully.",
                        result.Data
                    ));
                }

                // Edge case: Success but null data (should not happen based on service logic)
                return StatusCode(StatusCodes.Status201Created, new ApiResponse(StatusCodes.Status201Created, "Booking created."));

            }
            catch (Exception ex)
            {
                // 6. Global Exception Safety (Controller Level)
                _logger.LogError(ex, "Unhandled exception in CreateBooking controller for User '{User}'.", userName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred. Please contact support."
                ));
            }
        }

        // Retrieves a paginated list of the current user's bookings
        [HttpGet("my-bookings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetMyBookings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Retrieving bookings for user {UserEmail}", User.Identity?.Name);
            var result = await _bookingService.GetMyBookingsAsync(User, pageNumber, pageSize);

            return HandleResult(result);
        }

        // Retrieves detailed information for a specific booking by ID (must be owned by user)
        [HttpGet("{bookingId}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetBookingDetails([FromRoute] int bookingId)
        {
            _logger.LogDebug("User {UserEmail} retrieving details for BookingId {BookingId}", User.Identity?.Name, bookingId);
            var result = await _bookingService.GetBookingDetailsByIdAsync(bookingId, User);

            return HandleResult(result);
        }

        // Retrieves detailed information for a specific booking by reference (must be owned by user)
        [HttpGet("by-reference/{bookingReference}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetBookingByReference([FromRoute] string bookingReference)
        {
            _logger.LogDebug("User {UserEmail} retrieving details for BookingRef {BookingRef}", User.Identity?.Name, bookingReference);
            var result = await _bookingService.GetBookingDetailsByReferenceAsync(bookingReference, User);

            return HandleResult(result);
        }

        // Cancels a booking (must be owned by user)
        [HttpPost("{bookingId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> CancelBooking([FromRoute] int bookingId, [FromBody] BookingCancelDto cancelDto)
        {
            _logger.LogInformation("User {UserEmail} attempting to cancel BookingId {BookingId}", User.Identity?.Name, bookingId);
            var result = await _bookingService.CancelBookingAsync(bookingId, User, cancelDto.Reason);

            return HandleResult(result);
        }

        // --- Passenger Management (For User's Profile)  ---

        // Retrieves all passenger profiles saved to the current user's account
        [HttpGet("my-passengers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetMySavedPassengers()
        {
            _logger.LogInformation("User {UserEmail} retrieving saved passengers list.", User.Identity?.Name);
            var result = await _passengerService.GetMyPassengersAsync(User);

            return HandleResult(result);
        }

        // Retrieves a specific passenger profile (must be owned by user)
        [HttpGet("passengers/{passengerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetSavedPassenger([FromRoute] int passengerId)
        {
            _logger.LogDebug("User {UserEmail} retrieving PassengerId {PassengerId}.", User.Identity?.Name, passengerId);
            var result = await _passengerService.GetPassengerByIdAsync(passengerId, User);

            return HandleResult(result);
        }

        // Updates a passenger profile saved to the user's account
        [HttpPut("passengers/{passengerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> UpdateSavedPassenger([FromRoute] int passengerId, [FromBody] UpdatePassengerDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleInvalidModelState();
            }

            _logger.LogInformation("User {UserEmail} updating PassengerId {PassengerId}.", User.Identity?.Name, passengerId);
            var result = await _passengerService.UpdatePassengerAsync(passengerId, updateDto, User);

            return HandleResult(result);
        }

        // Deletes a passenger profile from the user's account
        [HttpDelete("passengers/{passengerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> DeleteSavedPassenger([FromRoute] int passengerId)
        {
            _logger.LogInformation("User {UserEmail} deleting PassengerId {PassengerId}.", User.Identity?.Name, passengerId);
            var result = await _passengerService.DeletePassengerAsync(passengerId, User);

            return HandleResult(result);
        }


        // --- Helper Methods ---

        // Handles returning a standardized API response from a ServiceResult
        private IActionResult HandleResult<T>(ServiceResult<T> result, int successStatusCode = StatusCodes.Status200OK)
        {
            if (result.IsSuccess)
            {
                // Handle success with data
                if (result.Data != null)
                {
                    return StatusCode(successStatusCode, new ApiResponse(successStatusCode, "Operation successful", result.Data));
                }
                // Handle success with no data (e.g., Update/Delete)
                return StatusCode(successStatusCode, new ApiResponse(successStatusCode, "Operation successful"));
            }

            // Handle failure
            var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
            if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
            }
            if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
            {
                //return Forbid(error); // 403 Forbidden
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
            }
            // Default to BadRequest
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
        }

        // Handles returning a standardized API response for non-generic ServiceResult
        private IActionResult HandleResult(ServiceResult result)
        {
            if (result.IsSuccess)
            {
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful"));
            }

            // Handle failure
            var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
            if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
            }
            if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
            {
                //return Forbid(error); // 403 Forbidden
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
            }
            // Default to BadRequest
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
        }

        // Handles invalid model state
        private BadRequestObjectResult HandleInvalidModelState()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiValidationErrorResponse { Errors = errors });
        }
    }

    // Helper DTO for cancel reason
    public class BookingCancelDto
    {
        public string? Reason { get; set; }
    }
}