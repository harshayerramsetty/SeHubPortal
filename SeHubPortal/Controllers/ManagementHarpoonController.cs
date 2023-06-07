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

        private static List<SelectListItem> populateEmployees(string clientID, string location)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var EmployeeList = db.tbl_harpoon_employee.Where(x => x.client_id == clientID && x.auto_loc_id.ToString() == location).ToList();

            foreach (var emp in EmployeeList)
            {
                items.Add(new SelectListItem
                {
                    Text = emp.first_name + ", " + emp.last_name,
                    Value = emp.auto_emp_id.ToString()
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

        public JsonResult GetSwipDetails(string date, string empid, string locid)
        {

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
                    string query = "select time_stamp, event_id, job_id from tbl_harpoon_attendance_log where time_stamp > '" + date + "'" + "and time_stamp < '" + date + " 23:59:00.000" + "'" + "and auto_emp_id = '" + empid + "'" + "and auto_loc_id = '" + location + "'" + "order by time_stamp asc";
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
                                string job_id;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                eventID = Convert.ToString(sdr["event_id"]);
                                job_id = Convert.ToString(sdr["job_id"]);
                                itemslist.Add(timeStamp + ">" + eventID + ">" + job_id);
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
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            var empAttendance = db.tbl_harpoon_employee_attendance.Where(x => x.auto_emp_id == model.CreateEvent.auto_emp_id).FirstOrDefault();

            DateTime outTime;
            string TimeString = System.DateTime.Today.Date.ToString().Substring(0, 10) + " " + model.CreateEvent.time_stamp + ":00:000";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string job_id = "";

            if (model.CreateEvent.event_id == "AssignJob")
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select top 1 time_stamp, job_id from tbl_harpoon_attendance_log where auto_emp_id = " + model.CreateEvent.auto_emp_id + " and (event_id = 'clockIN' or event_id = 'adminIN') order by time_stamp desc";
                    //Debug.WriteLine("Query:" + query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                TimeString = sdr["time_stamp"].ToString().Substring(0, 18);
                                if (sdr["job_id"] != null)
                                {
                                    job_id = sdr["job_id"].ToString();
                                }

                            }

                        }
                        con.Close();
                    }
                }

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE ");
                    sb.Append("tbl_harpoon_attendance_log ");
                    sb.Append("SET job_id = @job_id ");
                    sb.Append("WHERE auto_emp_id = @empId and (event_id = 'clockIN' or event_id = 'adminIN') and time_stamp >= @timestamp");

                    string sql = sb.ToString();


                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (job_id != "")
                        {
                            command.Parameters.AddWithValue("@job_id", job_id + ";" + model.CreateEvent.job_id);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@job_id", model.CreateEvent.job_id);
                        }

                        command.Parameters.AddWithValue("@empId", model.CreateEvent.auto_emp_id);
                        command.Parameters.AddWithValue("@timestamp", TimeString);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();

                }

                tbl_harpoon_job_log newJob = new tbl_harpoon_job_log();
                newJob.job_id = model.CreateEvent.job_id;
                newJob.start_time = System.DateTime.Now;
                newJob.auto_emp_id = model.CreateEvent.auto_emp_id;
                db.tbl_harpoon_job_log.Add(newJob);
                db.SaveChanges();
            }
            else
            {
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
                    sb.Append("INSERT INTO tbl_harpoon_attendance_log VALUES (@auto_loc_id, @auto_emp_id, @event_id, @time_stamp , @client_id, @job_id, @comments)");

                    string sql = sb.ToString();
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
                        if (model.CreateEvent.job_id != null)
                        {
                            command.Parameters.AddWithValue("@job_id", model.CreateEvent.job_id);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@job_id", "");
                        }

                        if (model.CreateEvent.comments != null)
                        {
                            command.Parameters.AddWithValue("@comments", model.CreateEvent.comments);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@comments", "");
                        }

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }

            return RedirectToAction("TimeClockEvents");

        }

        [HttpPost]
        public ActionResult addAdminEvent(AttendanceModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            Trace.WriteLine("This is the date " + model.addAdminEventDateTime);
            Trace.WriteLine("This is the employee " + model.addAdminEventemp);
            Trace.WriteLine("This is the event " + model.addAdminEventEvent);

            tbl_harpoon_attendance_log addLog = new tbl_harpoon_attendance_log();

            string uesrEmail = Session["userID"].ToString();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            var clientID = user.client_id;

            addLog.auto_loc_id = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == model.addAdminEventemp).Select(x => x.auto_loc_id).FirstOrDefault().Value.ToString();
            addLog.auto_emp_id = model.addAdminEventemp;
            addLog.event_id = model.addAdminEventEvent;
            addLog.time_stamp = model.addAdminEventDateTime;
            addLog.client_id = clientID;

            db.tbl_harpoon_attendance_log.Add(addLog);
            db.SaveChanges();

            return RedirectToAction("Attendance");
        }



        [HttpPost]
        public ActionResult CloseJob(AttendanceModel model)
        {
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(constr))
            {
                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE ");
                sb.Append("tbl_harpoon_job_log ");
                sb.Append("SET end_time = @endTime ");
                sb.Append("WHERE job_id = @jobId and auto_emp_id = @empId and start_time > @currentDate and end_time is null");

                string sql = sb.ToString();


                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@jobId", model.openJobID);
                    command.Parameters.AddWithValue("@empId", model.openJobID_autoEmpID);
                    command.Parameters.AddWithValue("@currentDate", System.DateTime.Today.ToString("yyyy-MM-dd hh:mm tt"));
                    command.Parameters.AddWithValue("@endTime", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));

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

            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

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
                if(user.profile == "Administrator" || user.profile == "Global Manager")
                {
                    location = "All";
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

            string queryString = "";

            if (location != "All")
            {
                queryString = "a.auto_loc_id = '" + location + "' and";
            }
            else
            {
                queryString = "a.client_id = '" + user.client_id + "' and";
            }

            DateTime current = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            DateTime lastDay = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));

            List<KeyValuePair<int, string>> keyValuePair = new List<KeyValuePair<int, string>>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct a.auto_emp_id, b.first_name, b.last_name, b.status from tbl_harpoon_attendance_log a, tbl_harpoon_employee b where "+ queryString + " a.auto_emp_id = b.auto_emp_id and time_stamp > '" + current + "' and time_stamp < '" + lastDay + "'";
  
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {

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

                            keyValuePair.Add(new KeyValuePair<int, string>(emp, fulnam));
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

            model.employeeListChangeLocation = empchlococatList;
            model.employeeList = emplyAttList.OrderBy(x => x.fullName).ToList();

            if (model.multipleLocation)
            {
                model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail, false);
            }
            else
            {
                model.MatchedLocs = populateLocationsNames(clientID, uesrEmail, false);
            }

            
            model.MatchedLocID = location;

            if (employeeId is null)
            {
                employeeId = keyValuePair.OrderBy(x => x.Value).Select(x => x.Key).FirstOrDefault().ToString();
            }


            if (employeeId is null)
            {

            }
            else
            {
                if (employeeId != null || employeeId != "")
                {
                    model.SelectedEmployeeId = employeeId;
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

        public JsonResult GetAttendanceDates(string empid, int mon, int yar)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            DateTime startDate = new DateTime(yar, mon, 1).AddDays(-6); 
            DateTime endDate = new DateTime(yar, mon, System.DateTime.DaysInMonth(yar, mon)).AddDays(6); 

            var itemslist = new List<string>();

            var events = db.tbl_harpoon_attendance_log.Where(x => x.auto_emp_id.ToString() == empid && x.time_stamp > startDate && x.time_stamp < endDate).OrderBy(x => x.time_stamp);

            foreach (var evnt in events)
            {
                itemslist.Add(evnt.time_stamp.Date + ";" + evnt.auto_loc_id);
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

            model.AdminClockingEditTime = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.adminClocking_editTime).FirstOrDefault();

            model.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                model.multipleLocation = true;
            }
            else
            {
                model.multipleLocation = false;
            }

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.job_id_clocking).FirstOrDefault() == 1)
            {
                model.job_id_clocking = true;
            }
            else
            {
                model.job_id_clocking = false;
            }


            var locationDetails = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).FirstOrDefault();
            string location = "AHO";

            model.userProfile = user.profile;

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


            List<EmployeeAttendanceListModel> emplyAttList = new List<EmployeeAttendanceListModel>();

            if (location == "ALL")
            {
                var employeeList = db.tbl_harpoon_employee.Where(x => x.status == 1 && x.client_id == clientID).OrderBy(x => x.last_name).ThenBy(n => n.first_name).ToList();

                foreach (var item in employeeList)
                {
                    EmployeeAttendanceListModel obj = new EmployeeAttendanceListModel(); // ViewModel
                                                                                         //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                    obj.employeeId = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.employee_id).FirstOrDefault();
                    obj.auto_emp_id = item.auto_emp_id;
                    obj.profilePic = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.profile_pic).FirstOrDefault();
                    obj.fullName = item.last_name + ", " + item.first_name;
                    obj.atWork = db.tbl_harpoon_employee_attendance.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.at_work).FirstOrDefault().ToString();
                    if(db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == item.auto_emp_id && x.start_time > System.DateTime.Today && x.start_time < System.DateTime.Now && x.end_time == null).Count() > 0)
                    {
                        obj.atJob = true;
                    }
                    else
                    {
                        obj.atJob = false;
                    }
                    obj.position = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.position).FirstOrDefault().ToString();
                    obj.events = new List<TimeChardViewModel>();

                    string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select auto_loc_id, time_stamp, event_id, job_id from tbl_harpoon_attendance_log where time_stamp > '" + System.DateTime.Now.Date + "'" + " and auto_emp_id = '" + item.auto_emp_id + "'" + "order by time_stamp asc";
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

                                    int locatID = Convert.ToInt32(sdr["auto_loc_id"]);

                                    card.timeStamp = Convert.ToDateTime(sdr["time_stamp"]);
                                    card.eventID = Convert.ToString(sdr["event_id"]);
                                    card.locationID = db.tbl_harpoon_locations.Where(x => x.auto_loc_id == locatID).Select(x => x.location_id).FirstOrDefault();
                                    card.job_id = Convert.ToString(sdr["job_id"]);
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
                    if (db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == item.auto_emp_id && x.start_time > System.DateTime.Today && x.start_time < System.DateTime.Now && x.end_time == null).Count() > 0)
                    {
                        obj.atJob = true;
                    }
                    else
                    {
                        obj.atJob = false;
                    }
                    obj.position = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == item.auto_emp_id).Select(x => x.position).FirstOrDefault().ToString();
                    obj.events = new List<TimeChardViewModel>();

                    string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        string query = "select auto_loc_id, time_stamp, event_id, job_id from tbl_harpoon_attendance_log where time_stamp > '" + System.DateTime.Now.Date + "'" + " and auto_emp_id = '" + item.auto_emp_id + "'" + "order by time_stamp asc";
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

                                    int locatID = Convert.ToInt32(sdr["auto_loc_id"]);

                                    card.timeStamp = Convert.ToDateTime(sdr["time_stamp"]);
                                    card.eventID = Convert.ToString(sdr["event_id"]);
                                    card.locationID = db.tbl_harpoon_locations.Where(x => x.auto_loc_id == locatID).Select(x => x.location_id).FirstOrDefault();
                                    card.job_id = Convert.ToString(sdr["job_id"]);
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

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail, false);
            }
            else
            {
                model.MatchedLocs = populateLocationsNames(clientID, uesrEmail, false);
            }

            model.MatchedLocID = location;

            return View(model);
        }

        public String ContainerNameEmployeeFiles = "harpoon-employee-files";
        [HttpGet]
        public ActionResult EmployeeFolder(string locId, string employeeId)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            FileURL FileUrl = new FileURL();
            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;

            FileUrl.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

            int empId = 0;

            if (locId == null)
            {
                if (user.profile == "Administrator" || user.profile == "Global Manager")
                {
                    FileUrl.Location_ID = "All";
                }
                else
                {
                    FileUrl.Location_ID = db.tbl_harpoon_locations.Where(x => x.client_id == user.client_id).Select(x => x.auto_loc_id).FirstOrDefault().ToString();
                }
                
            }
            else
            {
                FileUrl.Location_ID = locId;
            }

            if (employeeId == null)
            {
                if(FileUrl.Location_ID == "All")
                {
                    empId = db.tbl_harpoon_employee.Where(x => x.client_id == clientID).OrderBy(x => x.last_name).Select(x => x.auto_emp_id).FirstOrDefault();
                }
                else
                {
                    empId = db.tbl_harpoon_employee.Where(x => x.client_id == clientID && x.auto_loc_id.ToString() == FileUrl.Location_ID).OrderBy(x => x.last_name).Select(x => x.auto_emp_id).FirstOrDefault();
                }
                
            }
            else
            {
                empId = Convert.ToInt32(employeeId);
            }

            FileUrl.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();
            var empDetails = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == empId).FirstOrDefault();

            List<tbl_harpoon_employee> employeeListLocation = new List<tbl_harpoon_employee>();

            if (FileUrl.Location_ID == "All")
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).OrderBy(x => x.last_name).ToList();
                
            }
            else
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == locId).OrderBy(x => x.last_name).ToList();
            }

            List<EmployeeAttendanceListModel> EmpDetailsList = new List<EmployeeAttendanceListModel>();
            foreach (var emp in employeeListLocation)
            {
                EmployeeAttendanceListModel empdetails = new EmployeeAttendanceListModel();
                empdetails.employeeId = emp.auto_emp_id.ToString() + ";" + emp.status;
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

            

            

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                FileUrl.LocationsList = populateLocationsPermissions(clientID, uesrEmail, false);
            }
            else
            {
                FileUrl.LocationsList = populateLocationsNames(clientID, uesrEmail, false);
            }

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
            string uesrEmail = Session["userID"].ToString();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            if (model.Location_ID == "All")
            {
                string empID = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id && x.status == 1).OrderBy(x => x.last_name).Select(x => x.auto_emp_id).FirstOrDefault().ToString();
                return RedirectToAction("EmployeeFolder", new { locId = model.Location_ID, employeeId = empID });
            }
            else
            {
                string empID = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == model.Location_ID && x.status == 1).OrderBy(x => x.last_name).Select(x => x.auto_emp_id).FirstOrDefault().ToString();
                return RedirectToAction("EmployeeFolder", new { locId = model.Location_ID, employeeId = empID });
            }

            
        }

        [HttpPost]
        public ActionResult BuildAttendanceReport(AttendanceReportViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            return RedirectToAction("AttendanceReport", new { start = model.StartDate, end = model.EndDate, autoEmp = model.SelectedEmpID, showJob = model.ShowJob });
        }

        public ActionResult populateEmployeeList(DateTime date, string loc)
        {
            DateTime lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            List<KeyValuePair<int, string>> keyValuePair = new List<KeyValuePair<int, string>>();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            List<int> employees = new List<int>();

            if (loc != "")
            {
                Trace.WriteLine(employees.Count + " This is the count of employees");
                employees = db.tbl_harpoon_attendance_log.Where(x => x.client_id == user.client_id && x.auto_loc_id == loc && x.time_stamp > date && x.time_stamp < lastDay).Select(x => x.auto_emp_id).Distinct().ToList();
            }
            else
            {
                Trace.WriteLine("Reched no loc");
                Trace.WriteLine("Client ID " + user.client_id);
                Trace.WriteLine("Start " + date);
                Trace.WriteLine("End " + lastDay);
                employees = db.tbl_harpoon_attendance_log.Where(x => x.client_id == user.client_id && x.time_stamp > date && x.time_stamp < lastDay).Select(x => x.auto_emp_id).Distinct().ToList();
            }

            

            foreach(int emp in employees)
            {
                var empl = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == emp).FirstOrDefault();
                keyValuePair.Add(new KeyValuePair<int, string>(emp, empl.last_name + "," + empl.first_name + ";" + empl.status));
            }
                       
            return Json(keyValuePair.Select(x => new
            {
                value = x.Key,
                text = x.Value
            }).OrderBy(x => x.text).ToList(), JsonRequestBehavior.AllowGet);
        }

        public string populateEmployeeDetails(int auto_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var emp = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == auto_id).FirstOrDefault();
            string loc = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 auto_loc_id from tbl_harpoon_attendance_log where auto_emp_id = '"+ emp.auto_emp_id + "' order by time_stamp desc";
                //Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string eventID;
                            loc = Convert.ToString(sdr["auto_loc_id"]);
                        }
                    }
                    con.Close();
                }
            }

            if (emp.profile_pic != null)
            {
                return emp.first_name + " " + emp.last_name + "seperator" + "data:image/png;base64," + Convert.ToBase64String(emp.profile_pic) + "seperator" + loc;
            }
            else
            {
                return emp.first_name + " " + emp.last_name + "seperator" + "/Content/profilepic.jpg" + "seperator" + loc;
            }
            
        }

        public string getOpenJob(int auto_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var job = db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == auto_id && x.start_time > System.DateTime.Today && x.start_time < System.DateTime.Now && x.end_time == null).FirstOrDefault();
            return job.job_id;
        }

        public ActionResult populateJobModal(int auto_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var jobs = db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == auto_id && x.start_time > System.DateTime.Today).OrderBy(x => x.start_time).ToList();

            return Json(jobs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AttendanceReport(AttendanceReportViewModel model, string locId, DateTime? start, DateTime? end, int? autoEmp, bool showJob)
        {
            model.ShowJob = showJob;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            model.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            var clientID = user.client_id;
            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

            model.LocationsList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

            model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail, true);

            var locationDetails = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).FirstOrDefault();
            string location = "";

            if (locId is null)
            {
                location = "All";
            }
            else
            {
                location = locId;
            }

            List<tbl_harpoon_employee> employeeListLocation = new List<tbl_harpoon_employee>();

            if (location == "All")
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).OrderBy(x => x.last_name).ToList();
            }
            else
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == location).OrderBy(x => x.last_name).ToList();
            }

            model.emp_list = employeeListLocation;

            model.EmployeeList = populateEmployees(clientID, location);

            if (autoEmp != null)
            {
                model.SelectedEmpID = autoEmp.Value.ToString();
            }

            model.MatchedLocID = location;

            if(start != null && end != null)
            {
                if (model.SelectedEmpID != null)
                {
                    model.StartDate = start.Value;
                    model.EndDate = end.Value;
                    var db1 = (from a in db.tbl_harpoon_attendance_log.Where(x => x.client_id == clientID && x.time_stamp > model.StartDate && x.time_stamp < model.EndDate && x.auto_loc_id == model.MatchedLocID && x.auto_emp_id.ToString() == model.SelectedEmpID) select a).OrderBy(x => x.time_stamp).ToList();
                    var db2 = (from a in db.tbl_harpoon_employee select a).ToList();


                    var attendance = (from a in db1
                                      join b in db2 on a.auto_emp_id equals b.auto_emp_id
                                      orderby b.first_name
                                      select new { emp_id = a.auto_emp_id, ful_name = b.first_name + ' ' + b.last_name, event_id = a.event_id, time_stamp = a.time_stamp, loc_id = a.auto_loc_id }).ToList();


                    List<HarpoonAttendanceRecordViewModel> AttendanceRecords = new List<HarpoonAttendanceRecordViewModel>();

                    foreach (var item in attendance)
                    {
                        HarpoonAttendanceRecordViewModel record = new HarpoonAttendanceRecordViewModel();
                        record.auto_emp_id = item.emp_id;
                        record.ful_name = item.ful_name;
                        record.event_id = item.event_id;
                        record.time_stamp = item.time_stamp;
                        record.loc_id = item.loc_id;
                        AttendanceRecords.Add(record);
                    }

                    model.AttendanceList = AttendanceRecords;
                }
                else
                {
                    model.StartDate = start.Value;
                    model.EndDate = end.Value;
                    var db1 = (from a in db.tbl_harpoon_attendance_log.Where(x => x.client_id == clientID && x.time_stamp > model.StartDate && x.time_stamp < model.EndDate && x.auto_loc_id == model.MatchedLocID) select a).OrderBy(x => x.time_stamp).ToList();
                    var db2 = (from a in db.tbl_harpoon_employee select a).ToList();
                    var db3 = (from a in db.tbl_harpoon_attendance_log.Where(x => x.client_id == clientID && x.time_stamp > model.StartDate && x.time_stamp < model.EndDate ) select a).OrderBy(x => x.time_stamp).ToList();

                    if (location == "All")
                    {
                        var attendance = (from a in db3
                                          join b in db2 on a.auto_emp_id equals b.auto_emp_id
                                          orderby b.first_name
                                          select new { emp_id = a.auto_emp_id, ful_name = b.first_name + ' ' + b.last_name, event_id = a.event_id, time_stamp = a.time_stamp, loc_id = a.auto_loc_id, job_ids = a.job_id }).ToList();

                        List<HarpoonAttendanceRecordViewModel> AttendanceRecords = new List<HarpoonAttendanceRecordViewModel>();

                        foreach (var item in attendance)
                        {
                            HarpoonAttendanceRecordViewModel record = new HarpoonAttendanceRecordViewModel();
                            record.auto_emp_id = item.emp_id;
                            record.ful_name = item.ful_name;
                            record.event_id = item.event_id;
                            record.time_stamp = item.time_stamp;
                            record.loc_id = item.loc_id;
                            record.job_ids = item.job_ids;

                            if (item.event_id == "clockIN" || item.event_id == "adminIN")
                            {
                                DateTime startTim = item.time_stamp.Date;
                                DateTime endTim = item.time_stamp.Date.AddDays(1);
                                record.jobs = db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == item.emp_id && x.start_time > startTim && x.end_time < endTim).ToList();
                            }
                            //record.totalJobDuration = item.job_ids;
                            AttendanceRecords.Add(record);
                        }
                        model.AttendanceList = AttendanceRecords;
                    }
                    else
                    {
                        var attendance = (from a in db1
                                          join b in db2 on a.auto_emp_id equals b.auto_emp_id
                                          orderby b.first_name
                                          select new { emp_id = a.auto_emp_id, ful_name = b.first_name + ' ' + b.last_name, event_id = a.event_id, time_stamp = a.time_stamp, loc_id = a.auto_loc_id, job_ids = a.job_id }).ToList();


                        List<HarpoonAttendanceRecordViewModel> AttendanceRecords = new List<HarpoonAttendanceRecordViewModel>();

                        foreach (var item in attendance)
                        {
                            HarpoonAttendanceRecordViewModel record = new HarpoonAttendanceRecordViewModel();
                            record.auto_emp_id = item.emp_id;
                            record.ful_name = item.ful_name;
                            record.event_id = item.event_id;
                            record.time_stamp = item.time_stamp;
                            record.loc_id = item.loc_id;
                            record.job_ids = item.job_ids;

                            if (item.event_id == "clockIN" || item.event_id == "adminIN")
                            {
                                DateTime startTim = item.time_stamp.Date;
                                DateTime endTim = item.time_stamp.Date.AddDays(1);
                                record.jobs = db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == item.emp_id && x.start_time > startTim && x.end_time < endTim).ToList();
                            }
                            //record.totalJobDuration = item.job_ids;
                            AttendanceRecords.Add(record);
                        }
                        model.AttendanceList = AttendanceRecords;
                    }                   

                }

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeLocAttendanceReport(AttendanceModel model)
        {
            return RedirectToAction("AttendanceReport", new { locId = model.MatchedLocID, showJob = false });
        }

        public FileContentResult ExportList(AttendanceReportViewModel model)
        {
            var csvString = GenerateCSVString(model.MatchedLocID, model.StartDate, model.EndDate, Convert.ToInt32(model.SelectedEmpID), model.ShowJob);
            var fileName = "AttendanceData " + DateTime.Now.ToString() + ".csv";
            return File(new System.Text.UTF8Encoding().GetBytes(csvString), "text/csv", fileName);
        }
        private string GenerateCSVString(string locId, DateTime? start, DateTime? end, int? autoEmp, bool showJobs)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            var client = db.tbl_harpoon_clients.Where(x => x.client_id == user.client_id).FirstOrDefault();
            var persons = getAttendanceInfo(locId, start, end, autoEmp);
            StringBuilder sb = new StringBuilder();
            sb.Append("Harpoon Attendance Reported");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(client.client_name + " (" + client.client_id + ")");
            sb.AppendLine();
            sb.Append("Report Date: " + System.DateTime.Today.ToString("dddd dd MMMM yyyy"));
            sb.AppendLine();
            sb.Append("Include Job Details: " + (showJobs? "Yes":"No"));
            sb.AppendLine();
            sb.Append("From: " + start.Value.ToString("dddd dd MMMM yyyy"));
            sb.AppendLine();
            sb.Append("End: " + end.Value.ToString("dddd dd MMMM yyyy"));
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("Location: " + locId);
            sb.AppendLine();
            sb.Append("Employee ID: " + autoEmp);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("Emp_ID");
            sb.Append(",");
            sb.Append("Employee");
            sb.Append(",");
            sb.Append("Date");
            sb.Append(",");
            sb.Append("ClockIN time");
            sb.Append(",");
            sb.Append("ClockOUT time");
            sb.Append(",");
            sb.Append("Duration");
            if (showJobs)
            {
                sb.Append(",");
                sb.Append(",");
                sb.Append("Job ID");
                sb.Append(",");
                sb.Append("Count");
                sb.Append(",");
                sb.Append("Total");
                sb.Append(",");
                sb.Append("Average");
            }
            sb.AppendLine();
            foreach (var person in persons)
            {
                sb.Append(person.EmpID);
                sb.Append(",");
                sb.Append(person.Name);
                sb.Append(",");
                sb.Append(person.date);
                sb.Append(",");
                sb.Append(person.clockIN);
                sb.Append(",");
                sb.Append(person.clockOUT);
                sb.Append(",");
                sb.Append(person.duration);
                if (showJobs)
                {
                    sb.Append(",");
                    sb.Append(",");
                    sb.Append(person.job_id);
                    sb.Append(",");
                    sb.Append(person.count);
                    sb.Append(",");
                    sb.Append(person.total);
                    sb.Append(",");
                    sb.Append(person.average);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private List<HarpoonAttendanceReportTimeDuration> getAttendanceInfo(string locId, DateTime? start, DateTime? end, int? autoEmp)
        {
            AttendanceReportViewModel model = new AttendanceReportViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            model.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            var clientID = user.client_id;
            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

            model.LocationsList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

            model.MatchedLocs = populateLocationsPermissions(clientID, uesrEmail, true);
            var locationDetails = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).FirstOrDefault();
            string location = "";

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

            List<tbl_harpoon_employee> employeeListLocation = new List<tbl_harpoon_employee>();

            if (location == "All")
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.client_id == user.client_id).OrderBy(x => x.last_name).ToList();

            }
            else
            {
                employeeListLocation = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == location).OrderBy(x => x.last_name).ToList();
            }
            model.emp_list = employeeListLocation;

            model.EmployeeList = populateEmployees(clientID, location);

            if (autoEmp != null)
            {
                model.SelectedEmpID = autoEmp.Value.ToString();
            }

            model.MatchedLocID = location;

            Trace.WriteLine("This is the autoEmp " + autoEmp);

            if (start != null && end != null)
            {
                if (autoEmp != null && autoEmp != 0)
                {
                    Trace.WriteLine("Reachd 5");
                    model.StartDate = start.Value;
                    model.EndDate = end.Value;
                    var db1 = (from a in db.tbl_harpoon_attendance_log.Where(x => x.client_id == clientID && x.time_stamp > model.StartDate && x.time_stamp < model.EndDate && x.auto_loc_id == model.MatchedLocID && x.auto_emp_id.ToString() == model.SelectedEmpID) select a).OrderBy(x => x.time_stamp).ToList();
                    var db2 = (from a in db.tbl_harpoon_employee select a).ToList();

                    //Trace.WriteLine(db1.Count + "This is the count");

                    var attendance = (from a in db1
                                      join b in db2 on a.auto_emp_id equals b.auto_emp_id
                                      orderby b.first_name
                                      select new { emp_id = a.auto_emp_id, ful_name = b.first_name + ' ' + b.last_name, event_id = a.event_id, time_stamp = a.time_stamp, loc_id = a.auto_loc_id }).ToList();


                    List<HarpoonAttendanceRecordViewModel> AttendanceRecords = new List<HarpoonAttendanceRecordViewModel>();

                    foreach (var item in attendance)
                    {
                        Trace.WriteLine("Reachd 6");
                        HarpoonAttendanceRecordViewModel record = new HarpoonAttendanceRecordViewModel();
                        record.auto_emp_id = item.emp_id;
                        record.ful_name = item.ful_name;
                        record.event_id = item.event_id;
                        record.time_stamp = item.time_stamp;
                        record.loc_id = item.loc_id;
                        AttendanceRecords.Add(record);
                    }

                    model.AttendanceList = AttendanceRecords;
                }
                else
                {
                    model.StartDate = start.Value;
                    model.EndDate = end.Value;
                    var db1 = (from a in db.tbl_harpoon_attendance_log.Where(x => x.client_id == clientID && x.time_stamp > model.StartDate && x.time_stamp < model.EndDate && x.auto_loc_id == model.MatchedLocID) select a).OrderBy(x => x.time_stamp).ToList();
                    var db2 = (from a in db.tbl_harpoon_employee select a).ToList();


                    var attendance = (from a in db1
                                      join b in db2 on a.auto_emp_id equals b.auto_emp_id
                                      orderby b.first_name
                                      select new { emp_id = a.auto_emp_id, ful_name = b.first_name + ' ' + b.last_name, event_id = a.event_id, time_stamp = a.time_stamp, loc_id = a.auto_loc_id, jobs_ids = a.job_id }).ToList();


                    List<HarpoonAttendanceRecordViewModel> AttendanceRecords = new List<HarpoonAttendanceRecordViewModel>();

                    foreach (var item in attendance)
                    {
                        HarpoonAttendanceRecordViewModel record = new HarpoonAttendanceRecordViewModel();
                        record.auto_emp_id = item.emp_id;
                        record.ful_name = item.ful_name;
                        record.event_id = item.event_id;
                        record.time_stamp = item.time_stamp;
                        record.loc_id = item.loc_id;
                        record.job_ids = item.jobs_ids;
                        if (item.event_id == "clockIN" || item.event_id == "adminIN")
                        {
                            DateTime startTim = item.time_stamp.Date;
                            DateTime endTim = item.time_stamp.Date.AddDays(1);
                            record.jobs = db.tbl_harpoon_job_log.Where(x => x.auto_emp_id == item.emp_id && x.start_time > startTim && x.end_time < endTim).ToList();
                        }
                        AttendanceRecords.Add(record);
                    }
                    model.AttendanceList = AttendanceRecords;
                }

            }
            List<HarpoonAttendanceReportTimeDuration> records = new List<HarpoonAttendanceReportTimeDuration>();
            foreach (var item in model.emp_list)
            {
                if (model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id).ToList().Count() > 0)
                {

                    for (int j = 0; j < (model.EndDate - model.StartDate).TotalDays; j++)
                    {
                        HarpoonAttendanceReportTimeDuration record = new HarpoonAttendanceReportTimeDuration();
                        if (model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList().Count() > 0)
                        {
                            TimeSpan totalDuration = new TimeSpan(0, 0, 0);
                            for (int i = 0; i < model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList().Count(); i++)
                            {
                                if (i % 2 == 0)
                                {
                                    Trace.WriteLine(i + " Value of i " + model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].time_stamp.ToString("hh:mm tt"));
                                    if (model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].event_id == "clockIN" || model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].event_id == "adminIN")
                                    {
                                        record.EmpID = item.employee_id;
                                        record.Name = item.last_name + " " + item.first_name;
                                        record.date = model.StartDate.AddDays(j).ToString("dddd MMMM dd yyyy");

                                        record.clockIN = model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].time_stamp.ToString("hh:mm tt");

                                        if (i + 1 < model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList().Count)
                                        {
                                            if (model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i + 1].event_id == "clockOUT" || model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i + 1].event_id == "adminOUT" || model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i + 1].event_id == "autoOUT")
                                            {
                                                record.clockOUT = model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i + 1].time_stamp.ToString("hh:mm tt");
                                            }
                                        }

                                        if (i + 1 < model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList().Count)
                                        {
                                            TimeSpan diff2 = (model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i + 1].time_stamp - model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].time_stamp);
                                            totalDuration += diff2;

                                            record.duration = diff2.ToString(@"hh\:mm");

                                        }

                                        if(model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].job_ids != null)
                                        {
                                            int count = model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].job_ids.Split(';').Count();
                                            
                                            TimeSpan totalJobDuration = new TimeSpan(0);
                                            
                                            foreach (var job in model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].jobs)
                                            {
                                                totalJobDuration += (job.end_time - job.start_time).Value;
                                            }
                                            
                                            TimeSpan averageDuaration = new TimeSpan(totalJobDuration.Ticks / count);
                                            
                                            record.job_id = string.Join(" ", model.AttendanceList.Where(x => x.auto_emp_id == item.auto_emp_id && x.time_stamp.ToString("dd MM yyyy") == model.StartDate.AddDays(j).ToString("dd MM yyyy")).ToList()[i].job_ids.Split(';').Select(x => x.Trim()).Distinct().ToArray());
                                            record.count = count.ToString();
                                            record.total = totalJobDuration.ToString(@"hh\:mm");
                                            record.average = averageDuaration.ToString(@"hh\:mm");
                                        }                                        

                                        records.Add(record);
                                    }
                                }
                            }
                        }
                        
                    }

                }
                
            }
            return records;
        }
    }
}