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
    
    public partial class tbl_harpoon_settings
    {
        public string client_id { get; set; }
        public Nullable<int> AutoClockOut_OnOff { get; set; }
        public Nullable<double> AutoClockOut_Adjustment { get; set; }
        public Nullable<int> ManualCode_OnOff { get; set; }
        public Nullable<int> auto_generate_loc_id { get; set; }
        public Nullable<int> LocIDinListBox_OnOff { get; set; }
        public Nullable<int> DefaultEmpIDlength_OnOff { get; set; }
        public Nullable<int> EmpID_Length { get; set; }
        public Nullable<int> MasterLoc_OnOff { get; set; }
        public string MasterLoc_id { get; set; }
        public Nullable<int> job_id_clocking { get; set; }
        public Nullable<System.DateTime> timeSheet_start { get; set; }
        public string timeSheet_type { get; set; }
        public Nullable<int> timeSheet_delay { get; set; }
    }
}
