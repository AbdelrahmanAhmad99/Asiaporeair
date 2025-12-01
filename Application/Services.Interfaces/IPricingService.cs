using Application.DTOs.Booking;
using Application.Models;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Service for dynamic flight and booking price calculation.
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// Calculates the base price for a single flight instance based on a fare basis code.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <param name="fareBasisCode">The fare basis code (e.g., 'ECO', 'BUS').</param>
        /// <returns>The calculated base price.</returns>
        Task<ServiceResult<decimal>> CalculateBasePriceAsync(int flightInstanceId, string fareBasisCode);

        /// <summary>
        /// Calculates the total price for an entire booking, including all passengers and ancillaries.
        /// </summary>
        /// <param name="bookingDto">The booking data transfer object.</param>
        /// <returns>The total calculated price.</returns>
        Task<ServiceResult<decimal>> CalculateBookingPriceAsync(CreateBookingDto bookingDto); 

        /// <summary>
        /// Logs a price that was quoted to a user for analytics.
        /// (Management System feature)
        /// </summary>
        /// <param name="flightInstanceId">The flight instance ID.</param>
        /// <param name="fareCode">The fare code quoted.</param>
        /// <param name="quotedPrice">The price that was calculated and shown.</param>
        /// <param name="contextId">The ID of the contextual attribute set used for pricing.</param>
        /// <returns>A ServiceResult indicating success or failure of the logging.</returns>
        Task<ServiceResult> LogPriceOfferAsync(int flightInstanceId, string fareCode, decimal quotedPrice, int contextId);

        // Calculates the price for selecting a *specific seat* (e.g., exit row)
        Task<ServiceResult<decimal?>> CalculateSeatPriceAsync(string seatId, int flightInstanceId);

    }
}