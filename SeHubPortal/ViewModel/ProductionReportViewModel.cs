using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class ProductionReportViewModel
    {
        public List<SelectListItem> Customers { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public string addressSTREET1 { get; set; }
        public string addressSTREET2 { get; set; }
        public string addressCITY { get; set; }
        public string addressPROVINCE { get; set; }
        public string addressPOSTAL { get; set; }
        public string locPHONE { get; set; }
        public string locFAX { get; set; }
        public string locID { get; set; }
    }
}