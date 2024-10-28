using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IRegionService
    {
        Task<int> AddAsync(Region region, CancellationToken cancellationToken);
        Task<Result<RegionDto>> GetByIdAsync(int regionId, CancellationToken cancellationToken);
    }
}
