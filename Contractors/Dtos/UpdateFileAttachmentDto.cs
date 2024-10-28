namespace Contractors.Dtos
{
    public class UpdateFileAttachmentDto : BaseUpdateAuditableDto
    {
        public int Id { get; set; }
        public bool? IsAcceptedByClient { get; set; }
    }
}
