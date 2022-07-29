using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
namespace SeHubPortal.ViewModel
{
    public class HarpoonLocationsViewModel
    {
        public List<tbl_harpoon_locations> Locations { get; set; }
        public List<tbl_harpoon_departments> Departments { get; set; }
        public List<tbl_harpoon_jobclock> JobClocks { get; set; }
        public tbl_harpoon_locations newLocation { get; set; }
        public tbl_harpoon_locations EditLocation { get; set; }
        public tbl_harpoon_departments newDepartment { get; set; }
        public tbl_harpoon_departments EditDepartment { get; set; }
        public tbl_harpoon_jobclock newJobClock { get; set; }
        public tbl_harpoon_jobclock EditJobClock { get; set; }
        public bool autoGenLoc { get; set; }
        public int newSeqNum { get; set; }
        public string deleteLocation { get; set; }
        public bool status { get; set; }
        public tbl_harpoon_clients client_info { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
    }
}