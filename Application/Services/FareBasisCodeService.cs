using Application.DTOs.FareBasisCode;
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
using Microsoft.EntityFrameworkCore;  

namespace Application.Services
{
    // Service implementation for managing Fare Basis Codes.
    public class FareBasisCodeService : IFareBasisCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FareBasisCodeService> _logger;

        // Constructor for dependency injection
        public FareBasisCodeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FareBasisCodeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Retrieves a single active fare basis code by its unique code.
        public async Task<ServiceResult<FareBasisCodeDto>> GetFareByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return ServiceResult<FareBasisCodeDto>.Failure("Fare code cannot be empty.");

            try
            {
                var fareCode = await _unitOfWork.FareBasisCodes.GetByCodeAsync(code);
                if (fareCode == null)
                {
                    _logger.LogWarning("Fare basis code {Code} not found or is inactive.", code);
                    return ServiceResult<FareBasisCodeDto>.Failure($"Fare code '{code}' not found or is inactive.");
                }

                var dto = _mapper.Map<FareBasisCodeDto>(fareCode);
                return ServiceResult<FareBasisCodeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fare basis code {Code}.", code);
                return ServiceResult<FareBasisCodeDto>.Failure("An error occurred while retrieving the fare code.");
            }
        }

        // Retrieves all active fare basis codes, ordered by code.
        public async Task<ServiceResult<IEnumerable<FareBasisCodeDto>>> GetAllActiveFaresAsync()
        {
            try
            {
                var fareCodes = await _unitOfWork.FareBasisCodes.GetAllActiveAsync();
                var dtos = _mapper.Map<IEnumerable<FareBasisCodeDto>>(fareCodes.OrderBy(f => f.Code));

                _logger.LogInformation("Successfully retrieved {Count} active fare basis codes.", dtos.Count());
                return ServiceResult<IEnumerable<FareBasisCodeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active fare basis codes.");
                return ServiceResult<IEnumerable<FareBasisCodeDto>>.Failure("An error occurred while retrieving fare codes.");
            }
        }

        // Retrieves a paginated list of active fare basis codes. (Management System)
        public async Task<ServiceResult<PaginatedResult<FareBasisCodeDto>>> GetPaginatedFaresAsync(int pageNumber, int pageSize, string? descriptionFilter = null)
        {
            try
            {
                // Build the filter expression
                Expression<Func<FareBasisCode, bool>> filter = f => !f.IsDeleted;
                if (!string.IsNullOrWhiteSpace(descriptionFilter))
                {
                    filter = filter.And(f => EF.Functions.Like(f.Description, $"%{descriptionFilter}%"));
                }

                // Get paged results from the repository
                var (items, totalCount) = await _unitOfWork.FareBasisCodes.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filter,
                    orderBy: q => q.OrderBy(f => f.Code)
                );

                var dtos = _mapper.Map<List<FareBasisCodeDto>>(items);
                var paginatedResult = new PaginatedResult<FareBasisCodeDto>(dtos, totalCount, pageNumber, pageSize);

                _logger.LogInformation("Retrieved paginated fare codes (Page {PageNumber}/{TotalPages}) with filter '{Filter}'.", pageNumber, paginatedResult.TotalPages, descriptionFilter);
                return ServiceResult<PaginatedResult<FareBasisCodeDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated fare codes on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<FareBasisCodeDto>>.Failure("An error occurred during paginated search.");
            }
        }

        // Creates a new fare basis code. (Management System)
        public async Task<ServiceResult<FareBasisCodeDto>> CreateFareCodeAsync(CreateFareBasisCodeDto createDto)
        {
            var codeUpper = createDto.Code.ToUpper();
            _logger.LogInformation("Attempting to create new fare basis code: {Code}", codeUpper);

            // Check for uniqueness (including soft-deleted records)
            if (await _unitOfWork.FareBasisCodes.ExistsByCodeAsync(codeUpper))
            {
                _logger.LogWarning("Creation failed: Fare code {Code} already exists.", codeUpper);
                return ServiceResult<FareBasisCodeDto>.Failure($"Fare code '{codeUpper}' already exists. Check active and inactive records.");
            }

            try
            {
                var newFareCode = _mapper.Map<FareBasisCode>(createDto);

                await _unitOfWork.FareBasisCodes.AddAsync(newFareCode);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<FareBasisCodeDto>(newFareCode);
                _logger.LogInformation("Successfully created fare basis code: {Code}", dto.Code);
                return ServiceResult<FareBasisCodeDto>.Success(dto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating fare code {Code}.", codeUpper);
                return ServiceResult<FareBasisCodeDto>.Failure($"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fare code {Code}.", codeUpper);
                return ServiceResult<FareBasisCodeDto>.Failure("An unexpected error occurred.");
            }
        }

        // Updates an existing fare basis code's description and rules. (Management System)
        public async Task<ServiceResult<FareBasisCodeDto>> UpdateFareCodeAsync(string code, UpdateFareBasisCodeDto updateDto)
        {
            var codeUpper = code.ToUpper();
            _logger.LogInformation("Attempting to update fare code: {Code}", codeUpper);

            // Get active fare code
            var fareCode = await _unitOfWork.FareBasisCodes.GetByCodeAsync(codeUpper);
            if (fareCode == null)
            {
                _logger.LogWarning("Update failed: Active fare code {Code} not found.", codeUpper);
                return ServiceResult<FareBasisCodeDto>.Failure($"Active fare code '{codeUpper}' not found.");
            }

            try
            {
                // Map updated fields (Description, Rules) from DTO to the entity.
                // AutoMapper will handle the properties defined in FareBasisCodeMappingProfile.
                _mapper.Map(updateDto, fareCode);

                _unitOfWork.FareBasisCodes.Update(fareCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated fare code: {Code}", codeUpper);

                // Map the updated entity back to DTO before returning.
                var updatedDto = _mapper.Map<FareBasisCodeDto>(fareCode);
                return ServiceResult<FareBasisCodeDto>.Success(updatedDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error updating fare code {Code}. Record may have changed.", codeUpper);
                return ServiceResult<FareBasisCodeDto>.Failure("The record was modified by another user. Please reload and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fare code {Code}.", codeUpper);
                return ServiceResult<FareBasisCodeDto>.Failure("An error occurred while updating the fare code.");
            }
        }

        // Soft deletes a fare basis code. (Management System)
        public async Task<ServiceResult> DeleteFareCodeAsync(string code)
        {
            var codeUpper = code.ToUpper();
            _logger.LogInformation("Attempting to soft-delete fare code: {Code}", codeUpper);

            var fareCode = await _unitOfWork.FareBasisCodes.GetByCodeAsync(codeUpper);
            if (fareCode == null)
            {
                _logger.LogWarning("Soft delete failed: Active fare code {Code} not found.", codeUpper);
                return ServiceResult.Failure($"Active fare code '{codeUpper}' not found.");
            }

            // Check for dependencies: Active bookings or price offer logs
            bool hasActiveBookings = await _unitOfWork.Bookings.AnyAsync(b => b.FareBasisCodeId == codeUpper && !b.IsDeleted);
            bool hasActivePriceLogs = await _unitOfWork.PriceOfferLogs.AnyAsync(p => p.FareId == codeUpper && !p.IsDeleted);

            if (hasActiveBookings || hasActivePriceLogs)
            {
                var dependencies = new List<string>();
                if (hasActiveBookings) dependencies.Add("active bookings");
                if (hasActivePriceLogs) dependencies.Add("price offer logs");

                _logger.LogWarning("Failed to delete fare code {Code}: active dependencies exist.", codeUpper);
                return ServiceResult.Failure($"Cannot delete fare code '{codeUpper}'. It is used by: {string.Join(", ", dependencies)}.");
            }

            try
            {
                _unitOfWork.FareBasisCodes.SoftDelete(fareCode);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted fare code: {Code}", codeUpper);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting fare code {Code}.", codeUpper);
                return ServiceResult.Failure("An error occurred during deletion.");
            }
        }

        // Reactivates a soft-deleted fare basis code. (Management System)
        public async Task<ServiceResult> ReactivateFareCodeAsync(string code)
        {
            var codeUpper = code.ToUpper();
            _logger.LogInformation("Attempting to reactivate fare code: {Code}", codeUpper);

            // Fetch including deleted (using generic GetByIdAsync which finds by PK)
            var fareCode = await _unitOfWork.FareBasisCodes.GetByIdAsync(codeUpper);
            if (fareCode == null)
            {
                _logger.LogWarning("Reactivation failed: Fare code {Code} not found.", codeUpper);
                return ServiceResult.Failure($"Fare code '{codeUpper}' not found.");
            }

            if (!fareCode.IsDeleted)
            {
                _logger.LogWarning("Reactivation failed: Fare code {Code} is already active.", codeUpper);
                return ServiceResult.Failure($"Fare code '{codeUpper}' is already active.");
            }

            try
            {
                fareCode.IsDeleted = false; // Reactivate
                _unitOfWork.FareBasisCodes.Update(fareCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully reactivated fare code: {Code}", codeUpper);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating fare code {Code}.", codeUpper);
                return ServiceResult.Failure("An error occurred during reactivation.");
            }
        }

        // Retrieves all fare codes, including soft-deleted ones.
        public async Task<ServiceResult<IEnumerable<FareBasisCodeDto>>> GetAllFaresIncludingDeletedAsync()
        {
            try
            {
                var fareCodes = await _unitOfWork.FareBasisCodes.GetAllIncludingDeletedAsync();
                var dtos = _mapper.Map<IEnumerable<FareBasisCodeDto>>(fareCodes.OrderBy(f => f.Code));

                _logger.LogInformation("Successfully retrieved all {Count} fare codes (including deleted).", dtos.Count());
                return ServiceResult<IEnumerable<FareBasisCodeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all fare codes (including deleted).");
                return ServiceResult<IEnumerable<FareBasisCodeDto>>.Failure("An error occurred retrieving all fare codes.");
            }
        }

         
    }
     
}