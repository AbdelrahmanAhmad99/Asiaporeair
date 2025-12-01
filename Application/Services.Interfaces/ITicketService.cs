using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks; 
using Application.DTOs.Ticket;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface ITicketService
    {
        
        // Generates tickets for all passengers in a confirmed booking.
        Task<ServiceResult<List<TicketDto>>> GenerateTicketsForBookingAsync(int bookingId);

        // Retrieves a specific ticket by its ID with detailed information. Requires authorization.
        Task<ServiceResult<TicketDetailDto>> GetTicketDetailsByIdAsync(int ticketId, ClaimsPrincipal user);

        // Retrieves a specific ticket by its unique code with detailed information. Requires authorization.
        Task<ServiceResult<TicketDetailDto>> GetTicketDetailsByCodeAsync(string ticketCode, ClaimsPrincipal user);

        // Retrieves all tickets associated with a specific booking. Requires authorization.
        Task<ServiceResult<IEnumerable<TicketDto>>> GetTicketsByBookingAsync(int bookingId, ClaimsPrincipal user);

        // Retrieves all tickets for the currently authenticated user (paginated).
        Task<ServiceResult<PaginatedResult<TicketDto>>> GetMyTicketsAsync(ClaimsPrincipal user, int pageNumber, int pageSize);

        // Updates the status of a specific ticket (e.g., CheckedIn, Boarded, Cancelled). Requires authorization.
        Task<ServiceResult> UpdateTicketStatusAsync(int ticketId, UpdateTicketStatusDto statusDto, ClaimsPrincipal performingUser);

        // Voids/Cancels a specific ticket. Requires authorization.
        Task<ServiceResult> VoidTicketAsync(int ticketId, string reason, ClaimsPrincipal performingUser);

        // Performs a paginated search for tickets (Admin/Support).
        Task<ServiceResult<PaginatedResult<TicketDto>>> SearchTicketsAsync(TicketFilterDto filter, int pageNumber, int pageSize);

        // (Kept from previous - might be replaced by GetMyTicketsAsync)
        Task<ServiceResult<IEnumerable<TicketDto>>> GetUserTicketsAsync(string userId);
    }
}