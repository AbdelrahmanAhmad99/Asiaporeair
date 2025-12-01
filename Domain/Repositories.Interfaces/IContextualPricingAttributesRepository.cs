 
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for ContextualPricingAttributes entity, extending the generic repository.
    /// Provides methods for querying attributes used in dynamic pricing calculations.
    /// </summary>
    public interface IContextualPricingAttributesRepository : IGenericRepository<ContextualPricingAttributes>
    {

        /// <summary>
        /// Retrieves pricing attributes for dynamic calculation.
        /// </summary>
        /// <param name="attributeId">Attribute ID.</param>
        /// <returns>Pricing attributes.</returns>
        Task<ContextualPricingAttributes?> GetForPricingAsync(int attributeId);
        /// <summary>
        /// Retrieves active pricing attributes by its unique ID.
        /// </summary>
        /// <param name="attributeId">The primary key ID.</param>
        /// <returns>The ContextualPricingAttributes entity if found and active; otherwise, null.</returns>
        Task<ContextualPricingAttributes?> GetActiveByIdAsync(int attributeId);

        /// <summary>
        /// Retrieves active pricing attributes relevant for a given time until departure.
        /// Finds the attribute set that best matches the provided days.
        /// </summary>
        /// <param name="daysUntilDeparture">The number of days remaining until the flight's departure.</param>
        /// <returns>The most relevant active ContextualPricingAttributes entity; otherwise, null.</returns>
        Task<ContextualPricingAttributes?> GetByTimeUntilDepartureAsync(int daysUntilDeparture);

        /// <summary>
        /// Retrieves active pricing attributes relevant for a given length of stay.
        /// Finds the attribute set that best matches the provided duration.
        /// </summary>
        /// <param name="lengthOfStayDays">The duration of the stay in days.</param>
        /// <returns>The most relevant active ContextualPricingAttributes entity; otherwise, null.</returns>
        Task<ContextualPricingAttributes?> GetByLengthOfStayAsync(int lengthOfStayDays);

        /// <summary>
        /// Retrieves all contextual pricing attribute sets, including those marked as soft-deleted.
        /// For administrative review or historical analysis.
        /// </summary>
        /// <returns>An enumerable collection of all ContextualPricingAttributes entities.</returns>
        Task<IEnumerable<ContextualPricingAttributes>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) contextual pricing attribute sets.
        /// </summary>
        /// <returns>An enumerable collection of active ContextualPricingAttributes entities.</returns>
        Task<IEnumerable<ContextualPricingAttributes>> GetAllActiveAsync();

        /// <summary>
        /// Checks if pricing attributes with the specified ID exist (active or soft-deleted).
        /// </summary>
        /// <param name="attributeId">The ID to check.</param>
        /// <returns>True if attributes with the ID exist; otherwise, false.</returns>
        Task<bool> ExistsByIdAsync(int attributeId); 
    }
}