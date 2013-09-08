using Exrate.Infrastructure;

namespace Exrate.Models
{
    public class RateForm
    {
        public int RecCount { get; set; }
        public string Currency { get; set; }
        public Enums.Cities City { get; set; }
        public int SortBy { get; set; }
    }
}