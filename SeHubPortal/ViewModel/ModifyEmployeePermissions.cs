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
        public bool monitorEmployee { get; set; }
    }
}