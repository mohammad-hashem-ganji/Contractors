using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractorsAuctioneer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IProjectService _projectService;

        public RequestController(IRequestService requestService, IProjectService projectService)
        {
            _requestService = requestService;
            _projectService = projectService;
        }
        
        [HttpPost]
        [Route(nameof(AddRequest))]
        public async Task<IActionResult> AddRequest([FromBody] AddRequestDto requestDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _requestService.AddAsync(requestDto, cancellationToken);
                if (result)
                {
                    return Ok();
                }
                return StatusCode(500, "خطایی هنگام اضافه کردن درخواست رخ داد.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [Authorize(Roles = "Contractor")]
        [HttpGet]
        [Route(nameof(GetAllRequests))]
        public async Task<IActionResult> GetAllRequests(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _requestService.GetAllAsync(cancellationToken);
                if (result.IsSuccessful)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(result);
                }
            }
            catch (Exception ex)
            {
                // Log 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An error occurred while retrieving the requests.",
                        Details = ex.Message
                    });
            }
        }



    }
}
