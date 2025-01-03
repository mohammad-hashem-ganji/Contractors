﻿
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
        private readonly IRequestService _requestService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BidOfContractorService(ApplicationDbContext context,
            IContractorService contractorService,
            IHttpContextAccessor httpContextAccessor,
            IRequestService requestService)
        {
            _context = context;
            _contractorService = contractorService;
            _httpContextAccessor = httpContextAccessor;
            _requestService = requestService;
        }
        public async Task<Result<AddBidOfContractorDto>> AddAsync(AddBidOfContractorDto bidOfContractorDto, CancellationToken cancellationToken)
        {

            if (bidOfContractorDto == null)
            {
                return new Result<AddBidOfContractorDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {
                if (bidOfContractorDto.SuggestedFee < 0)
                {
                    return new Result<AddBidOfContractorDto>().WithValue(null).Failure("عدد قرار داد باید بزرگ تر از 0 باشد.");
                }
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
                                      && x.ExpireAt > DateTime.Now
                                      && x.CanChangeBid == true
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

                        SuggestedFee = bidOfContractor.SuggestedFee,
                        CreatedAt = bidOfContractor.CreatedAt,
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
                    contractor = new ContractorDetailsDto
                    {
                        ContractorName = x.Contractor.Name,
                        ContractorAddress = x.Contractor.Address,
                        ContractorPhoneNumber = x.Contractor.MobileNumber
                    },
                    SuggestedFee = x.SuggestedFee,
                    CreatedAt = x.CreatedAt,

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
                if (bidOfContractorDto.SuggestedFee < 0)
                {
                    return new Result<UpdateBidOfContractorDto>().WithValue(null).Failure("عدد قرار داد باید بزرگ تر از 0 باشد.");
                }

                int userId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                if (!isconverted)
                {
                    return new Result<UpdateBidOfContractorDto>().WithValue(null).Failure("خطا هنگام تغییر پیشنهاد");
                }
                BidOfContractor? bidOfContractor = await _context.BidOfContractors
                  .Where(x => x.Id == bidOfContractorDto.BidId
                  && x.ExpireAt > DateTime.Now
                  && x.CanChangeBid == true)
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
                    contractor = new ContractorDetailsDto
                    {
                        ContractorName = x.Contractor.Name,
                        ContractorAddress = x.Contractor.Address,
                        ContractorPhoneNumber = x.Contractor.MobileNumber
                    },
                    Id = x.Id,
                    SuggestedFee = x.SuggestedFee,
                    RequestId = x.RequestId,
                    CreatedAt = x.CreatedAt,
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
        public async Task<Result<List<BidOfContractorDto>>> GetBidsOfRequestAsync(CancellationToken cancellationToken)
        {
            try
            {
                int userId;
                bool isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
                if (!isconverted)
                {
                    return new Result<List<BidOfContractorDto>>().WithValue(null).Failure("خطا هنگام تغییر پیشنهاد");
                }
                var request = await _requestService.GetRequestOfClientAsync(cancellationToken);
                if (request.Data == null)
                {
                    return new Result<List<BidOfContractorDto>>()
                        .WithValue(null)
                        .Failure(ErrorMessages.ErrorWhileRetrievingBidsOfContracotrs);
                }
                List<BidOfContractorDto> bidsOfContractor = await _context.BidOfContractors
                        .Where(x => x.RequestId == request.Data.Id
                           && x.Request.IsActive == true
                           && x.ExpireAt > DateTime.Now
                           && x.ExpireAt != null
                           && x.IsDeleted == false
                           && x.BidStatuses.Any(b =>
                             b.Status != BidStatusEnum.BidRejectedByContractor &&
                             b.Status != BidStatusEnum.TimeForCheckingBidForClientExpired &&
                             b.Status == BidStatusEnum.ReviewBidByClientPhase &&
                             b.ContractorBidId == x.Id))
                        .Include(x => x.Request)
                        .Select(bid => new BidOfContractorDto
                        {
                            Id = bid.Id,
                            RequestId = bid.RequestId,
                            contractor = new ContractorDetailsDto
                            {
                                ContractorName = bid.Contractor.Name,
                                ContractorAddress = bid.Contractor.Address,
                                ContractorPhoneNumber = bid.Contractor.MobileNumber
                            },

                            SuggestedFee = bid.SuggestedFee,

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
                .Where(b => b.ContractorId == contractorId
                && (b.ExpireAt > DateTime.Now && b.ExpireAt != null)
                && b.BidStatuses.Any(x => x.Status == BidStatusEnum.BidApprovedByClient))
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
                    .Include(x => x.Contractor)
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
                        contractor = new ContractorDetailsDto
                        {
                            ContractorId = bidOfContractor.ContractorId,
                            ContractorName = bidOfContractor.Contractor.Name,
                            ContractorAddress = bidOfContractor.Contractor.Address,
                            ContractorPhoneNumber = bidOfContractor.Contractor.MobileNumber
                        },
                        SuggestedFee = bidOfContractor.SuggestedFee,
                        CreatedAt = bidOfContractor.CreatedAt,
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