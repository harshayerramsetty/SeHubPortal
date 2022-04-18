using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Data;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SeHubPortal.Controllers
{
    public class RetreadController : Controller
    {
        // GET: ToolsSample

        string Baseurl = "http://169.254.67.83/";
        public async Task<ActionResult> GetAPIdataWothoutAuth()
        {
            using (var client = new HttpClient())
            {
                Trace.WriteLine("Passing service base url");
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                Trace.WriteLine("Define request data format");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Trace.WriteLine("Sending request to find web api REST service resource GetAllEmployees using HttpClient");
                HttpResponseMessage Res = await client.GetAsync("data/files/local/");
                Trace.WriteLine("Checking the response is successful or not which is sent using HttpClient");
                if (Res.IsSuccessStatusCode)
                {
                    Trace.WriteLine("Storing the response details recieved from web api");
                    var EmpResponse = Res.Content.ReadAsStringAsync().Result;
                    Trace.WriteLine("Deserializing the response recieved from web api and storing into the Employee list");
                    Trace.WriteLine(EmpResponse);
                    return Content(EmpResponse);
                }
                return Content("");
            }
        }

        public async Task<ActionResult> GetAPIdata()
        {
            using (var client = new HttpClient())
            {
                var baseAddress = "http://169.254.67.83";
                var api = "/data/files/local/";
                client.BaseAddress = new Uri(baseAddress);
                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(contentType); 
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("admin:admin");
                string base64EncodedAuthenticationString = System.Convert.ToBase64String(plainTextBytes);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                var response = await client.GetAsync(api);
                var statusCode = response.StatusCode.ToString();
                Trace.WriteLine(response.RequestMessage.RequestUri);
                if (response.IsSuccessStatusCode)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<object>(stringData);
                    return Content(result.ToString());
                }
                return Content("");
            }
        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Plant(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            return View(model);
        }

        public ActionResult Station(PlantViewModel model, string pane, string barcode, string stationBarcode)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //int empId = Convert.ToInt32(Session["userID"].ToString());

            var workStations = db.tbl_treadTracker_workStations.Where(x => x.status == 1).OrderBy(x => x.barcode_id).ToList();

            var stationInfo = db.tbl_treadTracker_workStations.Where(x => x.barcode_id == model.stationBarcode && x.status == 1).FirstOrDefault();

            if (stationInfo != null)
            {
                model.station = stationInfo.station;
                model.icon = stationInfo.icon;
                model.status = stationInfo.status.Value;
                model.tread = stationInfo.tread.Value;
                model.size = stationInfo.size.Value;
                model.brand = stationInfo.brand.Value;
                model.shipto = stationInfo.ship_to.Value;
                model.consumables = stationInfo.consumables.Value;
                model.reprint = stationInfo.reprint.Value;

                //model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

                model.paneType = pane;

                if (pane == "barcode")
                {
                    model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                    model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.barcodeInfo.retread_workorder).FirstOrDefault();
                    model.customerName = db.tbl_customer_list.Where(x => x.cust_us1 == model.workOrderInfo.customer_number).Select(x => x.cust_name).FirstOrDefault();
                    model.FailureCodes = db.tbl_source_RARcodes.Where(x => x.TT600 == 1).ToList();
                    model.TreadList = db.tbl_source_retread_tread.ToList();
                    model.SizeList = db.tbl_treadtracker_casing_sizes.ToList();
                    model.LocationList = db.tbl_cta_location_info.Where(x => x.tread_tracker_access == 1).ToList();
                    model.BrandList = db.tbl_source_commercial_tire_manufacturers.ToList();
                    model.workStations = workStations;

                    List<KeyValuePair<string, bool>> stationResults = new List<KeyValuePair<string, bool>>();

                    foreach (var item in workStations)
                    {
                        /*
                         
                            if(item.station == "Preliminary")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("Preliminary", model.barcodeInfo.TT050_date.HasValue));
                        }
                             
                             */
                        if (item.station == "NDT")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("NDT", model.barcodeInfo.TT100_date.HasValue));
                        }
                        else if (item.station == "Buffer")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("Buffer", model.barcodeInfo.TT200_date.HasValue));
                        }
                        else if (item.station == "Repair")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("Repair", model.barcodeInfo.TT300_date.HasValue));
                        }
                        else if (item.station == "Builder")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("Builder", model.barcodeInfo.TT400_date.HasValue));
                        }
                        else if (item.station == "Chamber")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("Chamber", model.barcodeInfo.TT500_date.HasValue));
                        }
                        else if (item.station == "Final")
                        {
                            stationResults.Add(new KeyValuePair<string, bool>("Final", model.barcodeInfo.TT600_date.HasValue));
                        }

                    }

                    model.workStationResults = stationResults;

                }

                return View(model);
            }
            else
            {
                return RedirectToAction("Plant", new { ac = "IncorrectBarcode" });
            }

        }

        public ActionResult Chamber(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return View(model);
        }
        
        public ActionResult StationBarcode(PlantViewModel model)
        {
            return RedirectToAction("Station", new { pane = "barcode", barcode = model.Barcode, stationBarcode = model.stationBarcode });
        }

        public ActionResult BarcodeSave(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();
            
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_treadtracker_activity_log VALUES (@barcode, @retread_workorder, @work_stationId, @activity_date, @activity_result, @inventory_transfer)");

                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@barcode", barcodeInfo.barcode);
                        command.Parameters.AddWithValue("@retread_workorder", barcodeInfo.retread_workorder);
                        command.Parameters.AddWithValue("@work_stationId", db.tbl_treadTracker_workStations.Where(x => x.barcode_id == "0100").Select(x => x.processID).FirstOrDefault());
                        command.Parameters.AddWithValue("@activity_date", System.DateTime.Today.Date.ToString());
                        command.Parameters.AddWithValue("@activity_result", model.barcodeInfo.ndt_machine_result);
                        command.Parameters.AddWithValue("@inventory_transfer", "");


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

            if(model.station == "Preliminary")
            {
                barcodeInfo.TT050_result = model.barcodeInfo.TT050_result;
                barcodeInfo.TT050_date = System.DateTime.Today.Date;
            }
            else if (model.station == "NDT")
            {
                barcodeInfo.TT100_result = model.barcodeInfo.TT100_result;
                barcodeInfo.TT100_date = System.DateTime.Today.Date;
            }
            else if (model.station == "Buffer")
            {
                barcodeInfo.TT200_result = model.barcodeInfo.TT200_result;
                barcodeInfo.TT200_date = System.DateTime.Today.Date;
            }
            else if (model.station == "Repair")
            {
                barcodeInfo.TT300_result = model.barcodeInfo.TT300_result;
                barcodeInfo.TT300_date = System.DateTime.Today.Date;
            }
            else if (model.station == "Builder")
            {
                barcodeInfo.TT400_result = model.barcodeInfo.TT400_result;
                barcodeInfo.TT400_date = System.DateTime.Today.Date;
            }
            else if (model.station == "Chamber")
            {
                barcodeInfo.TT500_result = model.barcodeInfo.TT500_result;
                barcodeInfo.TT500_date = System.DateTime.Today.Date;
            }
            else if (model.station == "Final")
            {
                barcodeInfo.TT600_result = model.barcodeInfo.TT600_result;
                barcodeInfo.TT600_date = System.DateTime.Today.Date;
            }

            barcodeInfo.retread_design = model.barcodeInfo.retread_design;
            barcodeInfo.casing_size = model.barcodeInfo.casing_size;
            barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
            barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
            barcodeInfo.comments = model.barcodeInfo.comments;
            db.SaveChanges();

            return RedirectToAction("Station", new { pane = "barcode", barcode = model.barcodeInfo.barcode, stationBarcode = model.stationBarcode });
        }

        public ActionResult BufferBuilderBarcodeSave(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_treadtracker_activity_log VALUES (@barcode, @retread_workorder, @work_stationId, @activity_date, @activity_result, @inventory_transfer)");

                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@barcode", barcodeInfo.barcode);
                        command.Parameters.AddWithValue("@retread_workorder", barcodeInfo.retread_workorder);
                        command.Parameters.AddWithValue("@work_stationId", db.tbl_treadTracker_workStations.Where(x => x.barcode_id == "0200").Select(x => x.processID).FirstOrDefault());
                        command.Parameters.AddWithValue("@activity_date", System.DateTime.Today.Date.ToString());
                        command.Parameters.AddWithValue("@activity_result", model.barcodeInfo.ndt_machine_result);
                        command.Parameters.AddWithValue("@inventory_transfer", "");


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

            barcodeInfo.buffer_builder_result = model.barcodeInfo.buffer_builder_result;
            barcodeInfo.retread_design = model.barcodeInfo.retread_design;
            barcodeInfo.casing_size = model.barcodeInfo.casing_size;
            barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
            barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
            barcodeInfo.comments = model.barcodeInfo.comments;
            db.SaveChanges();

            return RedirectToAction("BufferBuilder", new { pane = "barcode", barcode = model.barcodeInfo.barcode });
        }

        public ActionResult FinalStationBarcodeSave(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_treadtracker_activity_log VALUES (@barcode, @retread_workorder, @work_stationId, @activity_date, @activity_result, @inventory_transfer)");

                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@barcode", barcodeInfo.barcode);
                        command.Parameters.AddWithValue("@retread_workorder", barcodeInfo.retread_workorder);
                        command.Parameters.AddWithValue("@work_stationId", db.tbl_treadTracker_workStations.Where(x => x.barcode_id == "0600").Select(x => x.processID).FirstOrDefault());
                        command.Parameters.AddWithValue("@activity_date", System.DateTime.Today.Date.ToString());
                        command.Parameters.AddWithValue("@activity_result", model.barcodeInfo.ndt_machine_result);
                        command.Parameters.AddWithValue("@inventory_transfer", "");


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

            barcodeInfo.final_inspection_result = model.barcodeInfo.final_inspection_result;
            barcodeInfo.retread_design = model.barcodeInfo.retread_design;
            barcodeInfo.casing_size = model.barcodeInfo.casing_size;
            barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
            barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
            barcodeInfo.comments = model.barcodeInfo.comments;
            db.SaveChanges();

            return RedirectToAction("FinalStation", new { pane = "barcode", barcode = model.barcodeInfo.barcode });
        }

        public string getSizeID(string size)
        {
            Trace.WriteLine(size);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            return db.tbl_treadtracker_casing_sizes.Where(x => x.casing_size == size).Select(x => x.size_id).FirstOrDefault().ToString();
        }

    }
}