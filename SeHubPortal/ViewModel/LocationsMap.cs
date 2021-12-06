using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class LocationsMap
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public tbl_cta_location_info locdesc { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public string SelectedLocationId { get; set; }
    }
}