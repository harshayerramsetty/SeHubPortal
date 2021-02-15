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
        public List<tbl_target_accounts> targetAccountDetails { get; set; }
        public List<tbl_dispute_resolution> DisputeResolutionDetails { get; set; }
        public tbl_target_accounts AddTA { get; set; }
        public tbl_dispute_resolution AddDR { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
    }
}