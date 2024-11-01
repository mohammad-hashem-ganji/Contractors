using Contractors.Dtos;
using Contractors.Interfaces;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace Contractors.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController(IProjectService projectService) : ControllerBase
    {
        [Authorize(Roles = $"{RoleNames.Client}, {RoleNames.Contractor}")]
        [HttpGet]
        [Route("{projectId}")]
        public async Task<IActionResult> GetProjectById(int projectId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await projectService.GetByIdAsync(projectId, cancellationToken);
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

        //[Authorize(Roles = "Contractor, Client")]
        //[HttpGet]
        //[Route(nameof(GetProjectOfBid))]
        //public async Task<IActionResult> GetProject(CancellationToken cancellationToken)
        //{

        //}

    }
}
