using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class HarpoonAttendanceRecordViewModel
    {
        public int auto_emp_id { get; set; }
        public string ful_name { get; set; }
        public string event_id { get; set; }
        public DateTime time_stamp { get; set; }
        public string loc_id { get; set; }
        public string job_ids { get; set; }
        public List<tbl_harpoon_job_log> jobs { get; set; }
    }
}