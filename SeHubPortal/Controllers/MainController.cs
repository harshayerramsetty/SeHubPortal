using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult Calendar()
        {
            return View();
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

       
    }
}