namespace Exrate.Models
{
    public class RateViewModel
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string BankCode { get; set; }
        public string LastUpdDt { get; set; }
        public bool IsOutDated { get; set; }
        public decimal Buy { get; set; }
        public decimal Sell { get; set; }
        public decimal BuyDiff { get; set; }
        public decimal SellDiff { get; set; }
    }
}