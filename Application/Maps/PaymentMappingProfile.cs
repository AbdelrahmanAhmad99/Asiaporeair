using Application.DTOs.Payment;
using AutoMapper;
using Domain.Entities;

namespace Application.Maps
{
    // AutoMapper profile for Payment entity and DTOs.
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // Map Payment (Entity) to PaymentDto
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
                 // BookingReference is added manually in the service after fetching Booking if needed
                 //.ForMember(dest => dest.BookingReference, opt => opt.Ignore());
                 .ForMember(dest => dest.BookingReference, opt => opt.MapFrom(src => src.Booking.BookingRef));

            // Map CreatePaymentDto (Input DTO) to Payment (Entity) - for initial record
            CreateMap<CreatePaymentDto, Payment>()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore()) // Amount comes from Booking
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending")) // Initial status
                .ForMember(dest => dest.TransactionId, opt => opt.Ignore()) // Set after gateway interaction
                .ForMember(dest => dest.TransactionDateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));
        }
    }
}