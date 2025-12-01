using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for AncillaryProduct entity, extending the generic repository.
    /// Provides methods for querying ancillary products like meals, baggage, Wi-Fi, etc.
    /// </summary>
    public interface IAncillaryProductRepository : IGenericRepository<AncillaryProduct>
    {
        /// <summary>
        /// Retrieves an active ancillary product by its unique ID.
        /// </summary>
        /// <param name="productId">The primary key ID of the product.</param>
        /// <returns>The AncillaryProduct entity if found and active; otherwise, null.</returns>
        Task<AncillaryProduct?> GetActiveByIdAsync(int productId);

        /// <summary>
        /// Retrieves all available active ancillary products.
        /// Filters out products marked as IsDeleted.
        /// </summary>
        /// <returns>An enumerable collection of active AncillaryProduct entities.</returns>
        Task<IEnumerable<AncillaryProduct>> GetAvailableAsync(); // Existing method

        /// <summary>
        /// Retrieves active ancillary products matching a specific category (case-insensitive).
        /// Useful for filtering options in the UI (e.g., show only 'Meals').
        /// </summary>
        /// <param name="category">The product category name.</param>
        /// <returns>An enumerable collection of active AncillaryProduct entities in the specified category.</returns>
        Task<IEnumerable<AncillaryProduct>> GetByCategoryAsync(string category);

        /// <summary>
        /// Finds active ancillary products where the name contains the specified text (case-insensitive search).
        /// Useful for searching products in the management system.
        /// </summary>
        /// <param name="nameSubstring">The text to search for within the product name.</param>
        /// <returns>An enumerable collection of matching active AncillaryProduct entities.</returns>
        Task<IEnumerable<AncillaryProduct>> FindByNameAsync(string nameSubstring);

        /// <summary>
        /// Retrieves the specific ancillary products that were sold (linked via AncillarySale) for a given booking.
        /// This method returns the Product entities themselves.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of AncillaryProduct entities sold for the booking.</returns>
        Task<IEnumerable<AncillaryProduct>> GetProductsSoldForBookingAsync(int bookingId);  

        /// <summary>
        /// Retrieves all ancillary products, including those marked as soft-deleted.
        /// For administrative review or historical data lookup.
        /// </summary>
        /// <returns>An enumerable collection of all AncillaryProduct entities.</returns>
        Task<IEnumerable<AncillaryProduct>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) ancillary products.
        /// </summary>
        /// <returns>An enumerable collection of active AncillaryProduct entities.</returns>
        Task<IEnumerable<AncillaryProduct>> GetAllActiveAsync();

        /// <summary>
        /// Checks if an ancillary product with the specified name exists (active or soft-deleted, case-insensitive).
        /// </summary>
        /// <param name="name">The product name to check.</param>
        /// <returns>True if a product with the given name exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Retrieves active ancillary products within a specified price range.
        /// Useful for filtering options for customers or analysis in management.
        /// </summary>
        /// <param name="minCost">Minimum base cost.</param>
        /// <param name="maxCost">Maximum base cost.</param>
        /// <returns>An enumerable collection of active AncillaryProduct entities within the price range.</returns>
        Task<IEnumerable<AncillaryProduct>> GetByPriceRangeAsync(decimal minCost, decimal maxCost);

       
        Task<IEnumerable<AncillaryProduct>> GetByBookingAsync(int bookingId); 
    }
}