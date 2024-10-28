
using Azure.Core;
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;

namespace Contractors.Services
{
    public class BidOfContractorService : IBidOfContractorService
    {

        private readonly ApplicationDbContext _context;
        private readonly IContractorService _contractorService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BidOfContractorService(ApplicationDbContext context,
            IContractorService contractorService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _contractorService = contractorService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<AddBidOfContractorDto>> AddAsync(AddBidOfContractorDto bidOfContractorDto, CancellationToken cancellationToken)
        {

            if (bidOfContractorDto == null)
            {
                return new Result<AddBidOfContractorDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {
                var user = await UserManagement.GetRoleBaseUserId(_httpContextAccessor.HttpContext, _context);

                if (!user.IsSuccessful)
                {
                    return new Result<AddBidOfContractorDto>().WithValue(null).Failure("خطا");
                }
                var contractorId = user.Data.UserId;

                var isAppUserIdConvert = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int appUserId);
                if (!isAppUserIdConvert)
                {
                    return new Result<AddBidOfContractorDto>().WithValue(null).Failure("خطا");
                }
                var isContractorAddedBidForRequest = await _context.BidOfContractors
                    .AnyAsync(x => x.RequestId == bidOfContractorDto.RequestId
                    && x.CreatedBy == appUserId);
                if (isContractorAddedBidForRequest)
                {
                    return new Result<AddBidOfContractorDto>().WithValue(null).Success("قبلا پینهاد قیمت داده اید.");
                }
                var bidOfContractor = new BidOfContractor
                {
                    SuggestedFee = bidOfContractorDto.SuggestedFee,
                    CanChangeBid = true,
                    ContractorId = contractorId,
                    RequestId = bidOfContractorDto.RequestId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = appUserId
                };
                await _context.BidOfContractors.AddAsync(bidOfContractor, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<AddBidOfContractorDto>().WithValue(bidOfContractorDto).Success(SuccessMessages.OperationSuccessful);
            }
            catch (Exception)
            {
                return new Result<AddBidOfContractorDto>().WithValue(null).Failure(ErrorMessages.ErrorWileAddingBidOfContractor);
            }
        }
        public async Task<Result<BidOfContractorDto>> GetByIdAsync(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                int userId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                if (!isconverted)
                {
                    return new Result<BidOfContractorDto>().WithValue(null).Failure("خطا هنگام تغییر پیشنهاد");
                }

                BidOfContractor? bidOfContractor = await _context.BidOfContractors
                    .Where(x => x.Id == bidId)
                    .Include(x => x.BidStatuses)
                    .FirstOrDefaultAsync(cancellationToken);
                if (bidOfContractor == null)
                {
                    return new Result<BidOfContractorDto>().WithValue(null).Success(ErrorMessages.BidOfContractorNotFound);
                }
                else
                {
                    if (bidOfContractor.IsDeleted == true)
                    {
                        return new Result<BidOfContractorDto>().WithValue(null).Success(ErrorMessages.BidIsDeleted);
                    }
                    var bidOfContractorDto = new BidOfContractorDto
                    {
                        Id = bidOfContractor.Id,
                        RequestId = bidOfContractor.RequestId,
                        ContractorId = bidOfContractor.ContractorId,
                        SuggestedFee = bidOfContractor.SuggestedFee,
                        CreatedAt = bidOfContractor.CreatedAt,
                        BidStatuses = bidOfContractor.BidStatuses.Select(b => new BidStatus
                        {
                            Id = b.Id,
                            Status = b.Status,
                            UpdatedAt = b.UpdatedAt,
                            UpdatedBy = b.CreatedBy
                        })
                        .OrderByDescending(b => b.CreatedAt)
                        .ToList()
                    };
                    return new Result<BidOfContractorDto>().WithValue(bidOfContractorDto).Success("پیشنهاد پیدا شد");
                }
            }
            catch (Exception ex)
            {
                return new Result<BidOfContractorDto>().WithValue(null).Failure(ex.Message);
            }
        }
        public async Task<Result<List<BidOfContractorDto>>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                var bidsOfContractor = await _context.BidOfContractors
                    .Include(x => x.Contractor)
                    .Include(x => x.Request)
                    .Include(x => x.BidStatuses)
                    .ToListAsync(cancellationToken);
                var bidsOfContractorDto = bidsOfContractor.Where(a => a.IsDeleted == false).Select(x => new BidOfContractorDto
                {
                    Id = x.Id,
                    RequestId = x.RequestId,
                    ContractorId = x.ContractorId,
                    SuggestedFee = x.SuggestedFee,
                    CreatedAt = x.CreatedAt,
                    BidStatuses = x.BidStatuses.Select(b => new BidStatus
                    {
                        Id = b.Id,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        CreatedBy = b.CreatedBy
                    })
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList()

                }).ToList();
                if (bidsOfContractorDto.Any())
                {
                    return new Result<List<BidOfContractorDto>>().WithValue(bidsOfContractorDto).Success("پیشنهاد ها یافت شدند .");
                }
                else
                {
                    return new Result<List<BidOfContractorDto>>().WithValue(null).Success("پیشنهادی وجود ندارد");
                }
            }
            catch (Exception ex)
            {
                return new Result<List<BidOfContractorDto>>().WithValue(null).Failure(ErrorMessages.ErrorWhileRetrievingBidsOfContracotrs);
            }
        }
        public async Task<Result<UpdateBidOfContractorDto>> UpdateAsync(UpdateBidOfContractorDto bidOfContractorDto, CancellationToken cancellationToken)
        {
            try
            {


                int userId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                if (!isconverted)
                {
                    return new Result<UpdateBidOfContractorDto>().WithValue(null).Failure("خطا هنگام تغییر پیشنهاد");
                }
                BidOfContractor? bidOfContractor = await _context.BidOfContractors
                  .Where(x => x.Id == bidOfContractorDto.BidId)
                  .FirstOrDefaultAsync(cancellationToken);
                if (bidOfContractor is null)
                {
                    return new Result<UpdateBidOfContractorDto>().WithValue(null).Failure(ErrorMessages.BidOfContractorNotFound);
                }
                bidOfContractor.UpdatedAt = DateTime.Now;
                bidOfContractor.UpdatedBy = userId;
                if (bidOfContractorDto.CanChangeBid.HasValue)
                {
                    bidOfContractor.CanChangeBid = bidOfContractorDto.CanChangeBid.Value;
                }
                if (bidOfContractorDto.IsDeleted.HasValue)
                {
                    bidOfContractor.IsDeleted = bidOfContractorDto.IsDeleted.Value;
                }
                if (bidOfContractorDto.SuggestedFee.HasValue)
                {
                    bidOfContractor.SuggestedFee = bidOfContractorDto.SuggestedFee.Value;
                }
                if (bidOfContractorDto.ExpireAt.HasValue)
                {
                    bidOfContractor.ExpireAt = bidOfContractorDto.ExpireAt.Value;
                }
                //_context.BidOfContractors.Update(bidOfContractor);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<UpdateBidOfContractorDto>().WithValue(bidOfContractorDto).Success(" آپدیت انجام شد.");
            }
            catch (Exception)
            {
                return new Result<UpdateBidOfContractorDto>().WithValue(null).Failure("خطا هنگام تغییر پیشنهاد");
            }
        }
        public async Task<Result<List<BidOfContractorDto>>> GetBidsOfContractorAsync(CancellationToken cancellationToken)
        {
            try
            {

                int appUserId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out appUserId);
                if (!isconverted)
                {
                    return new Result<List<BidOfContractorDto>>().WithValue(null).Failure("خطایی .");
                }
                var bids = await _context
                .BidOfContractors
                .Where(x => x.CreatedBy == appUserId && x.IsDeleted == false)
                .Select(x => new BidOfContractorDto
                {
                    ContractorId = x.ContractorId,
                    Id = x.Id,
                    SuggestedFee = x.SuggestedFee,
                    IsDeleted = x.IsDeleted,
                    RequestId = x.RequestId,
                    CreatedAt = x.CreatedAt,
                    BidStatuses = x.BidStatuses.Select(b => new BidStatus
                    {
                        Id = b.Id,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        CreatedBy = b.CreatedBy
                    }).ToList()
                })
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
                if (bids.Any())
                {
                    return new Result<List<BidOfContractorDto>>().WithValue(bids).Success("پیشنهاد ها یافت شدند .");
                }
                else
                {
                    return new Result<List<BidOfContractorDto>>().WithValue(null).Success("پیشنهادی وجود ندارد");
                }
            }
            catch (Exception)
            {
                return new Result<List<BidOfContractorDto>>().WithValue(null).Failure("خطایی در بازیابی پیشنهادات رخ داده است.");
            }

        }
        public async Task<Result<List<BidOfContractorDto>>> GetBidsOfRequestAsync(int requestId, CancellationToken cancellationToken)
        {
            try
            {
                
                List<BidOfContractorDto> bidsOfContractor = await _context.BidOfContractors
                        .Where(x => (x.RequestId == requestId

                        && x.Request.IsActive == true
                        && x.IsDeleted == false)
                        && (x.BidStatuses
                        .Any(b => b.Status != BidStatusEnum.BidRejectedByContractor
                        && b.Status != BidStatusEnum.TimeForCheckingBidForClientExpired 
                        && b.Status == BidStatusEnum.ReviewBidByClientPhase
                        && b.ContractorBidId == x.Id)
                        ))
                        .Include(x => x.Request)
                        .Select(bid => new BidOfContractorDto
                        {
                            Id = bid.Id,
                            RequestId = bid.RequestId,
                            ContractorId = bid.ContractorId,
                            SuggestedFee = bid.SuggestedFee,
                            CreatedBy = bid.CreatedBy,
                            BidStatuses = bid.BidStatuses,
                        })
                        .OrderBy(x => x.SuggestedFee)
                        .ToListAsync(cancellationToken);
                if (bidsOfContractor.Any())
                {
                    return new Result<List<BidOfContractorDto>>()
                        .WithValue(bidsOfContractor)
                        .Success(SuccessMessages.BidsOfRequestFound);
                }
                else
                {
                    return new Result<List<BidOfContractorDto>>()
                        .WithValue(null)
                        .Success(ErrorMessages.BidsOfRequestNotFound);
                }
            }
            catch (Exception)
            {
                return new Result<List<BidOfContractorDto>>()
                    .WithValue(null)
                    .Failure(ErrorMessages.ErrorWhileRetrievingBidsOfContracotrs);
            }

        }

        public async Task<Result<List<BidOfContractorDto>>> GetBidsAcceptedByClientAsync(CancellationToken cancellationToken)
        {
            var user = await UserManagement.GetRoleBaseUserId(_httpContextAccessor.HttpContext, _context);
            if (!user.IsSuccessful)
            {
                return new Result<List<BidOfContractorDto>>().WithValue(null).Failure("خطا");
            }
            var contractorId = user.Data.UserId;

            var acceptedBids = await _context.BidOfContractors
                .Where(b => b.ContractorId == contractorId &&
                b.BidStatuses.Any(x => x.Status == BidStatusEnum.BidApprovedByClient))
                .Include(x => x.BidStatuses)
                .Select(x => new BidOfContractorDto
                {

                    Id = x.Id,
                    SuggestedFee = x.SuggestedFee,
                    RequestId = x.RequestId,

                }).ToListAsync(cancellationToken);
            if (acceptedBids.Count != 0)
            {
                return new Result<List<BidOfContractorDto>>()
                    .WithValue(acceptedBids)
                    .Success(SuccessMessages.AcceptedBidsFound);
            }
            else
            {
                return new Result<List<BidOfContractorDto>>()
                    .WithValue(null)
                    .Failure(ErrorMessages.BidOfContractorNotFound);
            }
        }



        public async Task<Result<BidOfContractorDto>> CheckBidIsAcceptedByClientAsync(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                int userId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                if (!isconverted)
                {
                    return new Result<BidOfContractorDto>().WithValue(null).Failure("خطا هنگام تغییر پیشنهاد");
                }

                BidOfContractor? bidOfContractor = await _context.BidOfContractors
                    .Where(x => x.Id == bidId && x.CreatedBy == userId)
                    .Include(x => x.BidStatuses)
                    .FirstOrDefaultAsync(cancellationToken);
                if (bidOfContractor == null)
                {
                    return new Result<BidOfContractorDto>().WithValue(null).Failure(ErrorMessages.BidOfContractorNotFound);
                }
                else
                {
                    if (bidOfContractor.IsDeleted == true)
                    {
                        return new Result<BidOfContractorDto>().WithValue(null).Failure(ErrorMessages.BidIsDeleted);
                    }
                    if (!bidOfContractor.BidStatuses.Any(x => x.Status == BidStatusEnum.BidApprovedByClient))
                    {
                        return new Result<BidOfContractorDto>().WithValue(null).Failure("پیشنهاد توسط متقاضی تایید نشده است.");

                    }
                    var bidOfContractorDto = new BidOfContractorDto
                    {
                        Id = bidOfContractor.Id,
                        RequestId = bidOfContractor.RequestId,
                        ContractorId = bidOfContractor.ContractorId,
                        SuggestedFee = bidOfContractor.SuggestedFee,
                        CreatedAt = bidOfContractor.CreatedAt,
                        BidStatuses = bidOfContractor.BidStatuses.Select(b => new BidStatus
                        {
                            Id = b.Id,
                            Status = b.Status,
                            UpdatedAt = b.UpdatedAt,
                            UpdatedBy = b.CreatedBy
                        })
                     .OrderByDescending(b => b.CreatedAt)
                     .ToList()
                    };
                    return new Result<BidOfContractorDto>().WithValue(bidOfContractorDto).Success(SuccessMessages.BidsOfRequestFound);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}