using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class Region : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int? ContractorSystemCode { get; set; } //******** datatype?
        public ICollection<Request>? Requests { get; set; }

    }
}
