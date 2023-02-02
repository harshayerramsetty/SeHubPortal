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

        private static List<SelectListItem> populateTimesheetID(string client_id, DateTime currentDate)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var clientSettings = db.tbl_harpoon_settings.Where(x => x.client_id == client_id).FirstOrDefault();

            if (clientSettings.timeSheet_start.Value < currentDate && clientSettings.timeSheet_start.Value.AddDays(14) > currentDate)
            {
                items.Add(new SelectListItem
                {
                    Text = clientSettings.timeSheet_start.Value.ToString("yyyy") + " " + clientSettings.timeSheet_start.Value.ToString("MMM-dd") + " to " + clientSettings.timeSheet_start.Value.AddDays(14).ToString("MMM-dd"),
                    Value = "1"
                });
            }
            else
            {
                var prevTimesheets = db.tbl_harpoon_timesheet.Where(x => x.client_id.ToString() == client_id).ToList();
                var prevTimesheet_ids = prevTimesheets.Where(x => x.client_id.ToString() == client_id).Select(x => x.timesheet_id).Distinct().ToList();

                DateTime start = new DateTime(0);
                DateTime end = new DateTime(0);
                string tid = "";

                foreach (var timesheet_id in prevTimesheet_ids)
                {
                    tid = timesheet_id;

                    var timesheet_records = prevTimesheets.Where(x => x.timesheet_id == timesheet_id).OrderBy(x => x.date).Select(x => x.date).ToList();

                    start = timesheet_records.FirstOrDefault();
                    end = timesheet_records.LastOrDefault();

                    items.Add(new SelectListItem
                    {
                        Text = start.ToString("yyyy") + " " + start.ToString("MMM-dd") + " to " + end.ToString("MMM-dd"),
                        Value = timesheet_id
                    });
                }

                items.Add(new SelectListItem
                {
                    Text = end.AddDays(1).ToString("yyyy") + " " + end.AddDays(1).ToString("MMM-dd") + " to " + end.AddDays(14).ToString("MMM-dd"),
                    Value = (Convert.ToInt32(tid) + 1).ToString()
                });

            }

            return items.OrderByDescending(x => x.Value).ToList();
        }

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

        private static (List<SelectListItem>, List<tbl_source_harpoon_timesheet_categories>) populateTimesheetCategories(string clientID)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            List<tbl_source_harpoon_timesheet_categories> categories_full = new List<tbl_source_harpoon_timesheet_categories>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var categories = db.tbl_source_harpoon_timesheet_categories.ToList();
            var category_selection = db.tbl_source_harpoon_timesheet_category_selection.Where(x => x.client_id == clientID).FirstOrDefault();

            foreach (var cat in categories)
            {
                string state = category_selection.GetType().GetProperty(cat.category).GetValue(category_selection).ToString();

                if (bool.Parse(state))
                {
                    items.Add(new SelectListItem
                    {
                        Text = cat.name,
                        Value = cat.category
                    });

                    categories_full.Add(categories.Where(x => x.category == cat.category).FirstOrDefault());
                }

            }
            return (items, categories_full);
        }

        public JsonResult GetAttendanceDates(int empid, DateTime start, DateTime end)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var attendanceLog = db.tbl_harpoon_attendance_log.Where(x => x.auto_emp_id == empid && x.time_stamp > start && x.time_stamp < end).ToList();

            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>>();

            for (var day = start.Date; day <= end; day = day.AddDays(1))
            {
                var attendanceInfo = attendanceLog.Where(x => x.time_stamp.Date == day).OrderBy(x => x.time_stamp).Select(x => x.time_stamp).ToList();

                DateTime eventStart = attendanceInfo.FirstOrDefault();
                DateTime eventEnd = attendanceInfo.LastOrDefault();
                if (eventStart != eventEnd)
                {
                    events.Add(new KeyValuePair<string, string>(day.Date.ToString("yyyy-MM-dd"), (eventEnd - eventStart).ToString(@"hh\:mm")));
                }

            }

            var timesheetEvents = db.tbl_harpoon_timesheet.Where(x => x.auto_emp_id == empid && x.date > start && x.date < end).ToList();

            foreach(var timesheetEvent in timesheetEvents)
            {
                events.Add(new KeyValuePair<string, string>(timesheetEvent.date.ToString("yyyy-MM-dd"), timesheetEvent.type + ": " + timesheetEvent.value));
            }

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult TimeSheet(string locId, string employeeId, string timesheetID)
        {
            
            if (Session["userID"] == null)
            {
                return RedirectToAction("HarpoonLogin", "Login");
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            HarpoonTimesheetViewModel model = new HarpoonTimesheetViewModel();
            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();
            model.client_settings = db.tbl_harpoon_settings.Where(x => x.client_id == clientID).FirstOrDefault();

            DateTime currentDate = System.DateTime.Today.AddDays(-model.client_settings.timeSheet_delay.Value);

            model.Timeshet_ids = populateTimesheetID(clientID, currentDate);

            if (timesheetID != null)
            {
                model.timesheet_start = model.client_settings.timeSheet_start.Value.AddDays((Convert.ToInt32(timesheetID) - 1) * 14).AddDays(-1);
                model.timesheet_end = model.timesheet_start.AddDays(14).AddDays(1);
                model.Timesheet_id = timesheetID;
            }
            else
            {                
                int days = (int)Math.Ceiling((currentDate - model.client_settings.timeSheet_start).Value.TotalDays / 14);
                model.timesheet_start = model.client_settings.timeSheet_start.Value.AddDays((days-1) * 14).AddDays(-1);
                model.timesheet_end = model.timesheet_start.AddDays(14).AddDays(1);
                model.Timesheet_id = days.ToString();
            }            

            int empId = 0;

            if (locId == null)
            {
                model.MatchedLocID = db.tbl_harpoon_locations.Where(x => x.client_id == user.client_id).Select(x => x.auto_loc_id).FirstOrDefault().ToString();
            }
            else
            {
                model.MatchedLocID = locId;
            }

            var payroll = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == 2225).FirstOrDefault();
            
            DateTime startDate = payroll.start_date.Value;
            DateTime endDate = payroll.end_date.Value;

            var attendanceLog = db.tbl_harpoon_attendance_log.Where(x => x.auto_emp_id == empId && x.time_stamp > startDate && x.time_stamp < endDate).ToList();

            List<ClockEventsOfDayViewModel> attendanceHelper = new List<ClockEventsOfDayViewModel>();

            for (int i = 0; i < 14; i++)
            {
                ClockEventsOfDayViewModel attendanceItem = new ClockEventsOfDayViewModel();
                DateTime TempDate = startDate.AddDays(i);
                attendanceItem.Start = attendanceLog.Where(x => x.auto_emp_id == empId && x.time_stamp.Date == TempDate.Date).Select(x => x.time_stamp).FirstOrDefault();
                attendanceItem.End = attendanceLog.Where(x => x.auto_emp_id == empId && x.time_stamp.Date == TempDate.Date).Select(x => x.time_stamp).LastOrDefault();
                attendanceItem.Total = attendanceItem.End - attendanceItem.Start;
                attendanceHelper.Add(attendanceItem);
            }

            model.AttendanceHelper = attendanceHelper;

            var empDetails = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == empId).FirstOrDefault();

            List<tbl_harpoon_employee> employeeListLocation = new List<tbl_harpoon_employee>();

            employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == model.MatchedLocID && x.status == 1).OrderBy(x => x.last_name).ToList();

            List<EmployeePayrollListModel> EmpDetailsList = new List<EmployeePayrollListModel>();
            foreach (var emp in employeeListLocation)
            {
                EmployeePayrollListModel empdetails = new EmployeePayrollListModel();
                empdetails.employeeId = emp.auto_emp_id.ToString();
                empdetails.fullName = emp.last_name + ", " + emp.first_name;
                EmpDetailsList.Add(empdetails);
            }

            if (employeeId == null)
            {
                empId = Convert.ToInt32(EmpDetailsList.OrderBy(x => x.fullName).Select(x => x.employeeId).FirstOrDefault());
            }
            else
            {
                empId = Convert.ToInt32(employeeId);
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
            model.employeepayrollList = EmpDetailsList.OrderBy(x => x.fullName).ToList();

            (model.Categories, model.categories_full) = populateTimesheetCategories(clientID);

            return View(model);
        }
        
        public void SaveTimesheetRecord(float Re, string Opt, float OptVal, DateTime dat, int emp, string tid)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();           

            var regExist = db.tbl_harpoon_timesheet.Where(x => x.client_id.ToString() == user.client_id && x.auto_emp_id == emp && x.timesheet_id == "1" && x.date == dat.Date && x.type == "Re").FirstOrDefault();

            if (regExist is null)
            {
                tbl_harpoon_timesheet Reg = new tbl_harpoon_timesheet();
                Reg.client_id = Convert.ToInt32(user.client_id);
                Reg.auto_emp_id = emp;
                Reg.timesheet_id = tid;
                Reg.date = dat.Date;
                Reg.type = "Re";
                Reg.value = Re;
                db.tbl_harpoon_timesheet.Add(Reg);
            }
            else
            {
                regExist.value = Re;
            }

            
            if (Opt != "")
            {
                var OthExist = db.tbl_harpoon_timesheet.Where(x => x.client_id.ToString() == user.client_id && x.auto_emp_id == emp && x.timesheet_id == "1" && x.date == dat.Date && x.type != "Re").FirstOrDefault();

                if (OthExist is null)
                {
                    tbl_harpoon_timesheet Oth = new tbl_harpoon_timesheet();
                    Oth.client_id = Convert.ToInt32(user.client_id);
                    Oth.auto_emp_id = emp;
                    Oth.timesheet_id = "1";
                    Oth.date = dat.Date;
                    Oth.type = Opt;
                    Oth.value = OptVal;
                    db.tbl_harpoon_timesheet.Add(Oth);
                }
                else
                {
                    db.tbl_harpoon_timesheet.Remove(OthExist);
                    tbl_harpoon_timesheet Oth = new tbl_harpoon_timesheet();
                    Oth.client_id = Convert.ToInt32(user.client_id);
                    Oth.auto_emp_id = emp;
                    Oth.timesheet_id = "1";
                    Oth.date = dat.Date;
                    Oth.type = Opt;
                    Oth.value = OptVal;
                    db.tbl_harpoon_timesheet.Add(Oth);
                }
            }
            db.SaveChanges();
        }

        public class DatDetails
        {
            public string timeduration { get; set; }
            public string reg { get; set; }
            public string opt { get; set; }
            public string optVal { get; set; }

        }

        public string GetDateDetails(DateTime dat, int emp)
        {

            DatDetails details = new DatDetails();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            
            DateTime nextDat = dat.AddDays(1);

            var attendanceInfo = db.tbl_harpoon_attendance_log.Where(x => x.auto_emp_id == emp && x.time_stamp > dat && x.time_stamp < nextDat).OrderBy(x => x.time_stamp).Select(x => x.time_stamp).ToList();

            if (attendanceInfo != null)
            {
                DateTime eventStart = attendanceInfo.FirstOrDefault();
                DateTime eventEnd = attendanceInfo.LastOrDefault();
                details.timeduration = (eventEnd - eventStart).ToString(@"hh\:mm");
            }
            var regInfo = db.tbl_harpoon_timesheet.Where(x => x.type == "Re" && x.date == dat.Date).FirstOrDefault();
            var optInfo = db.tbl_harpoon_timesheet.Where(x => x.type != "Re" && x.date == dat.Date).FirstOrDefault();

            if (regInfo != null)
            {
                details.reg = regInfo.value.ToString();
            }
            if (optInfo != null)
            {
                details.opt = optInfo.type;
                details.optVal = optInfo.value.ToString();
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(details);
        }

        public class SummaryItems
        {
            public string timeduration { get; set; }
            public string reg { get; set; }
            public string opt { get; set; }
            public string optVal { get; set; }

        }

        public string UpdateSummary(int emp, string timeSheei_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_source_harpoon_timesheet_category_selection summary = new tbl_source_harpoon_timesheet_category_selection();

            var categories = db.tbl_source_harpoon_timesheet_categories.Select(x => x.category).ToList();

            foreach (var cat in categories)
            {
                string val = db.tbl_harpoon_timesheet.Where(x => x.auto_emp_id == emp && x.timesheet_id == timeSheei_id && x.type == cat).Sum(x => x.value).ToString();
                SetObjectProperty(cat + "_type", val, summary);
            }

            SetObjectProperty("total", db.tbl_harpoon_timesheet.Where(x => x.timesheet_id == timeSheei_id && x.auto_emp_id == emp).Sum(x => x.value).ToString(), summary);

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(summary);
        }

        private void SetObjectProperty(string propertyName, string value, object obj)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            // make sure object has the property we are after
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        [HttpPost]
        public ActionResult ChangeTimeSheetID(HarpoonTimesheetViewModel model)
        {
            return RedirectToAction("Timesheet", new { locId = model.MatchedLocID, employeeId = model.SelectedEmployeeId, timesheetID = model.Timesheet_id });
        }

        [HttpPost]
        public ActionResult ChangeLocTimeSheet(HarpoonTimesheetViewModel model)
        {
            return RedirectToAction("Timesheet", new { locId = model.MatchedLocID, employeeId = model.SelectedEmployeeId, timesheetID = model.Timesheet_id });
        }
    }
}