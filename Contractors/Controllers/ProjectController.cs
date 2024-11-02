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
    [Route("api/projects")]
    [ApiController]
    public class ProjectController(IProjectService projectService) : ControllerBase
    {
        /// <summary>
        /// دریافت جزئیات پروژه بر اساس شناسه پروژه.
        /// </summary>
        /// <param name="projectId">شناسه پروژه مورد نظر.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>جزئیات پروژه یا پیام خطا در صورت عدم موفقیت.</returns>
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
    }
}
