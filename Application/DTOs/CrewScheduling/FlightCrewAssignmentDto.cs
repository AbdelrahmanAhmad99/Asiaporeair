namespace Application.DTOs.CrewScheduling
{
    // DTO representing a single crew member assigned to a flight.
    public class FlightCrewAssignmentDto
    {
        public int FlightInstanceId { get; set; }
        public int CrewMemberEmployeeId { get; set; }
        public string CrewMemberName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty; // Pilot/Attendant
        public string AssignedRole { get; set; } = string.Empty; // Captain/First Officer/Attendant etc.
        public string CrewBase { get; set; } = string.Empty;
    }
}