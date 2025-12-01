using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for BoardingPass entity, extending the generic repository.
    /// Provides methods for querying and managing boarding passes issued to passengers for specific flights.
    /// Essential for check-in processes, gate management, and passenger tracking.
    /// </summary>
    public interface IBoardingPassRepository : IGenericRepository<BoardingPass>
    {
        /// <summary>
        /// Retrieves an active boarding pass by its unique ID.
        /// </summary>
        /// <param name="passId">The primary key ID of the boarding pass.</param>
        /// <returns>The BoardingPass entity if found and active; otherwise, null.</returns>
        Task<BoardingPass?> GetActiveByIdAsync(int passId);

        /// <summary>
        /// Retrieves the active boarding pass associated with a specific BookingPassenger entry (unique passenger on a booking).
        /// Includes details like Seat and Passenger.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>The active BoardingPass entity if found; otherwise, null.</returns>
        Task<BoardingPass?> GetByBookingPassengerAsync(int bookingId, int passengerId);

        /// <summary>
        /// Retrieves all active boarding passes issued for a specific booking.
        /// Includes Passenger and Seat details.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of active BoardingPass entities for the booking.</returns>
        Task<IEnumerable<BoardingPass>> GetByBookingAsync(int bookingId);

        /// <summary>
        /// Retrieves all active boarding passes for a specific flight instance.
        /// Crucial for generating gate lists or manifests in the airport management system.
        /// Includes Passenger and Seat details.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <returns>An enumerable collection of active BoardingPass entities for the flight.</returns>
        Task<IEnumerable<BoardingPass>> GetByFlightInstanceAsync(int flightInstanceId);

        /// <summary>
        /// Retrieves the active boarding pass associated with a specific seat on a specific flight instance.
        /// Useful for gate agents checking seat assignments.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <param name="seatId">The ID of the seat.</param>
        /// <returns>The active BoardingPass entity assigned to the seat on that flight, if found; otherwise, null.</returns>
        Task<BoardingPass?> GetByFlightAndSeatAsync(int flightInstanceId, string seatId);

        /// <summary>
        /// Retrieves all boarding passes, including those marked as soft-deleted.
        /// For administrative review or historical data lookup.
        /// </summary>
        /// <returns>An enumerable collection of all BoardingPass entities.</returns>
        Task<IEnumerable<BoardingPass>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) boarding passes.
        /// </summary>
        /// <returns>An enumerable collection of active BoardingPass entities.</returns>
        Task<IEnumerable<BoardingPass>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a boarding pass has already been issued for a specific passenger on a booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>True if an active boarding pass exists; otherwise, false.</returns>
        Task<bool> ExistsForBookingPassengerAsync(int bookingId, int passengerId);

        /// <summary>
        /// Updates boarding details like boarding time or precheck status.
        /// Used during the check-in or boarding process.
        /// </summary>
        /// <param name="passId">The ID of the boarding pass.</param>
        /// <param name="boardingTime">The updated boarding time (optional).</param>
        /// <param name="precheckStatus">The updated precheck status (optional).</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateBoardingDetailsAsync(int passId, DateTime? boardingTime, bool? precheckStatus);
    }
}