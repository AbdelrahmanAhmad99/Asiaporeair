using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Infrastructure.ExternalServices.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        
        private string GetEmailTemplate(string content)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Asiaporeair</title>
                <style>
                    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f7f6; }}
                    .container {{ max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.07); overflow: hidden; }}
                    .header {{ background-color: #00529b; color: #ffffff; padding: 40px 20px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 28px; font-weight: 600; }}
                    .content {{ padding: 30px 40px; }}
                    .credentials {{ background-color: #f9f9f9; border-radius: 8px; padding: 20px; margin: 25px 0; border-left: 4px solid #f9a825; }}
                    .button {{ display: inline-block; background-color: #f9a825; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 30px; margin-top: 20px; font-weight: 600; text-align: center; }}
                    .footer {{ text-align: center; padding: 25px 20px; font-size: 14px; color: #777; background-color: #f1f1f1; }}
                    .warning {{ color: #d32f2f; font-weight: 600; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Asiaporeair</h1>
                    </div>
                    <div class='content'>
                        {content}
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.UtcNow.Year} Asiaporeair. All rights reserved.
                    </div>
                </div>
            </body>
            </html>";
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string username, string resetToken)
        {
            var subject = "Password Reset Request";
            var content = $@"
                <h2>Password Reset</h2>
                <p>Hello {username},</p>
                <p>You requested to reset your password. Please click the button below to set a new one.</p>
                <p>If you did not request this, please ignore this email.</p>
                <a href='{_emailSettings.FrontendBaseUrl}/reset-password?email={toEmail}&token={Uri.EscapeDataString(resetToken)}' class='button'>Reset Password</a>
            ";
            await SendEmailAsync(toEmail, subject, GetEmailTemplate(content));
        }

        public async Task SendPasswordChangedNotificationAsync(string toEmail, string username)
        {
            var subject = "Your Password Has Been Changed";
            var content = $@"
                <h2>Password Changed Successfully</h2>
                <p>Hello {username},</p>
                <p>This is a confirmation that the password for your account has been changed successfully.</p>
                <p>If you did not make this change, please contact our support team immediately.</p>
                <a href='{_emailSettings.FrontendBaseUrl}/login' class='button'>Login to Your Account</a>
            ";
            await SendEmailAsync(toEmail, subject, GetEmailTemplate(content));
        }

     
        public Task SendUserCredientialsEmailAsync(string toEmail, string username, string password)
        {
            var subject = "Welcome to Asiaporeair - Your Account Credentials";
            var content = $@"
                <h2>Welcome to Asiaporeair!</h2>
                <p>Your account has been created successfully.</p>
                <p>Below are your login credentials:</p>
                <div class='credentials'>
                    <p><strong>Username/Email:</strong> {username}</p>
                    <p><strong>Password:</strong> {password}</p>
                </div>
                <p class='warning'>Please change your password after your first login for security reasons.</p>
                <a href='{_emailSettings.FrontendBaseUrl}/login' class='button'>Login to Your Account</a>
            ";

            return SendEmailAsync(toEmail, subject, GetEmailTemplate(content));
        }
        public Task SendCompanyUserCredientialsEmailAsync(string toEmail, string username, string password, string fullName, string roleName, string companyName)
        {
            
            return Task.CompletedTask;
        }
    }
}