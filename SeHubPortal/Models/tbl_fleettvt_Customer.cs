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
    
    public partial class tbl_fleettvt_Customer
    {
        public string customer_number { get; set; }
        public string reporting_contact { get; set; }
        public string reporting_email { get; set; }
        public string reporting_frequency { get; set; }
        public Nullable<int> mileage_required { get; set; }
        public string fleet_size { get; set; }
        public byte[] logo { get; set; }
        public Nullable<double> pull_point_1 { get; set; }
        public Nullable<double> pull_point_2 { get; set; }
        public Nullable<double> pull_point_3 { get; set; }
        public Nullable<int> exp_date_required { get; set; }
    }
}
