using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class tbl_target_accounts
    {
        public int cta_employee_number { get; set; }
        public string visit_type { get; set; }
        public string customer_name { get; set; }
        public string customer_contact { get; set; }
        public DateTime visit_date { get; set; }
        public string discusssion_details { get; set; }
        public string remainder { get; set; }
        public DateTime submission_date { get; set; }
    }
}