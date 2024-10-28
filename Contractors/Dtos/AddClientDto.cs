using Contractors.Entites;

namespace Contractors.Dtos
{
    public class AddClientDto 
    {

        public string? PostalCode { get; set; }
        public string? LicensePlate { get; set; }
        public string? address { get; set; }
        public string? MainSection { get; set; }
        public string? SubSection { get; set; }
    }
}
