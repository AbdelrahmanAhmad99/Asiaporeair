using Application.DTOs.Crew;
using Application.DTOs.CrewScheduling;
using Application.Models;
using Application.Services.Interfaces;
using Domain.Enums;
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
    // This controller manages Crew Profiles, Certifications, and Scheduling (Rostering).
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/crew")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Base authorization
    public class CrewController : ControllerBase
    {
        private readonly ICrewManagementService _crewMgmtService;
        private readonly ICrewSchedulingService _crewSchedulingService;
        private readonly ILogger<CrewController> _logger;

        public CrewController(
            ICrewManagementService crewMgmtService,
            ICrewSchedulingService crewSchedulingService,
            ILogger<CrewController> logger)
        {
            _crewMgmtService = crewMgmtService;
            _crewSchedulingService = crewSchedulingService;
            _logger = logger;
        }

        #region --- Crew Management Endpoints ---

        // GET: api/v1/admin/crew/search
        // Retrieves a paginated and filtered list of all crew members.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCrewMembersPaginated(
            [FromQuery] CrewFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var result = await _crewMgmtService.GetCrewMembersPaginatedAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Crew members retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving paginated crew.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/crew/{employeeId:int}/details
        // Retrieves detailed information for a specific crew member.
        [HttpGet("{employeeId:int}/details")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCrewMemberDetailsById(int employeeId)
        {
            try
            {
                var result = await _crewMgmtService.GetCrewMemberDetailsByIdAsync(employeeId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Crew member details retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving crew member details for ID {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/crew/{employeeId:int}/base
        // Updates the base airport for a crew member.
        [HttpPut("{employeeId:int}/base")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Stricter auth
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCrewBase(int employeeId, [FromBody] UpdateCrewBaseRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Service now returns ServiceResult<CrewMemberSummaryDto>
                var result = await _crewMgmtService.UpdateCrewBaseAsync(employeeId, dto.NewBaseAirportIata, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        // Ensure data is null for 404
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        // Ensure data is null for 403
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First(), null));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Success: Return the updated DTO in the 'data' field of the ApiResponse
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Crew base updated successfully.",
                    result.Data // <--- Returning the updated data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating crew base for Employee ID {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Crew Certification Endpoints ---

        // GET: api/v1/admin/crew/{employeeId:int}/certifications
        // Retrieves all active certifications for a specific crew member.
        [HttpGet("{employeeId:int}/certifications")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCertificationsForCrewMember(int employeeId)
        {
            try
            {
                var result = await _crewMgmtService.GetCertificationsForCrewMemberAsync(employeeId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Certifications retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving certifications for Employee ID {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/crew/{employeeId:int}/certifications
        // Adds a new certification record for a crew member.
        [HttpPost("{employeeId:int}/certifications")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Stricter auth
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCertification(int employeeId, [FromBody] CreateCertificationDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                var result = await _crewMgmtService.AddCertificationAsync(employeeId, createDto, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return 201 Created (no GetById endpoint for certs, so just return data)
                return StatusCode(StatusCodes.Status201Created,
                    new ApiResponse(StatusCodes.Status201Created, "Certification created successfully.", result.Data)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception adding certification for Employee ID {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // PUT: api/v1/admin/crew/certifications/{certId:int}
        // Updates an existing certification record.
        [HttpPut("certifications/{certId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Stricter auth
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCertification(int certId, [FromBody] CreateCertificationDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Service returns ServiceResult<CertificationDto>
                var result = await _crewMgmtService.UpdateCertificationAsync(certId, updateDto, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First(), null));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Success: The service method already returned the updated CertificationDto, which is passed here.
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Certification updated successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating certification ID {CertId}.", certId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/crew/certifications/{certId:int}
        // Soft-deletes a certification record.
        [HttpDelete("certifications/{certId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Stricter auth
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCertification(int certId)
        {
            try
            {
                var result = await _crewMgmtService.DeleteCertificationAsync(certId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Certification soft-deleted successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting certification ID {CertId}.", certId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Crew Scheduling & Rostering Endpoints ---

        // POST: api/v1/admin/crew/assignments
        // Assigns one or more crew members to a flight instance.
        [HttpPost("assignments")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Schedulers
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignCrewToFlight([FromBody] AssignCrewRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _crewSchedulingService.AssignCrewToFlightAsync(request, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    // Handle validation errors (e.g., "conflicting assignment", "not type-rated")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Crew assigned successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception assigning crew to Flight ID {FlightId}.", request.FlightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/crew/assignments/{flightInstanceId:int}/{crewMemberId:int}
        // Removes a specific crew member from a flight instance roster.
        [HttpDelete("assignments/{flightInstanceId:int}/{crewMemberId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Schedulers
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveCrewFromFlight(int flightInstanceId, int crewMemberId)
        {
            try
            {
                // Call the service method
                var result = await _crewSchedulingService.RemoveCrewFromFlightAsync(flightInstanceId, crewMemberId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Crew assignment removed successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception removing crew {CrewId} from flight {FlightId}.", crewMemberId, flightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/crew/roster/{flightInstanceId:int}
        // Retrieves the full crew roster (all assigned members) for a specific flight.
        [HttpGet("roster/{flightInstanceId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightRoster(int flightInstanceId)
        {
            try
            {
                // Call the service method
                var result = await _crewSchedulingService.GetFlightRosterAsync(flightInstanceId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight roster retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving roster for Flight ID {FlightId}.", flightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/crew/schedule
        // Retrieves the flight schedule for a specific crew member in a date range.
        [HttpGet("schedule")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCrewMemberSchedule([FromQuery] CrewScheduleRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _crewSchedulingService.GetCrewMemberScheduleAsync(request);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Crew member schedule retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving schedule for crew {CrewId}.", request.CrewMemberEmployeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/crew/find-available
        // Finds potentially available and qualified crew members for a specific flight.
        [HttpPost("find-available")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindAvailableCrew([FromBody] CrewAvailabilityRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _crewSchedulingService.FindAvailableCrewAsync(request);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Available crew retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception finding available crew.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Analytics & Reporting Endpoints ---

        // GET: api/v1/admin/crew/analytics
        // Gets analytics data for the HR/Crew dashboard.
        [HttpGet("analytics")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCrewDashboardAnalytics()
        {
            try
            {
                // Call the service method
                var result = await _crewMgmtService.GetCrewDashboardAnalyticsAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Crew analytics retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving crew analytics.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/crew/reports/expiring-certifications
        // Retrieves crew members whose certifications are expiring soon or have expired.
        [HttpGet("reports/expiring-certifications")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCrewWithExpiringCertifications([FromQuery] int daysUntilExpiry = 30)
        {
            try
            {
                // Call the service method
                var result = await _crewMgmtService.GetCrewWithExpiringCertificationsAsync(daysUntilExpiry);

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Expiring certification report retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving expiring certifications report.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }


        // DTO defined internally for the UpdateCrewBase endpoint
        public class UpdateCrewBaseRequestDto
        {
            [Required]
            [StringLength(3, MinimumLength = 3, ErrorMessage = "Airport IATA code must be 3 characters.")]
            public string NewBaseAirportIata { get; set; }
        }


        #endregion
    }
}