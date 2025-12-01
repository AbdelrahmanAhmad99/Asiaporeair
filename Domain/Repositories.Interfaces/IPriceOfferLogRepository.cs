using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for PriceOfferLog entity, extending the generic repository.
    /// Provides methods for querying historical price quotes offered for fares and ancillaries,
    /// based on contextual attributes. Useful for pricing analysis and potentially machine learning.
    /// </summary>
    public interface IPriceOfferLogRepository : IGenericRepository<PriceOfferLog>
    {
        /// <summary>
        /// Retrieves an active price offer log entry by its unique ID.
        /// </summary>
        /// <param name="offerId">The primary key ID of the offer log.</param>
        /// <returns>The PriceOfferLog entity if found and active; otherwise, null.</returns>
        Task<PriceOfferLog?> GetActiveByIdAsync(int offerId);

        /// <summary>
        /// Retrieves active price offer logs recorded within a specific timestamp range.
        /// Includes related FareBasisCode, AncillaryProduct, and ContextualPricingAttributes details.
        /// </summary>
        /// <param name="startDate">The start timestamp of the range.</param>
        /// <param name="endDate">The end timestamp of the range.</param>
        /// <returns>An enumerable collection of active PriceOfferLog entities within the range.</returns>
        Task<IEnumerable<PriceOfferLog>> GetByTimestampRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves active price offer logs associated with a specific fare basis code.
        /// </summary>
        /// <param name="fareCode">The fare basis code.</param>
        /// <returns>An enumerable collection of active PriceOfferLog entities for the fare code.</returns>
        Task<IEnumerable<PriceOfferLog>> GetByFareCodeAsync(string fareCode);

        /// <summary>
        /// Retrieves active price offer logs associated with a specific ancillary product ID.
        /// </summary>
        /// <param name="ancillaryProductId">The ID of the ancillary product.</param>
        /// <returns>An enumerable collection of active PriceOfferLog entities for the ancillary product.</returns>
        Task<IEnumerable<PriceOfferLog>> GetByAncillaryProductAsync(int ancillaryProductId);

        /// <summary>
        /// Retrieves active price offer logs associated with a specific set of contextual pricing attributes.
        /// </summary>
        /// <param name="contextAttributeId">The ID of the ContextualPricingAttributes entry.</param>
        /// <returns>An enumerable collection of active PriceOfferLog entities linked to the context attributes.</returns>
        Task<IEnumerable<PriceOfferLog>> GetByContextAttributesAsync(int contextAttributeId);

        /// <summary>
        /// Retrieves all price offer logs, including those marked as soft-deleted.
        /// For administrative review or historical data analysis.
        /// </summary>
        /// <returns>An enumerable collection of all PriceOfferLog entities.</returns>
        Task<IEnumerable<PriceOfferLog>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) price offer logs.
        /// </summary>
        /// <returns>An enumerable collection of active PriceOfferLog entities.</returns>
        Task<IEnumerable<PriceOfferLog>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves the most recent active price offer log for a specific fare and context.
        /// </summary>
        /// <param name="fareCode">The fare basis code.</param>
        /// <param name="contextAttributeId">The ID of the context attributes.</param>
        /// <returns>The most recent active PriceOfferLog entity matching the criteria; otherwise, null.</returns>
        Task<PriceOfferLog?> GetLatestForFareAndContextAsync(string fareCode, int contextAttributeId);

        /// <summary>
        /// Retrieves the most recent active price offer log for a specific ancillary product and context.
        /// </summary>
        /// <param name="ancillaryProductId">The ID of the ancillary product.</param>
        /// <param name="contextAttributeId">The ID of the context attributes.</param>
        /// <returns>The most recent active PriceOfferLog entity matching the criteria; otherwise, null.</returns>
        Task<PriceOfferLog?> GetLatestForAncillaryAndContextAsync(int ancillaryProductId, int contextAttributeId);

 
        // Retrieves a paginated list based on filter criteria using primitive types and Expression.
        Task<(IEnumerable<PriceOfferLog> Items, int TotalCount)> GetPaginatedLogsAsync(
            Expression<Func<PriceOfferLog, bool>> filter, // Pass a filter expression instead of DTO
            int pageNumber,
            int pageSize);

        // Calculates pricing analytics for a fare code. Returns anonymous type or tuple (no DTO).
        Task<(decimal AveragePrice, decimal MinPrice, decimal MaxPrice, int OfferCount)?> GetPricingAnalyticsForFareAsync(
            string fareCode,
            DateTime startDate,
            DateTime endDate);

        // Calculates pricing analytics for an ancillary product. Returns anonymous type or tuple (no DTO).
        Task<(decimal AveragePrice, decimal MinPrice, decimal MaxPrice, int OfferCount)?> GetPricingAnalyticsForAncillaryAsync(
            int ancillaryProductId,
            DateTime startDate,
            DateTime endDate);
         

    }
}