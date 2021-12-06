using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class NewWorkOrder
    {
        public tbl_customer_list customerInfo { get; set; }
        public string workOrder { get; set; }
        public string OrderDate { get; set; }
        public string SubmittedEmployee { get; set; }
        public DateTime InspectionDate { get; set; }

        public tbl_sehub_access SehubAccess { get; set; }

    }
}