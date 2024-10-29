
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;
using System.Threading;

namespace Contractors.Services
{
    public class RequestStatusService : IRequestStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestStatusService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<AddRequestStatusDto>> AddAsync(AddRequestStatusDto requestStatusDto, CancellationToken cancellationToken)
        {
            if (requestStatusDto is null)
            {
                return new Result<AddRequestStatusDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {

                int userId;
                if (requestStatusDto.CreatedBy == 100)
                {
                    userId = 100;
                }
                else
                {
                    bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                }


                var requestStatus = new RequestStatus
                {
                    RequestId = requestStatusDto.RequestId,
                    Status = requestStatusDto.Status,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                };
                await _context.RequestStatuses.AddAsync(requestStatus, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new Result<AddRequestStatusDto>().WithValue(requestStatusDto).Success(SuccessMessages.RequestStatusAdded);
            }
            catch (Exception)
            {
                return new Result<AddRequestStatusDto>().WithValue(null).Failure(ErrorMessages.ErrorWhileAddingRequestStatus);
            }
        }
        public async Task<Result<RequestStatusDto>> GetByIdAsync(int reqStatusId, CancellationToken cancellationToken)
        {
            try
            {
                RequestStatus? requestStatus = await _context.RequestStatuses
                    .Where(x => x.Id == reqStatusId)
                    .Include(s => s.Request)
                    .FirstOrDefaultAsync(cancellationToken);
                if (requestStatus == null)
                {
                    return new Result<RequestStatusDto>()
                        .WithValue(null)
                        .Failure(ErrorMessages.EntityIsNull);
                }
                else
                {
                    var requestStatusDto = new RequestStatusDto
                    {
                        Id = requestStatus.Id,
                        Status = requestStatus.Status,
                        RequestId = requestStatus.RequestId,
                        CreatedAt = requestStatus.CreatedAt,
                        CreatedBy = requestStatus.CreatedBy,
                    };
                    return new Result<RequestStatusDto>()
                        .WithValue(requestStatusDto)
                        .Success("وضعیت پیدا شد .");
                }
            }
            catch (Exception)
            {
                return new Result<RequestStatusDto>()
                    .WithValue(null)
                    .Failure(ErrorMessages.ErrorWhileRetrievingStatus);
            }
        }
        public async Task<Result<RequestStatusDto>> UpdateAsync(RequestStatusDto requestStatusDto, CancellationToken cancellationToken)
        {
            try
            {
                int userId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                RequestStatus? requestStatus = await _context.RequestStatuses
                    .Where(x => x.Id == requestStatusDto.Id)
                    .Include(s => s.Request)
                    .FirstOrDefaultAsync(cancellationToken);
                if (requestStatus == null)
                {
                    return new Result<RequestStatusDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
                }
                requestStatus.RequestId = requestStatusDto.RequestId;
                requestStatus.Status = requestStatusDto.Status;
                requestStatus.UpdatedAt = DateTime.Now;
                requestStatus.UpdatedBy = userId;
                _context.RequestStatuses.Update(requestStatus);
                await _context.SaveChangesAsync();

                return new Result<RequestStatusDto>()
                    .WithValue(requestStatusDto)
                    .Success($"وضعیت تغییر به {requestStatusDto.Status}تغییر پیدا کرد.");
            }
            catch (Exception)
            {
                return new Result<RequestStatusDto>()
                    .WithValue(null).Failure(ErrorMessages.AnErrorWhileUpdatingStatus);
            }
        }
        public async Task<Result<List<RequestStatusDto>>> GetRequestStatusesByRequestId(int requesId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.RequestStatuses
                    .Where(x =>
                    x.RequestId == requesId)
                    .Include(s => s.Request)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(r => new RequestStatusDto
                    {
                        RequestId = r.RequestId,
                        Status = r.Status,
                        CreatedBy = r.CreatedBy,
                        CreatedAt = r.CreatedAt
                    }).ToListAsync(cancellationToken);
                if (result.Count == 0) return new Result<List<RequestStatusDto>>()
                        .WithValue(null)
                        .Success(ErrorMessages.RequestStatusNotFound);
                else return new Result<List<RequestStatusDto>>()
                        .WithValue(result)
                        .Success(SuccessMessages.RequestStatusFound);
            }
            catch (Exception)
            {
                return new Result<List<RequestStatusDto>>()
                    .WithValue(null)
                    .Failure(ErrorMessages.ErroWhileRetrievingRequestStatus);
            }
        }
    }
}



