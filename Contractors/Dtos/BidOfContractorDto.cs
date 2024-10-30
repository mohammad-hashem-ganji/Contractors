using Contractors.Entites;

namespace Contractors.Dtos
{
    public class BidOfContractorDto : BaseGetAuditaleDto
    {
        public int Id { get; set; }
        public int? SuggestedFee { get; set; }
        public int ContractorId { get; set; }
        public int RequestId { get; set; }
        public DateTime ExpireAt { get; set; }
        // add last status for bid

    }
}
