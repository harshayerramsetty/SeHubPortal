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
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public List<SelectListItem> Customers { get; set; }
        public string Custname { get; set; }
        public tbl_cta_customers customerDetails { get; set; }
        public tbl_customer_reporting_viewmodel AddCRM { get; set; }
        public List<targetAcountViewModel> targetAccountDetails { get; set; }
        public List<tbl_dispute_resolution> DisputeResolutionDetails { get; set; }
        public tbl_target_accounts AddTA { get; set; }
        public tbl_dispute_resolution AddDR { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public string username { get; set; }
        public List<SelectListItem> EmployeesList { get; set; }
        //public List<tbl_employee> emp_tbl { get; set; }
    }
}