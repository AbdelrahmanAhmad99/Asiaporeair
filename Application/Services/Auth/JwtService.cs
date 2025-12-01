using Application.Models;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;

        public JwtService(IOptions<JwtSettings> jwtSettings, UserManager<AppUser> userManager)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(AppUser user)
        {
            // Creating basic claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim("UserType", user.UserType.ToString())
            };

            // Adding customized prompts based on user type
            // This step is very important for differentiating users in the front-end
            switch (user.UserType)
            {
                case Domain.Enums.UserType.Pilot:
                case Domain.Enums.UserType.Attendant:
                case Domain.Enums.UserType.Admin:
                case Domain.Enums.UserType.SuperAdmin:
                case Domain.Enums.UserType.Supervisor:
                   // You can add EmployeeId to the Claims here if available.
                   // The associated entity must be loaded first if it is not already loaded.
                   // if (user.Admin?.EmployeeId != null) claims.Add(new Claim("EmployeeId", user.Admin.EmployeeId.ToString()));
                    break;
                case Domain.Enums.UserType.User:
                    // You can add KrisFlyerTier if available
                    // if (user.User?.KrisFlyerTier != null) claims.Add(new Claim("KrisFlyerTier", user.User.KrisFlyerTier));
                    break;
            }

            // Adding user roles as prompts
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Create a signature key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_jwtSettings.DurationInDays);

            // Token Creation
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}