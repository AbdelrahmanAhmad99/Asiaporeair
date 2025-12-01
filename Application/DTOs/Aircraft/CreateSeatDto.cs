using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft // Or Application.DTOs.Seat
{
    // DTO for creating a new seat (Admin action)
    public class CreateSeatDto
    {
        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } // e.g., "1A"

        [Required]
        public int CabinClassId { get; set; }

        public bool IsWindow { get; set; } = false;
        public bool IsExitRow { get; set; } = false;
         
        public bool IsAisle { get; set; } = false;
    }
}