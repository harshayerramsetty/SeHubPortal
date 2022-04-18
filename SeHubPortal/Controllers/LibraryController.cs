using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Configuration;
using SeHubPortal.ViewModel;
using System.IO;
using SeHubPortal.Models;
using System.Diagnostics;
using Aspose.Pdf.Facades;
using System.Net.Mail;
using System.Data.SqlClient;


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
        public ActionResult Dashboard(LibraryAddDocumentRequest model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.library_access == 0)
            {
                return RedirectToAction("Dashboard", "Tools");
            }


            model.Permission = Convert.ToInt32(empDetails.library_access);

            if (model.SehubAccess.library_dashboard == 0) 
            {
                return RedirectToAction("Branch_Shared_drive", "Library");
            }
            

            return View(model);
        }

        public PartialViewResult DeleteItem() 
        {

            return PartialView();

        }

        [HttpPost]
        public ActionResult MergePDFStreamArray(HttpPostedFileBase file1, HttpPostedFileBase file2, HttpPostedFileBase file3, HttpPostedFileBase file4, HttpPostedFileBase file5)
        {
            string path3 = Path.Combine(Server.MapPath("~/Content/merge.pdf"));

            //System.IO.File.Delete(path3);
            // create PdfFileEditor object
            PdfFileEditor pdfEditor = new PdfFileEditor();
            // output stream
            FileStream outputStream = new FileStream(path3, FileMode.Create);

            Stream[] streamArray = new Stream[5];

            if (file1 != null) {
                streamArray[0] = file1.InputStream;
            }
            if (file2 != null)
            {
                streamArray[1] = file2.InputStream;
            }
            if (file3 != null)
            {
                streamArray[2] = file3.InputStream;
            }
            if (file4 != null)
            {
                streamArray[3] = file4.InputStream;
            }
            if (file5 != null)
            {
                streamArray[4] = file5.InputStream;
            }

            pdfEditor.Concatenate(streamArray, outputStream);
            return RedirectToAction("Dashboard", "Settings", new { ac = "MergeSuccessful" });
        }

        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                string filePath = file.FileName + Path.GetExtension(file.FileName);
                file.SaveAs(Path.Combine(Server.MapPath("~/Images"), filePath));
                //Here you can write code for save this information in your database if you want
            }

            return Json("file uploaded successfully");
        }

        [HttpPost]
        public ActionResult AddDocumentRequest(HttpPostedFileBase Document, LibraryAddDocumentRequest modal)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var locationDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            var DocumentName = Path.GetFileName(Document.FileName);

            Debug.WriteLine(locationDetails.loc_ID);

            MailMessage msg = new MailMessage();

            if (modal.ResourceType == "Company Documents" | modal.ResourceType == "Supplier Documents" | modal.ResourceType == "Management" | modal.ResourceType == "Customer Documents") {
                msg.To.Add(new MailAddress("IT@citytire.com", "IT Team")); //jordan.blackwood
            }
            else if (modal.ResourceType == "Branch Shared Drive")
            {
                msg.To.Add(new MailAddress("IT@citytire.com", "IT Team")); //cta["+locationDetails.loc_ID+"]@citytire.com", "IT Team
            }
            
            msg.From = new MailAddress("sehub@citytire.com", "Sehub");
            msg.Subject = "SEHUB Add document request";
            msg.Body = "Resource Type :" + modal.ResourceType + "<br />Document Name :" + modal.DocumentName + "<br />Description :" + modal.Description;
            msg.IsBodyHtml = true;

            msg.Attachments.Add(new Attachment(Document.InputStream, DocumentName, "text/pdf"));

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sehub@citytire.com", "$3hub1977");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public ActionResult UploadCompanyDocuments(HttpPostedFileBase CompanyDocument)
        {
            var imageName = Path.GetFileName(CompanyDocument.FileName);

            Debug.WriteLine(imageName);

            if (CompanyDocument != null && CompanyDocument.ContentLength > 0)
            {

                Debug.WriteLine("reportName:" + imageName);

                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
            }
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCompanyDocuments);



            var blockBlob = container.ListBlobs();

            FileURL FileUrl = new FileURL();

            var Names = new List<string>();

            foreach (var blob in blockBlob)
            {                
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                Names.Add(blobFileName);
            }
                       

            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = CompanyDocument.ContentType;

            if (imageName.Substring(imageName.Length - 4) == ".pdf") {
                Blob.UploadFromStream(CompanyDocument.InputStream);
            }            

            if (Names.Contains(imageName) == true)
            {
                return RedirectToAction("Company_Documents", new { ac = "FileAlreadyExists" });
            }
            else if(imageName.Substring(imageName.Length-4) != ".pdf")
            {
                return RedirectToAction("Company_Documents", new { ac = "WrongFormat" });
            }
            else 
            {
                return RedirectToAction("Company_Documents", new { ac = "success" });
            }

            
        }

        [HttpPost]
        public ActionResult UploadManagement(HttpPostedFileBase CompanyDocument)
        {
            var imageName = Path.GetFileName(CompanyDocument.FileName);

            Debug.WriteLine(imageName);

            if (CompanyDocument != null && CompanyDocument.ContentLength > 0)
            {

                Debug.WriteLine("reportName:" + imageName);

                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
            }
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameManagement);



            var blockBlob = container.ListBlobs();

            FileURL FileUrl = new FileURL();

            var Names = new List<string>();

            foreach (var blob in blockBlob)
            {
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                Names.Add(blobFileName);
            }


            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = CompanyDocument.ContentType;

            if (imageName.Substring(imageName.Length - 4) == ".pdf")
            {
                Blob.UploadFromStream(CompanyDocument.InputStream);
            }

            if (Names.Contains(imageName) == true)
            {
                return RedirectToAction("Management", new { ac = "FileAlreadyExists" });
            }
            else if (imageName.Substring(imageName.Length - 4) != ".pdf")
            {
                return RedirectToAction("Management", new { ac = "WrongFormat" });
            }
            else
            {
                return RedirectToAction("Management", new { ac = "success" });
            }


        }

        [HttpPost]
        public ActionResult UploadBranchShareDrive(HttpPostedFileBase BranchShareDrive, FileURL model)
        {

            string imageName = model.Location_ID + "_" + model.Pane + "_" + Path.GetFileName(BranchShareDrive.FileName);

            if (BranchShareDrive != null && BranchShareDrive.ContentLength > 0)
            {

                Debug.WriteLine("reportName:" + imageName);

                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
            }
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameBranchSharedDrive);

            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = BranchShareDrive.ContentType;

            Blob.UploadFromStream(BranchShareDrive.InputStream);

            return RedirectToAction("Branch_Shared_drive");
        }

        [HttpPost]
        public ActionResult UploadSupplierDocuments(HttpPostedFileBase SupplierDocuments)
        {
            var imageName =  Path.GetFileName(SupplierDocuments.FileName);

            if (SupplierDocuments != null && SupplierDocuments.ContentLength > 0)
            {

                Debug.WriteLine("reportName:" + imageName);

                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
            }
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameSupplierDocuments);

            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = SupplierDocuments.ContentType;

            Blob.UploadFromStream(SupplierDocuments.InputStream);

            return RedirectToAction("Supplier_Documents");
        }

        [HttpPost]
        public ActionResult UploadCustomerDocuments(HttpPostedFileBase CustomerDocuments)
        {
            var imageName = Path.GetFileName(CustomerDocuments.FileName);

            if (CustomerDocuments != null && CustomerDocuments.ContentLength > 0)
            {

                Debug.WriteLine("reportName:" + imageName);

                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
            }
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCustomerDocuments);

            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = CustomerDocuments.ContentType;

            Blob.UploadFromStream(CustomerDocuments.InputStream);

            return RedirectToAction("Customer_Documents");
        }

        public String ContainerNameCompanyDocuments = "cta-library-company-documents";
        [HttpGet]
        public ActionResult Company_Documents()
        {
            FileURL FileUrl = new FileURL();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            FileUrl.SehubAccess = empDetails;
            if (FileUrl.SehubAccess.library_company_Documents == 0)
            {
                return RedirectToAction("Customer_Documents", "Library");
            }

            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCompanyDocuments);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();

            //var blobList = blockBlob.ToList();            

            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");
                URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
            }

            FileUrl.Permission = Convert.ToInt32(empDetails.library_access);
            FileUrl.URLName = URLNames;
            return View(FileUrl);
        }

        private static List<SelectListItem> populateLocationsPermissions(int empId)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_cta_location_info.ToList();

            var sehubloc = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

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


            return items;
        }

        [HttpPost]
        public ActionResult Branch_Shared_drive_ChangeLocation(FileURL model)
        {
            return RedirectToAction("Branch_Shared_drive", new { loc = model.Location_ID });
        }

        public String ContainerNameBranchSharedDrive = "cta-library-branch-shared-drive";
        [HttpGet]
        public ActionResult Branch_Shared_drive(string loc)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            FileURL FileUrl = new FileURL();
            var empDetails1 = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            FileUrl.Permission = Convert.ToInt32(empDetails1.library_access);
            FileUrl.SehubAccess = empDetails1;

            if (FileUrl.SehubAccess.library_branch_shared_drive == 0)
            {
                return RedirectToAction("Company_Documents", "Library");
            }

            
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            
            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            string locatId = "";

            if (loc != null)
            {
                locatId = loc;
            }
            else
            {
                locatId = empDetails.loc_ID;
            }

            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameBranchSharedDrive);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();



            var blobList = blockBlob.ToList();            

            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");
                URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
            }


            FileUrl.URLName = URLNames;
            FileUrl.Location_ID = locatId;            
            FileUrl.LocationsList = populateLocationsPermissions(empId);            

            return View(FileUrl);
        }

        public String ContainerNameSupplierDocuments = "cta-library-supplier-documents";
        [HttpGet]
        public ActionResult Supplier_Documents()
        {
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameSupplierDocuments);

            // Retrieve reference to a blob ie "picture.jpg".
            
            var blockBlob = container.ListBlobs();

            var blobList = blockBlob.ToList();

            FileURL FileUrl = new FileURL();

            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");
                URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            FileUrl.Permission = Convert.ToInt32(empDetails.library_access);

            FileUrl.SehubAccess = empDetails;

            FileUrl.URLName = URLNames;

            if (FileUrl.SehubAccess.library_supplier_documents == 0)
            {
                return RedirectToAction("Dashboard", "Tools");
            }

            return View(FileUrl);
        }

        public String ContainerNameCustomerDocuments = "cta-library-customer-documents";
        [HttpGet]
        public ActionResult Customer_Documents()
        {
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCustomerDocuments);

            // Retrieve reference to a blob ie "picture.jpg".

            var blockBlob = container.ListBlobs();

            var blobList = blockBlob.ToList();

            FileURL FileUrl = new FileURL();

            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");
                URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            FileUrl.Permission = Convert.ToInt32(empDetails.library_access);

            FileUrl.SehubAccess = empDetails;

            FileUrl.URLName = URLNames;

            if (FileUrl.SehubAccess.library_supplier_documents == 0)
            {
                return RedirectToAction("Management", "Library");
            }

            return View(FileUrl);
        }

        public String ContainerNameManagement = "cta-library-management";
        [HttpGet]
        public ActionResult Management()
        {
            FileURL FileUrl = new FileURL();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            FileUrl.Permission = Convert.ToInt32(empDetails.library_access);
            FileUrl.SehubAccess = empDetails;

            if (FileUrl.SehubAccess.library_Management == 0)
            {
                return RedirectToAction("Supplier_Documents", "Library");
            }

            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameManagement);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();

            //var blobList = blockBlob.ToList();

            
            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");
                URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
            }

            

            FileUrl.URLName = URLNames;
            return View(FileUrl);
        }

        public ActionResult DeleteBranchSharedDriveBlob(string fileName)
        {
            fileName = fileName.Remove(fileName.Length - 1) + ".pdf";
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameBranchSharedDrive);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("Branch_Shared_drive");
        }

        [HttpPost]
        public ActionResult RenameBranchSharedDriveBlob(string currentFileName, string newFileName, FileURL model)
        {
            currentFileName = currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameBranchSharedDrive);

            var blob = container.GetBlockBlobReference(currentFileName);
            string correct_format = model.RenameString + ".pdf"; //model.Location_ID + "_" + 
            var blob1 = container.GetBlockBlobReference(correct_format);

            blob1.StartCopy(blob);

            blob.Delete();

            return RedirectToAction("Branch_Shared_drive");
        }
        
        public ActionResult DeleteSupplireBlob(string fileName)
        {            
            fileName = fileName.Remove(fileName.Length - 1) + ".pdf";
            Debug.WriteLine(fileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameSupplierDocuments);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("Supplier_Documents");
        }
        [HttpPost]
        public ActionResult RenameSupplireBlob(string currentFileName, string newFileName, FileURL model)
        {
            currentFileName = currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameSupplierDocuments);

            var blob = container.GetBlockBlobReference(currentFileName);
            var blob1 = container.GetBlockBlobReference(model.RenameString+".pdf");

            blob1.StartCopy(blob);

            blob.DeleteIfExists();

            return RedirectToAction("Supplier_Documents");
        }

        public ActionResult DeleteCustomerBlob(string fileName)
        {
            fileName = fileName.Remove(fileName.Length - 1) + ".pdf";
            Debug.WriteLine(fileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCustomerDocuments);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("Customer_Documents");
        }
        [HttpPost]
        public ActionResult RenameCustomerBlob(string currentFileName, string newFileName, FileURL model)
        {
            currentFileName = currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCustomerDocuments);

            var blob = container.GetBlockBlobReference(currentFileName);
            var blob1 = container.GetBlockBlobReference(model.RenameString + ".pdf");

            blob1.StartCopy(blob);

            blob.DeleteIfExists();

            return RedirectToAction("Customer_Documents");
        }

        public ActionResult DeleteCompanyDocumentsBlob(string fileName)
        {
            fileName = fileName.Remove(fileName.Length - 1) + ".pdf";
            Debug.WriteLine(fileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCompanyDocuments);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("Company_Documents");
        }
        [HttpPost]
        public ActionResult RenameCompanyDocumentBlob(string currentFileName, string newFileName, FileURL model)
        {
            currentFileName = currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameCompanyDocuments);

            var blob = container.GetBlockBlobReference(currentFileName);
            var blob1 = container.GetBlockBlobReference(model.RenameString + ".pdf");

            blob1.StartCopy(blob);

            blob.DeleteIfExists();

            return RedirectToAction("Company_Documents");
        }

        public ActionResult DeleteManagement(string fileName)
        {
            fileName = fileName.Remove(fileName.Length - 1) + ".pdf";
            Debug.WriteLine(fileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameManagement);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("Management");
        }
        [HttpPost]
        public ActionResult RenameManagement(string currentFileName, string newFileName, FileURL model)
        {
            currentFileName = currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameManagement);

            var blob = container.GetBlockBlobReference(currentFileName);
            var blob1 = container.GetBlockBlobReference(model.RenameString + ".pdf");

            blob1.StartCopy(blob);

            blob.DeleteIfExists();

            return RedirectToAction("Management");
        }

    }
}



