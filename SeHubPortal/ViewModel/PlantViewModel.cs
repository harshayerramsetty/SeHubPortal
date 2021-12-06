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
        public string stationBarcode { get; set; }
        public string NdtBarcode { get; set; }
        public string BufferBarcode { get; set; }
        public string FinalBarcode { get; set; }
        public string paneType { get; set; }

        public tbl_treadtracker_barcode barcodeInfo { get; set; }
        public tbl_treadtracker_workorder workOrderInfo { get; set; }
        public string customerName { get; set; }
        public List<tbl_source_RARcodes> FailureCodes { get; set; }
        public List<tbl_retread_tread> TreadList { get; set; }
        public List<tbl_treadtracker_casing_sizes> SizeList { get; set; }
        public List<tbl_commercial_tire_manufacturers> BrandList { get; set; }
        public List<tbl_cta_location_info> LocationList { get; set; }
    }
}