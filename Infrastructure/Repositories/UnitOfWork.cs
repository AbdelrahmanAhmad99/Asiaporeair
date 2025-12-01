using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories.Common;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Private backing fields for lazy initialization
        private IAircraftConfigRepository? _aircraftConfigs;
        private IAircraftRepository? _aircrafts;
        private IAircraftTypeRepository? _aircraftTypes;
        private IAirlineRepository? _airlines;
        private IAirportRepository? _airports;
        private IAncillaryProductRepository? _ancillaryProducts;
        private IAncillarySaleRepository? _ancillarySales;
        private IBoardingPassRepository? _boardingPasses;
        private IBookingPassengerRepository? _bookingPassengers;
        private IBookingRepository? _bookings;
        private ICabinClassRepository? _cabinClasses;
        private ICertificationRepository? _certifications;
        private IContextualPricingAttributesRepository? _contextualPricingAttributes;
        private ICountryRepository? _countries;
        private ICrewMemberRepository? _crewMembers;
        private IEmployeeRepository? _employees;
        private IFareBasisCodeRepository? _fareBasisCodes;
        private IFlightCrewRepository? _flightCrews;
        private IFlightInstanceRepository? _flightInstances;
        private IFlightLegDefRepository? _flightLegDefs;
        private IFlightScheduleRepository? _flightSchedules;
        private IFrequentFlyerRepository? _frequentFlyers;
        private IPassengerRepository? _passengers;
        private IPaymentRepository? _payments; 
        private IPriceOfferLogRepository? _priceOfferLogs;
        private IRouteOperatorRepository? _routeOperators;
        private IRouteRepository? _routes;
        private ISeatRepository? _seats;
        private ITicketRepository? _tickets;
        private IUserRepository? _users;
        private IPilotRepository _pilots;
        public UnitOfWork(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --- Identity Managers ---
        public UserManager<AppUser> UserManager => _userManager;
        public RoleManager<IdentityRole> RoleManager => _roleManager;

        // --- Repositories as Lazy Properties ---
        public IAircraftConfigRepository AircraftConfigs => _aircraftConfigs ??= new AircraftConfigRepository(_context);
        public IAircraftRepository Aircrafts => _aircrafts ??= new AircraftRepository(_context);
        public IAircraftTypeRepository AircraftTypes => _aircraftTypes ??= new AircraftTypeRepository(_context);
        public IAirlineRepository Airlines => _airlines ??= new AirlineRepository(_context);
        public IAirportRepository Airports => _airports ??= new AirportRepository(_context);
        public IAncillaryProductRepository AncillaryProducts => _ancillaryProducts ??= new AncillaryProductRepository(_context);
        public IAncillarySaleRepository AncillarySales => _ancillarySales ??= new AncillarySaleRepository(_context);
        public IBoardingPassRepository BoardingPasses => _boardingPasses ??= new BoardingPassRepository(_context);
        public IBookingPassengerRepository BookingPassengers => _bookingPassengers ??= new BookingPassengerRepository(_context);
        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
        public ICabinClassRepository CabinClasses => _cabinClasses ??= new CabinClassRepository(_context);
        public ICertificationRepository Certifications => _certifications ??= new CertificationRepository(_context);
        public IContextualPricingAttributesRepository ContextualPricingAttributes => _contextualPricingAttributes ??= new ContextualPricingAttributesRepository(_context);
        public ICountryRepository Countries => _countries ??= new CountryRepository(_context);
        public ICrewMemberRepository CrewMembers => _crewMembers ??= new CrewMemberRepository(_context);
        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
        public IFareBasisCodeRepository FareBasisCodes => _fareBasisCodes ??= new FareBasisCodeRepository(_context);
        public IFlightCrewRepository FlightCrews => _flightCrews ??= new FlightCrewRepository(_context);
        public IFlightInstanceRepository FlightInstances => _flightInstances ??= new FlightInstanceRepository(_context);
        public IFlightLegDefRepository FlightLegDefs => _flightLegDefs ??= new FlightLegDefRepository(_context);
        public IFlightScheduleRepository FlightSchedules => _flightSchedules ??= new FlightScheduleRepository(_context);
        public IFrequentFlyerRepository FrequentFlyers => _frequentFlyers ??= new FrequentFlyerRepository(_context);
        public IPassengerRepository Passengers => _passengers ??= new PassengerRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
        public IPriceOfferLogRepository PriceOfferLogs => _priceOfferLogs ??= new PriceOfferLogRepository(_context);
        public IRouteOperatorRepository RouteOperators => _routeOperators ??= new RouteOperatorRepository(_context);
        public IRouteRepository Routes => _routes ??= new RouteRepository(_context);
        public ISeatRepository Seats => _seats ??= new SeatRepository(_context);
        public ITicketRepository Tickets => _tickets ??= new TicketRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context, _userManager);
        public IPilotRepository Pilots => _pilots ??= new PilotRepository(_context);



        /// <summary> 
        // This starts a new database transaction. 
        // This allows for the rollback of all changes if one part of the process fails. 
        // </summary>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /// <summary> 
        // Clears all tracked entities from Entity Framework memory. 
        // This is useful when we want to ensure that the next query will fetch completely new data from the database 
        // instead of using cached data stored in memory. 
        // </summary>
        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }

         
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Dispose method is often handled by Dependency Injection lifetime scope (e.g., Scoped)
        // but explicit implementation is also fine.
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}