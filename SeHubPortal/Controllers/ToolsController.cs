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
using System.IO;
using System;
using Newtonsoft.Json.Linq;





namespace SeHubPortal.Controllers
{
    public class ToolsController : Controller
    {
        public string Current_sequence;
        public string Loc_ID;
        int NewSequenceNum;
        public string locId;

        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        [HttpPost]
        public (double, double) PartsPricing(double value)
        {
            Debug.WriteLine(value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var parts = db.tbl_calculator_GM_parts.ToList();

            double retail_price = 0;
            double N_A_price = 0;

            for (int i = 1; i < parts.Count; i++)
            {
                if (parts[i].minimum_cost <= value && value <= parts[i].maximum_cost)
                {
                    retail_price = Convert.ToDouble(value / (1 - parts[i].GrossMargin));
                    N_A_price = Convert.ToDouble(value / (1 - parts[i].GrossMargin_NA));

                }
            }

            //string retail_Price = Convert.ToString(Math.Round(retail_price, 2));

            //string N_A_Price = Convert.ToString(Math.Round(N_A_price, 2));


            return (Math.Round(retail_price, 2), Math.Round(N_A_price, 2));
        }


        [HttpPost]
        public (double, double) TirePricing(double value)
        {
            //Debug.WriteLine(value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var Tire = db.tbl_calculator_GM_tire.ToList();

            double retail_price = 0;
            double N_A_price = 0;

            //if (Tire[2].minimum_cost < value) {
            //    Debug.WriteLine(Tire[2].minimum_cost + "Success");
            //}

            if (value <= 54.99)
            {
                retail_price = Convert.ToDouble(value / (1 - 0.29));
                N_A_price = Convert.ToDouble(value / (1 - 0.3965));
            }
            else
            {
                for (int i = 1; i < Tire.Count; i++)
                {
                    //Debug.WriteLine(i);
                    //Debug.WriteLine(Tire[i].minimum_cost + " LowerBound " + value + " UpperBound " + Tire[i].maximum_cost + "and the GM is " + Tire[i].GrossMargin);
                    if (Tire[i].minimum_cost <= value && value <= Tire[i].maximum_cost)
                    {
                        retail_price = Convert.ToDouble(value / (1 - Tire[i].GrossMargin));
                        N_A_price = Convert.ToDouble(value / (1 - Tire[i].GrossMargin_NA));

                        Debug.WriteLine(Tire[i].minimum_cost + " LowerBound " + value + " UpperBound " + Tire[i].maximum_cost + "and the GM is " + Tire[i].GrossMargin);

                    }
                }
            }



            return (Math.Round(retail_price, 2), Math.Round(N_A_price, 2));
        }

        [HttpPost]
        public string FreightPricing(string value)
        {
            Debug.WriteLine(value);



            return "Freight";
        }


        [HttpPost]
        public string LoadTAform(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var TA = db.tbl_tire_adjustment.Where(a => a.Tire_Adjustment_id == value).FirstOrDefault();

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(TA);

        }


        [HttpPost]
        public ActionResult FuelLogChangeLocation(EmployeePayrollModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var vehicalsList = db.tbl_vehicle_info.Where(x => x.loc_id == model.MatchedLocID);

            if (vehicalsList.Count() == 1)
            {
                var soloVehicalDetails = vehicalsList.Where(x => x.loc_id == model.MatchedLocID).FirstOrDefault();
                return RedirectToAction("FuelLog", new { VIN = soloVehicalDetails.VIN, loc = model.MatchedLocID });
            }
            else
            {
                return RedirectToAction("FuelLog", new { VIN = "NoSelectedVehicle", loc = model.MatchedLocID });
            }

        }


        [HttpPost]
        public ActionResult ChangeVehicalFuelLog(FuelLogViewModel model)
        {
            return RedirectToAction("FuelLog", new { VIN = model.MatchedVehicle, loc = model.MatchedLocID });
        }


        // GET: Tools

        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                string filePath = file.FileName + Path.GetExtension(file.FileName);
                file.SaveAs(Path.Combine(Server.MapPath("~/Images"), filePath));
                //Here you can write code for save this information in your database if you want
            }

            return Json("file uploaded successfully");
        }



