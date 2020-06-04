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

namespace SeHubPortal.Controllers
{
    public class ManagementController : Controller
    {       
        public tbl_sehub_access CheckPermissions(int employeeID)
        {
             CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());         
            var empDetails= db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
     
            return empDetails;
        }


        // GET: Management
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult QuickGuide()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EmployeePermissions(string locId)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            EmployeePermissionsViewModel model = new EmployeePermissionsViewModel();

            string location = "";

            if(locId is null || locId=="")
            {
                
            }
            else
            {
                model.MatchedLocID = locId;
                location = locId;
            }

            //var employeeList =
            //            (from employee in db.tbl_employee
            //             join credentials in db.tbl_employee_credentials on employee.employee_id equals credentials.employee_id
            //             where employee.loc_ID.Contains(location)
            //             orderby employee.full_name
            //             select new
            //             {
            //                 employee.employee_id,
            //                 employee.first_name,
            //                 employee.middle_initial,
            //                 employee.last_name,
            //                 employee.cta_email,
            //                 employee.cta_cell,
            //                 employee.cta_position,
            //                 employee.loc_ID,
            //                 employee.rfid_number,
            //                 employee.sales_id,
            //                 employee.full_name,
            //                 employee.cta_direct_phone,
            //                 employee.Date_of_birth,
            //                 employee.status,
            //                 employee.pic_status,
            //                 employee.profile_pic
            //             }).ToList();
            //List<tbl_employee> emplyAttList = new List<tbl_employee>();
            //foreach (var item in employeeList)
            //{
            //    tbl_employee obj = new tbl_employee(); // ViewModel

            //    if (item.status == 1)
            //    {
            //        obj.employee_id = item.employee_id;
            //        obj.full_name = item.full_name;
            //        obj.cta_email = item.cta_email;
            //        obj.cta_position = item.cta_position;
            //        obj.profile_pic = item.profile_pic;
            //        emplyAttList.Add(obj);
            //    }

