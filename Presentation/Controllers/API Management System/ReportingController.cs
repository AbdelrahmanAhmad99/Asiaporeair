using Application.DTOs.Reporting;
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
    // This controller provides endpoints for generating detailed operational reports.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/reports")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Reports are sensitive data
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(IReportingService reportingService, ILogger<ReportingController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }

        #region --- Report Generation Endpoints ---

        // GET: api/v1/admin/reports/sales-summary
        // Generates a comprehensive sales and revenue summary.
        [HttpGet("sales-summary")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSalesSummaryReport([FromQuery] ReportRequestDto request)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            if (request.StartDate > request.EndDate)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
            }

            try
            {
                // Call the service method
                var result = await _reportingService.GetSalesSummaryReportAsync(request);

                if (!result.IsSuccess)
                {
                    // This is likely an internal data aggregation error
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Sales summary report retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception generating Sales Summary Report.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/reports/flight-performance
        // Generates an operational report on flight performance (delays, cancellations).
        [HttpGet("flight-performance")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFlightPerformanceReport([FromQuery] ReportRequestDto request)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            if (request.StartDate > request.EndDate)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
            }

            try
            {
                // Call the service method
                var result = await _reportingService.GetFlightPerformanceReportAsync(request);

                if (!result.IsSuccess)
                {
                    // Handle "No flights found" error from service
                    if (result.Errors.Any(e => e.Contains("No flights found")))
                    {
                        return Ok(new ApiResponse(StatusCodes.Status200OK, "No flight performance data found for this range.", null));
                    }
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight performance report retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception generating Flight Performance Report.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/reports/load-factor
        // Generates a report on flight occupancy and load factors.
        [HttpGet("load-factor")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLoadFactorReport([FromQuery] ReportRequestDto request)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            if (request.StartDate > request.EndDate)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
            }

            try
            {
                // Call the service method
                var result = await _reportingService.GetLoadFactorReportAsync(request);

                if (!result.IsSuccess)
                {
                    // Handle "No operated flights found" error from service
                    if (result.Errors.Any(e => e.Contains("No operated flights found")))
                    {
                        return Ok(new ApiResponse(StatusCodes.Status200OK, "No load factor data found for this range.", null));
                    }
                    if (result.Errors.Any(e => e.Contains("No flights with seat capacity found")))
                    {
                        return Ok(new ApiResponse(StatusCodes.Status200OK, "No flights with readable seat capacity found for this range.", null));
                    }

                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Load factor report retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception generating Load Factor Report.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/reports/passenger-manifest/{flightInstanceId:int}
        // Retrieves the full passenger manifest for a single flight instance.
        [HttpGet("passenger-manifest/{flightInstanceId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPassengerManifest(int flightInstanceId)
        {
            try
            {
                // Call the service method
                var result = await _reportingService.GetPassengerManifestAsync(flightInstanceId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Passenger manifest retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception generating Passenger Manifest for FlightInstanceId {FlightId}.", flightInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/reports/daily-departure-manifests
        // Retrieves a list of manifests for all flights departing from an airport on a specific day.
        [HttpGet("daily-departure-manifests")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyDepartureManifests(
            [FromQuery][Required] string airportIataCode,
            [FromQuery][Required] DateTime forDate)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            try
            {
                // Call the service method
                var result = await _reportingService.GetDailyDepartureManifestsAsync(airportIataCode, forDate);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Daily departure manifests retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving daily departure manifests for {Airport} on {Date}.", airportIataCode, forDate);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}