using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PriceOfferLog
{
    // DTO used specifically to log a price offer.
    public class CreatePriceOfferLogDto
    {
        [Required]
        [Range(0.01, 100000.00)] // Realistic price range
        public decimal OfferPriceQuote { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public int ContextAttributesFk { get; set; } // Link to contextual attributes used

        // Link to *either* a fare or an ancillary product
        [StringLength(10)]
        public string? FareFk { get; set; } // Fare Basis Code

        public int? AncillaryFk { get; set; } // Ancillary Product ID

        // Validation: Ensure either FareFk or AncillaryFk is provided, but not both.
        // This could be done via custom validation attribute or in the service layer.
    }
}