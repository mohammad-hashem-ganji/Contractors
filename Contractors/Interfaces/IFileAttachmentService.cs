using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Results;
using Microsoft.AspNetCore.Mvc;

namespace Contractors.Interfaces
{
    public interface IFileAttachmentService
    {
        Task<Result<FileAttachmentDto>> AddAsync(FileUploadDto model, CancellationToken cancellationToken);
        Task<Result<FileStreamResult>> GetFileAsync(int fileId, CancellationToken cancellationToken);
        Task<Result<UpdateFileAttachmentDto>> UpdateAsync(UpdateFileAttachmentDto fileAttachmentDto, CancellationToken cancellationToken);
        Task<FileAttachment?> GetByRequestIdAndFileTypeAsync(int requestId, FileAttachmentType fileType, CancellationToken cancellationToken);
    }
}
