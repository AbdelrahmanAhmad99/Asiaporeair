using Application.DTOs.Payment;
using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Errors;  
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    // API Controller for handling all financial transactions, Stripe integrations,
    // and administrative payment oversight for Asiaporeair.
    // Base route: api/v1/payments
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILogger<PaymentsController> _logger;

        // Dependency Injection of the specialized PaymentsService
        public PaymentsController(IPaymentsService paymentsService, ILogger<PaymentsController> logger)
        {
            _paymentsService = paymentsService;
            _logger = logger;
        }

         #region --- Public API (Customer Facing) ---

        // ==============================================================================================
        // SECTION 1: CUSTOMER & BOOKING FLOW
        // Operations related to initiating payments and checking status for passengers.
        // ==============================================================================================

        // POST: api/v1/payments/intent
        // Creates a Stripe Payment Intent. This is the first step in the secure payment flow.
        // The frontend will receive a ClientSecret to complete the transaction via Stripe.js.
        [HttpPost("intent")]
        [Authorize] // Requires Authenticated Passenger or Agent
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto createDto)
        {
            // Validate incoming DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                _logger.LogInformation("User {UserId} requesting payment intent for Booking {BookingId}.", User.Identity?.Name, createDto.BookingId);

                // Call service to create intent
                var result = await _paymentsService.CreatePaymentIntentAsync(createDto, User);

                if (!result.IsSuccess)
                {
                    // Map service errors to HTTP responses
                    if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                        return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));

                    if (result.Errors.Any(e => e.Contains("Access Denied", StringComparison.OrdinalIgnoreCase)))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, result.Errors.First()));

                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                // Return the ClientSecret and IntentID to the client
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Payment Intent created. Proceed to Stripe Gateway.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error creating payment intent for Booking {BookingId}.", createDto.BookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "System error processing payment request."));
            }
        }

        /// <summary>
        /// Confirms a Stripe Payment Intent using data provided by the client (e.g., after 3D Secure completion).
        /// This is often the final step the client triggers before the webhook is expected.
        /// Requires Authentication.
        /// </summary>
        /// <param name="confirmationDto">DTO containing PaymentIntentId and optional PaymentMethodId.</param>
        /// <returns>The final status of the Payment Intent after confirmation.</returns>
        [HttpPost("confirm")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiExceptionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto confirmationDto)
        {
            // The DTO must not be null for us to proceed and log correctly
            var piId = confirmationDto?.PaymentIntentId ?? "UNKNOWN";

            try
            {
                // 1. Validate incoming data model state
                if (!ModelState.IsValid)
                {
                    // FIX (CS1503): Using ApiValidationErrorResponse to return validation errors professionally.
                    _logger.LogWarning("Payment confirmation failed due to invalid model state for PI: {PIId}", piId);

                    var errorResponse = new ApiValidationErrorResponse();
                    // Extract all model errors and add them to the response's Errors list
                    errorResponse.Errors = ModelState.Where(e => e.Value != null && e.Value.Errors.Count > 0)
                                                     .SelectMany(x => x.Value!.Errors)
                                                     .Select(x => x.ErrorMessage)
                                                     .ToList();

                    return BadRequest(errorResponse);
                }

                // 2. Execute service logic
                var result = await _paymentsService.ConfirmPaymentAsync(confirmationDto.PaymentIntentId, confirmationDto.PaymentMethodId,confirmationDto.ReturnUrl);

                // 3. Handle Service Failure
                if (!result.IsSuccess)
                {
                    // FIX (CS1061 - Message): Using result.Errors to consolidate error messages.
                    var errorMessage = result.Errors.Any() ? string.Join("; ", result.Errors) : "Payment confirmation failed.";
                    _logger.LogWarning("Payment confirmation failed via service for PI: {PIId}. Error: {Error}", piId, errorMessage);
                    return BadRequest(new ApiExceptionResponse(StatusCodes.Status400BadRequest, errorMessage));
                }

                // 4. Handle Service Success (The Data property contains the final Stripe status)
                var status = result.Data;

                if (status.Equals("succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Payment confirmed successfully for PI: {PIId}. Status: Succeeded.", piId);
                    // Return 200 OK for final success
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Payment confirmed successfully and is Succeeded.", new { PaymentIntentId = piId, Status = status }));
                }
                else if (status.Equals("requires_action", StringComparison.OrdinalIgnoreCase) || status.Equals("requires_source_action", StringComparison.OrdinalIgnoreCase))
                {
                    // Payment requires further action (e.g., 3D Secure, or bank redirection). Return 202 Accepted.
                    _logger.LogWarning("Payment confirmed but requires client action for PI: {PIId}. Status: {Status}.", piId, status);
                    return StatusCode(StatusCodes.Status202Accepted, new ApiResponse(StatusCodes.Status202Accepted, "Payment requires additional client action (e.g., 3D Secure or redirection).", new { PaymentIntentId = piId, Status = status }));
                }
                else
                {
                    // Other non-successful status (e.g., requires_payment_method, failed, canceled, requires_capture)
                    _logger.LogWarning("Payment confirmation resulted in an inconclusive or non-final status for PI: {PIId}. Status: {Status}",
                        piId, status);
                    // Treat any other status as a failure to proceed from the client's perspective
                    return BadRequest(new ApiExceptionResponse(StatusCodes.Status400BadRequest, $"Payment confirmation resulted in status: {status}. Client should inspect status details."));
                }
            }
            catch (Exception ex)
            {
                // 5. Handle unexpected exceptions
                _logger.LogError(ex, "CRITICAL ERROR confirming payment for PI: {PIId}", piId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "An unexpected internal server error occurred during payment confirmation."));
            }
        }

        // GET: api/v1/payments/poll/{intentId}
        // A polling endpoint for the frontend to check the status of a specific PaymentIntent.
        // Useful if the Webhook is delayed or the UI needs immediate confirmation after 3DSecure.
        [HttpGet("poll/{intentId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaymentIntentStatus(string intentId)
        {
            if (string.IsNullOrEmpty(intentId))
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Intent ID is required." } });

            try
            {
                var result = await _paymentsService.GetPaymentStatusAsync(intentId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Payment intent not found in system."));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Status retrieved.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling status for Intent {IntentId}.", intentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "Error checking payment status."));
            }
        }

        // GET: api/v1/payments/history/booking/{bookingId}
        // Retrieves the full payment history for a specific booking.
        // Used by passengers to see receipts or by agents to verify payment status.
        [HttpGet("history/booking/{bookingId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBookingPaymentHistory(int bookingId)
        {
            try
            {
                var result = await _paymentsService.GetPaymentsByBookingAsync(bookingId, User);

                if (!result.IsSuccess)
                {
                    if (result.Errors.Any(e => e.Contains("Access Denied")))
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(StatusCodes.Status403Forbidden, "You do not have permission to view this booking's payments."));

                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, result.Errors.First()));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Payment history retrieved.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for Booking {BookingId}.", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "Internal error fetching history."));
            }
        }


        #endregion


         #region --- Admin API (Management System) ---

        // ==============================================================================================
        // SECTION 2: SYSTEM WEBHOOKS
        // Secure endpoints for external Payment Gateways (Stripe) to notify us of events.
        // ==============================================================================================

        // POST: api/v1/payments/webhook
        // The critical endpoint that receives asynchronous events from Stripe (Success, Fail, Refund).
        // This triggers the Ticket Generation and Booking Confirmation logic.
        [HttpPost("webhook")]
        [AllowAnonymous] // Stripe is an External system, no Bearer token provided
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];

            try
            {
                // Delegate processing to the Service layer
                var result = await _paymentsService.HandleStripeWebhookAsync(json, signatureHeader);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Webhook processing failed: {Error}", result.Errors.First());
                    // Return 400 so Stripe knows something went wrong (signature mismatch, etc.)
                    return BadRequest(result.Errors.First());
                }

                // Always return 200 OK to Stripe to acknowledge receipt
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Stripe Webhook.");
                // Return 500 so Stripe retries later
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ==============================================================================================
        // SECTION 3: ADMINISTRATION & FINANCE
        // Endpoints for SuperAdmins, Admins, and Finance Officers to manage the airport revenue.
        // ==============================================================================================

        // GET: api/v1/payments/admin/details/{paymentId}
        // Retrieves deep details of a specific payment transaction by its internal ID.
        [HttpGet("admin/details/{paymentId:int}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaymentDetailsAdmin(int paymentId)
        {
            try
            {
                var result = await _paymentsService.GetPaymentByIdAsync(paymentId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Payment record not found."));
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Payment details retrieved.", result.Data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin retrieval failed for Payment {PaymentId}.", paymentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "System error."));
            }
        }

        // POST: api/v1/payments/admin/refund
        // Process a refund for a customer. Requires high-level privileges.
        // This talks to Stripe to reverse the charge and updates our local DB.
        [HttpPost("admin/refund")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessRefund([FromBody] RefundRequestDto refundDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                _logger.LogWarning("Admin {AdminUser} initiating refund for Payment {PaymentId}. Reason: {Reason}", User.Identity?.Name, refundDto.PaymentId, refundDto.Reason);

                var result = await _paymentsService.RefundPaymentAsync(refundDto, User);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
                }

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Refund processed successfully. Funds reversed to customer.", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund process exception for Payment {PaymentId}.", refundDto.PaymentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "Critical error processing refund."));
            }
        }

        // GET: api/v1/payments/admin/search
        // Advanced search endpoint for the Finance Dashboard.
        // Supports filtering by Date Range, Amount, Status, Booking Ref, etc.
        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchPayments(
            [FromQuery] PaymentFilterDto filter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            // Sanitize pagination inputs
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize > 100 ? 100 : pageSize;

            try
            {
                var result = await _paymentsService.SearchPaymentsAsync(filter, pageNumber, pageSize);

                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "Search failed."));
                }

                // Construct a detailed response object
                var response = new
                {
                    Transactions = result.Data.Items,
                    Meta = new
                    {
                        TotalRecords = result.Data.TotalCount,
                        Page = result.Data.PageNumber,
                        Size = result.Data.PageSize,
                        TotalPages = result.Data.TotalPages,
                        FilterApplied = filter
                    }
                };

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Search results retrieved.", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin payment search.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "Search execution error."));
            }
        }

        // GET: api/v1/payments/admin/daily-revenue
        // A specialized endpoint utilizing the search service to provide a quick revenue snapshot for a specific day.
        // This is useful for the Admin Dashboard widgets.
        [HttpGet("admin/daily-revenue")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow.Date;

                // Create a filter for the specific day (00:00 to 23:59)
                var filter = new PaymentFilterDto
                {
                    DateFrom = targetDate,
                    DateTo = targetDate.AddDays(1).AddTicks(-1),
                    Status = "Success",
                    IncludeDeleted = false
                };

                // Fetch all payments for that day (using a large page size to aggregate)
                // Note: For massive scale, we would add a dedicated Aggregate method in Service.
                // utilizing existing Search capability for now.
                var result = await _paymentsService.SearchPaymentsAsync(filter, 1, 10000);

                if (result.IsSuccess && result.Data.Items != null)
                {
                    var totalRevenue = result.Data.Items.Sum(p => p.Amount);
                    var count = result.Data.Items.Count();

                    var stats = new
                    {
                        Date = targetDate.ToString("yyyy-MM-dd"),
                        TotalRevenue = totalRevenue,
                        TransactionCount = count,
                        Currency = "SGD" // Base currency for Asiaporeair
                    };

                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Daily revenue calculated.", stats));
                }

                return BadRequest(new ApiExceptionResponse(StatusCodes.Status400BadRequest, "Could not calculate revenue."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating daily revenue.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(StatusCodes.Status500InternalServerError, "Calculation error."));
            }
        }

        #endregion


    }
}