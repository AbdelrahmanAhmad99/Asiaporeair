using Application.DTOs.FrequentFlyer;
using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Errors; 
using System;
using System.Collections.Generic;  
using System.ComponentModel.DataAnnotations;  
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller handles all Frequent Flyer (KrisFlyer) program actions.
    // It provides endpoints for customers to view their account
    // and for administrators to manage all accounts, points, and tiers.
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize] // All actions require a logged-in user unless specified
    public class FrequentFlyerController : ControllerBase
    {
        private readonly IFrequentFlyerService _frequentFlyerService;
        private readonly ILogger<FrequentFlyerController> _logger;

        public FrequentFlyerController(IFrequentFlyerService frequentFlyerService, ILogger<FrequentFlyerController> logger)
        {
            _frequentFlyerService = frequentFlyerService;
            _logger = logger;
        }

        #region --- Public API (Customer Facing) ---
        // Endpoints used by the public SingaporeAir website

        // GET: api/v1/frequentflyer/my-account
        // Retrieves the frequent flyer account details for the currently authenticated user.
        [HttpGet("my-account")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyAccount()
        {
            try
            {
                // Call the service method
                var result = await _frequentFlyerService.GetMyAccountAsync(User);

                if (!result.IsSuccess)
                {
                    // This means the authenticated user has no FF account linked
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Frequent flyer account retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving 'My Account' for user {UserId}.", User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Admin API (Management System) ---
        // Endpoints used by the Airport Management System / Admin Dashboard

        // GET: api/v1/frequentflyer/admin/search
        // Admin: Searches all frequent flyer accounts with advanced filters.
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAccounts(
            [FromQuery] FrequentFlyerFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size limit

            try
            {
                // Call the search service method
                var result = await _frequentFlyerService.SearchAccountsAsync(filter, pageNumber, pageSize);

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

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Frequent flyer accounts retrieved successfully.", paginatedData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during frequent flyer account search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/frequentflyer/admin/user/{userId:int}
        // Admin: Retrieves the FF account linked to a specific internal User ID.
        [HttpGet("admin/user/{userId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountByUserId(int userId)
        {
            try
            {
                // Call the service method
                var result = await _frequentFlyerService.GetAccountByUserIdAsync(userId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Account retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving FF account for UserId {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/frequentflyer/admin/card/{cardNumber}
        // Admin: Retrieves the FF account by its card number.
        [HttpGet("admin/card/{cardNumber}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountByCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Card number cannot be empty." } });
            }

            try
            {
                // Call the service method
                var result = await _frequentFlyerService.GetAccountByCardNumberAsync(cardNumber);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Account retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving FF account for CardNumber {CardNumber}.", cardNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/frequentflyer/admin/create
        // Admin: Creates a new Frequent Flyer account and links it to a User.
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateFrequentFlyerDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _frequentFlyerService.CreateAccountAsync(createDto, User);

                if (!result.IsSuccess)
                {
                    // Check for different failure types
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("already exists")) || result.Errors.Any(e => e.Contains("is already linked")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors }); // 400 Bad Request for business logic validation

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetAccountByUserId), // Points to the new resource location
                    new { userId = result.Data.LinkedUserId },
                    new ApiResponse(StatusCodes.Status201Created, "Frequent flyer account created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating Frequent Flyer account for UserId {UserId}.", createDto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DTO for the ManualAdjustPoints endpoint body
        public class UpdatePointsRequestDto
        {
            [Required]
            [Range(Int32.MinValue, Int32.MaxValue, ErrorMessage = "PointsDelta value is required and cannot be zero.")]
            public int PointsDelta { get; set; } // Can be positive or negative

            [Required]
            [MinLength(10, ErrorMessage = "A reason is required (min 10 characters).")]
            public string Reason { get; set; }
        }

        // PUT: api/v1/frequentflyer/admin/points/{flyerId:int}
        // Admin: Manually adds or subtracts points from an account.
        [HttpPut("admin/points/{flyerId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ManualAdjustPoints(int flyerId, [FromBody] UpdatePointsRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var serviceDto = new UpdatePointsDto { PointsDelta = dto.PointsDelta, Reason = dto.Reason };
 
                var result = await _frequentFlyerService.ManualAdjustPointsAsync(flyerId, serviceDto, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First(), null));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                 
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Points adjusted successfully.",
                    result.Data // This is now the full FrequentFlyerDto
                )); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception adjusting points for FlyerId {FlyerId}.", flyerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DTO for the UpdateLevel endpoint body
        public class UpdateLevelRequestDto
        {
            [Required]
            [StringLength(20, MinimumLength = 3, ErrorMessage = "NewLevel must be between 3 and 20 characters.")]
            public string NewLevel { get; set; }
        }

        // PUT: api/v1/frequentflyer/admin/level/{flyerId:int}
        // Admin: Updates the tier (Level) of a frequent flyer account.
        [HttpPut("admin/level/{flyerId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLevel(int flyerId, [FromBody] UpdateLevelRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                 
                var result = await _frequentFlyerService.UpdateLevelAsync(flyerId, dto.NewLevel, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First(), null));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }
                 
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Account level updated successfully.",
                    result.Data  
                )); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating level for FlyerId {FlyerId}.", flyerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/frequentflyer/admin/add-points/booking/{bookingId:int}
        // Admin: Manually triggers the awarding of points for a confirmed booking.
        [HttpPost("admin/add-points/booking/{bookingId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPointsForBooking(int bookingId)
        {
            try
            {
                // Call the service method
                var result = await _frequentFlyerService.AddPointsForBookingAsync(bookingId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // e.g., "Booking payment is not confirmed" or "User is not linked"
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Points awarded for booking successfully.", new { pointsAdded = result.Data }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception adding points for BookingId {BookingId}.", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/frequentflyer/admin/delete/{flyerId:int}
        // Admin: Soft-deletes a frequent flyer account and unlinks it from the user.
        [HttpDelete("admin/delete/{flyerId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Restricted to SuperAdmin only
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount(int flyerId)
        {
            try
            {
                // Call the service method
                var result = await _frequentFlyerService.DeleteAccountAsync(flyerId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Frequent flyer account soft-deleted and unlinked successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting FlyerId {FlyerId}.", flyerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}