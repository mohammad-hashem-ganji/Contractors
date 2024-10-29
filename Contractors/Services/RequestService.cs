
using Azure.Core;
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using ContractorsAuctioneer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Contractors.Services
{
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractorService _contractorService;
        private readonly IClientService _clientService;
        private readonly IRegionService _regionService;
        private readonly IAuthService _authService;
        private readonly IFileAttachmentService _fileAttachmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RequestService(ApplicationDbContext context,
            IClientService clientService,
            IRegionService regionService,
            IAuthService authService,
            IFileAttachmentService fileAttachmentService,
            IContractorService contractorService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _clientService = clientService;
            _regionService = regionService;
            _authService = authService;
            _fileAttachmentService = fileAttachmentService;
            _contractorService = contractorService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> AddAsync(AddRequestDto requestDto, CancellationToken cancellationToken)
        {
            string role = "Client";

            if (requestDto == null)
            {
                // log
                return false;
            }
            try
            {

                var applicationUserResult = await _authService.RegisterAsync(requestDto.NCode, requestDto.PhoneNumber, role);
                if (applicationUserResult.Data.RegisteredUserId == 0)
                {
                    return false;
                }
                var clientId = await _clientService.AddAsync(new Client
                {
                    NCcode = requestDto.NCode,
                    MobileNubmer = requestDto.PhoneNumber,
                    MainSection = requestDto.Client.MainSection,
                    SubSection = requestDto.Client.SubSection,
                    address = requestDto.Client.address,
                    LicensePlate = requestDto.Client.LicensePlate,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    //CreatedBy = user.UserId,
                    DeletedBy = null,
                    DeletedAt = null,
                    ApplicationUserId = applicationUserResult.Data.RegisteredUserId,
                }, cancellationToken);
                if (clientId == 0)
                {
                    return false;
                }
                var regionId = await _regionService.AddAsync(new Region
                {
                    Title = requestDto.Region.Title,
                    ContractorSystemCode = requestDto.Region.ContractorSystemCode,
                    CreatedAt = DateTime.Now,
                    //CreatedBy = user.UserId,
                    IsDeleted = false,
                }, cancellationToken);
                var request = new Entites.Request
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    RegistrationDate = requestDto.RegistrationDate,
                    ConfirmationDate = requestDto.ConfirmationDate,
                    IsActive = true,
                    ExpireAt = DateTime.Now.AddDays(3),
                    RegionId = regionId,
                    ClientId = clientId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = applicationUserResult.Data.RegisteredUserId,
                    RequestNumber = requestDto.RequestNumber,
                    IsTenderOver = false,
                    IsDeleted = false
                };


                await _context.Requests.AddAsync(request, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                //log
                return false;
            }

        }
        public async Task<Result<List<RequestDto>>?> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                var requests = await _context.Requests
                   .Include(x => x.Client)
                   .Include(x => x.Region)
                   .Include(x => x.RequestStatuses)
                   .Include(x => x.FileAttachments)
                   .Include(x => x.BidOfContractors)
                   .ToListAsync(cancellationToken);
                var requestDtos = requests.Select(x => new RequestDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    RegistrationDate = x.RegistrationDate,
                    ConfirmationDate = x.ConfirmationDate,
                    ClientId = x.ClientId,
                    RegionId = x.RegionId,

                    FileAttachments = x.FileAttachments
                    .Where(f => f.IsDeleted == false)
                    .Select(f => new FileAttachmentDto
                    {
                        Id = f.Id
                    }).ToList()
                }).ToList();
                if (requestDtos.Any())
                {
                    return new Result<List<RequestDto>>().WithValue(requestDtos).Success("درخواست ها یافت شدند .");
                }
                else
                {

                    return new Result<List<RequestDto>>().WithValue(requestDtos).Failure("درخواستی وجود ندارد");
                }
            }
            catch (Exception ex)
            {
                return new Result<List<RequestDto>>().WithValue(null).Failure(ex.Message);
            }
        }
        public async Task<Result<RequestDto>> GetByIdAsync(int requestId, CancellationToken cancellationToken)
        {
            try
            {
                var request = await _context.Requests
                   .Where(x =>
                   x.Id == requestId && x.IsTenderOver == false && x.IsActive == true)
                   .Include(x => x.Client)
                   .Include(x => x.Region)
                   .Include(x => x.RequestStatuses)
                   .Include(x => x.FileAttachments)
                   .Include(x => x.BidOfContractors)
                   .FirstOrDefaultAsync(cancellationToken);
                if (request == null)
                {
                    return new Result<RequestDto>().WithValue(null).Failure(ErrorMessages.RequestNotFound);
                }

                var requestDto = new RequestDto
                {
                    Id = request.Id,
                    Title = request.Title,
                    Description = request.Description,
                    RegistrationDate = request.RegistrationDate,
                    ConfirmationDate = request.ConfirmationDate,
                    ClientId = request.ClientId,
                    RegionId = request.RegionId,

                    RequestStatuses = request.RequestStatuses.Select(rs => new RequestStatusDto
                    {
                        Id = rs.Id,
                        Status = rs.Status,

                    }).ToList(),
                    FileAttachments = request.FileAttachments.Select(f => new FileAttachmentDto
                    {
                        Id = f.Id,

                    }).ToList()
                };

                return new Result<RequestDto>().WithValue(requestDto).Success(SuccessMessages.Regionfound);
            }
            catch (Exception)
            {
                return new Result<RequestDto>().WithValue(null).Failure("خطا!");
            }
        }
        public async Task<Result<RequestDto>> CheckRequestOfClientAsync(CancellationToken cancellationToken)
        {
            try
            {

                var user = await UserManagement.GetRoleBaseUserId(_httpContextAccessor.HttpContext, _context);

                if (!user.IsSuccessful)
                {
                    return new Result<RequestDto>().WithValue(null).Failure("خطا");
                }
                var clientId = user.Data.UserId;

                var appId = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int appUserId);
                if (!appId)
                {
                    return new Result<RequestDto>().WithValue(null).Failure("خطا");
                }
                var requestResult = await _context.Requests
                   .Where(x =>
                   x.ClientId == clientId
                   && x.IsTenderOver == false && x.IsActive == true
                   )
                   .Include(x => x.Client)
                   .Include(x => x.Region)
                   .Include(x => x.RequestStatuses)
                   .Include(x => x.FileAttachments)
                   .Include(x => x.BidOfContractors)
                   .Select(x => new RequestDto
                   {
                       Id = x.Id,
                       Title = x.Title,
                       Description = x.Description,
                       RegistrationDate = x.RegistrationDate,
                       ConfirmationDate = x.ConfirmationDate,
                       IsActive = x.IsActive,
                       ExpireAt = x.ExpireAt,
                       IsAcceptedByClient = x.IsAcceptedByClient,
                       IsTenderOver = x.IsTenderOver,
                       ClientId = x.ClientId,
                       RegionId = x.RegionId,
                       RequestNumber = x.RequestNumber,

                       RequestStatuses = x.RequestStatuses.Select(rs => new RequestStatusDto
                       {
                           Id = rs.Id,
                           Status = rs.Status,

                       }).ToList(),
                       BidOfContractors = x.BidOfContractors.Select(b => new BidOfContractorDto
                       {
                           Id = b.Id,
                           SuggestedFee = b.SuggestedFee,
                           ContractorId = b.ContractorId,
                           CreatedAt = b.CreatedAt,
                       }).ToList(),
                       FileAttachments = x.FileAttachments.Select(f => new FileAttachmentDto
                       {
                           Id = f.Id,

                       }).ToList()

                   }).FirstOrDefaultAsync(cancellationToken);
                var requestStatusResult = await _context.RequestStatuses.Where(rs => rs.CreatedBy == appUserId).ToListAsync(cancellationToken);
                if (requestResult is not null)
                {
                    if (requestResult.ExpireAt > DateTime.Now)
                    {
                        if (requestResult.RequestStatuses.Any(rs => rs.Status == RequestStatusEnum.RequestApprovedByClient
                                                                 || rs.Status == RequestStatusEnum.RequestRejectedByClient))
                        {
                            return new Result<RequestDto>().WithValue(requestResult).Success("درخواست  قبلا بررسی شده است.");
                        }
                        else
                        {
                            return new Result<RequestDto>().WithValue(requestResult).Success("لطفا  پروژه درخواستی خود را بررسی کنید.");
                        }
                    }
                    else
                    {
                        return new Result<RequestDto>().WithValue(null).Success("مهلت تایید درخواست تمام شده است!");
                    }
                }
                return new Result<RequestDto>().WithValue(null).Success("درخواست  یافت نشد.");
            }
            catch (Exception)
            {
                return new Result<RequestDto>().WithValue(null).Failure("خطا");
            }

        }

        public async Task<Result<RequestDto>> GetRequestOfClientAsync(CancellationToken cancellationToken)
        {
            try
            {

                var user = await UserManagement.GetRoleBaseUserId(_httpContextAccessor.HttpContext, _context);

                if (!user.IsSuccessful)
                {
                    return new Result<RequestDto>().WithValue(null).Failure("خطا");
                }
                var clientId = user.Data.UserId;

                var isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int appUserId);
                if (!isconverted)
                {
                    return new Result<RequestDto>().WithValue(null).Failure("خطا");
                }
                var requestResult = await _context.Requests
                   .Where(x =>
                   x.ClientId == clientId
                   && x.IsTenderOver == false && x.IsActive == true
                   )
                   .Include(x => x.Client)
                   .Include(x => x.Region)
                   .Include(x => x.RequestStatuses)
                   .Include(x => x.FileAttachments)
                   .Include(x => x.BidOfContractors)
                   .Select(x => new RequestDto
                   {
                       Id = x.Id,
                       Title = x.Title,
                       Description = x.Description,
                       RegistrationDate = x.RegistrationDate,
                       ConfirmationDate = x.ConfirmationDate,
                       IsActive = x.IsActive,
                       ExpireAt = x.ExpireAt,
                       IsAcceptedByClient = x.IsAcceptedByClient,
                       IsTenderOver = x.IsTenderOver,
                       ClientId = x.ClientId,
                       RegionId = x.RegionId,
                       RequestNumber = x.RequestNumber,

                       RequestStatuses = x.RequestStatuses.Select(rs => new RequestStatusDto
                       {
                           Id = rs.Id,
                           Status = rs.Status,

                       }).ToList(),
                       BidOfContractors = x.BidOfContractors.Select(b => new BidOfContractorDto
                       {
                           Id = b.Id,
                           SuggestedFee = b.SuggestedFee,
                           ContractorId = b.ContractorId,
                           CreatedAt = b.CreatedAt,
                       }).ToList(),
                       FileAttachments = x.FileAttachments.Select(f => new FileAttachmentDto
                       {
                           Id = f.Id,

                       }).ToList()

                   }).FirstOrDefaultAsync(cancellationToken);
                var requestStatusResult = await _context.RequestStatuses.Where(rs => rs.CreatedBy == appUserId).ToListAsync(cancellationToken);
                if (requestResult is not null)
                {
                    if (requestResult.RequestStatuses.Any(rs => rs.Status == RequestStatusEnum.RequestRejectedByClient))
                    {
                        return new Result<RequestDto>().WithValue(null).Success("درخواست قبلا رد شده است.");
                    }
                    else
                    {
                        return new Result<RequestDto>().WithValue(requestResult).Success("درخواست پیدا شد.");
                    }
                }
                return new Result<RequestDto>().WithValue(requestResult).Success("درخواست  پیدا نشد.");
            }
            catch (Exception)
            {
                return new Result<RequestDto>().WithValue(null).Failure("خطا");
            }

        }



        public async Task<Result<List<RequestDto>>> GetRequestsforContractor(CancellationToken cancellationToken)
        {
            try
            {
                var appId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(appId, out var contractorId))
                {
                    return new Result<List<RequestDto>>().WithValue(null).Failure("خطا");
                }


                var requests = await _context.Requests
                      .Where(r =>
                      r.RequestStatuses.Any(rs => rs.Status == RequestStatusEnum.RequestApprovedByClient &&
                                                  rs.Status != RequestStatusEnum.RequestRejectedByContractor))
                      .Select(r => new RequestDto
                      {
                          Id = r.Id,
                          RequestNumber = r.RequestNumber,
                          Title = r.Title,
                          Description = r.Description,
                          RegistrationDate = r.RegistrationDate,
                          ConfirmationDate = r.ConfirmationDate,
                          ExpireAt = r.ExpireAt,
                          ClientId = r.ClientId,
                          RegionId = r.RegionId,
                          IsTenderOver = r.IsTenderOver,
                          IsActive = r.IsActive,
                          IsAcceptedByClient = r.IsAcceptedByClient,
                          CreatedAt = r.CreatedAt,
                          UpdatedAt = r.UpdatedAt,
                          RequestStatuses = r.RequestStatuses.Select(rs => new RequestStatusDto
                          {
                              Status = rs.Status,
                              CreatedAt = rs.CreatedAt,
                              CreatedBy = rs.CreatedBy,
                              Id = rs.Id,
                              RequestId = rs.RequestId
                          }).ToList(),
                          Region = r.Region.Title
                          
                      }).ToListAsync(cancellationToken);

                if (requests.Any())
                {
                    return new Result<List<RequestDto>>()
                        .WithValue(requests)
                        .Success("درخواست ها یافت شدند .");
                }
                else
                {

                    return new Result<List<RequestDto>>()
                        .WithValue(null)
                        .Success("درخواستی وجود ندارد");
                }
            }
            catch (Exception ex)
            {
                return new Result<List<RequestDto>>()
                    .WithValue(null)
                    .Failure("هنگام اجرا خطایی پیش آمد!");
            }
        }


        public async Task<Result<RequestDto>> UpdateAsync(RequestDto requestDto, CancellationToken cancellationToken)
        {
            try
            {
                Entites.Request? request = await _context.Requests
                .Where(x =>
                x.Id == requestDto.Id && x.IsActive == true && x.IsTenderOver == false)
                .FirstOrDefaultAsync(cancellationToken);
                if (request is null)
                {
                    return new Result<RequestDto>().WithValue(null).Failure(ErrorMessages.RequestNotFound);
                }
                request.Title = requestDto.Title;
                request.RequestNumber = requestDto.RequestNumber;
                request.RegistrationDate = requestDto.RegistrationDate;
                request.ConfirmationDate = request.ConfirmationDate;
                request.Description = requestDto.Description;
                request.IsAcceptedByClient = requestDto.IsAcceptedByClient;
                request.ExpireAt = requestDto.ExpireAt;
                request.IsTenderOver = requestDto.IsTenderOver;
                request.IsActive = requestDto.IsActive;
                request.UpdatedAt = DateTime.Now;
                //request.UpdatedBy = requestDto.UpdatedBy;
                _context.Requests.Update(request);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<RequestDto>().WithValue(requestDto).Success("درخواست آپدیت شد");
            }
            catch (Exception ex)
            {
                return new Result<RequestDto>().WithValue(null).Failure(ex.Message);
            }






        }





    }
}
