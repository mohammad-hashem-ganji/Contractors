using Contractors.Entites;

namespace Contractors.Dtos
{
    /// <summary>
    /// مدل داده‌ای برای افزودن پیشنهاد جدید توسط پیمانکار.
    /// </summary>
    public class AddBidOfContractorDto
    {
        private int? _suggestedFee;
        /// <summary>
        /// مبلغ پیشنهادی پیمانکار. می‌تواند مقدار null باشد.
        /// </summary>
        public int? SuggestedFee
        {
            get => _suggestedFee;
            set
            {
                if (value.HasValue && value <= 0)
                    throw new ArgumentException("عدد قرار داد باید بزرگ تر از 0 باشد.");
                _suggestedFee = value;
            }
        }

        /// <summary>
        /// شناسه درخواست که این پیشنهاد برای آن ارائه شده است.
        /// </summary>
        public int RequestId { get; set; }
    }

}
