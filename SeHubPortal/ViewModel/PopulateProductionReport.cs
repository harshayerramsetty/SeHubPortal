using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SeHubPortal.Models;

namespace SeHubPortal.ViewModel
{
    public class PopulateProductionReport
    {
        public List<tbl_treadtracker_barcode> barcodeInfo { get; set; }
        public List<tbl_treadtracker_workorder> workOrderInfo { get; set; }
        public List<tbl_cta_customers> customerInfo { get; set; }

        public List<KeyValuePair<string, int>> TransferOUT { get; set; }
        public List<KeyValuePair<string, int>> TransferIN { get; set; }

        public tbl_cta_location_info locInfo { get; set; }
        public DateTime date { get; set; }

        public List<tbl_source_treadtracker_creditSchedule> creditSchedule { get; set; }
        public List<tbl_source_treadtracker_freightSchedule> freightSchedule { get; set; }

        public List<tbl_source_treadtracker_creditSchedule_revision> creditSchedule_revision { get; set; }
        public List<tbl_source_treadtracker_freightSchedule_revision> freightSchedule_revision { get; set; }
        public List<tbl_source_treadtracker_casingTire> casingTire { get; set; }
        public List<tbl_treadtracker_customer_retread_spec> customerProfile { get; set; }
    }
}