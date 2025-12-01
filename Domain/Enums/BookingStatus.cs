using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
namespace Domain.Enums
{
    public enum BookingStatus
    {
        Pending,        // The booking has been created but payment is pending.
        Confirmed,      // The payment has been received and the booking is confirmed.
        Cancelled,      // The booking has been cancelled.
        Completed       // The flight has been completed. 
    }
}