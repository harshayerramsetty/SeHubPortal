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
    
    public partial class tbl_harpoon_timesheet
    {
        public int client_id { get; set; }
        public int auto_emp_id { get; set; }
        public string timesheet_id { get; set; }
        public System.DateTime date { get; set; }
        public string type { get; set; }
        public Nullable<double> value { get; set; }
    }
}
