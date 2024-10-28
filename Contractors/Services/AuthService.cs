using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Models;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Contractors.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManger;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            SignInManager<ApplicationUser> signInManger,
            ApplicationDbContext context,
            IConfiguration configuration,
            JwtSettings jwtSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManger = signInManger;
            _context = context;
            _configuration = configuration;
            _jwtSettings = jwtSettings;
        }
        public async Task<Result<ApplicationUser>> AuthenticateAsync(string nCode, string phoneNumber)
        {
            const string key = "ParsianContractorAuthenearproject";
            var user = await _userManager.FindByNameAsync(string.Concat(nCode, key));

            if (user == null || user.PhoneNumber != phoneNumber)
            {
                return new Result<ApplicationUser>()
                    .WithValue(null)
                    .Failure(ErrorMessages.InvalidUserNameOrPassword);
            }

            return new Result<ApplicationUser>()
                .WithValue(user)
                .Success("کاربر پیدا شد.");
        }

        public async Task<Result<RegisterResultDto>> RegisterAsync(string nCode, string phoneNumber, string role)
        {
            const string key = "ParsianContractorAuthenearproject";

            var user = new ApplicationUser
            {
                UserName = string.Concat(nCode, key),
                PhoneNumber = phoneNumber
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                RegisterResultDto registerResult = new RegisterResultDto
                {
                    IdentityResult = result,
                    RegisteredUserId = 0,
                };
                return new Result<RegisterResultDto>()
                    .WithValue(registerResult)
                    .Failure("کاربر ساخته نشد !");
            }
            else
            {

                var addToRoleResult = await _userManager.AddToRoleAsync(user, role);

                if (!addToRoleResult.Succeeded)
                {
                    RegisterResultDto registerResult = new RegisterResultDto
                    {
                        IdentityResult = result,
                        RegisteredUserId = 0
                    };
                    return new Result<RegisterResultDto>()
                        .WithValue(registerResult)
                        .Failure($"  نقش  {role}یافت نشد  ");
                }
                else
                {
                    RegisterResultDto registerResult = new RegisterResultDto
                    {
                        IdentityResult = IdentityResult.Success,
                        RegisteredUserId = user.Id
                    };
                    return new Result<RegisterResultDto>()
                        .WithValue(registerResult)
                        .Success(SuccessMessages.UserRegistered);
                }
            }
        }

        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            //var claims = new[]
            //{
            //    //new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    new Claim(ClaimTypes.Name, user.UserName),
            //    new Claim(ClaimTypes.Role,"Client")
            //};
            var roles = _userManager.GetRolesAsync(user);
            var role = "";
            foreach (var item in roles.Result)
            {
                role = item.ToString();
            }
            var claims = new List<Claim>
             {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString() ?? string.Empty),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 //new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.DateTime),
                 new Claim("FirstName", user.UserName ?? string.Empty),
                 //new Claim("LastName", user.LastName ?? string.Empty),
                 
                 new Claim(ClaimTypes.Role,role)
             };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
