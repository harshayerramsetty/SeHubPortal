using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class ShopSuppliesViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<tbl_fuel_log_shopSupplies> fuelLogShopSuppliesList { get; set; }
        public tbl_fuel_log_shopSupplies fuelLogShopSuppliesTableValues { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public string MatchedLocation { get; set; }
    }
}