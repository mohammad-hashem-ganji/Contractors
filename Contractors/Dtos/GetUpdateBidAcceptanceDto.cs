using System.ComponentModel.DataAnnotations;

namespace Contractors.Dtos
{
    /// <summary>
    /// مدل برای تأیید پیشنهاد از طرف مشتری.
    /// </summary>
    public class GetUpdateBidAcceptanceDto
    {
        /// <summary>
        /// شناسه پیشنهاد که باید تأیید شود.
        /// </summary>
        [Required(ErrorMessage = "شناسه پیشنهاد الزامی است.")]
        public int Id { get; set; }

        /// <summary>
        /// وضعیت تأیید پیشنهاد. اگر مقدار true باشد، پیشنهاد تأیید شده است.
        /// </summary>
        [Required(ErrorMessage = "وضعیت تأیید الزامی است.")]
        public bool IsAccepted { get; set; }
    }
}
