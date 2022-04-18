using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class TrainingViewModel
    {
        public string ResourceType { get; set; }
        public string employee { get; set; }
        public DateTime expirationDate { get; set; }
        public List<SelectListItem> employeesList { get; set; }
        public string location { get; set; }
        public List<SelectListItem> locationsList { get; set; }
        public string Description { get; set; }
        public int Permission { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<tbl_management_training> TrainingList { get; set; }
    }
}