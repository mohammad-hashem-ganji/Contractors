using Contractors.Dtos;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IProjectService
    {
        Task<Result<AddProjectDto>> AddAsync(AddProjectDto addProjectDto, CancellationToken cancellationToken);
        Task<Result<GetProjectDto>> GetByIdAsync(int projectId, CancellationToken cancellationToken);
        Task<Result<GetProjectDto>> GetProjectOfbidAsync(int bidId, CancellationToken cancellationToken);
        Task<Result<GetProjectDto>> UpdateAsync(GetProjectDto projectDto, CancellationToken cancellationToken);
    }
}
