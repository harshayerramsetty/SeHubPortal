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
    
    public partial class tbl_fuel_log
    {
        public string VIN { get; set; }
        public Nullable<int> employee_id { get; set; }
        public Nullable<System.DateTime> date_of_purchase { get; set; }
        public Nullable<int> odometer { get; set; }
        public Nullable<double> no_of_liters { get; set; }
        public Nullable<double> price_per_liter { get; set; }
        public string transaction_number { get; set; }
        public bool audit_status { get; set; }
        public string comments { get; set; }
        public string change_type { get; set; }
    }
}
