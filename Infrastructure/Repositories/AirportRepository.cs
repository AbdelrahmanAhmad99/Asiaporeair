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
    public class AirportRepository : GenericRepository<Airport>, IAirportRepository
    {
        public AirportRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<Airport?> GetByIataCodeAsync(string iataCode)
        {
            // Use FirstOrDefaultAsync with the primary key and IsDeleted check
            return await _dbSet
                .Where(a => a.IataCode == iataCode && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<Airport?> GetByIcaoCodeAsync(string icaoCode)
        {
            return await _dbSet
                .Where(a => a.IcaoCode == icaoCode && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<Airport>> FindByNameAsync(string name)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && EF.Functions.Like(a.Name, $"%{name}%"))  
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Airport>> GetByCityAsync(string city)
        {
            return await _dbSet
                 .Where(a => !a.IsDeleted && a.City.ToUpper() == city.ToUpper())
                 .OrderBy(a => a.Name)
                 .ToListAsync();
        }
         
        public async Task<IEnumerable<Airport>> GetByCountryAsync(string countryIsoCode)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.CountryId == countryIsoCode)
                .Include(a => a.Country) 
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Airport>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.ToListAsync();
        }
         
        public async Task<IEnumerable<Airport>> GetAllActiveAsync()
        { 
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.Country)  
                .ToListAsync();
        }
         
        public async Task<Airport?> GetWithCountryAsync(string iataCode)
        {
            return await _dbSet
                .Include(a => a.Country)  
                .Where(a => a.IataCode == iataCode && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<bool> ExistsByIataCodeAsync(string iataCode)
        {
            return await _dbSet.AnyAsync(a => a.IataCode == iataCode);
        }
         
        public async Task<bool> ExistsByIcaoCodeAsync(string icaoCode)
        {
            return await _dbSet.AnyAsync(a => a.IcaoCode == icaoCode);
        }
         
        public override async Task<IEnumerable<Airport>> GetAllAsync()
        {
            return await _dbSet.Where(a => !a.IsDeleted).ToListAsync();
        }
    }
}