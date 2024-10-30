using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ContractorsAuctioneer.Controllers
{
    /// <summary>
    /// کنترلر مربوط به مدیریت پیشنهادات پیمانکاران.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BidOfContractorController : ControllerBase
    {
        private readonly IBidOfContractorService _bidOfContractorService;

        public BidOfContractorController(IBidOfContractorService bidOfContractorService)
        {
            _bidOfContractorService = bidOfContractorService;
        }

        /// <summary>
        /// افزودن پیشنهاد جدید توسط پیمانکار.
        /// </summary>
        /// <param name="bidOfContractorDto">اطلاعات پیشنهاد پیمانکار. <see cref="AddBidOfContractorDto"/>.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت‌آمیز بودن، نتیجه موفقیت بازگردانده می‌شود، در غیر این صورت خطا برگردانده می‌شود.</returns>
        [Authorize(Roles = "Contractor")]
        [HttpPost]
        [Route(nameof(AddBid))]
        [ProducesResponseType(typeof(Result<AddBidOfContractorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddBid([FromBody] AddBidOfContractorDto bidOfContractorDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addResult = await _bidOfContractorService.AddAsync(bidOfContractorDto, cancellationToken);
            if (addResult.IsSuccessful)
            {
                return Ok(addResult);
            }
            return StatusCode(500, addResult);
        }

        /// <summary>
        /// دریافت پیشنهاد پیمانکار بر اساس شناسه.
        /// </summary>
        /// <param name="bidId">شناسه پیشنهاد.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت پیدا شدن پیشنهاد، آن را باز می‌گرداند، در غیر این صورت خطا برگردانده می‌شود.</returns>
        [HttpGet]
        [Route(nameof(GetBidById))]
        [ProducesResponseType(typeof(BidOfContractorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBidById(int bidId, CancellationToken cancellationToken)
        {
            if (bidId <= 0)
            {
                return BadRequest("ورودی نامعتبر");
            }

            var result = await _bidOfContractorService.GetByIdAsync(bidId, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        /// <summary>
        /// به‌روزرسانی پیشنهاد پیمانکار.
        /// </summary>
        /// <param name="bidDto">اطلاعات به‌روزرسانی پیشنهاد پیمانکار. <see cref="UpdateBidOfContractorDto"/>.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، پیشنهاد به‌روزرسانی شده و نتیجه بازگشتی ارسال می‌شود.</returns>
        [Authorize(Roles = "Contractor")]
        [HttpPut]
        [Route(nameof(UpdateBid))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBid([FromBody] ChangeBidOfContractorDto bidDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = await _bidOfContractorService.GetByIdAsync(bidDto.BidId, cancellationToken);
                if (entity is { IsSuccessful: true, Data: not null })
                {
                  
                    var updatecontract = new UpdateBidOfContractorDto
                    {
                        BidId = entity.Data.Id,
                        SuggestedFee = bidDto.SuggestedFee,
                    };
                    var result = await _bidOfContractorService.UpdateAsync(updatecontract, cancellationToken);
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
        /// حذف پیشنهاد پیمانکار.
        /// </summary>
        /// <param name="bidId">شناسه پیشنهاد پیمانکار.</param>
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>در صورت موفقیت، پیشنهاد حذف می‌شود و نتیجه ارسال می‌گردد.</returns>
        [Authorize(Roles = "Contractor")]
        [HttpDelete]
        [Route(nameof(DeleteBid))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBid(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _bidOfContractorService.GetByIdAsync(bidId, cancellationToken);
                if (entity is { IsSuccessful: true, Data: not null })
                {
                    
                    var updatecontract = new UpdateBidOfContractorDto
                    {
                        BidId = entity.Data.Id,
                        IsDeleted = true,
                    };
                    var result = await _bidOfContractorService.UpdateAsync(updatecontract, cancellationToken);
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
        /// <param name="cancellationToken">توکن برای لغو درخواست در صورت نیاز.</param>
        /// <returns>لیست پیشنهادات پیمانکار را بازمی‌گرداند، در غیر این صورت پیام خطا برمی‌گرداند.</returns>
        [Authorize(Roles = "Contractor")]
        [HttpGet]
        [Route(nameof(GetBidsOfContractor))]
        [ProducesResponseType(typeof(IEnumerable<BidOfContractorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBidsOfContractor(CancellationToken cancellationToken)
        {
            var bidsOfContractor = await _bidOfContractorService.GetBidsOfContractorAsync(cancellationToken);
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
    }


}
