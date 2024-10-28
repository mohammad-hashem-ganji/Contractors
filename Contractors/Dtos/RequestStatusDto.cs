using Contractors.Dtos;
using Contractors.Entites;

namespace Contractors.Dtos
{
    public class RequestStatusDto : BaseAddAuditableDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public RequestStatusEnum? Status { get; set; }

    }
}
