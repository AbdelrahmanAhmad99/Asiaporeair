using Application.DTOs.BoardingPass;
using Application.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for managing boarding pass generation and boarding process.
    public interface IBoardingPassService
    {
        // Generates a boarding pass for a specific passenger on a booking (Check-In process).
        Task<ServiceResult<BoardingPassDto>> GenerateBoardingPassAsync(GenerateBoardingPassRequestDto request, ClaimsPrincipal user);

        // Retrieves a boarding pass by its unique ID. Requires authorization.
        Task<ServiceResult<BoardingPassDto>> GetBoardingPassByIdAsync(int passId, ClaimsPrincipal user);

        // Retrieves the boarding pass for a specific passenger on a specific booking. Requires authorization.
        Task<ServiceResult<BoardingPassDto>> GetBoardingPassByBookingPassengerAsync(int bookingId, int passengerId, ClaimsPrincipal user);

        // Retrieves all boarding passes for a specific flight instance (Gate List/Manifest).
        Task<ServiceResult<IEnumerable<BoardingPassDto>>> GetBoardingPassesForFlightAsync(int flightInstanceId);

        // Simulates scanning a boarding pass at the gate and updates the ticket status to 'Boarded'.
        Task<ServiceResult> ScanBoardingPassAtGateAsync(GateScanRequestDto scanRequest, ClaimsPrincipal gateAgentUser);

        // Performs a paginated search for boarding passes (Admin/Support).
        Task<ServiceResult<PaginatedResult<BoardingPassDto>>> SearchBoardingPassesAsync(BoardingPassFilterDto filter, int pageNumber, int pageSize);

        // Soft-deletes a boarding pass (e.g., if check-in is cancelled). Requires authorization.
        Task<ServiceResult> VoidBoardingPassAsync(int passId, ClaimsPrincipal performingUser);
    }
}