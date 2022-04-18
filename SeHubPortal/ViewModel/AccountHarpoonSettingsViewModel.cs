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

        public string clientID { get; set; }
        public bool custom { get; set; }
        public bool autoClockOut { get; set; }
        public float adjustment { get; set; }
        public float manualCode { get; set; }
        public float multipleLocations { get; set; }
        public float autoGenLocId { get; set; }
        public float useLocIdInList { get; set; }

        public string sundayOpen { get; set; }
        public string sundayStart { get; set; }
        public string sundayEnd { get; set; }

        public string monOpen { get; set; }
        public string monStart { get; set; }
        public string monEnd { get; set; }

        public string TueOpen { get; set; }
        public string TueStart { get; set; }
        public string TueEnd { get; set; }

        public string WedOpen { get; set; }
        public string WedStart { get; set; }
        public string WedEnd { get; set; }

        public string ThuOpen { get; set; }
        public string ThuStart { get; set; }
        public string ThuEnd { get; set; }

        public string FriOpen { get; set; }
        public string FriStart { get; set; }
        public string FriEnd { get; set; }

        public string SatOpen { get; set; }
        public string SatStart { get; set; }
        public string SatEnd { get; set; }

    }
}