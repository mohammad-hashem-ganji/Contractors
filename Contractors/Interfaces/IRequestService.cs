using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IRequestService
    {
        Task<bool> AddAsync(AddRequestDto requestDto, CancellationToken cancellationToken);
        Task<Result<List<RequestDto>>?> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<RequestDto>> GetByIdAsync(int reqId, CancellationToken cancellationToken);
        Task<Result<RequestDto>> GetRequestOfClientAsync(CancellationToken cancellationToken);
        Task<Result<RequestDto>> UpdateAsync(RequestDto requestDto, CancellationToken cancellationToken);
        Task<Result<List<RequestDto>>> GetRequestsforContractor(CancellationToken cancellationToken);
        Task<Result<RequestDto>> CheckRequestOfClientAsync(CancellationToken cancellationToken);
    }
}
