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
        [Authorize]
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
        [Route(nameof(DonlowdByRequestId))]
        public async Task<IActionResult> DonlowdByRequestId(int requestId, int fileTypeId, CancellationToken cancellationToken)
        {
            // Fetch the file attachment based on requestId and fileTypeId
            var fileAttachment = await _fileAttachmentService.GetByRequestIdAndFileTypeAsync(requestId, (FileAttachmentType)fileTypeId, cancellationToken);

            // Check if the file was found
            if (fileAttachment == null || string.IsNullOrEmpty(fileAttachment.FilePath) || !System.IO.File.Exists(fileAttachment.FilePath))
            {
                return NotFound("File not found.");
            }

            // Read the file contents
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fileAttachment.FilePath, cancellationToken);
            var fileName = fileAttachment.FileName ?? "downloaded_file";

            // Return the file with appropriate content type
            return File(fileBytes, "application/octet-stream", fileName);
        }

    }
}
