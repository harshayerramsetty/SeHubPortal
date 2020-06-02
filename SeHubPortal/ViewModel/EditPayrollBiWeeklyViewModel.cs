using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeHubPortal.ViewModel
{
    public class EditPayrollBiWeeklyViewModel
    {
        public int employee_id { get; set; }
        public int payroll_id { get; set; }
        public Nullable<double> sat_1_reg { get; set; }
        public Nullable<double> mon_1_reg { get; set; }
        public Nullable<double> tues_1_reg { get; set; }
        public Nullable<double> wed_1_reg { get; set; }
        public Nullable<double> thurs_1_reg { get; set; }
        public Nullable<double> fri_1_reg { get; set; }
        public Nullable<double> sat_2_reg { get; set; }
        public Nullable<double> mon_2_reg { get; set; }
        public Nullable<double> tues_2_reg { get; set; }
        public Nullable<double> wed_2_reg { get; set; }
        public Nullable<double> thurs_2_reg { get; set; }
        public Nullable<double> fri_2_reg { get; set; }
        public Nullable<double> sat_1_opt { get; set; }
        public Nullable<double> mon_1_opt { get; set; }
        public Nullable<double> tues_1_opt { get; set; }
        public Nullable<double> wed_1_opt { get; set; }
        public Nullable<double> thurs_1_opt { get; set; }
        public Nullable<double> fri_1_opt { get; set; }
        public Nullable<double> sat_2_opt { get; set; }
        public Nullable<double> mon_2_opt { get; set; }
        public Nullable<double> tues_2_opt { get; set; }
        public Nullable<double> wed_2_opt { get; set; }
        public Nullable<double> thurs_2_opt { get; set; }
        public Nullable<double> fri_2_opt { get; set; }
        public string sat_1_sel { get; set; }
        public string mon_1_sel { get; set; }
        public string tues_1_sel { get; set; }
        public string wed_1_sel { get; set; }
        public string thurs_1_sel { get; set; }
        public string fri_1_sel { get; set; }
        public string sat_2_sel { get; set; }
        public string mon_2_sel { get; set; }
        public string tues_2_sel { get; set; }
        public string wed_2_sel { get; set; }
        public string thurs_2_sel { get; set; }
        public string fri_2_sel { get; set; }
        public Nullable<double> sat_1_sum { get; set; }
        public Nullable<double> mon__1_sum { get; set; }
        public Nullable<double> tues_1_sum { get; set; }
        public Nullable<double> wed_1_sum { get; set; }
        public Nullable<double> thurs_1_sum { get; set; }
        public Nullable<double> fri_1_sum { get; set; }
        public Nullable<double> sat_2_sum { get; set; }
        public Nullable<double> mon_2_sum { get; set; }
        public Nullable<double> tues_2_sum { get; set; }
        public Nullable<double> wed_2_Sum { get; set; }
        public Nullable<double> thurs_2_Sum { get; set; }
        public Nullable<double> fri_2_sum { get; set; }
        public string bi_week_chkin_avg { get; set; }
        public string bi_week_chkout_avg { get; set; }
        public Nullable<int> last_updated_by { get; set; }
        public Nullable<System.DateTime> last_update_date { get; set; }
        public Nullable<int> recordflag { get; set; }
        public string comments { get; set; }
        public Nullable<double> timeClock_sat1 { get; set; }
        public Nullable<double> timeClock_mon1 { get; set; }
        public Nullable<double> timeClock_tues1 { get; set; }
        public Nullable<double> timeClock_wed1 { get; set; }
        public Nullable<double> timeClock_thurs1 { get; set; }
        public Nullable<double> timeClock_fri1 { get; set; }
        public Nullable<double> timeClock_sat2 { get; set; }
        public Nullable<double> timeClock_mon2 { get; set; }
        public Nullable<double> timeClock_tues2 { get; set; }
        public Nullable<double> timeClock_wed2 { get; set; }
        public Nullable<double> timeClock_thurs2 { get; set; }
        public Nullable<double> timeClock_fri2 { get; set; }
        public Nullable<double> sun_1_reg { get; set; }
        public Nullable<double> sun_2_reg { get; set; }
        public Nullable<double> sun_1_opt { get; set; }
        public Nullable<double> sun_2_opt { get; set; }
        public string sun_1_sel { get; set; }
        public string sun_2_sel { get; set; }
        public Nullable<double> sun_1_sum { get; set; }
        public Nullable<double> sun_2_sum { get; set; }
        public Nullable<double> timeClock_sun1 { get; set; }
        public Nullable<double> timeClock_sun2 { get; set; }

        public int sat_1_sel_id { get; set; }
        public int sun_1_sel_id { get; set; }
        public int mon_1_sel_id { get; set; }
        public int tues_1_sel_id { get; set; }
        public int wed_1_sel_id { get; set; }
        public int thurs_1_sel_id { get; set; }
        public int fri_1_sel_id { get; set; }
        public int sat_2_sel_id { get; set; }
        public int sun_2_sel_id { get; set; }
        public int mon_2_sel_id { get; set; }
        public int tues_2_sel_id { get; set; }
        public int wed_2_sel_id { get; set; }
        public int thurs_2_sel_id { get; set; }
        public int fri_2_sel_id { get; set; }
    }
}