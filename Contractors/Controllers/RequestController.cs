using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController(
        IRequestService requestService,
        IBidOfContractorService bidOfContractorService,
        IRequestStatusService requestStatusService,
        IRejectService rejectService)
        : ControllerBase
    {
        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] AddRequestDto requestDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await requestService.AddAsync(requestDto, cancellationToken);
                if (result)
                {
                    return Ok();
                }
                return StatusCode(500, "خطایی هنگام اضافه کردن درخواست رخ داد.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [Authorize(Roles = RoleNames.Contractor)]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = await requestService.GetAllAsync(cancellationToken);
                if (result is not null && result.IsSuccessful)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                // Log 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An error occurred while retrieving the requests.",
                        Details = ex.Message
                    });
            }
        }

        [Authorize(Roles = RoleNames.Client)]
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)] // Successful response with request details
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ShowRequestOfClient(CancellationToken cancellationToken)
        {
            var request = await requestService.GetRequestOfClientAsync(cancellationToken);

            if (!request.IsSuccessful)
            {
                return BadRequest(request);
            }
            var requestIsChecked = await requestService.CheckRequestOfClientAsync(cancellationToken);
            if (!requestIsChecked.IsSuccessful || request.Data == null)
            {
                return Problem(detail: request.ErrorMessage, statusCode: 500, title: "Bad Request");
            }
            return Ok(request);
        }

        [Authorize(Roles = RoleNames.Client)]
        [HttpGet]
        [Route("bids")]
        public async Task<IActionResult> GetBidsOfRequest(CancellationToken cancellationToken)
        {
            var bids = await bidOfContractorService.GetBidsOfRequestAsync( cancellationToken);
            if (bids.Data is null)
            {
                return BadRequest(bids);
            }
            return Ok(bids);
        }

        [Authorize(Roles = RoleNames.Client)]
        [HttpPut]
        [Route("reject")]
        public async Task<IActionResult> RejectRequestByClient(AddReasonToRejectRequestDto reasonDto, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var addReason = await rejectService.AddRejectRequestAsync(reasonDto, cancellationToken);
            if (addReason.IsSuccessful)
            {
                return Ok(addReason);
            }

            return StatusCode(500, addReason);
        }

        [Authorize(Roles = RoleNames.Contractor)]
        [HttpPut]
        [Route("not-interested")]
        public async Task<IActionResult> RejectRequestByContractor(UpdateRequestAcceptanceDto rejectedRequestDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = await requestService.GetByIdAsync(rejectedRequestDto.RequestId, cancellationToken);
            if (!request.IsSuccessful)
            {
                return NotFound();
            }

            if (rejectedRequestDto.IsAccepted == false)
            {
                var newStatus = new AddRequestStatusDto
                {
                    RequestId = rejectedRequestDto.RequestId,
                    Status = RequestStatusEnum.RequestRejectedByContractor
                };
                var newRequestStatus = await requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful)
                {
                    return BadRequest(newRequestStatus);
                }

                return Ok();
            }

            return BadRequest("مقادیر ورودی نا معتبر است");
        }
    }
}
