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
    
    public partial class Currency
    {
        public Currency()
        {
            this.Rate = new HashSet<Rate>();
        }
    
        public string ISO { get; set; }
        public short Code { get; set; }
        public string NameENG { get; set; }
        public string NameRUS { get; set; }
    
        public virtual ICollection<Rate> Rate { get; set; }
    }
}
