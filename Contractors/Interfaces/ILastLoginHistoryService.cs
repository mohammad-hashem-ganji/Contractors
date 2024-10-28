using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface ILastLoginHistoryService
    {
        Task<Result<AddLastLoginHistoryDto>> AddAsync(AddLastLoginHistoryDto lastLoginHistoryDto, CancellationToken cancellationToken);
        Task<Result<UpdateLastLoginHistoryDto>> UpdateAsync(UpdateLastLoginHistoryDto lastLoginHistoryDto, CancellationToken cancellationToken);
    }
}
