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
    [Route("api/clients")]
    [ApiController]
    public class ClientController(
        IBidOfContractorService bidOfContractorService,
        IRequestService requestService,
        IRequestStatusService requestStatusService,
        IBidStatusService bidStatusService,
        IHttpContextAccessor httpContextAccessor)
        : ControllerBase
    {
        /// <summary>
        /// تأیید پیشنهاد از طرف مشتری.
        /// </summary>
        /// <param name="bidDto">مدل شامل اطلاعات پیشنهاد و وضعیت تأیید.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>پیشنهاد به‌روزشده در صورت موفقیت، یا پیام خطا در صورت شکست.</returns>
        [Authorize(Roles = RoleNames.Client)]
        [HttpPut]
        [Route("accept")]
        [ProducesResponseType(typeof(UpdateBidOfContractorDto), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptBidByClient(GetUpdateBidAcceptanceDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bid = await bidOfContractorService.GetByIdAsync(bidDto.Id, cancellationToken);
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
                var newBidStatus = await bidStatusService.AddAsync(newStatus, cancellationToken);
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
                var updatedBid = await bidOfContractorService.UpdateAsync(updatecontract, cancellationToken);
                if (!updatedBid.IsSuccessful)
                {
                    return Problem(updatedBid.Message);
                }
                return Ok(updatedBid);
            }

            return BadRequest();
        }

        /// <summary>
        /// تغییر وضعیت درخواست توسط مشتری.
        /// </summary>
        /// <param name="requestDto">مدل شامل شناسه درخواست و وضعیت تأیید.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>وضعیت جدید درخواست یا پیام خطا در صورت شکست.</returns>
        [Authorize(Roles = RoleNames.Client)]
        [HttpPut]
        [Route("change-state")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeRequestStatus([FromBody] UpdateRequestAcceptanceDto requestDto, CancellationToken cancellationToken)
        {
            var newStatus = new AddRequestStatusDto();
            var newRequestStatus = new Result<AddRequestStatusDto>();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var appId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(appId, out var userId))
            {
                return Problem(detail: "خطا!", statusCode: 500, title: "Bad Request");
            }
            var request = await requestService.CheckRequestOfClientAsync(cancellationToken);
            if (!request.IsSuccessful || request.Data == null)
            {
                return NotFound("درخواست مورد نظر منقضی یا از دسترس خارج شده است.");
            }

            if (requestDto.RequestId != request.Data.Id || request.Data.IsActive == false)
            {
                return NotFound();
            }

            if (requestDto.IsAccepted)
            {
                newStatus = new AddRequestStatusDto
                {
                    RequestId = request.Data.Id,
                    Status = Entites.RequestStatusEnum.RequestApprovedByClient
                };

                newRequestStatus = await requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful)
                {
                    return Problem(detail: newRequestStatus.ErrorMessage, statusCode: 500, title: newRequestStatus.ErrorMessage);
                }

                var updateRequestDto = new UpdateRequestDto
                {
                    Id = request.Data.Id,
                    IsActive = true,
                    ExpireAt = DateTime.Now.AddMinutes(7),
                    IsAcceptedByClient = true
                };
                var updateResult = await requestService.UpdateAsync(updateRequestDto, cancellationToken);
                if (!updateResult.IsSuccessful)
                {
                    return Problem(detail: updateResult.ErrorMessage, statusCode: 500, title: "Internal Server Error");
                }
                newStatus = new AddRequestStatusDto
                {
                    RequestId = request.Data.Id,
                    Status = Entites.RequestStatusEnum.RequestIsInTenderphase
                };

                newRequestStatus = await requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful)
                {
                    return Problem(detail: newRequestStatus.ErrorMessage, statusCode: 500, title: newRequestStatus.ErrorMessage);
                }
                return Ok(requestDto);
            }
            else
            {
                newStatus = new AddRequestStatusDto
                {
                    RequestId = requestDto.RequestId,
                    Status = Entites.RequestStatusEnum.RequestRejectedByClient
                };
   
                newRequestStatus = await requestStatusService.AddAsync(newStatus, cancellationToken);
                if (!newRequestStatus.IsSuccessful) return Problem(detail: newRequestStatus.ErrorMessage,
                    statusCode: 400, title : newRequestStatus.ErrorMessage);
                var updateRequestDto = new UpdateRequestDto
                {
                    IsActive = false,
                    ExpireAt = null,
                    IsAcceptedByClient = false
                };
                var updateResult = await requestService.UpdateAsync(updateRequestDto, cancellationToken);
                if (!updateResult.IsSuccessful)
                {
                    return Problem(detail: updateResult.ErrorMessage, statusCode: 500, title: updateResult.ErrorMessage);
                }
                return Ok(requestDto);
            }
            
            //return Problem(detail: "خطا!", statusCode: 400, title: "Bad Request");
        }

        /// <summary>
        /// دریافت لیست پیشنهادات مرتبط با درخواست از طرف مشتری.
        /// </summary>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>لیست پیشنهادات یا پیام خطا در صورت شکست.</returns>
        [Authorize(Roles = RoleNames.Client)]
        [HttpGet]
        [Route("bids")]
        [ProducesResponseType(typeof(Result<List<BidOfContractorDto>>), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBidsOfRequestByClient( CancellationToken cancellationToken)
        {
            var bidsOfRequest = await bidOfContractorService.GetBidsOfRequestAsync(cancellationToken);
            if (!bidsOfRequest.IsSuccessful)
            {
                return Problem(bidsOfRequest.Message);
            }
            return Ok(bidsOfRequest);
        }
    }


}
