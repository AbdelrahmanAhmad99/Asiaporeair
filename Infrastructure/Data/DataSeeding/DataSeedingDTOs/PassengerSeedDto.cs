using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for deserializing Passenger data from the JSON file.
    /// It matches the required properties of the Passenger entity (excluding Identity fields).
    /// </summary>
    public class PassengerSeedDto
    { 
        [JsonPropertyName("User_fk")]
        public int UserId { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("passport_number")]
        public string PassportNumber { get; set; }

        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}