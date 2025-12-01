using Domain.Enums;

namespace Application.DTOs.Auth
{
   
        public class AuthResponseDto
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string UserType { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string? ProfilePictureUrl { get; set; }  
        }
    

    public class UserProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserType { get; set; }
    }
}