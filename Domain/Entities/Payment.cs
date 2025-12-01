using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Required]
        [Column("booking_fk")]
        public int BookingId { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("method")]
        [StringLength(20)]
        public string Method { get; set; }

        [Required]
        [Column("transaction_datetime")]
        public DateTime TransactionDateTime { get; set; }

        [Required]
        [StringLength(20)]  
        public string Status { get; set; } = "Pending"; // Default status
   
        [StringLength(100)] 
        public string? TransactionId { get; set; }  
        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}