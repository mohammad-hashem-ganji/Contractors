using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        [Route(nameof(AddRegion))]
        public async Task<IActionResult> AddRegion([FromForm] AddRegionDto regionDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addResult = await _regionService.AddAsync(regionDto, cancellationToken);
            if (addResult.IsSuccessful)
            {
                return Ok(addResult);
            }
            return StatusCode(500, addResult);
        }
    }
}
