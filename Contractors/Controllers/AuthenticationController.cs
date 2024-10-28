using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        private readonly IVerificationService _verificationService;
        public AuthenticationController(IAuthService authService, IVerificationService verificationService)
        {
            _authService = authService;
            _verificationService = verificationService;
        }

        [AllowAnonymous]
        [HttpPost("Signin")]
        public async Task<IActionResult> Signin(LoginDto login, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _authService.AuthenticateAsync(login.NationalCode, login.PhoneNumber);
            if (user.Data == null || user.IsSuccessful == false)
            {
                return BadRequest(user);
            }
            var smsCode = await _verificationService
                .GenerateAndSendCodeAsync(user.Data.Id, user.Data.PhoneNumber, CancellationToken.None);
            if (!smsCode.IsSuccessful)
            {
                return BadRequest(smsCode);
            }
            return Ok(smsCode);
        }

        [AllowAnonymous]
        [HttpPost("VerifyTwoFactorCode")]
        public async Task<IActionResult> VerifyTowFactorCode(GetVerificationCodeDto verificationCodeDto, CancellationToken cancellationToken)
        {
            var token = await _verificationService.VerifyCodeAsync(verificationCodeDto, cancellationToken);
            if (token.Data is null)
            {
                return Unauthorized();
            }

            return Ok(new { Token = token.Data });
        }
    }
}
