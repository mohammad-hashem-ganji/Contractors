﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class RequestStatus : BaseAuditableEntity
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }
        public int StatusId { get; set; }

        // Computed property to expose Status as an enum
        public RequestStatusEnum? Status
        {
            get => (RequestStatusEnum)StatusId;
            set => StatusId = (int)value;
        }

    }
}
