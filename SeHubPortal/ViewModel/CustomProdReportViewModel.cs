using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class CustomProdReportViewModel
    {
        public Nullable<int> line_number { get; set; }
        public string barcode { get; set; }
        public string serial_dot { get; set; }
        public string casing_size { get; set; }
        public string casing_brand { get; set; }
        public string retread_design { get; set; }
        public string ship_to_location { get; set; }
        public string retread_workorder { get; set; }
        public string customer_number { get; set; }
        public string cust_name { get; set; }
    }
}