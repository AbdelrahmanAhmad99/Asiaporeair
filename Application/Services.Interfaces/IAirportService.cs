using Application.DTOs.Airport;
using Application.Models;
using Domain.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{ 
    public interface IAirportService
    {
        /// <summary>
        /// Retrieves all active airports, ordered by name. Suitable for general lookups and dropdowns.
        /// Includes basic country name.
        /// </summary>
        /// <returns>A ServiceResult containing a list of active AirportDto objects.</returns>
        Task<ServiceResult<IEnumerable<AirportDto>>> GetAllActiveAirportsAsync();

        /// <summary>
        /// Retrieves a specific active airport by its IATA code. Includes country name.
        /// </summary>
        /// <param name="iataCode">The 3-letter IATA code.</param>
        /// <returns>A ServiceResult containing the AirportDto if found and active, or a failure result.</returns>
        Task<ServiceResult<AirportDto>> GetAirportByIataCodeAsync(string iataCode);

        /// <summary>
        /// Retrieves a specific active airport by its ICAO code. Includes country name.
        /// </summary>
        /// <param name="icaoCode">The 4-letter ICAO code.</param>
        /// <returns>A ServiceResult containing the AirportDto if found and active, or a failure result.</returns>
        Task<ServiceResult<AirportDto>> GetAirportByIcaoCodeAsync(string icaoCode);

        /// <summary>
        /// Finds active airports where the name contains the specified text (case-insensitive). Includes country name.
        /// </summary>
        /// <param name="nameSubstring">The text to search for within the airport name.</param>
        /// <returns>A ServiceResult containing a list of matching active AirportDto objects.</returns>
        Task<ServiceResult<IEnumerable<AirportDto>>> FindAirportsByNameAsync(string nameSubstring);

        /// <summary>
        /// Retrieves all active airports located within a specific city (case-insensitive). Includes country name.
        /// </summary>
        /// <param name="city">The name of the city.</param>
        /// <returns>A ServiceResult containing a list of active AirportDto objects in the city.</returns>
        Task<ServiceResult<IEnumerable<AirportDto>>> GetAirportsByCityAsync(string city);

        /// <summary>
        /// Retrieves all active airports located within a specific country (by ISO code). Includes country name.
        /// </summary>
        /// <param name="countryIsoCode">The 3-letter ISO code of the country.</param>
        /// <returns>A ServiceResult containing a list of active AirportDto objects in the country.</returns>
        Task<ServiceResult<IEnumerable<AirportDto>>> GetAirportsByCountryAsync(string countryIsoCode);

        /// <summary>
        /// Performs an advanced search for airports based on multiple optional filter criteria.
        /// Includes country name. Supports pagination. Used primarily in the management system.
        /// </summary>
        /// <param name="filter">The filter criteria (name, city, country, coordinates, etc.).</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <returns>A ServiceResult containing a paginated list of AirportDto objects matching the filters.</returns>
        Task<ServiceResult<PaginatedResult<AirportDto>>> SearchAirportsAsync(AirportFilterDto filter, int pageNumber, int pageSize);

        /// <summary>
        /// Creates a new airport. Validates uniqueness of IATA and ICAO codes.
        /// Used by the management system.
        /// </summary>
        /// <param name="createDto">The data for the new airport.</param>
        /// <returns>A ServiceResult containing the created AirportDto, or a failure result with validation errors.</returns>
        Task<ServiceResult<AirportDto>> CreateAirportAsync(CreateAirportDto createDto);

        /// <summary>
        /// Updates an existing airport's details. IATA and ICAO codes are generally not updatable.
        /// Used by the management system.
        /// </summary>
        /// <param name="iataCode">The IATA code of the airport to update.</param>
        /// <param name="updateDto">The updated data for the airport.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult<AirportDto>> UpdateAirportAsync(string iataCode, UpdateAirportDto updateDto);

        /// <summary>
        /// Soft deletes an airport by its IATA code. Checks for dependencies like active routes or schedules.
        /// Used by the management system.
        /// </summary>
        /// <param name="iataCode">The IATA code of the airport to soft delete.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> DeleteAirportAsync(string iataCode);

        /// <summary>
        /// Reactivates a soft-deleted airport. Used by the management system.
        /// </summary>
        /// <param name="iataCode">The IATA code of the airport to reactivate.</param>
        /// <returns>A ServiceResult indicating success or failure.</returns>
        Task<ServiceResult> ReactivateAirportAsync(string iataCode);

        /// <summary>
        /// Retrieves all airports, including soft-deleted ones (for administrative views). Includes country name.
        /// </summary>
        /// <returns>A ServiceResult containing a list of all AirportDto objects (including deleted).</returns>
        Task<ServiceResult<IEnumerable<AirportDto>>> GetAllAirportsIncludingDeletedAsync();
    }
}