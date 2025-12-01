using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Employee
{
    // A specific DTO for the HR-only action of updating a salary.
    public class UpdateSalaryRequestDto
    {
        [Required]
        [Range(1000, 1000000, ErrorMessage = "Salary must be within a valid range.")]
        public decimal NewSalary { get; set; }
    }
}