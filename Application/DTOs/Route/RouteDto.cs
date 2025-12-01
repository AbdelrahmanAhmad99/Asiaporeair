namespace Application.DTOs.Route
{
    public class RouteDto
    {
        public int RouteId { get; set; }
        public string OriginAirportIataCode { get; set; } = string.Empty;
        public string OriginAirportName { get; set; } = string.Empty;
        public string OriginCity { get; set; } = string.Empty;
        public string DestinationAirportIataCode { get; set; } = string.Empty;
        public string DestinationAirportName { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public int? DistanceKm { get; set; }
    }
}