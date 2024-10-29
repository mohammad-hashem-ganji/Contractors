using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace ContractorsAuctioneer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractorController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IRequestStatusService _requestStatusService;
        private readonly IBidStatusService _bidStatusService;
        private readonly IContractorService _contractorService;
        private readonly IBidOfContractorService _bidOfContractorService;
        private readonly IProjectService _projectService;
        public ContractorController(IRequestService requestService, IRequestStatusService requestStatusService, IContractorService contractorService, IBidOfContractorService bidOfContractorService, IBidStatusService bidStatusService, IProjectService projectService)
        {
            _requestService = requestService;
            _requestStatusService = requestStatusService;
            _contractorService = contractorService;
            _bidOfContractorService = bidOfContractorService;
            _bidStatusService = bidStatusService;
            _projectService = projectService;
        }
        [Authorize(Roles = "Contractor")]
        [HttpPut]
        [Route(nameof(RejectRequestByContractor))]
        public async Task<IActionResult> RejectRequestByContractor(UpdateRequestAcceptanceDto rejectedRequestDto, CancellationToken cancellationToken)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var request = await _requestService.GetByIdAsync(rejectedRequestDto.RequestId, cancellationToken);
            if (!request.IsSuccessful)
            {
                return NotFound(request);
            }
            if (rejectedRequestDto.IsAccepted == false)
            {
                var newStatus = new AddRequestStatusDto
                {
                    RequestId = rejectedRequestDto.RequestId,
                    Status = RequestStatusEnum.RequestRejectedByContractor
                };
                var newRequestStatus = await _requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful) return BadRequest(newRequestStatus);
                return Ok();
            }
            else return BadRequest("مقادیر ورودی نا معتبر است");
        }

        [Authorize(Roles = "Contractor")]
        [HttpGet]
        [Route(nameof(ShowRequestsToContractor))]
        public async Task<IActionResult> ShowRequestsToContractor( CancellationToken cancellationToken)
        {
            var requests = await _requestService.GetRequestsforContractor( cancellationToken);
            if (!requests.IsSuccessful)
            {
                return Problem(requests.ErrorMessage);
            }
            return Ok(requests);
        }
        [Authorize(Roles = "Contractor")]
        [HttpGet]
        [Route(nameof(ShowBidsAcceptedByClient))]
        public async Task<IActionResult> ShowBidsAcceptedByClient(CancellationToken cancellationToken)
        {
            var acceptedBids = await _bidOfContractorService.GetBidsAcceptedByClientAsync(cancellationToken);
            if (!acceptedBids.IsSuccessful)
            {
                return NotFound(acceptedBids);
            }
            return Ok(acceptedBids);
        }

 
        [Authorize(Roles = "Contractor")]
        [HttpPut]
        [Route(nameof(AcceptBidByContractor))]
        public async Task<IActionResult> AcceptBidByContractor(UpdateBidAcceptanceDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var bid = await _bidOfContractorService.GetByIdAsync(bidDto.BidId, cancellationToken);
            if (!bid.IsSuccessful)
            {
                return NotFound(bid);
            }
            if (bidDto.IsAccepted == true)
            {
                var isAcceptedByClient = await _bidOfContractorService.CheckBidIsAcceptedByClientAsync(bid.Data.Id, cancellationToken);
                if (!isAcceptedByClient.IsSuccessful)
                {
                    return Problem(detail: isAcceptedByClient.ErrorMessage,
                    statusCode: 400,
                    title: "Bad Request");
                }
                var newStatus = new AddBidStatusDto
                {
                    BidOfContractorId = bid.Data.Id,
                    Status = BidStatusEnum.BidApprovedByContractor,
                };
                var newBidStatus = await _bidStatusService.AddAsync(newStatus, cancellationToken);
                if (!newBidStatus.IsSuccessful)
                {
                    return Problem(newBidStatus.Message);
                }
                else
                {
                    var newProject = new AddProjectDto
                    {
                        ContractorBidId = bidDto.BidId
                    };
                    var projectResult = await _projectService.AddAsync(newProject, cancellationToken);
                    if (!projectResult.IsSuccessful)
                    {
                        return Problem(projectResult.Message);
                    }
                    return Ok(projectResult);
                }
            }

            return Problem(ErrorMessages.ErrorWhileAcceptingBid);
        }
        [Authorize(Roles = "Contractor")]
        [HttpPut]
        [Route(nameof(RejectBidByContractor))]
        public async Task<IActionResult> RejectBidByContractor(UpdateBidAcceptanceDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var bid = await _bidOfContractorService.GetByIdAsync(bidDto.BidId, cancellationToken);
            if (!bid.IsSuccessful)
            {
                return NotFound(bid);
            }
            if (bidDto.IsAccepted == false)
            {

                var newStatus = new AddBidStatusDto
                {
                    BidOfContractorId = bid.Data.Id,
                    Status = BidStatusEnum.BidRejectedByContractor,
                };
                var newBidStatus = await _bidStatusService.AddAsync(newStatus, cancellationToken);
                if (!newBidStatus.IsSuccessful)
                {
                    return Problem(newBidStatus.Message);
                }
                var requestDto = await _requestService.GetByIdAsync(bid.Data.RequestId, cancellationToken);
                if (!requestDto.IsSuccessful)
                {
                    return Problem(requestDto.Message);
                }
               
                var updateRequestDto = new UpdateRequestDto
                {
                    ExpireAt = DateTime.Now.AddMinutes(7),   
                };
                var requestResult = await _requestService.UpdateAsync(updateRequestDto, cancellationToken);
                if (!requestResult.IsSuccessful)
                {
                    return Problem(requestResult.Message);
                }
                return Ok(newBidStatus);

            }
            return BadRequest(ErrorMessages.AnErrorWhileUpdatingStatus);
        }
        
        [HttpPut]
        [Route(nameof(AddContractor))]
        public async Task<IActionResult> AddContractor(AddContractorDto contractorDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _contractorService.AddAsync(contractorDto, cancellationToken);
            if (!result.IsSuccessful)
            {
                return Problem(result.Message);
            }
            return Ok(result);
        }

    }
}
