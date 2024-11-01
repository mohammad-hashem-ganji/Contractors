using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IRegionService
    {
        Task<Result<AddRegionDto>> AddAsync(AddRegionDto regionDto, CancellationToken cancellationToken);
        Task<Result<RegionDto>> GetByIdAsync(int regionId, CancellationToken cancellationToken);
    }
}
