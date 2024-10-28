using Contractors.Entites;

namespace Contractors.Dtos
{
    public class AddRequestStatusDto 
    {
        public int RequestId { get; set; }
        public RequestStatusEnum Status { get; set; }
        public int CreatedBy { get; set; }

    }

}
