namespace Exrate.Models
{
    public class StatViewModel : RateViewModel
    {
        public string[][] BuyChart { get; set; }
        public string[][] SellChart { get; set; }
    }
}