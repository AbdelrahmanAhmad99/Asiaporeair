using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{ 
    public abstract class UpdateEmployeeProfileDto : UpdateProfileDto
    { 
        public DateTime? DateOfHire { get; set; }
         
        [Range(0, 1000000, ErrorMessage = "Salary must be a non-negative value.")] // Example range
        public decimal? Salary { get; set; }
    }
}