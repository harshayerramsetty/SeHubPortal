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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Plant(PlantViewModel model)
        {

            return View(model);
        }

        public ActionResult ErrorPage()
        {
            return View();
        }

        private static List<SelectListItem> populateTreads()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var treads = db.tbl_source_retread_part_number.Select(x => x.tread).Distinct().ToList();

            items.Add(new SelectListItem
            {
                Text = "Select"
            });

            foreach (var tread in treads)
            {
                items.Add(new SelectListItem
                {
                    Text = tread,
                    Value = tread
                });
            }
            return items;
        }

        private static List<SelectListItem> populateWidths()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var treads = db.tbl_source_treadtracker_consumables_TreadRubber.Select(x => x.width).Distinct().ToList();

            items.Add(new SelectListItem
            {
                Text = "Select"
            });

            foreach (var tread in treads)
            {
                items.Add(new SelectListItem
                {
                    Text = tread,
                    Value = tread
                });
            }
            return items;
        }
        
        public ActionResult Station(PlantViewModel model, string pane, string barcode, string stationBarcode)
        {
            try
            {

                if (model.stationBarcode == "0500")
                {
                    return RedirectToAction("Chamber", "Retread");
                }

                if (model.stationBarcode == "0025")
                {
                    return RedirectToAction("ProductionSchedule", "TreadTracker");
                }

                Trace.WriteLine(barcode + " This is the barcode");

                CityTireAndAutoEntities db = new CityTireAndAutoEntities();

                //int empId = Convert.ToInt32(Session["userID"].ToString());
                model.TreadsConsumables = populateTreads();
                model.WidthsConsumables = populateWidths();
                

                var workStations = db.tbl_treadTracker_workStations.OrderBy(x => x.barcode_id).ToList();    // .Where(x => x.status == 1)
                var activeWorkStations = db.tbl_treadTracker_workStations.Where(x => x.status == 1).OrderBy(x => x.barcode_id).ToList();    // .Where(x => x.status == 1)

                tbl_treadTracker_workStations stationInfo = workStations.Where(x => x.barcode_id == model.stationBarcode).FirstOrDefault(); // && x.status == 1  Removed to test retread plant

                Trace.WriteLine("This is the workStation" + stationBarcode);

                if(stationBarcode == "0015")
                {
                    stationInfo = workStations.Where(x => x.barcode_id == "0015").FirstOrDefault();
                    Trace.WriteLine(stationInfo.station + " This is the station");
                }

                Trace.WriteLine("Reached 1");
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
                    model.cap_count = stationInfo.cap_count.Value;
                    Trace.WriteLine("Reached 2");
                    //model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                    model.TreadRubber = db.tbl_source_treadtracker_consumables_TreadRubber.OrderBy(x => x.tread).OrderBy(x => x.width).ToList();
                    model.nonTreadRubber = db.tbl_source_treadtracker_consumables_nonTreadRubber.ToList();
                    model.paneType = pane;

                    if (pane == "barcode")
                    {

                        model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                        model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.barcodeInfo.retread_workorder).FirstOrDefault();
                        model.customerName = db.tbl_customer_list.Where(x => x.cust_us1 == model.workOrderInfo.customer_number).Select(x => x.cust_name).FirstOrDefault();

                        var custInfo = db.tbl_treadtracker_customer_retread_spec.Where(x => x.customer_number.ToString() == model.workOrderInfo.customer_number).FirstOrDefault();

                        Trace.WriteLine("This is the customer number" + model.workOrderInfo.customer_number);

                        if (custInfo != null)
                        {
                            model.AgeLimit = custInfo.age_limit;
                            model.BrandRequirements = custInfo.brand_name_only;
                        }

                        model.FailureCodes = db.tbl_source_RARcodes.Where(x => x.TT600 == 1).ToList();
                        model.TreadList = db.tbl_source_retread_tread.ToList();
                        model.SizeList = db.tbl_treadtracker_casing_sizes.ToList();
                        model.LocationList = db.tbl_cta_location_info.Where(x => x.tread_tracker_access == 1).ToList();
                        model.BrandList = db.tbl_source_commercial_tire_manufacturers.ToList();
                        model.workStations = workStations;
                        model.activeWorkStations = activeWorkStations;
                        model.CapCountList = db.tbl_source_treadtracker_cap_count.ToList();
                        model.TreadRubber = db.tbl_source_treadtracker_consumables_TreadRubber.Where(x => x.tread == model.barcodeInfo.retread_design).OrderBy(x => x.tread).OrderBy(x => x.width).ToList();
                        model.nonTreadRubber = db.tbl_source_treadtracker_consumables_nonTreadRubber.ToList();
                        Trace.WriteLine("Reached 3");
                        List<KeyValuePair<string, bool>> stationResults = new List<KeyValuePair<string, bool>>();

                        foreach (var item in activeWorkStations)
                        {
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
                        Trace.WriteLine("Reached 4");
                        model.workStationResults = stationResults;

                    }
                    Trace.WriteLine("Reached 5");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Plant", new { ac = "IncorrectBarcode" });
                }
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Retread");
            }           

        }

        [HttpPost]
        public double? GetTemperatureValue(int val)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
            //DateTime backTime = currentTime.AddMinutes(-2.1).AddSeconds(-5);
            DateTime backTime = currentTime.AddSeconds(-5);

            if (val == 1)
            {
                double? t1 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.temperature_1).FirstOrDefault();
                if (t1 != null)
                {
                    return t1;
                }
                else
                {
                    return 0;
                }
            }
            else if (val == 2)
            {
                double? t2 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.temperature_2).FirstOrDefault();
                if (t2 != null)
                {
                    return t2;
                }
                else
                {
                    return 0;
                }
            }
            else if (val == 3)
            {
                double? t3 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.temperature_3).FirstOrDefault();
                if (t3 != null)
                {
                    return t3;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                double? t4 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.temperature_4).FirstOrDefault();
                if (t4 != null)
                {
                    return t4;
                }
                else
                {
                    return 0;
                }
            }

        }


        [HttpPost]
        public double? GetPressureValue(int val)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
            DateTime backTime = currentTime.AddSeconds(-5);
            //DateTime backTime = currentTime.AddMinutes(2).AddSeconds(11).AddSeconds(-5);

            if (val == 1)
            {
                double? p1 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.pressure_1).FirstOrDefault();
                if (p1 != null)
                {
                    return p1;
                }
                else
                {
                    return 0;
                }                
            }
            else if (val == 2)
            {
                double? p2 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.pressure_2).FirstOrDefault();
                if (p2 != null)
                {
                    return p2;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                double? p3 = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).Select(x => x.pressure_3).FirstOrDefault();
                if (p3 != null)
                {
                    return p3;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string GetActiveSensors(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            if (value == "Pressure")
            {
                return String.Join(",", db.tbl_treadTracker_chamber_senser.Where(x => x.name.Contains("Pressure") && x.status == true).Select(x => x.name));
            }
            else
            {
                return String.Join(",", db.tbl_treadTracker_chamber_senser.Where(x => x.name.Contains("Temperature") && x.status == true).Select(x => x.name));
            }
            
        }

        [HttpPost]
        public string GetAllPressureValue()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
            DateTime backTime = currentTime.AddSeconds(-5);
            //DateTime backTime = currentTime.AddMinutes(2).AddSeconds(11).AddSeconds(-5);

            string finalString = "";

            var record = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).FirstOrDefault();

            var activeSensers = db.tbl_treadTracker_chamber_senser.Where(x => x.senser == "Pressure" && x.status == true).Select(x => x.name).ToList();

            foreach (string senser in activeSensers)
            {
                finalString += GetColumn(record, senser.ToLower()) + ";";
            }

            finalString.Remove(finalString.LastIndexOf(";"));

            return finalString;
        }


        [HttpPost]
        public string GetAllTemperatureValue()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
            //DateTime backTime = currentTime.AddMinutes(-2.1).AddSeconds(-5);
            DateTime backTime = currentTime.AddSeconds(-5);

            string finalString = "";

            var record = db.tbl_tread_tracker_chamber_data_logger.Where(x => x.timestamp > backTime).OrderByDescending(x => x.timestamp).FirstOrDefault();

            var activeSensers = db.tbl_treadTracker_chamber_senser.Where(x => x.senser == "Temperature" && x.status == true).Select(x => x.name).ToList();

            foreach (string senser in activeSensers)
            {
                finalString += GetColumn(record, senser.ToLower()) + ";";
            }

            finalString.Remove(finalString.LastIndexOf(";"));
            
            return finalString;
        }

        public string GetColumn(tbl_tread_tracker_chamber_data_logger items, string columnName)
        {
            if (items != null)
            {
                return items.GetType().GetProperty(columnName).GetValue(items).ToString();
            }
            else
            {
                return "0.0";
            }

            
        }

        public string GetColumn(tbl_treadtracker_barcode items, string columnName)
        {
            try
            {
                return items.GetType().GetProperty(columnName).GetValue(items).ToString();
            }
            catch
            {
                return "";
            }            
        }


        public class DataPoint
        {
            public DataPoint(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            public Nullable<double> X = null;

            public Nullable<double> Y = null;
        }

        public ActionResult Chamber(PlantViewModel model)
        {
            try
            {
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                model.chamberSensors = db.tbl_treadTracker_chamber_senser.Where(x => x.status == true).OrderBy(x => x.name).ToList();
                return View(model);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Retread");
            }
            
        }
        
        public ActionResult StationBarcode(PlantViewModel model)
        {
            return RedirectToAction("Station", new { pane = "barcode", barcode = model.Barcode, stationBarcode = model.stationBarcode });
        }

        public ActionResult BarcodeSave(PlantViewModel model)
        {
            
            try
            {
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();

                if (model.station == "Preliminary")
                {
                    barcodeInfo.TT050_result = model.barcodeInfo.TT050_result;
                    if (barcodeInfo.TT050_date == null)
                    {
                        barcodeInfo.TT050_date = System.DateTime.Today.Date;
                    }                    
                }
                else if (model.station == "NDT")
                {
                    barcodeInfo.TT100_result = model.barcodeInfo.TT100_result;
                    if (barcodeInfo.TT100_date == null)
                    {
                        barcodeInfo.TT100_date = System.DateTime.Today.Date;
                    }                    
                }
                else if (model.station == "Buffer")
                {
                    barcodeInfo.TT200_result = model.barcodeInfo.TT200_result;
                    if (barcodeInfo.TT200_date == null)
                    {
                        barcodeInfo.TT200_date = System.DateTime.Today.Date;
                    }                    
                }
                else if (model.station == "Repair")
                {
                    barcodeInfo.TT300_result = model.barcodeInfo.TT300_result;
                    if (barcodeInfo.TT300_date == null)
                    {
                        barcodeInfo.TT300_date = System.DateTime.Today.Date;                        
                    }
                    barcodeInfo.TT300_patch = model.barcodeInfo.TT300_patch;
                    barcodeInfo.TT300_rope = model.barcodeInfo.TT300_rope;
                }
                else if (model.station == "Builder")
                {
                    barcodeInfo.TT400_result = model.barcodeInfo.TT400_result;
                    barcodeInfo.TT400_cushion = model.barcodeInfo.TT400_cushion;
                    barcodeInfo.TT400_strip = model.barcodeInfo.TT400_strip;
                    barcodeInfo.TT400_tread = model.barcodeInfo.TT400_tread;
                    barcodeInfo.TT400_rope = model.barcodeInfo.TT400_rope;
                    if (barcodeInfo.TT400_date == null)
                    {
                        barcodeInfo.TT400_date = System.DateTime.Today.Date;                        
                    }
                }
                else if (model.station == "Chamber")
                {
                    barcodeInfo.TT500_result = model.barcodeInfo.TT500_result;
                    if (barcodeInfo.TT500_date == null)
                    {
                        barcodeInfo.TT500_date = System.DateTime.Today.Date;
                    }                    
                }
                else if (model.station == "Final")
                {
                    barcodeInfo.TT600_result = model.barcodeInfo.TT600_result;
                    if (barcodeInfo.TT600_date == null)
                    {
                        barcodeInfo.TT600_date = System.DateTime.Today.Date;
                    }
                    
                }

                barcodeInfo.retread_design = model.barcodeInfo.retread_design;
                barcodeInfo.casing_size = model.barcodeInfo.casing_size;
                barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
                barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
                barcodeInfo.cap_count = model.barcodeInfo.cap_count;

                tbl_treadtracker_production_log production_log = new tbl_treadtracker_production_log();
                DateTime timeUtc = DateTime.UtcNow;
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
                DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
                string station_id = db.tbl_treadTracker_workStations.Where(x => x.station == model.station).Select(x => x.processID).FirstOrDefault();

                production_log.barcode = model.barcodeInfo.barcode;
                production_log.work_station = station_id;
                production_log.timestamp = currentTime;
                production_log.status = GetColumn(model.barcodeInfo, station_id + "_result");
                production_log.details = "Tread -> " + model.barcodeInfo.retread_design + " ; "+"Size -> " + model.barcodeInfo.casing_size + " ; " + "Brand -> " + model.barcodeInfo.casing_brand + " ; " + "Loc -> " + model.barcodeInfo.ship_to_location + " ; " + "Cap -> " + model.barcodeInfo.cap_count + " ; ";

                db.tbl_treadtracker_production_log.Add(production_log);

                db.SaveChanges();

                return RedirectToAction("Station", new { pane = "barcode", barcode = model.barcodeInfo.barcode, stationBarcode = model.stationBarcode });

            }
            catch
            {
                return RedirectToAction("ErrorPage", "Retread");
            }

        }

        public string getSizeID(string size)
        {
            Trace.WriteLine(size);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            return db.tbl_treadtracker_casing_sizes.Where(x => x.casing_size == size).Select(x => x.size_id).FirstOrDefault().ToString();
        }

    }
}