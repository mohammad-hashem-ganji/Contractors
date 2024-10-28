using Contractors.Entites;

namespace Contractors.Dtos
{
    /// <summary>
    /// مدل داده‌ای برای به‌روزرسانی پیشنهاد پیمانکار.
    /// </summary>
    public class UpdateBidOfContractorDto 
    {
        /// <summary>
        /// شناسه پیشنهاد پیمانکار.
        /// </summary>
        public int BidId { get; set; }

        /// <summary>
        /// مبلغ پیشنهادی جدید پیمانکار. می‌تواند مقدار null داشته باشد.
        /// </summary>
        public int? SuggestedFee { get; set; }

        /// <summary>
        /// آیا پیمانکار امکان تغییر پیشنهاد را دارد.
        /// </summary>
        public bool? CanChangeBid { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? ExpireAt { get; set; }
    }

}
