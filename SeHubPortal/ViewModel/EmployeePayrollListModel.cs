using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class EmployeePayrollListModel
    {
        public string employeeId { get; set; }
        public string fullName { get; set; }
        public string submissionStatus { get; set; }
        public string submissionStatusCorporate { get; set; }
    }
}