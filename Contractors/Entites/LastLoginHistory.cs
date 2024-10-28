

namespace Contractors.Entites
{
    public class LastLoginHistory : BaseAuditableEntity
    {
        public int Id { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
