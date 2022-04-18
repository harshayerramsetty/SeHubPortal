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

            var config = db.tbl_tread_tracker_customers.ToList();

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
                    Text = custDetails.cust_name + " (" + val.customer_number + ")",
                    Value = Convert.ToString(val.customer_number)
                });
            }
            return items;
        }

        public ActionResult ProductionSchedule(ProductionScheduleViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            model.ProductionItems = db.tbl_production_schedule.OrderBy(x => x.priority).ToList();

            tbl_production_schedule newOrder = new tbl_production_schedule();

            newOrder.production_id = db.tbl_production_schedule.OrderByDescending(x => x.production_id).Select(x => x.production_id).FirstOrDefault()+1;

            model.NewOrder = newOrder;

            model.ProductionItems = db.tbl_production_schedule.OrderBy(x => x.priority).ToList();
            
            return View(model);
        }

        public ActionResult ProductionCustomer(ProductionScheduleViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            DateTime initialDate = new DateTime(2018, 01, 25);

            var workorders = db.tbl_treadtracker_barcode.Where(x => (x.line_code == 1 || x.line_code == 5 || x.line_code == 10 || x.line_code == 11) && (x.preliminary_inspection_result == "GOOD" && (x.ndt_machine_result == "GOOD" || x.ndt_machine_result == null) && (x.buffer_builder_result == "GOOD" || x.buffer_builder_result == null) && x.final_inspection_result == null && x.preliminary_inspection_date > initialDate)).Select(x => new { x.retread_workorder, x.retread_design, x.casing_size, x.virgin, x.preliminary_inspection_date }).Distinct().ToList();

            List<tbl_production_schedule> lineItems = new List<tbl_production_schedule>();

            foreach (var work in workorders)
            {
                tbl_production_schedule lineItem = new tbl_production_schedule();
                lineItem.production_id = Convert.ToInt32(work.retread_workorder);
                lineItem.production_date = work.preliminary_inspection_date;
                lineItem.size = work.casing_size;
                lineItem.tread = work.retread_design;
                lineItem.quantity = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == work.retread_workorder && (x.line_code == 1 || x.line_code == 5 || x.line_code == 10 || x.line_code == 11) && (x.preliminary_inspection_result == "GOOD" && (x.ndt_machine_result == "GOOD" || x.ndt_machine_result == null) && (x.buffer_builder_result == "GOOD" || x.buffer_builder_result == null) && x.final_inspection_result == null && x.preliminary_inspection_date > initialDate)).Count();
                lineItem.quantity_complete = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == work.retread_workorder && x.final_inspection_result == "GOOD" && (x.line_code == 1 || x.line_code == 5 || x.line_code == 10 || x.line_code == 11) && (x.preliminary_inspection_result == "GOOD" && (x.ndt_machine_result == "GOOD" || x.ndt_machine_result == null) && (x.buffer_builder_result == "GOOD" || x.buffer_builder_result == null) && x.final_inspection_result == null && x.preliminary_inspection_date > initialDate)).Count();
                if (work.virgin == 1)
                {
                    lineItem.virgin = true;
                }
                else
                {
                    lineItem.virgin = false;
                }

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
            model.CustomerList = populateCustomers();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var tb1 = (from a in db.tbl_treadtracker_workorder.Where(x => x.customer_number == custID) select a).ToList();
            var tb2 = (from a in db.tbl_treadtracker_barcode select a).ToList();

            var CasingList = (from a in tb1
                          join b in tb2 on a.retread_workorder equals b.retread_workorder
                              where (a.customer_number == custID)
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

            if (custID != null)
            {
                model.Customer = custID;
                model.customerDetails = db.tbl_tread_tracker_customers.Where(x => x.customer_number == custID).FirstOrDefault();

                if (model.customerDetails != null)
                {
                    model.reportingContact = model.customerDetails.reporting_contact;
                    model.reportingEmail = model.customerDetails.reporting_email;
                }

                var workOrders = db.tbl_treadtracker_workorder.Where(x => x.customer_number == custID).ToList();

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
                    woinfo.closedBarcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == WO.retread_workorder && (x.preliminary_inspection_result == "GOOD" && x.buffer_builder_result == "GOOD" && x.ndt_machine_result == "GOOD" && x.final_inspection_result == "GOOD")).Count();
                    woinfo.failedBarcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == WO.retread_workorder && ((x.preliminary_inspection_result != "GOOD" && x.preliminary_inspection_result != null) || (x.buffer_builder_result != "GOOD" && x.buffer_builder_result != null) || (x.ndt_machine_result != "GOOD" && x.ndt_machine_result != null) || (x.final_inspection_result != "GOOD" && x.final_inspection_result != null))).Count();
                    woinfo.openBarcodes = barcodes - woinfo.closedBarcodes - woinfo.failedBarcodes;


                    WorkOrderInfoList.Add(woinfo);
                }

                model.WorkOrdersForCustomer = WorkOrderInfoList;

            }
            else
            {

            }

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            model.CustomerList = populateCustomers();
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
                workOrder.closedBarcodes = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == item.retread_workorder && x.preliminary_inspection_result == "GOOD" && x.ndt_machine_result == "GOOD" && x.buffer_builder_result == "GOOD" && x.final_inspection_result == "GOOD").Count();
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

            var units = db.tbl_customer_list.Where(x => x.cust_us1 == value).FirstOrDefault();

            return units.cust_us1 + ";" + units.cust_add1 + ";" + units.cust_add2 + ";"+ units.cust_add3 + ";" + units.cust_city +";" + units.cust_state + ";" + units.cust_zip;
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

        [HttpPost]
        public string PullBarcodesInfo(string value)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var units = db.tbl_treadtracker_barcode.Where(x => x.barcode == value).FirstOrDefault();

            string status = units.preliminary_inspection_result + ";" + units.ndt_machine_result + ";" + units.buffer_builder_result + ";" + units.final_inspection_result;

            return status;
        }

        public JsonResult GetInspectionDates()
        {
            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                var events = dc.tbl_treadtracker_barcode.Where(x => x.final_inspection_date > nDaysAgo).Select(x => x.final_inspection_date).Distinct().ToList();

                //Trace.WriteLine(events.preliminary_inspection_date);
                
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpGet]
        public ActionResult Dashboard(TreadTrackerDashboard model)
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
            CloudBlobContainer container = blobClient.GetContainerReference("cta-library-company-documents");

            // Retrieve reference to a blob ie "picture.jpg".

            model.FieldSheetURL = container.GetBlockBlobReference("Tread Tracker Field Sheet.pdf").Uri.AbsoluteUri;


            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            var casing = db.tbl_treadtracker_inventory_casing.ToList();
            var custList = db.tbl_customer_list.Where(x => x.cscttc == "True").GroupBy(i => i.cust_name).Select(group => group.FirstOrDefault()).ToList();


            model.CustList = custList;
            model.CasingInventory = casing;
            model.SehubAccess = empDetails;
            model.Customers = PopulateCustomers("Nothing");
            model.CustomersList = PopulateCustomersTTC();
            model.RetreadList = populateRetreads();

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
                msg.From = new MailAddress("noreply@citytire.com", "Sehub");
                msg.Subject = "Testing";
                msg.Body = "Mail for tread tracker open order";
                msg.IsBodyHtml = true;


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
            Debug.WriteLine("Inside PopulateCustomers with value:" + value);
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem
            {
                Text = "",
                Value = "",
                //Selected = sdr["cust_name"].ToString() == "CITY TIRE AND AUTO CENTRE LTD" ? true : false
            });

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select cust_name,cust_us1 FRom tbl_customer_list  where cscttc ='True' and cust_us2='0' order by cust_name";
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
            int empIdUser = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empIdUser).FirstOrDefault();
            model.Location = db.tbl_employee.Where(x => x.employee_id == empIdUser).Select(x => x.loc_ID).FirstOrDefault();
            model.Customers = populateTreadTrackerCustomers();
            model.Locations = populateLocationInfo();
            model.FailureCodes = db.tbl_source_RARcodes.ToList();
            

            DateTime startDate = Convert.ToDateTime("2018-01-01");

            model.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.preliminary_inspection_date > startDate && ((x.preliminary_inspection_result != "Good" && x.preliminary_inspection_result != null) || (x.ndt_machine_result != "GOOD" && x.ndt_machine_result != null) || (x.buffer_builder_result != "GOOD" && x.buffer_builder_result != null) || (x.final_inspection_result != "GOOD" && x.final_inspection_result != null))).ToList();
            model.workOrderInfo = db.tbl_treadtracker_workorder.Where(x => x.creation_date > startDate).ToList();

            return View(model);
        }

        public ActionResult ProductionReport(ProductionReportViewModel model, string values)
        {
            int empIdUser = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empIdUser).FirstOrDefault();
            string location = db.tbl_employee.Where(x => x.employee_id == empIdUser).Select(x => x.loc_ID).FirstOrDefault();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select * from tblLocation where locID = '" + location + "'";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            model.locID = Convert.ToString(sdr["locID"]);
                            model.addressSTREET1 = Convert.ToString(sdr["addressSTREET1"]);
                            model.addressSTREET2 = Convert.ToString(sdr["addressSTREET2"]);
                            model.addressCITY = Convert.ToString(sdr["addressCITY"]);
                            model.addressPROVINCE = Convert.ToString(sdr["addressPROVINCE"]);
                            model.addressPOSTAL = Convert.ToString(sdr["addressPOSTAL"]);
                            model.locPHONE = Convert.ToString(sdr["locPHONE"]);
                            model.locFAX = Convert.ToString(sdr["locFAX"]);
                        }

                    }
                    con.Close();
                }
            }

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

            var config = db.tbl_customer_list.ToList();

            items.Add(new SelectListItem
            {
                Text = "Select Customer",
                Value = ""
            });

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.cust_name,
                    Value = Convert.ToString(val.cust_us1)
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

            var customers = db.tbl_tread_tracker_customers.Select(x => x.customer_number).Distinct().ToList();
            foreach (var cust in customers)
            {
                items.Add(new SelectListItem
                {
                    Text = db.tbl_customer_list.Where(x => x.cust_us1 == cust).Select(x => x.cust_name).FirstOrDefault(),
                    Value = Convert.ToString(cust)
                });
            }

            return items.OrderBy(x => x.Text).ToList();
        }

        [HttpPost]
        public ActionResult SearchByName(CustomerListViewModel custList)
        {
            if (custList.searchWithName == null)
            {
                return RedirectToAction("NewOrder", new { values = "" });
            }
            string NameLike = custList.searchWithName.ToString().ToUpper();
            string custName = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select TOP 1 cust_name from tbl_customer_list where cscttc='True' and upper(cust_name) like '%" + NameLike + "%' ";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            custName = sdr["cust_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }

            return RedirectToAction("NewOrder", new { values = custName });
        }
        [HttpPost]
        public ActionResult Generate(CustomerListViewModel custList)
        {

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
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select cust_us1 FRom tbl_customer_list where cust_name=" + "'" + selectedItem.Text.ToString() + "'";
                    Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                custNum = sdr["cust_us1"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
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

                    newBarcode.barcode = Request["barcode" + i.ToString()];
                    newBarcode.retread_workorder = model.workOrder;
                    newBarcode.line_number = i;
                    newBarcode.line_code = Convert.ToInt32(Request["line" + i.ToString()]);
                    newBarcode.serial_dot = Request["serial" + i.ToString()];
                    newBarcode.casing_size = Request["casing" + i.ToString()];
                    newBarcode.casing_brand = Request["brand" + i.ToString()];
                    if(Request["virgin" + i.ToString()] == "on")
                    {
                        newBarcode.virgin = 1;
                    }
                    else
                    {
                        newBarcode.virgin = 0;
                    }
                    newBarcode.retread_design = Request["tread" + i.ToString()]; //"TBD" "--Select--"
                    newBarcode.unit_ID = Request["unit" + i.ToString()];
                    newBarcode.preliminary_inspection_result = "Good";
                    newBarcode.preliminary_inspection_date = model.InspectionDate;
                    newBarcode.ship_to_location = loc;

                    db.tbl_treadtracker_barcode.Add(newBarcode);
                    db.SaveChanges();


                }
            }

            tbl_treadtracker_workorder newWorkOrder = new tbl_treadtracker_workorder();
            newWorkOrder.retread_workorder = model.workOrder;
            newWorkOrder.customer_number = model.customerInfo.cust_us1;
            newWorkOrder.employee_Id = Convert.ToString(empId);
            newWorkOrder.loc_Id = loc;
            newWorkOrder.creation_date = model.InspectionDate;

            db.tbl_treadtracker_workorder.Add(newWorkOrder);
            db.SaveChanges();


            System.Collections.ArrayList NewOrderList = new System.Collections.ArrayList();

            NewOrderList.Clear();

            
            return RedirectToAction("Dashboard", "TreadTracker");
        }

        public ActionResult PlaceOrder(string custNo, string workOrd)
        {

            //Trace.WriteLine("custNo:" + custNo + "  " + workOrd);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();       

            var NewWorkOrderModel = new NewWorkOrder
            {

                customerInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNo).FirstOrDefault(),
                workOrder = workOrd,
                OrderDate = DateTime.Now.ToString("yyyy-MM-dd"),
                SubmittedEmployee = Session["userID"].ToString()
            };

            NewWorkOrderModel.SehubAccess = empDetails;

            return PartialView(NewWorkOrderModel);
        }
        //private OpenOrderModel openOrder = new OpenOrderModel();

        public ActionResult PopulateProductionReport(DateTime inspDate)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());            

            ProductionListViewModel finalProdList = new ProductionListViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            

            PopulateProductionReport ProdReportList = new PopulateProductionReport();

            ProdReportList.barcodeInfo = db.tbl_treadtracker_barcode.Where(x => x.final_inspection_date == inspDate && x.final_inspection_result == "GOOD").ToList();
            ProdReportList.workOrderInfo = db.tbl_treadtracker_workorder.ToList();
            ProdReportList.customerInfo = db.tbl_customer_list.ToList();

            

            //Trace.WriteLine("Reached with date" + inspDate.Date);

            return PartialView(ProdReportList);
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
                    custInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault(),
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
                    custInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault(),
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
                    custInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault(),
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

        [HttpPost]
        public ActionResult SearchWithName(NewOrderSearchCriteria model)
        {
            model.MatchedNames = PopulateMatchedValues(searchNameValue,"Name");
            var selectedItem = model.MatchedNames.Find(p => p.Value == model.MatchedNameId.ToString());
            if (selectedItem != null)
            {
                Debug.WriteLine(selectedItem.Text);

            }
            string NameLike = selectedItem.Text;
            string custName = "";

            var jPos = NameLike.IndexOf("'");
            if(jPos!=-1)
            {
                NameLike = NameLike.Insert(jPos, "'");
            }
            
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select TOP 1 cust_name from tbl_customer_list where cscttc='True' and cust_us2='0' and upper(cust_name) like '%" + NameLike + "%' ";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            custName = sdr["cust_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }

            return RedirectToAction("NewOrder", new { values = custName });
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

        [HttpPost]
        public ActionResult SearchWithNo(NewOrderSearchCriteria model)
        {
            model.MatchedNo = PopulateMatchedValues(searchNoValue, "Name");
            var selectedItem = model.MatchedNo.Find(p => p.Value == model.MatchedNoId.ToString());
            if (selectedItem != null)
            {
                Debug.WriteLine(selectedItem.Text);

            }
            string NameLike = selectedItem.Text;
            string custName = "";
            var jPos = NameLike.IndexOf("'");
            if (jPos != -1)
            {
                NameLike = NameLike.Insert(jPos, "'");
            }
           

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select TOP 1 cust_name from tbl_customer_list where cscttc='True' and cust_us2='0' and upper(cust_name) like '%" + NameLike + "%' ";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            custName = sdr["cust_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }

            return RedirectToAction("NewOrder", new { values = custName });
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

            if(custNumber == "0578749")
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

                objcvm.preliminary_inspection_result = item.preliminary_inspection_result;

                objcvm.preliminary_inspection_date = item.preliminary_inspection_date;

                objcvm.ndt_machine_result = item.ndt_machine_result;

                objcvm.ndt_machine_date = item.ndt_machine_date;

                objcvm.buffer_builder_result = item.buffer_builder_result;

                objcvm.buffer_builder_date = item.buffer_builder_date;

                objcvm.final_inspection_result = item.final_inspection_result;

                objcvm.final_inspection_date = item.final_inspection_date;

                //objcvm.ship_to_location = item.ship_to_location;

                if (item.virgin == 1)
                {
                    objcvm.virgin = true;
                }
                else
                {
                    objcvm.virgin = false;
                }


                objcvm.size_id = db.tbl_treadtracker_casing_sizes.Where(x => x.casing_size == item.casing_size).Select(x => x.size_id).FirstOrDefault();

                objcvm.brand_id = db.tbl_treadtracker_casing_brands.Where(x => x.casing_brand == item.casing_brand).Select(x => x.brand_id).FirstOrDefault();

                //objcvm.tread_id = db.tbl_source_retread_tread.Where(x => x.tread_design == item.retread_design).Select(x => x.tread_id).FirstOrDefault();

                BarcodeInfoVMlist.Add(objcvm);

            }
            var CustomerInfoList = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault();
            string custNameValue = CustomerInfoList.cust_name;
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

                objcvm.preliminary_inspection_result = item.preliminary_inspection_result;

                objcvm.preliminary_inspection_date = item.preliminary_inspection_date;

                objcvm.ndt_machine_result = item.ndt_machine_result;

                objcvm.ndt_machine_date = item.ndt_machine_date;

                objcvm.buffer_builder_result = item.buffer_builder_result;

                objcvm.buffer_builder_date = item.buffer_builder_date;

                objcvm.final_inspection_result = item.final_inspection_result;

                objcvm.final_inspection_date = item.final_inspection_date;

                objcvm.ship_to_location = item.ship_to_location;

                if(item.virgin == 1)
                {
                    objcvm.virgin = true;
                }
                else
                {
                    objcvm.virgin = false;
                }
                

                objcvm.size_id = db.tbl_treadtracker_casing_sizes.Where(x => x.casing_size == item.casing_size).Select(x => x.size_id).FirstOrDefault();

                objcvm.brand_id = db.tbl_treadtracker_casing_brands.Where(x => x.casing_brand == item.casing_brand).Select(x => x.brand_id).FirstOrDefault();

                //objcvm.tread_id = db.tbl_source_retread_tread.Where(x => x.tread_design == item.retread_design).Select(x => x.tread_id).FirstOrDefault();

                BarcodeInfoVMlist.Add(objcvm);

            }
            var CustomerInfoList = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault();
            string custNameValue = CustomerInfoList.cust_name;
            Debug.WriteLine("custNameValue:" + custNameValue + "hkjhkh");

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
            var selectedItem = model.CustomersList.Find(p => p.Value == model.ChangedCustomerId.ToString());
            if (selectedItem != null)
            {
               
                Debug.WriteLine("The selected CustomersList value in the edit button is:"+selectedItem.Text);

            }
            //end

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            
            var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.WorkOrderInfo.retread_workorder).FirstOrDefault();
            if (WorkOrderDetails != null)
            {
                
                WorkOrderDetails.customer_number = selectedItem.Value;
                //Testing
                //Debug.WriteLine("Testing the owrk order");
                //Debug.WriteLine(WorkOrderDetails.retread_workorder);
                //Debug.WriteLine(WorkOrderDetails.customer_number + "    " + selectedItem.Value);
            }

            var casingSizeList = (from a in db.tbl_treadtracker_casing_sizes select a).ToList();
            var casingBrandList = (from a in db.tbl_treadtracker_casing_brands select a).ToList();
            var casingTreadList = (from a in db.tbl_source_retread_tread select a).ToList();

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
                    string casingTread = "";


                    var result = db.tbl_treadtracker_barcode.Where(a => a.barcode.Equals(items.barcode)).FirstOrDefault();

                    if (changedBarcode != null && changedBarcode != "" && result != null)
                    {
                        var result1 = new tbl_treadtracker_barcode();

                        result1.barcode = changedBarcode;
                        result1.retread_workorder = result.retread_workorder;
                        result1.line_number = result.line_number;

                        result1.line_code = items.line_code;
                        result1.serial_dot = items.serial_dot;
                        result1.casing_size = casingSize;
                        result1.casing_brand = casingBrand;
                        result1.retread_design = casingTread;
                        result1.unit_ID = items.unit_ID;
                        result1.preliminary_inspection_result = result.preliminary_inspection_result;
                        result1.preliminary_inspection_date = result.preliminary_inspection_date;
                        result1.ndt_machine_result = result.ndt_machine_result;
                        result1.ndt_machine_date = result.ndt_machine_date;
                        result1.buffer_builder_result = result.buffer_builder_result;
                        result1.buffer_builder_date = result.buffer_builder_date;
                        result1.final_inspection_result = result.final_inspection_result;
                        result1.final_inspection_date = result.final_inspection_date;

                        if(items.virgin == true)
                        {
                            result1.virgin = 1;
                        }
                        else
                        {
                            result1.virgin = 0;
                        }


                        



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

                        result1.barcode = changedBarcode;
                        result1.retread_workorder = model.WorkOrderInfo.retread_workorder;
                        result1.line_number = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == model.WorkOrderInfo.retread_workorder).OrderByDescending(x => x.line_number).Select(x => x.line_number).FirstOrDefault()+1;

                        result1.line_code = items.line_code;
                        result1.serial_dot = items.serial_dot;
                        result1.casing_size = casingSize;
                        result1.casing_brand = casingBrand;
                        result1.retread_design = casingTread;
                        result1.unit_ID = items.unit_ID;
                        result1.preliminary_inspection_result = "GOOD";

                        Trace.WriteLine("This is the newly added barcode" + result1.barcode);

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
            var CustomerInfoList = db.tbl_customer_list.Where(x => x.cust_us1 == value).FirstOrDefault();
            if(CustomerInfoList!=null)
            {              
                ViewData["CustomerNum"] = CustomerInfoList.cust_us1;
                ViewData["Address"] = CustomerInfoList.cust_add1+","+ CustomerInfoList.cust_state+","+ CustomerInfoList.cust_zip;
            }
            
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddProductionItem(ProductionScheduleViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_production_schedule item = new tbl_production_schedule();
            item = model.NewOrder;
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

    }
}