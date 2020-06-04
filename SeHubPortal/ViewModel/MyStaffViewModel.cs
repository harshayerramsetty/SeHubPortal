using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class MyStaffViewModel
    {
        public List<tbl_employee> employeeDetails { get; set; }
        public List<SelectListItem> MatchedStaffLocs { get; set; }
        public string MatchedStaffLocID { get; set; }
        public int EmployeePermissions { get; set; }
        public tbl_employee NewEmployee { get; set; }
        public tbl_employee_personal NewEmployeePersonal { get; set; }
        public bool active_status { get; set; }

    }
}