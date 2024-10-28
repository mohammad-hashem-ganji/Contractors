using Microsoft.AspNetCore.Identity;

namespace Contractors.Dtos
{
    public class RegisterResultDto
    {
        public int RegisteredUserId { get; set; }
        public IdentityResult IdentityResult { get; set; }
        public List<IdentityError> IdentityError { get; set; }
    }
}
