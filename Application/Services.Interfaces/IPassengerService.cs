using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks; 
using Application.DTOs.Passenger;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IPassengerService
    {
        /// <summary>
        /// Adds one or more passengers to a specific booking.
        /// </summary>
        /// <param name="passengersDto">List of passenger details.</param>
        /// <param name="bookingId">Booking ID.</param>
        /// <returns>Result of the operation with a list of passengers added.</returns>
        Task<ServiceResult<List<PassengerDto>>> AddMultiplePassengersAsync(List<CreatePassengerDto> passengersDto, int bookingId);

        /// <summary>
        /// Retrieves the list of passengers associated with a specific booking.
        /// </summary>
        /// <param name="bookingId">Booking ID.</param>
        /// <returns>Result of the operation with the passenger list.</returns>
        Task<ServiceResult<IEnumerable<PassengerDto>>> GetPassengersByBookingAsync(int bookingId);

        /// <summary>
        /// Updates existing passenger details.
        /// </summary>
        /// <param name="passengerId">Passenger ID.</param>
        /// <param name="updateDto">Updated passenger data.</param>
        /// <returns>Result of operation with updated passenger data.</returns>
        Task<ServiceResult<PassengerDto>> UpdatePassengerAsync(string passengerId, UpdatePassengerDto updateDto);

        // Creates or finds existing passenger profiles and links them to a booking.
        Task<ServiceResult<List<PassengerDto>>> CreateOrUpdatePassengersForBookingAsync(List<CreatePassengerDto> passengersDto, int bookingId, int bookingUserId);

        // Retrieves details for a specific passenger by their ID. Requires authorization.
        Task<ServiceResult<PassengerDto>> GetPassengerByIdAsync(int passengerId, ClaimsPrincipal user);

        // Updates details of an existing passenger profile. Requires authorization.
        Task<ServiceResult<PassengerDto>> UpdatePassengerAsync(int passengerId, UpdatePassengerDto updateDto, ClaimsPrincipal user);

        // Retrieves all passenger profiles associated with the currently logged-in user.
        Task<ServiceResult<IEnumerable<PassengerDto>>> GetMyPassengersAsync(ClaimsPrincipal user);

        // Retrieves all passenger profiles associated with a specific user ID (for admins).
        Task<ServiceResult<IEnumerable<PassengerDto>>> GetPassengersByUserIdAsync(int userId);

        // Performs a paginated search for passenger profiles (admin/support use).
        Task<ServiceResult<PaginatedResult<PassengerDto>>> SearchPassengersAsync(PassengerFilterDto filter, int pageNumber, int pageSize);

        // Soft-deletes a passenger profile. Requires authorization and dependency checks.
        Task<ServiceResult> DeletePassengerAsync(int passengerId, ClaimsPrincipal user);




    }
}