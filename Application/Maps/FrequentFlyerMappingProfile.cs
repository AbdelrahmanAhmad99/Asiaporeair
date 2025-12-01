using Application.DTOs.FrequentFlyer;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for FrequentFlyer entity and DTOs.
    public class FrequentFlyerMappingProfile : Profile
    {
        public FrequentFlyerMappingProfile()
        {
            // Map FrequentFlyer (Entity) to FrequentFlyerDto
            CreateMap<FrequentFlyer, FrequentFlyerDto>()
                .ForMember(dest => dest.AwardPoints, opt => opt.MapFrom(src => src.AwardPoints ?? 0)) // Handle nullable points
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted))
                // LinkedUserId and LinkedUserName are populated manually in the service
                .ForMember(dest => dest.LinkedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LinkedUserName, opt => opt.Ignore());

            // Map CreateFrequentFlyerDto to FrequentFlyer (Entity)
            CreateMap<CreateFrequentFlyerDto, FrequentFlyer>()
                .ForMember(dest => dest.FlyerId, opt => opt.Ignore()) // Don't map ID on create
                .ForMember(dest => dest.AwardPoints, opt => opt.MapFrom(src => src.InitialAwardPoints))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default IsDeleted
        }
    }
}
