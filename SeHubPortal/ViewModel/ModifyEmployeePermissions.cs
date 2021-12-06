using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class ModifyEmployeePermissions
    {
        public tbl_employee_credentials EmployeeCredentials { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public tbl_employee empDetails { get; set; }
        public bool appAccess { get; set; }
        public bool loc_001 { get; set; }
        public bool loc_002 { get; set; }
        public bool loc_003 { get; set; }
        public bool loc_004 { get; set; }
        public bool loc_005 { get; set; }
        public bool loc_007 { get; set; }
        public bool loc_009 { get; set; }
        public bool loc_010 { get; set; }
        public bool loc_011 { get; set; }
        public bool loc_347 { get; set; }
        public bool loc_AHO { get; set; }
    }
}