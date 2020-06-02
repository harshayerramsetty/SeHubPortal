using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.Controllers
{
    public class TreadTrackerController : Controller
    {
        // GET: TreadTracker
        public ActionResult Index()
        {
            return View();
        }
        private static List<SelectListItem> PopulateCustomers(string value)
        {
            Debug.WriteLine("Inside PopulateCustomers with value:" + value);
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select cust_name,cust_us1 FRom tbl_customer_list  where cscttc ='True' and cust_us2='0'";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (value == "Nothing")
                        {
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["cust_name"].ToString(),
                                    Value = sdr["cust_us1"].ToString(),
                                    //Selected = sdr["cust_name"].ToString() == "CITY TIRE AND AUTO CENTRE LTD" ? true : false
                                });
                            }
                        }
                        else
                        {
                            Debug.WriteLine("In else");
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["cust_name"].ToString(),
                                    Value = sdr["cust_us1"].ToString(),
                                    Selected = sdr["cust_name"].ToString() == value ? true : false
                                });
                            }
                        }

                    }
                    con.Close();
                }
            }

            return items;
        }

        public ActionResult NewOrder(string values)
        {
            Debug.WriteLine("values:" + values);
            if (values == "")
            {
                CustomerListViewModel custList = new CustomerListViewModel();
                custList.Customers = PopulateCustomers("Nothing");
                return View(custList);
            }
            else
            {
                CustomerListViewModel custList = new CustomerListViewModel();
                custList.Customers = PopulateCustomers(values);


                //custList.WorkOrderNumber = "000007";
                Debug.WriteLine("custList.CustId :" + custList.CustId);

                return View(custList);


            }

        }
        [HttpPost]
        public ActionResult SearchByName(CustomerListViewModel custList)
        {
            if (custList.searchWithName == null)
            {
                return RedirectToAction("NewOrder", new { values = "" });
            }
            string NameLike = custList.searchWithName.ToString().ToUpper();
            string custName = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select TOP 1 cust_name from tbl_customer_list where cscttc='True' and upper(cust_name) like '%" + NameLike + "%' ";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            custName = sdr["cust_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }

            return RedirectToAction("NewOrder", new { values = custName });
        }
        [HttpPost]
        public ActionResult Generate(CustomerListViewModel custList)
        {

            custList.Customers = PopulateCustomers("Nothing");
            if (custList.CustId == null || custList.WorkOrderNumber == null)
            {
                return RedirectToAction("NewOrder", "TreadTracker");
            }


            else
            {
                var selectedItem = custList.Customers.Find(p => p.Value == custList.CustId.ToString());
                if (selectedItem != null)
                {
                    Debug.WriteLine(selectedItem.Text);

                }
                string workOrderNumber = custList.WorkOrderNumber.ToString();
                string Value = "";
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select retread_workorder FRom tbl_treadtracker_workorder where retread_workorder=" + workOrderNumber;
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                Value = sdr["retread_workorder"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
                string custNum = "";
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select cust_us1 FRom tbl_customer_list where cust_name=" + "'" + selectedItem.Text.ToString() + "'";
                    Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                custNum = sdr["cust_us1"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
                Debug.WriteLine("Value:" + Value);
                if (Value.Length > 0)
                {
                    return RedirectToAction("NewOrder", "TreadTracker");
                }
                else
                {
                    return RedirectToAction("PlaceOrder", new { parameters = workOrderNumber + ";" + custNum });
                }
            }

            //return PartialView(NewOrder);

        }

        public ActionResult submitNewOrder()
        {
            String value = Request["casing" + "1"];
            Debug.WriteLine("Yes" + Request["line1"]);
            Debug.WriteLine(value);


            System.Collections.ArrayList NewOrderList = new System.Collections.ArrayList();

            NewOrderList.Clear();

            for (int i = 1; i <= 11; i++)
            {
                if (Request["barcode" + i.ToString()].Length >= 7)
                {
                    Debug.WriteLine(Request["barcode" + i.ToString()]);

                    try
                    {
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
                        {
                            DataSource = "sehub.database.windows.net",
                            UserID = "sehubadmin",
                            Password = "C1tyT1r3$",
                            InitialCatalog = "CityTireAndAuto"
                        };

                        using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("insert into tbl_treadtracker_barcode values(@barcode,@retread_workorder,@line_number,@line_code,@serial_dot,@casing_size,@casing_brand,@retread_design,@unit_ID,@preliminary_inspection_result,@preliminary_inspection_date,null,null,null,null,null,null,@ship_to_location)");

                            string sql = sb.ToString();

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@barcode", Request["barcode" + i.ToString()]);
                                command.Parameters.AddWithValue("@retread_workorder", "000000");
                                command.Parameters.AddWithValue("@line_number", Request["line" + i.ToString()]);
                                command.Parameters.AddWithValue("@line_code", "1");
                                command.Parameters.AddWithValue("@serial_dot", Request["serial" + i.ToString()]);
                                command.Parameters.AddWithValue("@casing_size", Request["casing" + i.ToString()]);
                                command.Parameters.AddWithValue("@casing_brand", Request["brand" + i.ToString()]);
                                command.Parameters.AddWithValue("@retread_design", Request["tread1" + i.ToString()]);
                                command.Parameters.AddWithValue("@unit_ID", Request["unit" + i.ToString()]);
                                command.Parameters.AddWithValue("@preliminary_inspection_result", "Good");
                                command.Parameters.AddWithValue("@preliminary_inspection_date", DateTime.Now);
                                command.Parameters.AddWithValue("@ship_to_location", "101");
                                command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }
                            connection.Close();
                        }
                    }
                    catch (SqlException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }
            }
            return RedirectToAction("PlaceOrder", "TreadTracker");
        }
        public ActionResult PlaceOrder(string parameters)
        {

            Debug.WriteLine("parameters:" + parameters);
            string[] param = parameters.Split(';');
            string custNo = param[1];
            string workOrd = param[0];
            Debug.WriteLine("custNo:" + custNo + "  " + workOrd);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var NewWorkOrderModel = new NewWorkOrder
            {

                customerInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNo).FirstOrDefault(),
                workOrder = workOrd,
                OrderDate = DateTime.Now.ToString("yyyy-MM-dd"),
                SubmittedEmployee = Session["userID"].ToString()
            };
            return View(NewWorkOrderModel);
        }
        //private OpenOrderModel openOrder = new OpenOrderModel();

        public ActionResult BackPlaceOrder(string parameters)
        {

            return RedirectToAction("NewOrder", "TreadTracker", new { values = "" });
        }

        public ActionResult OpenOrder(string id, string barcode)
        {
            //id = "";
            //barcode = "28452118";
            Debug.WriteLine("OpenOrder: id:" + id + "  barcode:" + barcode);
            if (id != "")
            {
                Debug.WriteLine("In Id");
                string custNumber = "0";
                dynamic mymodel = new ExpandoObject();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == id).FirstOrDefault();
                if (WorkOrderDetails != null)
                {
                    custNumber = WorkOrderDetails.customer_number;
                }
                else
                {
                    custNumber = "0";
                }
                Debug.WriteLine("custNumber:" + custNumber);
                var ViewOrderModel = new ViewWorkOrderModel
                {
                    barcodeInformation = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == id).ToList(),
                    custInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault(),
                    workOrderInfo = WorkOrderDetails,
                    WorkOrderNumber = id,
                    Barcode = barcode
                };
                return View(ViewOrderModel);
            }
            else if (barcode != "")
            {
                Debug.WriteLine("In Barcode");
                Debug.WriteLine("Barcode:" + barcode);
                string workOrderNumber = "0";
                string custNumber = "0";
                dynamic mymodel = new ExpandoObject();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                var getWorkOrder = db.tbl_treadtracker_barcode.Where(x => x.barcode == barcode).FirstOrDefault();
                if (getWorkOrder != null)
                {
                    workOrderNumber = getWorkOrder.retread_workorder;

                }
                else
                {
                    workOrderNumber = "0";
                }
                var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == workOrderNumber).FirstOrDefault();
                if (WorkOrderDetails != null)
                {
                    custNumber = WorkOrderDetails.customer_number;
                }
                else
                {
                    custNumber = "0";
                }
                var ViewOrderModel = new ViewWorkOrderModel
                {
                    barcodeInformation = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == workOrderNumber).ToList(),
                    custInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault(),
                    workOrderInfo = WorkOrderDetails,
                    WorkOrderNumber = id,
                    Barcode = barcode
                };
                return View(ViewOrderModel);
            }

            else
            {
                Debug.WriteLine("In Else");
                string custNumber = "0";
                dynamic mymodel = new ExpandoObject();
                CityTireAndAutoEntities db = new CityTireAndAutoEntities();
                var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == id).FirstOrDefault();

                var ViewOrderModel = new ViewWorkOrderModel
                {
                    barcodeInformation = db.tbl_treadtracker_barcode.Where(x => x.retread_workorder == id).ToList(),
                    custInfo = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault(),
                    workOrderInfo = WorkOrderDetails,
                    WorkOrderNumber = id,
                    Barcode = barcode
                };
                return View(ViewOrderModel);
            }

        }

        [HttpPost]
        public ActionResult ViewOrder(ViewWorkOrderModel modeldata)
        {
            Debug.WriteLine("modeldata.Barcode in ViewOrder:" + modeldata.Barcode);
            return RedirectToAction("OpenOrder", new { id = modeldata.WorkOrderNumber, barcode = modeldata.Barcode });
        }

       


        private string searchNameValue = "";
        private string searchNoValue = "";
        public ActionResult NameDropDown(string value)
        {
            searchNameValue = value;
            Debug.WriteLine("In NameDropDown");
            if (value == "" || value is null)
            {
                value = "";
            }
            NewOrderSearchCriteria custList = new NewOrderSearchCriteria();
            custList.MatchedNames = PopulateMatchedValues(value,"Name");
            return PartialView(custList);
        }

       
        private static List<SelectListItem> PopulateMatchedValues(string param,string Fromtype)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "";
                if(Fromtype=="Name")
                {
                     query = "select cust_name,cust_us1 FRom tbl_customer_list  where cscttc ='True' and cust_us2='0' and cust_name like '%" + param + "%'";
                }
                else
                {
                    query = "select cust_name,cust_us1 FRom tbl_customer_list  where cscttc ='True' and cust_us2='0' and cust_us1 like '%" + param + "%'";
                }
                Debug.WriteLine("Query:" + query);
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
                                    Text = sdr["cust_name"].ToString(),
                                    Value = sdr["cust_us1"].ToString()
                                });
                            }
                        

                    }
                    con.Close();
                }
            }

            return items;
        }

        [HttpPost]
        public ActionResult SearchWithName(NewOrderSearchCriteria model)
        {
            model.MatchedNames = PopulateMatchedValues(searchNameValue,"Name");
            var selectedItem = model.MatchedNames.Find(p => p.Value == model.MatchedNameId.ToString());
            if (selectedItem != null)
            {
                Debug.WriteLine(selectedItem.Text);

            }
            string NameLike = selectedItem.Text;
            string custName = "";

            var jPos = NameLike.IndexOf("'");
            if(jPos!=-1)
            {
                NameLike = NameLike.Insert(jPos, "'");
            }
            
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select TOP 1 cust_name from tbl_customer_list where cscttc='True' and cust_us2='0' and upper(cust_name) like '%" + NameLike + "%' ";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            custName = sdr["cust_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }

            return RedirectToAction("NewOrder", new { values = custName });
        }

        public ActionResult NumberDropDown(string value)
        {
            searchNoValue = value;
            Debug.WriteLine("In NoDropDown");
            if (value == "" || value is null)
            {
                value = "";
            }
            NewOrderSearchCriteria custList = new NewOrderSearchCriteria();
            custList.MatchedNo = PopulateMatchedValues(value, "Number");
            return PartialView(custList);
        }


        [HttpPost]
        public ActionResult SearchWithNo(NewOrderSearchCriteria model)
        {
            model.MatchedNo = PopulateMatchedValues(searchNoValue, "Name");
            var selectedItem = model.MatchedNo.Find(p => p.Value == model.MatchedNoId.ToString());
            if (selectedItem != null)
            {
                Debug.WriteLine(selectedItem.Text);

            }
            string NameLike = selectedItem.Text;
            string custName = "";
            var jPos = NameLike.IndexOf("'");
            if (jPos != -1)
            {
                NameLike = NameLike.Insert(jPos, "'");
            }
           

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select TOP 1 cust_name from tbl_customer_list where cscttc='True' and cust_us2='0' and upper(cust_name) like '%" + NameLike + "%' ";
                Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            custName = sdr["cust_name"].ToString();
                        }
                    }
                    con.Close();
                }
            }

            return RedirectToAction("NewOrder", new { values = custName });
        }

        public ActionResult EditOrder(string orderNumber)
        {
            Debug.WriteLine("orderNumber:" + orderNumber);
            Debug.WriteLine("In EditOrder");
            string custNumber = "0";
            dynamic mymodel = new ExpandoObject();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == orderNumber).FirstOrDefault();
            if (WorkOrderDetails != null)
            {
                custNumber = WorkOrderDetails.customer_number;
            }
            else
            {
                custNumber = "0";
            }

            var casingSizeList= (from a in db.tbl_treadtracker_casing_sizes select a).ToList();

            ViewBag.CasingSizeList = casingSizeList;
            var casingBrandList = (from a in db.tbl_treadtracker_casing_brands select a).ToList();
            ViewBag.CasingBrandList = casingBrandList;
            var casingTreadList = (from a in db.tbl_treadtracker_treads select a).ToList();
            ViewBag.CasingTreadList = casingTreadList;

            var BarcodeJoinList =
                            (from barcode in db.tbl_treadtracker_barcode
                            join size in db.tbl_treadtracker_casing_sizes on barcode.casing_size equals size.casing_size
                            join brands in db.tbl_treadtracker_casing_brands on barcode.casing_brand equals brands.casing_brand
                            join tread in db.tbl_treadtracker_treads on barcode.retread_design equals tread.tread_design
                            where barcode.retread_workorder == orderNumber
                            orderby barcode.barcode
                             select new
                            {
                               barcode.barcode,
                               barcode.retread_workorder,
                               barcode.line_number,
                               barcode.line_code,
                               barcode.serial_dot,
                               barcode.casing_size,
                               barcode.casing_brand,
                               barcode.retread_design,
                               barcode.unit_ID,
                               barcode.preliminary_inspection_result,
                               barcode.preliminary_inspection_date,
                               barcode.ndt_machine_result,
                               barcode.ndt_machine_date,
                               barcode.buffer_builder_result,
                               barcode.buffer_builder_date,
                               barcode.final_inspection_result,
                               barcode.final_inspection_date,
                               barcode.ship_to_location,
                               size.size_id,
                               brands.brand_id,
                               tread.tread_id
                            }).ToList();
            Debug.WriteLine("custNumber:" + custNumber);
            List<EditOrderBarcodeJoinModel> BarcodeInfoVMlist = new List<EditOrderBarcodeJoinModel>();
            foreach (var item in BarcodeJoinList)

            {

                EditOrderBarcodeJoinModel objcvm = new EditOrderBarcodeJoinModel(); // ViewModel

                objcvm.barcode = item.barcode;

                objcvm.retread_workorder = item.retread_workorder;

                objcvm.line_number = item.line_number;

                objcvm.line_code = item.line_code;

                objcvm.serial_dot = item.serial_dot;

                objcvm.casing_size = item.casing_size;

                objcvm.casing_brand = item.casing_brand;

                objcvm.retread_design = item.retread_design;

                objcvm.unit_ID = item.unit_ID;

                objcvm.preliminary_inspection_result = item.preliminary_inspection_result;

                objcvm.preliminary_inspection_date = item.preliminary_inspection_date;

                objcvm.ndt_machine_result = item.ndt_machine_result;

                objcvm.ndt_machine_date = item.ndt_machine_date;

                objcvm.buffer_builder_result = item.buffer_builder_result;

                objcvm.buffer_builder_date = item.buffer_builder_date;

                objcvm.final_inspection_result = item.final_inspection_result;

                objcvm.final_inspection_date = item.final_inspection_date;

                objcvm.ship_to_location = item.ship_to_location;

                objcvm.size_id = item.size_id;

                objcvm.brand_id = item.brand_id;

                objcvm.tread_id = item.tread_id;

                BarcodeInfoVMlist.Add(objcvm);

            }
            var CustomerInfoList = db.tbl_customer_list.Where(x => x.cust_us1 == custNumber).FirstOrDefault();
            string custNameValue = CustomerInfoList.cust_name;
            Debug.WriteLine("custNameValue:" + custNameValue + "hkjhkh");

            List<SelectListItem> custListItems = PopulateCustomers(custNameValue);
            
            var EditOrderModel = new EditOrderModel
            {
                BarcodeInformation = BarcodeInfoVMlist,
                CustomerInfo = CustomerInfoList,
                WorkOrderInfo = WorkOrderDetails,
                CustomersList = custListItems
                //ChangedCustomerId=-1
            };
            return View(EditOrderModel);
        }

        [NonAction]
        public SelectList ToSelectList(List<tbl_treadtracker_casing_sizes> lstsize)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (tbl_treadtracker_casing_sizes item in lstsize)
            {
                Debug.WriteLine(item.casing_size + "   " + item.size_id);
                list.Add(new SelectListItem()
                {
                    
                    Text = item.casing_size,
                    Value = Convert.ToString(item.size_id)
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        [HttpPost]
        public ActionResult SubmitEditOrder(EditOrderModel model)
        {
            Debug.WriteLine("Model Values:"+ model.WorkOrderInfo.retread_workorder);
           

            //This code is to find changed customer number
            model.CustomersList = PopulateCustomers("Nothing");
            var selectedItem = model.CustomersList.Find(p => p.Value == model.ChangedCustomerId.ToString());
            if (selectedItem != null)
            {
               
                Debug.WriteLine("The selected CustomersList value in the edit button is:"+selectedItem.Text);

            }
            //end

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            
            var WorkOrderDetails = db.tbl_treadtracker_workorder.Where(x => x.retread_workorder == model.WorkOrderInfo.retread_workorder).FirstOrDefault();
            if (WorkOrderDetails != null)
            {
                
                WorkOrderDetails.customer_number = selectedItem.Value;
                //Testing
                //Debug.WriteLine("Testing the owrk order");
                //Debug.WriteLine(WorkOrderDetails.retread_workorder);
                //Debug.WriteLine(WorkOrderDetails.customer_number + "    " + selectedItem.Value);
            }

            var casingSizeList = (from a in db.tbl_treadtracker_casing_sizes select a).ToList();
            var casingBrandList = (from a in db.tbl_treadtracker_casing_brands select a).ToList();
            var casingTreadList = (from a in db.tbl_treadtracker_treads select a).ToList();

            foreach (var items in model.BarcodeInformation)
            {
               
                var selectedSize = casingSizeList.Find(p => p.size_id == Convert.ToInt32(items.casing_size));
                string casingSize = "";
                if (selectedSize != null)
                {
                    casingSize= selectedSize.casing_size;

                }

                var selectedBrand = casingBrandList.Find(p => p.brand_id == Convert.ToInt32(items.casing_brand));
                string casingBrand = "";
                if (selectedBrand != null)
                {
                    casingBrand = selectedBrand.casing_brand;

                }

                var selectedTread = casingTreadList.Find(p => p.tread_id == Convert.ToInt32(items.retread_design));
                string casingTread = "";
                if (selectedTread != null)
                {
                    casingTread = selectedTread.tread_design;

                }

                
                var result = db.tbl_treadtracker_barcode.Where(a => a.barcode.Equals(items.barcode)).FirstOrDefault();
                if (result != null)
                {
                    result.line_code = items.line_code;
                    result.serial_dot = items.serial_dot;
                    result.casing_size = casingSize;
                    result.casing_brand = casingBrand;
                    result.retread_design = casingTread;
                    result.unit_ID = items.unit_ID;

                    //Testing
                    //Debug.WriteLine(result.barcode +"    "+ items.barcode);
                    //Debug.WriteLine(result.line_code + "    " + items.line_code);
                    //Debug.WriteLine(result.serial_dot + "    " + items.serial_dot);
                    //Debug.WriteLine(result.casing_size + "    " + casingSize);
                    //Debug.WriteLine(result.casing_brand + "    " + casingBrand);
                    //Debug.WriteLine(result.retread_design + "    " + casingTread);
                    //Debug.WriteLine(result.unit_ID + "    " + items.unit_ID);
                    //Debug.WriteLine("**************************");
                }
            }
            
            db.SaveChanges();
            return RedirectToAction("OpenOrder", new { id = model.WorkOrderInfo.retread_workorder, barcode = "" });
        }


        public ActionResult CanceEditOrderOrder(string workOrder)
        {
            return RedirectToAction("OpenOrder", new { id = workOrder, barcode = "" });
        }

        public ActionResult ShowEditCustInfo(string value)
        {
            Debug.WriteLine("In ShowEditCustInfo");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var CustomerInfoList = db.tbl_customer_list.Where(x => x.cust_us1 == value).FirstOrDefault();
            if(CustomerInfoList!=null)
            {              
                ViewData["CustomerNum"] = CustomerInfoList.cust_us1;
                ViewData["Address"] = CustomerInfoList.cust_add1+","+ CustomerInfoList.cust_state+","+ CustomerInfoList.cust_zip;
            }
            
            return PartialView();
        }
    }
}