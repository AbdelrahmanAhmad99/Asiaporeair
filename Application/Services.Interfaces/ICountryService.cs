using Application.DTOs.Country;
using Application.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing Country data.
    /// Provides methods for CRUD operations and querying country information,
    /// used by both booking (lookups) and management systems.
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Retrieves all active countries, ordered by name. Suitable for populating dropdown lists.
        /// </summary>
        /// <returns>A ServiceResult containing a list of active CountryDto objects.</returns>
        Task<ServiceResult<IEnumerable<CountryDto>>> GetAllActiveCountriesAsync();

        /// <summary>
        /// Retrieves a specific active country by its ISO code.
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code of the country.</param>
        /// <returns>A ServiceResult containing the CountryDto if found and active, or a failure result.</returns>
        Task<ServiceResult<CountryDto>> GetCountryByIsoCodeAsync(string isoCode);

        /// <summary>
        /// Retrieves a specific active country by its name (case-insensitive).
        /// </summary>
        /// <param name="name">The name of the country.</param>
        /// <returns>A ServiceResult containing the CountryDto if found and active, or a failure result.</returns>
        Task<ServiceResult<CountryDto>> GetCountryByNameAsync(string name);

        /// <summary>
        /// Retrieves all active countries belonging to a specific continent (case-insensitive).
        /// </summary>
        /// <param name="continentName">The name of the continent.</param>
        /// <returns>A ServiceResult containing a list of active CountryDto objects for the specified continent.</returns>
        Task<ServiceResult<IEnumerable<CountryDto>>> GetCountriesByContinentAsync(string continentName);


        Task<ServiceResult<CountryWithAirportsDto>> GetCountryWithAirportsByIsoCodeAsync(string isoCode);
        /// <summary>
        /// Creates a new country. Primarily used by the management system.
        /// Performs validation to ensure the ISO code and name are unique.
        /// </summary>
        /// <param name="createDto">The data for the new country.</param>
        /// <returns>A ServiceResult containing the created CountryDto, or a failure result with validation errors.</returns>
        Task<ServiceResult<CountryDto>> CreateCountryAsync(CreateCountryDto createDto);

        /// <summary>
        /// Updates an existing country's details (Name, Continent).
        /// The ISO code is typically immutable and used for identification.
        /// Primarily used by the management system.
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code of the country to update.</param>
        /// <param name="updateDto">The updated data for the country.</param>
        /// <returns>A ServiceResult indicating success or failure of the update operation.</returns>
        Task<ServiceResult<CountryDto>> UpdateCountryAsync(string isoCode, UpdateCountryDto updateDto);

        /// <summary>
        /// Soft deletes a country by its ISO code. Associated airports might restrict deletion.
        /// Primarily used by the management system.
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code of the country to soft delete.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> DeleteCountryAsync(string isoCode);

        /// <summary>
        /// Reactivates a soft-deleted country. Primarily used by the management system.
        /// </summary>
        /// <param name="isoCode">The 3-letter ISO code of the country to reactivate.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> ReactivateCountryAsync(string isoCode); 

    }
}