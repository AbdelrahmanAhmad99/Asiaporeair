using Application.DTOs.PriceOfferLog;
using Application.Models; // For ServiceResult & PaginatedResult
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions; // Add this
using System.Threading.Tasks;

namespace Application.Services
{
    public class PriceOfferLogService : IPriceOfferLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PriceOfferLogService> _logger;

        public PriceOfferLogService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PriceOfferLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        
        // Creates a new price offer log entry.
        public async Task<ServiceResult<PriceOfferLogDto>> LogPriceOfferAsync(CreatePriceOfferLogDto createDto)
        {
            _logger.LogInformation("Logging new price offer. FareFk: {FareFk}, AncillaryFk: {AncillaryFk}, Price: {Price}",
                createDto.FareFk, createDto.AncillaryFk, createDto.OfferPriceQuote);

            // Validation: Ensure exactly one of FareFk or AncillaryFk is provided
            if (string.IsNullOrWhiteSpace(createDto.FareFk) && !createDto.AncillaryFk.HasValue)
                return ServiceResult<PriceOfferLogDto>.Failure("Either Fare Code or Ancillary Product ID must be provided.");
            if (!string.IsNullOrWhiteSpace(createDto.FareFk) && createDto.AncillaryFk.HasValue)
                return ServiceResult<PriceOfferLogDto>.Failure("Cannot log a price offer for both a Fare Code and an Ancillary Product simultaneously.");


            try
            {
                // Validate foreign keys (using Exists checks for efficiency)
                if (createDto.FareFk != null && !(await _unitOfWork.FareBasisCodes.ExistsByCodeAsync(createDto.FareFk)))
                    return ServiceResult<PriceOfferLogDto>.Failure($"Fare code '{createDto.FareFk}' does not exist.");

                if (createDto.AncillaryFk.HasValue && !(await _unitOfWork.AncillaryProducts.AnyAsync(p => p.ProductId == createDto.AncillaryFk.Value))) // Use AnyAsync for existence
                    return ServiceResult<PriceOfferLogDto>.Failure($"Ancillary product with ID '{createDto.AncillaryFk}' not found.");

                if (!(await _unitOfWork.ContextualPricingAttributes.ExistsByIdAsync(createDto.ContextAttributesFk))) // Assumes ExistsByIdAsync exists
                    return ServiceResult<PriceOfferLogDto>.Failure($"Context attribute set with ID '{createDto.ContextAttributesFk}' not found.");

                var newLog = _mapper.Map<PriceOfferLog>(createDto);
                newLog.Timestamp = createDto.Timestamp == default ? DateTime.UtcNow : createDto.Timestamp; // Ensure timestamp is set

                await _unitOfWork.PriceOfferLogs.AddAsync(newLog);
                await _unitOfWork.SaveChangesAsync();

                // Manually load related entities for the response DTO
                if (newLog.FareId != null) newLog.Fare = await _unitOfWork.FareBasisCodes.GetByCodeAsync(newLog.FareId);
                if (newLog.AncillaryId != null) newLog.Ancillary = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(newLog.AncillaryId.Value);
                if (newLog.ContextAttributes == null) newLog.ContextAttributes = await _unitOfWork.ContextualPricingAttributes.GetActiveByIdAsync(newLog.ContextAttributesId);


                var dto = _mapper.Map<PriceOfferLogDto>(newLog);
                _logger.LogInformation("Successfully logged price offer ID {OfferId}.", newLog.OfferId);
                return ServiceResult<PriceOfferLogDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging price offer for FareFk: {FareFk}, AncillaryFk: {AncillaryFk}.", createDto.FareFk, createDto.AncillaryFk);
                return ServiceResult<PriceOfferLogDto>.Failure("An unexpected error occurred while logging the price offer.");
            }
        }

