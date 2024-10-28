
using Azure.Core;
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace Contractors.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
       

        public ClientService(ApplicationDbContext context, IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
           
        }
        public async Task<int> AddAsync(Client client, CancellationToken cancellationToken)
        {
            //client.ApplicationUser = _authService.RegisterAsync() 
            await _context.Clients.AddAsync(client, cancellationToken);
            var trackeNum = await _context.SaveChangesAsync(cancellationToken);
            if (trackeNum >= 1)
            {
                return client.Id;
            }
            else
            {
                return 0;
            }

        }
        public async Task<Result<ClientDto>> GetByIdAsync(int clientId, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _context.Clients
                  .Where(x => x.Id == clientId)
                  .Include(x => x.Requests)
                  .Include(x => x.ApplicationUser)
                  .FirstOrDefaultAsync(cancellationToken);
                if (client is null)
                {
                    return new Result<ClientDto>().WithValue(null).Failure(ErrorMessages.ClientNotFound);
                }
                else
                {
                    var clientDto = new ClientDto
                    {
                        Id = client.Id,
                        address = client.address,
                        ApplicationUserId = client.ApplicationUserId,
                        CreatedAt = client.CreatedAt,
                        CreatedBy = client.CreatedBy,
                        IsDeleted = client.IsDeleted,
                        DeletedAt = client.DeletedAt,
                        DeletedBy = client.DeletedBy,
                        LicensePlate = client.LicensePlate,
                        MainSection = client.MainSection,
                        MobileNubmer = client.MobileNubmer,
                        NCcode = client.NCcode,
                        PostalCode = client.PostalCode,
                        SubSection = client.SubSection,
                        Requests = client.Requests.Select(rs => new RequestDto
                        {
                            Id = rs.Id,
                            Title = rs.Title,
                            ConfirmationDate = rs.ConfirmationDate,
                            RegistrationDate = rs.RegistrationDate,
                            Description = rs.Description,
                        }).ToList(),
                        UpdatedAt = client.UpdatedAt,
                        UpdatedBy = client.UpdatedBy,
                    };
                    return new Result<ClientDto>().WithValue(clientDto).Success(SuccessMessages.ClientFound);
                }
            }
            catch (Exception ex)
            {
                return new Result<ClientDto>().WithValue(null).Failure(ex.Message);
            }
        }



    }
}
