namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{ 
    public class EmployeeSeedDto
    { 
        public string AppUserId { get; set; } = string.Empty; 
        public DateTime? DateOfHire { get; set; } 
        public decimal? Salary { get; set; }
         
        public int? ShiftPreferenceId { get; set; }
    }
}