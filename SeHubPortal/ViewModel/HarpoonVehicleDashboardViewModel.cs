using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class HarpoonVehicleDashboardViewModel
    {
        public List<tbl_harpoon_vehicle> Vehicles { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public int Location { get; set; }
        public tbl_harpoon_vehicle NewVehicle { get; set; }
        public List<SelectListItem> LocationsWoithoutAll { get; set; }
        public tbl_harpoon_clients client_info { get; set; }
    }
}