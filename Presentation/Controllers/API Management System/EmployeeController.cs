using Application.DTOs.Employee;
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
    // This controller manages all HR and Employee data.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/employees")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Base authorization
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeManagementService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeManagementService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        #region --- Read Endpoints ---

        // GET: api/v1/admin/employees/search
        // Retrieves a paginated and filtered list of all employees.
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesPaginated([FromQuery] EmployeeFilterDto filter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Note: filter.Role (which is UserType?) will automatically bind from string names (e.g., "?Role=Pilot")
                var result = await _employeeService.GetEmployeesPaginatedAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiExceptionResponse(StatusCodes.Status500InternalServerError, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employees retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving employees paginated.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/employees/analytics
        // Gets analytics data for the HR dashboard.
        [HttpGet("analytics")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeeDashboardAnalytics()
        {
            try
            {
                // Call the service method
                var result = await _employeeService.GetEmployeeDashboardAnalyticsAsync();

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employee analytics retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving employee analytics.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/employees/{employeeId:int}
        // Retrieves a single employee's summary by their internal Employee ID.
        [HttpGet("{employeeId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeSummaryById(int employeeId)
        {
            try
            {
                // Call the service method
                var result = await _employeeService.GetEmployeeSummaryByIdAsync(employeeId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employee summary retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving employee ID {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/employees/by-appuser/{appUserId}
        // Retrieves a single employee's summary by their AppUser (Identity) ID.
        [HttpGet("by-appuser/{appUserId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeSummaryByAppUserId(string appUserId)
        {
            if (string.IsNullOrWhiteSpace(appUserId))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "AppUser ID cannot be empty." } });
            }

            try
            {
                // Call the service method
                var result = await _employeeService.GetEmployeeSummaryByAppUserIdAsync(appUserId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employee summary retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving employee by AppUser ID {AppUserId}.", appUserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/employees/by-role
        // Retrieves all employees belonging to a specific role.
        [HttpGet("by-role")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEmployeesByRole([FromQuery] UserType role)
        {
             
            try
            {
                var result = await _employeeService.GetEmployeesByRoleAsync(role);

                if (!result.IsSuccess)
                {
                    // Handle failure cases like invalid/non-employee role
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employees retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving employees by role {Role}.", role);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/employees/hiring-report
        // Retrieves employees hired within a specific date range.
        [HttpGet("hiring-report")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEmployeesHiredByDateRange(
            [FromQuery][Required] DateTime startDate,
            [FromQuery][Required] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
            }

            try
            {
                // Call the service method
                var result = await _employeeService.GetEmployeesHiredByDateRangeAsync(startDate, endDate);

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Hiring report retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving hiring report.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion

        #region --- Write Endpoints (SuperAdmin Only) ---

        // PUT: api/v1/admin/employees/{employeeId:int}/salary
        // Updates an employee's salary (SuperAdmin only).
        [HttpPut("{employeeId:int}/salary")]
        [Authorize(Roles = "SuperAdmin")] // Override class auth
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEmployeeSalary(int employeeId, [FromBody] UpdateSalaryRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method, passing the ClaimsPrincipal for auth check
                // The service method now returns ServiceResult<EmployeeSummaryDto>
                var result = await _employeeService.UpdateEmployeeSalaryAsync(employeeId, dto, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        // Ensure data is null for 404
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First(), null));

                    if (result.Errors.Any(e => e.Contains("Access denied")))
                        // Ensure data is null for 403
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First(), null));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Success: Return the updated DTO in the 'data' field of the ApiResponse
                return Ok(new ApiResponse(
                    StatusCodes.Status200OK,
                    "Employee salary updated successfully.",
                    result.Data // <--- Returning the updated data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating salary for employee {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // DELETE: api/v1/admin/employees/{employeeId:int}
        // Soft-deletes (deactivates) an employee account.
        [HttpDelete("{employeeId:int}")]
        [Authorize(Roles = "SuperAdmin")] // Override class auth
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateEmployee(int employeeId)
        {
            try
            {
                // Call the service method, passing the ClaimsPrincipal for auth check
                var result = await _employeeService.DeactivateEmployeeAsync(employeeId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // Handle validation errors (e.g., "Cannot deactivate a SuperAdmin")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employee deactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deactivating employee {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // POST: api/v1/admin/employees/{employeeId:int}/reactivate
        // Reactivates a soft-deleted employee account.
        [HttpPost("{employeeId:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")] // Override class auth
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateEmployee(int employeeId)
        {
            try
            {
                // Call the service method, passing the ClaimsPrincipal for auth check
                var result = await _employeeService.ReactivateEmployeeAsync(employeeId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("not found")))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    // Handle validation errors (e.g., "already active")
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Employee reactivated successfully.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception reactivating employee {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}