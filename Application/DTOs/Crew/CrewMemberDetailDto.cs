using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Application.DTOs.Crew
{
    // Detailed DTO for viewing a specific crew member's profile and certifications.
    public class CrewMemberDetailDto
    {
        // Basic Info (from AppUser & Employee)
        public int EmployeeId { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfHire { get; set; }
        public UserType UserType { get; set; } // Pilot or Attendant
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Crew Specific Info
        public string Position { get; set; } = string.Empty;
        public string CrewBaseAirportIata { get; set; } = string.Empty;
        public string CrewBaseAirportName { get; set; } = string.Empty; // Added for display

        // Pilot Specific (if applicable)
        public string? LicenseNumber { get; set; }
        public int? TotalFlightHours { get; set; }
        public string? AircraftTypeRatingModel { get; set; } // e.g., "Boeing 777"
        public DateTime? LastSimCheckDate { get; set; }

        // Certifications
        public List<CertificationDto> Certifications { get; set; } = new List<CertificationDto>();
    }
}