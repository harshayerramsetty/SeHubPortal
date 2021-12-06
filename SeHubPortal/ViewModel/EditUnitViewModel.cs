﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class EditUnitViewModel
    {
        public tbl_fleetTVT_unit Unit { get; set; }
        public List<SelectListItem> ConfigurationsListTractor { get; set; }
        public List<SelectListItem> ConfigurationsListTrailer { get; set; }
        public List<SelectListItem> SizesList { get; set; }
    }
}