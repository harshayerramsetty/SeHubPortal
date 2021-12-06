using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class PopulateProductionReport
    {
        public List<tbl_treadtracker_barcode> barcodeInfo { get; set; }
        public List<tbl_treadtracker_workorder> workOrderInfo { get; set; }
        public List<tbl_customer_list> customerInfo { get; set; }
        
    }
}