using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Contractors.Dtos
{
    public class AddContractorDto
    {
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? LandlineNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? FaxNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string NCode { get; set; }
        public string PhoneNumber { get; set; }


    }
}
