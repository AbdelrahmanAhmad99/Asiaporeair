namespace Application.DTOs.FareBasisCode
{
    // DTO for representing a Fare Basis Code and its details
    public class FareBasisCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Rules { get; set; } = string.Empty;
    }
}
