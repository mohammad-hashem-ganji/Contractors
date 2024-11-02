using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    /// <summary>
    /// کنترلر مربوط به مدیریت پیشنهادات پیمانکاران.
    /// </summary>
    [Route("api/bids")]
    [ApiController]
    public class BidOfContractorController(
        IBidOfContractorService bidOfContractorService,
        IProjectService projectService,
        IBidStatusService bidStatusService,
        IRequestService requestService)
        : ControllerBase
    {
        /// <summary>
        /// افزودن پیشنهاد جدید توسط پیمانکار.
        /// </summary>
        /// <param name="bidOfContractorDto">اطلاعات پیشنهاد پیمانکار. <see cref="AddBidOfContractorDto"/>.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت‌آمیز بودن، نتیجه موفقیت بازگردانده می‌شود، در غیر این صورت خطا برگردانده می‌شود.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(Result<AddBidOfContractorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AddBidOfContractorDto bidOfContractorDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addResult = await bidOfContractorService.AddAsync(bidOfContractorDto, cancellationToken);
            if (addResult.IsSuccessful)
            {
                return Ok(addResult);
            }
            return StatusCode(500, addResult);
        }

        /// <summary>
        /// دریافت اطلاعات پیشنهاد بر اساس شناسه پیشنهاد.
        /// </summary>
        /// <param name="bidId">شناسه پیشنهاد که اطلاعات آن مورد نظر است.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>جزئیات پیشنهاد در صورت موفقیت یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = $"{RoleNames.Contractor},{RoleNames.Client}")]
        [HttpGet]
        [Route("{bidId}")]
        [ProducesResponseType(typeof(BidOfContractorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBidById(int bidId, CancellationToken cancellationToken)
        {
            if (bidId <= 0)
            {
                return BadRequest("ورودی نامعتبر");
            }

            var result = await bidOfContractorService.GetByIdAsync(bidId, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        /// <summary>
        /// به‌روزرسانی پیشنهاد یک پیمانکار.
        /// </summary>
        /// <param name="bidDto">مدل به‌روزرسانی شامل شناسه پیشنهاد و هزینه پیشنهادی.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>نتیجه به‌روزرسانی: بدون محتوا در صورت موفقیت یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpPut]
        [Route("update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // به‌روزرسانی موفق
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // درخواست نامعتبر
        [ProducesResponseType(StatusCodes.Status404NotFound)] // پیشنهاد پیدا نشد
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // خطای داخلی
        public async Task<IActionResult> UpdateBid([FromBody] ChangeBidOfContractorDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = await bidOfContractorService.GetByIdAsync(bidDto.BidId, cancellationToken);
                if (entity is { IsSuccessful: true, Data: not null })
                {
                  
                    var updatecontract = new UpdateBidOfContractorDto
                    {
                        BidId = entity.Data.Id,
                        SuggestedFee = bidDto.SuggestedFee,
                    };
                    var result = await bidOfContractorService.UpdateAsync(updatecontract, cancellationToken);
                    // if succes
                    return NoContent();
                }
                return NotFound(entity);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "خطایی در بازیابی پیشنهادات رخ داده است.",
                    });
            }
        }

        /// <summary>
        /// حذف یک پیشنهاد پیمانکار.
        /// </summary>
        /// <param name="bidId">شناسه پیشنهاد که باید حذف شود.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>نتیجه حذف: بدون محتوا در صورت موفقیت یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpDelete]
        [Route("{bidId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // حذف موفق
        [ProducesResponseType(StatusCodes.Status404NotFound)] // پیشنهاد پیدا نشد
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // خطای داخلی
        public async Task<IActionResult> DeleteBid(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await bidOfContractorService.GetByIdAsync(bidId, cancellationToken);
                if (entity is { IsSuccessful: true, Data: not null })
                {
                    
                    var updatecontract = new UpdateBidOfContractorDto
                    {
                        BidId = entity.Data.Id,
                        IsDeleted = true,
                    };
                    var result = await bidOfContractorService.UpdateAsync(updatecontract, cancellationToken);
                    return NoContent();
                }
                return NotFound(entity);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "خطایی در بازیابی پیشنهادات رخ داده است.",
                    });
            }
        }

        /// <summary>
        /// دریافت لیست پیشنهادات پیمانکار.
        /// </summary>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>لیستی از پیشنهادات پیمانکار یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpGet]
        [Route("contractor-bids")]
        [ProducesResponseType(typeof(IEnumerable<BidOfContractorDto>), StatusCodes.Status200OK)] // موفقیت آمیز
        [ProducesResponseType(StatusCodes.Status404NotFound)] // پیشنهادات پیدا نشد
        public async Task<IActionResult> GetBidsOfContractor(CancellationToken cancellationToken)
        {
            var bidsOfContractor = await bidOfContractorService.GetBidsOfContractorAsync(cancellationToken);
            if (bidsOfContractor.IsSuccessful)
            {
                if (bidsOfContractor.Data != null && bidsOfContractor.Data.Any())
                {
                    return Ok(bidsOfContractor);
                }
                return NotFound(bidsOfContractor);
            }
            return NotFound(bidsOfContractor);
        }

        /// <summary>
        /// رد پیشنهاد توسط پیمانکار.
        /// </summary>
        /// <param name="bidDto">شیء حاوی اطلاعات مربوط به پیشنهاد برای رد.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>نتیجهٔ عملیات رد پیشنهاد، شامل وضعیت جدید پیشنهاد یا پیام خطا.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpPut]
        [Route("reject")]
        public async Task<IActionResult> RejectBidByContractor(UpdateBidAcceptanceDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var bid = await bidOfContractorService.GetByIdAsync(bidDto.BidId, cancellationToken);
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
                var newBidStatus = await bidStatusService.AddAsync(newStatus, cancellationToken);
                if (!newBidStatus.IsSuccessful)
                {
                    return Problem(newBidStatus.Message);
                }
                var requestDto = await requestService.GetByIdAsync(bid.Data.RequestId, cancellationToken);
                if (!requestDto.IsSuccessful)
                {
                    return Problem(requestDto.Message);
                }

                var updateRequestDto = new UpdateRequestDto
                {
                    ExpireAt = DateTime.Now.AddMinutes(7),
                };
                var requestResult = await requestService.UpdateAsync(updateRequestDto, cancellationToken);
                if (!requestResult.IsSuccessful)
                {
                    return Problem(requestResult.Message);
                }
                return Ok(newBidStatus);

            }
            return BadRequest(ErrorMessages.AnErrorWhileUpdatingStatus);
        }

        /// <summary>
        /// به دست آوردن لیست پیشنهادات پذیرفته شده توسط مشتری.
        /// </summary>
        /// <param name="cancellationToken">توکن لغو درخواست.</param>
        /// <returns>لیستی از پیشنهادات پذیرفته شده توسط مشتری.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpGet]
        [Route("client-accepted")]
        public async Task<IActionResult> ShowBidsAcceptedByClient(CancellationToken cancellationToken)
        {
            var acceptedBids = await bidOfContractorService.GetBidsAcceptedByClientAsync(cancellationToken);
            if (!acceptedBids.IsSuccessful)
            {
                return NotFound(acceptedBids);
            }
            return Ok(acceptedBids);
        }

        /// <summary>
        /// قبول پیشنهاد توسط پیمانکار.
        /// </summary>
        /// <param name="bidDto">مدل اطلاعات پیشنهاد که شامل شناسه پیشنهاد و وضعیت پذیرش است.</param>
        /// <param name="cancellationToken">توکن لغو درخواست.</param>
        /// <returns>نتیجه عملیات پذیرش پیشنهاد.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpPut]
        [Route("accept")]
        public async Task<IActionResult> AcceptBidByContractor(UpdateBidAcceptanceDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var bid = await bidOfContractorService.GetByIdAsync(bidDto.BidId, cancellationToken);
            if (!bid.IsSuccessful)
            {
                return NotFound(bid);
            }
            if (bidDto.IsAccepted == true)
            {
                var isAcceptedByClient = await bidOfContractorService.CheckBidIsAcceptedByClientAsync(bid.Data.Id, cancellationToken);
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
                var newBidStatus = await bidStatusService.AddAsync(newStatus, cancellationToken);
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
                    var projectResult = await projectService.AddAsync(newProject, cancellationToken);
                    if (!projectResult.IsSuccessful)
                    {
                        return Problem(projectResult.Message);
                    }
                    return Ok(projectResult);
                }
            }

            return Problem(ErrorMessages.ErrorWhileAcceptingBid);
        }

        /// <summary>
        /// دریافت پروژه مرتبط با پیشنهاد.
        /// </summary>
        /// <param name="bidId">شناسه پیشنهاد.</param>
        /// <param name="cancellationToken">توکن لغو درخواست.</param>
        /// <returns>پروژه مرتبط با پیشنهاد.</returns>
        [Authorize(Roles = RoleNames.Contractor)]
        [HttpGet]
        [Route("{bidId}/project")]
        public async Task<IActionResult> GetProjectOfBid(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await projectService.GetProjectOfbidAsync(bidId, cancellationToken);
                if (result.IsSuccessful)
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
                        Message = "An error occurred while retrieving the bid.",
                        Details = ex.Message
                    });
            }
        }
    }
}
