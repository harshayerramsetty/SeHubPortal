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

    }
}