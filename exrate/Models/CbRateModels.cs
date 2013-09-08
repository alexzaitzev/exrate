using System.Collections.Generic;
using System.Xml.Serialization;

namespace Exrate.Models
{
    [XmlRoot("ValCurs")]
    public class CbRateListModel
    {
        public CbRateListModel() { Items = new List<CbRateModel>(); }
        [XmlElement("Record")]
        public List<CbRateModel> Items { get; set; }
    }

    public class CbRateModel
    {
        public string Value { get; set; }
    }

    public class CbRateViewModel
    {
        public string Currency { get; set; }
        public decimal Value { get; set; }
        public decimal Diff { get; set; }
    }
}