using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Services;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Controllers
{
    [Route("api/attachments")]
    [ApiController]
    public class FileAttachmentController(IFileAttachmentService fileAttachmentService) : ControllerBase
    {
        /// <summary>
        /// بارگذاری فایل جدید.
        /// </summary>
        /// <param name="model">مدل بارگذاری فایل شامل اطلاعات فایل.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>نتیجه بارگذاری فایل یا پیام خطا در صورت شکست.</returns>
        [Authorize(Roles = $"{RoleNames.Client},{RoleNames.Contractor}")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto model, CancellationToken cancellationToken)
        {
            var result = await fileAttachmentService.AddAsync(model, cancellationToken);

            if (result.IsSuccessful)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// دریافت فایل بر اساس شناسه فایل.
        /// </summary>
        /// <param name="fileId">شناسه فایل.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>فایل درخواست شده یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = $"{RoleNames.Client},{RoleNames.Contractor}")]
        [HttpGet]
        [Route("{fileId}")]
        public async Task<IActionResult> GetFile(int fileId, CancellationToken cancellationToken)
        {
            var file = await fileAttachmentService.GetFileAsync(fileId, cancellationToken);
            if (file.IsSuccessful)
            {
                return Ok(file);
            }
            return BadRequest(file);
        }

        /// <summary>
        /// دانلود فایل بر اساس شناسه درخواست و نوع فایل.
        /// </summary>
        /// <param name="requestId">شناسه درخواست.</param>
        /// <param name="fileTypeId">نوع فایل.</param>
        /// <param name="cancellationToken">توکن برای لغو عملیات در صورت نیاز.</param>
        /// <returns>فایل دانلود شده یا پیام خطا در صورت عدم موفقیت.</returns>
        [Authorize(Roles = $"{RoleNames.Client},{RoleNames.Contractor}")]
        [HttpGet]
        [Route("{requestId}/{fileTypeId}")]
        public async Task<IActionResult> DownloadByRequestId(int requestId, int fileTypeId, CancellationToken cancellationToken)
        {
            // Fetch the file attachment based on requestId and fileTypeId
            var fileAttachment = await fileAttachmentService.GetByRequestIdAndFileTypeAsync(requestId, (FileAttachmentType)fileTypeId, cancellationToken);

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
