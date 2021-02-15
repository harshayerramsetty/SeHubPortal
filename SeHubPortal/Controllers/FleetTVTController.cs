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
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Net.Mail;

namespace SeHubPortal.Controllers
{
    public class FleetTVTController : Controller
    {
        // GET: FleetTVT
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string PullCustomerInfo(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var custDetails = db.tbl_fleettvt_Customer.Where(x => x.customer_number == value).FirstOrDefault();
            var cust_list = db.tbl_customer_list.Where(x => x.cust_us1 == value).FirstOrDefault();
            if(custDetails != null)
            {
                custDetails.customer_number = cust_list.cust_name;
            }
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string custDetailsJson = serializer.Serialize(custDetails);

            return custDetailsJson;
        }

        [HttpPost]
        public string pullUnitInfo(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var unitDetails = db.tbl_fleetTVT_unit.Where(x => x.unit_number == value).FirstOrDefault();
            var cust_list = db.tbl_customer_list.Where(x => x.cust_us1 == value).FirstOrDefault();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string unitDetailsJson = serializer.Serialize(unitDetails);
            return unitDetailsJson;
        }

        [HttpPost]
        public ActionResult PullUnits(string value)
        {
            string customerNumber = value;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_fleetTVT_unit.Where(x => x.customer_number == customerNumber).ToList();

            return Json(units.Select(x => new
            {
                value = x.unit_number,
                text = x.unit_number
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult PullEmployeesChangeLocation(string value)
        {
            string Loaction = value;

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_employee.Where(x => x.loc_ID == Loaction && x.status == 1).OrderBy(x => x.full_name).ToList();

            return Json(units.Select(x => new
            {
                value = x.employee_id,
                text = x.full_name
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
        

        [HttpPost]
        public string GetConfigurationDetails(string type, string config)
        {
            Debug.WriteLine("This is the type : " + type + " and this is the configuration " + config);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var configur = db.tbl_fleettvt_configurations.Where(x => x.Type == type && x.Configuration == config).FirstOrDefault();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string ConfigurationDetails = serializer.Serialize(configur);
            return ConfigurationDetails;
        }

        [HttpPost]
        public string InsertRecordToFleetSurveyTire(string cust, string unt, string dat, string pn, string pres, string trdDpth, string wr, string brnd, string mdl, string sze, string vlv, string com)
        {
            Debug.WriteLine(cust);
            Debug.WriteLine(unt);
            Debug.WriteLine(dat);
            Debug.WriteLine(pn);
            Debug.WriteLine(pres);
            Debug.WriteLine(trdDpth);
            Debug.WriteLine(wr);
            Debug.WriteLine(brnd);
            Debug.WriteLine(mdl);
            Debug.WriteLine(sze);
            Debug.WriteLine(vlv);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_fleetTVT_fieldsurvey_tire tireDetails = new tbl_fleetTVT_fieldsurvey_tire();

            tireDetails.customer_number = cust;
            tireDetails.unit_number = unt;
            tireDetails.survey_date = Convert.ToDateTime(dat);
            tireDetails.position = pn;
            tireDetails.actual_psi = Convert.ToInt32(pres);
            tireDetails.actual_32nds = Convert.ToInt32(trdDpth);
            tireDetails.condition_wear = wr;
            tireDetails.brand = brnd;
            tireDetails.model = mdl;
            tireDetails.size = sze;
            tireDetails.valve = vlv;
            tireDetails.comments = com;


            db.tbl_fleetTVT_fieldsurvey_tire.Add(tireDetails);
            db.SaveChanges();

            return "";
        }

        [HttpPost]
        public string InsertRecordToFleetSurveyUnit(string cu, string un, string mlg, string loct, string dat, string empl, string cmts)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_fleetTVT_fieldsurvey_unit unitSurveyDetails = new tbl_fleetTVT_fieldsurvey_unit();


            unitSurveyDetails.customer_number = cu;
            unitSurveyDetails.unit_number = un;
            unitSurveyDetails.mileage = Convert.ToInt32(mlg);
            unitSurveyDetails.survey_location = loct;
            unitSurveyDetails.survey_date = System.DateTime.Today;
            unitSurveyDetails.employee_id = Convert.ToInt32(empl);
            unitSurveyDetails.date_expired = Convert.ToDateTime(dat);
            unitSurveyDetails.comments = cmts;


            db.tbl_fleetTVT_fieldsurvey_unit.Add(unitSurveyDetails);
            db.SaveChanges();

            return "";
        }


        public ActionResult Dashboard(FleetTVT model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            var cust_data = db.tbl_fleettvt_Customer;

            var cust_table = new List<FleetTVTDashboardCustomers>();

            foreach(var item in cust_data)
            {
                var customer = new FleetTVTDashboardCustomers();
                var cust_list = db.tbl_customer_list.Where(x => x.cust_us1 == item.customer_number).FirstOrDefault();
                var Units = db.tbl_fleetTVT_unit.Where(x => x.customer_number == item.customer_number).ToList();

                if (cust_list != null)
                {
                    customer.customer_name = cust_list.cust_name;
                }
                else {
                    customer.customer_name = "Blackwood, Jordan";
                }
                customer.customer_number = item.customer_number;
                customer.fleet_size = Convert.ToString(Units.Count());
                customer.reporting_contact = item.reporting_contact;
                customer.reporting_email = item.reporting_email;
                customer.reporting_frequency = item.reporting_frequency;

                cust_table.Add(customer);
            }


            model.customer_table = cust_table;
            model.SehubAccess = empDetails;

            if (model.SehubAccess.fleetTVT == 0)
            {
                return RedirectToAction("Dashboard", "Management");
            }

            if (model.SehubAccess.fleetTvt_dashboard == 0)
            {
                return RedirectToAction("FieldSurveyEditAccount", "FleetTVT");
            }

            model.ConfigurationsListTractor = populateConfigurationsTractor();
            model.ConfigurationsListTrailer = populateConfigurationsTrailer();
            


            (model.Name, model.URL) = Company_Documents();
            Debug.WriteLine(model.URL);

            return View(model);
        }

        public String ContainerNameCompanyDocuments = "cta-library-company-documents";
        [HttpGet]
        public (string, string) Company_Documents()
        {
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCompanyDocuments);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();

            //var blobList = blockBlob.ToList();

            string filName = "";
            string filUrl = "";


            foreach (var blob in blockBlob)
            {

                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");
                //Debug.WriteLine(blobFileName);
                if(blobFileName == "Fleet Field Survey ")
                {                    
                    filUrl = Convert.ToString(newUri);
                    filName = blobFileName;
                    //Debug.WriteLine(filUrl);
                }                
            }
            
            return (filName, filUrl);
        }

        public ActionResult FieldSurveyEditAccount(Field_Survey_Edit_Account model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.fleetTvt_fieldSurvey == 0)
            {
                return RedirectToAction("EditAccount", "FleetTVT");
            }

            model.CustomerList = populateCustomers();
            model.ConfigurationsListTractor = populateConfigurationsTractor();
            model.ConfigurationsListTrailer = populateConfigurationsTrailer();
            model.BrandList = populateBrands();
            model.SizeList = populateSizes();
            model.LocationList = populateLocations();
            model.WearList = populateWears();
            model.ValveList = populateValve();
            model.TireConditionList = populateTireCondition();
            model.WheelConditionList = populateWheelCondition();


            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            return View(model);
        }

        public ActionResult EditAccount(FleetTvtEditAccount model)
        {
            model.CustomerList = populateCustomers();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.fleetTvt_EditAccount == 0)
            {
                return RedirectToAction("Dashboard", "Management");
            }

            return View(model);
        }

        private static List<SelectListItem> populateConfigurationsTractor()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_fleettvt_configurations.Where(x => x.Type == "Tractor").ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.Configuration,
                    Value = Convert.ToString(val.Configuration)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateConfigurationsTrailer()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_fleettvt_configurations.Where(x => x.Type == "Trailer").ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.Configuration,
                    Value = Convert.ToString(val.Configuration)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateBrands()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_commercial_tire_manufacturers.Where(x => x.fleetTVT == 1).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.manufacturers,
                    Value = Convert.ToString(val.manufacturers)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateLocations()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locs = db.tblLocations.Select(x => x.locID ).ToList();

            foreach (var loc in locs)
            {
                if (loc != "008") {
                    items.Add(new SelectListItem
                    {
                        Text = loc,
                        Value = loc
                    });
                }
                
            }
            return items;
        }

        private static List<SelectListItem> populateValve()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_source_valve.Select(x => x.valve).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = val
                });
            }
            return items;
        }

