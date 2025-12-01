using System;

namespace Application.DTOs.PriceOfferLog
{
    // DTO for representing a single price offer log entry
    public class PriceOfferLogDto
    {
        public int OfferId { get; set; }

        // The ID of the product this offer was for (if any)
        public int? ProductFk { get; set; }

        public decimal OfferPriceQuote { get; set; }
        public DateTime Timestamp { get; set; }

        // The context (e.g., "7 days to departure") used for this quote
        public int ContextAttributesFk { get; set; }

        // The fare code this quote was for (if any)
        public string? FareFk { get; set; }

        // The ancillary product this quote was for (if any)
        public int? AncillaryFk { get; set; }

        // Includes names/details for easier display
        public string? FareDescription { get; set; }
        public string? AncillaryProductName { get; set; }
    }
}