using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FrequentFlyer
{
    // DTO for creating a new frequent flyer account and linking it to a user.
    public class CreateFrequentFlyerDto
    {
        [Required]
        public int UserId { get; set; } // The User.UserId to link this account to.

        [Required]
        [StringLength(20, MinimumLength = 5)] // Assuming card numbers have a min length
        public string CardNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string Level { get; set; } = "Member"; // Default level

        [Range(0, int.MaxValue)]
        public int InitialAwardPoints { get; set; } = 0;
    }
}