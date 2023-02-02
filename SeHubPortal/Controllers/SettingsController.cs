using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Threading;

namespace SeHubPortal.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddCustomer (SettingsCustomers model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            if(db.tbl_customer_list.Where(x => x.cust_us1 == model.NewCustomer.cust_us1).Count() == 0)
            {
                tbl_customer_list cust_row = new tbl_customer_list();
                cust_row.cust_us1 = model.NewCustomer.cust_us1;
                cust_row.cust_name = model.NewCustomer.cust_name;
                cust_row.cust_add1 = model.NewCustomer.cust_add1;
                cust_row.cust_add2 = model.NewCustomer.cust_add2;
                cust_row.cust_add3 = model.NewCustomer.cust_add3;
                cust_row.cust_add4 = model.NewCustomer.cust_add4;
                cust_row.cust_city = model.NewCustomer.cust_city;
                cust_row.cust_state = model.NewCustomer.cust_state;
                cust_row.cust_zip = model.NewCustomer.cust_zip;
                db.tbl_customer_list.Add(cust_row);
                db.SaveChanges();
            }

            if(db.tbl_tread_tracker_customers.Where(x => x.customer_number == model.NewCustomer.cust_us1).Count() == 0 && model.treadTracker == true)
            {
                tbl_tread_tracker_customers treadTracker_cust = new tbl_tread_tracker_customers();
                treadTracker_cust.customer_number = model.NewCustomer.cust_us1;
                db.tbl_tread_tracker_customers.Add(treadTracker_cust);
            }

            if (db.tbl_fleettvt_Customer.Where(x => x.customer_number == model.NewCustomer.cust_us1).Count() == 0 && model.fleetTVT == true)
            {
                tbl_fleettvt_Customer fleetTVT_cust = new tbl_fleettvt_Customer();
                fleetTVT_cust.customer_number = model.NewCustomer.cust_us1;
                db.tbl_fleettvt_Customer.Add(fleetTVT_cust);
            }

            if (db.tbl_customer_reporting_customers.Where(x => x.customer_number == model.NewCustomer.cust_us1).Count() == 0 && model.CRM == true)
            {
                tbl_customer_reporting_customers CRM_cust = new tbl_customer_reporting_customers();
                CRM_cust.customer_number = model.NewCustomer.cust_us1;
                db.tbl_customer_reporting_customers.Add(CRM_cust);
            }

            return RedirectToAction("Customers");
        }

        public ActionResult EditCustomer(SettingsCustomers model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (model.treadTrackerEdit == true)
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customeredit).FirstOrDefault();
                custm.tread_tracker = true;
            }
            else
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customeredit).FirstOrDefault();
                custm.tread_tracker = false;
            }

            if (model.CRMedit == true)
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customeredit).FirstOrDefault();
                custm.call_reporting = true;
            }
            else
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customeredit).FirstOrDefault();
                custm.call_reporting = false;
            }

            if (model.fleetTVTEdit == true)
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customeredit).FirstOrDefault();
                custm.fleet_tvt = true;
            }
            else
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customeredit).FirstOrDefault();
                custm.fleet_tvt = false;
            }

            db.SaveChanges();


            return RedirectToAction("Customers");
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
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            SettingsViewModel model = new SettingsViewModel();
            tbl_payroll_settings payset = new tbl_payroll_settings();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            var wheel = db.tbl_calculator_GP_fixed.Where(x => x.category == "Wheels, Rims & Lugs").FirstOrDefault();
            var freight = db.tbl_calculator_GP_fixed.Where(x => x.category == "Freight").FirstOrDefault();

            model.wheelRetail = wheel.retail.Value;
            model.wheelNA = wheel.national_account.Value;

            model.freightRetail = freight.retail.Value;
            model.freightNA = freight.national_account.Value;


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

            model.topBar_left = db.tbl_sehub_color_scheme.Where(x => x.component == "topBar_left").Select(x => x.color).FirstOrDefault();
            model.topBar_right = db.tbl_sehub_color_scheme.Where(x => x.component == "topBar_right").Select(x => x.color).FirstOrDefault();

            return View(model);
        }

        public ActionResult Customers(SettingsCustomers model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            model.CustomerList = db.tbl_cta_customers.ToList();
            model.CustomerListFleetTVT = db.tbl_fleettvt_Customer.ToList();
            model.CustomerListCRM = db.tbl_customer_reporting_customers.ToList();

            if (model.SehubAccess.settings_customers == 0)
            {
                return RedirectToAction("SignIn", "Login");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult SavePayrollSettings(SettingsViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var payrollSettings = db.tbl_payroll_settings.Where(x => x.ID == 1).FirstOrDefault();
            payrollSettings.payroll_date = model.payroll_Settings.payroll_date;
            payrollSettings.payroll_submission = model.payroll_Settings.payroll_submission;

            db.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public void ChangeColor(string component, string color)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var change_color = db.tbl_sehub_color_scheme.Where(x => x.component == component).FirstOrDefault();
            change_color.color = color;
            db.SaveChanges();

        }


        public ActionResult ChangeCalculatorSettings(SettingsViewModel model)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var wheels = db.tbl_calculator_GP_fixed.Where(x => x.category == "Wheels, Rims & Lugs").FirstOrDefault();
            var freight = db.tbl_calculator_GP_fixed.Where(x => x.category == "Freight").FirstOrDefault();

            wheels.retail = model.wheelRetail;
            wheels.national_account = model.wheelNA;

            freight.retail = model.freightRetail;
            freight.national_account = model.freightNA;

            db.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        private static List<SelectListItem> populateLocations()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select loc_id From tbl_cta_location_info where loc_status=1";
                //Debug.WriteLine("Query:" + query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["loc_id"].ToString(),
                                Value = sdr["loc_id"].ToString()
                            });
                        }

                    }
                    con.Close();
                }
            }

            return items;
        }

        private static List<SelectListItem> populatePositions()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_position_info.ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.PositionTitle,
                    Value = Convert.ToString(val.PositionTitle)
                });
            }
            return items;
        }

        [HttpPost]
        public ActionResult Administration(MyStaffViewModel modal)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            return RedirectToAction("Administration", new { LocId = modal.MatchedStaffLocID });
        }

        [HttpPost]
        public ActionResult EditTimeClock(TimeClockViewModel modal)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var device = db.tbl_timeclock_devices.Where(x => x.serial_number == modal.serial).FirstOrDefault();
            device.loc_id = modal.loc;
            db.SaveChanges();

            return RedirectToAction("TimeClock");
        }

        [HttpPost]
        public ActionResult DeleteTimeClock(TimeClockViewModel modal)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var device = db.tbl_timeclock_devices.Where(x => x.serial_number == modal.serialDelete).FirstOrDefault();
            db.tbl_timeclock_devices.Remove(device);
            Trace.WriteLine(modal.serialDelete);
            db.SaveChanges();

            return RedirectToAction("TimeClock");
        }

        [HttpGet]
        public ActionResult TimeClock(string LocId)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            TimeClockViewModel model = new TimeClockViewModel();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;
            model.Devices = db.tbl_timeclock_devices.ToList();
            model.locations = populateLocations();

            return View(model);
        }

        [HttpGet]
        public ActionResult EditEmployeeInfo(string value)
        {
            //Debug.WriteLine("Inside EditEmployeeInfo:" + value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            MyStaffViewModel model = new MyStaffViewModel();
            int empId = Convert.ToInt32(value);

            model.empStatusInfo = db.tbl_employee_status.Where(x => x.employee_id == empId).FirstOrDefault();

            model.full_name = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();

            model.CompensationType = db.tbl_employee_status.Where(x => x.employee_id == empId).Select(x => x.compensation_type).FirstOrDefault();

            model.position = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_position).FirstOrDefault();

            model.PayrollIdList = populatePayrollId();

            tbl_employee_payroll_final PayrollData = new tbl_employee_payroll_final();

            PayrollData.RegularPay_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.RegularPay_H);
            PayrollData.OvertimePay_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.OvertimePay_H);
            PayrollData.VacationPay_D = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.VacationPay_D);
            PayrollData.SickLeave_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.SickLeave_H);
            PayrollData.StatutoryHolidayPay_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.StatutoryHolidayPay_H);
            PayrollData.StatutoryHolidayPay_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.StatutoryHolidayPay_H);
            PayrollData.CommissionPay_D = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.CommissionPay_D);
            PayrollData.OnCallCommission_D = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.OnCallCommission_D);
            PayrollData.OtherPay_D = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.OtherPay_D);
            PayrollData.OvertimePay_2_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.OvertimePay_2_H);
            PayrollData.OvertimePay_3_H = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.OvertimePay_3_H);
            PayrollData.SalaryPay_D = db.tbl_employee_payroll_final.Where(x => x.employee_id == empId).Sum(x => x.SalaryPay_D);

            model.PayrollInfo = PayrollData;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult EditEmployeeInfo(MyStaffViewModel model, HttpPostedFileBase EmployeeImage)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var statusInfo = db.tbl_employee_status.Where(a => a.employee_id.Equals(model.empStatusInfo.employee_id)).FirstOrDefault();

            string loc = db.tbl_employee.Where(x => x.employee_id == model.empStatusInfo.employee_id).Select(x => x.loc_ID).FirstOrDefault();

            if (statusInfo != null)
            {
                statusInfo.date_of_joining = model.empStatusInfo.date_of_joining;
                statusInfo.date_of_leaving = model.empStatusInfo.date_of_leaving;
                statusInfo.vacation = model.empStatusInfo.vacation;
                statusInfo.vacation_buyin = model.empStatusInfo.vacation_buyin;
                statusInfo.sick_days = model.empStatusInfo.sick_days;
                statusInfo.compensation_type = model.empStatusInfo.compensation_type;
            }

            var empInfo = db.tbl_employee.Where(x => x.employee_id == model.empStatusInfo.employee_id).FirstOrDefault();

            if(model.empStatusInfo.date_of_leaving != null)
            {

                if (empInfo != null)
                {
                    empInfo.status = 0;
                }
                int CurrentPayrollId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).Select(x => x.payroll_Id).FirstOrDefault();
                Trace.WriteLine("This is the test" + CurrentPayrollId.ToString());
                var deletepayrollBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == model.empStatusInfo.employee_id && x.payroll_id == CurrentPayrollId).FirstOrDefault();
                var deletepayrollSummery = db.tbl_employee_payroll_summary.Where(x => x.employee_id == model.empStatusInfo.employee_id && x.payroll_id == CurrentPayrollId).FirstOrDefault();
                var deletepayrollSubmission = db.tbl_employee_payroll_submission.Where(x => x.employee_id == model.empStatusInfo.employee_id && x.payroll_id == CurrentPayrollId).FirstOrDefault();

                if (deletepayrollBiweekly != null)
                {
                    db.tbl_employee_payroll_biweekly.Remove(deletepayrollBiweekly);

                }
                if (deletepayrollSummery != null)
                {
                    db.tbl_employee_payroll_summary.Remove(deletepayrollSummery);
                }
                if (deletepayrollSubmission != null)
                {
                    db.tbl_employee_payroll_submission.Remove(deletepayrollSubmission);

                }
            }
            else
            {
                empInfo.status = 1;
            }


            var payrollFinalRecord = db.tbl_employee_payroll_final.Where(x => x.employee_id == model.empStatusInfo.employee_id && x.payroll_id.ToString() == model.SelectedPayrollId).FirstOrDefault();

            var empdetails = db.tbl_employee.Where(x => x.employee_id == model.empStatusInfo.employee_id).FirstOrDefault();

            if (payrollFinalRecord != null)
            {
                payrollFinalRecord = model.PayrollInfo;
                payrollFinalRecord.payroll_id = Convert.ToInt32(model.SelectedPayrollId);
                payrollFinalRecord.employee_id = empdetails.employee_id;
                payrollFinalRecord.location_id = empdetails.loc_ID;
                payrollFinalRecord.full_name = empdetails.full_name;
                db.SaveChanges();
            }
            else
            {
                tbl_employee_payroll_final insertPayrollFinal = new tbl_employee_payroll_final();
                insertPayrollFinal = model.PayrollInfo;
                insertPayrollFinal.payroll_id = Convert.ToInt32(model.SelectedPayrollId);
                insertPayrollFinal.employee_id = empdetails.employee_id;
                insertPayrollFinal.location_id = empdetails.loc_ID;
                insertPayrollFinal.full_name = empdetails.full_name;
                db.tbl_employee_payroll_final.Add(insertPayrollFinal);
            }

            db.SaveChanges();
            return RedirectToAction("Administration", new { LocId = loc });
        }

        [HttpGet]
        public ActionResult Administration(string LocId)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if (modal.SehubAccess.my_staff == 0)
            {
                return RedirectToAction("Attendance", "Management");
            }

            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int permissions = CheckPermissions().my_staff.Value;

            string locationid = "";
            if (LocId == "" || LocId is null)
            {
                var empRememberLocation = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                if (empRememberLocation != null)
                {
                    locationid = empRememberLocation.loc_current;
                }
                else
                {
                    locationid = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
                }
            }
            else
            {
                locationid = LocId;
            }

            var EmployeeDetails = db.tbl_employee.Where(x => x.loc_ID == locationid && x.full_name != "Auto Technician").OrderBy(x => x.full_name).ToList();

            List<tbl_employee_status> empdetList = new List<tbl_employee_status>();

            foreach (var emp in EmployeeDetails)
            {
                tbl_employee_status empdet = new tbl_employee_status();
                empdet = db.tbl_employee_status.Where(x => x.employee_id == emp.employee_id).FirstOrDefault();
                if (empdet != null)
                {
                    empdetList.Add(empdet);
                }
            }


            //Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {
                modal.positionsTable = db.tbl_position_info.ToList();
                modal.employeeDetails = EmployeeDetails;
                modal.employeeStatusDetails = empdetList;
                modal.MatchedStaffLocs = populateLocationsPermissions(empId);
                modal.MatchedStaffLocID = locationid;
                modal.EmployeePermissions = permissions;


                modal.Positions = populatePositions();

                return View(modal);
            }
            else
            {
                return View(modal);
            }


        }

        private static List<SelectListItem> populateLocationsPermissions(int empId)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_cta_location_info.ToList();

            var sehubloc = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            if (sehubloc.loc_001 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "001",
                    Value = "001"
                });
            }
            if (sehubloc.loc_002 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "002",
                    Value = "002"
                });
            }
            if (sehubloc.loc_003 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "003",
                    Value = "003"
                });
            }
            if (sehubloc.loc_004 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "004",
                    Value = "004"
                });
            }
            if (sehubloc.loc_005 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "005",
                    Value = "005"
                });
            }
            if (sehubloc.loc_007 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "007",
                    Value = "007"
                });
            }
            if (sehubloc.loc_009 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "009",
                    Value = "009"
                });
            }
            if (sehubloc.loc_010 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "010",
                    Value = "010"
                });
            }
            if (sehubloc.loc_011 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "011",
                    Value = "011"
                });
            }
            if (sehubloc.loc_347 == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "347",
                    Value = "347"
                });
            }
            if (sehubloc.loc_AHO == 1)
            {
                items.Add(new SelectListItem
                {
                    Text = "AHO",
                    Value = "AHO"
                });
            }


            return items;
        }

        private static List<SelectListItem> populatePayrollId()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string todatDate = DateTime.Now.AddDays(-(db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value)).ToString("yyyy-MM-dd");
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT payroll_id FRom tbl_employee_payroll_dates where start_date <='" + todatDate + "' order by payroll_id desc";
                //Debug.WriteLine("Query:" + query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = "20" + sdr["payroll_id"].ToString().Substring(0, 2) + "-" + sdr["payroll_id"].ToString().Substring(sdr["payroll_id"].ToString().Length - 2),
                                Value = "20" + sdr["payroll_id"].ToString().Substring(0, 2) + sdr["payroll_id"].ToString().Substring(sdr["payroll_id"].ToString().Length - 2),
                                
                            });
                        }


                    }
                    con.Close();
                }
            }

            return items;
        }

        /*
         
            public void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            Trace.WriteLine("Reached sp_DataReceived");

            SerialPort _serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();

            Thread.Sleep(500);
            string data = _serialPort.ReadLine();

            bool _continue;

            _continue = true;

            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    Trace.WriteLine(message);
                }
                catch (TimeoutException) { }
            }


            // Invokes the delegate on the UI thread, and sends the data that was received to the invoked method.  
            // ---- The "si_DataReceived" method will be executed on the UI thread which allows populating of the textbox.  
            //this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }

             
             */



        public class PortChat
        {
            static bool _continue;
            static SerialPort _serialPort;

            public static void Main()
            {
                string name;
                string message;
                StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
                Thread readThread = new Thread(Read);

                // Create a new SerialPort object with default settings.  
                _serialPort = new SerialPort();

                // Allow the user to set the appropriate properties.  
                _serialPort.PortName = SetPortName(_serialPort.PortName);
                _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
                _serialPort.Parity = SetPortParity(_serialPort.Parity);
                _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
                _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
                _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

                // Set the read/write timeouts  
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                _serialPort.Open();
                _continue = true;
                readThread.Start();

                Console.Write("Name: ");
                name = Console.ReadLine();

                Console.WriteLine("Type QUIT to exit");

                while (_continue)
                {
                    message = Console.ReadLine();

                    if (stringComparer.Equals("quit", message))
                    {
                        _continue = false;
                    }
                    else
                    {
                        _serialPort.WriteLine(
                            String.Format("<{0}>: {1}", name, message));
                    }
                }

                readThread.Join();
                _serialPort.Close();
            }

            public static void Read()
            {
                while (_continue)
                {
                    try
                    {
                        string message = _serialPort.ReadLine();
                        Console.WriteLine(message);
                    }
                    catch (TimeoutException) { }
                }
            }

            public static string SetPortName(string defaultPortName)
            {
                string portName;

                Console.WriteLine("Available Ports:");
                foreach (string s in SerialPort.GetPortNames())
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("COM port({0}): ", defaultPortName);
                portName = Console.ReadLine();

                if (portName == "")
                {
                    portName = defaultPortName;
                }
                return portName;
            }

            public static int SetPortBaudRate(int defaultPortBaudRate)
            {
                string baudRate;

                Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
                baudRate = Console.ReadLine();

                if (baudRate == "")
                {
                    baudRate = defaultPortBaudRate.ToString();
                }

                return int.Parse(baudRate);
            }

            public static Parity SetPortParity(Parity defaultPortParity)
            {
                string parity;

                Console.WriteLine("Available Parity options:");
                foreach (string s in Enum.GetNames(typeof(Parity)))
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Parity({0}):", defaultPortParity.ToString());
                parity = Console.ReadLine();

                if (parity == "")
                {
                    parity = defaultPortParity.ToString();
                }

                return (Parity)Enum.Parse(typeof(Parity), parity);
            }

            public static int SetPortDataBits(int defaultPortDataBits)
            {
                string dataBits;

                Console.Write("Data Bits({0}): ", defaultPortDataBits);
                dataBits = Console.ReadLine();

                if (dataBits == "")
                {
                    dataBits = defaultPortDataBits.ToString();
                }

                return int.Parse(dataBits);
            }

            public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
            {
                string stopBits;

                Console.WriteLine("Available Stop Bits options:");
                foreach (string s in Enum.GetNames(typeof(StopBits)))
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
                stopBits = Console.ReadLine();

                if (stopBits == "")
                {
                    stopBits = defaultPortStopBits.ToString();
                }

                return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
            }

            public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
            {
                string handshake;

                Console.WriteLine("Available Handshake options:");
                foreach (string s in Enum.GetNames(typeof(Handshake)))
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
                handshake = Console.ReadLine();

                if (handshake == "")
                {
                    handshake = defaultPortHandshake.ToString();
                }

                return (Handshake)Enum.Parse(typeof(Handshake), handshake);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            SerialPort _serialPort = new SerialPort("COM1", 19200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
            try
            {
                if (!(_serialPort.IsOpen))
                    _serialPort.Open();
                _serialPort.Write("SI\r\n");
            }
            catch (Exception ex)
            {
            
            }
        }

        //private void si_DataReceived(string data) { textBox1.Text = data.Trim(); }

        public void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort _serialPort = new SerialPort("COM1", 19200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
            Thread.Sleep(500);
            string data = _serialPort.ReadLine();
            // Invokes the delegate on the UI thread, and sends the data that was received to the invoked method.  
            // ---- The "si_DataReceived" method will be executed on the UI thread which allows populating of the textbox.  
            
            //this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }

        private delegate void SetTextDeleg(string text);

        [HttpGet]
        public ActionResult DataResources(string ack)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;
            modal.csvData = ack;

            var unqPayroll = db.tbl_Efficiancy_Technician_Commissions.Select(x => x.payroll_id).Distinct();

            List<tbl_employee_payroll_dates> pdates = new List<tbl_employee_payroll_dates>();

            foreach (var payrolid in unqPayroll)
            {
                pdates.Add(db.tbl_employee_payroll_dates.Where(x => x.payroll_Id.ToString() == payrolid).OrderByDescending(x => x.payroll_Id).FirstOrDefault());
            }

            modal.DataResources = pdates.OrderByDescending(x => x.payroll_Id).ToList();
            modal.ImportHistory = db.tbl_data_import_history.ToList();

            modal.LabourRatesList = db.tbl_source_labour_rates.ToList();

            return View(modal);
        }

        [HttpPost]
        public ActionResult UploadCSVCustomerList(HttpPostedFileBase postedFile)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Trace.WriteLine("The drag and drop function");

            string filePath = string.Empty;
            if (postedFile != null)
            {
                string path = Server.MapPath("~/Content/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);

                //Read the contents of CSV file.
                string csvData = System.IO.File.ReadAllText(filePath);
                Trace.WriteLine(csvData.Split('\n')[0]);
                string[] Headercell = csvData.Split('\n')[0].Split(',');
                if (Headercell[0] == "CustomerCode" && Headercell[1] == "Address1" && Headercell[2] == "Address2" && Headercell[3] == "Address3" && Headercell[4] == "CustomerName" && Headercell[5] == "Fax" && Headercell[6] == "Postcode")
                {
                    
                    for (int i = 1; i < csvData.Split('\n').Length; i++)
                    {

                        tbl_cta_customers cust = new tbl_cta_customers();
                        string[] cell = csvData.Split('\n')[i].Split(',');
                        int custm = Convert.ToInt32(cell[0]);
                        var custUpdt = db.tbl_cta_customers.Where(x => x.CustomerCode == custm).FirstOrDefault();
                        if (custUpdt != null)
                        {
                            custUpdt.Address1 = cell[1];
                            custUpdt.Address2 = cell[2];
                            custUpdt.Address3 = cell[3];
                            custUpdt.CustomerName = cell[4];
                            custUpdt.Fax = cell[5];
                            custUpdt.Postcode = cell[6];
                            custUpdt.Telephone = cell[7];
                            db.SaveChanges();
                        }
                        else
                        {
                            cust.CustomerCode = Convert.ToInt32(cell[0]);
                            cust.Address1 = cell[1];
                            cust.Address2 = cell[2];
                            cust.Address3 = cell[3];
                            cust.CustomerName = cell[4];
                            cust.Fax = cell[5];
                            cust.Postcode = cell[6];
                            cust.Telephone = cell[7];
                            db.tbl_cta_customers.Add(cust);
                            db.SaveChanges();
                        }
                    }

                    tbl_data_import_history importInfo = new tbl_data_import_history();
                    importInfo.data_type = "CustomerList";
                    importInfo.performed_by = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();
                    importInfo.import_date = System.DateTime.Now;
                    db.tbl_data_import_history.Add(importInfo);

                    db.SaveChanges();
                    return RedirectToAction("DataResources", "Settings", new { ack = "DataImported" });
                }

            }
            
            return RedirectToAction("DataResources", "Settings");
        }

        
         
            [HttpPost]
        public ActionResult UploadCSVbranchSalesReport(HttpPostedFileBase branchSalesReport)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            Trace.WriteLine("Reached 1");
            string month1 = "";
            string month2 = "";
            string filePath_ei = string.Empty;
            if (branchSalesReport != null)
            {
                Trace.WriteLine("Reached 2");
                string path = Server.MapPath("~/Content/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath_ei = path + Path.GetFileName(branchSalesReport.FileName);
                string extension = Path.GetExtension(branchSalesReport.FileName);
                branchSalesReport.SaveAs(filePath_ei);

                //Read the contents of CSV file.
                string csvData_ei = System.IO.File.ReadAllText(filePath_ei);
                Trace.WriteLine(csvData_ei.Split('\n')[0]);
                string[] Headercell_ei = csvData_ei.Split('\n')[0].Split(',');
                for (int i = 1; i < csvData_ei.Split('\n').Length; i++)
                {
                    string[] cell_ei = csvData_ei.Split('\n')[i].Split(',');
                    if (csvData_ei.Split('\n')[i] != "")
                    {
                        if (cell_ei[0] == "Account Code")
                        {
                            month1 = cell_ei[3].Split(' ')[0];
                            month2 = cell_ei[4].Split(' ')[0];
                        }

                        if (cell_ei[0].StartsWith("300") && cell_ei[1].Trim() != "")
                        {
                            tbl_salesReport_branch_monthly newReportMonth1 = new tbl_salesReport_branch_monthly();
                            newReportMonth1.loc_id = cell_ei[1];
                            newReportMonth1.year = System.DateTime.Today.Year.ToString();
                            newReportMonth1.month = month1;
                            if (cell_ei[3].Trim() != "")
                            {
                                newReportMonth1.total_sale = Convert.ToDouble(cell_ei[3]);
                            }
                            else
                            {
                                newReportMonth1.total_sale = 0;
                            }
                            var updateMonth1 = db.tbl_salesReport_branch_monthly.Where(x => x.loc_id == newReportMonth1.loc_id && x.year == newReportMonth1.year && x.month == newReportMonth1.month).FirstOrDefault();
                            if (updateMonth1 != null)
                            {
                                if (newReportMonth1.total_sale != 0)
                                {
                                    Trace.WriteLine(newReportMonth1.total_sale + " update " + newReportMonth1.loc_id);
                                    updateMonth1.total_sale = newReportMonth1.total_sale;
                                    db.SaveChanges();
                                }                                
                            }
                            else
                            {
                                Trace.WriteLine(newReportMonth1.total_sale + " insert " + newReportMonth1.loc_id);
                                db.tbl_salesReport_branch_monthly.Add(newReportMonth1);
                                db.SaveChanges();
                            }
                            

                            tbl_salesReport_branch_monthly newReportMonth2 = new tbl_salesReport_branch_monthly();
                            newReportMonth2.loc_id = cell_ei[1];
                            newReportMonth2.year = System.DateTime.Today.Year.ToString();
                            newReportMonth2.month = month2;
                            if (cell_ei[4].Trim() != "")
                            {
                                newReportMonth2.total_sale = Convert.ToDouble(cell_ei[4]);
                            }
                            else
                            {
                                newReportMonth2.total_sale = 0;
                            }
                            var updateMonth2 = db.tbl_salesReport_branch_monthly.Where(x => x.loc_id == newReportMonth2.loc_id && x.year == newReportMonth2.year && x.month == newReportMonth2.month).FirstOrDefault();
                            if (updateMonth2 != null)
                            {
                                if (newReportMonth2.total_sale != 0)
                                {
                                    Trace.WriteLine(newReportMonth2.total_sale + " update " + newReportMonth2.loc_id);
                                    updateMonth2.total_sale = newReportMonth2.total_sale;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                Trace.WriteLine(newReportMonth2.total_sale + " insert " + newReportMonth2.loc_id);
                                db.tbl_salesReport_branch_monthly.Add(newReportMonth2);
                                db.SaveChanges();
                            }
                            

                            
                        }
                        
                    }

                }

            }
            
            tbl_data_import_history importInfo = new tbl_data_import_history();
            importInfo.data_type = "BranchSalesReport" + ";" + month1;
            importInfo.performed_by = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();
            importInfo.import_date = System.DateTime.Now;
            db.tbl_data_import_history.Add(importInfo);

            db.SaveChanges();
            //return RedirectToAction("DataResources", "Settings", new { ack = "DataImported" });
            return RedirectToAction("DataResources", "Settings");
        }

        
        [HttpPost]
        public ActionResult UploadCSVpayrollDeductions(HttpPostedFileBase payrollEI, HttpPostedFileBase payrollCPP)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            Trace.WriteLine("Reached 1");

            string filePath_ei = string.Empty;
            if (payrollEI != null)
            {
                Trace.WriteLine("Reached 2");
                string path = Server.MapPath("~/Content/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath_ei = path + Path.GetFileName(payrollEI.FileName);
                string extension = Path.GetExtension(payrollEI.FileName);
                payrollEI.SaveAs(filePath_ei);

                //Read the contents of CSV file.
                string csvData_ei = System.IO.File.ReadAllText(filePath_ei);
                Trace.WriteLine(csvData_ei.Split('\n')[0]);
                string[] Headercell_ei = csvData_ei.Split('\n')[0].Split(',');
                Trace.WriteLine(";" + Headercell_ei[0] + ";");
                Trace.WriteLine(";" + Headercell_ei[1] + ";");
                Trace.WriteLine(";" + Headercell_ei[2] + ";");
                Trace.WriteLine(";" + Headercell_ei[3] + ";");
                Trace.WriteLine(";" + Headercell_ei[4] + ";");
                Trace.WriteLine(";" + Headercell_ei[5] + ";");
                if (Headercell_ei[0] == "EI" && Headercell_ei[1] == "Annual max insurable earnings" && Headercell_ei[2] == "Employee contribution rate" && Headercell_ei[3] == "Employer contribution rate" && Headercell_ei[4] == "Annual max employee premium" && Headercell_ei[5].Contains("Annual max employer premium"))
                {
                    Trace.WriteLine("Reached 3");
                    for (int i = 1; i < csvData_ei.Split('\n').Length; i++)
                    {
                        Trace.WriteLine("Reached 4");
                        tbl_cta_customers cust = new tbl_cta_customers();
                        string[] cell_ei = csvData_ei.Split('\n')[i].Split(',');
                        if (csvData_ei.Split('\n')[i] != "")
                        {
                            string EI = cell_ei[0];
                            var EIUpdate = db.tbl_source_payrollDeductions_ei.Where(x => x.EI == EI).FirstOrDefault();
                            if (EIUpdate != null)
                            {
                                Trace.WriteLine("Reached 5");
                                EIUpdate.Annual_max_insurable_earnings = Convert.ToDouble(cell_ei[1]);
                                EIUpdate.Employee_contribution_rate = Convert.ToDouble(cell_ei[2]);
                                EIUpdate.Employer_contribution_rate = Convert.ToDouble(cell_ei[3]);
                                EIUpdate.Annual_max_employee_premium = Convert.ToDouble(cell_ei[4]);
                                EIUpdate.Annual_max_employer_premium = Convert.ToDouble(cell_ei[5]);
                                db.SaveChanges();
                            }
                            else
                            {
                                Trace.WriteLine("Reached 6");
                                tbl_source_payrollDeductions_ei new_ei = new tbl_source_payrollDeductions_ei();
                                new_ei.EI = cell_ei[0];
                                new_ei.Annual_max_insurable_earnings = Convert.ToDouble(cell_ei[1]);
                                new_ei.Employee_contribution_rate = Convert.ToDouble(cell_ei[2]);
                                new_ei.Employer_contribution_rate = Convert.ToDouble(cell_ei[3]);
                                new_ei.Annual_max_employee_premium = Convert.ToDouble(cell_ei[4]);
                                new_ei.Annual_max_employer_premium = Convert.ToDouble(cell_ei[5]);
                                db.tbl_source_payrollDeductions_ei.Add(new_ei);
                                db.SaveChanges();
                            }
                        }
                        
                    }
                    
                }

            }


            string filePath_cpp = string.Empty;
            if (payrollCPP != null)
            {
                Trace.WriteLine("Reached 7");
                string path = Server.MapPath("~/Content/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath_cpp = path + Path.GetFileName(payrollCPP.FileName);
                string extension = Path.GetExtension(payrollCPP.FileName);
                payrollCPP.SaveAs(filePath_cpp);

                //Read the contents of CSV file.
                string csvData_cpp = System.IO.File.ReadAllText(filePath_cpp);
                Trace.WriteLine(csvData_cpp.Split('\n')[0]);
                string[] Headercell_cpp = csvData_cpp.Split('\n')[0].Split(',');
                if (Headercell_cpp[0] == "CPP/QPP" && Headercell_cpp[1] == "YMPE" && Headercell_cpp[2] == "Basic Exemption" && Headercell_cpp[3] == "Max contributory earnings YMCE" && Headercell_cpp[4] == "Employee contribution rate" && Headercell_cpp[5] == "Employee max contribution" && Headercell_cpp[6] == "Self-employed max contribution" && Headercell_cpp[7].Contains("YMPE before rounding"))
                {
                    Trace.WriteLine("Reached 8");
                    for (int i = 1; i < csvData_cpp.Split('\n').Length; i++)
                    {
                        Trace.WriteLine("Reached 9");
                        tbl_cta_customers cust = new tbl_cta_customers();
                        string[] cell_cpp = csvData_cpp.Split('\n')[i].Split(',');
                        if (csvData_cpp.Split('\n')[i] != "")
                        {
                            string CPP = cell_cpp[0];
                            var cppUpdate = db.tbl_source_payrollDeductions_cpp.Where(x => x.CPP_QPP == CPP).FirstOrDefault();
                            if (cppUpdate != null)
                            {
                                Trace.WriteLine("Reached 10");
                                cppUpdate.YMPE = Convert.ToDouble(cell_cpp[1]);
                                cppUpdate.Basic_Exemption = Convert.ToDouble(cell_cpp[2]);
                                cppUpdate.Max_contributory_earnings_YMCE = Convert.ToDouble(cell_cpp[3]);
                                cppUpdate.Employee_contribution_rate = Convert.ToDouble(cell_cpp[4]);
                                cppUpdate.Employee_max_contribution = Convert.ToDouble(cell_cpp[5]);
                                cppUpdate.Self_employed_max_contribution = Convert.ToDouble(cell_cpp[6]);
                                cppUpdate.YMPE_before_rounding = Convert.ToDouble(cell_cpp[7]);
                                db.SaveChanges();
                            }
                            else
                            {
                                Trace.WriteLine("Reached 11");
                                tbl_source_payrollDeductions_cpp new_cpp = new tbl_source_payrollDeductions_cpp();
                                new_cpp.CPP_QPP = cell_cpp[0];
                                new_cpp.YMPE = Convert.ToDouble(cell_cpp[1]);
                                new_cpp.Basic_Exemption = Convert.ToDouble(cell_cpp[2]);
                                new_cpp.Max_contributory_earnings_YMCE = Convert.ToDouble(cell_cpp[3]);
                                new_cpp.Employee_contribution_rate = Convert.ToDouble(cell_cpp[4]);
                                new_cpp.Employee_max_contribution = Convert.ToDouble(cell_cpp[5]);
                                new_cpp.Self_employed_max_contribution = Convert.ToDouble(cell_cpp[6]);
                                new_cpp.YMPE_before_rounding = Convert.ToDouble(cell_cpp[7]);
                                db.tbl_source_payrollDeductions_cpp.Add(new_cpp);
                                db.SaveChanges();
                            }
                        }                        
                    }

                }

            }

            tbl_data_import_history importInfo = new tbl_data_import_history();
            importInfo.data_type = "PayrollDeductions";
            importInfo.performed_by = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();
            importInfo.import_date = System.DateTime.Now;
            db.tbl_data_import_history.Add(importInfo);

            db.SaveChanges();
            //return RedirectToAction("DataResources", "Settings", new { ack = "DataImported" });
            return RedirectToAction("DataResources", "Settings");
        }


    }
}