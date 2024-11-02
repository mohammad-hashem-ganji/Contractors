using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    /// <summary>
    /// احراز هویت که مسئول مدیریت عملیات‌های ورود و تأیید دو مرحله‌ای است.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController(IAuthService authService, IVerificationService verificationService)
        : ControllerBase
    {
        /// <summary>
        /// ورود کاربر با تأیید کد ملی و شماره تلفن ارائه شده.
        /// </summary>
        /// <param name="login">مدل کاربر شامل کد ملی و شماره تلفن.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>کد تأیید در صورت موفقیت، یا پیام خطا در صورت شکست فرآیند.</returns>
        /// <response code="200">کد تأیید برای کاربر را باز می‌گرداند.</response>
        /// <response code="400">در صورت نامعتبر بودن اعتبارنامه‌ها یا وجود خطا در پردازش شماره تلفن.</response>
        /// <response code="500">در صورت وجود خطای داخلی در سرور.</response>
        [AllowAnonymous]
        [HttpPost("signin")]
        [ProducesResponseType(typeof(Result<GetVerificationCodeDto>), 200)]  // Successful response
        [ProducesResponseType(typeof(Result<object>), 400)]  // Bad request response
        [ProducesResponseType(500)]  // Internal server error response
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

        /// <summary>
        /// تأیید کد دو مرحله‌ای ارائه شده توسط کاربر.
        /// </summary>
        /// <param name="verificationCodeDto">مدل شامل کد تأیید دو مرحله‌ای.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، اطلاعات کاربر را باز می‌گرداند. در غیر این صورت، پیام خطا.</returns>
        [AllowAnonymous]
        [HttpPost("2fa")]
        [ProducesResponseType(typeof(Result<string>), 200)]  // Successful response
        [ProducesResponseType(401)]  // Unauthorized response
        [ProducesResponseType(400)]  // Bad request response (if needed)
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
