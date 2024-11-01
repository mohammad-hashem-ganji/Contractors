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
    [Route("api/[controller]")]
    [ApiController]
    public class ReasonOfRejectRequestController(IRejectService rejectService) : ControllerBase
    {
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
