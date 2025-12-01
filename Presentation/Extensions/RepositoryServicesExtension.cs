using Domain.Repositories.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Application.Services.Interfaces;  
namespace Presentation.Extensions
{
    public static class RepositoryServicesExtension
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Register Unit of Work (Scoped lifetime ensures DbContext is shared per request)
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register ALL Repositories (Alphabetical Order for maintainability)
            services.AddScoped<IAircraftConfigRepository, AircraftConfigRepository>();

            services.AddScoped<IAircraftRepository, AircraftRepository>();

            services.AddScoped<IAircraftTypeRepository, AircraftTypeRepository>();
            
            services.AddScoped<IAirlineRepository, AirlineRepository>();
            
            services.AddScoped<IAirportRepository, AirportRepository>();
            
            services.AddScoped<IAncillaryProductRepository, AncillaryProductRepository>();
            
            services.AddScoped<IAncillarySaleRepository, AncillarySaleRepository>();
            
            services.AddScoped<IBoardingPassRepository, BoardingPassRepository>();
            
            services.AddScoped<IBookingPassengerRepository, BookingPassengerRepository>();
            
            services.AddScoped<IBookingRepository, BookingRepository>();
            
            services.AddScoped<ICabinClassRepository, CabinClassRepository>();
            
            services.AddScoped<ICertificationRepository, CertificationRepository>();
            
            services.AddScoped<IContextualPricingAttributesRepository, ContextualPricingAttributesRepository>();
            
            services.AddScoped<ICountryRepository, CountryRepository>();
            
            services.AddScoped<ICrewMemberRepository, CrewMemberRepository>();
            
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            
            services.AddScoped<IFareBasisCodeRepository, FareBasisCodeRepository>();
            
            services.AddScoped<IFlightCrewRepository, FlightCrewRepository>();
            
            services.AddScoped<IFlightInstanceRepository, FlightInstanceRepository>();
            
            services.AddScoped<IFlightLegDefRepository, FlightLegDefRepository>();
            
            services.AddScoped<IFlightScheduleRepository, FlightScheduleRepository>();
            
            services.AddScoped<IFrequentFlyerRepository, FrequentFlyerRepository>();
            
            services.AddScoped<IPassengerRepository, PassengerRepository>();
            
            services.AddScoped<IPaymentRepository, PaymentRepository>();  

            services.AddScoped<IPilotRepository, PilotRepository>();  

            services.AddScoped<IPriceOfferLogRepository, PriceOfferLogRepository>();
            
            services.AddScoped<IRouteOperatorRepository, RouteOperatorRepository>();
            
            services.AddScoped<IRouteRepository, RouteRepository>();
            
            services.AddScoped<ISeatRepository, SeatRepository>();
            
            services.AddScoped<ITicketRepository, TicketRepository>();
            
            services.AddScoped<IUserRepository, UserRepository>(); 


            return services;
        }
    }
}