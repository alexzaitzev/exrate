using System.Collections.Generic;

namespace Exrate.Models
{
    public class BankViewModel
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Website { get; set; }
        public List<CurrencyModel> Rates { get; set; }
        public List<AddressModel> Addresses { get; set; }
    }
}