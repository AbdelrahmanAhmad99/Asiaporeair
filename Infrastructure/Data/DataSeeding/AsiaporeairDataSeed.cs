using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Infrastructure.Data.DataSeeding.Seeders;  
using System.Threading.Tasks;

namespace Infrastructure.Data.DataSeeding
{
    /// <summary>
    /// Centralized service orchestrating the entire Data Seeding process for Asiaporeair.
    /// It ensures a professional, dependency-ordered execution of all individual Seeders.
    /// </summary>
    public class AsiaporeairDataSeed
    {
        private readonly RoleSeeder _roleSeeder;
        private readonly FrequentFlyerSeeder _frequentFlyerSeeder;
        private readonly CountrySeeder _countrySeeder;  
        private readonly UserSeeder _userSeeder;
        private readonly SuperAdminSeeder _superAdminSeeder;
        private readonly AdminSeeder _adminSeeder;
        private readonly SupervisorSeeder _supervisorSeeder;
        private readonly FareBasisCodeSeeder _fareBasisCodeSeeder;
        private readonly AncillaryProductSeeder _ancillaryProductSeeder; 
        private readonly AircraftTypeSeeder _aircraftTypeSeeder;
        private readonly AirportSeeder _airportSeeder;
        private readonly AirlineSeeder _airlineSeeder;
        private readonly AircraftSeeder _aircraftSeeder;
        private readonly AircraftConfigSeeder _aircraftConfigSeeder;
        private readonly CabinClassSeeder _cabinClassSeeder;
        private readonly SeatSeeder _seatSeeder;
        private readonly RouteOperatorSeeder _routeOperatorSeeder;
        private readonly ContextualPricingAttributesSeeder _contextualPricingAttributesSeeder; 
        private readonly RouteSeeder _routeSeeder;
        private readonly PriceOfferLogSeeder _priceOfferLogSeeder; 
        private readonly FlightScheduleSeeder _flightScheduleSeeder;
        private readonly FlightInstanceSeeder _flightInstanceSeeder;
        private readonly FlightLegDefSeeder _flightLegDefSeeder; 
        private readonly PassengerSeeder _passengerSeeder;
        private readonly BookingSeeder _bookingSeeder;
        private readonly BookingPassengerSeeder _bookingPassengerSeeder;
        private readonly BoardingPassSeeder _boardingPassSeeder;
        private readonly PaymentSeeder _paymentSeeder;
        private readonly PilotSeeder _pilotSeeder;
        private readonly AttendantSeeder _attendantSeeder;
        private readonly CertificationSeeder _certificationSeeder;
        private readonly FlightCrewSeeder _flightCrewSeeder;
        private readonly AncillarySaleSeeder _ancillarySaleSeeder;
        private readonly TicketSeeder _ticketSeeder;
        private readonly ILogger<AsiaporeairDataSeed> _logger;

        public AsiaporeairDataSeed(
            RoleSeeder roleSeeder,
            FrequentFlyerSeeder frequentFlyerSeeder,
            CountrySeeder countrySeeder,
            AircraftTypeSeeder aircraftTypeSeeder,
            AirportSeeder airportSeeder,
            AirlineSeeder airlineSeeder,
            AircraftSeeder aircraftSeeder,
            AircraftConfigSeeder aircraftConfigSeeder,
            CabinClassSeeder cabinClassSeeder,
            SeatSeeder seatSeeder,
            RouteSeeder routeSeeder,
            RouteOperatorSeeder routeOperatorSeeder,
            FareBasisCodeSeeder fareBasisCodeSeeder,
            AncillaryProductSeeder ancillaryProductSeeder,
            ContextualPricingAttributesSeeder contextualPricingAttributesSeeder,
            PriceOfferLogSeeder priceOfferLogSeeder,
            FlightScheduleSeeder flightScheduleSeeder,
            FlightInstanceSeeder flightInstanceSeeder,
            FlightLegDefSeeder flightLegDefSeeder,
            UserSeeder userSeeder,
            SuperAdminSeeder superAdminSeeder,
            AdminSeeder adminSeeder,
            SupervisorSeeder supervisorSeeder,
            PassengerSeeder passengerSeeder,
            BookingSeeder bookingSeeder,
            BookingPassengerSeeder bookingPassengerSeeder,
            BoardingPassSeeder boardingPassSeeder,
            PaymentSeeder paymentSeeder,
            PilotSeeder pilotSeeder,
            AttendantSeeder attendantSeeder,
            CertificationSeeder certificationSeeder,
            FlightCrewSeeder flightCrewSeeder,
            AncillarySaleSeeder ancillarySaleSeeder,
            TicketSeeder ticketSeeder,
            ILogger<AsiaporeairDataSeed> logger)
        {
            _roleSeeder = roleSeeder;
            _frequentFlyerSeeder = frequentFlyerSeeder;
            _countrySeeder = countrySeeder;
            _aircraftTypeSeeder = aircraftTypeSeeder;
            _airportSeeder = airportSeeder;
            _airlineSeeder = airlineSeeder;
            _aircraftSeeder = aircraftSeeder;
            _aircraftConfigSeeder = aircraftConfigSeeder;
            _cabinClassSeeder = cabinClassSeeder;
            _seatSeeder = seatSeeder;
            _routeSeeder = routeSeeder;
            _routeOperatorSeeder = routeOperatorSeeder; 
            _fareBasisCodeSeeder = fareBasisCodeSeeder;
            _ancillaryProductSeeder = ancillaryProductSeeder;
            _contextualPricingAttributesSeeder = contextualPricingAttributesSeeder;
            _priceOfferLogSeeder = priceOfferLogSeeder; 
            _flightScheduleSeeder = flightScheduleSeeder;
            _flightInstanceSeeder = flightInstanceSeeder;
            _flightLegDefSeeder = flightLegDefSeeder; 
            _userSeeder = userSeeder;
            _superAdminSeeder = superAdminSeeder;
            _adminSeeder = adminSeeder;
            _supervisorSeeder = supervisorSeeder;
            _passengerSeeder = passengerSeeder; 
            _bookingSeeder = bookingSeeder;
            _bookingPassengerSeeder = bookingPassengerSeeder;
            _boardingPassSeeder = boardingPassSeeder;
            _paymentSeeder = paymentSeeder; 
            _pilotSeeder = pilotSeeder;
            _attendantSeeder = attendantSeeder;
            _certificationSeeder = certificationSeeder;
            _flightCrewSeeder = flightCrewSeeder;
            _ancillarySaleSeeder = ancillarySaleSeeder;
            _ticketSeeder = ticketSeeder;
            _logger = logger;
        }


