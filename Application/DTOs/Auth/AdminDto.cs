namespace Application.DTOs.Auth
{
    public class AdminDto : EmployeeRegisterDtoBase
    {
        // Properties specific to an Admin
        public string? Department { get; set; }
    }
}