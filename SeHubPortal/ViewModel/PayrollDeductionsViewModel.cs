using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class PayrollDeductionsViewModel
    {
        public int emp_id { get; set; }
        public string emp_name { get; set; }
        public string hoursWorked { get; set; }
        public string gross_earnings { get; set; }
        public string federal_tax { get; set; }
        public string provincial_tax { get; set; }
        public string CPP { get; set; }
        public string EI { get; set; }
        public string group_ins { get; set; }
        public string pension_plan { get; set; }
        public string other_pension_plan { get; set; }
        public string total_deductions { get; set; }
        public string net_pay { get; set; }
    }
}