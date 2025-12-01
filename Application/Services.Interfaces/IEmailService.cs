namespace Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPasswordResetEmailAsync(string toEmail, string username, string resetToken);
        Task SendPasswordChangedNotificationAsync(string toEmail, string username);
        Task SendUserCredientialsEmailAsync(string toEmail, string username, string password);
        Task SendCompanyUserCredientialsEmailAsync(string toEmail, string username, string password, string fullName, string roleName, string companyName);
    }
}