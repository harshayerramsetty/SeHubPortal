using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class fleetTVTCustomerViewModel
    {
        public string custNum { get; set; }
        public string reportingContact { get; set; }
        public string reportingEmail { get; set; }
        public bool mileageRequired { get; set; }
        public bool dateExpired { get; set; }
        public double pullPointSteer { get; set; }
        public double pullPointDrive { get; set; }
        public double pullPointTrailer { get; set; }
    }
}