namespace Application.DTOs.Airport
{
    // A lightweight DTO for embedding airport info inside other DTOs.
    public class AirportBriefDto
    {
        public string IataCode { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }
}