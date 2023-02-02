using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{ 
    public class PlantViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<KeyValuePair<string, bool>> workStationResults { get; set; }
        public List<tbl_treadTracker_workStations> workStations { get; set; }
        public List<tbl_treadTracker_workStations> activeWorkStations { get; set; }
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
        public int cap_count { get; set; }

        public tbl_treadtracker_barcode barcodeInfo { get; set; }
        public tbl_treadtracker_workorder workOrderInfo { get; set; }
        
        public List<tbl_source_RARcodes> FailureCodes { get; set; }
        public List<tbl_source_retread_tread> TreadList { get; set; }
        public List<tbl_treadtracker_casing_sizes> SizeList { get; set; }
        public List<tbl_source_commercial_tire_manufacturers> BrandList { get; set; }
        public List<tbl_cta_location_info> LocationList { get; set; }
        public List<tbl_source_treadtracker_cap_count> CapCountList { get; set; }

        public List<tbl_treadTracker_chamber_senser> chamberSensors { get; set; }

        public List<tbl_source_treadtracker_consumables_nonTreadRubber> nonTreadRubber { get; set; }
        public List<tbl_source_treadtracker_consumables_TreadRubber> TreadRubber { get; set; }

        public List<SelectListItem> TreadsConsumables { get; set; }
        public string  TreadConsumables { get; set; }

        public List<SelectListItem> WidthsConsumables { get; set; }
        public float WidthConsumables { get; set; }

        public List<SelectListItem> PartnumbersConsumables { get; set; }
        public float PartnumberConsumables { get; set; }

        public string customerName { get; set; }

        public string buffer_spec { get; set; }
        public int? AgeLimit { get; set; }
        public bool? BrandRequirements { get; set; }

    }
}