using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for AircraftType entity seeding via JSON file.
    /// Matches the non-key properties of the Domain.Entities.AircraftType entity.
    /// </summary>
    public class AircraftTypeSeedDto
    {
        // Corresponds to [Column("model")] and Model (VARCHAR(50) NOT NULL)
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        // Corresponds to [Column("manufacturer")] and Manufacturer (VARCHAR(50) NOT NULL)
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        // Corresponds to [Column("range_km")] and RangeKm (INT NULL)
        [JsonPropertyName("range_km")]
        public int? RangeKm { get; set; }

        // Corresponds to [Column("max_seats")] and MaxSeats (INT NULL)
        [JsonPropertyName("max_seats")]
        public int? MaxSeats { get; set; }

        // Corresponds to [Column("cargo_capacity")] and CargoCapacity (DECIMAL(10,2) NULL)
        [JsonPropertyName("cargo_capacity")]
        public decimal? CargoCapacity { get; set; }

        // Corresponds to [Column("cruising_velocity")] and CruisingVelocity (INT NULL)
        [JsonPropertyName("cruising_velocity")]
        public int? CruisingVelocity { get; set; }

        // Corresponds to [Column("IsDeleted")] and IsDeleted (BIT NOT NULL DEFAULT 0)
        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}