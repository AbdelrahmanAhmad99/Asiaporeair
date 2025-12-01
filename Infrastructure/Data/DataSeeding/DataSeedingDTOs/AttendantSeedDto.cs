using System.Text.Json.Serialization;
using System;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for Attendant Seeding.
    /// Combines properties from AppUser, Employee, CrewMember, and Attendant entities
    /// for professional deserialization from a single JSON file.
    /// </summary>
    public class AttendantSeedDto
    {
        // AppUser Properties (Base Identity and Common User Data)
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty; // Temporary password for seeding

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; } = "https://example.com/images/default_attendant.jpg";

        // Employee Properties
        [JsonPropertyName("date_of_hire")]
        public DateTime? DateOfHire { get; set; }

        [JsonPropertyName("salary")]
        public decimal? Salary { get; set; }

        [JsonPropertyName("shift_preference_fk")]
        public int? ShiftPreferenceFk { get; set; } // Foreign Key to ShiftPreference table

        // CrewMember Properties
        [JsonPropertyName("crew_base_airport_id")]
        public string CrewBaseAirportId { get; set; } = string.Empty; // FK to Airport (IATA Code)

        [JsonPropertyName("position")]
        public string Position { get; set; } = "Flight Attendant"; // Default position for clarity
    }
}