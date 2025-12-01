using Application.DTOs.FlightOperations; 
using Application.DTOs.Reporting;  
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
    // This controller provides aggregated data for the main admin dashboard widgets.
    // It is part of the Admin API (Airport Management System).
    [ApiController]
    [Route("api/v1/admin/dashboard")] // Admin area route
    [Produces("application/json")]
    [Authorize(Roles = "Admin, SuperAdmin, Supervisor")] // Only management roles can view dashboards
    public class DashboardController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly IFlightOperationsService _flightOpsService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IReportingService reportingService,
            IFlightOperationsService flightOpsService,
            ILogger<DashboardController> logger)
        {
            _reportingService = reportingService;
            _flightOpsService = flightOpsService;
            _logger = logger;
        }

        #region --- Dashboard Widgets API ---

        // GET: api/v1/admin/dashboard/operational
        // Retrieves the main operational dashboard summary for a specific airport and date.
        [HttpGet("operational")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOperationalDashboard(
            [FromQuery] string airportIataCode = "SIN", // Default to Singapore
            [FromQuery] DateTime? date = null)
        {
            try
            {
               var airportIataCodeToUpper = airportIataCode.ToUpper();
                // Default to today's date if not provided
                var queryDate = date ?? DateTime.UtcNow.Date;

                // Call the service method
                var result = await _flightOpsService.GetOperationalDashboardAsync(airportIataCodeToUpper, queryDate);

                if (!result.IsSuccess)
                {
                    // This is likely an internal data aggregation error
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Operational dashboard retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving operational dashboard for {Airport} on {Date}.", airportIataCode, date);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/dashboard/fids-departures
        // Retrieves live departure data for the Flight Information Display System (FIDS) widget.
        [HttpGet("fids-departures")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFidsDepartures([FromQuery] string airportIataCode = "SIN")
        {
            try
            {
               var airportIataCodeToUpper = airportIataCode.ToUpper();
                // Call the service method
                var result = await _flightOpsService.GetFlightInstancesForFidsAsync(airportIataCodeToUpper, "Departure");

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Live departures retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving FIDS departures for {Airport}.", airportIataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/dashboard/fids-arrivals
        // Retrieves live arrival data for the Flight Information Display System (FIDS) widget.
        [HttpGet("fids-arrivals")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFidsArrivals([FromQuery] string airportIataCode = "SIN")
        {
            try
            {
                var airportIataCodeToUpper = airportIataCode.ToUpper();
                // Call the service method
                var result = await _flightOpsService.GetFlightInstancesForFidsAsync(airportIataCodeToUpper, "Arrival");

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Live arrivals retrieved successfully.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving FIDS arrivals for {Airport}.", airportIataCode);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/dashboard/sales-summary
        // Retrieves a high-level sales and revenue summary for a date range (defaults to today).
        [HttpGet("sales-summary")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        // --- (FIX APPLIED HERE) ---
        public async Task<IActionResult> GetSalesSummaryReportAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? airlineIataCode = null)
        {
            try
            {
               var airlineIataCodeToUpper = airlineIataCode?.ToUpper();
                var request = new ReportRequestDto
                {
                    StartDate = startDate ?? DateTime.UtcNow.Date,
                    EndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1), // End of today
                    AirlineIataCode = airlineIataCodeToUpper
                };

                if (request.StartDate > request.EndDate)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
                }

                // Call the service method
                var result = await _reportingService.GetSalesSummaryReportAsync(request);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return a simplified widget view
                var widgetData = new
                {
                    totalBookingRevenue = result.Data.TotalBookingRevenue,
                    totalAncillaryRevenue = result.Data.TotalAncillaryRevenue,
                    totalRevenue = result.Data.TotalRevenue,
                    totalBookingsConfirmed = result.Data.TotalBookingsConfirmed,
                    totalPassengers = result.Data.TotalPassengers
                };

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Sales summary widget retrieved successfully.", widgetData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving sales summary widget.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/dashboard/performance-summary
        // Retrieves a high-level flight performance summary (on-time, delays) for a date range (defaults to today).
        [HttpGet("performance-summary")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
         public async Task<IActionResult> GetFlightPerformanceReportAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? airportIataCode = null)
        {
            try
            {
                var airportIataCodeToUpper = airportIataCode?.ToUpper();
                var request = new ReportRequestDto
                {
                    StartDate = startDate ?? DateTime.UtcNow.Date,
                    EndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1), // End of today
                    AirportIataCode = airportIataCodeToUpper
                };

                if (request.StartDate > request.EndDate)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
                }

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

                // Return a simplified widget view
                var widgetData = new
                {
                    totalFlightsScheduled = result.Data.TotalFlightsScheduled,
                    totalFlightsOperated = result.Data.TotalFlightsOperated,
                    flightsCancelled = result.Data.FlightsCancelled,
                    onTimeDeparturePercentage = result.Data.OnTimeDeparturePercentage,
                    onTimeArrivalPercentage = result.Data.OnTimeArrivalPercentage,
                    averageDepartureDelayMinutes = result.Data.AverageDepartureDelayMinutes
                };

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight performance widget retrieved successfully.", widgetData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving performance summary widget.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        // GET: api/v1/admin/dashboard/loadfactor-summary
        // Retrieves a high-level load factor summary for a date range (defaults to today).
        [HttpGet("loadfactor-summary")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLoadFactorReportAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var request = new ReportRequestDto
                {
                    StartDate = startDate ?? DateTime.UtcNow.Date,
                    EndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1) // End of today
                };

                if (request.StartDate > request.EndDate)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Start date must be before end date." } });
                }

                // Call the service method
                var result = await _reportingService.GetLoadFactorReportAsync(request);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("No operated flights found")))
                    {
                        return Ok(new ApiResponse(StatusCodes.Status200OK, "No load factor data found for this range.", null));
                    }
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return a simplified widget view
                var widgetData = new
                {
                    totalFlightsOperated = result.Data.TotalFlightsOperated,
                    totalCapacityOffered = result.Data.TotalCapacityOffered,
                    totalPassengersConfirmed = result.Data.TotalPassengersConfirmed,
                    averageLoadFactorPercent = result.Data.AverageLoadFactorPercent
                };

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Load factor widget retrieved successfully.", widgetData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception retrieving load factor summary widget.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace?.ToString()));
            }
        }

        #endregion
    }
}