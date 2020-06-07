using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class FuelLogViewModel
    {
        public List<tbl_vehicle_info> vehicleInfoList { get; set; }
        public List<tbl_fuel_log> fuelLogList { get; set; }
        public string selectedVIN { get; set; }
        public tbl_vehicle_info SelectedVehicleInfo { get; set; }
        public tbl_fuel_log fuelLogTableValues { get; set; }
        public int fuel_log_access { get; set; }
    }
}