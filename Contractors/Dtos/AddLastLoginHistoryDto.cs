namespace Contractors.Dtos
{
    public class AddLastLoginHistoryDto : BaseAddAuditableDto
    {
        public DateTime? LastLoginTime { get; set; }
        public DateTime Last2FaAuthentication { get; set; }
        public int ApplicationUserId { get; set; }
    }
}
