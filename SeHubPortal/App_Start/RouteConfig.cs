using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SeHubPortal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
          routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
          "RenameCompanyDocumentBlob",
          "Library/{action}/{values}",
          new { controller = "Library", action = "RenameCompanyDocumentBlob", currentFileName = "", newFileName = "" }
           );

            routes.MapRoute(
          "RenameBranchSharedDriveBlob",
          "Library/{action}/{values}",
          new { controller = "Library", action = "RenameBranchSharedDriveBlob", currentFileName = "", newFileName = "" }
           );


            routes.MapRoute(
          "RenameSupplireBlob",
          "Library/{action}/{values}",
          new { controller = "Library", action = "RenameSupplireBlob", currentFileName = "" , newFileName = ""}
           );

            routes.MapRoute(
          "DeleteSupplireBlob",
          "Library/{action}/{values}",
          new { controller = "Library", action = "DeleteSupplireBlob", fileileName = ""}
           );

            routes.MapRoute(
        "CRM",
        "Tools/{action}/{values}",
        new { controller = "Tools", action = "CRM", CustId = "" }
         );


            routes.MapRoute(
          "MyStaffPermissions",
          "Management/{action}/{values}",
          new { controller = "Management", action = "MyStaff", LocId="" }
           );

            routes.MapRoute(
           "EmployeesPermissions",
           "Tools/{action}/{values}",
           new { controller = "Tools", action = "EmployeePermissions", locId = "" }
            );

            routes.MapRoute(
           "FuelInfo",
           "Tools/{action}/{values}",
           new { controller = "Tools", action = "FuelLog", VIN = "" }
            );

            routes.MapRoute(
            "Attendance",
            "Attendance/{locId}/{employeeId}",
            new { controller = "Management", action = "Attendance", locId = UrlParameter.Optional, employeeId = UrlParameter.Optional }
             );

            routes.MapRoute(
            "Payroll",
            "Payroll/{locId}/{employeeId}/{payrollID}",
            new { controller = "Management", action = "Payroll", locId = UrlParameter.Optional, employeeId = UrlParameter.Optional, payrollID = UrlParameter.Optional }
             );

            routes.MapRoute(
            "StaffInfo",
            "Management/{action}/{values}",
            new { controller = "Management", action = "StaffInfo", values = "" }
             );

            routes.MapRoute(
             "EditOrder",
             "TreadTracker/{action}/{values}",
             new { controller = "TreadTracker", action = "EditOrder", orderNumber = "" }
              );
            routes.MapRoute(
             "NewOrder",
             "TreadTracker/{action}/{values}",
             new { controller = "TreadTracker", action = "NewOrder", values = "" }
              );
            routes.MapRoute(
             "TreadTrackerPlace",
             "TreadTracker/{action}/{parameters}",
             new { controller = "TreadTracker", action = "PlaceOrder", parameters = ""}
              );
            routes.MapRoute(
             "TreadTracker",
             "TreadTracker/{action}/{id}/{barcode}",
             new { controller = "TreadTracker", action = "OpenOrder", id = "", barcode="" }
              );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "SignIn", id = UrlParameter.Optional }
            );
        }
    }
}
