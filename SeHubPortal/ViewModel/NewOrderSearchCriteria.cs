using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class NewOrderSearchCriteria
    {
        public List<SelectListItem> MatchedNo { get; set; }
        public string MatchedNoId { get; set; }

        public List<SelectListItem> MatchedNames { get; set; }
        public string MatchedNameId { get; set; }
    }
}