using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IRequestService
    {
        Task<bool> AddAsync(AddRequestDto requestDto, CancellationToken cancellationToken);
        Task<Result<List<RequestDto>>?> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<RequestDto>> GetByIdAsync(int reqId, CancellationToken cancellationToken);
        Task<Result<GetRequestDto>> GetRequestOfClientAsync(CancellationToken cancellationToken);
        Task<Result<UpdateRequestDto>> UpdateAsync(UpdateRequestDto requestDto, CancellationToken cancellationToken);
        Task<Result<List<RequestForShowingDetailsToContractorDto>>> GetRequestsforContractor(CancellationToken cancellationToken);
        Task<Result<RequestDto>> CheckRequestOfClientAsync(CancellationToken cancellationToken);
    }
}
