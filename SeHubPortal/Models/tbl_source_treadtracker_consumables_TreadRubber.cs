//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SeHubPortal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_source_treadtracker_consumables_TreadRubber
    {
        public string part_number { get; set; }
        public string supplier { get; set; }
        public string description { get; set; }
        public string width { get; set; }
        public string tread { get; set; }
        public string short_description { get; set; }
        public Nullable<int> tread_depth { get; set; }
        public Nullable<bool> active { get; set; }
        public Nullable<double> weight_per_foot { get; set; }
        public string compatible_widths { get; set; }
    }
}
