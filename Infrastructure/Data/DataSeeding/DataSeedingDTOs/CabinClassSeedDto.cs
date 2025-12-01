using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for seeding the CabinClass entity.
    /// Used to deserialize data from CabinClass.json.
    /// The CabinClassId is intentionally omitted as it is an IDENTITY column managed by the database.
    /// </summary>
    public class CabinClassSeedDto
    { 
        [JsonPropertyName("ConfigId")]
        public int ConfigId { get; set; }
         
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}