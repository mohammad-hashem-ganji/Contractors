using Contractors.Dtos;
using Contractors.Entites;

namespace Contractors.Dtos
{
    public class BidStatusDto : BaseGetAuditaleDto
    {
        public int BidOfContractorId { get; set; }
        public BidStatusEnum Status { get; set; }
        public int CreatedBy { get; set; }
    }
}
