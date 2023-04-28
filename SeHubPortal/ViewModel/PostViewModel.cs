using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class PostViewModel
    {
        public string post_id { get; set; }
        public string catgory { get; set; }
        public string subject { get; set; }
        public string details { get; set; }
        public string auther { get; set; }
        public string auther_shrtName { get; set; }
        public string auther_img { get; set; }
        public string auther_loc { get; set; }
        public string auther_position { get; set; }

        public List<KeyValuePair<string, string>> attachments { get; set; }

        public DateTime date { get; set; }
        public string loc_id { get; set; }
    }
}