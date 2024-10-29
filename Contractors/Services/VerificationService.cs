using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Identity;

namespace Contractors.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly Random _random = new Random();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManger;
        private readonly IAuthService _autService;

        public VerificationService(ApplicationDbContext context,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManger,
            IAuthService autService)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _signInManger = signInManger;
            _autService = autService;
        }



        public async Task<Result<GetVerificationCodeDto>> GenerateAndSendCodeAsync(int userId, string phoneNumber, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                const string key = "ParsianContractorAuthenearproject";
                string userName = user.UserName;
                if (userName.EndsWith(key))
                {
                    userName = userName.Substring(0, userName.Length - key.Length);
                }
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
                //await _userManager.UpdateAsync(user);
                var result = new GetVerificationCodeDto
                {
                    VerificationCode = token,
                    PhoneNumber = user.PhoneNumber,
                    Nationalcode= userName
                };
                //await SendSmsAsync(phoneNumber, $"کد: {token}", cancellationToken);
                return new Result<GetVerificationCodeDto>()
                    .WithValue(result)
                    .Success(SuccessMessages.CodeGeneratedAndSent);
            }
            return new Result<GetVerificationCodeDto>().WithValue(null).Failure("کد ساخته نشد");
        }
        public async Task<Result<string>> VerifyCodeAsync(GetVerificationCodeDto verificationCodeDto, CancellationToken cancellationToken)
        {

            var result = await _autService.AuthenticateAsync(verificationCodeDto.Nationalcode, verificationCodeDto.PhoneNumber);
            if (result.Data != null)
            {
                var isCodeValid = await _userManager
                    .VerifyTwoFactorTokenAsync(result.Data, TokenOptions.DefaultPhoneProvider, verificationCodeDto.VerificationCode);

                if (isCodeValid)
                {
                    await _signInManger.SignInAsync(result.Data, isPersistent: false);

                    var token = await _autService.GenerateJwtTokenAsync(result.Data);
                   
                    if (!string.IsNullOrEmpty(token))
                    {
                        return new Result<string>()
                       .WithValue(token)
                       .Success("کد تایید شد");
                    }

                    return new Result<string>()
                        .WithValue(null)
                        .Failure("خطا!");

                }

                return new Result<string>()
                       .WithValue(null)
                       .Success("کد نامعتبر است!");
            }
            return new Result<string>()
            .WithValue(null)
            .Success("کاربر یافت نشد");
        }
        //private async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var sender = _configuration["Kavenegar:Sender"];
        //        var apiKey = _configuration["Kavenegar:ApiKey"];
        //        var api = new Kavenegar.KavenegarApi(apiKey);
        //        api.Send(sender, phoneNumber, message);
        //    }
        //    catch (Kavenegar.Exceptions.ApiException ex)
        //    {
        //        throw new InvalidOperationException($"Kavenegar API error: {ex.Message}");
        //    }
        //    catch (Kavenegar.Exceptions.HttpException ex)
        //    {
        //        throw new InvalidOperationException($"Kavenegar HTTP error: {ex.Message}");
        //    }
        //}
    }
}
