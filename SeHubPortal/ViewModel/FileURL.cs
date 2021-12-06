using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class FileURL
    {
        public List<KeyValuePair<string, string>> URLName { get; set; }
        public string Location_ID { get; set; }
        public string Pane { get; set; }
        public string RenameString { get; set; }
        public int Permission { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public List<EmployeeAttendanceListModel> employeeList { get; set; }
        public string SelectedEmployeeId { get; set; }
        public List<tbl_vacation_schedule> vacations { get; set; }
        public List<tbl_employee> employeeVacList { get; set; }
        public tbl_employee_status employeeStatus { get; set; }
        public tbl_vacation_schedule newLeave { get; set; }
        public tbl_vacation_schedule EditLeave { get; set; }
        public tbl_employee employee { get; set; }
        public int editVacationEmployee { get; set; }
         
        public string SortBy { get; set; }
        public string Year { get; set; }
    }
}