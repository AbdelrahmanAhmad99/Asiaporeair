using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Payment;
using Application.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IPaymentsService
    {
        // --- Customer / Frontend Methods ---
        // Creates a Stripe Payment Intent. This is the first step when a user clicks "Pay Now".
        // It calculates the amount, contacts Stripe, and saves a "Pending" record in the DB.
        Task<ServiceResult<PaymentIntentResponseDto>> CreatePaymentIntentAsync(CreatePaymentIntentDto createDto, ClaimsPrincipal user);

        // Confirms a payment intent on Stripe. This is typically used to complete the payment
        // after a 3D Secure action or a manual confirmation flow.
        Task<ServiceResult<string>> ConfirmPaymentAsync(string paymentIntentId, string? paymentMethodId = null, string? returnUrl = null);

        // Polling endpoint to check if the payment status has updated in the DB (useful if webhook is delayed).
        Task<ServiceResult<PaymentDto>> GetPaymentStatusAsync(string paymentIntentId);

        // --- System / Webhook Methods ---
        // Handles the secure callback from Stripe (Webhook).
        // This is the source of truth for confirming payments and triggering ticket generation.
        Task<ServiceResult> HandleStripeWebhookAsync(string jsonPayload, string signatureHeader);

        // --- Admin / Management Methods ---
        // Retrieves a single payment by its internal ID.
        Task<ServiceResult<PaymentDto>> GetPaymentByIdAsync(int paymentId);

        // Retrieves all payments associated with a specific booking (User history or Admin view).
        Task<ServiceResult<IEnumerable<PaymentDto>>> GetPaymentsByBookingAsync(int bookingId, ClaimsPrincipal user);

        // Advanced search for Admins (Filter by date, amount, status, etc.).
        Task<ServiceResult<PaginatedResult<PaymentDto>>> SearchPaymentsAsync(PaymentFilterDto filter, int pageNumber, int pageSize);

        // Process a refund via Stripe and update the database. Restricted to Admin/SuperAdmin.
        Task<ServiceResult> RefundPaymentAsync(RefundRequestDto refundDto, ClaimsPrincipal performingUser);

    }
} 