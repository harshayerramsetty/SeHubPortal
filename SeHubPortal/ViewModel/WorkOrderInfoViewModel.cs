using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class WorkOrderInfoViewModel
    {
        public string WorkOrderNumber { get; set; }
        public int openBarcodes { get; set; }
        public int closedBarcodes { get; set; }
        public int failedBarcodes { get; set; }
        public string customer_number { get; set; }
        public string employee_Id { get; set; }
        public string loc_Id { get; set; }
        public Nullable<System.DateTime> creation_date { get; set; }
        public string comments { get; set; }
    }
}