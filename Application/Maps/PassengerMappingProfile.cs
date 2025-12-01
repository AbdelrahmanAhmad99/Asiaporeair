using Application.DTOs.Passenger;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for Passenger entity and DTOs.
    public class PassengerMappingProfile : Profile
    {
        public PassengerMappingProfile()
        {
            // Map Passenger (Entity) to PassengerDto
            CreateMap<Passenger, PassengerDto>()
                .ForMember(dest => dest.LinkedUserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FrequentFlyerCardNumber,
                           opt => opt.MapFrom(src => src.User.FrequentFlyer.CardNumber));

            

            // Map CreatePassengerDto (Input DTO) to Passenger (Entity)
            CreateMap<CreatePassengerDto, Passenger>()
                .ForMember(dest => dest.PassengerId, opt => opt.Ignore()) // Don't map ID on create
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Map UserId if provided
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)) // Default IsDeleted
                .ForMember(dest => dest.User, opt => opt.Ignore()) // Don't map navigation property back
                .ForMember(dest => dest.BookingPassengers, opt => opt.Ignore()); // Don't map collection

            // Map UpdatePassengerDto (Input DTO) to Passenger (Entity) for updates
            // Use CreateMap<UpdateDto, Entity> for _mapper.Map(dto, entity) pattern
            CreateMap<UpdatePassengerDto, Passenger>()
               .ForMember(dest => dest.PassengerId, opt => opt.Ignore()) // Ignore PK
               .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Don't change linked user
               .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Don't change delete status
               .ForMember(dest => dest.User, opt => opt.Ignore())
               .ForMember(dest => dest.BookingPassengers, opt => opt.Ignore())
               // Only map non-null values from DTO during update? AutoMapper handles this by default for reference types,
               // but value types like DateTime? need care if you only want to update if provided.
               // Using standard mapping first, service layer has checks.
               .ForMember(dest => dest.DateOfBirth, opt => opt.Condition(src => src.DateOfBirth.HasValue)) // Only map if not null
               .ForMember(dest => dest.PassportNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PassportNumber))); // Only map if not null/empty
        }
    }
}