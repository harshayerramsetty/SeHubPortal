using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;



namespace SeHubPortal.Controllers
{
    public class VehicleHarpoonController : Controller
    {
        // GET: VehicleHarpoon
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard(HarpoonVehicleDashboardViewModel model, string loc)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;
            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();
            string location;

            if(loc == null || loc == "0")
            {
                if (user.profile == "Administrator" || user.profile == "Global Manager")
                {
                    location = "All";
                }
                else
                {
                    location = user.loc_id;
                }
            }
            else
            {
                location = loc;
            }

            Trace.WriteLine(loc);

            

            var settings = db.tbl_harpoon_settings.Where(x => x.client_id == clientID).FirstOrDefault();

            if (settings.LocIDinListBox_OnOff == 1)
            {
                model.Locations = populateLocationsPermissions(clientID, uesrEmail, true);
                model.LocationsWoithoutAll = populateLocationsPermissions(clientID, uesrEmail, false);
            }
            else
            {
                model.Locations = populateLocationsNames(clientID, uesrEmail, true);
                model.LocationsWoithoutAll = populateLocationsNames(clientID, uesrEmail, false);
            }

            
            if(location != "All")
            {
                model.Location = Convert.ToInt32(location);
                model.Vehicles = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID && x.auto_loc_id == model.Location).ToList();
            }
            else
            {
                model.Vehicles = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID).ToList();
            }

            

            return View(model);
        }

        public ActionResult FuelReporting(HarpoonFuelReportingViewModel model, string loc, string vid)
        {

            Trace.WriteLine("This is the loc" + loc + "This is the vid" + vid);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            var clientID = user.client_id;

            model.ChargeAccounts = populateChargeAccounts(clientID);

            model.client_info = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).FirstOrDefault();
            string location;

            if (loc == null || loc == "0")
            {
                if (user.profile == "Administrator" || user.profile == "Global Manager")
                {
                    location = "All";
                }
                else
                {
                    location = user.loc_id;
                }
            }
            else
            {
                location = loc;
            }
            
            if(vid != null)
            {
                model.selectedVehicle = Convert.ToInt32(vid);
            }
            else
            {
                if (location != "All")
                {
                    model.selectedVehicle = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID && x.auto_loc_id.ToString() == location).Select(x => x.vehicle_auto_id).FirstOrDefault();
                }
                else
                {
                    model.selectedVehicle = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID).Select(x => x.vehicle_auto_id).FirstOrDefault();
                }
                
            }

            model.fuelLog = db.tbl_harpoon_fuel_log.Where(x => x.client_id == clientID && x.vehicle_auto_id == model.selectedVehicle).OrderByDescending(x => x.odometer).ToList();
            
            var settings = db.tbl_harpoon_settings.Where(x => x.client_id == clientID).FirstOrDefault();

            if (settings.LocIDinListBox_OnOff == 1)
            {
                model.Locations = populateLocationsPermissions(clientID, uesrEmail, true);
                model.LocationsWoithoutAll = populateLocationsPermissions(clientID, uesrEmail, false);
            }
            else
            {
                model.Locations = populateLocationsNames(clientID, uesrEmail, true);
                model.LocationsWoithoutAll = populateLocationsNames(clientID, uesrEmail, false);
            }


            if (location != "All")
            {
                model.Location = Convert.ToInt32(location);
                model.Vehicles = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID && x.auto_loc_id == model.Location).ToList();
            }
            else
            {
                model.Vehicles = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID).ToList();
            }

            return View(model);
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
        public ActionResult AddNewVehicle(HarpoonVehicleDashboardViewModel model, HttpPostedFileBase vehicleImage)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_harpoon_vehicle newVehicle = new tbl_harpoon_vehicle();

            string uesrEmail = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();

            var clientID = user.client_id;


            byte[] imageBytesThumbNail = null;
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var imageNameThumbNail = Path.GetFileName(vehicleImage.FileName);
                using (Image imageThumbNail = Image.FromStream(vehicleImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        imageThumbNail.Save(m, imageThumbNail.RawFormat);
                        imageBytesThumbNail = m.ToArray();
                    }
                }
            }


            byte[] imageBytes = null;
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(vehicleImage.FileName);
                using (Image image = Image.FromStream(vehicleImage.InputStream, true, true))
                {
                    double height = 130 * image.Height / image.Width;
                    Image img = Resize(image, 130, (int)Math.Round(height));

                    using (MemoryStream m = new MemoryStream())
                    {
                        img.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();
                    }
                }
            }

            var current = db.tbl_harpoon_vehicle.Where(x => x.client_id.ToString() == clientID).OrderByDescending(x => x.vehicle_auto_id).Select(x => x.vehicle_auto_id).FirstOrDefault();

            if(current != null)
            {
                newVehicle.vehicle_auto_id = current + 1;
            }
            else
            {
                newVehicle.vehicle_auto_id = 0;
            }

            newVehicle.client_id = Convert.ToInt32(clientID);
            newVehicle.VIN = model.NewVehicle.VIN;
            newVehicle.vehicle_long_id = model.NewVehicle.vehicle_long_id;
            newVehicle.plate = model.NewVehicle.plate;
            newVehicle.auto_loc_id = model.Location;
            newVehicle.manufacturer = model.NewVehicle.manufacturer;
            newVehicle.model = model.NewVehicle.model;
            newVehicle.status = true;
            newVehicle.vehicle_image = imageBytesThumbNail;
            newVehicle.thumbnail = imageBytes;
            db.tbl_harpoon_vehicle.Add(newVehicle);

            db.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public ActionResult ChangeLocVehicles(HarpoonVehicleDashboardViewModel model)
        {
            return RedirectToAction("Dashboard", new { loc = model.Location });
        }

        [HttpPost]
        public ActionResult ChangeLocVehicleFuelReporting(HarpoonVehicleDashboardViewModel model)
        {
            return RedirectToAction("FuelReporting", new { loc = model.Location });
        }

        [HttpGet]
        public ActionResult pullFuelLog(pullHarpoonFuleLog model, int auto, DateTime start, DateTime end)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            if(start == new DateTime(1977,01,01) && end == new DateTime(1977, 01, 01))
            {
                model.fuelLog = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == auto).OrderByDescending(x => x.odometer).ToList();
            }
            else
            {
                model.fuelLog = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == auto && x.date_of_purchase > start && x.date_of_purchase < end).OrderByDescending(x => x.odometer).ToList();
            }
            model.start = start;
            model.end = end;
            model.selectedVehicle = auto;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SaveFuelReceipt(HarpoonFuelReportingViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_harpoon_fuel_log newLog = new tbl_harpoon_fuel_log();
            string uesrEmail = Session["userID"].ToString();
            var user = db.tbl_harpoon_users.Where(x => x.email == uesrEmail).FirstOrDefault();
            var clientID = user.client_id;
            var selectedVehicle = db.tbl_harpoon_vehicle.Where(x => x.vehicle_auto_id == model.selectedVehicle).FirstOrDefault();

            newLog.vehicle_auto_id = model.selectedVehicle;
            newLog.entry_user = uesrEmail;
            newLog.entry_date = System.DateTime.Now;
            newLog.VIN = selectedVehicle.VIN;
            newLog.flag = false;

            string transactionReceiptNumber = Convert.ToDateTime(model.fuelRecipt.date_of_purchase).ToString("yyyyMMdd");
            var fuelList = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == model.selectedVehicle && x.transaction_number.Contains(selectedVehicle.VIN + "-" + transactionReceiptNumber)).OrderByDescending(x => x.transaction_number).FirstOrDefault();

            int lastTwoSequence = 0;
            if (fuelList != null)
            {
                lastTwoSequence = Convert.ToInt32(fuelList.transaction_number.ToString().Substring(fuelList.transaction_number.ToString().Length - 1));
            }

            if (lastTwoSequence == 0)
            {
                newLog.transaction_number = selectedVehicle.VIN + "-" + Convert.ToDateTime(model.fuelRecipt.date_of_purchase).ToString("yyyyMMdd") + "-" + "1";
            }
            else
            {
                newLog.transaction_number = selectedVehicle.VIN + "-" + Convert.ToDateTime(model.fuelRecipt.date_of_purchase).ToString("yyyyMMdd") + "-" + (lastTwoSequence + 1).ToString();
            }
            newLog.client_id = clientID;
            newLog.date_of_purchase = model.fuelRecipt.date_of_purchase;
            newLog.odometer = model.fuelRecipt.odometer;
            newLog.no_of_liters = model.fuelRecipt.no_of_liters;
            newLog.price_per_liter = model.fuelRecipt.price_per_liter;
            newLog.charge_type = model.fuelRecipt.charge_type;
            newLog.audit_status = false;
            newLog.comments = model.fuelRecipt.comments;

            db.tbl_harpoon_fuel_log.Add(newLog);

            db.SaveChanges();

            return RedirectToAction("FuelReporting", new { vid = model.selectedVehicle });
        }

        [HttpPost]
        public ActionResult EditFuelReceipt(HarpoonFuelReportingViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var reciept = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == model.EditfuelRecipt.vehicle_auto_id && x.client_id == model.EditfuelRecipt.client_id && x.transaction_number == model.EditfuelRecipt.transaction_number && x.entry_user == model.EditfuelRecipt.entry_user).FirstOrDefault();
            Trace.WriteLine("Reached Edit" + model.EditfuelRecipt.vehicle_auto_id + ";" + model.EditfuelRecipt.client_id + ";" + model.EditfuelRecipt.transaction_number + ";" + model.EditfuelRecipt.entry_user);
            if (reciept != null)
            {
                Trace.WriteLine("Reached Edit recipt");
                reciept.odometer = model.EditfuelRecipt.odometer;
                reciept.no_of_liters = model.EditfuelRecipt.no_of_liters;
                reciept.price_per_liter = model.EditfuelRecipt.price_per_liter;
                reciept.charge_type = model.EditfuelRecipt.charge_type;
                reciept.comments = model.EditfuelRecipt.comments;
                db.SaveChanges();
            }
            return RedirectToAction("FuelReporting", new { vid = model.selectedVehicle });
        }

        [HttpPost]
        public ActionResult DeleteFuelReceipt(HarpoonFuelReportingViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var reciept = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == model.DeletefuelRecipt.vehicle_auto_id && x.client_id == model.DeletefuelRecipt.client_id && x.transaction_number == model.DeletefuelRecipt.transaction_number && x.entry_user == model.DeletefuelRecipt.entry_user).FirstOrDefault();
            if (reciept != null)
            {
                db.tbl_harpoon_fuel_log.Remove(reciept);
                db.SaveChanges();
            }
            return RedirectToAction("FuelReporting", new { vid = model.selectedVehicle });
        }

        [HttpPost]
        public ActionResult FlagFuelReceipt(HarpoonFuelReportingViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var reciept = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == model.FlagfuelRecipt.vehicle_auto_id && x.client_id == model.FlagfuelRecipt.client_id && x.transaction_number == model.FlagfuelRecipt.transaction_number && x.entry_user == model.FlagfuelRecipt.entry_user).FirstOrDefault();
            if (reciept != null)
            {
                reciept.flag = !reciept.flag;
                db.SaveChanges();
            }
            return RedirectToAction("FuelReporting", new { vid = model.selectedVehicle });
        }

        [HttpPost]
        public ActionResult SubmitAuditStatus(HarpoonFuelReportingViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            foreach (var items in model.fuelLog)
            {
                var result = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == items.vehicle_auto_id && x.transaction_number == items.transaction_number && x.client_id == items.client_id && x.entry_user == items.entry_user).FirstOrDefault();
                if (result != null)
                {
                    result.audit_status = items.audit_status;
                    result.flag = items.flag;
                }
            }
            db.SaveChanges();

            return RedirectToAction("FuelReporting", new { vid = model.selectedVehicle });
        }

        public ActionResult SubmitAuditStatusPartial(pullHarpoonFuleLog model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            if (model.fuelLog != null)
            {
                foreach (var items in model.fuelLog)
                {
                    var result = db.tbl_harpoon_fuel_log.Where(x => x.vehicle_auto_id == items.vehicle_auto_id && x.transaction_number == items.transaction_number && x.client_id == items.client_id && x.entry_user == items.entry_user).FirstOrDefault();
                    if (result != null)
                    {
                        result.audit_status = items.audit_status;
                        result.flag = items.flag;
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("FuelReporting", new { vid = model.selectedVehicle });
        }

        public ActionResult ChangeVehicalFuelLog(string loc, string vid)
        {
            return RedirectToAction("FuelReporting", new { loc = loc, vid = vid, });
        }

        private static List<SelectListItem> populateChargeAccounts(string clientID)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var chargeAccounts = db.tbl_harpoon_fuelReporting_chargeAccounts.Where(x => x.client_id == clientID).ToList();

            foreach (var act in chargeAccounts)
            {
                items.Add(new SelectListItem
                {
                    Text = act.charge_account,
                    Value = act.charge_account
                });
            }

            return items;
        }

    }
}