        [HttpPost]
        public ActionResult TireAdjustment(tbl_tire_adjustment model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string locId = empDetails.loc_ID;
            string cta_emp_name = empDetails.full_name;

            //Debug.WriteLine("This is the tire adjustment ID" + model.Tire_Adjustment_id);
            var present = db.tbl_tire_adjustment.Where(x => x.Tire_Adjustment_id == model.Tire_Adjustment_id).FirstOrDefault();


            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select top 1 * from tbl_tire_adjustment where Tire_Adjustment_id like '%" + locId + "%' order by Tire_Adjustment_id desc";
                    Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {

                                Current_sequence = Convert.ToString(sdr["Tire_Adjustment_id"]);
                                Debug.WriteLine(Current_sequence);
                                if (Current_sequence != null)
                                {

                                    Debug.WriteLine(Current_sequence.Substring(7));
                                    NewSequenceNum = Convert.ToInt32(Current_sequence.Substring(6)) + 1;
                                }
                                else
                                {
                                    NewSequenceNum = 0;
                                }
                            }
                        }
                        con.Close();
                    }
                }

                if (present != null)
                {
                    db.tbl_tire_adjustment.Remove(present);
                    NewSequenceNum = Convert.ToInt32(model.Tire_Adjustment_id.Substring(model.Tire_Adjustment_id.Length - 5));
                }

                tbl_tire_adjustment data = new tbl_tire_adjustment();

                data.Tire_Adjustment_id = "TA-" + locId + NewSequenceNum.ToString("D5");

                data.date = System.DateTime.Today.Date;
                data.cta_employee_name = empDetails.full_name;
                data.non_sig_number = model.non_sig_number;
                data.location_id = empDetails.loc_ID;
                data.replacement_invoice = model.replacement_invoice;


                Debug.WriteLine("********************************This is the name" + model.name);

                data.name = model.name;
                data.phone = model.phone;
                data.street = model.street;
                data.city = model.city;
                data.province = model.province;
                data.postal_code = model.postal_code;
                data.brand = model.brand;
                data.tread = model.tread;
                data.size = model.size;
                data.quantity = model.quantity;
                data.current_EDL = model.current_EDL;
                data.original_32nds = model.original_32nds;
                data.manufacture_approval = model.manufacture_approval;
                data.modified_adjustment_modified = model.modified_adjustment_modified;
                data.mileage_warranty = model.mileage_warranty;
                data.average_tire_mileage = model.average_tire_mileage;
                data.modified_adjustment_mileage = model.modified_adjustment_mileage;

                data.dot_0 = model.dot_0;
                data.product_code_0 = model.product_code_0;
                data.thirty_seconds_0 = model.thirty_seconds_0;
                data.mileage_0 = model.mileage_0;
                data.percent_worn_0 = model.percent_worn_0;
                data.repl_cost_0 = model.repl_cost_0;
                data.reason_for_removal_0 = model.reason_for_removal_0;

                data.dot_1 = model.dot_1;
                data.product_code_1 = model.product_code_1;
                data.thirty_seconds_1 = model.thirty_seconds_1;
                data.mileage_1 = model.mileage_1;
                data.percent_worn_1 = model.percent_worn_1;
                data.repl_cost_1 = model.repl_cost_1;
                data.reason_for_removal_1 = model.reason_for_removal_1;

                data.dot_2 = model.dot_2;
                data.product_code_2 = model.product_code_2;
                data.thirty_seconds_2 = model.thirty_seconds_2;
                data.mileage_2 = model.mileage_2;
                data.percent_worn_2 = model.percent_worn_2;
                data.repl_cost_2 = model.repl_cost_2;
                data.reason_for_removal_2 = model.reason_for_removal_2;

                data.dot_3 = model.dot_3;
                data.product_code_3 = model.product_code_3;
                data.thirty_seconds_3 = model.thirty_seconds_3;
                data.mileage_3 = model.mileage_3;
                data.percent_worn_3 = model.percent_worn_3;
                data.repl_cost_3 = model.repl_cost_3;
                data.reason_for_removal_3 = model.reason_for_removal_3;

                data.dot_4 = model.dot_3;
                data.product_code_4 = model.product_code_4;
                data.thirty_seconds_4 = model.thirty_seconds_4;
                data.mileage_4 = model.mileage_4;
                data.percent_worn_4 = model.percent_worn_4;
                data.repl_cost_4 = model.repl_cost_4;
                data.reason_for_removal_4 = model.reason_for_removal_4;

                data.dot_5 = model.dot_5;
                data.product_code_5 = model.product_code_5;
                data.thirty_seconds_5 = model.thirty_seconds_5;
                data.mileage_5 = model.mileage_5;
                data.percent_worn_5 = model.percent_worn_5;
                data.repl_cost_5 = model.repl_cost_5;
                data.reason_for_removal_5 = model.reason_for_removal_5;

                data.additional_comments = model.additional_comments;

                db.tbl_tire_adjustment.Add(data);
                db.SaveChanges();

            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return RedirectToAction("Dashboard");
        }


        [HttpPost]
        public string SaveTireAdjustment(string value)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string locId = empDetails.loc_ID;
            string cta_emp_name = empDetails.full_name;
            Debug.Write("Reached");
            var TADetails = JObject.Parse(value);

            Debug.Write(TADetails["replacement_invoice"]);

            string TAID = Convert.ToString(TADetails["Tire_Adjustment_id"]);

            var present = db.tbl_tire_adjustment.Where(x => x.Tire_Adjustment_id == TAID).FirstOrDefault();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 * from tbl_tire_adjustment where Tire_Adjustment_id like '%" + locId + "%' order by Tire_Adjustment_id desc";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            Current_sequence = Convert.ToString(sdr["Tire_Adjustment_id"]);
                            Debug.WriteLine(Current_sequence);
                            if (Current_sequence != null)
                            {

                                Debug.WriteLine(Current_sequence.Substring(7));
                                NewSequenceNum = Convert.ToInt32(Current_sequence.Substring(6)) + 1;
                            }
                            else
                            {
                                NewSequenceNum = 0;
                            }
                        }
                    }
                    con.Close();
                }
            }

            if (present != null)
            {
                db.tbl_tire_adjustment.Remove(present);
                NewSequenceNum = Convert.ToInt32(Convert.ToString(TADetails["Tire_Adjustment_id"]).Substring(Convert.ToString(TADetails["Tire_Adjustment_id"]).Length - 5));
            }

            tbl_tire_adjustment data = new tbl_tire_adjustment();

            data.Tire_Adjustment_id = "TA-" + locId + NewSequenceNum.ToString("D5");

            data.date = System.DateTime.Today.Date;
            data.cta_employee_name = Convert.ToString(TADetails["cta_employee_name"]);
            data.non_sig_number = Convert.ToString(TADetails["non_sig_number"]);
            data.location_id = Convert.ToString(TADetails["location_id"]);
            data.replacement_invoice = Convert.ToString(TADetails["replacement_invoice"]);



            data.name = Convert.ToString(TADetails["name"]);
            data.phone = Convert.ToString(TADetails["phone"]);
            data.street = Convert.ToString(TADetails["street"]);
            data.city = Convert.ToString(TADetails["city"]);
            data.province = Convert.ToString(TADetails["province"]);
            data.postal_code = Convert.ToString(TADetails["postal_code"]);
            data.brand = Convert.ToString(TADetails["brand"]);
            data.tread = Convert.ToString(TADetails["tread"]);
            data.size = Convert.ToString(TADetails["size"]);



            if (TADetails["quantity"] != null && Convert.ToString(TADetails["quantity"]) != "")
            {
                data.quantity = Convert.ToInt32(TADetails["quantity"]);
            }
            if (TADetails["current_EDL"] != null && Convert.ToString(TADetails["current_EDL"]) != "")
            {
                data.current_EDL = Convert.ToDouble(TADetails["current_EDL"]);
            }
            if (TADetails["original_32nds"] != null && Convert.ToString(TADetails["original_32nds"]) != "")
            {
                data.original_32nds = Convert.ToInt32(TADetails["original_32nds"]);
            }
            data.manufacture_approval = Convert.ToString(TADetails["manufacture_approval"]);

            if (TADetails["modified_adjustment_modified"] != null && Convert.ToString(TADetails["modified_adjustment_modified"]) != "")
            {
                data.modified_adjustment_modified = Convert.ToDouble(TADetails["modified_adjustment_modified"]);
            }

            if (TADetails["mileage_warranty"] != null && Convert.ToString(TADetails["mileage_warranty"]) != "")
            {
                data.mileage_warranty = Convert.ToInt32(TADetails["mileage_warranty"]);
            }

            if (TADetails["average_tire_mileage"] != null && Convert.ToString(TADetails["average_tire_mileage"]) != "")
            {
                data.average_tire_mileage = Convert.ToInt32(TADetails["average_tire_mileage"]);
            }

            if (TADetails["modified_adjustment_mileage"] != null && Convert.ToString(TADetails["modified_adjustment_mileage"]) != "")
            {
                data.modified_adjustment_mileage = Convert.ToDouble(TADetails["modified_adjustment_mileage"]);
            }

            data.dot_0 = Convert.ToString(TADetails["dot_0"]);
            data.product_code_0 = Convert.ToString(TADetails["product_code_0"]);

            if (TADetails["thirty_seconds_0"] != null && Convert.ToString(TADetails["thirty_seconds_0"]) != "")
            {
                data.thirty_seconds_0 = Convert.ToInt32(TADetails["thirty_seconds_0"]);
            }
            if (TADetails["mileage_0"] != null && Convert.ToString(TADetails["mileage_0"]) != "")
            {
                data.mileage_0 = Convert.ToInt32(TADetails["mileage_0"]);
            }
            if (TADetails["percent_worn_0"] != null && Convert.ToString(TADetails["percent_worn_0"]) != "")
            {
                data.percent_worn_0 = Convert.ToDouble(TADetails["percent_worn_0"]);
            }
            if (TADetails["repl_cost_0"] != null && Convert.ToString(TADetails["repl_cost_0"]) != "")
            {
                data.repl_cost_0 = Convert.ToDouble(TADetails["repl_cost_0"]);
            }
            data.reason_for_removal_0 = Convert.ToString(TADetails["reason_for_removal_0"]);

            data.dot_1 = Convert.ToString(TADetails["dot_1"]);
            data.product_code_1 = Convert.ToString(TADetails["product_code_1"]);
            if (TADetails["thirty_seconds_1"] != null && Convert.ToString(TADetails["thirty_seconds_1"]) != "")
            {
                data.thirty_seconds_1 = Convert.ToInt32(TADetails["thirty_seconds_1"]);
            }
            if (TADetails["mileage_1"] != null && Convert.ToString(TADetails["mileage_1"]) != "")
            {
                data.mileage_1 = Convert.ToInt32(TADetails["mileage_1"]);
            }
            if (TADetails["percent_worn_1"] != null && Convert.ToString(TADetails["percent_worn_1"]) != "")
            {
                data.percent_worn_1 = Convert.ToDouble(TADetails["percent_worn_1"]);
            }
            if (TADetails["repl_cost_1"] != null && Convert.ToString(TADetails["repl_cost_1"]) != "")
            {
                data.repl_cost_1 = Convert.ToDouble(TADetails["repl_cost_1"]);
            }
            data.reason_for_removal_1 = Convert.ToString(TADetails["reason_for_removal_1"]);

            data.dot_2 = Convert.ToString(TADetails["dot_2"]);
            data.product_code_2 = Convert.ToString(TADetails["product_code_2"]);
            if (TADetails["thirty_seconds_2"] != null && Convert.ToString(TADetails["thirty_seconds_2"]) != "")
            {
                data.thirty_seconds_2 = Convert.ToInt32(TADetails["thirty_seconds_2"]);
            }
            if (TADetails["mileage_2"] != null && Convert.ToString(TADetails["mileage_2"]) != "")
            {
                data.mileage_2 = Convert.ToInt32(TADetails["mileage_2"]);
            }
            if (TADetails["percent_worn_2"] != null && Convert.ToString(TADetails["percent_worn_2"]) != "")
            {
                data.percent_worn_2 = Convert.ToDouble(TADetails["percent_worn_2"]);
            }
            if (TADetails["repl_cost_2"] != null && Convert.ToString(TADetails["repl_cost_2"]) != "")
            {
                data.repl_cost_2 = Convert.ToDouble(TADetails["repl_cost_2"]);
            }
            data.reason_for_removal_2 = Convert.ToString(TADetails["reason_for_removal_2"]);

            data.dot_3 = Convert.ToString(TADetails["dot_3"]);
            data.product_code_3 = Convert.ToString(TADetails["product_code_3"]);
            if (TADetails["thirty_seconds_3"] != null && Convert.ToString(TADetails["thirty_seconds_3"]) != "")
            {
                data.thirty_seconds_3 = Convert.ToInt32(TADetails["thirty_seconds_3"]);
            }
            if (TADetails["mileage_3"] != null && Convert.ToString(TADetails["mileage_3"]) != "")
            {
                data.mileage_3 = Convert.ToInt32(TADetails["mileage_3"]);
            }
            if (TADetails["percent_worn_3"] != null && Convert.ToString(TADetails["percent_worn_3"]) != "")
            {
                data.percent_worn_3 = Convert.ToDouble(TADetails["percent_worn_3"]);
            }
            if (TADetails["repl_cost_3"] != null && Convert.ToString(TADetails["repl_cost_3"]) != "")
            {
                data.repl_cost_3 = Convert.ToDouble(TADetails["repl_cost_3"]);
            }
            data.reason_for_removal_3 = Convert.ToString(TADetails["reason_for_removal_3"]);

            data.dot_4 = Convert.ToString(TADetails["dot_4"]);
            data.product_code_4 = Convert.ToString(TADetails["product_code_4"]);
            if (TADetails["thirty_seconds_4"] != null && Convert.ToString(TADetails["thirty_seconds_4"]) != "")
            {
                data.thirty_seconds_4 = Convert.ToInt32(TADetails["thirty_seconds_4"]);
            }
            if (TADetails["mileage_4"] != null && Convert.ToString(TADetails["mileage_4"]) != "")
            {
                data.mileage_4 = Convert.ToInt32(TADetails["mileage_4"]);
            }
            if (TADetails["percent_worn_4"] != null && Convert.ToString(TADetails["percent_worn_4"]) != "")
            {
                data.percent_worn_4 = Convert.ToDouble(TADetails["percent_worn_4"]);
            }
            if (TADetails["repl_cost_4"] != null && Convert.ToString(TADetails["repl_cost_4"]) != "")
            {
                data.repl_cost_4 = Convert.ToDouble(TADetails["repl_cost_4"]);
            }
            data.reason_for_removal_4 = Convert.ToString(TADetails["reason_for_removal_4"]);

            data.dot_5 = Convert.ToString(TADetails["dot_5"]);
            data.product_code_5 = Convert.ToString(TADetails["product_code_5"]);
            if (TADetails["thirty_seconds_5"] != null && Convert.ToString(TADetails["thirty_seconds_5"]) != "")
            {
                data.thirty_seconds_5 = Convert.ToInt32(TADetails["thirty_seconds_5"]);
            }
            if (TADetails["mileage_5"] != null && Convert.ToString(TADetails["mileage_5"]) != "")
            {
                data.mileage_5 = Convert.ToInt32(TADetails["mileage_5"]);
            }
            if (TADetails["percent_worn_5"] != null && Convert.ToString(TADetails["percent_worn_5"]) != "")
            {
                data.percent_worn_5 = Convert.ToDouble(TADetails["percent_worn_5"]);
            }
            if (TADetails["repl_cost_5"] != null && Convert.ToString(TADetails["repl_cost_5"]) != "")
            {
                data.repl_cost_5 = Convert.ToDouble(TADetails["repl_cost_5"]);
            }
            data.reason_for_removal_5 = Convert.ToString(TADetails["reason_for_removal_5"]);

            data.additional_comments = Convert.ToString(TADetails["additional_comments"]);


            db.tbl_tire_adjustment.Add(data);
            db.SaveChanges();

            return data.Tire_Adjustment_id;
        }

        public ActionResult Dashboard(tbl_tire_adjustment model)
        {
            tbl_sehub_access Access = new tbl_sehub_access();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            Access = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            if (Access.tools == 0)
            {
                return RedirectToAction("Dashboard", "TreadTracker");
            }

            if (Access.tools_dashboard == 0)
            {
                return RedirectToAction("FuelLog", "Tools");
            }


            ViewData["tools_dashboard"] = Access.tools_dashboard;
            ViewData["fuel_log"] = Access.fuel_log;
            ViewData["crm"] = Access.customer_reporting;
            ViewData["main"] = Access.main;
            ViewData["library"] = Access.library_access;
            ViewData["management"] = Access.management;
            ViewData["treadTracker"] = Access.treadTracker;
            ViewData["fleetTVT"] = Access.fleetTVT;
            ViewData["payroll"] = Access.payroll;
            ViewData["settings"] = Access.settings;


            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string locatId = empDetails.loc_ID;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 * from tbl_tire_adjustment where Tire_Adjustment_id like '%" + locatId + "%' order by Tire_Adjustment_id desc";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            Current_sequence = Convert.ToString(sdr["Tire_Adjustment_id"]);
                            Debug.WriteLine(Current_sequence);
                            if (Current_sequence != null)
                            {

                                Debug.WriteLine(Current_sequence.Substring(7));
                                NewSequenceNum = Convert.ToInt32(Current_sequence.Substring(6)) + 1;
                            }
                            else
                            {
                                NewSequenceNum = 0;
                            }
                        }

                    }
                    con.Close();
                }
            }

            using (SqlConnection con = new SqlConnection(constr))
            {

                string query = "select * from tbl_employee where employee_id = '" + empId + "'";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            model.cta_employee_name = Convert.ToString(sdr["full_name"]);
                            model.location_id = Convert.ToString(sdr["loc_ID"]);
                            locId = model.location_id;
                            model.Tire_Adjustment_id = "TA-" + locId + "XXXXX";
                            Debug.WriteLine("test1");
                            Debug.WriteLine(model.Tire_Adjustment_id);
                        }

                    }
                    con.Close();
                }

            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 * from tbl_tire_adjustment a, tblLocation b where a.location_id = b.locID";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            model.non_sig_number = Convert.ToString(sdr["locNONSIG"]);
                        }

                    }
                    con.Close();
                }
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select * from tblLocation where locID = '" + locId + "'";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            ViewBag.locID = Convert.ToString(sdr["locID"]);
                            ViewBag.addressSTREET1 = Convert.ToString(sdr["addressSTREET1"]);
                            ViewBag.addressSTREET2 = Convert.ToString(sdr["addressSTREET2"]);
                            ViewBag.addressCITY = Convert.ToString(sdr["addressCITY"]);
                            ViewBag.addressPROVINCE = Convert.ToString(sdr["addressPROVINCE"]);
                            ViewBag.addressPOSTAL = Convert.ToString(sdr["addressPOSTAL"]);
                            ViewBag.locPHONE = Convert.ToString(sdr["locPHONE"]);
                            ViewBag.locFAX = Convert.ToString(sdr["locFAX"]);
                        }

                    }
                    con.Close();
                }
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult FuelLog(string VIN, string loc)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            Debug.WriteLine("*********************************************************************" + loc + "********************************************************************");


            Debug.WriteLine("In AssetControl");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());


            //Debug.WriteLine("empId:" + empId);
            var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            var acccess_level = db.tbl_sehub_access.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            int permission_level = 0;
            if (acccess_level != null)
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
            if (permission_level == 1)
            {
                VehicleDetails = db.tbl_vehicle_info.Where(x => x.assigned_to == empId).OrderBy(x => x.vehicle_short_id);
            }
            else if (permission_level > 1)
            {
                if (loc != null && loc != "")
                {
                    VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == loc).OrderBy(x => x.vehicle_short_id);
                }
                else
                {
                    VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == locationid).OrderBy(x => x.vehicle_short_id);
                }
            }
            if (VIN is null || VIN == "")
            {
                FuelLogViewModel fuelLogModel = new FuelLogViewModel();

                fuelLogModel.MatchedLocs = populateLocations();
                var empDetail = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

                if (loc != null && loc != "")
                {
                    fuelLogModel.MatchedLocID = loc;
                    Debug.WriteLine("This is the chosen location id" + loc);
                }
                else
                {
                    fuelLogModel.MatchedLocID = empDetail.loc_ID;
                    Debug.WriteLine("Current location is reaching");
                }

                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                fuelLogModel.SehubAccess = empDetails;

                if (fuelLogModel.SehubAccess.fuel_log == 0)
                {
                    return RedirectToAction("CRM", "Tools");
                }

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
            else if (VIN == "NoSelectedVehicle")
            {

                tbl_vehicle_info selectVehicleInfo = new tbl_vehicle_info();
                List<tbl_fuel_log> fuelList = new List<tbl_fuel_log>();

                selectVehicleInfo.VIN = "";
                selectVehicleInfo.vehicle_short_id = "";
                selectVehicleInfo.vehicle_long_id = "";
                selectVehicleInfo.vehicle_plate = "";
                selectVehicleInfo.vehicle_manufacturer = "";
                selectVehicleInfo.vehicle_model = "";
                selectVehicleInfo.current_milage = 0;
                selectVehicleInfo.efficiency_price = 0;
                selectVehicleInfo.efficiency_liter = 0;


                FuelLogViewModel fuelLogModel = new FuelLogViewModel();

                fuelLogModel.MatchedLocs = populateLocations();
                fuelLogModel.MatchedVehicle = "101-20003";//selectVehicleInfo.vehicle_short_id;

                var empDetail = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

                if (loc != null && loc != "")
                {
                    fuelLogModel.MatchedLocID = loc;
                    Debug.WriteLine("This is the chosen location id" + loc);
                }
                else
                {
                    fuelLogModel.MatchedLocID = empDetail.loc_ID;
                    Debug.WriteLine("Current location is reaching");
                }

                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                fuelLogModel.SehubAccess = empDetails;

                if (fuelLogModel.SehubAccess.fuel_log == 0)
                {
                    return RedirectToAction("CRM", "Tools");
                }

                if (VehicleDetails != null)
                {
                    Debug.WriteLine("Vehicle info there are details");
                    fuelLogModel.vehicleInfoList = VehicleDetails.ToList();
                    fuelLogModel.fuelLogList = fuelList.ToList();
                    fuelLogModel.selectedVIN = VIN;
                    fuelLogModel.SelectedVehicleInfo = selectVehicleInfo;
                    fuelLogModel.fuel_log_access = permission_level;
                    fuelLogModel.MatchedVehicals = populateVehicles(loc, selectVehicleInfo.vehicle_short_id);
                    fuelLogModel.MatchedVehicle = selectVehicleInfo.vehicle_short_id;
                    return View(fuelLogModel);
                }
                else
                {
                    Debug.WriteLine("Vehicle info empty");
                    fuelLogModel.fuel_log_access = permission_level;
                    return View(fuelLogModel);
                }
            }
            else
            {
                var fuelList = db.tbl_fuel_log.Where(x => x.VIN == VIN).OrderByDescending(x => x.date_of_purchase);
                var selectVehicleInfo = db.tbl_vehicle_info.Where(x => x.VIN == VIN).FirstOrDefault();

                FuelLogViewModel fuelLogModel = new FuelLogViewModel();


                var empDetail = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();


                

                if (loc != null && loc != "")
                {
                    fuelLogModel.MatchedLocID = loc;
                    Debug.WriteLine("This is the chosen location id" + loc);
                }
                else
                {
                    fuelLogModel.MatchedLocID = empDetail.loc_ID;
                    Debug.WriteLine("Current location is reaching");
                }

                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                fuelLogModel.SehubAccess = empDetails;

                if (fuelLogModel.SehubAccess.fuel_log == 0)
                {
                    return RedirectToAction("CRM", "Tools");
                }

                if (VehicleDetails != null)
                {
                    Debug.WriteLine("Vehicle info there are details");
                    fuelLogModel.vehicleInfoList = VehicleDetails.ToList();
                    fuelLogModel.fuelLogList = fuelList.ToList();
                    fuelLogModel.selectedVIN = VIN;
                    fuelLogModel.SelectedVehicleInfo = selectVehicleInfo;
                    fuelLogModel.fuel_log_access = permission_level;
                    fuelLogModel.MatchedLocs = populateLocations();
                    fuelLogModel.MatchedVehicals = populateVehicles(loc, selectVehicleInfo.vehicle_short_id);
                    fuelLogModel.MatchedVehicle = selectVehicleInfo.vehicle_short_id;
                    Debug.WriteLine("-------------++++++++++++" + fuelLogModel.MatchedVehicle + "+++++++++-----------");

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

            if (fuelInfo != null)
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
            var fuelList = db.tbl_fuel_log.Where(x => x.VIN == model.SelectedVehicleInfo.VIN && x.transaction_number.Contains(model.SelectedVehicleInfo.VIN + "-" + transactionReceiptNumber)).OrderByDescending(x => x.transaction_number).FirstOrDefault();

            int lastTwoSequence = 0;
            if (fuelList != null)
            {
                lastTwoSequence = Convert.ToInt32(fuelList.transaction_number.ToString().Substring(fuelList.transaction_number.ToString().Length - 1));
            }

            if (lastTwoSequence == 0)
            {
                model.fuelLogTableValues.transaction_number = model.SelectedVehicleInfo.VIN + "-" + Convert.ToDateTime(model.fuelLogTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + "1";
            }
            else
            {
                model.fuelLogTableValues.transaction_number = model.SelectedVehicleInfo.VIN + "-" + Convert.ToDateTime(model.fuelLogTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + (lastTwoSequence + 1).ToString();
            }
            model.fuelLogTableValues.employee_id = 10902;
            model.fuelLogTableValues.change_type = "Irving Account";

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

            foreach (object item in items)
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
                        command.Parameters.AddWithValue("@atNumber", itemval[0] + "-" + Convert.ToDateTime(itemval[1]).ToString("yyyyMMdd") + "-1");
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
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.settings_managePermissions == 0)
            {
                return RedirectToAction("Customers", "Settings");
            }

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
                newAccess.library_dashboard = 0;
                newAccess.library_company_Documents = 0;
                newAccess.library_branch_shared_drive = 0;
                newAccess.library_supplier_documents = 0;
                newAccess.library_Management = 0;
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
            if (obj.SehubAccess.app_access == 0)
            {
                obj.appAccess = false;
            }
            else
            {
                obj.appAccess = true;
            }
            if (empDetails.pic_status == 0)
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
                SehubAccessObj.library_dashboard = model.SehubAccess.library_dashboard;
                SehubAccessObj.library_company_Documents = model.SehubAccess.library_company_Documents;
                SehubAccessObj.library_branch_shared_drive = model.SehubAccess.library_branch_shared_drive;
                SehubAccessObj.library_supplier_documents = model.SehubAccess.library_supplier_documents;
                SehubAccessObj.library_Management = model.SehubAccess.library_Management;
                SehubAccessObj.manufacturing_plant = model.SehubAccess.manufacturing_plant;
                SehubAccessObj.user_management = 3;
                SehubAccessObj.customer_reporting = model.SehubAccess.customer_reporting;
                SehubAccessObj.main = model.SehubAccess.main;
                SehubAccessObj.mainDashboard = model.SehubAccess.mainDashboard;
                SehubAccessObj.mainCalendar = model.SehubAccess.mainCalendar;
                SehubAccessObj.treadTracker = model.SehubAccess.treadTracker;
                SehubAccessObj.treadTrackerDashboard = model.SehubAccess.treadTrackerDashboard;
                SehubAccessObj.fleetTVT = model.SehubAccess.fleetTVT;
                SehubAccessObj.fleetTvt_dashboard = model.SehubAccess.fleetTvt_dashboard;
                SehubAccessObj.fleetTvt_EditAccount = model.SehubAccess.fleetTvt_EditAccount;
                SehubAccessObj.fleetTvt_fieldSurvey = model.SehubAccess.fleetTvt_fieldSurvey;
                SehubAccessObj.settings_dashboard = model.SehubAccess.settings_dashboard;
                SehubAccessObj.settings_managePermissions = model.SehubAccess.settings_managePermissions;
                SehubAccessObj.settings_customers = model.SehubAccess.settings_customers;
                SehubAccessObj.settings = model.SehubAccess.settings;
                SehubAccessObj.management = model.SehubAccess.management;
                SehubAccessObj.management_dashboard = model.SehubAccess.management_dashboard;
                SehubAccessObj.tools = model.SehubAccess.tools;
                SehubAccessObj.tools_dashboard = model.SehubAccess.tools_dashboard;
                SehubAccessObj.library_customer_Documents = model.SehubAccess.library_customer_Documents;
                SehubAccessObj.fleetTVT_vier_or_edit_survey = model.SehubAccess.fleetTVT_vier_or_edit_survey;

            }
            if (Empdetails != null)
            {
                Empdetails.rfid_number = model.empDetails.rfid_number;
                Empdetails.loc_ID = model.empDetails.loc_ID;
                if (model.monitorEmployee == true)
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

        [HttpGet]
        public ActionResult CRM(string CustId)
        {

            Debug.WriteLine("In CustomerReporting:" + CustId);
            CustomerReportingViewModel modal = new CustomerReportingViewModel();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if (modal.SehubAccess.customer_reporting == 0)
            {
                return RedirectToAction("Dashboard", "TreadTracker");
            }

            if (CustId == "")
            {
                Debug.WriteLine("Null");
                modal.Customers = PopulateCustomers("Nothing");
            }
            else
            {
                modal.Custname = CustId;
                Debug.WriteLine("Custoomer details:" + CustId);
                modal.Customers = PopulateCustomers(CustId);

                var result = db.tbl_customer_list.Where(a => a.cust_us1.Equals(CustId) && a.cust_us2 == "0").FirstOrDefault();
                if (result != null)
                {
                    modal.customerDetails = result;
                }



                List<tbl_customer_reporting_viewmodel> customerReportingTable = new List<tbl_customer_reporting_viewmodel>();
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select * from tbl_customer_reporting a, tbl_employee b where a.cta_employee_number= b.employee_id and customer_number='" + CustId + "'";
                    Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                tbl_customer_reporting_viewmodel cust_report_tbl = new tbl_customer_reporting_viewmodel();
                                cust_report_tbl.cta_employee_number = Convert.ToInt32(sdr["cta_employee_number"]);
                                cust_report_tbl.visit_type = sdr["visit_type"].ToString();
                                cust_report_tbl.report_type = sdr["report_type"].ToString();
                                cust_report_tbl.customer_name = sdr["customer_name"].ToString();
                                cust_report_tbl.customer_number = sdr["customer_number"].ToString();
                                cust_report_tbl.customer_contact = sdr["customer_contact"].ToString();
                                cust_report_tbl.visit_date = Convert.ToDateTime(sdr["visit_date"]);
                                cust_report_tbl.discusssion_details = sdr["discussion_details"].ToString();
                                cust_report_tbl.remainder = sdr["reminders"].ToString();
                                cust_report_tbl.submission_date = Convert.ToDateTime(sdr["submission_date"]);
                                cust_report_tbl.EmployeeName = sdr["full_name"].ToString();
                                customerReportingTable.Add(cust_report_tbl);

                            }

                        }
                        con.Close();
                    }
                }

                List<tbl_dispute_resolution> disputeResolutionTable = new List<tbl_dispute_resolution>();
                string constr1 = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select * from tbl_dispute_resolution";
                    Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                tbl_dispute_resolution dispute_resolution_tbl = new tbl_dispute_resolution();
                                dispute_resolution_tbl.invoice_number = Convert.ToInt32(sdr["invoice_number"]);
                                dispute_resolution_tbl.date_dispute = Convert.ToDateTime(sdr["date_dispute"]);
                                dispute_resolution_tbl.mileage_dispute = Convert.ToInt32(sdr["mileage_dispute"]);
                                dispute_resolution_tbl.date_service = Convert.ToDateTime(sdr["date_service"]);
                                dispute_resolution_tbl.mileage_service = Convert.ToInt32(sdr["mileage_service"]);
                                dispute_resolution_tbl.customer_number = Convert.ToInt32(sdr["customer_number"]);
                                dispute_resolution_tbl.customer_name_NoAccount = Convert.ToString(sdr["customer_name_NoAccount"]);
                                dispute_resolution_tbl.employee_id = Convert.ToInt32(sdr["employee_id"]);
                                dispute_resolution_tbl.details = Convert.ToString(sdr["details"]);
                                dispute_resolution_tbl.recommandation = Convert.ToString(sdr["recommandation"]);
                                dispute_resolution_tbl.final_outcome = Convert.ToString(sdr["final_outcome"]);
                                dispute_resolution_tbl.complete = Convert.ToInt32(sdr["complete"]);

                                disputeResolutionTable.Add(dispute_resolution_tbl);
                            }

                        }
                        con.Close();
                    }
                }

                List<tbl_target_accounts> targetAccountTable = new List<tbl_target_accounts>();
                string constr2 = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select * from tbl_target_accounts";
                    Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                tbl_target_accounts target_account_tbl = new tbl_target_accounts();
                                target_account_tbl.cta_employee_number = Convert.ToInt32(sdr["cta_employee_number"]);
                                target_account_tbl.visit_type = sdr["visit_type"].ToString();
                                target_account_tbl.customer_contact = sdr["customer_contact"].ToString();
                                target_account_tbl.visit_date = Convert.ToDateTime(sdr["visit_date"]);
                                target_account_tbl.discusssion_details = sdr["discussion_details"].ToString();
                                target_account_tbl.remainder = sdr["reminders"].ToString();
                                target_account_tbl.submission_date = Convert.ToDateTime(sdr["submission_date"]);
                                target_account_tbl.customer_name = sdr["customer_name"].ToString();

                                targetAccountTable.Add(target_account_tbl);
                            }

                        }
                        con.Close();
                    }
                }

                if (customerReportingTable != null)
                {
                    modal.customerReportingDetails = customerReportingTable;
                }

                if (targetAccountTable != null)
                {
                    modal.targetAccountDetails = targetAccountTable;
                }

                if (targetAccountTable != null)
                {
                    modal.DisputeResolutionDetails = disputeResolutionTable;
                }


            }

            return View(modal);
        }


        [HttpPost]
        public ActionResult CRMSelectedCustomer(CustomerReportingViewModel model)
        {
            Debug.WriteLine("Custoomer details:" + model.Custname);
            return RedirectToAction("CRM", new { CustId = model.Custname });

        }
        [HttpPost]
        public ActionResult AddNewCRMDetails(CustomerReportingViewModel model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            Debug.WriteLine("In AddNewCRMDetails");
            Debug.WriteLine(model.customerDetails.cust_us1);
            Debug.WriteLine(model.AddCRM.visit_type);
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_customer_reporting VALUES (@empId, @visittype, 'Call Report', @custName, @custId, @custContact,@visitDate,null,@Comments,@remainders,@submissionDate)");

                    string sql = sb.ToString();
                    Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@empId", empId);
                        command.Parameters.AddWithValue("@visittype", model.AddCRM.visit_type);
                        command.Parameters.AddWithValue("@custId", model.customerDetails.cust_us1);
                        command.Parameters.AddWithValue("@custName", model.customerDetails.cust_name);
                        command.Parameters.AddWithValue("@Comments", model.AddCRM.discusssion_details);
                        command.Parameters.AddWithValue("@remainders", model.AddCRM.remainder);
                        command.Parameters.AddWithValue("@visitDate", model.AddCRM.visit_date.Date);
                        command.Parameters.AddWithValue("@custContact", model.AddCRM.customer_contact);
                        command.Parameters.AddWithValue("@submissionDate", System.DateTime.Today.Date);


                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return RedirectToAction("CRM", new { CustId = "" });
        }
        private static List<SelectListItem> PopulateCustomers(string value)
        {
            Debug.WriteLine("Inside PopulateCustomers with value:" + value);
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select cust_name,cust_us1 FRom tbl_customer_list  where cust_us2='0' order by cust_name";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (value == "Nothing")
                        {
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["cust_name"].ToString(),
                                    Value = sdr["cust_us1"].ToString(),
                                    //Selected = sdr["cust_name"].ToString() == "CITY TIRE AND AUTO CENTRE LTD" ? true : false
                                });
                            }
                        }
                        else
                        {
                            Debug.WriteLine("In else");
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["cust_name"].ToString(),
                                    Value = sdr["cust_us1"].ToString(),
                                    Selected = sdr["cust_name"].ToString() == value ? true : false
                                });
                            }
                        }

                    }
                    con.Close();
                }
            }

            return items;
        }

        [HttpPost]
        public ActionResult AddNewTADetails(CustomerReportingViewModel model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            Debug.WriteLine("In AddNewCRMDetails");
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_target_accounts VALUES (@cta_employee_number, @visittype, 'customer_name', @customer_contact, @visit_date, @discussion_details,@reminders,@submission_date)");

                    string sql = sb.ToString();
                    Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@cta_employee_number", empId);
                        command.Parameters.AddWithValue("@visittype", model.AddTA.visit_type);
                        command.Parameters.AddWithValue("@customer_contact", model.AddTA.customer_contact);
                        command.Parameters.AddWithValue("@visit_date", model.AddTA.visit_date);
                        command.Parameters.AddWithValue("@discussion_details", model.AddTA.discusssion_details);
                        command.Parameters.AddWithValue("@reminders", model.AddTA.remainder);
                        command.Parameters.AddWithValue("@submission_date", System.DateTime.Today.Date);


                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return RedirectToAction("CustomerReporting", new { CustId = "" });
        }


        [HttpPost]
        public ActionResult AddNewDRDetails(CustomerReportingViewModel model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            Debug.WriteLine("In AddNewCRMDetails");
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_dispute_resolution VALUES (@invoice_number, @date_dispute, @mileage_dispute, @date_service, @mileage_service, @customer_number,@customer_name_NoAccount,@employee_id,@details,@recommandation,null ,@final_outcome, @complete)");

                    string sql = sb.ToString();
                    Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@invoice_number", model.AddDR.invoice_number);
                        command.Parameters.AddWithValue("@date_dispute", model.AddDR.date_dispute);
                        command.Parameters.AddWithValue("@mileage_dispute", model.AddDR.mileage_dispute);
                        command.Parameters.AddWithValue("@date_service", model.AddDR.date_service);
                        command.Parameters.AddWithValue("@mileage_service", model.AddDR.mileage_service);
                        command.Parameters.AddWithValue("@customer_number", model.AddDR.customer_number);
                        command.Parameters.AddWithValue("@customer_name_NoAccount", model.AddDR.customer_name_NoAccount);
                        command.Parameters.AddWithValue("@employee_id", empId);
                        command.Parameters.AddWithValue("@details", model.AddDR.details);
                        command.Parameters.AddWithValue("@recommandation", model.AddDR.recommandation);
                        //command.Parameters.AddWithValue("@attachments", model.AddDR.invoice_number);
                        command.Parameters.AddWithValue("@final_outcome", model.AddDR.invoice_number);
                        command.Parameters.AddWithValue("@complete", 1);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return RedirectToAction("CustomerReporting", new { CustId = "" });
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

        private static List<SelectListItem> populateVehicles(string location, string vehicle)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var veh = db.tbl_vehicle_info.Where(x => x.loc_id == location && x.vehicle_short_id != vehicle && x.vehicle_status == 1).ToList();

            foreach (var val in veh)
            {
                items.Add(new SelectListItem
                {
                    Text = val.vehicle_short_id,
                    Value = Convert.ToString(val.VIN)
                });
            }
            return items;
        }

    }
}