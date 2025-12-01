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
    public class AirlineRepository : GenericRepository<Airline>, IAirlineRepository
    {
        public AirlineRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<Airline?> GetByIataCodeAsync(string iataCode)
        {
            // Use FirstOrDefaultAsync with the primary key and IsDeleted check
            return await _dbSet
                .Where(a => a.IataCode == iataCode && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<Airline>> FindByNameAsync(string name)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && EF.Functions.Like(a.Name, $"%{name}%"))
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Airline>> GetByBaseAirportAsync(string airportIataCode)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.BaseAirportId == airportIataCode)
                .Include(a => a.BaseAirport)  
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Airline>> GetByOperatingRegionAsync(string region)
        {
            var upperRegion = region.ToUpper();
            return await _dbSet
                .Where(a => !a.IsDeleted && a.OperatingRegion.ToUpper() == upperRegion)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Airline>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.ToListAsync();
        }
         
        public async Task<IEnumerable<Airline>> GetAllActiveAsync()
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.BaseAirport)
                .ToListAsync();
        }
         
        public async Task<Airline?> GetWithBaseAirportAsync(string iataCode)
        {
            return await _dbSet
                .Include(a => a.BaseAirport)  
                .Where(a => a.IataCode == iataCode && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<Airline?> GetWithAircraftAsync(string iataCode)
        {
            return await _dbSet 
                 .Include(a => a.Aircrafts)
                     .ThenInclude(ac => ac.AircraftType)
                 .Include(a => a.BaseAirport)  
                 .Where(a => a.IataCode == iataCode && !a.IsDeleted)
                 .FirstOrDefaultAsync();
        }
         
        public async Task<bool> ExistsByIataCodeAsync(string iataCode)
        {
            return await _dbSet.AnyAsync(a => a.IataCode == iataCode);
        }
         
        public async Task<bool> ExistsByNameAsync(string name)
        {
            var upperName = name.ToUpper();
            return await _dbSet.AnyAsync(a => a.Name.ToUpper() == upperName);
        }
         
        public override async Task<IEnumerable<Airline>> GetAllAsync()
        {
            return await _dbSet
                 .Where(a => !a.IsDeleted)
                 .Include(a => a.BaseAirport)
                 .ToListAsync();
        }
    }
}