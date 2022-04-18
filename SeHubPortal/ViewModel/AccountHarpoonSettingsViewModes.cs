using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class AccountHarpoonSettingsViewModel
    {

        public tbl_harpoon_clients client { get; set; }
        public tbl_harpoon_clients editClient { get; set; }
        public List<SelectListItem> Locations { get; set; }


        public string listOfLocations { get; set; }

        public string clientID { get; set; }
        public bool custom { get; set; }
        public bool autoClockOut { get; set; }
        public double adjustment { get; set; }
        public bool manualCode { get; set; }
        public bool multipleLocations { get; set; }
        public bool master_loc_id { get; set; }
        public bool useLocIdInList { get; set; }
        public bool custom_empID_len { get; set; }
        public int custEmpIDLength { get; set; }

        public string masterLocID { get; set; }

        public bool sundayOpen { get; set; }
        public string sundayStart { get; set; }
        public string sundayEnd { get; set; }

        public bool monOpen { get; set; }
        public string monStart { get; set; }
        public string monEnd { get; set; }

        public bool TueOpen { get; set; }
        public string TueStart { get; set; }
        public string TueEnd { get; set; }

        public bool WedOpen { get; set; }
        public string WedStart { get; set; }
        public string WedEnd { get; set; }

        public bool ThuOpen { get; set; }
        public string ThuStart { get; set; }
        public string ThuEnd { get; set; }

        public bool FriOpen { get; set; }
        public string FriStart { get; set; }
        public string FriEnd { get; set; }

        public bool SatOpen { get; set; }
        public string SatStart { get; set; }
        public string SatEnd { get; set; }

        public string AccessType { get; set; }

    }
}