using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for BookingPassenger entity, extending the generic repository.
    /// Manages the many-to-many relationship between Bookings and Passengers,
    /// including seat assignments. Essential for managing who is on which booking and where they sit.
    /// </summary>
    public interface IBookingPassengerRepository : IGenericRepository<BookingPassenger>
    {
        /// <summary>
        /// Retrieves an active BookingPassenger link by its composite primary key.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>The BookingPassenger entity if found and active; otherwise, null.</returns>
        Task<BookingPassenger?> GetActiveByIdAsync(int bookingId, int passengerId);

        /// <summary>
        /// Adds multiple BookingPassenger relationships, typically when creating a booking with multiple passengers.
        /// </summary>
        /// <param name="bookingPassengers">An enumerable collection of BookingPassenger entities to add.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddMultipleAsync(IEnumerable<BookingPassenger> bookingPassengers); // Existing method retained

        /// <summary>
        /// Retrieves all active BookingPassenger entries associated with a specific booking ID.
        /// Includes related Passenger and Seat entities.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of active BookingPassenger entities with details for the specified booking.</returns>
        Task<IEnumerable<BookingPassenger>> GetByBookingAsync(int bookingId); // Existing method, enhanced to include details

        /// <summary>
        /// Retrieves all active BookingPassenger entries associated with a specific passenger ID.
        /// Useful for finding all bookings a particular passenger is associated with.
        /// </summary>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>An enumerable collection of active BookingPassenger entities for the specified passenger.</returns>
        Task<IEnumerable<BookingPassenger>> GetByPassengerAsync(int passengerId);

        /// <summary>
        /// Retrieves a specific BookingPassenger entry, including Passenger and Seat details.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>The detailed BookingPassenger entity if found and active; otherwise, null.</returns>
        Task<BookingPassenger?> GetWithDetailsAsync(int bookingId, int passengerId);

        /// <summary>
        /// Updates the seat assignment for a specific passenger within a booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <param name="seatId">The ID of the seat to assign (can be null to unassign).</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateSeatAssignmentAsync(int bookingId, int passengerId, string? seatId);

        /// <summary>
        /// Retrieves all BookingPassenger entries, including those marked as soft-deleted.
        /// For administrative review or data history.
        /// </summary>
        /// <returns>An enumerable collection of all BookingPassenger entities.</returns>
        Task<IEnumerable<BookingPassenger>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) BookingPassenger entries.
        /// </summary>
        /// <returns>An enumerable collection of active BookingPassenger entities.</returns>
        Task<IEnumerable<BookingPassenger>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a specific passenger is already associated with a specific booking (active link only).
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <param name="passengerId">The ID of the passenger.</param>
        /// <returns>True if the passenger is actively linked to the booking; otherwise, false.</returns>
        Task<bool> ExistsAsync(int bookingId, int passengerId);

        /// <summary>
        /// Retrieves the count of active passengers for a specific booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>The number of active passengers associated with the booking.</returns>
        Task<int> GetPassengerCountForBookingAsync(int bookingId);

        // Added: New method signature required by FlightService
        Task<int> GetPassengerCountForFlightAsync(int flightInstanceId);

        // Added: New method signature required by FlightService
        Task<int> GetPassengerCountForCabinAsync(int flightInstanceId, int cabinClassId);

     
        // Retrieves assignments for a specific flight, including SeatAssignment.
        Task<IEnumerable<BookingPassenger>> GetAssignmentsByFlightAsync(int flightInstanceId);

        // Retrieves a specific assignment based on the flight and the assigned seat.
        Task<BookingPassenger?> GetAssignmentByFlightAndSeatAsync(int flightInstanceId, string seatId);

        // Retrieves assignments for a specific booking, including Passenger, SeatAssignment, and CabinClass.
        Task<IEnumerable<BookingPassenger>> GetAssignmentsByBookingWithDetailsAsync(int bookingId);

        // Checks if a specific seat is already assigned to ANY active passenger on a specific flight instance.
        Task<bool> IsSeatAssignedOnFlightAsync(int flightInstanceId, string seatId);

    }
}

