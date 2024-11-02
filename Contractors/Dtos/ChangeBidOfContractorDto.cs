namespace Contractors.Dtos
{
    /// <summary>
    /// مدل داده‌ای برای به‌روزرسانی پیشنهاد پیمانکار.
    /// </summary>
    public class ChangeBidOfContractorDto
    {
        private int? _suggestedFee;

        /// <summary>
        /// شناسه پیشنهاد که باید به‌روزرسانی شود.
        /// </summary>
        public int BidId { get; set; }

        /// <summary>
        /// هزینه پیشنهادی که باید به‌روزرسانی شود.
        /// مقدار باید بزرگ‌تر از صفر باشد.
        /// </summary>
        /// <exception cref="ArgumentException">در صورتی که هزینه پیشنهادی کمتر یا مساوی صفر باشد.</exception>
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
    }

}
