using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data; 
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{ 
    public class CrewMemberRepository : GenericRepository<CrewMember>, ICrewMemberRepository
    {
        public CrewMemberRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<CrewMember?> GetActiveByEmployeeIdAsync(int employeeId)
        {
            // FindAsync works directly with the PK which is EmployeeId for CrewMember 
                 return await _dbSet
             .Include(cm => cm.Employee)
                 .ThenInclude(e => e.AppUser)
             .Where(cm => cm.EmployeeId == employeeId && !cm.IsDeleted)
             .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<CrewMember>> GetByBaseAirportAsync(string airportIataCode)
        {
            return await _dbSet
                .Include(cm => cm.Employee.AppUser) // Include Employee and AppUser for details
                .Where(cm => cm.CrewBaseAirportId == airportIataCode && !cm.IsDeleted)
                .OrderBy(cm => cm.Employee.AppUser.LastName) // Order by name
                .ToListAsync();
        }
         
        public async Task<IEnumerable<CrewMember>> GetByPositionAsync(string position)
        {
            var upperPosition = position.ToUpper();
            return await _dbSet
                .Include(cm => cm.Employee.AppUser)
                .Where(cm => cm.Position.ToUpper() == upperPosition && !cm.IsDeleted)
                .OrderBy(cm => cm.Employee.AppUser.LastName)
                .ToListAsync();
        }
         
        public async Task<CrewMember?> GetWithEmployeeDetailsAsync(int employeeId)
        {
            return await _dbSet
                .Include(cm => cm.CrewBaseAirport)
                    .ThenInclude(e => e.Country)
                .Include(cm => cm.Employee) // Eager load Employee
                    .ThenInclude(e => e.AppUser)  
                .Where(cm => cm.EmployeeId == employeeId && !cm.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<CrewMember?> GetWithCertificationsAsync(int employeeId)
        {
            return await _dbSet
                .Include(cm => cm.Certifications.Where(c => !c.IsDeleted)) // Eager load active Certifications
                .Where(cm => cm.EmployeeId == employeeId && !cm.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<CrewMember>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                                .Include(cm => cm.Employee.AppUser)  
                                .ToListAsync();
        }
         
        public async Task<IEnumerable<CrewMember>> GetAllActiveWithEmployeeAsync()
        {
            return await _dbSet
                .Include(cm => cm.Employee.AppUser)  
                .Where(cm => !cm.IsDeleted)
                .OrderBy(cm => cm.Employee.AppUser.LastName)
                .ToListAsync();
        }
         
        public async Task<bool> ExistsByEmployeeIdAsync(int employeeId)
        {
            return await _dbSet.AnyAsync(cm => cm.EmployeeId == employeeId && !cm.IsDeleted);
        } 
        public async Task<IEnumerable<CrewMember>> FindAvailableCrewAsync(string? requiredPosition = null, string? baseAirportIataCode = null)
        {
            var query = _dbSet.Include(cm => cm.Employee.AppUser)
                              .Where(cm => !cm.IsDeleted);

            if (!string.IsNullOrEmpty(requiredPosition))
            { 
                var upperPosition = requiredPosition.ToUpper();
                query = query.Where(cm => cm.Position.ToUpper() == upperPosition);
            }
            if (!string.IsNullOrEmpty(baseAirportIataCode))
            {
                query = query.Where(cm => cm.CrewBaseAirportId == baseAirportIataCode);
            }

            // Further filtering based on current assignments would happen in the service layer
            return await query.OrderBy(cm => cm.Employee.AppUser.LastName).ToListAsync();
        }
         
        public override async Task<IEnumerable<CrewMember>> GetAllAsync()
        {
            return await _dbSet.Where(cm => !cm.IsDeleted).ToListAsync();
        }
    }
}