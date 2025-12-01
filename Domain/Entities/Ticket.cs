using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Domain.Enums; 

namespace Domain.Entities
{
    public class Ticket
    { 
        public int TicketId { get; set; }
         
        public string TicketCode { get; set; } = string.Empty;
         
        public DateTime IssueDate { get; set; }
 
        public TicketStatus Status { get; set; }

        public int PassengerId { get; set; }  
         
        public int BookingId { get; set; }
         
        public int FlightInstanceId { get; set; }
         
        public string? SeatId { get; set; }
         
        public int? FrequentFlyerId { get; set; }
         
        public bool IsDeleted { get; set; }

        // --- Navigation Properties ---  
        public virtual Passenger Passenger { get; set; } = null!;
        
        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;
        public virtual FlightInstance FlightInstance { get; set; } = null!;
        public virtual Seat? Seat { get; set; } 
        public virtual FrequentFlyer? FrequentFlyer { get; set; }
    }
}