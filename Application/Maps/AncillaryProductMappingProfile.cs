using Application.DTOs.AncillaryProduct;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for AncillaryProduct and AncillarySale entities/DTOs.
    public class AncillaryProductMappingProfile : Profile
    {
        public AncillaryProductMappingProfile()
        {
            // Map AncillaryProduct (Entity) to AncillaryProductDto
            CreateMap<AncillaryProduct, AncillaryProductDto>()
                .ForMember(dest => dest.BaseCost, opt => opt.MapFrom(src => src.BaseCost ?? 0)); // Handle nullable cost

            // Map CreateAncillaryProductDto to AncillaryProduct (Entity)
            CreateMap<CreateAncillaryProductDto, AncillaryProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Don't map ID on create
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false)); // Default IsDeleted

            // Map UpdateAncillaryProductDto to AncillaryProduct (Entity) for updates
            CreateMap<UpdateAncillaryProductDto, AncillaryProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Ignore PK
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); // Don't change delete status

            // Map AncillarySale (Entity) to AncillarySaleDto
            CreateMap<AncillarySale, AncillarySaleDto>()
                .ForMember(dest => dest.PricePaid, opt => opt.MapFrom(src => src.PricePaid ?? 0)) // Handle nullable price
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity ?? 0)) // Handle nullable quantity
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name)); // Requires Product Include
        }
    }
}