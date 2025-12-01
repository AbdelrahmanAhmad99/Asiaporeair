namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{ 
    public class CrewMemberSeedDto
    { 
        public int EmployeeId { get; set; }
         
        public string CrewBaseAirportId { get; set; } = string.Empty;
         
        public string Position { get; set; } = string.Empty;
    }
}