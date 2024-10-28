using System.ComponentModel.DataAnnotations;

namespace Contractors.Dtos
{
    public class GetUpdateBidAcceptanceDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool IsAccepted { get; set; }
    }
}
