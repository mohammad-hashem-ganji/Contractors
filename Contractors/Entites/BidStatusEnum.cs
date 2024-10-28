using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public enum BidStatusEnum
    {
        BidApprovedByClient = 0,
        TimeForCheckingBidForClientExpired = 1,
        BidApprovedByContractor = 2,
        TimeForCheckingBidForContractorExpired = 3,
        BidRejectedByContractor = 4,
        ReviewBidByClientPhase = 5
    }
}
