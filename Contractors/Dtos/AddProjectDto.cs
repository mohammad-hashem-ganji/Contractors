using Contractors.Entites;

namespace Contractors.Dtos
{
    public class AddProjectDto : BaseAddAuditableDto
    {
        public int ContractorBidId { get; set; }
    }
}
