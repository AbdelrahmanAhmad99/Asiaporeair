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
    public class RouteOperatorRepository : GenericRepository<RouteOperator>, IRouteOperatorRepository
    {
        public RouteOperatorRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<RouteOperator?> GetActiveByIdAsync(int routeId, string airlineIataCode)
        {
            // Note: FindAsync does not support Include, so we switch to Where/FirstOrDefaultAsync.
            var routeOperator = await _dbSet
                .Include(ro => ro.Airline)  
                .Where(ro => ro.RouteId == routeId &&
                             ro.AirlineId == airlineIataCode.ToUpper() && // Ensure IATA code is normalized
                             !ro.IsDeleted)
                .FirstOrDefaultAsync();

            return routeOperator;
        } 
        public async Task<IEnumerable<RouteOperator>> GetOperatorsByRouteAsync(int routeId)
        {
            return await _dbSet
                .Include(ro => ro.Airline) // Eager load Airline details
                .Where(ro => ro.RouteId == routeId && !ro.IsDeleted)
                .OrderBy(ro => ro.Airline.Name) // Order by airline name
                .ToListAsync();
        }
         
        public async Task<IEnumerable<RouteOperator>> GetRoutesByOperatorAsync(string airlineIataCode)
        {
            return await _dbSet
                .Include(ro => ro.Route) // Eager load Route details
                    .ThenInclude(r => r.OriginAirport) // Include nested details if needed
                .Include(ro => ro.Route)
                    .ThenInclude(r => r.DestinationAirport)
                .Where(ro => ro.AirlineId == airlineIataCode && !ro.IsDeleted)
                .OrderBy(ro => ro.Route.OriginAirportId).ThenBy(ro => ro.Route.DestinationAirportId) // Order by route
                .ToListAsync();
        }
         
        public async Task<IEnumerable<RouteOperator>> GetCodesharePartnersAsync(int routeId, string operatingAirlineIataCode)
        {
            return await _dbSet
                .Include(ro => ro.Airline)
                .Where(ro => ro.RouteId == routeId &&
                             ro.AirlineId != operatingAirlineIataCode && // Exclude the operating airline
                             ro.CodeshareStatus == true && // Filter for codeshares
                             !ro.IsDeleted)
                .OrderBy(ro => ro.Airline.Name)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<RouteOperator>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set
        }
         
        public async Task<IEnumerable<RouteOperator>> GetAllActiveAsync()
        {
            return await _dbSet.Where(ro => !ro.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsAsync(int routeId, string airlineIataCode)
        {
            return await _dbSet.AnyAsync(ro => ro.RouteId == routeId && ro.AirlineId == airlineIataCode && !ro.IsDeleted);
        }
         
        public override async Task<IEnumerable<RouteOperator>> GetAllAsync()
        {
            return await _dbSet.Where(ro => !ro.IsDeleted).ToListAsync();
        }
    }
}