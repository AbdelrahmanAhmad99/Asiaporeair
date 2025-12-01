using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for deserializing FareBasisCode data from a JSON file.
    /// Ensures a decoupled, professional, and maintainable seeding process.
    /// </summary>
    public class FareBasisCodeSeedDto
    { 
        [JsonPropertyName("code")]
        public string Code { get; set; }
         
        [JsonPropertyName("description")]
        public string Description { get; set; }
         
        [JsonPropertyName("rules")]
        public string Rules { get; set; }
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}