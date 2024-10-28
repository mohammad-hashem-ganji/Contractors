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
        InProgress = 3,     // The request is currently being processed
        Completed = 4,      // The request has been completed
        Cancelled = 5,      // The request has been cancelled
        OnHold = 6,         // The request is on hold
        RequestRejectedByClient = 7,
        RequestRejectedByContractor = 8,
        RequestTenderFinished =9,
        TimeForCheckingBidForClientExpired =10,

    }
}
