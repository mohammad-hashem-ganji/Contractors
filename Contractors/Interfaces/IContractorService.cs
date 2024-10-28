using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IContractorService
    {
        Task<Result<AddContractorDto>> AddAsync(AddContractorDto contractorDto, CancellationToken cancellationToken);
        Task<Result<ContractorDto>> GetByIdAsync(int contractorId, CancellationToken cancellationToken);
    }
}
