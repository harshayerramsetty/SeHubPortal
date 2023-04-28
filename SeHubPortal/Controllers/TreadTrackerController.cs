using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Net.Mail;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;

namespace SeHubPortal.Controllers
{
    public class TreadTrackerController : Controller
    {
        // GET: TreadTracker
        public ActionResult Index()
        {
            return View();
        }

        public void Update(int pr, int wr, string ac)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            if(ac == "up1" && pr != 1)
            {
                var current = db.tbl_production_schedule.Where(x => x.production_id == wr && x.priority == pr).FirstOrDefault();
                var previous = db.tbl_production_schedule.Where(x => x.priority == (pr - 1)).FirstOrDefault();
                current.priority = current.priority - 1;
                previous.priority = previous.priority + 1;
                db.SaveChanges();
            }
            else if (ac == "down1" && pr != db.tbl_production_schedule.Select(x => x.priority).Max())
            {
                var current = db.tbl_production_schedule.Where(x => x.production_id == wr && x.priority == pr).FirstOrDefault();
                var Next = db.tbl_production_schedule.Where(x => x.priority == (pr + 1)).FirstOrDefault();
                current.priority = current.priority + 1;
                Next.priority = Next.priority - 1;
                db.SaveChanges();
            }
            else if (ac == "top" && pr != 1)
            {
                var current = db.tbl_production_schedule.Where(x => x.production_id == wr && x.priority == pr).FirstOrDefault();
                current.priority = 1;
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE ");
                    sb.Append("tbl_production_schedule ");
                    sb.Append("SET priority = priority + 1");
                    sb.Append("WHERE priority < @priority");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@priority", pr);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();

                }
                db.SaveChanges();
            }
            else if (ac == "bottom" && pr != db.tbl_production_schedule.Select(x => x.priority).Max())
            {
                var current = db.tbl_production_schedule.Where(x => x.production_id == wr && x.priority == pr).FirstOrDefault();
                current.priority = db.tbl_production_schedule.Select(x => x.priority).Max();
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE ");
                    sb.Append("tbl_production_schedule ");
                    sb.Append("SET priority = priority - 1");
                    sb.Append("WHERE priority > @priority");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@priority", pr);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();

                }
                db.SaveChanges();
            }
            else if (ac == "delete")
            {
                var current = db.tbl_production_schedule.Where(x => x.production_id == wr && x.priority == pr).FirstOrDefault();
                db.tbl_production_schedule.Remove(current);
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE ");
                    sb.Append("tbl_production_schedule ");
                    sb.Append("SET priority = priority - 1");
                    sb.Append("WHERE priority > @priority");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@priority", pr);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();

                }
                db.SaveChanges();
            }
        }

        private static List<SelectListItem> populateCustomers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_cta_customers.Where(x => x.tread_tracker == true).ToList();

            items.Add(new SelectListItem
            {
                Text = "Please Select Customer",
                Value = "Please Select Customer"
            });

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.CustomerName + " (" + val.CustomerCode + ")",
                    Value = val.CustomerCode.ToString()
                }); ;
            }
            return items;
        }

        private static List<SelectListItem> populatePartNumber()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var partNumbers = db.tbl_source_retread_part_number.ToList();

            foreach (var part in partNumbers)
            {
                items.Add(new SelectListItem
                {
                    Text = part.size+" "+part.tread,
                    Value = part.part_number
                });
            }
            return items;
        }

        private static List<SelectListItem> populateSizes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var sizes = db.tbl_source_retread_part_number.Select(x => x.size).Distinct().ToList();

            foreach (var size in sizes)
            {
                items.Add(new SelectListItem
                {
                    Text = size,
                    Value = size
                });
            }
            return items;
        }

        private static List<SelectListItem> populateTreads(bool List)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var treads = db.tbl_source_retread_part_number.Select(x => x.tread).Distinct().ToList();

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

        public ActionResult ProductionSchedule(ProductionScheduleViewModel model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            model.ProductionItems = db.tbl_production_schedule.OrderBy(x => x.priority).ToList();

            tbl_production_schedule newOrder = new tbl_production_schedule();

            newOrder.production_id = db.tbl_production_schedule.OrderByDescending(x => x.production_id).Select(x => x.production_id).FirstOrDefault() + 1;
            newOrder.employee = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.full_name).FirstOrDefault();
            model.NewOrder = newOrder;

            model.ProductionItems = db.tbl_production_schedule.OrderBy(x => x.priority).ToList();
            model.ShipTo = populateLocationInfo();
            model.Customers = populateCustomers();
            model.PartNumbers = populatePartNumber();
            model.SizeList = populateSizes();
            model.Treadlist = populateTreads(true);
            model.PartNum = db.tbl_source_retread_part_number.Select(x => x.part_number).ToList();

            DateTime initialDate = new DateTime(2022, 01, 01);

            var workorders = db.tbl_treadtracker_barcode.Where(x => (x.line_code == 1 || x.line_code == 5 || x.line_code == 10 || x.line_code == 11) && (x.TT050_result == "GOOD" && (x.TT100_result == "GOOD" || x.TT100_result == null) && (x.TT200_result == "GOOD" || x.TT200_result == null) && x.TT600_result == null && x.TT050_date > initialDate)).Select(x => x.retread_workorder).Distinct().ToList();

            List<tbl_production_schedule> lineItems = new List<tbl_production_schedule>();

            foreach (var work in workorders)
            {
                tbl_production_schedule lineItem = new tbl_production_schedule();
                var workOrder = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == work).FirstOrDefault();
                var barcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == work).ToList();
                lineItem.production_id = Convert.ToInt32(work);
                lineItem.production_date = workOrder.creation_date;
                lineItem.size = String.Join(", ", barcodes.Select(x => x.casing_size).Distinct().ToList());
                lineItem.tread = String.Join(", ", barcodes.Select(x => x.retread_design).Distinct().ToList());

                string partNumber = "";

                if(lineItem.size.Contains(',') || lineItem.tread.Contains(','))
                {
                    foreach (var bar in barcodes)
                    {
                        string part = db.tbl_source_retread_part_number.Where(x => x.size == bar.casing_size && x.tread == bar.retread_design).Select(x => x.part_number).FirstOrDefault();
                        if(part != null)
                        {
                            partNumber = partNumber + part + " ";
                        }
                        
                    }
                    lineItem.part_no = String.Join(", ", partNumber.Trim(' ').Split(' ').Distinct().ToList()); ; 
                }
                else
                {
                    lineItem.part_no = db.tbl_source_retread_part_number.Where(x => x.size == lineItem.size && x.tread == lineItem.tread).Select(x => x.part_number).FirstOrDefault();
                }


                lineItem.quantity = barcodes.Count();
                lineItem.shipTo = workOrder.loc_Id + ";" + db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == workOrder.customer_number).Select(x => x.CustomerName).FirstOrDefault(); ;
                lineItem.quantity_complete = barcodes.Where(x => x.TT600_result == "GOOD").Count();
                lineItem.failure_codes = barcodes.Where(x => x.TT050_result != null && x.TT050_result != "GOOD" && x.TT050_result != "Good").Count().ToString();

                lineItems.Add(lineItem);
            }

            model.LineItems = lineItems.OrderByDescending(x => x.production_date).ToList();

            return View(model);
        }


        [HttpPost]
        public ActionResult TReadTrackerSelectedCustomer(TreadTrackerCustomersViewModel model)
        {
            //Debug.WriteLine("Custoomer details:" + model.Custname);
            return RedirectToAction("Customers", new { CustId = model.Customer});

        }

        public ActionResult Customers(TreadTrackerCustomersViewModel model, string custID)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            model.CustomerList = populateCustomers();
            if (custID == null)
            {
                model.Customer = model.CustomerList.Where(x => x.Value != "Please Select Customer").OrderBy(x => x.Text).Select(x => x.Value).FirstOrDefault();  
            }
            else
            {
                model.Customer = model.CustomerList.Where(x => x.Value == custID).OrderBy(x => x.Text).Select(x => x.Value).FirstOrDefault();
            }

            model.customerDetails = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == model.Customer).FirstOrDefault();


            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            

            var tb1 = (from a in db.tbl_treadtracker_workorder.Where(x => x.customer_number == model.Customer) select a).ToList();
            var tb2 = (from a in db.tbl_treadtracker_barcode select a).ToList();

            var CasingList = (from a in tb1
                          join b in tb2 on a.retread_workorder equals b.retread_workorder
                              where (a.customer_number == model.Customer)
                              select new { b.casing_size }).Distinct().ToList();

            List<string> CsngSiz = new List<string>();

            foreach(var casing in CasingList)
            {
                string str = casing.ToString();
                str = str.Substring(15);
                str = str.Remove(str.Length - 1, 1);
                CsngSiz.Add(str);
            }

            model.CasingSizes = CsngSiz;

            var workOrders = db.tbl_treadtracker_workorder.Where(x => x.customer_number == model.Customer).ToList();

            List<WorkOrderInfoViewModel> WorkOrderInfoList = new List<WorkOrderInfoViewModel>();

            foreach (var WO in workOrders)
            {
                WorkOrderInfoViewModel woinfo = new WorkOrderInfoViewModel();

                int barcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == WO.retread_workorder).Count();

                woinfo.WorkOrderNumber = WO.retread_workorder;
                woinfo.customer_number = WO.customer_number;
                woinfo.employee_Id = db.tbl_employee.Where(x => x.employee_id.ToString() == WO.employee_Id).Select(x => x.full_name).FirstOrDefault();
                woinfo.loc_Id = WO.loc_Id;
                woinfo.creation_date = WO.creation_date;
                woinfo.comments = WO.comments;
                woinfo.closedBarcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == WO.retread_workorder && x.TT600_result == "GOOD").Count();
                woinfo.failedBarcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == WO.retread_workorder && ((x.TT050_result != "GOOD" && x.TT050_result != null) || (x.TT200_result != "GOOD" && x.TT200_result != null) || (x.TT100_result != "GOOD" && x.TT100_result != null) || (x.TT600_result != "GOOD" && x.TT600_result != null))).Count();
                woinfo.openBarcodes = barcodes - woinfo.closedBarcodes - woinfo.failedBarcodes;


                WorkOrderInfoList.Add(woinfo);
            }

            model.WorkOrdersForCustomer = WorkOrderInfoList.OrderByDescending(x => x.creation_date).ToList();

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            model.CustomerList = populateCustomers().OrderBy(x => x.Text).ToList();
            model.LocationList = populateLocationsPermissions(empId);
            
            model.Location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

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

        [HttpPost]
        public string PullWorkOrdersChangeCustomer(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_treadtracker_workorder.Where(x => x.customer_number == value).ToList();

            List<WorkOrderInfoViewModel> workOrders = new List<WorkOrderInfoViewModel>();

            foreach (var item in units)
            {
                WorkOrderInfoViewModel workOrder = new WorkOrderInfoViewModel();
                workOrder.WorkOrderNumber = item.retread_workorder;
                workOrder.closedBarcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == item.retread_workorder && x.TT050_result == "GOOD" && x.TT100_result == "GOOD" && x.TT200_result == "GOOD" && x.TT600_result == "GOOD").Count();
                workOrder.openBarcodes = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == item.retread_workorder).Count() - workOrder.openBarcodes;
                workOrders.Add(workOrder);
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(workOrders);
        }

        [HttpPost]
        public string PullCustInfo(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == value).FirstOrDefault();

            return units.CustomerCode + ";" + units.Address1 + ";" + units.Address2 + ";"+ units.Address3 + ";" + " " +";" + " " + ";" + units.Postcode;
        }

        [HttpPost]
        public ActionResult PullBarcodesChangeWorkOrders(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == value).ToList();

            return Json(units.Select(x => new
            {
                value = x.barcode,
                text = x.barcode
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        public string getPartDetails(string td, string sz)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            return db.tbl_source_retread_part_number.Where(x => x.size == sz && x.tread == td).Select(x => x.part_number).FirstOrDefault();
        }

        [HttpPost]
        public string PullBarcodesInfo(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_treadtracker_barcode.Where(x => x.barcode == value).FirstOrDefault();

            string status = units.TT050_result + ";" + units.TT100_result + ";" + units.TT200_result + ";" + units.TT600_result;

            return status;
        }

        public JsonResult GetInspectionDates()
        {
            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                var events = dc.tbl_treadtracker_barcode.Where(x => x.TT600_date > nDaysAgo).Select(x => x.TT600_date).Distinct().ToList();

                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public JsonResult GetInspectionDatesCasingCredit()
        {
            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2022-08-01");

                var barcodes = dc.tbl_treadtracker_barcode.Where(x => x.TT600_date > nDaysAgo && x.cap_count != null && x.cap_count != "" && x.line_code == 6).Select(x => x.TT600_date).Distinct().ToList();
                
                return new JsonResult { Data = barcodes, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public JsonResult GetConsumptionDates()
        {
            DateTime startDate = new DateTime(2023, 3, 1);
            DateTime currentDate = DateTime.Now;

            List<DateTime> dates = new List<DateTime>();

            while (startDate <= currentDate)
            {
                if (startDate.DayOfWeek == DayOfWeek.Monday)
                {
                    dates.Add(startDate);
                }
                startDate = startDate.AddDays(1);
            }

            return new JsonResult { Data = dates, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public JsonResult PopulateFinishedBarcodes(int pid)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == pid.ToString()).Count() > 0)
            {
                var barcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == pid.ToString() && x.TT600_result == "GOOD").ToList();
                return new JsonResult { Data = barcodes, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                var barcodes = db.tbl_treadtracke_stock_production_schedule.Where(x => x.production_id == pid).ToList();
                return new JsonResult { Data = barcodes, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpGet]
        public ActionResult Dashboard(TreadTrackerDashboard model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
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

            model.FieldSheetURL = container.GetBlockBlobReference("Tread Tracker Field Sheet.pdf").Uri.AbsoluteUri;


            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            var casing = db.tbl_treadtracker_inventory_casing.ToList();
            var custList = db.tbl_cta_customers.Where(x => x.tread_tracker == true).ToList();

            model.CustList = custList;
            model.CasingInventory = casing;
            model.SehubAccess = empDetails;
            model.Customers = PopulateCustomers("Nothing").OrderBy(x => x.Text).ToList();
            model.CustomersList = PopulateCustomersTTC();
            model.RetreadList = populateRetreads();

            model.productionSchedule = db.tbl_production_schedule.OrderBy(x => x.priority).ToList();

            if (model.SehubAccess.treadTracker == 0)
            {
                return RedirectToAction("Dashboard", "FleetTVT");
            }

            if (model.SehubAccess.treadTrackerDashboard == 0)
            {
                return RedirectToAction("NewOrder", "TreadTracker");
            }

            return View(model);
        }

        [HttpPost]
        public string MailImageOpenOrder(string imgBase64)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string actual = imgBase64.Substring(22, imgBase64.Length - 22);
            
            byte[] bytes = Convert.FromBase64String(actual);

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            if (empDetails.cta_email != null)
            {
                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }

                string path1 = Path.Combine(Server.MapPath("~/Content/OpenOrder.png"));

                //image.Save(path1, System.Drawing.Imaging.ImageFormat.Png);

                //Trace.WriteLine(path1 + "   This is actual path");

                image.Save(path1, System.Drawing.Imaging.ImageFormat.Png);

                //Trace.WriteLine("Saved");

                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(empDetails.cta_email, "IT Team")); //payroll     harsha.yerramsetty
                msg.From = new MailAddress("no_reply@citytire.com", "Sehub");
                msg.Subject = "Testing";
                msg.Body = "Mail for tread tracker open order";
                msg.IsBodyHtml = true;

                msg.Attachments.Add(new Attachment(path1));

                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("no_reply@citytire.com", "U@dx/Z8Ry{");
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

                //Trace.WriteLine("Reached");

                return imgBase64;
            }
            else
            {
                return imgBase64;
            }
        }

        [HttpPost]
        public string validateBarcode(string value)
        {
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

        [HttpPost]
        public string validateWorkOrder(string value)
        {

            //Trace.WriteLine(value + "This is the work order");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var workOrder = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == value).Select(x => x.retread_workorder).FirstOrDefault();

            if (workOrder != null)
            {
                return "Exists";
            }
            else
            {
                return "Proceed";
            }

        }

        [HttpPost]
        public string validateWorkOrderBarcode(string value)
        {

            //Trace.WriteLine(value + "This is the work order");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var workOrder = db.tbl_treadtracker_barcode.Where(x => x.barcode == value).Select(x => x.retread_workorder).FirstOrDefault();

            if (workOrder != null)
            {
                return workOrder;
            }
            else
            {
                return "";
            }

        }

        private static List<SelectListItem> PopulateCustomers(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            items.Add(new SelectListItem
            {
                Text = "",
                Value = "",
                //Selected = sdr["cust_name"].ToString() == "CITY TIRE AND AUTO CENTRE LTD" ? true : false
            });
            var custList = db.tbl_cta_customers.Where(x => x.tread_tracker == true).ToList();

            foreach (var cust in custList)
            {
                items.Add(new SelectListItem
                {
                    Text = cust.CustomerName,
                    Value = cust.CustomerCode.ToString(),
                    //Selected = sdr["cust_name"].ToString() == "CITY TIRE AND AUTO CENTRE LTD" ? true : false
                });

            }

            return items;
        }

        public ActionResult NewOrder(string values)
        {


            Debug.WriteLine("values:" + values);
            if (values == "")
            {
                CustomerListViewModel custList = new CustomerListViewModel();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                int empId = Convert.ToInt32(Session["userID"].ToString());
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                custList.SehubAccess = empDetails;
                custList.Customers = PopulateCustomers("Nothing");

                if (custList.SehubAccess.neworder == 0)
                {
                    return RedirectToAction("OpenOrder", "TreadTracker");
                }

                return View(custList);
            }
            else
            {
                CustomerListViewModel custList = new CustomerListViewModel();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                int empId = Convert.ToInt32(Session["userID"].ToString());
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                custList.SehubAccess = empDetails;

                custList.Customers = PopulateCustomers(values);

                if (custList.SehubAccess.neworder == 0)
                {
                    return RedirectToAction("OpenOrder", "TreadTracker");
                }

                //custList.WorkOrderNumber = "000007";
                Debug.WriteLine("custList.CustId :" + custList.CustId);

                return View(custList);


            }

        }

        private static List<SelectListItem> populateRetreads()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct model from tbl_source_mediumTruckTires where brand = 'Goodyear RT'";
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
                                Text = sdr["model"].ToString(),
                                Value = sdr["model"].ToString()
                            });
                        }


                    }
                    con.Close();
                }
            }

            return items;
        }

        public ActionResult FailureAnalysis(FailurAnalysisViewmodel model, string values)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int empIdUser = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empIdUser).FirstOrDefault();
            model.Location = db.tbl_employee.Where(x => x.employee_id == empIdUser).Select(x => x.loc_ID).FirstOrDefault();
            model.Customers = populateTreadTrackerCustomers();
            model.Locations = populateLocationInfo();
            model.FailureCodes = db.tbl_source_RARcodes.ToList();
            

            DateTime startDate = Convert.ToDateTime("2018-01-01");

            model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.TT050_date > startDate && ((x.TT050_result != "Good" && x.TT050_result != null) || (x.TT100_result != "GOOD" && x.TT100_result != null) || (x.TT200_result != "GOOD" && x.TT200_result != null) || (x.TT600_result != "GOOD" && x.TT600_result != null))).ToList();
            model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.creation_date > startDate).ToList();

            return View(model);
        }

        public ActionResult ProductionReport(ProductionReportViewModel model, string values)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int empIdUser = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empIdUser).FirstOrDefault();
            string location = db.tbl_employee.Where(x => x.employee_id == empIdUser).Select(x => x.loc_ID).FirstOrDefault();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            var locationDetails = db.tbl_cta_location_info.Where(x => x.loc_id == location).FirstOrDefault();

            model.locID = locationDetails.loc_id;
            model.addressSTREET1 = locationDetails.cta_street1;
            model.addressSTREET2 = locationDetails.cta_street2;
            model.addressCITY = locationDetails.cta_city;
            model.addressPROVINCE = locationDetails.cta_province;
            model.addressPOSTAL = locationDetails.cta_postal_code;
            model.locPHONE = locationDetails.cta_phone;
            model.locFAX = locationDetails.cta_fax;

            return View(model);
        }

        public JsonResult populateTreads()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            List<string> treads = new List<string>();
            var treadList = db.tbl_source_retread_tread.Where(x => x.tread_tracker == 1).Select(x => x.tread_design).ToList();
            foreach(var item in treadList)
            {
                treads.Add(item);
            }

            return new JsonResult { Data = treads, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private static List<SelectListItem> PopulateCustomersTTC()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_cta_customers.ToList();

            items.Add(new SelectListItem
            {
                Text = "Select Customer",
                Value = ""
            });

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.CustomerName,
                    Value = Convert.ToString(val.CustomerCode)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateLocationInfo()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_cta_location_info.Where( x => x.tread_tracker_access == 1).ToList();

            items.Add(new SelectListItem
            {
                Text = "Select Location",
                Value = ""
            });

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.loc_id,
                    Value = Convert.ToString(val.loc_id)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateTreadTrackerCustomers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var customers = db.tbl_cta_customers.Where(x => x.tread_tracker == true).ToList();
            foreach (var cust in customers)
            {
                items.Add(new SelectListItem
                {
                    Text = cust.CustomerName,
                    Value = Convert.ToString(cust.CustomerCode)
                });
            }

            return items.OrderBy(x => x.Text).ToList();
        }


        [HttpPost]
        public ActionResult Generate(CustomerListViewModel custList)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            custList.Customers = PopulateCustomers("Nothing");
            if (custList.CustId == null || custList.WorkOrderNumber == null)
            {
                return RedirectToAction("NewOrder", "TreadTracker");
            }
            else
            {
                var selectedItem = custList.Customers.Find(p => p.Value == custList.CustId.ToString());
                if (selectedItem != null)
                {
                    Debug.WriteLine(selectedItem.Text);

                }
                string workOrderNumber = custList.WorkOrderNumber.ToString();
                string Value = "";
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select retread_workorder FRom tbl_treadtracker_workorder where retread_workorder=" + workOrderNumber;
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                Value = sdr["retread_workorder"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
                string custNum = "";
                custNum = db.tbl_cta_customers.Where(x => x.CustomerName == selectedItem.Text.ToString()).Select(x => x.CustomerCode).FirstOrDefault().ToString();
                Debug.WriteLine("Value:" + Value);
                if (Value.Length > 0)
                {
                    return RedirectToAction("NewOrder", "TreadTracker", new { values = "AlreadyExixts", ac = "AlreadyExixts" });
                }
                else
                {
                    return RedirectToAction("PlaceOrder", new { parameters = workOrderNumber + ";" + custNum });
                }
            }

            //return PartialView(NewOrder);

        }

        public ActionResult submitNewOrder(NewWorkOrder model)
        {
            String value = Request["casing" + "1"];
            Debug.WriteLine("Yes" + Request["line1"]);
            Debug.WriteLine(model.workOrder);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            var loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            for (int i = 1; i <= 11; i++)
            {
                if (Request["barcode" + i.ToString()].Length >= 7)
                {
                    Debug.WriteLine(Request["barcode" + i.ToString()]);

                    tbl_treadtracker_barcode newBarcode = new tbl_treadtracker_barcode();
                    tbl_treadtracker_production_log logEvent = new tbl_treadtracker_production_log();

                    newBarcode.barcode = Request["barcode" + i.ToString()];
                    newBarcode.retread_workorder = model.workOrder;
                    newBarcode.line_number = i;
                    newBarcode.line_code = Convert.ToInt32(Request["line" + i.ToString()]);
                    newBarcode.serial_dot = Request["serial" + i.ToString()]; 
                    newBarcode.casing_size = Request["casing" + i.ToString()];
                    newBarcode.casing_brand = Request["brand" + i.ToString()];
                    newBarcode.cap_count = Request["capCount" + i.ToString()];
                    newBarcode.retread_design = Request["tread" + i.ToString()]; //"TBD" "--Select--"
                    newBarcode.unit_ID = Request["unit" + i.ToString()];
                    if (Request["status" + i.ToString()] != "")
                    {
                        newBarcode.TT050_result = Request["status" + i.ToString()];
                    }
                    else
                    {
                        newBarcode.TT050_result = "GOOD";
                    }
                    newBarcode.TT050_date = model.InspectionDate;
                    newBarcode.ship_to_location = loc;

                    logEvent.barcode = newBarcode.barcode;
                    logEvent.work_station = "TT050";
                    logEvent.timestamp = System.DateTime.Now;
                    logEvent.status = newBarcode.TT050_result;
                    logEvent.details = "Tread -> "+ newBarcode.retread_design + " ; Size -> "+ newBarcode.casing_size + " ; Brand -> " + newBarcode.casing_brand + " ; Loc -> "+ newBarcode.ship_to_location + " ; Cap -> " + newBarcode.cap_count + " ; Serial Dot -> " + newBarcode.serial_dot;

                    db.tbl_treadtracker_barcode.Add(newBarcode);
                    db.tbl_treadtracker_production_log.Add(logEvent);
                    db.SaveChanges();


                }
            }

            tbl_treadtracker_workorder newWorkOrder = new tbl_treadtracker_workorder();
            newWorkOrder.retread_workorder = model.workOrder;
            newWorkOrder.customer_number = model.customerInfo.CustomerCode.ToString();
            newWorkOrder.employee_Id = Convert.ToString(empId);
            newWorkOrder.loc_Id = loc;
            newWorkOrder.creation_date = model.InspectionDate;
            newWorkOrder.comments = model.comments;

            db.tbl_treadtracker_workorder.Add(newWorkOrder);
            db.SaveChanges();


            System.Collections.ArrayList NewOrderList = new System.Collections.ArrayList();

            NewOrderList.Clear();

            
            return RedirectToAction("Dashboard", "TreadTracker");
        }

        public ActionResult PlaceOrder(string custNo, string workOrd)
        {

            Trace.WriteLine("custNo:" + custNo + "  " + workOrd);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            var custInfo = db.tbl_treadtracker_customer_retread_spec.Where(x => x.customer_number.ToString() == custNo).FirstOrDefault();
            var custInfoDetails = db.tbl_tread_tracker_customers.Where(x => x.customer_number.ToString() == custNo).FirstOrDefault();

            NewWorkOrder NewWorkOrderModel = new NewWorkOrder();

            NewWorkOrderModel.customerInfo = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNo).FirstOrDefault();
            NewWorkOrderModel.workOrder = workOrd;
            NewWorkOrderModel.OrderDate = DateTime.Now.ToString("yyyy-MM-dd");
            NewWorkOrderModel.SubmittedEmployee = Session["userID"].ToString();

            if (custInfo != null)
            {
                NewWorkOrderModel.AgeLimit = custInfo.age_limit;
                NewWorkOrderModel.BrandRequirements = custInfo.brand_name_only;
            }
              
            if (custInfoDetails != null)
            {
                NewWorkOrderModel.customerContact = custInfoDetails.reporting_contact;
                NewWorkOrderModel.reportingEmail = custInfoDetails.reporting_email;
            }
              
            

            NewWorkOrderModel.SehubAccess = empDetails;

            return PartialView(NewWorkOrderModel);
        }
        //private OpenOrderModel openOrder = new OpenOrderModel();

        public ActionResult PopulateProductionReport(DateTime inspDate)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());            

            ProductionListViewModel finalProdList = new ProductionListViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            PopulateProductionReport ProdReportList = new PopulateProductionReport();

            ProdReportList.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.TT600_date == inspDate && x.TT600_result != null).ToList();

            var workorders = ProdReportList.barcodeInfo.OrderBy(x => x.retread_workorder).Select(x => x.retread_workorder).Distinct().ToList();

            List<tbl_treadtracker_workorder> workOrderList = new List<tbl_treadtracker_workorder>();

            foreach (var work in workorders )
            {
                workOrderList.Add(db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == work).FirstOrDefault());
            }

            ProdReportList.workOrderInfo = workOrderList;

            var customers = ProdReportList.workOrderInfo.Select(x => x.customer_number).Distinct().ToList();

            List<tbl_cta_customers> customerList = new List<tbl_cta_customers>();

            foreach (var cust in customers)
            {
                customerList.Add(db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == cust).FirstOrDefault());
            }

            ProdReportList.customerInfo = customerList;
            ProdReportList.locInfo = db.tbl_cta_location_info.Where(x => x.loc_id == loc).FirstOrDefault();            
            ProdReportList.date = inspDate;

            //Trace.WriteLine("Reached with date" + inspDate.Date);

            return PartialView(ProdReportList);
        }

        public ActionResult PopulateTransferReport(DateTime inspDate)
        {
            PopulateProductionReport Transfer = new PopulateProductionReport();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            string loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            Transfer.locInfo = db.tbl_cta_location_info.Where(x => x.loc_id == loc).FirstOrDefault();
            Transfer.date = inspDate;
            

            List<KeyValuePair<string, int>> Tout = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> Tin = new List<KeyValuePair<string, int>>();

            var currrentData = db.tbl_treadtracker_barcode.Where(x => x.TT100_date == inspDate || x.TT600_date == inspDate).ToList();
            Trace.WriteLine("This is the date" + currrentData.Where(x => x.ship_to_location == "004" && x.TT100_result == "GOOD").Count());
            Tout.Add(new KeyValuePair<string, int>("004", currrentData.Where(x => x.ship_to_location == "004" && x.TT100_result == "GOOD").Count()));
            Tout.Add(new KeyValuePair<string, int>("005", currrentData.Where(x => x.ship_to_location == "005" && x.TT100_result == "GOOD").Count()));
            Tout.Add(new KeyValuePair<string, int>("009", currrentData.Where(x => x.ship_to_location == "009" && x.TT100_result == "GOOD").Count()));
            Tout.Add(new KeyValuePair<string, int>("010", currrentData.Where(x => x.ship_to_location == "010" && x.TT100_result == "GOOD").Count()));
            Tout.Add(new KeyValuePair<string, int>("347", currrentData.Where(x => x.ship_to_location == "347" && x.TT100_result == "GOOD").Count()));
            Tout.Add(new KeyValuePair<string, int>("007", currrentData.Where(x => x.ship_to_location == "007" && x.TT100_result == "GOOD").Count()));

            Tin.Add(new KeyValuePair<string, int>("004", currrentData.Where(x => x.ship_to_location == "004" && x.TT600_result == "GOOD").Count()));
            Tin.Add(new KeyValuePair<string, int>("005", currrentData.Where(x => x.ship_to_location == "005" && x.TT600_result == "GOOD").Count()));
            Tin.Add(new KeyValuePair<string, int>("009", currrentData.Where(x => x.ship_to_location == "009" && x.TT600_result == "GOOD").Count()));
            Tin.Add(new KeyValuePair<string, int>("010", currrentData.Where(x => x.ship_to_location == "010" && x.TT600_result == "GOOD").Count()));
            Tin.Add(new KeyValuePair<string, int>("347", currrentData.Where(x => x.ship_to_location == "347" && x.TT600_result == "GOOD").Count()));
            Tin.Add(new KeyValuePair<string, int>("007", currrentData.Where(x => x.ship_to_location == "007" && x.TT600_result == "GOOD").Count()));

            Transfer.TransferIN = Tin;
            Transfer.TransferOUT = Tout;

            return PartialView(Transfer);
        }

        public ActionResult PopulateCasingCreditReport(DateTime inspDate)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            PopulateProductionReport ProdReportList = new PopulateProductionReport();

            ProdReportList.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.TT600_date == inspDate && x.TT600_result == "GOOD" && x.cap_count != null && x.cap_count != "").ToList();

            var workorders = ProdReportList.barcodeInfo.OrderBy(x => x.retread_workorder).Select(x => x.retread_workorder).Distinct().ToList();

            List<tbl_treadtracker_workorder> workOrderList = new List<tbl_treadtracker_workorder>();

            foreach (var work in workorders)
            {
                workOrderList.Add(db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == work).FirstOrDefault());
            }

            ProdReportList.workOrderInfo = workOrderList;

            var customers = ProdReportList.workOrderInfo.Select(x => x.customer_number).Distinct().ToList();

            List<tbl_cta_customers> customerList = new List<tbl_cta_customers>();

            foreach (var cust in customers)
            {
                customerList.Add(db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == cust).FirstOrDefault());
            }

            ProdReportList.customerInfo = customerList;
            ProdReportList.locInfo = db.tbl_cta_location_info.Where(x => x.loc_id == loc).FirstOrDefault();
            ProdReportList.date = inspDate;

            ProdReportList.creditSchedule = db.tbl_source_treadtracker_creditSchedule.ToList();
            ProdReportList.freightSchedule = db.tbl_source_treadtracker_freightSchedule.ToList();
            //Trace.WriteLine("Reached with date" + inspDate.Date);

            ProdReportList.creditSchedule_revision = db.tbl_source_treadtracker_creditSchedule_revision.ToList();
            ProdReportList.freightSchedule_revision = db.tbl_source_treadtracker_freightSchedule_revision.ToList();
            ProdReportList.casingTire = db.tbl_source_treadtracker_casingTire.ToList();
            //ProdReportList.customerProfile = db.tbl_treadtracker_customer_retread_spec.ToList();

            return PartialView(ProdReportList);
        }

        public ActionResult PopulateConsumptionReport(DateTime inspDate)
        {
            
            List<ConsumablesViewModel> consumables = new List<ConsumablesViewModel>();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            PopulateProductionReport ConsumablesReport = new PopulateProductionReport();
            ConsumablesReport.locInfo = db.tbl_cta_location_info.Where(x => x.loc_id == loc).FirstOrDefault();
            DateTime StartDate = inspDate.AddDays(-8);

            ConsumablesReport.startDateConsumables = StartDate;
            ConsumablesReport.endDateConsumables = inspDate.AddDays(-2);
            
            while (StartDate <= inspDate.AddDays(-2))
            {
                var barcodes = db.tbl_treadtracker_barcode.Where(x => x.TT600_date == StartDate).ToList();

                foreach(var barcode in barcodes)
                {
                    var circumfrance = db.tbl_source_truckTire_sizes.Where(x => x.tire_size == barcode.casing_size).Select(x => x.circumference).FirstOrDefault();
                    if (barcode.TT400_tread != "n/a" && barcode.TT400_tread != null && barcode.TT400_tread != "")
                    {
                        ConsumablesViewModel consumable = new ConsumablesViewModel();
                        consumable.type = "Tread";
                        string shortDesc = barcode.TT400_tread.Split('(')[0];
                        consumable.PartNumber = db.tbl_source_treadtracker_consumables_TreadRubber.Where(x => x.short_description == shortDesc).Select(x => x.part_number).FirstOrDefault();

                        var treadRubber = db.tbl_source_treadtracker_consumables_TreadRubber.Where(x => x.part_number == consumable.PartNumber).FirstOrDefault();
                        if (treadRubber != null)
                        {
                            consumable.Description = treadRubber.description;                            
                            if (circumfrance != null)
                            {                                
                                consumable.ftConsumed = Convert.ToDouble(circumfrance) * Convert.ToDouble(barcode.TT400_tread.Split('(')[1].Split('x')[1].Split(')')[0]);
                            }                            
                            consumable.lbPerFt = Convert.ToDouble(treadRubber.weight_per_foot);
                            consumable.lbsConsumed = consumable.ftConsumed * consumable.lbPerFt;
                            consumables.Add(consumable);
                        }
                        
                    }
                    if (barcode.TT400_cushion != "n/a" && barcode.TT400_cushion != null && barcode.TT400_cushion != "")
                    {
                        ConsumablesViewModel consumable = new ConsumablesViewModel();
                        consumable.type = "Cushion";
                        consumable.PartNumber = barcode.TT400_cushion.Split('(')[0];
                        Trace.WriteLine(consumable.PartNumber + "*");
                        var treadRubber = db.tbl_source_treadtracker_consumables_nonTreadRubber.Where(x => x.part_number == consumable.PartNumber).FirstOrDefault();
                        if (treadRubber != null)
                        {
                            consumable.Description = treadRubber.description;
                            if (circumfrance != null)
                            {
                                consumable.ftConsumed = Convert.ToDouble(circumfrance) * Convert.ToDouble(barcode.TT400_cushion.Split('(')[1].Split('x')[1].Split(')')[0]);
                            }
                            consumable.lbPerFt = Convert.ToDouble(treadRubber.lb_per_ft);
                            consumable.lbsConsumed = consumable.ftConsumed * consumable.lbPerFt;
                            consumables.Add(consumable);
                        }

                    }
                    if (barcode.TT400_strip != "n/a" && barcode.TT400_strip != null && barcode.TT400_strip != "")
                    {
                        ConsumablesViewModel consumable = new ConsumablesViewModel();
                        consumable.type = "Strip";
                        consumable.PartNumber = barcode.TT400_strip.Split('(')[0];

                        var treadRubber = db.tbl_source_treadtracker_consumables_nonTreadRubber.Where(x => x.part_number == consumable.PartNumber).FirstOrDefault();
                        if (treadRubber != null)
                        {
                            consumable.Description = treadRubber.description;
                            if (circumfrance != null)
                            {
                                consumable.ftConsumed = Convert.ToDouble(circumfrance) * Convert.ToDouble(barcode.TT400_strip.Split('(')[1].Split('x')[1].Split(')')[0]);
                            }
                            consumable.lbPerFt = Convert.ToDouble(treadRubber.lb_per_ft);
                            consumable.lbsConsumed = consumable.ftConsumed * consumable.lbPerFt;
                            consumables.Add(consumable);
                        }

                    }
                    if (barcode.TT400_rope != "n/a" && barcode.TT400_rope != null && barcode.TT400_rope != "")
                    {
                        ConsumablesViewModel consumable = new ConsumablesViewModel();
                        consumable.type = "Rope";
                        consumable.PartNumber = barcode.TT400_rope.Split('(')[0];

                        var treadRubber = db.tbl_source_treadtracker_consumables_nonTreadRubber.Where(x => x.part_number == consumable.PartNumber).FirstOrDefault();
                        if (treadRubber != null)
                        {
                            consumable.Description = treadRubber.description;
                            consumable.ftConsumed = Convert.ToDouble(barcode.TT400_rope.Split('(')[1].Split('x')[1].Split(')')[0]);
                            consumable.lbPerFt = Convert.ToDouble(treadRubber.lb_per_ft);
                            consumable.lbsConsumed = consumable.ftConsumed * consumable.lbPerFt;
                            consumables.Add(consumable);
                        }

                    }


                }
                StartDate = StartDate.AddDays(1);
            }

            ConsumablesReport.consumables = consumables;

            return PartialView(ConsumablesReport);
        }

        //private OpenOrderModel openOrder = new OpenOrderModel();

        public ActionResult BackPlaceOrder(string parameters)
        {
            return RedirectToAction("NewOrder", "TreadTracker", new { values = "" });
        }

        public ActionResult OpenOrder(string id, string barcode)
        {
            //id = "";
            //barcode = "28452118";
            Debug.WriteLine("OpenOrder: id:" + id + "  barcode:" + barcode);
            if (id != "")
            {
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                int empId = Convert.ToInt32(Session["userID"].ToString());

                string locId = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

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

                Debug.WriteLine("In Id");
                string custNumber = "0";
                dynamic mymodel = new ExpandoObject();
                
                var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == id).FirstOrDefault();
                if (WorkOrderDetails != null)
                {
                    custNumber = WorkOrderDetails.customer_number;
                }
                else
                {
                    custNumber = "0";
                }
                Debug.WriteLine("custNumber:" + custNumber);
                var ViewOrderModel = new ViewWorkOrderModel
                {
                    barcodeInformation = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == id).OrderBy(x => x.line_number).ToList(),
                    custInfo = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNumber).FirstOrDefault(),
                    workOrderInfo = WorkOrderDetails,
                    WorkOrderNumber = id,
                    Barcode = barcode
                };                
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                ViewOrderModel.SehubAccess = empDetails;

                if (ViewOrderModel.SehubAccess.openorder == 0)
                {
                    return RedirectToAction("Dashboard", "FleetTVT");
                }

                return View(ViewOrderModel);
            }
            else if (barcode != "")
            {
                Debug.WriteLine("In Barcode");
                Debug.WriteLine("Barcode:" + barcode);
                string workOrderNumber = "0";
                string custNumber = "0";
                dynamic mymodel = new ExpandoObject();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                var getWorkOrder = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                if (getWorkOrder != null)
                {
                    workOrderNumber = getWorkOrder.retread_workorder;

                }
                else
                {
                    workOrderNumber = "0";
                }
                var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == workOrderNumber).FirstOrDefault();
                if (WorkOrderDetails != null)
                {
                    custNumber = WorkOrderDetails.customer_number;
                }
                else
                {
                    custNumber = "0";
                }
                var ViewOrderModel = new ViewWorkOrderModel
                {
                    barcodeInformation = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == workOrderNumber).ToList(),
                    custInfo = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNumber).FirstOrDefault(),
                    workOrderInfo = WorkOrderDetails,
                    WorkOrderNumber = id,
                    Barcode = barcode
                };

                int empId = Convert.ToInt32(Session["userID"].ToString());
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                ViewOrderModel.SehubAccess = empDetails;

                if (ViewOrderModel.SehubAccess.openorder == 0)
                {
                    return RedirectToAction("Dashboard", "FleetTVT");
                }


                return View(ViewOrderModel);
            }
            else
            {
                Debug.WriteLine("In Else");
                string custNumber = "0";
                dynamic mymodel = new ExpandoObject();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == id).FirstOrDefault();

                var ViewOrderModel = new ViewWorkOrderModel
                {

                    barcodeInformation = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == id).ToList(),
                    custInfo = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNumber).FirstOrDefault(),
                    workOrderInfo = WorkOrderDetails,
                    WorkOrderNumber = id,
                    Barcode = barcode
                };

                int empId = Convert.ToInt32(Session["userID"].ToString());
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                ViewOrderModel.SehubAccess = empDetails;

                return PartialView(ViewOrderModel);
            }

        }

        [HttpPost]
        public ActionResult ViewOrder(ViewWorkOrderModel modeldata)
        {
            Debug.WriteLine("modeldata.Barcode in ViewOrder:" + modeldata.Barcode);
            return RedirectToAction("OpenOrder", new { id = modeldata.WorkOrderNumber, barcode = modeldata.Barcode });
        }

        private string searchNameValue = "";
        private string searchNoValue = "";
        public ActionResult NameDropDown(string value)
        {
            searchNameValue = value;
            Debug.WriteLine("In NameDropDown");
            if (value == "" || value is null)
            {
                value = "";
            }
            NewOrderSearchCriteria custList = new NewOrderSearchCriteria();
            custList.MatchedNames = PopulateMatchedValues(value,"Name");
            return PartialView(custList);
        }
 
        private static List<SelectListItem> PopulateMatchedValues(string param,string Fromtype)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "";
                if(Fromtype=="Name")
                {
                     query = "select cust_name,cust_us1 FRom tbl_customer_list  where cscttc ='True' and cust_us2='0' and cust_name like '%" + param + "%'";
                }
                else
                {
                    query = "select cust_name,cust_us1 FRom tbl_customer_list  where cscttc ='True' and cust_us2='0' and cust_us1 like '%" + param + "%'";
                }
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
                                    Text = sdr["cust_name"].ToString(),
                                    Value = sdr["cust_us1"].ToString()
                                });
                            }
                        

                    }
                    con.Close();
                }
            }

            return items;
        }

        public ActionResult NumberDropDown(string value)
        {
            searchNoValue = value;
            Debug.WriteLine("In NoDropDown");
            if (value == "" || value is null)
            {
                value = "";
            }
            NewOrderSearchCriteria custList = new NewOrderSearchCriteria();
            custList.MatchedNo = PopulateMatchedValues(value, "Number");
            return PartialView(custList);
        }

        public ActionResult EditOrder(string orderNumber)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            //Trace.WriteLine("orderNumber:" + orderNumber);
            //Trace.WriteLine("In EditOrder");
            string custNumber = "0";
            dynamic mymodel = new ExpandoObject();
           
            var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == orderNumber).FirstOrDefault();
            if (WorkOrderDetails != null)
            {
                custNumber = WorkOrderDetails.customer_number;
            }
            else
            {
                custNumber = "0";
            }

            var casingSizeList= (from a in db.tbl_treadtracker_casing_sizes select a).ToList();

            ViewBag.CasingSizeList = casingSizeList;
            var casingBrandList = (from a in db.tbl_treadtracker_casing_brands select a).ToList();
            ViewBag.CasingBrandList = casingBrandList;

            List<tbl_treadtracker_casing_brands> capCountList = new List<tbl_treadtracker_casing_brands>();

            capCountList.Add(new tbl_treadtracker_casing_brands
            {
                brand_id = 1,
                casing_brand = "Virgin"
            });
            capCountList.Add(new tbl_treadtracker_casing_brands
            {
                brand_id = 2,
                casing_brand = "Capped Prior 1"
            });
            capCountList.Add(new tbl_treadtracker_casing_brands
            {
                brand_id = 3,
                casing_brand = "Capped Prior 2+"
            });

            ViewBag.CapCountList = capCountList;



            if (custNumber == "0578749")
            {
                var casingTreadList = (from a in db.tbl_source_retread_tread select a).ToList();
                ViewBag.CasingTreadList = casingTreadList;
            }
            else
            {
                var casingTreadList = (from a in db.tbl_source_retread_tread.Where(x => x.tread_tracker == 1) select a).ToList();
                ViewBag.CasingTreadList = casingTreadList;
            }
            
            

            var BarcodeJoinList = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == orderNumber).OrderBy(x => x.line_number).ToList();


            Debug.WriteLine("custNumber:" + custNumber);
            List<EditOrderBarcodeJoinModel> BarcodeInfoVMlist = new List<EditOrderBarcodeJoinModel>();
            foreach (var item in BarcodeJoinList)
            {

                EditOrderBarcodeJoinModel objcvm = new EditOrderBarcodeJoinModel(); // ViewModel

                objcvm.barcode = item.barcode;

                objcvm.changed_barcode = item.barcode;

                objcvm.retread_workorder = item.retread_workorder;

                objcvm.line_number = item.line_number;

                objcvm.line_code = item.line_code;

                objcvm.serial_dot = item.serial_dot;

                objcvm.casing_size = item.casing_size;

                objcvm.casing_brand = item.casing_brand;

                objcvm.retread_design = item.retread_design;

                objcvm.unit_ID = item.unit_ID;

                objcvm.TT050_result = item.TT050_result;

                objcvm.TT050_date = item.TT050_date;

                objcvm.TT100_result = item.TT100_result;

                objcvm.TT100_date = item.TT100_date;

                objcvm.TT200_result = item.TT200_result;

                objcvm.TT200_date = item.TT200_date;

                objcvm.TT600_result = item.TT600_result;

                objcvm.TT600_date = item.TT600_date;

                objcvm.cap_count = capCountList.Where(x => x.casing_brand == item.cap_count).Select(x => x.brand_id).FirstOrDefault().ToString();

                

                objcvm.size_id = db.tbl_treadtracker_casing_sizes.Where(x => x.casing_size == item.casing_size).Select(x => x.size_id).FirstOrDefault();

                objcvm.brand_id = db.tbl_treadtracker_casing_brands.Where(x => x.casing_brand == item.casing_brand).Select(x => x.brand_id).FirstOrDefault();

                //objcvm.tread_id = db.tbl_source_retread_tread.Where(x => x.tread_design == item.retread_design).Select(x => x.tread_id).FirstOrDefault();

                BarcodeInfoVMlist.Add(objcvm);

            }
            var CustomerInfoList = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNumber).FirstOrDefault();
            string custNameValue = CustomerInfoList.CustomerName;
            Debug.WriteLine("custNameValue:" + custNameValue + "hkjhkh");

            List<SelectListItem> custListItems = PopulateCustomers(custNameValue);

            //Trace.WriteLine("This the count " + (11 - BarcodeInfoVMlist.Count));

            int remaining = 11 - BarcodeInfoVMlist.Count;

            for (int i = 0; i < remaining; i++)
            {
                //Trace.WriteLine("This is the iteration " + i);
                EditOrderBarcodeJoinModel objcvm = new EditOrderBarcodeJoinModel();
                objcvm.line_number = BarcodeInfoVMlist.Count + i;
                BarcodeInfoVMlist.Add(objcvm);
            }

            //Trace.WriteLine("This the count after adding items " + (BarcodeInfoVMlist.Count));

            var EditOrderModel = new EditOrderModel
            {
                BarcodeInformation = BarcodeInfoVMlist,
                CustomerInfo = CustomerInfoList,
                WorkOrderInfo = WorkOrderDetails,
                CustomersList = custListItems
                //ChangedCustomerId=-1
            };

            EditOrderModel.SehubAccess = empDetails;


            return PartialView(EditOrderModel);
        }

        public ActionResult ViewOrder(string orderNumber)
        {

            int empId = Convert.ToInt32(Session["userID"].ToString());

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string locId = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            var locDetails = db.tbl_cta_location_info.Where(x => x.loc_id == locId).FirstOrDefault();

            if (locDetails != null)
            {
                ViewBag.locID = locDetails.loc_id;
                ViewBag.addressSTREET1 = locDetails.cta_street1;
                ViewBag.addressSTREET2 = locDetails.cta_street2;
                ViewBag.addressCITY = locDetails.cta_city;
                ViewBag.addressPROVINCE = locDetails.cta_province;
                ViewBag.addressPOSTAL = locDetails.cta_postal_code;
                ViewBag.locPHONE = locDetails.cta_phone;
                ViewBag.locFAX = locDetails.cta_fax;
            }
            
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            //Trace.WriteLine("orderNumber:" + orderNumber);
            //Trace.WriteLine("In EditOrder");
            string custNumber = "0";
            dynamic mymodel = new ExpandoObject();

            var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == orderNumber).FirstOrDefault();
            if (WorkOrderDetails != null)
            {
                custNumber = WorkOrderDetails.customer_number;
            }
            else
            {
                custNumber = "0";
            }

            var casingSizeList = (from a in db.tbl_treadtracker_casing_sizes select a).ToList();

            ViewBag.CasingSizeList = casingSizeList;
            var casingBrandList = (from a in db.tbl_treadtracker_casing_brands select a).ToList();
            ViewBag.CasingBrandList = casingBrandList;
            var casingTreadList = (from a in db.tbl_source_retread_tread select a).ToList();
            ViewBag.CasingTreadList = casingTreadList;

            var BarcodeJoinList = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == orderNumber).OrderBy(x => x.line_number).ToList();


            Debug.WriteLine("custNumber:" + custNumber);
            List<EditOrderBarcodeJoinModel> BarcodeInfoVMlist = new List<EditOrderBarcodeJoinModel>();
            foreach (var item in BarcodeJoinList)
            {

                EditOrderBarcodeJoinModel objcvm = new EditOrderBarcodeJoinModel(); // ViewModel

                objcvm.barcode = item.barcode;

                objcvm.changed_barcode = item.barcode;

                objcvm.retread_workorder = item.retread_workorder;

                objcvm.line_number = item.line_number;

                objcvm.line_code = item.line_code;

                objcvm.serial_dot = item.serial_dot;

                objcvm.casing_size = item.casing_size;

                objcvm.casing_brand = item.casing_brand;

                objcvm.retread_design = item.retread_design;

                objcvm.unit_ID = item.unit_ID;

                objcvm.TT050_result = item.TT050_result;

                objcvm.TT050_date = item.TT050_date;

                objcvm.TT100_result = item.TT100_result;

                objcvm.TT100_date = item.TT100_date;

                objcvm.TT200_result = item.TT200_result;

                objcvm.TT200_date = item.TT200_date;

                objcvm.TT600_result = item.TT600_result;

                objcvm.TT600_date = item.TT600_date;

                objcvm.ship_to_location = item.ship_to_location;

                objcvm.cap_count = item.cap_count;


                objcvm.size_id = db.tbl_treadtracker_casing_sizes.Where(x => x.casing_size == item.casing_size).Select(x => x.size_id).FirstOrDefault();

                objcvm.brand_id = db.tbl_treadtracker_casing_brands.Where(x => x.casing_brand == item.casing_brand).Select(x => x.brand_id).FirstOrDefault();

                //objcvm.tread_id = db.tbl_source_retread_tread.Where(x => x.tread_design == item.retread_design).Select(x => x.tread_id).FirstOrDefault();

                BarcodeInfoVMlist.Add(objcvm);

            }
            var CustomerInfoList = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNumber).FirstOrDefault();
            string custNameValue = CustomerInfoList.CustomerName;

            var custInfo = db.tbl_treadtracker_customer_retread_spec.Where(x => x.customer_number.ToString() == custNumber).FirstOrDefault();
            var custInfoDetails = db.tbl_tread_tracker_customers.Where(x => x.customer_number.ToString() == custNumber).FirstOrDefault();

            List<SelectListItem> custListItems = PopulateCustomers(custNameValue);

            int remaining = 11 - BarcodeInfoVMlist.Count;

            for (int i = 0; i < remaining; i++)
            {
                //Trace.WriteLine("This is the iteration " + i);
                EditOrderBarcodeJoinModel objcvm = new EditOrderBarcodeJoinModel();
                objcvm.line_number = BarcodeInfoVMlist.Count + i;
                BarcodeInfoVMlist.Add(objcvm);
            }

            var EditOrderModel = new EditOrderModel
            {
                BarcodeInformation = BarcodeInfoVMlist,
                CustomerInfo = CustomerInfoList,
                WorkOrderInfo = WorkOrderDetails,
                CustomersList = custListItems
                //ChangedCustomerId=-1
            };

            if (custInfo != null)
            {
                EditOrderModel.AgeLimit = custInfo.age_limit;
                EditOrderModel.BrandRequirements = custInfo.brand_name_only;
            }

            if (custInfoDetails != null)
            {
                EditOrderModel.customerContact = custInfoDetails.reporting_contact;
                EditOrderModel.reportingEmail = custInfoDetails.reporting_email;
            }

            EditOrderModel.FailureCodesBarcodeInformation = db.tbl_source_RARcodes.ToList();
            EditOrderModel.SehubAccess = empDetails;

            return PartialView(EditOrderModel);
        }

        [NonAction]
        public SelectList ToSelectList(List<tbl_treadtracker_casing_sizes> lstsize)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (tbl_treadtracker_casing_sizes item in lstsize)
            {
                Debug.WriteLine(item.casing_size + "   " + item.size_id);
                list.Add(new SelectListItem()
                {
                    
                    Text = item.casing_size,
                    Value = Convert.ToString(item.size_id)
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        [HttpPost]
        public ActionResult SubmitEditOrder(EditOrderModel model)
        {
            Debug.WriteLine("Model Values:"+ model.WorkOrderInfo.retread_workorder);
           
            //This code is to find changed customer number
            model.CustomersList = PopulateCustomers("Nothing");
            /*
             var selectedItem = model.CustomersList.Find(p => p.Value == model.ChangedCustomerId.ToString());
            if (selectedItem != null)
            {
               
                Debug.WriteLine("The selected CustomersList value in the edit button is:"+selectedItem.Text);

            }
            
            */
            //end

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            
            var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.WorkOrderInfo.retread_workorder).FirstOrDefault();
            if (WorkOrderDetails != null)
            {
                
                //WorkOrderDetails.customer_number = selectedItem.Value;
                WorkOrderDetails.comments = model.WorkOrderInfo.comments;
                //Testing
                //Debug.WriteLine("Testing the owrk order");
                //Debug.WriteLine(WorkOrderDetails.retread_workorder);
                //Debug.WriteLine(WorkOrderDetails.customer_number + "    " + selectedItem.Value);
            }

            var casingSizeList = (from a in db.tbl_treadtracker_casing_sizes select a).ToList();
            var casingBrandList = (from a in db.tbl_treadtracker_casing_brands select a).ToList();
            var casingTreadList = (from a in db.tbl_source_retread_tread select a).ToList();

            List<tbl_treadtracker_casing_brands> capCountList = new List<tbl_treadtracker_casing_brands>();

            capCountList.Add(new tbl_treadtracker_casing_brands
            {
                brand_id = 1,
                casing_brand = "Virgin"
            });
            capCountList.Add(new tbl_treadtracker_casing_brands
            {
                brand_id = 2,
                casing_brand = "Capped Prior 1"
            });
            capCountList.Add(new tbl_treadtracker_casing_brands
            {
                brand_id = 3,
                casing_brand = "Capped Prior 2+"
            });

            foreach (var items in model.BarcodeInformation)
            {
                if(items.barcode != null && items.barcode != "")
                {
                    string changedBarcode = Convert.ToString(items.changed_barcode);


                    var selectedSize = casingSizeList.Find(p => p.size_id == Convert.ToInt32(items.casing_size));
                    string casingSize = "";
                    if (selectedSize != null)
                    {
                        casingSize = selectedSize.casing_size;
                    }

                    var selectedBrand = casingBrandList.Find(p => p.brand_id == Convert.ToInt32(items.casing_brand));
                    string casingBrand = "";
                    if (selectedBrand != null)
                    {
                        casingBrand = selectedBrand.casing_brand;

                    }

                    //var selectedTread = casingTreadList.Find(p => p.tread_id == Convert.ToInt32(items.retread_design));
                    string casingTread = items.retread_design;


                    var result = db.tbl_treadtracker_barcode.Where(a => a.barcode.Equals(items.barcode)).FirstOrDefault();

                    if (changedBarcode != null && changedBarcode != "" && result != null)
                    {
                        var result1 = new tbl_treadtracker_barcode();
                        var logEvent = new tbl_treadtracker_production_log();

                        result1.barcode = changedBarcode;
                        result1.retread_workorder = result.retread_workorder;
                        result1.line_number = result.line_number;

                        result1.line_code = items.line_code;
                        result1.serial_dot = items.serial_dot;
                        result1.casing_size = casingSize;
                        result1.casing_brand = casingBrand;
                        result1.retread_design = casingTread;
                        result1.unit_ID = items.unit_ID;
                        result1.TT050_result = result.TT050_result;
                        result1.TT050_date = result.TT050_date;
                        result1.TT100_result = result.TT100_result;
                        result1.TT100_date = result.TT100_date;
                        result1.TT200_result = result.TT200_result;
                        result1.TT200_date = result.TT200_date;
                        result1.TT600_result = result.TT600_result;
                        result1.TT600_date = result.TT600_date;
                        result1.cap_count = capCountList.Where(x => x.brand_id.ToString() == items.cap_count).Select(x => x.casing_brand).FirstOrDefault();

                        Trace.WriteLine("This is the cap count result " + result1.cap_count + " for the barcode " + result1.barcode);
                        Trace.WriteLine("This is the Retread design " + items.retread_design + " for the barcode " + result1.barcode);

                        logEvent.barcode = result1.barcode;
                        logEvent.work_station = "TT050";
                        logEvent.timestamp = System.DateTime.Now;
                        logEvent.status = result1.TT050_result;
                        logEvent.details = "Tread -> " + result1.retread_design + " ; Size -> " + result1.casing_size + " ; Brand -> " + result1.casing_brand + " ; Loc -> " + result1.ship_to_location + " ; Cap -> " + result1.cap_count + " ; Serial Dot -> " + result1.serial_dot;

                        db.tbl_treadtracker_production_log.Add(logEvent);

                        db.tbl_treadtracker_barcode.Add(result1);
                        db.tbl_treadtracker_barcode.Remove(result);
                    }
                    if (changedBarcode == null || changedBarcode == "" && result != null)
                    {
                        result.line_code = items.line_code;
                        result.serial_dot = items.serial_dot;
                        result.casing_size = casingSize;
                        result.casing_brand = casingBrand;
                        result.retread_design = casingTread;
                        result.unit_ID = items.unit_ID;
                        result.cap_count = items.cap_count;
                    }
                }
                else
                {
                    string changedBarcode = Convert.ToString(items.changed_barcode);

                    var selectedSize = casingSizeList.Find(p => p.size_id == Convert.ToInt32(items.casing_size));
                    string casingSize = "";
                    if (selectedSize != null)
                    {
                        casingSize = selectedSize.casing_size;

                    }

                    var selectedBrand = casingBrandList.Find(p => p.brand_id == Convert.ToInt32(items.casing_brand));
                    string casingBrand = "";
                    if (selectedBrand != null)
                    {
                        casingBrand = selectedBrand.casing_brand;

                    }

                    //var selectedTread = casingTreadList.Find(p => p.tread_id == Convert.ToInt32(items.retread_design));
                    string casingTread = "";

                    /*
                     if (selectedTread != null)
                    {
                        casingTread = selectedTread.tread_design;

                    }
                     */



                    if (changedBarcode != null && changedBarcode != "" && db.tbl_treadtracker_barcode.Where(a => a.barcode.Equals(changedBarcode)).FirstOrDefault() == null)
                    {
                        var result1 = new tbl_treadtracker_barcode();
                        tbl_treadtracker_production_log logEvent = new tbl_treadtracker_production_log();

                        result1.barcode = changedBarcode;
                        result1.retread_workorder = model.WorkOrderInfo.retread_workorder;
                        result1.line_number = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == model.WorkOrderInfo.retread_workorder).OrderByDescending(x => x.line_number).Select(x => x.line_number).FirstOrDefault()+1;
                        result1.line_code = items.line_code;
                        result1.serial_dot = items.serial_dot;
                        result1.casing_size = casingSize;
                        result1.casing_brand = casingBrand;
                        result1.retread_design = casingTread;
                        result1.unit_ID = items.unit_ID;
                        result1.TT050_result = "GOOD";

                        Trace.WriteLine("This is the newly added barcode" + result1.barcode);

                        logEvent.barcode = result1.barcode;
                        logEvent.work_station = "TT050";
                        logEvent.timestamp = System.DateTime.Now;
                        logEvent.status = result1.TT050_result;
                        logEvent.details = "Tread -> " + result1.retread_design + " ; Size -> " + result1.casing_size + " ; Brand -> " + result1.casing_brand + " ; Loc -> " + result1.ship_to_location + " ; Cap -> " + result1.cap_count + " ; Serial Dot -> " + result1.serial_dot;

                        db.tbl_treadtracker_production_log.Add(logEvent);
                        db.tbl_treadtracker_barcode.Add(result1);
                    }
                }
                
            }
            
            db.SaveChanges();
            return RedirectToAction("Dashboard", "TreadTracker");
        }

        public ActionResult CanceEditOrderOrder(string workOrder)
        {
            return RedirectToAction("OpenOrder", new { id = workOrder, barcode = "" });
        }

        public ActionResult ShowEditCustInfo(string value)
        {
            Debug.WriteLine("In ShowEditCustInfo");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var CustomerInfoList = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == value).FirstOrDefault();
            if(CustomerInfoList!=null)
            {              
                ViewData["CustomerNum"] = CustomerInfoList.Address1;
                ViewData["Address"] = CustomerInfoList.Address2+","+ CustomerInfoList.Address3+","+ CustomerInfoList.Postcode;
            }
            
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddProductionItem(ProductionScheduleViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_production_schedule item = new tbl_production_schedule();
            item = model.NewOrder;
            item.production_date = System.DateTime.Today;
            item.priority = db.tbl_production_schedule.OrderByDescending(x => x.priority).Select(x => x.priority).FirstOrDefault()+1;
            db.tbl_production_schedule.Add(item);
            db.SaveChanges();

            return RedirectToAction("ProductionSchedule");
        }

        [HttpPost]
        public ActionResult EditProductionItem(ProductionScheduleViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var item = db.tbl_production_schedule.Where(x => x.production_id == model.EditOrder.production_id).FirstOrDefault();

            item.production_date = model.EditOrder.production_date;
            item.quantity = model.EditOrder.quantity;
            item.quantity_complete = model.EditOrder.quantity_complete;
            item.part_no = model.EditOrder.part_no;
            item.casing_spec = model.EditOrder.casing_spec;
            item.size = model.EditOrder.size;
            item.tread = model.EditOrder.tread;

            db.SaveChanges();

            return RedirectToAction("ProductionSchedule");
        }

        [HttpPost]
        public ActionResult TreadChange(string tread)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_source_retread_part_number.Where(x => x.tread == tread).ToList();

            return Json(units.Select(x => new
            {
                value = x.size,
                text = x.size
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SizeChange(string size)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_source_retread_part_number.Where(x => x.size == size).ToList();

            return Json(units.Select(x => new
            {
                value = x.tread,
                text = x.tread
            }).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PlaceOrderIpad(string custNo, string workOrd)
        {

            Trace.WriteLine("custNo:" + custNo + "  " + workOrd);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            
            var custInfo = db.tbl_treadtracker_customer_retread_spec.Where(x => x.customer_number.ToString() == custNo).FirstOrDefault();
            var custInfoDetails = db.tbl_tread_tracker_customers.Where(x => x.customer_number.ToString() == custNo).FirstOrDefault();

            NewWorkOrder NewWorkOrderModel = new NewWorkOrder();

            NewWorkOrderModel.customerInfo = db.tbl_cta_customers.Where(x => x.CustomerCode.ToString() == custNo).FirstOrDefault();
            NewWorkOrderModel.workOrder = workOrd;
            NewWorkOrderModel.OrderDate = DateTime.Now.ToString("yyyy-MM-dd");
            //NewWorkOrderModel.SubmittedEmployee = Session["userID"].ToString();

            if (custInfo != null)
            {
                NewWorkOrderModel.AgeLimit = custInfo.age_limit;
                NewWorkOrderModel.BrandRequirements = custInfo.brand_name_only;
            }

            if (custInfoDetails != null)
            {
                NewWorkOrderModel.customerContact = custInfoDetails.reporting_contact;
                NewWorkOrderModel.reportingEmail = custInfoDetails.reporting_email;
            }

            return PartialView(NewWorkOrderModel);
        }

    }
}
