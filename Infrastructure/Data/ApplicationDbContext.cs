using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.Metrics;

namespace Infrastructure.Data
{
     
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

         
        public DbSet<Admin> Admins { get; set; }
        public DbSet<SuperAdmin> SuperAdmins { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<User> Users { get; set; }  

         
        // DbSets for business entities 
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<AircraftConfig> AircraftConfigs { get; set; }
        public DbSet<AircraftType> AircraftTypes { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<AncillaryProduct> AncillaryProducts { get; set; }
        public DbSet<AncillarySale> AncillarySales { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Attendant> Attendants { get; set; }
        public DbSet<BoardingPass> BoardingPasses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingPassenger> BookingPassengers { get; set; }
        public DbSet<CabinClass> CabinClasses { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<ContextualPricingAttributes> ContextualPricingAttributes { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<CrewMember> CrewMembers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<FareBasisCode> FareBasisCodes { get; set; }
        public DbSet<FlightCrew> FlightCrews { get; set; }
        public DbSet<FlightInstance> FlightInstances { get; set; }
        public DbSet<FlightLegDef> FlightLegs { get; set; }
        public DbSet<FlightSchedule> FlightSchedules { get; set; }
        public DbSet<FrequentFlyer> FrequentFlyers { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<PriceOfferLog> PriceOfferLogs { get; set; }
        public DbSet<Domain.Entities.Route> Routes { get; set; }
        public DbSet<RouteOperator> RouteOperators { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             
            base.OnModelCreating(modelBuilder);
  
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}