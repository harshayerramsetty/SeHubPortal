using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
namespace SeHubPortal.ViewModel
{
    public class HarpoonDevicesViewModel
    {
        public List<tbl_harpoon_devices> devices { get; set; }
        public List<tbl_harpoon_source_serialNumbers> Serials { get; set; }
        public tbl_harpoon_devices newDevice { get; set; }
        public tbl_harpoon_devices editDevice { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public List<SelectListItem> LocationZonesList { get; set; }
        public List<SelectListItem> ColorsList { get; set; }
        public List<tbl_harpoon_locations> Locations { get; set; }

        public string verificationCode { get; set; }
        public bool locIDorName { get; set; }
        public tbl_harpoon_clients client_info { get; set; }
    }
}