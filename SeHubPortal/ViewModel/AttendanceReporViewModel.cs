using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class AttendanceReportViewModel
    {
        public List<SelectListItem> MatchedLocs { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        public List<tbl_harpoon_locations> LocationsList { get; set; }
        public string MatchedLocID { get; set; }
        public string SelectedEmpID { get; set; }
        public tbl_harpoon_clients client_info { get; set; }
        public string AccessType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool ShowJob { get; set; }
        public List<HarpoonAttendanceRecordViewModel> AttendanceList { get; set; }
        public List<tbl_harpoon_employee> emp_list { get; set; }
        public List<HarpoonAttendanceReportTimeDuration> records { get; set; }
    }
}