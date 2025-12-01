using Application.DTOs.ContextualPricingAttribute; 
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for ContextualPricingAttributes entity and DTOs
    public class ContextualPricingMappingProfile : Profile
    {
        public ContextualPricingMappingProfile()
        {
            // Map Entity -> DTO
            CreateMap<ContextualPricingAttributes, ContextualPricingAttributeDto>();

            // Map Create DTO -> Entity
            CreateMap<CreatePricingAttributeDto, ContextualPricingAttributes>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default on create

            // Map Update DTO -> Entity
            CreateMap<UpdatePricingAttributeDto, ContextualPricingAttributes>()
                .ForMember(dest => dest.AttributeId, opt => opt.Ignore()) // Never update PK
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Deletion is a separate process
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Allow partial updates
        }
    }
}