using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    // Paginated result class remains the same
    public class PaginatedBookingsResult
    {
        public int TotalCount { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public interface IBookingRepository : IGenericRepository<Booking>
    {


        /// <summary>
        /// Retrieves an active booking by its unique ID.
        /// </summary>
        /// <param name="bookingId">The primary key ID of the booking.</param>
        /// <returns>The Booking entity if found and active; otherwise, null.</returns>
        Task<Booking?> GetActiveByIdAsync(int bookingId);

        /// <summary>
        /// Retrieves an active booking by its unique booking reference code.
        /// </summary>
        /// <param name="bookingReference">The unique reference code (e.g., "ABC123XYZ").</param>
        /// <returns>The Booking entity if found and active; otherwise, null.</returns>
        Task<Booking?> GetByReferenceAsync(string bookingReference);

        /// <summary>
        /// Retrieves an active booking with comprehensive details including Passengers (with User/FrequentFlyer),
        /// FlightInstance (with Schedule/Route/Airports/Aircraft/Type/Airline), assigned Seats,
        /// AncillarySales (with Products), Payment details, and FareBasisCode.
        /// </summary>
        /// <param name="bookingId">The booking ID.</param>
        /// <returns>A detailed Booking entity if found and active; otherwise, null.</returns>
        Task<Booking?> GetWithDetailsAsync(int bookingId); // Existing method, signature confirmed

        /// <summary>
        /// Retrieves paginated active bookings made by a specific user (identified by AppUser ID).
        /// Includes basic flight and user details.
        /// </summary>
        /// <param name="userId">The AppUser ID (string GUID from AspNetUsers).</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of bookings per page.</param>
        /// <returns>A paginated result containing the user's active bookings.</returns>
        Task<PaginatedBookingsResult> GetPaginatedByUserAsync(string userId, int pageNumber, int pageSize); // Existing method, signature confirmed

        /// <summary>
        /// Retrieves all active bookings associated with a specific flight instance.
        /// Useful for generating passenger manifests in the management system.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <returns>An enumerable collection of active Booking entities for the flight.</returns>
        Task<IEnumerable<Booking>> GetByFlightInstanceAsync(int flightInstanceId);

        /// <summary>
        /// Finds active bookings containing a passenger with the specified passport number (case-insensitive).
        /// Useful for locating bookings in the management system.
        /// </summary>
        /// <param name="passportNumber">The passport number to search for.</param>
        /// <returns>An enumerable collection of active Booking entities containing the passenger.</returns>
        Task<IEnumerable<Booking>> FindByPassengerPassportAsync(string passportNumber);

        /// <summary>
        /// Retrieves active bookings created within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active Booking entities created within the range.</returns>
        Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves active bookings based on their current payment status (case-insensitive).
        /// </summary>
        /// <param name="status">The payment status string (e.g., 'Pending', 'Confirmed', 'Cancelled').</param>
        /// <returns>An enumerable collection of active Booking entities with the specified status.</returns>
        Task<IEnumerable<Booking>> GetByPaymentStatusAsync(string status);

        /// <summary>
        /// Retrieves all bookings (including soft-deleted) with pagination.
        /// Primarily for administrative dashboards and auditing.
        /// </summary>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A paginated result containing all bookings.</returns>
        Task<PaginatedBookingsResult> GetAllIncludingDeletedPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Updates the payment status for a specific booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking to update.</param>
        /// <param name="newStatus">The new payment status string.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdatePaymentStatusAsync(int bookingId, string newStatus);

        /// <summary>
        /// Checks if a booking with the specified reference code exists (active or soft-deleted).
        /// </summary>
        /// <param name="bookingReference">The booking reference code.</param>
        /// <returns>True if a booking with the reference exists; otherwise, false.</returns>
        Task<bool> ExistsByReferenceAsync(string bookingReference);
         
        // Retrieves active bookings for a specific User (passenger profile) ID.
        Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);

    }
}