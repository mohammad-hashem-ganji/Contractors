using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public enum RequestStatusEnum
    {
        Pending = 0,        // The request is pending and has not been processed yet
        RequestApprovedByClient = 1,       // The request has been approved
        Rejected = 2,       // The request has been rejected
        RequestRejectedByClient = 3,
        RequestRejectedByContractor = 4,
        RequestTenderFinished =5,
        TimeForCheckingBidForClientExpired =6,
        RequestIsInTenderphase = 7,

    }
}
