namespace Contractors.Dtos
{
    public class BaseGetAuditaleDto
    {
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
