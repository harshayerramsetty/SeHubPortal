using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class EditOrderBarcodeJoinModel
    {
        public string barcode { get; set; }
        public string changed_barcode { get; set; }
        public string retread_workorder { get; set; }
        public Nullable<int> line_number { get; set; }
        public Nullable<int> line_code { get; set; }
        public string serial_dot { get; set; }
        public string casing_size { get; set; }
        public string casing_brand { get; set; }
        public string retread_design { get; set; }
        public string unit_ID { get; set; }
        public string preliminary_inspection_result { get; set; }
        public Nullable<System.DateTime> preliminary_inspection_date { get; set; }
        public string ndt_machine_result { get; set; }
        public Nullable<System.DateTime> ndt_machine_date { get; set; }
        public string buffer_builder_result { get; set; }
        public Nullable<System.DateTime> buffer_builder_date { get; set; }
        public string final_inspection_result { get; set; }
        public Nullable<System.DateTime> final_inspection_date { get; set; }
        public string ship_to_location { get; set; }
        public int size_id { get; set; }
        public int brand_id { get; set; }
        public int tread_id { get; set; }
        public bool virgin { get; set; }
    }
}