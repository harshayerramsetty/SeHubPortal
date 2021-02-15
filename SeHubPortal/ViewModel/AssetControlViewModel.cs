using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class AssetControlViewModel
    {       
        public List<tbl_vehicle_info> VehicalInfoList { get; set; }
        public string MatchedLoc { get; set; }
    }
}