using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contractors.Controllers
{
    /// <summary>
    /// کنترلر برای مدیریت دلایل رد درخواست‌ها.
    /// </summary>
    [Route("api/rejectReasons")]
    [ApiController]
    public class ReasonOfRejectRequestController(IRejectService rejectService) : ControllerBase
    {
        /// <summary>
        /// دریافت دلایل رد درخواست بر اساس شناسه درخواست.
        /// </summary>
        /// <param name="requestId">شناسه درخواست که دلایل رد آن مورد نظر است.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>لیست دلایل رد درخواست یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = RoleNames.Client)]
        [HttpGet]
        [Route("{requestId}")]
        public async Task<IActionResult> GetReasonsOfRejectingRequestByRequestId(int requestId, CancellationToken cancellationToken)
        {
            if (requestId <= 0)
            {
                return BadRequest(new Result<List<GetReasonOfRejectRequestDto>>()
                    .WithValue(null)
                    .Failure("Invalid requestId"));
            }
            var reasons = await rejectService.GetReasonsOfRejectingRequestByRequestIdAsync(requestId, cancellationToken);
            if (!reasons.IsSuccessful)
            {
                return NotFound(reasons);
            }

            return Ok(reasons);
        }

        /// <summary>
        /// دریافت دلایل رد درخواست‌ها برای کاربر.
        /// </summary>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>لیست دلایل رد درخواست‌ها یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = RoleNames.Client)]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetReasonsOfRejectingRequestByClient(CancellationToken cancellationToken)
        {
            var reasons = await rejectService.GetReasonsOfRejectingRequestByClientAsync(cancellationToken);
            if (!reasons.IsSuccessful)
            {
                return NotFound(reasons);
            }

            return Ok(reasons);
        }
    }
}
