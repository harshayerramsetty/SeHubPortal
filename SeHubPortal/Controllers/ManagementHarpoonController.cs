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
    public class ManagementHarpoonController : Controller
    {
        // GET: ManagementHarpoon
        public ActionResult Index()
        {
            return View();
        }

        private static List<SelectListItem> populateLocationsNames(string clientID, string email)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where( x => x.client_id == clientID).ToList();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();

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

        private static List<SelectListItem> populateLocationsPermissions(string clientID, string email)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();

            items.Add(new SelectListItem
            {
                Text = "All",
                Value = "All"
            });

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

        public JsonResult GetSwipDetails(string date, string empid, string locid)
        {
            //Trace.WriteLine("This is the date " + date + "This is the employee_id " + empid);
            //Trace.WriteLine("This is the employee ID " + empid);

            var itemslist = new List<string>();
            var itemslist1 = new List<string>();

            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                string uesrEmail = Session["userID"].ToString();

                var user = dc.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

                var clientID = user.client_id;

                string location = dc.tbl_harpoon_locations.Where(x => x.location_id == locid && x.client_id == clientID).Select(x => x.auto_loc_id).FirstOrDefault().ToString();

                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select time_stamp, event_id, comments from tbl_harpoon_attendance_log where time_stamp > '" + date + "'" + "and time_stamp < '" + date + " 23:59:00.000" + "'" + "and auto_emp_id = '" + empid + "'" + "and auto_loc_id = '" + location + "'" + "order by time_stamp asc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string timeStamp;
                                string eventID;
                                string comments;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                eventID = Convert.ToString(sdr["event_id"]);
                                comments = Convert.ToString(sdr["comments"]);
                                itemslist.Add(timeStamp + ">" + eventID + ">" + comments);
                            }
                        }
                        con.Close();
                    }
                }
            }

            var finalTimeStampList = itemslist.ToList();

            return new JsonResult { Data = finalTimeStampList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetEmpDetails(int empID)
        {
            TimeClockEvent EmpDetails = new TimeClockEvent();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var emp = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == empID).FirstOrDefault();

            EmpDetails.auto_loc_id = emp.auto_loc_id.Value;
            EmpDetails.auto_emp_id = emp.auto_emp_id;
            EmpDetails.client_id = emp.client_id;

            return new JsonResult { Data = EmpDetails, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public ActionResult AddEvent(AttendanceModel model)
        {

            //Trace.WriteLine(System.DateTime.Today.Date.ToString().Substring(0, 10) + " " + model.CreateEvent.time_stamp +":00:000" + "This is the time stamp");

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            var empAttendance = db.tbl_harpoon_employee_attendance.Where(x => x.auto_emp_id == model.CreateEvent.auto_emp_id).FirstOrDefault();

            DateTime outTime;
            string TimeString = System.DateTime.Today.Date.ToString().Substring(0, 10) + " " + model.CreateEvent.time_stamp + ":00:000";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            if (model.CreateEvent.event_id == "clockIN" || model.CreateEvent.event_id == "adminIN")
            {
                empAttendance.at_work = true;
                empAttendance.at_work_location = model.CreateEvent.auto_loc_id.ToString();
            }
            else
            {
                empAttendance.at_work = false;
                empAttendance.at_work_location = model.CreateEvent.auto_loc_id.ToString();

                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select top 1 time_stamp from tbl_harpoon_attendance_log where auto_emp_id = " + model.CreateEvent.auto_emp_id + " and (event_id = 'clockIN' or event_id = 'adminIN') order by time_stamp desc";
                    //Debug.WriteLine("Query:" + query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                outTime = Convert.ToDateTime(sdr["time_stamp"]);
                                if (Convert.ToDateTime((System.DateTime.Today.Date.ToString("yyy-MM-dd") + " " + model.CreateEvent.time_stamp + ":00.000").Substring(0, 18)) > outTime)
                                {
                                    TimeString = System.DateTime.Today.Date.ToString("yyy-MM-dd") + " " + model.CreateEvent.time_stamp + ":00.000";
                                }
                                else
                                {
                                    TimeString = outTime.AddSeconds(1).ToString("yyyy-MM-dd hh:mm tt");
                                }
                            }

                        }
                        con.Close();
                    }
                }
            }

            db.SaveChanges();

            string timeStamp = "";
            using (SqlConnection connection = new SqlConnection(constr))
            {
                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO tbl_harpoon_attendance_log VALUES (@auto_loc_id, @auto_emp_id, @event_id, @time_stamp , @client_id, @comments)");

                string sql = sb.ToString();
                //Trace.WriteLine(sql);
                using (SqlCommand command = new SqlCommand(sql, connection))
                {

                    

                    command.Parameters.AddWithValue("@auto_loc_id", model.CreateEvent.auto_loc_id);
                    command.Parameters.AddWithValue("@auto_emp_id", model.CreateEvent.auto_emp_id);
                    command.Parameters.AddWithValue("@event_id", model.CreateEvent.event_id);

                    if (model.CreateEvent.event_id == "clockIN" || model.CreateEvent.event_id == "adminIN")
                    {
                        timeStamp = System.DateTime.Today.Date.ToString("yyyy-MM-dd") + " " + model.CreateEvent.time_stamp + ":00.000";
                    }
                    else
                    {
                        timeStamp = TimeString;
                    }

                    command.Parameters.AddWithValue("@time_stamp", timeStamp);

                    command.Parameters.AddWithValue("@client_id", clientID);
                    if (model.CreateEvent.comments != null)
                    {
                        command.Parameters.AddWithValue("@comments", model.CreateEvent.comments);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@comments", "");
                    }

                    Trace.WriteLine(command.CommandText);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                connection.Close();
            }

            return RedirectToAction("TimeClockEvents");

        }

        public string GetLocatID(string locid)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            return db.tbl_harpoon_locations.Where(x => x.auto_loc_id.ToString() == locid).Select(x => x.location_id).FirstOrDefault();

        }

        [HttpGet]
        public ActionResult Attendance(string locId, string employeeId)
        {

            AttendanceModel model = new AttendanceModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            model.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                model.multipleLocation = true;
            }
            else
            {
                model.multipleLocation = false;
            }

            var locationDetails = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).FirstOrDefault();
            string location = "AHO";
            
            if (locId is null)
            {
                if (locationDetails != null)
                {
                    location = locationDetails.auto_loc_id.Value.ToString();
                }
            }
            else
            {
                location = locId;
            }

            //Trace.WriteLine("This is the location" + location);

            //var employeeList = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == location && x.client_id == clientID).OrderBy(x => x.last_name).ThenBy(n => n.first_name).ToList();

            DateTime current = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

            List<KeyValuePair<int, string>> keyValuePair = new List<KeyValuePair<int, string>>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct a.auto_emp_id, b.first_name, b.last_name, b.status from tbl_harpoon_attendance_log a, tbl_harpoon_employee b where a.auto_loc_id = '" + location + "' and a.auto_emp_id = b.auto_emp_id and time_stamp > '" + current + "' and time_stamp < '" + current.AddDays(28) + "'";
                Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Trace.WriteLine("Reached till 2");
                            int emp;
                            string fulnam;
                            string status;

                            if (sdr["status"].ToString() == "1")
                            {
                                status = "Active";
                            }
                            else
                            {
                                status = "InActive";
                            }

                            emp = Convert.ToInt32(sdr["auto_emp_id"]);
                            fulnam = Convert.ToString(sdr["last_name"]) + ", " + Convert.ToString(sdr["first_name"]) + ";" + status;

                            Trace.WriteLine("This is the emp ID " + emp + " This is the full name" + fulnam);

                            keyValuePair.Add(new KeyValuePair<int, string>(emp, fulnam));
                            Trace.WriteLine("Reached till 3");
                        }

                    }
                    con.Close();
                }
            }


            List<EmployeeAttendanceListModel> emplyAttList = new List<EmployeeAttendanceListModel>();
            foreach (var item in keyValuePair)
            {
                EmployeeAttendanceListModel obj = new EmployeeAttendanceListModel(); // ViewModel
                //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                var employee = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.Key).FirstOrDefault();
                obj.employeeId = employee.auto_emp_id.ToString();
                obj.fullName = employee.last_name + ", " + employee.first_name;
                if (employee.status == 1)
                {
                    obj.atWork = "Active";
                }
                else
                {
                    obj.atWork = "InActive";
                }
                emplyAttList.Add(obj);
            }

            var itemslist = new List<int>();

            DateTime todayDate = DateTime.Today.AddDays(0); //-14

            var Payroll = db.tbl_employee_payroll_dates.Where(x => x.start_date <= todayDate && x.end_date >= todayDate).FirstOrDefault();

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct auto_emp_id from tbl_harpoon_attendance_log where time_stamp > '" + Payroll.start_date + "' and time_stamp < '" + Payroll.end_date + "' and auto_loc_id = '" + location + "'";
                //Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            string emp;
                            string fulnam;
                            emp = Convert.ToString(sdr["auto_emp_id"]);

                            itemslist.Add(Convert.ToInt32(emp));
                        }

                    }
                    con.Close();
                }
            }

            List<EmployeeAttendanceListModel> empchlococatList = new List<EmployeeAttendanceListModel>();

            foreach (var val in itemslist)
            {
                //Trace.WriteLine(val);
                EmployeeAttendanceListModel empchlococat = new EmployeeAttendanceListModel();
                if (val != 0)
                {

                    var EmpSwipedInLocation = db.tbl_harpoon_employee.Where(x => x.employee_id == val.ToString() && x.client_id == clientID).FirstOrDefault();
                    if (EmpSwipedInLocation != null)
                    {
                        if (EmpSwipedInLocation.auto_loc_id.ToString() != location)
                        {
                            empchlococat.employeeId = EmpSwipedInLocation.employee_id.ToString();
                            empchlococat.fullName = EmpSwipedInLocation.first_name + "," + EmpSwipedInLocation.last_name;
                            string loc;
                            string eventType;

                            using (SqlConnection con = new SqlConnection(constr))
                            {
                                string query = "select top 1 * from tbl_attendance_log where auto_emp_id = '" + val + "' order by time_stamp desc ";
                                //Debug.WriteLine(query);
                                using (SqlCommand cmd = new SqlCommand(query))
                                {
                                    cmd.Connection = con;
                                    con.Open();
                                    using (SqlDataReader sdr = cmd.ExecuteReader())
                                    {
                                        while (sdr.Read())
                                        {
                                            loc = Convert.ToString(sdr["loc_id"]);
                                            eventType = Convert.ToString(sdr["event_id"]);

                                            if (loc == location && eventType == "clockIN")
                                            {
                                                empchlococat.atWork = "True";
                                            }
                                            else
                                            {
                                                empchlococat.atWork = "False";
                                            }

                                        }

                                    }
                                    con.Close();
                                }
                            }
                            empchlococatList.Add(empchlococat);
                        }
                    }
                }

            }

            //Trace.WriteLine("Reached");
            model.employeeListChangeLocation = empchlococatList;
            model.employeeList = emplyAttList;

            if (model.multipleLocation)
            {
                model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail);
            }
            else
            {
                model.MatchedLocs = populateLocationsNames(clientID, uesrEmail);
            }

            
            model.MatchedLocID = location;

            if (employeeId is null)
            {
                employeeId = keyValuePair.OrderBy(x => x.Value).Select(x => x.Key).FirstOrDefault().ToString();
            }

            //Trace.WriteLine("First Emp with emp ID" + employeeId);

            if (employeeId is null)
            {

            }
            else
            {
                if (employeeId != null || employeeId != "")
                {
                    model.SelectedEmployeeId = employeeId;
                    //Debug.WriteLine("In ShowEditCustInfo");
                    int employeeIdNum = Convert.ToInt32(employeeId);
                    var EmployeeDetails = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == employeeIdNum && x.client_id == clientID).FirstOrDefault();
                    string empId1 = "";
                    int auto_emp_id = 0;
                    string locId1 = "";
                    string position = "";
                    string fullName = "";
                    byte[] profileImg = null;
                    if (EmployeeDetails != null)
                    {
                        empId1 = EmployeeDetails.employee_id.ToString();
                        auto_emp_id = EmployeeDetails.auto_emp_id;
                        locId1 = EmployeeDetails.auto_loc_id.Value.ToString();
                        fullName = EmployeeDetails.first_name + " " + EmployeeDetails.last_name;
                        position = EmployeeDetails.position;
                        profileImg = EmployeeDetails.profile_pic;
                    }
                    string base64ProfilePic = "";
                    if (profileImg is null)
                    {
                        base64ProfilePic = "";
                    }
                    else
                    {
                        base64ProfilePic = Convert.ToBase64String(profileImg);
                    }

                    //Debug.WriteLine("Image_base64:" + base64ProfilePic);
                    ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
                    ViewData["EmployeeName"] = fullName;
                    ViewData["EmployeeId"] = empId1;
                    if(auto_emp_id != 0)
                    {
                        ViewData["auto_emp_id"] = auto_emp_id.ToString();
                    }
                    ViewData["Position"] = position;
                }
            }

            return View(model);
        }

        public JsonResult GetAttendanceDates(string empid)
        {
            //Trace.WriteLine("This is the employee ID " + empid);

            var itemslist = new List<string>();

            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select time_stamp, event_id, auto_loc_id from tbl_harpoon_attendance_log where auto_emp_id = '" + empid + "'" + "order by time_stamp asc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string timeStamp;
                                string locID;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                locID = Convert.ToString(sdr["auto_loc_id"]);
                                itemslist.Add(timeStamp.Split(' ')[0] + ";" + locID);
                            }
                        }
                        con.Close();
                    }
                }
            }

            var finalTimeStampList = itemslist.Distinct().ToList();

            return new JsonResult { Data = finalTimeStampList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult ChangeLocAttendance(AttendanceModel model)
        {
            return RedirectToAction("Attendance", new { locId = model.MatchedLocID, employeeId = "" });
        }

        [HttpPost]
        public ActionResult ChangeLocTimeClockEvent(AttendanceModel model)
        {
            return RedirectToAction("TimeClockEvents", new { locId = model.MatchedLocID, employeeId = "" });
        }

        public ActionResult TimeClockEvents(string locId, string employeeId)
        {
            AttendanceModel model = new AttendanceModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();


            string uesrEmail = Session["userID"].ToString();

            model.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                model.multipleLocation = true;
            }
            else
            {
                model.multipleLocation = false;
            }

            var locationDetails = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).FirstOrDefault();
            string location = "AHO";


            

            if (locId is null)
            {

                if (user.profile == "Administrator" || user.profile == "Global Manager")
                {
                    location = "ALL";
                }
                else
                {
                    if (locationDetails != null)
                    {
                        location = locationDetails.auto_loc_id.Value.ToString();
                    }
                }
            }
            else
            {
                location = locId;
            }

            TimeClockEvent timeEvent = new TimeClockEvent();
            if(location != "ALL")
            {
                timeEvent.auto_loc_id = Convert.ToInt32(location);
            }
            timeEvent.location_id = db.tbl_harpoon_locations.Where(x => x.auto_loc_id.ToString() == location).Select(x => x.location_id).FirstOrDefault();

            model.CreateEvent = timeEvent;

            //Trace.WriteLine("This is the location" + location);

            List<EmployeeAttendanceListModel> emplyAttList = new List<EmployeeAttendanceListModel>();

            if (location == "ALL")
            {
                var employeeList = db.tbl_harpoon_employee.Where(x => x.status == 1 && x.client_id == clientID).OrderBy(x => x.last_name).ThenBy(n => n.first_name).ToList();
                Trace.WriteLine("Reached");
                foreach (var item in employeeList)
                {
                    EmployeeAttendanceListModel obj = new EmployeeAttendanceListModel(); // ViewModel
                                                                                         //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                    obj.employeeId = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.employee_id).FirstOrDefault();
                    obj.auto_emp_id = item.auto_emp_id;
                    obj.profilePic = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.profile_pic).FirstOrDefault();
                    obj.fullName = item.last_name + ", " + item.first_name;
                    obj.atWork = db.tbl_harpoon_employee_attendance.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.at_work).FirstOrDefault().ToString();
                    obj.position = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.position).FirstOrDefault().ToString();
                    obj.events = new List<TimeChardViewModel>();

                    string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select time_stamp, event_id, comments from tbl_harpoon_attendance_log where time_stamp > '" + System.DateTime.Now.Date + "'" + " and auto_emp_id = '" + item.auto_emp_id + "'" + "order by time_stamp asc";
                        //Debug.WriteLine(query);
                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            con.Open();
                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {
                                    TimeChardViewModel card = new TimeChardViewModel();

                                    card.timeStamp = Convert.ToDateTime(sdr["time_stamp"]);
                                    card.eventID = Convert.ToString(sdr["event_id"]);
                                    card.comments = Convert.ToString(sdr["comments"]);
                                    obj.events.Add(card);
                                }
                            }
                            con.Close();
                        }
                    }

                    emplyAttList.Add(obj);

                }
            }
            else
            {
                var employeeList = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == location && x.status == 1 && x.client_id == clientID).OrderBy(x => x.last_name).ThenBy(n => n.first_name).ToList();
                foreach (var item in employeeList)
                {
                    EmployeeAttendanceListModel obj = new EmployeeAttendanceListModel(); // ViewModel
                                                                                         //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                    obj.employeeId = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.employee_id).FirstOrDefault();
                    obj.auto_emp_id = item.auto_emp_id;
                    obj.profilePic = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.profile_pic).FirstOrDefault();
                    obj.fullName = item.last_name + ", " + item.first_name;
                    obj.atWork = db.tbl_harpoon_employee_attendance.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.at_work).FirstOrDefault().ToString();
                    obj.position = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.position).FirstOrDefault().ToString();
                    obj.events = new List<TimeChardViewModel>();

                    string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select time_stamp, event_id, comments from tbl_harpoon_attendance_log where time_stamp > '" + System.DateTime.Now.Date + "'" + " and auto_emp_id = '" + item.auto_emp_id + "'" + "order by time_stamp asc";
                        //Debug.WriteLine(query);
                        using (SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            con.Open();
                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {
                                    TimeChardViewModel card = new TimeChardViewModel();

                                    card.timeStamp = Convert.ToDateTime(sdr["time_stamp"]);
                                    card.eventID = Convert.ToString(sdr["event_id"]);
                                    card.comments = Convert.ToString(sdr["comments"]);
                                    obj.events.Add(card);
                                }
                            }
                            con.Close();
                        }
                    }

                    emplyAttList.Add(obj);

                }
            }

            

            model.employeeList = emplyAttList;

            model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail);

            model.MatchedLocID = location;

            //Trace.WriteLine("First Emp with emp ID" + employeeId);

            

            return View(model);
        }

        public String ContainerNameEmployeeFiles = "harpoon-employee-files";
        [HttpGet]
        public ActionResult EmployeeFolder(string locId, string employeeId)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            int empId = 0;

            if (employeeId == null)
            {
                empId = db.tbl_harpoon_employee.Where(x => x.client_id == clientID).OrderBy(x => x.last_name).Select(x => x.auto_emp_id).FirstOrDefault();
            }
            else
            {
                empId = Convert.ToInt32(employeeId);
            }

            
            
            FileURL FileUrl = new FileURL();

            var empDetails = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == empId).FirstOrDefault();

            List<tbl_harpoon_employee> employeeListLocation = new List<tbl_harpoon_employee>();

            if (locId == null)
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id == empDetails.auto_loc_id).OrderBy(x => x.last_name).ToList();
            }
            else
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == locId).OrderBy(x => x.last_name).ToList();
            }

            List<EmployeeAttendanceListModel> EmpDetailsList = new List<EmployeeAttendanceListModel>();
            foreach (var emp in employeeListLocation)
            {
                EmployeeAttendanceListModel empdetails = new EmployeeAttendanceListModel();
                empdetails.employeeId = emp.auto_emp_id.ToString()+ ";" + emp.status;
                empdetails.fullName = emp.last_name + ", " + emp.first_name;
                EmpDetailsList.Add(empdetails);
            }

            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();

            var blobList = blockBlob.ToList();

            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");

                if (blobFileName.Contains(empDetails.auto_emp_id.ToString()))
                {
                    blobFileName = blobFileName.Replace(empDetails.auto_emp_id.ToString() + "_", "");
                    URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
                }
            }

            FileUrl.URLName = URLNames;
            FileUrl.employeeList = EmpDetailsList;
            FileUrl.Location_ID = empDetails.auto_loc_id.ToString();
            FileUrl.LocationsList = populateLocationsPermissions(clientID, user.email);
            FileUrl.SelectedEmployeeId = empId.ToString();

            return View(FileUrl);
        }

        [HttpPost]
        public ActionResult UploadEmployeeFolder(HttpPostedFileBase CompanyDocument, FileURL model)
        {
            //int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID = db.tbl_harpoon_employee.Where(x => x.auto_emp_id.ToString() == model.SelectedEmployeeId).Select(x => x.auto_emp_id).FirstOrDefault().ToString();

            string imageName;

            if (model.SelectedEmployeeId != null)
            {
                imageName = model.SelectedEmployeeId + "_" + Path.GetFileName(CompanyDocument.FileName);
            }
            else
            {
                imageName = empID + "_" + Path.GetFileName(CompanyDocument.FileName);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = CompanyDocument.ContentType;

            Blob.UploadFromStream(CompanyDocument.InputStream);

            return RedirectToAction("EmployeeFolder", "ManagementHarpoon");
        }

        public ActionResult DeleteEmployeeFolder(string fileName, string employeeID)
        {
            //int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID = db.tbl_harpoon_employee.Where(x => x.auto_emp_id.ToString() == employeeID).Select(x => x.auto_emp_id).FirstOrDefault().ToString();

            if (employeeID == null)
            {
                fileName = empID + "_" + fileName.Remove(fileName.Length - 1) + ".pdf";
            }
            else
            {
                fileName = employeeID + "_" + fileName.Remove(fileName.Length - 1) + ".pdf";
            }

            Debug.WriteLine(fileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("EmployeeFolder", "ManagementHarpoon");
        }

        [HttpPost]
        public ActionResult RenameEmployeeFolder(string currentFileName, string newFileName, FileURL model)
        {

            //int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID;

            empID = model.SelectedEmployeeId;

            currentFileName = empID + "_" + currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            var blob = container.GetBlockBlobReference(currentFileName);
            var blob1 = container.GetBlockBlobReference(empID + "_" + model.RenameString + ".pdf");

            blob1.StartCopy(blob);

            blob.DeleteIfExists();

            return RedirectToAction("EmployeeFolder", "ManagementHarpoon");
        }

        [HttpPost]
        public ActionResult ChangeLocEmployeeFolder(FileURL model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID = db.tbl_employee.Where(x => x.loc_ID == model.Location_ID && x.status == 1).OrderBy(x => x.full_name).Select(x => x.employee_id).FirstOrDefault().ToString();

            return RedirectToAction("EmployeeFiles", new { locId = model.Location_ID, employeeId = empID });
        }

        public ActionResult populateEmployeeList(DateTime date, string loc)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            List<KeyValuePair<int, string>> keyValuePair = new List<KeyValuePair<int, string>>();

            Trace.WriteLine("Reached till 1");

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct a.auto_emp_id, b.first_name, b.last_name, b.status from tbl_harpoon_attendance_log a, tbl_harpoon_employee b where a.auto_loc_id = '"+ loc +"' and a.auto_emp_id = b.auto_emp_id and time_stamp > '" + date + "' and time_stamp < '" + date.AddDays(28) + "'";
                Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Trace.WriteLine("Reached till 2");
                            int emp;
                            string fulnam;
                            string status;

                            if (sdr["status"].ToString() == "1")
                            {
                                status = "Active";
                            }
                            else
                            {
                                status = "InActive";
                            }

                            emp = Convert.ToInt32(sdr["auto_emp_id"]);
                            fulnam = Convert.ToString(sdr["first_name"]) + ", " + Convert.ToString(sdr["last_name"]) + ";" + status;

                            Trace.WriteLine("This is the emp ID " + emp + " This is the full name" + fulnam); 

                            keyValuePair.Add(new KeyValuePair<int, string>(emp, fulnam));
                            Trace.WriteLine("Reached till 3");
                        }

                    }
                    con.Close();
                }
            }

            Trace.WriteLine("Reached till 4");

            return Json(keyValuePair.Select(x => new
            {
                value = x.Key,
                text = x.Value
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        public string populateEmployeeDetails(int auto_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var emp = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == auto_id).FirstOrDefault();
            return emp.first_name + " " + emp.last_name + "seperator" + "data:image/png;base64," + Convert.ToBase64String(emp.profile_pic);
        }

    }
}