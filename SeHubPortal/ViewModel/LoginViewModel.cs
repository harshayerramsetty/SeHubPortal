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
        public string ip { get; set; }

        public string resendEmail { get; set; }

        public SignUpViewModel newHarpoonUser { get; set; }

        public string OldPassWord { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

    }
}