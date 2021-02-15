using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class ToolsSampleController : Controller
    {
        // GET: ToolsSample
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }
    }
}