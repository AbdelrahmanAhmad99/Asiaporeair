using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for Supervisor seeding data, combining properties from AppUser, Employee, and Supervisor entities.
    /// This structure allows for a single, comprehensive JSON record for each Supervisor.
    /// </summary>
    public class SupervisorSeedDto
    {
        // --- AppUser (Identity & Common) Properties --- 
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;
         
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty; 
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
         
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty; 
        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty; 
        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }
         
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
         
        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
         
        // --- Employee Properties ---
         
        [JsonPropertyName("date_of_hire")]
        public DateTime? DateOfHire { get; set; } 
        [JsonPropertyName("salary")]
        public decimal? Salary { get; set; } 
        [JsonPropertyName("shift_preference_fk")]
        public int? ShiftPreferenceFk { get; set; } 

        // --- Supervisor Properties ---
         
        [JsonPropertyName("managed_area")]
        public string? ManagedArea { get; set; }
         
        [JsonPropertyName("added_by_id")]
        public string? AddedById { get; set; }
    }
}
 