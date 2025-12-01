using Application.Services.Interfaces;
using Application.Services;
using Application.Services.Auth;  
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.ExternalServices.PaymentsService;


namespace Presentation.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServiceDependencies(this IServiceCollection services)
        {
             

            // --- Core & Management Services ---  

            services.AddScoped<ICountryService, CountryService>();
            
            services.AddScoped<IAirportService, AirportService>();
            
            services.AddScoped<IAirlineService, AirlineService>();
            
            services.AddScoped<IAircraftTypeService, AircraftTypeService>();
            
            services.AddScoped<IAircraftManagementService, AircraftManagementService>();
            
            services.AddScoped<IRouteManagementService, RouteManagementService>();
            
            services.AddScoped<IFareBasisCodeService, FareBasisCodeService>();
            
            services.AddScoped<IContextualPricingService, ContextualPricingService>();
            
            services.AddScoped<IPricingService, PricingService>();  
            
            services.AddScoped<IFlightSchedulingService, FlightSchedulingService>();
            
            services.AddScoped<IFlightOperationsService, FlightOperationsService>();
            
            services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
            
            services.AddScoped<ICrewManagementService, CrewManagementService>();
            
            services.AddScoped<ICrewSchedulingService, CrewSchedulingService>();
            
            services.AddScoped<IPriceOfferLogService, PriceOfferLogService>();


            // --- Booking Flow Services ---
            services.AddScoped<IFlightService, FlightService>();  
            
            services.AddScoped<IPassengerService, PassengerService>();  
            
            services.AddScoped<IFrequentFlyerService, FrequentFlyerService>(); 
            
            services.AddScoped<IBookingService, BookingService>();  
            
            services.AddScoped<ISeatService, SeatService>(); 
            
            services.AddScoped<IAncillaryProductService, AncillaryProductService>();  
             


            services.AddScoped<IPaymentsService, PaymentsService>();

            services.AddScoped<ITicketService, TicketService>();  
            
            services.AddScoped<IBoardingPassService, BoardingPassService>();

            // --- Overall Management & Reporting (Commented Out) ---
            services.AddScoped<IUserManagementService, UserManagementService>();  
          
            services.AddScoped<IReportingService, ReportingService>();          


            return services;
        }
    }
}