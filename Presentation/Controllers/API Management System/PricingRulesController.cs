using Application.DTOs.ContextualPricingAttribute;
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
    // This controller manages the contextual attributes that drive dynamic pricing.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/pricing-rules")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only
    public class PricingRulesController : ControllerBase
    {
        private readonly IContextualPricingService _pricingService;
        private readonly ILogger<PricingRulesController> _logger;

        public PricingRulesController(IContextualPricingService pricingService, ILogger<PricingRulesController> logger)
        {
            _pricingService = pricingService;
            _logger = logger;
        }

        #region --- Read Endpoints ---

        // GET: api/v1/admin/pricing-rules
        // Retrieves a list of all active pricing attribute sets.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllActiveAttributeSets()
        {
            try
            {
                // Call the service method
                var result = await _pricingService.GetAllActiveAttributeSetsAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Active pricing attribute sets retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving all active pricing attribute sets.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/pricing-rules/search
        // Performs an advanced, paginated search for pricing attribute sets.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAttributeSets(
            [FromQuery] PricingAttributeFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size limit

            try
            {
                // Call the service method
                var result = await _pricingService.SearchAttributeSetsAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Pricing attribute search retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during pricing attribute search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/pricing-rules/{attributeId:int}
        // Retrieves a single pricing attribute set by its ID.
        [HttpGet("{attributeId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttributeSetById(int attributeId)
        {
            try
            {
                // Call the service method
                var result = await _pricingService.GetAttributeSetByIdAsync(attributeId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Pricing attribute set retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving pricing attribute set ID {AttributeId}.", attributeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/pricing-rules/find-matches
        // Finds the best matching pricing rules for a given booking context.
        [HttpPost("find-matches")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Not for Supervisors
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindBestMatchingAttributeSets([FromBody] PricingContextDto contextDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _pricingService.FindBestMatchingAttributeSetsAsync(contextDto);

                if (!result.IsSuccess)
                {
                    // This service fails if no matches are found
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Matching attribute sets retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception finding matching attribute sets.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Write Endpoints ---

        // POST: api/v1/admin/pricing-rules
        // Creates a new pricing attribute set.
        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")] // Write access
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAttributeSet([FromBody] CreatePricingAttributeDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _pricingService.CreateAttributeSetAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "already exists")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetAttributeSetById),
                    new { attributeId = result.Data.AttributeId },
                    new ApiResponse(StatusCodes.Status201Created, "Pricing attribute set created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating pricing attribute set.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/pricing-rules/{attributeId:int}
        // Updates an existing pricing attribute set.
        [HttpPut("{attributeId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Write access
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAttributeSet(int attributeId, [FromBody] UpdatePricingAttributeDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method, which now returns ServiceResult<ContextualPricingAttributeDto>
                var result = await _pricingService.UpdateAttributeSetAsync(attributeId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        // Ensure data is null for 404
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));

                    // Handle validation errors (e.g., "already exists")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Success: Return the updated DTO in the 'data' field of the ApiResponse
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Pricing attribute set updated successfully.",
                    result.Data // <--- Returning the updated data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating pricing attribute set {AttributeId}.", attributeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/pricing-rules/{attributeId:int}
        // Soft-deletes a pricing attribute set (if no dependencies exist).
        [HttpDelete("{attributeId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Destructive action
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttributeSet(int attributeId)
        {
            try
            {
                // Call the service method
                var result = await _pricingService.DeleteAttributeSetAsync(attributeId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle dependency error
                    if (result.Errors.Any(e => e.Contains("dependencies")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Pricing attribute set soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting pricing attribute set {AttributeId}.", attributeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/pricing-rules/{attributeId:int}/reactivate
        // Reactivates a soft-deleted pricing attribute set.
        [HttpPost("{attributeId:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateAttributeSet(int attributeId)
        {
            try
            {
                // Call the service method
                var result = await _pricingService.ReactivateAttributeSetAsync(attributeId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle error (e.g., "already active")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Pricing attribute set reactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception reactivating pricing attribute set {AttributeId}.", attributeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}