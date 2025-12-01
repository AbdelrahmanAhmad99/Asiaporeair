using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Identity managers for user authentication and roles
        UserManager<AppUser> UserManager { get; }
        RoleManager<IdentityRole> RoleManager { get; }

        // Core Repositories (Alphabetical Order for better readability)
        IAircraftConfigRepository AircraftConfigs { get; }
        IAircraftRepository Aircrafts { get; }
        IAircraftTypeRepository AircraftTypes { get; }
        IAirlineRepository Airlines { get; }
        IAirportRepository Airports { get; }
        IAncillaryProductRepository AncillaryProducts { get; }
        IAncillarySaleRepository AncillarySales { get; }
        IBoardingPassRepository BoardingPasses { get; }
        IBookingPassengerRepository BookingPassengers { get; }
        IBookingRepository Bookings { get; }
        ICabinClassRepository CabinClasses { get; }
        ICertificationRepository Certifications { get; }
        IContextualPricingAttributesRepository ContextualPricingAttributes { get; }
        ICountryRepository Countries { get; }
        ICrewMemberRepository CrewMembers { get; }
        IEmployeeRepository Employees { get; }
        IFareBasisCodeRepository FareBasisCodes { get; }
        IFlightCrewRepository FlightCrews { get; }
        IFlightInstanceRepository FlightInstances { get; }
        IFlightLegDefRepository FlightLegDefs { get; }
        IFlightScheduleRepository FlightSchedules { get; }
        IFrequentFlyerRepository FrequentFlyers { get; }
        IPassengerRepository Passengers { get; }
        IPaymentRepository Payments { get; } 
        IPriceOfferLogRepository PriceOfferLogs { get; }
        IRouteOperatorRepository RouteOperators { get; }
        IRouteRepository Routes { get; }
        ISeatRepository Seats { get; }
        ITicketRepository Tickets { get; }
        IUserRepository Users { get; }
        IPilotRepository Pilots { get; }


        // Initiating a transaction to ensure data integrity (Rollback in case of error)
        Task<IDbContextTransaction> BeginTransactionAsync();

        // Clear the tracker cache (to reload data directly from the database)
        void ClearChangeTracker();

        // Save changes asynchronously
        Task<int> SaveChangesAsync();
    }
}