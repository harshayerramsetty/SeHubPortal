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
    
    public partial class tbl_dispute_resolution
    {
        public int invoice_number { get; set; }
        public Nullable<System.DateTime> date_dispute { get; set; }
        public Nullable<int> mileage_dispute { get; set; }
        public Nullable<System.DateTime> date_service { get; set; }
        public Nullable<int> mileage_service { get; set; }
        public Nullable<int> customer_number { get; set; }
        public string customer_name_NoAccount { get; set; }
        public Nullable<int> employee_id { get; set; }
        public string details { get; set; }
        public string recommandation { get; set; }
        public byte[] attachments { get; set; }
        public string final_outcome { get; set; }
        public Nullable<int> complete { get; set; }
    }
}
