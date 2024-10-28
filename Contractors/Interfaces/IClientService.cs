using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Results;

namespace Contractors.Interfaces
{
    public interface IClientService
    {
        Task<int> AddAsync(Client client, CancellationToken cancellationToken);
        Task<Result<ClientDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    }
}
