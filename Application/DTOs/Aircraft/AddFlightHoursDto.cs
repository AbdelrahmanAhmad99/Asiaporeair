using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for adding flight hours to an aircraft's total
    public class AddFlightHoursDto
    {
        [Range(0.1, 24, ErrorMessage = "Flight hours to add must be a reasonable positive value (0.1 to 24).")]
        public int HoursToAdd { get; set; }
    }
}