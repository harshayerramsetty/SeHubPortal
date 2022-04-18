using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class TimeClockEvent
    {
        public int auto_loc_id { get; set; }
        public string location_id { get; set; }
        public int auto_emp_id { get; set; }
        public string event_id { get; set; }
        public string time_stamp { get; set; }
        public string client_id { get; set; }
        public string comments { get; set; }
    }
}