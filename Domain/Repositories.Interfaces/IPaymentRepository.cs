using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Payment entity, extending the generic repository.
    /// Provides methods for querying payment records associated with bookings.
    /// </summary>
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        /// <summary>
        /// Retrieves an active payment record by its unique ID.
        /// </summary>
        /// <param name="paymentId">The primary key ID of the payment.</param>
        /// <returns>The Payment entity if found and active; otherwise, null.</returns>
        Task<Payment?> GetActiveByIdAsync(int paymentId);

        /// <summary>
        /// Retrieves the active payment record associated with a specific booking ID.
        /// Includes Booking details for context.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>The active Payment entity linked to the booking, if found; otherwise, null.</returns>
        //Task<Payment?> GetByBookingAsync(int bookingId); // Existing method, enhanced return type
        Task<IEnumerable<Payment>> GetByBookingAsync(int bookingId);
        /// <summary>
        /// Retrieves all active payment records made within a specific date range based on transaction time.
        /// Useful for financial reporting in the management system. Includes Booking details.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active Payment entities within the date range.</returns>
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves all active payment records made using a specific payment method (case-insensitive).
        /// Useful for filtering payments in the management system. Includes Booking details.
        /// </summary>
        /// <param name="method">The payment method name (e.g., 'CreditCard', 'PayPal').</param>
        /// <returns>An enumerable collection of active Payment entities matching the method.</returns>
        Task<IEnumerable<Payment>> GetByMethodAsync(string method);

        /// <summary>
        /// Calculates the total revenue from active payments within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>The total sum of payment amounts within the range.</returns>
        Task<decimal> GetTotalRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves all payment records, including those marked as soft-deleted.
        /// For administrative review or auditing.
        /// </summary>
        /// <returns>An enumerable collection of all Payment entities.</returns>
        Task<IEnumerable<Payment>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) payment records.
        /// </summary>
        /// <returns>An enumerable collection of active Payment entities.</returns>
        Task<IEnumerable<Payment>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a payment record exists for a specific booking (active only).
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>True if an active payment record exists for the booking; otherwise, false.</returns>
        Task<bool> ExistsForBookingAsync(int bookingId);


        /// <summary>
        /// Updates payment status after confirmation.
        /// </summary>
        /// <param name="paymentId">Payment ID.</param>
        /// <param name="status">New status.</param>
        /// <returns>Task completion.</returns>
        Task UpdateStatusAsync(int paymentId, string status);

        /// <summary>
        /// Retrieves a payment record by the external gateway transaction ID (e.g., Stripe PaymentIntentId).
        /// </summary>
        /// <param name="transactionId">The external transaction ID.</param>
        /// <returns>The payment record if found.</returns>
        Task<Payment?> GetByTransactionIdAsync(string transactionId);

    }
}
 