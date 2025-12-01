using Application.DTOs.Passenger;
using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.Errors;  
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // This controller manages a user's list of saved passengers (e.g., family members).
    [ApiController] 
    [Area("Public API")] // As per architecture doc 
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize] // All actions require a logged-in user
    public class PassengersController : ControllerBase
    {
        private readonly IPassengerService _passengerService;
        private readonly ILogger<PassengersController> _logger;

        // Inject the passenger service
        public PassengersController(
            IPassengerService passengerService,
            ILogger<PassengersController> logger)
        {
            _passengerService = passengerService;
            _logger = logger;
        }

        // Retrieves all passenger profiles saved to the current user's account
        [HttpGet("my-passengers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetMySavedPassengers()
        {
            _logger.LogInformation("User {UserEmail} retrieving saved passengers list.", User.Identity?.Name);
            try
            {
                // Call the service method
                var result = await _passengerService.GetMyPassengersAsync(User);

                if (result.IsSuccess)
                {
                    // Return 200 OK with data and custom message
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Saved passengers retrieved successfully", result.Data));
                }

                // Handle service failures
                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("Authentication required", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error retrieving passengers for user {UserEmail}.", User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Retrieves a specific passenger profile (must be owned by user)
        [HttpGet("{passengerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetSavedPassenger([FromRoute] int passengerId)
        {
            _logger.LogDebug("User {UserEmail} retrieving PassengerId {PassengerId}.", User.Identity?.Name, passengerId);
            try
            {
                // Call the service method
                var result = await _passengerService.GetPassengerByIdAsync(passengerId, User);

                if (result.IsSuccess)
                {
                    // Return 200 OK with data and custom message
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Passenger details retrieved successfully", result.Data));
                }

                // Handle service failures
                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
                }
                if (error.Contains("Authentication required", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error retrieving PassengerId {PassengerId} for user {UserEmail}.", passengerId, User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Updates a passenger profile saved to the user's account
        [HttpPut("{passengerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> UpdateSavedPassenger([FromRoute] int passengerId, [FromBody] UpdatePassengerDto updateDto)
        {
            // Explicitly handle invalid model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            _logger.LogInformation("User {UserEmail} updating PassengerId {PassengerId}.", User.Identity?.Name, passengerId);
            try
            {
                // Call the service method
                var result = await _passengerService.UpdatePassengerAsync(passengerId, updateDto, User);

                if (result.IsSuccess)
                {
                    // Return 200 OK with updated data and custom message
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Passenger updated successfully", result.Data));
                }

                // Handle service failures
                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
                }
                if (error.Contains("Authentication required", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, error));
                }
                // Handle business logic errors (like duplicate passport)
                if (error.Contains("Passport number", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error updating PassengerId {PassengerId} for user {UserEmail}.", passengerId, User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        // Deletes a passenger profile from the user's account
        [HttpDelete("{passengerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse))]
        public async Task<IActionResult> DeleteSavedPassenger([FromRoute] int passengerId)
        {
            _logger.LogInformation("User {UserEmail} deleting PassengerId {PassengerId}.", User.Identity?.Name, passengerId);
            try
            {
                // Call the service method
                var result = await _passengerService.DeletePassengerAsync(passengerId, User);

                if (result.IsSuccess)
                {
                    // Return 200 OK with no data and custom message
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Passenger profile deleted successfully"));
                }

                // Handle service failures
                var error = result.Errors.FirstOrDefault() ?? "An unknown error occurred.";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, error));
                }
                if (error.Contains("denied", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, error));
                }
                if (error.Contains("Authentication required", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, error));
                }
                // Handle business logic errors (passenger on active booking)
                if (error.Contains("associated with active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
                }
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error deleting PassengerId {PassengerId} for user {UserEmail}.", passengerId, User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}