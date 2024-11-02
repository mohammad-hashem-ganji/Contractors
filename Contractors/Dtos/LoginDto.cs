using System.ComponentModel.DataAnnotations;

namespace Contractors.Dtos
{
    public class LoginDto
    {
        /// <summary>
        /// کد ملی کاربر.
        /// </summary>
        [Required(ErrorMessage = "کد ملی الزامی است.")]
        public string NationalCode { get; set; }

        /// <summary>
        /// شماره تلفن کاربر.
        /// </summary>
        [Required(ErrorMessage = "شماره تلفن الزامی است.")]
        public string PhoneNumber { get; set; }
    }

}
