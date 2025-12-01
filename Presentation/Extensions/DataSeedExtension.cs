using Infrastructure.Data.DataSeeding;
using Infrastructure.Data.DataSeeding.Seeders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Presentation.Extensions
{
    /// <summary>
    /// Extension methods for registering and executing the Asiaporeair Data Seeding process.
    /// This ensures clean separation of concerns and controlled environment execution.
    /// </summary>
    public static class DataSeedExtension
    {
        /// <summary>
        /// Registers all individual Seeders and the central orchestrator service.
        /// </summary>
        public static IServiceCollection AddDataSeedServices(this IServiceCollection services)
        {



            // Register the central data orchestrator service
            services.AddScoped<AsiaporeairDataSeed>();

            // Register the individual Seeders as scoped services
            services.AddScoped<RoleSeeder>();
            services.AddScoped<FrequentFlyerSeeder>();
            services.AddScoped<CountrySeeder>();
            services.AddScoped<AircraftTypeSeeder>();
            services.AddScoped<AirportSeeder>();
            services.AddTransient<AirlineSeeder>();
            services.AddTransient<AircraftSeeder>();
            services.AddScoped<AircraftConfigSeeder>();
            services.AddTransient<CabinClassSeeder>();
            services.AddTransient<SeatSeeder>();
            services.AddScoped<RouteSeeder>();
            services.AddScoped<RouteOperatorSeeder>();


            services.AddScoped<FareBasisCodeSeeder>();
            services.AddScoped<AncillaryProductSeeder>();
            services.AddScoped<ContextualPricingAttributesSeeder>(); 
            services.AddTransient<PriceOfferLogSeeder>();

            services.AddScoped<FlightScheduleSeeder>();
            services.AddScoped<FlightInstanceSeeder>();
            services.AddTransient<FlightLegDefSeeder>();

            services.AddScoped<UserSeeder>();
            services.AddScoped<SuperAdminSeeder>();
            services.AddScoped<AdminSeeder>();
            services.AddScoped<SupervisorSeeder>();
            services.AddScoped<PassengerSeeder>();

            services.AddScoped<BookingSeeder>();
            services.AddScoped<BookingPassengerSeeder>();
            services.AddScoped<BoardingPassSeeder>();
            services.AddScoped<PaymentSeeder>();

            services.AddScoped<PilotSeeder>();
            services.AddScoped<AttendantSeeder>();
            services.AddScoped<CertificationSeeder>();
            services.AddScoped<FlightCrewSeeder>();
            services.AddTransient<AncillarySaleSeeder>();
            services.AddScoped<TicketSeeder>();

            return services;
        }

        /// <summary>
        /// Executes the database seeding process upon application startup.
        /// Uses an isolated scope to manage service lifetimes.
        /// </summary>
        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            // Create a scope to resolve Scoped services (like DbContext and Seeders)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Get logger using the central orchestrator type (avoids CS0718)
                var logger = services.GetRequiredService<ILogger<AsiaporeairDataSeed>>();

                try
                {
                    logger.LogInformation("--- Starting Asiaporeair Data Seeding Process ---");

                    var seedService = services.GetRequiredService<AsiaporeairDataSeed>();
                    await seedService.SeedAsync(); // Call the central orchestrator

                    logger.LogInformation("--- Asiaporeair Database Seeding Completed Successfully! ---");
                }
                catch (Exception ex)
                {
                    // Log any critical error during the seeding process
                    logger.LogCritical(ex, "FATAL ERROR during Asiaporeair database seeding process.");
                }
            }

            return app;
        }
    }
}