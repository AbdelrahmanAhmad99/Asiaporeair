using Application.DTOs.Employee;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for Employee management DTOs.
    public class EmployeeMappingProfile : Profile
    {
        public EmployeeMappingProfile()
        {
            // Map from Employee (Entity) to EmployeeSummaryDto
            CreateMap<Employee, EmployeeSummaryDto>()
                .ForMember(dest => dest.AppUserId, opt => opt.MapFrom(src => src.AppUserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.AppUser.FirstName} {src.AppUser.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.AppUser.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.AppUser.UserType))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted && !src.AppUser.IsDeleted))
                .ForMember(dest => dest.DateOfHire, opt => opt.MapFrom(src => src.DateOfHire))
                .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary))
                // Get position/base from the related CrewMember, if it exists
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src =>
                    src.CrewMember != null ? src.CrewMember.Position : src.AppUser.UserType.ToString()))
                .ForMember(dest => dest.CrewBaseAirportIata, opt => opt.MapFrom(src =>
                    src.CrewMember != null ? src.CrewMember.CrewBaseAirportId : null));
        }
    }
}