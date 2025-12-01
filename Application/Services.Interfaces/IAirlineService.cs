using Application.DTOs.Airline;
using Application.Models; // Assuming ServiceResult & PaginatedResult are here
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Service interface for managing Airline data.
    public interface IAirlineService
    {
        // Retrieves all active airlines, ordered by name.
        Task<ServiceResult<IEnumerable<AirlineDto>>> GetAllActiveAirlinesAsync();

        // Retrieves a specific active airline by its IATA code, including base airport name.
        Task<ServiceResult<AirlineDto>> GetAirlineByIataCodeAsync(string iataCode);

        // Finds active airlines where the name contains the specified text (case-insensitive).
        Task<ServiceResult<IEnumerable<AirlineDto>>> FindAirlinesByNameAsync(string nameSubstring);

        // Retrieves all active airlines based at a specific airport (by IATA code).
        Task<ServiceResult<IEnumerable<AirlineDto>>> GetAirlinesByBaseAirportAsync(string airportIataCode);

        // Retrieves all active airlines operating within a specific region (case-insensitive).
        Task<ServiceResult<IEnumerable<AirlineDto>>> GetAirlinesByOperatingRegionAsync(string region);

        // Retrieves detailed information for an active airline, including its fleet.
        Task<ServiceResult<AirlineDetailDto>> GetAirlineWithFleetAsync(string iataCode);

        // Creates a new airline. Validates uniqueness of IATA code and name. (Management System)
        Task<ServiceResult<AirlineDto>> CreateAirlineAsync(CreateAirlineDto createDto);

        // Updates an existing airline's details. (Management System)
        Task<ServiceResult<AirlineDto>> UpdateAirlineAsync(string iataCode, UpdateAirlineDto updateDto);

        // Soft deletes an airline by its IATA code. Checks for dependencies (active aircraft, schedules). (Management System)
        Task<ServiceResult> DeleteAirlineAsync(string iataCode);

        // Reactivates a soft-deleted airline. (Management System)
        Task<ServiceResult> ReactivateAirlineAsync(string iataCode);

        // Retrieves all airlines, including soft-deleted ones (for administrative views).
        Task<ServiceResult<IEnumerable<AirlineDto>>> GetAllAirlinesIncludingDeletedAsync();

        // Retrieves a paginated list of active airlines, optionally filtered by region. (Management System)
        Task<ServiceResult<PaginatedResult<AirlineDto>>> GetPaginatedAirlinesAsync(int pageNumber, int pageSize, string? regionFilter = null);
    }
}