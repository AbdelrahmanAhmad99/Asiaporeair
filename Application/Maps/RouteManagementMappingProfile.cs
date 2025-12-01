using Application.DTOs.Route;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // Defines AutoMapper mappings for Route and RouteOperator entities
    public class RouteManagementMappingProfile : Profile
    {
        public RouteManagementMappingProfile()
        {
            // Map Route Entity to RouteDto
            CreateMap<Route, RouteDto>()
                .ForMember(dest => dest.OriginAirportIataCode, opt => opt.MapFrom(src => src.OriginAirportId))
                .ForMember(dest => dest.OriginAirportName, opt => opt.MapFrom(src => src.OriginAirport.Name))
                .ForMember(dest => dest.OriginCity, opt => opt.MapFrom(src => src.OriginAirport.City))
                .ForMember(dest => dest.DestinationAirportIataCode, opt => opt.MapFrom(src => src.DestinationAirportId))
                .ForMember(dest => dest.DestinationAirportName, opt => opt.MapFrom(src => src.DestinationAirport.Name))
                .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.DestinationAirport.City));

            // Map RouteOperator Entity to RouteOperatorDto
            CreateMap<RouteOperator, RouteOperatorDto>()
                .ForMember(dest => dest.AirlineIataCode, opt => opt.MapFrom(src => src.AirlineId))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Airline.Name))
                .ForMember(dest => dest.IsCodeshare, opt => opt.MapFrom(src => src.CodeshareStatus));

            // Map Route Entity to RouteDetailDto
            CreateMap<Route, RouteDetailDto>()
                .IncludeBase<Route, RouteDto>() // Inherit base mappings
                .ForMember(dest => dest.Operators, opt => opt.MapFrom(src => src.RouteOperators));

            // Map Create DTO to Route Entity
            CreateMap<CreateRouteDto, Route>()
                .ForMember(dest => dest.OriginAirportId, opt => opt.MapFrom(src => src.OriginAirportIataCode.ToUpper()))
                .ForMember(dest => dest.DestinationAirportId, opt => opt.MapFrom(src => src.DestinationAirportIataCode.ToUpper()))
                .ForMember(dest => dest.DistanceKm, opt => opt.MapFrom(src => src.DistanceKm))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default on create

            // Map Update DTO to Route Entity (only updates distance)
            CreateMap<UpdateRouteDto, Route>()
                .ForMember(dest => dest.DistanceKm, opt => opt.MapFrom(src => src.DistanceKm)); 

            // Map Assign Operator DTO to RouteOperator Entity
            CreateMap<AssignOperatorDto, RouteOperator>()
                .ForMember(dest => dest.RouteId, opt => opt.MapFrom(src => src.RouteId))
                .ForMember(dest => dest.AirlineId, opt => opt.MapFrom(src => src.AirlineIataCode.ToUpper()))
                .ForMember(dest => dest.CodeshareStatus, opt => opt.MapFrom(src => src.IsCodeshare))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));
        }
    }
}