using Contractors.Entites;

namespace Contractors.Dtos
{
    public class GetRequestDto
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public DateTime? ExpireAt { get; set; }
        public int ClientId { get; set; }
        public bool? IsAcceptedByClient { get; set; }
        public string? RegionTitle { get; set; }
        public RequestStatusEnum? LastStatus { get; set; }
    }
}
