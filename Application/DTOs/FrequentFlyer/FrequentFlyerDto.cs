namespace Application.DTOs.FrequentFlyer
{
    // DTO for displaying frequent flyer account details.
    public class FrequentFlyerDto
    {
        public int FlyerId { get; set; }
        public int LinkedUserId { get; set; } // The User.UserId this account belongs to
        public string LinkedUserName { get; set; } = string.Empty; // e.g., "John Doe"
        public string CardNumber { get; set; } = string.Empty;
        public string Level { get; set; } = "Member"; // e.g., Member, Silver, Gold, PPS
        public int AwardPoints { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}