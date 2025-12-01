using Application.DTOs.PriceOfferLog;
using Application.Models; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing and analyzing Price Offer Logs.
    /// Used by the pricing engine to log quotes and by the management system for analytics.
    /// </summary>
    public interface IPriceOfferLogService
    {
        // Logs a new price offer (typically called by PricingService).
        Task<ServiceResult<PriceOfferLogDto>> LogPriceOfferAsync(CreatePriceOfferLogDto createDto);

        // Retrieves a single active price offer log by its ID.
        Task<ServiceResult<PriceOfferLogDto>> GetLogByIdAsync(int offerId);

        // Retrieves a paginated list of price offer logs based on advanced filters.
        Task<ServiceResult<PaginatedResult<PriceOfferLogDto>>> SearchLogsAsync(PriceOfferLogFilterDto filter, int pageNumber, int pageSize);

        // Retrieves pricing analytics (Avg, Min, Max, Count) for a specific fare code within a date range.
        Task<ServiceResult<PriceAnalyticsDto>> GetAnalyticsForFareAsync(string fareCode, DateTime startDate, DateTime endDate);

        // Retrieves pricing analytics (Avg, Min, Max, Count) for a specific ancillary product within a date range.
        Task<ServiceResult<PriceAnalyticsDto>> GetAnalyticsForAncillaryAsync(int ancillaryProductId, DateTime startDate, DateTime endDate);

        // Soft deletes a price offer log entry (Admin).
        Task<ServiceResult> DeleteLogAsync(int offerId);

        // Reactivates a soft-deleted price offer log entry (Admin).
        Task<ServiceResult> ReactivateLogAsync(int offerId);
       
    }
}