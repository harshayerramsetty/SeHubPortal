using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;


namespace SeHubPortal.ViewModel
{
    public class FleetTvtEditAccount
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> CustomerList { get; set; }
    }
}