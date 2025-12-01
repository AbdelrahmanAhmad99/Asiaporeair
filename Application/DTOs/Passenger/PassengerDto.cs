using System;

namespace Application.DTOs.Passenger
{
    // DTO for displaying passenger details.
    public class PassengerDto
    {
        public int PassengerId { get; set; }
        public int LinkedUserId { get; set; } // The User.UserId this passenger profile belongs to
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? PassportNumber { get; set; }
        public string? FrequentFlyerCardNumber { get; set; }
        // We can add FrequentFlyer info if needed, but it's linked via User, not directly Passenger
    }
}