using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class PayRollViewModel
    {
        public tbl_employee_payroll_dates payDates { get; set; }
        public EditPayrollBiWeeklyViewModel payBiweek { get; set; }
        public tbl_employee_payroll_summary paySummary { get; set; }
    }
}