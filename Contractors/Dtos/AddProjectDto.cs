using Contractors.Entites;

namespace Contractors.Dtos
{
    public class AddProjectDto : BaseAddAuditableDto
    {
        public int Id { get; set; }
        public int ContractorBidId { get; set; }

    }
}
