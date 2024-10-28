using Contractors.Entites;

namespace Contractors.Dtos
{
    public class ContractorDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? LandlineNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? FaxNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int ApplicationUserId { get; set; }
        //public ApplicationUser? ApplicationUser { get; set; }
        public ICollection<BidOfContractorDto>? BidOfContractors { get; set; }

    }
}
