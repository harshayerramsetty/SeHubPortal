//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SeHubPortal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_source_payroll_EI_brackets
    {
        public double lower_income_bracket { get; set; }
        public double upper_income_bracket { get; set; }
        public double rate { get; set; }
        public Nullable<double> max_EI_income { get; set; }
        public int year { get; set; }
    }
}
