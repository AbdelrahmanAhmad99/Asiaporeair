using Application.DTOs.FareBasisCode;
using Application.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{ 
    public interface IFareBasisCodeService
    {
        /// <summary>
        /// Retrieves a single active fare basis code by its unique code.
        /// </summary>
        /// <param name="code">The unique fare basis code (e.g., 'ECOFLEX').</param>
        /// <returns>A ServiceResult containing the FareBasisCodeDto, or a failure result.</returns>
        Task<ServiceResult<FareBasisCodeDto>> GetFareByCodeAsync(string code);

        /// <summary>
        /// Retrieves all active fare basis codes, ordered by code.
        /// </summary>
        /// <returns>A ServiceResult containing a list of active FareBasisCodeDto objects.</returns>
        Task<ServiceResult<IEnumerable<FareBasisCodeDto>>> GetAllActiveFaresAsync();

        /// <summary>
        /// Retrieves a paginated list of active fare basis codes, optionally filtered by description.
        /// (Management System)
        /// </summary>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <param name="descriptionFilter">Optional text to search for in the description.</param>
        /// <returns>A ServiceResult containing a paginated list of matching FareBasisCodeDto objects.</returns>
        Task<ServiceResult<PaginatedResult<FareBasisCodeDto>>> GetPaginatedFaresAsync(int pageNumber, int pageSize, string? descriptionFilter = null);

        /// <summary>
        /// Creates a new fare basis code. (Management System)
        /// </summary>
        /// <param name="createDto">The data for the new fare code.</param>
        /// <returns>A ServiceResult containing the created FareBasisCodeDto, or a failure result.</returns>
        Task<ServiceResult<FareBasisCodeDto>> CreateFareCodeAsync(CreateFareBasisCodeDto createDto);

        /// <summary>
        /// Updates an existing fare basis code's description and rules. (Management System)
        /// </summary>
        /// <param name="code">The code of the fare basis to update.</param>
        /// <param name="updateDto">The updated data.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<FareBasisCodeDto>> UpdateFareCodeAsync(string code, UpdateFareBasisCodeDto updateDto);

        /// <summary>
        /// Soft deletes a fare basis code. Fails if the code is in use by active bookings. (Management System)
        /// </summary>
        /// <param name="code">The code of the fare basis to soft delete.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> DeleteFareCodeAsync(string code);

        /// <summary>
        /// Reactivates a soft-deleted fare basis code. (Management System)
        /// </summary>
        /// <param name="code">The code of the fare basis to reactivate.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> ReactivateFareCodeAsync(string code);

        /// <summary>
        /// Retrieves all fare basis codes, including soft-deleted ones (for administrative views).
        /// </summary>
        /// <returns>A ServiceResult containing a list of all FareBasisCodeDto objects (including deleted).</returns>
        Task<ServiceResult<IEnumerable<FareBasisCodeDto>>> GetAllFaresIncludingDeletedAsync();
    }
}