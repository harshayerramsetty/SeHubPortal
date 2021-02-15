using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class FileURL
    {
        public List<KeyValuePair<string, string>> URLName { get; set; }
        public string Location_ID { get; set; }
        public string RenameString { get; set; }
        public int Permission { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
    }
}