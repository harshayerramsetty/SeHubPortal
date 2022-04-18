using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class PlantViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<KeyValuePair<string, bool>> workStationResults { get; set; }
        public List<tbl_treadTracker_workStations> workStations { get; set; }
        public string stationBarcode { get; set; }
        public string station { get; set; }
        public string Barcode { get; set; }
        public string paneType { get; set; }
        public byte[] icon { get; set; }


        public int status { get; set; }
        public int tread { get; set; }
        public int size { get; set; }
        public int brand { get; set; }
        public int shipto { get; set; }
        public int consumables { get; set; }
        public int reprint { get; set; }

        public tbl_treadtracker_barcode barcodeInfo { get; set; }
        public tbl_treadtracker_workorder workOrderInfo { get; set; }
        public string customerName { get; set; }
        public List<tbl_source_RARcodes> FailureCodes { get; set; }
        public List<tbl_source_retread_tread> TreadList { get; set; }
        public List<tbl_treadtracker_casing_sizes> SizeList { get; set; }
        public List<tbl_source_commercial_tire_manufacturers> BrandList { get; set; }
        public List<tbl_cta_location_info> LocationList { get; set; }
    }
}