using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IRejectService
    {
        Task<Result<AddReasonToRejectRequestDto>> AddRjectRquestAsync(AddReasonToRejectRequestDto rejectRequestDto, CancellationToken cancellationToken);
        Task<Result<List<GetReasonOfRejectRequestDto>>> GetReasonsOfRejectingRequestByRequestIdAsync(int requestId, CancellationToken cancellationToken);
        Task<Result<List<GetReasonOfRejectRequestDto>>> GetReasonsOfRejectingRequestByClientAsycn(CancellationToken cancellationToken);
    }
}
