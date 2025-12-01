using Application.DTOs.Crew;
using AutoMapper;
using Domain.Entities;
using System;

namespace Application.Maps
{
    // AutoMapper profile for Crew Management DTOs.
    public class CrewMappingProfile : Profile
    {
        public CrewMappingProfile()
        {
            // Map from CrewMember (Entity) to CrewMemberSummaryDto
            CreateMap<CrewMember, CrewMemberSummaryDto>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.AppUserId, opt => opt.MapFrom(src => src.Employee.AppUserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Employee.AppUser.FirstName} {src.Employee.AppUser.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Employee.AppUser.Email))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.CrewBaseAirportIata, opt => opt.MapFrom(src => src.CrewBaseAirportId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted && !src.Employee.IsDeleted && !src.Employee.AppUser.IsDeleted));

            // Map from CrewMember (Entity) to CrewMemberDetailDto (Base Info)
            CreateMap<CrewMember, CrewMemberDetailDto>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.AppUserId, opt => opt.MapFrom(src => src.Employee.AppUserId))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Employee.AppUser.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Employee.AppUser.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Employee.AppUser.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Employee.AppUser.PhoneNumber))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Employee.AppUser.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Employee.AppUser.Address))
                .ForMember(dest => dest.DateOfHire, opt => opt.MapFrom(src => src.Employee.DateOfHire))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.Employee.AppUser.UserType))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.Employee.AppUser.ProfilePictureUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted && !src.Employee.IsDeleted && !src.Employee.AppUser.IsDeleted))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.CrewBaseAirportIata, opt => opt.MapFrom(src => src.CrewBaseAirportId))
                .ForMember(dest => dest.CrewBaseAirportName, opt => opt.MapFrom(src => src.CrewBaseAirport.Name)) // Assuming CrewBaseAirport is included
                                                                                                                  // Ignore Pilot/Attendant specific fields and Certifications here, map them separately
                .ForMember(dest => dest.LicenseNumber, opt => opt.Ignore())
                .ForMember(dest => dest.TotalFlightHours, opt => opt.Ignore())
                .ForMember(dest => dest.AircraftTypeRatingModel, opt => opt.Ignore())
                .ForMember(dest => dest.LastSimCheckDate, opt => opt.Ignore())
                .ForMember(dest => dest.Certifications, opt => opt.Ignore());

            // Map Pilot specific fields (Entity) onto existing CrewMemberDetailDto
            CreateMap<Pilot, CrewMemberDetailDto>()
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.TotalFlightHours, opt => opt.MapFrom(src => src.TotalFlightHours))
                .ForMember(dest => dest.AircraftTypeRatingModel, opt => opt.MapFrom(src => src.TypeRating.Model)) // Assuming TypeRating is included
                .ForMember(dest => dest.LastSimCheckDate, opt => opt.MapFrom(src => src.LastSimCheckDate))
                // Ignore all base fields already mapped from CrewMember
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Map Certification (Entity) to CertificationDto
            CreateMap<Certification, CertificationDto>()
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.ExpiryDate.HasValue && src.ExpiryDate.Value < DateTime.UtcNow));

            // Map CreateCertificationDto (Input DTO) to Certification (Entity)
            CreateMap<CreateCertificationDto, Certification>()
                .ForMember(dest => dest.CertId, opt => opt.Ignore()) // Don't map ID on create/update
                .ForMember(dest => dest.CrewMemberId, opt => opt.Ignore()) // Set manually in service
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); // Set manually if needed
        }
    }
}