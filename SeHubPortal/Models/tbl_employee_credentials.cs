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
    
    public partial class tbl_employee_credentials
    {
        public int employee_id { get; set; }
        public string password { get; set; }
        public Nullable<bool> permission { get; set; }
        public string user_name { get; set; }
        public string password365 { get; set; }
        public Nullable<bool> management_permissions { get; set; }
        public Nullable<bool> administrative_permissions { get; set; }
        public string additional_recipient { get; set; }
    }
}