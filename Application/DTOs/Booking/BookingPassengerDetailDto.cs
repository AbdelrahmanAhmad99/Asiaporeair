using System;

namespace Application.DTOs.Booking
{
    // Helper DTO for BookingDetailDto to show passenger with seat info.
    public class BookingPassengerDetailDto
    {
        public int PassengerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? AssignedSeatNumber { get; set; } // e.g., "12A"
        public string? CabinClassName { get; set; } // e.g., "Business Class"
    }
}