using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class ViewWorkOrderModel
    {
        public List<tbl_treadtracker_barcode> barcodeInformation { get; set; }
        public tbl_customer_list custInfo { get; set; }
        public tbl_treadtracker_workorder workOrderInfo { get; set; }
        public string WorkOrderNumber { get; set; }
        public string Barcode { get; set; }
    }
}