namespace Contractors.Entites
{
    public class BaseAddAuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
