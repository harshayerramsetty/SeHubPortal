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
    
    public partial class tbl_treadTracker_workStations
    {
        public string station { get; set; }
        public string processID { get; set; }
        public Nullable<int> active { get; set; }
        public Nullable<int> barcode_scan_required { get; set; }
        public string barcode_id { get; set; }
        public Nullable<int> allow_print { get; set; }
        public byte[] icon { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> tread { get; set; }
        public Nullable<int> size { get; set; }
        public Nullable<int> brand { get; set; }
        public Nullable<int> ship_to { get; set; }
        public Nullable<int> consumables { get; set; }
        public Nullable<int> reprint { get; set; }
        public Nullable<int> cap_count { get; set; }
    }
}
