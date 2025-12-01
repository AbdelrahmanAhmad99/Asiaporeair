using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    // Interface specific to Pilot entity data operations.
    public interface IPilotRepository : IGenericRepository<Pilot>
    {
        // Retrieves an active Pilot profile by the linked Employee ID.
        Task<Pilot?> GetByEmployeeIdAsync(int employeeId);

        // Retrieves an active Pilot profile by the linked AppUser ID.
        Task<Pilot?> GetByAppUserIdAsync(string appUserId);

        // Retrieves active Pilots who are type-rated for a specific aircraft type.
        Task<IEnumerable<Pilot>> GetPilotsByTypeRatingAsync(int aircraftTypeId);

        // Retrieves active Pilots based at a specific airport.
        Task<IEnumerable<Pilot>> GetPilotsByBaseAirportAsync(string airportIataCode);

        // Finds active Pilots by license number.
        Task<Pilot?> FindByLicenseNumberAsync(string licenseNumber);

        // Retrieves all Pilot profiles, including soft-deleted ones.
        Task<IEnumerable<Pilot>> GetAllIncludingDeletedAsync();

        // Retrieves all active Pilot profiles with full details (AppUser, Employee, CrewMember, TypeRating).
        Task<IEnumerable<Pilot>> GetAllActiveWithDetailsAsync();

        // Checks if an active Pilot record exists for a given Employee ID.
        Task<bool> ExistsByEmployeeIdAsync(int employeeId);
    }
}