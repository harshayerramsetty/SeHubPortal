using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class AddNewVehicleViewModel
    {
        public tbl_vehicle_info VehicleInfo { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public string MatchedEmployeeID { get; set; }
        public string MatchedEmployeeName { get; set; }
        
    }
}