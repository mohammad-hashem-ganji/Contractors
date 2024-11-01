
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
                var request = new Entites.Request
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    RegistrationDate = requestDto.RegistrationDate,
                    ConfirmationDate = requestDto.ConfirmationDate,
                    IsActive = true,
                    ExpireAt = DateTime.Now.AddDays(3),
                    RegionId = requestDto.RegionId,
                    ClientId = clientId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = applicationUserResult.Data.RegisteredUserId,
                    RequestNumber = requestDto.RequestNumber,
                    IsTenderOver = false,
                    IsDeleted = false,
                    IsAcceptedByClient = null,
                    IsFileCheckedByClient = false

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
                    RegionTitle = x.Region.Title



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
                    RegionTitle = request.Region.Title


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
                if (user.Data.Role != "Client")
                {
                    return new Result<RequestDto>().WithValue(null).Failure("نقش نامتعارف .");
                }

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
                       RegionTitle = x.Region.Title,
                       RequestNumber = x.RequestNumber,



                   }).FirstOrDefaultAsync(cancellationToken);
                var requestStatusResult = await _context.RequestStatuses.Where(rs => rs.CreatedBy == appUserId).ToListAsync(cancellationToken);
                if (requestResult is not null)
                {
                    if (requestResult.ExpireAt > DateTime.Now)
                    {
                        if (requestStatusResult.Any(rs => rs.Status == RequestStatusEnum.RequestApprovedByClient
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

        public async Task<Result<GetRequestDto>> GetRequestOfClientAsync(CancellationToken cancellationToken)
        {
            try
            {

                var user = await UserManagement.GetRoleBaseUserId(_httpContextAccessor.HttpContext, _context);

                if (!user.IsSuccessful)
                {
                    return new Result<GetRequestDto>().WithValue(null).Failure("خطا");
                }
                var clientId = user.Data.UserId;

                var isconverted = int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out int appUserId);
                if (!isconverted)
                {
                    return new Result<GetRequestDto>().WithValue(null).Failure("خطا");
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
                   .Select(x => new GetRequestDto
                   {
                       Id = x.Id,
                       Title = x.Title,
                       Description = x.Description,
                       RegistrationDate = x.RegistrationDate,
                       ConfirmationDate = x.ConfirmationDate,
                       ExpireAt = x.ExpireAt,
                       IsAcceptedByClient = x.IsAcceptedByClient,
                       ClientId = x.ClientId,
                       RequestNumber = x.RequestNumber,
                       RegionTitle = x.Region.Title,
                       LastStatus = x.RequestStatuses
                       .OrderByDescending(rs => rs.CreatedAt)
                       .Select(rs => rs.Status)
                       .FirstOrDefault()


                   }).FirstOrDefaultAsync(cancellationToken);
                var requestStatusResult = await _context.RequestStatuses.Where(rs => rs.CreatedBy == appUserId).ToListAsync(cancellationToken);
                if (requestResult is not null)
                {
                    if (requestStatusResult.Any(rs => rs.Status == RequestStatusEnum.RequestRejectedByClient))
                    {
                        return new Result<GetRequestDto>().WithValue(null).Success("درخواست قبلا رد شده است.");
                    }
                    else
                    {
                        return new Result<GetRequestDto>().WithValue(requestResult).Success("درخواست پیدا شد.");
                    }
                }
                return new Result<GetRequestDto>().WithValue(requestResult).Success("درخواست  پیدا نشد.");
            }
            catch (Exception)
            {
                return new Result<GetRequestDto>().WithValue(null).Failure("خطا");
            }

        }

        public async Task<Result<List<RequestForShowingDetailsToContractorDto>>> GetRequestsforContractor(CancellationToken cancellationToken)
        {
            try
            {
                var appId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(appId, out var contractorId))
                {
                    return new Result<List<RequestForShowingDetailsToContractorDto>>().WithValue(null).Failure("خطا");
                }


                var requests = await _context.Requests
                      .Where(r =>
                      r.RequestStatuses.Any(rs => rs.Status == RequestStatusEnum.RequestApprovedByClient &&
                                                  rs.Status != RequestStatusEnum.RequestRejectedByContractor))
                      .Select(r => new RequestForShowingDetailsToContractorDto
                      {
                          RequestId = r.Id,
                          RequestNumber = r.RequestNumber,
                          Title = r.Title,
                          Description = r.Description,
                          RegionTitle = r.Region.Title,
                          LastStatus = r.RequestStatuses
                             .OrderByDescending(rs => rs.CreatedAt)
                             .Select(rs => rs.Status)
                             .FirstOrDefault()
                      }).ToListAsync(cancellationToken);

                if (requests.Any())
                {
                    return new Result<List<RequestForShowingDetailsToContractorDto>>()
                        .WithValue(requests)
                        .Success("درخواست ها یافت شدند .");
                }
                else
                {

                    return new Result<List<RequestForShowingDetailsToContractorDto>>()
                        .WithValue(null)
                        .Success("درخواستی وجود ندارد");
                }
            }
            catch (Exception ex)
            {
                return new Result<List<RequestForShowingDetailsToContractorDto>>()
                    .WithValue(null)
                    .Failure("هنگام اجرا خطایی پیش آمد!");
            }
        }

        public async Task<Result<UpdateRequestDto>> UpdateAsync(UpdateRequestDto requestDto, CancellationToken cancellationToken)
        {
            try
            {
                Entites.Request? request = await _context.Requests
                .Where(x =>
                x.Id == requestDto.Id && x.IsActive == true && x.IsTenderOver == false)
                .FirstOrDefaultAsync(cancellationToken);
                if (request is null)
                {
                    return new Result<UpdateRequestDto>().WithValue(null).Failure(ErrorMessages.RequestNotFound);
                }
                if (requestDto.IsAcceptedByClient.HasValue)
                {

                    request.IsAcceptedByClient = requestDto.IsAcceptedByClient;
                }
                if (requestDto.ExpireAt.HasValue)
                {

                    request.ExpireAt = requestDto.ExpireAt;
                }
                if (requestDto.IsTenderOver.HasValue)
                {

                    request.IsTenderOver = requestDto.IsTenderOver;
                }
                if (requestDto.IsActive.HasValue)
                {

                    request.IsActive = requestDto.IsActive;
                }
                request.UpdatedAt = DateTime.Now;
                //request.UpdatedBy = requestDto.UpdatedBy;
                _context.Requests.Update(request);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<UpdateRequestDto>().WithValue(requestDto).Success("درخواست آپدیت شد");
            }
            catch (Exception ex)
            {
                return new Result<UpdateRequestDto>().WithValue(null).Failure(ex.Message);
            }






        }





    }
}
