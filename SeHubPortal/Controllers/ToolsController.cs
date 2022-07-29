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
using Newtonsoft.Json.Linq;
using System.Net.Mail;


namespace SeHubPortal.Controllers
{
    public class ToolsController : Controller
    {
        public string Current_sequence;
        public string Loc_ID;
        int NewSequenceNum;
        public string locId;

        [HttpPost]
        public ActionResult DashboardChangeLocation(FileURL model)
        {
            return RedirectToAction("Dashboard", new { loc = model.Location_ID });
        }

        public ActionResult Dashboard(tbl_tire_adjustment model, string loc)
        {

            if (Session["userID"] == null)
            {
                Session["userID"] = 61000;
            }

            tbl_sehub_access Access = new tbl_sehub_access();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empId = Convert.ToInt32(Session["userID"].ToString());

            Access = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            ViewBag.TypeSelectionList = populateLocationsPermissions(empId);

            if (Access.tools == 0)
            {
                return RedirectToAction("Dashboard", "TreadTracker");
            }

            if (Access.tools_dashboard == 0)
            {
                return RedirectToAction("FuelLog", "Tools");
            }


            ViewData["usrid"] = empId;

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
            ViewData["plant"] = Access.plant;
            ViewData["UserEmail"] = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault();


            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string locatId = empDetails.loc_ID;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 * from tbl_tire_adjustment where Tire_Adjustment_id like '%" + locatId + "%' order by Tire_Adjustment_id desc";
                //Debug.WriteLine(query);
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

                                //Debug.WriteLine(Current_sequence.Substring(7));
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
                //Debug.WriteLine(query);
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

                            if (loc != null)
                            {
                                model.Tire_Adjustment_id = "TA-" + loc + "XXXXX";
                            }
                            else
                            {
                                model.Tire_Adjustment_id = "TA-" + locId + "XXXXX";
                            }
                            //Debug.WriteLine("test1");
                            //Debug.WriteLine(model.Tire_Adjustment_id);
                        }

                    }
                    con.Close();
                }

            }

            

            if (loc != null)
            {
                model.location_id = loc;
            }
            else
            {
                model.location_id = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            var locDetatils = db.tbl_cta_location_info.Where(x => x.loc_id == model.location_id).FirstOrDefault();

            if (locDetatils != null)
            {
                model.non_sig_number = db.tbl_cta_location_info.Where(x => x.loc_id == model.location_id).Select(x => x.nonsig_num).FirstOrDefault();

                ViewBag.locID = locDetatils.loc_id;
                ViewBag.addressSTREET1 = locDetatils.cta_street1;
                ViewBag.addressSTREET2 = locDetatils.cta_street2;
                ViewBag.addressCITY = locDetatils.cta_city;
                ViewBag.addressPROVINCE = locDetatils.cta_province;
                ViewBag.addressPOSTAL = locDetatils.cta_postal_code;
                ViewBag.locPHONE = locDetatils.cta_phone;
                ViewBag.locFAX = locDetatils.cta_fax;
            }

            return View(model);
        }

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
            //Debug.WriteLine(value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var parts = db.tbl_calculator_GM_parts.ToList();

            double retail_price = 0;
            double N_A_price = 0;

            for (int i = 0; i < parts.Count; i++)
            {

                if (parts[i].minimum_cost <= value && value <= parts[i].maximum_cost)
                {
                    //Trace.WriteLine("Reached and present in table");
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

                        //Debug.WriteLine(Tire[i].minimum_cost + " LowerBound " + value + " UpperBound " + Tire[i].maximum_cost + "and the GM is " + Tire[i].GrossMargin);

                    }
                }
            }



            return (Math.Round(retail_price, 2), Math.Round(N_A_price, 2));
        }

        [HttpPost]
        public (double, double) WheelPricing(double value)
        {
            //Debug.WriteLine(value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var Wheel = db.tbl_calculator_GP_fixed.Where(x => x.category == "Wheels, Rims & Lugs").FirstOrDefault();

            double retail_price = 0;
            double N_A_price = 0;

            retail_price = Convert.ToDouble(value + ((Wheel.retail / 100) * value));
            N_A_price = Convert.ToDouble(value + ((Wheel.national_account / 100) * value));

            return (Math.Round(retail_price, 2), Math.Round(N_A_price, 2));
        }

        [HttpPost]
        public (double, double) FreightPricing(double value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var Freight = db.tbl_calculator_GP_fixed.Where(x => x.category == "Freight").FirstOrDefault();

            double retail_price = 0;
            double N_A_price = 0;

            retail_price = Convert.ToDouble(value + ((Freight.retail / 100) * value));
            N_A_price = Convert.ToDouble(value + ((Freight.national_account / 100) * value));

            return (Math.Round(retail_price, 2), Math.Round(N_A_price, 2));
        }

        [HttpPost]
        public (double, double) SCSPricing(double value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var SCS = db.tbl_calculator_GP_fixed.Where(x => x.category == "subcontracted").FirstOrDefault();

            double retail_price = 0;
            double N_A_price = 0;

            retail_price = Convert.ToDouble(value + ((SCS.retail / 100) * value));
            N_A_price = Convert.ToDouble(value + ((SCS.national_account / 100) * value));

            return (Math.Round(retail_price, 2), Math.Round(N_A_price, 2));
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

            var vehicalsList = db.tbl_vehicle_info.Where(x => x.loc_id == model.MatchedLocID && x.vehicle_status == 1).FirstOrDefault();

            if (vehicalsList != null)
            {
                return RedirectToAction("FuelLog", new { VIN = vehicalsList.VIN, loc = model.MatchedLocID });
            }
            else
            {
                return RedirectToAction("FuelLog", new { VIN = "NoSelectedVehicle", loc = model.MatchedLocID });
            }

        }

        [HttpPost]
        public ActionResult SaveExpenseClaim(ExpenseClaimViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            Trace.WriteLine(model.Claim1.exp_date + model.Claim1.exp_description);

            SequenceNumberAction(model.empid, "2021-07-15");

            if (model.Claim1.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim1;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim1.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim2.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim2;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim2.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim3.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim3;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim3.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim4.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim4;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim4.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim5.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim5;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim5.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim6.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim6;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim6.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim7.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim7;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim7.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim8.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim8;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim8.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim9.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim9;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim9.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            }
            if (model.Claim10.exp_date != null)
            {
                tbl_expense_claim claim = new tbl_expense_claim();
                claim = model.Claim10;
                claim.transaction_number = model.empid + "-" + claim.exp_date.Value.ToString("yyyy-MM-dd") + "-" + SequenceNumberAction(model.empid, model.Claim10.exp_date.Value.ToString("yyyy-MM-dd"));
                claim.empid = model.empid;
                db.tbl_expense_claim.Add(claim);
                db.SaveChanges();
            } 
            

            return RedirectToAction("ExpnseClaim");
        }
        
        public string SequenceNumberAction(string empid, string date)
        {

            Trace.WriteLine("These are the inputs " + empid + date);

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 * from tbl_expense_claim where transaction_number like '" + empid + "-" + date + "%' order by transaction_number desc ";
                Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            string ClaimTN = Convert.ToString(sdr["transaction_number"]);
                            string NewTN = ClaimTN.Substring(17);

                            if(ClaimTN == null)
                            {
                                return "0";
                            }
                            else
                            {
                                return (Convert.ToInt32(NewTN) + 1).ToString();
                            }

                        }
                    }
                    con.Close();
                }
                return "0";
            }

        }

        public ActionResult ExpnseClaim(ExpenseClaimViewModel model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            
            model.SehubAccess = empDetails;
            model.Locations = populateLocationsExp();
            model.Acounts = populateExpAccount();
            model.empid = empId.ToString();
            model.employy = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            model.ExpClaims = populateExpClaims();

            model.seq = SequenceNumberAction(model.empid, System.DateTime.Today.ToString("yyyy-MM-dd"));

            return View(model);
        }

        public ActionResult ChangeVehicalFuelLog(string vin, string loc)
        {
            return RedirectToAction("FuelLog", new { VIN = vin, loc = loc });
        }

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
                    string query = "select top 1 * from tbl_tire_adjustment where location_id = '" + locId + "' order by Tire_Adjustment_id desc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {

                                Current_sequence = Convert.ToString(sdr["Tire_Adjustment_id"]);
                                //Debug.WriteLine(Current_sequence);
                                if (Current_sequence != null)
                                {

                                    //Debug.WriteLine(Current_sequence.Substring(7));
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

                Trace.WriteLine(data.Tire_Adjustment_id);

                data.date = System.DateTime.Today.Date;
                data.cta_employee_name = empDetails.full_name;
                data.non_sig_number = model.non_sig_number;
                data.location_id = empDetails.loc_ID;
                data.replacement_invoice = model.replacement_invoice;

                //Debug.WriteLine("********************************This is the name" + model.name);

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

                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress("service" + locId + "@citytire", "IT Team")); //jordan.blackwood      harsha.yerramsetty      payroll
                msg.From = new MailAddress("noreply@citytire.com", "Sehub");
                msg.Subject = "Tire Adjustment : " + data.Tire_Adjustment_id;
                msg.Body = "<i><u><b>Tire Adjustment Submission</b></u></i>" +
                    "<br /><br />" +
                    "Tire Adjustment Number: &nbsp &nbsp &nbsp" + "<font color='black'>" + data.Tire_Adjustment_id + "</font>" + "<br />" +
                    "Customer Name: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp " + "<font color='black'>" + data.name + "</font>" + " <br />" +
                    "Invoice Number: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp " + "<font color='black'>" + data.replacement_invoice + "</font>" + " <br />" +
                    "<i><b><font size=1>SEHUB Automated Email Notifications</font></b></i>";

                //"There is a branch to corporate payroll submission from " + locId + " for the pay period " + "20" + payrollId.ToString().Substring(0, 2) + "-" + payrollId.ToString().Substring(payrollId.ToString().Length - 2) + "<br /> Location IDs which have submitted  till now from branch to corporate are : " + locationNumber + " for payroll ID : " + "20" + payrollId.ToString().Substring(0, 2) + "-" + payrollId.ToString().Substring(payrollId.ToString().Length - 2);
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
                    //Debug.WriteLine("Message Sent Succesfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }


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

            //Trace.WriteLine("Reached till here");

            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string locId = empDetails.loc_ID;
            string cta_emp_name = empDetails.full_name;
            //Debug.Write("Reached");
            var TADetails = JObject.Parse(value);

            //Debug.Write(TADetails["replacement_invoice"]);

            string TAID = Convert.ToString(TADetails["Tire_Adjustment_id"]);

            var present = db.tbl_tire_adjustment.Where(x => x.Tire_Adjustment_id == TAID).FirstOrDefault();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select top 1 * from tbl_tire_adjustment where Tire_Adjustment_id like '%TA-" + locId + "%' order by Tire_Adjustment_id desc";
                //Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            Current_sequence = Convert.ToString(sdr["Tire_Adjustment_id"]);
                            //Debug.WriteLine(Current_sequence);
                            if (Current_sequence != null)
                            {

                                //Debug.WriteLine(Current_sequence.Substring(7));
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

            //Trace.WriteLine("Reached till here 2 ");

            tbl_tire_adjustment data = new tbl_tire_adjustment();

            data.Tire_Adjustment_id = "TA-" + locId + NewSequenceNum.ToString("D5");

            //Trace.WriteLine("Reached till here " + data.Tire_Adjustment_id);

            data.date = System.DateTime.Today.Date;
            data.cta_employee_name = Convert.ToString(TADetails["cta_employee_name"]);
            data.non_sig_number = Convert.ToString(TADetails["non_sig_number"]);
            data.location_id = Convert.ToString(TADetails["location_id"]);
            data.replacement_invoice = Convert.ToString(TADetails["replacement_invoice"]);

            //Trace.WriteLine("Reached till here 3 ");

            data.name = Convert.ToString(TADetails["name"]);
            data.phone = Convert.ToString(TADetails["phone"]);
            data.street = Convert.ToString(TADetails["street"]);
            data.city = Convert.ToString(TADetails["city"]);
            data.province = Convert.ToString(TADetails["province"]);
            data.postal_code = Convert.ToString(TADetails["postal_code"]);
            data.brand = Convert.ToString(TADetails["brand"]);
            data.tread = Convert.ToString(TADetails["tread"]);
            data.size = Convert.ToString(TADetails["size"]);

            //Trace.WriteLine("Reached till here 4 ");

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

            //Trace.WriteLine("Reached till here 5 ");

            db.tbl_tire_adjustment.Add(data);
            db.SaveChanges();

            return data.Tire_Adjustment_id;
        }

        [HttpPost]
        public string CheckPlantCodes(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var plantCode = db.tbl_source_DOT_plantcodes.Where(x => x.code == value).Select(x => x.DefaultSelection).FirstOrDefault();

            if(plantCode != null)
            {
                return plantCode;
            }
            else
            {
                return " ";
            }

            
        }

        [HttpPost]
        public string getDropdownOptions(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var plantCode = db.tbl_source_DOT_plantcodes.Where(x => x.code == value).Select(x => x.DefaultSelection).FirstOrDefault();

            var dropdownItems = db.tbl_source_DOT_plantcodes.Where(x => x.DefaultSelection == plantCode && x.Manufacturerlist != null).Select(x => x.Manufacturerlist).FirstOrDefault();

            if (dropdownItems != null)
            {
                return dropdownItems;
            }
            else
            {
                return " ";
            }


        }

        [HttpPost]
        public string CheckSizeCodes(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var sizeCode = db.tbl_source_DOT_sizecodes.Where(x => x.code == value).Select(x => x.size).FirstOrDefault();

            if(sizeCode != null)
            {
                return sizeCode;
            }
            else
            {
                return " ";
            }

            
        }

        public ActionResult ExpenseClaim()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            string userEmailID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault();

            MailMessage email = new MailMessage();

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(userEmailID, "IT Team")); //jordan.blackwood     harsha.yerramsetty     payroll
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.Subject = "Expense Claim";
            msg.Body = "";
            msg.IsBodyHtml = true;

            string path1 = Path.Combine(Server.MapPath("~/Content/ExpenseClaimXL.XLSM"));
            msg.Attachments.Add(new Attachment(path1));

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
                //Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }


            return RedirectToAction("Dashboard", "Tools");
        }

        public ActionResult FuelLog(FuelLogViewModel model, string loc, string VIN)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (loc == null || loc == "")
            {
                loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

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

            if (VIN == null || VIN == "")
            {
                VIN = db.tbl_vehicle_info.Where(x => x.loc_id == loc && x.vehicle_status == 1).Select(x => x.VIN).FirstOrDefault();
                
            }

            model.selectedVIN = VIN;
            model.SelectedVehicleInfo = db.tbl_vehicle_info.Where(x => x.VIN == VIN).FirstOrDefault();
            model.fuelLogList = db.tbl_fuel_log_fleet.Where(x => x.VIN == model.selectedVIN).OrderByDescending(x => x.date_of_purchase).ToList();

            List<tbl_vehicle_info> VehicleDetails = new List<tbl_vehicle_info>();
            if (loc != null && loc != "")
            {
                VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == loc && x.vehicle_status == 1).OrderBy(x => x.vehicle_short_id).ToList();
            }
            else
            {
                VehicleDetails = db.tbl_vehicle_info.Where(x => x.loc_id == locationid && x.vehicle_status == 1).OrderBy(x => x.vehicle_short_id).ToList();
            }

            if(VIN != null)
            {
                model.MatchedLocs = populateLocationsPermissions(empId);
                var empDetail = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

                if (loc != null && loc != "")
                {
                    model.MatchedLocID = loc;
                }
                else
                {
                    model.MatchedLocID = empDetail.loc_ID;
                }
            }

            model.vehicleInfoList = VehicleDetails.ToList();
            return View(model);
        }

        [HttpGet]
        public ActionResult FuelLogMainPage()
        {

            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            //Debug.WriteLine("In AssetControl");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            FuelLogViewModel model = new FuelLogViewModel();

            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return View(model);

        }

        [HttpGet]
        public ActionResult FuelInvoice(FuelInvoiceViewModel modal)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;
            modal.LocationsList = populateLocationsPermissions(empId);

            if (modal.MatchedLocation != null)
            {

            }
            else
            {
                modal.MatchedLocation = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            modal.fuelLogInvoicedList = db.tbl_fuel_log_invoiced.Where(x => x.loc_id == modal.MatchedLocation).ToList();

            

            return PartialView(modal);
        }

        [HttpGet]
        public ActionResult ShopSupplies(ShopSuppliesViewModel modal)
        {

            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if (modal.MatchedLocation != null)
            {

            }
            else
            {
                modal.MatchedLocation = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            modal.fuelLogShopSuppliesList = db.tbl_fuel_log_shopSupplies.Where(x => x.transaction_number.Contains(modal.MatchedLocation)).ToList();
            modal.LocationsList = populateLocationsPermissions(empId);
            
            

            return PartialView(modal);
        }

        [HttpPost]
        public ActionResult SubmitAuditStatus(FuelLogViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            foreach (var items in model.fuelLogList)
            {
                var result = db.tbl_fuel_log_fleet.Where(a => a.transaction_number.Equals(items.transaction_number)).FirstOrDefault();
                if (result != null)
                {
                    //Debug.WriteLine("items.audit_status:" + items.audit_status);
                    result.audit_status = items.audit_status;
                }
                else
                {
                    //Debug.WriteLine("Null");
                }
            }
            db.SaveChanges();
            return RedirectToAction("FuelLog", new { VIN = model.SelectedVehicleInfo.VIN });
        }

        [HttpGet]
        public ActionResult DeleteFuelTransaction(string value)
        {

            Trace.WriteLine("In EditVehicleInfo:" + value);
            tbl_fuel_log_fleet fuelLog = new tbl_fuel_log_fleet();
            fuelLog.transaction_number = value;
            return PartialView(fuelLog);
        }

        [HttpPost]
        public ActionResult DeleteFuelTransaction(FuelLogViewModel model)
        {
            string transcationNumber = model.deleteTransactionNumber;
            string VinNumber = model.deleteTransactionNumber.ToString().Split('-')[0];

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var fuelInfo = db.tbl_fuel_log_fleet.Where(x => x.transaction_number == transcationNumber).FirstOrDefault();

            if (fuelInfo != null)
            {
                db.tbl_fuel_log_fleet.Remove(fuelInfo);
            }
            db.SaveChanges();
            //Debug.WriteLine("Vin Number:" + VinNumber);
            return RedirectToAction("FuelLog", new { VIN = VinNumber });

        }

        [HttpPost]
        public ActionResult DeleteFuelTransactionInvoice(FuelInvoiceViewModel model)
        {
            string transcationNumber = model.deleteTransactionNumber;
            string VinNumber = model.deleteTransactionNumber.ToString().Split('-')[0];

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var fuelInfo = db.tbl_fuel_log_invoiced.Where(x => x.transaction_number == transcationNumber).FirstOrDefault();

            if (fuelInfo != null)
            {
                db.tbl_fuel_log_invoiced.Remove(fuelInfo);
            }
            db.SaveChanges();
            //Debug.WriteLine("Vin Number:" + VinNumber);
            return RedirectToAction("FuelLog", new { ac = "Invoice" });

        }

        [HttpPost]
        public ActionResult DeleteFuelTransactionShop(ShopSuppliesViewModel model)
        {
            string transcationNumber = model.deleteTransactionNumber;
            string VinNumber = model.deleteTransactionNumber.ToString().Split('-')[0];

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var fuelInfo = db.tbl_fuel_log_shopSupplies.Where(x => x.transaction_number == transcationNumber).FirstOrDefault();

            if (fuelInfo != null)
            {
                db.tbl_fuel_log_shopSupplies.Remove(fuelInfo);
            }
            db.SaveChanges();
            //Debug.WriteLine("Vin Number:" + VinNumber);
            return RedirectToAction("FuelLog", new { ac = "Shop" });

        }

        [HttpPost]
        public ActionResult SaveFuelReceipt(FuelLogViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            //Debug.WriteLine(model.SelectedVehicleInfo.VIN);
            //Debug.WriteLine(model.fuelLogTableValues.no_of_liters);
            model.fuelLogTableValues.VIN = model.SelectedVehicleInfo.VIN;
            model.fuelLogTableValues.employee_id = model.SelectedVehicleInfo.assigned_to;

            string transactionReceiptNumber = Convert.ToDateTime(model.fuelLogTableValues.date_of_purchase).ToString("yyyyMMdd");
            var fuelList = db.tbl_fuel_log_fleet.Where(x => x.VIN == model.SelectedVehicleInfo.VIN && x.transaction_number.Contains(model.SelectedVehicleInfo.VIN + "-" + transactionReceiptNumber)).OrderByDescending(x => x.transaction_number).FirstOrDefault();

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

            model.fuelLogTableValues.audit_status = false;
            db.tbl_fuel_log_fleet.Add(model.fuelLogTableValues);

            db.SaveChanges();

            return RedirectToAction("FuelLog", new { VIN = model.SelectedVehicleInfo.VIN });
        }

        [HttpPost]
        public ActionResult EditFuelReceipt(FuelLogViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var reciept = db.tbl_fuel_log_fleet.Where(x => x.VIN == model.selectedVIN && x.transaction_number == model.editFuelLogTableValues.transaction_number).FirstOrDefault();
            if (reciept != null)
            {
                Trace.WriteLine("Reached Edit recipt");
                reciept.employee_id = empId;
                reciept.odometer = model.editFuelLogTableValues.odometer;
                reciept.no_of_liters = model.editFuelLogTableValues.no_of_liters;
                reciept.price_per_liter = model.editFuelLogTableValues.price_per_liter;
                reciept.change_type = model.editFuelLogTableValues.change_type;
                reciept.comments = model.editFuelLogTableValues.comments;
                db.SaveChanges();
            }
            return RedirectToAction("FuelLog", new { VIN = model.selectedVIN });
        }

        [HttpPost]
        public ActionResult EditFuelReceiptInvoice(FuelInvoiceViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var reciept = db.tbl_fuel_log_invoiced.Where(x => x.transaction_number == model.EditfuelLogInvoiceTableValues.transaction_number).FirstOrDefault();
            if (reciept != null)
            {
                Trace.WriteLine("Reached Edit recipt");
                reciept.employee_id = empId;
                reciept.invoice_number = model.EditfuelLogInvoiceTableValues.invoice_number;
                reciept.no_of_liters = model.EditfuelLogInvoiceTableValues.no_of_liters;
                reciept.price_per_liter = model.EditfuelLogInvoiceTableValues.price_per_liter;
                reciept.change_type = model.EditfuelLogInvoiceTableValues.change_type;
                reciept.comments = model.EditfuelLogInvoiceTableValues.comments;
                db.SaveChanges();
            }
            return RedirectToAction("FuelLog", new { ac = "Invoice" });
        }

        [HttpPost]
        public ActionResult EditFuelReceiptShop(ShopSuppliesViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var reciept = db.tbl_fuel_log_shopSupplies.Where(x => x.transaction_number == model.editFuelLogShopSuppliesTableValues.transaction_number).FirstOrDefault();
            if (reciept != null)
            {
                Trace.WriteLine("Reached Edit recipt");
                reciept.employee_id = empId;
                reciept.no_of_liters = model.editFuelLogShopSuppliesTableValues.no_of_liters;
                reciept.price_per_liter = model.editFuelLogShopSuppliesTableValues.price_per_liter;
                reciept.change_type = model.editFuelLogShopSuppliesTableValues.change_type;
                reciept.comments = model.editFuelLogShopSuppliesTableValues.comments;
                db.SaveChanges();
            }
            return RedirectToAction("FuelLog", new { ac = "Shop" });
        }

        [HttpPost]
        public ActionResult SaveFuelInvoiceReceipt(FuelInvoiceViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            string transactionReceiptNumber = Convert.ToDateTime(model.fuelLogInvoiceTableValues.date_of_purchase).ToString("yyyyMMdd");
            var fuelInvoiceList = db.tbl_fuel_log_invoiced.Where(x => x.invoice_number == model.fuelLogInvoiceTableValues.invoice_number && x.transaction_number.Contains(model.fuelLogInvoiceTableValues.invoice_number + "-" + transactionReceiptNumber)).OrderByDescending(x => x.transaction_number).FirstOrDefault();

            int lastTwoSequence = 0;
            if (fuelInvoiceList != null)
            {
                lastTwoSequence = Convert.ToInt32(fuelInvoiceList.transaction_number.ToString().Substring(fuelInvoiceList.transaction_number.ToString().Length - 1));
            }

            if (lastTwoSequence == 0)
            {
                model.fuelLogInvoiceTableValues.transaction_number = model.fuelLogInvoiceTableValues.invoice_number + "-" + Convert.ToDateTime(model.fuelLogInvoiceTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + "1";
            }
            else
            {
                model.fuelLogInvoiceTableValues.transaction_number = model.fuelLogInvoiceTableValues.invoice_number + "-" + Convert.ToDateTime(model.fuelLogInvoiceTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + (lastTwoSequence + 1).ToString();
            }

            model.fuelLogInvoiceTableValues.loc_id = model.MatchedLocation;
            model.fuelLogInvoiceTableValues.audit_status = false;
            model.fuelLogInvoiceTableValues.employee_id = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.employee_id).FirstOrDefault();
            db.tbl_fuel_log_invoiced.Add(model.fuelLogInvoiceTableValues);

            db.SaveChanges();

            return RedirectToAction("FuelLog");
        }

        [HttpPost]
        public ActionResult SaveFuelShopSuppliesReceipt(ShopSuppliesViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            string transactionReceiptNumber = Convert.ToDateTime(model.fuelLogShopSuppliesTableValues.date_of_purchase).ToString("yyyyMMdd");
            var fuelShopSuppliesList = db.tbl_fuel_log_shopSupplies.Where(x => x.date_of_purchase == model.fuelLogShopSuppliesTableValues.date_of_purchase && x.transaction_number.Contains(model.MatchedLocation + "-" + transactionReceiptNumber)).OrderByDescending(x => x.transaction_number).FirstOrDefault();

            int lastTwoSequence = 0;
            if (fuelShopSuppliesList != null)
            {
                lastTwoSequence = Convert.ToInt32(fuelShopSuppliesList.transaction_number.ToString().Substring(fuelShopSuppliesList.transaction_number.ToString().Length - 1));
                
            }

            if (lastTwoSequence == 0)
            {
                model.fuelLogShopSuppliesTableValues.transaction_number = model.MatchedLocation + "-" + Convert.ToDateTime(model.fuelLogShopSuppliesTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + "1";
                
            }
            else
            {
                model.fuelLogShopSuppliesTableValues.transaction_number = model.MatchedLocation + "-" + Convert.ToDateTime(model.fuelLogShopSuppliesTableValues.date_of_purchase).ToString("yyyyMMdd") + "-" + (lastTwoSequence + 1).ToString();
                
            }

            model.fuelLogShopSuppliesTableValues.audit_status = false;
            model.fuelLogShopSuppliesTableValues.employee_id = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.employee_id).FirstOrDefault();
            db.tbl_fuel_log_shopSupplies.Add(model.fuelLogShopSuppliesTableValues);

            db.SaveChanges();

            return RedirectToAction("FuelLog");
        }

        public ActionResult DeleteTransaction(string id)
        {
            //Debug.WriteLine("Value to be delated:" + id);
            return RedirectToAction("FuelLog", new { VIN = id.Split(';')[1] });
        }

        public void updateFuelLogTable()
        {
            List<string> items = new List<string>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select VIN,date_of_purchase From tbl_fuel_log_fleet";
                //Debug.WriteLine("Query:" + query);
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
                //Debug.WriteLine("value:" + item);
                string[] itemval = item.ToString().Split(';');
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE ");
                    sb.Append("tbl_fuel_log_fleet ");
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

            int empId = Convert.ToInt32(Session["userID"].ToString());

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            EmployeePermissionsViewModel model = new EmployeePermissionsViewModel();
            model.userManagementAccessLevel = permissions;
            string location = "";

            if (locId is null || locId == "")
            {
                location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
                model.MatchedLocID = location;
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
            var employeeList = db.tbl_employee.Where(x => (x.loc_ID.Contains(location) && x.status == 1) || x.status == null ).OrderBy(x => x.employee_id).ToList();
            model.EmployeesList = employeeList;
            model.MatchedLocs = populateLocationsPermissions(empId);
            
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

            //Debug.WriteLine("In ManageEmployeePermisssions:" + value);
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
                newAccess.employee_folder = 0;
                newAccess.settings = 0;
                newAccess.settings_managePermissions = 0;
                newAccess.settings_administration = 0;
                newAccess.treadTracker_customers = 0;

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

            if (obj.SehubAccess.loc_001 == 1)
            {
                obj.loc_001 = true;
            }
            else
            {
                obj.loc_001 = false;
            }
            if (obj.SehubAccess.loc_002 == 1)
            {
                obj.loc_002 = true;
            }
            else
            {
                obj.loc_002 = false;
            }
            if (obj.SehubAccess.loc_003 == 1)
            {
                obj.loc_003 = true;
            }
            else
            {
                obj.loc_003 = false;
            }
            if (obj.SehubAccess.loc_004 == 1)
            {
                obj.loc_004 = true;
            }
            else
            {
                obj.loc_004 = false;
            }
            if (obj.SehubAccess.loc_005 == 1)
            {
                obj.loc_005 = true;
            }
            else
            {
                obj.loc_005 = false;
            }
            if (obj.SehubAccess.loc_007 == 1)
            {
                obj.loc_007 = true;
            }
            else
            {
                obj.loc_007 = false;
            }
            if (obj.SehubAccess.loc_009 == 1)
            {
                obj.loc_009 = true;
            }
            else
            {
                obj.loc_009 = false;
            }
            if (obj.SehubAccess.loc_010 == 1)
            {
                obj.loc_010 = true;
            }
            else
            {
                obj.loc_010 = false;
            }
            if (obj.SehubAccess.loc_011 == 1)
            {
                obj.loc_011 = true;
            }
            else
            {
                obj.loc_011 = false;
            }
            if (obj.SehubAccess.loc_347 == 1)
            {
                obj.loc_347 = true;
            }
            else
            {
                obj.loc_347 = false;
            }
            if (obj.SehubAccess.loc_AHO == 1)
            {
                obj.loc_AHO = true;
            }
            else
            {
                obj.loc_AHO = false;
            }

            obj.empDetails = empDetails;
            return PartialView(obj);
        }

        [HttpPost]
        public ActionResult ManageEmployeePermisssions(ModifyEmployeePermissions model)
        {
            //Debug.WriteLine("App access:" + model.SehubAccess.app_access);
            //Debug.WriteLine("Library access:" + model.SehubAccess.library_access);
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

                if (model.loc_001 == true)
                {
                    SehubAccessObj.loc_001 = 1;
                }
                else
                {
                    SehubAccessObj.loc_001 = 0;
                }

                if (model.loc_002 == true)
                {
                    SehubAccessObj.loc_002 = 1;
                }
                else
                {
                    SehubAccessObj.loc_002 = 0;
                }

                if (model.loc_003 == true)
                {
                    SehubAccessObj.loc_003 = 1;
                }
                else
                {
                    SehubAccessObj.loc_003 = 0;
                }

                if (model.loc_004 == true)
                {
                    SehubAccessObj.loc_004 = 1;
                }
                else
                {
                    SehubAccessObj.loc_004 = 0;
                }

                if (model.loc_005 == true)
                {
                    SehubAccessObj.loc_005 = 1;
                }
                else
                {
                    SehubAccessObj.loc_005 = 0;
                }

                if (model.loc_007 == true)
                {
                    SehubAccessObj.loc_007 = 1;
                }
                else
                {
                    SehubAccessObj.loc_007 = 0;
                }

                if (model.loc_009 == true)
                {
                    SehubAccessObj.loc_009 = 1;
                }
                else
                {
                    SehubAccessObj.loc_009 = 0;
                }

                if (model.loc_010 == true)
                {
                    SehubAccessObj.loc_010 = 1;
                }
                else
                {
                    SehubAccessObj.loc_010 = 0;
                }

                if (model.loc_011 == true)
                {
                    SehubAccessObj.loc_011 = 1;
                }
                else
                {
                    SehubAccessObj.loc_011 = 0;
                }

                if (model.loc_347 == true)
                {
                    SehubAccessObj.loc_347 = 1;
                }
                else
                {
                    SehubAccessObj.loc_347 = 0;
                }

                if (model.loc_AHO == true)
                {
                    SehubAccessObj.loc_AHO = 1;
                }
                else
                {
                    SehubAccessObj.loc_AHO = 0;
                }



                SehubAccessObj.dashboard = model.SehubAccess.dashboard;
                SehubAccessObj.calendar = model.SehubAccess.calendar;
                SehubAccessObj.newsletter = model.SehubAccess.newsletter;
                SehubAccessObj.treadTracker_failure_analysis = model.SehubAccess.treadTracker_failure_analysis;
                SehubAccessObj.treadTracker_production_report = model.SehubAccess.treadTracker_production_report;
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
                SehubAccessObj.fleetTvt_snapshot = model.SehubAccess.fleetTvt_snapshot;
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
                SehubAccessObj.employee_folder = model.SehubAccess.employee_folder;
                SehubAccessObj.settings_administration = model.SehubAccess.settings_administration;
                SehubAccessObj.treadTracker_customers = model.SehubAccess.treadTracker_customers;

            }
            if (Empdetails != null)
            {
                Empdetails.rfid_number = model.empDetails.rfid_number;
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
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            //Debug.WriteLine("In CustomerReporting:" + CustId);
            CustomerReportingViewModel modal = new CustomerReportingViewModel();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //modal.emp_tbl = db.tbl_employee.ToList();

            int empId = Convert.ToInt32(Session["userID"].ToString());

            modal.username = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if (modal.SehubAccess.customer_reporting == 0)
            {
                return RedirectToAction("Dashboard", "TreadTracker");
            }

            modal.Customers = PopulateCustomers("Nothing");
            

            if (CustId == null)
            {
                CustId = modal.Customers.OrderBy(x => x.Text).Select(x => x.Value).FirstOrDefault();
            }


            if (CustId == "")
            {
                //Debug.WriteLine("Null");
                
            }
            else
            {
                modal.Custname = CustId;
                //Debug.WriteLine("Custoomer details:" + CustId);
                modal.Customers = PopulateCustomers(CustId);

                var result = db.tbl_cta_customers.Where(a => a.CustomerCode.ToString() == CustId).FirstOrDefault();
                if (result != null)
                {
                    modal.customerDetails = result;
                }
                List<tbl_customer_reporting_viewmodel> customerReportingTable = new List<tbl_customer_reporting_viewmodel>();
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select * from tbl_customer_reporting a, tbl_employee b where a.cta_employee_number= b.employee_id and customer_number='" + CustId + "' order by visit_date DESC";
                    //Debug.WriteLine(query);
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

                if (customerReportingTable != null)
                {
                    modal.customerReportingDetails = customerReportingTable;
                }

            }

            return View(modal);
        }

        [HttpPost]
        public ActionResult ChangeLocTargetAccount(CustomerReportingViewModel model)
        {
            return RedirectToAction("TargetAccount", new { loc_id = model.MatchedLocID });
        }

        private static List<SelectListItem> populateEmployees(string loc_id)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (loc_id == "All" || loc_id == null)
            {
                var emp = db.tbl_employee.Where(x => x.status == 1).ToList();
                foreach (var val in emp)
                {
                    items.Add(new SelectListItem
                    {
                        Text = val.full_name,
                        Value = Convert.ToString(val.employee_id)
                    });
                }
                return items;
            }
            else
            {
                var emp = db.tbl_employee.Where(x => x.status == 1 && x.loc_ID == loc_id).ToList();
                foreach (var val in emp)
                {
                    items.Add(new SelectListItem
                    {
                        Text = val.full_name,
                        Value = Convert.ToString(val.employee_id)
                    });
                }
                return items;
            } 
        }

        [HttpGet]
        public ActionResult TargetAccount(string loc_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            CustomerReportingViewModel modal = new CustomerReportingViewModel();

            int empId = Convert.ToInt32(Session["userID"].ToString());

            modal.MatchedLocs = populateLocations();

            modal.username = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if (modal.SehubAccess.customer_reporting >=3)
            {
                modal.MatchedLocs.Add(new SelectListItem
                 {
                     Text = "All",
                     Value = "All"
                 });
            }

            string location = "";

            if (loc_id == "All")
            {
                modal.MatchedLocID = "All";
            }
            else
            {
                if (loc_id != null)
                {
                    location = "and a.location_id = '" + loc_id + "'";
                    modal.MatchedLocID = loc_id;
                }
                else
                {
                    location = "and a.location_id = '" + db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault() + "'";
                    modal.MatchedLocID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
                }
            }

            

            modal.EmployeesList = populateEmployees(loc_id);

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            List<targetAcountViewModel> targetAccountTable = new List<targetAcountViewModel>();
            string constr2 = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select full_name, visit_type, customer_name, customer_contact, visit_date, discussion_details, reminders, submission_date from tbl_target_accounts a, tbl_employee b where a.cta_employee_number = b.employee_id "+location;
                //Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            targetAcountViewModel target_account_tbl = new targetAcountViewModel();
                            target_account_tbl.full_name = sdr["full_name"].ToString();
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

            if (targetAccountTable != null)
            {
                modal.targetAccountDetails = targetAccountTable.OrderByDescending(x => x.visit_date).ToList();
            }

            return View(modal);
        }

        [HttpGet]
        public ActionResult DisputeResolution()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            CustomerReportingViewModel modal = new CustomerReportingViewModel();

            int empId = Convert.ToInt32(Session["userID"].ToString());

            modal.username = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            List<tbl_dispute_resolution> disputeResolutionTable = new List<tbl_dispute_resolution>();

            disputeResolutionTable = db.tbl_dispute_resolution.ToList();

            if (disputeResolutionTable != null)
            {
                modal.DisputeResolutionDetails = disputeResolutionTable;
            }

            return View(modal);
        }

        [HttpPost]
        public string forward(string CustomerNum)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            List<tbl_customer_reporting_viewmodel> customerReportingTable = new List<tbl_customer_reporting_viewmodel>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select * from tbl_customer_reporting a, tbl_employee b where a.cta_employee_number= b.employee_id and customer_number='" + CustomerNum + "' order by visit_date";
                //Debug.WriteLine(query);
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

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string custDetailsJson = serializer.Serialize(customerReportingTable);

            //Trace.WriteLine("Reached");

            return custDetailsJson;
        }

        [HttpPost]
        public ActionResult CRMSelectedCustomer(CustomerReportingViewModel model)
        {
            //Debug.WriteLine("Custoomer details:" + model.Custname);
            return RedirectToAction("CRM", new { CustId = model.Custname });

        }

        [HttpPost]
        public ActionResult AddNewCRMDetails(CustomerReportingViewModel model)
        {

            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("customerreporting@citytire.com")); //jordan.blackwood          customerreporting
            msg.From = new MailAddress(db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault(), "Sehub");
            msg.CC.Add(new MailAddress(db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault()));

            msg.Subject = "SE-HUB: Customer Reporting - " + model.customerDetails.CustomerName + ", " + DateTime.Today.ToString().Substring(0, 10);
            msg.Body =
                "Hello," +
                "<br /><br />" +
                "Immediately below are the details of my customer visitation report." +
                "<br /><br />" +
                "<i><b><font size=3>Call Report - " + model.AddCRM.visit_type + " Visit</font></b></i>" +
                "<br /><br />" +
                "<u><b>Customer</b></u>: " + model.customerDetails.CustomerName +
                "<br /><br />" +
                "<u><b>Contact</b></u>: " + model.AddCRM.customer_contact +
                "<br /><br />" +
                "<u><b>Date of Visit</b></u>: " + Convert.ToDateTime(model.AddCRM.visit_date.Date).ToString("dddd, MMMM dd, yyyy") +
                "<br /><br />" +
                "<u><b>Discussion Details</b></u>: " + model.AddCRM.discusssion_details +
                "<br /><br />" +
                "<u><b>Reminders</b></u>: " + model.AddCRM.remainder +
                "<br /><br /><br />" +
                "<b>Thank You,</b>" +
                "<br />" +
                db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential( db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault() , db.tbl_employee_credentials.Where(x => x.employee_id == empId).Select(x => x.password365).FirstOrDefault());
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                //Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }


            //Debug.WriteLine("In AddNewCRMDetails");
            //Debug.WriteLine(model.customerDetails.cust_us1);
            //Debug.WriteLine(model.AddCRM.visit_type);
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
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
                    command.Parameters.AddWithValue("@custId", model.customerDetails.CustomerCode);
                    command.Parameters.AddWithValue("@custName", model.customerDetails.CustomerName);
                    command.Parameters.AddWithValue("@Comments", model.AddCRM.discusssion_details);

                    if(model.AddCRM.remainder != null)
                    {
                        command.Parameters.AddWithValue("@remainders", model.AddCRM.remainder);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@remainders", "");
                    }

                    
                    command.Parameters.AddWithValue("@visitDate", model.AddCRM.visit_date.Date);
                    command.Parameters.AddWithValue("@custContact", model.AddCRM.customer_contact);
                    command.Parameters.AddWithValue("@submissionDate", System.DateTime.Today.Date);


                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                connection.Close();
            }

            return RedirectToAction("CRM", new { CustId = model.customerDetails.CustomerCode });
        }

        private static List<SelectListItem> PopulateCustomers(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var custList = db.tbl_cta_customers.Where(x => x.call_reporting == true).OrderBy(x => x.CustomerName).ToList();

            foreach (var cust in custList)
            {
                items.Add(new SelectListItem
                {
                    Text = cust.CustomerName,
                    Value = cust.CustomerCode.ToString()
                });
            }


            return items;
        }

        [HttpPost]
        public ActionResult AddNewTADetails(CustomerReportingViewModel model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("In AddNewCRMDetails");

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            /*

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("customerreporting@citytire.com")); //jordan.blackwood     harsha.yerramsetty     customerreporting
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.CC.Add(new MailAddress(db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault()));

            msg.Subject = "SE-HUB: Customer Reporting - " + model.customerDetails.cust_name + ", " + DateTime.Today;
            msg.Body =
                "Hello," +
                "<br /><br />" +
                "Immediately below are the details of my customer visitation report." +
                "<br /><br />" +
                "<i><b><font size=3>Target Account - " + model.AddCRM.visit_type + " Visit</font></b></i>" +
                "<br /><br />" +
                "<u><b>Customer</b></u>: " + model.customerDetails.cust_name +
                "<br /><br />" +
                "<u><b>Contact</b></u>: " + model.AddCRM.customer_contact +
                "<br /><br />" +
                "<u><b>Date of Visit</b></u>: " + Convert.ToDateTime(model.AddCRM.visit_date.Date).ToString("dddd, MMMM dd, yyyy") +
                "<br /><br />" +
                "<u><b>Date of Next Visit</b></u>: " + " Not Required" +
                "<br /><br />" +
                "<u><b>Discussion Details</b></u>: " + model.AddCRM.discusssion_details +
                "<br /><br />" +
                "<u><b>Reminders</b></u>: " + model.AddCRM.remainder +
                "<br /><br /><br />" +
                "<b>Thank You,</b>" +
                "<br />" +
                db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();
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
                //Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex).
            +
            {
                Debug.WriteLine(ex.ToString());
            }

            */

            Trace.WriteLine("Reached till here 1");

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_target_accounts VALUES (@cta_employee_number, @visittype, 'customer_name', @customer_contact, @visit_date, @discussion_details,@reminders,@submission_date,@location_id)");

                    string sql = sb.ToString();
                    Trace.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@cta_employee_number", empId);
                        command.Parameters.AddWithValue("@visittype", model.AddTA.visit_type);
                        command.Parameters.AddWithValue("@customer_contact", model.AddTA.customer_contact);
                        command.Parameters.AddWithValue("@visit_date", model.AddTA.visit_date);
                        command.Parameters.AddWithValue("@discussion_details", model.AddTA.discusssion_details);
                        command.Parameters.AddWithValue("@reminders", model.AddTA.remainder);
                        command.Parameters.AddWithValue("@submission_date", System.DateTime.Today.Date);
                        command.Parameters.AddWithValue("@location_id", model.MatchedLocID);

                        Trace.WriteLine("Reached till here 2");

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

            Trace.WriteLine("Reached till here 3");

            return RedirectToAction("TargetAccount", new { CustId = "" });
        }

        [HttpPost]
        public ActionResult AddNewDRDetails(CustomerReportingViewModel model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("In AddNewCRMDetails");
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_dispute_resolution VALUES (@invoice_number, @date_dispute, @mileage_dispute, @date_service, @mileage_service, @customer_number,@customer_name_NoAccount,@employee_id,@details,@recommandation,null ,@final_outcome, @complete)");

                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
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

        private static List<SelectListItem> populateLocationsExp()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_cta_location_info.ToList();

            foreach(var loc in locaList)
            {

                if(loc.loc_id == "001")
                {
                    items.Add(new SelectListItem
                    {
                        Text = loc.loc_id.ToString() + "_AOH",
                        Value = loc.loc_id.ToString()
                    });
                }
                else
                {
                    items.Add(new SelectListItem
                    {
                        Text = loc.loc_id.ToString(),
                        Value = loc.loc_id.ToString()
                    });
                }

                
            }

            return items;
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

        private static List<SelectListItem> populateExpAccount()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Accounts = db.tbl_expense_claim_account.ToList();

            foreach (var act in Accounts)
            {
                items.Add(new SelectListItem
                {
                    Text = act.account_description.ToString(),
                    Value = act.account_number.ToString()
                });

            }

            return items;
        }

        private static List<SelectListItem> populateExpClaims()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Accounts = db.tbl_expense_claim.ToList();

            foreach (var act in Accounts)
            {
                items.Add(new SelectListItem
                {
                    Text = act.transaction_number.ToString(),
                    Value = act.transaction_number.ToString()
                });

            }

            return items;
        }

        private static List<SelectListItem> populateLocationsFleet()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct loc_id from tbl_vehicle_info where vehicle_status = 1";
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

        private static List<SelectListItem> populateVehicles(string location, string vehicle)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var veh = db.tbl_vehicle_info.Where(x => x.loc_id == location && x.vehicle_status == 1).ToList();

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

        [HttpPost]
        public string LoadEXform(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var EX = db.tbl_expense_claim.Where(a => a.transaction_number == value).FirstOrDefault();

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(EX);

        }

        [HttpPost]
        public string validateBarcode(string value)
        {

            Trace.WriteLine(value + "This is the work order");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var barcode = db.tbl_treadtracker_barcode.Where(x => x.barcode == value).Select(x => x.barcode).FirstOrDefault();

            if (barcode != null)
            {
                return "Exists";
            }
            else
            {
                return "Proceed";
            }

        }

        public ActionResult PopulateTireAdjustment(string loc)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            List<KeyValuePair<string, string>> keyValuePair = new List<KeyValuePair<string, string>>();

            var tADetails = db.tbl_tire_adjustment.Where(x => x.location_id == loc).ToList();

            foreach (var TA in tADetails)
            {
                keyValuePair.Add(new KeyValuePair<string, string>(TA.date.Value.ToString("yyyy-MM-dd"), TA.Tire_Adjustment_id));
            }

            return Json(keyValuePair.Select(x => new
            {
                value = x.Key,
                text = x.Value
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateTreadTracker(string loc)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var customers = db.tbl_cta_customers.ToList();
            foreach (var cust in customers)
            {
                var custm = db.tbl_cta_customers.Where(x => x.CustomerCode == cust.CustomerCode).FirstOrDefault();
                if(db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == cust.CustomerName).Count() > 0)
                {
                    custm.call_reporting = true;
                }
                if (db.tbl_treadtracker_workorder.Where(x => x.customer_number == cust.CustomerCode.ToString()).Count() > 0)
                {
                    custm.tread_tracker = true;
                }
                db.SaveChanges();
            }

            return RedirectToAction("ProductionReport", "TreadTracker");
        }

    }
}