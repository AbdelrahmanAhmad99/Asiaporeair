using Application.DTOs.Ticket;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;  

namespace Application.Maps
{
    // AutoMapper profile for Ticket entity and DTOs.
    public class TicketMappingProfile : Profile
    {
        public TicketMappingProfile()
        {
            // Map Ticket (Entity) to TicketDto (Summary)
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Map enum to string
                                                                                                  // PassengerName, FlightNumber, SeatNumber, BookingReference, FlightDepartureTime require includes and are often set manually/via richer mapping
                .ForMember(dest => dest.PassengerName, opt => opt.MapFrom(src => src.Passenger != null ? $"{src.Passenger.FirstName} {src.Passenger.LastName}" : "N/A")) // Requires Passenger include
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Booking.FlightInstance.Schedule.FlightNo)) // Requires Booking.FlightInstance.Schedule include
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.Seat != null ? src.Seat.SeatNumber : "N/A")) // Requires Seat include
                .ForMember(dest => dest.BookingReference, opt => opt.MapFrom(src => src.Booking.BookingRef)) // Requires Booking include
                .ForMember(dest => dest.FlightDepartureTime, opt => opt.MapFrom(src => src.FlightInstance.ScheduledDeparture)); // Requires FlightInstance include

            // Map Ticket (Entity) to TicketDetailDto
            CreateMap<Ticket, TicketDetailDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.BookingReference, opt => opt.MapFrom(src => src.Booking.BookingRef))
                .ForMember(dest => dest.PassengerFirstName, opt => opt.MapFrom(src => src.Passenger.FirstName))
                .ForMember(dest => dest.PassengerLastName, opt => opt.MapFrom(src => src.Passenger.LastName))
                .ForMember(dest => dest.PassengerDateOfBirth, opt => opt.MapFrom(src => src.Passenger.DateOfBirth))
                .ForMember(dest => dest.PassengerPassportNumber, opt => opt.MapFrom(src => src.Passenger.PassportNumber))
                .ForMember(dest => dest.FrequentFlyerNumber, opt => opt.MapFrom(src => src.FrequentFlyer.CardNumber)) // Requires FF include
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightInstance.Schedule.FlightNo))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Airline.Name))
                .ForMember(dest => dest.FlightDepartureTime, opt => opt.MapFrom(src => src.FlightInstance.ScheduledDeparture))
                .ForMember(dest => dest.FlightArrivalTime, opt => opt.MapFrom(src => src.FlightInstance.ScheduledArrival))
                .ForMember(dest => dest.OriginAirportCode, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.OriginAirport.IataCode))
                .ForMember(dest => dest.OriginAirportName, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.OriginAirport.Name))
                .ForMember(dest => dest.DestinationAirportCode, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.DestinationAirport.IataCode))
                .ForMember(dest => dest.DestinationAirportName, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.DestinationAirport.Name))
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.Seat != null ? src.Seat.SeatNumber : "N/A"))
                .ForMember(dest => dest.CabinClassName, opt => opt.MapFrom(src => src.Seat.CabinClass.Name)) // Requires Seat.CabinClass include
                .ForMember(dest => dest.FareBasisCode, opt => opt.MapFrom(src => src.Booking.FareBasisCodeId)); // Requires Booking include

            // Note: Assumes necessary .Include() calls are made in the repository methods fetching the Ticket entity.
        }
    }
}