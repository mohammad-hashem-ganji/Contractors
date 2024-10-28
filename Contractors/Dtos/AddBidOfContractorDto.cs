using Contractors.Entites;

namespace Contractors.Dtos
{
    /// <summary>
    /// مدل داده‌ای برای افزودن پیشنهاد جدید توسط پیمانکار.
    /// </summary>
    public class AddBidOfContractorDto
    {
        /// <summary>
        /// مبلغ پیشنهادی پیمانکار. می‌تواند مقدار null باشد.
        /// </summary>
        public int? SuggestedFee { get; set; }

        /// <summary>
        /// شناسه درخواست که این پیشنهاد برای آن ارائه شده است.
        /// </summary>
        public int RequestId { get; set; }
    }

}
