using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class AddNewEmployeeViewModel
    {
        public tbl_employee StaffWorkDetails { get; set; }
        public tbl_employee_personal StaffPersonalDetails { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
    }
}