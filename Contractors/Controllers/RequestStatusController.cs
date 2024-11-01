using Contractors.Interfaces;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/requestStatuses")]
    [ApiController]
    public class RequestStatusController(IRequestStatusService requestStatusService) : ControllerBase
    {
        /// <summary>
        /// دریافت وضعیت درخواست بر اساس شناسه درخواست.
        /// </summary>
        /// <param name="requestId">شناسه درخواست.</param>
        /// <param name="cancellationToken">توکن لغو درخواست.</param>
        /// <returns>وضعیت درخواست.</returns>
        [Authorize(Roles = RoleNames.Client)]
        [HttpGet]
        [Route("{requestId}")]
        public async Task<IActionResult> GetStatusOfRequest(int requestId, CancellationToken cancellationToken)
        {
            var requests = await requestStatusService.GetRequestStatusesByRequestId(requestId, cancellationToken);
            if (requests.Data is null)
            {
                return Ok(requests);
            }
            return Ok(requests);
        }

    }
}
