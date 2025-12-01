using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTOs.FrequentFlyer;
using Application.DTOs.Passenger;
using Application.DTOs.AncillaryProduct; 

namespace Application.DTOs.Booking
{
    // DTO for initiating the booking creation process.
    public class CreateBookingDto
    {
        [Required]
        public int FlightInstanceId { get; set; }

        [Required]
        [StringLength(10)]
        public string FareBasisCode { get; set; } = string.Empty;

        // Use the dedicated CreatePassengerDto
        [Required]
        [MinLength(1, ErrorMessage = "At least one passenger is required.")]
        public List<CreatePassengerDto> Passengers { get; set; } = new List<CreatePassengerDto>();

        // Optional list of ancillary product purchases.
        public List<AddAncillaryDto>? AncillaryPurchases { get; set; }
    }
} 

namespace Application.DTOs.Booking
{
    public class PassengerDetailsDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string PassportNumber { get; set; }

        // Optional: for linking to a frequent flyer account
        public FrequentFlyerDto? FrequentFlyer { get; set; }
    }
}
 