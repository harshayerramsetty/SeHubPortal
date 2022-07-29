using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class SettingsViewModel
    {
        public List<tbl_Calendar_events> Calendar_Events { get; set; }
        public tbl_payroll_settings payroll_Settings { get; set; }
        public string ID { get; set; }
        public CallenderPostViewModel CalenderPost { get; set; }
        public string temp { get; set; }
        public tbl_Calendar_events AddCalendarEvents { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public double wheelRetail { get; set; }
        public double wheelNA { get; set; }
        public double freightRetail { get; set; }
        public double freightNA { get; set; }

        public string topBar_left { get; set; }
        public string topBar_right { get; set; }
    }
}