
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;

namespace Contractors.Services
{
    public class RejectService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRejectService
    {
        public async Task<Result<AddReasonToRejectRequestDto>> AddRejectRequestAsync(AddReasonToRejectRequestDto rejectRequestDto, CancellationToken cancellationToken)
        {
            try
            {
                var isAppUserIdConvert = int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out int appUserId);
                if (!isAppUserIdConvert)
                {
                    return new Result<AddReasonToRejectRequestDto>().WithValue(null).Failure("خطا");
                }
                var rejectRequest = new Reject
                {
                    UserId = appUserId,
                    RequestId = rejectRequestDto.RequestId,
                    Reason = rejectRequestDto.Reason,
                    Comment = rejectRequestDto.Comment,
                    CreatedAt = DateTime.Now,
                    CreatedBy = appUserId
                };
                await context.Rejects.AddAsync(rejectRequest, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                return new Result<AddReasonToRejectRequestDto>().WithValue(rejectRequestDto).Success("دلیل رد پروژه ثبت شد.");
            }
            catch (Exception)
            {

                return new Result<AddReasonToRejectRequestDto>().WithValue(null).Failure("خطا");
            }
        }


        public async Task<Result<List<GetReasonOfRejectRequestDto>>> GetReasonsOfRejectingRequestByRequestIdAsync(int requestId, CancellationToken cancellationToken)
        {
            try
            {
                var reason = await context.Rejects
                .Where(x => x.RequestId == requestId)
                .ToListAsync(cancellationToken);
                if (reason is null)
                {
                    return new Result<List<GetReasonOfRejectRequestDto>>().WithValue(null).Success("دلیلی پیدا نشد.");
                }
                List<GetReasonOfRejectRequestDto> reasonDto = reason.Select(r => new GetReasonOfRejectRequestDto
                {
                    Id = r.Id,
                    RequestId = r.RequestId,
                    Reason = r.Reason,
                    DateRejected = r.CreatedAt,
                    UserId = r.CreatedBy
                }).ToList();
                return new Result<List<GetReasonOfRejectRequestDto>>()
                    .WithValue(reasonDto)
                    .Success("دلیل‌های رد درخواست یافت شد.");
            }
            catch (Exception)
            {
                return new Result<List<GetReasonOfRejectRequestDto>>().WithValue(null).Failure("خطا!");
            }
            
        }


        public async Task<Result<List<GetReasonOfRejectRequestDto>>> GetReasonsOfRejectingRequestByClientAsync(CancellationToken cancellationToken)
        {
            try
            {
                var isAppUserIdConvert = int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out int appUserId);
                if (!isAppUserIdConvert)
                {
                    return new Result<List<GetReasonOfRejectRequestDto>>().WithValue(null).Failure("خطا");
                }
                var reason = await context.Rejects
                    .Where(x => x.UserId == appUserId)
                    .ToListAsync(cancellationToken);
                if (!reason.Any())
                {
                    return new Result<List<GetReasonOfRejectRequestDto>>().WithValue(null).Success("دلیلی پیدا نشد.");
                }
                List<GetReasonOfRejectRequestDto> reasonDto = reason.Select(r => new GetReasonOfRejectRequestDto
                {
                    Id = r.Id,
                    RequestId = r.RequestId,
                    Reason = r.Reason,
                    DateRejected = r.CreatedAt,
                    UserId = r.CreatedBy
                }).ToList();
                return new Result<List<GetReasonOfRejectRequestDto>>()
                    .WithValue(reasonDto)
                    .Success("دلیل‌های رد درخواست یافت شد.");

            }
            catch (Exception)
            {
                return new Result<List<GetReasonOfRejectRequestDto>>().WithValue(null).Failure("خطا!");
            }
        }
    }
}
