
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class Project : BaseAuditableEntity
    { 
        public int Id { get; set; }
        public int ContractorBidId { get; set; }
        public BidOfContractor? ContractorBid { get; set; }
        public int ContractorId { get; set; }
        public ApplicationUser Contractor { get; set; }
        public int ClientId { get; set; }
        public ApplicationUser Client { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ICollection<ProjectStatus>? ProjectStatuses { get; set; }

    }
}
