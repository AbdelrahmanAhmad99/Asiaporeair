using Application.DTOs.AircraftType;
using Application.Models; // For ServiceResult & PaginatedResult
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Service interface for managing Aircraft Type data.
    public interface IAircraftTypeService
    {
        // Retrieves all active aircraft types, ordered by manufacturer and model.
        Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAllActiveAircraftTypesAsync();

        // Retrieves a specific active aircraft type by its ID.
        Task<ServiceResult<AircraftTypeDto>> GetAircraftTypeByIdAsync(int typeId);

        // Finds active aircraft types where the model name contains the specified text.
        Task<ServiceResult<IEnumerable<AircraftTypeDto>>> FindAircraftTypesByModelAsync(string modelSubstring);

        // Retrieves active aircraft types produced by a specific manufacturer.
        Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAircraftTypesByManufacturerAsync(string manufacturer);

        // Performs an advanced search for aircraft types based on multiple filter criteria. Supports pagination. (Management System)
        Task<ServiceResult<PaginatedResult<AircraftTypeDto>>> SearchAircraftTypesAsync(AircraftTypeFilterDto filter, int pageNumber, int pageSize);

        // Creates a new aircraft type. Validates uniqueness of Model/Manufacturer combination. (Management System)
        Task<ServiceResult<AircraftTypeDto>> CreateAircraftTypeAsync(CreateAircraftTypeDto createDto);

        // Updates an existing aircraft type's details. (Management System)
        Task<ServiceResult<AircraftTypeDto>> UpdateAircraftTypeAsync(int typeId, UpdateAircraftTypeDto updateDto);

        // Soft deletes an aircraft type by its ID. Checks for dependencies (active aircraft, schedules). (Management System)
        Task<ServiceResult> DeleteAircraftTypeAsync(int typeId);

        // Reactivates a soft-deleted aircraft type. (Management System)
        Task<ServiceResult> ReactivateAircraftTypeAsync(int typeId);

        // Retrieves all aircraft types, including soft-deleted ones (for administrative views).
        Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAllAircraftTypesIncludingDeletedAsync();

        // Retrieves aircraft types suitable for a given route distance (Management/Planning).
        Task<ServiceResult<IEnumerable<AircraftTypeDto>>> GetAircraftTypesSuitableForRouteAsync(int routeDistanceKm);
    }
}