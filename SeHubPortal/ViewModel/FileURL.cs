using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class FileURL
    {
        public List<KeyValuePair<string, string>> URLName { get; set; }
        public string Location_ID { get; set; }
        public string Payroll_ID { get; set; }
        public string Pane { get; set; }
        public string RenameString { get; set; }
        public int Permission { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public List<SelectListItem> PayrollIdList { get; set; }
        public List<EmployeeAttendanceListModel> employeeList { get; set; }
        public string SelectedEmployeeId { get; set; }
        public List<tbl_vacation_schedule> vacations { get; set; }
        public List<tbl_employee> employeeVacList { get; set; }
        public tbl_employee_status employeeStatus { get; set; }
        public tbl_vacation_schedule newLeave { get; set; }
        public tbl_vacation_schedule EditLeave { get; set; }
        public tbl_employee employee { get; set; }
        public int editVacationEmployee { get; set; }
         
        public List<TechnicianEfficiencyViewModel> techefficiencyList { get; set; }
        public List<SelectListItem> TechnicianTypeList { get; set; }
        public string TechnicianType { get; set; }
        public string SortBy { get; set; }
        public string Year { get; set; }

        public int ytd_barcode { get; set; }
        public int ytd_barcode_prev_year { get; set; }
        public double ytd_tech_minutes { get; set; }
        public double ytd_tech_production { get; set; }
        public double anual_production { get; set; }
        public double comparison { get; set; }
        public double ytd_tech_minutes_last_year { get; set; }

        public string commercialCustomerSurvey_link { get; set; }
        public string employeeFullName { get; set; }
        public string employeePosition { get; set; }
        public string Position { get; set; }
        public string AccessType { get; set; }

        public List<SelectListItem> Positions { get; set; }
        public tbl_harpoon_clients client_info { get; set; }

        public string signature { get; set; }
        public bool mailTo { get; set; }

        public string work_place_survey { get; set; }
        public string customer_satisfaction_survey { get; set; }


        public string employeeSurvey_read { get; set; }
        public string customerSurvey_read { get; set; }
        public string commercialCustomerSurvey_read { get; set; }

    }
}