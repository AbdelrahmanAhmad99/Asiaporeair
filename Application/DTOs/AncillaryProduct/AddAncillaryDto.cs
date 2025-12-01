using System.ComponentModel.DataAnnotations; 
using System.Text.Json.Serialization;
namespace Application.DTOs.AncillaryProduct
{
    // DTO used when adding an ancillary product *during* the booking creation flow.
    public class AddAncillaryDto // Renamed for clarity vs. CreateAncillarySaleDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; } = 1;

        // PassengerId might be needed if the ancillary is per passenger (e.g., meal)
        [JsonIgnore] // Hides from Swagger/Input
        public int? PassengerId { get; set; }
        // SegmentId might be needed if ancillary applies to a specific flight leg
         public int? SegmentId { get; set; }
    }
}