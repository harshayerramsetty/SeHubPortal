using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class EmployeePayrollModel
    {
        public List<EmployeePayrollListModel> employeepayrollList { get; set; }
        public List<EmployeePayrollListModel> employeepayrollListChangeLocation { get; set; }
        public List<EmployeePayrollListModel> employeeListValidation { get; set; }

        public List<PayrollCorporateDashboard> corpDashboard { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public string SelectedEmployeeId { get; set; }
        public List<SelectListItem> PayrollIdList { get; set; }
        public string SelectedPayrollId { get; set; }
        public int currentPayRollId { get; set; }
        public PayRollViewModel employeepayroll { get; set; }
        public tbl_employee_payroll_submission employeepayrollSubmission { get; set; }
        public List<tbl_payroll_submission_branch> branchSubmission { get; set; }
        public List<tbl_payroll_submission_corporate> corporateSubmission { get; set; }
        public int payrollSettings { get; set; }

        public List<SelectListItem> MatchedEmployees { get; set; }
        public List<SelectListItem> MatchedEmployeesChangeLoc { get; set; }

        public string MatchedEmployeeId { get; set; }

        public string AddEmployeToPayroll { get; set; }

        public List<SelectListItem> NewMatchedEmployees { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }

        public int LockStatusLocation { get; set; }

        public List<tbl_employee_payroll_biweekly> validate { get; set; }
        public List<tbl_employee_payroll_submission> validateCorp { get; set; }
        public List<tbl_employee_payroll_summary> summary { get; set; }

        public List<ValidationAdjustment> ValidationAdjustmentList { get; set; }

        public double YearlyRegular { get; set; }
        public double YearlyOT { get; set; }
        public double YearlyVac { get; set; }
        public double YearlySic { get; set; }

        public List<KeyValuePair<DateTime, double?>> vacDatesYearly { get; set; }
        public List<KeyValuePair<DateTime, double?>> sicDatesYearly { get; set; }

        public bool enableBranchPayrollEntry { get; set; }

        public bool twoActivePayrolls { get; set; }

    }
}