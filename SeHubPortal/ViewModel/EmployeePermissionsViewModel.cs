using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class EmployeePermissionsViewModel
    {
       
        public List<Models.tbl_employee> EmployeesList { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public int userManagementAccessLevel { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
    }
}