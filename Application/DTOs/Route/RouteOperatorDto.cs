namespace Application.DTOs.Route
{
    public class RouteOperatorDto
    {
        public int RouteId { get; set; }
        public string AirlineIataCode { get; set; }
        public string AirlineName { get; set; }
        public bool? IsCodeshare { get; set; }
    }
}