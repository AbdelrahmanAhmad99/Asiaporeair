using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Passenger entity, extending the generic repository.
    /// Provides methods for managing passenger data associated with bookings.
    /// Supports multi-passenger scenarios as required by the booking flow.
    /// </summary>
    public interface IPassengerRepository : IGenericRepository<Passenger>
    {
        /// <summary>
        /// Retrieves an active passenger by their unique ID.
        /// </summary>
        /// <param name="passengerId">The primary key ID of the passenger.</param>
        /// <returns>The Passenger entity if found and active; otherwise, null.</returns>
        Task<Passenger?> GetActiveByIdAsync(int passengerId);

        /// <summary>
        /// Adds multiple *new* Passenger entities to the database.
        /// Note: Linking these passengers to a booking happens via BookingPassengerRepository.
        /// This method is primarily for creating passenger records if they don't exist.
        /// </summary>
        /// <param name="passengers">List of new passenger entities.</param>
        /// <returns>Task representing the async operation.</returns>
        Task AddMultiplePassengersAsync(IEnumerable<Passenger> passengers); // Modified from original to clarify purpose

        /// <summary>
        /// Retrieves all active passengers associated with a specific booking ID.
        /// Includes details about the User (if linked).
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of active Passenger entities for the booking.</returns>
        Task<IEnumerable<Passenger>> GetByBookingAsync(int bookingId); // Existing method retained

        /// <summary>
        /// Retrieves an active passenger by their ID, including details like linked User and FrequentFlyer account.
        /// </summary>
        /// <param name="id">Passenger ID (int, matches DB schema).</param>
        /// <returns>Passenger entity with details if found and active; otherwise, null.</returns>
        Task<Passenger?> GetWithDetailsAsync(int id); // Existing method retained and validated

        /// <summary>
        /// Retrieves all active passengers associated with a specific User ID (the user who booked or the passenger's linked account).
        /// </summary>
        /// <param name="userId">The ID of the User entity.</param>
        /// <returns>An enumerable collection of active Passenger entities linked to the user.</returns>
        Task<IEnumerable<Passenger>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Finds active passengers matching a specific passport number (case-insensitive).
        /// Useful for identity verification in the management system.
        /// </summary>
        /// <param name="passportNumber">The passport number to search for.</param>
        /// <returns>An enumerable collection of active Passenger entities matching the passport number.</returns>
        Task<IEnumerable<Passenger>> FindByPassportAsync(string passportNumber);

        /// <summary>
        /// Finds active passengers matching first and/or last names (case-insensitive, partial match).
        /// Useful for searching in the management system.
        /// </summary>
        /// <param name="firstName">Partial or full first name (optional).</param>
        /// <param name="lastName">Partial or full last name (optional).</param>
        /// <returns>An enumerable collection of matching active Passenger entities.</returns>
        Task<IEnumerable<Passenger>> FindByNameAsync(string? firstName = null, string? lastName = null);

        /// <summary>
        /// Retrieves all passengers, including those marked as soft-deleted.
        /// For administrative review or data history.
        /// </summary>
        /// <returns>An enumerable collection of all Passenger entities.</returns>
        Task<IEnumerable<Passenger>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) passengers.
        /// </summary>
        /// <returns>An enumerable collection of active Passenger entities.</returns>
        Task<IEnumerable<Passenger>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a passenger with the specified passport number exists (active or soft-deleted).
        /// </summary>
        /// <param name="passportNumber">The passport number to check.</param>
        /// <returns>True if a passenger with the passport number exists; otherwise, false.</returns>
        Task<bool> ExistsByPassportAsync(string passportNumber);

        /// <summary>
        /// Adds multiple passengers to a booking, linking to AspNetUsers if registered.
        /// </summary>
        /// <param name="passengers">List of passengers.</param>
        /// <param name="bookingId">The booking ID.</param>
        /// <returns>Task completion.</returns>
        Task AddMultipleAsync(IEnumerable<Passenger> passengers, int bookingId);
         
    }
} 