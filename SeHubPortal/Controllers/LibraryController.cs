using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class LibraryController : Controller
    {
        // GET: Library
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Documents()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Policies()
        {
            return View();
        }
    }
}