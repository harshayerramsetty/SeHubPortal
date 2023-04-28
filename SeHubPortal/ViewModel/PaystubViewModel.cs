using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class PaystubViewModel
    {
        public double empid { get; set; }
        public double pid { get; set; }
        public string name { get; set; }
        public string position { get; set; }
        public string loc { get; set; }
        public string email { get; set; }
        public PayrollDeductionsViewModel deductions { get; set; }
        public PayrollDeductionsViewModel deductionsYTD { get; set; }
    }
}