            //}
            var employeeList = db.tbl_employee.Where(x => x.loc_ID.Contains(location) &&x.status==1).OrderBy(x=>x.employee_id).ToList();
            model.EmployeesList = employeeList;
            model.MatchedLocs = populateLocations();
            return View(model);
        }
        [HttpPost]
        public ActionResult EmployeePermissionsChangelocation(EmployeePermissionsViewModel model)
        {
            return RedirectToAction("EmployeePermissions", new { locId = model.MatchedLocID });
        }

        [HttpGet]
        public ActionResult ManageEmployeePermisssions(string value)
        {

            Debug.WriteLine("In ManageEmployeePermisssions:" + value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(value);
            var credentialsObj = db.tbl_employee_credentials.Where(x => x.employee_id == empId).FirstOrDefault();
            var SehubAccessObj = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            var empDetails= db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
           
            if (SehubAccessObj is null)
            {
                tbl_sehub_access newAccess = new tbl_sehub_access();
                newAccess.employee_id = empId;
                newAccess.app_access = 0;
                newAccess.dashboard = 0;
                newAccess.calendar = 0;
                newAccess.newsletter = 0;
                newAccess.neworder = 0;
                newAccess.openorder = 0;
                newAccess.my_staff = 0;
                newAccess.payroll = 0;
                newAccess.attendance = 0;
                newAccess.new_hire_package = 0;
                newAccess.vacation_schedule = 0;
                newAccess.asset_control = 0;
                newAccess.calculator = 0;
                newAccess.fuel_log = 0;
                newAccess.library_access = 0;
                newAccess.manufacturing_plant = 0;
                newAccess.user_management = 0;
                newAccess.customer_reporting = 0;
                db.tbl_sehub_access.Add(newAccess);
                db.SaveChanges();
            }
            if(credentialsObj is null)
            {
                tbl_employee_credentials newCredentials = new tbl_employee_credentials();
                newCredentials.employee_id = empId;
                newCredentials.password = null;
                newCredentials.permission = true;
                newCredentials.user_name = empDetails.first_name;
                newCredentials.password365 = null;
                newCredentials.management_permissions = false;
                newCredentials.administrative_permissions = false;
                newCredentials.additional_recipient = null;
                db.tbl_employee_credentials.Add(newCredentials);
                db.SaveChanges();
            }
            var AddSehubAccessObj = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            var AddcredentialsObj = db.tbl_employee_credentials.Where(x => x.employee_id == empId).FirstOrDefault();
            ModifyEmployeePermissions obj = new ModifyEmployeePermissions();
            obj.EmployeeCredentials = AddcredentialsObj;
            obj.SehubAccess = AddSehubAccessObj;
            obj.empDetails = empDetails;
            return PartialView(obj);
        }

        [HttpPost]
        public ActionResult ManageEmployeePermisssions(ModifyEmployeePermissions model)
        {
            Debug.WriteLine("App access:"+model.SehubAccess.app_access);
            Debug.WriteLine("Library access:" + model.SehubAccess.library_access);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var SehubAccessObj = db.tbl_sehub_access.Where(x => x.employee_id == model.EmployeeCredentials.employee_id).FirstOrDefault();
            var credentialsObj = db.tbl_employee_credentials.Where(x => x.employee_id == model.EmployeeCredentials.employee_id).FirstOrDefault();
            var Empdetails = db.tbl_employee.Where(x => x.employee_id == model.empDetails.employee_id).FirstOrDefault();
            var empAttendace=db.tbl_employee_attendance.Where(x => x.employee_id == model.empDetails.employee_id).FirstOrDefault();
            if (credentialsObj!=null)
            {
                credentialsObj.password = model.EmployeeCredentials.password;
                credentialsObj.password365 = model.EmployeeCredentials.password365;
            }
            if (SehubAccessObj!=null)
            {
                SehubAccessObj.app_access = model.SehubAccess.app_access;
                SehubAccessObj.dashboard = model.SehubAccess.dashboard;
                SehubAccessObj.calendar = model.SehubAccess.calendar;
                SehubAccessObj.newsletter = model.SehubAccess.newsletter;
                SehubAccessObj.neworder = model.SehubAccess.neworder;
                SehubAccessObj.openorder = model.SehubAccess.openorder;
                SehubAccessObj.my_staff = model.SehubAccess.my_staff;
                SehubAccessObj.payroll = model.SehubAccess.payroll;
                SehubAccessObj.attendance = model.SehubAccess.attendance;
                SehubAccessObj.new_hire_package = model.SehubAccess.new_hire_package;
                SehubAccessObj.vacation_schedule = model.SehubAccess.vacation_schedule;
                SehubAccessObj.asset_control = model.SehubAccess.asset_control;
                SehubAccessObj.calculator = model.SehubAccess.calculator;
                SehubAccessObj.fuel_log = model.SehubAccess.fuel_log;
                SehubAccessObj.library_access = model.SehubAccess.library_access;
                SehubAccessObj.manufacturing_plant = model.SehubAccess.manufacturing_plant;
                SehubAccessObj.user_management = model.SehubAccess.user_management;
                SehubAccessObj.customer_reporting = model.SehubAccess.customer_reporting;
            }
            if(Empdetails!=null)
            {
                Empdetails.rfid_number = model.empDetails.rfid_number;
                Empdetails.loc_ID = model.empDetails.loc_ID;
            }

            if(empAttendace!=null)
            {
                empAttendace.rfid_number = model.empDetails.rfid_number;
                empAttendace.at_work_location = model.empDetails.loc_ID;
            }
            else
            {
                tbl_employee_attendance newAttendacnRow = new tbl_employee_attendance();
                newAttendacnRow.employee_id = model.empDetails.employee_id;
                newAttendacnRow.rfid_number = model.empDetails.rfid_number;
                newAttendacnRow.at_work = false;
                newAttendacnRow.at_work_location = model.empDetails.loc_ID;
                db.tbl_employee_attendance.Add(newAttendacnRow);

            }
            db.SaveChanges();
            return RedirectToAction("EmployeePermissions", new { locId = "" });
        }

        //[HttpGet]
        //public ActionResult AddEmployeePermisssions(string value)
        //{
        //    Debug.WriteLine("In AddEmployeePermisssions");
        //    AddNewPermissions obj = new AddNewPermissions();
          
        //    obj.MatchedLocsCred = populateLocations();
        //    obj.MatchedEmployeeNameCred = "Select";
        //    return PartialView(obj);
        //}
        //[HttpPost]
        //public ActionResult AddEmployeePermisssions(AddNewPermissions modal)
        //{
        //    int employeeId = Convert.ToInt32(modal.MatchedEmployeeIdCred);
        //    CityTireAndAutoEntities db = new CityTireAndAutoEntities();
        //    var employeeDetails = db.tbl_employee.Where(x => x.employee_id == employeeId).FirstOrDefault();
        //    if (employeeDetails != null)
        //    {
        //        employeeDetails.rfid_number = modal.RFIDCred;

        //    }
        //    modal.SehubAccess.employee_id = employeeId;
        //    db.tbl_sehub_access.Add(modal.SehubAccess);
        //    db.SaveChanges();
        //    return RedirectToAction("EmployeePermissions", new { locId = "" });
        //}

        [HttpGet]
        public ActionResult AssetControl()
        {
            Debug.WriteLine("In AssetControl");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("empId:" + empId);
            var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            string locationid = "";
            if (result != null)
            {
                locationid = result.loc_ID;
            }
            else
            {
                locationid = "";
            }
            var VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == locationid).OrderBy(x => x.vehicle_short_id);


            if (VehicleDetails != null)
            {
                Debug.WriteLine("Vehicle info there are details");
                return View(VehicleDetails.ToList());
            }
            else
            {
                Debug.WriteLine("Vehicle info empty");
                return View();
            }

        }

        [HttpGet]
        public ActionResult AddNewVehicle(string value)
        {
          
            Debug.WriteLine("In AddNewVehicle");

            AddNewVehicleViewModel obj = new AddNewVehicleViewModel();
            obj.MatchedLocs = populateLocations();
            obj.MatchedEmployeeName = "Branch";
            return PartialView(obj);
        }

        [HttpGet]
        public ActionResult EditVehicleInfo(string value)
        {

            Debug.WriteLine("In EditVehicleInfo:"+ value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var vehicleInfoObj= db.tbl_vehicle_info.Where(x => x.VIN == value).FirstOrDefault();
            string employeeNameValue = "";
            if(vehicleInfoObj.assigned_to is null)
            {
                //Do Nothing
            }
            else
            {
                if(vehicleInfoObj.assigned_to!=0)
                {
                    var employeeTableCheck = db.tbl_employee.Where(x => x.employee_id == vehicleInfoObj.assigned_to).FirstOrDefault();
                    employeeNameValue = employeeTableCheck.full_name;
                }
                
            }

            AddNewVehicleViewModel obj = new AddNewVehicleViewModel();
            obj.VehicleInfo = vehicleInfoObj;
            obj.MatchedLocs = populateLocations();
            obj.MatchedLocID = vehicleInfoObj.loc_id;
            Debug.WriteLine("Full Name:" + employeeNameValue);
           
            obj.MatchedEmployeeName = employeeNameValue;
            Debug.WriteLine("In EditVehicleInfo date:" + vehicleInfoObj.inspection_due_date);
            return PartialView(obj);
        }


        public JsonResult GetEmployeeList(string locationId)
        {

            Debug.WriteLine("In GetEmployeeId requested location:" + locationId);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<tbl_employee> empList = db.tbl_employee.Where(x => x.loc_ID == locationId && x.status==1).OrderBy(x =>x.full_name).ToList();
            JsonResult result = Json(empList, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = 8675309;   
            return result;
            //return Json(empList, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetUnAssignedEmployeeCredentials(string locationId)
        {

            Debug.WriteLine("In GetUnAssignedEmployeeCredentials:" + locationId);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            db.Configuration.ProxyCreationEnabled = false;


            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select * FRom tbl_employee where employee_id not in (select employee_id from tbl_employee_credentials) and loc_ID="+ locationId+" and status=1";
                Debug.WriteLine("Query:" + query);
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
                                Text = sdr["employee_id"].ToString(),
                                Value = sdr["full_name"].ToString()
                            });
                        }


                    }
                    con.Close();
                }
            }
            //var UnAssinedCredentials = (from emp in db.tbl_employee

            //                            join cred in db.tbl_employee_credentials

            //                            on emp.employee_id equals cred.employee_id into empValues

            //                            from ed in empValues.DefaultIfEmpty()

            //                            select new

            //                            {

            //                                emp.employee_id,
            //                                emp.full_name,
            //                                emp.status

            //                            }).ToList();

            List<tbl_employee> emplyAttList = new List<tbl_employee>();
            foreach (var item in items)
            {
                tbl_employee emp_obj = new tbl_employee();

               
                    Debug.WriteLine("item.employee_id:" + item.Text);
                    emp_obj.employee_id = Convert.ToInt32(item.Text);
                    emp_obj.full_name = item.Value;
                    emplyAttList.Add(emp_obj);


            }           
            JsonResult result = Json(emplyAttList, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = 8675309;
            return result;
            //return Json(empList, JsonRequestBehavior.AllowGet);

        }
        private static List<SelectListItem> populateLocations()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select loc_id From tbl_cta_location_info where loc_status=1";
                Debug.WriteLine("Query:" + query);
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

        private static List<SelectListItem> populatePayrollId(string empId, string locId)
        {
            
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string todatDate = DateTime.Now.ToString("yyyy-MM-dd");
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT payroll_id FRom tbl_employee_payroll_dates where start_date <='"+ todatDate + "' order by payroll_id desc";
                Debug.WriteLine("Query:" + query);
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
                                Text ="20"+sdr["payroll_id"].ToString().Substring(0, 2)+"-"+ sdr["payroll_id"].ToString().Substring(sdr["payroll_id"].ToString().Length - 2),
                                Value = sdr["payroll_id"].ToString()+";"+ empId+";"+ locId
                            });
                        }


                    }
                    con.Close();
                }
            }

            return items;
        }



        [HttpPost]
        public ActionResult AddNewVehicle(AddNewVehicleViewModel model, HttpPostedFileBase vehicleImage)
        {

           
            byte[] imageBytes = null;
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(vehicleImage.FileName);
                Debug.WriteLine("vehicleImage:" + imageName);
                string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;
                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
                using (Image image = Image.FromFile(fileName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        Debug.WriteLine("Image base64:" + base64String);                                          
                    }
                }
            }
            model.VehicleInfo.assigned_to =Convert.ToInt32(model.MatchedEmployeeID);
            model.VehicleInfo.loc_id = model.MatchedLocID.ToString();
            model.VehicleInfo.vehicle_status = 1;
            model.VehicleInfo.vehicle_image = imageBytes;
            model.VehicleInfo.current_milage = 0;
            model.VehicleInfo.efficiency_liter = 0;
            model.VehicleInfo.efficiency_price = 0;
            string vehicleFirst3= model.VehicleInfo.vehicle_long_id.ToString().Substring(0, 3);
            string vehicleLast4 = model.VehicleInfo.vehicle_long_id.ToString().Substring(model.VehicleInfo.vehicle_long_id.ToString().Length - 4);
            model.VehicleInfo.vehicle_short_id = vehicleFirst3 + "-" + vehicleLast4;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            db.tbl_vehicle_info.Add(model.VehicleInfo);
           
            db.SaveChanges();


            return RedirectToAction("AssetControl");
        }


        [HttpPost]
        public ActionResult EditVehicleInfo(AddNewVehicleViewModel model, HttpPostedFileBase vehicleImage)
        {


            byte[] imageBytes = null;
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(vehicleImage.FileName);
                Debug.WriteLine("vehicleImage:" + imageName);
                string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;
                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
                using (Image image = Image.FromFile(fileName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }
          
            string vehicleFirst3 = model.VehicleInfo.vehicle_long_id.ToString().Substring(0, 3);
            string vehicleLast4 = model.VehicleInfo.vehicle_long_id.ToString().Substring(model.VehicleInfo.vehicle_long_id.ToString().Length - 4);
           
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var result = db.tbl_vehicle_info.Where(a => a.VIN== model.VehicleInfo.VIN).FirstOrDefault();
            if (result != null)
            {
                result.vehicle_long_id = model.VehicleInfo.vehicle_long_id;
                result.vehicle_short_id = vehicleFirst3 + "-" + vehicleLast4;
                result.vehicle_plate = model.VehicleInfo.vehicle_plate;
                result.loc_id = model.MatchedLocID.ToString();
                //if(model.MatchedEmployeeName is null )
                //{
                //    Debug.WriteLine("Level 1:"+ model.MatchedEmployeeName);
                //    if(Convert.ToInt32(model.MatchedEmployeeID) != 0)
                //    {
                //        Debug.WriteLine("Level 2:"+ Convert.ToInt32(model.MatchedEmployeeID));
                //        result.assigned_to = Convert.ToInt32(model.MatchedEmployeeID);
                //    }

                //}               
                Debug.WriteLine("Level 2:" + Convert.ToInt32(model.MatchedEmployeeID));
                if (Convert.ToInt32(model.MatchedEmployeeID) != 0)
                {
                   
                    result.assigned_to = Convert.ToInt32(model.MatchedEmployeeID);
                }

                result.vehicle_year = model.VehicleInfo.vehicle_year;
                result.vehicle_manufacturer = model.VehicleInfo.vehicle_manufacturer;
                result.vehicle_model = model.VehicleInfo.vehicle_model;
                result.inspection_due_date = model.VehicleInfo.inspection_due_date;
                if(imageBytes!=null)
                {
                    result.vehicle_image = imageBytes;
                }
               
               
            }
          
            db.SaveChanges();
            return RedirectToAction("AssetControl");
        }
        [HttpGet]
        public ActionResult AddStaff()
        {
            AddNewEmployeeViewModel model = new AddNewEmployeeViewModel();
            model.MatchedLocs = populateLocations();
            return View(model);
        }
        [HttpPost]
        public ActionResult AddStaff(AddNewEmployeeViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_employee empdetails = new tbl_employee();
            empdetails = model.StaffWorkDetails;
            empdetails.full_name = model.StaffWorkDetails.last_name + "," + model.StaffWorkDetails.first_name;
            empdetails.status = 1;
            empdetails.pic_status = 0;
            tbl_employee_personal personalDetails = new tbl_employee_personal();
            personalDetails = model.StaffPersonalDetails;
            personalDetails.employee_id = empdetails.employee_id;

            db.tbl_employee.Add(empdetails);
            db.tbl_employee_personal.Add(personalDetails);
            db.SaveChanges();
            return RedirectToAction("MyStaff", new { LocId = "" });
        }
        [HttpPost]
        public ActionResult AddEmployee(MyStaffViewModel model, HttpPostedFileBase EmployeeImage)
        {

            byte[] imageBytes = null;
            if (EmployeeImage != null && EmployeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(EmployeeImage.FileName);
                Debug.WriteLine("EmployeeImage:" + imageName);
                string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;
                
                using (Image image = Image.FromFile(fileName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_employee empdetails = new tbl_employee();
            empdetails = model.NewEmployee;
            empdetails.full_name = model.NewEmployee.last_name + "," + model.NewEmployee.first_name;
            empdetails.status = 1;
            empdetails.pic_status = 0;
            empdetails.profile_pic = imageBytes;
            tbl_employee_personal personalDetails = new tbl_employee_personal();
            personalDetails = model.NewEmployeePersonal;
            personalDetails.employee_id = empdetails.employee_id;

            db.tbl_employee.Add(empdetails);
            db.tbl_employee_personal.Add(personalDetails);
            db.SaveChanges();
            return RedirectToAction("MyStaff", new { LocId = "" });
        }


        [HttpGet]
        public ActionResult EditEmployeeInfo(string value)
        {
            Debug.WriteLine("Inside EditEmployeeInfo:" + value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            MyStaffViewModel model = new MyStaffViewModel();
            int empId=Convert.ToInt32(value);
            var employeeInfoObj = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            var employeePersonalInfoObj = db.tbl_employee_personal.Where(x => x.employee_id == empId).FirstOrDefault();
            if (employeeInfoObj is null)
            {
                //Do Nothing
            }
            else
            {
                
                model.NewEmployee = employeeInfoObj;     
                if(employeeInfoObj.status==1)
                {
                    model.active_status = true;

                }
                else
                {
                    model.active_status = false;
                }
            }
            if(employeePersonalInfoObj is null)
            {

            }
            else
            {
                model.NewEmployeePersonal = employeePersonalInfoObj;
            }
            model.MatchedStaffLocs = populateLocations();
            //obj.MatchedLocs = populateLocations();
            //obj.MatchedLocID = vehicleInfoObj.loc_id;        
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult EditEmployeeInfo(MyStaffViewModel model, HttpPostedFileBase EmployeeImage)
        {

            byte[] imageBytes = null;
            if (EmployeeImage != null && EmployeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(EmployeeImage.FileName);
                Debug.WriteLine("EmployeeImage:" + imageName);
                string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;

                using (Image image = Image.FromFile(fileName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeInfo = db.tbl_employee.Where(a => a.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();
            var PersonalDetails = db.tbl_employee_personal.Where(a => a.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();
            if (EmployeeInfo!=null)
            {
                EmployeeInfo.first_name = model.NewEmployee.first_name;
                EmployeeInfo.middle_initial = model.NewEmployee.middle_initial;
                EmployeeInfo.last_name = model.NewEmployee.last_name;
                EmployeeInfo.cta_email = model.NewEmployee.cta_email;
                EmployeeInfo.cta_cell = model.NewEmployee.cta_cell;
                EmployeeInfo.cta_position = model.NewEmployee.cta_position;
                EmployeeInfo.loc_ID = model.NewEmployee.loc_ID;
                EmployeeInfo.sales_id = model.NewEmployee.sales_id;
                EmployeeInfo.full_name = model.NewEmployee.last_name + "," + model.NewEmployee.first_name;
                EmployeeInfo.cta_direct_phone = model.NewEmployee.cta_direct_phone;
                if(model.active_status==true)
                {
                    EmployeeInfo.status = 1;
                }
                else
                {
                    EmployeeInfo.status = 0;
                }
                if(imageBytes!=null)
                {
                    EmployeeInfo.profile_pic = imageBytes;
                }                       
            }
            if(PersonalDetails != null)
            {
                PersonalDetails.personal_email = model.NewEmployeePersonal.personal_email;
                PersonalDetails.home_street1 = model.NewEmployeePersonal.home_street1;
                PersonalDetails.home_street2 = model.NewEmployeePersonal.home_street2;
                PersonalDetails.city = model.NewEmployeePersonal.city;
                PersonalDetails.province = model.NewEmployeePersonal.province;
                PersonalDetails.country = model.NewEmployeePersonal.country;
                PersonalDetails.postal_code = model.NewEmployeePersonal.postal_code;
                PersonalDetails.emergency_contact_name = model.NewEmployeePersonal.emergency_contact_name;
                PersonalDetails.emergency_contact_number = model.NewEmployeePersonal.emergency_contact_number;

            }

            db.SaveChanges();
            return RedirectToAction("MyStaff", new { LocId = model.NewEmployee.loc_ID });
        }


        [HttpGet]
        public ActionResult MyStaff(string LocId)
        {
            Debug.WriteLine("In MyStaff");
            int empId = Convert.ToInt32(Session["userID"].ToString());
            int permissions=CheckPermissions(empId).my_staff.Value;

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
           
            string locationid = "";
            if (LocId == "" || LocId is null )
            {
                //Debug.WriteLine("empId:" + empId);
                var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
               
                if (result != null)
                {
                    locationid = result.loc_ID;
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


            var EmployeeDetails = db.tbl_employee.Where(x => x.loc_ID == locationid).OrderBy(x => x.full_name).ToList();
            MyStaffViewModel modal = new MyStaffViewModel();

            Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {

                modal.employeeDetails = EmployeeDetails;
                modal.MatchedStaffLocs= populateLocations();
                modal.MatchedStaffLocID = locationid;
                modal.EmployeePermissions = permissions;
                return View(modal);
            }
            else
            {
                return View();
            }

            
        }
        [HttpPost]
        public ActionResult MyStaff(MyStaffViewModel modal)
        {
            return RedirectToAction("MyStaff", new { LocId = modal.MatchedStaffLocID});
        }

        [HttpGet]
        public ActionResult StaffInfo(string values)
        {
            Debug.WriteLine("values:" + values);
            int EmpId = Convert.ToInt32(values);
            StaffEditInformationViewModel StaffDetails = new StaffEditInformationViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == EmpId).FirstOrDefault();
            var PersonalDetails = db.tbl_employee_personal.Where(x => x.employee_id == EmpId).FirstOrDefault();
            StaffDetails.StaffWorkDetails = EmployeeDetails;
            StaffDetails.StaffPersonalDetails = PersonalDetails;
            if(EmployeeDetails.status==1)
            {
                StaffDetails.active_status = true;
            }
            else
            {
                StaffDetails.active_status = false;
            }

            if (EmployeeDetails.pic_status == 1)
            {
                StaffDetails.monitor_status = true;
            }
            else
            {
                StaffDetails.monitor_status = false;
            }

            return View(StaffDetails);
        }

      

        [HttpPost]
        public ActionResult StaffInfoSave(StaffEditInformationViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == model.StaffWorkDetails.employee_id).FirstOrDefault();
            var PersonalDetails = db.tbl_employee_personal.Where(x => x.employee_id == model.StaffWorkDetails.employee_id).FirstOrDefault();
            int statusVal = 0;
            int picStatus = 0;
            Debug.WriteLine( model.active_status);
            if (model.active_status == true)
            {
                statusVal = 1;
            }
            else
            {
                statusVal = 0;
            }

            if (model.monitor_status==true)
            {
                picStatus = 1;
            }
            else
            {
                picStatus = 0;
            }
            if (EmployeeDetails!=null)
            {
                EmployeeDetails.first_name = model.StaffWorkDetails.first_name;
                EmployeeDetails.middle_initial = model.StaffWorkDetails.middle_initial;
                EmployeeDetails.last_name = model.StaffWorkDetails.last_name;
                EmployeeDetails.cta_email = model.StaffWorkDetails.cta_email;
                EmployeeDetails.cta_cell = model.StaffWorkDetails.cta_cell;
                EmployeeDetails.cta_position = model.StaffWorkDetails.cta_position;
                EmployeeDetails.loc_ID = model.StaffWorkDetails.loc_ID;
                EmployeeDetails.Date_of_birth = model.StaffWorkDetails.Date_of_birth;
                EmployeeDetails.status = statusVal;
                EmployeeDetails.pic_status = picStatus;

                Debug.WriteLine("EmployeeDetails in save:"+EmployeeDetails.employee_id);
                Debug.WriteLine("EmployeeDetails in save:" + EmployeeDetails.first_name);
                Debug.WriteLine("EmployeeDetails in save:" + statusVal);
                Debug.WriteLine("EmployeeDetails in save:" + picStatus);
            }

            if (PersonalDetails != null)
            {
                PersonalDetails.personal_email = model.StaffPersonalDetails.personal_email;
                PersonalDetails.primary_phone = model.StaffPersonalDetails.primary_phone;
                PersonalDetails.home_street1 = model.StaffPersonalDetails.home_street1;
                PersonalDetails.home_street2 = model.StaffPersonalDetails.home_street2;
                PersonalDetails.city = model.StaffPersonalDetails.city;
                PersonalDetails.province = model.StaffPersonalDetails.province;
                PersonalDetails.country = model.StaffPersonalDetails.country;
                PersonalDetails.postal_code = model.StaffPersonalDetails.postal_code;
                PersonalDetails.emergency_contact_name = model.StaffPersonalDetails.emergency_contact_name;
                PersonalDetails.emergency_contact_number = model.StaffPersonalDetails.emergency_contact_number;

                Debug.WriteLine("EmployeeDetails in save:" + PersonalDetails.employee_id);
                Debug.WriteLine("EmployeeDetails in save:" + PersonalDetails.primary_phone);
            }
            db.SaveChanges();
            return RedirectToAction("StaffInfo", new { values = model.StaffWorkDetails.employee_id });
        }




        [HttpGet]
        public ActionResult Payroll(string locId,string employeeId,string payrollID)
        {
            Debug.WriteLine("On Payroll Load:"+ locId+"  :"+ employeeId+":"+ payrollID);
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string location = "101";
            
            if(locId is null || locId =="")
            {
                var locationDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();


                if (locationDetails != null)
                {
                    location = locationDetails.loc_ID;
                }

            }
            else
            {
                location = locId;
            }
            Debug.WriteLine("Location:" + location);
            ViewData["LocationList"] = location;

            var CurrentPayrollId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).FirstOrDefault();
           
            System.Collections.ArrayList arrayLIST = new System.Collections.ArrayList();
            arrayLIST.Clear();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    sb.Append("SELECT emp.employee_id as empdId,full_name,recordflag FROM tbl_employee emp, tbl_employee_payroll_biweekly biweek, tbl_employee_payroll_dates paydates where emp.employee_id = biweek.employee_id and biweek.payroll_id = paydates.payroll_Id and emp.loc_ID ="+ location + " and paydates.payroll_id = (select payroll_Id FRom  tbl_employee_payroll_dates where start_date <='"+Convert.ToDateTime(CurrentPayrollId.start_date).ToString("yyyy-MM-dd") + "' and end_date >= '"+ Convert.ToDateTime(CurrentPayrollId.end_date).ToString("yyyy-MM-dd") + "')");
                    sb2.Append("select employee_id,full_name FRom tbl_employee where loc_ID="+ location +" and status=1 and employee_id not in (SELECT emp.employee_id FROM tbl_employee emp, tbl_employee_payroll_biweekly biweek, tbl_employee_payroll_dates paydates where emp.employee_id = biweek.employee_id and biweek.payroll_id = paydates.payroll_Id and emp.loc_ID ="+ location + " and paydates.payroll_id = (select payroll_Id FRom  tbl_employee_payroll_dates where start_date <='" + Convert.ToDateTime(CurrentPayrollId.start_date).ToString("yyyy-MM-dd") + "' and end_date >= '" + Convert.ToDateTime(CurrentPayrollId.end_date).ToString("yyyy-MM-dd") + "'))");

                    string sql = sb.ToString();
                    string sql2 = sb2.ToString();
                    Debug.WriteLine("sql :" + sql);
                    Debug.WriteLine("sql2 :" + sql2);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        //command.Parameters.AddWithValue("@payid", currentPayRollId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                arrayLIST.Add((dr["empdId"].ToString()+";"+dr["full_name"].ToString()+";" + dr["recordflag"].ToString()));
                                //arrayLIST.Add((dr["recordflag"].ToString()));
                            }

                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {

                        //command.Parameters.AddWithValue("@payid", currentPayRollId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                arrayLIST.Add((dr["employee_id"].ToString() + ";"+ dr["full_name"].ToString() + ";" + "0"));
                                //arrayLIST.Add((dr["recordflag"].ToString()));
                            }

                        }
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            foreach(var item in arrayLIST)
            {
                Debug.WriteLine("arrayLIST:" + item.ToString());
            }
           
            List<EmployeePayrollListModel> emplyPayrollList = new List<EmployeePayrollListModel>();
            foreach (var item in arrayLIST)
            {
                EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                               //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                string[] values = item.ToString().Split(';');
                obj.employeeId = values[0];
                obj.fullName = values[1];
                obj.submissionStatus = values[2];
                emplyPayrollList.Add(obj);

            }

            EmployeePayrollModel payrollModel = new EmployeePayrollModel();
            payrollModel.employeepayrollList = emplyPayrollList;
            payrollModel.MatchedLocs = populateLocations();
            payrollModel.PayrollIdList = populatePayrollId(employeeId, location);
            payrollModel.MatchedLocID = location;
            payrollModel.SelectedEmployeeId = employeeId;
           // payrollModel.SelectedPayrollId = payrollID + ";" + employeeId + ";" + location;
            string EmployeeIdvalue =employeeId;

            if (employeeId is null || employeeId == "")
            {

            }
            else
            {
                int passThisValue = Convert.ToInt32(EmployeeIdvalue);
                var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == passThisValue).FirstOrDefault();
                string EmployeeID = "";
                string LocationID = "";
                string position = "";
                string FullName = "";
                byte[] profileImg = null;
                if (EmployeeDetails != null)
                {
                    EmployeeID = EmployeeDetails.employee_id.ToString();
                    FullName = EmployeeDetails.full_name;
                    LocationID = EmployeeDetails.loc_ID;
                    position = EmployeeDetails.cta_position;
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

                Debug.WriteLine("Image_base64:" + base64ProfilePic);
                ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
                ViewData["EmployeeName"] = FullName;
                ViewData["EmployeeId"] = EmployeeID;
                ViewData["Position"] = position;
                DateTime todatDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                //DateTime todatDate = DateTime.Parse("2020-04-30");
                Debug.WriteLine("Date time today:" + todatDate);
                int payrollIDInfo = 0;
                string payrollStartDate = "";
                string payrollEndDate = "";


                var checkCurrentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= todatDate && x.end_date >= todatDate).FirstOrDefault();
                payrollModel.currentPayRollId = checkCurrentPayrollID.payroll_Id;

                if (payrollID is null || payrollID=="")
                {
                    var payRollDates = db.tbl_employee_payroll_dates.Where(x => x.start_date <= todatDate && x.end_date >= todatDate).FirstOrDefault();
                    payrollIDInfo = payRollDates.payroll_Id;
                    payrollStartDate = payRollDates.start_date.ToString();
                    payrollEndDate = payRollDates.end_date.ToString();

                    payrollModel.SelectedPayrollId = payrollIDInfo + ";" + employeeId + ";" + location;
                }
                else
                {                   
                    int payIdNumber = Convert.ToInt32(payrollID);
                    var payRollDates = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id== payIdNumber).FirstOrDefault();
                    payrollIDInfo = payRollDates.payroll_Id;
                    payrollStartDate = payRollDates.start_date.ToString();
                    payrollEndDate = payRollDates.end_date.ToString();                
                    payrollModel.SelectedPayrollId = payrollIDInfo + ";" + employeeId + ";" + location;

                }
                
                Debug.WriteLine("payRollDates Values:" + payrollIDInfo);
                LoadPayroll(EmployeeID, payrollStartDate, payrollEndDate, payrollIDInfo.ToString());
                LoadLeaveHistory(EmployeeID);


                int EmployeeIdParam = Convert.ToInt32(EmployeeID);
                var payrollBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == EmployeeIdParam && x.payroll_id == payrollIDInfo).FirstOrDefault();
                var payrollSummary = db.tbl_employee_payroll_summary.Where(x => x.employee_id == EmployeeIdParam && x.payroll_id == payrollIDInfo).FirstOrDefault();

                PayRollViewModel payrollDetails = new PayRollViewModel();
                if (payrollBiweekly != null)
                {
                    EditPayrollBiWeeklyViewModel payrollBiWeekDetails = new EditPayrollBiWeeklyViewModel();
                    payrollBiWeekDetails.employee_id = payrollBiweekly.employee_id;
                    payrollBiWeekDetails.payroll_id = payrollBiweekly.payroll_id;
                    payrollBiWeekDetails.sat_1_reg = payrollBiweekly.sat_1_reg;
                    payrollBiWeekDetails.mon_1_reg = payrollBiweekly.mon_1_reg;
                    payrollBiWeekDetails.tues_1_reg = payrollBiweekly.tues_1_reg;
                    payrollBiWeekDetails.wed_1_reg = payrollBiweekly.wed_1_reg;
                    payrollBiWeekDetails.thurs_1_reg = payrollBiweekly.thurs_1_reg;
                    payrollBiWeekDetails.fri_1_reg = payrollBiweekly.fri_1_reg;
                    payrollBiWeekDetails.sat_2_reg = payrollBiweekly.sat_2_reg;
                    payrollBiWeekDetails.mon_2_reg = payrollBiweekly.mon_2_reg;
                    payrollBiWeekDetails.tues_2_reg = payrollBiweekly.tues_2_reg;
                    payrollBiWeekDetails.wed_2_reg = payrollBiweekly.wed_2_reg;
                    payrollBiWeekDetails.thurs_2_reg = payrollBiweekly.thurs_2_reg;
                    payrollBiWeekDetails.fri_2_reg = payrollBiweekly.fri_2_reg;
                    payrollBiWeekDetails.sat_1_opt = payrollBiweekly.sat_1_opt;
                    payrollBiWeekDetails.mon_1_opt = payrollBiweekly.mon_1_opt;
                    payrollBiWeekDetails.tues_1_opt = payrollBiweekly.tues_1_opt;
                    payrollBiWeekDetails.wed_1_opt = payrollBiweekly.wed_1_opt;
                    payrollBiWeekDetails.thurs_1_opt = payrollBiweekly.thurs_1_opt;
                    payrollBiWeekDetails.fri_1_opt = payrollBiweekly.fri_1_opt;
                    payrollBiWeekDetails.sat_2_opt = payrollBiweekly.sat_2_opt;
                    payrollBiWeekDetails.mon_2_opt = payrollBiweekly.mon_2_opt;
                    payrollBiWeekDetails.tues_2_opt = payrollBiweekly.tues_2_opt;
                    payrollBiWeekDetails.wed_2_opt = payrollBiweekly.wed_2_opt;
                    payrollBiWeekDetails.thurs_2_opt = payrollBiweekly.thurs_2_opt;
                    payrollBiWeekDetails.fri_2_opt = payrollBiweekly.fri_2_opt;
                    payrollBiWeekDetails.sat_1_sel = payrollBiweekly.sat_1_sel;
                    payrollBiWeekDetails.mon_1_sel = payrollBiweekly.mon_1_sel;
                    payrollBiWeekDetails.tues_1_sel = payrollBiweekly.tues_1_sel;
                    payrollBiWeekDetails.wed_1_sel = payrollBiweekly.wed_1_sel;
                    payrollBiWeekDetails.thurs_1_sel = payrollBiweekly.thurs_1_sel;
                    payrollBiWeekDetails.fri_1_sel = payrollBiweekly.fri_1_sel;
                    payrollBiWeekDetails.sat_2_sel = payrollBiweekly.sat_2_sel;
                    payrollBiWeekDetails.mon_2_sel = payrollBiweekly.mon_2_sel;
                    payrollBiWeekDetails.tues_2_sel = payrollBiweekly.tues_2_sel;
                    payrollBiWeekDetails.wed_2_sel = payrollBiweekly.wed_2_sel;
                    payrollBiWeekDetails.thurs_2_sel = payrollBiweekly.thurs_2_sel;
                    payrollBiWeekDetails.fri_2_sel = payrollBiweekly.fri_2_sel;
                    payrollBiWeekDetails.sat_1_sum = payrollBiweekly.sat_1_sum;
                    payrollBiWeekDetails.mon__1_sum = payrollBiweekly.mon__1_sum;
                    payrollBiWeekDetails.tues_1_sum = payrollBiweekly.tues_1_sum;
                    payrollBiWeekDetails.wed_1_sum = payrollBiweekly.wed_1_sum;
                    payrollBiWeekDetails.thurs_1_sum = payrollBiweekly.thurs_1_sum;
                    payrollBiWeekDetails.fri_1_sum = payrollBiweekly.fri_1_sum;
                    payrollBiWeekDetails.sat_2_sum = payrollBiweekly.sat_2_sum;
                    payrollBiWeekDetails.mon_2_sum = payrollBiweekly.mon_2_sum;
                    payrollBiWeekDetails.tues_2_sum = payrollBiweekly.tues_2_sum;
                    payrollBiWeekDetails.wed_2_Sum = payrollBiweekly.wed_2_Sum;
                    payrollBiWeekDetails.thurs_2_Sum = payrollBiweekly.thurs_2_Sum;
                    payrollBiWeekDetails.fri_2_sum = payrollBiweekly.fri_2_sum;
                    payrollBiWeekDetails.bi_week_chkin_avg = payrollBiweekly.bi_week_chkin_avg;
                    payrollBiWeekDetails.bi_week_chkout_avg = payrollBiweekly.bi_week_chkout_avg;
                    payrollBiWeekDetails.last_updated_by = payrollBiweekly.last_updated_by;
                    payrollBiWeekDetails.last_update_date = payrollBiweekly.last_update_date;
                    payrollBiWeekDetails.recordflag = payrollBiweekly.recordflag;
                    payrollBiWeekDetails.comments = payrollBiweekly.comments;
                    payrollBiWeekDetails.timeClock_sat1 = payrollBiweekly.timeClock_sat1;
                    payrollBiWeekDetails.timeClock_mon1 = payrollBiweekly.timeClock_mon1;
                    payrollBiWeekDetails.timeClock_tues1 = payrollBiweekly.timeClock_tues1;
                    payrollBiWeekDetails.timeClock_wed1 = payrollBiweekly.timeClock_wed1;
                    payrollBiWeekDetails.timeClock_thurs1 = payrollBiweekly.timeClock_thurs1;
                    payrollBiWeekDetails.timeClock_fri1 = payrollBiweekly.timeClock_fri1;
                    payrollBiWeekDetails.timeClock_sat2 = payrollBiweekly.timeClock_sat2;
                    payrollBiWeekDetails.timeClock_mon2 = payrollBiweekly.timeClock_mon2;
                    payrollBiWeekDetails.timeClock_tues2 = payrollBiweekly.timeClock_tues2;
                    payrollBiWeekDetails.timeClock_wed2 = payrollBiweekly.timeClock_wed2;
                    payrollBiWeekDetails.timeClock_thurs2 = payrollBiweekly.timeClock_thurs2;
                    payrollBiWeekDetails.timeClock_fri2 = payrollBiweekly.timeClock_fri2;
                    payrollBiWeekDetails.sun_1_reg = payrollBiweekly.sun_1_reg;
                    payrollBiWeekDetails.sun_2_reg = payrollBiweekly.sun_2_reg;
                    payrollBiWeekDetails.sun_1_opt = payrollBiweekly.sun_1_opt;
                    payrollBiWeekDetails.sun_2_opt = payrollBiweekly.sun_2_opt;
                    payrollBiWeekDetails.sun_1_sel = payrollBiweekly.sun_1_sel;
                    payrollBiWeekDetails.sun_2_sel = payrollBiweekly.sun_2_sel;
                    payrollBiWeekDetails.sun_1_sum = payrollBiweekly.sun_1_sum;
                    payrollBiWeekDetails.sun_2_sum = payrollBiweekly.sun_2_sum;
                    payrollBiWeekDetails.timeClock_sun1 = payrollBiweekly.timeClock_sun1;
                    payrollBiWeekDetails.timeClock_sun2 = payrollBiweekly.timeClock_sun2;

                    if (payrollBiweekly.sat_1_sel == " " || payrollBiweekly.sat_1_sel is null)
                    {
                        payrollBiWeekDetails.sat_1_sel_id = 1;
                    }
                    else
                    {
                        var sat1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.sat_1_sel_id = sat1details.category_id;
                        Debug.WriteLine("payrollBiWeekDetails.sat_1_sel_id:" + payrollBiWeekDetails.sat_1_sel_id);
                    }

                    if (payrollBiweekly.sun_1_sel == " " || payrollBiweekly.sun_1_sel is null)
                    {
                        Debug.WriteLine("Yes2:" + payrollBiweekly.sun_1_sel + ":");
                        payrollBiWeekDetails.sun_1_sel_id = 1;
                    }
                    else
                    {
                        var sun1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.sun_1_sel_id = sun1details.category_id;


                    }

                    if (payrollBiweekly.mon_1_sel == " " || payrollBiweekly.mon_1_sel is null)
                    {
                        payrollBiWeekDetails.mon_1_sel_id = 1;
                    }
                    else
                    {
                        var mon1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.mon_1_sel_id = mon1details.category_id;
                    }

                    if (payrollBiweekly.tues_1_sel == " " || payrollBiweekly.tues_1_sel is null)
                    {
                        payrollBiWeekDetails.tues_1_sel_id = 1;
                    }
                    else
                    {
                        var tues1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.tues_1_sel_id = tues1details.category_id;
                    }

                    if (payrollBiweekly.wed_1_sel == " " || payrollBiweekly.wed_1_sel is null)
                    {
                        payrollBiWeekDetails.wed_1_sel_id = 1;
                    }
                    else
                    {
                        var wed1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.wed_1_sel_id = wed1details.category_id;
                    }

                    if (payrollBiweekly.thurs_1_sel == " " || payrollBiweekly.thurs_1_sel is null)
                    {
                        payrollBiWeekDetails.thurs_1_sel_id = 1;
                    }
                    else
                    {
                        var thurs1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.thurs_1_sel_id = thurs1details.category_id;
                    }

                    if (payrollBiweekly.fri_1_sel == " " || payrollBiweekly.fri_1_sel is null)
                    {
                        payrollBiWeekDetails.fri_1_sel_id = 1;
                    }
                    else
                    {
                        var fri1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.fri_1_sel_id = fri1details.category_id;
                    }

                    if (payrollBiweekly.sat_2_sel == " " || payrollBiweekly.sat_2_sel is null)
                    {
                        payrollBiWeekDetails.sat_2_sel_id = 1;
                    }
                    else
                    {
                        var sat2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.sat_2_sel_id = sat2details.category_id;
                    }

                    if (payrollBiweekly.sun_2_sel == " " || payrollBiweekly.sun_2_sel is null)
                    {
                        payrollBiWeekDetails.sun_2_sel_id = 1;
                    }
                    else
                    {
                        var sun2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.sun_2_sel_id = sun2details.category_id;
                    }

                    if (payrollBiweekly.mon_2_sel == " " || payrollBiweekly.mon_2_sel is null)
                    {
                        payrollBiWeekDetails.mon_2_sel_id = 1;
                    }
                    else
                    {
                        var mon2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.mon_2_sel_id = mon2details.category_id;
                    }

                    if (payrollBiweekly.tues_2_sel == " " || payrollBiweekly.tues_2_sel is null)
                    {
                        payrollBiWeekDetails.tues_2_sel_id = 1;
                    }
                    else
                    {
                        var tues2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.tues_2_sel_id = tues2details.category_id;
                    }

                    if (payrollBiweekly.wed_2_sel == " " || payrollBiweekly.wed_2_sel is null)
                    {
                        payrollBiWeekDetails.wed_2_sel_id = 1;
                    }
                    else
                    {
                        var wed2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.wed_2_sel_id = wed2details.category_id;
                    }

                    if (payrollBiweekly.thurs_2_sel == " " || payrollBiweekly.thurs_2_sel is null)
                    {
                        payrollBiWeekDetails.thurs_2_sel_id = 1;
                    }
                    else
                    {
                        var thurs2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.thurs_2_sel_id = thurs2details.category_id;
                    }

                    if (payrollBiweekly.fri_2_sel == " " || payrollBiweekly.fri_2_sel is null)
                    {
                        payrollBiWeekDetails.fri_2_sel_id = 1;
                    }
                    else
                    {
                        var fri2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.fri_2_sel_id = fri2details.category_id;
                    }

                    payrollDetails.payBiweek = payrollBiWeekDetails;
                }
                if (payrollSummary != null)
                {
                    payrollDetails.paySummary = payrollSummary;
                }
                var selectionList = (from a in db.tbl_payroll_category_selection select a).ToList();

                ViewBag.TypeSelectionList = selectionList;
                payrollModel.employeepayroll = payrollDetails;
            }
            return View(payrollModel);
        }

        private void LoadLeaveHistory(string employeeID)
        {

            Debug.WriteLine("In LoadLeave History");
            string year = DateTime.Now.ToString("yyyy").Substring(DateTime.Now.ToString("yyyy").Length - 2);
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select sum(regular) regular_sum,sum(ot) ot_sum From tbl_employee_payroll_summary where employee_id="+ employeeID + " and payroll_id like '"+ year + "%'");
                   
                    string sql = sb.ToString();
                    Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                ViewData["regualr_hours_year"] = dr["regular_sum"].ToString();
                                ViewData["ot_hours_year"] = dr["ot_sum"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }


        }

        [HttpPost]
        public ActionResult PayrrollChangeLocation(EmployeePayrollModel model)
        {
            Debug.WriteLine("PayrrollChangeLocation");
            
            return RedirectToAction("Payroll", new { locId = model.MatchedLocID, employeeId = "", payrollID="" });
          
        }

        [HttpPost]
        public ActionResult ChangePayrollID(EmployeePayrollModel model)
        {
            Debug.WriteLine("ChangePayrollID:");
            string[] values = model.SelectedPayrollId.Split(';');

            return RedirectToAction("Payroll", new { locId = values[2], employeeId = values[1], payrollID = values[0] });

        }
        public ActionResult ShowEmployeePayroll(string value)
        {

            Debug.WriteLine("In ShowEditCustInfo:"+ value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeDetails = db.tbl_employee.Where(x => x.full_name == value).FirstOrDefault();
            string empId = "";
            string locId = "";
            string position = "";
            byte[] profileImg = null;
            if (EmployeeDetails != null)
            {
                empId = EmployeeDetails.employee_id.ToString();
                locId = EmployeeDetails.loc_ID;
                position = EmployeeDetails.cta_position;
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

            Debug.WriteLine("Image_base64:" + base64ProfilePic);
            ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
            ViewData["EmployeeName"] = value;
            ViewData["EmployeeId"] = empId;
            ViewData["Position"] = position;
            //DateTime todatDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime todatDate = DateTime.Parse("2020-04-30");
            Debug.WriteLine("Date time today:" + todatDate);          
            var payRollDates = db.tbl_employee_payroll_dates.Where(x => x.start_date <= todatDate && x.end_date >= todatDate).FirstOrDefault();
            Debug.WriteLine("payRollDates Values:" + payRollDates.payroll_Id);

            LoadPayroll(empId, payRollDates.start_date.ToString(), payRollDates.end_date.ToString(), payRollDates.payroll_Id.ToString());

            int employeeId = Convert.ToInt32(empId);
            var payrollBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == employeeId && x.payroll_id == payRollDates.payroll_Id).FirstOrDefault();
            var payrollSummary = db.tbl_employee_payroll_summary.Where(x => x.employee_id == employeeId && x.payroll_id == payRollDates.payroll_Id).FirstOrDefault();
           
            PayRollViewModel payrollDetails = new PayRollViewModel();
            if (payrollBiweekly!=null)
            {
                EditPayrollBiWeeklyViewModel payrollBiWeekDetails = new EditPayrollBiWeeklyViewModel();
                payrollBiWeekDetails.employee_id = payrollBiweekly.employee_id;
                payrollBiWeekDetails.payroll_id = payrollBiweekly.payroll_id;
                payrollBiWeekDetails.sat_1_reg = payrollBiweekly.sat_1_reg;
                payrollBiWeekDetails.mon_1_reg = payrollBiweekly.mon_1_reg;
                payrollBiWeekDetails.tues_1_reg = payrollBiweekly.tues_1_reg;
                payrollBiWeekDetails.wed_1_reg = payrollBiweekly.wed_1_reg;
                payrollBiWeekDetails.thurs_1_reg = payrollBiweekly.thurs_1_reg;
                payrollBiWeekDetails.fri_1_reg = payrollBiweekly.fri_1_reg;
                payrollBiWeekDetails.sat_2_reg = payrollBiweekly.sat_2_reg;
                payrollBiWeekDetails.mon_2_reg = payrollBiweekly.mon_2_reg;
                payrollBiWeekDetails.tues_2_reg = payrollBiweekly.tues_2_reg;
                payrollBiWeekDetails.wed_2_reg = payrollBiweekly.wed_2_reg;
                payrollBiWeekDetails.thurs_2_reg = payrollBiweekly.thurs_2_reg;
                payrollBiWeekDetails.fri_2_reg = payrollBiweekly.fri_2_reg;
                payrollBiWeekDetails.sat_1_opt = payrollBiweekly.sat_1_opt;
                payrollBiWeekDetails.mon_1_opt = payrollBiweekly.mon_1_opt;
                payrollBiWeekDetails.tues_1_opt = payrollBiweekly.tues_1_opt;
                payrollBiWeekDetails.wed_1_opt = payrollBiweekly.wed_1_opt;
                payrollBiWeekDetails.thurs_1_opt = payrollBiweekly.thurs_1_opt;
                payrollBiWeekDetails.fri_1_opt = payrollBiweekly.fri_1_opt;
                payrollBiWeekDetails.sat_2_opt = payrollBiweekly.sat_2_opt;
                payrollBiWeekDetails.mon_2_opt = payrollBiweekly.mon_2_opt;
                payrollBiWeekDetails.tues_2_opt = payrollBiweekly.tues_2_opt;
                payrollBiWeekDetails.wed_2_opt = payrollBiweekly.wed_2_opt;
                payrollBiWeekDetails.thurs_2_opt = payrollBiweekly.thurs_2_opt;
                payrollBiWeekDetails.fri_2_opt = payrollBiweekly.fri_2_opt;
                payrollBiWeekDetails.sat_1_sel = payrollBiweekly.sat_1_sel;
                payrollBiWeekDetails.mon_1_sel = payrollBiweekly.mon_1_sel;
                payrollBiWeekDetails.tues_1_sel = payrollBiweekly.tues_1_sel;
                payrollBiWeekDetails.wed_1_sel = payrollBiweekly.wed_1_sel;
                payrollBiWeekDetails.thurs_1_sel = payrollBiweekly.thurs_1_sel;
                payrollBiWeekDetails.fri_1_sel = payrollBiweekly.fri_1_sel;
                payrollBiWeekDetails.sat_2_sel = payrollBiweekly.sat_2_sel;
                payrollBiWeekDetails.mon_2_sel = payrollBiweekly.mon_2_sel;
                payrollBiWeekDetails.tues_2_sel = payrollBiweekly.tues_2_sel;
                payrollBiWeekDetails.wed_2_sel = payrollBiweekly.wed_2_sel;
                payrollBiWeekDetails.thurs_2_sel = payrollBiweekly.thurs_2_sel;
                payrollBiWeekDetails.fri_2_sel = payrollBiweekly.fri_2_sel;
                payrollBiWeekDetails.sat_1_sum = payrollBiweekly.sat_1_sum;
                payrollBiWeekDetails.mon__1_sum = payrollBiweekly.mon__1_sum;
                payrollBiWeekDetails.tues_1_sum = payrollBiweekly.tues_1_sum;
                payrollBiWeekDetails.wed_1_sum = payrollBiweekly.wed_1_sum;
                payrollBiWeekDetails.thurs_1_sum = payrollBiweekly.thurs_1_sum;
                payrollBiWeekDetails.fri_1_sum = payrollBiweekly.fri_1_sum;
                payrollBiWeekDetails.sat_2_sum = payrollBiweekly.sat_2_sum;
                payrollBiWeekDetails.mon_2_sum = payrollBiweekly.mon_2_sum;
                payrollBiWeekDetails.tues_2_sum = payrollBiweekly.tues_2_sum;
                payrollBiWeekDetails.wed_2_Sum = payrollBiweekly.wed_2_Sum;
                payrollBiWeekDetails.thurs_2_Sum = payrollBiweekly.thurs_2_Sum;
                payrollBiWeekDetails.fri_2_sum = payrollBiweekly.fri_2_sum;
                payrollBiWeekDetails.bi_week_chkin_avg = payrollBiweekly.bi_week_chkin_avg;
                payrollBiWeekDetails.bi_week_chkout_avg = payrollBiweekly.bi_week_chkout_avg;
                payrollBiWeekDetails.last_updated_by = payrollBiweekly.last_updated_by;
                payrollBiWeekDetails.last_update_date = payrollBiweekly.last_update_date;
                payrollBiWeekDetails.recordflag = payrollBiweekly.recordflag;
                payrollBiWeekDetails.comments = payrollBiweekly.comments;
                payrollBiWeekDetails.timeClock_sat1 = payrollBiweekly.timeClock_sat1;
                payrollBiWeekDetails.timeClock_mon1 = payrollBiweekly.timeClock_mon1;
                payrollBiWeekDetails.timeClock_tues1 = payrollBiweekly.timeClock_tues1;
                payrollBiWeekDetails.timeClock_wed1 = payrollBiweekly.timeClock_wed1;
                payrollBiWeekDetails.timeClock_thurs1 = payrollBiweekly.timeClock_thurs1;
                payrollBiWeekDetails.timeClock_fri1 = payrollBiweekly.timeClock_fri1;
                payrollBiWeekDetails.timeClock_sat2 = payrollBiweekly.timeClock_sat2;
                payrollBiWeekDetails.timeClock_mon2 = payrollBiweekly.timeClock_mon2;
                payrollBiWeekDetails.timeClock_tues2 = payrollBiweekly.timeClock_tues2;
                payrollBiWeekDetails.timeClock_wed2 = payrollBiweekly.timeClock_wed2;
                payrollBiWeekDetails.timeClock_thurs2 = payrollBiweekly.timeClock_thurs2;
                payrollBiWeekDetails.timeClock_fri2 = payrollBiweekly.timeClock_fri2;
                payrollBiWeekDetails.sun_1_reg = payrollBiweekly.sun_1_reg;
                payrollBiWeekDetails.sun_2_reg = payrollBiweekly.sun_2_reg;
                payrollBiWeekDetails.sun_1_opt = payrollBiweekly.sun_1_opt;
                payrollBiWeekDetails.sun_2_opt = payrollBiweekly.sun_2_opt;
                payrollBiWeekDetails.sun_1_sel = payrollBiweekly.sun_1_sel;
                payrollBiWeekDetails.sun_2_sel = payrollBiweekly.sun_2_sel;
                payrollBiWeekDetails.sun_1_sum = payrollBiweekly.sun_1_sum;
                payrollBiWeekDetails.sun_2_sum = payrollBiweekly.sun_2_sum;
                payrollBiWeekDetails.timeClock_sun1 = payrollBiweekly.timeClock_sun1;
                payrollBiWeekDetails.timeClock_sun2 = payrollBiweekly.timeClock_sun2;

                if(payrollBiweekly.sat_1_sel==" " || payrollBiweekly.sat_1_sel is null)
                {
                    payrollBiWeekDetails.sat_1_sel_id = 1;
                }
                else
                {
                    var sat1details= db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.sat_1_sel_id = sat1details.category_id;
                    Debug.WriteLine("payrollBiWeekDetails.sat_1_sel_id:" + payrollBiWeekDetails.sat_1_sel_id);
                }

                if (payrollBiweekly.sun_1_sel == " " || payrollBiweekly.sun_1_sel is null)
                {
                    Debug.WriteLine("Yes2:" + payrollBiweekly.sun_1_sel + ":");
                    payrollBiWeekDetails.sun_1_sel_id = 1;
                }
                else
                {
                    var sun1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.sun_1_sel_id = sun1details.category_id;
                   
                   
                }

                if (payrollBiweekly.mon_1_sel == " " || payrollBiweekly.mon_1_sel is null)
                {
                    payrollBiWeekDetails.mon_1_sel_id = 1;
                }
                else
                {
                    var mon1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.mon_1_sel_id = mon1details.category_id;
                }

                if (payrollBiweekly.tues_1_sel == " " || payrollBiweekly.tues_1_sel is null)
                {
                    payrollBiWeekDetails.tues_1_sel_id = 1;
                }
                else
                {
                    var tues1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.tues_1_sel_id = tues1details.category_id;
                }

                if (payrollBiweekly.wed_1_sel == " " || payrollBiweekly.wed_1_sel is null)
                {
                    payrollBiWeekDetails.wed_1_sel_id = 1;
                }
                else
                {
                    var wed1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.wed_1_sel_id = wed1details.category_id;
                }

                if (payrollBiweekly.thurs_1_sel == " " || payrollBiweekly.thurs_1_sel is null)
                {
                    payrollBiWeekDetails.thurs_1_sel_id = 1;
                }
                else
                {
                    var thurs1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.thurs_1_sel_id = thurs1details.category_id;
                }

                if (payrollBiweekly.fri_1_sel == " " || payrollBiweekly.fri_1_sel is null)
                {
                    payrollBiWeekDetails.fri_1_sel_id = 1;
                }
                else
                {
                    var fri1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.fri_1_sel_id = fri1details.category_id;
                }

                if (payrollBiweekly.sat_2_sel == " " || payrollBiweekly.sat_2_sel is null)
                {
                    payrollBiWeekDetails.sat_2_sel_id = 1;
                }
                else
                {
                    var sat2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.sat_2_sel_id = sat2details.category_id;
                }

                if (payrollBiweekly.sun_2_sel == " " || payrollBiweekly.sun_2_sel is null)
                {
                    payrollBiWeekDetails.sun_2_sel_id = 1;
                }
                else
                {
                    var sun2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.sun_2_sel_id = sun2details.category_id;
                }

                if (payrollBiweekly.mon_2_sel == " " || payrollBiweekly.mon_2_sel is null)
                {
                    payrollBiWeekDetails.mon_2_sel_id = 1;
                }
                else
                {
                    var mon2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.mon_2_sel_id = mon2details.category_id;
                }

                if (payrollBiweekly.tues_2_sel == " " || payrollBiweekly.tues_2_sel is null)
                {
                    payrollBiWeekDetails.tues_2_sel_id = 1;
                }
                else
                {
                    var tues2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.tues_2_sel_id = tues2details.category_id;
                }

                if (payrollBiweekly.wed_2_sel == " " || payrollBiweekly.wed_2_sel is null)
                {
                    payrollBiWeekDetails.wed_2_sel_id = 1;
                }
                else
                {
                    var wed2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.wed_2_sel_id = wed2details.category_id;
                }

                if (payrollBiweekly.thurs_2_sel == " " || payrollBiweekly.thurs_2_sel is null)
                {
                    payrollBiWeekDetails.thurs_2_sel_id = 1;
                }
                else
                {
                    var thurs2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.thurs_2_sel_id = thurs2details.category_id;
                }

                if (payrollBiweekly.fri_2_sel == " " || payrollBiweekly.fri_2_sel is null)
                {
                    payrollBiWeekDetails.fri_2_sel_id = 1;
                }
                else
                {
                    var fri2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.fri_2_sel_id = fri2details.category_id;
                }

                payrollDetails.payBiweek = payrollBiWeekDetails;
            }
            if (payrollSummary != null)
            {
                payrollDetails.paySummary = payrollSummary;
            }
            var selectionList = (from a in db.tbl_payroll_category_selection select a).ToList();
            
            ViewBag.TypeSelectionList = selectionList;

            return PartialView(payrollDetails);
        }

        [HttpPost]
        public ActionResult SubmitEmployeePayroll(EmployeePayrollModel model)
        {
            Debug.WriteLine("In Payroll Submission:"+ model.employeepayroll.payBiweek.employee_id+" "+ model.employeepayroll.payBiweek.payroll_id);
            int empID = model.employeepayroll.payBiweek.employee_id;
            int payrollId = model.employeepayroll.payBiweek.payroll_id;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var BiWeekDetails = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == empID && x.payroll_id== payrollId).FirstOrDefault();
            var SummaryDetails = db.tbl_employee_payroll_summary.Where(x => x.employee_id == empID && x.payroll_id == payrollId).FirstOrDefault();
            if (BiWeekDetails!=null && SummaryDetails!=null)
            {               
                BiWeekDetails.sat_1_reg = model.employeepayroll.payBiweek.sat_1_reg;
                BiWeekDetails.mon_1_reg = model.employeepayroll.payBiweek.mon_1_reg;
                BiWeekDetails.tues_1_reg = model.employeepayroll.payBiweek.tues_1_reg;
                BiWeekDetails.wed_1_reg = model.employeepayroll.payBiweek.wed_1_reg;
                BiWeekDetails.thurs_1_reg = model.employeepayroll.payBiweek.thurs_1_reg;
                BiWeekDetails.fri_1_reg = model.employeepayroll.payBiweek.fri_1_reg;
                BiWeekDetails.sat_2_reg = model.employeepayroll.payBiweek.sat_2_reg;
                BiWeekDetails.mon_2_reg = model.employeepayroll.payBiweek.mon_2_reg;
                BiWeekDetails.tues_2_reg = model.employeepayroll.payBiweek.tues_2_reg;
                BiWeekDetails.wed_2_reg = model.employeepayroll.payBiweek.wed_2_reg;
                BiWeekDetails.thurs_2_reg = model.employeepayroll.payBiweek.thurs_2_reg;
                BiWeekDetails.fri_2_reg = model.employeepayroll.payBiweek.fri_2_reg;
                BiWeekDetails.sat_1_opt = model.employeepayroll.payBiweek.sat_1_opt;
                BiWeekDetails.mon_1_opt = model.employeepayroll.payBiweek.mon_1_opt;
                BiWeekDetails.tues_1_opt = model.employeepayroll.payBiweek.tues_1_opt;
                BiWeekDetails.wed_1_opt = model.employeepayroll.payBiweek.wed_1_opt;
                BiWeekDetails.thurs_1_opt = model.employeepayroll.payBiweek.thurs_1_opt;
                BiWeekDetails.fri_1_opt = model.employeepayroll.payBiweek.fri_1_opt;
                BiWeekDetails.sat_2_opt = model.employeepayroll.payBiweek.sat_2_opt;
                BiWeekDetails.mon_2_opt = model.employeepayroll.payBiweek.mon_2_opt;
                BiWeekDetails.tues_2_opt = model.employeepayroll.payBiweek.tues_2_opt;
                BiWeekDetails.wed_2_opt = model.employeepayroll.payBiweek.wed_2_opt;
                BiWeekDetails.thurs_2_opt = model.employeepayroll.payBiweek.thurs_2_opt;
                BiWeekDetails.fri_2_opt = model.employeepayroll.payBiweek.fri_2_opt;

                BiWeekDetails.sat_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sat_1_sel_id);
                BiWeekDetails.mon_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.mon_1_sel_id);
                BiWeekDetails.tues_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.tues_1_sel_id);
                BiWeekDetails.wed_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.wed_1_sel_id);
                BiWeekDetails.thurs_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.thurs_1_sel_id);
                BiWeekDetails.fri_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.fri_1_sel_id);
                BiWeekDetails.sat_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sat_2_sel_id);
                BiWeekDetails.mon_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.mon_2_sel_id);
                BiWeekDetails.tues_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.tues_2_sel_id);
                BiWeekDetails.wed_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.wed_2_sel_id);
                BiWeekDetails.thurs_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.thurs_2_sel_id); ;
                BiWeekDetails.fri_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.fri_2_sel_id);
                BiWeekDetails.sat_1_sum = model.employeepayroll.payBiweek.sat_1_sum;
                BiWeekDetails.mon__1_sum = model.employeepayroll.payBiweek.mon__1_sum;
                BiWeekDetails.tues_1_sum = model.employeepayroll.payBiweek.tues_1_sum;
                BiWeekDetails.wed_1_sum = model.employeepayroll.payBiweek.wed_1_sum;
                BiWeekDetails.thurs_1_sum = model.employeepayroll.payBiweek.thurs_1_sum;
                BiWeekDetails.fri_1_sum = model.employeepayroll.payBiweek.fri_1_sum;
                BiWeekDetails.sat_2_sum = model.employeepayroll.payBiweek.sat_2_sum;
                BiWeekDetails.mon_2_sum = model.employeepayroll.payBiweek.mon_2_sum;
                BiWeekDetails.tues_2_sum = model.employeepayroll.payBiweek.tues_2_sum;
                BiWeekDetails.wed_2_Sum = model.employeepayroll.payBiweek.wed_2_Sum;
                BiWeekDetails.thurs_2_Sum = model.employeepayroll.payBiweek.thurs_2_Sum;
                BiWeekDetails.fri_2_sum = model.employeepayroll.payBiweek.fri_2_sum;
                BiWeekDetails.bi_week_chkin_avg = model.employeepayroll.payBiweek.bi_week_chkin_avg;
                BiWeekDetails.bi_week_chkout_avg = model.employeepayroll.payBiweek.bi_week_chkout_avg;
                BiWeekDetails.last_updated_by = model.employeepayroll.payBiweek.last_updated_by;
                BiWeekDetails.last_update_date = model.employeepayroll.payBiweek.last_update_date;
                BiWeekDetails.recordflag = model.employeepayroll.payBiweek.recordflag;
                BiWeekDetails.comments = model.employeepayroll.payBiweek.comments;
                BiWeekDetails.timeClock_sat1 = model.employeepayroll.payBiweek.timeClock_sat1;
                BiWeekDetails.timeClock_mon1 = model.employeepayroll.payBiweek.timeClock_mon1;
                BiWeekDetails.timeClock_tues1 = model.employeepayroll.payBiweek.timeClock_tues1;
                BiWeekDetails.timeClock_wed1 = model.employeepayroll.payBiweek.timeClock_wed1;
                BiWeekDetails.timeClock_thurs1 = model.employeepayroll.payBiweek.timeClock_thurs1;
                BiWeekDetails.timeClock_fri1 = model.employeepayroll.payBiweek.timeClock_fri1;
                BiWeekDetails.timeClock_sat2 = model.employeepayroll.payBiweek.timeClock_sat2;
                BiWeekDetails.timeClock_mon2 = model.employeepayroll.payBiweek.timeClock_mon2;
                BiWeekDetails.timeClock_tues2 = model.employeepayroll.payBiweek.timeClock_tues2;
                BiWeekDetails.timeClock_wed2 = model.employeepayroll.payBiweek.timeClock_wed2;
                BiWeekDetails.timeClock_thurs2 = model.employeepayroll.payBiweek.timeClock_thurs2;
                BiWeekDetails.timeClock_fri2 = model.employeepayroll.payBiweek.timeClock_fri2;
                BiWeekDetails.sun_1_reg = model.employeepayroll.payBiweek.sun_1_reg;
                BiWeekDetails.sun_2_reg = model.employeepayroll.payBiweek.sun_2_reg;
                BiWeekDetails.sun_1_opt = model.employeepayroll.payBiweek.sun_1_opt;
                BiWeekDetails.sun_2_opt = model.employeepayroll.payBiweek.sun_2_opt;
                BiWeekDetails.sun_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sun_1_sel_id); ;
                BiWeekDetails.sun_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sun_2_sel_id); ;
                BiWeekDetails.sun_1_sum = model.employeepayroll.payBiweek.sun_1_sum;
                BiWeekDetails.sun_2_sum = model.employeepayroll.payBiweek.sun_2_sum;
                BiWeekDetails.timeClock_sun1 = model.employeepayroll.payBiweek.timeClock_sun1;
                BiWeekDetails.timeClock_sun2 = model.employeepayroll.payBiweek.timeClock_sun2;

                BiWeekDetails.recordflag = 2;

                SummaryDetails.regular = model.employeepayroll.paySummary.regular;
                SummaryDetails.ot = model.employeepayroll.paySummary.ot;
                SummaryDetails.sick = model.employeepayroll.paySummary.sick;
                SummaryDetails.vac = model.employeepayroll.paySummary.vac;
                SummaryDetails.brev = model.employeepayroll.paySummary.brev;
                SummaryDetails.record_flag = 2;
                SummaryDetails.stat = model.employeepayroll.paySummary.stat;
                SummaryDetails.split = model.employeepayroll.paySummary.split;
                SummaryDetails.gen_ins = model.employeepayroll.paySummary.gen_ins;
                SummaryDetails.emp_ins = model.employeepayroll.paySummary.emp_ins;
                SummaryDetails.workers_comp = model.employeepayroll.paySummary.workers_comp;
            }
            db.SaveChanges();
            ViewData["submit_payroll_confirmation"] = "yes";
            return RedirectToAction("Payroll", new { locId = model.MatchedLocID, employeeId = empID, payrollID = payrollId });
        }

        public string SelectedCategoryValue(int value)
        {
            string returnvalue = null;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var details = db.tbl_payroll_category_selection.Where(x => x.category_id == value).FirstOrDefault();
            returnvalue = details.category;
            return returnvalue;
        }

        [HttpGet]
        public ActionResult Attendance(string locId,string employeeId)
        {
            Debug.WriteLine("On Attendence Load");
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var locationDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string location = "101";
            if(locationDetails!=null)
            {
                location = locationDetails.loc_ID;
            }
            if(locId is null)
            {

            }
            else
            {
                location = locId;
            }
            var employeeList =
                           (from employee in db.tbl_employee
                            join attendance in db.tbl_employee_attendance on employee.employee_id equals attendance.employee_id                           
                            where employee.loc_ID == location 
                            orderby employee.full_name
                            select new
                            {
                                employee.employee_id,
                                employee.full_name,
                                attendance.at_work,
                                employee.status
                            }).ToList();
           
            List<EmployeeAttendanceListModel> emplyAttList = new List<EmployeeAttendanceListModel>();
            foreach(var item in employeeList)
            {
                EmployeeAttendanceListModel obj = new EmployeeAttendanceListModel(); // ViewModel
                //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                if(item.status==1)
                {
                    obj.employeeId = item.employee_id.ToString();
                    obj.fullName = item.full_name;
                    obj.atWork = item.at_work.ToString();
                    emplyAttList.Add(obj);
                }              

            }
            AttendanceModel model = new AttendanceModel();
            model.employeeList = emplyAttList;
            model.MatchedLocs = populateLocations();
            model.MatchedLocID = location;

            if(employeeId is null)
            {

            }
            else
            {
                if (employeeId != null || employeeId != "")
                {
                    model.SelectedEmployeeId = employeeId;
                    Debug.WriteLine("In ShowEditCustInfo");
                    int employeeIdNum = Convert.ToInt32(employeeId);
                    var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == employeeIdNum).FirstOrDefault();
                    string empId1 = "";
                    string locId1 = "";
                    string position = "";
                    string fullName = "";
                    byte[] profileImg = null;
                    if (EmployeeDetails != null)
                    {
                        empId1 = EmployeeDetails.employee_id.ToString();
                        locId1 = EmployeeDetails.loc_ID;
                        fullName = EmployeeDetails.full_name;
                        position = EmployeeDetails.cta_position;
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

                    Debug.WriteLine("Image_base64:" + base64ProfilePic);
                    ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
                    ViewData["EmployeeName"] = fullName;
                    ViewData["EmployeeId"] = empId1;
                    ViewData["Position"] = position;
                    loadattendanceInfo(empId1);
                    getDataEmployee(empId1);

                }
            }
           
            return View(model);
        }

     

        [HttpPost] 
        public ActionResult ChangeLocAttendance(AttendanceModel model)
        {
            return RedirectToAction("Attendance", new { locId = model.MatchedLocID, employeeId =""});
        }
        
        [HttpGet]
        public ActionResult NewHire()
        {
            return View();
        }
        [HttpGet]
        public ActionResult VacationSchedule()
        {
            return View();
        }

        private void loadattendanceInfo(string employeeid)
        {

            string todayDate = DateTime.Today.ToString("yyyy-MM-dd");
            string currentPayRollId = "";
            string startDate = "";
            string endDate = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select * FRom  tbl_employee_payroll_dates where start_date <=@SDate and end_date >= @SDate ");
                    string sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@SDate", todayDate);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                currentPayRollId = dr["payroll_id"].ToString();
                                startDate = dr["start_date"].ToString();
                                endDate = dr["end_date"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }

            string retrunVal = loadTimeCalculations(employeeid, startDate);
            string[] valuesToBeAssigned = retrunVal.Split(';');
            ViewData["avgInHoursCurrent"]= valuesToBeAssigned[0];
            ViewData["avgOutHoursCurrent"] = valuesToBeAssigned[1];
            ViewData["BiweekHours"]= valuesToBeAssigned[2];
            //loadAllTimeCalculations(employeeid, currentPayRollId);

            string status = checkIfAverageExists(employeeid, currentPayRollId);

            if (status == "NoAverage")
            {
                int last_payroll = Int32.Parse(currentPayRollId) - 1;
                string Previousstatus = checkIfAverageExists(employeeid, last_payroll.ToString());
                float totalAverageInHrs = 0;
                float totalAverageOutHrs = 0;

                System.Collections.ArrayList arrayLIST = new System.Collections.ArrayList();
                arrayLIST.Clear();
                try
                {
                    
                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("select * From tbl_employee_payroll_dates where payroll_Id < @payid");
                        // Debug.WriteLine("locID :"+ locID);
                        string sql = sb.ToString();

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {

                            command.Parameters.AddWithValue("@payid", currentPayRollId);
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    arrayLIST.Add((dr["start_date"].ToString()));
                                }

                            }
                            command.Parameters.Clear();
                        }
                        connection.Close();
                    }
                }
                catch (SqlException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
                if (Previousstatus == "NoAverage")
                {
                    int InCount = 0;

                    int OutCount = 0;
                    foreach (object o in arrayLIST)
                    {
                        Debug.WriteLine("Checking previous ayrolls:" + employeeid + "  " + o.ToString());
                        string retrunValues = loadTimeCalculations(employeeid, o.ToString());

                        string[] returnVals = retrunValues.Split(';');
                        string inVal = returnVals[0];
                        string outVal = returnVals[1];
                        if (inVal != "No Data")
                        {
                            int comp1 = (int)TimeSpan.Parse(inVal).TotalMinutes;
                            totalAverageInHrs = totalAverageInHrs + comp1;
                            InCount = InCount + 1;
                        }
                        if (outVal != "No Data")
                        {
                            int comp1 = (int)TimeSpan.Parse(outVal).TotalMinutes;
                            totalAverageOutHrs = totalAverageOutHrs + comp1;
                            OutCount = OutCount + 1;
                        }
                    }
                    totalAverageInHrs = totalAverageInHrs / InCount;
                    totalAverageOutHrs = totalAverageOutHrs / OutCount;


                    //Here insert 
                    string inHrs = ((int)totalAverageInHrs / 60).ToString();
                    string inMins = ((int)totalAverageInHrs % 60).ToString();
                    string outHrs = ((int)totalAverageOutHrs / 60).ToString();
                    string outMins = ((int)totalAverageOutHrs % 60).ToString();

                    if (inHrs.Length < 2)
                    {

                        inHrs = "0" + inHrs;
                    }
                    if (inMins.Length < 2)
                    {
                        inMins = "0" + inMins;
                    }
                    if (outHrs.Length < 2)
                    {
                        outHrs = "0" + outHrs;
                    }
                    if (outMins.Length < 2)
                    {
                        outMins = "0" + outMins;
                    }

                    if (Convert.ToInt32(inHrs) < 0)
                    {
                        ViewData["avgInHoursOverall"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgInHoursOverall"] = inHrs + ":" + inMins;
                    }

                    if (Convert.ToInt32(outHrs) < 0)
                    {
                        ViewData["avgOutHoursOverall.Text"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgOutHoursOverall"] = outHrs + ":" + outMins;
                    }

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("insert into tbl_attendance_management values(@employee_id,@last_payroll,@Average_In, @Average_out)");
                            string sql = sb.ToString();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@employee_id", employeeid);
                                command.Parameters.AddWithValue("@last_payroll", last_payroll);
                                command.Parameters.AddWithValue("@Average_In", inHrs + ":" + inMins);
                                command.Parameters.AddWithValue("@Average_out", outHrs + ":" + outMins);
                                command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }

                            connection.Close();
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
                    }
                }
                else
                {
                    string date = "";
                    try
                    {
                        
                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("select * From tbl_employee_payroll_dates where payroll_Id = @payid");
                            // Debug.WriteLine("locID :"+ locID);
                            string sql = sb.ToString();

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@payid", last_payroll);
                                using (SqlDataReader dr = command.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        date = ((dr["start_date"].ToString()));
                                    }

                                }
                                command.Parameters.Clear();
                            }
                            connection.Close();
                        }
                    }
                    catch (SqlException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }



                    string[] previous = Previousstatus.Split(';');
                    string[] PrevIn = previous[0].Split(':');
                    string[] PrevOut = previous[1].Split(':');
                    string retrunValues = loadTimeCalculations(employeeid, date);
                    string[] returnVals = retrunValues.Split(';');
                    string inVal = returnVals[0];
                    string outVal = returnVals[1];
                    if (inVal != "No Data")
                    {
                        int comp1 = (int)TimeSpan.Parse(inVal).TotalMinutes;
                        totalAverageInHrs = totalAverageInHrs + comp1;

                    }
                    if (outVal != "No Data")
                    {
                        int comp1 = (int)TimeSpan.Parse(outVal).TotalMinutes;
                        totalAverageOutHrs = totalAverageOutHrs + comp1;

                    }
                    totalAverageInHrs = (totalAverageInHrs + (int)TimeSpan.Parse(PrevIn[0] + ":" + PrevIn[1]).TotalMinutes) / (arrayLIST.Capacity + 1);
                    totalAverageOutHrs = (totalAverageOutHrs + (int)TimeSpan.Parse(PrevOut[0] + ":" + PrevOut[1]).TotalMinutes) / (arrayLIST.Capacity + 1);

                    //here update

                    string inHrs = ((int)totalAverageInHrs / 60).ToString();
                    string inMins = ((int)totalAverageInHrs % 60).ToString();
                    string outHrs = ((int)totalAverageOutHrs / 60).ToString();
                    string outMins = ((int)totalAverageOutHrs % 60).ToString();

                    if (inHrs.Length < 2)
                    {

                        inHrs = "0" + inHrs;
                    }
                    if (inMins.Length < 2)
                    {
                        inMins = "0" + inMins;
                    }
                    if (outHrs.Length < 2)
                    {
                        outHrs = "0" + outHrs;
                    }
                    if (outMins.Length < 2)
                    {
                        outMins = "0" + outMins;
                    }

                    if (Convert.ToInt32(inHrs) < 0)
                    {
                        ViewData["avgInHoursOverall"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgInHoursOverall"] = inHrs + ":" + inMins;
                    }

                    if (Convert.ToInt32(outHrs) < 0)
                    {
                        ViewData["avgOutHoursOverall"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgOutHoursOverall"]= outHrs + ":" + outMins;
                    }

                    try
                    {


                       
                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("update tbl_attendance_management set last_payroll=@last_payroll,Average_In=@Average_In,Average_out=@Average_out Where employee_id=@employee_id and last_payroll=@previousPayroll");
                            string sql = sb.ToString();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@employee_id", employeeid);
                                command.Parameters.AddWithValue("@previousPayroll", last_payroll - 1);
                                command.Parameters.AddWithValue("@last_payroll", last_payroll);
                                command.Parameters.AddWithValue("@Average_In", inHrs + ":" + inMins);
                                command.Parameters.AddWithValue("@Average_out", outHrs + ":" + outMins);
                                command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }

                            connection.Close();
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
                    }
                }

            }
            else
            {
                string[] previous = status.Split(';');
                string[] PrevIn = previous[0].Split(':');
                string[] PrevOut = previous[1].Split(':');
                ViewData["avgInHoursOverall"] = PrevIn[0] + ":" + PrevIn[1];
                ViewData["avgOutHoursOverall"] = PrevOut[0] + ":" + PrevOut[1];
            }

        }
        private string loadTimeCalculations(string employeeid, string startDate)
        {
            System.Collections.ArrayList instantArrayList = new System.Collections.ArrayList();
            instantArrayList.Clear();
            string[,] BiWeeklyHours = new string[14, 2];
            string returnThis = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb.Append("Select convert(varchar(5),time_stamp, 108) as hours from tbl_attendance_log where  CONVERT(DATE, time_stamp) = @DailyWise and employee_id = @empId ");

                    string sql = sb.ToString();

                    for (int day = 0; day <= 13; day++)
                    {
                        //Debug.WriteLine("Entering looop:" + day);

                        DateTime ConvrunDate = Convert.ToDateTime(startDate).AddDays(day);
                        //Debug.WriteLine("   Start date>>>>>>>>>>>>>>>>>>>>>>>>>>>>>"+ ConvrunDate);
                        string runDate = Convert.ToDateTime(ConvrunDate).ToString("yyyy-MM-dd");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {

                            command.Parameters.AddWithValue("@DailyWise", runDate);
                            command.Parameters.AddWithValue("@empId", employeeid);
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                    instantArrayList.Add((dr["hours"].ToString()));
                            }
                            command.Parameters.Clear();
                        }
                        //foreach(object o in instantArrayList)
                        //{

                        //}

                        //Debug.WriteLine("instantArrayList.Count" + instantArrayList.Count);
                        if (instantArrayList.Count == 0)
                        {
                            BiWeeklyHours[day, 0] = "0:00";
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else if (instantArrayList.Count == 1)
                        {
                            //Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else
                        {
                            //Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            var comp1 = (int)TimeSpan.Parse(instantArrayList[0].ToString()).TotalMinutes;
                            var comp2 = (int)TimeSpan.Parse(instantArrayList[instantArrayList.Count - 1].ToString()).TotalMinutes;
                            if (comp1 > comp2)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[instantArrayList.Count - 1].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[0].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "17:00";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "17:00";
                                }

                            }
                            else if (comp2 > comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "17:00";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "17:00";
                                }
                            }
                            else if (comp2 == comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                            }

                        }

                        instantArrayList.Clear();
                    }


                    connection.Close();
                }
            }
            catch (Exception e2)
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet :" + e2);
            }
            //

            //Debug.WriteLine(" instantArrayList's capacity :" + instantArrayList.Capacity);

            string[,] dailyHours = new string[14, 2];
            float checkinTotal = 0;
            int checkInCount = 0;
            float checkOutTotal = 0;
            int checkOutCount = 0;
            for (int i = 0; i <= 13; i++)
            {
                //Debug.WriteLine("............................................");
                //Debug.WriteLine(" i,0 :" + i + ":" + BiWeeklyHours[i, 0]);
                //Debug.WriteLine("i,1" + BiWeeklyHours[i, 1]);

                var firstSignIn = BiWeeklyHours[i, 0];
                var LastSignIn = BiWeeklyHours[i, 1];

                if (firstSignIn == null || firstSignIn == "")
                {
                    firstSignIn = "00:00";
                }
                if (LastSignIn == null || LastSignIn == "")
                {
                    LastSignIn = "00:00";
                }
                var leftMinutes = (int)TimeSpan.Parse(firstSignIn).TotalMinutes;
                var rightMinutes = (int)TimeSpan.Parse(LastSignIn).TotalMinutes;
                //Debug.WriteLine("Overall Minutes left" + leftMinutes);
                //Debug.WriteLine("Overall minutes right" + rightMinutes);

                if (leftMinutes != 0 && rightMinutes != 0)
                {
                    //Debug.WriteLine("Overall Hours to database :" + (leftMinutes - rightMinutes) / 60);
                    //Debug.WriteLine("Overall Minutes to database :" + (leftMinutes - rightMinutes) % 60);
                    checkinTotal = checkinTotal + leftMinutes;
                    checkOutTotal = checkOutTotal + rightMinutes;
                    dailyHours[i, 0] = (Math.Abs((leftMinutes - rightMinutes)) / 60).ToString();
                    dailyHours[i, 1] = (Math.Abs((leftMinutes - rightMinutes)) % 60).ToString();
                    checkInCount = checkInCount + 1;
                    checkOutCount = checkOutCount + 1;
                }
                else
                {
                    //dailyHours[i, 0] ="0";
                    //dailyHours[i, 1] ="00";
                    dailyHours[i, 0] = "";
                    dailyHours[i, 1] = "";
                }

                if (leftMinutes != 0 && rightMinutes == 0)
                {

                    checkinTotal = checkinTotal + leftMinutes;
                    checkInCount = checkInCount + 1;
                }



            }
            //Calculating average hours
            float checkInAvgHours = Math.Abs((checkinTotal / checkInCount)) / 60;
            float checkInAvgMins = Math.Abs((checkinTotal / checkInCount)) % 60;

            float checkOutAvgHours = Math.Abs((checkOutTotal / checkOutCount)) / 60;
            float checkOutAvgMins = Math.Abs((checkOutTotal / checkOutCount)) % 60;
            if (checkInCount > 0)
            {
                string sInHours = ((int)checkInAvgHours).ToString();
                string sOutMins = ((int)checkInAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }

                //this.avgInHoursCurrent.Text = sInHours + ":" + sOutMins;

                returnThis = sInHours + ":" + sOutMins;


            }
            else
            {
                //this.avgInHoursCurrent.Text = "No Data";
                returnThis = "No Data";


            }
            if (checkOutCount > 0)
            {
                string sInHours = ((int)checkOutAvgHours).ToString();
                string sOutMins = ((int)checkOutAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }

                //this.avgOutHoursCurrent.Text = sInHours + ":" + sOutMins;
                returnThis = returnThis + ";" + sInHours + ":" + sOutMins;

            }
            else
            {
                //this.avgOutHoursCurrent.Text = "";
                returnThis = returnThis + ";" + "No Data";

            }

            string[] totalString = new string[14];
            double[] BiWeekString = new double[14];
            string[] BiWeekDates= new string[14];
            float totHrs = 0;
            float totMins = 0;
            for (int i = 0; i <= 13; i++)
            {
                BiWeekDates[i] = Convert.ToDateTime(Convert.ToDateTime(startDate).AddDays(i)).ToString("dd-MM");
                if (dailyHours[i, 0] == "" && dailyHours[i, 1] == "")
                {
                    totalString[i] = "";
                    BiWeekString[i] = 0;
                }
                else
                {
                    if (dailyHours[i, 0].Length == 1)
                    {
                        dailyHours[i, 0] = "0" + dailyHours[i, 0][0];
                    }
                    if (dailyHours[i, 1].Length == 1)
                    {
                        dailyHours[i, 1] = "0" + dailyHours[i, 1][0];
                    }
                    //Debug.WriteLine("666:" + dailyHours[i, 0]);
                    totHrs = (float)Convert.ToDouble(dailyHours[i, 0]) + totHrs;
                    //Debug.WriteLine("666:" + totHrs.ToString());
                    totMins = (float)Convert.ToDouble(dailyHours[i, 1]) + totMins;
                    totalString[i] = dailyHours[i, 0] + ":" + dailyHours[i, 1];
                    double mintValCalc = Convert.ToInt32(dailyHours[i, 1]) / 60;
                    BiWeekString[i] = Convert.ToDouble(dailyHours[i, 0]) + mintValCalc;




                }
            }
            for (int l= 0;l<=13;l++)
            {
               
                Debug.WriteLine(BiWeekString[l]);
            }
            
            ViewData["BarChartValues"] = BiWeekString;
            ViewData["BarChartDates"] = BiWeekDates;
            float mins = 0;
            if (totMins > 60)
            {
                mins = totMins % 60;
                totHrs = totHrs + (int)(totMins / 60);
            }

            //Debug.WriteLine("Total String:---------------------"+totHrs+"     " + mins);
            //for (int i=0;i<= totalString.Length-1;i++)
            //{
            //    Debug.WriteLine(totalString[i]);
            //}

            //this.BiweekHours.Text = totHrs.ToString();
           
            returnThis = returnThis + ";" + totHrs.ToString();
            Debug.WriteLine("returnThis:" + returnThis);

            return returnThis;

        }
        private string checkIfAverageExists(string empId, string currentPayRollId)
        {
            int last_payroll = Int32.Parse(currentPayRollId) - 1;
            string employeeId = "";
            string returnString = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select * from tbl_attendance_management where employee_id = @employee_id and Last_payroll = @lastPayroll");
                    string sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@employee_id", empId);
                        command.Parameters.AddWithValue("@lastPayroll", last_payroll);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                employeeId = dr["employee_id"].ToString();
                                returnString = dr["Average_In"].ToString() + ";" + dr["Average_out"].ToString();
                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }
            if (employeeId == null || employeeId == "")
            {
                returnString = "NoAverage";
            }

            return returnString;
        }

        public double getDataEmployee(string employeeId)
        {
            Debug.WriteLine("In getDataEmployee");
            string clockCount = "";
            string manualCount = "";
            string clockInCount = "";
            string manualInCount = "";
            string clockOutCount = "";
            string manualOutCount = "";
            string autoOutCount = "";
            string atWork = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sbX = new StringBuilder();
                    StringBuilder sbY = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    StringBuilder sb3 = new StringBuilder();
                    StringBuilder sb4 = new StringBuilder();
                    StringBuilder sb5 = new StringBuilder();
                    StringBuilder sb6 = new StringBuilder();
                    sbX.Append("select count(employee_id) as empCount FRom tbl_attendance_log  where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id not in ('manualIN', 'manualOUT')");
                    sbY.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id in ('manualIN', 'manualOUT')");
                    sb1.Append("select count(employee_id) as empCount FRom tbl_attendance_log  where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'manualIN'");
                    sb2.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'clockIN'");
                    sb3.Append("select count(employee_id) as empCount FRom tbl_attendance_log  where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'manualOUT'");
                    sb4.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'clockOUT'");
                    sb5.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'autoOUT'");
                    sb6.Append("select at_work FRom tbl_employee_attendance where employee_id=@employeeId");

                    string sqlX = sbX.ToString();
                    string sqlY = sbY.ToString();
                    string sql1 = sb1.ToString();
                    string sql2 = sb2.ToString();
                    string sql3 = sb3.ToString();
                    string sql4 = sb4.ToString();
                    string sql5 = sb5.ToString();
                    string sql6 = sb6.ToString();
                    using (SqlCommand command = new SqlCommand(sqlX, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                clockCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sqlY, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                manualCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql1, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                manualInCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                clockInCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql3, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                manualOutCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql4, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                clockOutCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql5, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                autoOutCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    using (SqlCommand command = new SqlCommand(sql6, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                atWork = dr["at_work"].ToString();


                            }
                        }
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            int a = Int32.Parse(clockCount);
            int b = Int32.Parse(manualCount);
            double percetage = 0.00;
            int sum = a + b;
            if (sum == 0)
            {

            }
            else
            {
                percetage = Math.Round(((double)a / (double)(sum)) * 100);
            }
            // Debug.WriteLine("Percentage :" + percetage);

            ////Calculating for individual employee
            int cIn = Int32.Parse(clockInCount);
            int mIn = Int32.Parse(manualInCount);
            double clockInPer = 0.00;

            if (cIn + mIn == 0)
            {

            }
            else
            {
                clockInPer = Math.Round(((double)cIn / (double)(cIn + mIn)) * 100);
            }

            ViewData["StackedClockInFob"] = clockInPer;
            ViewData["StackedClockInManual"] = 100-clockInPer;
            Debug.WriteLine("clockInPer:" + clockInPer);
            int cOut = Int32.Parse(clockOutCount);
            int mOut = Int32.Parse(manualOutCount);
            int aOut = Int32.Parse(autoOutCount);
            double clockOutPer = 0.00;
            double manualOutPer = 0.00;
            double autoOutPer = 0.00;

            if (cOut + mOut + aOut == 0)
            {

            }
            else
            {
                clockOutPer = Math.Round(((double)cOut / (double)(cOut + mOut + aOut)) * 100);
                manualOutPer = Math.Round(((double)mOut / (double)(cOut + mOut + aOut)) * 100);
                autoOutPer = Math.Round(((double)aOut / (double)(cOut + mOut + aOut)) * 100);
            }

            ViewData["StackedClockOutFob"] = clockOutPer;
            ViewData["StackedClockOutManual"] = manualOutPer;
            ViewData["StackedClockOutAuto"] = autoOutPer;

            Debug.WriteLine("clockOutPer:" + clockOutPer);
            Debug.WriteLine("manualOutPer:" + manualOutPer);
            Debug.WriteLine("autoOutPer:" + autoOutPer);


            return percetage;
        }


        private void LoadPayroll(string  management_emp_id, string sdate,string edate, string currPayId)
        {
            double totalDays = (DateTime.Today - Convert.ToDateTime(sdate)).TotalDays;
            double remainingDays= (Convert.ToDateTime(edate) -DateTime.Today ).TotalDays;
            string startDate = sdate;
            string endDate= edate;
            string PrevPayRollId;
            string currentPayRollId = currPayId;
            string payrollid = currPayId;
            ViewData["RemainingDays"] = remainingDays;
            Debug.WriteLine("*************TotalDays*****************************:"+ totalDays);
            Debug.WriteLine("*************remainingDays*****************************:" + remainingDays);
            if (totalDays <= 3)
            {
                //Debug.WriteLine("*************Changed dates*****************************:" + DateTime.Today.AddDays(-14- totalDays).ToString());
                startDate = DateTime.Today.AddDays(-14 - totalDays).ToString();
                endDate = DateTime.Today.AddDays(-totalDays).ToString();
                currentPayRollId = (Convert.ToInt32(currentPayRollId) - 1).ToString();
                PrevPayRollId = (Convert.ToInt32(currentPayRollId) - 1).ToString();

            }
            ViewData["payId"] = payrollid;
            ViewData["payFrom"] = Convert.ToDateTime(startDate).ToString("MMMM dd, yyyy");
            ViewData["payTo"] = Convert.ToDateTime(endDate).ToString("MMMM dd, yyyy");
            //End of getting paydates from database
            //Accessing Clock In and ClockOut timings for an employee
            Debug.WriteLine("employee_id:" + management_emp_id);
            System.Collections.ArrayList instantArrayList = new System.Collections.ArrayList();
            instantArrayList.Clear();
            string[,] BiWeeklyHours = new string[14, 2];
            Debug.WriteLine("todayDate :" + Convert.ToDateTime(startDate).ToString("yyyy-MM-dd"));
            //This code for date columns on the top of the table
            DateTime convStartDate = Convert.ToDateTime(Convert.ToDateTime(startDate).ToString("yyyy-MM-dd"));

            ViewData["start_date"] = Convert.ToDateTime(convStartDate).ToString("MMM dd,yyyy");
            ViewData["sat_1_date"]= Convert.ToDateTime(convStartDate).ToString("dd-MMM");
            ViewData["sun_1_date"] = Convert.ToDateTime(convStartDate.AddDays(1)).ToString("dd-MMM");
            ViewData["mun_1_date"] = Convert.ToDateTime(convStartDate.AddDays(2)).ToString("dd-MMM");
            ViewData["tues_1_date"] = Convert.ToDateTime(convStartDate.AddDays(3)).ToString("dd-MMM");
            ViewData["wed_1_date"] = Convert.ToDateTime(convStartDate.AddDays(4)).ToString("dd-MMM");
            ViewData["thur_1_date"] = Convert.ToDateTime(convStartDate.AddDays(5)).ToString("dd-MMM");
            ViewData["fri_1_date"] = Convert.ToDateTime(convStartDate.AddDays(6)).ToString("dd-MMM");
            ViewData["sat_2_date"] = Convert.ToDateTime(convStartDate.AddDays(7)).ToString("dd-MMM");
            ViewData["sun_2_date"] = Convert.ToDateTime(convStartDate.AddDays(8)).ToString("dd-MMM");
            ViewData["mon_2_date"] = Convert.ToDateTime(convStartDate.AddDays(9)).ToString("dd-MMM");
            ViewData["tues_2_date"] = Convert.ToDateTime(convStartDate.AddDays(10)).ToString("dd-MMM");
            ViewData["wed_2_date"] = Convert.ToDateTime(convStartDate.AddDays(11)).ToString("dd-MMM");
            ViewData["thurs_2_date"] = Convert.ToDateTime(convStartDate.AddDays(12)).ToString("dd-MMM");
            ViewData["fri_2_date"]= Convert.ToDateTime(convStartDate.AddDays(13)).ToString("dd-MMM");
            ViewData["end_date"] = Convert.ToDateTime(convStartDate.AddDays(13)).ToString("MMM dd,yyyy");
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {               
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb.Append("Select convert(varchar(5),time_stamp, 108) as hours from tbl_attendance_log where  CONVERT(DATE, time_stamp) = @DailyWise and employee_id = @empId ");

                    string sql = sb.ToString();

                    for (int day = 0; day <= 13; day++)
                    {
                        Debug.WriteLine("Entering looop:" + day);

                        DateTime ConvrunDate = convStartDate.AddDays(day);
                        string runDate = Convert.ToDateTime(ConvrunDate).ToString("yyyy-MM-dd");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@DailyWise", runDate);
                            command.Parameters.AddWithValue("@empId", management_emp_id);
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                    instantArrayList.Add((dr["hours"].ToString()));
                            }
                            command.Parameters.Clear();
                        }
                        Debug.WriteLine("instantArrayList.Count" + instantArrayList.Count);
                        if (instantArrayList.Count == 0)
                        {
                            BiWeeklyHours[day, 0] = "0:00";
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else if (instantArrayList.Count == 1)
                        {
                            Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else
                        {
                            Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            var comp1 = (int)TimeSpan.Parse(instantArrayList[0].ToString()).TotalMinutes;
                            var comp2 = (int)TimeSpan.Parse(instantArrayList[instantArrayList.Count - 1].ToString()).TotalMinutes;
                            if (comp1 > comp2)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[instantArrayList.Count - 1].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[0].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "16:30";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "16:30";
                                }

                            }
                            else if (comp2 > comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "16:30";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "16:30";
                                }
                            }
                            else if (comp2 == comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                            }

                        }

                        instantArrayList.Clear();
                    }


                    connection.Close();
                }
            }
            catch (Exception e2)
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet :" + e2);
            }
            //

            Debug.WriteLine(" instantArrayList's capacity :" + instantArrayList.Capacity);

            string[,] dailyHours = new string[14, 2];
            float checkinTotal = 0;
            int checkInCount = 0;
            float checkOutTotal = 0;
            int checkOutCount = 0;
            for (int i = 0; i <= 13; i++)
            {
                Debug.WriteLine("............................................");
                Debug.WriteLine(" i,0 :" + i + ":" + BiWeeklyHours[i, 0]);
                Debug.WriteLine("i,1" + BiWeeklyHours[i, 1]);

                var firstSignIn = BiWeeklyHours[i, 0];
                var LastSignIn = BiWeeklyHours[i, 1];


                var leftMinutes = (int)TimeSpan.Parse(firstSignIn).TotalMinutes;
                var rightMinutes = (int)TimeSpan.Parse(LastSignIn).TotalMinutes;
                Debug.WriteLine("Overall Minutes left" + leftMinutes);
                Debug.WriteLine("Overall minutes right" + rightMinutes);

                if (leftMinutes != 0 && rightMinutes != 0)
                {
                    Debug.WriteLine("Overall Hours to database :" + (leftMinutes - rightMinutes) / 60);
                    Debug.WriteLine("Overall Minutes to database :" + (leftMinutes - rightMinutes) % 60);
                    checkinTotal = checkinTotal + leftMinutes;
                    checkOutTotal = checkOutTotal + rightMinutes;
                    dailyHours[i, 0] = (Math.Abs((leftMinutes - rightMinutes)) / 60).ToString();
                    dailyHours[i, 1] = (Math.Abs((leftMinutes - rightMinutes)) % 60).ToString();
                    checkInCount = checkInCount + 1;
                    checkOutCount = checkOutCount + 1;
                }
                else
                {
                    //dailyHours[i, 0] ="0";
                    //dailyHours[i, 1] ="00";
                    dailyHours[i, 0] = "";
                    dailyHours[i, 1] = "";
                }

                if (leftMinutes != 0 && rightMinutes == 0)
                {

                    checkinTotal = checkinTotal + leftMinutes;
                    checkInCount = checkInCount + 1;
                }



            }
            Debug.WriteLine("checkinTotal :" + checkinTotal);
            Debug.WriteLine("checkOutTotal:" + checkOutTotal);

            //Calculating average hours
            float checkInAvgHours = Math.Abs((checkinTotal / checkInCount)) / 60;
            float checkInAvgMins = Math.Abs((checkinTotal / checkInCount)) % 60;

            float checkOutAvgHours = Math.Abs((checkOutTotal / checkOutCount)) / 60;
            float checkOutAvgMins = Math.Abs((checkOutTotal / checkOutCount)) % 60;
            string ChkInAvg = "";
            string ChkoutAvg = "";
            if (checkInCount > 0)
            {
                string sInHours = ((int)checkInAvgHours).ToString();
                string sOutMins = ((int)checkInAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }

                ChkInAvg= sInHours + ":" + sOutMins;
                ViewData["ChkInAvgDisp"] = "Clock-In : " + sInHours + ":" + sOutMins;
                //this.IN1.Text = "Check-In (" + sInHours + ":" + sOutMins + ")";
            }
            else
            {
                ChkInAvg = "";
                ViewData["ChkInAvgDisp"] = "Clock-In : ";
                //this.IN1.Text = "Check-In ( )";
            }
            if (checkOutCount > 0)
            {
                string sInHours = ((int)checkOutAvgHours).ToString();
                string sOutMins = ((int)checkOutAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }

                ChkoutAvg = sInHours + ":" + sOutMins;
               ViewData["ChkoutAvgDisp"] = "Clock-Out : " + sInHours + ":" + sOutMins;
                //this.OUT1.Text = "Check-out (" + sInHours + ":" + sOutMins + ")";
            }
            else
            {
               ChkoutAvg= "";
                ViewData["ChkoutAvgDisp"] = "Clock-Out : ";
                //this.OUT1.Text = "Check-out ( )";
            }

            string[] totalString = new string[14];
            for (int i = 0; i <= 13; i++)
            {
                if (dailyHours[i, 0] == "" && dailyHours[i, 1] == "")
                {
                    totalString[i] = "";
                }
                else
                {
                    if (dailyHours[i, 0].Length == 1)
                    {
                        dailyHours[i, 0] = "0" + dailyHours[i, 0][0];
                    }
                    if (dailyHours[i, 1].Length == 1)
                    {
                        dailyHours[i, 1] = "0" + dailyHours[i, 1][0];
                    }
                    Debug.WriteLine("666:" + dailyHours[i, 0]);
                    float totHrs = (float)Convert.ToDouble(dailyHours[i, 0]);
                    Debug.WriteLine("666:" + totHrs.ToString());
                    float totMins = (float)Convert.ToDouble(dailyHours[i, 1]);
                    totalString[i] = dailyHours[i, 0] + ":" + dailyHours[i, 1];
                }
            }

            var totAvgIn = "";
            var totAvgOut = "";
            if (ChkInAvg== "")
            {
                totAvgIn = "0";

            }
            else
            {
                totAvgIn = ChkInAvg;
            }
            if (ChkoutAvg == "")
            {
                totAvgOut = "0";
            }
            else
            {
                totAvgOut =ChkoutAvg;
            }

            var totleftMinutes = (int)TimeSpan.Parse(totAvgIn).TotalMinutes;
            var totrightMinutes = (int)TimeSpan.Parse(totAvgOut).TotalMinutes;
            string TotAvgHours = (Math.Abs((totleftMinutes - totrightMinutes)) / 60).ToString();
            string TotAvgMins = (Math.Abs((totleftMinutes - totrightMinutes)) % 60).ToString();
            Debug.WriteLine("totalHrsOnLoad aBG HOURS" + TotAvgHours + "dcsdcsd :" + TotAvgMins);
            if (TotAvgHours.Length == 1)
            {
                TotAvgHours = "0" + TotAvgHours;
            }
            if (TotAvgMins.Length == 1)
            {
                TotAvgMins = "0" + TotAvgMins;
            }
            
           ViewData["OverallAvgDisp"] = "Total Avg : " + TotAvgHours + ":" + TotAvgMins;


            ViewData["TOT1"]= totalString[0];
            ViewData["TOT2"] = totalString[1];
            ViewData["TOT3"] = totalString[2];
            ViewData["TOT4"] = totalString[3];
            ViewData["TOT5"] = totalString[4];
            ViewData["TOT6"] = totalString[5];
            ViewData["TOT7"] = totalString[6];
            ViewData["TOT8"] = totalString[7];
            ViewData["TOT9"] = totalString[8];
            ViewData["TOT10"] = totalString[9];
            ViewData["TOT11"] = totalString[10];
            ViewData["TOT12"] = totalString[11];
            ViewData["TOT13"] = totalString[12];
            ViewData["TOT14"] = totalString[13];

            //End of Table 1

            //Assigning Values to Table 3

            //this.IN0.Text ="0:00";
            //this.IN1.Text = "0:00";
            for (int i = 0; i <= 13; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (BiWeeklyHours[i, j] == "0:00")
                    {
                        BiWeeklyHours[i, j] = "";
                    }
                }
            }

            ViewData["IN1"]= BiWeeklyHours[0, 0];
            ViewData["IN2"] = BiWeeklyHours[1, 0];
            ViewData["IN3"] = BiWeeklyHours[2, 0];
            ViewData["IN4"] = BiWeeklyHours[3, 0];
            ViewData["IN5"] = BiWeeklyHours[4, 0];
            ViewData["IN6"] = BiWeeklyHours[5, 0];
            ViewData["IN7"] = BiWeeklyHours[6, 0];
            ViewData["IN8"] = BiWeeklyHours[7, 0];
            ViewData["IN9"] = BiWeeklyHours[8, 0];
            ViewData["IN10"] = BiWeeklyHours[9, 0];
            ViewData["IN11"] = BiWeeklyHours[10, 0];
            ViewData["IN12"] = BiWeeklyHours[11, 0];
            ViewData["IN13"] = BiWeeklyHours[12, 0];
            ViewData["IN14"] = BiWeeklyHours[13, 0];
            //this.OUT0.Text = "0:00";
            //this.OUT1.Text = "0:00";
            ViewData["OUT1"] = BiWeeklyHours[0, 1];
            ViewData["OUT2"] = BiWeeklyHours[1, 1];
            ViewData["OUT3"] = BiWeeklyHours[2, 1];
            ViewData["OUT4"] = BiWeeklyHours[3, 1];
            ViewData["OUT5"] = BiWeeklyHours[4, 1];
            ViewData["OUT6"] = BiWeeklyHours[5, 1];
            ViewData["OUT7"] = BiWeeklyHours[6, 1];
            ViewData["OUT8"] = BiWeeklyHours[7, 1];
            ViewData["OUT9"] = BiWeeklyHours[8, 1];
            ViewData["OUT10"] = BiWeeklyHours[9, 1];
            ViewData["OUT11"] = BiWeeklyHours[10, 1];
            ViewData["OUT12"] = BiWeeklyHours[11, 1];
            ViewData["OUT13"] = BiWeeklyHours[12, 1];
            ViewData["OUT14"] = BiWeeklyHours[13, 1];
            //End of Table 3


            //Checking wheather ,is there any record in tbl_employee_payroll_biweekly. If there is any record, then no need to insert otherwise insert null values
            string recordFlag = "0";
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb.Append("select * FRom  tbl_employee_payroll_biweekly where employee_id=@empId and payroll_id= @payRollId ");

                    string sql = sb.ToString();


                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@empId", management_emp_id);
                        command.Parameters.AddWithValue("@payRollId", payrollid);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                recordFlag = dr["recordflag"].ToString();


                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch (Exception payRollBiWeekly)
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet" + payRollBiWeekly);
            }
            Debug.WriteLine("is there any values in tbl_employee_payroll_biweekly :" + recordFlag);
            //End of checking

            //If there is no record, then insert null values to the database upon opening the employee and if there is any record try to fetch details of the employee.
            if (recordFlag == "0")
            {
                //This is if there is no record in the table insert nulls
                try
                {
                   
                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();

                        sb.Append("insert into tbl_employee_payroll_biweekly values(@empId,@payRollId,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,@whoChangedthis,@recordChangeDate,1,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null)");
                        string sql = sb.ToString();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@empId", management_emp_id);
                            command.Parameters.AddWithValue("@payRollId", payrollid);
                            command.Parameters.AddWithValue("@whoChangedthis", Session["userID"]);
                            command.Parameters.AddWithValue("@recordChangeDate", DateTime.Now);

                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        connection.Close();
                    }
                }
                catch (SqlException l1)
                {
                    System.Diagnostics.Debug.WriteLine("111111111 :" + l1.ToString() + l1);
                }

                //Inserting record in summary table
                try
                {
                   
                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("insert into tbl_employee_payroll_summary values(@empId,@payRollId,null,null,null,null,null,1,@whoChangedthis,@recordChangeDate,null,null,null,null,null)");
                        string sql = sb.ToString();

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@empId", management_emp_id);
                            command.Parameters.AddWithValue("@payRollId", payrollid);
                            command.Parameters.AddWithValue("@whoChangedthis", Session["userID"]);
                            command.Parameters.AddWithValue("@recordChangeDate", DateTime.Now);

                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        connection.Close();
                    }
                }
                catch (SqlException l2)
                {
                    System.Diagnostics.Debug.WriteLine("2222222222 :" + l2.ToString() + l2);
                }

            }
            else if (recordFlag == "1")
            {


                //Do Nothing for current Payroll report because no need to insert any record...



            }

         

        }
    }
}