using Domain.Entities;
using Domain.Enums;
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
 
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context) { }

        public async Task AddMultipleAsync(IEnumerable<Ticket> tickets, int bookingId)
        {
            await _context.Tickets.AddRangeAsync(tickets);
        }
        
        public async Task<Ticket?> GetActiveByIdAsync(int ticketId)
        {
            var ticket = await _dbSet.FindAsync(ticketId);
            return (ticket != null && !ticket.IsDeleted) ? ticket : null;
        }

        public async Task<Ticket?> GetByCodeWithDetailsAsync(string ticketCode)
        {
            return await _dbSet 
                .Include(t => t.Booking)
                    .ThenInclude(b => b.User.AppUser)
                .Include(t => t.Booking)
                    .ThenInclude(b => b.FareBasisCode)  
                .Include(t => t.Passenger)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u.FrequentFlyer) 
                .Include(t => t.FlightInstance)
                    .ThenInclude(fi => fi.Schedule)
                        .ThenInclude(s => s.Route)
                            .ThenInclude(r => r.OriginAirport) 
                .Include(t => t.FlightInstance.Schedule.Route.DestinationAirport)  
                .Include(t => t.FlightInstance.Schedule.Airline)  
                .Include(t => t.Seat)
                    .ThenInclude(s => s.CabinClass) 
                .Where(t => t.TicketCode == ticketCode && !t.IsDeleted)
                .FirstOrDefaultAsync();
        }


        public async Task AddMultipleAsync(IEnumerable<Ticket> tickets)  
        {
            await _dbSet.AddRangeAsync(tickets);
        }

        public async Task<PaginatedTicketsResult> GetPaginatedByUserAsync(string userId, int pageNumber, int pageSize) // Existing method, corrected query
        {
            
            var query = _dbSet
                .Include(t => t.Booking)
                    .ThenInclude(b => b.User)  
                        .ThenInclude(u => u.AppUser)  
                .Include(t => t.Passenger)  
                .Include(t => t.FlightInstance.Schedule.Route.OriginAirport)  
                .Include(t => t.FlightInstance.Schedule.Route.DestinationAirport)
                 .Include(t => t.Seat)
                    .ThenInclude(s => s.CabinClass)
                .Where(t => t.Booking.User.AppUserId == userId && !t.IsDeleted);  

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.IssueDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedTicketsResult
            {
                TotalCount = totalCount,
                Tickets = tickets
            };
        }

        public async Task<IEnumerable<Ticket>> GetByBookingAsync(int bookingId) 
        {
            return await _dbSet
                .Include(t => t.Passenger)  
                .Include(t => t.Seat)  
                .Where(t => t.BookingId == bookingId && !t.IsDeleted)
                .OrderBy(t => t.Passenger.LastName)  
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByFlightInstanceAsync(int flightInstanceId)
        {
            return await _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Seat)
                .Where(t => t.FlightInstanceId == flightInstanceId && !t.IsDeleted)
                .OrderBy(t => t.Seat.SeatNumber)  
                .ToListAsync();
        }

         
        public async Task<Ticket?> GetByBookingAndPassengerAsync(int bookingId, int passengerId)
        {
            return await _dbSet
                .Where(t => t.BookingId == bookingId && t.PassengerId == passengerId && !t.IsDeleted)
                .FirstOrDefaultAsync();
        }


        public async Task<IEnumerable<Ticket>> GetByStatusAsync(string status)
        {
            // Convert enum to string for comparison
            return await _dbSet
                .Where(t => t.Status.ToString().ToUpper() == status.ToUpper() && !t.IsDeleted) // Convert enum to string and compare
                .Include(t => t.Booking.FlightInstance.Schedule)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByIssueDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var exclusiveEndDate = endDate.Date.AddDays(1);
            return await _dbSet
                .Where(t => t.IssueDate >= startDate.Date && t.IssueDate < exclusiveEndDate && !t.IsDeleted)
                .Include(t => t.Booking)
                .Include(t => t.Passenger)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }

       
        public async Task<IEnumerable<Ticket>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }

        
        public async Task<IEnumerable<Ticket>> GetAllActiveAsync()
        {
            return await _dbSet.Where(t => !t.IsDeleted).ToListAsync();
        }

        
        public async Task<bool> ExistsByCodeAsync(string ticketCode)
        {
            return await _dbSet.AnyAsync(t => t.TicketCode == ticketCode);
        }

        public async Task<bool> UpdateStatusAsync(int ticketId, string newStatus)
        {
            var ticket = await _dbSet.FindAsync(ticketId);
            if (ticket == null || ticket.IsDeleted)
            {
                return false;  
            }

            if (Enum.TryParse<TicketStatus>(newStatus, true, out TicketStatus parsedStatus))  
            {
                ticket.Status = parsedStatus; 
                Update(ticket);  
                return true;
            }
            else
            {
                return false;  
            }
        }

         
        // Retrieves tickets for a booking, including Passenger details.
        public async Task<IEnumerable<Ticket>> GetByBookingWithDetailsAsync(int bookingId)
        {
            return await _dbSet
                .Include(t => t.Booking)  
                        .ThenInclude(b => b.User) 
                            .ThenInclude(u => u.AppUser)  
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.FareBasisCode)  
                    .Include(t => t.Passenger)  
                        .ThenInclude(p => p.User)  
                            .ThenInclude(u => u.FrequentFlyer)  
                    .Include(t => t.FlightInstance) 
                        .ThenInclude(fi => fi.Schedule)  
                            .ThenInclude(s => s.Route)  
                                .ThenInclude(r => r.OriginAirport) 
                .Include(t => t.Seat)      
                .Where(t => t.BookingId == bookingId && !t.IsDeleted)
                .OrderBy(t => t.Passenger.LastName) // Order consistently
                .ToListAsync();
        }

 
            
            // Retrieves a single ticket by ID with comprehensive related details.
            public async Task<Ticket?> GetWithFullDetailsAsync(int ticketId)
            {
                return await _dbSet
                    .Include(t => t.Booking)     
                        .ThenInclude(b => b.User)  
                            .ThenInclude(u => u.AppUser)  
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.FareBasisCode)  
                    .Include(t => t.Passenger)    
                        .ThenInclude(p => p.User) 
                            .ThenInclude(u => u.FrequentFlyer)  
                    .Include(t => t.FlightInstance) 
                        .ThenInclude(fi => fi.Schedule)  
                            .ThenInclude(s => s.Route)   
                                .ThenInclude(r => r.OriginAirport)  
                    .Include(t => t.FlightInstance.Schedule.Route.DestinationAirport)  
                    .Include(t => t.FlightInstance.Schedule.Airline)  
                    .Include(t => t.Seat)  
                        .ThenInclude(s => s.CabinClass)   
                    .Where(t => t.TicketId == ticketId && !t.IsDeleted) // Filter by ID and active status
                    .FirstOrDefaultAsync();
            }
        public override async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _dbSet.Where(t => !t.IsDeleted).ToListAsync();
        }
    }
}
