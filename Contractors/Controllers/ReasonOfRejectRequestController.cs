using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractorsAuctioneer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReasonOfRejectRequestController : ControllerBase
    {
        private readonly IRejectService _rejectService;

        public ReasonOfRejectRequestController(IRejectService rejectService)
        {
            _rejectService = rejectService;
        }
        [Authorize(Roles = "Client")]
        [HttpPost]
        [Route(nameof(AddRjectRquest))]
        public async Task<IActionResult> AddRjectRquest(AddReasonToRejectRequestDto reasonDto, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var addReason = await _rejectService.AddRjectRquestAsync(reasonDto, cancellationToken);
            if (addReason.IsSuccessful)
            {
                return Ok(addReason);
            }
            else
            {
                return StatusCode(500, addReason);
            }
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(GetReasonsOfRejectingRequestByRequestId))]
        public async Task<IActionResult> GetReasonsOfRejectingRequestByRequestId(int requestId, CancellationToken cancellationToken)
        {
            if (requestId <= 0)
            {
                return BadRequest(new Result<List<GetReasonOfRejectRequestDto>>()
                    .WithValue(null)
                    .Failure("Invalid requestId"));
            }
            var reasons = await _rejectService.GetReasonsOfRejectingRequestByRequestIdAsync(requestId, cancellationToken);

            if (reasons is null || !reasons.IsSuccessful)
            {
                return NotFound(reasons);
            }

            return Ok(reasons);
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(GetReasonsOfRejectingRequestByClient))]
        public async Task<IActionResult> GetReasonsOfRejectingRequestByClient(CancellationToken cancellationToken)
        {
            var reasons = await _rejectService.GetReasonsOfRejectingRequestByClientAsycn(cancellationToken);
            if (reasons is null || !reasons.IsSuccessful)
            {
                return NotFound(reasons);
            }

            return Ok(reasons);
        }
    }
}
