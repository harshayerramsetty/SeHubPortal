using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
 

namespace SeHubPortal.ViewModel
{
    public class FleetTvtEditAccount
    { 
        public List<SelectListItem> CustomerList { get; set; }
        public string Customer { get; set; }
        public List<tbl_fleetTVT_unit> UnitsForCustomer { get; set; }
        public tbl_sehub_access SehubAccess { get; set; } 
        public List<SelectListItem> LocationList { get; set; }
        public string Location { get; set; }
        public tbl_fleettvt_Customer customerDetails { get; set; }
        public bool Active { get; set; }
        public int tractorCount { get; set; }
        public int trailerCount { get; set; }
        public int forkliftCount { get; set; }
        public double SteerTiresCount { get; set; }
        public double DriveTiresCount { get; set; }
        public double trailerTiresCount { get; set; }
        public double forkliftTiresCount { get; set; }

        public tbl_fleetTVT_unit AddUnit { get; set; }

        public string reportingContact { get; set; }
        public string reportingEmail { get; set; }
        public string reportingFrequency { get; set; }
        public double pullPointSteer { get; set; }
        public double pullPointDrive { get; set; }
        public double pullPointTrailer { get; set; }

        public double reGroovePointForklift { get; set; }

        public bool mileageRequired { get; set; }
        public bool inspectionDateRequired { get; set; }
        public byte[] image { get; set; }

        public bool mileageRequiredAdd { get; set; }

        public List<SelectListItem> ConfigurationsListTractor { get; set; }
        public List<SelectListItem> ConfigurationsListTrailer { get; set; }
        public List<SelectListItem> ConfigurationsListForklift { get; set; }
        public List<SelectListItem> ConfigurationsType { get; set; }
        public List<SelectListItem> SizesList { get; set; }

        public List<tbl_fleetTVT_fieldsurvey_tire> LatestSurvey { get; set; }

        public List<tbl_fleettvt_configurations> ConfigurationsTable { get; set; }

        public List<tbl_source_tire_condition> tireCondition { get; set; }
        public List<tbl_source_wheel_condition> wheelCondition { get; set; }
        public List<tbl_source_tire_wear> tireWear { get; set; }

        public List<tbl_fleetTVT_fieldsurvey_unit> survyUnits { get; set; }
    }
}