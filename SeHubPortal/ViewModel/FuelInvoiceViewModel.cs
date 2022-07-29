using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class FuelInvoiceViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }

        public List<tbl_fuel_log_invoiced> fuelLogInvoicedList { get; set; }
        public tbl_fuel_log_invoiced fuelLogInvoiceTableValues { get; set; }
        public tbl_fuel_log_invoiced EditfuelLogInvoiceTableValues { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public string MatchedLocation { get; set; }
        public string deleteTransactionNumber { get; set; }

    }
}