using Application.DTOs.FareBasisCode;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for FareBasisCode entity and DTOs
    public class FareBasisCodeMappingProfile : Profile
    {
        public FareBasisCodeMappingProfile()
        {
            // Map Entity -> DTO
            CreateMap<FareBasisCode, FareBasisCodeDto>();

            // Map Create DTO -> Entity
            CreateMap<CreateFareBasisCodeDto, FareBasisCode>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.ToUpper())) // Ensure PK is uppercase
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default on create

            // Map Update DTO -> Entity
            CreateMap<UpdateFareBasisCodeDto, FareBasisCode>()
                .ForMember(dest => dest.Code, opt => opt.Ignore()) // Do not update the Primary Key
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); // Do not change deletion status on update
        }
    }
}