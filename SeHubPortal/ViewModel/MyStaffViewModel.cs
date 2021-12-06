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
        public List<tbl_employee_status> employeeStatusDetails { get; set; }
        public List<tbl_position_info> positionsTable { get; set; }
        public List<SelectListItem> MatchedStaffLocs { get; set; }
        public string MatchedStaffLocID { get; set; }
        public string CompensationType { get; set; }
        public int EmployeePermissions { get; set; }
        public tbl_employee NewEmployee { get; set; }
        public tbl_employee_personal NewEmployeePersonal { get; set; }
        public bool active_status { get; set; }
        public List<SelectListItem> Positions { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public tbl_employee_status empStatusInfo { get; set; }
        public tbl_employee_payroll_final PayrollInfo { get; set; }
        public string position { get; set; }
        public string full_name { get; set; }
        public string SelectedPayrollId { get; set; }
        public List<SelectListItem> PayrollIdList { get; set; }
    }
}