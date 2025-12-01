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
    public class RouteRepository : GenericRepository<Route>, IRouteRepository
    {
        public RouteRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<Route?> GetActiveByIdAsync(int routeId)
        {
            return await _dbSet
                .Where(r => r.RouteId == routeId && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<Route>> FindByOriginDestinationAsync(string originIataCode, string destinationIataCode)
        {
            return await _dbSet
                .Where(r => r.OriginAirportId == originIataCode &&
                             r.DestinationAirportId == destinationIataCode &&
                             !r.IsDeleted)
                .Include(r => r.OriginAirport)  
                .Include(r => r.DestinationAirport)  
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Route>> GetByOriginAsync(string originIataCode)
        {
            return await _dbSet
                .Where(r => r.OriginAirportId == originIataCode && !r.IsDeleted)
                .Include(r => r.OriginAirport)
                .Include(r => r.DestinationAirport)  
                .OrderBy(r => r.DestinationAirportId)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Route>> GetByDestinationAsync(string destinationIataCode)
        { 
                return await _dbSet
                .Where(r => r.DestinationAirportId == destinationIataCode && !r.IsDeleted)
                .Include(r => r.OriginAirport)  
                .Include(r => r.DestinationAirport)  
                .OrderBy(r => r.OriginAirportId)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<Route>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set
        }
         
        public async Task<IEnumerable<Route>> GetAllActiveAsync()
        {
            return await _dbSet.Where(r => !r.IsDeleted).ToListAsync();
        }
         
        public async Task<Route?> GetWithAirportsAsync(int routeId)
        {
            return await _dbSet
                .Include(r => r.OriginAirport)
                .Include(r => r.DestinationAirport)
                .Where(r => r.RouteId == routeId && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<Route?> GetWithOperatorsAsync(int routeId)
        {
            return await _dbSet
                .Include(r => r.RouteOperators)
                    .ThenInclude(ro => ro.Airline) // Load Airline through RouteOperator
                .Where(r => r.RouteId == routeId && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        
        public async Task<bool> ExistsBetweenAirportsAsync(string originIataCode, string destinationIataCode)
        {
            return await _dbSet.AnyAsync(r => r.OriginAirportId == originIataCode &&
                                              r.DestinationAirportId == destinationIataCode &&
                                              !r.IsDeleted);
        }
         
        public override async Task<IEnumerable<Route>> GetAllAsync()
        {
            return await _dbSet.Where(r => !r.IsDeleted).ToListAsync();
        }
    }
}