        // Retrieves a single active price offer log by its ID.
        public async Task<ServiceResult<PriceOfferLogDto>> GetLogByIdAsync(int offerId)
        {
            _logger.LogDebug("Retrieving Price Offer Log ID {OfferId}.", offerId);
            try
            {
                // Fetch with related entities needed for DTO
                var log = await _unitOfWork.PriceOfferLogs.GetByIdAsync(offerId); // Use generic GetByIdAsync
                if (log == null || log.IsDeleted) // Check IsDeleted
                {
                    _logger.LogWarning("Price offer log with ID {OfferId} not found or inactive.", offerId);
                    return ServiceResult<PriceOfferLogDto>.Failure($"Log entry with ID {offerId} not found or is inactive.");
                }
                // Manually load related entities if GetByIdAsync doesn't include them
                if (log.FareId != null && log.Fare == null) log.Fare = await _unitOfWork.FareBasisCodes.GetByCodeAsync(log.FareId);
                if (log.AncillaryId != null && log.Ancillary == null) log.Ancillary = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(log.AncillaryId.Value);
                if (log.ContextAttributes == null) log.ContextAttributes = await _unitOfWork.ContextualPricingAttributes.GetActiveByIdAsync(log.ContextAttributesId);


                var dto = _mapper.Map<PriceOfferLogDto>(log);
                return ServiceResult<PriceOfferLogDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price offer log with ID {OfferId}.", offerId);
                return ServiceResult<PriceOfferLogDto>.Failure("An error occurred while retrieving the log entry.");
            }
        }
        // --- Updated Method Implementations ---

        // Retrieves a paginated list of price offer logs based on advanced filters.
        public async Task<ServiceResult<PaginatedResult<PriceOfferLogDto>>> SearchLogsAsync(PriceOfferLogFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching price offer logs page {PageNumber}.", pageNumber);
            try
            {
                // Build the filter expression in the service layer
                Expression<Func<PriceOfferLog, bool>> filterExpression = log => (filter.IncludeDeleted || !log.IsDeleted);
                if (filter.StartDate.HasValue) filterExpression = filterExpression.And(log => log.Timestamp >= filter.StartDate.Value);
                if (filter.EndDate.HasValue) filterExpression = filterExpression.And(log => log.Timestamp <= filter.EndDate.Value); // Inclusive end date
                if (!string.IsNullOrWhiteSpace(filter.FareFk)) filterExpression = filterExpression.And(log => log.FareId == filter.FareFk);
                if (filter.AncillaryFk.HasValue) filterExpression = filterExpression.And(log => log.AncillaryId == filter.AncillaryFk.Value);
                if (filter.ContextAttributesFk.HasValue) filterExpression = filterExpression.And(log => log.ContextAttributesId == filter.ContextAttributesFk.Value);
                if (filter.MinPrice.HasValue) filterExpression = filterExpression.And(log => log.OfferPriceQuote >= filter.MinPrice.Value);
                if (filter.MaxPrice.HasValue) filterExpression = filterExpression.And(log => log.OfferPriceQuote <= filter.MaxPrice.Value);

                // Call the repository with the expression
                var (logs, totalCount) = await _unitOfWork.PriceOfferLogs.GetPaginatedLogsAsync(filterExpression, pageNumber, pageSize);

                var dtos = _mapper.Map<List<PriceOfferLogDto>>(logs); // Mapper handles included entities
                var paginatedResult = new PaginatedResult<PriceOfferLogDto>(dtos, totalCount, pageNumber, pageSize);

                _logger.LogInformation("Retrieved {Count} price logs on page {PageNumber}/{TotalPages}.", dtos.Count, pageNumber, paginatedResult.TotalPages);
                return ServiceResult<PaginatedResult<PriceOfferLogDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching price offer logs on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<PriceOfferLogDto>>.Failure("An error occurred during paginated log search.");
            }
        }

