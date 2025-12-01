namespace Application.DTOs.Passenger
{
    // DTO for searching/filtering passenger records.
    public class PassengerFilterDto
    {
        public string? NameContains { get; set; }
        public string? PassportNumber { get; set; }
        public int? LinkedUserId { get; set; } // Filter by the User account they belong to
        public bool IncludeDeleted { get; set; } = false;
    }
}