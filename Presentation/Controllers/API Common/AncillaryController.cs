using Application.DTOs.AncillaryProduct;
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
    // This controller manages both public-facing ancillary actions (for bookings)
    // and admin-facing product management (CRUD for ancillary definitions).
    [ApiController]
    [Route("api/v1/[controller]")] // Standardized v1 API routing
    [Produces("application/json")]
    public class AncillaryController : ControllerBase
    {
        private readonly IAncillaryProductService _ancillaryService;

        public AncillaryController(IAncillaryProductService ancillaryService)
        {
            _ancillaryService = ancillaryService;
        }

        #region --- Public API (Customer Facing) ---

        // GET: api/v1/ancillary/available/{flightInstanceId}
        // Retrieves a list of all ancillary products available for purchase for a specific flight.
        [HttpGet("available/{flightInstanceId:int}")]
        [AllowAnonymous] // Anyone can see available products
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableProducts(int flightInstanceId)
        {
            try
            {
                var result = await _ancillaryService.GetAvailableProductsAsync(flightInstanceId);

                if (!result.IsSuccess)
                {
                    // Use ApiValidationErrorResponse for service-level validation errors
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Available ancillary products retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                // Use ApiExceptionResponse for unhandled exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/ancillary/booking/{bookingId}
        // Retrieves all ancillary items already purchased for a specific booking.
        [HttpGet("booking/{bookingId:int}")]
        [Authorize] // User must be authenticated
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAncillariesForBooking(int bookingId)
        {
            var result = await _ancillaryService.GetAncillariesForBookingAsync(bookingId, User);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => e.Contains("Access denied")))
                {
                    // FIX: Use StatusCode(403, object) instead of Forbid(object)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new ApiResponse(StatusCodes.Status403Forbidden, "Access denied to this booking's ancillary items."));
                }

                if (result.Errors.Any(e => e.Contains("Booking not found")))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Booking not found."));
                }

                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary items for booking retrieved successfully.", result.Data));
        }

        // POST: api/v1/ancillary/booking
        // Adds a new ancillary product (e.g., extra bag, meal) to an existing booking.
        [HttpPost("booking")]
        [Authorize] // User must be authenticated
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAncillaryToBooking([FromBody] CreateAncillarySaleDto saleDto)
        {
            if (!ModelState.IsValid)
            {
                // Use ApiValidationErrorResponse for model state errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _ancillaryService.AddAncillaryToBookingAsync(saleDto, User);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => e.Contains("Access denied")))
                {
                     
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new ApiResponse(StatusCodes.Status403Forbidden, "Access denied to modify this booking."));
                }

                if (result.Errors.Any(e => e.Contains("not found")))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Booking or Ancillary Product not found."));
                }

                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            // Return 201 Created with the new resource and a success message
            var response = new ApiResponse(StatusCodes.Status201Created, "Ancillary item added to booking successfully.", result.Data);

            return CreatedAtAction(
                nameof(GetAncillariesForBooking),
                new { bookingId = result.Data.BookingId },
                response
            );
        }

        // DELETE: api/v1/ancillary/booking/sale/{saleId}
        // Removes a previously purchased ancillary item from a booking.
        [HttpDelete("booking/sale/{saleId:int}")]
        [Authorize] // User must be authenticated
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveAncillaryFromBooking(int saleId)
        {
            var result = await _ancillaryService.RemoveAncillaryFromBookingAsync(saleId, User);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => e.Contains("Access denied")))
                {
                    // FIX: Use StatusCode(403, object)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new ApiResponse(StatusCodes.Status403Forbidden, "Access denied to remove this item."));
                }

                if (result.Errors.Any(e => e.Contains("not found")))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Ancillary sale item not found."));
                }

                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            // Return 200 OK with success message
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary item removed from booking successfully."));
        }

        #endregion

        #region --- Admin API (Management System) ---
        // All endpoints here will be prefixed: api/v1/ancillary/admin/

        // GET: api/v1/ancillary/admin/products
        // Retrieves a paginated list of all ancillary product definitions.
        [HttpGet("admin/products")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsPaginated(
            [FromQuery] AncillaryProductFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size limit

            var result = await _ancillaryService.GetProductsPaginatedAsync(filter, pageNumber, pageSize);

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

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary products retrieved successfully.", paginatedData));
        }

        // GET: api/v1/ancillary/admin/product/{productId}
        // Retrieves a single ancillary product definition by its ID.
        [HttpGet("admin/product/{productId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProductById(int productId)
        {
            var result = await _ancillaryService.GetProductByIdAsync(productId);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary product retrieved successfully.", result.Data));
        }

        // POST: api/v1/ancillary/admin/product
        // Creates a new ancillary product definition (e.g., "Extra Baggage 15kg").
        [HttpPost("admin/product")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)] // For duplicates
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateAncillaryProductDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _ancillaryService.CreateProductAsync(createDto, User);

            if (!result.IsSuccess)
            {
                // Check for duplicate name error
                if (result.Errors.Any(e => e.Contains("already exists")))
                {
                    return Conflict(new ApiResponse(StatusCodes.Status409Conflict, result.Errors.First()));
                }

                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            var response = new ApiResponse(StatusCodes.Status201Created, "Ancillary product created successfully.", result.Data);

            return CreatedAtAction(
                nameof(GetProductById),
                new { productId = result.Data.ProductId },
                response
            );
        }

        // PUT: api/v1/ancillary/admin/product/{productId}
        // Updates an existing ancillary product definition.
        [HttpPut("admin/product/{productId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)] // For duplicates
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] UpdateAncillaryProductDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _ancillaryService.UpdateProductAsync(productId, updateDto, User);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => e.Contains("not found")))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
                }

                if (result.Errors.Any(e => e.Contains("already exists")))
                {
                    return Conflict(new ApiResponse(StatusCodes.Status409Conflict, result.Errors.First()));
                }

                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary product updated successfully.", result.Data));
        }

        // DELETE: api/v1/ancillary/admin/product/{productId}
        // Soft-deletes an ancillary product definition.
        [HttpDelete("admin/product/{productId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)] // For dependency error
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var result = await _ancillaryService.DeleteProductAsync(productId, User);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => e.Contains("not found")))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
                }

                // Handle dependency error from service
                if (result.Errors.Any(e => e.Contains("associated sales records")))
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Ancillary product soft-deleted successfully."));
        }

        #endregion
    }
}