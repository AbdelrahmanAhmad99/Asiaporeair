using Application.DTOs.Aircraft;
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
    // This controller handles all fleet management: Aircraft, Configurations, and Cabin Classes.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/fleet")] // Use 'fleet' as the base route
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors get read-only
    public class AircraftController : ControllerBase
    {
        private readonly IAircraftManagementService _aircraftService;
        private readonly ILogger<AircraftController> _logger;

        public AircraftController(IAircraftManagementService aircraftService, ILogger<AircraftController> logger)
        {
            _aircraftService = aircraftService;
            _logger = logger;
        }

        #region --- Aircraft Endpoints ---

        // GET: api/v1/admin/fleet/aircraft/search
        // Performs an advanced, paginated search for aircraft.
        [HttpGet("aircraft/search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAircraft(
            [FromQuery] AircraftFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var result = await _aircraftService.GetAircraftPaginatedAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft search retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during aircraft search.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/fleet/aircraft/{tailNumber}
        // Retrieves a single aircraft by its tail number.
        [HttpGet("aircraft/{tailNumber}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAircraftByTailNumber(string tailNumber)
        {
            if (string.IsNullOrWhiteSpace(tailNumber))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Tail number cannot be empty." } });
            }

            try
            {
                var result = await _aircraftService.GetAircraftByTailNumberAsync(tailNumber);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/fleet/aircraft/{tailNumber}/details
        // Retrieves full details for an aircraft, including configurations and cabin classes.
        [HttpGet("aircraft/{tailNumber}/details")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAircraftDetails(string tailNumber)
        {
            if (string.IsNullOrWhiteSpace(tailNumber))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Tail number cannot be empty." } });
            }

            try
            {
                var result = await _aircraftService.GetAircraftDetailsAsync(tailNumber);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft details retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving details for aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/fleet/aircraft
        // Creates a new aircraft record.
        [HttpPost("aircraft")]
        [Authorize(Roles = "SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAircraft([FromBody] CreateAircraftDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftService.CreateAircraftAsync(createDto);

                if (!result.IsSuccess)
                {
                    // Handle validation errors (e.g., "already exists", "Airline not found")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created
                return CreatedAtAction(
                    nameof(GetAircraftByTailNumber),
                    new { tailNumber = result.Data.TailNumber },
                    new ApiResponse(StatusCodes.Status201Created, "Aircraft created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating aircraft {TailNumber}.", createDto.TailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/fleet/aircraft/{tailNumber}
        // Updates an existing aircraft's core data.
        [HttpPut("aircraft/{tailNumber}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAircraft(string tailNumber, [FromBody] UpdateAircraftDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method (which now returns AircraftDto in Data)
                var result = await _aircraftService.UpdateAircraftAsync(tailNumber, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return the updated AircraftDto in the response data
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/fleet/aircraft/{tailNumber}/status
        // Updates only the operational status of an aircraft (e.g., "Operational", "Maintenance").
        [HttpPut("aircraft/{tailNumber}/status")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Supervisors can ground/activate aircraft
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAircraftStatus(string tailNumber, [FromBody] UpdateAircraftStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftService.UpdateAircraftStatusAsync(tailNumber, dto);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft status updated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating status for aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/fleet/aircraft/{tailNumber}/add-hours
        // Adds flight hours to an aircraft's log after a flight.
        [HttpPost("aircraft/{tailNumber}/add-hours")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Operational update
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddFlightHours(string tailNumber, [FromBody] AddFlightHoursDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftService.AddFlightHoursAsync(tailNumber, dto);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight hours added successfully.", new { newTotalHours = result.Data }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception adding flight hours for aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/fleet/aircraft/{tailNumber}
        // Soft-deletes an aircraft (if no dependencies exist).
        [HttpDelete("aircraft/{tailNumber}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAircraft(string tailNumber)
        {
            try
            {
                var result = await _aircraftService.DeleteAircraftAsync(tailNumber);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle dependency error
                    if (result.Errors.Any(e => e.Contains("dependencies")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/fleet/aircraft/{tailNumber}/reactivate
        // Reactivates a soft-deleted aircraft.
        [HttpPost("aircraft/{tailNumber}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateAircraft(string tailNumber)
        {
            try
            {
                var result = await _aircraftService.ReactivateAircraftAsync(tailNumber);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft reactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception reactivating aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion


        #region --- Aircraft Configuration Endpoints ---

        // GET: api/v1/admin/fleet/aircraft/{tailNumber}/configurations
        // Retrieves all active configurations for a specific aircraft.
        [HttpGet("aircraft/{tailNumber}/configurations")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfigsForAircraft(string tailNumber)
        {
            try
            {
                var result = await _aircraftService.GetConfigsForAircraftAsync(tailNumber);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Aircraft configurations retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving configurations for aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/fleet/configurations/{configId:int}
        // Retrieves the details of a single configuration, including its cabin classes.
        [HttpGet("configurations/{configId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfigDetails(int configId)
        {
            try
            {
                var result = await _aircraftService.GetConfigDetailsAsync(configId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Configuration details retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving config details for ID {ConfigId}.", configId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/fleet/aircraft/{tailNumber}/configurations
        // Creates a new configuration for an aircraft.
        [HttpPost("aircraft/{tailNumber}/configurations")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAircraftConfig(string tailNumber, [FromBody] CreateAircraftConfigDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftService.CreateAircraftConfigAsync(tailNumber, createDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return CreatedAtAction(
                    nameof(GetConfigDetails),
                    new { configId = result.Data.ConfigId },
                    new ApiResponse(StatusCodes.Status201Created, "Aircraft configuration created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating config for aircraft {TailNumber}.", tailNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/fleet/configurations/{configId:int}
        // Updates an existing aircraft configuration's name.
        [HttpPut("configurations/{configId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAircraftConfig(int configId, [FromBody] UpdateAircraftConfigDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method (which now returns AircraftConfigDto in Data)
                var result = await _aircraftService.UpdateAircraftConfigAsync(configId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return the updated DTO in the response data
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Configuration updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating config ID {ConfigId}.", configId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/fleet/configurations/{configId:int}
        // Soft-deletes an aircraft configuration (if no dependencies exist).
        [HttpDelete("configurations/{configId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAircraftConfig(int configId)
        {
            try
            {
                var result = await _aircraftService.DeleteAircraftConfigAsync(configId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("dependencies")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Configuration soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting config ID {ConfigId}.", configId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion


        #region --- Cabin Class Endpoints ---

        // GET: api/v1/admin/fleet/configurations/{configId:int}/cabin-classes
        // Retrieves all active cabin classes for a specific configuration.
        [HttpGet("configurations/{configId:int}/cabin-classes")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCabinClassesForConfig(int configId)
        {
            try
            {
                var result = await _aircraftService.GetCabinClassesForConfigAsync(configId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Cabin classes retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving cabin classes for config {ConfigId}.", configId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/fleet/configurations/cabin-classes
        // Creates a new cabin class within a configuration.
        [HttpPost("configurations/cabin-classes")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCabinClass([FromBody] CreateCabinClassDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftService.CreateCabinClassAsync(createDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // We don't have a "GetCabinClassById" endpoint, so just return 201 with data
                return StatusCode(StatusCodes.Status201Created,
                    new ApiResponse(StatusCodes.Status201Created, "Cabin class created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating cabin class {Name}.", createDto.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/fleet/configurations/cabin-classes/{cabinClassId:int}
        // Soft-deletes a cabin class (if no dependencies exist).
        [HttpDelete("configurations/cabin-classes/{cabinClassId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCabinClass(int cabinClassId)
        {
            try
            {
                var result = await _aircraftService.DeleteCabinClassAsync(cabinClassId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("dependencies")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Cabin class soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting cabin class ID {CabinClassId}.", cabinClassId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/fleet/configurations/cabin-classes/{cabinClassId:int}/seat-map
        // Retrieves all seats for a specific cabin class.
        [HttpGet("configurations/cabin-classes/{cabinClassId:int}/seat-map")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSeatMapForCabinClass(int cabinClassId)
        {
            try
            {
                var result = await _aircraftService.GetSeatMapForCabinClassAsync(cabinClassId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Seat map retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving seat map for cabin {CabinClassId}.", cabinClassId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion


        #region --- Seat Endpoints ---

        // POST: api/v1/admin/fleet/seats
        // Creates a new seat and assigns it to a cabin class.
        [HttpPost("seats")]
        [Authorize(Roles = "SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateSeat([FromBody] CreateSeatDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _aircraftService.CreateSeatAsync(createDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // We don't have a "GetSeatById" endpoint, so just return 201 with data
                return StatusCode(StatusCodes.Status201Created,
                    new ApiResponse(StatusCodes.Status201Created, "Seat created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating seat {SeatNumber}.", createDto.SeatNumber);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }



        // PUT: api/v1/admin/fleet/seats/{seatId}
        // Updates an existing seat's properties or re-assigns its cabin class.
        [HttpPut("seats/{seatId}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSeat(string seatId, [FromBody] UpdateSeatDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(seatId))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Seat ID cannot be empty." } });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method (which now returns SeatDto in Data)
                var result = await _aircraftService.UpdateSeatAsync(seatId, updateDto);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return the updated SeatDto in the response data
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Seat updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating seat {SeatId}.", seatId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }





        // DELETE: api/v1/admin/fleet/seats/{seatId}
        // Soft-deletes a seat (if no dependencies exist).
        [HttpDelete("seats/{seatId}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSeat(string seatId)
        {
            if (string.IsNullOrWhiteSpace(seatId))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Seat ID cannot be empty." } });
            }

            try
            {
                var result = await _aircraftService.DeleteSeatAsync(seatId);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("dependencies")))
                        return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Seat soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting seat {SeatId}.", seatId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

    }
}