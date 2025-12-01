using Application.DTOs.Aircraft;
using Application.DTOs.Seat;
using Application.Models; // For ServiceResult & PaginatedResult
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Service interface for managing the aircraft fleet and their configurations.
    public interface IAircraftManagementService
    {
        // --- Aircraft Methods ---

        // Retrieves a single active aircraft by its tail number.
        Task<ServiceResult<AircraftDto>> GetAircraftByTailNumberAsync(string tailNumber);

        // Retrieves a paginated list of active aircraft based on advanced filters.
        Task<ServiceResult<PaginatedResult<AircraftDto>>> GetAircraftPaginatedAsync(AircraftFilterDto filter, int pageNumber, int pageSize);

        // Retrieves full details for a single aircraft, including configurations, cabins, and seat counts.
        Task<ServiceResult<AircraftDetailDto>> GetAircraftDetailsAsync(string tailNumber);

        // Creates a new aircraft record in the fleet.
        Task<ServiceResult<AircraftDto>> CreateAircraftAsync(CreateAircraftDto createDto);

        // Updates an existing aircraft's core properties (airline, type, acquisition date).
        Task<ServiceResult<AircraftDto>> UpdateAircraftAsync(string tailNumber, UpdateAircraftDto updateDto);

        // Soft deletes an aircraft (sets IsDeleted = true) after checking dependencies.
        Task<ServiceResult> DeleteAircraftAsync(string tailNumber);

        // Reactivates a soft-deleted aircraft.
        Task<ServiceResult> ReactivateAircraftAsync(string tailNumber);

        // Updates the operational status of an aircraft (e.g., "Active", "Maintenance").
        Task<ServiceResult> UpdateAircraftStatusAsync(string tailNumber, UpdateAircraftStatusDto dto);

        // Adds flight hours to an aircraft's total log.
        Task<ServiceResult<int?>> AddFlightHoursAsync(string tailNumber, AddFlightHoursDto dto);

        // --- Aircraft Configuration Methods ---

        // Retrieves all active configurations for a specific aircraft.
        Task<ServiceResult<IEnumerable<AircraftConfigDto>>> GetConfigsForAircraftAsync(string tailNumber);

        // Retrieves the details of a single configuration, including its cabin classes and seat counts.
        Task<ServiceResult<AircraftConfigDto>> GetConfigDetailsAsync(int configId);

        // Creates a new configuration (layout) for an aircraft.
        Task<ServiceResult<AircraftConfigDto>> CreateAircraftConfigAsync(string tailNumber, CreateAircraftConfigDto createDto);

        // Updates an existing aircraft configuration's name.
        Task<ServiceResult<AircraftConfigDto>> UpdateAircraftConfigAsync(int configId, UpdateAircraftConfigDto updateDto);

        // Soft deletes an aircraft configuration after checking dependencies (cabin classes).
        Task<ServiceResult> DeleteAircraftConfigAsync(int configId);

        // --- Cabin Class Methods ---

        // Retrieves all active cabin classes for a specific configuration.
        Task<ServiceResult<IEnumerable<CabinClassDto>>> GetCabinClassesForConfigAsync(int configId);

        // Creates a new cabin class (e.g., "Economy") within a configuration.
        Task<ServiceResult<CabinClassDto>> CreateCabinClassAsync(CreateCabinClassDto createDto);

        // Soft deletes a cabin class after checking dependencies (seats).
        Task<ServiceResult> DeleteCabinClassAsync(int cabinClassId);

        // Retrieves all seats for a specific cabin class (useful for seat map display).
        Task<ServiceResult<IEnumerable<object>>> GetSeatMapForCabinClassAsync(int cabinClassId); // DTO for Seat needed

        Task<ServiceResult<SeatDto>> CreateSeatAsync(CreateSeatDto createDto);

        Task<ServiceResult<SeatDto>> UpdateSeatAsync(string seatId, UpdateSeatDto updateDto);

        Task<ServiceResult> DeleteSeatAsync(string seatId);
    }


}