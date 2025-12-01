using Application.DTOs.ContextualPricingAttribute;
using Application.Models; // For ServiceResult & PaginatedResult
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For EF.Functions

namespace Application.Services
{
    // Service implementation for managing Contextual Pricing Attributes.
    public class ContextualPricingService : IContextualPricingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContextualPricingService> _logger;

         
        public ContextualPricingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ContextualPricingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Retrieves a single active pricing attribute set by its ID.
        public async Task<ServiceResult<ContextualPricingAttributeDto>> GetAttributeSetByIdAsync(int attributeId)
        {
            try
            {
                var attributes = await _unitOfWork.ContextualPricingAttributes.GetActiveByIdAsync(attributeId);
                if (attributes == null)
                {
                    _logger.LogWarning("Active pricing attribute set with ID {AttributeId} not found.", attributeId);
                    return ServiceResult<ContextualPricingAttributeDto>.Failure($"Attribute set with ID {attributeId} not found or is inactive.");
                }

                var dto = _mapper.Map<ContextualPricingAttributeDto>(attributes);
                return ServiceResult<ContextualPricingAttributeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pricing attribute set with ID {AttributeId}.", attributeId);
                return ServiceResult<ContextualPricingAttributeDto>.Failure("An error occurred while retrieving the attribute set.");
            }
        }

        // Retrieves all active pricing attribute sets, ordered by relevance.
        public async Task<ServiceResult<IEnumerable<ContextualPricingAttributeDto>>> GetAllActiveAttributeSetsAsync()
        {
            try
            {
                var attributes = await _unitOfWork.ContextualPricingAttributes.GetAllActiveAsync();
                var dtos = _mapper.Map<IEnumerable<ContextualPricingAttributeDto>>(
                    attributes.OrderBy(a => a.TimeUntilDeparture).ThenBy(a => a.LengthOfStay)
                );

                _logger.LogInformation("Successfully retrieved {Count} active pricing attribute sets.", dtos.Count());
                return ServiceResult<IEnumerable<ContextualPricingAttributeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active pricing attribute sets.");
                return ServiceResult<IEnumerable<ContextualPricingAttributeDto>>.Failure("An error occurred while retrieving attribute sets.");
            }
        }

