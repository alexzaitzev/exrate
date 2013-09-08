using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exrate.Models
{
    public class RateUpdateModel
    {
        public string Bank { get; set; }
        public int CityId { get; set; }
        public DateTime Date { get; set; }
        public CurrencyModel USD { get; set; }
        public CurrencyModel EUR { get; set; }

        public bool IsUSDSet()
        {
            return USD != null;
        }

        public bool IsEURSet()
        {
            return EUR != null;
        }
    }
}