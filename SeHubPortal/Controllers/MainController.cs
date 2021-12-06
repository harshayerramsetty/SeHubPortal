using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.ViewModel;
using System.Configuration;
using System.Data.SqlClient;


namespace SeHubPortal.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        [HttpPost]
        public ActionResult DashboardChangeLocation(LocationsMap model)
        {
            return RedirectToAction("LocationsMap", new { loc = model.SelectedLocationId });
        }

        public ActionResult Dashboard(MainDashboard modal)
        {
            //System.Diagnostics.Trace.WriteLine("whatever");                       

            Debug.WriteLine("NotEver");

            Console.WriteLine("Reached");

            if (CheckPermissions()!=null)
            {
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                int empId = Convert.ToInt32(Session["userID"].ToString());
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                modal.SehubAccess = empDetails;

                if (modal.SehubAccess.main == 0)
                {
                    return RedirectToAction("Dashboard", "Library");
                }
                else if (modal.SehubAccess.mainDashboard == 0) {
                    return RedirectToAction("Calendar", "Main");
                }
            }
            else
            {
                return RedirectToAction("SignIn", "Login");
            }

            //System.Diagnostics.Trace.WriteLine(" this is the permissions for dashboard main *******   " + modal.SehubAccess.mainDashboard + "      **********");

            return View(modal);
        }
        public ActionResult LocationsMap(LocationsMap modal, string loc)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            
            int empId = Convert.ToInt32(Session["userID"].ToString());

            modal.LocationsList = populateLocationsPermissions(empId);

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if(loc != null)
            {
                modal.SelectedLocationId = loc;
            }
            else
            {
                modal.SelectedLocationId = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            modal.locdesc = db.tbl_cta_location_info.Where(x => x.loc_id == modal.SelectedLocationId).FirstOrDefault();

            //System.Diagnostics.Trace.WriteLine(" this is the permissions for dashboard main *******   " + modal.SehubAccess.mainDashboard + "      **********");

            return View(modal);
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

        public JsonResult GetEvents()
        {
            CityTireAndAutoEntities dc = new CityTireAndAutoEntities();

            var events = dc.tbl_Calendar_events.ToList();

            var vacations = dc.tbl_vacation_schedule.ToList();

            foreach (var item in vacations)
            {
                if(dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.status).FirstOrDefault() == 1)
                {
                    tbl_Calendar_events eve = new tbl_Calendar_events();
                    eve.subject = "Vacation";
                    eve.start_date = item.start_date.AddDays(1);
                    eve.end_date = item.end_date.Value.AddDays(1);
                    eve.Description = dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.full_name).FirstOrDefault() + " " + item.leave_type;

                    events.Add(eve);
                }
            }

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public JsonResult GetBirthdayEvents()
        {
            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                var events = dc.tbl_employee.Where(x => (x.status == 1)).ToList();
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public ActionResult Calendar(FileURL model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            model.LocationsList = populateLocationsPermissions(empId);

            if (model.SehubAccess.mainCalendar == 0)
            {
                return RedirectToAction("Dashboard", "Library");
            }

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


        public ActionResult Newsletter()
        {
            return View();
        }
        public ActionResult LogOut()
        {
            int userId = (int)Session["userID"];
            Session.Abandon();
            return RedirectToAction("SignIn", "Login");
        }
        [HttpPost]
        public ActionResult Newsletter(HttpPostedFileBase reportName)
        {
           
            if (reportName != null && reportName.ContentLength > 0)
            {
                var imageName = Path.GetFileName(reportName.FileName);
                Debug.WriteLine("reportName:" + imageName);
                string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/"+ imageName;
                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
                using (Image image = Image.FromFile(fileName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        Debug.WriteLine("Image base64:" + base64String);
                        CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                        int empId = Convert.ToInt32(Session["userID"].ToString());
                        //Debug.WriteLine("empId:" + empId);
                        var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
                        if (result != null)
                        {
                            result.profile_pic = imageBytes;                            

                            //Testing
                            Debug.WriteLine(result.employee_id +"    "+ result.full_name);
                          
                            //Debug.WriteLine("**************************");
                        }
                        db.SaveChanges();
                    }
                }


            }
            return View();
        }

        public ActionResult ProfileBlock(tbl_employee employee)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            employee = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            return PartialView(employee) ;
        }

        [HttpPost]
        public ActionResult UploadProfileImage(HttpPostedFileBase EmployeeImage)
        {

            int empId = Convert.ToInt32(Session["userID"].ToString());

            byte[] imageBytes = null;
            if (EmployeeImage != null && EmployeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(EmployeeImage.FileName);
                //Debug.WriteLine("EmployeeImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;

                using (Image image = Image.FromStream(EmployeeImage.InputStream, true, true))
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
            var EmployeeInfo = db.tbl_employee.Where(a => a.employee_id == empId).FirstOrDefault();

            if (imageBytes != null)
            {
                EmployeeInfo.profile_pic = imageBytes;
            }

            db.SaveChanges();
            return RedirectToAction("Dashboard", "Main");
        }

    }
}