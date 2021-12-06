using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel 
{
    public class FleetTVT
    {
        public List<SelectListItem> CustomerList { get; set; }
        public string Customer { get; set; }

        public List<SelectListItem> ConfigurationsListTractor { get; set; }
        public List<SelectListItem> ConfigurationsListTrailer { get; set; }
        public List<SelectListItem> ConfigurationsList { get; set; }
         
        public List<SelectListItem> CustList { get; set; }

        public string Type { get; set; }
        public string Configuration { get; set; }

        public string URL { get; set; }
        public string UnitURI { get; set; }
        public string Name { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<FleetTVTDashboardCustomers> customer_table { get; set; }
        public fleetTVTCustomerViewModel AddCustomer { get; set; }
    }
}