using Application.DTOs.UserManagement;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for User Management DTOs.
    public class UserManagementMappingProfile : Profile
    {
        public UserManagementMappingProfile()
        {
            // Map from AppUser (Entity) to UserSummaryDto
            CreateMap<AppUser, UserSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted));
        }
    }
}