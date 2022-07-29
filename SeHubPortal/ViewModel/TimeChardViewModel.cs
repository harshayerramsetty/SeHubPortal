using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class TimeChardViewModel
    {
        public string eventID { get; set; }
        public DateTime timeStamp { get; set; }
        public string clientID { get; set; }
        public string locationID { get; set; }
        public string job_id { get; set; }
    }
}