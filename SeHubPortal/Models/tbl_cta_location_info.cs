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
    
    public partial class tbl_cta_location_info
    {
        public string loc_id { get; set; }
        public string location_desc { get; set; }
        public string cta_street1 { get; set; }
        public string cta_street2 { get; set; }
        public string cta_city { get; set; }
        public string cta_province { get; set; }
        public string cta_postal_code { get; set; }
        public string cta_country { get; set; }
        public string nonsig_num { get; set; }
        public string cta_phone { get; set; }
        public string cta_fax { get; set; }
        public string cta_24_hour_phone { get; set; }
        public Nullable<int> loc_status { get; set; }
        public Nullable<int> tread_tracker_access { get; set; }
        public string service_email { get; set; }
        public string management_email { get; set; }
        public string googleMap { get; set; }
    }
}
