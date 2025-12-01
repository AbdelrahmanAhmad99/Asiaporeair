using Application.DTOs.Airline;
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
    // This controller handles all CRUD operations for Airline data.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/airlines")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only
    public class AirlinesController : ControllerBase
    {
        private readonly IAirlineService _airlineService;
        private readonly ILogger<AirlinesController> _logger;

        public AirlinesController(IAirlineService airlineService, ILogger<AirlinesController> logger)
        {
            _airlineService = airlineService;
            _logger = logger;
        }

        #region --- Read Endpoints ---

        // GET: api/v1/admin/airlines
        // Retrieves a list of all active airlines.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllActiveAirlines()
        {
            try
            {
                var result = await _airlineService.GetAllActiveAirlinesAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Active airlines retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving all active airlines.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/paginated
        // Retrieves a paginated list of active airlines, optionally filtered by region.
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaginatedAirlines(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? region = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var result = await _airlineService.GetPaginatedAirlinesAsync(pageNumber, pageSize, region);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Paginated airlines retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving paginated airlines.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/{iataCode}
        // Retrieves a single airline by its 2-letter IATA code.
        [HttpGet("{iataCode}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAirlineByIataCode(string iataCode)
        {
            if (string.IsNullOrWhiteSpace(iataCode) || iataCode.Length != 2)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "IATA code must be 2 characters." } });
            }

            try
            {
                var result = await _airlineService.GetAirlineByIataCodeAsync(iataCode);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airline retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving airline by IATA code {IataCode}.", iataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/{iataCode}/fleet
        // Retrieves a single airline and its associated fleet of aircraft.
        [HttpGet("{iataCode}/fleet")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAirlineWithFleet(string iataCode)
        {
            if (string.IsNullOrWhiteSpace(iataCode) || iataCode.Length != 2)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "IATA code must be 2 characters." } });
            }

            try
            {
                var result = await _airlineService.GetAirlineWithFleetAsync(iataCode);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airline and fleet retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving airline with fleet for {IataCode}.", iataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/by-name
        // Finds airlines where the name contains the search term.
        [HttpGet("by-name")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindAirlinesByName([FromQuery][Required] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Search term 'name' must be provided in the query string." } });
            }

            try
            {
                var result = await _airlineService.FindAirlinesByNameAsync(name);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airlines retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception finding airlines by name {Name}.", name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/by-base-airport/{airportIataCode}
        // Retrieves all active airlines based in a specific airport.
        [HttpGet("by-base-airport/{airportIataCode}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAirlinesByBaseAirport(string airportIataCode)
        {
            if (string.IsNullOrWhiteSpace(airportIataCode) || airportIataCode.Length != 3)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Airport IATA code must be 3 characters." } });
            }

            try
            {
                var result = await _airlineService.GetAirlinesByBaseAirportAsync(airportIataCode);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airlines retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving airlines by base airport {AirportIataCode}.", airportIataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/by-region
        // Retrieves all active airlines operating in a specific region.
        [HttpGet("by-region")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAirlinesByOperatingRegion([FromQuery][Required] string region)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Region must be provided in the query string." } });
            }

            try
            {
                var result = await _airlineService.GetAirlinesByOperatingRegionAsync(region);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airlines retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving airlines by region {Region}.", region);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/airlines/all-including-deleted
        // Retrieves all airlines, including soft-deleted ones (for admin auditing).
        [HttpGet("all-including-deleted")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAirlinesIncludingDeleted()
        {
            try
            {
                var result = await _airlineService.GetAllAirlinesIncludingDeletedAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "All airlines (including deleted) retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving all airlines (including deleted).");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Write Endpoints ---

        // POST: api/v1/admin/airlines
        // Creates a new airline.
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can create new core data
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAirline([FromBody] CreateAirlineDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _airlineService.CreateAirlineAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "already exists", "Base airport not found")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetAirlineByIataCode), // Points to the new resource location
                    new { iataCode = result.Data.IataCode },
                    new ApiResponse(StatusCodes.Status201Created, "Airline created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating airline {Name}.", createDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/airlines/{iataCode}
        // Updates an existing airline.
        [HttpPut("{iataCode}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Admins can update
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAirline(string iataCode, [FromBody] UpdateAirlineDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method (which now returns AirlineDto in Data)
                var result = await _airlineService.UpdateAirlineAsync(iataCode, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle validation errors (e.g., "name already exists", "Base airport not found")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return the updated AirlineDto in the response data
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airline updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating airline {IataCode}.", iataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/airlines/{iataCode}
        // Soft-deletes an airline (if no dependencies exist).
        [HttpDelete("{iataCode}")]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete core data
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAirline(string iataCode)
        {
            try
            {
                var result = await _airlineService.DeleteAirlineAsync(iataCode);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle dependency error
                    if (result.Errors.Any(e => e.Contains("dependencies first")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airline soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting airline {IataCode}.", iataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/airlines/{iataCode}/reactivate
        // Reactivates a soft-deleted airline.
        [HttpPost("{iataCode}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateAirline(string iataCode)
        {
            try
            {
                var result = await _airlineService.ReactivateAirlineAsync(iataCode);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle error (e.g., "already active")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Airline reactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception reactivating airline {IataCode}.", iataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}