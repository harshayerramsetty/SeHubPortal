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
    
    public partial class tbl_vehicle_info
    {
        public string VIN { get; set; }
        public string vehicle_short_id { get; set; }
        public string vehicle_long_id { get; set; }
        public string vehicle_plate { get; set; }
        public string loc_id { get; set; }
        public Nullable<int> assigned_to { get; set; }
        public Nullable<int> vehicle_year { get; set; }
        public string vehicle_manufacturer { get; set; }
        public string vehicle_model { get; set; }
        public Nullable<double> current_milage { get; set; }
        public Nullable<double> efficiency_price { get; set; }
        public Nullable<double> efficiency_liter { get; set; }
        public Nullable<System.DateTime> inspection_due_date { get; set; }
        public byte[] vehicle_image { get; set; }
        public Nullable<int> vehicle_status { get; set; }
    }
}
