namespace Contractors.Dtos
{
    /// <summary>
    /// کلاس DTO برای به‌روزرسانی وضعیت پذیرش پیشنهاد.
    /// </summary>
    public class UpdateBidAcceptanceDto
    {
        /// <summary>
        /// شناسه پیشنهاد.
        /// </summary>
        public int BidId { get; set; }

        /// <summary>
        /// نشان‌دهنده پذیرش یا رد پیشنهاد.
        /// </summary>
        public bool IsAccepted { get; set; }
    }

}
