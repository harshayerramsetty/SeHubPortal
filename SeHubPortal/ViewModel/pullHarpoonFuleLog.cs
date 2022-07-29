using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class pullHarpoonFuleLog
    {
        public int selectedVehicle { get; set; }
        public List<tbl_harpoon_fuel_log> fuelLog { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }
}