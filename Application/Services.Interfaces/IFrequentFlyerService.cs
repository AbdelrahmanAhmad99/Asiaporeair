using System.Security.Claims;
using System.Threading.Tasks; 
using Application.DTOs.FrequentFlyer;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IFrequentFlyerService
    { 

        // Creates a new Frequent Flyer account and links it to an existing User profile.
        Task<ServiceResult<FrequentFlyerDto>> CreateAccountAsync(CreateFrequentFlyerDto dto, ClaimsPrincipal performingUser);

        // Retrieves the Frequent Flyer account details for the currently logged-in user.
        Task<ServiceResult<FrequentFlyerDto>> GetMyAccountAsync(ClaimsPrincipal user);

        // Retrieves Frequent Flyer account details by User ID (admin access).
        Task<ServiceResult<FrequentFlyerDto>> GetAccountByUserIdAsync(int userId);

        // Retrieves Frequent Flyer account details by Card Number.
        Task<ServiceResult<FrequentFlyerDto>> GetAccountByCardNumberAsync(string cardNumber);

        // Calculates and adds points for a completed and confirmed booking.
        Task<ServiceResult<int>> AddPointsForBookingAsync(int bookingId);

        // Manually adds or subtracts points from an account (admin action).
        Task<ServiceResult<FrequentFlyerDto>> ManualAdjustPointsAsync(int flyerId, UpdatePointsDto dto, ClaimsPrincipal performingUser);

        // Updates the tier level of a Frequent Flyer account (admin action or automated process).
        Task<ServiceResult<FrequentFlyerDto>> UpdateLevelAsync(int flyerId, string newLevel, ClaimsPrincipal performingUser);

        // Performs a paginated search for Frequent Flyer accounts (admin use).
        Task<ServiceResult<PaginatedResult<FrequentFlyerDto>>> SearchAccountsAsync(FrequentFlyerFilterDto filter, int pageNumber, int pageSize);

        // Soft-deletes a Frequent Flyer account (admin action).
        Task<ServiceResult> DeleteAccountAsync(int flyerId, ClaimsPrincipal performingUser);

    }
}