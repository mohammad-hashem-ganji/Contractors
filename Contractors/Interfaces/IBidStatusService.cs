using Contractors.Dtos;
using Contractors.Results;
using Contractors.Dtos;

namespace ContractorsAuctioneer.Interfaces
{
    public interface IBidStatusService
    {
        Task<Result<AddBidStatusDto>> AddAsync(AddBidStatusDto bidDto, CancellationToken cancellationToken);
        Task<Result<List<BidStatusDto>>> GetRequestStatusesByBidId(int bidId, CancellationToken cancellationToken);
    }
}
