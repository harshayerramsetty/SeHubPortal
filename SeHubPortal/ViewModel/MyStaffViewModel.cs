using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class MyStaffViewModel
    {
        public List<tbl_employee> employeeDetails { get; set; }
        public List<SelectListItem> MatchedStaffLocs { get; set; }
        public string MatchedStaffLocID { get; set; }

    }
}