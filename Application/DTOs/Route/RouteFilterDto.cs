namespace Application.DTOs.Route
{
    public class RouteFilterDto
    {
        public string? OriginAirportIataCode { get; set; }
        public string? DestinationAirportIataCode { get; set; }
        public string? OriginCountryIsoCode { get; set; }
        public string? DestinationCountryIsoCode { get; set; }
        public string? OperatingAirlineIataCode { get; set; } // Find routes where this airline operates
        public int? MinDistance { get; set; }
        public int? MaxDistance { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}