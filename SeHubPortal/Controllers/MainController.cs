using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeHubPortal.ViewModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Net.Mail;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;


namespace SeHubPortal.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        [HttpPost]
        public ActionResult DashboardChangeLocation(LocationsMap model)
        {
            return RedirectToAction("LocationsMap", new { loc = model.SelectedLocationId });
        }

        public ActionResult Dashboard(MainDashboard modal)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

            if (CheckPermissions()!=null)
            {
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                int empId = Convert.ToInt32(Session["userID"].ToString());
                var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
                modal.SehubAccess = empDetails;

                if (modal.SehubAccess.main == 0)
                {
                    return RedirectToAction("Dashboard", "Library");
                }
                else if (modal.SehubAccess.mainDashboard == 0) {
                    return RedirectToAction("Calendar", "Main");
                }
            }
            else
            {
                return RedirectToAction("SignIn", "Login");
            }

            //System.Diagnostics.Trace.WriteLine(" this is the permissions for dashboard main *******   " + modal.SehubAccess.mainDashboard + "      **********");

            return View(modal);
        }

        public ActionResult LocationsMap(LocationsMap modal, string loc)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            
            int empId = Convert.ToInt32(Session["userID"].ToString());

            modal.LocationsList = populateLocationsPermissions(empId);

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if(loc != null)
            {
                modal.SelectedLocationId = loc;
            }
            else
            {
                modal.SelectedLocationId = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            modal.locdesc = db.tbl_cta_location_info.Where(x => x.loc_id == modal.SelectedLocationId).FirstOrDefault();
            modal.employees = db.tbl_employee.Where(x => x.loc_ID == modal.SelectedLocationId && x.status == 1).OrderBy(x => x.full_name).ToList();
            //System.Diagnostics.Trace.WriteLine(" this is the permissions for dashboard main *******   " + modal.SehubAccess.mainDashboard + "      **********");

            return View(modal);
        }

        private static List<SelectListItem> populateLocations()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select loc_id From tbl_cta_location_info where loc_status=1";
                //Debug.WriteLine("Query:" + query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["loc_id"].ToString(),
                                Value = sdr["loc_id"].ToString()
                            });
                        }


                    }
                    con.Close();
                }
            }

            return items;
        }

        public JsonResult GetEvents()
        {
            CityTireAndAutoEntities dc = new CityTireAndAutoEntities();

            var events = dc.tbl_Calendar_events.ToList();

            var vacations = dc.tbl_vacation_schedule.ToList();

            foreach (var item in vacations)
            {
                if(dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.status).FirstOrDefault() == 1)
                {
                    tbl_Calendar_events eve = new tbl_Calendar_events();
                    eve.subject = "Vacation";
                    eve.start_date = item.start_date.AddDays(1);
                    eve.end_date = item.end_date.Value.AddDays(1);
                    eve.Description = dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.full_name).FirstOrDefault() + " " + item.leave_type;

                    events.Add(eve);
                }
            }

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public JsonResult GetBirthdayEvents()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var employees = db.tbl_employee.Where(x => x.status == 1 && x.Date_of_birth != null).ToList();

            List<tbl_employee> events = new List<tbl_employee>();

            foreach (var emp in employees)
            {
                tbl_employee eve = new tbl_employee();
                eve.employee_id = emp.employee_id;
                eve.full_name = emp.full_name;
                eve.Date_of_birth = emp.Date_of_birth;

                events.Add(eve);
            }

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetBirthdayEventsMonth()
        {
            //Trace.WriteLine("Reached till here GetBirthdayEventsMonth");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            DateTime start = System.DateTime.Today;
            var employees = db.tbl_employee.Where(x => x.status == 1 && x.Date_of_birth != null && x.Date_of_birth.Value.Month == start.Month).ToList();
            
            return new JsonResult { Data = employees, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetEventsMonth()
        {
            CityTireAndAutoEntities dc = new CityTireAndAutoEntities();

            DateTime start = System.DateTime.Today.AddDays(-7);
            DateTime end = System.DateTime.Today.AddDays(7);

            var events = dc.tbl_Calendar_events.Where(x => x.start_date > start && x.end_date < end).ToList();

            var vacations = dc.tbl_vacation_schedule.Where(x => x.start_date > start && x.end_date < end).ToList();

            foreach (var item in vacations)
            {
                if(dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.status).FirstOrDefault() == 1)
                {
                    tbl_Calendar_events eve = new tbl_Calendar_events();
                    eve.subject = "Vacation";
                    eve.start_date = item.start_date.AddDays(1);
                    eve.end_date = item.end_date.Value.AddDays(1);
                    eve.Description = dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.full_name).FirstOrDefault() + " " + item.leave_type;

                    events.Add(eve);
                }
            }

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public ActionResult Calendar(FileURL model)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            model.LocationsList = populateLocationsPermissions(empId);

            if (model.SehubAccess.mainCalendar == 0)
            {
                return RedirectToAction("Dashboard", "Library");
            }

            return View(model);
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


        public ActionResult Newsletter()
        {
            return View();
        }
        public ActionResult LogOut()
        {
            int userId = (int)Session["userID"];
            Session.Abandon();
            return RedirectToAction("SignIn", "Login");
        }
        [HttpPost]
        public ActionResult Newsletter(HttpPostedFileBase reportName)
        {
           
            if (reportName != null && reportName.ContentLength > 0)
            {
                var imageName = Path.GetFileName(reportName.FileName);
                Debug.WriteLine("reportName:" + imageName);
                string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/"+ imageName;
                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
                using (Image image = Image.FromFile(fileName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        Debug.WriteLine("Image base64:" + base64String);
                        CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                        int empId = Convert.ToInt32(Session["userID"].ToString());
                        //Debug.WriteLine("empId:" + empId);
                        var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
                        if (result != null)
                        {
                            result.profile_pic = imageBytes;                            

                            //Testing
                            Debug.WriteLine(result.employee_id +"    "+ result.full_name);
                          
                            //Debug.WriteLine("**************************");
                        }
                        db.SaveChanges();
                    }
                }


            }
            return View();
        }

        public ActionResult ProfileBlock(tbl_employee employee)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            employee = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            //Trace.WriteLine("Reached till here " + employee.full_name);

            return PartialView(employee) ;
        }

        public ActionResult RenderColor()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var color = db.tbl_sehub_color_scheme;

            return new JsonResult { Data = color, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        protected Image Resize(Image img, int resizedW, int resizedH)
        {
            Bitmap bmp = new Bitmap(resizedW, resizedH);
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.DrawImage(img, 0, 0, resizedW, resizedH);
            graphic.Dispose();
            return (Image)bmp;
        }

        [HttpPost]
        public ActionResult UploadProfileImage(HttpPostedFileBase EmployeeImage)
        {

            int empId = Convert.ToInt32(Session["userID"].ToString());

            byte[] imageBytes = null;
            if (EmployeeImage != null && EmployeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(EmployeeImage.FileName);
                using (Image image = Image.FromStream(EmployeeImage.InputStream, true, true))
                {
                    double height = 170 * image.Height / image.Width;
                    Image img = Resize(image, 170, (int)Math.Round(height));

                    using (MemoryStream m = new MemoryStream())
                    {
                        img.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var EmployeeInfo = db.tbl_employee.Where(a => a.employee_id == empId).FirstOrDefault();
            //var pic = db.tbl_fleettvt_configurations.Where(x => x.Type == "Trailer" && x.Configuration == "4A/8T").FirstOrDefault();

            if (imageBytes != null)
            {
                //pic.configuration_image = imageBytes;
                EmployeeInfo.profile_pic = imageBytes;
            }

            db.SaveChanges();
            return RedirectToAction("Dashboard", "Main");
        }

        public ActionResult StoryBoard(StoryBoardViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            var posts_db = db.tbl_storyBoard_posts.ToList();

            List<PostViewModel> Posts = new List<PostViewModel>();

            String ContainerName = "message-board-attachments";
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
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);

            foreach (var post_db in posts_db)
            {
                var emp = db.tbl_employee.Where(x => x.employee_id == post_db.auther).FirstOrDefault();
                PostViewModel Post = new PostViewModel
                {
                    post_id = post_db.post_id,
                    catgory = post_db.category,
                    subject = post_db.subject,
                    details = post_db.details,
                    auther = emp.full_name,
                    auther_shrtName = emp.first_name,
                    auther_position = emp.cta_position,
                    auther_loc = emp.loc_ID
                };
                if (emp.profile_pic is null)
                {
                    Post.auther_img = "";
                }
                else
                {
                    Post.auther_img = Convert.ToBase64String(emp.profile_pic);
                }
                Post.date = post_db.date.Value;
                Post.loc_id = post_db.loc_id;

                List<KeyValuePair<string, string>> attachments = new List<KeyValuePair<string, string>>();

                foreach (string attachment in post_db.attachment_name.Split(';'))
                {
                    if (attachment != "" && attachment.Contains("-"))
                    {
                        int attachment_id = Convert.ToInt32(attachment.Split('-')[1]);

                        CloudBlockBlob fa_blob = container.GetBlockBlobReference(Post.post_id + '-' + attachment_id);
                        if (fa_blob.Exists())
                        {
                            KeyValuePair<string, string> attach = new KeyValuePair<string, string>(attachment.Split('-')[0], new Uri(fa_blob.Uri.AbsoluteUri).ToString());
                            attachments.Add(attach);
                        }

                    }
                }
                Post.attachments = attachments;
                Posts.Add(Post);
            }

            var add_emp = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            model.AddEmpName = add_emp.full_name;
            model.AddPosition = add_emp.cta_position;
            model.AddLoc = add_emp.loc_ID;
            if (add_emp.profile_pic is null)
            {
                model.AddEmpImage = "";
            }
            else
            {
                model.AddEmpImage = Convert.ToBase64String(add_emp.profile_pic);
            }
            model.Categories = populatePostCategories();
            model.Posts = Posts.OrderByDescending(x => x.date).ToList();
            model.SehubAccess = empDetails;
            model.MatchedLocs = populateLocations();
            return View(model);
        }

        [HttpGet]
        public ActionResult ReplyToPost(StoryBoardViewModel model, string parent_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var reply_emp = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            model.ReplyEmpName = reply_emp.full_name;
            model.ReplyPosition = reply_emp.cta_position;
            model.ReplyLoc = reply_emp.loc_ID;
            if (reply_emp.profile_pic is null)
            {
                model.ReplyEmpImage = "";
            }
            else
            {
                model.ReplyEmpImage = Convert.ToBase64String(reply_emp.profile_pic);
            }

            List<PostViewModel> Posts = new List<PostViewModel>();
            var parent_post_db = db.tbl_storyBoard_posts.Where(x => x.post_id == parent_id).FirstOrDefault();

            PostViewModel parent_post = new PostViewModel();

            var emp_parent = db.tbl_employee.Where(x => x.employee_id == parent_post_db.auther).FirstOrDefault();
            PostViewModel parentPost = new PostViewModel();
            parentPost.post_id = parent_post_db.post_id;
            parentPost.catgory = parent_post_db.category;
            parentPost.subject = parent_post_db.subject;
            parentPost.details = parent_post_db.details;
            parentPost.auther = emp_parent.full_name;
            parentPost.auther_shrtName = emp_parent.first_name;
            parentPost.auther_position = emp_parent.cta_position;
            parentPost.auther_loc = emp_parent.loc_ID;
            if (emp_parent.profile_pic is null)
            {
                parentPost.auther_img = "";
            }
            else
            {
                parentPost.auther_img = Convert.ToBase64String(emp_parent.profile_pic);
            }
            parentPost.date = parent_post_db.date.Value;
            parentPost.loc_id = parent_post_db.loc_id;
            Posts.Add(parentPost);

            var posts_db = db.tbl_storyBoard_posts.Where(x => x.post_id.StartsWith(parent_id + "_")).ToList();

            foreach (var post_db in posts_db)
            {
                var emp = db.tbl_employee.Where(x => x.employee_id == post_db.auther).FirstOrDefault();
                PostViewModel Post = new PostViewModel();
                Post.post_id = post_db.post_id;
                Post.catgory = post_db.category;
                Post.subject = post_db.subject;
                Post.details = post_db.details;
                Post.auther = emp.full_name;
                Post.auther_shrtName = emp.first_name;
                Post.auther_position = emp.cta_position;
                Post.auther_loc = emp.loc_ID;
                if (emp.profile_pic is null)
                {
                    Post.auther_img = "";
                }
                else
                {
                    Post.auther_img = Convert.ToBase64String(emp.profile_pic);
                }
                Post.date = post_db.date.Value;
                Post.loc_id = post_db.loc_id;
                Posts.Add(Post);
            }

            model.Posts = Posts;
            model.ReplytoID = parent_id;
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult ReplyToPost(StoryBoardViewModel model, IEnumerable<HttpPostedFileBase> addReplyAttachment)
        {
            Trace.WriteLine(model.ReplytoID + "This is the reply ID");

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            
            int id_count = db.tbl_storyBoard_posts.Where(x => x.post_id.StartsWith(model.ReplytoID + "_")).Count()+1;

            tbl_storyBoard_posts newReply = new tbl_storyBoard_posts();

            newReply.post_id = model.ReplytoID + "_" + id_count;
            newReply.details = model.ReplyDetails;
            newReply.auther = empId;
            newReply.date = DateTime.Now;
            newReply.loc_id = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            newReply.attachment_name = "";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("message-board-attachments");

            int i = 0;

            foreach (var file in addReplyAttachment)
            {
                if (file != null && file.ContentLength > 0)
                {
                    CloudBlockBlob Blob = container.GetBlockBlobReference(newReply.post_id + "-" + i);

                    Blob.Properties.ContentType = file.ContentType;

                    if (file.FileName.Substring(file.FileName.Length - 4) == ".pdf" || file.FileName.Substring(file.FileName.Length - 4) == ".png" || file.FileName.Substring(file.FileName.Length - 4) == ".jpg")
                    {
                        Blob.UploadFromStream(file.InputStream);
                        newReply.attachment_name = file.FileName + "-" + i + ";" + newReply.attachment_name;
                        i++;
                    }
                }
            }

            db.tbl_storyBoard_posts.Add(newReply);            
            db.SaveChanges();

            var replyEmps = db.tbl_storyBoard_posts.Where(x => x.post_id.StartsWith(model.ReplytoID + "_") || x.post_id == model.ReplytoID).Select(x => x.auther).Distinct().ToList();

            foreach (var replyEmp in replyEmps)
            {
                if (empId != replyEmp)
                {
                    string emp_email = db.tbl_employee.Where(x => x.employee_id == replyEmp).Select(x => x.cta_email).FirstOrDefault();

                    MailMessage msg_rep = new MailMessage();
                    msg_rep.To.Add(emp_email);
                    msg_rep.From = new MailAddress("no_reply@citytire.com", "Sehub");
                    msg_rep.Subject = "Reply to the post that you conributed";
                    msg_rep.Body = "Hi <br /> There is a reply to the post that you have contributed. <br /> Thanks.";
                    msg_rep.IsBodyHtml = true;

                    SmtpClient client_rep = new SmtpClient();
                    client_rep.UseDefaultCredentials = false;
                    client_rep.Credentials = new System.Net.NetworkCredential("no_reply@citytire.com", "U@dx/Z8Ry{");
                    client_rep.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
                    client_rep.Host = "smtp.office365.com";
                    client_rep.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client_rep.EnableSsl = true;
                    try
                    {
                        client_rep.Send(msg_rep);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
                
            }

            return RedirectToAction("StoryBoard", "Main");
        }

        [HttpPost]
        public ActionResult AddPost(StoryBoardViewModel model, IEnumerable<HttpPostedFileBase> addAttachment)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            
            int count = db.tbl_storyBoard_posts.Where(x => !x.post_id.Contains("_")).Count();

            string id = (1000 + count).ToString();

            tbl_storyBoard_posts newPost = new tbl_storyBoard_posts();

            newPost.post_id = id;
            newPost.category = model.AddCategory;
            newPost.subject = model.AddSubject;
            newPost.details = model.AddDetails;
            newPost.auther = empId;
            newPost.date = DateTime.Now;
            newPost.loc_id = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            newPost.attachment_name = "";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("message-board-attachments");

            int i = 0;

            foreach (var file in addAttachment)
            {
                if (file != null && file.ContentLength > 0)
                {                   
                    CloudBlockBlob Blob = container.GetBlockBlobReference(id+"-"+i);

                    Blob.Properties.ContentType = file.ContentType;

                    if (file.FileName.Substring(file.FileName.Length - 4) == ".pdf" || file.FileName.Substring(file.FileName.Length - 4) == ".png" || file.FileName.Substring(file.FileName.Length - 4) == ".jpg")
                    {
                        Blob.UploadFromStream(file.InputStream);
                        newPost.attachment_name = file.FileName + "-" + i + ";" + newPost.attachment_name;
                        i++;
                    }
                }
            }

            
            db.tbl_storyBoard_posts.Add(newPost);    
            db.SaveChanges();
                       
            MailMessage msg_rep = new MailMessage();
            msg_rep.To.Add("allstaff@citytire.com"); //harsha.yerramsetty@citytire.com
            msg_rep.From = new MailAddress("no_reply@citytire.com", "Sehub");
            msg_rep.Subject = "There is a new post in story board";
            msg_rep.Body = "Hi, <br /> New post in story board.<br /> Please check it out in the link: https://sehubportal.azurewebsites.net/Main/StoryBoard <br /> Thanks.";
            msg_rep.IsBodyHtml = true;

            SmtpClient client_rep = new SmtpClient();
            client_rep.UseDefaultCredentials = false;
            client_rep.Credentials = new System.Net.NetworkCredential("no_reply@citytire.com", "U@dx/Z8Ry{");
            client_rep.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client_rep.Host = "smtp.office365.com";
            client_rep.DeliveryMethod = SmtpDeliveryMethod.Network;
            client_rep.EnableSsl = true;
            try
            {
                client_rep.Send(msg_rep);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return RedirectToAction("StoryBoard", "Main");
        }

        private static List<SelectListItem> populatePostCategories()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var emp = db.tbl_source_storyBoard_categories.Select(x => x.category).ToList();

            foreach (var val in emp)
            {

                items.Add(new SelectListItem
                {
                    Text = val,
                    Value = val
                });
            }
            return items;
        }

        public void TestArduinoNano(float dbs)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_tools_decibel_meter record = new tbl_tools_decibel_meter();
            record.timestamp = DateTime.Now;
            record.gps_latitude = 1;
            record.gps_longitude = 2;
            record.reading_A = dbs;
            db.tbl_tools_decibel_meter.Add(record);
            db.SaveChanges();
        }

    }
}