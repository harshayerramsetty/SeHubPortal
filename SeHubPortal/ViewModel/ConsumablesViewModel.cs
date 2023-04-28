using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class ConsumablesViewModel
    {
        public string type { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public double? ftConsumed { get; set; }
        public double? lbPerFt { get; set; }
        public double? lbsConsumed { get; set; }
    }
}