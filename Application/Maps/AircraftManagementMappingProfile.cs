using Application.DTOs.Aircraft;
using AutoMapper;
using Domain.Entities;
using System.Linq;

namespace Application.Maps
{
    // Defines AutoMapper mappings for Aircraft and related entities
    public class AircraftManagementMappingProfile : Profile
    {
        public AircraftManagementMappingProfile()
        {
            // Map Aircraft Entity to AircraftDto
            CreateMap<Aircraft, AircraftDto>()
                .ForMember(dest => dest.AirlineIataCode, opt => opt.MapFrom(src => src.AirlineId))
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Airline.Name))
                .ForMember(dest => dest.AircraftTypeId, opt => opt.MapFrom(src => src.AircraftTypeId))
                .ForMember(dest => dest.AircraftTypeModel, opt => opt.MapFrom(src => src.AircraftType.Model));

            // Map Aircraft Entity to detailed DTO
            CreateMap<Aircraft, AircraftDetailDto>()
                .IncludeBase<Aircraft, AircraftDto>() // Inherit base mappings
                .ForMember(dest => dest.Configurations, opt => opt.MapFrom(src => src.Configurations));

            // Map AircraftConfig Entity to DTO
            CreateMap<AircraftConfig, AircraftConfigDto>()
                .ForMember(dest => dest.CabinClasses, opt => opt.MapFrom(src => src.CabinClasses));

            // Map CabinClass Entity to DTO
            CreateMap<CabinClass, CabinClassDto>()
                .ForMember(dest => dest.SeatCount, opt => opt.MapFrom(src => src.Seats.Count(s => !s.IsDeleted))); // Calculate active seats

            // Map Create DTO to Aircraft Entity
            CreateMap<CreateAircraftDto, Aircraft>()
                .ForMember(dest => dest.TailNumber, opt => opt.MapFrom(src => src.TailNumber.ToUpper()))
                .ForMember(dest => dest.AirlineId, opt => opt.MapFrom(src => src.AirlineIataCode))
                .ForMember(dest => dest.AircraftTypeId, opt => opt.MapFrom(src => src.AircraftTypeId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.AcquisitionDate, opt => opt.MapFrom(src => src.AcquisitionDate))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)) // Default on create
                .ForMember(dest => dest.TotalFlightHours, opt => opt.MapFrom(src => 0)); // Default on create

            // Map Update DTO to Aircraft Entity (for updating existing entity)
            CreateMap<UpdateAircraftDto, Aircraft>()
                .ForMember(dest => dest.AirlineId, opt => opt.MapFrom(src => src.AirlineIataCode))
                .ForMember(dest => dest.AircraftTypeId, opt => opt.MapFrom(src => src.AircraftTypeId))
                .ForMember(dest => dest.AcquisitionDate, opt => opt.MapFrom(src => src.AcquisitionDate))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls from DTO

            // Map Create Config DTO to Entity
            CreateMap<CreateAircraftConfigDto, AircraftConfig>()
                .ForMember(dest => dest.ConfigurationName, opt => opt.MapFrom(src => src.ConfigurationName))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Map Create Cabin Class DTO to Entity
            CreateMap<CreateCabinClassDto, CabinClass>()
                .ForMember(dest => dest.ConfigId, opt => opt.MapFrom(src => src.ConfigId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Map Update Config DTO to Entity
            CreateMap<UpdateAircraftConfigDto, AircraftConfig>()
                .ForMember(dest => dest.ConfigId, opt => opt.Ignore())
                .ForMember(dest => dest.AircraftId, opt => opt.Ignore());

            CreateMap<CreateAircraftConfigDto, AircraftConfig>()
                .ForMember(dest => dest.ConfigurationName, opt => opt.MapFrom(src => src.ConfigurationName)) 
                .ForMember(dest => dest.TotalSeatsCount, opt => opt.MapFrom(src => src.TotalSeatsCount)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        }
    }
}