using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Net.Mail;
using System.Data;
using System.Globalization;

namespace SeHubPortal.Controllers
{
    public class TimeSheetHarpoonController : Controller
    {

        private static List<SelectListItem> populateLocationsNames(string clientID, string email, bool all)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();

            if (all && (user.profile == "Administrator" || user.profile == "Global Manager"))
            {
                items.Add(new SelectListItem
                {
                    Text = "All",
                    Value = null
                });
            }


            foreach (var loc in locaList)
            {
                items.Add(new SelectListItem
                {
                    Text = loc.location_name,
                    Value = loc.auto_loc_id.ToString()
                });
            }

            return items;
        }

        private static List<SelectListItem> populateLocationsPermissions(string clientID, string email, bool all)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();

            if (all && (user.profile == "Administrator" || user.profile == "Global Manager"))
            {
                items.Add(new SelectListItem
                {
                    Text = "All",
                    Value = null
                });
            }

            foreach (var loc in locaList)
            {
                items.Add(new SelectListItem
                {
                    Text = loc.location_id,
                    Value = loc.auto_loc_id.ToString()
                });
            }

            return items;
        }

        [HttpGet]
        public ActionResult TimeSheet(string locId, string employeeId, string payrollID)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            HarpoonTimesheetViewModel model = new HarpoonTimesheetViewModel();
            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

            int empId = 0;

            if (locId == null)
            {
                model.MatchedLocID = db.tbl_harpoon_locations.Where(x => x.client_id == user.client_id).Select(x => x.auto_loc_id).FirstOrDefault().ToString();
            }
            else
            {
                model.MatchedLocID = locId;
            }

            if (employeeId == null)
            {
                empId = db.tbl_harpoon_employee.Where(x => x.client_id == clientID && x.auto_loc_id.ToString() == model.MatchedLocID).OrderBy(x => x.last_name).Select(x => x.auto_emp_id).FirstOrDefault();
            }
            else
            {
                empId = Convert.ToInt32(employeeId);
            }

            //model.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();
            var empDetails = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == empId).FirstOrDefault();

            List<tbl_harpoon_employee> employeeListLocation = new List<tbl_harpoon_employee>();

            employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == locId && x.status == 1).OrderBy(x => x.last_name).ToList();

            List<EmployeeAttendanceListModel> EmpDetailsList = new List<EmployeeAttendanceListModel>();
            foreach (var emp in employeeListLocation)
            {
                EmployeeAttendanceListModel empdetails = new EmployeeAttendanceListModel();
                empdetails.employeeId = emp.auto_emp_id.ToString() + ";" + emp.status;
                empdetails.fullName = emp.last_name + ", " + emp.first_name;
                EmpDetailsList.Add(empdetails);
            }

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail, false);
            }
            else
            {
                model.MatchedLocs = populateLocationsNames(clientID, uesrEmail, false);
            }

            model.SelectedEmployeeId = empId.ToString();
            //model.employeepayrollList = EmpDetailsList.OrderBy(x => x.fullName).ToList();

            return View(model);
        }
    }
}