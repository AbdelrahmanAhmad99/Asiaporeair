using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks; 
using Application.DTOs.Seat;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface ISeatService
    {

        /// <summary>
        /// One or more seats are reserved for passengers on a specific booking.
        /// </summary>
        /// <param name="bookingId">Booking ID.</param>
        /// <param name="reservesDto">List of seat reservation details.</param>
        /// <returns>Result of the operation with a list of the reserved seats.</returns>
        Task<ServiceResult<List<SeatDto>>> ReserveSeatsAsync(int bookingId, List<ReserveSeatDto> reservesDto);

        // Retrieves the seat map for a specific flight instance, showing available and reserved seats.
        Task<ServiceResult<SeatMapDto>> GetSeatMapForFlightAsync(int flightInstanceId);

        // Retrieves only the available seats for a flight instance, optionally filtered by cabin class.
        Task<ServiceResult<IEnumerable<SeatDto>>> GetAvailableSeatsAsync(SeatAvailabilityRequestDto request);

        // Assigns a specific seat to a passenger within a booking.
        Task<ServiceResult> AssignSeatAsync(AssignSeatRequestDto request, ClaimsPrincipal user);

        // Removes the seat assignment for a specific passenger within a booking.
        Task<ServiceResult> RemoveSeatAssignmentAsync(int bookingId, int passengerId, ClaimsPrincipal user);

        // Retrieves all current seat assignments for a given booking.
        Task<ServiceResult<IEnumerable<SeatAssignmentDto>>> GetSeatAssignmentsForBookingAsync(int bookingId, ClaimsPrincipal user);
    
        
    
    }
}