        private static List<SelectListItem> populateTireCondition()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_source_tire_condition.Select(x => x.tire_condition).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = val
                });
            }
            return items;
        }

        private static List<SelectListItem> populateWheelCondition()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_source_wheel_condition.Select(x => x.wheel_condition).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = val
                });
            }
            return items;
        }

        private static List<SelectListItem> populateWears()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_source_tire_wear.Select(x => x.tire_wear).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = val
                });
            }
            return items;
        }

        private static List<SelectListItem> populateSizes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_commercial_tire_sizes.Where(x => x.fleetTVT == 1).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.sizes,
                    Value = Convert.ToString(val.sizes)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateCustomers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_fleettvt_Customer.ToList();

            items.Add(new SelectListItem
            {
                Text = "Please Select Customer",
                Value = "Please Select Customer"
            });

            foreach (var val in config)
            {
                var custDetails = db.tbl_customer_list.Where(x => x.cust_us1 == val.customer_number).FirstOrDefault();
                items.Add(new SelectListItem
                {
                    Text = custDetails.cust_name +" ("+ val.customer_number + ")" ,
                    Value = Convert.ToString(val.customer_number)
                });
            }
            return items;
        }

        public ActionResult ViewOrEditSurvey(Field_Survey_Edit_Account model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;
            model.CustomerList = populateCustomers();
            model.ConfigurationsListTractor = populateConfigurationsTractor();
            model.ConfigurationsListTrailer = populateConfigurationsTrailer();
            model.BrandList = populateBrands();
            model.SizeList = populateSizes();
            model.LocationList = populateLocations();
            model.WearList = populateWears();
            model.ValveList = populateValve();
            model.TireConditionList = populateTireCondition();
            model.WheelConditionList = populateWheelCondition();


            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            return View(model);

        }


    }
}