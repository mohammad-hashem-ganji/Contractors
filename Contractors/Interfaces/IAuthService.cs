using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IAuthService
    {
        Task<Result<RegisterResultDto>> RegisterAsync(string username, string password, string role,string requestNo = "");
        Task<Result<ApplicationUser>> AuthenticateAsync(string nCode, string phoneNumber, string requestNo = "");
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
