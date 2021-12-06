using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Net;

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
                    var userDetails = db.tbl_employee_credentials.Where(x => x.employee_id == empDetails.employee_id && x.password== userCredentials.password).FirstOrDefault();
                    if(userDetails is null)
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
        public ActionResult HarpoonSignIn()
        {
            return RedirectToAction("Employees", "SettingsHarpoon");
        }

        public ActionResult GeneralAccess()
        {
            string myIP = System.Web.HttpContext.Current.Request.UserHostAddress;

            if(myIP == "198.49.66.25") //myIP == "198.49.66.25"
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
                

                return RedirectToAction("SignIn", "Login", new {ac = "Denied" });
            }

        }

        [HttpGet]
        public ActionResult SignUp(string value)
        {

            Debug.WriteLine("In SignUp");
            SignUpViewModel obj = new SignUpViewModel();
            return PartialView(obj);
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

    }
}