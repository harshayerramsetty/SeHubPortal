using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class SignUpViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string employeeId { get; set; }



        public string email { get; set; }
        public string clientID { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string profile { get; set; }
        public string companyName { get; set; }
        public string companyAddress1 { get; set; }
        public string companyAddress2 { get; set; }
        public string companyCity { get; set; }
        public string companyProvince { get; set; }
        public string companyPostalCode { get; set; }
        public string Country { get; set; }
        public string companyPhone { get; set; }
        public string companyFax { get; set; }
        public string companyEmail { get; set; }
        public string companyWebsite { get; set; }
    }
}