        // Retrieves pricing analytics for a specific fare code.
        public async Task<ServiceResult<PriceAnalyticsDto>> GetAnalyticsForFareAsync(string fareCode, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Getting analytics for Fare Code {FareCode} between {StartDate} and {EndDate}.", fareCode, startDate, endDate);
            if (string.IsNullOrWhiteSpace(fareCode))
                return ServiceResult<PriceAnalyticsDto>.Failure("Fare code cannot be empty.");

            try
            {
                // Call repository which returns a tuple
                var statsTuple = await _unitOfWork.PriceOfferLogs.GetPricingAnalyticsForFareAsync(fareCode.ToUpper(), startDate, endDate);
                if (!statsTuple.HasValue)
                {
                    _logger.LogWarning("No pricing analytics found for fare code {FareCode} between {StartDate} and {EndDate}.", fareCode, startDate, endDate);
                    return ServiceResult<PriceAnalyticsDto>.Failure("No data found for the specified fare code and date range.");
                }

                // Map tuple to DTO
                var statsDto = new PriceAnalyticsDto
                {
                    ItemCode = fareCode.ToUpper(),
                    AveragePrice = statsTuple.Value.AveragePrice,
                    MinPrice = statsTuple.Value.MinPrice,
                    MaxPrice = statsTuple.Value.MaxPrice,
                    OfferCount = statsTuple.Value.OfferCount
                };

                return ServiceResult<PriceAnalyticsDto>.Success(statsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for fare code {FareCode}.", fareCode);
                return ServiceResult<PriceAnalyticsDto>.Failure("An error occurred while generating pricing analytics.");
            }
        }

        // Retrieves pricing analytics for a specific ancillary product.
        public async Task<ServiceResult<PriceAnalyticsDto>> GetAnalyticsForAncillaryAsync(int ancillaryProductId, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Getting analytics for Ancillary Product ID {ProductId} between {StartDate} and {EndDate}.", ancillaryProductId, startDate, endDate);
            try
            {
                // Call repository which returns a tuple
                var statsTuple = await _unitOfWork.PriceOfferLogs.GetPricingAnalyticsForAncillaryAsync(ancillaryProductId, startDate, endDate);
                if (!statsTuple.HasValue)
                {
                    _logger.LogWarning("No pricing analytics found for ancillary product ID {ProductId} between {StartDate} and {EndDate}.", ancillaryProductId, startDate, endDate);
                    return ServiceResult<PriceAnalyticsDto>.Failure("No data found for the specified ancillary product and date range.");
                }

                // Map tuple to DTO
                var statsDto = new PriceAnalyticsDto
                {
                    ItemCode = ancillaryProductId.ToString(), // Use ID as the code
                    AveragePrice = statsTuple.Value.AveragePrice,
                    MinPrice = statsTuple.Value.MinPrice,
                    MaxPrice = statsTuple.Value.MaxPrice,
                    OfferCount = statsTuple.Value.OfferCount
                };

                return ServiceResult<PriceAnalyticsDto>.Success(statsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for ancillary product ID {ProductId}.", ancillaryProductId);
                return ServiceResult<PriceAnalyticsDto>.Failure("An error occurred while generating pricing analytics.");
            }
        }
         
         
        // Soft deletes a price offer log entry.
        public async Task<ServiceResult> DeleteLogAsync(int offerId)
        {
            _logger.LogInformation("Attempting to soft-delete price offer log ID {OfferId}.", offerId);
            // Authorization check? Assume only admin roles can delete logs. Add if needed.

            var log = await _unitOfWork.PriceOfferLogs.GetActiveByIdAsync(offerId); // Fetch only active
            if (log == null)
            {
                _logger.LogWarning("Soft delete failed: Active log entry {OfferId} not found.", offerId);
                return ServiceResult.Failure($"Active log entry with ID {offerId} not found.");
            }

            try
            {
                _unitOfWork.PriceOfferLogs.SoftDelete(log);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted log entry ID {OfferId}.", offerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting log entry ID {OfferId}.", offerId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // Reactivates a soft-deleted price offer log entry.
        public async Task<ServiceResult> ReactivateLogAsync(int offerId)
        {
            _logger.LogInformation("Attempting to reactivate log entry ID {OfferId}.", offerId);
            // Authorization check?

            // Fetch including deleted using Generic GetByIdAsync
            var log = await _unitOfWork.PriceOfferLogs.GetByIdAsync(offerId);
            if (log == null)
            {
                _logger.LogWarning("Reactivation failed: Log entry {OfferId} not found.", offerId);
                return ServiceResult.Failure($"Log entry with ID {offerId} not found.");
            }

            if (!log.IsDeleted)
            {
                _logger.LogWarning("Reactivation failed: Log entry {OfferId} is already active.", offerId);
                return ServiceResult.Failure($"Log entry ID {offerId} is already active.");
            }

            try
            {
                log.IsDeleted = false; // Reactivate
                _unitOfWork.PriceOfferLogs.Update(log);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully reactivated log entry ID {OfferId}.", offerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating log entry ID {OfferId}.", offerId);
                return ServiceResult.Failure("An error occurred during reactivation.");
            }
        }
    }
}