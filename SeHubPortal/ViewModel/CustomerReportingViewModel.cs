using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class CustomerReportingViewModel
    {
        public List<tbl_customer_reporting_viewmodel> customerReportingDetails { get; set; }
        public List<SelectListItem> Customers { get; set; }
        public string Custname { get; set; }
        public tbl_customer_list customerDetails { get; set; }

        public tbl_customer_reporting_viewmodel AddCRM { get; set; }
    }
}