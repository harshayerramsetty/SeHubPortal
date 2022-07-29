using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class SettingsCustomers
    {

        public tbl_sehub_access SehubAccess { get; set; }
        public tbl_customer_list NewCustomer { get; set; }
        public bool treadTracker { get; set; }
        public bool treadTrackerEdit { get; set; }
        public bool fleetTVT { get; set; }
        public bool fleetTVTEdit { get; set; }
        public bool CRM { get; set; }
        public bool CRMedit { get; set; }
        public string Customeredit { get; set; }
        public List<tbl_cta_customers> CustomerList { get; set; }
        public List<tbl_customer_reporting_customers> CustomerListCRM { get; set; }
        public List<tbl_fleettvt_Customer> CustomerListFleetTVT { get; set; }

        
    }
}