        // Performs an advanced, paginated search for attribute sets. (Management System)
        public async Task<ServiceResult<PaginatedResult<ContextualPricingAttributeDto>>> SearchAttributeSetsAsync(PricingAttributeFilterDto filter, int pageNumber, int pageSize)
        {
            try
            {
                // Build the filter expression dynamically
                Expression<Func<ContextualPricingAttributes, bool>> filterExpression = a => (filter.IncludeDeleted || !a.IsDeleted);

                if (filter.MinTimeUntilDeparture.HasValue)
                    filterExpression = filterExpression.And(a => a.TimeUntilDeparture >= filter.MinTimeUntilDeparture.Value);
                if (filter.MaxTimeUntilDeparture.HasValue)
                    filterExpression = filterExpression.And(a => a.TimeUntilDeparture <= filter.MaxTimeUntilDeparture.Value);
                if (filter.MinLengthOfStay.HasValue)
                    filterExpression = filterExpression.And(a => a.LengthOfStay >= filter.MinLengthOfStay.Value);
                if (filter.MaxLengthOfStay.HasValue)
                    filterExpression = filterExpression.And(a => a.LengthOfStay <= filter.MaxLengthOfStay.Value);

                // Get paged results from the repository
                var (items, totalCount) = await _unitOfWork.ContextualPricingAttributes.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(a => a.TimeUntilDeparture).ThenBy(a => a.LengthOfStay)
                );

                var dtos = _mapper.Map<List<ContextualPricingAttributeDto>>(items);
                var paginatedResult = new PaginatedResult<ContextualPricingAttributeDto>(dtos, totalCount, pageNumber, pageSize);

                _logger.LogInformation("Retrieved paginated pricing attributes (Page {PageNumber}/{TotalPages}).", pageNumber, paginatedResult.TotalPages);
                return ServiceResult<PaginatedResult<ContextualPricingAttributeDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching pricing attributes on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<ContextualPricingAttributeDto>>.Failure("An error occurred during paginated search.");
            }
        }

        // Creates a new pricing attribute set. (Management System)
        public async Task<ServiceResult<ContextualPricingAttributeDto>> CreateAttributeSetAsync(CreatePricingAttributeDto createDto)
        {
            _logger.LogInformation("Attempting to create new pricing attribute set.");

            // Ensure time-related attributes are non-negative.
            if (createDto.TimeUntilDeparture < 0 || createDto.LengthOfStay < 0)
            {
                return ServiceResult<ContextualPricingAttributeDto>.Failure("Time Until Departure and Length of Stay attributes must be zero or positive.");
            }

            // Check for duplicate rule combination (e.g., same time AND same stay duration)
            if (await _unitOfWork.ContextualPricingAttributes.AnyAsync(
                a => a.TimeUntilDeparture == createDto.TimeUntilDeparture &&
                        a.LengthOfStay == createDto.LengthOfStay &&
                        !a.IsDeleted))
            {
                _logger.LogWarning("Creation failed: A pricing rule with TimeUntilDeparture={Time} and LengthOfStay={Stay} already exists.", createDto.TimeUntilDeparture, createDto.LengthOfStay);
                return ServiceResult<ContextualPricingAttributeDto>.Failure("A pricing rule with this combination of Time Until Departure and Length of Stay already exists.");
            }

            try
            {
                var newAttributeSet = _mapper.Map<ContextualPricingAttributes>(createDto);

                await _unitOfWork.ContextualPricingAttributes.AddAsync(newAttributeSet);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<ContextualPricingAttributeDto>(newAttributeSet);
                _logger.LogInformation("Successfully created pricing attribute set ID {AttributeId}.", newAttributeSet.AttributeId);
                return ServiceResult<ContextualPricingAttributeDto>.Success(dto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating pricing attribute set.");
                return ServiceResult<ContextualPricingAttributeDto>.Failure($"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pricing attribute set.");
                return ServiceResult<ContextualPricingAttributeDto>.Failure("An unexpected error occurred.");
            }
        }

        // Updates an existing pricing attribute set. (Management System)
        public async Task<ServiceResult<ContextualPricingAttributeDto>> UpdateAttributeSetAsync(int attributeId, UpdatePricingAttributeDto updateDto)
        {
            _logger.LogInformation("Attempting to update pricing attribute set ID {AttributeId}.", attributeId);

            // 1. Fetch the active attribute set
            var attributeSet = await _unitOfWork.ContextualPricingAttributes.GetActiveByIdAsync(attributeId);
            if (attributeSet == null)
            {
                _logger.LogWarning("Update failed: Active pricing attribute set {AttributeId} not found.", attributeId);
                // Changed return type for failure
                return ServiceResult<ContextualPricingAttributeDto>.Failure($"Active pricing attribute set with ID {attributeId} not found.");
            }

            // 2. Check for duplicate rule combination (excluding self)
            if (await _unitOfWork.ContextualPricingAttributes.AnyAsync(
                a => a.TimeUntilDeparture == updateDto.TimeUntilDeparture &&
                        a.LengthOfStay == updateDto.LengthOfStay &&
                        a.AttributeId != attributeId && // Exclude self
                        !a.IsDeleted))
            {
                _logger.LogWarning("Update failed: A pricing rule with TimeUntilDeparture={Time} and LengthOfStay={Stay} already exists.", updateDto.TimeUntilDeparture, updateDto.LengthOfStay);
                // Changed return type for failure
                return ServiceResult<ContextualPricingAttributeDto>.Failure("Another pricing rule with this combination of Time Until Departure and Length of Stay already exists.");
            }

            try
            {
                // 3. Map updated fields (ignores nulls from DTO thanks to mapper config)
                _mapper.Map(updateDto, attributeSet);

                _unitOfWork.ContextualPricingAttributes.Update(attributeSet);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated pricing attribute set ID {AttributeId}.", attributeId);

                // 4. Return the updated entity mapped to DTO
                var updatedDto = _mapper.Map<ContextualPricingAttributeDto>(attributeSet);
                return ServiceResult<ContextualPricingAttributeDto>.Success(updatedDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error updating pricing attribute set {AttributeId}.", attributeId);
                // Changed return type for failure
                return ServiceResult<ContextualPricingAttributeDto>.Failure("The record was modified by another user. Please reload and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pricing attribute set {AttributeId}.", attributeId);
                // Changed return type for failure
                return ServiceResult<ContextualPricingAttributeDto>.Failure("An error occurred while updating the attribute set.");
            }
        }

        // Soft deletes a pricing attribute set. (Management System)
        public async Task<ServiceResult> DeleteAttributeSetAsync(int attributeId)
        {
            _logger.LogInformation("Attempting to soft-delete pricing attribute set ID {AttributeId}.", attributeId);
            var attributeSet = await _unitOfWork.ContextualPricingAttributes.GetActiveByIdAsync(attributeId);
            if (attributeSet == null)
            {
                _logger.LogWarning("Soft delete failed: Active attribute set {AttributeId} not found.", attributeId);
                return ServiceResult.Failure($"Active attribute set with ID {attributeId} not found.");
            }

            // Check for dependencies: Active PriceOfferLogs
            bool hasActiveLogs = await _unitOfWork.PriceOfferLogs.AnyAsync(log => log.ContextAttributesId == attributeId && !log.IsDeleted);
            if (hasActiveLogs)
            {
                _logger.LogWarning("Failed to delete attribute set {AttributeId}: active price offer logs exist.", attributeId);
                return ServiceResult.Failure($"Cannot delete attribute set ID {attributeId}. It is referenced by active price offer logs. Please delete logs first or disassociate them.");
            }

            try
            {
                _unitOfWork.ContextualPricingAttributes.SoftDelete(attributeSet);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted attribute set ID {AttributeId}.", attributeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting attribute set {AttributeId}.", attributeId);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // Reactivates a soft-deleted pricing attribute set. (Management System)
        public async Task<ServiceResult> ReactivateAttributeSetAsync(int attributeId)
        {
            _logger.LogInformation("Attempting to reactivate attribute set ID {AttributeId}.", attributeId);

            // Fetch including deleted
            var attributeSet = await _unitOfWork.ContextualPricingAttributes.GetByIdAsync(attributeId);
            if (attributeSet == null)
            {
                _logger.LogWarning("Reactivation failed: Attribute set {AttributeId} not found.", attributeId);
                return ServiceResult.Failure($"Attribute set with ID {attributeId} not found.");
            }

            if (!attributeSet.IsDeleted)
            {
                _logger.LogWarning("Reactivation failed: Attribute set {AttributeId} is already active.", attributeId);
                return ServiceResult.Failure($"Attribute set ID {attributeId} is already active.");
            }

            try
            {
                attributeSet.IsDeleted = false; // Reactivate
                _unitOfWork.ContextualPricingAttributes.Update(attributeSet);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully reactivated attribute set ID {AttributeId}.", attributeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating attribute set ID {AttributeId}.", attributeId);
                return ServiceResult.Failure("An error occurred during reactivation.");
            }
        }

        // Finds the most relevant pricing attribute set(s) based on the current booking context.
        public async Task<ServiceResult<IEnumerable<ContextualPricingAttributeDto>>> FindBestMatchingAttributeSetsAsync(PricingContextDto context)
        {
            _logger.LogInformation("Finding best matching price attributes for context: DaysToDeparture={Days}, LengthOfStay={Stay}.", context.DaysToDeparture, context.LengthOfStayDays);

            try
            {
                // This logic assumes we find two *separate* rules: one for time, one for stay.
                // A more complex model might find one rule that matches *both*.

                // Find best match for Time Until Departure (closest upper bound)
                var timeAttribute = await _unitOfWork.ContextualPricingAttributes.GetByTimeUntilDepartureAsync(context.DaysToDeparture);

                // Find best match for Length of Stay (closest upper bound)
                var stayAttribute = await _unitOfWork.ContextualPricingAttributes.GetByLengthOfStayAsync(context.LengthOfStayDays);

                var results = new List<ContextualPricingAttributes>();
                if (timeAttribute != null) results.Add(timeAttribute);
                if (stayAttribute != null) results.Add(stayAttribute);

                if (!results.Any())
                {
                    _logger.LogWarning("No matching contextual pricing attributes found for the given context.");
                    return ServiceResult<IEnumerable<ContextualPricingAttributeDto>>.Failure("No matching pricing attributes found.");
                }

                // Return unique attributes (in case one rule matched both)
                var dtos = _mapper.Map<IEnumerable<ContextualPricingAttributeDto>>(results.Distinct());
                return ServiceResult<IEnumerable<ContextualPricingAttributeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding best matching attribute sets for context.");
                return ServiceResult<IEnumerable<ContextualPricingAttributeDto>>.Failure("An error occurred while finding matching pricing rules.");
            }
        }
         
    }

}