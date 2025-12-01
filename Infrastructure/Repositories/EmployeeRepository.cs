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
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<Employee?> GetActiveByIdAsync(int employeeId)
        {
            var employee = await _dbSet.FindAsync(employeeId); 
            return (employee != null && !employee.IsDeleted) ? employee : null;
        }
         
        public async Task<Employee?> GetByAppUserIdAsync(string appUserId)
        {
            return await _dbSet
                .Include(e => e.AppUser)
                .Include(e => e.CrewMember)
                .Where(e => e.AppUserId == appUserId && !e.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<Employee>> GetByHireDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Include(e => e.AppUser)
                  .Include(e => e.CrewMember)
                .Where(e => e.DateOfHire.HasValue &&
                             e.DateOfHire.Value >= startDate.Date &&
                             e.DateOfHire.Value < exclusiveEndDate &&
                             !e.IsDeleted)
                .OrderBy(e => e.DateOfHire)
                .ToListAsync();
        }

        
        public async Task<IEnumerable<Employee>> FindByNameAsync(string nameSubstring)
        {
            return await _dbSet
                .Include(e => e.AppUser)
                  .Include(e => e.CrewMember)
                .Where(e => !e.IsDeleted &&
                            (EF.Functions.Like(e.AppUser.FirstName, $"%{nameSubstring}%") ||
                             EF.Functions.Like(e.AppUser.LastName, $"%{nameSubstring}%")))
                .OrderBy(e => e.AppUser.LastName).ThenBy(e => e.AppUser.FirstName)
                .ToListAsync();
        }

        public async Task<Employee?> GetWithRoleDetailsAsync(int employeeId)
        {
             return await _dbSet
                .Include(e => e.AppUser)
                .Include(e => e.CrewMember) 
                    .ThenInclude(cm => cm.Pilot)  
                .Include(e => e.CrewMember)
                    .ThenInclude(cm => cm.Attendant)  
                .Include(e => e.Admin)  
                .Include(e => e.SuperAdmin)  
                .Include(e => e.Supervisor)  
                .Where(e => e.EmployeeId == employeeId && !e.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<Employee>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                                .Include(e => e.AppUser)
                                .Include(e => e.CrewMember)
                                .ToListAsync();
        }
         
        public async Task<IEnumerable<Employee>> GetAllActiveWithAppUserAsync()
        {
            return await _dbSet
                .Include(e => e.AppUser)
                  .Include(e => e.CrewMember)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.AppUser.LastName).ThenBy(e => e.AppUser.FirstName)
                .ToListAsync();
        }

        public async Task<bool> ExistsByAppUserIdAsync(string appUserId)
        {
            return await _dbSet.AnyAsync(e => e.AppUserId == appUserId);
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet
                .Include(e => e.AppUser)
                  .Include(e => e.CrewMember)
                .Where(e => !e.IsDeleted).ToListAsync();
        }
    }
}