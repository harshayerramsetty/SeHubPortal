using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class TechnicianEfficiencyViewModel
    {
        public string Technician { get; set; }
        public string TechClass { get; set; }
        public string LastPayrollEff { get; set; }
        public string OverallEfficiency { get; set; }
        public string RegularHours { get; set; }
        public string OtHours { get; set; }
        public string ComissionableSalesDue { get; set; }
        public string Doorrate { get; set; }
        public string BilledHoursPayroll { get; set; }
        public string BilledHoursYear { get; set; }
        public string CompensationType { get; set; }
        public string CompensationPlan { get; set; }
        public int Status { get; set; }
    }
}