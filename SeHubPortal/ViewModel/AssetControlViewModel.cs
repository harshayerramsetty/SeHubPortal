using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class AssetControlViewModel
    {       
        public List<tbl_vehicle_info> VehicalInfoList { get; set; }
        public string MatchedLoc { get; set; }
        public List<SelectListItem> LocationList { get; set; }
    }
}