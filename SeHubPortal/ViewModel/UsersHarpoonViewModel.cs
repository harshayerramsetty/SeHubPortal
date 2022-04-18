using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.Models;
namespace SeHubPortal.ViewModel
{
    public class UsersHarpoonViewModel
    {
        public List<tbl_harpoon_users> users { get; set; }
        public tbl_harpoon_users newUser { get; set; }
        public tbl_harpoon_users editUser { get; set; }
        public List<SelectListItem> Profiles { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public bool locIDorName { get; set; }
    }
}