
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Contractors.Services
{
    public class BidStatusService : IBidStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BidStatusService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<AddBidStatusDto>> AddAsync(AddBidStatusDto bidDto, CancellationToken cancellationToken)
        {
            if (bidDto is null)
            {
                return new Result<AddBidStatusDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {
                int userId;
                if (bidDto.CreatedBy == 100)
                {
                    userId = 100;
                }
                else
                {
                    bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                }

                var bidStatus = new BidStatus
                {
                    ContractorBidId = bidDto.BidOfContractorId,
                    CreatedBy = userId,
                    Status = bidDto.Status,
                    CreatedAt = DateTime.Now
                };
                await _context.BidStatuses.AddAsync(bidStatus, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<AddBidStatusDto>().WithValue(bidDto).Success(SuccessMessages.BidStatusAdded);
            }
            catch (Exception)
            {
                return new Result<AddBidStatusDto>().WithValue(null).Failure(ErrorMessages.ErrorWhileAddingBidStatus);
            }
        }

        public async Task<Result<List<BidStatusDto>>> GetRequestStatusesByBidId(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.BidStatuses
                    .Where(x => x.ContractorBidId == bidId)
                    .Include(x => x.ContractorBid)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new BidStatusDto
                    {
                        CreatedAt = x.CreatedAt,
                        CreatedBy = x.CreatedBy,
                        BidOfContractorId = x.ContractorBidId,
                        Status = x.Status
                    }).ToListAsync(cancellationToken);
                if (result is null) return new Result<List<BidStatusDto>>()
                        .WithValue(null)
                        .Failure(ErrorMessages.BidStatusNotFound);
                else return new Result<List<BidStatusDto>>()
                        .WithValue(result)
                        .Success(SuccessMessages.BidStatusFound);
            }
            catch (Exception)
            {
                return new Result<List<BidStatusDto>>()
                   .WithValue(null)
                   .Failure(ErrorMessages.ErrorWhileRetrievingStatus);
            }
        }


    }
}
