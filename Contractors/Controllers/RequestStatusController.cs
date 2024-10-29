using Contractors.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestStatusController : ControllerBase
    {
        private readonly IRequestStatusService _requestStatusService;

        public RequestStatusController(IRequestStatusService requestStatusService)
        {
            _requestStatusService = requestStatusService;
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(GetStatusOfRequest))]
        public async Task<IActionResult> GetStatusOfRequest(int requestId, CancellationToken cancellationToken)
        {
            var requests = await _requestStatusService.GetRequestStatusesByRequestId(requestId, cancellationToken);
            if (requests.Data is null)
            {
                return Ok(requests);
            }
            return Ok(requests);
        }

    }
}
