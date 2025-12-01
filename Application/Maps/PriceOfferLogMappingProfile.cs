using Application.DTOs.PriceOfferLog;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for PriceOfferLog entity and DTOs
    public class PriceOfferLogMappingProfile : Profile
    {
        public PriceOfferLogMappingProfile()
        {
     
            CreateMap<PriceOfferLog, PriceOfferLogDto>() 
                .ForMember(dest => dest.FareFk, opt => opt.MapFrom(src => src.FareId))
                .ForMember(dest => dest.AncillaryFk, opt => opt.MapFrom(src => src.AncillaryId))
                .ForMember(dest => dest.ContextAttributesFk, opt => opt.MapFrom(src => src.ContextAttributesId)) 
                .ForMember(dest => dest.FareDescription, opt => opt.MapFrom(src => src.Fare.Description))
                .ForMember(dest => dest.AncillaryProductName, opt => opt.MapFrom(src => src.Ancillary.Name));

             
            CreateMap<CreatePriceOfferLogDto, PriceOfferLog>() 
                .ForMember(dest => dest.FareId, opt => opt.MapFrom(src => src.FareFk))
                .ForMember(dest => dest.AncillaryId, opt => opt.MapFrom(src => src.AncillaryFk))
                .ForMember(dest => dest.ContextAttributesId, opt => opt.MapFrom(src => src.ContextAttributesFk)) 
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));
        }
    }
}