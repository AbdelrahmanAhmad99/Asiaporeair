using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for FareBasisCode entity, extending the generic repository.
    /// Provides methods for querying fare rules and descriptions, essential for pricing calculations.
    /// </summary>
    public interface IFareBasisCodeRepository : IGenericRepository<FareBasisCode>
    {
        /// <summary>
        /// Retrieves an active fare basis code by its unique code (primary key).
        /// </summary>
        /// <param name="code">The unique fare basis code (e.g., 'Y', 'J', 'FLEX').</param>
        /// <returns>The FareBasisCode entity if found and active; otherwise, null.</returns>
        Task<FareBasisCode?> GetByCodeAsync(string code); // Existing method

        /// <summary>
        /// Finds active fare basis codes where the description contains the specified text (case-insensitive search).
        /// Useful for searching fare types in the management system.
        /// </summary>
        /// <param name="descriptionSubstring">The text to search for within the description.</param>
        /// <returns>An enumerable collection of matching active FareBasisCode entities.</returns>
        Task<IEnumerable<FareBasisCode>> FindByDescriptionAsync(string descriptionSubstring);

        /// <summary>
        /// Retrieves all fare basis codes, including those marked as soft-deleted.
        /// For administrative review or historical data lookup.
        /// </summary>
        /// <returns>An enumerable collection of all FareBasisCode entities.</returns>
        Task<IEnumerable<FareBasisCode>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) fare basis codes.
        /// Standard method for populating lists or applying rules.
        /// </summary>
        /// <returns>An enumerable collection of active FareBasisCode entities.</returns>
        Task<IEnumerable<FareBasisCode>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a fare basis code with the specified code exists (active or soft-deleted).
        /// </summary>
        /// <param name="code">The fare basis code to check.</param>
        /// <returns>True if a fare basis code with the given code exists; otherwise, false.</returns>
        Task<bool> ExistsByCodeAsync(string code);


    }
}
