using System.ComponentModel.DataAnnotations;

namespace Contractors.Dtos
{
    public class GetVerificationCodeDto
    {
        /// <summary>
        /// کد تأیید ارسال شده به کاربر.
        /// </summary>
        [Required(ErrorMessage = "کد تأیید الزامی است.")]
        public string VerificationCode { get; set; }

        /// <summary>
        /// کد ملی کاربر.
        /// </summary>
        [Required(ErrorMessage = "کد ملی الزامی است.")]
        public string Nationalcode { get; set; }

        /// <summary>
        /// شماره تلفن کاربر.
        /// </summary>
        [Required(ErrorMessage = "شماره تلفن الزامی است.")]
        public string PhoneNumber { get; set; }
    }
}
