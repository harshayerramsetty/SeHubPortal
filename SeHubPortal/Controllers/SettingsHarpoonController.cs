using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Net.Mail;

namespace SeHubPortal.Controllers
{
    public class SettingsHarpoonController : Controller
    {
        // GET: SettingsHarpoon
        public ActionResult Index()
        {
            return View();
        }

        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        [HttpGet]
        public ActionResult Employees(string LocId)
        {
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            int permissions = CheckPermissions().my_staff.Value;

            string locationid = "";
            if (LocId == "" || LocId is null)
            {
                //Debug.WriteLine("empId:" + empId);
                var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();

                if (result != null)
                {
                    locationid = result.loc_ID;
                }
                else
                {
                    locationid = "";
                }

            }
            else
            {
                locationid = LocId;
            }


            var EmployeeDetails = db.tbl_employee.Where(x => x.loc_ID == locationid).OrderBy(x => x.full_name).ToList();



            List<tbl_employee_status> empdetList = new List<tbl_employee_status>();

            foreach (var emp in EmployeeDetails)
            {
                tbl_employee_status empdet = new tbl_employee_status();
                empdet = db.tbl_employee_status.Where(x => x.employee_id == emp.employee_id).FirstOrDefault();
                if (empdet != null)
                {
                    empdetList.Add(empdet);
                }
            }


            //Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {
                modal.positionsTable = db.tbl_position_info.ToList();
                modal.employeeDetails = EmployeeDetails;
                modal.employeeStatusDetails = empdetList;
                modal.MatchedStaffLocs = populateLocationsPermissions(empId);
                modal.MatchedStaffLocID = locationid;
                modal.EmployeePermissions = permissions;


                modal.Positions = populatePositions();

                return View(modal);
            }
            else
            {
                return View(modal);
            }


        }

        [HttpGet]
        public ActionResult Locations(string LocId)
        {
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            int permissions = CheckPermissions().my_staff.Value;

            string locationid = "";
            if (LocId == "" || LocId is null)
            {
                //Debug.WriteLine("empId:" + empId);
                var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();

                if (result != null)
                {
                    locationid = result.loc_ID;
                }
                else
                {
                    locationid = "";
                }

            }
            else
            {
                locationid = LocId;
            }


            var EmployeeDetails = db.tbl_employee.Where(x => x.loc_ID == locationid).OrderBy(x => x.full_name).ToList();



            List<tbl_employee_status> empdetList = new List<tbl_employee_status>();

            foreach (var emp in EmployeeDetails)
            {
                tbl_employee_status empdet = new tbl_employee_status();
                empdet = db.tbl_employee_status.Where(x => x.employee_id == emp.employee_id).FirstOrDefault();
                if (empdet != null)
                {
                    empdetList.Add(empdet);
                }
            }


            //Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {
                modal.positionsTable = db.tbl_position_info.ToList();
                modal.employeeDetails = EmployeeDetails;
                modal.employeeStatusDetails = empdetList;
                modal.MatchedStaffLocs = populateLocationsPermissions(empId);
                modal.MatchedStaffLocID = locationid;
                modal.EmployeePermissions = permissions;


                modal.Positions = populatePositions();

                return View(modal);
            }
            else
            {
                return View(modal);
            }


        }

        [HttpGet]
        public ActionResult Devices(string LocId)
        {
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;

            return View(modal);

        }

        [HttpGet]
        public ActionResult System(string LocId)
        {
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;

            return View(modal);

        }

        [HttpGet]
        public ActionResult Users(string LocId)
        {
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = 10901;

            return View(modal);

        }



        [HttpPost]
        public ActionResult Employees(MyStaffViewModel modal)
        {
            return RedirectToAction("Employees", new { LocId = modal.MatchedStaffLocID });
        }

        private static List<SelectListItem> populatePositions()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_position_info.ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.PositionTitle,
                    Value = Convert.ToString(val.PositionTitle)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateLocationsPermissions(int empId)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_cta_location_info.ToList();

            var sehubloc = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            if (sehubloc != null)
            {
                if (sehubloc.loc_001 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "001",
                        Value = "001"
                    });
                }
                if (sehubloc.loc_002 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "002",
                        Value = "002"
                    });
                }
                if (sehubloc.loc_003 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "003",
                        Value = "003"
                    });
                }
                if (sehubloc.loc_004 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "004",
                        Value = "004"
                    });
                }
                if (sehubloc.loc_005 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "005",
                        Value = "005"
                    });
                }
                if (sehubloc.loc_007 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "007",
                        Value = "007"
                    });
                }
                if (sehubloc.loc_009 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "009",
                        Value = "009"
                    });
                }
                if (sehubloc.loc_010 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "010",
                        Value = "010"
                    });
                }
                if (sehubloc.loc_011 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "011",
                        Value = "011"
                    });
                }
                if (sehubloc.loc_347 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "347",
                        Value = "347"
                    });
                }
                if (sehubloc.loc_AHO == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "AHO",
                        Value = "AHO"
                    });
                }
            }
            else
            {
                items.Add(new SelectListItem
                {
                    Text = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault(),
                    Value = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault()
                });
            }




            return items;
        }

    }
}