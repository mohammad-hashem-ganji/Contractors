using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class Client : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string? NCcode { get; set; }
        public string? MobileNubmer { get; set; }
        public string? PostalCode { get; set; }
        public string? LicensePlate { get; set; }
        public string? address { get; set; }
        public string? MainSection { get; set; }
        public string? SubSection { get; set; }
        public int ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public ICollection<Request>? Requests { get; set; }

    }
}
