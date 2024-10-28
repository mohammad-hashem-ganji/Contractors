using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace ContractorsAuctioneer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [Authorize(Roles = "Contractor, Client")]
        [HttpGet]
        [Route(nameof(GetProjectById))]
        public async Task<IActionResult> GetProjectById(int projectId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _projectService.GetByIdAsync(projectId, cancellationToken);
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
        [Authorize(Roles = "Contractor")]
        [HttpGet]
        [Route(nameof(GetProjectOfBid))]
        public async Task<IActionResult> GetProjectOfBid(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _projectService.GetProjectOfbidAsync(bidId, cancellationToken);
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
