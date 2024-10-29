
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using System.Threading;

namespace Contractors.Services
{
    public class FileAttachmentService : IFileAttachmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public FileAttachmentService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<Result<FileAttachmentDto>> AddAsync(FileUploadDto model, CancellationToken cancellationToken)
        {
            try
            {
                string path = string.Empty;
                if (model is null)
                {
                    return new Result<FileAttachmentDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
                }
                const long maxFileSize = 7 * 1024 * 1024;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                if (model.File.Length > maxFileSize)
                {
                    return new Result<FileAttachmentDto>()
                        .WithValue(null)
                        .Failure("حجم فایل زیاد است.");
                }
                var fileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new Result<FileAttachmentDto>()
                        .WithValue(null)
                        .Failure("File type is not allowed.");
                }
                if (model.FileAttachmentType == FileAttachmentType.PlanNotebook)
                {
                    path = Path.Combine(_environment.WebRootPath, "FileAttachments", "planNotebooks");
                    var a = 0;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                if (model.FileAttachmentType == FileAttachmentType.Other)
                {
                    path = Path.Combine(_environment.WebRootPath, "FileAttachments", "Others");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                Guid newguid = Guid.NewGuid();
                string fileName = $"{newguid}_{model.File.FileName}";
                var filePath = Path.Combine(path, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream, cancellationToken);
                }
                var fileAttachment = new FileAttachment
                {
                    FileName = model.File.FileName,
                    FilePath = filePath,
                    RequestId = model.RequestId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                };
                await _context.FileAttachments.AddAsync(fileAttachment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Return the result
                return new Result<FileAttachmentDto>()
                    .WithValue(null)
                    .Success("فایل آپلود شد");
            }
            catch (Exception ex)
            {
                return new Result<FileAttachmentDto>()
                    .WithValue(null)
                    .Failure(ex.Message);
            }
        }


        public async Task<Result<UpdateFileAttachmentDto>> UpdateAsync(UpdateFileAttachmentDto fileAttachmentDto, CancellationToken cancellationToken)
        {
            try
            {
                var file = await _context.FileAttachments
                    .Where(f => f.Id == fileAttachmentDto.Id && f.IsDeleted == false)
                    .Include(f => f.Request)
                    .FirstOrDefaultAsync(cancellationToken);
                if (file == null)
                {
                    return new Result<UpdateFileAttachmentDto>().WithValue(null).Failure(ErrorMessages.FileNotFound);
                }
                //file.IsDeleted = fileAttachmentDto.IsDeleted;
                file.UpdatedBy = fileAttachmentDto.UpdatedBy;
                file.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);

                return new Result<UpdateFileAttachmentDto>().WithValue(fileAttachmentDto).Success(SuccessMessages.FileDetailsUpdated);
            }
            catch (Exception ex)
            {
                return new Result<UpdateFileAttachmentDto>().WithValue(null).Failure(ex.Message);
            }
        }

        public async Task<Result<FileStreamResult>> GetFileAsync(int fileId,CancellationToken cancellationToken)
        {
            var fileAttachment = await _context.FileAttachments.FindAsync(fileId);
            if (fileAttachment == null)
            {
                return new Result<FileStreamResult>().WithValue(null).Failure("فایل پیدا نشد");
            }

            var filePath = fileAttachment.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return new Result<FileStreamResult>().WithValue(null).Failure("خطا!");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(filePath);
            return new Result<FileStreamResult>()
                .WithValue(new FileStreamResult(memory, contentType)
                {
                    FileDownloadName = fileAttachment.FileName
                }).Success("فایل یافت شد");

        }


        public async Task<FileAttachment?> GetByRequestIdAndFileTypeAsync(int requestId, FileAttachmentType fileType, CancellationToken cancellationToken)
        {
            return await _context.FileAttachments
                .FirstOrDefaultAsync(f => f.RequestId == requestId && f.FileTypeId == (int)fileType, cancellationToken);
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".png", "image/png" },
            };
        }




    }
}
