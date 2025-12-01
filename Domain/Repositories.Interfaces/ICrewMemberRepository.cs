using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for CrewMember entity, extending the generic repository.
    /// Provides methods for querying and managing flight crew members (pilots, attendants).
    /// Links Employee records to flight-specific roles and base airports. Crucial for crew scheduling.
    /// </summary>
    public interface ICrewMemberRepository : IGenericRepository<CrewMember>
    {
        /// <summary>
        /// Retrieves an active crew member by their Employee ID (primary key).
        /// </summary>
        /// <param name="employeeId">The Employee ID acting as the primary key for CrewMember.</param>
        /// <returns>The CrewMember entity if found and active; otherwise, null.</returns>
        Task<CrewMember?> GetActiveByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// Retrieves active crew members based at a specific airport.
        /// Includes Employee and potentially Pilot/Attendant details.
        /// </summary>
        /// <param name="airportIataCode">The 3-letter IATA code of the crew base airport.</param>
        /// <returns>An enumerable collection of active CrewMember entities based at the airport.</returns>
        Task<IEnumerable<CrewMember>> GetByBaseAirportAsync(string airportIataCode);

        /// <summary>
        /// Retrieves active crew members holding a specific position (e.g., 'Pilot', 'Attendant', 'Captain', 'First Officer', 'Cabin Crew'). Case-insensitive.
        /// Includes Employee details.
        /// </summary>
        /// <param name="position">The position title.</param>
        /// <returns>An enumerable collection of active CrewMember entities holding the specified position.</returns>
        Task<IEnumerable<CrewMember>> GetByPositionAsync(string position);

        /// <summary>
        /// Retrieves a specific active crew member, including their detailed Employee information (and potentially linked AppUser).
        /// </summary>
        /// <param name="employeeId">The Employee ID of the crew member.</param>
        /// <returns>The CrewMember entity with detailed Employee information, if found and active; otherwise, null.</returns>
        Task<CrewMember?> GetWithEmployeeDetailsAsync(int employeeId);

        /// <summary>
        /// Retrieves a specific active crew member, including their associated Certifications (eager loading).
        /// </summary>
        /// <param name="employeeId">The Employee ID of the crew member.</param>
        /// <returns>The CrewMember entity with Certifications loaded, if found and active; otherwise, null.</returns>
        Task<CrewMember?> GetWithCertificationsAsync(int employeeId);

        /// <summary>
        /// Retrieves all crew members, including those marked as soft-deleted.
        /// For administrative review or historical records.
        /// </summary>
        /// <returns>An enumerable collection of all CrewMember entities.</returns>
        Task<IEnumerable<CrewMember>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) crew members.
        /// Includes Employee details for basic identification.
        /// </summary>
        /// <returns>An enumerable collection of active CrewMember entities.</returns>
        Task<IEnumerable<CrewMember>> GetAllActiveWithEmployeeAsync();

        /// <summary>
        /// Checks if an active crew member record exists for a given Employee ID.
        /// </summary>
        /// <param name="employeeId">The Employee ID to check.</param>
        /// <returns>True if an active crew member record exists; otherwise, false.</returns>
        Task<bool> ExistsByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// Finds active crew members available for assignment (e.g., not currently assigned to conflicting flights).
        /// Note: Availability logic can be complex and might involve checking FlightCrew assignments against dates/times.
        /// This method provides a basic filter placeholder.
        /// </summary>
        /// <param name="requiredPosition">The position required (optional).</param>
        /// <param name="baseAirportIataCode">The base airport required (optional).</param>
        /// <returns>An enumerable collection of potentially available active CrewMember entities.</returns>
        Task<IEnumerable<CrewMember>> FindAvailableCrewAsync(string? requiredPosition = null, string? baseAirportIataCode = null);
    }
}