using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Contractors.Controllers
{
    [Route("api/contractors")]
    [ApiController]
    public class ContractorController(IContractorService contractorService)
        : ControllerBase
    {
        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(AddContractorDto contractorDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await contractorService.AddAsync(contractorDto, cancellationToken);
            if (!result.IsSuccessful)
            {
                return Problem(result.Message);
            }
            return Ok(result);
        }
    }
}
