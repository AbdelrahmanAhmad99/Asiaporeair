using Application.DTOs.FareBasisCode;
using Application.DTOs.Flight;
using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller is part of the Public API (customer-facing)
    // It handles flight searches, details, and fare rule retrieval
    [ApiController]
    [Area("Public API")] // Defined in architecture document
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly IFareBasisCodeService _fareBasisCodeService;
        private readonly ILogger<FlightsController> _logger;

        // Constructor for injecting the required services
        public FlightsController(
            IFlightService flightService,
            IFareBasisCodeService fareBasisCodeService,
            ILogger<FlightsController> logger)
        {
            _flightService = flightService;
            _fareBasisCodeService = fareBasisCodeService;
            _logger = logger;
        }

        // --- Flight Search & Details Endpoints ---

        // Searches for available flight itineraries based on complex criteria
        [HttpPost("search")]
        [AllowAnonymous] // Public search functionality
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> SearchFlights([FromBody] FlightSearchRequestDto searchRequest)
        {
            _logger.LogInformation("Attempting flight search for {Origin} to {Destination}",
                searchRequest.Segments.FirstOrDefault()?.OriginIataCode,
                searchRequest.Segments.FirstOrDefault()?.DestinationIataCode);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _flightService.SearchFlightsAsync(searchRequest);

            if (!result.IsSuccess)
            {
                // This is a 404 because the *search* was valid, but found 0 results.
                _logger.LogWarning("Flight search found no results: {Error}", result.Errors.FirstOrDefault());
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Flights found successfully", result.Data));
        }

        // Retrieves detailed information for a specific flight instance
        [HttpGet("{flightInstanceId}/details")]
        [AllowAnonymous] // Public can view flight details
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetFlightDetails([FromRoute] int flightInstanceId)
        {
            _logger.LogInformation("Retrieving details for FlightInstanceId: {Id}", flightInstanceId);

            if (flightInstanceId <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Invalid Flight ID." } });
            }

            var result = await _flightService.GetFlightDetailsAsync(flightInstanceId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to get details for FlightInstanceId {Id}: {Error}", flightInstanceId, result.Errors.FirstOrDefault());
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight details retrieved successfully", result.Data));
        }

        // Retrieves the available fare options (e.g., Lite, Standard) for a specific flight
        [HttpGet("{flightInstanceId}/fares")]
        [AllowAnonymous] // Public can view fare options
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetFareOptions([FromRoute] int flightInstanceId, [FromQuery] int passengers = 1)
        {
            _logger.LogInformation("Retrieving fare options for FlightInstanceId: {Id} for {Count} passengers", flightInstanceId, passengers);

            if (flightInstanceId <= 0 || passengers < 1)
            {
                var errors = new List<string>();
                if (flightInstanceId <= 0) errors.Add("Invalid Flight ID.");
                if (passengers < 1) errors.Add("Passenger count must be at least 1.");
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _flightService.GetFareOptionsForFlightAsync(flightInstanceId, passengers);

            if (!result.IsSuccess || (result.Data != null && !result.Data.Any()))
            {
                _logger.LogWarning("No fare options found for FlightInstanceId {Id}: {Error}", flightInstanceId, result.Errors.FirstOrDefault());
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.FirstOrDefault() ?? "No fare options found for this flight."));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fare options retrieved successfully", result.Data));
        }

        // Checks the real-time status of a flight by its flight number and date
        [HttpPost("status")]
        [AllowAnonymous] // Public flight status check
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetFlightStatus([FromBody] FlightStatusRequestDto statusRequest)
        {
            _logger.LogInformation("Checking status for FlightNumber: {FlightNumber} on {Date}", statusRequest.FlightNumber, statusRequest.FlightDate);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _flightService.GetFlightStatusByNumberAsync(statusRequest);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Status check failed for {FlightNumber}: {Error}", statusRequest.FlightNumber, result.Errors.FirstOrDefault());
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Flight status retrieved successfully", result.Data));
        }

        // Performs a quick check for seat availability on a flight
        [HttpGet("{flightInstanceId}/check-availability")]
        [AllowAnonymous] // Public check
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> CheckFlightAvailability([FromRoute] int flightInstanceId, [FromQuery] int passengers = 1)
        {
            _logger.LogInformation("Checking availability for FlightInstanceId: {Id}, Passengers: {Count}", flightInstanceId, passengers);

            if (flightInstanceId <= 0 || passengers < 1)
            {
                var errors = new List<string>();
                if (flightInstanceId <= 0) errors.Add("Invalid Flight ID.");
                if (passengers < 1) errors.Add("Passenger count must be at least 1.");
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _flightService.CheckFlightAvailabilityAsync(flightInstanceId, passengers);

            // If IsSuccess is false, it means availability check failed (e.g., not enough seats)
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Availability check failed for {Id}: {Error}", flightInstanceId, result.Errors.FirstOrDefault());
                // We return BadRequest (400) because the *request* for N seats is invalid, not that the flight (resource) wasn't found.
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Seats are available", result.Data));
        }


        // --- Fare Rules Endpoints ---

        // Retrieves the specific rules and description for a single fare basis code
        [HttpGet("fares/{code}")]
        [AllowAnonymous] // Public fare rule lookup
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetFareRulesByCode([FromRoute] string code)
        {
            _logger.LogInformation("Retrieving fare rules for Code: {Code}", code);

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Fare code cannot be empty." } });
            }

            var result = await _fareBasisCodeService.GetFareByCodeAsync(code);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Fare rules not found for {Code}: {Error}", code, result.Errors.FirstOrDefault());
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fare details retrieved successfully", result.Data));
        }

        // Retrieves all active, public fare basis codes (e.g., for a 'Compare Fares' page)
        [HttpGet("fares/all-active")]
        [AllowAnonymous] // Public information
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetAllActiveFares()
        {
            _logger.LogInformation("Retrieving all active fare codes");

            var result = await _fareBasisCodeService.GetAllActiveFaresAsync();

            if (!result.IsSuccess)
            {
                // This is likely a server/DB error, not a user error.
                _logger.LogError("Failed to retrieve all active fares: {Error}", result.Errors.FirstOrDefault());
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Active fare codes retrieved successfully", result.Data));
        }
    }
}