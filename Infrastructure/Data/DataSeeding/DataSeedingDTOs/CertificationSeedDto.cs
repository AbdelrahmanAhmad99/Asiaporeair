using System.Text.Json.Serialization;
using System;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for Certification Seeding.
    /// Maps directly to the Certification entity properties for professional deserialization.
    /// </summary>
    public class CertificationSeedDto
    {
        // Properties from Certification Entity

        [JsonPropertyName("crew_member_id")]
        public int CrewMemberId { get; set; } // FK to CrewMember (which is also EmployeeId)

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("issue_date")]
        public DateTime? IssueDate { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}