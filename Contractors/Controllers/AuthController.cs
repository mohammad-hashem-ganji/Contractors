//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Contractors.Models;
//using Contractors.Entites;
//using Contractors.Dtos;
//using Contractors.Services;
//using Contractors.Utilities.Constants;
//using Contractors.Interfaces;
//using Contractors.Results;
//using Azure.Core;

//[Route("api/[controller]")]
//[ApiController]
//public class AuthController(UserManager<ApplicationUser> userManager, JwtSettings jwtSettings)
//    : ControllerBase
//{


//    //private readonly IVerificationService _verificationService;
//    //private readonly ILastLoginHistoryService _lastLoginHistoryService;
//    [HttpPost("login")]
//    public async Task<IActionResult> Login([FromBody] LoginModel model)
//    {
//        var user = await userManager.FindByNameAsync(model.Username);
//        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
//        {
//            var token = GenerateJwtToken(user);

//            Console.WriteLine("Generated JWT: " + token);

//            return Ok(new { Token = token });
//        }

//        return Unauthorized();
//    }
   
//    private string GenerateJwtToken(ApplicationUser user)
//    {
//        //var claims = new[]
//        //{
//        //    //new Claim(JwtRegisteredClaimNames.Sub, user.Id),
//        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//        //    new Claim(ClaimTypes.Name, user.UserName),
//        //    new Claim(ClaimTypes.Role,"Client")
//        //};
//        var roles = userManager.GetRolesAsync(user);
//        var role = "";
//        foreach (var item in roles.Result)
//        {
//            role = item.ToString();
//        }
//        var claims = new List<Claim>
//             {
//                 new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
//                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                 //new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.DateTime),
//                 new Claim("FirstName", user.UserName ?? string.Empty),
//                 //new Claim("LastName", user.LastName ?? string.Empty),
                 
//                 new Claim(ClaimTypes.Role,role)
//             };

//        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
//        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//        var token = new JwtSecurityToken(
//            issuer: jwtSettings.Issuer,
//            audience: jwtSettings.Audience,
//            claims: claims,
//            expires: DateTime.Now.AddMinutes(jwtSettings.ExpiryMinutes),
//            signingCredentials: creds
//        );

//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }
//}

//public class LoginModel
//{
//    public string Username { get; set; }
//    public string Password { get; set; }
//}