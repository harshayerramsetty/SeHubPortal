using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
namespace SeHubPortal.ViewModel
{
    public class HarpoonProfileBlockViewModel
    {
        public tbl_harpoon_users user { get; set; }
        public tbl_harpoon_clients client { get; set; }
    }
}