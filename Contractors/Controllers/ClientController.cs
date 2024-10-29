using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Net.Http;
using System.Security.Claims;

namespace Contractors.Controllers
{
    /// <summary>
    /// کنترلر مربوط به مشتری برای مدیریت پیشنهادات و درخواست‌ها.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IBidOfContractorService _bidOfContractorService;
        private readonly IProjectService _projectService;
        private readonly IRequestService _requestService;
        private readonly IRequestStatusService _requestStatusService;
        private readonly IBidStatusService _bidStatusService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// سازنده کنترلر مشتری.
        /// </summary>
        /// <param name="bidOfContractorService">سرویسی برای مدیریت پیشنهادات پیمانکار.</param>
        /// <param name="projectService">سرویسی برای مدیریت پروژه‌ها.</param>
        /// <param name="requestService">سرویسی برای مدیریت درخواست‌ها.</param>
        /// <param name="requestStatusService">سرویسی برای مدیریت وضعیت درخواست‌ها.</param>
        /// <param name="bidStatusService">سرویسی برای مدیریت وضعیت پیشنهادات.</param>
        /// <param name="httpContextAccessor">دستیار برای دسترسی به زمینه HTTP.</param>
        public ClientController(IBidOfContractorService bidOfContractorService, IProjectService projectService,
            IRequestService requestService, IRequestStatusService requestStatusService, IBidStatusService bidStatusService,
            IHttpContextAccessor httpContextAccessor)
        {
            _bidOfContractorService = bidOfContractorService;
            _projectService = projectService;
            _requestService = requestService;
            _requestStatusService = requestStatusService;
            _bidStatusService = bidStatusService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// تایید پیشنهاد توسط مشتری.
        /// </summary>
        /// <param name="bidDto">مدل اطلاعات پیشنهاد برای تایید.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، اطلاعات پیشنهاد تایید شده.</returns>
        [Authorize(Roles = "Client")]
        [HttpPut]
        [Route(nameof(AcceptBidByClient))]
        [ProducesResponseType(typeof(UpdateBidOfContractorDto), StatusCodes.Status200OK)] // Successful update response
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptBidByClient(GetUpdateBidAcceptanceDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bid = await _bidOfContractorService.GetByIdAsync(bidDto.Id, cancellationToken);
            if (!bid.IsSuccessful)
            {
                return NotFound(bid);
            }

            if (bidDto.IsAccepted)
            {
                var newStatus = new AddBidStatusDto
                {
                    BidOfContractorId = bid.Data.Id,
                    Status = Entites.BidStatusEnum.BidApprovedByClient,
                };
                var newBidStatus = await _bidStatusService.AddAsync(newStatus, cancellationToken);
                if (!newBidStatus.IsSuccessful)
                {
                    return Problem(newBidStatus.Message);
                }
                bid.Data.ExpireAt = DateTime.Now.AddMinutes(3);
                var updatecontract = new UpdateBidOfContractorDto
                {
                    ExpireAt = bid.Data.ExpireAt,
                    BidId = bid.Data.Id,
                };
                var updatedBid = await _bidOfContractorService.UpdateAsync(updatecontract, cancellationToken);
                if (!updatedBid.IsSuccessful)
                {
                    return Problem(updatedBid.Message);
                }
                return Ok(updatedBid);
            }

            return BadRequest();
        }

        /// <summary>
        /// تایید درخواست توسط مشتری.
        /// </summary>
        /// <param name="requestDto">مدل اطلاعات درخواست برای تایید.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، هیچ محتوایی بازگشت نمی‌دهد.</returns>
        [Authorize(Roles = "Client")]
        [HttpPut]
        [Route(nameof(ClientRequestStatusController))]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // No content on successful acceptance
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClientRequestStatusController([FromBody] UpdateRequestAcceptanceDto requestDto, CancellationToken cancellationToken)
        {
            var newStatus = new AddRequestStatusDto();
            var newRequestStatus = new Result<AddRequestStatusDto>();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var appId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(appId, out var userId))
            {
                return Problem(detail: "خطا!", statusCode: 500, title: "Bad Request");
            }
            var request = await _requestService.CheckRequestOfClientAsync(cancellationToken);
            if (!request.IsSuccessful || request.Data == null)
            {
                return Problem(detail: request.ErrorMessage, statusCode: 500, title: "Bad Request");
            }
            if (requestDto.RequestId != request.Data.Id && request.Data.IsActive == false) return NotFound(request);
            //if (request.Data.RequestStatuses != null && request.Data.RequestStatuses.Any(rs => rs.Status == RequestStatusEnum.RequestApprovedByClient))
            //{
            //    return Problem(detail: "درخواست قبلا تایید شده!", statusCode: 500, title: "Bad Request");
            //}
            if (requestDto.IsAccepted == true)
            {
                newStatus = new AddRequestStatusDto
                {
                    RequestId = request.Data.Id,
                    Status = Entites.RequestStatusEnum.RequestApprovedByClient
                };

                newRequestStatus = await _requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful) return Problem(detail: newRequestStatus.ErrorMessage, statusCode: 500, title: "Internal Server Error");

                request.Data.ExpireAt = DateTime.Now.AddMinutes(7);
                var updateResult = await _requestService.UpdateAsync(request.Data, cancellationToken);
                if (!updateResult.IsSuccessful)
                {
                    return Problem(detail: updateResult.ErrorMessage, statusCode: 500, title: "Internal Server Error");
                }

                return Ok();
            }
            else
            {
                newStatus = new AddRequestStatusDto
                {
                    RequestId = requestDto.RequestId,
                    Status = Entites.RequestStatusEnum.RequestRejectedByClient
                };
                newRequestStatus = await _requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful) return Problem(detail: newRequestStatus.ErrorMessage,
                    statusCode: 400, title: "Bad Request");
                return Ok();
            }
            
            //return Problem(detail: "خطا!", statusCode: 400, title: "Bad Request");
        }

        /// <summary>
        /// رد درخواست توسط مشتری.
        /// </summary>
        /// <param name="requestDto">مدل اطلاعات درخواست برای رد.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، هیچ محتوایی بازگشت نمی‌دهد.</returns>
        //[Authorize(Roles = "Client")]
        //[HttpPut]
        //[Route(nameof(RejectingRequestByClient))]
        //[ProducesResponseType(StatusCodes.Status204NoContent)] // No content on successful rejection
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> RejectingRequestByClient([FromBody] UpdateRequestAcceptanceDto requestDto, CancellationToken cancellationToken)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var request = await _requestService.CheckRequestOfClientAsync(cancellationToken);
        //    if (!request.IsSuccessful || request.Data == null) return Problem(detail: request.ErrorMessage,
        //        statusCode: 400, title: "Bad Request");
        //    if (requestDto.IsAccepted == false)
        //    {
        //        var newStatus = new AddRequestStatusDto
        //        {
        //            RequestId = requestDto.RequestId,
        //            Status = Entites.RequestStatusEnum.RequestRejectedByClient
        //        };
        //        var newRequestStatus = await _requestStatusService.AddAsync(newStatus, cancellationToken);
        //        if (!newRequestStatus.IsSuccessful) return Problem(detail: newRequestStatus.ErrorMessage,
        //            statusCode: 400, title: "Bad Request");
        //        return Ok();
        //    }

        //    return BadRequest("مقادیر ورودی نا معتبر است");
        //}

        /// <summary>
        /// دریافت پیشنهادات مرتبط با درخواست مشتری.
        /// </summary>
        /// <param name="requestId">شناسه درخواست.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، لیستی از پیشنهادات مرتبط با درخواست.</returns>
        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(GetBidsOfRequestByClient))]
        [ProducesResponseType(typeof(Result<List<BidOfContractorDto>>), StatusCodes.Status200OK)] // Successful response with bid list
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBidsOfRequestByClient(int requestId, CancellationToken cancellationToken)
        {
            if (requestId < 0)
            {
                return BadRequest("مقدار ورودی نا معتبر است");
            }
            var bidsOfRequest = await _bidOfContractorService.GetBidsOfRequestAsync(requestId, cancellationToken);
            if (!bidsOfRequest.IsSuccessful)
            {
                return Problem(bidsOfRequest.Message);
            }
            return Ok(bidsOfRequest);
        }

        /// <summary>
        /// نمایش درخواست مشتری.
        /// </summary>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، اطلاعات درخواست مشتری.</returns>
        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(ShowRequestOfClient))]
        [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)] // Successful response with request details
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ShowRequestOfClient(CancellationToken cancellationToken)
        {
            var request = await _requestService.GetRequestOfClientAsync(cancellationToken);

            if (!request.IsSuccessful)
            {
                return BadRequest(request);
            }
            return Ok(request);
        }
    }


}
