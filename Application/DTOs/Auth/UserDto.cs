namespace Application.DTOs.Auth
{
    public class UserDto : RegisterDtoBase
    {
        // Properties specific to a regular user (Passenger)
        public int? FrequentFlyerId { get; set; }
        public string? KrisFlyerTier { get; set; }
    }
}