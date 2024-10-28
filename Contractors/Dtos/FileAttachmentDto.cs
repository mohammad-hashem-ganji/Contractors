using Contractors.Entites;

namespace Contractors.Dtos
{
    public class FileAttachmentDto 
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public int RequestId { get; set; }

    }
}