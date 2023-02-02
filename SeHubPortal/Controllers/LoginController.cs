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
using System.Net.Security;
using Renci.SshNet;


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


        [HttpGet]
        public void AzureEmail()
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var activeLocations = db.tbl_cta_locations_reporting.Where(x => x.salesReport_daily == true).Select(x => x.loc_id).OrderByDescending(x => x).ToList();

            foreach (var loc in activeLocations)
            {
                string textBody = " <table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr bgcolor='#4da6ff'><td> <b>Name</b> </td> <td> <b>Position</b> </td></tr>";

                var Employees = db.tbl_employee_attendance.Where(x => x.at_work_location == loc && x.at_work == true).Select(x => x.employee_id).ToList();

                var db1 = (from a in db.tbl_employee_attendance.Where(x => x.at_work_location == loc && x.at_work == true) select a).ToList();
                var db2 = (from a in db.tbl_employee select a).ToList();

                var EmployeesList = (from a in db1
                                     join b in db2 on a.employee_id equals b.employee_id
                                     orderby b.full_name
                                     select new { ful_name = b.full_name, position = b.cta_position }).ToList();


                int auto_tech = 0;
                int tire_tech = 0;
                int comm_tire_tech = 0;

                foreach (var emp in EmployeesList)
                {

                    if (emp.position == "Journeyman" | emp.position == "1st Year Apprentice" | emp.position == "2nd Year Apprentice" | emp.position == "3rd Year Apprentice" | emp.position == "4th Year Apprentice")
                    {
                        auto_tech++;
                    }
                    else if (emp.position == "Tire Tech 1" | emp.position == "Tire Tech 2" | emp.position == "Tire Tech 3" )
                    {
                        tire_tech++;
                    }
                    else if (emp.position == "Tire Tech 4" | emp.position == "Tire Tech 5" | emp.position == "Tire Tech 6")
                    {
                        comm_tire_tech++;
                    }
                    textBody += "<tr size="+ 1 + "><td size=" + 1 + "> " + emp.ful_name + "</td><td> " + emp.position + "</td> </tr>";
                }
                textBody += "</table>";

                if (auto_tech + tire_tech + comm_tire_tech != 0)
                {
                    var doorrate = db.tbl_source_labour_rates.Where(x => x.loc_id == loc).FirstOrDefault();

                    double? mechanical_labour = doorrate.unit_price * 8 * (auto_tech);
                    double? tire_labour = doorrate.unit_price * 8 * (comm_tire_tech + (0.75 * tire_tech));

                    string headder = "<u>Sales Objectives for Today</u><br><br>";

                    if (mechanical_labour != 0)
                    {
                        headder += "<b>Mechanical Labour &nbsp; &nbsp;$ " + Math.Round(mechanical_labour.Value, 2).ToString("N2") + "</b><br>";
                    }

                    if (tire_labour != 0)
                    {
                        headder += "<b>Tire Labour &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  $ " + Math.Round(tire_labour.Value, 2).ToString("N2") + "</b><br><br>";
                    }

                    if (mechanical_labour != 0 || tire_labour != 0)
                    {
                        headder += "<p>The values above are based upon current attendance. See table below for complete list</p>";
                    }

                    tbl_reporting_sales_labour_daily record = new tbl_reporting_sales_labour_daily();
                    record.loc_id = loc;
                    record.date = System.DateTime.Today;
                    record.mechanical_labour = mechanical_labour.Value;
                    record.tire_labour = tire_labour.Value;

                    db.tbl_reporting_sales_labour_daily.Add(record);
                    db.SaveChanges();

                    textBody = headder + textBody;

                    MailMessage msg = new MailMessage();
                    msg.To.Add(new MailAddress(db.tbl_cta_location_info.Where(x => x.loc_id == loc).Select(x => x.management_email).FirstOrDefault()));
                    msg.Bcc.Add(new MailAddress("jordan.blackwood@citytire.com", "IT Team")); //jordan.blackwood@citytire.com      harsha.yerramsetty@citytire.com
                    msg.Bcc.Add(new MailAddress("everett.blackwood@citytire.com", "IT Team")); //jordan.blackwood@citytire.com      harsha.yerramsetty@citytire.com
                    msg.From = new MailAddress("noreply@citytire.com", "Sehub");
                    msg.Subject = "Daily Labour Sales Objective - " + System.DateTime.Today.ToString("dddd MMMM dd, yyyy");
                    msg.Body = textBody;
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
                }
                
            }
            
        }

        [HttpGet]
        public void AzureEmailMonthlySales()
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            var last = month.AddDays(-1);

            var salesForMonth = db.tbl_reporting_sales_labour_daily
                .Where(x => x.date > first && x.date < last)
                .GroupBy(n => new { n.loc_id})
                .Select(g => new {
                    location = g.Key.loc_id,
                    mechanical_labour = g.Sum(x => x.mechanical_labour),
                    tire_labour = g.Sum(x => x.tire_labour)
                }).ToList();

            

            foreach (var sal in salesForMonth)
            {
                double? total = sal.mechanical_labour + sal.tire_labour;

                string LB = " <br/><br/> ";
                string Line1 = "<u>Did You Meet Your Target?</u>";
                string Line2 = "Mechanical Labour Target &nbsp &nbsp &nbsp &nbsp $" + sal.mechanical_labour.Value.ToString("N2");
                string Line3 = "Tire Labour Target  &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp $" + sal.tire_labour.Value.ToString("N2");                
                string Line4 = "<b> Total Labour Sales Target &nbsp &nbsp &nbsp &nbsp $" + total.Value.ToString("N2")+"</b>";
                string Line5 = "Run your Statistics Inquiry for the Month (like this – picture below)";                
                string Line6 = "<img src=" + '"' + "https://ctasehub.blob.core.windows.net/sehub-content/Statistics_Inquiry.png" + '"' + "></img>";
                string Line7 = "Don’t Forget to change the Settings from Current to “Last Month” and Todays to “Month to Date” (like this – pictures below)";
                string Line8 = "<img src=" + '"' + "https://ctasehub.blob.core.windows.net/sehub-content/Last_month.png" + '"' + "></img>";
                string Line9 = "<img src=" + '"' + "https://ctasehub.blob.core.windows.net/sehub-content/Month_to_date.png" + '"' + "></img>";

                string mailString = Line1 + LB + Line2 + "<br/>" + Line3 + LB + Line4 + LB + LB + Line5 + LB + Line6 + LB + Line7 + LB + Line8 + Line9;

                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(db.tbl_cta_location_info.Where(x => x.loc_id == sal.location).Select(x => x.management_email).FirstOrDefault()));
                msg.Bcc.Add(new MailAddress("jordan.blackwood@citytire.com", "IT Team")); //jordan.blackwood@citytire.com      harsha.yerramsetty@citytire.com
                msg.Bcc.Add(new MailAddress("everett.blackwood@citytire.com", "IT Team")); //jordan.blackwood@citytire.com      harsha.yerramsetty@citytire.com
                //msg.To.Add(new MailAddress("harsha.yerramsetty@citytire.com", "IT Team")); //jordan.blackwood@citytire.com      harsha.yerramsetty@citytire.com
                msg.From = new MailAddress("noreply@citytire.com", "Sehub");
                msg.Subject = sal.location + " Labour Sales Performance - " + first.ToString("yyyy MMMM");
                msg.Body = mailString;
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
            }
        }

        [HttpGet]
        public void restartRaspberryPI()
        {
            /*
            SshClient sshclient = new SshClient("172.0.0.1", userName, password);
            sshclient.Connect();
            SshCommand sc = sshclient.CreateCommand("Your Commands here");
            sc.Execute();
            string answer = sc.Result;
            */


            //Connection information
            string user = "cta-admin";
            string pass = "$ince1977";
            string host = "10.232.7.3"; //System.Web.HttpContext.Current.Request.UserHostAddress;

            //Set up the SSH connection
            using (var client = new SshClient(host, user, pass))
            {
                //Start the connection
                client.Connect();
                SshCommand sc = client.CreateCommand("sudo reboot");
                try
                {
                    sc.Execute();
                }
                catch
                {
                    Trace.WriteLine("Reached restart");
                }                
                
            }
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
            Trace.WriteLine("Yes:" + userCredentials.EmailAddress);
            using (CityTireAndAutoEntities db = new CityTireAndAutoEntities())
            {
                Trace.WriteLine(" Reached 1 ");
                var empDetails = db.tbl_employee.Where(x => x.cta_email == userCredentials.EmailAddress).FirstOrDefault();

                int appAccess;

                if (db.tbl_sehub_access.Where(x => x.employee_id == empDetails.employee_id).Select(x => x.app_access).FirstOrDefault().HasValue)
                {
                    Trace.WriteLine(" Reached 2 ");
                    appAccess = db.tbl_sehub_access.Where(x => x.employee_id == empDetails.employee_id).Select(x => x.app_access).FirstOrDefault().Value;
                }
                else
                {
                    Trace.WriteLine(" Reached 3 ");
                    appAccess = 0;
                }



                if (empDetails is null || userCredentials.EmailAddress == "" || userCredentials.password == "" || appAccess == 0)
                {
                    Trace.WriteLine(" Reached 4 ");
                    userCredentials.LoginErrorMessage = "Invalid EmployeeId or Password";
                    return View("SignIn", userCredentials);
                }
                else
                {
                    Trace.WriteLine(" Reached 5 ");
                    var userDetails = db.tbl_employee_credentials.Where(x => x.employee_id == empDetails.employee_id && x.password == userCredentials.password).FirstOrDefault();
                    if (userDetails is null)
                    {
                        Trace.WriteLine(" Reached 6 ");
                        userCredentials.LoginErrorMessage = "Invalid EmployeeId or Password";
                        return View("SignIn", userCredentials);
                    }
                    else
                    {
                        Trace.WriteLine(" Reached 7 ");
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

        public ActionResult Adds()
        {

            return View();
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
            user.loc_id = "All";

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