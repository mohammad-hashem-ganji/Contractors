using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IAuthService
    {
        Task<Result<RegisterResultDto>> RegisterAsync(string username, string password, string role);
        Task<Result<ApplicationUser>> AuthenticateAsync(string nCode, string phoneNumber);
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
