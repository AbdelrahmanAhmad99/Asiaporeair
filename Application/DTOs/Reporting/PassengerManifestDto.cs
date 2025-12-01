using System;
using System.Collections.Generic;

namespace Application.DTOs.Reporting
{
    // DTO representing a single passenger manifest for a flight.
    public class PassengerManifestDto
    {
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime ScheduledDeparture { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string AircraftTailNumber { get; set; } = string.Empty;
        public int ConfirmedPassengers { get; set; }

        public List<ManifestPassengerDto> Passengers { get; set; } = new List<ManifestPassengerDto>();
    }

    // Helper DTO for PassengerManifestDto
    public class ManifestPassengerDto
    {
        public int PassengerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PassportNumber { get; set; }
        public string SeatNumber { get; set; } = "N/A";
        public string CabinClass { get; set; } = "N/A";
        public string TicketStatus { get; set; } = string.Empty; // Issued, CheckedIn, Boarded
        public string BookingReference { get; set; } = string.Empty;
        public string? FrequentFlyerNumber { get; set; }
    }
}