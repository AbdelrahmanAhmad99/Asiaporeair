namespace Application.DTOs.AircraftType
{
    // Data Transfer Object for filtering Aircraft Type searches
    public class AircraftTypeFilterDto
    {
        public string? ModelContains { get; set; }
        public string? Manufacturer { get; set; }
        public int? MinRangeKm { get; set; }
        public int? MaxRangeKm { get; set; }
        public int? MinSeats { get; set; }
        public int? MaxSeats { get; set; }
        public decimal? MinCargoCapacity { get; set; }
        public decimal? MaxCargoCapacity { get; set; }
        public bool IncludeDeleted { get; set; } = false; // Default to active only
    }
}
