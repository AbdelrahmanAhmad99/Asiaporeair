using Application.DTOs.FlightOperations;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for Flight Operations (Instance) related entities and DTOs.
    public class FlightOperationsMappingProfile : Profile
    {
        public FlightOperationsMappingProfile()
        {
            // Map from FlightInstance (Entity) to FlightInstanceDto (Detailed DTO)
            CreateMap<FlightInstance, FlightInstanceDto>()
                .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
                .ForMember(dest => dest.FlightNo, opt => opt.MapFrom(src => src.Schedule.FlightNo))
                .ForMember(dest => dest.AirlineIataCode, opt => opt.MapFrom(src => src.Schedule.Airline.IataCode))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Schedule.Airline.Name))
                .ForMember(dest => dest.OriginIataCode, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.IataCode))
                .ForMember(dest => dest.OriginAirportName, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.Name))
                .ForMember(dest => dest.OriginCity, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.City))
                .ForMember(dest => dest.DestinationIataCode, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.IataCode))
                .ForMember(dest => dest.DestinationAirportName, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.Name))
                .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.City))
                .ForMember(dest => dest.ScheduledAircraftModel, opt => opt.MapFrom(src => src.Schedule.AircraftType.Model))
                .ForMember(dest => dest.AssignedAircraftTailNumber, opt => opt.MapFrom(src => src.AircraftId))
                .ForMember(dest => dest.AssignedAircraftModel, opt => opt.MapFrom(src => src.Aircraft.AircraftType.Model))
                .ForMember(dest => dest.ScheduledDeparture, opt => opt.MapFrom(src => src.ScheduledDeparture))
                .ForMember(dest => dest.ScheduledArrival, opt => opt.MapFrom(src => src.ScheduledArrival))
                .ForMember(dest => dest.ActualDeparture, opt => opt.MapFrom(src => src.ActualDeparture))
                .ForMember(dest => dest.ActualArrival, opt => opt.MapFrom(src => src.ActualArrival))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // Corrected: Removed all non-existent properties
            //.ForMember(dest => dest.EstimatedDeparture, opt => opt.Ignore())
            //.ForMember(dest => dest.EstimatedArrival, opt => opt.Ignore())
            //.ForMember(dest => dest.DepartureGate, opt => opt.Ignore())
            //.ForMember(dest => dest.ArrivalGate, opt => opt.Ignore())
            //.ForMember(dest => dest.BaggageCarousel, opt => opt.Ignore());

            // Map from FlightInstance (Entity) to FlightInstanceBriefDto (FIDS DTO)
            CreateMap<FlightInstance, FlightInstanceBriefDto>()
                .ForMember(dest => dest.FlightNo, opt => opt.MapFrom(src => src.Schedule.FlightNo))
                .ForMember(dest => dest.AirlineIataCode, opt => opt.MapFrom(src => src.Schedule.Airline.IataCode))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Schedule.Airline.Name))
                .ForMember(dest => dest.OriginIataCode, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.IataCode))
                .ForMember(dest => dest.OriginCity, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.City))
                .ForMember(dest => dest.DestinationIataCode, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.IataCode))
                .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.City));
                //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))

                // Corrected: Removed non-existent properties
                //.ForMember(dest => dest.BaggageCarousel, opt => opt.Ignore())
                //.ForMember(dest => dest.ScheduledTime, opt => opt.Ignore())
                //.ForMember(dest => dest.EstimatedTime, opt => opt.Ignore())
                //.ForMember(dest => dest.Gate, opt => opt.Ignore());

            // Mappings for updates
            CreateMap<UpdateFlightTimesDto, FlightInstance>()
                // Corrected: Removed non-existent properties
                // .ForMember(dest => dest.EstimatedDeparture, opt => opt.MapFrom(src => src.EstimatedDeparture))
                // .ForMember(dest => dest.EstimatedArrival, opt => opt.MapFrom(src => src.EstimatedArrival))
                .ForMember(dest => dest.ActualDeparture, opt => opt.MapFrom(src => src.ActualDeparture))
                .ForMember(dest => dest.ActualArrival, opt => opt.MapFrom(src => src.ActualArrival));

            // Corrected: This mapping is invalid as properties do not exist
            // CreateMap<UpdateGateInfoDto, FlightInstance>();
        }
    }
}