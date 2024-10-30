
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Contractors.Services
{
    public class ContractorService : IContractorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        
        public ContractorService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
            
        }
        public async Task<Result<AddContractorDto>> AddAsync(AddContractorDto contractorDto, CancellationToken cancellationToken)
        {
            try
            {
                const string role = "Contractor";
                var applicationUserResult = await _authService.RegisterAsync(contractorDto.NCode, contractorDto.PhoneNumber, role);
                if (applicationUserResult.Data.RegisteredUserId == 0)
                {
                    return new Result<AddContractorDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
                }
                var contractor = new Contractor
                {
                    Name = contractorDto.Name,
                    CompanyName = contractorDto.CompanyName,
                    Email = contractorDto.Email,
                    FaxNumber = contractorDto.FaxNumber,
                    Address = contractorDto.Address,
                    LandlineNumber = contractorDto.LandlineNumber,
                    MobileNumber = contractorDto.MobileNumber,
                    ApplicationUserId = applicationUserResult.Data.RegisteredUserId
                };
                await _context.Contractors.AddAsync(contractor, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<AddContractorDto>().WithValue(contractorDto).Success(SuccessMessages.ContractorAdded);
            }
            catch (Exception ex)
            {
                return new Result<AddContractorDto>().WithValue(null).Failure(ex.Message);
            }

        }
        public async Task<Result<ContractorDto>> GetByIdAsync(int contractorId, CancellationToken cancellationToken)
        {
            try
            {
                var contractor = await _context.Contractors
                  .Where(x => x.Id == contractorId)
                  .Include(x => x.BidOfContractors)
                  .Include(x => x.ApplicationUser)
                  .FirstOrDefaultAsync(cancellationToken);
                if (contractor is null)
                {
                    return new Result<ContractorDto>().WithValue(null).Failure(ErrorMessages.ContractorNotFound);
                }
                else
                {
                    var contractorDto = new ContractorDto
                    {
                        Id = contractor.Id,
                        Address = contractor.Address,
                        CompanyName = contractor.CompanyName,
                        Name = contractor.Name,
                        Email = contractor.Email,
                        FaxNumber = contractor.FaxNumber,
                        LandlineNumber = contractor.LandlineNumber,
                        MobileNumber = contractor.MobileNumber,
                        ApplicationUserId = contractor.ApplicationUserId,
                        BidOfContractors = contractor.BidOfContractors.Select(b => new BidOfContractorDto
                        {
                            Id = b.Id,
                            SuggestedFee = b.SuggestedFee,
                            RequestId = b.RequestId,
                            CreatedAt = b.CreatedAt,
                        }).ToList(),
                    };
                    return new Result<ContractorDto>().WithValue(contractorDto).Success(SuccessMessages.ClientFound);
                }
            }
            catch (Exception ex)
            {
                return new Result<ContractorDto>().WithValue(null).Failure(ex.Message);
            }
        }
        
    }
}
