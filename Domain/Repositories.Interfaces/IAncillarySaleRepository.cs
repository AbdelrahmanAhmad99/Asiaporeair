using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for AncillarySale entity, extending the generic repository.
    /// Provides methods for querying sales of ancillary products linked to bookings.
    /// Useful for calculating booking totals, generating receipts, and sales reporting.
    /// </summary>
    public interface IAncillarySaleRepository : IGenericRepository<AncillarySale>
    {
        /// <summary>
        /// Retrieves an active ancillary sale by its unique ID.
        /// </summary>
        /// <param name="saleId">The primary key ID of the sale.</param>
        /// <returns>The AncillarySale entity if found and active; otherwise, null.</returns>
        Task<AncillarySale?> GetActiveByIdAsync(int saleId);

        /// <summary>
        /// Retrieves all active ancillary sales associated with a specific booking ID.
        /// Includes details of the purchased AncillaryProduct.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of active AncillarySale entities with product details for the booking.</returns>
        Task<IEnumerable<AncillarySale>> GetByBookingAsync(int bookingId);  

        /// <summary>
        /// Retrieves all active ancillary sales for a specific product ID.
        /// Useful for sales analysis in the management system.
        /// </summary>
        /// <param name="productId">The ID of the ancillary product.</param>
        /// <returns>An enumerable collection of active AncillarySale entities for the product.</returns>
        Task<IEnumerable<AncillarySale>> GetByProductAsync(int productId);

        /// <summary>
        /// Calculates the total revenue from active ancillary sales for a specific booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>The total calculated price paid for ancillaries in the booking.</returns>
        Task<decimal> GetTotalRevenueForBookingAsync(int bookingId);

        /// <summary>
        /// Calculates the total revenue from active ancillary sales for a specific flight instance.
        /// Useful for analysing ancillary revenue per flight in the management system.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <returns>The total calculated price paid for ancillaries across all bookings on the flight.</returns>
        Task<decimal> GetTotalRevenueForFlightInstanceAsync(int flightInstanceId);

        /// <summary>
        /// Retrieves all ancillary sales, including those marked as soft-deleted.
        /// For administrative review or auditing.
        /// </summary>
        /// <returns>An enumerable collection of all AncillarySale entities.</returns>
        Task<IEnumerable<AncillarySale>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) ancillary sales.
        /// </summary>
        /// <returns>An enumerable collection of active AncillarySale entities.</returns>
        Task<IEnumerable<AncillarySale>> GetAllActiveAsync();
       
    }
}
 
