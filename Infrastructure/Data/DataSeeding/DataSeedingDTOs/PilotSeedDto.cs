using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Professional DTO for deserializing Pilot data, consolidating fields for AppUser, Employee, CrewMember, and Pilot entities.
    /// This DTO supports the multi-table insertion required for employee/crew roles.
    /// </summary>
    public class PilotSeedDto
    {
        // --- AppUser Properties ---
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("added_by_id")]
        public string AddedById { get; set; } = string.Empty; // FK to the AppUser who created this pilot (e.g., Admin/SuperAdmin)

        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        // --- Employee Properties ---
        [JsonPropertyName("date_of_hire")]
        public DateTime DateOfHire { get; set; }

        [JsonPropertyName("salary")]
        public decimal Salary { get; set; }

        // ShiftPreferenceFk is nullable in Employee.cs, can be omitted or included as nullable int if needed.
        [JsonPropertyName("shift_preference_fk")]
        public int? ShiftPreferenceFk { get; set; }

        // --- CrewMember Properties ---
        [JsonPropertyName("crew_base_airport_id")] // FK to Airport.json (iata_code)
        public string CrewBaseAirportId { get; set; } = string.Empty;

        [JsonPropertyName("position")] // Should be "Pilot"
        public string Position { get; set; } = "Pilot";


        // --- Pilot Specific Properties ---
        [JsonPropertyName("license_number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [JsonPropertyName("total_flight_hours")]
        public int TotalFlightHours { get; set; }

        [JsonPropertyName("aircraft_type_fk")] // FK to AircraftType.json (type_id)
        public int AircraftTypeId { get; set; }

        [JsonPropertyName("last_sim_check_date")]
        public DateTime LastSimCheckDate { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}