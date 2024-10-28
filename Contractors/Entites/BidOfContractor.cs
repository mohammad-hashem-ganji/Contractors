


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class BidOfContractor : BaseAuditableEntity
    {
        public int Id { get; set; }
        public int? SuggestedFee { get; set; }
        public DateTime? ExpireAt { get; set; }
        public int ContractorId { get; set; }
        public Contractor? Contractor { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }
        public bool? IsAcceptedByClient { get; set; }
        public bool? CanChangeBid { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<BidStatus>? BidStatuses { get; set; }
        public Project? Project { get; set; }

    }
}
