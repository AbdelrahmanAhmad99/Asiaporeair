namespace Application.DTOs.Seat
{
    // DTO showing which passenger is assigned to which seat in a booking.
    public class SeatAssignmentDto
    {
        public int BookingId { get; set; }
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string SeatId { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string CabinClassName { get; set; } = string.Empty;
    }
}