namespace Contractors.Dtos
{
    public class RequestOfClientDto
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public DateTime? ExpireAt { get; set; }
        public int ClientId { get; set; }
        public int RegionId { get; set; }
        public bool IsTenderOver { get; set; }
        public bool IsActive { get; set; }
        public bool IsAcceptedByClient { get; set; }
        public string? Region { get; set; }
        public ICollection<RequestStatusDto> RequestStatuses { get; set; }
    }
}
