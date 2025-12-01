using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for changing an aircraft's operational status
    public class UpdateAircraftStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = string.Empty;
    }
}