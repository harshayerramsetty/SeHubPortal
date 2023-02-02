using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Net.Mail;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Web.UI;


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
        public string pullConfigImage(string type, string con)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var configuration = db.tbl_fleettvt_configurations.Where(x => x.Type == type && x.Configuration == con).FirstOrDefault();

            string base64ProfilePic = "";
            if (configuration.configuration_image is null)
            {
                base64ProfilePic = "";
            }
            else
            {
                base64ProfilePic = "data:image/png;base64," + Convert.ToBase64String(configuration.configuration_image);
            }

            return base64ProfilePic;
        }



        public JsonResult GetSurveyDates(string unit)
        {
            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                var events = dc.tbl_fleetTVT_fieldsurvey_tire.Where(x => x.unit_number == unit).Select(x => x.survey_date).Distinct().ToList();


                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public ActionResult EditUnitInfo(HttpPostedFileBase UnitImage, EditUnitViewModel model)
        {

            int empId = Convert.ToInt32(Session["userID"].ToString());

            byte[] imageBytes = null;
            if (UnitImage != null && UnitImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(UnitImage.FileName);
                //Debug.WriteLine("EmployeeImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;

                using (System.Drawing.Image image = System.Drawing.Image.FromStream(UnitImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        //Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var UnitInfo = db.tbl_fleetTVT_unit.Where(a => a.unit_number == model.Unit.unit_number).FirstOrDefault();

            UnitInfo.unit_name = model.Unit.unit_name;
            UnitInfo.survey_type = model.Unit.survey_type;
            UnitInfo.survey_configuration = model.Unit.survey_configuration;
            UnitInfo.year = model.Unit.year;
            UnitInfo.make = model.Unit.make;
            UnitInfo.model = model.Unit.model;
            UnitInfo.plate_number = model.Unit.plate_number;
            UnitInfo.mileage = model.Unit.mileage;
            UnitInfo.hours = model.Unit.hours;
            UnitInfo.length = model.Unit.length;
            UnitInfo.date_expired = model.Unit.date_expired;
            UnitInfo.spec_psi_1 = model.Unit.spec_psi_1;
            UnitInfo.spec_psi_2 = model.Unit.spec_psi_2;
            UnitInfo.spec_psi_3 = model.Unit.spec_psi_3;
            if (UnitInfo.survey_type == "Trailer")
            {
                UnitInfo.tire_size_3 = model.Unit.tire_size_3;
            }
            else if (UnitInfo.survey_type == "Tractor")
            {
                UnitInfo.tire_size_1 = model.Unit.tire_size_1;
                UnitInfo.tire_size_2 = model.Unit.tire_size_2;
            }
                        
            UnitInfo.short_description = model.Unit.short_description;
            UnitInfo.last_survey = model.Unit.last_survey;
            UnitInfo.Active = model.Unit.Active;
            UnitInfo.VIN = model.Unit.VIN;

            if (imageBytes != null)
            {
                UnitInfo.unit_image = imageBytes;
            }

            db.SaveChanges();
            return RedirectToAction("EditAccount", new { active = false });
        }

        [HttpPost]
        public ActionResult AddUnit(HttpPostedFileBase UnitImage, FleetTvtEditAccount model)
        {

            byte[] imageBytes = null;
            if (UnitImage != null && UnitImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(UnitImage.FileName);
                //Debug.WriteLine("EmployeeImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;

                using (System.Drawing.Image image = System.Drawing.Image.FromStream(UnitImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        //Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_fleetTVT_unit UnitInfo = new tbl_fleetTVT_unit();

            UnitInfo.customer_number = model.Customer;
            UnitInfo.unit_number = model.AddUnit.unit_number;
            UnitInfo.unit_name = model.AddUnit.unit_name;
            UnitInfo.survey_type = model.AddUnit.survey_type;
            UnitInfo.survey_configuration = model.AddUnit.survey_configuration;
            UnitInfo.year = model.AddUnit.year;
            UnitInfo.make = model.AddUnit.make;
            UnitInfo.model = model.AddUnit.model;
            UnitInfo.plate_number = model.AddUnit.plate_number;
            UnitInfo.spec_psi_1 = model.AddUnit.spec_psi_1;
            UnitInfo.spec_psi_2 = model.AddUnit.spec_psi_2;
            UnitInfo.spec_psi_3 = model.AddUnit.spec_psi_3;
            UnitInfo.tire_size_1 = model.AddUnit.tire_size_1;
            UnitInfo.tire_size_2 = model.AddUnit.tire_size_2;
            UnitInfo.tire_size_3 = model.AddUnit.tire_size_3;
            UnitInfo.short_description = model.AddUnit.short_description;
            UnitInfo.last_survey = model.AddUnit.last_survey;
            UnitInfo.VIN = model.AddUnit.VIN;
            UnitInfo.Active = 1;

            if (imageBytes != null)
            {
                UnitInfo.unit_image = imageBytes;
            }

            db.tbl_fleetTVT_unit.Add(UnitInfo);
            db.SaveChanges();
            return RedirectToAction("EditAccount", new { active = false });
        }

        public ActionResult UnitEditForm(EditUnitViewModel model, string UnitNum)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            if(UnitNum != null)
            {
                model.Unit = db.tbl_fleetTVT_unit.Where(x => x.unit_number == UnitNum).FirstOrDefault();
                if (model.Unit.survey_type == "Tractor")
                {
                    model.ConfigurationsList = populateConfigurationsTractor();
                }
                else
                {
                    model.ConfigurationsList = populateConfigurationsTrailer();
                }
            }

            model.SizesList = populateTruckSizes();

            return PartialView(model);
        }

        private static List<SelectListItem> PopulateCustomers()
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //Debug.WriteLine("Inside PopulateCustomers with value:" + value);
            List<SelectListItem> items = new List<SelectListItem>();
            var custList = db.tbl_cta_customers.OrderBy(x => x.CustomerName).ToList();

            foreach(var cust in custList)
            {
                items.Add(new SelectListItem
                {

                    Value = cust.CustomerCode.ToString(),
                    Text = cust.CustomerName
                });
            }

            return items;
        }

        [HttpPost]
        public ActionResult FTVTSelectedCustomer(FleetTvtEditAccount model)
        {
            //Debug.WriteLine("Custoomer details:" + model.Custname);
            return RedirectToAction("EditAccount", new { CustId = model.Customer, active = false});

        }

        [HttpPost]
        public ActionResult FTVTSelectedCustomerSnapShot(FleetTvtEditAccount model)
        {
            //Debug.WriteLine("Custoomer details:" + model.Custname);
            return RedirectToAction("FleetSnapShot", new { CustId = model.Customer, active = false });

        }

        [HttpPost]
        public ActionResult EditCustomerData(HttpPostedFileBase CustImage, FleetTvtEditAccount model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            byte[] imageBytes = null;
            if (CustImage != null && CustImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(CustImage.FileName);
                //Debug.WriteLine("EmployeeImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;

                using (System.Drawing.Image image = System.Drawing.Image.FromStream(CustImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);

                    }
                }
            }

            var CustDetails = db.tbl_fleettvt_Customer.Where(x => x.customer_number == model.Customer).FirstOrDefault();

            CustDetails.reporting_contact = model.reportingContact;
            CustDetails.reporting_email = model.reportingEmail;
            CustDetails.reporting_frequency = model.reportingFrequency;
            CustDetails.pull_point_1 = model.pullPointDrive;
            CustDetails.pull_point_2 = model.pullPointSteer;
            CustDetails.pull_point_3 = model.pullPointTrailer;

            if(model.mileageRequired == true)
            {
                CustDetails.mileage_required = 1;
            }
            else
            {
                CustDetails.mileage_required = 0;
            }


            if (model.inspectionDateRequired == true)
            {
                CustDetails.exp_date_required = 1;
            }
            else
            {
                CustDetails.exp_date_required = 0;
            }


            if (imageBytes != null)
            {
                CustDetails.logo = imageBytes;
            }

            db.SaveChanges();

            return RedirectToAction("EditAccount", new { active = false});
        }

        [HttpPost]
        public ActionResult AddCustomer(FleetTVT model)
        {

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("IT@citytire.com", "IT Team")); //jordan.blackwood     harsha.yerramsetty     payroll
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.Subject = "FleetTVT New Customer";
            msg.Body = "<i><u><b>Customer Number: </b></u></i>" + model.AddCustomer.custNum +
                "<br /><br />" +
                "<i><u><b>Reporting Contact: </b></u></i>" + model.AddCustomer.reportingContact +
                "<br /><br />" +
                "<i><u><b>Reporting Email: </b></u></i>" + model.AddCustomer.reportingEmail +
                "<br /><br />" +
                "<i><u><b>Mileage Required: </b></u></i>" + model.AddCustomer.mileageRequired +
                "<br /><br />" +
                "<i><u><b>Date Expired: </b></u></i>" + model.AddCustomer.dateExpired +
                "<br /><br />" +
                "<i><u><b>Pull Point - Steer: </b></u></i>" + model.AddCustomer.pullPointSteer +
                "<br /><br />" +
                "<i><u><b>Pull Point - Drive</b></u></i>" + model.AddCustomer.pullPointDrive +
                "<br /><br />" +
                "<i><u><b>Pull Point - Trailer: </b></u></i>" + model.AddCustomer.pullPointTrailer;
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

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public string PullCustomerInfo(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var custDetails = db.tbl_fleettvt_Customer.Where(x => x.customer_number == value).FirstOrDefault();
            var cust_list = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == value).FirstOrDefault();
            if(custDetails != null)
            {
                custDetails.customer_number = cust_list.CustomerName;
                custDetails.fleet_size = db.tbl_fleetTVT_unit.Where(x => x.customer_number == value).Count().ToString();
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
            var cust_list = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == value).FirstOrDefault();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string unitDetailsJson = serializer.Serialize(unitDetails);
            return unitDetailsJson;
        }

        [HttpPost]
        public string pullUnitLatestSurvey(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var unitLatestSurveyDetails = db.tbl_fleetTVT_fieldsurvey_unit.Where(x => x.unit_number == value).OrderBy(x => x.survey_date).FirstOrDefault();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string unitDetailsJson = serializer.Serialize(unitLatestSurveyDetails);
            return unitDetailsJson;
        }

        [HttpPost]
        public ActionResult PullUnits(string value)
        {
            string customerNumber = value;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_fleetTVT_unit.Where(x => x.customer_number == customerNumber && x.Active == 1).ToList();

            return Json(units.Select(x => new
            {
                value = x.unit_number,
                text = x.unit_number
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult PullEmployeesChangeLocation(string loc)
        {
            Trace.WriteLine("Reached till here " + loc);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_employee.Where(x => x.status == 1 && x.loc_ID == loc).OrderBy(x => x.full_name).ToList();

            return Json(units.Select(x => new
            {
                value = x.employee_id,
                text = x.full_name
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public string GetConfigurationDetails(string type, string config)
        {
            //Debug.WriteLine("This is the type : " + type + " and this is the configuration " + config);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var configur = db.tbl_fleettvt_configurations.Where(x => x.Type == type && x.Configuration == config).FirstOrDefault();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string ConfigurationDetails = serializer.Serialize(configur);
            return ConfigurationDetails;
        }

        public string GetTireInfo(string customer, string unit, string date, string pos)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var tire = db.tbl_fleetTVT_fieldsurvey_tire.Where(x => x.customer_number == customer && x.unit_number == unit && x.survey_date.ToString() == date && x.position == pos).FirstOrDefault();

            string TireDetails = serializer.Serialize(tire);
            return TireDetails;
        }

        public string PullEquipmentSurvey(string customer, string unit, string date)
        {


            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var tire = db.tbl_fleetTVT_fieldsurvey_unit.Where(x => x.customer_number == customer && x.unit_number == unit && x.survey_date.ToString() == date).FirstOrDefault();

            string EqpDetails = serializer.Serialize(tire);
            return EqpDetails;
        }

        [HttpPost]
        public string GetLatestSurvey(string unit)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var date = db.tbl_fleetTVT_fieldsurvey_tire.Where(x => x.unit_number == unit).OrderByDescending(x => x.survey_date).Select(x => x.survey_date).FirstOrDefault();
            var unitInfo = db.tbl_fleetTVT_fieldsurvey_tire.Where(x => x.unit_number == unit && x.survey_date == date).ToList();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string LatestSurvey = serializer.Serialize(unitInfo);

            return LatestSurvey;
        }

        [HttpPost]
        public string InsertRecordToFleetSurveyTire(string cust, string unt, string dat, string pn, string pres, string trdDpth, string wr, string brnd, string mdl, string sze, string vlv, string com, string wc, string tc)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_fleetTVT_fieldsurvey_tire tireDetails = new tbl_fleetTVT_fieldsurvey_tire();

            tireDetails.customer_number = cust;
            tireDetails.unit_number = unt;
            tireDetails.survey_date = Convert.ToDateTime(dat);
            tireDetails.position = pn;
            if (pres != "n/a")
            {
                tireDetails.actual_psi = Convert.ToInt32(pres);
            }
            tireDetails.actual_32nds = Convert.ToInt32(trdDpth);
            tireDetails.condition_wear = wr;
            tireDetails.brand = brnd;
            tireDetails.model = mdl;
            tireDetails.size = sze;
            tireDetails.valve = vlv;
            tireDetails.comments = com;
            tireDetails.condition_wheel = wc;
            tireDetails.condition_tire = tc;


            db.tbl_fleetTVT_fieldsurvey_tire.Add(tireDetails);
            db.SaveChanges();

            return "";
        }

        [HttpPost]
        public string UpdateRecordToFleetSurveyTire(string cust, string unt, string dat, string pn, string pres, string trdDpth, string wr, string brnd, string mdl, string sze, string com)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            DateTime surdat = Convert.ToDateTime(dat);

            //Trace.WriteLine("Cust: " + cust + " Unit: " + unt + " date: " + surdat + " posit: " + pn);

            var tire = db.tbl_fleetTVT_fieldsurvey_tire.Where(x => x.customer_number == cust && x.unit_number == unt && x.survey_date == surdat && x.position == pn).FirstOrDefault();

            tire.actual_psi = Convert.ToInt32(pres);
            tire.actual_32nds = Convert.ToInt32(trdDpth);
            tire.condition_wear = wr;
            tire.brand = brnd;
            tire.model = mdl;
            tire.size = sze;
            tire.comments = com;

            //Trace.WriteLine("This is the brand " + brnd);

            db.SaveChanges();

            return "";
        }

        [HttpPost]
        public string InsertRecordToFleetSurveyUnit(string cu, string un, string loct, string empl, string cmts, string sdat)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_fleetTVT_fieldsurvey_unit unitSurveyDetails = new tbl_fleetTVT_fieldsurvey_unit();

            unitSurveyDetails.customer_number = cu;

            unitSurveyDetails.unit_number = un;


            if (loct != null)
            {
                unitSurveyDetails.survey_location = loct;
            }
            

            if (sdat != null)
            {
                unitSurveyDetails.survey_date = Convert.ToDateTime(sdat);
            }
            

            if (empl != null)
            {
                unitSurveyDetails.employee_id = Convert.ToInt32(empl);
            }
            

            if (cmts != null)
            {
                unitSurveyDetails.comments = cmts;
            }
            

            db.tbl_fleetTVT_fieldsurvey_unit.Add(unitSurveyDetails);
            db.SaveChanges();

            return "";
        }

        [HttpPost]
        public string UpdateRecordToFleetSurveyUnit(string cu, string un, string mlg, string loct, string dat, string empl, string cmts, string sdat)
        {

            Trace.WriteLine("This is the date " + sdat);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            DateTime surdate = Convert.ToDateTime(sdat);

            var unitSurveyDetails = db.tbl_fleetTVT_fieldsurvey_unit.Where(x => x.customer_number == cu && x.unit_number == un && x.survey_date == surdate).FirstOrDefault();

            if (mlg != "")
            {
                //unitSurveyDetails.mileage = Convert.ToInt32(mlg);
            }

            if (loct != null)
            {
                unitSurveyDetails.survey_location = loct;
            }
            

            if (sdat != null)
            {
                unitSurveyDetails.survey_date = Convert.ToDateTime(sdat);
            }
            

            if (empl != null)
            {
                unitSurveyDetails.employee_id = Convert.ToInt32(empl);
            }
            

            if (dat != "")
            {
                //unitSurveyDetails.date_expired = Convert.ToDateTime(dat);
            }
            

            if (cmts != null)
            {
                unitSurveyDetails.comments = cmts;
            }

            db.SaveChanges();

            return "";
        }



        public ActionResult Dashboard(FleetTVT model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            var cust_data = db.tbl_fleettvt_Customer;

            var cust_table = new List<FleetTVTDashboardCustomers>();

            foreach(var item in cust_data)
            {
                var customer = new FleetTVTDashboardCustomers();
                var cust_list = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == item.customer_number).FirstOrDefault();
                var Units = db.tbl_fleetTVT_unit.Where(x => x.customer_number == item.customer_number).ToList();

                if (cust_list != null)
                {
                    customer.customer_name = cust_list.CustomerName;
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
            model.CustList = PopulateCustomers();


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
            CloudBlobContainer container = blobClient.GetContainerReference("cta-library-company-documents");

            // Retrieve reference to a blob ie "picture.jpg".

            model.URL = container.GetBlockBlobReference("Fleet TVT - Survey Field Sheet.pdf").Uri.AbsoluteUri;
            model.UnitURI = container.GetBlockBlobReference("Fleet TVT - New Unit Field Sheet.pdf").Uri.AbsoluteUri;
            Debug.WriteLine(model.URL);

            model.CustomerList = populateCustomers();

            return View(model);
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

        public ActionResult FieldSurveyEditAccount(Field_Survey_Edit_Account model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
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
            model.LocationList = populateLocationsPermissions(empId);
            model.WearList = populateWears();
            model.ValveList = populateValve();
            model.TireConditionList = populateTireCondition();
            model.WheelConditionList = populateWheelCondition();
            model.ModelList = populateModel();


            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            return View(model);
        }

        public ActionResult EditAccount(FleetTvtEditAccount model, string custID, bool active)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            model.CustomerList = populateCustomers();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (custID != null)
            {
                model.Customer = custID;
                model.customerDetails = db.tbl_fleettvt_Customer.Where(x => x.customer_number == custID).FirstOrDefault();
                
                if(model.customerDetails != null)
                {
                    model.reportingContact = model.customerDetails.reporting_contact;
                    model.reportingEmail = model.customerDetails.reporting_email;
                    model.reportingFrequency = model.customerDetails.reporting_frequency;
                    model.image = model.customerDetails.logo;

                    if (model.customerDetails.mileage_required == 1)
                    {
                        model.mileageRequired = true;
                    }
                    else
                    {
                        model.mileageRequired = false;
                    }

                    if (model.customerDetails.exp_date_required == 1)
                    {
                        model.inspectionDateRequired = true;
                    }
                    else
                    {
                        model.inspectionDateRequired = false;
                    }

                    if (model.customerDetails.pull_point_1.HasValue)
                    {
                        model.pullPointSteer = model.customerDetails.pull_point_1.Value;
                    }
                    else
                    {
                        model.pullPointSteer = 0;
                    }

                    if (model.customerDetails.pull_point_2.HasValue)
                    {
                        model.pullPointDrive = model.customerDetails.pull_point_2.Value;
                    }
                    else
                    {
                        model.pullPointDrive = 0;
                    }

                    if (model.customerDetails.pull_point_3.HasValue)
                    {
                        model.pullPointTrailer = model.customerDetails.pull_point_3.Value;
                    }
                    else
                    {
                        model.pullPointTrailer = 0;
                    }


                    model.customerDetails.fleet_size = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1).Count().ToString();
                }

                

                if (active != null)
                {
                    if (active == true)
                    {
                        model.UnitsForCustomer = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 0).ToList();
                        model.tractorCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 0 && x.survey_type == "Tractor").Count();
                        model.trailerCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 0 && x.survey_type == "Trailer").Count();
                        model.Active = true;
                    }
                    else
                    {
                        model.UnitsForCustomer = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1).ToList();
                        model.tractorCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1 && x.survey_type == "Tractor").Count();
                        model.trailerCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1 && x.survey_type == "Trailer").Count();
                        model.Active = false;
                    }
                }

                model.SteerTiresCount = 0;
                model.DriveTiresCount = 0;
                model.trailerTiresCount = 0;

                foreach(var unit in model.UnitsForCustomer)
                {
                    tbl_fleettvt_configurations config = new tbl_fleettvt_configurations();

                    config = db.tbl_fleettvt_configurations.Where(x => x.Type == unit.survey_type && x.Configuration == unit.survey_configuration).FirstOrDefault();

                    if(config != null)
                    {
                        if (config.one_li == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.one_li == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.one_li == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }


                        if (config.one_lo == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.one_lo == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.one_lo == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.one_ri == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.one_ri == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.one_ri == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.one_ro == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.one_ro == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.one_ro == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.two_li == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.two_li == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.two_li == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.two_lo == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.two_lo == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.two_lo == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.two_ri == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.two_ri == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.two_ri == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.two_ro == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.two_ro == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.two_ro == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }


                        if (config.three_li == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.three_li == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.three_li == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }


                        if (config.three_lo == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.three_lo == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.three_lo == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.three_ri == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.three_ri == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.three_ri == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.three_ro == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.three_ro == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.three_ro == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.four_li == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.four_li == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.four_li == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.four_lo == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.four_lo == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.four_lo == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.four_ri == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.four_ri == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.four_ri == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.four_ro == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.four_ro == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.four_ro == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.five_li == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.five_li == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.five_li == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.five_lo == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.five_lo == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.five_lo == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.five_ri == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.five_ri == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.five_ri == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }

                        if (config.five_ro == 1)
                        {
                            model.SteerTiresCount = model.SteerTiresCount + 1;
                        }
                        else if (config.five_ro == 2)
                        {
                            model.DriveTiresCount = model.DriveTiresCount + 1;
                        }
                        else if (config.five_ro == 3)
                        {
                            model.trailerTiresCount = model.trailerTiresCount + 1;
                        }
                    }

                }
            }
            else
            {

            }

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.fleetTvt_EditAccount == 0)
            {
                return RedirectToAction("Dashboard", "Management");
            }

            model.CustomerList = populateCustomers();
            model.LocationList = populateLocationsPermissions(empId);

            model.ConfigurationsListTractor = populateConfigurationsTractor();
            model.ConfigurationsListTrailer = populateConfigurationsTrailer();

            model.SizesList = populateTruckSizes();

            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public ActionResult ActiveInaciveUnits (FleetTvtEditAccount model)
        {
            return RedirectToAction("EditAccount", new { CustId = model.Customer, active = model.Active });
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

        [HttpPost]
        public ActionResult PullConfig(string type)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            Trace.WriteLine("This is the type" + type);
            var units = db.tbl_fleettvt_configurations.Where(x => x.Type == type).ToList();

            return Json(units.Select(x => new
            {
                value = x.Configuration,
                text = x.Configuration
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        private static List<SelectListItem> populateConfigurationsTrailer()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_fleettvt_configurations.Where(x => x.Type == "Trailer").ToList();

            foreach (var val in config)
            {
                //Trace.WriteLine("This is the configuration" + val.Configuration);
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

            var config = db.tbl_source_mediumTruckTires.OrderBy(x => x.brand).Select(x => x.brand).Distinct().ToList();

            items.Add(new SelectListItem
            {
                Text = "CTA Retread",
                Value = "CTA Retread"
            });

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = Convert.ToString(val)
                });
            }

            items.Add(new SelectListItem
            {
                Text = "Other",
                Value = "Other"
            });

            return items;
        }

        private static List<SelectListItem> populateLocations()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locs = db.tbl_cta_location_info.Select(x => x.loc_id ).ToList();

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

        public string SelectModel(string brand, string size)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            List<string> model = db.tbl_source_mediumTruckTires.Where(x => x.brand == brand).Select(x => x.model).Distinct().ToList();
            model.Add("Other");

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string custDetailsJson = serializer.Serialize(model);

            return custDetailsJson;
        }

        private static List<SelectListItem> populateModel()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var model = db.tbl_source_mediumTruckTires.Select(x => x.model).Distinct().ToList();

            foreach (var val in model)
            {
                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = val
                });
            }

            items.Add(new SelectListItem
            {
                Text = "Other",
                Value = "Other"
            });


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

        private static List<SelectListItem> populateTruckSizes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_source_truckTire_sizes.Where(x => x.retread == 1).ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.tire_size,
                    Value = Convert.ToString(val.tire_size)
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
                var custDetails = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == val.customer_number).FirstOrDefault();
                items.Add(new SelectListItem
                {
                    Text = custDetails.CustomerName +" ("+ val.customer_number + ")" ,
                    Value = Convert.ToString(val.customer_number)
                });
            }
            return items;
        }

        public ActionResult ViewOrEditSurvey(Field_Survey_Edit_Account model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;
            model.CustomerList = populateCustomers();
            model.ConfigurationsListTractor = populateConfigurationsTractor();
            model.ConfigurationsListTrailer = populateConfigurationsTrailer();
            model.BrandList = populateBrands();
            model.SizeList = populateSizes();
            model.LocationList = populateLocationsPermissions(empId);
            model.WearList = populateWears();
            model.ValveList = populateValve();
            model.TireConditionList = populateTireCondition();
            model.WheelConditionList = populateWheelCondition();
            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            return View(model);

        }

        public ActionResult FleetSnapShot(FleetTvtEditAccount model, string custID, bool active)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            model.CustomerList = populateCustomers();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (custID != null)
            {
                model.Customer = custID;
                model.customerDetails = db.tbl_fleettvt_Customer.Where(x => x.customer_number == custID).FirstOrDefault();

                if (model.customerDetails != null)
                {
                    model.reportingContact = model.customerDetails.reporting_contact;
                    model.reportingEmail = model.customerDetails.reporting_email;
                    model.reportingFrequency = model.customerDetails.reporting_frequency;

                    if (model.customerDetails.mileage_required == 1)
                    {
                        model.mileageRequired = true;
                    }
                    else
                    {
                        model.mileageRequired = false;
                    }
                    model.customerDetails.fleet_size = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1).Count().ToString();
                }

                if (active)
                {
                    model.UnitsForCustomer = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 0).ToList();
                    model.tractorCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 0 && x.survey_type == "Tractor").Count();
                    model.trailerCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 0 && x.survey_type == "Trailer").Count();
                    model.Active = true;
                }
                else
                {
                    model.UnitsForCustomer = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1).ToList();
                    model.tractorCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1 && x.survey_type == "Tractor").Count();
                    model.trailerCount = db.tbl_fleetTVT_unit.Where(x => x.customer_number == custID && x.Active == 1 && x.survey_type == "Trailer").Count();
                    model.Active = false;
                }

                model.SteerTiresCount = 0;
                model.DriveTiresCount = 0;
                model.trailerTiresCount = 0;

                foreach (var unit in model.UnitsForCustomer)
                {
                    tbl_fleettvt_configurations config = new tbl_fleettvt_configurations();

                    config = db.tbl_fleettvt_configurations.Where(x => x.Type == unit.survey_type && x.Configuration == unit.survey_configuration).FirstOrDefault();

                    if (config != null)
                    {
                        model.SteerTiresCount += config.steerTiresCount.Value;
                        model.DriveTiresCount += config.driveTiresCount.Value;
                        model.trailerTiresCount += config.trailerTiresCount.Value;
                    }

                }

                var surveyItems = db.tbl_fleetTVT_fieldsurvey_tire.ToList();

                List<tbl_fleetTVT_fieldsurvey_tire> listFildSurvey = new List<tbl_fleetTVT_fieldsurvey_tire>();

                foreach (var survey in surveyItems)
                {
                    tbl_fleetTVT_fieldsurvey_tire fieldSurvey = new tbl_fleetTVT_fieldsurvey_tire();
                    fieldSurvey = survey;

                    var brand = db.tbl_source_mediumTruckTires.Where(x => x.brand == survey.brand && x.model == survey.model).FirstOrDefault();
                    if (brand != null)
                    {
                        fieldSurvey.new_32nds = Convert.ToDouble(brand.original32nds);
                    }

                    Trace.WriteLine("This is the Unit number " + survey.unit_number);

                    var unit = db.tbl_fleetTVT_unit.Where(x => x.unit_number == survey.unit_number).FirstOrDefault();

                    //Trace.WriteLine("This is the survey type " + unit.survey_type + " And this is the config " + unit.survey_configuration);

                    var config = db.tbl_fleettvt_configurations.Where(x => x.Type == unit.survey_type && x.Configuration == unit.survey_configuration).FirstOrDefault();

                    double tire_type;

                    if (survey.position == "1_lo")
                    {
                        tire_type = config.one_lo.Value;
                    }
                    else if (survey.position == "1_li")
                    {
                        tire_type = config.one_li.Value;
                    }
                    else if (survey.position == "1_ri")
                    {
                        tire_type = config.one_ri.Value;
                    }
                    else if (survey.position == "1_ro")
                    {
                        tire_type = config.one_ro.Value;
                    }
                    else if (survey.position == "2_lo")
                    {
                        tire_type = config.two_lo.Value;
                    }
                    else if (survey.position == "2_li")
                    {
                        tire_type = config.two_li.Value;
                    }
                    else if (survey.position == "2_ri")
                    {
                        tire_type = config.two_ri.Value;
                    }
                    else if (survey.position == "2_ro")
                    {
                        tire_type = config.two_ro.Value;
                    }
                    else if (survey.position == "3_lo")
                    {
                        tire_type = config.three_lo.Value;
                    }
                    else if (survey.position == "3_li")
                    {
                        tire_type = config.three_li.Value;
                    }
                    else if (survey.position == "3_ri")
                    {
                        tire_type = config.three_ri.Value;
                    }
                    else if (survey.position == "3_ro")
                    {
                        tire_type = config.three_ro.Value;
                    }
                    else if (survey.position == "4_lo")
                    {
                        tire_type = config.four_lo.Value;
                    }
                    else if (survey.position == "4_li")
                    {
                        tire_type = config.four_li.Value;
                    }
                    else if (survey.position == "4_ri")
                    {
                        tire_type = config.four_ri.Value;
                    }
                    else if (survey.position == "4_ro")
                    {
                        tire_type = config.four_ro.Value;
                    }
                    else if (survey.position == "5_lo")
                    {
                        tire_type = config.five_lo.Value;
                    }
                    else if (survey.position == "5_li")
                    {
                        tire_type = config.five_li.Value;
                    }
                    else if (survey.position == "5_ri")
                    {
                        tire_type = config.five_ri.Value;
                    }
                    else if (survey.position == "5_ro")
                    {
                        tire_type = config.five_ro.Value;
                    }
                    else
                    {
                        tire_type = 0;
                    }


                    if (fieldSurvey.new_32nds.HasValue)
                    {
                        //Trace.WriteLine(tire_type);
                        if (tire_type == 1)
                        {
                            fieldSurvey.percent_worn = Math.Round((1 - (fieldSurvey.actual_32nds.Value - model.customerDetails.pull_point_1.Value) / (fieldSurvey.new_32nds.Value - model.customerDetails.pull_point_1.Value)) * 100, 1);

                        }
                        else if (tire_type == 2)
                        {
                            fieldSurvey.percent_worn = Math.Round((1 - (fieldSurvey.actual_32nds.Value - model.customerDetails.pull_point_2.Value) / (fieldSurvey.new_32nds.Value - model.customerDetails.pull_point_2.Value)) * 100, 1);

                        }
                        else
                        {
                            fieldSurvey.percent_worn = Math.Round((1 - (fieldSurvey.actual_32nds.Value - model.customerDetails.pull_point_3.Value) / (fieldSurvey.new_32nds.Value - model.customerDetails.pull_point_3.Value)) * 100, 1);

                        }
                    }

                    if (tire_type == 1)
                    {
                        fieldSurvey.spec_psi = Convert.ToInt32(unit.spec_psi_1);
                    }
                    else if (tire_type == 2)
                    {
                        fieldSurvey.spec_psi = Convert.ToInt32(unit.spec_psi_2);
                    }
                    else if (tire_type == 3)
                    {
                        fieldSurvey.spec_psi = Convert.ToInt32(unit.spec_psi_3);
                    }
                    else
                    {
                        fieldSurvey.spec_psi = 0;
                    }
           
                    listFildSurvey.Add(fieldSurvey);

                }

                model.LatestSurvey = listFildSurvey;

            }
            else
            {

            }

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.fleetTvt_EditAccount == 0)
            {
                return RedirectToAction("Dashboard", "Management");
            }

            model.CustomerList = populateCustomers();
            model.LocationList = populateLocationsPermissions(empId);

            model.ConfigurationsListTractor = populateConfigurationsTractor();
            model.ConfigurationsListTrailer = populateConfigurationsTrailer();

            model.SizesList = populateTruckSizes();

            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            model.ConfigurationsTable = db.tbl_fleettvt_configurations.ToList();

            model.tireCondition = db.tbl_source_tire_condition.ToList();
            model.wheelCondition = db.tbl_source_wheel_condition.ToList();
            model.tireWear = db.tbl_source_tire_wear.ToList();

            return View(model);
        }

        public string SendPDFEmail(string imgBase64)
        {
            string actual = imgBase64.Substring(22, imgBase64.Length - 22);

            byte[] bytes = Convert.FromBase64String(actual);

            System.Drawing.Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(ms);
            }

            string path1 = Path.Combine(Server.MapPath("~/Content/FleetSnapshot.png"));

            //image.Save(path1, System.Drawing.Imaging.ImageFormat.Png);

            image.Save(path1, System.Drawing.Imaging.ImageFormat.Png);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empId = Convert.ToInt32(Session["userID"].ToString());

            string email = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.cta_email).FirstOrDefault();

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {

                    Document pdfDoc = new Document();
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfDoc.Open();

                        string imageURL = Server.MapPath("~/Content/FleetSnapshot.png");

                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);

                        //jpg.ScaleToFit(10f, 10f);

                        jpg.ScalePercent(45f);

                        pdfDoc.Add(jpg);

                        float w = jpg.ScaledWidth;
                        float h = jpg.ScaledHeight;

                        if (h >= 800)
                        {
                            PdfTemplate t = writer.DirectContent.CreateTemplate(w, h - 800);
                            t.AddImage(jpg, w, 0, 0, h, 0, 0);
                            iTextSharp.text.Image clipped = iTextSharp.text.Image.GetInstance(t);
                            pdfDoc.Add(clipped);
                        }

                        if(h >= 1600)
                        {
                            PdfTemplate t1 = writer.DirectContent.CreateTemplate(w, h - 1600);
                            t1.AddImage(jpg, w, 0, 0, h, 0, 0);
                            iTextSharp.text.Image clipped1 = iTextSharp.text.Image.GetInstance(t1);
                            pdfDoc.Add(clipped1);
                        }

                        if (h >= 2400)
                        {
                            PdfTemplate t2 = writer.DirectContent.CreateTemplate(w, h - 2400);
                            t2.AddImage(jpg, w, 0, 0, h, 0, 0);
                            iTextSharp.text.Image clipped2 = iTextSharp.text.Image.GetInstance(t2);
                            pdfDoc.Add(clipped2);
                        }

                        if (h >= 3200)
                        {
                            PdfTemplate t3 = writer.DirectContent.CreateTemplate(w, h - 3200);
                            t3.AddImage(jpg, w, 0, 0, h, 0, 0);
                            iTextSharp.text.Image clipped3 = iTextSharp.text.Image.GetInstance(t3);
                            pdfDoc.Add(clipped3);
                        }

                        if (h >= 4000)
                        {
                            PdfTemplate t4 = writer.DirectContent.CreateTemplate(w, h - 4000);
                            t4.AddImage(jpg, w, 0, 0, h, 0, 0);
                            iTextSharp.text.Image clipped4 = iTextSharp.text.Image.GetInstance(t4);
                            pdfDoc.Add(clipped4);
                        }

                        if (h >= 4800)
                        {
                            PdfTemplate t5 = writer.DirectContent.CreateTemplate(w, h - 4800);
                            t5.AddImage(jpg, w, 0, 0, h, 0, 0);
                            iTextSharp.text.Image clipped5 = iTextSharp.text.Image.GetInstance(t5);
                            pdfDoc.Add(clipped5);
                        }

                        pdfDoc.Close();
                        byte[] bytespdf = memoryStream.ToArray();
                        memoryStream.Close();

                        MailMessage mm = new MailMessage();
                        mm.To.Add(new MailAddress(email, "IT Team")); //jordan.blackwood      harsha.yerramsetty      payroll
                        mm.From = new MailAddress("noreply@citytire.com", "Sehub");
                        mm.Subject = "";
                        mm.Body = "";
                        mm.Attachments.Add(new Attachment(new MemoryStream(bytespdf), "FleetSnapshot.pdf"));
                        
                        mm.IsBodyHtml = true;

                        SmtpClient client = new SmtpClient();
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
                        client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
                        client.Host = "smtp.office365.com";
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.EnableSsl = true;
                        try
                        {
                            client.Send(mm);
                            //Debug.WriteLine("Message Sent Succesfully");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }



                    }
                }
            }

            return "Successfully sent email";
            
        }

    }
}