using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class TreadTrackerDashboard
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<tbl_treadtracker_inventory_casing> CasingInventory { get; set; }
        public List<tbl_customer_list> CustList { get; set; }
        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> CustomersList { get; set; }
        public String Customer { get; set; }
        public string FieldSheetURL { get; set; }
        public tbl_tread_tracker_customers AddCustomer { get; set; }
        public List<SelectListItem> RetreadList { get; set; }


    }
}