        /// <summary>
        /// Executes the entire data seeding workflow in dependency order.
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting Asiaporeair Data Seeding process...");

            // Seed Core Independent Data (L0)
            await _roleSeeder.SeedRolesAsync();



            // Seed Static/Reference Data (L1) (Must be done before entities depending on them)
            await _countrySeeder.SeedAsync();   

            await _frequentFlyerSeeder.SeedAsync();

            await _aircraftTypeSeeder.SeedAsync();

            await _fareBasisCodeSeeder.SeedAsync();

            await _ancillaryProductSeeder.SeedAsync();

            await _contextualPricingAttributesSeeder.SeedAsync();




            // Seed Dependent Base Data (L2)
            // Seed Pricing Related Logs (Must run after FareBasisCode, AncillaryProduct, and ContextualPricingAttributes)
            await _priceOfferLogSeeder.SeedAsync();

            // Seed Airport Data (Must run after CountrySeeder)
            await _airportSeeder.SeedAsync();

            // Seed Airline Data (Must run after AirportSeeder as it has a FK to Airport)
            await _airlineSeeder.SeedAsync();

            // Seed Aircraft Data (Depends on Airline and AircraftType)
            await _aircraftSeeder.SeedAsync();

            await _aircraftConfigSeeder.SeedAsync();

            // Seed Cabin Class Data (Depends on AircraftConfig)
            await _cabinClassSeeder.SeedAsync();

            // Seed Seat Data (Depends on Aircraft and CabinClass)
            await _seatSeeder.SeedAsync();




            // Seed Dependent Flight Planning & Operational Data (L3)
            // Seed Route Data (Depends on Airport) 
            await _routeSeeder.SeedAsync();

            // Seed RouteOperator Data (Depends on Route and Airline)
            await _routeOperatorSeeder.SeedAsync();

            // Seed FlightSchedule Data (Depends on Route, Airline, AircraftType)
            await _flightScheduleSeeder.SeedAsync();

            // Seed FlightInstance Data (Depends on FlightSchedule and Aircraft) 
            await _flightInstanceSeeder.SeedAsync();

            // Seed FlightLegDef Data (Depends on FlightSchedule and Airport)
            await _flightLegDefSeeder.SeedAsync();




            // Seed User/Employee Data (L4)
            // Seed Customer Users (Depends on Roles and FrequentFlyer)
            await _userSeeder.SeedUsersAsync();

            // Seed Critical Administrative Users (Depend on Roles)
            await _superAdminSeeder.SeedAsync();

            await _adminSeeder.SeedAsync();
            
            await _supervisorSeeder.SeedAsync();
            
            // Seed Passenger Data ( Depends on User )
            await _passengerSeeder.SeedAsync();
            
            // Pilot Seeding (Depends on AppUser, Employee Identity, AircraftType, Airport)
            await _pilotSeeder.SeedAsync();

            // Attendant Seeding (Depends on AppUser, Employee Identity, Airport)
            await _attendantSeeder.SeedAsync(); 
            
            await _certificationSeeder.SeedAsync();
            
            await _flightCrewSeeder.SeedAsync();



            // 4. Seed Dependent Booking & Ticket Data (L5)
            // Booking depends on User, FlightInstance, and FareBasisCode, all of which are now seeded.
            await _bookingSeeder.SeedAsync();

            // Final Junction Table Seeding
            await _bookingPassengerSeeder.SeedAsync();
          
            // Depends on BookingPassenger and Seat
            await _boardingPassSeeder.SeedAsync();  
            
            await _paymentSeeder.SeedAsync();

            // Seed AncillarySales Data
            await _ancillarySaleSeeder.SeedAsync();

            // Seed Ticket Data ( depend on all table)
            await _ticketSeeder.SeedAsync();
             
            _logger.LogInformation("Asiaporeair Data Seeding process finished.");
        }
    }

}
 