using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class TreadTrackerDashboard
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<tbl_treadtracker_inventory_casing> CasingInventory { get; set; }
        public List<tbl_customer_list> CustList { get; set; }

    }
}