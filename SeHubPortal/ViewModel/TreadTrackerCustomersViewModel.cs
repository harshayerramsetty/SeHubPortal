using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class TreadTrackerCustomersViewModel
    {
        public List<SelectListItem> CustomerList { get; set; }
        public string Customer { get; set; }
        public List<WorkOrderInfoViewModel> WorkOrdersForCustomer { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> LocationList { get; set; }
        public string Location { get; set; }
        public tbl_tread_tracker_customers customerDetails { get; set; }

        public string reportingContact { get; set; }
        public string reportingEmail { get; set; }
        public string reportingFrequency { get; set; }
        public List<string> CasingSizes { get; set; }
    }
}