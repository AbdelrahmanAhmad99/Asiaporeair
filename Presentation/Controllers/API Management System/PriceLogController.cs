using Application.DTOs.PriceOfferLog;
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
    // This controller manages viewing and analyzing Price Offer Logs.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/pricelogs")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only
    public class PriceLogController : ControllerBase
    {
        private readonly IPriceOfferLogService _priceLogService;
        private readonly ILogger<PriceLogController> _logger;

        public PriceLogController(IPriceOfferLogService priceLogService, ILogger<PriceLogController> logger)
        {
            _priceLogService = priceLogService;
            _logger = logger;
        }

        #region --- Read & Analytics Endpoints ---

        // GET: api/v1/admin/pricelogs/search
        // Retrieves a paginated list of price offer logs based on filters.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchLogs(
            [FromQuery] PriceOfferLogFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            try
            {
                // Call the service method
                var result = await _priceLogService.SearchLogsAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    // Search failures are typically internal
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Price logs retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during price log search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/pricelogs/{offerId:int}
        // Retrieves a single price offer log by its ID.
        [HttpGet("{offerId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLogById(int offerId)
        {
            try
            {
                // Call the service method
                var result = await _priceLogService.GetLogByIdAsync(offerId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Price log retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving price log ID {OfferId}.", offerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/pricelogs/analytics/fare
        // Retrieves pricing analytics for a specific fare code over a date range.
        [HttpGet("analytics/fare")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAnalyticsForFareAsync(
            [FromQuery][Required] string fareCode,
            [FromQuery][Required] DateTime startDate,
            [FromQuery][Required] DateTime endDate)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _priceLogService.GetAnalyticsForFareAsync(fareCode, startDate, endDate);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Fare analytics retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving analytics for fare {FareCode}.", fareCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/pricelogs/analytics/ancillary
        // Retrieves pricing analytics for a specific ancillary product over a date range.
        [HttpGet("analytics/ancillary")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAnalyticsForAncillaryAsync(
            [FromQuery][Required] int ancillaryProductId,
            [FromQuery][Required] DateTime startDate,
            [FromQuery][Required] DateTime endDate)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _priceLogService.GetAnalyticsForAncillaryAsync(ancillaryProductId, startDate, endDate);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary product analytics retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving analytics for ancillary product {ProductId}.", ancillaryProductId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Write Endpoints ---

        // POST: api/v1/admin/pricelogs
        // Manually logs a new price offer. (Typically logged automatically by PricingService).
        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")] // Write access
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LogPriceOffer([FromBody] CreatePriceOfferLogDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _priceLogService.LogPriceOfferAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "Fare code does not exist")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetLogById),
                    new { offerId = result.Data.OfferId },
                    new ApiResponse(StatusCodes.Status201Created, "Price offer logged successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception logging price offer.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/pricelogs/{offerId:int}
        // Soft-deletes a price offer log entry.
        [HttpDelete("{offerId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Destructive action
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLog(int offerId)
        {
            try
            {
                // Call the service method
                var result = await _priceLogService.DeleteLogAsync(offerId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Price log soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting price log {OfferId}.", offerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/pricelogs/{offerId:int}/reactivate
        // Reactivates a soft-deleted price offer log.
        [HttpPost("{offerId:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateLog(int offerId)
        {
            try
            {
                // Call the service method
                var result = await _priceLogService.ReactivateLogAsync(offerId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle error (e.g., "already active")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Price log reactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception reactivating price log {OfferId}.", offerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}