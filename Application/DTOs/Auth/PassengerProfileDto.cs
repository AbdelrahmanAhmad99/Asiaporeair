namespace Application.DTOs.Auth
{
    /// <summary>
    /// Profile DTO for a User (Passenger).
    /// </summary>
    public class PassengerProfileDto : UserProfileBaseDto
    {
        // Fields specific to the 'User' entity
        public int? FrequentFlyerId { get; set; }
        public string? KrisFlyerTier { get; set; }

        // You could add related data here if needed, e.g.,
        // public int TotalBookings { get; set; }
    }
}