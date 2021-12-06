using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class FailurAnalysisViewmodel
    {
        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public string Location { get; set; }
        public string Customer { get; set; }
        public List<tbl_source_RARcodes> FailureCodes { get; set; }
        public List<tbl_treadtracker_barcode> barcodeInfo { get; set; }
        public List<tbl_treadtracker_workorder> workOrderInfo { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
    }
}