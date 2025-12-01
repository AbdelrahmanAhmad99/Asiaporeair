using Application.DTOs.BoardingPass;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for BoardingPass entity and DTOs.
    public class BoardingPassMappingProfile : Profile
    {
        public BoardingPassMappingProfile()
        {
            // Map BoardingPass (Entity) to BoardingPassDto
            // Many fields require manual mapping/enrichment in the service due to complex joins needed
            CreateMap<BoardingPass, BoardingPassDto>()
                .ForMember(dest => dest.PassId, opt => opt.MapFrom(src => src.PassId))
                .ForMember(dest => dest.PrecheckStatus, opt => opt.MapFrom(src => src.PrecheckStatus))
                // Ignore complex fields that will be manually mapped in the service helper
                .ForMember(dest => dest.PassengerId, opt => opt.Ignore())
                .ForMember(dest => dest.PassengerName, opt => opt.Ignore())
                .ForMember(dest => dest.FrequentFlyerNumber, opt => opt.Ignore())
                .ForMember(dest => dest.FlightInstanceId, opt => opt.Ignore())
                .ForMember(dest => dest.FlightNumber, opt => opt.Ignore())
                .ForMember(dest => dest.OriginAirportCode, opt => opt.Ignore())
                .ForMember(dest => dest.DestinationAirportCode, opt => opt.Ignore())
                .ForMember(dest => dest.DepartureTime, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalTime, opt => opt.Ignore())
                .ForMember(dest => dest.SeatNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CabinClass, opt => opt.Ignore())
                .ForMember(dest => dest.BoardingTime, opt => opt.Ignore()) // Use calculated/retrieved value
                //.ForMember(dest => dest.Gate, opt => opt.Ignore())
                .ForMember(dest => dest.SequenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.BookingReference, opt => opt.Ignore())
                .ForMember(dest => dest.TicketCode, opt => opt.Ignore());
        }
    }
}