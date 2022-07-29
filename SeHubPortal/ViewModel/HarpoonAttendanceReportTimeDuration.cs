using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class HarpoonAttendanceReportTimeDuration
    {
        public string EmpID { get; set; }
        public string Name { get; set; }
        public string date { get; set; }
        public string clockIN { get; set; }
        public string clockOUT { get; set; }
        public string duration { get; set; }

        public string job_id { get; set; }
        public string count { get; set; }
        public string total { get; set; }
        public string average { get; set; }
    }
}