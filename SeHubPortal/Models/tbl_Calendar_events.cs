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
    
    public partial class tbl_Calendar_events
    {
        public int event_ID { get; set; }
        public string subject { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
    }
}
