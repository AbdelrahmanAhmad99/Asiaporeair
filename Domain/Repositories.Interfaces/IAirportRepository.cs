using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Airport entity, extending the generic repository.
    /// Provides specific methods for querying airport data, crucial for flight scheduling,
    /// routing, and airport management functionalities.
    /// </summary>
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        /// <summary>
        /// Retrieves an active airport by its unique IATA code.
        /// </summary>
        /// <param name="iataCode">The 3-letter IATA code.</param>
        /// <returns>The Airport entity if found and active; otherwise, null.</returns>
        Task<Airport?> GetByIataCodeAsync(string iataCode);

        /// <summary>
        /// Retrieves an active airport by its unique ICAO code.
        /// </summary>
        /// <param name="icaoCode">The 4-letter ICAO code.</param>
        /// <returns>The Airport entity if found and active; otherwise, null.</returns>
        Task<Airport?> GetByIcaoCodeAsync(string icaoCode);

        /// <summary>
        /// Retrieves active airports matching a specific name (case-insensitive).
        /// </summary>
        /// <param name="name">The name or partial name of the airport.</param>
        /// <returns>An enumerable collection of matching active Airport entities.</returns>
        Task<IEnumerable<Airport>> FindByNameAsync(string name);

        /// <summary>
        /// Retrieves all active airports located within a specific city (case-insensitive).
        /// </summary>
        /// <param name="city">The name of the city.</param>
        /// <returns>An enumerable collection of active Airport entities in the specified city.</returns>
        Task<IEnumerable<Airport>> GetByCityAsync(string city);

        /// <summary>
        /// Retrieves all active airports belonging to a specific country.
        /// </summary>
        /// <param name="countryIsoCode">The 3-letter ISO code of the country.</param>
        /// <returns>An enumerable collection of active Airport entities in the specified country.</returns>
        Task<IEnumerable<Airport>> GetByCountryAsync(string countryIsoCode);

        /// <summary>
        /// Retrieves all airports, including those marked as soft-deleted.
        /// Primarily for administrative views in the airport management system.
        /// </summary>
        /// <returns>An enumerable collection of all Airport entities.</returns>
        Task<IEnumerable<Airport>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) airports.
        /// Standard method for populating lists and general use.
        /// </summary>
        /// <returns>An enumerable collection of active Airport entities.</returns>
        Task<IEnumerable<Airport>> GetAllActiveAsync();

        /// <summary>
        /// Retrieves an active airport by its IATA code, including its associated Country details (eager loading).
        /// </summary>
        /// <param name="iataCode">The 3-letter IATA code.</param>
        /// <returns>The Airport entity with its Country loaded, if found and active; otherwise, null.</returns>
        Task<Airport?> GetWithCountryAsync(string iataCode);

        /// <summary>
        /// Checks if an airport with the specified IATA code exists (active or soft-deleted).
        /// </summary>
        /// <param name="iataCode">The 3-letter IATA code to check.</param>
        /// <returns>True if an airport with the given IATA code exists; otherwise, false.</returns>
        Task<bool> ExistsByIataCodeAsync(string iataCode);

        /// <summary>
        /// Checks if an airport with the specified ICAO code exists (active or soft-deleted).
        /// </summary>
        /// <param name="icaoCode">The 4-letter ICAO code to check.</param>
        /// <returns>True if an airport with the given ICAO code exists; otherwise, false.</returns>
        Task<bool> ExistsByIcaoCodeAsync(string icaoCode);
    }
}