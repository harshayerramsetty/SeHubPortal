using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class ClockEventsOfDayViewModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Total { get; set; }
    }
}