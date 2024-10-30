using Contractors.Entites;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Contractors.Dtos
{
    public class RequestForShowingDetailsToContractorDto
    {
        public int RequestId { get; set; }
        public string? RequestNumber { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? RegionTitle { get; set; }
        public RequestStatusEnum? LastStatus { get; set; }
    }
}
