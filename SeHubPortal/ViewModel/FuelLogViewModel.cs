using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;


namespace SeHubPortal.ViewModel
{
    public class FuelLogViewModel
    {
        public List<tbl_vehicle_info> vehicleInfoList { get; set; }
        public List<tbl_fuel_log_fleet> fuelLogList { get; set; }
        public string selectedVIN { get; set; }
        public tbl_vehicle_info SelectedVehicleInfo { get; set; }
        public tbl_fuel_log_fleet fuelLogTableValues { get; set; }
        public tbl_fuel_log_fleet editFuelLogTableValues { get; set; }
        public int fuel_log_access { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public List<SelectListItem> MatchedVehicals { get; set; }
        public string MatchedVehicle { get; set; }
        public string deleteTransactionNumber { get; set; }
        public List<SelectListItem> ChargeAccounts { get; set; }
    }
}