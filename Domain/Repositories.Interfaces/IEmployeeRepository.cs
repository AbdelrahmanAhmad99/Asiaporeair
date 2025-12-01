using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Employee entity, extending the generic repository.
    /// Provides methods for querying and managing general employee data, linking to AspNetUsers (AppUser).
    /// Forms the base for specific roles like Admin, Supervisor, Pilot, Attendant.
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Retrieves an active employee by their unique Employee ID (primary key).
        /// </summary>
        /// <param name="employeeId">The primary key ID of the employee.</param>
        /// <returns>The Employee entity if found and active; otherwise, null.</returns>
        Task<Employee?> GetActiveByIdAsync(int employeeId);

        /// <summary>
        /// Retrieves the active employee record associated with a specific AppUser ID (AspNetUsers ID).
        /// Includes AppUser details.
        /// </summary>
        /// <param name="appUserId">The string GUID ID from AspNetUsers.</param>
        /// <returns>The Employee entity linked to the AppUser, if found and active; otherwise, null.</returns>
        Task<Employee?> GetByAppUserIdAsync(string appUserId);

        /// <summary>
        /// Retrieves active employees hired within a specific date range.
        /// Includes AppUser details. Useful for HR reporting in the management system.
        /// </summary>
        /// <param name="startDate">The start date of the hiring range.</param>
        /// <param name="endDate">The end date of the hiring range.</param>
        /// <returns>An enumerable collection of active Employee entities hired within the range.</returns>
        Task<IEnumerable<Employee>> GetByHireDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Finds active employees whose names (first or last) contain the specified text (case-insensitive).
        /// Relies on the linked AppUser for name information.
        /// </summary>
        /// <param name="nameSubstring">The text to search for within the first or last name.</param>
        /// <returns>An enumerable collection of matching active Employee entities.</returns>
        Task<IEnumerable<Employee>> FindByNameAsync(string nameSubstring);

        /// <summary>
        /// Retrieves an active employee by ID, including detailed related role information
        /// (e.g., CrewMember, Admin, Supervisor details) using eager loading.
        /// </summary>
        /// <param name="employeeId">The ID of the employee.</param>
        /// <returns>The Employee entity with related role details loaded, if found and active; otherwise, null.</returns>
        Task<Employee?> GetWithRoleDetailsAsync(int employeeId);

        /// <summary>
        /// Retrieves all employees, including those marked as soft-deleted. Includes AppUser details.
        /// For administrative review or historical data.
        /// </summary>
        /// <returns>An enumerable collection of all Employee entities.</returns>
        Task<IEnumerable<Employee>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) employees. Includes AppUser details.
        /// </summary>
        /// <returns>An enumerable collection of active Employee entities.</returns>
        Task<IEnumerable<Employee>> GetAllActiveWithAppUserAsync();

        /// <summary>
        /// Checks if an employee record exists for a given AppUser ID (active or soft-deleted).
        /// </summary>
        /// <param name="appUserId">The AppUser ID to check.</param>
        /// <returns>True if an employee record exists for the AppUser; otherwise, false.</returns>
        Task<bool> ExistsByAppUserIdAsync(string appUserId);
    }
}