using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class AttendanceModel
    {
        public List<EmployeeAttendanceListModel> employeeList { get; set; }
        public List<EmployeeAttendanceListModel> employeeListChangeLocation { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public string SelectedEmployeeId { get; set; }
        public int AccessLevel { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
    }
}