namespace Contractors.Dtos
{
    public class UpdateLastLoginHistoryDto : BaseUpdateAuditableDto
    {
        public int Id { get; set; }
        public DateTime? Last2FaAuthentication { get; set; }
        public int ApplicationUserId { get; set; }

    }
}
