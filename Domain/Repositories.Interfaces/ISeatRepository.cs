using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    public interface ISeatRepository : IGenericRepository<Seat>
    {
        /// <summary>
        /// Retrieves a Seat entity including its CabinClass details.
        /// </summary>
        /// <param name="seatId">The ID of the seat.</param>
        /// <returns>Seat entity with CabinClass loaded.</returns>
        Task<Seat?> GetWithCabinClassAsync(string seatId);
        /// <summary>
        /// Retrieves available seats for a flight instance and cabin class.
        /// </summary>
        /// <param name="flightInstanceId">Flight instance ID.</param>
        /// <param name="cabinClassId">Cabin class ID (from implied cabin_class table).</param>
        /// <returns>List of available seats.</returns>
        Task<IEnumerable<Seat>> GetAvailableAsync(int flightInstanceId, int cabinClassId);

        /// <summary>
        /// Reserves multiple seats for a booking, linking to tickets/passengers.
        /// </summary>
        /// <param name="seatIds">List of seat IDs.</param>
        /// <param name="bookingId">The booking ID.</param>
        /// <returns>Task completion.</returns>
        Task ReserveMultipleAsync(IEnumerable<string> seatIds, int bookingId);

        /// <summary>
        /// Retrieves seats by booking ID.
        /// </summary>
        /// <param name="bookingId">Booking ID.</param>
        /// <returns>List of seats.</returns>
        Task<IEnumerable<Seat>> GetByBookingAsync(int bookingId);
     
        /// <summary>
        /// Retrieves all seats (including reserved) for a specific aircraft, grouped by cabin class.
        /// Useful for displaying seat maps.
        /// </summary>
        /// <param name="aircraftTailNumber">The tail number of the aircraft.</param>
        /// <returns>An enumerable collection of Seat entities for the specified aircraft.</returns>
        Task<IEnumerable<Seat>> GetSeatsByAircraftAsync(string aircraftTailNumber);
  
        /// <summary>
        /// Retrieves all seats associated with a specific aircraft configuration.
        /// Useful for the airport management system when defining or viewing layouts.
        /// </summary>
        /// <param name="configId">The ID of the aircraft configuration.</param>
        /// <returns>An enumerable collection of Seat entities for the configuration.</returns>
        Task<IEnumerable<Seat>> GetByAircraftConfigAsync(int configId); // Added for management

        /// <summary>
        /// Retrieves all seats, including those marked as soft-deleted.
        /// For administrative review or auditing.
        /// </summary>
        /// <returns>An enumerable collection of all Seat entities.</returns>
        Task<IEnumerable<Seat>> GetAllIncludingDeletedAsync(); // Added for management

        /// <summary>
        /// Retrieves all active (not soft-deleted) seats.
        /// </summary>
        /// <returns>An enumerable collection of active Seat entities.</returns>
        Task<IEnumerable<Seat>> GetAllActiveAsync(); // Added for clarity

        /// <summary>
        /// Checks if a specific seat ID exists (active or soft-deleted).
        /// </summary>
        /// <param name="seatId">The seat ID to check.</param>
        /// <returns>True if the seat exists; otherwise, false.</returns>
        Task<bool> ExistsByIdAsync(string seatId); // Added for validation
 
        // Retrieves available seats for a specific flight and optional cabin class ID.
        Task<IEnumerable<Seat>> GetAvailableSeatsForFlightAsync(int flightInstanceId, string aircraftId, IEnumerable<string> reservedSeatIds, int? cabinClassId = null);

         
        Task<IEnumerable<Seat>> GetByCabinClassAsync(int cabinClassId);



    }
}