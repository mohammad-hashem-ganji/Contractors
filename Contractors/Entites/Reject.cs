namespace Contractors.Entites
{
    public class Reject: BaseAuditableEntity
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public Request Request { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public RejectReasonEnum? Reason { get; set; }
    }
}
