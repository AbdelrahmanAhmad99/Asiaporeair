using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
   
    public interface ICountryRepository : IGenericRepository<Country>
    {
        /// <summary>
        /// Retrieves a country by its unique ISO code, ignoring soft-deleted records.
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code of the country.</param>
        /// <returns>The Country entity if found and not deleted; otherwise, null.</returns>
        Task<Country?> GetByIsoCodeAsync(string isoCode);

        /// <summary>
        /// Retrieves a country by its name, performing a case-insensitive search and ignoring soft-deleted records.
        /// </summary>
        /// <param name="name">The name of the country.</param>
        /// <returns>The Country entity if found and not deleted; otherwise, null.</returns>
        Task<Country?> GetByNameAsync(string name);

        /// <summary>
        /// Retrieves all countries belonging to a specific continent, ignoring soft-deleted records.
        /// </summary>
        /// <param name="continentName">The name of the continent.</param>
        /// <returns>An enumerable collection of Country entities matching the continent.</returns>
        Task<IEnumerable<Country>> GetByContinentAsync(string continentName);

        /// <summary>
        /// Retrieves all active (not soft-deleted) countries.
        /// This is typically used for populating dropdown lists or general display.
        /// </summary>
        /// <returns>An enumerable collection of active Country entities.</returns>
        Task<IEnumerable<Country>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves a country by its ISO code, including its associated airports (eager loading).
        /// Ignores soft-deleted countries but may include soft-deleted airports depending on configuration.
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code of the country.</param>
        /// <returns>The Country entity with its Airports collection loaded, if found and not deleted; otherwise, null.</returns>
        Task<Country?> GetWithAirportsAsync(string isoCode);

        /// <summary>
        /// Checks if a country with the specified ISO code exists (active or soft-deleted).
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code to check.</param>
        /// <returns>True if a country with the given ISO code exists; otherwise, false.</returns>
        Task<bool> ExistsByIsoCodeAsync(string isoCode);

        /// <summary>
        /// Checks if a country with the specified name exists (active or soft-deleted, case-insensitive).
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if a country with the given name exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);
    }
}