using Domain.Entities;
using Domain.Enums;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
 
        // --- Core User Retrieval Implementations ---
        public async Task<AppUser?> GetByIdAsync(string userId)
        {
            // Eager load related data commonly needed
            return await _userManager.Users
                         .Include(u => u.Employee)
                             .ThenInclude(e => e.CrewMember)  
                         .Include(u => u.User) // Include Passenger Profile
                         .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);  
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _userManager.Users
                        .Include(u => u.Employee)
                            .ThenInclude(e => e.CrewMember)
                        .Include(u => u.User)
                        .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper() && !u.IsDeleted);
        }

        public Task<string?> GetUserIdFromClaimsPrincipalAsync(ClaimsPrincipal user)
        {
            return Task.FromResult(_userManager.GetUserId(user));
        }

        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }


        // --- Role Profile Retrieval Implementations ---
        // These methods retrieve the specific role entity *including* its related AppUser
        // for easier mapping in the AuthService.

        public async Task<User?> GetPassengerProfileByUserIdAsync(string userId)
        {
            return await _context.Users
                         .Include(u => u.AppUser) // Include base user data
                         .FirstOrDefaultAsync(u => u.AppUserId == userId && !u.AppUser.IsDeleted);
        }

        public async Task<Admin?> GetAdminProfileByUserIdAsync(string userId)
        {
            return await _context.Admins
                         .Include(a => a.AppUser)
                         .Include(a => a.Employee)
                         .Include(a => a.AddedBy) // Include the user who added this admin
                         .FirstOrDefaultAsync(a => a.AppUserId == userId && !a.AppUser.IsDeleted);
        }

        public async Task<SuperAdmin?> GetSuperAdminProfileByUserIdAsync(string userId)
        {
            return await _context.SuperAdmins
                         .Include(s => s.AppUser)
                         .Include(s => s.Employee)
                         .FirstOrDefaultAsync(s => s.AppUserId == userId && !s.AppUser.IsDeleted);
        }

        public async Task<Supervisor?> GetSupervisorProfileByUserIdAsync(string userId)
        {
            return await _context.Supervisors
                         .Include(s => s.AppUser)
                         .Include(s => s.Employee)
                         .Include(s => s.AddedBy)
                         .FirstOrDefaultAsync(s => s.AppUserId == userId && !s.AppUser.IsDeleted);
        }

        public async Task<Pilot?> GetPilotProfileByUserIdAsync(string userId)
        {
            return await _context.Pilots
                         .Include(p => p.AppUser)
                         .Include(p => p.CrewMember)
                             .ThenInclude(cm => cm.Employee)
                         .Include(p => p.TypeRating)
                         .Include(p => p.AddedBy)
                         .FirstOrDefaultAsync(p => p.AppUserId == userId && !p.AppUser.IsDeleted);
        }

        public async Task<Attendant?> GetAttendantProfileByUserIdAsync(string userId)
        {
            return await _context.Attendants
                         .Include(a => a.AppUser)
                         .Include(a => a.CrewMember)
                             .ThenInclude(cm => cm.Employee)
                         .Include(a => a.AddedBy)
                         .FirstOrDefaultAsync(a => a.AppUserId == userId && !a.AppUser.IsDeleted);
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
        {
            // Retrieves Employee directly if needed, usually included in role profiles
            return await _context.Employees
                         .Include(e => e.AppUser)
                         .FirstOrDefaultAsync(e => e.AppUserId == userId && !e.AppUser.IsDeleted);
        }

        // --- Role Profile Update Tracking Implementations ---
        // These simply mark the entity state as Modified
        public void TrackUserForUpdate(AppUser user)
        {
            // UserManager handles AppUser updates directly, but explicit tracking can be useful
            _context.Entry(user).State = EntityState.Modified;
        }

        public void TrackEmployeeForUpdate(Employee employee)
        {
            _context.Entry(employee).State = EntityState.Modified;
        }

        public void TrackCrewMemberForUpdate(CrewMember crewMember)
        {
            _context.Entry(crewMember).State = EntityState.Modified;
        }

        public void TrackPassengerProfileForUpdate(User passengerProfile)
        {
            _context.Entry(passengerProfile).State = EntityState.Modified;
        }

        public void TrackPilotProfileForUpdate(Pilot pilotProfile)
        {
            _context.Entry(pilotProfile).State = EntityState.Modified;
        }

        public void TrackAdminProfileForUpdate(Admin adminProfile)
        {
            _context.Entry(adminProfile).State = EntityState.Modified;
        }

        public void TrackSuperAdminProfileForUpdate(SuperAdmin superAdminProfile)
        {
            _context.Entry(superAdminProfile).State = EntityState.Modified;
        }

        public void TrackSupervisorProfileForUpdate(Supervisor supervisorProfile)
        {
            _context.Entry(supervisorProfile).State = EntityState.Modified;
        }

        public void TrackAttendantProfileForUpdate(Attendant attendantProfile)
        {
            _context.Entry(attendantProfile).State = EntityState.Modified;
        }


        public async Task<IEnumerable<AppUser>> GetAllActiveUsersAsync()
        {
            return await _userManager.Users.Where(u => !u.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersIncludingDeletedAsync()
        {
            return await _userManager.Users.IgnoreQueryFilters().ToListAsync();
        }

        public async Task<(IEnumerable<AppUser> Items, int TotalCount)> GetPaginatedUsersAsync(int pageNumber, int pageSize, UserType? userTypeFilter = null)
        {
            var query = _userManager.Users.Where(u => !u.IsDeleted);

            if (userTypeFilter.HasValue)
            {
                query = query.Where(u => u.UserType == userTypeFilter.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<AppUser>> FindUsersByNameAsync(string nameSubstring)
        {
            return await _userManager.Users
                .Where(u => !u.IsDeleted &&
                            (EF.Functions.Like(u.FirstName, $"%{nameSubstring}%") ||
                             EF.Functions.Like(u.LastName, $"%{nameSubstring}%")))
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            // Optionally: Clear sensitive info or lock the account via UserManager
            // await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            var result = await _userManager.UpdateAsync(user); // Use UserManager to update
            return result.Succeeded;
        }

        public async Task<bool> ReactivateUserAsync(string userId)
        {
            // Need to query including deleted users
            var user = await _userManager.Users.IgnoreQueryFilters()
                                          .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.IsDeleted = false;
            // Optionally: Unlock the account if locked during deletion
            // await _userManager.SetLockoutEndDateAsync(user, null);
            var result = await _userManager.UpdateAsync(user); // Use UserManager to update
            return result.Succeeded;
        }

        public async Task<IEnumerable<Pilot>> GetAllActivePilotsAsync()
        {
            return await _context.Pilots
                         .Include(p => p.AppUser)
                         .Include(p => p.CrewMember.Employee)
                         .Where(p => !p.AppUser.IsDeleted)
                         .OrderBy(p => p.AppUser.LastName)
                         .ToListAsync();
        }

        public async Task<IEnumerable<Admin>> GetAllActiveAdminsAsync()
        {
            return await _context.Admins
                         .Include(a => a.AppUser)
                         .Include(a => a.Employee)
                         .Where(a => !a.AppUser.IsDeleted)
                         .OrderBy(a => a.AppUser.LastName)
                         .ToListAsync();
        }

        public async Task<IEnumerable<Supervisor>> GetAllActiveSupervisorsAsync()
        {
            return await _context.Supervisors
                         .Include(s => s.AppUser)
                         .Include(s => s.Employee)
                         .Where(s => !s.AppUser.IsDeleted)
                         .OrderBy(s => s.AppUser.LastName)
                         .ToListAsync();
        }


        public async Task<IEnumerable<Attendant>> GetAllActiveAttendantsAsync()
        {
            return await _context.Attendants
                         .Include(a => a.AppUser)
                         .Include(a => a.CrewMember.Employee)
                         .Where(a => !a.AppUser.IsDeleted)
                         .OrderBy(a => a.AppUser.LastName)
                         .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllActivePassengerProfilesAsync()
        {
            return await _context.Users
                         .Include(u => u.AppUser)
                         .Include(u => u.FrequentFlyer)
                         .Where(u => !u.AppUser.IsDeleted)
                         .OrderBy(u => u.AppUser.LastName)
                         .ToListAsync();
        }

        public async Task<int> GetUserCountByRoleAsync(UserType userType)
        {
            return await _userManager.Users.CountAsync(u => u.UserType == userType && !u.IsDeleted);
        }

        public async Task<int> GetTotalActiveUserCountAsync()
        {
            return await _userManager.Users.CountAsync(u => !u.IsDeleted);
        }

     
        // Retrieves the User (passenger/customer) profile linked to an AppUser ID.
        public async Task<User?> GetUserByAppUserIdAsync(string appUserId)
        {
            // The 'User' entity has a direct FK to AppUserId
            return await _context.Users // DbSet<User> (the passenger profile table)
                                 .Include(u => u.AppUser)  
                                 .Where(u => u.AppUserId == appUserId && !u.IsDeleted && (u.AppUser != null && !u.AppUser.IsDeleted))
                                 .FirstOrDefaultAsync();
        }

        
        // Retrieves the User profile by its own primary key (int UserId).
        public async Task<User?> GetUserProfileByIdAsync(int userId)
        {
            return await _context.Users
                                 .Include(u => u.AppUser) // Include base AppUser details                        
                                 .Include(u => u.FrequentFlyer)
                                 .Where(u => u.UserId == userId && !u.IsDeleted && (u.AppUser != null && !u.AppUser.IsDeleted))
                                 .FirstOrDefaultAsync();
        }

         
        // Retrieves the User profile linked to a specific FrequentFlyer ID.
        public async Task<User?> GetByFrequentFlyerIdAsync(int flyerId)
        {
            return await _context.Users
                                 .Include(u => u.AppUser) // Include base AppUser details
                                 .Where(u => u.FrequentFlyerId == flyerId && !u.IsDeleted && (u.AppUser != null && !u.AppUser.IsDeleted))
                                 .FirstOrDefaultAsync(); // Should be unique if FK constraint is proper
        }
    }
}