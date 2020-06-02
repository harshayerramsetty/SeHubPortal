using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SeHubPortal.ViewModel
{
    public class CustomerListViewModel
    {
        public List<SelectListItem> Customers { get; set; }
        public string CustId { get; set; }
        public string WorkOrderNumber { get; set; }

        public string searchWithName { get; set; }
        public List<SelectListItem> MatchedNames { get; set; }
        public string MatchedNameId { get; set; }

        public string searchWithNo { get; set; }
       

    }
}