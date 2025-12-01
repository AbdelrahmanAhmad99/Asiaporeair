using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CrewScheduling
{
    // DTO for assigning one or more crew members to a specific flight instance.
    public class AssignCrewRequestDto
    {
        [Required]
        public int FlightInstanceId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one crew member must be assigned.")]
        public List<CrewAssignmentDetailDto> Assignments { get; set; } = new List<CrewAssignmentDetailDto>();
    }

    // Details for assigning a single crew member.
    public class CrewAssignmentDetailDto
    {
        [Required]
        public int CrewMemberEmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // e.g., "Captain", "First Officer", "Lead Attendant", "Attendant"
    }
}