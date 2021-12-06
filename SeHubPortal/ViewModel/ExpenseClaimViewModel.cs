using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class ExpenseClaimViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public List<SelectListItem> Acounts { get; set; }
        public List<SelectListItem> ExpClaims { get; set; }
        public string Expense { get; set; }
        public string Location { get; set; }
        public string Acount { get; set; }
        public string empid { get; set; }
        public tbl_employee employy { get; set; }
        public string seq { get; set; }


        public tbl_expense_claim Claim1 { get; set; }
        public tbl_expense_claim Claim2 { get; set; }
        public tbl_expense_claim Claim3 { get; set; }
        public tbl_expense_claim Claim4 { get; set; }
        public tbl_expense_claim Claim5 { get; set; }
        public tbl_expense_claim Claim6 { get; set; }
        public tbl_expense_claim Claim7 { get; set; }
        public tbl_expense_claim Claim8 { get; set; }
        public tbl_expense_claim Claim9 { get; set; }
        public tbl_expense_claim Claim10 { get; set; }

    }
}