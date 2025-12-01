using System;

namespace Application.DTOs.PriceOfferLog
{
    // DTO for filtering price offer logs during search.
    public class PriceOfferLogFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? FareFk { get; set; }
        public int? AncillaryFk { get; set; }
        public int? ContextAttributesFk { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}