using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CrewScheduling
{
    // DTO for requesting a crew member's schedule.
    public class CrewScheduleRequestDto
    {
        [Required]
        public int CrewMemberEmployeeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}