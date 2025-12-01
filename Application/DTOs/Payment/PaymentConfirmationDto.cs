using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment
{
    public class PaymentConfirmationDto
    { 
        [Required(ErrorMessage = "PaymentIntentId is required for confirmation.")]
        public string PaymentIntentId { get; set; }
         
        public string? PaymentMethodId { get; set; }
        public string? ReturnUrl { get; set; }  
    }
}