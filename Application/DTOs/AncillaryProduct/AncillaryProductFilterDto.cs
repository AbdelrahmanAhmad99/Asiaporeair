namespace Application.DTOs.AncillaryProduct
{
    // DTO for filtering ancillary product definitions (Admin).
    public class AncillaryProductFilterDto
    {
        public string? NameContains { get; set; }
        public string? Category { get; set; }
        public decimal? MinCost { get; set; }
        public decimal? MaxCost { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}