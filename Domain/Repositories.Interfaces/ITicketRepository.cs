using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    // Paginated result class remains the same
    public class PaginatedTicketsResult
    {
        public int TotalCount { get; set; }
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    /// <summary>
    /// Repository interface for Ticket entity, extending the generic repository.
    /// Provides methods for querying and managing e-tickets issued for bookings.
    /// Essential for passenger check-in, boarding, and booking management.
    /// </summary>
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        /// <summary>
        /// Retrieves an active ticket by its unique ID.
        /// </summary>
        /// <param name="ticketId">The primary key ID of the ticket.</param>
        /// <returns>The Ticket entity if found and active; otherwise, null.</returns>
        Task<Ticket?> GetActiveByIdAsync(int ticketId);

        /// <summary>
        /// Retrieves an active ticket by its unique ticket code.
        /// Includes related Booking, Passenger, FlightInstance, and Seat details.
        /// </summary>
        /// <param name="ticketCode">The unique code associated with the ticket.</param>
        /// <returns>The detailed Ticket entity if found and active; otherwise, null.</returns>
        Task<Ticket?> GetByCodeWithDetailsAsync(string ticketCode);

        /// <summary>
        /// Adds multiple Ticket entities, typically when generating tickets for a booking.
        /// Note: The original signature only took tickets; adding bookingId might be redundant if tickets already contain it.
        /// </summary>
        /// <param name="tickets">An enumerable collection of Ticket entities to add.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddMultipleAsync(IEnumerable<Ticket> tickets); // Simplified signature

        /// <summary>
        /// Retrieves paginated active tickets associated with a specific user (via Booking -> User -> AppUser).
        /// Includes basic details like flight number and passenger name.
        /// </summary>
        /// <param name="userId">The AppUser ID (string GUID from AspNetUsers).</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of tickets per page.</param>
        /// <returns>A paginated result containing the user's active tickets.</returns>
        Task<PaginatedTicketsResult> GetPaginatedByUserAsync(string userId, int pageNumber, int pageSize); // Existing method, signature confirmed

        /// <summary>
        /// Retrieves all active tickets associated with a specific booking ID.
        /// Includes Passenger and Seat details.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of active Ticket entities for the booking.</returns>
        Task<IEnumerable<Ticket>> GetByBookingAsync(int bookingId); // Existing method, enhanced return type

        /// <summary>
        /// Retrieves all active tickets issued for a specific flight instance.
        /// Crucial for passenger manifests and gate management. Includes Passenger and Seat details.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <returns>An enumerable collection of active Ticket entities for the flight.</returns>
        Task<IEnumerable<Ticket>> GetByFlightInstanceAsync(int flightInstanceId);

        /// <summary>
        /// Retrieves the active ticket associated with a specific passenger on a specific booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>The active Ticket entity if found; otherwise, null.</returns>
        Task<Ticket?> GetByBookingAndPassengerAsync(int bookingId, int passengerId);

        /// <summary>
        /// Retrieves active tickets based on their current status (e.g., 'Issued', 'Used', 'Cancelled').
        /// </summary>
        /// <param name="status">The status string (case-insensitive).</param>
        /// <returns>An enumerable collection of active Ticket entities with the specified status.</returns>
        Task<IEnumerable<Ticket>> GetByStatusAsync(string status);

        /// <summary>
        /// Retrieves active tickets issued within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active Ticket entities issued within the range.</returns>
        Task<IEnumerable<Ticket>> GetByIssueDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves all tickets, including those marked as soft-deleted.
        /// For administrative review or historical data lookup.
        /// </summary>
        /// <returns>An enumerable collection of all Ticket entities.</returns>
        Task<IEnumerable<Ticket>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) tickets.
        /// </summary>
        /// <returns>An enumerable collection of active Ticket entities.</returns>
        Task<IEnumerable<Ticket>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a ticket with the specified code exists (active or soft-deleted).
        /// </summary>
        /// <param name="ticketCode">The ticket code to check.</param>
        /// <returns>True if a ticket with the code exists; otherwise, false.</returns>
        Task<bool> ExistsByCodeAsync(string ticketCode);

        /// <summary>
        /// Updates the status of a specific ticket (e.g., to 'Used' upon boarding).
        /// </summary>
        /// <param name="ticketId">The ID of the ticket to update.</param>
        /// <param name="newStatus">The new status string.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateStatusAsync(int ticketId, string newStatus);


        /// <summary>
        /// Adds multiple tickets for a booking, linking to passengers and flight instances.
        /// </summary>
        /// <param name="tickets">List of tickets.</param>
        /// <param name="bookingId">The booking ID.</param>
        /// <returns>Task completion.</returns>
        Task AddMultipleAsync(IEnumerable<Ticket> tickets, int bookingId);
         
        // Retrieves tickets for a booking, including Passenger details.
        Task<IEnumerable<Ticket>> GetByBookingWithDetailsAsync(int bookingId);
         
        // Retrieves a single ticket by ID with comprehensive related details.
        Task<Ticket?> GetWithFullDetailsAsync(int ticketId);
         
    }
}