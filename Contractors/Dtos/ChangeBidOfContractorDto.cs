namespace Contractors.Dtos
{
    public class ChangeBidOfContractorDto
    {
        private int? _suggestedFee;
        public int BidId { get; set; }
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
