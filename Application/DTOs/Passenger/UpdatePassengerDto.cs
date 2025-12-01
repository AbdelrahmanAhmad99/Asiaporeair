using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Passenger
{
    public class UpdatePassengerDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? PassportNumber { get; set; }
    }
}