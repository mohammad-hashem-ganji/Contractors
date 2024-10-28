using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IVerificationService
    {
        Task<Result<GetVerificationCodeDto>> GenerateAndSendCodeAsync(int userId, string phoneNumber, CancellationToken cancellationToken);
        Task<Result<string>> VerifyCodeAsync(GetVerificationCodeDto verificationCodeDto, CancellationToken cancellationToken);
    }
}
