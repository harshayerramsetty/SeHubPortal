using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
namespace SeHubPortal.ViewModel
{
    public class HarpoonTimesheetViewModel
    {
        public List<EmployeePayrollListModel> employeepayrollList { get; set; }
        public List<EmployeePayrollListModel> employeepayrollListChangeLocation { get; set; }

        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public string SelectedEmployeeId { get; set; }
        public List<SelectListItem> PayrollIdList { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public string SelectedPayrollId { get; set; }
        public string SelectedCategory { get; set; }
        public int currentPayRollId { get; set; }
        public List<tbl_payroll_submission_branch> branchSubmission { get; set; }
        public List<tbl_payroll_submission_corporate> corporateSubmission { get; set; }

        public List<SelectListItem> MatchedEmployees { get; set; }
        public List<SelectListItem> MatchedEmployeesChangeLoc { get; set; }

        public List<SelectListItem> Timeshet_ids { get; set; }
        public string Timesheet_id { get; set; }

        public string MatchedEmployeeId { get; set; }

        public List<tbl_employee_payroll_summary> summary { get; set; }
        public tbl_harpoon_clients client_info { get; set; }
        public tbl_harpoon_settings client_settings { get; set; }

        public List<ValidationAdjustment> ValidationAdjustmentList { get; set; }
        public tbl_harpoon_employee EmployeeDetails { get; set; }
        public List<tbl_source_harpoon_timesheet_categories> categories_full { get; set; }
        public List<ClockEventsOfDayViewModel> AttendanceHelper { get; set; }
        public DateTime timesheet_start { get; set; }
        public DateTime timesheet_end { get; set; }
    }
}