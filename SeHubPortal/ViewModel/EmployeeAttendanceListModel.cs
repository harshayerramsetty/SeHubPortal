using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class EmployeeAttendanceListModel
    {
        public string employeeId { get; set; }
        public int auto_emp_id { get; set; }
        public string fullName { get; set; }
        public string atWork { get; set; }
        public bool atJob { get; set; }
        public string position { get; set; }
        public byte[] profilePic { get; set; }
        public List<TimeChardViewModel> events { get; set; }
    }
}