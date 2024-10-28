namespace Contractors.Dtos
{
    public class BaseAddAuditableDto
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
