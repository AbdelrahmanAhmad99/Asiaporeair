using Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    public interface IUserRepository
    { 

        // --- Core User Retrieval ---
        Task<AppUser?> GetByIdAsync(string userId);
        Task<AppUser?> GetByEmailAsync(string email);
        Task<string?> GetUserIdFromClaimsPrincipalAsync(ClaimsPrincipal user);
        Task<bool> CheckPasswordAsync(AppUser user, string password);

        // --- Role Profile Retrieval ---
        Task<User?> GetPassengerProfileByUserIdAsync(string userId); 
        Task<Admin?> GetAdminProfileByUserIdAsync(string userId);
        Task<SuperAdmin?> GetSuperAdminProfileByUserIdAsync(string userId);
        Task<Supervisor?> GetSupervisorProfileByUserIdAsync(string userId);
        Task<Pilot?> GetPilotProfileByUserIdAsync(string userId);
        Task<Attendant?> GetAttendantProfileByUserIdAsync(string userId);
        Task<Employee?> GetEmployeeByUserIdAsync(string userId);  

        // --- Role Profile Update Tracking ---
        // These methods mark the specific profile entity as modified for UnitOfWork
        void TrackUserForUpdate(AppUser user); // For AppUser updates via UserManager
        void TrackEmployeeForUpdate(Employee employee);
        void TrackCrewMemberForUpdate(CrewMember crewMember); // Needed for Pilot/Attendant
        void TrackPassengerProfileForUpdate(User passengerProfile);
        void TrackPilotProfileForUpdate(Pilot pilotProfile);
        void TrackAdminProfileForUpdate(Admin adminProfile);
        void TrackSuperAdminProfileForUpdate(SuperAdmin superAdminProfile);
        void TrackSupervisorProfileForUpdate(Supervisor supervisorProfile);
        void TrackAttendantProfileForUpdate(Attendant attendantProfile);


        // ---  Methods for Listing and Management ---

        /// <summary> Retrieves all active (not soft-deleted) AppUser entities. </summary>
        /// <returns> An enumerable collection of active AppUsers. </returns>
        Task<IEnumerable<AppUser>> GetAllActiveUsersAsync();

        /// <summary> Retrieves all AppUser entities, including those marked as soft-deleted. </summary>
        /// <returns> An enumerable collection of all AppUsers. </returns>
        Task<IEnumerable<AppUser>> GetAllUsersIncludingDeletedAsync();

        /// <summary> Retrieves a paginated list of active AppUsers, optionally filtered by role (UserType). </summary>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of users per page.</param>
        /// <param name="userTypeFilter">Optional filter by UserType enum.</param>
        /// <returns>A tuple containing the list of users for the page and the total count of matching users.</returns>
        Task<(IEnumerable<AppUser> Items, int TotalCount)> GetPaginatedUsersAsync(int pageNumber, int pageSize, Domain.Enums.UserType? userTypeFilter = null);

        /// <summary> Finds active AppUsers whose first or last name contains the specified text (case-insensitive). </summary>
        /// <param name="nameSubstring">The text to search for.</param>
        /// <returns>An enumerable collection of matching active AppUsers.</returns>
        Task<IEnumerable<AppUser>> FindUsersByNameAsync(string nameSubstring);

        /// <summary> Marks an AppUser as soft-deleted. </summary>
        /// <param name="userId">The ID of the user to soft delete.</param>
        /// <returns>True if the user was found and marked for deletion, false otherwise.</returns>
        Task<bool> SoftDeleteUserAsync(string userId);

        /// <summary> Reactivates a soft-deleted AppUser. </summary>
        /// <param name="userId">The ID of the user to reactivate.</param>
        /// <returns>True if the user was found and marked for reactivation, false otherwise.</returns>
        Task<bool> ReactivateUserAsync(string userId);

        /// <summary> Retrieves all active Pilot profiles, including AppUser and CrewMember details. </summary>
        /// <returns> An enumerable collection of active Pilot profiles. </returns>
        Task<IEnumerable<Pilot>> GetAllActivePilotsAsync();

        /// <summary> Retrieves all active Admin profiles, including AppUser and Employee details. </summary>
        /// <returns> An enumerable collection of active Admin profiles. </returns>
        Task<IEnumerable<Admin>> GetAllActiveAdminsAsync();

        /// <summary> Retrieves all active Supervisor profiles, including AppUser and Employee details. </summary>
        /// <returns> An enumerable collection of active Supervisor profiles. </returns>
        Task<IEnumerable<Supervisor>> GetAllActiveSupervisorsAsync();

        /// <summary> Retrieves all active Attendant profiles, including AppUser, CrewMember and Employee details. </summary>
        /// <returns> An enumerable collection of active Attendant profiles. </returns>
        Task<IEnumerable<Attendant>> GetAllActiveAttendantsAsync();

        /// <summary> Gets the count of active users for a specific role (UserType). </summary>
        /// <param name="userType">The UserType enum value.</param>
        /// <returns>The number of active users with the specified role.</returns>
        Task<int> GetUserCountByRoleAsync(Domain.Enums.UserType userType);

        /// <summary> Gets the count of all active users. </summary>
        /// <returns>The total number of active users.</returns>
        Task<int> GetTotalActiveUserCountAsync();
         
        // Retrieves the User (passenger/customer) profile linked to an AppUser ID.
        Task<User?> GetUserByAppUserIdAsync(string appUserId);
         
        // Retrieves the User profile by its own primary key (int UserId).
        Task<User?> GetUserProfileByIdAsync(int userId);
         
        // Retrieves the User profile linked to a specific FrequentFlyer ID.
        Task<User?> GetByFrequentFlyerIdAsync(int flyerId);


    }
}