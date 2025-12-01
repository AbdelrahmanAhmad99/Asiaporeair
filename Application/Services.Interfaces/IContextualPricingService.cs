using Application.DTOs.ContextualPricingAttribute; 
using Application.Models;  
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{ 
    public interface IContextualPricingService
    {
        /// <summary>
        /// Retrieves a single active pricing attribute set by its ID.
        /// </summary>
        /// <param name="attributeId">The unique ID of the attribute set.</param>
        /// <returns>A ServiceResult containing the attribute set DTO, or a failure result.</returns>
        Task<ServiceResult<ContextualPricingAttributeDto>> GetAttributeSetByIdAsync(int attributeId);

        /// <summary>
        /// Retrieves all active pricing attribute sets, ordered by relevance (e.g., time until departure).
        /// </summary>
        /// <returns>A ServiceResult containing a list of active attribute set DTOs.</returns>
        Task<ServiceResult<IEnumerable<ContextualPricingAttributeDto>>> GetAllActiveAttributeSetsAsync();

        /// <summary>
        /// Retrieves a paginated list of pricing attribute sets based on filters. (Management System)
        /// </summary>
        /// <param name="filter">The filter criteria (time to departure, length of stay, etc.).</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <returns>A ServiceResult containing a paginated list of matching attribute set DTOs.</returns>
        Task<ServiceResult<PaginatedResult<ContextualPricingAttributeDto>>> SearchAttributeSetsAsync(PricingAttributeFilterDto filter, int pageNumber, int pageSize);

        /// <summary>
        /// Creates a new pricing attribute set. (Management System)
        /// </summary>
        /// <param name="createDto">The data for the new attribute set.</param>
        /// <returns>A ServiceResult containing the created DTO, or a failure result.</returns>
        Task<ServiceResult<ContextualPricingAttributeDto>> CreateAttributeSetAsync(CreatePricingAttributeDto createDto);

        /// <summary>
        /// Updates an existing pricing attribute set. (Management System)
        /// </summary>
        /// <param name="attributeId">The ID of the attribute set to update.</param>
        /// <param name="updateDto">The updated data.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<ContextualPricingAttributeDto>> UpdateAttributeSetAsync(int attributeId, UpdatePricingAttributeDto updateDto);

        /// <summary>
        /// Soft deletes a pricing attribute set. Fails if the set is in use by price logs. (Management System)
        /// </summary>
        /// <param name="attributeId">The ID of the attribute set to soft delete.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> DeleteAttributeSetAsync(int attributeId);

        /// <summary>
        /// Reactivates a soft-deleted pricing attribute set. (Management System)
        /// </summary>
        /// <param name="attributeId">The ID of the attribute set to reactivate.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> ReactivateAttributeSetAsync(int attributeId);

        /// <summary>
        /// Finds the most relevant pricing attribute set(s) based on the current booking context.
        /// This is a key method used by the main IPricingService.
        /// </summary>
        /// <param name="context">The current booking context (days to departure, length of stay, etc.).</param>
        /// <returns>A ServiceResult containing one or more matching attribute DTOs.</returns>
        Task<ServiceResult<IEnumerable<ContextualPricingAttributeDto>>> FindBestMatchingAttributeSetsAsync(PricingContextDto context);
    }
}