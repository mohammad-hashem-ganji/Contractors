
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class ProjectStatus : BaseAuditableEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public ProjectStatusEnum? Status { get; set; }

    }
}
