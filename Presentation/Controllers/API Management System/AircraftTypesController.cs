using Application.DTOs.AircraftType;
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
    // This controller handles all CRUD operations for Aircraft Type specifications.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/aircraft-types")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only
    public class AircraftTypesController : ControllerBase
    {
        private readonly IAircraftTypeService _aircraftTypeService;
        private readonly ILogger<AircraftTypesController> _logger;

        public AircraftTypesController(IAircraftTypeService aircraftTypeService, ILogger<AircraftTypesController> logger)
        {
            _aircraftTypeService = aircraftTypeService;
            _logger = logger;
        }

        #region --- Read Endpoints ---

        // GET: api/v1/admin/aircraft-types
        // Retrieves a list of all active aircraft types.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllActiveAircraftTypes()
        {
            try
            {
                var result = await _aircraftTypeService.GetAllActiveAircraftTypesAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Active aircraft types retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving all active aircraft types.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/aircraft-types/search
        // Performs an advanced, paginated search for aircraft types.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAircraftTypes(
            [FromQuery] AircraftTypeFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var result = await _aircraftTypeService.SearchAircraftTypesAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft type search retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during aircraft type search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/aircraft-types/{typeId:int}
        // Retrieves a single aircraft type by its ID.
        [HttpGet("{typeId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAircraftTypeById(int typeId)
        {
            try
            {
                var result = await _aircraftTypeService.GetAircraftTypeByIdAsync(typeId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft type retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving aircraft type by ID {TypeId}.", typeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/aircraft-types/by-model
        // Finds aircraft types where the model name contains the search term.
        [HttpGet("by-model")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindAircraftTypesByModel([FromQuery][Required] string model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Search term 'model' must be provided in the query string." } });
            }

            try
            {
                var result = await _aircraftTypeService.FindAircraftTypesByModelAsync(model);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft types retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception finding aircraft types by model {Model}.", model);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/aircraft-types/by-manufacturer
        // Retrieves all active aircraft types from a specific manufacturer.
        [HttpGet("by-manufacturer")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAircraftTypesByManufacturer([FromQuery][Required] string manufacturer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Manufacturer name must be provided in the query string." } });
            }

            try
            {
                var result = await _aircraftTypeService.GetAircraftTypesByManufacturerAsync(manufacturer);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft types retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving aircraft types by manufacturer {Manufacturer}.", manufacturer);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/aircraft-types/suitable-for-route
        // Retrieves types that can fly a certain distance (in km).
        [HttpGet("suitable-for-route")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAircraftTypesSuitableForRoute([FromQuery][Range(1, 40000)] int distanceKm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftTypeService.GetAircraftTypesSuitableForRouteAsync(distanceKm);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, $"Aircraft types suitable for {distanceKm}km retrieved.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception finding suitable aircraft types for distance {Distance}.", distanceKm);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/aircraft-types/all-including-deleted
        // Retrieves all types, including soft-deleted ones (for admin auditing).
        [HttpGet("all-including-deleted")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAircraftTypesIncludingDeleted()
        {
            try
            {
                var result = await _aircraftTypeService.GetAllAircraftTypesIncludingDeletedAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "All aircraft types (including deleted) retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving all aircraft types (including deleted).");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Write Endpoints ---

        // POST: api/v1/admin/aircraft-types
        // Creates a new aircraft type.
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can create new core data
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAircraftType([FromBody] CreateAircraftTypeDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftTypeService.CreateAircraftTypeAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "already exists")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetAircraftTypeById), // Points to the new resource location
                    new { typeId = result.Data.TypeId },
                    new ApiResponse(StatusCodes.Status201Created, "Aircraft type created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating aircraft type {Model}.", createDto.Model);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/aircraft-types/{typeId:int}
        // Updates an existing aircraft type.
        [HttpPut("{typeId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Admins can update
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAircraftType(int typeId, [FromBody] UpdateAircraftTypeDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method (which now returns AircraftTypeDto in Data)
                var result = await _aircraftTypeService.UpdateAircraftTypeAsync(typeId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle validation errors (e.g., "already exists", "Concurrency error")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return the updated AircraftTypeDto in the response data
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft type updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating aircraft type {TypeId}.", typeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }
        // DELETE: api/v1/admin/aircraft-types/{typeId:int}
        // Soft-deletes an aircraft type (if no dependencies exist).
        [HttpDelete("{typeId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete core data
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAircraftType(int typeId)
        {
            try
            {
                var result = await _aircraftTypeService.DeleteAircraftTypeAsync(typeId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle dependency error
                    if (result.Errors.Any(e => e.Contains("dependencies")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft type soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting aircraft type {TypeId}.", typeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/aircraft-types/{typeId:int}/reactivate
        // Reactivates a soft-deleted aircraft type.
        [HttpPost("{typeId:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateAircraftType(int typeId)
        {
            try
            {
                var result = await _aircraftTypeService.ReactivateAircraftTypeAsync(typeId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle error (e.g., "already active")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft type reactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception reactivating aircraft type {TypeId}.", typeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}