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
using System.Data;
using System.Net.Mail;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace SeHubPortal.Controllers
{
    public class SettingsHarpoonController : Controller
    {
        // GET: SettingsHarpoon
        public ActionResult Index()
        {
            return View();
        }

        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        [HttpGet]
        public ActionResult Employees(string LocId)
        {
            MyStaffHarpoonViewModel modal = new MyStaffHarpoonViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = userdetails.client_id;

            modal.MatchedStaffLocs = populateLocationsPermissionsLoc(clientID);

            if (db.tbl_harpoon_settings.Where(x => x.client_id == clientID).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                modal.multipleLocation = true;
            }
            else
            {
                modal.multipleLocation = false;
            }

            modal.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();


            string locationid = "";
            if (LocId == "" || LocId is null)
            {
                //Debug.WriteLine("empId:" + empId);
                var result = db.tbl_harpoon_employee.Where(a => a.client_id.Equals(clientID)).FirstOrDefault();

                if (result != null)
                {
                    locationid = result.auto_loc_id.Value.ToString();
                }
                else
                {
                    locationid = "";
                }

            }
            else
            {
                locationid = LocId;
            }


            var EmployeeDetails = db.tbl_harpoon_employee.Where(x => x.auto_loc_id.ToString() == locationid && x.client_id == clientID).OrderBy(x => x.last_name).ToList();
            

            List<HarpoonEmployeeViewModel> empldetList = new List<HarpoonEmployeeViewModel>();

            foreach (var emp in EmployeeDetails)
            {
                HarpoonEmployeeViewModel empdet = new HarpoonEmployeeViewModel();
                empdet.employee_id = emp.employee_id;
                empdet.first_name = emp.first_name;
                empdet.middle_initial = emp.middle_initial;
                empdet.last_name = emp.last_name;
                empdet.position = emp.position;
                empdet.Date_of_birth = emp.Date_of_birth;
                empdet.client_id = emp.client_id;
                empdet.status = emp.status;
                empdet.profile_pic = emp.profile_pic;
                empdet.locationId = emp.auto_loc_id.ToString();
                empdet.auto_emp_id = emp.auto_emp_id;

                var rfd = db.tbl_harpoon_employee_rfid.Where(x => x.auto_emp_id == emp.auto_emp_id).Select(x => x.rfid_number).FirstOrDefault();

                if(rfd != null)
                {
                    empdet.rfidPaired = rfd;
                }
                else
                {
                    empdet.rfidPaired = "NO";
                }

                

                empldetList.Add(empdet);
            }



            //Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {
                modal.employeeDetails = empldetList;

                if (modal.multipleLocation)
                {
                    modal.MatchedStaffLocs = populateLocationsPermissions(clientID);
                }
                else
                {
                    modal.MatchedStaffLocs = populateLocationsNames(clientID);
                }

                modal.MatchedStaffLocID = locationid;

            }

            return View(modal);
        }

        private static List<SelectListItem> populateLocationsNames(string clientID)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

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

        private static List<SelectListItem> populateLocationsPermissions(string clientID)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

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

        private static List<SelectListItem> populateLocationsPermissionsLoc(string clientID)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID).ToList();

            foreach (var loc in locaList)
            {
                items.Add(new SelectListItem
                {
                    Text = loc.location_id,
                    Value = loc.location_id
                });
            }

            return items;
        }


        [HttpGet]
        public ActionResult Locations(string LocId)
        {
            HarpoonLocationsViewModel modal = new HarpoonLocationsViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            List<tbl_harpoon_locations> locations = new List<tbl_harpoon_locations>();
            List<tbl_harpoon_departments> departments = new List<tbl_harpoon_departments>();
            List<tbl_harpoon_jobclock> jobClocks = new List<tbl_harpoon_jobclock>();

            locations = db.tbl_harpoon_locations.Where( x => x.client_id == userdetails.client_id).ToList();
            departments = db.tbl_harpoon_departments.ToList();
            jobClocks = db.tbl_harpoon_jobclock.ToList();

            modal.Locations = locations;
            modal.Departments = departments;
            modal.JobClocks = jobClocks;

            var auto = db.tbl_harpoon_settings.Where(x => x.client_id == userdetails.client_id).Select(x => x.auto_generate_loc_id).FirstOrDefault();

            if(auto == 1)
            {
                modal.autoGenLoc = true;
            }
            else
            {
                modal.autoGenLoc = false;
            }


            return View(modal);
        }

        [HttpGet]
        public ActionResult Devices(string LocId)
        {
            HarpoonDevicesViewModel modal = new HarpoonDevicesViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            modal.Locations = db.tbl_harpoon_locations.ToList();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            if (db.tbl_harpoon_settings.Where(x => x.client_id == userdetails.client_id).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                modal.locIDorName = true;
            }
            else
            {
                modal.locIDorName = false;
            }


            var clientID = userdetails.client_id;

            var allDevices = db.tbl_harpoon_devices.Where(x => x.client_id == userdetails.client_id).ToList();

            modal.devices = allDevices;
            modal.LocationsList = populateLocationsPermissionsDevices(clientID);
            modal.LocationZonesList = populateLocationZones();
            modal.ColorsList = populateColors();


            return View(modal);
        }

        [HttpGet]
        public ActionResult System(string ack)
        {
            AccountHarpoonSettingsViewModel modal = new AccountHarpoonSettingsViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();

            modal.AccessType = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).Select(x => x.profile).FirstOrDefault();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            modal.client = db.tbl_harpoon_clients.Where(x => x.client_id == userdetails.client_id).FirstOrDefault();

            modal.Locations = populateLocationsPermissions(userdetails.client_id);

            var locList = db.tbl_harpoon_locations.Where(x => x.client_id == modal.client.client_id).Select(x => x.location_id).ToList();

            foreach(var loc in locList)
            {
                modal.listOfLocations = modal.listOfLocations + ";" + loc;
            }

            var settings = db.tbl_harpoon_settings.Where(x => x.client_id == modal.client.client_id).FirstOrDefault();

            var sunday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "sunday").FirstOrDefault();
            var monday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "monday").FirstOrDefault();
            var tuesday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "tuesday").FirstOrDefault();
            var wednesday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "wednesday").FirstOrDefault();
            var thursday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "thursday").FirstOrDefault();
            var friday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "friday").FirstOrDefault();
            var saturday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == modal.client.client_id && x.monday_saturday == "saturday").FirstOrDefault();

            if(settings != null)
            {

                if (settings.MasterLoc_id != null)
                {
                    modal.masterLocID = settings.MasterLoc_id;
                }
                else
                {
                    modal.masterLocID = "";
                }

                if (settings.AutoClockOut_OnOff.HasValue)
                {
                    modal.autoClockOut = changeToBool(settings.AutoClockOut_OnOff.Value);
                }
                else
                {
                    modal.autoClockOut = false;
                }

                if (settings.AutoClockOut_Adjustment.HasValue)
                {
                    modal.adjustment = settings.AutoClockOut_Adjustment.Value;
                }
                else
                {
                    modal.adjustment = 0;
                }

                if (settings.ManualCode_OnOff.HasValue)
                {
                    modal.manualCode = changeToBool(settings.ManualCode_OnOff.Value);
                }
                else
                {
                    modal.manualCode = false;
                }

                if (settings.LocIDinListBox_OnOff.HasValue)
                {
                    modal.useLocIdInList = changeToBool(settings.LocIDinListBox_OnOff.Value);
                }
                else
                {
                    modal.useLocIdInList = false;
                }

                if (settings.DefaultEmpIDlength_OnOff.HasValue)
                {
                    modal.custom_empID_len = changeToBool(settings.DefaultEmpIDlength_OnOff.Value);
                }
                else
                {
                    modal.custom_empID_len = false;
                }

                if (settings.EmpID_Length.HasValue)
                {
                    modal.custEmpIDLength = settings.EmpID_Length.Value;
                }
                else
                {
                }


                if (settings.MasterLoc_OnOff.HasValue)
                {
                    modal.master_loc_id = changeToBool(settings.MasterLoc_OnOff.Value);
                }
                else
                {
                    modal.master_loc_id = false;
                }

                if (sunday.sunday_open.HasValue)
                {
                    modal.sundayOpen = changeToBool(sunday.sunday_open.Value);
                    modal.sundayStart = sunday.sunday_start;
                    modal.sundayEnd = sunday.sunday_end;
                }
                else
                {
                    modal.sundayOpen = false;
                    modal.sundayStart = "08:00";
                    modal.sundayEnd = "17:00";
                }

                if (monday.sunday_open.HasValue)
                {
                    modal.monOpen = changeToBool(monday.sunday_open.Value);
                    modal.monStart = monday.sunday_start;
                    modal.monEnd = monday.sunday_end;
                }
                else
                {
                    modal.monOpen = false;
                    modal.monStart = "08:00";
                    modal.monEnd = "17:00";
                }

                if (tuesday.sunday_open.HasValue)
                {
                    modal.TueOpen = changeToBool(tuesday.sunday_open.Value);
                    modal.TueStart = tuesday.sunday_start;
                    modal.TueEnd = tuesday.sunday_end;
                }
                else
                {
                    modal.TueOpen = false;
                    modal.TueStart = "08:00";
                    modal.TueEnd = "17:00";
                }

                if (wednesday.sunday_open.HasValue)
                {
                    modal.WedOpen = changeToBool(wednesday.sunday_open.Value);
                    modal.WedStart = wednesday.sunday_start;
                    modal.WedEnd = wednesday.sunday_end;
                }
                else
                {
                    modal.WedOpen = false;
                    modal.WedStart = "08:00";
                    modal.WedEnd = "17:00";
                }

                if (thursday.sunday_open.HasValue)
                {
                    modal.ThuOpen = changeToBool(thursday.sunday_open.Value);
                    modal.ThuStart = thursday.sunday_start;
                    modal.ThuEnd = thursday.sunday_end;
                }
                else
                {
                    modal.ThuOpen = false;
                    modal.ThuStart = "08:00";
                    modal.ThuEnd = "17:00";
                }

                if (friday.sunday_open.HasValue)
                {
                    modal.FriOpen = changeToBool(friday.sunday_open.Value);
                    modal.FriStart = friday.sunday_start;
                    modal.FriEnd = friday.sunday_end;
                }
                else
                {
                    modal.FriOpen = false;
                    modal.FriStart = "08:00";
                    modal.FriEnd = "17:00";
                }

                if (saturday.sunday_open.HasValue)
                {
                    modal.SatOpen = changeToBool(saturday.sunday_open.Value);
                    modal.SatStart = saturday.sunday_start;
                    modal.SatEnd = saturday.sunday_end;
                }
                else
                {
                    modal.SatOpen = false;
                    modal.SatStart = "08:00";
                    modal.SatEnd = "17:00";
                }

            }



            return View(modal);

        }

        public bool CheckUserOnlyAdmin()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            if(db.tbl_harpoon_users.Where(x => x.client_id == userdetails.client_id).Count() == 1 && db.tbl_harpoon_users.Where(x => x.client_id == userdetails.client_id).Select(x => x.profile).FirstOrDefault() == "Administrator")
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public bool CheckOperatingHoursExist()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            if(db.tbl_harpoon_settings_schedule.Where(x => x.client_id == userdetails.client_id).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool checkLocationExistance()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();
            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            if(db.tbl_harpoon_locations.Where(x => x.client_id == userdetails.client_id).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        [HttpGet]
        public ActionResult Users(string LocId)
        {
            UsersHarpoonViewModel modal = new UsersHarpoonViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = userdetails.client_id;
            modal.Locations = populateLocationsPermissions(clientID);

            if (db.tbl_harpoon_settings.Where(x => x.client_id == userdetails.client_id).Select(x => x.LocIDinListBox_OnOff).FirstOrDefault() == 1)
            {
                modal.locIDorName = true;
            }
            else
            {
                modal.locIDorName = false;
            }

            List<tbl_harpoon_users> UsersList = new List<tbl_harpoon_users>();

            var users = db.tbl_harpoon_users.Where(x => x.client_id == userdetails.client_id).ToList();

            foreach(var user in users)
            {
                tbl_harpoon_users userTemp = new tbl_harpoon_users();

                userTemp = user;
                userTemp.client_id = db.tbl_harpoon_clients.Where(x => x.client_id == user.client_id).Select(x => x.client_name).FirstOrDefault();
                UsersList.Add(userTemp);
            }

            modal.users = users;
            modal.Profiles = populateProfiles();
            

            return View(modal);

        }

        [HttpPost]
        public ActionResult Employees(MyStaffViewModel modal)
        {
            return RedirectToAction("Employees", new { LocId = modal.MatchedStaffLocID });
        }

        [HttpPost]
        public void deleteDevice(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var device = db.tbl_harpoon_devices.Where(x => x.serial_number == value).FirstOrDefault();

            db.tbl_harpoon_devices.Remove(device);
            db.SaveChanges();
        }

        [HttpPost]
        public void deleteUser(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var user = db.tbl_harpoon_users.Where(x => x.email == value).FirstOrDefault();

            db.tbl_harpoon_users.Remove(user);
            db.SaveChanges();
        }

        [HttpPost]
        public string checkEmpIdLength(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Emps = db.tbl_harpoon_employee.Where(x => x.client_id == value).Select(x => x.employee_id).ToList();

            return Emps.Max().ToString().Length.ToString();

        }

        [HttpPost]
        public string checkSerialNumber(string value, string verif)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var device = db.tbl_harpoon_devices.Where(x => x.serial_number == value).FirstOrDefault();

            var serial = db.tbl_harpoon_source_serialNumbers.Where(x => x.serial_number == value && x.verification_code == verif).FirstOrDefault();

            if(device != null)
            {
                return "true";
            }
            else
            {
                if (serial != null)
                {
                    return "false";
                }
                else
                {
                    return "incorrect verification";
                }
            }

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


        private static List<SelectListItem> populateLocationsPermissionsDevices(string clientID)
        {

            //Trace.WriteLine("This is the client ID "+ clientID);

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.Where(x => x.client_id == clientID && x.status == 1).ToList();

            var settings = db.tbl_harpoon_settings.Where(x => x.client_id == clientID).FirstOrDefault();

            //var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();

            items.Add(new SelectListItem
            {
                Text = "Select",
                Value = "Select"
            });

            foreach (var loc in locaList)
            {
                if (settings.LocIDinListBox_OnOff != 0)
                {
                    items.Add(new SelectListItem
                    {
                        Text = loc.location_id,
                        Value = loc.auto_loc_id.ToString()
                    });
                }
                else
                {
                    items.Add(new SelectListItem
                    {
                        Text = loc.location_name,
                        Value = loc.auto_loc_id.ToString()
                    });
                }

            }

            /*

            if (user.profile == "Administrator" || user.profile == "Global Manager")
            {
                
            }
            else
            {
                items.Add(new SelectListItem
                {
                    Text = user.loc_id,
                    Value = user.loc_id
                });
            }

            */

            return items;
        }

        private static List<SelectListItem> populateLocations()
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_locations.ToList();

            items.Add(new SelectListItem
            {
                Text = "Select",
                Value = ""
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

        private static List<SelectListItem> populateLocationZones()
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_source_locationZone.ToList();

            items.Add(new SelectListItem
            {
                Text = "Select",
                Value = "Select"
            });

            foreach (var loc in locaList)
            {
                items.Add(new SelectListItem
                {
                    Text = loc.zone,
                    Value = loc.zone
                });
            }

            return items;
        }

        private static List<SelectListItem> populateColors()
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_harpoon_source_displayColors.ToList();

            items.Add(new SelectListItem
            {
                Text = "Select",
                Value = "Select"
            });

            foreach (var loc in locaList)
            {
                items.Add(new SelectListItem
                {
                    Text = loc.color,
                    Value = loc.color
                });
            }

            return items;
        }

        private static List<SelectListItem> populateProfiles()
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Profiles = db.tbl_harpoon_source_userProfiles.Select(x => x.profile).ToList();

            items.Add(new SelectListItem
            {
                Text = "Select",
                Value = "Select"
            });

            foreach (var profile in Profiles)
            {
                items.Add(new SelectListItem
                {
                    Text = profile,
                    Value = profile
                });
            }

            return items;
        }


        public ActionResult ProfileBlock(HarpoonProfileBlockViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string email = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();

            model.user = userdetails;

            model.client = db.tbl_harpoon_clients.Where(x => x.client_id == userdetails.client_id).FirstOrDefault();

            return PartialView(model);
        }

        public static string GetRandomPassword(int length)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }

        [HttpPost]
        public ActionResult AddUser(UsersHarpoonViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_users newUser = new tbl_harpoon_users();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            string password = GetRandomPassword(6);

            newUser = model.newUser;
            newUser.client_id = userdetails.client_id;
            newUser.password = password;

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(newUser.email, "IT Team"));
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.Subject = "Temporary password for User";
            msg.Body = "Temporary password : " + password;
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            db.tbl_harpoon_users.Add(newUser);
            db.SaveChanges();

            return RedirectToAction("Users", "SettingsHarpoon");
        }

        [HttpPost]
        public ActionResult EditLocation(HarpoonLocationsViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();


            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var editLocation = db.tbl_harpoon_locations.Where(x => x.auto_loc_id == model.EditLocation.auto_loc_id && x.client_id == userdetails.client_id).FirstOrDefault();

            if (editLocation != null)
            {

                editLocation.location_id = model.EditLocation.location_id;
                editLocation.location_name = model.EditLocation.location_name;
                editLocation.address1 = model.EditLocation.address1;
                editLocation.address2 = model.EditLocation.address2;
                editLocation.city = model.EditLocation.city;
                editLocation.province = model.EditLocation.province;
                editLocation.postal_code = model.EditLocation.postal_code;
                editLocation.country = model.EditLocation.country;
                editLocation.phone = model.EditLocation.phone;
                editLocation.fax = model.EditLocation.fax;
                editLocation.google_maps = model.EditLocation.google_maps;

                if(model.status && db.tbl_harpoon_devices.Where(x => x.auto_loc_id == editLocation.auto_loc_id).Count() == 0 && db.tbl_harpoon_employee.Where(x => x.auto_loc_id == editLocation.auto_loc_id).Count() == 0)
                {
                    editLocation.status = changeToInt(model.status);
                }
                else
                {
                    editLocation.status = changeToInt(model.status);
                }
                
                var devices = db.tbl_harpoon_devices.Where(x => x.auto_loc_id == model.EditLocation.auto_loc_id).ToList();
                var employees = db.tbl_harpoon_devices.Where(x => x.auto_loc_id == model.EditLocation.auto_loc_id).ToList();
                
                if (devices != null)
                {
                    devices.ForEach(x => x.auto_loc_id = model.EditLocation.auto_loc_id);
                }

                if (employees != null)
                {
                    employees.ForEach(x => x.auto_loc_id = model.EditLocation.auto_loc_id);
                }

                db.SaveChanges();
            }
            

            return RedirectToAction("Locations", "SettingsHarpoon");
        }


        [HttpPost]
        public ActionResult AddLocation(HarpoonLocationsViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_locations newLocation = new tbl_harpoon_locations();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var location = db.tbl_harpoon_locations.Where(x => x.client_id == userdetails.client_id && x.location_id == model.newLocation.location_id).FirstOrDefault();

            if(location == null)
            {
                newLocation.location_id = model.newLocation.location_id;
                newLocation.client_id = userdetails.client_id;
                newLocation.location_name = model.newLocation.location_name;
                newLocation.address1 = model.newLocation.address1;
                newLocation.address2 = model.newLocation.address2;
                newLocation.city = model.newLocation.city;
                newLocation.province = model.newLocation.province;
                newLocation.postal_code = model.newLocation.postal_code;
                newLocation.country = model.newLocation.country;
                newLocation.phone = model.newLocation.phone;
                newLocation.fax = model.newLocation.fax;
                newLocation.google_maps = model.newLocation.google_maps;
                newLocation.status = 1;

                db.tbl_harpoon_locations.Add(newLocation);
                db.SaveChanges();
            }


            return RedirectToAction("Locations", "SettingsHarpoon");
        }

        [HttpPost]
        public ActionResult AddDepartment(HarpoonLocationsViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_departments newDepartment = new tbl_harpoon_departments();

            var department = db.tbl_harpoon_departments.Where(x => x.department_id == model.newDepartment.department_id).FirstOrDefault();

            if (department == null)
            {
                newDepartment.department_id = model.newDepartment.department_id;
                newDepartment.name = model.newDepartment.name;
                newDepartment.available_to_all_loc = model.newDepartment.available_to_all_loc;
                newDepartment.parent_location = model.newDepartment.parent_location;
                newDepartment.admin = model.newDepartment.admin;
                newDepartment.clocking_comments = model.newDepartment.clocking_comments;

                db.tbl_harpoon_departments.Add(newDepartment);
                db.SaveChanges();
            }


            return RedirectToAction("Locations", "SettingsHarpoon");
        }

        [HttpPost]
        public ActionResult AddJobClock(HarpoonLocationsViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_jobclock newJobClock = new tbl_harpoon_jobclock();

            var jobClock = db.tbl_harpoon_jobclock.Where(x => x.jobclock_id == model.newJobClock.jobclock_id).FirstOrDefault();

            if (jobClock == null)
            {
                newJobClock.jobclock_id = model.newJobClock.jobclock_id;
                newJobClock.name = model.newJobClock.name;
                newJobClock.available_to_all_locations = model.newJobClock.available_to_all_locations;
                newJobClock.parent_location = model.newJobClock.parent_location;
                newJobClock.admin = model.newJobClock.admin;
                newJobClock.clocking_comments = model.newJobClock.clocking_comments;

                db.tbl_harpoon_jobclock.Add(newJobClock);
                db.SaveChanges();
            }


            return RedirectToAction("Locations", "SettingsHarpoon");
        }

        public bool checkEmpForClient(string empid)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            if(db.tbl_harpoon_employee.Where(x => x.client_id == userdetails.client_id && x.employee_id == empid).Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        [HttpPost]
        public ActionResult AddEmployee(MyStaffHarpoonViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_employee newEmp = new tbl_harpoon_employee();
            tbl_harpoon_employee_rfid newEmpRF = new tbl_harpoon_employee_rfid();
            tbl_harpoon_employee_attendance newEmpAtten = new tbl_harpoon_employee_attendance();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            int nextEmp = db.tbl_harpoon_employee.OrderByDescending(x => x.auto_emp_id).Select(x => x.auto_emp_id).FirstOrDefault() + 1;

            Trace.WriteLine("Next emp" + nextEmp);

            newEmp.auto_emp_id = nextEmp;
            newEmp.employee_id = model.newemp.employee_id;
            newEmp.first_name = model.newemp.first_name;
            newEmp.middle_initial = model.newemp.middle_initial;
            newEmp.last_name = model.newemp.last_name;
            newEmp.position = model.newemp.position;
            newEmp.auto_loc_id = model.newemp.auto_loc_id;
            newEmp.Date_of_birth = model.newemp.Date_of_birth;
            newEmp.client_id = userdetails.client_id;
            newEmp.status = 1;

            newEmpRF.client_id = userdetails.client_id;
            newEmpRF.auto_emp_id = nextEmp;

            newEmpAtten.client_id = userdetails.client_id;
            newEmpAtten.auto_emp_id = nextEmp;

            db.tbl_harpoon_employee.Add(newEmp);
            db.tbl_harpoon_employee_rfid.Add(newEmpRF);
            db.tbl_harpoon_employee_attendance.Add(newEmpAtten);
            db.SaveChanges();

            return RedirectToAction("Employees", "SettingsHarpoon");
        }

        protected Image Resize(Image img, int resizedW, int resizedH)
        {
            Bitmap bmp = new Bitmap(resizedW, resizedH);
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.DrawImage(img, 0, 0, resizedW, resizedH);
            graphic.Dispose();
            return (Image)bmp;
        }

        [HttpPost]
        public ActionResult EditEmployee(MyStaffHarpoonViewModel model, HttpPostedFileBase employeeImage)
        {
            byte[] imageBytes = null;
            if (employeeImage != null && employeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(employeeImage.FileName);
                using (Image image = Image.FromStream(employeeImage.InputStream, true, true))
                {
                    double height = 170 * image.Height / image.Width;
                    Image img = Resize(image, 170, (int)Math.Round(height));

                    using (MemoryStream m = new MemoryStream())
                    {
                        img.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();
                    }
                }
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            Trace.WriteLine("This is the auto emp id" + model.ediemp.auto_emp_id);

            var editemp = db.tbl_harpoon_employee.Where(x => x.auto_emp_id == model.ediemp.auto_emp_id && x.client_id == userdetails.client_id).FirstOrDefault();

            if(editemp != null)
            {
                editemp.employee_id = model.ediemp.employee_id;
                editemp.first_name = model.ediemp.first_name;
                editemp.middle_initial = model.ediemp.middle_initial;
                editemp.last_name = model.ediemp.last_name;
                editemp.position = model.ediemp.position;
                editemp.auto_loc_id = Convert.ToInt32(model.ediemp.locationId);
                editemp.Date_of_birth = model.ediemp.Date_of_birth;
                editemp.status = Convert.ToInt32(model.status);

                if(imageBytes != null)
                {
                    editemp.profile_pic = imageBytes;
                }

                var rdid = db.tbl_harpoon_employee_rfid.Where(x => x.auto_emp_id == model.ediemp.auto_emp_id).FirstOrDefault();

                if(rdid != null)
                {
                    if (model.ediemp.rfidPaired == "No Key Fob Assigned")
                    {
                        rdid.rfid_number = null;
                    }
                    else
                    {

                    }
                }

                db.SaveChanges();
            }
            else
            {
                //Trace.WriteLine("Employee Doesnot exist");
            }

            return RedirectToAction("Employees", "SettingsHarpoon");
        }

        [HttpPost]
        public ActionResult AddDevice(HarpoonDevicesViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_devices newDevice = new tbl_harpoon_devices();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            newDevice.serial_number = model.newDevice.serial_number;
            newDevice.auto_loc_id = model.newDevice.auto_loc_id;
            newDevice.location_zone = model.newDevice.location_zone;
            newDevice.display_color = model.newDevice.display_color;
            newDevice.client_id = userdetails.client_id;

            db.tbl_harpoon_devices.Add(newDevice);
            db.SaveChanges();

            return RedirectToAction("Devices", "SettingsHarpoon");
        }

        [HttpPost]
        public ActionResult EditDevice(HarpoonDevicesViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Devices = db.tbl_harpoon_devices.Where(x => x.serial_number == model.editDevice.serial_number).FirstOrDefault();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            if (Devices != null)
            {
                Devices.client_id = userdetails.client_id;
                Devices.auto_loc_id = model.editDevice.auto_loc_id;
                Devices.location_zone = model.editDevice.location_zone;
                Devices.display_color = model.editDevice.display_color;
                db.SaveChanges();
            }

            return RedirectToAction("Devices", "SettingsHarpoon");
        }

        [HttpPost]
        public ActionResult EditUser(UsersHarpoonViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Users = db.tbl_harpoon_users.Where(x => x.email == model.editUser.email).FirstOrDefault();

            if (Users != null)
            {
                Users.profile = model.editUser.profile;
                Users.loc_id = model.editUser.loc_id;
                db.SaveChanges();
            }

            return RedirectToAction("Users", "SettingsHarpoon");
        }

        public int changeToInt(bool flag)
        {
            int i = 0;

            if (flag)
            {
                i = 1;
            }

            return i;
        }

        public bool changeToBool(int i)
        {
            bool flag = false;

            if (i == 1)
            {
                flag = true;
            }

            return flag;
        }

        [HttpPost]
        public ActionResult SaveAccountSettings(AccountHarpoonSettingsViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var settings = db.tbl_harpoon_settings.Where(x => x.client_id == model.client.client_id).FirstOrDefault();

            var sunday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "sunday").FirstOrDefault();
            var saturday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "saturday").FirstOrDefault();
            var monday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "monday").FirstOrDefault();
            var tuesday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "tuesday").FirstOrDefault();
            var wednesday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "wednesday").FirstOrDefault();
            var thursday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "thursday").FirstOrDefault();
            var friday = db.tbl_harpoon_settings_schedule.Where(x => x.client_id == model.client.client_id && x.monday_saturday == "friday").FirstOrDefault();

            if(settings != null)
            {
                settings.client_id = model.client.client_id;
                settings.AutoClockOut_OnOff = changeToInt(model.autoClockOut);
                settings.AutoClockOut_Adjustment = model.adjustment;
                settings.ManualCode_OnOff = changeToInt(model.manualCode);
                settings.LocIDinListBox_OnOff = changeToInt(model.useLocIdInList);
                settings.MasterLoc_OnOff = changeToInt(model.master_loc_id);
                settings.DefaultEmpIDlength_OnOff = changeToInt(model.custom_empID_len);

                if(model.masterLocID != null)
                {
                    settings.MasterLoc_id = model.masterLocID;
                }

                settings.EmpID_Length = model.custEmpIDLength;

                ChangeCustomLength(model.client.client_id, model.custEmpIDLength);

                sunday.sunday_open = changeToInt(model.sundayOpen);
                sunday.sunday_start = model.sundayStart;
                sunday.sunday_end = model.sundayEnd;

                saturday.sunday_open = changeToInt(model.SatOpen);
                saturday.sunday_start = model.SatStart;
                saturday.sunday_end = model.SatEnd;

                monday.sunday_open = changeToInt(model.monOpen);
                monday.sunday_start = model.monStart;
                monday.sunday_end = model.monEnd;

                tuesday.sunday_open = changeToInt(model.TueOpen);
                tuesday.sunday_start = model.TueStart;
                tuesday.sunday_end = model.TueEnd;

                wednesday.sunday_open = changeToInt(model.WedOpen);
                wednesday.sunday_start = model.WedStart;
                wednesday.sunday_end = model.WedEnd;

                thursday.sunday_open = changeToInt(model.ThuOpen);
                thursday.sunday_start = model.ThuStart;
                thursday.sunday_end = model.ThuEnd;

                friday.sunday_open = changeToInt(model.FriOpen);
                friday.sunday_start = model.FriStart;
                friday.sunday_end = model.FriEnd;

                db.SaveChanges();
            }
            else
            {
                tbl_harpoon_settings Newsettings = new tbl_harpoon_settings();
                tbl_harpoon_settings_schedule NewSunday = new tbl_harpoon_settings_schedule();
                tbl_harpoon_settings_schedule NewMonday = new tbl_harpoon_settings_schedule();
                tbl_harpoon_settings_schedule NewTuesday = new tbl_harpoon_settings_schedule();
                tbl_harpoon_settings_schedule NewWednesday = new tbl_harpoon_settings_schedule();
                tbl_harpoon_settings_schedule NewThursday = new tbl_harpoon_settings_schedule();
                tbl_harpoon_settings_schedule NewFriday = new tbl_harpoon_settings_schedule();
                tbl_harpoon_settings_schedule NewSaturday = new tbl_harpoon_settings_schedule();

                Newsettings.client_id = model.client.client_id;
                Newsettings.AutoClockOut_OnOff = changeToInt(model.autoClockOut);
                Newsettings.AutoClockOut_Adjustment = model.adjustment;
                Newsettings.ManualCode_OnOff = changeToInt(model.manualCode);
                Newsettings.LocIDinListBox_OnOff = changeToInt(model.useLocIdInList);
                Newsettings.MasterLoc_OnOff = changeToInt(model.master_loc_id);

                if(model.masterLocID != null)
                {
                    Newsettings.MasterLoc_id = model.masterLocID;
                }

                NewSunday.client_id = model.client.client_id;
                NewSunday.sunday_open = changeToInt(model.sundayOpen);
                NewSunday.sunday_start = model.sundayStart;
                NewSunday.sunday_end = model.sundayEnd;
                NewSunday.monday_saturday = "sunday";

                NewMonday.client_id = model.client.client_id;
                NewMonday.sunday_open = changeToInt(model.monOpen);
                NewMonday.sunday_start = model.monStart;
                NewMonday.sunday_end = model.monEnd;
                NewMonday.monday_saturday = "monday";

                NewTuesday.client_id = model.client.client_id;
                NewTuesday.sunday_open = changeToInt(model.TueOpen);
                NewTuesday.sunday_start = model.TueStart;
                NewTuesday.sunday_end = model.TueEnd;
                NewTuesday.monday_saturday = "tuesday";

                NewWednesday.client_id = model.client.client_id;
                NewWednesday.sunday_open = changeToInt(model.WedOpen);
                NewWednesday.sunday_start = model.WedStart;
                NewWednesday.sunday_end = model.WedEnd;
                NewWednesday.monday_saturday = "wednesday";

                NewThursday.client_id = model.client.client_id;
                NewThursday.sunday_open = changeToInt(model.ThuOpen);
                NewThursday.sunday_start = model.ThuStart;
                NewThursday.sunday_end = model.ThuEnd;
                NewThursday.monday_saturday = "thursday";

                NewFriday.client_id = model.client.client_id;
                NewFriday.sunday_open = changeToInt(model.FriOpen);
                NewFriday.sunday_start = model.FriStart;
                NewFriday.sunday_end = model.FriEnd;
                NewFriday.monday_saturday = "friday";

                NewSaturday.client_id = model.client.client_id;
                NewSaturday.sunday_open = changeToInt(model.SatOpen);
                NewSaturday.sunday_start = model.SatStart;
                NewSaturday.sunday_end = model.SatEnd;
                NewSaturday.monday_saturday = "saturday";

                db.tbl_harpoon_settings_schedule.Add(NewSunday);
                db.tbl_harpoon_settings_schedule.Add(NewMonday);
                db.tbl_harpoon_settings_schedule.Add(NewTuesday);
                db.tbl_harpoon_settings_schedule.Add(NewWednesday);
                db.tbl_harpoon_settings_schedule.Add(NewThursday);
                db.tbl_harpoon_settings_schedule.Add(NewFriday);
                db.tbl_harpoon_settings_schedule.Add(NewSaturday);

                db.tbl_harpoon_settings.Add(Newsettings);
                db.SaveChanges();

            }

            return RedirectToAction("System", "SettingsHarpoon");
        }

        public void ChangeCustomLength(string clientID, int custLen)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var empls = db.tbl_harpoon_employee.Where(x => x.client_id == clientID).ToList();
            foreach(var emp in empls)
            {
                if (emp.employee_id.Length > custLen)
                {
                    if(Convert.ToInt32(emp.employee_id.Substring(0, emp.employee_id.Length - custLen)) == 0)
                    {
                        emp.employee_id = emp.employee_id.Substring(emp.employee_id.Length - custLen);
                        db.SaveChanges();
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditClientInfo(AccountHarpoonSettingsViewModel model, HttpPostedFileBase clientImage)
        {

            byte[] imageBytes = null;
            if (clientImage != null && clientImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(clientImage.FileName);
                using (Image image = Image.FromStream(clientImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        string base64String = Convert.ToBase64String(imageBytes);
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var userdetails = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = userdetails.client_id;

            var client = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();

            if (client != null)
            {
                client.client_name = model.editClient.client_name;
                client.client_address1 = model.editClient.client_address1;
                client.client_address2 = model.editClient.client_address2;
                client.client_phone = model.editClient.client_phone;
                client.client_fax = model.editClient.client_fax;
                client.client_website = model.editClient.client_website;
                if(imageBytes != null)
                {
                    client.client_logo = model.editClient.client_logo;
                }
            }
            

            db.SaveChanges();
            return RedirectToAction("System");
        }

    }
}