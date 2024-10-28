using Contractors.Entites;

namespace Contractors.Dtos
{
    /// <summary>
    /// مدل داده‌ای برای به‌روزرسانی پیشنهاد پیمانکار.
    /// </summary>
    public class UpdateBidOfContractorDto : BaseUpdateAuditableDto
    {
        /// <summary>
        /// شناسه پیشنهاد پیمانکار.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// مبلغ پیشنهادی جدید پیمانکار. می‌تواند مقدار null داشته باشد.
        /// </summary>
        public int? SuggestedFee { get; set; }

        /// <summary>
        /// آیا پیمانکار امکان تغییر پیشنهاد را دارد.
        /// </summary>
        public bool? CanChangeBid { get; set; }

        /// <summary>
        /// وضعیت حذف شدن پیشنهاد. اگر true باشد، پیشنهاد حذف شده است.
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// تاریخ انقضای پیشنهاد. می‌تواند null باشد.
        /// </summary>
        public DateTime? ExpireAt { get; set; }
    }

}
