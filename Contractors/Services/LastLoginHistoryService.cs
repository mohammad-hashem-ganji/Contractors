
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.EntityFrameworkCore;

namespace Contractors.Services
{
    public class LastLoginHistoryService : ILastLoginHistoryService
    {
        private readonly ApplicationDbContext _context;
        public LastLoginHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Result<AddLastLoginHistoryDto>> AddAsync(AddLastLoginHistoryDto lastLoginHistoryDto, CancellationToken cancellationToken)
        {
            if (lastLoginHistoryDto is null)
            {
                return new Result<AddLastLoginHistoryDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {
                var lastLogin = new LastLoginHistory
                {
                    ApplicationUserId = lastLoginHistoryDto.ApplicationUserId,
                    LastLoginTime = lastLoginHistoryDto.LastLoginTime,
                    CreatedAt = lastLoginHistoryDto.CreatedAt,
                    CreatedBy = lastLoginHistoryDto.CreatedBy
                };
                await _context.LastLoginHistories.AddAsync(lastLogin, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<AddLastLoginHistoryDto>().WithValue(lastLoginHistoryDto).Success(SuccessMessages.LoginHistoryAdded);
            }
            catch (Exception ex)
            {
                return new Result<AddLastLoginHistoryDto>().WithValue(null).Failure(ex.Message);
            }
        }
        public async Task<Result<UpdateLastLoginHistoryDto>> UpdateAsync(UpdateLastLoginHistoryDto lastLoginHistoryDto, CancellationToken cancellationToken)
        {
            if (lastLoginHistoryDto is null)
            {
                return new Result<UpdateLastLoginHistoryDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {
                var lastLogin = await _context.LastLoginHistories
                    .Where(x => x.ApplicationUserId == lastLoginHistoryDto.ApplicationUserId)
                    .OrderByDescending(x => x.LastLoginTime)
                    .FirstOrDefaultAsync(cancellationToken);
                if (lastLogin == null)
                {
                    return new Result<UpdateLastLoginHistoryDto>().WithValue(null).Failure(ErrorMessages.LoginHistoryNotFound);
                }
                lastLogin.UpdatedAt = lastLoginHistoryDto.UpdatedAt;
                lastLogin.UpdatedBy = lastLoginHistoryDto.UpdatedBy;

                _context.LastLoginHistories.Update(lastLogin);
                await _context.SaveChangesAsync(cancellationToken);

                return new Result<UpdateLastLoginHistoryDto>().WithValue(lastLoginHistoryDto).Success(SuccessMessages.LoginHistoryUpdated);
            }
            catch (Exception ex)
            {
                return new Result<UpdateLastLoginHistoryDto>().WithValue(null).Failure(ex.Message);
            }
        }

    }
}
