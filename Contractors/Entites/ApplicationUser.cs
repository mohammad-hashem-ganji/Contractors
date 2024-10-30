
using Microsoft.AspNetCore.Identity;

namespace Contractors.Entites
{
    public class ApplicationUser : IdentityUser<int>
    {
        public Contractor? Contractor { get; set; }
        public Client? Client { get; set; }
        public List<LastLoginHistory>? LastLoginHistories{ get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<Project> ContractorProjects { get; set; }
        public ICollection<Project> ClientProjects{ get; set; }

    }
}
