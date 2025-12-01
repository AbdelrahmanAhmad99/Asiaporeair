using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Certification entity, extending the generic repository.
    /// Provides methods for querying and managing crew member certifications (e.g., licenses, ratings).
    /// Essential for ensuring crew compliance and qualification tracking in the management system.
    /// </summary>
    public interface ICertificationRepository : IGenericRepository<Certification>
    {
        /// <summary>
        /// Retrieves an active certification by its unique ID.
        /// </summary>
        /// <param name="certificationId">The primary key ID of the certification.</param>
        /// <returns>The Certification entity if found and active; otherwise, null.</returns>
        Task<Certification?> GetActiveByIdAsync(int certificationId);

        /// <summary>
        /// Retrieves all active certifications associated with a specific crew member.
        /// Includes CrewMember and Employee details for context.
        /// </summary>
        /// <param name="crewMemberEmployeeId">The Employee ID of the crew member.</param>
        /// <returns>An enumerable collection of active Certification entities for the crew member.</returns>
        Task<IEnumerable<Certification>> GetByCrewMemberAsync(int crewMemberEmployeeId);

        /// <summary>
        /// Retrieves active certifications of a specific type (case-insensitive).
        /// Useful for finding all crew members with a particular qualification.
        /// </summary>
        /// <param name="certificationType">The type of certification (e.g., "Type Rating A380", "First Aid").</param>
        /// <returns>An enumerable collection of active Certification entities matching the type.</returns>
        Task<IEnumerable<Certification>> GetByTypeAsync(string certificationType);

        /// <summary>
        /// Retrieves active certifications that are expiring within a specified upcoming period.
        /// Crucial for proactive compliance management in the airport system.
        /// Includes CrewMember and Employee details.
        /// </summary>
        /// <param name="daysUntilExpiry">The maximum number of days until the certification expires.</param>
        /// <returns>An enumerable collection of active Certification entities nearing expiry.</returns>
        Task<IEnumerable<Certification>> GetExpiringSoonAsync(int daysUntilExpiry);

        /// <summary>
        /// Retrieves active certifications that have already expired as of today.
        /// Includes CrewMember and Employee details.
        /// </summary>
        /// <returns>An enumerable collection of active but expired Certification entities.</returns>
        Task<IEnumerable<Certification>> GetExpiredAsync();

        /// <summary>
        /// Retrieves all certifications, including those marked as soft-deleted.
        /// For administrative review or historical data.
        /// </summary>
        /// <returns>An enumerable collection of all Certification entities.</returns>
        Task<IEnumerable<Certification>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) certifications.
        /// </summary>
        /// <returns>An enumerable collection of active Certification entities.</returns>
        Task<IEnumerable<Certification>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a specific crew member holds an active certification of a specific type.
        /// </summary>
        /// <param name="crewMemberEmployeeId">The Employee ID of the crew member.</param>
        /// <param name="certificationType">The type of certification (case-insensitive).</param>
        /// <returns>True if the crew member holds the active certification; otherwise, false.</returns>
        Task<bool> ExistsForCrewMemberByTypeAsync(int crewMemberEmployeeId, string certificationType);

        Task<IEnumerable<Certification>> GetExpiredOrExpiringSoonAsync(DateTime expiryDateThreshold);
    }
}