using Contractors.Interfaces;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestStatusController(IRequestStatusService requestStatusService) : ControllerBase
    {
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
