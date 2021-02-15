using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System.IO;
using System.Diagnostics;


namespace SeHubPortal.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public ActionResult Index()
        {
            return View();
        }



        [HttpPost]
        public ActionResult EditCallenderEvent(SettingsViewModel model)
        {
            

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int evid = Convert.ToInt32(model.CalenderPost.eventID);

            var EditCalEve = db.tbl_Calendar_events.Where(x => x.event_ID == evid).FirstOrDefault();

            EditCalEve.subject = model.CalenderPost.Type;
            EditCalEve.Description = model.CalenderPost.Title;
            EditCalEve.start_date = Convert.ToDateTime(model.CalenderPost.Date);

            db.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            SettingsViewModel model = new SettingsViewModel();
            tbl_payroll_settings payset = new tbl_payroll_settings();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.settings_dashboard == 0)
            {
                return RedirectToAction("EmployeePermissions", "Tools");
            }

            var payrollSettingDetails = db.tbl_payroll_settings.Where(x => x.ID == 1).FirstOrDefault();

            Debug.WriteLine(payrollSettingDetails.payroll_date);
            Debug.WriteLine(payrollSettingDetails.payroll_submission);

            payset.payroll_date = payrollSettingDetails.payroll_date;
            payset.payroll_submission = payrollSettingDetails.payroll_submission;

            List<tbl_Calendar_events> calendar_Events_List = new List<tbl_Calendar_events>();

            calendar_Events_List = db.tbl_Calendar_events.ToList();

            Debug.WriteLine(calendar_Events_List);


            model.payroll_Settings = payset;
            model.Calendar_Events = calendar_Events_List;

            

            return View(model);
        }

        public ActionResult Customers(SettingsCustomers model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.settings_customers == 0)
            {
                return RedirectToAction("SignIn", "Login");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult SavePayrollSettings(tbl_payroll_settings model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            (from p in db.tbl_payroll_settings
             where p.ID == 1
             select p).ToList().ForEach(x => x.payroll_date = model.payroll_date);

            (from p in db.tbl_payroll_settings
             where p.ID == 1
             select p).ToList().ForEach(x => x.payroll_submission = model.payroll_submission);

            db.SaveChanges();

            return RedirectToAction("Dashboard");
        }

    }
}