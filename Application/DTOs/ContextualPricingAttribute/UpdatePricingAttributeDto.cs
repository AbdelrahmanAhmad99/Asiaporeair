using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ContextualPricingAttribute
{
    // DTO for updating existing contextual pricing attributes
    public class UpdatePricingAttributeDto
    {
        [Range(0, 365, ErrorMessage = "Time until departure must be between 0 and 365 days.")]
        public int? TimeUntilDeparture { get; set; }

        [Range(0, 90, ErrorMessage = "Length of stay must be between 0 and 90 days.")]
        public int? LengthOfStay { get; set; }

        public string? CompetitorFares { get; set; }

        [Range(0.01, 10000.00, ErrorMessage = "Willingness to pay must be a positive value.")]
        public decimal? WillingnessToPay { get; set; }
    }
}