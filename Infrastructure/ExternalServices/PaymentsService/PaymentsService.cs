using Application.DTOs.Booking;
using Application.DTOs.Payment;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices.PaymentsService
{
    // Service implementation for handling real Stripe payments and Admin management for Asiaporeair.
    public class PaymentsService : IPaymentsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookingService _bookingService;  
        private readonly ITicketService _ticketService;    
        private readonly ILogger<PaymentsService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly StripeSettings _stripeSettings;

        public PaymentsService(
            IUnitOfWork unitOfWork,
            IBookingService bookingService,
            ITicketService ticketService,
            ILogger<PaymentsService> logger,
            IMapper mapper,
            IUserRepository userRepository,
            IOptions<StripeSettings> stripeOptions)
        {
            _unitOfWork = unitOfWork;
            _bookingService = bookingService;
            _ticketService = ticketService;
            _logger = logger;
            _mapper = mapper;
            _userRepository = userRepository;
            _stripeSettings = stripeOptions.Value;

            // Initialize Stripe Global Configuration
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        // ---------------------------------------------------------
        // Customer Flow: Initiate Payment
        // ---------------------------------------------------------

        public async Task<ServiceResult<PaymentIntentResponseDto>> CreatePaymentIntentAsync(CreatePaymentIntentDto createDto, ClaimsPrincipal user)
        {
            _logger.LogInformation("Initiating Stripe Payment Intent for Booking ID {BookingId}.", createDto.BookingId);

            // 1. Validate Booking Existence
            var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(createDto.BookingId);
            if (booking == null) return ServiceResult<PaymentIntentResponseDto>.Failure("Booking not found.");

            // 2. Security: Authorize User (Owner or Admin)
            var authResult = await AuthorizeBookingAccessAsync(user, booking);
            if (!authResult.IsSuccess) return ServiceResult<PaymentIntentResponseDto>.Failure(authResult.Errors);

            // 3. Validate Booking Status
            // We allow payment only if status is Pending. If it's already Confirmed, no need to pay.
            if (booking.PaymentStatus != "Pending")
            {
                return ServiceResult<PaymentIntentResponseDto>.Failure($"Booking is already {booking.PaymentStatus}. Payment cannot be processed.");
            }

             
            // 4. Calculate Amount 
            // Check if override is provided AND is greater than 0. Otherwise, use Booking Total.
            decimal amountDecimal;

            if (createDto.AmountOverride.HasValue && createDto.AmountOverride.Value > 0)
            {
                amountDecimal = createDto.AmountOverride.Value;
                _logger.LogInformation("Using AmountOverride: {Amount}", amountDecimal);
            }
            else
            {
                // Fallback to booking price
                amountDecimal = booking.PriceTotal ?? 0;
                _logger.LogInformation("Using Booking PriceTotal: {Amount}", amountDecimal);
            }

            // Final Validation
            if (amountDecimal <= 0)
            {
                _logger.LogWarning("Payment initiation failed for Booking {BookingId}: Amount is zero or negative ({Amount}).", createDto.BookingId, amountDecimal);
                return ServiceResult<PaymentIntentResponseDto>.Failure("Invalid payment amount. The calculated amount to pay is 0.");
            }
            // Stripe requires amount in the smallest currency unit (e.g., Cents for USD/SGD)
            long amountInCents = (long)(amountDecimal * 100);

            try
            {
                // 5. Check for existing local payment record for idempotency (optional optimization)
                // If a pending payment exists for this booking, we might want to cancel it or reuse it. 
                // For simplicity, we create a new Intent here.

                // 6. Call Stripe API to create Intent
                var service = new PaymentIntentService();
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = createDto.Currency.ToLower(),
                    Description = $"Asiaporeair Booking Ref: {booking.BookingRef}",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", booking.BookingId.ToString() },
                        { "BookingRef", booking.BookingRef },
                        { "UserId", booking.UserId.ToString() }
                    }
                };

                var intent = await service.CreateAsync(options);

                // 7. Save Initial Record in Database (Status = Pending)
                // We save this NOW so we have a link between Stripe ID and our DB ID before the webhook hits.
                var paymentRecord = new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = amountDecimal,
                    Method = "Stripe",
                    Status = "Pending",
                    TransactionId = intent.Id, // Crucial: Store the Stripe Intent ID
                    TransactionDateTime = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.Payments.AddAsync(paymentRecord);
                await _unitOfWork.SaveChangesAsync();

                // 8. Return Client Secret to Frontend
                var response = new PaymentIntentResponseDto
                {
                    PaymentIntentId = intent.Id,
                    ClientSecret = intent.ClientSecret,
                    PublishableKey = _stripeSettings.PublishableKey,
                    Amount = amountDecimal,
                    Currency = createDto.Currency
                };

                return ServiceResult<PaymentIntentResponseDto>.Success(response);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe API Error for Booking {BookingId}.", createDto.BookingId);

                // Handle null StripeError safely
                // If the error is local (such as missing key), the StripeError will be null.
                string errorMessage = ex.StripeError?.Message ?? ex.Message;

                return ServiceResult<PaymentIntentResponseDto>.Failure($"Stripe Error: {errorMessage}");
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Error creating payment intent for Booking {BookingId}.", createDto.BookingId);
                return ServiceResult<PaymentIntentResponseDto>.Failure("An internal error occurred while initializing payment.");
            }
        }

        // ---------------------------------------------------------
        // System Flow: Webhook Handling (The Source of Truth)
        // ---------------------------------------------------------

        public async Task<ServiceResult> HandleStripeWebhookAsync(string jsonPayload, string signatureHeader)
        {
            try
            { 
                // 1. Security Check: Ensure Webhook Secret exists 
                if (string.IsNullOrEmpty(_stripeSettings.WebhookSecret))
                {
                    _logger.LogError("Stripe Webhook Secret is missing or null in appsettings.");
                    return ServiceResult.Failure("Configuration Error: Webhook Secret is missing.");
                }
        
                // 2. Verify the event came from Stripe
                var stripeEvent = EventUtility.ConstructEvent(
                    jsonPayload,
                    signatureHeader,
                    _stripeSettings.WebhookSecret
                );

                
                // 2. Handle specific event types
                // Use string literals instead of 'Events.PaymentIntentSucceeded' to avoid CS0103
                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        _logger.LogInformation("Stripe Payment Succeeded: {Id}", paymentIntent.Id);
                        await ProcessSuccessfulPaymentAsync(paymentIntent);
                    }
                }
                // Use string literals instead of 'Events.PaymentIntentPaymentFailed'
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        _logger.LogWarning("Stripe Payment Failed: {Id}. Reason: {Reason}", paymentIntent.Id, paymentIntent.LastPaymentError?.Message);
                        await ProcessFailedPaymentAsync(paymentIntent);
                    }
                }
                else
                {
                    _logger.LogDebug("Unhandled Stripe Event: {Type}", stripeEvent.Type);
                }

                return ServiceResult.Success();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe Webhook Signature Verification Failed.");
                return ServiceResult.Failure("Webhook Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe Webhook.");
                return ServiceResult.Failure("Internal Processing Error");
            }
        }

        
        private async Task<ServiceResult> ProcessSuccessfulPaymentAsync(PaymentIntent paymentIntent)
        {
            var transactionId = paymentIntent.Id;
            var bookingIdFromMetadata = 0; // Default value

            // 1. Extract BookingId from Metadata
            if (paymentIntent.Metadata.TryGetValue("BookingId", out string bookingIdString) && int.TryParse(bookingIdString, out bookingIdFromMetadata))
            {
                _logger.LogInformation($"Processing success for PI: {transactionId}. Found BookingId: {bookingIdFromMetadata}");
            }
            else
            {
                _logger.LogError($"PI: {transactionId} is missing or has invalid BookingId in metadata.");
                return ServiceResult.Failure("Missing BookingId in metadata.");
            }

            // 2. Try searching first with TransactionId (standard case)
            var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(transactionId);

            // 3. If it is not found (due to Testing/CLI), we search for the pending Payment record using BookingId.
            if (payment == null)
            {
                // You should assume there is a way to get a single 'Pending' payment record for a particular booking
                // We will use the method that retrieves all booking payments and filter here.
                var paymentsForBooking = await _unitOfWork.Payments.GetByBookingAsync(bookingIdFromMetadata);
                payment = paymentsForBooking?.FirstOrDefault(p => p.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));

                if (payment != null)
                {
                    _logger.LogInformation($"Found PENDING payment ID {payment.PaymentId} using BookingId {bookingIdFromMetadata}.");

                    // Update missing/outdated TransactionId in the database
                    payment.TransactionId = transactionId;
                }
                else
                {
                    _logger.LogWarning($"No PENDING payment record found for BookingId: {bookingIdFromMetadata}.");
                    return ServiceResult.Failure($"No PENDING payment record found for BookingId: {bookingIdFromMetadata}.");
                }
            }
            else
            {
                _logger.LogInformation($"Found existing payment record ID {payment.PaymentId} using TransactionId {transactionId}.");
            }

            // 4. Implementing updates (native logic)
            if (payment.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Payment {payment.PaymentId} is already successful. Skipping update.");
                return ServiceResult.Success();
            }

            // Update payment status to Success
            payment.Status = "Success";

            _unitOfWork.Payments.Update(payment);

            // Calling in other services (BookingService and TicketService)
            await _bookingService.UpdateBookingStatusAsync(payment.BookingId, "Confirmed");
            await _ticketService.GenerateTicketsForBookingAsync(payment.BookingId);

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Payment ID {payment.PaymentId} processed successfully. Booking {payment.BookingId} confirmed.");
            return ServiceResult.Success();
        }
        // Helper to handle failure logic
        private async Task ProcessFailedPaymentAsync(PaymentIntent intent)
        {
            var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(intent.Id);
            //var payment = payments.FirstOrDefault();

            if (payment != null && payment.Status != "Failed")
            {
                payment.Status = "Failed";
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                // Optionally update booking status, but typically we keep booking as 'Pending' until it expires or pays
            }
        }

        // ---------------------------------------------------------
        // Admin / Management Flow
        // ---------------------------------------------------------

        public async Task<ServiceResult> RefundPaymentAsync(RefundRequestDto refundDto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("Initiating Refund for Payment ID {PaymentId}. Reason: {Reason}", refundDto.PaymentId, refundDto.Reason);

            // 1. Authorization (Admin/SuperAdmin Only)
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                return ServiceResult.Failure("Access Denied. Only Admins can perform refunds.");
            }

            // 2. Retrieve Local Payment
            var payment = await _unitOfWork.Payments.GetActiveByIdAsync(refundDto.PaymentId);
            if (payment == null) return ServiceResult.Failure("Payment record not found.");

            if (payment.Status != "Success") return ServiceResult.Failure($"Cannot refund payment with status '{payment.Status}'.");
            if (string.IsNullOrEmpty(payment.TransactionId)) return ServiceResult.Failure("No external transaction ID found for this payment.");

            try
            {
                // 3. Call Stripe Refund API
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.TransactionId,
                    Reason = MapRefundReason(refundDto.Reason), // Helper to map to Stripe enum
                };

                var refund = await refundService.CreateAsync(refundOptions);

                // 4. Update Local Record
                payment.Status = "Refunded";
                _unitOfWork.Payments.Update(payment);

                // 5. Update Booking Status (Optional business logic)
                // Usually, a refund implies cancellation
                var bookingStatusDto = new UpdateBookingStatusDto { NewStatus = "Cancelled", Reason = $"Refunded: {refundDto.Reason}" };
                await _bookingService.UpdateBookingPaymentStatusAsync(payment.BookingId, bookingStatusDto, performingUser);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Refund successful for Payment ID {Id}. Stripe Refund ID: {StripeId}", payment.PaymentId, refund.Id);
                return ServiceResult.Success();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe Refund Failed for Payment {Id}.", payment.PaymentId);
                return ServiceResult.Failure($"Stripe Refund Error: {ex.StripeError.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Error processing refund for Payment {Id}.", payment.PaymentId);
                return ServiceResult.Failure("An error occurred while processing the refund.");
            }
        }

        public async Task<ServiceResult<PaginatedResult<PaymentDto>>> SearchPaymentsAsync(PaymentFilterDto filter, int pageNumber, int pageSize)
        {
            // Similar logic to your existing search, but updated for the new structure
            try
            {
                Expression<Func<Payment, bool>> filterExpression = p => (filter.IncludeDeleted || !p.IsDeleted);

                if (filter.BookingId.HasValue)
                    filterExpression = filterExpression.And(p => p.BookingId == filter.BookingId.Value);

                if (!string.IsNullOrWhiteSpace(filter.BookingReference))
                    filterExpression = filterExpression.And(p => p.Booking.BookingRef.Contains(filter.BookingReference));

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    filterExpression = filterExpression.And(p => p.Status == filter.Status);

                if (filter.DateFrom.HasValue)
                    filterExpression = filterExpression.And(p => p.TransactionDateTime >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    filterExpression = filterExpression.And(p => p.TransactionDateTime <= filter.DateTo.Value);

                var (items, totalCount) = await _unitOfWork.Payments.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderByDescending(p => p.TransactionDateTime),
                    includeProperties: "Booking"
                );

                var dtos = _mapper.Map<List<PaymentDto>>(items);

                // Manual enrichment if Mapper fails on nested objects
                foreach (var dto in dtos)
                {
                    var matchedItem = items.FirstOrDefault(i => i.PaymentId == dto.PaymentId);
                    if (matchedItem?.Booking != null) dto.BookingReference = matchedItem.Booking.BookingRef;
                }

                return ServiceResult<PaginatedResult<PaymentDto>>.Success(new PaginatedResult<PaymentDto>(dtos, totalCount, pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payments.");
                return ServiceResult<PaginatedResult<PaymentDto>>.Failure("Search failed.");
            }
        }

        /// <summary>
        /// Confirms a payment intent on Stripe, attaching an optional payment method.
        /// After confirmation, if the status is 'succeeded', this will trigger the local
        /// payment success processing via the Stripe Webhook asynchronously.
        /// </summary>
        /// <param name="paymentIntentId">The Stripe Payment Intent ID (pi_...).</param>
        /// <param name="paymentMethodId">Optional Payment Method ID to attach.</param>
        /// <returns>A ServiceResult containing the final Stripe status string.</returns>
        public async Task<ServiceResult<string>> ConfirmPaymentAsync(string paymentIntentId, string? paymentMethodId = null, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
            {
                return ServiceResult<string>.Failure("Payment Intent ID is required for confirmation.");
            }

            try
            {
                var service = new PaymentIntentService();

                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId,
                    ReturnUrl = returnUrl // <--- *Pass the parameter to Stripe*
                };

                // Retrieve the current payment intent status before confirmation
                var currentIntent = await service.GetAsync(paymentIntentId);

                if (currentIntent.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    //  Removed the second argument. Logging the message instead.
                    _logger.LogInformation("Payment is already successful for PI: {PIId}. Skipping confirmation.", paymentIntentId);
                    return ServiceResult<string>.Success(currentIntent.Status);
                }
                // Check if the current state is one that allows confirmation.
                if (!currentIntent.Status.Equals("requires_confirmation", StringComparison.OrdinalIgnoreCase) &&
                    !currentIntent.Status.Equals("requires_action", StringComparison.OrdinalIgnoreCase) &&
                    !currentIntent.Status.Equals("requires_payment_method", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult<string>.Failure($"Payment Intent is in an unconfirmable state: {currentIntent.Status}");
                }

                // Call Stripe API to confirm the intent
                var paymentIntent = await service.ConfirmAsync(paymentIntentId, options);
                var status = paymentIntent.Status;  

                _logger.LogInformation($"Stripe PI {paymentIntentId} confirmed. Final Status: {status}");

                if (status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    // *** 1. Update local payment history and associated entities ***

                    // A. Searching for the local payment history using TransactionId (paymentIntentId)
                    var paymentRecord = await _unitOfWork.Payments.GetByTransactionIdAsync(paymentIntentId);

                    if (paymentRecord != null)
                    {
                        // B. Payment status update
                        paymentRecord.Status = "Success"; // Update payment status to "Succeeded"
                        paymentRecord.TransactionDateTime = DateTime.UtcNow;// Transaction time update
                        _unitOfWork.Payments.Update(paymentRecord);

                        // C. Booking status update
                        var booking = paymentRecord.Booking; // Let's assume the Repository has loaded the booking (Include Booking)
                        if (booking != null)
                        {
                            // Update booking status to paid status
                            booking.PaymentStatus = "Confirmed";
                            _unitOfWork.Bookings.Update(booking);

                            // D. Ticket Creation 
                            // The service function must be called to create the tickets (we rely on the injected ITicketService)
                            var ticketResult = await _ticketService.GenerateTicketsForBookingAsync(booking.BookingId);

                            if (!ticketResult.IsSuccess)
                            {
                                // This error should be recorded, but we consider the payment successful (Stripe Succeeded)
                                _logger.LogError("Failed to generate tickets for Booking {BookingId}. Errors: {Errors}",
                                    booking.BookingId, string.Join(", ", ticketResult.Errors));
                                // The reservation can also be placed in the "PaymentReceived_TicketsFailed" state, for example.
                            }
                        }

                        // e. Saving changes to the database
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("Booking {BookingId} and Payment {PaymentId} successfully updated to Succeeded. Tickets processed.",
                            paymentRecord.BookingId, paymentRecord.PaymentId);
                    }
                }

                _logger.LogInformation($"Stripe PI {paymentIntentId} confirmed. Final Status: {paymentIntent.Status}");



                return ServiceResult<string>.Success(paymentIntent.Status);
            }
            catch (StripeException ex)
            {
                //  Using ex.Message for logging and return.
                _logger.LogError(ex, $"Stripe API Error during PI confirmation for {paymentIntentId}. Details: {ex.Message}");
                return ServiceResult<string>.Failure($"Stripe API Confirmation Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"General error during PI confirmation for {paymentIntentId}.");
                return ServiceResult<string>.Failure("An unexpected error occurred during payment confirmation.");
            }
        }


        // ---------------------------------------------------------
        // Helpers and Lookups
        // ---------------------------------------------------------

        public async Task<ServiceResult<PaymentDto>> GetPaymentStatusAsync(string paymentIntentId)
        {
            // Used for polling from frontend
            var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(paymentIntentId);
            //var payment = payments.FirstOrDefault();

            if (payment == null) return ServiceResult<PaymentDto>.Failure("Payment not found.");

            // 2. Map to DTO
            var dto = _mapper.Map<PaymentDto>(payment);

            // 3. FIX: Manually populate BookingReference
            // AutoMapper ignores this field based on your profile, so we set it here.
            if (payment.Booking != null)
            {
                dto.BookingReference = payment.Booking.BookingRef;
            }

            // Refresh from Stripe if it's still pending in our DB? 
            // Optional: Check Stripe API directly if DB says pending but time has passed. 

            return ServiceResult<PaymentDto>.Success(dto);
              
        }

        public async Task<ServiceResult<PaymentDto>> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetActiveByIdAsync(paymentId);
            if (payment == null) return ServiceResult<PaymentDto>.Failure("Payment not found.");

            // Ensure Booking is loaded for reference
            if (payment.Booking == null) payment.Booking = await _unitOfWork.Bookings.GetActiveByIdAsync(payment.BookingId);

            var dto = _mapper.Map<PaymentDto>(payment);
            dto.BookingReference = payment.Booking?.BookingRef ?? "N/A";

            return ServiceResult<PaymentDto>.Success(dto);
        }

        public async Task<ServiceResult<IEnumerable<PaymentDto>>> GetPaymentsByBookingAsync(int bookingId, ClaimsPrincipal user)
        {
            // 1. Validate Booking
            var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
            if (booking == null) return ServiceResult<IEnumerable<PaymentDto>>.Failure("Booking not found.");

            // 2. Authorization (Owner or Admin)
            var authResult = await AuthorizeBookingAccessAsync(user, booking);
            if (!authResult.IsSuccess) return ServiceResult<IEnumerable<PaymentDto>>.Failure(authResult.Errors);

            // 3. Fetch Payments
            var payments = await _unitOfWork.Payments.GetByBookingAsync(bookingId);
            var dtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            foreach (var dto in dtos) dto.BookingReference = booking.BookingRef;

            return ServiceResult<IEnumerable<PaymentDto>>.Success(dtos);
        }

        private async Task<ServiceResult> AuthorizeBookingAccessAsync(ClaimsPrincipal user, Booking booking)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult.Failure("Authentication required.");

            // Check User Ownership
            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile != null && booking.UserId == userProfile.UserId) return ServiceResult.Success();

            // Check Admin Roles
            if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin") || user.IsInRole("Supervisor")) return ServiceResult.Success();

            return ServiceResult.Failure("Access Denied.");
        }

        private bool IsPaymentMethodValid(string method)
        {
            if (string.IsNullOrWhiteSpace(method)) return false;

            // Define supported payment methods.
            var supportedMethods = new[] {
                "CreditCard",
                "PayPal",
                "BankTransfer",
                "DigitalWallet" // For example
            };

            // Use case-insensitive comparison (OrdinalIgnoreCase) for robustness.
            return supportedMethods.Any(m => m.Equals(method, StringComparison.OrdinalIgnoreCase));
        }


        private string MapRefundReason(string reason)
        {
            // Map custom string to Stripe RefundReasons enum
            return reason.ToLower() switch
            {
                "duplicate" => RefundReasons.Duplicate,
                "fraudulent" => RefundReasons.Fraudulent,
                _ => RefundReasons.RequestedByCustomer
            };
        }
    }
}