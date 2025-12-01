using Application.DTOs.Flight;
using AutoMapper;
using Domain.Entities;
using System;

namespace Application.Maps // تأكد من أن هذا هو الـ namespace الصحيح لمشروعك
{
    public class FlightMappingProfile : Profile
    {
        public FlightMappingProfile()
        {
            // --- هذا يحل خطأ البحث في السطر 150 ---
            // Mapping from FlightInstance -> FlightSegmentDto
            CreateMap<FlightInstance, FlightSegmentDto>()
                .ForMember(dest => dest.FlightInstanceId, opt => opt.MapFrom(src => src.InstanceId))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Schedule.FlightNo))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Schedule.Airline.Name))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.ScheduledDeparture))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.ScheduledArrival))
                .ForMember(dest => dest.OriginAirportIata, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.IataCode))
                .ForMember(dest => dest.DestinationAirportIata, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.IataCode))
                //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => (src.ScheduledArrival - src.ScheduledDeparture).TotalMinutes))
            .ForMember(dest => dest.AirlineIataCode, opt => opt.MapFrom(src => src.Schedule.Airline.IataCode))
            .ForMember(dest => dest.AircraftModel, opt => opt.MapFrom(src => src.Aircraft.AircraftType.Model)) // من الطائرة المخصصة
            .ForMember(dest => dest.OriginAirportName, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.Name))
            .ForMember(dest => dest.OriginCity, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.City))
            .ForMember(dest => dest.DestinationAirportName, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.Name))
            .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.City));
            
            // --- هذا يحل الخطأ القادم في GetFlightDetailsAsync ---
            // Mapping from FlightInstance -> FlightDetailsDto
            CreateMap<FlightInstance, FlightDetailsDto>()
                .ForMember(dest => dest.FlightInstanceId, opt => opt.MapFrom(src => src.InstanceId))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Schedule.FlightNo))
                .ForMember(dest => dest.AircraftModel, opt => opt.MapFrom(src => src.Aircraft.AircraftType.Model))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Schedule.Airline.Name))
                .ForMember(dest => dest.ScheduledDepartureTime, opt => opt.MapFrom(src => src.ScheduledDeparture))
                .ForMember(dest => dest.ScheduledArrivalTime, opt => opt.MapFrom(src => src.ScheduledArrival))
                .ForMember(dest => dest.OriginAirportName, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.Name))
                .ForMember(dest => dest.DestinationAirportName, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TailNumber, opt => opt.MapFrom(src => src.AircraftId)) // AircraftId هو نفسه TailNumber
                .ForMember(dest => dest.OriginIataCode, opt => opt.MapFrom(src => src.Schedule.Route.OriginAirport.IataCode))
                .ForMember(dest => dest.DestinationIataCode, opt => opt.MapFrom(src => src.Schedule.Route.DestinationAirport.IataCode))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => (src.ScheduledArrival - src.ScheduledDeparture).TotalMinutes))
                // We ignore CabinClasses because it is populated manually in the service
                .ForMember(dest => dest.CabinClasses, opt => opt.Ignore());
        }
    }
}