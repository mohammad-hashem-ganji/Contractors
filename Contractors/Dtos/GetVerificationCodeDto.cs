using System.ComponentModel.DataAnnotations;

namespace Contractors.Dtos
{
    /// <summary>
    /// داده‌های ورودی برای تأیید کد احراز هویت دو مرحله‌ای.
    /// </summary>
    public class GetVerificationCodeDto
    {
        /// <summary>
        /// کد تأیید ارسال شده به کاربر.
        /// </summary>
        [Required]
        public string VerificationCode { get; set; }

        /// <summary>
        /// کد ملی کاربر.
        /// </summary>
        [Required]
        public string Nationalcode { get; set; }

        /// <summary>
        /// شماره تلفن کاربر.
        /// </summary>
        [Required]
        public string PhoneNumber { get; set; }
    }
}
