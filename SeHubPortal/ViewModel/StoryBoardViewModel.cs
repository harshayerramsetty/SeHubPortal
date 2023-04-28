using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;
using System.Web.Mvc;
using SeHubPortal.ViewModel;


namespace SeHubPortal.ViewModel
{
    public class StoryBoardViewModel
    {
        public tbl_sehub_access SehubAccess { get; set; }
        public List<SelectListItem> MatchedLocs { get; set; }
        public string MatchedLocID { get; set; }
        public List<PostViewModel> Posts { get; set; }
        public List<SelectListItem> Categories { get; set; }

        public string ReplyDetails { get; set; }
        public string ReplytoID { get; set; }
        public string ReplyEmpName { get; set; }
        public string ReplyPosition { get; set; }
        public string ReplyLoc { get; set; }
        public string ReplyEmpImage { get; set; }

        public string AddDetails { get; set; }
        public string AddSubject { get; set; }
        public string AddID { get; set; }
        public string AddEmpName { get; set; }
        public string AddPosition { get; set; }
        public string AddLoc { get; set; }
        public string AddEmpImage { get; set; }
        public string AddCategory { get; set; }
    }
}