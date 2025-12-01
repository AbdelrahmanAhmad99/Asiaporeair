using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
namespace Domain.Enums
{
    public enum TicketStatus
    {                   
        Issued,         
        Confirmed,      
        Pending,        
        Used,           
        Cancelled,      
        Refunded,       
        Boarded,        
        CheckedIn
    }
}