using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO for updating a User (Passenger) profile.
    /// </summary>
    public class UpdateUserProfileDto : UpdateProfileDto
    {
        /// <summary>
        /// The KrisFlyer loyalty program tier.
        /// </summary>
        [StringLength(20)]
        public string? KrisFlyerTier { get; set; }

        // Note: FrequentFlyerId is usually managed internally or via a separate process,
        // so it's often not included in a general profile update DTO.
    }
}