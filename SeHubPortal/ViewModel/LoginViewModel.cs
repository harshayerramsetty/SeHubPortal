using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class LoginViewModel
    {
        public string EmailAddress { get; set; }
        public string password { get; set; }
        public string LoginErrorMessage { get; set; }
    }
}