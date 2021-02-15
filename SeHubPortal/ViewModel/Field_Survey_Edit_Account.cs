using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class Field_Survey_Edit_Account
    {
        public List<SelectListItem> CustomerList { get; set; }
        public string Customer { get; set; }
        public List<SelectListItem> ConfigurationsListTractor { get; set; }
        public List<SelectListItem> ConfigurationsListTrailer { get; set; }
        public List<SelectListItem> ConfigurationsList { get; set; }
        public string Type { get; set; }
        public string Configuration { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }

        public List<SelectListItem> BrandList { get; set; }
        public List<SelectListItem> SizeList { get; set; }
        public List<SelectListItem> LocationList { get; set; }
        public List<SelectListItem> WearList { get; set; }
        public List<SelectListItem> ValveList { get; set; }
        public List<SelectListItem> TireConditionList { get; set; }
        public List<SelectListItem> WheelConditionList { get; set; }

        public string Brand { get; set; }
        public string Size { get; set; }
        public string Location { get; set; }
        public string Wear { get; set; }
        public string Valve { get; set; }
        public string Tire { get; set; }
        public string Wheel { get; set; }
        

    }
}