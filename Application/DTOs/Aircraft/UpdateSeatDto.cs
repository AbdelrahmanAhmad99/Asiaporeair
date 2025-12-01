using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft  
{
    // DTO for updating an existing seat (Admin action)
    public class UpdateSeatDto
    {
        [Required]
        public int CabinClassId { get; set; } // To move seat to a different cabin

        public bool IsWindow { get; set; }
        public bool IsExitRow { get; set; }
         
        public bool IsAisle { get; set; }
    }
}