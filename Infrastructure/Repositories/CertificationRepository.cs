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
    public class CertificationRepository : GenericRepository<Certification>, ICertificationRepository
    {
        public CertificationRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<Certification?> GetActiveByIdAsync(int certificationId)
        {
            var certification = await _dbSet.FindAsync(certificationId);
            return (certification != null && !certification.IsDeleted) ? certification : null;
        }
         
        public async Task<IEnumerable<Certification>> GetByCrewMemberAsync(int crewMemberEmployeeId)
        {
            return await _dbSet
                .Include(c => c.CrewMember)  
                    .ThenInclude(cm => cm.Employee.AppUser)  
                .Where(c => c.CrewMemberId == crewMemberEmployeeId && !c.IsDeleted)
                .OrderBy(c => c.ExpiryDate)  
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Certification>> GetByTypeAsync(string certificationType)
        {
            return await _dbSet
                .Include(c => c.CrewMember.Employee.AppUser)  
                .Where(c => c.Type.Equals(certificationType, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted)
                .OrderBy(c => c.CrewMember.Employee.AppUser.LastName)  
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Certification>> GetExpiringSoonAsync(int daysUntilExpiry)
        {
            var expiryCutoffDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
            var today = DateTime.UtcNow.Date;

            return await _dbSet
                .Include(c => c.CrewMember.Employee.AppUser)
                .Where(c => c.ExpiryDate.HasValue &&
                             c.ExpiryDate.Value >= today && // Ensure it hasn't already expired
                             c.ExpiryDate.Value <= expiryCutoffDate &&
                             !c.IsDeleted)
                .OrderBy(c => c.ExpiryDate) // Order by soonest expiry
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Certification>> GetExpiredAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet
                .Include(c => c.CrewMember.Employee.AppUser)
                .Where(c => c.ExpiryDate.HasValue && c.ExpiryDate.Value < today && !c.IsDeleted)
                .OrderByDescending(c => c.ExpiryDate) // Show most recently expired first
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Certification>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().Include(c => c.CrewMember).ToListAsync();
        }
         
        public async Task<IEnumerable<Certification>> GetAllActiveAsync()
        {
            return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsForCrewMemberByTypeAsync(int crewMemberEmployeeId, string certificationType)
        {
            return await _dbSet.AnyAsync(c => c.CrewMemberId == crewMemberEmployeeId &&
                                               c.Type.Equals(certificationType, StringComparison.OrdinalIgnoreCase) &&
                                               !c.IsDeleted);
        }
         
        public override async Task<IEnumerable<Certification>> GetAllAsync()
        {
            return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<Certification>> GetExpiredOrExpiringSoonAsync(DateTime expiryDateThreshold)
        {
            var today = DateTime.UtcNow.Date; // Use UtcNow consistently
            return await _dbSet
                .Include(c => c.CrewMember.Employee.AppUser)
                .Where(c => c.ExpiryDate.HasValue &&
                             c.ExpiryDate.Value < expiryDateThreshold && // Expired or expiring soon
                             !c.IsDeleted)
                .OrderBy(c => c.ExpiryDate) // Order by expiry date (expired first)
                .ToListAsync();
        }
    }
}