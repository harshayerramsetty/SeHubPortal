using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class HarpoonEmployeeViewModel
    {
        public string employee_id { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string position { get; set; }
        public Nullable<System.DateTime> Date_of_birth { get; set; }
        public string client_id { get; set; }
        public Nullable<int> status { get; set; }
        public byte[] profile_pic { get; set; }
        public string locationId { get; set; }
        public int auto_emp_id { get; set; }
        public string rfidPaired { get; set; }
    }
}