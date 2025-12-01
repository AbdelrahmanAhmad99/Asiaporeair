namespace Application.DTOs.BoardingPass
{
    // DTO for filtering boarding passes (Gate Agent/Admin).
    public class BoardingPassFilterDto
    {
        public int? FlightInstanceId { get; set; }
        public string? SeatNumber { get; set; }
        public string? PassengerNameContains { get; set; }
        public bool? HasBoarded { get; set; } // Based on Ticket Status = Boarded
        public bool IncludeDeleted { get; set; } = false;
    }
}