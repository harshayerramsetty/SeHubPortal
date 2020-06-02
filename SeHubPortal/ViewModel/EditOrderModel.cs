﻿using SeHubPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeHubPortal.ViewModel
{
    public class EditOrderModel
    {
        public List<EditOrderBarcodeJoinModel> BarcodeInformation { get; set; }
        public tbl_customer_list CustomerInfo { get; set; }
        public tbl_treadtracker_workorder WorkOrderInfo { get; set; }
        public List<SelectListItem> CustomersList { get; set; }
        public string ChangedCustomerId { get; set; }
    }
}