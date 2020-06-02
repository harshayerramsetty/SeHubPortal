using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class StaffEditInformationViewModel
    {
        public tbl_employee StaffWorkDetails { get; set; }
        public tbl_employee_personal StaffPersonalDetails { get; set; }
        public bool active_status { get; set; }
        public bool monitor_status { get; set; }
    }
}