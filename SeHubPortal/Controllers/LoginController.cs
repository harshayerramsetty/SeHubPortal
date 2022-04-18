using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Net;
using System.Web;
using System.Data;
using System.Drawing;
using System.IO;

namespace SeHubPortal.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult SignIn(LoginViewModel model)
        {
            string myIP = System.Web.HttpContext.Current.Request.UserHostAddress;

            //string myIP = GetIPAddress();
            //Trace.WriteLine("My IP Address is :" + myIP);

            model.ip = myIP;

            return View(model);
        }

        public string GetIPAddress()
        {
            string ip;
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = null;
            Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            foreach (IPAddress IP in Host.AddressList)
            {
                ip = Convert.ToString(IP);
            }
            return Host.AddressList[1].ToString();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel userCredentials)
        {
            //Trace.WriteLine("Yes:" + userCredentials.EmailAddress);
            using (CityTireAndAutoEntities db = new CityTireAndAutoEntities())
            {
                var empDetails = db.tbl_employee.Where(x => x.cta_email == userCredentials.EmailAddress).FirstOrDefault();

                int appAccess;

                if (db.tbl_sehub_access.Where(x => x.employee_id == empDetails.employee_id).Select(x => x.app_access).FirstOrDefault().HasValue)
                {
                    appAccess = db.tbl_sehub_access.Where(x => x.employee_id == empDetails.employee_id).Select(x => x.app_access).FirstOrDefault().Value;
                }
                else
                {
                    appAccess = 0;
                }



                if (empDetails is null || userCredentials.EmailAddress == "" || userCredentials.password == "" || appAccess == 0)
                {
                    userCredentials.LoginErrorMessage = "Invalid EmployeeId or Password";
                    return View("SignIn", userCredentials);
                }
                else
                {
                    var userDetails = db.tbl_employee_credentials.Where(x => x.employee_id == empDetails.employee_id && x.password == userCredentials.password).FirstOrDefault();
                    if (userDetails is null)
                    {
                        userCredentials.LoginErrorMessage = "Invalid EmployeeId or Password";
                        return View("SignIn", userCredentials);
                    }
                    else
                    {
                        Session["userID"] = userDetails.employee_id;
                        Debug.WriteLine("Session ID:" + userDetails.employee_id);


                        tbl_login_log log = new tbl_login_log();


                        DateTime timeUtc = DateTime.UtcNow;
                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
                        DateTime nstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);


                        log.employee_id = userDetails.employee_id.ToString();
                        log.time_stamp = nstTime;
                        log.ip_address = System.Web.HttpContext.Current.Request.UserHostAddress;

                        db.tbl_login_log.Add(log);
                        db.SaveChanges();

                        return RedirectToAction("Dashboard", "Main");
                    }

                }
            }
        }

        [HttpPost]
        public ActionResult HarpoonSignIn(LoginViewModel userCredentials)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            if (db.tbl_harpoon_users.Where(x => x.email == userCredentials.EmailAddress && x.password == userCredentials.password).FirstOrDefault() != null && db.tbl_harpoon_clients.Where(x => x.client_id == db.tbl_harpoon_users.Where(z => z.email == userCredentials.EmailAddress).Select(y => y.client_id).FirstOrDefault()).Select(x => x.status).FirstOrDefault() != 0)
            {
                Session["userID"] = userCredentials.EmailAddress;

                /*
                 
                if (db.tbl_harpoon_users.Where(x => x.email == userCredentials.EmailAddress).Select(x => x.profile).FirstOrDefault() == "Administrator")
                {
                    return RedirectToAction("System", "SettingsHarpoon");
                }
                else
                {
                    
                }
                 
                 */

                return RedirectToAction("TimeClockEvents", "ManagementHarpoon", new { locId = "", employeeId = "" });

            }
            else
            {
                return RedirectToAction("HarpoonLogin", "Login");
            }

        }

        public ActionResult GeneralAccess()
        {
            string myIP = System.Web.HttpContext.Current.Request.UserHostAddress;

            if (myIP == "198.49.66.25") //myIP == "198.49.66.25"
            {
                Session["userID"] = 61000;
                return RedirectToAction("Dashboard", "Main");
            }
            else
            {

                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress("jordan.blackwood@citytire.com", "IT Team"));
                msg.From = new MailAddress("noreply@citytire.com", "Sehub");
                msg.Subject = "Alert";
                msg.Body = "Unauthorized attempt to login from IP address : " + myIP;
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }


                return RedirectToAction("SignIn", "Login", new { ac = "Denied" });
            }

        }

        [HttpGet]
        public ActionResult SignUp(string value)
        {

            Debug.WriteLine("In SignUp");
            SignUpViewModel obj = new SignUpViewModel();
            return PartialView(obj);
        }


        public ActionResult ChangePassword(string clientID)
        {
            LoginViewModel model = new LoginViewModel();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            model.newHarpoonUser = new SignUpViewModel();

            var email = db.tbl_harpoon_clients.Where(x => x.client_id == clientID).Select(x => x.client_email).FirstOrDefault();

            if (email != null)
            {
                model.newHarpoonUser.email = email;
                model.newHarpoonUser.clientID = clientID;
            }
            else
            {
                model.newHarpoonUser.clientID = clientID;
            }

            return View(model);
        }

        /*
         [HttpPost]
        public ActionResult EditEmployeeInfo(MyStaffViewModel model, HttpPostedFileBase EmployeeImage)
        { 
        
        }
        */

        [HttpGet]
        public ActionResult EditProfile(LoginViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            model.newHarpoonUser = new SignUpViewModel();
            model.newHarpoonUser.email = Session["userID"].ToString();
            model.newHarpoonUser.profile = db.tbl_harpoon_users.Where(x => x.email == model.newHarpoonUser.email).Select(x => x.profile).FirstOrDefault();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SignUp(SignUpViewModel model)
        {

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("IT@citytire.com", "IT Team"));
            msg.From = new MailAddress("sehub@citytire.com", "Sehub");
            msg.Subject = "Request for New Credentials";
            msg.Body = "Hello Team," + model.FirstName + " " + model.LastName + " has requested new credentials. Please help them with new credentials";
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sehub@citytire.com", "$3hub1977");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return RedirectToAction("SignIn", "Login");
        }

        [HttpPost]
        public ActionResult SetUpPassword(LoginViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var user = db.tbl_harpoon_users.Where(x => x.email == model.newHarpoonUser.email).FirstOrDefault();
            user.password = model.newHarpoonUser.password;
            user.client_id = model.newHarpoonUser.clientID;
            db.SaveChanges();

            return RedirectToAction("HarpoonLogin", "Login");
        }

        [HttpPost]
        public ActionResult ResetProfilePassword(LoginViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string email = Session["userID"].ToString();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();
            if (user != null) {
                if(user.password == model.OldPassWord)
                {
                    if (model.NewPassword == model.OldPassWord)
                    {
                        user.password = model.NewPassword;
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("HarpoonLogin", "Login");
        }

        [HttpPost]
        public ActionResult ResendEmail(LoginViewModel model)
        {
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(model.resendEmail, "IT Team"));
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.Subject = "Change Pasword";
            msg.Body = "<a href='https://sehubportal.azurewebsites.net/Login/ChangePassword?clientID=" + model.resendEmail + "'>click the link to setup password</a>";
            msg.IsBodyHtml = true;

            SmtpClient client1 = new SmtpClient();
            client1.UseDefaultCredentials = false;
            client1.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
            client1.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client1.Host = "smtp.office365.com";
            client1.DeliveryMethod = SmtpDeliveryMethod.Network;
            client1.EnableSsl = true;
            try
            {
                client1.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return RedirectToAction("HarpoonLogin", "Login");
        }


        [HttpPost]
        public string CheckUserExistance(string email)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();
            if (user == null)
            {
                return "true";
            }
            else
            {
                return email;
            }

        }

        [HttpPost]
        public bool ForgotPassword(string email)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var user = db.tbl_harpoon_users.Where(x => x.email == email).FirstOrDefault();
            if(user != null)
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(email, "IT Team"));
                msg.From = new MailAddress("noreply@citytire.com", "Sehub");
                msg.Subject = "Change Pasword";
                msg.Body = "<a href='https://sehubportal.azurewebsites.net/Login/ChangePassword?clientID=" + email + "'>click the link to setup password</a>";
                msg.IsBodyHtml = true;

                SmtpClient client1 = new SmtpClient();
                client1.UseDefaultCredentials = false;
                client1.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
                client1.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
                client1.Host = "smtp.office365.com";
                client1.DeliveryMethod = SmtpDeliveryMethod.Network;
                client1.EnableSsl = true;
                try
                {
                    client1.Send(msg);
                    Debug.WriteLine("Message Sent Succesfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                return true;

            }
            else
            {
                return false;
            }

        }

        [HttpPost]
        public ActionResult HarpoonSignUp(LoginViewModel model, HttpPostedFileBase Logo)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_harpoon_users user = new tbl_harpoon_users();
            tbl_harpoon_clients client = new tbl_harpoon_clients();

            user.email = model.newHarpoonUser.email;
            user.profile = "Administrator";

            client.client_id = (Convert.ToInt32(db.tbl_harpoon_clients.OrderByDescending(x => x.client_id).Select(x => x.client_id).FirstOrDefault()) + 1).ToString();
            client.client_name = model.newHarpoonUser.companyName;
            client.client_address1 = model.newHarpoonUser.companyAddress1;
            client.client_address2 = model.newHarpoonUser.companyAddress2;
            client.client_city = model.newHarpoonUser.companyCity;
            client.client_province = model.newHarpoonUser.companyProvince;
            client.client_postal = model.newHarpoonUser.companyPostalCode;
            client.client_country = model.newHarpoonUser.Country;
            client.client_phone = model.newHarpoonUser.companyPhone;
            client.client_email = model.newHarpoonUser.email;
            client.client_fax = model.newHarpoonUser.companyFax;
            client.client_website = model.newHarpoonUser.companyWebsite;

            byte[] imageBytes = null;
            if (Logo != null && Logo.ContentLength > 0)
            {
                var imageName = Path.GetFileName(Logo.FileName);
                //Debug.WriteLine("EmployeeImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;

                using (Image image = Image.FromStream(Logo.InputStream, true, true))
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
            client.client_logo = imageBytes;

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(model.newHarpoonUser.email, "IT Team"));
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.Subject = "Change Pasword";
            msg.Body = "<a href='https://sehubportal.azurewebsites.net/Login/ChangePassword?clientID=" + client.client_id + "'>click the link to setup password</a>";
            msg.IsBodyHtml = true;

            SmtpClient client1 = new SmtpClient();
            client1.UseDefaultCredentials = false;
            client1.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
            client1.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client1.Host = "smtp.office365.com";
            client1.DeliveryMethod = SmtpDeliveryMethod.Network;
            client1.EnableSsl = true;
            try
            {
                client1.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            db.tbl_harpoon_users.Add(user);
            db.tbl_harpoon_clients.Add(client);

            db.SaveChanges();
            return RedirectToAction("HarpoonLogin", "Login");

        }

        [HttpGet]
        public ActionResult ForgetPasssword(string value)
        {

            Debug.WriteLine("In ForgetPasssword");
            ForgetPasswordViewModel obj = new ForgetPasswordViewModel();
            return PartialView(obj);
        }

        [HttpPost]
        public ActionResult ForgetPasssword(ForgetPasswordViewModel model)
        {

            Debug.WriteLine("Values:"+ model.EmailAddress);

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("IT@citytire.com", "IT Team"));
            msg.From = new MailAddress("sehub@citytire.com", "Sehub");
            msg.Subject = "Forget Password";
            msg.Body = "Hello Team,"+model.Name+" forgot his/her password. Please help them with password for email address:"+model.EmailAddress;
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sehub@citytire.com", "$3hub1977");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return RedirectToAction("SignIn", "Login");
        }

        public ActionResult HarpoonLogin()
        {
            return View();
        }

        public ActionResult Download()
        {
            return View();
        }

        public FileResult DownloadFile()
        {
            string path = Server.MapPath("~/Content/Download/");
            string fileName = Path.GetFileName("Harpoon_IOT_TimeClock_1.0.10.0_arm.msixbundle");
            string fulPath = Path.Combine(path, fileName);

            return File(fulPath, "file/msixbundle", "App.msixbundle");
        }
    }
}