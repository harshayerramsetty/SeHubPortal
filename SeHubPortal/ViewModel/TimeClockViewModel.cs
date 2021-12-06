using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class TimeClockViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<tbl_timeclock_devices> Devices { get; set; }
        public List<SelectListItem> locations { get; set; }
        public string loc { get; set; }
        public string serial { get; set; }
        public string serialDelete { get; set; }
    }
}