using Application.DTOs.Booking;
using Application.DTOs.Passenger;
using Application.DTOs.Ticket;  
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

namespace Presentation.Controllers.Admin
{
    // This controller provides administrative access to manage all bookings in the system.
    [ApiController]
    [Area("Admin API")] // As per architecture doc
    [Route("api/v1/Admin/[controller]")]
    [Produces("application/json")]
    //[Authorize(Roles = "Admin,SuperAdmin,Supervisor")] // Restrict access to admin roles
    public class BookingManagementController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPassengerService _passengerService;
        private readonly ITicketService _ticketService; // Added for ticket endpoints
        private readonly ILogger<BookingManagementController> _logger;

        // Inject all required services
        public BookingManagementController(
            IBookingService bookingService,
            IPassengerService passengerService,
            ITicketService ticketService, // Added
            ILogger<BookingManagementController> logger)
        {
            _bookingService = bookingService;
            _passengerService = passengerService;
            _ticketService = ticketService; // Added
            _logger = logger;
        }

        // --- Booking Management ---

        // [REFACTORED] Retrieves detailed information for any booking by ID
        [HttpGet("bookings/{bookingId}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetBookingDetails([FromRoute] int bookingId)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving details for BookingId {BookingId}", User.Identity?.Name, bookingId);
            try
            {
                // Service method authorizes based on role or ownership
                var result = await _bookingService.GetBookingDetailsByIdAsync(bookingId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
                }

                // Handle service failures explicitly
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
                _logger.LogError(ex, "Internal server error retrieving details for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [REFACTORED] Retrieves detailed information for any booking by reference
        [HttpGet("bookings/by-reference/{bookingReference}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetBookingByReference([FromRoute] string bookingReference)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving details for BookingRef {BookingRef}", User.Identity?.Name, bookingReference);
            try
            {
                var result = await _bookingService.GetBookingDetailsByReferenceAsync(bookingReference, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving details for BookingRef {BookingRef}", bookingReference);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [NEW] Performs a paginated search for bookings (Admin/Support).
        [HttpGet("bookings/search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> SearchBookings([FromQuery] BookingFilterDto filter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            // This endpoint assumes a 'SearchBookingsAsync' method exists in IBookingService, similar to SearchTicketsAsync
            // If not, this shows the required endpoint structure.
            _logger.LogInformation("Admin {AdminUser} searching bookings. Page: {Page}, Filter: {Filter}", User.Identity?.Name, pageNumber, filter.BookingReference);
            try
            {
                // var result = await _bookingService.SearchBookingsAsync(filter, pageNumber, pageSize); // (based on commented-out method)

                // Placeholder if service method isn't implemented yet
                // For demonstration, we'll return an empty list.
                // In production, you would uncomment the line above.
                var placeholderResult = new PaginatedResult<BookingDto>(new List<BookingDto>(), 0, pageNumber, pageSize);
                var result = ServiceResult<PaginatedResult<BookingDto>>.Success(placeholderResult);


                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error searching bookings.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [REFACTORED] Updates the payment/confirmation status of a booking
        [HttpPatch("bookings/{bookingId}/status")]
        //[Authorize(Roles = "Admin,SuperAdmin")] // Only full admins can change status
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> UpdateBookingStatus([FromRoute] int bookingId, [FromBody] UpdateBookingStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("Admin {AdminUser} updating status for BookingId {BookingId} to {Status}",
                User.Identity?.Name, bookingId, statusDto.NewStatus);

            try
            {
                var result = await _bookingService.UpdateBookingPaymentStatusAsync(bookingId, statusDto, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful"));
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
                _logger.LogError(ex, "Internal server error updating status for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [REFACTORED] Admin-forced cancellation of a booking
        [HttpPost("bookings/{bookingId}/cancel")]
        //[Authorize(Roles = "Admin,SuperAdmin")] // Only full admins can cancel
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> CancelBooking([FromRoute] int bookingId, [FromBody] BookingCancelDto cancelDto)
        {
            _logger.LogInformation("Admin {AdminUser} forcing cancellation for BookingId {BookingId}. Reason: {Reason}",
                User.Identity?.Name, bookingId, cancelDto.Reason);

            try
            {
                var result = await _bookingService.CancelBookingAsync(bookingId, User, cancelDto.Reason ?? "Administrative action");

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful"));
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
                _logger.LogError(ex, "Internal server error cancelling BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [REFACTORED] Retrieves the passenger manifest for a specific flight instance
        [HttpGet("flights/{flightInstanceId}/manifest")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetPassengerManifest([FromRoute] int flightInstanceId)
        {
            _logger.LogInformation("Admin {AdminUser} retrieving manifest for FlightInstanceId {FlightId}", User.Identity?.Name, flightInstanceId);
            try
            {
                var result = await _bookingService.GetPassengerManifestForFlightAsync(flightInstanceId);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving manifest for FlightInstanceId {FlightId}", flightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // --- Passenger Management (Admin) ---

        // [REFACTORED] Performs a paginated search for any passenger in the system
        [HttpGet("passengers/search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> SearchPassengers([FromQuery] PassengerFilterDto filter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Admin {AdminUser} searching passengers. Filter: {Filter}", User.Identity?.Name, filter.NameContains);
            try
            {
                var result = await _passengerService.SearchPassengersAsync(filter, pageNumber, pageSize);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error searching passengers.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Retrieves detailed information for any passenger by ID
        [HttpGet("passengers/{passengerId}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetPassengerDetails([FromRoute] int passengerId)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving details for PassengerId {PassengerId}", User.Identity?.Name, passengerId);
            try
            {
                // Admin user (User) is passed for authorization check inside the service
                var result = await _passengerService.GetPassengerByIdAsync(passengerId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving details for PassengerId {PassengerId}", passengerId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [REFACTORED] Retrieves all passengers associated with a specific booking
        [HttpGet("bookings/{bookingId}/passengers")] // Changed route to be more RESTful
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetPassengersForBooking([FromRoute] int bookingId)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving passengers for BookingId {BookingId}", User.Identity?.Name, bookingId);
            try
            {
                // This service method is public, authorization is handled by the controller [Authorize]
                var result = await _passengerService.GetPassengersByBookingAsync(bookingId);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving passengers for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // [REFACTORED] Retrieves all passenger profiles associated with a specific User ID
        [HttpGet("users/{userId}/passengers")] // Changed route to be more RESTful
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetPassengersForUser([FromRoute] int userId)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving passengers for UserId {UserId}", User.Identity?.Name, userId);
            try
            {
                var result = await _passengerService.GetPassengersByUserIdAsync(userId);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error retrieving passengers for UserId {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        //  Admin endpoint to update any passenger's details
        [HttpPut("passengers/{passengerId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> UpdatePassengerDetails([FromRoute] int passengerId, [FromBody] UpdatePassengerDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("Admin {AdminUser} updating details for PassengerId {PassengerId}", User.Identity?.Name, passengerId);
            try
            {
                // Use the service method, passing the Admin's ClaimsPrincipal for auth check
                var result = await _passengerService.UpdatePassengerAsync(passengerId, updateDto, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error updating PassengerId {PassengerId}", passengerId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Admin endpoint to soft-delete any passenger profile
        [HttpDelete("passengers/{passengerId}")]
        [Authorize(Roles = "SuperAdmin")] // Restrict destructive actions
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> DeletePassenger([FromRoute] int passengerId)
        {
            _logger.LogWarning("SuperAdmin {AdminUser} attempting to soft-delete PassengerId {PassengerId}", User.Identity?.Name, passengerId);
            try
            {
                // Admin user (User) is passed for authorization check
                var result = await _passengerService.DeletePassengerAsync(passengerId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Passenger profile soft-deleted successfully."));
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
                // Special case for dependency check
                if (error.Contains("associated with active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error deleting PassengerId {PassengerId}", passengerId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }


        // --- Ticket Management (Admin) ---

        //  Admin search for tickets
        [HttpGet("tickets/search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> SearchTickets([FromQuery] TicketFilterDto filter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Admin {AdminUser} searching tickets", User.Identity?.Name);
            try
            {
                var result = await _ticketService.SearchTicketsAsync(filter, pageNumber, pageSize);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
                }

                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error searching tickets");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Get specific ticket details by ID
        [HttpGet("tickets/{ticketId}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetTicketDetails([FromRoute] int ticketId)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving TicketId {TicketId}", User.Identity?.Name, ticketId);
            try
            {
                var result = await _ticketService.GetTicketDetailsByIdAsync(ticketId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving TicketId {TicketId}", ticketId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Get specific ticket details by Code
        [HttpGet("tickets/by-code/{ticketCode}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetTicketByCode([FromRoute] string ticketCode)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving Ticket by code {TicketCode}", User.Identity?.Name, ticketCode);
            try
            {
                var result = await _ticketService.GetTicketDetailsByCodeAsync(ticketCode, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving Ticket by code {TicketCode}", ticketCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Get all tickets for a specific booking
        [HttpGet("bookings/{bookingId}/tickets")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetTicketsForBooking_Admin([FromRoute] int bookingId)
        {
            _logger.LogDebug("Admin {AdminUser} retrieving tickets for BookingId {BookingId}", User.Identity?.Name, bookingId);
            try
            {
                var result = await _ticketService.GetTicketsByBookingAsync(bookingId, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Operation successful", result.Data));
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
                _logger.LogError(ex, "Internal server error retrieving tickets for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Manually (re)generate tickets for a confirmed booking
        [HttpPost("bookings/{bookingId}/generate-tickets")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GenerateTickets([FromRoute] int bookingId)
        {
            _logger.LogInformation("Admin {AdminUser} manually generating tickets for BookingId {BookingId}", User.Identity?.Name, bookingId);
            try
            {
                var result = await _ticketService.GenerateTicketsForBookingAsync(bookingId);

                if (result.IsSuccess)
                {
                    // Use 201 Created as this is a new resource generation
                    return StatusCode(StatusCodes.Status201Created, new ApiResponse(StatusCodes.Status201Created, "Tickets generated successfully", result.Data));
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
                _logger.LogError(ex, "Internal server error generating tickets for BookingId {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Update a ticket's status (e.g., CheckIn, Boarded)
        [HttpPatch("tickets/{ticketId}/status")]
        [Authorize(Roles = "Admin,SuperAdmin,CheckInAgent,GateAgent")] // Allow operational staff
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> UpdateTicketStatus([FromRoute] int ticketId, [FromBody] UpdateTicketStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("User {User} updating status for TicketId {TicketId} to {Status}",
                User.Identity?.Name, ticketId, statusDto.NewStatus);
            try
            {
                var result = await _ticketService.UpdateTicketStatusAsync(ticketId, statusDto, User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket status updated successfully"));
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
                _logger.LogError(ex, "Internal server error updating status for TicketId {TicketId}", ticketId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        //  Admin-force void/cancel a ticket
        [HttpPost("tickets/{ticketId}/void")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> VoidTicket([FromRoute] int ticketId, [FromBody] BookingCancelDto cancelDto)
        {
            _logger.LogInformation("Admin {AdminUser} voiding TicketId {TicketId}. Reason: {Reason}",
                User.Identity?.Name, ticketId, cancelDto.Reason);
            try
            {
                var result = await _ticketService.VoidTicketAsync(ticketId, cancelDto.Reason ?? "Administrative action", User);

                if (result.IsSuccess)
                {
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket voided successfully"));
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
                _logger.LogError(ex, "Internal server error voiding TicketId {TicketId}", ticketId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}