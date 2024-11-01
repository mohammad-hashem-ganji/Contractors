using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContractorsAuctioneer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileAttachmentController : ControllerBase
    {
        private readonly IFileAttachmentService _fileAttachmentService;

        public FileAttachmentController(IFileAttachmentService fileAttachmentService)
        {
            _fileAttachmentService = fileAttachmentService;
        }
        
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto model, CancellationToken cancellationToken)
        {
            var result = await _fileAttachmentService.AddAsync(model, cancellationToken);

            if (result.IsSuccessful)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(GetFile))]
        public async Task<IActionResult> GetFile(int fileId, CancellationToken cancellationToken)
        {
            var file = await _fileAttachmentService.GetFileAsync(fileId, cancellationToken);
            if (file.IsSuccessful)
            {
                return Ok(file);
            }
            return BadRequest(file);
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        [Route(nameof(DownloadByRequestId))]
        public async Task<IActionResult> DownloadByRequestId(int requestId, int fileTypeId, CancellationToken cancellationToken)
        {
            // Call the service method to fetch the file attachment
            var result = await _fileAttachmentService.GetByRequestIdAndFileTypeAsync(requestId, (FileAttachmentType)fileTypeId, cancellationToken);

            // Check if the service returned a successful result with file data
            if (!result.IsSuccessful || result.Data == null)
            {
                return NotFound(result.ErrorMessage ?? "File not found.");
            }

            // Extract the file details from the result
            var fileData = result.Data;

            // Return the file with appropriate content type and file name
            return File(fileData.FileContent, fileData.ContentType, fileData.FileName);
        }


    }
}
