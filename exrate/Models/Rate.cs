//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Exrate.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Rate
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Bank { get; set; }
        public int CityId { get; set; }
        public System.DateTime Date { get; set; }
        public decimal Buy { get; set; }
        public decimal Sell { get; set; }
    
        public virtual Bank Bank1 { get; set; }
        public virtual City City { get; set; }
        public virtual Currency Currency { get; set; }
    }
}