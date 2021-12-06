using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class RetreadController : Controller
    {
        // GET: ToolsSample
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Plant(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return View(model);
        }

        public ActionResult Station(PlantViewModel model)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (model.stationBarcode == db.tbl_treadTracker_workStations.Where(x => x.station == "NDT").Select(x => x.barcode_id).FirstOrDefault())
            {
                return RedirectToAction("NdtStation");
            }
            else if(model.stationBarcode == db.tbl_treadTracker_workStations.Where(x => x.station == "Buffer").Select(x => x.barcode_id).FirstOrDefault())
            {
                return RedirectToAction("BufferBuilder");
            }
            else if(model.stationBarcode == db.tbl_treadTracker_workStations.Where(x => x.station == "Final").Select(x => x.barcode_id).FirstOrDefault())
            {
                return RedirectToAction("FinalStation");
            }
            else if (model.stationBarcode == db.tbl_treadTracker_workStations.Where(x => x.station == "Chamber").Select(x => x.barcode_id).FirstOrDefault())
            {
                return RedirectToAction("Chamber");
            }
            else
            {
                return RedirectToAction("Plant", new { ac = "IncorrectBarcode" });
            }
        }

        public ActionResult NdtStation(PlantViewModel model, string pane, string barcode)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            model.paneType = pane;

            if(pane == "barcode")
            {
                model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.barcodeInfo.retread_workorder).FirstOrDefault();
                model.customerName = db.tbl_customer_list.Where(x => x.cust_us1 == model.workOrderInfo.customer_number).Select(x => x.cust_name).FirstOrDefault();
                model.FailureCodes = db.tbl_source_RARcodes.ToList();
                model.TreadList = db.tbl_retread_tread.ToList();
                model.SizeList = db.tbl_treadtracker_casing_sizes.ToList();
                model.LocationList = db.tbl_cta_location_info.Where(x => x.tread_tracker_access == 1).ToList();
                model.BrandList = db.tbl_commercial_tire_manufacturers.ToList();
            }

            return View(model);
        }

        public ActionResult BufferBuilder(PlantViewModel model, string pane, string barcode)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            model.paneType = pane;

            if (pane == "barcode")
            {
                model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.barcodeInfo.retread_workorder).FirstOrDefault();
                model.customerName = db.tbl_customer_list.Where(x => x.cust_us1 == model.workOrderInfo.customer_number).Select(x => x.cust_name).FirstOrDefault();
                model.FailureCodes = db.tbl_source_RARcodes.ToList();
                model.TreadList = db.tbl_retread_tread.ToList();
                model.SizeList = db.tbl_treadtracker_casing_sizes.ToList();
                model.LocationList = db.tbl_cta_location_info.Where(x => x.tread_tracker_access == 1).ToList();
                model.BrandList = db.tbl_commercial_tire_manufacturers.ToList();
            }

            return View(model);
        }

        public ActionResult FinalStation(PlantViewModel model, string pane, string barcode)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            model.paneType = pane;

            if (pane == "barcode")
            {
                model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.barcodeInfo.retread_workorder).FirstOrDefault();
                model.customerName = db.tbl_customer_list.Where(x => x.cust_us1 == model.workOrderInfo.customer_number).Select(x => x.cust_name).FirstOrDefault();
                model.FailureCodes = db.tbl_source_RARcodes.ToList();
                model.TreadList = db.tbl_retread_tread.ToList();
                model.SizeList = db.tbl_treadtracker_casing_sizes.ToList();
                model.LocationList = db.tbl_cta_location_info.Where(x => x.tread_tracker_access == 1).ToList();
                model.BrandList = db.tbl_commercial_tire_manufacturers.ToList();
            }

            return View(model);
        }

        public ActionResult Chamber(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return View(model);
        }


        public ActionResult NdtBarcode(PlantViewModel model)
        {
            /*
             
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if(db.tbl_treadtracker_barcode.Where(x => x.barcode == model.NdtBarcode && x.preliminary_inspection_result == "GOOD" && x.buffer_builder_result == null && x.final_inspection_result == null).Count() == 0)
            {
                return RedirectToAction("NdtStation", new { pane = "IncorrectBarcode", barcode = model.NdtBarcode });
            }
             
            */


            return RedirectToAction("NdtStation", new { pane = "barcode", barcode = model.NdtBarcode });
        }

        public ActionResult BufferBuilderBarcode(PlantViewModel model)
        {

            /*
             * CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (db.tbl_treadtracker_barcode.Where(x => x.barcode == model.BufferBarcode && x.preliminary_inspection_result == "GOOD" && x.ndt_machine_result == "GOOD" && (x.final_inspection_result == null || x.final_inspection_result == "GOOD")).Count() == 0)
            {
                return RedirectToAction("BufferBuilder", new { pane = "IncorrectBarcode", barcode = model.BufferBarcode });
            }
            */
            return RedirectToAction("BufferBuilder", new { pane = "barcode", barcode = model.BufferBarcode });
        }

        public ActionResult FinalStationBarcode(PlantViewModel model)
        {
            /*
            
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (db.tbl_treadtracker_barcode.Where(x => x.barcode == model.FinalBarcode && x.preliminary_inspection_result == "GOOD" && x.ndt_machine_result == "GOOD" && x.final_inspection_result != null).Count() == 0)
            {
                return RedirectToAction("FinalStation", new { pane = "IncorrectBarcode", barcode = model.FinalBarcode });
            }
             
            */

            return RedirectToAction("FinalStation", new { pane = "barcode", barcode = model.FinalBarcode });
        }

        public ActionResult NdtBarcodeSave(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();

            barcodeInfo.ndt_machine_result = model.barcodeInfo.ndt_machine_result;
            barcodeInfo.retread_design = model.barcodeInfo.retread_design;
            barcodeInfo.casing_size = model.barcodeInfo.casing_size;
            barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
            barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
            db.SaveChanges();

            return RedirectToAction("NdtStation", new { pane = "barcode", barcode = model.barcodeInfo.barcode });
        }

        public ActionResult BufferBuilderBarcodeSave(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();

            barcodeInfo.buffer_builder_result = model.barcodeInfo.buffer_builder_result;
            barcodeInfo.retread_design = model.barcodeInfo.retread_design;
            barcodeInfo.casing_size = model.barcodeInfo.casing_size;
            barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
            barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
            db.SaveChanges();

            return RedirectToAction("BufferBuilder", new { pane = "barcode", barcode = model.barcodeInfo.barcode });
        }

        public ActionResult FinalStationBarcodeSave(PlantViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.barcode == model.barcodeInfo.barcode).FirstOrDefault();

            barcodeInfo.final_inspection_result = model.barcodeInfo.final_inspection_result;
            barcodeInfo.retread_design = model.barcodeInfo.retread_design;
            barcodeInfo.casing_size = model.barcodeInfo.casing_size;
            barcodeInfo.casing_brand = model.barcodeInfo.casing_brand;
            barcodeInfo.ship_to_location = model.barcodeInfo.ship_to_location;
            db.SaveChanges();

            return RedirectToAction("FinalStation", new { pane = "barcode", barcode = model.barcodeInfo.barcode });
        }

    }
}