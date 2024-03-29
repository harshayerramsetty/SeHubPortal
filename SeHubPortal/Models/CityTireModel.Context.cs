﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CityTireAndAutoEntities : DbContext
    {
        public CityTireAndAutoEntities()
            : base("name=CityTireAndAutoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tbl_customer_list> tbl_customer_list { get; set; }
        public virtual DbSet<tbl_employee> tbl_employee { get; set; }
        public virtual DbSet<tbl_employee_credentials> tbl_employee_credentials { get; set; }
        public virtual DbSet<tbl_treadtracker_workorder> tbl_treadtracker_workorder { get; set; }
        public virtual DbSet<tbl_treadtracker_casing_brands> tbl_treadtracker_casing_brands { get; set; }
        public virtual DbSet<tbl_treadtracker_casing_sizes> tbl_treadtracker_casing_sizes { get; set; }
        public virtual DbSet<tbl_employee_personal> tbl_employee_personal { get; set; }
        public virtual DbSet<tbl_payroll_category_selection> tbl_payroll_category_selection { get; set; }
        public virtual DbSet<tbl_vehicle_info> tbl_vehicle_info { get; set; }
        public virtual DbSet<database_firewall_rules> database_firewall_rules { get; set; }
        public virtual DbSet<tbl_tire_adjustment> tbl_tire_adjustment { get; set; }
        public virtual DbSet<tbl_dispute_resolution> tbl_dispute_resolution { get; set; }
        public virtual DbSet<tbl_Calendar_events> tbl_Calendar_events { get; set; }
        public virtual DbSet<tbl_payroll_settings> tbl_payroll_settings { get; set; }
        public virtual DbSet<tbl_employee_payroll_dates> tbl_employee_payroll_dates { get; set; }
        public virtual DbSet<tbl_position_info> tbl_position_info { get; set; }
        public virtual DbSet<tbl_payroll_change_loction> tbl_payroll_change_loction { get; set; }
        public virtual DbSet<tbl_calculator_GM_parts> tbl_calculator_GM_parts { get; set; }
        public virtual DbSet<tbl_calculator_GM_tire> tbl_calculator_GM_tire { get; set; }
        public virtual DbSet<tbl_fleettvt_Customer> tbl_fleettvt_Customer { get; set; }
        public virtual DbSet<tbl_commercial_tire_sizes> tbl_commercial_tire_sizes { get; set; }
        public virtual DbSet<tbl_fleettvt_configurations> tbl_fleettvt_configurations { get; set; }
        public virtual DbSet<tbl_fleetTVT_fieldsurvey_tire> tbl_fleetTVT_fieldsurvey_tire { get; set; }
        public virtual DbSet<tbl_fleetTVT_fieldsurvey_unit> tbl_fleetTVT_fieldsurvey_unit { get; set; }
        public virtual DbSet<tbl_source_tire_condition> tbl_source_tire_condition { get; set; }
        public virtual DbSet<tbl_source_tire_wear> tbl_source_tire_wear { get; set; }
        public virtual DbSet<tbl_source_valve> tbl_source_valve { get; set; }
        public virtual DbSet<tblLocation> tblLocations { get; set; }
        public virtual DbSet<tbl_source_wheel_condition> tbl_source_wheel_condition { get; set; }
        public virtual DbSet<tbl_treadtracker_inventory_casing> tbl_treadtracker_inventory_casing { get; set; }
        public virtual DbSet<tbl_fleetTVT_unit> tbl_fleetTVT_unit { get; set; }
        public virtual DbSet<tbl_employee_status> tbl_employee_status { get; set; }
        public virtual DbSet<tbl_fuel_log_fleet> tbl_fuel_log_fleet { get; set; }
        public virtual DbSet<tbl_employee_payroll_biweekly> tbl_employee_payroll_biweekly { get; set; }
        public virtual DbSet<tbl_employee_payroll_summary> tbl_employee_payroll_summary { get; set; }
        public virtual DbSet<tbl_payroll_submission_branch> tbl_payroll_submission_branch { get; set; }
        public virtual DbSet<tbl_employee_payroll_final> tbl_employee_payroll_final { get; set; }
        public virtual DbSet<tbl_payroll_submission_corporate> tbl_payroll_submission_corporate { get; set; }
        public virtual DbSet<tbl_cta_location_info> tbl_cta_location_info { get; set; }
        public virtual DbSet<tbl_tread_tracker_customers> tbl_tread_tracker_customers { get; set; }
        public virtual DbSet<tbl_employee_payroll_submission> tbl_employee_payroll_submission { get; set; }
        public virtual DbSet<tbl_sehub_access> tbl_sehub_access { get; set; }
        public virtual DbSet<tbl_fuel_log_invoiced> tbl_fuel_log_invoiced { get; set; }
        public virtual DbSet<tbl_fuel_log_shopSupplies> tbl_fuel_log_shopSupplies { get; set; }
        public virtual DbSet<tbl_login_log> tbl_login_log { get; set; }
        public virtual DbSet<tbl_source_tire> tbl_source_tire { get; set; }
        public virtual DbSet<tbl_customer_reporting_customers> tbl_customer_reporting_customers { get; set; }
        public virtual DbSet<tbl_vacation_schedule> tbl_vacation_schedule { get; set; }
        public virtual DbSet<tbl_expense_claim_account> tbl_expense_claim_account { get; set; }
        public virtual DbSet<tbl_expense_claim> tbl_expense_claim { get; set; }
        public virtual DbSet<tbl_timeclock_devices> tbl_timeclock_devices { get; set; }
        public virtual DbSet<tbl_calculator_GP_fixed> tbl_calculator_GP_fixed { get; set; }
        public virtual DbSet<tbl_treadTracker_workStations> tbl_treadTracker_workStations { get; set; }
        public virtual DbSet<tbl_source_DOT_plantcodes> tbl_source_DOT_plantcodes { get; set; }
        public virtual DbSet<tbl_source_DOT_sizecodes> tbl_source_DOT_sizecodes { get; set; }
        public virtual DbSet<tbl_harpoon_clients> tbl_harpoon_clients { get; set; }
        public virtual DbSet<tbl_pricelist> tbl_pricelist { get; set; }
        public virtual DbSet<tbl_harpoon_users> tbl_harpoon_users { get; set; }
        public virtual DbSet<tbl_harpoon_source_locationZone> tbl_harpoon_source_locationZone { get; set; }
        public virtual DbSet<tbl_harpoon_source_serialNumbers> tbl_harpoon_source_serialNumbers { get; set; }
        public virtual DbSet<tbl_harpoon_source_displayColors> tbl_harpoon_source_displayColors { get; set; }
        public virtual DbSet<tbl_harpoon_settings_schedule> tbl_harpoon_settings_schedule { get; set; }
        public virtual DbSet<tbl_harpoon_source_userProfiles> tbl_harpoon_source_userProfiles { get; set; }
        public virtual DbSet<tbl_harpoon_employee_attendance> tbl_harpoon_employee_attendance { get; set; }
        public virtual DbSet<tbl_harpoon_locations> tbl_harpoon_locations { get; set; }
        public virtual DbSet<tbl_harpoon_devices> tbl_harpoon_devices { get; set; }
        public virtual DbSet<tbl_harpoon_settings> tbl_harpoon_settings { get; set; }
        public virtual DbSet<tbl_harpoon_employee_rfid> tbl_harpoon_employee_rfid { get; set; }
        public virtual DbSet<tbl_harpoon_employee> tbl_harpoon_employee { get; set; }
        public virtual DbSet<tbl_source_commercial_tire_manufacturers> tbl_source_commercial_tire_manufacturers { get; set; }
        public virtual DbSet<tbl_source_retread_tread> tbl_source_retread_tread { get; set; }
        public virtual DbSet<tbl_source_RARcodes> tbl_source_RARcodes { get; set; }
        public virtual DbSet<tbl_cta_location_survey> tbl_cta_location_survey { get; set; }
        public virtual DbSet<tbl_management_training> tbl_management_training { get; set; }
        public virtual DbSet<tbl_harpoon_jobclock> tbl_harpoon_jobclock { get; set; }
        public virtual DbSet<tbl_harpoon_job_log> tbl_harpoon_job_log { get; set; }
        public virtual DbSet<tbl_sehub_color_scheme> tbl_sehub_color_scheme { get; set; }
        public virtual DbSet<tbl_treadtracke_stock_production_schedule> tbl_treadtracke_stock_production_schedule { get; set; }
        public virtual DbSet<tbl_harpoon_vehicle> tbl_harpoon_vehicle { get; set; }
        public virtual DbSet<tbl_source_retread_part_number> tbl_source_retread_part_number { get; set; }
        public virtual DbSet<tbl_harpoon_fuel_log> tbl_harpoon_fuel_log { get; set; }
        public virtual DbSet<tbl_production_schedule> tbl_production_schedule { get; set; }
        public virtual DbSet<tbl_harpoon_fuelReporting_chargeAccounts> tbl_harpoon_fuelReporting_chargeAccounts { get; set; }
        public virtual DbSet<tbl_cta_customers> tbl_cta_customers { get; set; }
        public virtual DbSet<tbl_data_import_history> tbl_data_import_history { get; set; }
        public virtual DbSet<tbl_harpoon_attendance_log> tbl_harpoon_attendance_log { get; set; }
        public virtual DbSet<tbl_attendance_log> tbl_attendance_log { get; set; }
        public virtual DbSet<tbl_source_payrollDeductions_cpp> tbl_source_payrollDeductions_cpp { get; set; }
        public virtual DbSet<tbl_source_payrollDeductions_ei> tbl_source_payrollDeductions_ei { get; set; }
        public virtual DbSet<tbl_harpoon_departments> tbl_harpoon_departments { get; set; }
        public virtual DbSet<tbl_salesReport_branch_monthly> tbl_salesReport_branch_monthly { get; set; }
        public virtual DbSet<tbl_data_logger_temperature> tbl_data_logger_temperature { get; set; }
        public virtual DbSet<tbl_location_departments> tbl_location_departments { get; set; }
        public virtual DbSet<tbl_Efficiancy_Technician_Commissions> tbl_Efficiancy_Technician_Commissions { get; set; }
        public virtual DbSet<tbl_source_mediumTruckTires> tbl_source_mediumTruckTires { get; set; }
        public virtual DbSet<tbl_tread_tracker_chamber_data_logger> tbl_tread_tracker_chamber_data_logger { get; set; }
        public virtual DbSet<tbl_treadTracker_chamber_senser> tbl_treadTracker_chamber_senser { get; set; }
        public virtual DbSet<tbl_source_treadtracker_cap_count> tbl_source_treadtracker_cap_count { get; set; }
        public virtual DbSet<tbl_source_treadtracker_creditSchedule> tbl_source_treadtracker_creditSchedule { get; set; }
        public virtual DbSet<tbl_source_treadtracker_freightSchedule> tbl_source_treadtracker_freightSchedule { get; set; }
        public virtual DbSet<tbl_source_labour_rates> tbl_source_labour_rates { get; set; }
        public virtual DbSet<tbl_cta_locations_reporting> tbl_cta_locations_reporting { get; set; }
        public virtual DbSet<tbl_source_treadtracker_consumables_TreadRubber> tbl_source_treadtracker_consumables_TreadRubber { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<tbl_customer_list_bakp> tbl_customer_list_bakp { get; set; }
        public virtual DbSet<tbl_data_logger_pressure> tbl_data_logger_pressure { get; set; }
        public virtual DbSet<tbl_employee_leave_annual_calculations> tbl_employee_leave_annual_calculations { get; set; }
        public virtual DbSet<tbl_GTX_customer_list> tbl_GTX_customer_list { get; set; }
        public virtual DbSet<tbl_source_treadtracker_consumables_nonTreadRubber> tbl_source_treadtracker_consumables_nonTreadRubber { get; set; }
        public virtual DbSet<tbl_source_workStation_info> tbl_source_workStation_info { get; set; }
        public virtual DbSet<tbl_timeclock_clients> tbl_timeclock_clients { get; set; }
        public virtual DbSet<tbl_treadtracker_barcode_bck_20200412> tbl_treadtracker_barcode_bck_20200412 { get; set; }
        public virtual DbSet<tbl_treadtracker_customer_TT200_spec> tbl_treadtracker_customer_TT200_spec { get; set; }
        public virtual DbSet<tbl_treadtracker_inventory_treadRubber> tbl_treadtracker_inventory_treadRubber { get; set; }
        public virtual DbSet<tbl_treadtracker_LineCode_PlantProcess> tbl_treadtracker_LineCode_PlantProcess { get; set; }
        public virtual DbSet<tbl_treadtracker_workorder_bck_20200412> tbl_treadtracker_workorder_bck_20200412 { get; set; }
        public virtual DbSet<tblCity> tblCities { get; set; }
        public virtual DbSet<tblEmployee> tblEmployees { get; set; }
        public virtual DbSet<tbl_customer_list_bkp> tbl_customer_list_bkp { get; set; }
        public virtual DbSet<tbl_employee_attendance_bck> tbl_employee_attendance_bck { get; set; }
        public virtual DbSet<tbl_employee_bck2> tbl_employee_bck2 { get; set; }
        public virtual DbSet<tbl_treadtracker_barcode_bck> tbl_treadtracker_barcode_bck { get; set; }
        public virtual DbSet<tblLocationBkp> tblLocationBkps { get; set; }
        public virtual DbSet<tblServiceEstimate_new> tblServiceEstimate_new { get; set; }
        public virtual DbSet<tblServiceEstimateLastDay> tblServiceEstimateLastDays { get; set; }
        public virtual DbSet<tbl_reporting_sales_labour_daily> tbl_reporting_sales_labour_daily { get; set; }
        public virtual DbSet<tbl_source_fuelLog_chargeAccount> tbl_source_fuelLog_chargeAccount { get; set; }
        public virtual DbSet<tbl_source_treadtracker_freightSchedule_revision> tbl_source_treadtracker_freightSchedule_revision { get; set; }
        public virtual DbSet<tbl_source_treadtracker_casingTire> tbl_source_treadtracker_casingTire { get; set; }
        public virtual DbSet<tbl_source_harpoon_timesheet_categories> tbl_source_harpoon_timesheet_categories { get; set; }
        public virtual DbSet<tbl_source_harpoon_timesheet_category_selection> tbl_source_harpoon_timesheet_category_selection { get; set; }
        public virtual DbSet<tbl_harpoon_timesheet> tbl_harpoon_timesheet { get; set; }
        public virtual DbSet<tbl_employee_attendance> tbl_employee_attendance { get; set; }
        public virtual DbSet<tbl_treadtracker_production_log> tbl_treadtracker_production_log { get; set; }
        public virtual DbSet<tbl_source_payroll_cpp_brackets> tbl_source_payroll_cpp_brackets { get; set; }
        public virtual DbSet<tbl_source_payroll_EI_brackets> tbl_source_payroll_EI_brackets { get; set; }
        public virtual DbSet<tbl_source_payroll_federal_tax_brackets> tbl_source_payroll_federal_tax_brackets { get; set; }
        public virtual DbSet<tbl_source_payroll_provincial_tax_brackets> tbl_source_payroll_provincial_tax_brackets { get; set; }
        public virtual DbSet<tbl_payroll_employee_paystubs> tbl_payroll_employee_paystubs { get; set; }
        public virtual DbSet<tbl_source_treadtracker_creditSchedule_revision> tbl_source_treadtracker_creditSchedule_revision { get; set; }
        public virtual DbSet<tbl_treadtracker_customer_retread_spec> tbl_treadtracker_customer_retread_spec { get; set; }
        public virtual DbSet<tbl_treadtracker_chamber_production> tbl_treadtracker_chamber_production { get; set; }
        public virtual DbSet<tbl_payroll_employee_specifications> tbl_payroll_employee_specifications { get; set; }
        public virtual DbSet<tbl_connectionCheck_arduino_decibleMeter> tbl_connectionCheck_arduino_decibleMeter { get; set; }
        public virtual DbSet<tbl_source_storyBoard_categories> tbl_source_storyBoard_categories { get; set; }
        public virtual DbSet<tbl_storyBoard_posts_temp> tbl_storyBoard_posts_temp { get; set; }
        public virtual DbSet<tbl_deviceLog_timeClock> tbl_deviceLog_timeClock { get; set; }
        public virtual DbSet<tbl_source_payroll_departments> tbl_source_payroll_departments { get; set; }
        public virtual DbSet<tbl_source_timeclock_displayColors> tbl_source_timeclock_displayColors { get; set; }
        public virtual DbSet<tbl_treadTracker_Envelope> tbl_treadTracker_Envelope { get; set; }
        public virtual DbSet<tbl_storyBoard_posts> tbl_storyBoard_posts { get; set; }
        public virtual DbSet<tbl_tools_decibel_meter> tbl_tools_decibel_meter { get; set; }
        public virtual DbSet<tbl_source_truckTire_sizes> tbl_source_truckTire_sizes { get; set; }
        public virtual DbSet<tbl_treadtracker_barcode> tbl_treadtracker_barcode { get; set; }
    }
}
