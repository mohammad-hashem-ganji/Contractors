using Contractors.Dtos;
using Contractors.Results;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Contractors.Utilities.Constants;
using Contractors.Entites;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Contractors.DbContractorsAuctioneerEF;


namespace Contractors.Services
{
    public static class UserManagement
    {
        



        public static async Task<Result<RoleBaseUserDto>> GetRoleBaseUserId(HttpContext httpContext, ApplicationDbContext context)
        {
            if (httpContext?.User == null)
            {
                return new Result<RoleBaseUserDto>().Failure("خطا.");
            }
            int userId = 0;
            var appId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(appId))
            {
                return new Result<RoleBaseUserDto>().Failure("کاربری یافت نشد!");
            }

            var userRole = httpContext.User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userRole))
            {
                return new Result<RoleBaseUserDto>().Failure("این نقش وجوذدندارد.");
            }
            int applicationUserId = int.Parse(appId);
            switch (userRole)
            {
                case "Client":
                    var client = await context.Clients.FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);
                    if (client != null)
                    {
                       userId  = client.Id;
                    }
                    else
                    {
                        return new Result<RoleBaseUserDto>().Failure("شناسه مشتری یافت نشد.");
                    }
                    break;
                case "Contractor":
                    var contractor = await context.Contractors.FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);
                    if (contractor is not null)
                    {
                        userId = contractor.Id;
                    }
                    else
                    {
                        return new Result<RoleBaseUserDto>().WithValue(null).Failure("شناسه مشتری یافت نشد.");
                    }
                    break;
                default:
                    break;
            }
            var roleBaseUserDto = new RoleBaseUserDto
            {
                UserId = userId, 
                Role = userRole
            };

            return new Result<RoleBaseUserDto>().WithValue(roleBaseUserDto).Success(SuccessMessages.UserIDandroleretrievedsuccessfully);
        }
    }

}
