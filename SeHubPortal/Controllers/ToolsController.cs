using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class ToolsController : Controller
    {
        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }



        // GET: Tools
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Calculator()
        {
            //updateFuelLogTable();
            return View();
        }
        [HttpGet]
        public ActionResult FuelLog(string VIN)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            
           

            Debug.WriteLine("In AssetControl");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("empId:" + empId);
            var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            var acccess_level = db.tbl_sehub_access.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            int  permission_level = 0;
            if(acccess_level!=null)
            {
                permission_level = acccess_level.fuel_log.Value;

            }
            string locationid = "";
            if (result != null)
            {
                locationid = result.loc_ID;
            }
            else
            {
                locationid = "";
            }

            IOrderedQueryable<tbl_vehicle_info> VehicleDetails = null;
            if (permission_level==1)
            {
                VehicleDetails = db.tbl_vehicle_info.Where(x => x.assigned_to == empId).OrderBy(x => x.vehicle_short_id);
            }
           else if(permission_level > 1)
            {
                VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == locationid).OrderBy(x => x.vehicle_short_id);
            }
            if (VIN is null || VIN =="")
            {
                FuelLogViewModel fuelLogModel = new FuelLogViewModel();
                fuelLogModel.fuel_log_access = permission_level;
                if (VehicleDetails != null)
                {
                    Debug.WriteLine("Vehicle info there are details");
                    fuelLogModel.vehicleInfoList = VehicleDetails.ToList();
                   
                    return View(fuelLogModel);
                }
                else
                {
                    Debug.WriteLine("Vehicle info empty");
                   
                    return View(fuelLogModel);
                }
                
            }
            else
            {
                var fuelList = db.tbl_fuel_log.Where(x => x.VIN == VIN).OrderByDescending(x => x.date_of_purchase);
                var selectVehicleInfo = db.tbl_vehicle_info.Where(x => x.VIN == VIN).FirstOrDefault();

                FuelLogViewModel fuelLogModel = new FuelLogViewModel();
                if (VehicleDetails != null)
                {
                    Debug.WriteLine("Vehicle info there are details");
                    fuelLogModel.vehicleInfoList = VehicleDetails.ToList();
                    fuelLogModel.fuelLogList = fuelList.ToList();
                    fuelLogModel.selectedVIN = VIN;
                    fuelLogModel.SelectedVehicleInfo = selectVehicleInfo;                   
                    fuelLogModel.fuel_log_access = permission_level;
                    return View(fuelLogModel);
                }
                else
                {
                    Debug.WriteLine("Vehicle info empty");
                    fuelLogModel.fuel_log_access = permission_level;
                    return View(fuelLogModel);
                }
            }
            
           


        }
        [HttpPost]
        public ActionResult SubmitAuditStatus(FuelLogViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            foreach (var items in model.fuelLogList)
            {
                var result = db.tbl_fuel_log.Where(a => a.transaction_number.Equals(items.transaction_number)).FirstOrDefault();
                if (result != null)
                {
                    Debug.WriteLine("items.audit_status:" + items.audit_status);
                    result.audit_status = items.audit_status;                 
                }
                else
                {
                    Debug.WriteLine("Null");
                }
            }
            db.SaveChanges();          
            return RedirectToAction("FuelLog", new { VIN = model.SelectedVehicleInfo.VIN });
        }


        [HttpGet]
        public ActionResult DeleteFuelTransaction(string value)
        {

            Debug.WriteLine("In EditVehicleInfo:" + value);
            tbl_fuel_log fuelLog = new tbl_fuel_log();
            fuelLog.transaction_number = value;
            //CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            //var vehicleInfoObj = db.tbl_vehicle_info.Where(x => x.VIN == value).FirstOrDefault();
            //string employeeNameValue = "";
            //if (vehicleInfoObj.assigned_to is null)
            //{
            //    //Do Nothing
            //}
            //else
            //{
            //    if (vehicleInfoObj.assigned_to != 0)
            //    {
            //        var employeeTableCheck = db.tbl_employee.Where(x => x.employee_id == vehicleInfoObj.assigned_to).FirstOrDefault();
            //        employeeNameValue = employeeTableCheck.full_name;
            //    }

            //}

            //AddNewVehicleViewModel obj = new AddNewVehicleViewModel();
            //obj.VehicleInfo = vehicleInfoObj;
            //obj.MatchedLocs = populateLocations();
            //obj.MatchedLocID = vehicleInfoObj.loc_id;
            //Debug.WriteLine("Full Name:" + employeeNameValue);

            //obj.MatchedEmployeeName = employeeNameValue;
            //Debug.WriteLine("In EditVehicleInfo date:" + vehicleInfoObj.inspection_due_date);
            return PartialView(fuelLog);
        }
        [HttpPost]
        public ActionResult DeleteFuelTransaction(tbl_fuel_log model)
        {
            string transcationNumber = model.transaction_number;
            string VinNumber = model.transaction_number.ToString().Split('-')[0];

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var fuelInfo = db.tbl_fuel_log.Where(x => x.transaction_number == transcationNumber).FirstOrDefault();
            
            if(fuelInfo!=null )
            {
                db.tbl_fuel_log.Remove(fuelInfo);
            }
            db.SaveChanges();
            Debug.WriteLine("Vin Number:" + VinNumber);
            return RedirectToAction("FuelLog", new { VIN = VinNumber });

        }

        [HttpPost]
        public ActionResult SaveFuelReceipt(FuelLogViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            Debug.WriteLine(model.SelectedVehicleInfo.VIN);
            Debug.WriteLine(model.fuelLogTableValues.no_of_liters);
            model.fuelLogTableValues.VIN = model.SelectedVehicleInfo.VIN;
            model.fuelLogTableValues.employee_id = model.SelectedVehicleInfo.assigned_to;


            string transactionReceiptNumber = Convert.ToDateTime(model.fuelLogTableValues.date_of_purchase).ToString("yyyyMMdd");
            var fuelList = db.tbl_fuel_log.Where(x => x.VIN == model.SelectedVehicleInfo.VIN && x.transaction_number.Contains(model.SelectedVehicleInfo.VIN + "-" + transactionReceiptNumber)).OrderByDescending(x=>x.transaction_number).FirstOrDefault();
            
            int lastTwoSequence = 0;
            if(fuelList!=null)
            {
                lastTwoSequence =Convert.ToInt32(fuelList.transaction_number.ToString().Substring(fuelList.transaction_number.ToString().Length - 1));
            }
           
            if(lastTwoSequence==0)
            {
                model.fuelLogTableValues.transaction_number = model.SelectedVehicleInfo.VIN + "-" + Convert.ToDateTime(model.fuelLogTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + "1";
            }
            else{
                model.fuelLogTableValues.transaction_number = model.SelectedVehicleInfo.VIN + "-" + Convert.ToDateTime(model.fuelLogTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + (lastTwoSequence + 1).ToString();
            }
            model.fuelLogTableValues.audit_status = false;
            db.tbl_fuel_log.Add(model.fuelLogTableValues);

            db.SaveChanges();

            return RedirectToAction("FuelLog", new { VIN = model.SelectedVehicleInfo.VIN });
        }
       
        public ActionResult DeleteTransaction(string id)
        {
            Debug.WriteLine("Value to be delated:" + id);
            return RedirectToAction("FuelLog", new { VIN = id.Split(';')[1] });  
        }
        public void updateFuelLogTable()
        {
            List<string> items = new List<string>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select VIN,date_of_purchase From tbl_fuel_log";
                Debug.WriteLine("Query:" + query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            items.Add(sdr["VIN"] + ";" + Convert.ToDateTime(sdr["date_of_purchase"]).ToString("yyyy-MM-dd"));

                            //string val = sdr["VIN"]+ "-" +Convert.ToDateTime(sdr["date_of_purchase"]).ToString("yyyymmdd")+"-";
                        }


                    }
                    con.Close();
                }
            }

            foreach(object item in items)
            {
                Debug.WriteLine("value:" + item);
                string[] itemval = item.ToString().Split(';');
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE ");
                    sb.Append("tbl_fuel_log ");
                    sb.Append("SET transaction_number = @atNumber  ");
                    sb.Append("WHERE VIN = @vin AND date_of_purchase=@date_of_purchase  ");
                    
                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@atNumber", itemval[0] + "-" + Convert.ToDateTime(itemval[1]).ToString("yyyyMMdd")+"-1");
                        command.Parameters.AddWithValue("@vin", itemval[0]);
                        command.Parameters.AddWithValue("@date_of_purchase", itemval[1]);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
        }


        [HttpGet]
        public ActionResult EmployeePermissions(string locId)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int permissions = CheckPermissions().user_management.Value;

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            EmployeePermissionsViewModel model = new EmployeePermissionsViewModel();
            model.userManagementAccessLevel = permissions;
            string location = "";

            if (locId is null || locId == "")
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
            var employeeList = db.tbl_employee.Where(x => x.loc_ID.Contains(location) && x.status == 1).OrderBy(x => x.employee_id).ToList();
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
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

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
            if (credentialsObj is null)
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
            if(obj.SehubAccess.app_access==0)
            {
                obj.appAccess = false;
            }
            else
            {
                obj.appAccess = true;
            }
           if(empDetails.pic_status==0)
            {
                obj.monitorEmployee = false;
            }
            else
            {
                obj.monitorEmployee = true;
            }
            obj.empDetails = empDetails;
            return PartialView(obj);
        }

        [HttpPost]
        public ActionResult ManageEmployeePermisssions(ModifyEmployeePermissions model)
        {
            Debug.WriteLine("App access:" + model.SehubAccess.app_access);
            Debug.WriteLine("Library access:" + model.SehubAccess.library_access);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var SehubAccessObj = db.tbl_sehub_access.Where(x => x.employee_id == model.EmployeeCredentials.employee_id).FirstOrDefault();
            var credentialsObj = db.tbl_employee_credentials.Where(x => x.employee_id == model.EmployeeCredentials.employee_id).FirstOrDefault();
            var Empdetails = db.tbl_employee.Where(x => x.employee_id == model.empDetails.employee_id).FirstOrDefault();
            var empAttendace = db.tbl_employee_attendance.Where(x => x.employee_id == model.empDetails.employee_id).FirstOrDefault();
            if (credentialsObj != null)
            {
                credentialsObj.password = model.EmployeeCredentials.password;
                credentialsObj.password365 = model.EmployeeCredentials.password365;
            }
            if (SehubAccessObj != null)
            {
                //SehubAccessObj.app_access = model.SehubAccess.app_access;
                if (model.appAccess == true)
                {
                    SehubAccessObj.app_access = 1;
                }
                else
                {
                    SehubAccessObj.app_access = 0;
                }
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
            if (Empdetails != null)
            {
                Empdetails.rfid_number = model.empDetails.rfid_number;
                Empdetails.loc_ID = model.empDetails.loc_ID;
                if(model.monitorEmployee==true)
                {
                    Empdetails.pic_status = 1;
                }
                else
                {
                    Empdetails.pic_status = 0;
                }
            }

            if (empAttendace != null)
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
        public ActionResult CRM()
        {
            return View();
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

    }
}