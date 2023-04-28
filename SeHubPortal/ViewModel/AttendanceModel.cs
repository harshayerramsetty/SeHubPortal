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
        public bool multipleLocation { get; set; }
        public string AccessType { get; set; }
        public TimeClockEvent CreateEvent  { get; set; }
        public string openJobID { get; set; }
        public int openJobID_autoEmpID { get; set; }
        public bool job_id_clocking { get; set; }

        public bool AdminClockingEditTime { get; set; }
        public tbl_harpoon_clients client_info { get; set; }

        public string userProfile { get; set; }

        public int addAdminEventemp { get; set; }
        public DateTime addAdminEventDateTime { get; set; }
        public string addAdminEventEvent { get; set; }
}
}