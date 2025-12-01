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
    public class FlightCrewRepository : GenericRepository<FlightCrew>, IFlightCrewRepository
    {
        public FlightCrewRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<FlightCrew?> GetActiveByIdAsync(int flightInstanceId, int crewMemberEmployeeId)
        {
            // Use FindAsync for composite key lookup, then check IsDeleted
            var flightCrew = await _dbSet.FindAsync(flightInstanceId, crewMemberEmployeeId);
            return (flightCrew != null && !flightCrew.IsDeleted) ? flightCrew : null;
        }
         
        public async Task<IEnumerable<FlightCrew>> GetCrewForFlightAsync(int flightInstanceId)
        {
            return await _dbSet
                .Include(fc => fc.CrewMember)  
                    .ThenInclude(cm => cm.Employee.AppUser)  
                .Where(fc => fc.FlightInstanceId == flightInstanceId && !fc.IsDeleted)
                .OrderBy(fc => fc.Role) // Order by role (e.g., Captain, First Officer, Attendant)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<FlightCrew>> GetFlightsForCrewMemberAsync(int crewMemberEmployeeId)
        {
            return await _dbSet
                .Include(fc => fc.FlightInstance) // Include FlightInstance
                    .ThenInclude(fi => fi.Schedule.Route) // Include Route details
                .Include(fc => fc.FlightInstance.Schedule.Airline) // Include Airline
                .Where(fc => fc.CrewMemberId == crewMemberEmployeeId && !fc.IsDeleted)
                .OrderBy(fc => fc.FlightInstance.ScheduledDeparture) // Order by flight departure time
                .ToListAsync();
        }
         
        public async Task<IEnumerable<FlightCrew>> GetAssignmentsForCrewMemberByDateRangeAsync(int crewMemberEmployeeId, DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);

            return await _dbSet 
                .Include(fc => fc.FlightInstance) 
                    .ThenInclude(fi => fi.Schedule) 
                        .ThenInclude(s => s.Route) 
                            .ThenInclude(r => r.OriginAirport)

                .Include(fc => fc.FlightInstance)
                    .ThenInclude(fi => fi.Schedule)
                        .ThenInclude(s => s.Route)
                            .ThenInclude(r => r.DestinationAirport)
                             
                .Include(fc => fc.FlightInstance)
                    .ThenInclude(fi => fi.Schedule)
                        .ThenInclude(s => s.AircraftType) 

                .Where(fc => fc.CrewMemberId == crewMemberEmployeeId &&
                              fc.FlightInstance.ScheduledDeparture >= startDate.Date &&
                              fc.FlightInstance.ScheduledDeparture < exclusiveEndDate &&
                              !fc.IsDeleted)
                .OrderBy(fc => fc.FlightInstance.ScheduledDeparture)
                .ToListAsync();
        }
         
        public async Task AddMultipleAssignmentsAsync(IEnumerable<FlightCrew> flightCrewAssignments)
        {
            await _dbSet.AddRangeAsync(flightCrewAssignments);
        }
         
        public Task RemoveMultipleAssignmentsAsync(IEnumerable<FlightCrew> flightCrewAssignments)
        {
            // Check if the entity supports soft delete
            var hasSoftDelete = typeof(FlightCrew).GetProperty("IsDeleted")?.PropertyType == typeof(bool);

            if (hasSoftDelete)
            {
                foreach (var assignment in flightCrewAssignments)
                {
                    SoftDelete(assignment); // Use the GenericRepository's SoftDelete method
                }
            }
            else
            {
                _dbSet.RemoveRange(flightCrewAssignments); // Hard delete if no soft delete
            }
            // SaveChangesAsync called by UnitOfWork
            return Task.CompletedTask;
        }

         
        public async Task<IEnumerable<FlightCrew>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
         
        public async Task<IEnumerable<FlightCrew>> GetAllActiveAsync()
        {
            return await _dbSet.Where(fc => !fc.IsDeleted).ToListAsync();
        }
         
        public async Task<bool> ExistsAssignmentAsync(int flightInstanceId, int crewMemberEmployeeId)
        {
            return await _dbSet.AnyAsync(fc => fc.FlightInstanceId == flightInstanceId &&
                                               fc.CrewMemberId == crewMemberEmployeeId &&
                                               !fc.IsDeleted);
        }
         
        public override async Task<IEnumerable<FlightCrew>> GetAllAsync()
        {
            return await _dbSet.Where(fc => !fc.IsDeleted).ToListAsync();
        }
    }
}