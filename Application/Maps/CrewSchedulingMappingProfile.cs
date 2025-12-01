using Application.DTOs.CrewScheduling;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for Crew Scheduling DTOs.
    public class CrewSchedulingMappingProfile : Profile
    {
        public CrewSchedulingMappingProfile()
        {
            // Map FlightCrew (Entity) to FlightCrewAssignmentDto
            CreateMap<FlightCrew, FlightCrewAssignmentDto>()
                .ForMember(dest => dest.FlightInstanceId, opt => opt.MapFrom(src => src.FlightInstanceId))
                .ForMember(dest => dest.CrewMemberEmployeeId, opt => opt.MapFrom(src => src.CrewMemberId))
                .ForMember(dest => dest.CrewMemberName, opt => opt.MapFrom(src => $"{src.CrewMember.Employee.AppUser.FirstName} {src.CrewMember.Employee.AppUser.LastName}"))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.CrewMember.Position)) // Pilot or Attendant
                .ForMember(dest => dest.AssignedRole, opt => opt.MapFrom(src => src.Role)) // Specific role like Captain
                .ForMember(dest => dest.CrewBase, opt => opt.MapFrom(src => src.CrewMember.CrewBaseAirportId));

            // Map FlightCrew (Entity) to ScheduledFlightDto (for CrewScheduleDto)
            CreateMap<FlightCrew, ScheduledFlightDto>()
                .ForMember(dest => dest.FlightInstanceId, opt => opt.MapFrom(src => src.FlightInstanceId))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightInstance.Schedule.FlightNo))
                .ForMember(dest => dest.OriginAirport, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.OriginAirport.IataCode))
                .ForMember(dest => dest.DestinationAirport, opt => opt.MapFrom(src => src.FlightInstance.Schedule.Route.DestinationAirport.IataCode))
                .ForMember(dest => dest.ScheduledDeparture, opt => opt.MapFrom(src => src.FlightInstance.ScheduledDeparture))
                .ForMember(dest => dest.ScheduledArrival, opt => opt.MapFrom(src => src.FlightInstance.ScheduledArrival))
                .ForMember(dest => dest.AssignedRole, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.AircraftType, opt => opt.MapFrom(src => src.FlightInstance.Schedule.AircraftType.Model));

            // Map CrewMember (Entity) to CrewAvailabilityResponseDto
            CreateMap<CrewMember, CrewAvailabilityResponseDto>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Employee.AppUser.FirstName} {src.Employee.AppUser.LastName}"))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.CrewBaseAirportIata, opt => opt.MapFrom(src => src.CrewBaseAirportId))
                // IsTypeRated and HasValidCertification are set dynamically in the service
                .ForMember(dest => dest.IsTypeRated, opt => opt.Ignore())
                .ForMember(dest => dest.HasValidCertification, opt => opt.Ignore());
        }
    }
}