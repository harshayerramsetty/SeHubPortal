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
    
    public partial class tbl_employee
    {
        public int employee_id { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string cta_email { get; set; }
        public string cta_cell { get; set; }
        public string cta_position { get; set; }
        public string loc_ID { get; set; }
        public string rfid_number { get; set; }
        public Nullable<int> sales_id { get; set; }
        public string full_name { get; set; }
        public string cta_direct_phone { get; set; }
        public Nullable<System.DateTime> Date_of_birth { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> pic_status { get; set; }
        public byte[] profile_pic { get; set; }
    }
}
