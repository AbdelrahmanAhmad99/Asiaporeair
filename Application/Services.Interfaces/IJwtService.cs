using Domain.Entities;
using Microsoft.AspNetCore.Identity;
 
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(AppUser user);
    }
}