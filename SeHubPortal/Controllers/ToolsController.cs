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
        // GET: Tools
        public ActionResult Index()
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

            Debug.WriteLine("In AssetControl");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("empId:" + empId);
            var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            var acccess_level = db.tbl_sehub_access.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            string permission_level = "0";
            if(acccess_level!=null)
            {
                permission_level = acccess_level.fuel_log.ToString();

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
            var VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == locationid).OrderBy(x => x.vehicle_short_id);

            
            if(VIN is null || VIN =="")
            {
                FuelLogViewModel fuelLogModel = new FuelLogViewModel();
                if (VehicleDetails != null)
                {
                    Debug.WriteLine("Vehicle info there are details");
                    fuelLogModel.vehicleInfoList = VehicleDetails.ToList();

                    return View(fuelLogModel);
                }
                else
                {
                    Debug.WriteLine("Vehicle info empty");
                    return View();
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
                    return View();
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

    }
}