using Application.DTOs.AircraftType;
using AutoMapper;
using Domain.Entities; 

namespace Application.Maps
{
    // AutoMapper profile for AircraftType entity and its DTOs
    public class AircraftTypeMappingProfile : Profile
    {
        public AircraftTypeMappingProfile()
        {
            // Map Entity to DTO
            CreateMap<AircraftType, AircraftTypeDto>();

            // Map Create DTO to Entity
            CreateMap<CreateAircraftTypeDto, AircraftType>();

            // Map Update DTO to Entity
            CreateMap<UpdateAircraftTypeDto, AircraftType>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore null values during update mapping
        }
    }
}