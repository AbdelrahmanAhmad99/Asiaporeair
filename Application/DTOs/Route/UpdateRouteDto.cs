using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Route
{
    public class UpdateRouteDto
    {
        [Required(ErrorMessage = "Distance is required.")]
        [Range(1, 40000, ErrorMessage = "Distance must be a positive value (up to 40,000 km).")]
        public int DistanceKm { get; set; }
    }
}