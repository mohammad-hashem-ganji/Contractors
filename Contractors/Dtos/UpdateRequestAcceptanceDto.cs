namespace Contractors.Dtos
{
    /// <summary>
    /// مدل برای تغییر وضعیت درخواست تأیید شده توسط مشتری.
    /// </summary>
    public class UpdateRequestAcceptanceDto
    {
        /// <summary>
        /// شناسه درخواست که باید تغییر وضعیت دهد.
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// وضعیت تأیید درخواست. اگر مقدار true باشد، درخواست تأیید شده است.
        /// </summary>
        public bool IsAccepted { get; set; }
    }
}
