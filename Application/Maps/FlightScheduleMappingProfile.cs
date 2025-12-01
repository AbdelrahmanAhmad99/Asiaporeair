using Application.DTOs.FlightSchedule;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for FlightSchedule and FlightLegDef entities/DTOs.
    public class FlightScheduleMappingProfile : Profile
    {
        public FlightScheduleMappingProfile()
        {
            // Map FlightSchedule (Entity) -> FlightScheduleDto
            CreateMap<FlightSchedule, FlightScheduleDto>()
                // RouteName is mapped manually in the service
                .ForMember(dest => dest.RouteName, opt => opt.Ignore())
                .ForMember(dest => dest.AirlineIataCode, opt => opt.MapFrom(src => src.AirlineId))
                // This requires AircraftType to be included in the query
                .ForMember(dest => dest.AircraftTypeModel, opt => opt.MapFrom(src => src.AircraftType.Model));

            // Map CreateFlightScheduleDto (DTO) -> FlightSchedule (Entity)
            CreateMap<CreateFlightScheduleDto, FlightSchedule>()
                .ForMember(dest => dest.ScheduleId, opt => opt.Ignore()) // Ignore PK on create
                .ForMember(dest => dest.FlightNo, opt => opt.MapFrom(src => src.FlightNo.ToUpper())) // Normalize
                .ForMember(dest => dest.RouteId, opt => opt.MapFrom(src => src.RouteId))
                .ForMember(dest => dest.AirlineId, opt => opt.MapFrom(src => src.AirlineIataCode))
                .ForMember(dest => dest.AircraftTypeId, opt => opt.MapFrom(src => src.AircraftTypeId))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default to active

            // Map FlightLegDef (Entity) -> FlightLegDefDto
            CreateMap<FlightLegDef, FlightLegDefDto>()
                .ForMember(dest => dest.DepartureAirportIataCode, opt => opt.MapFrom(src => src.DepartureAirportId))
                .ForMember(dest => dest.ArrivalAirportIataCode, opt => opt.MapFrom(src => src.ArrivalAirportId))
                // These require DepartureAirport and ArrivalAirport to be included
                .ForMember(dest => dest.DepartureAirportName, opt => opt.MapFrom(src => src.DepartureAirport.Name))
                .ForMember(dest => dest.ArrivalAirportName, opt => opt.MapFrom(src => src.ArrivalAirport.Name));

            // Map CreateFlightLegDefDto (DTO) -> FlightLegDef (Entity)
            CreateMap<CreateFlightLegDefDto, FlightLegDef>()
                .ForMember(dest => dest.LegDefId, opt => opt.Ignore()) // Ignore PK on create
                .ForMember(dest => dest.ScheduleId, opt => opt.Ignore()) // Set manually by the service
                .ForMember(dest => dest.DepartureAirportId, opt => opt.MapFrom(src => src.DepartureAirportIataCode.ToUpper()))
                .ForMember(dest => dest.ArrivalAirportId, opt => opt.MapFrom(src => src.ArrivalAirportIataCode.ToUpper()))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default to active
        }
    }
}