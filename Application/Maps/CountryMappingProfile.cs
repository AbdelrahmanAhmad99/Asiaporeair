using Application.DTOs.Airport;
using Application.DTOs.Country;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for Country and its related DTOs.
    public class CountryMappingProfile : Profile
    {
        public CountryMappingProfile()
        {
            // Map Country (Entity) to CountryDto
            CreateMap<Country, CountryDto>();

            // Map CreateCountryDto to Country (Entity)
            CreateMap<CreateCountryDto, Country>()
                .ForMember(dest => dest.IsoCode, opt => opt.MapFrom(src => src.IsoCode.ToUpperInvariant()))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Map UpdateCountryDto to Country (Entity)
            // (Handled by service logic, but mapping can be defined)
            CreateMap<UpdateCountryDto, Country>();

            // Map Airport (Entity) to AirportBriefDto (for nesting)
            CreateMap<Airport, AirportBriefDto>();

            // Map Country (Entity) to CountryWithAirportsDto
            // This assumes GetWithAirportsAsync eager-loaded the Airports list
            CreateMap<Country, CountryWithAirportsDto>()
                .ForMember(dest => dest.Airports, opt => opt.MapFrom(src => src.Airports));
        }
    }
}