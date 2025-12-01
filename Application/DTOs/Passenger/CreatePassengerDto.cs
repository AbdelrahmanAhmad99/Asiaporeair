 
using System.ComponentModel.DataAnnotations; 
using System.Text.Json.Serialization;
namespace Application.DTOs.Passenger
{
    public class CreatePassengerDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Passport number is required.")]
        [StringLength(20, ErrorMessage = "Passport number cannot exceed 20 characters.")]
        public string PassportNumber { get; set; }

        // Optional: for linking to an existing user's profile
        [JsonIgnore] // Hides from Swagger/Input
        public int? UserId { get; set; }
    }
}
 