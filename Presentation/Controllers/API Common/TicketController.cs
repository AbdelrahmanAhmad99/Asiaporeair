using Application.DTOs.Ticket; // DTOs for Ticket
using Application.Models; // For PaginatedResult
using Application.Services.Interfaces; // ITicketService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Errors; // Required for ApiResponse, ApiValidationErrorResponse, etc.
using System;
using System.Collections.Generic; // For IEnumerable
using System.ComponentModel.DataAnnotations; // For [Required]
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller manages e-tickets,
    // handling customer retrieval and admin management/operational tasks.
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize] // All actions require a logged-in user unless specified
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        #region --- Public API (Customer Facing) ---
        // Endpoints used by the public SingaporeAir website [cite: 2]

        // GET: api/v1/ticket/my-tickets
        // Retrieves a paginated list of tickets for the currently authenticated user.
        [HttpGet("my-tickets")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50; // Max page size limit

            try
            {
                // Call the service method
                var result = await _ticketService.GetMyTicketsAsync(User, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    // This error is likely an internal issue if authentication passed
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

                return Ok(new ApiResponse(StatusCodes.Status200OK, "User tickets retrieved successfully.", paginatedData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving 'My Tickets' for user {UserId}.", User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/ticket/booking/{bookingId}
        // Retrieves all tickets associated with a specific booking (e.g., for a family).
        [HttpGet("booking/{bookingId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsForBooking(int bookingId)
        {
            try
            {
                // Call the service method
                var result = await _ticketService.GetTicketsByBookingAsync(bookingId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("Booking not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Booking tickets retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving tickets for BookingId {BookingId}.", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/ticket/{ticketId}
        // Retrieves full details for a single e-ticket by its internal ID.
        [HttpGet("{ticketId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketDetails(int ticketId)
        {
            try
            {
                // Call the service method
                var result = await _ticketService.GetTicketDetailsByIdAsync(ticketId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("Ticket not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket details retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving TicketId {TicketId}.", ticketId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/ticket/by-code/{ticketCode}
        // Retrieves full details for a single e-ticket by its unique ticket code.
        [HttpGet("by-code/{ticketCode}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketDetailsByCode(string ticketCode)
        {
            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Ticket code cannot be empty." } });
            }

            try
            {
                // Call the service method
                var result = await _ticketService.GetTicketDetailsByCodeAsync(ticketCode, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("Ticket not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket details retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving TicketCode {TicketCode}.", ticketCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Admin API (Management System) ---
        // Endpoints used by the Airport Management System [cite: 6]

        // GET: api/v1/ticket/admin/search
        // Admin: Searches all tickets in the system with advanced filters.
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Roles that need to search
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchTickets(
            [FromQuery] TicketFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size limit

            try
            {
                // Call the search service method
                var result = await _ticketService.SearchTicketsAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    // Search failures are typically internal
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

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket search retrieved successfully.", paginatedData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during ticket search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/ticket/admin/user/{userId}
        // Admin: Retrieves all tickets for a specific user ID (legacy support).
        [HttpGet("admin/user/{userId}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsForUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "User ID cannot be empty." } });
            }

            try
            {
                // Call the legacy service method
                var result = await _ticketService.GetUserTicketsAsync(userId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "User tickets retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving tickets for AppUser {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/ticket/admin/status/{ticketId}
        // Admin/Operations: Updates a ticket's status (e.g., CheckedIn, Boarded).
         // This is used by Check-in counters or Gate Agents[cite: 10].
        [HttpPut("admin/status/{ticketId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor, CheckInAgent, GateAgent")] // Operational roles
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _ticketService.UpdateTicketStatusAsync(ticketId, statusDto, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("Ticket not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // Handle validation errors (e.g., "Cannot change status of a cancelled ticket")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket status updated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating status for TicketId {TicketId}.", ticketId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // Simple DTO for the VoidTicket request body
        public class VoidTicketRequestDto
        {
            [Required(ErrorMessage = "A reason for voiding the ticket is required.")]
            [MinLength(10, ErrorMessage = "Reason must be at least 10 characters.")]
            public string Reason { get; set; }
        }

        // PUT: api/v1/ticket/admin/void/{ticketId}
        // Admin: Voids/Cancels a ticket. This is a special status update.
        [HttpPut("admin/void/{ticketId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Higher privilege than simple status update
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VoidTicket(int ticketId, [FromBody] VoidTicketRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _ticketService.VoidTicketAsync(ticketId, dto.Reason, User);

                if (!result.IsSuccess)
                {
                    // This method inherits errors from UpdateTicketStatusAsync
                    if (result.Errors.Any(e => e.Contains("Ticket not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // Handle validation errors (e.g., "Cannot change status of a cancelled ticket")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Ticket voided successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception voiding TicketId {TicketId}.", ticketId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}