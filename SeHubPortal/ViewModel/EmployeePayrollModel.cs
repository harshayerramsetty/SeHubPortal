using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class EmployeePayrollModel
    {
        public List<EmployeePayrollListModel> employeepayrollList { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }      
        public string MatchedLocID { get; set; }
        public string SelectedEmployeeId { get; set; }
        public List<SelectListItem> PayrollIdList { get; set; }
        public string SelectedPayrollId { get; set; }
        public int currentPayRollId { get; set; }
        public PayRollViewModel employeepayroll { get; set; }
    }
}