using System.Security.Claims;
using System.Threading.Tasks; 
using Application.DTOs.Booking;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Creates a new booking for the user.
        /// </summary>
        /// <param name="createDto">Booking creation data, including passenger list and trip details.</param>
        /// <param name="userId">Booking user ID.</param>
        /// <returns>Result of the operation with the created booking details.</returns>
        Task<ServiceResult<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, string userId);

        /// <summary>
        /// Retrieves a specific booking with all its details (passengers, seats, extras).
        /// </summary>
        /// <param name="bookingId">Booking ID.</param>
        /// <returns>Result of the operation with booking details.</returns>
        Task<ServiceResult<BookingDto>> GetBookingByIdAsync(int bookingId);

        /// <summary>
        /// The booking is confirmed after payment is completed.
        /// </summary>
        /// <param name="bookingId">Booking ID.</param>
        /// <returns>Transaction result with booking confirmation.</returns>
        Task<ServiceResult<BookingConfirmationDto>> ConfirmBookingAsync(int bookingId);

        /// <summary>
        /// Cancels an existing reservation.
        /// </summary>
        /// <param name="bookingId">Booking ID to be canceled.</param>
        /// <param name="userId">User ID who made the reservation.</param>
        /// <returns>Result of the operation.</returns>
        Task<ServiceResult> CancelBookingAsync(int bookingId, string userId);

        // Creates a new booking in a Pending state.
        Task<ServiceResult<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, ClaimsPrincipal user);

        // Retrieves a booking summary by its ID. Requires authorization.
        Task<ServiceResult<BookingDto>> GetBookingSummaryByIdAsync(int bookingId, ClaimsPrincipal user);

        // Retrieves detailed booking information by ID. Requires authorization.
        Task<ServiceResult<BookingDetailDto>> GetBookingDetailsByIdAsync(int bookingId, ClaimsPrincipal user);

        // Retrieves detailed booking information by reference code. Requires authorization.
        Task<ServiceResult<BookingDetailDto>> GetBookingDetailsByReferenceAsync(string bookingReference, ClaimsPrincipal user);

        // Retrieves all bookings for the currently authenticated user (paginated).
        Task<ServiceResult<PaginatedResult<BookingDto>>> GetMyBookingsAsync(ClaimsPrincipal user, int pageNumber, int pageSize);

        // Updates the payment status of a booking (e.g., after payment confirmation or cancellation).
        Task<ServiceResult> UpdateBookingPaymentStatusAsync(int bookingId, UpdateBookingStatusDto statusDto, ClaimsPrincipal? performingUser = null); // User optional for system updates

        // Cancels a booking. Requires authorization and validation.
        Task<ServiceResult> CancelBookingAsync(int bookingId, ClaimsPrincipal user, string? reason = null);
 
        // Retrieves a simplified list of passengers for a specific flight instance (Passenger Manifest).
        Task<ServiceResult<IEnumerable<object>>> GetPassengerManifestForFlightAsync(int flightInstanceId);
        /// <summary>
        /// Updates the status of a specific booking (e.g., from Pending to Confirmed or Cancelled).
        /// </summary>
        /// <param name="bookingId">The ID of the booking to update.</param>
        /// <param name="newStatus">The new status to set (e.g., "Confirmed").</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> UpdateBookingStatusAsync(int bookingId, string newStatus);

    }
}