using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController(IAuthService authService, IVerificationService verificationService)
        : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> Signin(LoginDto login, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await authService.AuthenticateAsync(login.NationalCode, login.PhoneNumber);
            if (user.Data == null || user.IsSuccessful == false)
            {
                return BadRequest(user);
            }

            if (user.Data.PhoneNumber != null)
            {
                var smsCode = await verificationService
                    .GenerateAndSendCodeAsync(user.Data.Id, user.Data.PhoneNumber, CancellationToken.None);

                if (!smsCode.IsSuccessful)
                {
                    return BadRequest(smsCode);
                }
                return Ok(smsCode);
            }

            return BadRequest(ErrorMessages.PhoneNumberProcessingFailed);
        }

        [AllowAnonymous]
        [HttpPost("2fa")]
        public async Task<IActionResult> VerifyTowFactorCode(GetVerificationCodeDto verificationCodeDto, CancellationToken cancellationToken)
        {
            var token = await verificationService.VerifyCodeAsync(verificationCodeDto, cancellationToken);
            if (token.Data is null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }
    }
}
