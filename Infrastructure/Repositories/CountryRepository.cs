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
    public class CountryRepository : GenericRepository<Country>, ICountryRepository
    {
        public CountryRepository(ApplicationDbContext context) : base(context)
        {
            
        }
         
        public async Task<Country?> GetByIsoCodeAsync(string isoCode)
        {
            // Uses FindAsync for primary key lookup, then checks IsDeleted
            var country = await _dbSet.FindAsync(isoCode);
            return (country != null && !country.IsDeleted) ? country : null;
        }
         
        public async Task<Country?> GetByNameAsync(string name)
        { 
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Name.ToUpper() == name.ToUpper())
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<Country>> GetByContinentAsync(string continentName)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Continent.ToUpper() == continentName.ToUpper())
                .OrderBy(c => c.Name) // Optional: order results
                .ToListAsync();
        }
         
         
        public async Task<IEnumerable<Country>> GetAllActiveAsync()
        {
            // Explicitly filter for non-deleted entities
            return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
        }
         
        public async Task<Country?> GetWithAirportsAsync(string isoCode)
        {
            return await _dbSet
                .Include(c => c.Airports)  
                .Where(c => !c.IsDeleted && c.IsoCode == isoCode)
                .FirstOrDefaultAsync();
        }
         
        public async Task<bool> ExistsByIsoCodeAsync(string isoCode)
        {
            // Checks existence without loading the full entity
            return await _dbSet.AnyAsync(c => c.IsoCode == isoCode);
        }
         
        public async Task<bool> ExistsByNameAsync(string name)
        { 
            return await _dbSet.AnyAsync(c => c.Name.ToUpper() == name.ToUpper());
        }
         

        // --- Overriding GetAllAsync to correctly handle IsDeleted ---
        // The base GenericRepository provided had a potential issue where it might not filter.
        // This override ensures active records are returned by default.
        public override async Task<IEnumerable<Country>> GetAllAsync()
        {
            return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
        }
    }
}
