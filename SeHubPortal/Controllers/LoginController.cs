using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel userCredentials)
        {
            Debug.WriteLine("Yes:" + userCredentials.EmailAddress);
            using (CityTireAndAutoEntities db = new CityTireAndAutoEntities())
            {

                var empDetails = db.tbl_employee.Where(x => x.cta_email == userCredentials.EmailAddress).FirstOrDefault();
               
                if (empDetails is  null)
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
                        return RedirectToAction("Dashboard", "Main");
                    }
                    
                }
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


    }
}