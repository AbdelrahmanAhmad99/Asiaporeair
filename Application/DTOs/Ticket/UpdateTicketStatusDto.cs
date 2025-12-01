using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Ticket
{
    // DTO for updating the status of a ticket (e.g., CheckedIn, Boarded, Cancelled).
    public class UpdateTicketStatusDto
    {
        [Required]
        [EnumDataType(typeof(TicketStatus), ErrorMessage = "Invalid ticket status value.")]
        public TicketStatus NewStatus { get; set; }
        public string? Reason { get; set; } // Optional reason for status change (e.g., cancellation)
    }
}