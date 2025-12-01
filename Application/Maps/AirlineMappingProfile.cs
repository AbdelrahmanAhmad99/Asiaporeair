using Application.DTOs.Airline;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    /// <summary>
    /// AutoMapper profile for Airline entities and related DTOs.
    /// </summary>
    public class AirlineMappingProfile : Profile
    {
        public AirlineMappingProfile()
        {
            // REQUIRED FIX: Add the mapping from the Entity to the DTO
            // This mapping is used when returning the updated object from the service layer.
            CreateMap<Airline, AirlineDto>()
                .ForMember(dest => dest.BaseAirportIataCode, opt => opt.MapFrom(src => src.BaseAirportId))
                // Ensure other related fields, like BaseAirportName, are mapped if they exist in AirlineDto
                .ForMember(dest => dest.BaseAirportName, opt => opt.MapFrom(src => src.BaseAirport.Name))
                .ReverseMap(); // Optional: Allows mapping from DTO back to Entity

            // Map DTOs used for creation and updates
            CreateMap<CreateAirlineDto, Airline>()
                .ForMember(dest => dest.IataCode, opt => opt.MapFrom(src => src.IataCode.ToUpperInvariant()))
                .ForMember(dest => dest.BaseAirportId, opt => opt.MapFrom(src => src.BaseAirportIataCode.ToUpperInvariant()));

            CreateMap<UpdateAirlineDto, Airline>()
                .ForMember(dest => dest.BaseAirportId, opt => opt.MapFrom(src => src.BaseAirportIataCode.ToUpperInvariant()));



        }

        public static class AirlineMapper
        {
            // --- Helper Method for Mapping ---
            public static AirlineDto MapToDto(Airline airline)
            {
                return new AirlineDto
                {
                    IataCode = airline.IataCode,
                    Name = airline.Name,
                    Callsign = airline.Callsign,
                    OperatingRegion = airline.OperatingRegion,
                    BaseAirportIataCode = airline.BaseAirportId,
                    BaseAirportName = airline.BaseAirport?.Name ?? "N/A" // Safely access included airport name
                };
            }

        }

    }
}