using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{ 
    public interface IFrequentFlyerRepository : IGenericRepository<FrequentFlyer>
    { 

        /// <summary>
        /// Updates points from a booking, calculating based on fare and distance.
        /// </summary>
        /// <param name="bookingId">Booking ID.</param>
        /// <returns>Updated balance.</returns>
        Task<int> UpdatePointsFromBookingAsync(int bookingId, int pointsDelta);

        /// <summary>
        /// Retrieves frequent flyer by card number or user ID.
        /// </summary>
        /// <param name="cardNumber">Card number.</param>
        /// <returns>Frequent flyer entity.</returns>
        Task<FrequentFlyer?> GetByCardNumberAsync(string cardNumber);
        /// <summary>
        /// Retrieves an active frequent flyer account by its unique ID.
        /// </summary>
        /// <param name="flyerId">The primary key ID of the frequent flyer account.</param>
        /// <returns>The FrequentFlyer entity if found and active; otherwise, null.</returns>
        Task<FrequentFlyer?> GetActiveByIdAsync(int flyerId); 

        /// <summary>
        /// Retrieves the active frequent flyer account associated with a specific User ID.
        /// </summary>
        /// <param name="userId">The ID of the User entity (not AppUser ID).</param>
        /// <returns>The associated active FrequentFlyer entity if found; otherwise, null.</returns>
        Task<FrequentFlyer?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Retrieves all active frequent flyer accounts belonging to a specific tier/level (case-insensitive).
        /// Useful for targeted promotions or reports in the management system.
        /// </summary>
        /// <param name="level">The loyalty level/tier name (e.g., "KrisFlyer", "Elite Silver").</param>
        /// <returns>An enumerable collection of active FrequentFlyer entities matching the level.</returns>
        Task<IEnumerable<FrequentFlyer>> FindByLevelAsync(string level);

        /// <summary>
        /// Updates the award points for a frequent flyer account, typically after a flight is completed.
        /// Note: The calculation logic for pointsDelta ideally resides in the service layer.
        /// </summary>
        /// <param name="flyerId">The ID of the frequent flyer account.</param>
        /// <param name="pointsDelta">The number of points to add (can be negative for redemption).</param>
        /// <returns>The new total points balance if successful; otherwise, null.</returns>
        Task<int?> UpdatePointsAsync(int flyerId, int pointsDelta); 

        /// <summary>
        /// Retrieves all frequent flyer accounts, including those marked as soft-deleted.
        /// For administrative review or data auditing.
        /// </summary>
        /// <returns>An enumerable collection of all FrequentFlyer entities.</returns>
        Task<IEnumerable<FrequentFlyer>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) frequent flyer accounts.
        /// </summary>
        /// <returns>An enumerable collection of active FrequentFlyer entities.</returns>
        Task<IEnumerable<FrequentFlyer>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a frequent flyer account with the specified card number exists (active or soft-deleted).
        /// </summary>
        /// <param name="cardNumber">The card number to check.</param>
        /// <returns>True if an account with the card number exists; otherwise, false.</returns>
        Task<bool> ExistsByCardNumberAsync(string cardNumber);

        /// <summary>
        /// Retrieves frequent flyer accounts with points above a certain threshold.
        /// Useful for identifying high-value members in the management system.
        /// </summary>
        /// <param name="pointsThreshold">The minimum points required.</param>
        /// <returns>An enumerable collection of active FrequentFlyer entities meeting the threshold.</returns>
        Task<IEnumerable<FrequentFlyer>> GetMembersWithHighPointsAsync(int pointsThreshold);
    }
}
