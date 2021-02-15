using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class LibraryAddDocumentRequest
    {
        public string ResourceType { get; set; }
        public string DocumentName { get; set; }
        public string Description { get; set; }
        public int Permission { get; set; }
        public tbl_sehub_access SehubAccess { get; set; }
    }
}