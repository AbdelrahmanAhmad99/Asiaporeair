using Application.DTOs.Booking;
using Application.DTOs.Passenger; // Import PassengerDto
using Application.DTOs.AncillaryProduct;
using Application.DTOs.Payment;
using AutoMapper;
using Domain.Entities;
using System.Linq;

namespace Application.Maps
{
    // AutoMapper profile for Booking entity and related DTOs.
    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            // Map Booking (Entity) to BookingDto (Summary)
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.PriceTotal ?? 0))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightInstance.Schedule.FlightNo)) // Requires includes
                .ForMember(dest => dest.FlightDepartureTime, opt => opt.MapFrom(src => src.FlightInstance.ScheduledDeparture)) // Requires includes
                .ForMember(dest => dest.OriginAirportCode, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.OriginAirport.IataCode)) // Requires includes
                .ForMember(dest => dest.DestinationAirportCode, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.DestinationAirport.IataCode)) // Requires includes
                .ForMember(dest => dest.FareBasisCode, opt => opt.MapFrom(src => src.FareBasisCodeId))
                .ForMember(dest => dest.BookingReference, opt => opt.MapFrom(src => src.BookingRef))
                // Passengers are mapped separately in the service if needed for summary
                //.ForMember(dest => dest.Passengers, opt => opt.Ignore());
                .ForMember(dest => dest.Passengers, opt => opt.MapFrom(src => src.BookingPassengers.Select(bp => bp.Passenger)));
            // Map Booking (Entity) to BookingDetailDto (Detailed)
            CreateMap<Booking, BookingDetailDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.PriceTotal ?? 0))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => $"{src.User.AppUser.FirstName} {src.User.AppUser.LastName}")) // Requires includes
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.AppUser.Email)) // Requires includes
                .ForMember(dest => dest.FlightInstanceId, opt => opt.MapFrom(src => src.FlightInstanceId))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightInstance.Schedule.FlightNo)) // Requires includes
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Airline.Name)) // Requires includes
                .ForMember(dest => dest.AircraftModel, opt => opt.MapFrom(src => src.FlightInstance.Aircraft.AircraftType.Model)) // Requires includes
                .ForMember(dest => dest.FlightDepartureTime, opt => opt.MapFrom(src => src.FlightInstance.ScheduledDeparture)) // Requires includes
                .ForMember(dest => dest.FlightArrivalTime, opt => opt.MapFrom(src => src.FlightInstance.ScheduledArrival)) // Requires includes
                .ForMember(dest => dest.OriginAirportCode, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.OriginAirport.IataCode)) // Requires includes
                .ForMember(dest => dest.OriginAirportName, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.OriginAirport.Name)) // Requires includes
                .ForMember(dest => dest.DestinationAirportCode, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.DestinationAirport.IataCode)) // Requires includes
                .ForMember(dest => dest.DestinationAirportName, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.DestinationAirport.Name)) // Requires includes
                .ForMember(dest => dest.FareBasisCode, opt => opt.MapFrom(src => src.FareBasisCodeId))
                .ForMember(dest => dest.FareDescription, opt => opt.MapFrom(src => src.FareBasisCode.Description)) // Requires include
                .ForMember(dest => dest.BookingReference, opt => opt.MapFrom(src => src.BookingRef))

                // Map collections
                .ForMember(dest => dest.Passengers, opt => opt.MapFrom(src => src.BookingPassengers)) // Requires specific mapping below
                .ForMember(dest => dest.AncillarySales, opt => opt.MapFrom(src => src.AncillarySales)) // Assuming AncillarySaleDto mapping exists
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments)); // Assuming PaymentDto mapping exists

            // Map BookingPassenger (Link Entity) to BookingPassengerDetailDto
            CreateMap<BookingPassenger, BookingPassengerDetailDto>()
                .ForMember(dest => dest.PassengerId, opt => opt.MapFrom(src => src.PassengerId))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Passenger.FirstName)) // Requires Passenger include
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Passenger.LastName)) // Requires Passenger include
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Passenger.DateOfBirth)) // Requires Passenger include
                .ForMember(dest => dest.PassportNumber, opt => opt.MapFrom(src => src.Passenger.PassportNumber)) // Requires Passenger include
                .ForMember(dest => dest.AssignedSeatNumber, opt => opt.MapFrom(src => src.SeatAssignment.SeatNumber)) // Requires SeatAssignment include
                .ForMember(dest => dest.CabinClassName, opt => opt.MapFrom(src => src.SeatAssignment.CabinClass.Name)); // Requires SeatAssignment.CabinClass include


            // Map Passenger (Entity) back to PassengerDto (needed for summary DTOs)
            CreateMap<Passenger, PassengerDto>()
                .ForMember(dest => dest.LinkedUserId, opt => opt.MapFrom(src => src.UserId));

            // Define mappings for AncillarySaleDto and PaymentDto if they don't exist elsewhere
            // Example:
            // CreateMap<AncillarySale, AncillarySaleDto>();
            // CreateMap<Payment, PaymentDto>();
        }
    }
}