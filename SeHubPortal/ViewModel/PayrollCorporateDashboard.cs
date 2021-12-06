using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class PayrollCorporateDashboard
    {
        public string locID { get; set; }
        public int branchStatus { get; set; }
        public string branchSubmitter { get; set; }
        public string branchSubmissionDate{ get; set; }
        public int corpStatus { get; set; }
        public string corpSubmitter { get; set; }
        public string corpSubmissionDate { get; set; }
        public int employeCountAtLocation { get; set; }
        public int lockStatusbranch { get; set; }
        public int corpSaveStatus { get; set; }
    }
}