using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class ProductionScheduleViewModel
    {
        public List<tbl_production_schedule> ProductionItems { get; set; }
        public List<tbl_production_schedule> LineItems { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }

        public tbl_production_schedule NewOrder { get; set; }
        public tbl_production_schedule EditOrder { get; set; }
        public tbl_production_schedule EditOrderReadonly { get; set; }
        public List<SelectListItem> ShipTo { get; set; }
        public List<SelectListItem> CasingSpec { get; set; }


        public List<SelectListItem> SizeList { get; set; }
        public List<SelectListItem> Treadlist { get; set; }

        public List<string> PartNum { get; set; }

        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> PartNumbers { get; set; }
        public string customer { get; set; }
        public string order_for { get; set; }
    }
}