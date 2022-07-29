using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Net.Mail;
using System.Data;
using System.Globalization;
using System.Drawing.Drawing2D;


namespace SeHubPortal.Controllers
{
    public class ManagementController : Controller
    {
        [HttpGet]
        public ActionResult Payroll(string locId, string employeeId, string payrollID)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            //System.Diagnostics.Trace.WriteLine("On Payroll Load:" + locId + "  :" + employeeId + ":" + payrollID);

            int empId = Convert.ToInt32(Session["userID"].ToString());

            ViewData["UserEmployeeID"] = empId;

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            EmployeePayrollModel payrollModel = new EmployeePayrollModel();

            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            payrollModel.SehubAccess = empDetails;

            if (payrollModel.SehubAccess.payroll == 0)
            { 
                return RedirectToAction("Dashboard", "Settings");
            }

            string location = "AHO";

            if (locId is null || locId == "")
            {
                var locationDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

                if (locationDetails != null)
                {
                    location = locationDetails.loc_ID;
                }

            }
            else
            {
                location = locId;
            }
            ViewData["LocationList"] = location;

            DateTime CurrentDay = DateTime.Today.AddDays(-(db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value));

            var CurrentPayrollId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= CurrentDay && x.end_date >= CurrentDay).FirstOrDefault();
            
            int PayrollIDInt = Convert.ToInt32(payrollID);

            if (payrollID == null || payrollID == "")
            {
                PayrollIDInt = CurrentPayrollId.payroll_Id;
            }

            //Trace.WriteLine("This is the PayrollID" + PayrollIDInt);

            

            var CurrentEmployeeList = db.tbl_employee.Where(x => x.loc_ID == location).ToList();

            List<EmployeePayrollListModel> emplyPayrollList = new List<EmployeePayrollListModel>();
            foreach (var item in CurrentEmployeeList)
            {

                if(item.status == 1)
                {
                    var datejoin = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.date_of_joining).FirstOrDefault();

                    var payrollendDate = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == PayrollIDInt).Select(x => x.end_date).FirstOrDefault();

                    if (payrollendDate > datejoin) //
                    {
                        EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                                       //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                                                                                       //string[] values = item.ToString().Split(';');
                        obj.employeeId = item.employee_id.ToString();
                        obj.fullName = item.full_name;

                        var empInBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == item.employee_id && x.payroll_id == PayrollIDInt && x.loc_id == location).FirstOrDefault();

                        if (empInBiweekly != null)
                        {
                            obj.submissionStatus = empInBiweekly.recordflag.ToString();
                        }
                        else
                        {
                            obj.submissionStatus = "0";
                        }

                        var empInCorp = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == PayrollIDInt && x.location_id == location).FirstOrDefault();

                        if (empInCorp != null)
                        {
                            obj.submissionStatusCorporate = empInCorp.recordFlag.ToString();
                        }
                        else
                        {
                            obj.submissionStatusCorporate = "0";
                        }

                        emplyPayrollList.Add(obj);
                    }
                    else
                    {

                    }
                }
                else
                {
                    var dateleaving = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.date_of_leaving).FirstOrDefault();

                    var payrollStartDate = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == PayrollIDInt).Select(x => x.start_date).FirstOrDefault();

                    if (dateleaving > payrollStartDate) //
                    {
                        EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                                       //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                                                                                       //string[] values = item.ToString().Split(';');
                        obj.employeeId = item.employee_id.ToString();
                        obj.fullName = item.full_name;

                        var empInBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == item.employee_id && x.payroll_id == PayrollIDInt && x.loc_id == location).FirstOrDefault();

                        if (empInBiweekly != null)
                        {
                            obj.submissionStatus = empInBiweekly.recordflag.ToString();
                        }
                        else
                        {
                            obj.submissionStatus = "0";
                        }

                        var empInCorp = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == PayrollIDInt && x.location_id == location).FirstOrDefault();

                        if (empInCorp != null)
                        {
                            obj.submissionStatusCorporate = empInCorp.recordFlag.ToString();
                        }
                        else
                        {
                            obj.submissionStatusCorporate = "0";
                        }

                        emplyPayrollList.Add(obj);
                    }
                }
               
            }

            if (employeeId is null || employeeId == "")
            {
                string empDefault = Convert.ToString(emplyPayrollList.FirstOrDefault().employeeId);
                employeeId = empDefault;
            }

            var validateBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id == PayrollIDInt).ToList();
            var validateCorp = db.tbl_employee_payroll_submission.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id == PayrollIDInt).ToList();

            payrollModel.validate = validateBiweekly;

            List<tbl_employee_payroll_submission> validationWithAdjustment = new List<tbl_employee_payroll_submission>();

            foreach (var vald in validateCorp)
            {

                tbl_employee_payroll_submission tempSub = new tbl_employee_payroll_submission();

                double adp = 0;
                double adp1 = 0;
                double adp2 = 0;
                double reg = 0;
                double ot = 0;

                if (vald.adjustmentPay.HasValue)
                {
                    adp = vald.adjustmentPay.Value;
                }
                if (vald.adjustmentPay1.HasValue)
                {
                    adp1 = vald.adjustmentPay1.Value;
                }
                if (vald.adjustmentPay2.HasValue)
                {
                    adp2 = vald.adjustmentPay2.Value;
                }
                if (vald.regular.HasValue)
                {
                    reg = vald.regular.Value;
                }
                if (vald.ot.HasValue)
                {
                    ot = vald.ot.Value;
                }

                tempSub.regular = reg + checkAdjustments("Regular Pay", vald.adjustment_type, vald.adjustment_type1, vald.adjustment_type2, vald.plus_minus, vald.plus_minus1, vald.plus_minus2, adp, adp1, adp2);
                tempSub.ot = ot + checkAdjustments("Over Time", vald.adjustment_type, vald.adjustment_type1, vald.adjustment_type2, vald.plus_minus, vald.plus_minus1, vald.plus_minus2, adp, adp1, adp2);
                tempSub.location_id = vald.location_id;


                //Trace.WriteLine(" This is the vald_Regular" + vald.regular);

                validationWithAdjustment.Add(tempSub);
            }

            payrollModel.validateCorp = validationWithAdjustment;

            List<ValidationAdjustment> adjustmentsList = new List<ValidationAdjustment>();

            foreach (var item in payrollModel.validate)
            {
                ValidationAdjustment adjustment = new ValidationAdjustment();

                var adjustmentDetails = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.location_id == item.loc_id && x.payroll_id == item.payroll_id).FirstOrDefault();

                if (adjustmentDetails != null)
                {
                    adjustment.emp_ID = item.employee_id.ToString();
                    adjustment.loc_ID = item.loc_id;

                    //Trace.WriteLine("empId = " + item.employee_id + " LocID = " + item.loc_id + " Prid " + item.payroll_id);

                    //Trace.WriteLine(adjustmentDetails.adjustmentPay_validate.HasValue + " apv");
                    //Trace.WriteLine(adjustmentDetails.adjustmentPay_validate1.HasValue + " apv1");
                    //Trace.WriteLine(adjustmentDetails.adjustmentCategory_validate + " acv");
                    //Trace.WriteLine(adjustmentDetails.adjustmentCategory_validate1 + " acv1");
                    //Trace.WriteLine(adjustmentDetails.plus_minus_validate + " pmv");
                    //Trace.WriteLine(adjustmentDetails.plus_minus_validate1 + " pmv1");

                    if (adjustmentDetails.adjustmentPay_validate.HasValue)
                    {
                        adjustment.Adjustment_pay = adjustmentDetails.adjustmentPay_validate.Value;
                    }
                    if (adjustmentDetails.adjustmentPay_validate1.HasValue)
                    {
                        adjustment.Adjustment_pay1 = adjustmentDetails.adjustmentPay_validate1.Value;
                    }
                    adjustment.Category = adjustmentDetails.adjustmentCategory_validate;
                    adjustment.Category1 = adjustmentDetails.adjustmentCategory_validate1;
                    adjustment.plusMinus = adjustmentDetails.plus_minus_validate;
                    adjustment.plusMinus1 = adjustmentDetails.plus_minus_validate1;

                    adjustmentsList.Add(adjustment);
                }

            }

            payrollModel.ValidationAdjustmentList = adjustmentsList;

            List<EmployeePayrollListModel> emplyPayrollListValidation = new List<EmployeePayrollListModel>();

            var empList = db.tbl_employee.Where(x => x.status == 1).ToList();

            foreach (var emp in empList)
            {
                var biweeklyvalidation = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == emp.employee_id && x.payroll_id == PayrollIDInt).ToList();

                if (biweeklyvalidation.Count() > 1)
                {

                    double regularTotal = 0;

                    var corpdata = db.tbl_employee_payroll_final.Where(x => x.employee_id == emp.employee_id && x.payroll_id == PayrollIDInt);

                    if (corpdata != null)
                    {
                        foreach (var recs in corpdata)
                        {
                            if (recs.RegularPay_H != null)
                            {
                                regularTotal = regularTotal + recs.RegularPay_H.Value;
                            }

                        }
                    }

                    //Trace.WriteLine(" This is the Total " + regularTotal);

                    EmployeePayrollListModel empvalid = new EmployeePayrollListModel();  
                    empvalid.employeeId = emp.employee_id.ToString();
                    empvalid.fullName = emp.full_name;
                    if (regularTotal <= 80)
                    {
                        empvalid.submissionStatus = "1";
                    }
                    else
                    {
                        empvalid.submissionStatus = "0";
                    }

                    empvalid.submissionStatusCorporate = emp.loc_ID;
                    emplyPayrollListValidation.Add(empvalid);
                }
            }

            payrollModel.employeeListValidation = emplyPayrollListValidation;

            List<EmployeePayrollListModel> emplyPayrollListChangeLocation = new List<EmployeePayrollListModel>();
            var itemslist = new List<int>();
            var Payroll = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == PayrollIDInt).FirstOrDefault();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct employee_id from tbl_attendance_log where time_stamp > '" + Payroll.start_date + "' and time_stamp < '" + Payroll.end_date + "' and loc_id = '" + location + "'";
                //Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string emp;
                            //string fulnam;
                            emp = Convert.ToString(sdr["employee_id"]);
                            //emp = sdr.GetDateTime(sdr.GetOrdinal("Timestamp"));
                            itemslist.Add(Convert.ToInt32(emp));
                        }

                    }
                    con.Close();
                }
            }

            var items1 = new List<KeyValuePair<int, string>>();
            foreach (var val in itemslist)
            {
                var EmpSwipedInLocation = db.tbl_employee.Where(x => x.employee_id == val).FirstOrDefault();

                if (EmpSwipedInLocation != null && emplyPayrollList.Where(x => x.employeeId == val.ToString()).Count() == 0 )
                {

                    if(EmpSwipedInLocation.status == 1)
                    {
                        items1.Add(new KeyValuePair<int, string>(Convert.ToInt32(val), EmpSwipedInLocation.full_name));
                    }
                    else
                    {
                        if(db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == val).Count() > 0)
                        {
                            items1.Add(new KeyValuePair<int, string>(Convert.ToInt32(val), EmpSwipedInLocation.full_name));
                        }
                    }

                    

                }

            }

            foreach (var item in items1)
            {
                EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                               //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                                                                               //string[] values = item.ToString().Split(';');
                obj.employeeId = item.Key.ToString();
                obj.fullName = item.Value;

                var empInBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == item.Key && x.payroll_id == PayrollIDInt && x.loc_id == location).FirstOrDefault();

                if (empInBiweekly != null)
                {
                    obj.submissionStatus = empInBiweekly.recordflag.ToString();
                }
                else
                {
                    obj.submissionStatus = "0";
                }

                if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.Key && x.payroll_id == PayrollIDInt && x.location_id == location).Select(x => x.recordFlag).FirstOrDefault() == 2)
                {
                    obj.submissionStatusCorporate = "2";
                }
                else
                {
                    obj.submissionStatusCorporate = "0";
                }

                emplyPayrollListChangeLocation.Add(obj);

            }

            
            if (payrollID == CurrentPayrollId.payroll_Id.ToString() || payrollID == null || payrollID == "") //payrollID != null && payrollID != ""
            {
                payrollModel.employeepayrollListChangeLocation = emplyPayrollListChangeLocation.OrderBy(x => x.fullName).ToList();
            }



            List<EmployeePayrollListModel> PrevemplyPayrollList = new List<EmployeePayrollListModel>();

            //var prev_emps = db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == locId && x.payroll_id == PayrollIDInt).Select(x => new { x.employee_id, x.recordflag }).ToList();
            
            var db1 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == locId && x.payroll_id == PayrollIDInt && x.recordflag == 2) select a).ToList(); // && x.recordflag == 2
            var db2 = (from a in db.tbl_employee select a).ToList();

            var prev_emps = (from a in db1
                             join b in db2 on a.employee_id equals b.employee_id
                             orderby b.full_name
                             select new { employee_id = a.employee_id, recordflag = a.recordflag }).ToList();

            foreach (var item in prev_emps)
            {
                EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                               //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                var empd = db.tbl_employee.Where(x => x.employee_id == item.employee_id).FirstOrDefault();
                obj.employeeId = item.employee_id.ToString();
                obj.fullName = empd.full_name;
                obj.submissionStatus = item.recordflag.ToString();
                int tempEmpID = Convert.ToInt32(obj.employeeId);

                var tempRegular = db.tbl_employee_payroll_submission.Where(x => (x.employee_id == tempEmpID && x.payroll_id == PayrollIDInt && x.location_id == locId)).FirstOrDefault();
                if (tempRegular != null)
                {
                    obj.submissionStatusCorporate = Convert.ToString(tempRegular.recordFlag);
                }
                PrevemplyPayrollList.Add(obj);

            }

            //Trace.WriteLine("This is the selected payroll ID" + PayrollIDInt + " and location ID is " + locId);

            List<tbl_payroll_submission_branch> branchSubmissionList = new List<tbl_payroll_submission_branch>();
            using (var context = new CityTireAndAutoEntities())
            {
                var rows = context.tbl_payroll_submission_branch.Where(x => (x.location_id == location && x.payroll_id == PayrollIDInt)).OrderBy(x => x.submission_date);
                foreach (var item in rows)
                {
                    CityTireAndAutoEntities OdContext = new CityTireAndAutoEntities();
                    tbl_payroll_submission_branch objCourse = new tbl_payroll_submission_branch();
                    objCourse.submitter_name = item.submitter_name;
                    objCourse.submission_date = item.submission_date;
                    branchSubmissionList.Add(objCourse);
                }
            }

            List<tbl_payroll_submission_corporate> corporateSubmissionList = new List<tbl_payroll_submission_corporate>();
            using (var context = new CityTireAndAutoEntities())
            {
                var rows = context.tbl_payroll_submission_corporate.Where(x => (x.payroll_id == PayrollIDInt)).OrderBy(x => x.submission_date);
                foreach (var item in rows)
                {
                    CityTireAndAutoEntities OdContext = new CityTireAndAutoEntities();
                    tbl_payroll_submission_corporate objCourse = new tbl_payroll_submission_corporate();
                    objCourse.submitter_name = item.submitter_name;
                    objCourse.submission_date = item.submission_date;
                    corporateSubmissionList.Add(objCourse);
                }
            }

            payrollModel.branchSubmission = branchSubmissionList;
            var locStatus = db.tbl_payroll_submission_branch.Where(x => x.location_id == location && x.payroll_id == PayrollIDInt).OrderBy(x => x.submission_date).Select(x => x.loc_status).FirstOrDefault();

            if (locStatus != null)
            {
                payrollModel.LockStatusLocation = locStatus.Value;
            }
            else
            {
                payrollModel.LockStatusLocation = 2;
            }

            payrollModel.corporateSubmission = corporateSubmissionList;

            tbl_employee_payroll_submission PayrollCorpSubmission = new tbl_employee_payroll_submission();

            if (employeeId != null)
            {
                int emid = Convert.ToInt32(employeeId);

                //Trace.WriteLine("This is the location" + location);

                var payrollSubmission = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emid && x.payroll_id == PayrollIDInt && x.location_id == location).FirstOrDefault();
                if (payrollSubmission != null)
                {
                    PayrollCorpSubmission.regular = payrollSubmission.regular;
                    PayrollCorpSubmission.ot = payrollSubmission.ot;
                    PayrollCorpSubmission.vacationTime = payrollSubmission.vacationTime;
                    PayrollCorpSubmission.sickTime = payrollSubmission.sickTime;
                    PayrollCorpSubmission.StatutoryTime = payrollSubmission.StatutoryTime;
                    PayrollCorpSubmission.commission = payrollSubmission.commission;
                    PayrollCorpSubmission.callCommission = payrollSubmission.callCommission;
                    PayrollCorpSubmission.otherPay = payrollSubmission.otherPay;
                    PayrollCorpSubmission.vacationPay = payrollSubmission.vacationPay;
                    PayrollCorpSubmission.PaidLeave = payrollSubmission.PaidLeave;
                    PayrollCorpSubmission.NonPaidLeave = payrollSubmission.NonPaidLeave;
                    PayrollCorpSubmission.compensation_type = payrollSubmission.compensation_type;
                    PayrollCorpSubmission.comments = payrollSubmission.comments;
                    PayrollCorpSubmission.plus_minus = payrollSubmission.plus_minus;
                    PayrollCorpSubmission.plus_minus1 = payrollSubmission.plus_minus1;
                    PayrollCorpSubmission.plus_minus2 = payrollSubmission.plus_minus2;
                    PayrollCorpSubmission.adjustmentPay = payrollSubmission.adjustmentPay;
                    PayrollCorpSubmission.adjustmentPay1 = payrollSubmission.adjustmentPay1;
                    PayrollCorpSubmission.adjustmentPay2 = payrollSubmission.adjustmentPay2;
                    if (payrollSubmission.adjustment_type != null)
                    {
                        PayrollCorpSubmission.adjustment_type = payrollSubmission.adjustment_type;
                    }
                    else
                    {
                        PayrollCorpSubmission.adjustment_type = "Select";
                    }


                    if (payrollSubmission.adjustment_type1 != null)
                    {
                        PayrollCorpSubmission.adjustment_type1 = payrollSubmission.adjustment_type1;
                    }
                    else
                    {
                        PayrollCorpSubmission.adjustment_type1 = "Select";
                    }

                    if (payrollSubmission.adjustment_type2 != null)
                    {
                        PayrollCorpSubmission.adjustment_type2 = payrollSubmission.adjustment_type2;
                    }
                    else
                    {
                        PayrollCorpSubmission.adjustment_type2 = "Select";
                    }

                    payrollModel.employeepayrollSubmission = PayrollCorpSubmission;
                }
            }

            emplyPayrollList = emplyPayrollList.OrderBy(e => e.fullName).ToList();


            
            if (payrollID == CurrentPayrollId.payroll_Id.ToString() || payrollID == null || payrollID == "") //payrollID != null && payrollID != ""
            {
                payrollModel.employeepayrollList = emplyPayrollList;
            }
            else if (payrollID != CurrentPayrollId.payroll_Id.ToString())
            {

                List<EmployeePayrollListModel> prevempListRefined = new List<EmployeePayrollListModel>();

                foreach (var emp in PrevemplyPayrollList)
                {
                    prevempListRefined.Add(emp);
                }

                payrollModel.employeepayrollList = prevempListRefined;
            }

            payrollModel.MatchedLocs = populateLocationsPermissions(empId);
            payrollModel.MatchedEmployees = populateEmployees();

            payrollModel.NewMatchedEmployees = NewpopulateEmployees(location);



            payrollModel.MatchedEmployeesChangeLoc = populateEmployeesChangeLoc(location, PayrollIDInt);


            List<PayrollCorporateDashboard> items = new List<PayrollCorporateDashboard>();

            foreach (var loc in payrollModel.MatchedLocs)
            {
                var branchSubmissionDetails = db.tbl_payroll_submission_branch.Where(x => x.location_id == loc.Value && x.payroll_id == PayrollIDInt).FirstOrDefault();
                var corpSubmissionDetails = db.tbl_payroll_submission_corporate.Where(x => x.payroll_id == PayrollIDInt).FirstOrDefault();

                var branchCompletionsAtLocation = db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == loc.Value && x.payroll_id == PayrollIDInt && x.recordflag == 2);
                var branchEmployeesAtLocation = db.tbl_employee.Where(x => x.loc_ID == loc.Value && x.status == 1);
                var corporateCompletionsAtLocation = db.tbl_employee_payroll_submission.Where(x => x.location_id == loc.Value && x.payroll_id == PayrollIDInt);

                int corpstatus = 0;

                if (corporateCompletionsAtLocation.Count() != 0)
                {
                    if (db.tbl_employee_payroll_submission.Where(x => x.location_id == loc.Value && x.payroll_id == PayrollIDInt && x.recordFlag == 2).Count() == corporateCompletionsAtLocation.Count())
                    {
                        corpstatus = 1;
                    }
                    //Trace.WriteLine(loc.Value + " has values in table which are of record flag 2 is " + db.tbl_employee_payroll_submission.Where(x => x.location_id == loc.Value && x.payroll_id == PayrollIDInt).Count() + "and" + corporateCompletionsAtLocation.Count());
                }


                if (branchSubmissionDetails != null)
                {
                    if (branchSubmissionDetails.resubmit == 0)
                    {
                        items.Add(new PayrollCorporateDashboard
                        {
                            locID = loc.Value,
                            branchSubmitter = branchSubmissionDetails.submitter_name,
                            branchSubmissionDate = Convert.ToString(branchSubmissionDetails.submission_date),
                            branchStatus = branchCompletionsAtLocation.Count(),
                            lockStatusbranch = branchSubmissionDetails.loc_status.Value,
                            corpSubmitter = "",
                            corpSubmissionDate = "",
                            corpStatus = corporateCompletionsAtLocation.Count(),
                            employeCountAtLocation = branchEmployeesAtLocation.Count(),
                            corpSaveStatus = corpstatus
                        });
                    }
                    else
                    {
                        items.Add(new PayrollCorporateDashboard
                        {
                            locID = loc.Value,
                            branchSubmitter = "",
                            branchSubmissionDate = "",
                            branchStatus = branchCompletionsAtLocation.Count(),
                            lockStatusbranch = branchSubmissionDetails.loc_status.Value,
                            corpSubmitter = "",
                            corpSubmissionDate = "",
                            corpStatus = corporateCompletionsAtLocation.Count(),
                            employeCountAtLocation = branchEmployeesAtLocation.Count(),
                            corpSaveStatus = corpstatus
                        });
                    }

                    
                }
                else
                {
                    items.Add(new PayrollCorporateDashboard
                    {
                        locID = loc.Value,
                        branchSubmitter = "",
                        branchSubmissionDate = "",
                        lockStatusbranch = 2,
                        branchStatus = branchCompletionsAtLocation.Count(),
                        corpSubmitter = "",
                        corpSubmissionDate = "",
                        corpStatus = corporateCompletionsAtLocation.Count(),
                        employeCountAtLocation = branchEmployeesAtLocation.Count(),
                        corpSaveStatus = corpstatus
                    });
                }
            }
            payrollModel.corpDashboard = items;
            payrollModel.PayrollIdList = populatePayrollId(employeeId, location);

            payrollModel.SelectedEmployeeId = employeeId;
            payrollModel.SelectedPayrollId = PayrollIDInt + ";" + employeeId + ";" + location;
            string EmployeeIdvalue = employeeId;

            if (employeeId is null || employeeId == "")
            {

            }
            else
            {
                int passThisValue = Convert.ToInt32(EmployeeIdvalue);
                var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == passThisValue).FirstOrDefault();
                string EmployeeID = "";
                string LocationID = "";
                string position = "";
                string FullName = "";
                byte[] profileImg = null;
                if (EmployeeDetails != null)
                {
                    EmployeeID = EmployeeDetails.employee_id.ToString();
                    FullName = EmployeeDetails.full_name;
                    LocationID = EmployeeDetails.loc_ID;
                    position = EmployeeDetails.cta_position;
                    profileImg = EmployeeDetails.profile_pic;
                }
                string base64ProfilePic = "";
                if (profileImg is null)
                {
                    base64ProfilePic = "";
                }
                else
                {
                    base64ProfilePic = Convert.ToBase64String(profileImg);
                }

                //Debug.WriteLine("Image_base64:" + base64ProfilePic);
                ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
                ViewData["EmployeeName"] = FullName;
                ViewData["EmployeeId"] = EmployeeID;
                ViewData["Position"] = position;

                var OT_eligible = db.tbl_position_info.Where(x => x.PositionTitle == position).FirstOrDefault();


                //Trace.WriteLine("This is the employee ID " + EmployeeDetails.employee_id);

                var emp_status_data = db.tbl_employee_status.Where(x => x.employee_id == EmployeeDetails.employee_id).FirstOrDefault();

                //Trace.Write("*******************Reached untill here ****************** empID =" + EmployeeDetails.employee_id);
                //Trace.Write("*******************Reached untill here ****************** empID =" + emp_status_data.vacation);


                ViewData["vacation"] = emp_status_data.vacation;

                ViewData["sick"] = emp_status_data.sick_days;

                ViewData["vacationbuyin"] = emp_status_data.vacation_buyin;

                ViewData["compensation_type"] = emp_status_data.compensation_type;

                if (OT_eligible != null)
                {
                    if (OT_eligible.OverTimeEligible == 1)
                    {
                        ViewData["OT_eligible"] = "OT";
                    }
                    else if (OT_eligible.OverTimeEligible == 0)
                    {
                        ViewData["OT_eligible"] = "OTN";
                    }
                }
                else
                {
                    ViewData["OT_eligible"] = "N/A";
                }

                //Trace.WriteLine("vacation " + emp_status_data.vacation);
                //Trace.WriteLine("vacationbuyin " + emp_status_data.vacation_buyin);
                //Trace.WriteLine("compensation_type " + emp_status_data.compensation_type);
                //DateTime todatDate = DateTime.Parse("2020-04-30");
                //Debug.WriteLine("Date time today:" + todatDate);
                int payrollIDInfo = 0;
                string payrollStartDate = "";
                string payrollEndDate = "";


                var checkCurrentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= CurrentDay && x.end_date >= CurrentDay).FirstOrDefault();
                payrollModel.currentPayRollId = checkCurrentPayrollID.payroll_Id;

                if (payrollID is null || payrollID == "")
                {
                    var payRollDates = db.tbl_employee_payroll_dates.Where(x => x.start_date <= CurrentDay && x.end_date >= CurrentDay).FirstOrDefault();
                    payrollIDInfo = payRollDates.payroll_Id;
                    payrollStartDate = payRollDates.start_date.ToString();
                    payrollEndDate = payRollDates.end_date.ToString();
                    payrollModel.SelectedPayrollId = PayrollIDInt + ";" + employeeId + ";" + location;
                }
                else
                {
                    int payIdNumber = Convert.ToInt32(payrollID);
                    var payRollDates = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == payIdNumber).FirstOrDefault();
                    payrollIDInfo = payRollDates.payroll_Id;
                    payrollStartDate = payRollDates.start_date.ToString();
                    payrollEndDate = payRollDates.end_date.ToString();
                    payrollModel.SelectedPayrollId = PayrollIDInt + ";" + employeeId + ";" + location;

                }

                //Debug.WriteLine("payRollDates Values:" + payrollIDInfo);
                LoadPayroll(EmployeeID, payrollStartDate, payrollEndDate, payrollIDInfo.ToString(), location);
                LoadLeaveHistory(EmployeeID);


                int EmployeeIdParam = Convert.ToInt32(EmployeeID);
                var payrollBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == EmployeeIdParam && x.payroll_id == payrollIDInfo && x.loc_id == location).FirstOrDefault(); // 

                var payrollSummary = db.tbl_employee_payroll_summary.Where(x => x.employee_id == EmployeeIdParam && x.payroll_id == payrollIDInfo && x.loc_id == location).FirstOrDefault();

                DateTime convStartDate1 = Convert.ToDateTime(payrollStartDate);

                DateTime endingDateis = convStartDate1.AddDays(14);

                var StDates = db.tbl_Calendar_events.Where(x => (x.end_date > convStartDate1 || x.end_date == convStartDate1) && (x.end_date < endingDateis || x.end_date == endingDateis)).ToList();

                int[] positionOfStatDays = new int[14];
                Array.Clear(positionOfStatDays, 0, positionOfStatDays.Length);

                foreach (var stdt in StDates)
                {
                    if (stdt.end_date.HasValue)
                    {
                        double days = (stdt.end_date.Value.Date - convStartDate1.Date).TotalDays;
                        int i = Convert.ToInt32(days);

                        positionOfStatDays[i] = 1;
                    }

                }

                ViewData["p0"] = positionOfStatDays[0];
                ViewData["p1"] = positionOfStatDays[1];
                ViewData["p2"] = positionOfStatDays[2];
                ViewData["p3"] = positionOfStatDays[3];
                ViewData["p4"] = positionOfStatDays[4];
                ViewData["p5"] = positionOfStatDays[5];
                ViewData["p6"] = positionOfStatDays[6];
                ViewData["p7"] = positionOfStatDays[7];
                ViewData["p8"] = positionOfStatDays[8];
                ViewData["p9"] = positionOfStatDays[9];
                ViewData["p10"] = positionOfStatDays[10];
                ViewData["p11"] = positionOfStatDays[11];
                ViewData["p12"] = positionOfStatDays[12];
                ViewData["p13"] = positionOfStatDays[13];

                PayRollViewModel payrollDetails = new PayRollViewModel();
                if (payrollBiweekly != null)
                {
                    EditPayrollBiWeeklyViewModel payrollBiWeekDetails = new EditPayrollBiWeeklyViewModel();
                    payrollBiWeekDetails.employee_id = payrollBiweekly.employee_id;
                    payrollBiWeekDetails.payroll_id = payrollBiweekly.payroll_id;

                    payrollBiWeekDetails.sat_1_reg = payrollBiweekly.sat_1_reg;
                    payrollBiWeekDetails.mon_1_reg = payrollBiweekly.mon_1_reg;
                    payrollBiWeekDetails.tues_1_reg = payrollBiweekly.tues_1_reg;
                    payrollBiWeekDetails.wed_1_reg = payrollBiweekly.wed_1_reg;
                    payrollBiWeekDetails.thurs_1_reg = payrollBiweekly.thurs_1_reg;
                    payrollBiWeekDetails.fri_1_reg = payrollBiweekly.fri_1_reg;
                    payrollBiWeekDetails.sat_2_reg = payrollBiweekly.sat_2_reg;
                    payrollBiWeekDetails.mon_2_reg = payrollBiweekly.mon_2_reg;
                    payrollBiWeekDetails.tues_2_reg = payrollBiweekly.tues_2_reg;
                    payrollBiWeekDetails.wed_2_reg = payrollBiweekly.wed_2_reg;
                    payrollBiWeekDetails.thurs_2_reg = payrollBiweekly.thurs_2_reg;
                    payrollBiWeekDetails.fri_2_reg = payrollBiweekly.fri_2_reg;


                    payrollBiWeekDetails.sat_1_sum = payrollBiweekly.sat_1_sum;
                    payrollBiWeekDetails.mon_1_sum = payrollBiweekly.mon__1_sum;
                    payrollBiWeekDetails.tues_1_sum = payrollBiweekly.tues_1_sum;
                    payrollBiWeekDetails.wed_1_sum = payrollBiweekly.wed_1_sum;
                    payrollBiWeekDetails.thurs_1_sum = payrollBiweekly.thurs_1_sum;
                    payrollBiWeekDetails.fri_1_sum = payrollBiweekly.fri_1_sum;
                    payrollBiWeekDetails.sat_2_sum = payrollBiweekly.sat_2_sum;
                    payrollBiWeekDetails.mon_2_sum = payrollBiweekly.mon_2_sum;
                    payrollBiWeekDetails.tues_2_sum = payrollBiweekly.tues_2_sum;
                    payrollBiWeekDetails.wed_2_Sum = payrollBiweekly.wed_2_Sum;
                    payrollBiWeekDetails.thurs_2_Sum = payrollBiweekly.thurs_2_Sum;
                    payrollBiWeekDetails.fri_2_sum = payrollBiweekly.fri_2_sum;
                    payrollBiWeekDetails.bi_week_chkin_avg = payrollBiweekly.bi_week_chkin_avg;
                    payrollBiWeekDetails.bi_week_chkout_avg = payrollBiweekly.bi_week_chkout_avg;
                    payrollBiWeekDetails.last_updated_by = payrollBiweekly.last_updated_by;
                    payrollBiWeekDetails.last_update_date = payrollBiweekly.last_update_date;
                    payrollBiWeekDetails.recordflag = payrollBiweekly.recordflag;
                    payrollBiWeekDetails.comments = payrollBiweekly.comments;
                    payrollBiWeekDetails.timeClock_sat1 = payrollBiweekly.timeClock_sat1;
                    payrollBiWeekDetails.timeClock_mon1 = payrollBiweekly.timeClock_mon1;
                    payrollBiWeekDetails.timeClock_tues1 = payrollBiweekly.timeClock_tues1;
                    payrollBiWeekDetails.timeClock_wed1 = payrollBiweekly.timeClock_wed1;
                    payrollBiWeekDetails.timeClock_thurs1 = payrollBiweekly.timeClock_thurs1;
                    payrollBiWeekDetails.timeClock_fri1 = payrollBiweekly.timeClock_fri1;
                    payrollBiWeekDetails.timeClock_sat2 = payrollBiweekly.timeClock_sat2;
                    payrollBiWeekDetails.timeClock_mon2 = payrollBiweekly.timeClock_mon2;
                    payrollBiWeekDetails.timeClock_tues2 = payrollBiweekly.timeClock_tues2;
                    payrollBiWeekDetails.timeClock_wed2 = payrollBiweekly.timeClock_wed2;
                    payrollBiWeekDetails.timeClock_thurs2 = payrollBiweekly.timeClock_thurs2;
                    payrollBiWeekDetails.timeClock_fri2 = payrollBiweekly.timeClock_fri2;
                    payrollBiWeekDetails.sun_1_reg = payrollBiweekly.sun_1_reg;
                    payrollBiWeekDetails.sun_2_reg = payrollBiweekly.sun_2_reg;

                    payrollBiWeekDetails.sun_1_sum = payrollBiweekly.sun_1_sum;
                    payrollBiWeekDetails.sun_2_sum = payrollBiweekly.sun_2_sum;
                    payrollBiWeekDetails.timeClock_sun1 = payrollBiweekly.timeClock_sun1;
                    payrollBiWeekDetails.timeClock_sun2 = payrollBiweekly.timeClock_sun2;

                    payrollBiWeekDetails.sun_1_sel = payrollBiweekly.sun_1_sel;
                    payrollBiWeekDetails.mon_1_sel = payrollBiweekly.mon_1_sel;
                    payrollBiWeekDetails.tues_1_sel = payrollBiweekly.tues_1_sel;
                    payrollBiWeekDetails.wed_1_sel = payrollBiweekly.wed_1_sel;
                    payrollBiWeekDetails.thurs_1_sel = payrollBiweekly.thurs_1_sel;
                    payrollBiWeekDetails.fri_1_sel = payrollBiweekly.fri_1_sel;
                    payrollBiWeekDetails.sat_1_sel = payrollBiweekly.sat_1_sel;
                    payrollBiWeekDetails.mon_2_sel = payrollBiweekly.mon_2_sel;
                    payrollBiWeekDetails.tues_2_sel = payrollBiweekly.tues_2_sel;
                    payrollBiWeekDetails.wed_2_sel = payrollBiweekly.wed_2_sel;
                    payrollBiWeekDetails.thurs_2_sel = payrollBiweekly.thurs_2_sel;
                    payrollBiWeekDetails.fri_2_sel = payrollBiweekly.fri_2_sel;
                    payrollBiWeekDetails.sat_2_sel = payrollBiweekly.sat_2_sel;

                    payrollBiWeekDetails.vacation = emp_status_data.vacation.Value;
                    payrollBiWeekDetails.vacation_buyin = emp_status_data.vacation_buyin.Value;
                    payrollBiWeekDetails.compensation_type = emp_status_data.compensation_type;

                    if (payrollBiweekly.sat_1_sel == " " || payrollBiweekly.sat_1_sel is null)
                    {
                        if (positionOfStatDays[6] == 1)
                        {
                            payrollBiWeekDetails.sat_1_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.sat_1_sel_id = 1;
                        }

                    }
                    else
                    {
                        var sat1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.sat_1_sel_id = sat1details.category_id;
                        
                    }

                    if (payrollBiweekly.sun_1_sel == " " || payrollBiweekly.sun_1_sel is null)
                    {
                        if (positionOfStatDays[0] == 1)
                        {
                            payrollBiWeekDetails.sun_1_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.sun_1_sel_id = 1;
                        }
                    }
                    else
                    {
                        var sun1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.sun_1_sel_id = sun1details.category_id;
                    }

                    if (payrollBiweekly.mon_1_sel == " " || payrollBiweekly.mon_1_sel is null)
                    {
                        if (positionOfStatDays[1] == 1)
                        {
                            payrollBiWeekDetails.mon_1_sel_id = 3;
                            //Trace.WriteLine("payrollBiWeekDetails.mon_1_sel_id: 1 " + payrollBiWeekDetails.mon_1_sel_id);
                        }
                        else
                        {
                            payrollBiWeekDetails.mon_1_sel_id = 1;
                            //Trace.WriteLine("payrollBiWeekDetails.mon_1_sel_id: 2 " + payrollBiWeekDetails.mon_1_sel_id);
                        }
                    }
                    else
                    {
                        var mon1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.mon_1_sel_id = mon1details.category_id;
                        
                    }

                    if (payrollBiweekly.tues_1_sel == " " || payrollBiweekly.tues_1_sel is null)
                    {
                        if (positionOfStatDays[2] == 1)
                        {
                            payrollBiWeekDetails.tues_1_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.tues_1_sel_id = 1;
                        }
                    }
                    else
                    {
                        var tues1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.tues_1_sel_id = tues1details.category_id;
                    }

                    if (payrollBiweekly.wed_1_sel == " " || payrollBiweekly.wed_1_sel is null)
                    {
                        if (positionOfStatDays[3] == 1)
                        {
                            payrollBiWeekDetails.wed_1_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.wed_1_sel_id = 1;
                        }
                    }
                    else
                    {
                        var wed1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.wed_1_sel_id = wed1details.category_id;
                    }

                    if (payrollBiweekly.thurs_1_sel == " " || payrollBiweekly.thurs_1_sel is null)
                    {
                        if (positionOfStatDays[4] == 1)
                        {
                            payrollBiWeekDetails.thurs_1_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.thurs_1_sel_id = 1;
                        }
                    }
                    else
                    {
                        var thurs1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.thurs_1_sel_id = thurs1details.category_id;
                    }

                    if (payrollBiweekly.fri_1_sel == " " || payrollBiweekly.fri_1_sel is null)
                    {
                        if (positionOfStatDays[5] == 1)
                        {
                            payrollBiWeekDetails.fri_1_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.fri_1_sel_id = 1;
                        }
                    }
                    else
                    {

                        var fri1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_1_sel).FirstOrDefault();
                        payrollBiWeekDetails.fri_1_sel_id = fri1details.category_id;
                    }

                    if (payrollBiweekly.sat_2_sel == " " || payrollBiweekly.sat_2_sel is null)
                    {
                        if (positionOfStatDays[13] == 1)
                        {
                            payrollBiWeekDetails.sat_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.sat_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var sat2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.sat_2_sel_id = sat2details.category_id;
                    }

                    if (payrollBiweekly.sun_2_sel == " " || payrollBiweekly.sun_2_sel is null)
                    {
                        if (positionOfStatDays[7] == 1)
                        {
                            payrollBiWeekDetails.sun_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.sun_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var sun2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.sun_2_sel_id = sun2details.category_id;
                    }

                    if (payrollBiweekly.mon_2_sel == " " || payrollBiweekly.mon_2_sel is null)
                    {
                        if (positionOfStatDays[8] == 1)
                        {
                            payrollBiWeekDetails.mon_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.mon_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var mon2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.mon_2_sel_id = mon2details.category_id;
                    }

                    if (payrollBiweekly.tues_2_sel == " " || payrollBiweekly.tues_2_sel is null)
                    {
                        if (positionOfStatDays[9] == 1)
                        {
                            payrollBiWeekDetails.tues_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.tues_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var tues2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.tues_2_sel_id = tues2details.category_id;
                    }

                    if (payrollBiweekly.wed_2_sel == " " || payrollBiweekly.wed_2_sel is null)
                    {
                        if (positionOfStatDays[10] == 1)
                        {
                            payrollBiWeekDetails.wed_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.wed_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var wed2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.wed_2_sel_id = wed2details.category_id;
                    }

                    if (payrollBiweekly.thurs_2_sel == " " || payrollBiweekly.thurs_2_sel is null)
                    {
                        if (positionOfStatDays[11] == 1)
                        {
                            payrollBiWeekDetails.thurs_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.thurs_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var thurs2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.thurs_2_sel_id = thurs2details.category_id;
                    }

                    if (payrollBiweekly.fri_2_sel == " " || payrollBiweekly.fri_2_sel is null)
                    {
                        if (positionOfStatDays[12] == 1)
                        {
                            payrollBiWeekDetails.fri_2_sel_id = 3;
                        }
                        else
                        {
                            payrollBiWeekDetails.fri_2_sel_id = 1;
                        }
                    }
                    else
                    {
                        var fri2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_2_sel).FirstOrDefault();
                        payrollBiWeekDetails.fri_2_sel_id = fri2details.category_id;
                    }




                    if (positionOfStatDays[0] == 1 || payrollBiWeekDetails.sun_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.sun_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.sun_1_opt = payrollBiweekly.sun_1_opt;
                            payrollBiWeekDetails.sun_1_sel = payrollBiweekly.sun_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.sun_1_opt = 8;
                            payrollBiWeekDetails.sun_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.sun_1_opt = payrollBiweekly.sun_1_opt;
                        payrollBiWeekDetails.sun_1_sel = payrollBiweekly.sun_1_sel;
                    }

                    if (positionOfStatDays[1] == 1 || payrollBiWeekDetails.mon_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.mon_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.mon_1_opt = payrollBiweekly.mon_1_opt;
                            payrollBiWeekDetails.mon_1_sel = payrollBiweekly.mon_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.mon_1_opt = 8;
                            payrollBiWeekDetails.mon_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.mon_1_opt = payrollBiweekly.mon_1_opt;
                        payrollBiWeekDetails.mon_1_sel = payrollBiweekly.mon_1_sel;
                    }

                    if (positionOfStatDays[2] == 1 || payrollBiWeekDetails.tues_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.tues_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.tues_1_opt = payrollBiweekly.tues_1_opt;
                            payrollBiWeekDetails.tues_1_sel = payrollBiweekly.tues_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.tues_1_opt = 8;
                            payrollBiWeekDetails.tues_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.tues_1_opt = payrollBiweekly.tues_1_opt;
                        payrollBiWeekDetails.tues_1_sel = payrollBiweekly.tues_1_sel;
                    }

                    if (positionOfStatDays[3] == 1 || payrollBiWeekDetails.wed_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.wed_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.wed_1_opt = payrollBiweekly.wed_1_opt;
                            payrollBiWeekDetails.wed_1_sel = payrollBiweekly.wed_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.wed_1_opt = 8;
                            payrollBiWeekDetails.wed_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.wed_1_opt = payrollBiweekly.wed_1_opt;
                        payrollBiWeekDetails.wed_1_sel = payrollBiweekly.wed_1_sel;
                    }

                    if (positionOfStatDays[4] == 1 || payrollBiWeekDetails.thurs_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.thurs_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.thurs_1_opt = payrollBiweekly.thurs_1_opt;
                            payrollBiWeekDetails.thurs_1_sel = payrollBiweekly.thurs_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.thurs_1_opt = 8;
                            payrollBiWeekDetails.thurs_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.thurs_1_opt = payrollBiweekly.thurs_1_opt;
                        payrollBiWeekDetails.thurs_1_sel = payrollBiweekly.thurs_1_sel;
                    }

                    //Trace.WriteLine(" Stat Position info " + positionOfStatDays[5] + " DB selection string " + payrollBiWeekDetails.fri_1_sel + " selection ID " + payrollBiWeekDetails.fri_1_sel_id);


                    if (positionOfStatDays[5] == 1 || payrollBiWeekDetails.fri_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.fri_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.fri_1_opt = payrollBiweekly.fri_1_opt;
                            payrollBiWeekDetails.fri_1_sel = payrollBiweekly.fri_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.fri_1_opt = 8;
                            payrollBiWeekDetails.fri_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.fri_1_opt = payrollBiweekly.fri_1_opt;
                        payrollBiWeekDetails.fri_1_sel = payrollBiweekly.fri_1_sel;
                    }

                    if (positionOfStatDays[6] == 1 || payrollBiWeekDetails.sat_1_sel == "St")
                    {
                        if (payrollBiWeekDetails.sat_1_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.sat_1_opt = payrollBiweekly.sat_1_opt;
                            payrollBiWeekDetails.sat_1_sel = payrollBiweekly.sat_1_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.sat_1_opt = 8;
                            payrollBiWeekDetails.sat_1_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.sat_1_opt = payrollBiweekly.sat_1_opt;
                        payrollBiWeekDetails.sat_1_sel = payrollBiweekly.sat_1_sel;
                    }

                    if (positionOfStatDays[7] == 1 || payrollBiWeekDetails.sun_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.sun_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.sun_2_opt = payrollBiweekly.sun_2_opt;
                            payrollBiWeekDetails.sun_2_sel = payrollBiweekly.sun_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.sun_2_opt = 8;
                            payrollBiWeekDetails.sun_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.sun_2_opt = payrollBiweekly.sun_2_opt;
                        payrollBiWeekDetails.sun_2_sel = payrollBiweekly.sun_2_sel;
                    }

                    if (positionOfStatDays[8] == 1 || payrollBiWeekDetails.mon_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.mon_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.mon_2_opt = payrollBiweekly.mon_2_opt;
                            payrollBiWeekDetails.mon_2_sel = payrollBiweekly.mon_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.mon_2_opt = 8;
                            payrollBiWeekDetails.mon_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.mon_2_opt = payrollBiweekly.mon_2_opt;
                        payrollBiWeekDetails.mon_2_sel = payrollBiweekly.mon_2_sel;
                    }

                    if (positionOfStatDays[9] == 1 || payrollBiWeekDetails.tues_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.tues_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.tues_2_opt = payrollBiweekly.tues_2_opt;
                            payrollBiWeekDetails.tues_2_sel = payrollBiweekly.tues_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.tues_2_opt = 8;
                            payrollBiWeekDetails.tues_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.tues_2_opt = payrollBiweekly.tues_2_opt;
                        payrollBiWeekDetails.tues_2_sel = payrollBiweekly.tues_2_sel;
                    }

                    if (positionOfStatDays[10] == 1 || payrollBiWeekDetails.wed_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.wed_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.wed_2_opt = payrollBiweekly.wed_2_opt;
                            payrollBiWeekDetails.wed_2_sel = payrollBiweekly.wed_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.wed_2_opt = 8;
                            payrollBiWeekDetails.wed_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.wed_2_opt = payrollBiweekly.wed_2_opt;
                        payrollBiWeekDetails.wed_2_sel = payrollBiweekly.wed_2_sel;
                    }

                    if (positionOfStatDays[11] == 1 || payrollBiWeekDetails.thurs_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.thurs_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.thurs_2_opt = payrollBiweekly.thurs_2_opt;
                            payrollBiWeekDetails.thurs_2_sel = payrollBiweekly.thurs_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.thurs_2_opt = 8;
                            payrollBiWeekDetails.thurs_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.thurs_2_opt = payrollBiweekly.thurs_2_opt;
                        payrollBiWeekDetails.thurs_2_sel = payrollBiweekly.thurs_2_sel;
                    }

                    if (positionOfStatDays[12] == 1 || payrollBiWeekDetails.fri_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.fri_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.fri_2_opt = payrollBiweekly.fri_2_opt;
                            payrollBiWeekDetails.fri_2_sel = payrollBiweekly.fri_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.fri_2_opt = 8;
                            payrollBiWeekDetails.fri_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.fri_2_opt = payrollBiweekly.fri_2_opt;
                        payrollBiWeekDetails.fri_2_sel = payrollBiweekly.fri_2_sel;
                    }

                    if (positionOfStatDays[13] == 1 || payrollBiWeekDetails.sat_2_sel == "St")
                    {
                        if (payrollBiWeekDetails.sat_2_sel != "St" && payrollBiWeekDetails.recordflag == 2)
                        {
                            payrollBiWeekDetails.sat_2_opt = payrollBiweekly.sat_2_opt;
                            payrollBiWeekDetails.sat_2_sel = payrollBiweekly.sat_2_sel;
                        }
                        else
                        {
                            payrollBiWeekDetails.sat_2_opt = 8;
                            payrollBiWeekDetails.sat_2_sel_id = 3;
                        }
                    }
                    else
                    {
                        payrollBiWeekDetails.sat_2_opt = payrollBiweekly.sat_2_opt;
                        payrollBiWeekDetails.sat_2_sel = payrollBiweekly.sat_2_sel;
                    }

                    payrollDetails.payBiweek = payrollBiWeekDetails;
                    payrollDetails.payBiweek.comments = payrollBiWeekDetails.comments;
                }
                else
                {

                }


                if (payrollSummary != null)
                {
                    payrollDetails.paySummary = payrollSummary;
                }

                var permission = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();


                if (ViewData["OT_eligible"].ToString() == "OT")//changed 2022-07-08
                {
                    var selectionList = (from a in db.tbl_payroll_category_selection select a).Where(x => x.category_id != 10).ToList();
                    ViewBag.TypeSelectionList = selectionList;
                }
                else
                {
                    var selectionList = (from a in db.tbl_payroll_category_selection select a).ToList();
                    ViewBag.TypeSelectionList = selectionList;
                }

                ViewBag.WeekendTypeSelectionList = (from a in db.tbl_payroll_category_selection select a).Where(x => x.category_id == 5 || x.category_id == 1).ToList();

                ViewBag.StatSelectionList = (from a in db.tbl_payroll_category_selection select a).Where(x => x.category_id == 3 || x.category_id == 1 || x.category_id == 8 || x.category_id == 6 || x.category_id == 9).ToList();
                
                ViewBag.TypeSelectionListNoOT = (from a in db.tbl_payroll_category_selection select a).Where(x => x.category != "OT").ToList();

                payrollModel.employeepayroll = payrollDetails;
            }

            var PaySet = db.tbl_payroll_settings.Where(x => x.ID == 1).FirstOrDefault();

            payrollModel.payrollSettings = Convert.ToInt32(PaySet.payroll_date);
            //Debug.WriteLine("The connection is" + payrollModel.payrollSettings);

            if (payrollID == null || payrollID == "")
            {
                payrollModel.SelectedPayrollId = PayrollIDInt + ";" + "" + ";" + "";
            }


            var tb1 = (from a in db.tbl_employee_payroll_final.Where(x => x.employee_id.ToString() == employeeId) select a).ToList();
            var tb2 = (from a in db.tbl_employee_payroll_dates select a).ToList();

            var Yearly = (from a in tb1
                             join b in tb2 on a.payroll_id equals b.payroll_Id 
                             select new { reg = a.RegularPay_H, Ot1 = a.OvertimePay_H, Ot2 = a.OvertimePay_2_H, Ot3 = a.OvertimePay_3_H, start = b.start_date, vac = a.VacationTime_H, sic = a.SickLeave_H }).ToList();

            var YearlyRefined = Yearly.Where(x => x.start.Value.Year == db.tbl_employee_payroll_dates.Where(y => y.payroll_Id == PayrollIDInt).Select(y => y.start_date).FirstOrDefault().Value.Year);

            if(YearlyRefined != null)
            {
                //Trace.WriteLine("This is the Regular " + Yearly.Sum(x => x.reg) + " This is the OT " + Yearly.Sum(x => x.Ot1) + " This is the OT2 " + Yearly.Sum(x => x.Ot2) + " This is the OT3 " + Yearly.Sum(x => x.Ot3));
                payrollModel.YearlyRegular = YearlyRefined.Sum(x => x.reg).Value;
                payrollModel.YearlyVac = YearlyRefined.Sum(x => x.vac).Value;
                payrollModel.YearlySic = YearlyRefined.Sum(x => x.sic).Value;
                payrollModel.YearlyOT = YearlyRefined.Sum(x => x.Ot1).Value + Yearly.Sum(x => x.Ot2).Value + Yearly.Sum(x => x.Ot3).Value;
            }

            //Trace.WriteLine("Reached1");
            List<KeyValuePair<DateTime, double?>> VacDates = new List<KeyValuePair<DateTime, double?>>();
            List<KeyValuePair<DateTime, double?>> SicDates = new List<KeyValuePair<DateTime, double?>>();

            List<int> vacPayrollIDs = db.tbl_employee_payroll_final.Where(x => x.employee_id.ToString() == employeeId && ((x.VacationTime_H != 0 && x.VacationTime_H != null) || (x.SickLeave_H != 0 && x.SickLeave_H != null))).Select(x => x.payroll_id).ToList(); ;
            //Trace.WriteLine("Reached2");
            if (vacPayrollIDs != null)
            {
                //Trace.WriteLine("Reached3");
                foreach (int pid in vacPayrollIDs)
                {
                    var biweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id == pid).FirstOrDefault();

                    if(biweekly != null)
                    {
                        DateTime payrollStartDate = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == pid).Select(x => x.start_date).FirstOrDefault().Value;
                        //Trace.WriteLine("Reached4");
                        if (biweekly.sun_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate, biweekly.sun_1_opt));
                        }
                        else if (biweekly.sun_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate, biweekly.sun_1_opt));
                        }

                        if (biweekly.mon_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(1), biweekly.mon_1_opt));
                        }
                        else if (biweekly.mon_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(1), biweekly.mon_1_opt));
                        }

                        if (biweekly.tues_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(2), biweekly.tues_1_opt));
                        }
                        else if (biweekly.tues_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(2), biweekly.tues_1_opt));
                        }

                        if (biweekly.wed_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(3), biweekly.wed_1_opt));
                        }
                        else if (biweekly.wed_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(3), biweekly.wed_1_opt));
                        }

                        if (biweekly.thurs_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(4), biweekly.thurs_1_opt));
                        }
                        else if (biweekly.thurs_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(4), biweekly.thurs_1_opt));
                        }

                        if (biweekly.fri_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(5), biweekly.fri_1_opt));
                        }
                        else if (biweekly.fri_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(5), biweekly.fri_1_opt));
                        }

                        if (biweekly.sat_1_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(6), biweekly.sat_1_opt));
                        }
                        else if (biweekly.sat_1_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(6), biweekly.sat_1_opt));
                        }

                        if (biweekly.sun_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(7), biweekly.sun_2_opt));
                        }
                        else if (biweekly.sun_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(7), biweekly.sun_2_opt));
                        }

                        if (biweekly.mon_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(8), biweekly.mon_2_opt));
                        }
                        else if (biweekly.mon_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(8), biweekly.mon_2_opt));
                        }

                        if (biweekly.tues_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(9), biweekly.tues_2_opt));
                        }
                        else if (biweekly.tues_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(9), biweekly.tues_2_opt));
                        }

                        if (biweekly.wed_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(10), biweekly.wed_2_opt));
                        }
                        else if (biweekly.wed_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(10), biweekly.wed_2_opt));
                        }
                        //Trace.WriteLine("Reached5");
                        if (biweekly.thurs_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(11), biweekly.thurs_2_opt));
                        }
                        else if (biweekly.thurs_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(11), biweekly.thurs_2_opt));
                        }
                        //Trace.WriteLine("Reached6");
                        if (biweekly.fri_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(12), biweekly.fri_2_opt));
                        }
                        else if (biweekly.fri_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(12), biweekly.fri_2_opt));
                        }
                        //Trace.WriteLine("Reached7");
                        if (biweekly.sat_2_sel == "Va")
                        {
                            VacDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(13), biweekly.sat_2_opt));
                        }
                        else if (biweekly.sat_2_sel == "Si")
                        {
                            SicDates.Add(new KeyValuePair<DateTime, double?>(payrollStartDate.AddDays(13), biweekly.sat_2_opt));
                        }
                        //Trace.WriteLine("Reached8");
                    }

                }
            }
            //Trace.WriteLine("Reached9");
            payrollModel.vacDatesYearly = VacDates;
            payrollModel.sicDatesYearly = SicDates;

            payrollModel.summary = db.tbl_employee_payroll_summary.Where(x => x.payroll_id == PayrollIDInt && x.loc_id == location).ToList();


            payrollModel.MatchedLocID = location;

            return View(payrollModel);
        }

        [HttpPost]
        public ActionResult AddDocumentRequest(HttpPostedFileBase Document, TrainingViewModel modal)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var DocumentName = Path.GetFileName(Document.FileName);


            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("jordan.blackwood@citytire.com", "IT Team")); //jordan.blackwood
            msg.From = new MailAddress("sehub@citytire.com", "Sehub");
            msg.Subject = "SEHUB Add certificate request";
            msg.Body = "Certificate Type :" + modal.ResourceType + "<br />Employee ID :" + modal.employee + "<br />Expiration Date :" + modal.expirationDate + "<br />Description :" + modal.Description;
            msg.IsBodyHtml = true;

            msg.Attachments.Add(new Attachment(Document.InputStream, DocumentName, "text/pdf"));

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sehub@citytire.com", "$3hub1977");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return RedirectToAction("Training");
        }

        public ActionResult populateEmployeeList(DateTime date, string loc)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            Trace.WriteLine("This is the date" + date + "This is the location" + loc);

            List<KeyValuePair<int, string>> keyValuePair = new List<KeyValuePair<int, string>>();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct a.employee_id, b.full_name, b.status from tbl_attendance_log a, tbl_employee b where a.loc_id = '" + loc + "' and a.employee_id = b.employee_id and time_stamp > '" + date + "' and time_stamp < '" + date.AddDays(28) + "'";
                //Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Trace.WriteLine("Reached till 2");
                            int emp;
                            string fulnam;
                            string status;

                            if (sdr["status"].ToString() == "1")
                            {
                                status = "Active";
                            }
                            else
                            {
                                status = "InActive";
                            }

                            emp = Convert.ToInt32(sdr["employee_id"]);
                            fulnam = Convert.ToString(sdr["full_name"]) + ";" + status;

                            //Trace.WriteLine("This is the emp ID " + emp + " This is the full name" + fulnam);

                            keyValuePair.Add(new KeyValuePair<int, string>(emp, fulnam));
                            //Trace.WriteLine("Reached till 3");
                        }

                    }
                    con.Close();
                }
            }

            return Json(keyValuePair.Select(x => new
            {
                value = x.Key,
                text = x.Value
            }).OrderBy(x => x.text).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadCSV(HttpPostedFileBase postedFile)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //Trace.WriteLine("The drag and drop function");

            string filePath = string.Empty;
            if (postedFile != null)
            {
                string path = Server.MapPath("~/Content/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);

                //Read the contents of CSV file.
                string csvData = System.IO.File.ReadAllText(filePath);

                string empids = "";

                string date1 = csvData.Substring(93, 10);
                string date2 = csvData.Substring(109, 10);

                //Trace.WriteLine(date1 + ";" + date2);

                DateTime datetime1 = DateTime.ParseExact(date1, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime datetime2 = DateTime.ParseExact(date2, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (db.tbl_employee_payroll_dates.Where(x => x.start_date == datetime1 && x.end_date == datetime2).Count() != 0)
                {
                    if (csvData.Contains("Commission Report for Technicians"))
                    {

                        foreach (string row in csvData.Split('\n'))
                        {

                            tbl_Efficiancy_Technician_Commissions sales = new tbl_Efficiancy_Technician_Commissions();
                            

                            if (row != ",,")
                            {
                                string[] cell = row.Split(',');
                                if (cell.Length > 6)
                                {
                                    if (cell[3] == "AUTOMOTIVE TECH")
                                    {
                                        if (cell[0].All(char.IsDigit))
                                        {
                                            sales.employee_id = Convert.ToInt32(cell[0]);
                                            sales.loc_id = db.tbl_employee.Where(x => x.employee_id == sales.employee_id).Select(x => x.loc_ID).FirstOrDefault();
                                            sales.payroll_id = db.tbl_employee_payroll_dates.Where(x => x.start_date == datetime1 && x.end_date == datetime2).Select(x => x.payroll_Id).FirstOrDefault().ToString();
                                            sales.door_rate = Convert.ToDouble(db.tbl_pricelist.Where(x => x.loc_id == sales.loc_id).Select(x => x.door_rate).FirstOrDefault());
                                            Trace.WriteLine(cell[9]);
                                            sales.commissionable_sales_due = Convert.ToDouble(cell[9].Replace(",", ""));
                                            sales.commission_plan = cell[3];
                                            sales.commissions = cell[10];
                                            db.tbl_Efficiancy_Technician_Commissions.Add(sales);


                                            db.SaveChanges();
                                        }
                                    }
                                    else if (cell[3].Contains("TIRE TECH"))
                                    {
                                        if (cell[0].All(char.IsDigit))
                                        {
                                            //Trace.WriteLine(cell[0] + "Test");
                                            sales.employee_id = Convert.ToInt32(cell[0]);
                                            sales.loc_id = db.tbl_employee.Where(x => x.employee_id == sales.employee_id).Select(x => x.loc_ID).FirstOrDefault();
                                            sales.payroll_id = db.tbl_employee_payroll_dates.Where(x => x.start_date == datetime1 && x.end_date == datetime2).Select(x => x.payroll_Id).FirstOrDefault().ToString();

                                            //Trace.WriteLine(Convert.ToDouble(db.tbl_pricelist.Where(x => x.loc_id == sales.loc_id).Select(x => x.door_rate).FirstOrDefault()));

                                            sales.door_rate = Convert.ToDouble(db.tbl_pricelist.Where(x => x.loc_id == sales.loc_id).Select(x => x.door_rate).FirstOrDefault());
                                            
                                            sales.commissionable_sales_due = Convert.ToDouble(cell[9].Replace(",", ""));
                                            sales.commission_plan = cell[3];
                                            sales.commissions = cell[10];
                                            db.tbl_Efficiancy_Technician_Commissions.Add(sales);

                                            db.SaveChanges();
                                        }
                                    }
                                }
                                //empids = empids + cell[0] + "\n";
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("DataResources", "Settings", new { ack = "incorrectReport" });
                    }
                }
                else
                {
                    return RedirectToAction("DataResources", "Settings", new { ack = "incorrectPayroll" });
                }

            }

            return RedirectToAction("DataResources", "Settings", new { ack = "DataImported" });
        }


        public ActionResult UploadTemperatureReading()
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var files = Directory.GetFiles("C:\\Users\\Sri Harsha\\Downloads");

            foreach (var file in files)
            {
                if (file.Contains(".csv"))
                {
                    string filePath = "C:\\Users\\Sri Harsha\\Downloads\\" + file.Split('\\')[file.Split('\\').Length-1];
                    Trace.WriteLine(filePath);
                    string csvData = System.IO.File.ReadAllText(filePath);
                    foreach (string row in csvData.Split('\n'))
                    {
                        if (row.Contains(System.DateTime.Today.Year.ToString()))
                        {
                            string[] cell = row.Split(',');
                            string[] dateTime = cell[0].Split(' ');
                            string[] date = dateTime[0].Split('-');
                            string[] time = dateTime[1].Split(':');
                            DateTime newDate = new DateTime(Int32.Parse(date[0]), Int32.Parse(date[1]), Int32.Parse(date[2]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));

                            if (db.tbl_data_logger_temperature.Count(x => x.timestamp == newDate) == 0)
                            {
                                tbl_data_logger_temperature record = new tbl_data_logger_temperature();
                                record.timestamp = newDate;
                                record.temperature = Convert.ToDouble(cell[1]);
                                db.tbl_data_logger_temperature.Add(record);
                                db.SaveChanges();
                            }
                        }

                    }
                }
            }


            return RedirectToAction("DataResources", "Settings");
        }



        /*
         
            public JsonResult GetAttendanceDates(string empid)
        {

            //Trace.WriteLine("This is the employee ID " + empid);

            var itemslist = new List<string>();
            var itemslist1 = new List<string>();

            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select time_stamp, loc_id from tbl_attendance_log where employee_id = '" + empid +"'" + "order by time_stamp asc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                string timeStamp;
                                string locID;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                locID = Convert.ToString(sdr["loc_id"]);
                                itemslist.Add(timeStamp.Split(' ')[0] + ";" + locID);
                            }

                        }
                        con.Close();
                    }
                }
            }

            var finalTimeStampList = itemslist.ToList();

            return new JsonResult { Data = finalTimeStampList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
             
             */

        public JsonResult GetAttendanceDates(string empid, int mon)
        {
            string startDate = System.DateTime.Today.Year.ToString() + "-" + mon.ToString() + "-" + "1";
            string endDate = System.DateTime.Today.Year.ToString() + "-" + mon.ToString() + "-" + System.DateTime.DaysInMonth(System.DateTime.Today.Year, mon) + " 23:00:00.000";

            var itemslist = new List<string>();

            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                //DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select time_stamp, event_id, loc_id from tbl_attendance_log where employee_id = '" + empid + "'" + " and time_stamp > '" + startDate + "'" + " and time_stamp < '" + endDate + "'" + " order by time_stamp asc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string timeStamp;
                                string locID;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                locID = Convert.ToString(sdr["loc_id"]);
                                itemslist.Add(timeStamp.Split(' ')[0] + ";" + locID);
                            }
                        }
                        con.Close();
                    }
                }
            }

            var finalTimeStampList = itemslist.Distinct().ToList();

            return new JsonResult { Data = finalTimeStampList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /*
         
            public JsonResult GetSwipDetails(string date, string empid)
        {
            //Trace.WriteLine("This is the date " + date + "This is the employee_id " + empid);
            //Trace.WriteLine("This is the employee ID " + empid);

            var itemslist = new List<string>();
            var itemslist1 = new List<string>();

            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");

                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select time_stamp, event_id from tbl_attendance_log where time_stamp > '" + date + "'" + "and time_stamp < '" + date + " 23:59:00.000" + "'" + "and employee_id = '" + empid + "'" + "and loc_id = '" + empid + "'" + "  order by time_stamp asc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                                string timeStamp;
                                string eventID;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                eventID = Convert.ToString(sdr["event_id"]);
                                itemslist.Add(timeStamp + ">" + eventID);
                            }

                        }
                        con.Close();
                    }
                }
            }

            var finalTimeStampList = itemslist.ToList();

            return new JsonResult { Data = finalTimeStampList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
             
             */

        public JsonResult GetSwipDetails(string date, string empid, string locid)
        {
            //Trace.WriteLine("This is the date " + date + "This is the employee_id " + empid, "This is the locid" + locid);
            //Trace.WriteLine("This is the employee ID " + empid);

            var itemslist = new List<string>();
            var itemslist1 = new List<string>();

            using (CityTireAndAutoEntities dc = new CityTireAndAutoEntities())
            {
                DateTime nDaysAgo = Convert.ToDateTime("2018-01-01");
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string query = "select time_stamp, event_id from tbl_attendance_log where time_stamp > '" + date + "'" + "and time_stamp < '" + date + " 23:59:00.000" + "'" + "and employee_id = '" + empid + "'" + "and loc_id = '" + locid + "'" + "  order by time_stamp asc";
                    //Debug.WriteLine(query);
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string timeStamp;
                                string eventID;
                                timeStamp = Convert.ToString(sdr["time_stamp"]);
                                eventID = Convert.ToString(sdr["event_id"]);
                                itemslist.Add(timeStamp + ">" + eventID);
                            }
                        }
                        con.Close();
                    }
                }
            }

            var finalTimeStampList = itemslist.ToList();

            return new JsonResult { Data = finalTimeStampList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult Attendance(string locId, string employeeId)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            AttendanceModel model = new AttendanceModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;

            if (model.SehubAccess.attendance == 0)
            {
                return RedirectToAction("AssetControl", "Management");
            }

            var locationDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            string location = "AHO";

            if (locationDetails != null)
            {
                location = locationDetails.loc_ID;
            }
            if (locId is null)
            {

            }
            else
            {
                location = locId;
            }

            DateTime current = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

            List<KeyValuePair<int, string>> keyValuePair = new List<KeyValuePair<int, string>>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct a.employee_id, b.full_name, b.status from tbl_attendance_log a, tbl_employee b where a.loc_id = '" + location + "'" + " and a.employee_id = b.employee_id and time_stamp > '" + current + "' and time_stamp < '" + current.AddDays(28) + "'";
                //Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            //Trace.WriteLine("Reached till 2");
                            int emp;
                            string fulnam;
                            string status;

                            if (sdr["status"].ToString() == "1")
                            {
                                status = "Active";
                            }
                            else
                            {
                                status = "InActive";
                            }

                            emp = Convert.ToInt32(sdr["employee_id"]);
                            fulnam = Convert.ToString(sdr["full_name"]) + ";" + status;

                            //Trace.WriteLine("This is the emp ID " + emp + " This is the full name" + fulnam);

                            keyValuePair.Add(new KeyValuePair<int, string>(emp, fulnam));
                            //Trace.WriteLine("Reached till 3");
                        }

                    }
                    con.Close();
                }
            }


            List<EmployeeAttendanceListModel> emplyAttList = new List<EmployeeAttendanceListModel>();
            foreach (var item in keyValuePair)
            {
                EmployeeAttendanceListModel obj = new EmployeeAttendanceListModel(); // ViewModel
                //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                var employee = db.tbl_employee.Where(x => x.employee_id == item.Key).FirstOrDefault();
                obj.employeeId = employee.employee_id.ToString();
                obj.fullName = employee.full_name;
                if (employee.status == 1)
                {
                    obj.atWork = "Active";
                }
                else
                {
                    obj.atWork = "InActive";
                }
                emplyAttList.Add(obj);
            }

            var itemslist = new List<int>();

            DateTime todayDate = DateTime.Today.AddDays(0); //-14

            var Payroll = db.tbl_employee_payroll_dates.Where(x => x.start_date <= todayDate && x.end_date >= todayDate).FirstOrDefault();

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct employee_id from tbl_attendance_log where time_stamp > '" + Payroll.start_date + "' and time_stamp < '" + Payroll.end_date + "' and loc_id = '" + location + "'";
                //Debug.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            string emp;
                            string fulnam;
                            emp = Convert.ToString(sdr["employee_id"]);

                            itemslist.Add(Convert.ToInt32(emp));
                        }

                    }
                    con.Close();
                }
            }

            List<EmployeeAttendanceListModel> empchlococatList = new List<EmployeeAttendanceListModel>();

            foreach (var val in itemslist)
            {
                //Trace.WriteLine(val);
                EmployeeAttendanceListModel empchlococat = new EmployeeAttendanceListModel();
                if (val != 0)
                {

                    var EmpSwipedInLocation = db.tbl_employee.Where(x => x.employee_id == val).FirstOrDefault();
                    if (EmpSwipedInLocation != null)
                    {
                        if (EmpSwipedInLocation.loc_ID.ToString() != location)
                        {
                            empchlococat.employeeId = EmpSwipedInLocation.employee_id.ToString();
                            empchlococat.fullName = EmpSwipedInLocation.full_name;
                            string loc;
                            string eventType;

                            using (SqlConnection con = new SqlConnection(constr))
                            {
                                string query = "select top 1 * from tbl_attendance_log where employee_id = '" + val + "' order by time_stamp desc ";
                                //Debug.WriteLine(query);
                                using (SqlCommand cmd = new SqlCommand(query))
                                {
                                    cmd.Connection = con;
                                    con.Open();
                                    using (SqlDataReader sdr = cmd.ExecuteReader())
                                    {
                                        while (sdr.Read())
                                        {
                                            loc = Convert.ToString(sdr["loc_id"]);
                                            eventType = Convert.ToString(sdr["event_id"]);

                                            if (loc == location && eventType == "clockIN")
                                            {
                                                empchlococat.atWork = "True";
                                            }
                                            else
                                            {
                                                empchlococat.atWork = "False";
                                            }

                                        }

                                    }
                                    con.Close();
                                }
                            }
                            empchlococatList.Add(empchlococat);
                        }
                    }
                }

            }

            //Trace.WriteLine("Reached");
            model.employeeListChangeLocation = empchlococatList;
            model.employeeList = emplyAttList.OrderBy(x => x.fullName).ToList();

            model.MatchedLocs = populateLocationsPermissions(empId);

            model.MatchedLocID = location;

            if (employeeId is null)
            {
                employeeId = keyValuePair.OrderBy(x => x.Value).Select(x => x.Key).FirstOrDefault().ToString();
            }

            //Trace.WriteLine("First Emp with emp ID" + employeeId);

            if (employeeId is null)
            {

            }
            else
            {
                if (employeeId != null || employeeId != "")
                {
                    model.SelectedEmployeeId = employeeId;
                    //Debug.WriteLine("In ShowEditCustInfo");
                    int employeeIdNum = Convert.ToInt32(employeeId);
                    var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == employeeIdNum).FirstOrDefault();
                    string empId1 = "";
                    int auto_emp_id = 0;
                    string locId1 = "";
                    string position = "";
                    string fullName = "";
                    byte[] profileImg = null;
                    if (EmployeeDetails != null)
                    {
                        empId1 = EmployeeDetails.employee_id.ToString();
                        auto_emp_id = EmployeeDetails.employee_id;
                        locId1 = EmployeeDetails.loc_ID.ToString();
                        fullName = EmployeeDetails.full_name;
                        position = EmployeeDetails.cta_position;
                        profileImg = EmployeeDetails.profile_pic;
                    }
                    string base64ProfilePic = "";
                    if (profileImg is null)
                    {
                        base64ProfilePic = "";
                    }
                    else
                    {
                        base64ProfilePic = Convert.ToBase64String(profileImg);
                    }

                    //Debug.WriteLine("Image_base64:" + base64ProfilePic);
                    ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
                    ViewData["EmployeeName"] = fullName;
                    ViewData["EmployeeId"] = empId1;
                    if (auto_emp_id != 0)
                    {
                        ViewData["employee_id"] = auto_emp_id.ToString();
                    }
                    ViewData["Position"] = position;
                }
            }

            return View(model);
        }


        private static List<SelectListItem> populateLocationsPermissions(int empId)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_cta_location_info.ToList();

            var sehubloc = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            if(sehubloc != null)
            {
                if (sehubloc.loc_001 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "001",
                        Value = "001"
                    });
                }
                if (sehubloc.loc_002 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "002",
                        Value = "002"
                    });
                }
                if (sehubloc.loc_003 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "003",
                        Value = "003"
                    });
                }
                if (sehubloc.loc_004 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "004",
                        Value = "004"
                    });
                }
                if (sehubloc.loc_005 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "005",
                        Value = "005"
                    });
                }
                if (sehubloc.loc_007 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "007",
                        Value = "007"
                    });
                }
                if (sehubloc.loc_009 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "009",
                        Value = "009"
                    });
                }
                if (sehubloc.loc_010 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "010",
                        Value = "010"
                    });
                }
                if (sehubloc.loc_011 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "011",
                        Value = "011"
                    });
                }
                if (sehubloc.loc_347 == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "347",
                        Value = "347"
                    });
                }
                if (sehubloc.loc_AHO == 1)
                {
                    items.Add(new SelectListItem
                    {
                        Text = "AHO",
                        Value = "AHO"
                    });
                }
            }
            else
            {
                items.Add(new SelectListItem
                {
                    Text = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault(),
                    Value = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault()
                });
            }

            return items;
        }

        [HttpGet]
        public ActionResult Dashboard(string loc, string year)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int empId = Convert.ToInt32(Session["userID"].ToString());
            FileURL FileUrl = new FileURL();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            
            DateTime CurrentDay = DateTime.Today.AddDays(-(db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value)); //-(14 + db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value)

            int CurrentPayrollId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= CurrentDay && x.end_date >= CurrentDay).Select(x => x.payroll_Id).FirstOrDefault();

            //Trace.WriteLine(CurrentPayrollId + "This is the current payroll");

            var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date <= CurrentDay && x.end_date >= CurrentDay).FirstOrDefault();

            if (year == null)
            {
                year = System.DateTime.Now.Year.ToString();
            }

            DateTime start;

            if (year == "2021")
            {
                start = db.tbl_employee_payroll_dates.Select(x => x.start_date).FirstOrDefault().Value;
            }
            else
            {
                start = Convert.ToDateTime(year + "-01-01");
            }

            string previousYear = (Convert.ToInt32(year) - 1).ToString();
            DateTime prev_start = Convert.ToDateTime(previousYear + "-01-01");
            DateTime prev_end = Convert.ToDateTime(previousYear + DateTime.Now.ToString("yyyy-MM-dd").Substring(4));

            DateTime end = Convert.ToDateTime(year + "-12-31");

            if (loc == null)
            {
                loc = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            if (loc == "007")
            {
                FileUrl.ytd_tech_minutes = db.tbl_employee_payroll_summary.Where(x => x.loc_id == "007" && x.payroll_id != CurrentPayrollId && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).Value + db.tbl_employee_payroll_summary.Where(x => x.loc_id == "007" && x.payroll_id != CurrentPayrollId && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).Value;
                
                FileUrl.ytd_tech_minutes_last_year = db.tbl_employee_payroll_summary.Where(x => x.loc_id == "007" && x.payroll_id != CurrentPayrollId && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).Value + db.tbl_employee_payroll_summary.Where(x => x.loc_id == "007" && x.payroll_id != CurrentPayrollId && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).Value;
                if (year == System.DateTime.Now.Year.ToString())
                {
                    DateTime anual_production_start = Convert.ToDateTime(year + "-01-01");
                    DateTime anual_production_end = System.DateTime.Now;
                    FileUrl.ytd_barcode = db.tbl_treadtracker_barcode.Where(x => x.final_inspection_date > start && x.final_inspection_result == "GOOD").Count();
                    FileUrl.ytd_barcode_prev_year = db.tbl_treadtracker_barcode.Where(x => x.final_inspection_date > start && x.final_inspection_result == "GOOD").Count();
                    
                    FileUrl.ytd_tech_production = db.tbl_treadtracker_barcode.Where(x => x.final_inspection_date > start && x.final_inspection_date < currentPayroll.start_date && x.final_inspection_result == "GOOD").Count();
                    FileUrl.anual_production = FileUrl.ytd_barcode*(360 / ((anual_production_end - anual_production_start).TotalDays));

                    var efficiency = (FileUrl.ytd_tech_minutes * 60) / FileUrl.ytd_tech_production;
                    FileUrl.comparison = FileUrl.ytd_barcode*(360 / ((anual_production_end - anual_production_start).TotalDays));
                }
                else
                {
                    DateTime anual_production_start = Convert.ToDateTime(year + "-01-01");
                    DateTime anual_production_end = Convert.ToDateTime(year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day); ;
                    FileUrl.ytd_barcode = db.tbl_treadtracker_barcode.Where(x => x.final_inspection_date > start && x.final_inspection_date < end && x.final_inspection_result == "GOOD").Count();
                    FileUrl.ytd_tech_production = FileUrl.ytd_barcode;
                    FileUrl.anual_production = FileUrl.ytd_barcode * (360 / ((anual_production_end - anual_production_start).TotalDays));
                }

            }

            List<TechnicianEfficiencyViewModel> techEffList = new List<TechnicianEfficiencyViewModel>();

            
            CurrentPayrollId = CurrentPayrollId - 1;

            var techniciensPayroll = db.tbl_employee_payroll_submission.Where(x => x.payroll_id != CurrentPayrollId+1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2)) && x.location_id == loc).Select(x => x.employee_id).Distinct();

            List<tbl_employee> techniciens = new List<tbl_employee>();

            foreach(var emp in techniciensPayroll)
            {
                if (db.tbl_employee.Where(x => x.employee_id == emp && x.status == 1).Count() > 0)
                {
                    techniciens.Add(db.tbl_employee.Where(x => x.employee_id == emp).FirstOrDefault());
                }
            }

            if (techniciens != null)
            {
                foreach (var item in techniciens)
                {

                    if(db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Count() > 0)
                    {
                        TechnicianEfficiencyViewModel technicianInfo = new TechnicianEfficiencyViewModel();

                        technicianInfo.Technician = item.full_name + " (" + item.employee_id + ")";
                        technicianInfo.TechClass = item.cta_position;
                        technicianInfo.Status = item.status.Value;
                        technicianInfo.CompensationPlan = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == item.employee_id && x.payroll_id != (CurrentPayrollId + 1).ToString()).Select(x => x.commission_plan).FirstOrDefault();

                        technicianInfo.CompensationType = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault();

                        string temp1 = "";
                        string temp2 = "";
                        string temp3 = "";
                        
                        if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == CurrentPayrollId).Select(x => x.regular).FirstOrDefault() != null)
                        {
                            technicianInfo.RegularHours = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == CurrentPayrollId).Select(x => x.regular).FirstOrDefault().ToString();

                        }
                        else
                        {
                            if (db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                            {
                                technicianInfo.RegularHours = db.tbl_employee_payroll_summary.Where(x => x.employee_id == item.employee_id && x.payroll_id == CurrentPayrollId).Select(x => x.regular).FirstOrDefault().ToString();


                            }
                            else
                            {
                                technicianInfo.RegularHours = "0";
                                
                            }
                        }
                        
                        if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == CurrentPayrollId).Select(x => x.ot).FirstOrDefault() != null)
                        {
                            technicianInfo.OtHours = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == CurrentPayrollId).Select(x => x.ot).FirstOrDefault().ToString();
                        }
                        else
                        {
                            if (db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                            {
                                technicianInfo.OtHours = db.tbl_employee_payroll_summary.Where(x => x.employee_id == item.employee_id && x.payroll_id == CurrentPayrollId).Select(x => x.ot).FirstOrDefault().ToString();
                            }
                            else
                            {
                                technicianInfo.OtHours = "0";
                            }
                            
                        }

                        

                        if (db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == item.employee_id && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).FirstOrDefault() != null)
                        {
                            technicianInfo.ComissionableSalesDue = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == item.employee_id && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.commissionable_sales_due).ToString();
                        }
                        else
                        {
                            technicianInfo.ComissionableSalesDue = "0";
                        }
                        
                        if (db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == item.employee_id && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).FirstOrDefault() != null)
                        {
                            temp3 = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == item.employee_id && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).OrderByDescending(x => x.commissionable_sales_due).Select(x => x.commissionable_sales_due).FirstOrDefault().ToString();
                        }
                        else
                        {
                            temp3 = "0";
                        }


                        if (db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                        {
                            temp1 = db.tbl_employee_payroll_summary.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId+1  && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).Value.ToString();
                        }
                        else
                        {
                            if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).HasValue)
                            {
                                temp1 = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).Value.ToString();
                            }
                            else
                            {
                                temp1 = "0";
                            }
                        }


                        if (db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                        {
                            temp2 = db.tbl_employee_payroll_summary.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).Value.ToString();
                        }
                        else
                        {
                            if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).HasValue)
                            {
                                temp2 = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).Value.ToString();
                            }
                            else
                            {
                                temp2 = "0";
                            }
                            
                        }

                        technicianInfo.BilledHoursYear = (Convert.ToDouble(temp1) + Convert.ToDouble(temp2)).ToString();
                        

                        if (db.tbl_pricelist.Where(x => x.loc_id == item.loc_ID).Select(x => x.door_rate).FirstOrDefault() != null)
                        {
                            technicianInfo.Doorrate = db.tbl_pricelist.Where(x => x.loc_id == item.loc_ID).Select(x => x.door_rate).FirstOrDefault().ToString();
                        }
                        else
                        {
                            technicianInfo.Doorrate = "0";
                        }

                        technicianInfo.BilledHoursPayroll = (Convert.ToDouble(temp3) / (0.38 * Convert.ToDouble(technicianInfo.Doorrate))).ToString();
                        
                        technicianInfo.OverallEfficiency = (Convert.ToDouble(technicianInfo.ComissionableSalesDue) / (0.38 * Convert.ToDouble(technicianInfo.Doorrate))).ToString();
                        


                        if (db.tbl_employee_payroll_submission.Where(x => x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Select(x => x.payroll_id).Distinct().Count() == 1)
                        {
                            technicianInfo.BilledHoursPayroll = technicianInfo.OverallEfficiency;
                            technicianInfo.RegularHours = temp1;
                            technicianInfo.OtHours = temp2;
                        }

                        techEffList.Add(technicianInfo);
                    }

                }
            }

            FileUrl.techefficiencyList = techEffList;

            
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            FileUrl.SehubAccess = empDetails;
            FileUrl.LocationsList = populateLocationsPermissions(empId);
            FileUrl.PayrollIdList = populateYears();
            FileUrl.Location_ID = loc;
            
            FileUrl.employeeVacList = db.tbl_employee.Where(x => x.status == 1 && x.loc_ID == FileUrl.Location_ID).ToList();

            if (FileUrl.SehubAccess.management == 0)
            {
                return RedirectToAction("Payroll", "Management");
            }

            if (FileUrl.SehubAccess.management_dashboard == 0)
            {
                return RedirectToAction("MyStaff", "Management");
            }

            String ContainerName = "new-hire-package";
            String ContainerName1 = "cta-library-management";
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            CloudBlobContainer container1 = blobClient.GetContainerReference(ContainerName1);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();
            var blockBlob1 = container1.ListBlobs();

            //var blobList = blockBlob.ToList();

            var URLNames = new List<KeyValuePair<string, string>>();

            //Trace.WriteLine(location);

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");

                //Trace.WriteLine(blobFileName);

                if (blobFileName.Contains(FileUrl.Location_ID)) // 
                {
                    URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
                }

            }

            foreach (var blob1 in blockBlob1)
            {
                var newUri1 = new Uri(blob1.Uri.AbsoluteUri);
                var blobFileName1 = blob1.Uri.Segments.Last();
                blobFileName1 = blobFileName1.Replace("%20", " ");
                blobFileName1 = blobFileName1.Replace(".pdf", " ");

                //Trace.WriteLine(blobFileName);

                if (blobFileName1.Contains("New Hire Package")) // 
                {
                    URLNames.Add(new KeyValuePair<string, string>(newUri1.ToString(), blobFileName1));
                }

            }

            FileUrl.URLName = URLNames;
            FileUrl.Year = year;
            FileUrl.commercialCustomerSurvey_link = db.tbl_cta_location_survey.Where(x => x.loc_id == FileUrl.Location_ID).Select(x => x.commercialCustomerSurvey_link).FirstOrDefault();
            
            var emplDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            FileUrl.employeeFullName = emplDetails.first_name + " " + emplDetails.last_name;
            FileUrl.employeePosition = emplDetails.cta_position;


            return View(FileUrl);
        }

        public string TechnicianEfficiency(int emp, string year)
        {
            Trace.WriteLine("Reached TechnicianEfficiency");
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            DateTime CurrentDay = DateTime.Today.AddDays(-(db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value)); //-(14 + db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value)

            int CurrentPayrollId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= CurrentDay && x.end_date >= CurrentDay).Select(x => x.payroll_Id).FirstOrDefault();

            string loc = db.tbl_employee.Where(x => x.employee_id == emp).Select(x => x.loc_ID).FirstOrDefault();
            
            if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Count() > 0)
            {
                TechnicianEfficiencyViewModel technicianInfo = new TechnicianEfficiencyViewModel();

                technicianInfo.CompensationPlan = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id != (CurrentPayrollId + 1).ToString()).Select(x => x.commission_plan).FirstOrDefault();

                technicianInfo.CompensationType = db.tbl_employee_status.Where(x => x.employee_id == emp).Select(x => x.compensation_type).FirstOrDefault();
                
                string temp1 = "";
                string temp2 = "";
                string temp3 = "";

                if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == CurrentPayrollId).Select(x => x.regular).FirstOrDefault() != null)
                {
                    technicianInfo.RegularHours = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == CurrentPayrollId).Select(x => x.regular).FirstOrDefault().ToString();

                }
                else
                {
                    if (db.tbl_employee_status.Where(x => x.employee_id == emp).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                    {
                        technicianInfo.RegularHours = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.payroll_id == CurrentPayrollId).Select(x => x.regular).FirstOrDefault().ToString();
                    }
                    else
                    {
                        technicianInfo.RegularHours = "0";

                    }
                }

                if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == CurrentPayrollId).Select(x => x.ot).FirstOrDefault() != null)
                {
                    technicianInfo.OtHours = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == CurrentPayrollId).Select(x => x.ot).FirstOrDefault().ToString();
                }
                else
                {
                    if (db.tbl_employee_status.Where(x => x.employee_id == emp).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                    {
                        technicianInfo.OtHours = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.payroll_id == CurrentPayrollId).Select(x => x.ot).FirstOrDefault().ToString();
                    }
                    else
                    {
                        technicianInfo.OtHours = "0";
                    }

                }



                if (db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).FirstOrDefault() != null)
                {
                    technicianInfo.ComissionableSalesDue = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.commissionable_sales_due).ToString();
                }
                else
                {
                    technicianInfo.ComissionableSalesDue = "0";
                }

                if (db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).FirstOrDefault() != null)
                {
                    temp3 = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id != (CurrentPayrollId + 1).ToString() && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).OrderByDescending(x => x.commissionable_sales_due).Select(x => x.commissionable_sales_due).FirstOrDefault().ToString();
                }
                else
                {
                    temp3 = "0";
                }


                if (db.tbl_employee_status.Where(x => x.employee_id == emp).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                {
                    temp1 = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).Value.ToString();
                }
                else
                {
                    if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).HasValue)
                    {
                        temp1 = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.regular).Value.ToString();
                    }
                    else
                    {
                        temp1 = "0";
                    }
                }


                if (db.tbl_employee_status.Where(x => x.employee_id == emp).Select(x => x.compensation_type).FirstOrDefault() == "Commission")
                {
                    temp2 = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).Value.ToString();
                }
                else
                {
                    if (db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).HasValue)
                    {
                        temp2 = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id != CurrentPayrollId + 1 && x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Sum(x => x.ot).Value.ToString();
                    }
                    else
                    {
                        temp2 = "0";
                    }

                }

                technicianInfo.BilledHoursYear = (Convert.ToDouble(temp1) + Convert.ToDouble(temp2)).ToString();


                if (db.tbl_pricelist.Where(x => x.loc_id == loc).Select(x => x.door_rate).FirstOrDefault() != null)
                {
                    technicianInfo.Doorrate = db.tbl_pricelist.Where(x => x.loc_id == loc).Select(x => x.door_rate).FirstOrDefault().ToString();
                }
                else
                {
                    technicianInfo.Doorrate = "0";
                }

                technicianInfo.BilledHoursPayroll = (Convert.ToDouble(temp3) / (0.38 * Convert.ToDouble(technicianInfo.Doorrate))).ToString();

                technicianInfo.OverallEfficiency = (Convert.ToDouble(technicianInfo.ComissionableSalesDue) / (0.38 * Convert.ToDouble(technicianInfo.Doorrate))).ToString();
                /*
                if (db.tbl_employee_payroll_submission.Where(x => x.payroll_id.ToString().StartsWith(year.Substring(year.Length - 2))).Select(x => x.payroll_id).Distinct().Count() == 1)
                {
                    
                }
                */

                technicianInfo.BilledHoursPayroll = technicianInfo.OverallEfficiency;
                technicianInfo.RegularHours = temp1;
                technicianInfo.OtHours = temp2;
                double ytdHrsBilled = 0;
                double prlHrsAdjusted = 0;
                double TechnicianEfficiency = 0;
                double paidhrspayroll = 0;
                if (technicianInfo.RegularHours != "" && technicianInfo.OtHours != "")
                {
                    paidhrspayroll = double.Parse(technicianInfo.RegularHours) + double.Parse(technicianInfo.OtHours);
                }
                double billedhrspayroll = Convert.ToDouble(technicianInfo.BilledHoursPayroll);
                double billedhrsytd = Convert.ToDouble(technicianInfo.OverallEfficiency);
                double payrollEfficiency = Convert.ToDouble(billedhrspayroll.ToString("n2")) * 100 / Convert.ToDouble(paidhrspayroll);
                double ytdEfficiency = Convert.ToDouble(billedhrsytd.ToString("n2")) * 100 / Convert.ToDouble(technicianInfo.BilledHoursYear);
                if (billedhrsytd != 0)
                {
                    return ytdEfficiency.ToString("n2");
                }
                else
                {
                    return "0";
                }

            }

            

            return "";
        }

        public string TechnicianEfficiencyPerPayroll(int emp, string year)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var payrollIDs = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id.ToString().StartsWith(year.Substring(2, 2))).Select(x => x.payroll_Id).ToList();
            string percentages = "";
            Trace.WriteLine("TechnicianEfficiencyPerPayroll");
            foreach (var pid in payrollIDs)
            {
                percentages = percentages + pid + ";";
            }
            Trace.WriteLine("TechnicianEfficiencyPerPayroll 1");
            percentages = percentages + "~";

            foreach (var pid in payrollIDs)
            {
                var commissionData = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id == pid.ToString()).FirstOrDefault();
                var payrollInfo = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == pid).FirstOrDefault();
                Trace.WriteLine("TechnicianEfficiencyPerPayroll 5");
                if (commissionData != null && payrollInfo != null)
                {
                    double regular = 0;
                    double OT = 0;
                    Trace.WriteLine("TechnicianEfficiencyPerPayroll 6");
                    double commissionDollars = commissionData.commissionable_sales_due.Value;
                    Trace.WriteLine("TechnicianEfficiencyPerPayroll 7");
                    double doorRate = commissionData.door_rate;
                    Trace.WriteLine("TechnicianEfficiencyPerPayroll 8");
                    if (payrollInfo.regular.HasValue)
                    {
                        regular = payrollInfo.regular.Value;
                    }

                    if (payrollInfo.ot.HasValue)
                    {
                        OT = payrollInfo.ot.Value;
                    }
                    Trace.WriteLine("TechnicianEfficiencyPerPayroll 9");
                     
                    Trace.WriteLine("TechnicianEfficiencyPerPayroll 10");
                    double eff = ((commissionDollars / (0.38 * doorRate)) / (regular + OT)) * 100;
                    Trace.WriteLine("TechnicianEfficiencyPerPayroll 11");
                    percentages = percentages + eff.ToString("N2") + ";";
                }
                else
                {
                    percentages = percentages + ";";
                }
            }

            return percentages;
        }

        private static List<SelectListItem> populateAutoTechnicians(string loc_id)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var emp = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.loc_id == loc_id).Select(x => x.employee_id).Distinct().ToList();

            items.Add(new SelectListItem
            {
                Text = "All",
                Value = "All",
            });

            foreach (var val in emp)
            {

                items.Add(new SelectListItem
                {
                    Text = db.tbl_employee.Where(x => x.employee_id == val).Select(x => x.full_name).FirstOrDefault(),
                    Value = val.ToString()
                });
            }
            return items;
        }

        public string TechnicianEfficiencyPerPayrollAll(string loc, string year, string tech)
        {
            Trace.WriteLine("This is " + tech);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var pids = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id.ToString().StartsWith(year.Substring(2, 2))).OrderBy(x => x.payroll_Id).Select(x => x.payroll_Id).ToList();
            List<int> payrollIDs = new List<int>();
            if (year == "2021")
            {
                payrollIDs.Add(2101);
                payrollIDs.Add(2102);
                payrollIDs.Add(2103);
                payrollIDs.Add(2104);
                payrollIDs.Add(2105);
                payrollIDs.Add(2106);
                payrollIDs.Add(2107);
                payrollIDs.Add(2108);
                payrollIDs.Add(2109);
                foreach (var pid in pids)
                {
                    payrollIDs.Add(pid);
                }
            }
            else
            {
                foreach (var pid in pids)
                {
                    payrollIDs.Add(pid);
                }
            }



            
            string percentages = "";
            foreach (var pid in payrollIDs)
            {
                percentages = percentages + pid + ";";
            }
            percentages = percentages + "~";

            

            List<tbl_sehub_color_scheme> efficiencyData = new List<tbl_sehub_color_scheme>();

            if (tech == "auto")
            {
                var empls = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.loc_id == loc && x.commission_plan == "AUTOMOTIVE TECH").Select(x => x.employee_id).Distinct().ToList();

                foreach (var emp in empls)
                {
                    if (db.tbl_employee.Where(x => x.employee_id == emp).Select(x => x.status).FirstOrDefault() == 1)
                    {
                        string percentString = "";
                        double avg = 0;
                        int count = 0;

                        foreach (var pid in payrollIDs)
                        {
                            var commissionData = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id == pid.ToString()).FirstOrDefault();
                            var payrollInfo = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == pid && x.location_id == loc).FirstOrDefault();
                            var payrollInfoBranch = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.payroll_id == pid && x.loc_id == loc).FirstOrDefault();
                            if (commissionData != null && payrollInfo != null)
                            {
                                double regular = 0;
                                double OT = 0;
                                double commissionDollars = Convert.ToDouble(commissionData.commissions);
                                double doorRate = commissionData.door_rate;
                                if (payrollInfo.regular.HasValue)
                                {
                                    regular = payrollInfo.regular.Value;
                                }
                                else if (payrollInfoBranch.regular.HasValue)
                                {
                                    if (payrollInfoBranch.regular != 0)
                                    {
                                        regular = payrollInfoBranch.regular.Value;
                                    }
                                    else
                                    {
                                        Trace.WriteLine("emp " + emp + " pid " + pid);
                                        regular = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.regular != null && x.regular != 0).Average(x => x.regular).Value;
                                    }

                                }
                                else
                                {
                                    Trace.WriteLine("emp " + emp + " pid " + pid);
                                    regular = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.regular != null && x.regular != 0).Average(x => x.regular).Value;
                                }

                                if (payrollInfo.ot.HasValue)
                                {
                                    OT = payrollInfo.ot.Value;
                                }
                                else
                                {
                                    OT = payrollInfoBranch.ot.Value;
                                }

                                double eff = ((commissionDollars / (0.38 * doorRate)) / (regular + OT)) * 100;
                                if (commissionDollars != 0)
                                {
                                    avg += eff;
                                    count++;
                                }
                                percentString = percentString + eff.ToString("N2") + ";";
                            }
                            else
                            {
                                percentString = percentString + ";";
                            }
                        }

                        tbl_sehub_color_scheme empData = new tbl_sehub_color_scheme();

                        empData.component = db.tbl_employee.Where(x => x.employee_id == emp).Select(x => x.full_name).FirstOrDefault() + " (" + (avg / count).ToString("N2") + "%)";
                        empData.color = percentString;
                        efficiencyData.Add(empData);
                    }
                    
                }

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return percentages + serializer.Serialize(efficiencyData);
            }
            else
            {
                var empls = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.loc_id == loc && (x.commission_plan == "TIRE TECH SHOP" || x.commission_plan == "TIRE TECH ROAD")).Select(x => x.employee_id).Distinct().ToList();

                foreach (var emp in empls)
                {
                    if (db.tbl_employee.Where(x => x.employee_id == emp).Select(x => x.status).FirstOrDefault() == 1)
                    {
                        string percentString = "";
                        double avg = 0;
                        int count = 0;

                        foreach (var pid in payrollIDs)
                        {
                            var commissionData = db.tbl_Efficiancy_Technician_Commissions.Where(x => x.employee_id == emp && x.payroll_id == pid.ToString()).FirstOrDefault();
                            var payrollInfo = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp && x.payroll_id == pid).FirstOrDefault();
                            var payrollInfoBranch = db.tbl_employee_payroll_summary.Where(x => x.employee_id == emp && x.payroll_id == pid && x.loc_id == loc).FirstOrDefault();
                            if (commissionData != null && payrollInfo != null)
                            {
                                double regular = 0;
                                double OT = 0;
                                double commissionDollars = Convert.ToDouble(commissionData.commissions);
                                double doorRate = commissionData.door_rate;
                                if (payrollInfo.regular.HasValue)
                                {
                                    regular = payrollInfo.regular.Value;
                                }
                                else
                                {
                                    regular = payrollInfoBranch.regular.Value;
                                }

                                if (payrollInfo.ot.HasValue)
                                {
                                    OT = payrollInfo.ot.Value;
                                }
                                else
                                {
                                    OT = payrollInfoBranch.ot.Value;
                                }

                                double eff = ((commissionDollars / (0.38 * doorRate)) / (regular + OT)) * 100;
                                if (commissionDollars != 0)
                                {
                                    avg += eff;
                                    count++;
                                }
                                percentString = percentString + eff.ToString("N2") + ";";
                                
                            }
                            else
                            {
                                percentString = percentString + ";";
                            }
                        }

                        tbl_sehub_color_scheme empData = new tbl_sehub_color_scheme();

                        empData.component = db.tbl_employee.Where(x => x.employee_id == emp).Select(x => x.full_name).FirstOrDefault() + " (" + (avg / count).ToString("N2") + "%)";
                        empData.color = percentString;
                        efficiencyData.Add(empData);
                    }
                    
                }

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return percentages + serializer.Serialize(efficiencyData);
            }

            
        }



        [HttpPost]
        public ActionResult DashboardChangeLocation(FileURL model)
        {
            return RedirectToAction("Dashboard", new { loc = model.Location_ID });
        }

        [HttpPost]
        public ActionResult Dashboard_manChangeLocation(FileURL model)
        {
            return RedirectToAction("Dashboard_man", new { loc_id = model.Location_ID });
        }

        [HttpGet]
        public ActionResult Dashboard_man(string loc_id)
        {
            FileURL model = new FileURL();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            model.LocationsList = populateLocationsIncludingAll();
            model.Positions = populateYears();
            model.Year = System.DateTime.Today.Year.ToString();
            if (loc_id != null)
            {
                model.Location_ID = loc_id;
                model.PayrollIdList = populateAutoTechnicians(loc_id);
            }
            else
            {
                model.Location_ID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
                model.PayrollIdList = populateAutoTechnicians(model.Location_ID);
            }            

            return View(model);
        }

        [HttpPost]
        public ActionResult Dashboard_man(FileURL model)
        {
            return RedirectToAction("Dashboard_man", new { loc_id = model.Location_ID });
        }



        [HttpGet]
        public string getSalesOfLocation(string loc, string yar)
        {
            loc = loc.TrimStart(new Char[] { '0' });
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            string currentYear = yar;
            string prevYear = (Convert.ToInt32(yar) - 1).ToString();
            var data = db.tbl_salesReport_branch_monthly.Where(x => x.loc_id == loc); 


            string janS = "";
            string febS = "";
            string marS = "";
            string aprS = "";
            string mayS = "";
            string junS = "";
            string julS = "";
            string augS = "";
            string sepS = "";
            string octS = "";
            string novS = "";
            string decS = "";

            string janSP = "";
            string febSP = "";
            string marSP = "";
            string aprSP = "";
            string maySP = "";
            string junSP = "";
            string julSP = "";
            string augSP = "";
            string sepSP = "";
            string octSP = "";
            string novSP = "";
            string decSP = "";



            var jan = data.Where(x => x.month == "January" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var feb = data.Where(x => x.month == "February" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var mar = data.Where(x => x.month == "March" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var apr = data.Where(x => x.month == "April" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var may = data.Where(x => x.month == "May" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var jun = data.Where(x => x.month == "June" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var jul = data.Where(x => x.month == "July" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var aug = data.Where(x => x.month == "August" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var sep = data.Where(x => x.month == "September" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var oct = data.Where(x => x.month == "October" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var nov = data.Where(x => x.month == "November" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();
            var dec = data.Where(x => x.month == "December" && x.year == currentYear).Select(x => x.total_sale).FirstOrDefault();

            var janP = data.Where(x => x.month == "January" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var febP = data.Where(x => x.month == "February" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var marP = data.Where(x => x.month == "March" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var aprP = data.Where(x => x.month == "April" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var mayP = data.Where(x => x.month == "May" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var junP = data.Where(x => x.month == "June" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var julP = data.Where(x => x.month == "July" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var augP = data.Where(x => x.month == "August" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var sepP = data.Where(x => x.month == "September" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var octP = data.Where(x => x.month == "October" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var novP = data.Where(x => x.month == "November" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();
            var decP = data.Where(x => x.month == "December" && x.year == prevYear).Select(x => x.total_sale).FirstOrDefault();


            if (jan != null)
            { 
                janS = jan.ToString();
            }
            if (feb != null)
            {
                febS = feb.ToString();
            }
            if (mar != null)
            {
                marS = mar.ToString();
            }
            if (apr != null)
            {
                aprS = apr.ToString();
            }
            if (may != null)
            {
                mayS = may.ToString();
            }
            if (jun != null)
            {
                junS = jun.ToString();
            }
            if (jul != null)
            {
                julS = jul.ToString();
            }
            if (aug != null)
            {
                augS = aug.ToString();
            }
            if (sep != null)
            {
                sepS = sep.ToString();
            }
            if (oct != null)
            {
                octS = oct.ToString();
            }
            if (nov != null)
            {
                novS = nov.ToString();
            }
            if (dec != null)
            {
                decS = dec.ToString();
            }

            if (janP != null)
            { 
                janSP = janP.ToString();
            }
            if (febP != null)
            {
                febSP = febP.ToString();
            }
            if (marP != null)
            {
                marSP = marP.ToString();
            }
            if (aprP != null)
            {
                aprSP = aprP.ToString();
            }
            if (mayP != null)
            {
                maySP = mayP.ToString();
            }
            if (junP != null)
            {
                junSP = junP.ToString();
            }
            if (julP != null)
            {
                julSP = julP.ToString();
            }
            if (augP != null)
            {
                augSP = augP.ToString();
            }
            if (sepP != null)
            {
                sepSP = sepP.ToString();
            }
            if (octP != null)
            {
                octSP = octP.ToString();
            }
            if (novP != null)
            {
                novSP = novP.ToString();
            }
            if (decP != null)
            {
                decSP = decP.ToString();
            }



            return janS + ";" + febS + ";" + marS + ";" + aprS + ";" + mayS + ";" + junS + ";" + julS + ";" + augS + ";" + sepS + ";" + octS + ";" + novS + ";" + decS + "~" + janSP + ";" + febSP + ";" + marSP + ";" + aprSP + ";" + maySP + ";" + junSP + ";" + julSP + ";" + augSP + ";" + sepSP + ";" + octSP + ";" + novSP + ";" + decSP;
        }



        [HttpPost]
        public ActionResult DashboardChangeYear(FileURL model)
        {
            return RedirectToAction("Dashboard", new { loc = model.Location_ID, year = model.Year });
        }

        public ActionResult SortBy(FileURL model)
        {
            return RedirectToAction("LeaveScheduler", "Management", new { SortBy = model.SortBy });
        }

        public ActionResult LeaveScheduler( string SortBy)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            FileURL model = new FileURL();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            model.SehubAccess = empDetails;
            model.employeeVacList = db.tbl_employee.ToList();
            model.SortBy = SortBy;

            //Trace.WriteLine("This is the sort " + model.SortBy);

            if(model.SortBy == null)
            {
                model.vacations = db.tbl_vacation_schedule.ToList();
            }
            else if (model.SortBy == "Start Date")
            {
                model.vacations = db.tbl_vacation_schedule.OrderBy (x => x.start_date).ToList();
            }
            else if (model.SortBy == "End Date")
            {
                model.vacations = db.tbl_vacation_schedule.OrderBy(x => x.end_date).ToList();
            }
            else
            {
                model.vacations = db.tbl_vacation_schedule.ToList();
            }
            

            model.Location_ID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            

            if (model.SehubAccess.vacation_schedule == 0)
            {
                return RedirectToAction("Payroll", "Management");
            }

            model.employee = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            model.LocationsList = populateLocationsPermissions(empId);

            return View(model);
        }

        public tbl_sehub_access CheckPermissions()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            return empDetails;
        }

        // GET: Management
        public ActionResult Index()
        {
            return View();
        }

        //[HttpGet]
        //public ActionResult AddEmployeePermisssions(string value)
        //{
        //    Debug.WriteLine("In AddEmployeePermisssions");
        //    AddNewPermissions obj = new AddNewPermissions();

        //    obj.MatchedLocsCred = populateLocations();
        //    obj.MatchedEmployeeNameCred = "Select";
        //    return PartialView(obj);
        //}
        //[HttpPost]
        //public ActionResult AddEmployeePermisssions(AddNewPermissions modal)
        //{
        //    int employeeId = Convert.ToInt32(modal.MatchedEmployeeIdCred);
        //    CityTireAndAutoEntities db = new CityTireAndAutoEntities();
        //    var employeeDetails = db.tbl_employee.Where(x => x.employee_id == employeeId).FirstOrDefault();
        //    if (employeeDetails != null)
        //    {
        //        employeeDetails.rfid_number = modal.RFIDCred;

        //    }
        //    modal.SehubAccess.employee_id = employeeId;
        //    db.tbl_sehub_access.Add(modal.SehubAccess);
        //    db.SaveChanges();
        //    return RedirectToAction("EmployeePermissions", new { locId = "" });
        //}

        [HttpPost]
        public string PayrollBranchTotals(string payrollID, string locationID)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //Trace.WriteLine("The PayrollID is " + payrollID + " and the locationID is " + locationID);

            int payID = Convert.ToInt32(payrollID);

            var db1 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == locationID && x.payroll_id == payID) select a).ToList();
            var db2 = (from a in db.tbl_employee_payroll_summary.Where(x => x.payroll_id == payID) select a).ToList();

            var branchDetails = (from a in db1
                             join b in db2 on a.employee_id equals b.employee_id
                             select new {
                                 regular = b.regular,
                                 overtime = b.ot,
                                 vacation = b.vac,
                                 sick = b.sick,
                                 statutory = b.stat,
                                 paidLeave = b.PL,
                                 nonPaidLeave = b.NL,
                                 breavement = b.brev,
                                 groupInsurance = b.gen_ins,
                                 employmentInsurance = b.emp_ins,
                                 workersCompensation = b.workers_comp
                             }).ToList();

            double[] individualTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            foreach(var item in branchDetails)
            {
                if(item.regular == null)
                {
                    individualTotals[0] = individualTotals[0] + 0;
                }
                else
                {
                    individualTotals[0] = individualTotals[0] + item.regular.Value;
                }
                
                if(item.overtime == null)
                {
                    individualTotals[1] = individualTotals[1] + 0;
                }
                else
                {
                    individualTotals[1] = individualTotals[1] + item.overtime.Value;
                }
                
                if(item.vacation == null)
                {
                    individualTotals[2] = individualTotals[2] + 0;
                }
                else
                {
                    individualTotals[2] = individualTotals[2] + item.vacation.Value;
                }

                if (item.sick == null)
                {
                    individualTotals[3] = individualTotals[3] + 0;
                }
                else
                {
                    individualTotals[3] = individualTotals[3] + item.sick.Value;
                }

                if (item.statutory == null)
                {
                    individualTotals[4] = individualTotals[4] + 0;
                }
                else
                {
                    individualTotals[4] = individualTotals[4] + item.statutory.Value;
                }

                if (item.paidLeave == null)
                {
                    individualTotals[5] = individualTotals[5] + 0;
                }
                else
                {
                    individualTotals[5] = individualTotals[5] + item.paidLeave.Value;
                }

                if (item.nonPaidLeave == null)
                {
                    individualTotals[6] = individualTotals[6] + 0;
                }
                else
                {
                    individualTotals[6] = individualTotals[6] + item.nonPaidLeave.Value;
                }

                if (item.breavement == null)
                {
                    individualTotals[7] = individualTotals[7] + 0;
                }
                else
                {
                    individualTotals[7] = individualTotals[7] + item.breavement.Value;
                }

                if (item.groupInsurance == null)
                {
                    individualTotals[8] = individualTotals[8] + 0;
                }
                else
                {
                    individualTotals[8] = individualTotals[8] + item.groupInsurance.Value;
                }

                if (item.employmentInsurance == null)
                {
                    individualTotals[9] = individualTotals[9] + 0;
                }
                else
                {
                    individualTotals[9] = individualTotals[9] + item.employmentInsurance.Value;
                }

                if (item.workersCompensation == null)
                {
                    individualTotals[10] = individualTotals[10] + 0;
                }
                else
                {
                    individualTotals[10] = individualTotals[10] + item.workersCompensation.Value;
                }
            }

            string combinedTotaalsString = individualTotals[0].ToString() + ";" + individualTotals[1].ToString() + ";" + individualTotals[2].ToString() + ";" + individualTotals[3].ToString() + ";" + individualTotals[4].ToString() + ";" + individualTotals[5].ToString() + ";" + individualTotals[6].ToString() + ";" + individualTotals[7].ToString() + ";" + individualTotals[8].ToString() + ";" + individualTotals[9].ToString() + ";" + individualTotals[10].ToString();

            return combinedTotaalsString;
        }

        public ActionResult NewLeave(FileURL modal)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            //Trace.WriteLine(modal.newLeave.start_date + " **** " + modal.newLeave.end_date + " **** "+ modal.newLeave.comments);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            tbl_vacation_schedule newLeave = new tbl_vacation_schedule();

            if (db.tbl_vacation_schedule.Where(x => x.empid == empId && x.start_date <= modal.newLeave.start_date && x.end_date >= modal.newLeave.start_date).Count() > 0)
            {

            }
            else
            {
                newLeave.empid = modal.employee.employee_id;
                newLeave.start_date = modal.newLeave.start_date;
                newLeave.end_date = modal.newLeave.end_date;
                newLeave.comments = modal.newLeave.comments;
                db.tbl_vacation_schedule.Add(newLeave);
                db.SaveChanges();
            }

            return RedirectToAction("LeaveScheduler", "Management");
        }

        public ActionResult EditLeave(FileURL modal)
        {
            //Trace.WriteLine(modal.newLeave.start_date + " **** " + modal.newLeave.end_date + " **** "+ modal.newLeave.comments);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //Trace.WriteLine("This is the employee ID " + modal.editVacationEmployee + " This is the start date " + modal.EditLeave.start_date);

            var editLeave = db.tbl_vacation_schedule.Where(x => x.empid == modal.editVacationEmployee && x.start_date == modal.EditLeave.start_date).FirstOrDefault();

            if (editLeave != null)
            {
                editLeave.start_date = modal.EditLeave.start_date;
                editLeave.end_date = modal.EditLeave.end_date;
                editLeave.leave_type = modal.EditLeave.leave_type;
                editLeave.comments = modal.EditLeave.comments;
            }

            db.SaveChanges();

            return RedirectToAction("LeaveScheduler", "Management");
        }

        public ActionResult DeleteLeave(FileURL modal)
        {

            //Trace.WriteLine(modal.newLeave.start_date + " **** " + modal.newLeave.end_date + " **** "+ modal.newLeave.comments);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //Trace.WriteLine("This is the employee ID " + modal.editVacationEmployee + " This is the start date " + modal.EditLeave.start_date);

            var editLeave = db.tbl_vacation_schedule.Where(x => x.empid == modal.editVacationEmployee && x.start_date == modal.EditLeave.start_date).FirstOrDefault();

            if (editLeave != null)
            {
                if (editLeave.start_date > System.DateTime.Today)
                {
                    db.tbl_vacation_schedule.Remove(editLeave);
                }
            }

            db.SaveChanges();

            return RedirectToAction("LeaveScheduler", "Management");
        }

        [HttpPost]
        public string PayrollCorporateTotals(string payrollID)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //Trace.WriteLine("The PayrollID is " + payrollID + " and the locationID is " + locationID);

            int payID = Convert.ToInt32(payrollID);

            var corporateDetails = db.tbl_employee_payroll_final.Where(x => x.payroll_id == payID);

            double[] individualTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            foreach (var item in corporateDetails)
            {
                if (item.CommissionPay_D == null)
                {
                    individualTotals[0] = individualTotals[0] + 0;
                }
                else
                {
                    individualTotals[0] = individualTotals[0] + item.CommissionPay_D.Value;
                }

                if (item.StatutoryHolidayPay_H == null)
                {
                    individualTotals[1] = individualTotals[1] + 0;
                }
                else
                {
                    individualTotals[1] = individualTotals[1] + item.StatutoryHolidayPay_H.Value;
                }

                if (item.SalaryPay_D == null)
                {
                    individualTotals[2] = individualTotals[2] + 0;
                }
                else
                {
                    individualTotals[2] = individualTotals[2] + item.SalaryPay_D.Value;
                }

                if (item.VacationPay_D == null)
                {
                    individualTotals[3] = individualTotals[3] + 0;
                }
                else
                {
                    individualTotals[3] = individualTotals[3] + item.VacationPay_D.Value;
                }

                if (item.RegularPay_H == null)
                {
                    individualTotals[4] = individualTotals[4] + 0;
                }
                else
                {
                    individualTotals[4] = individualTotals[4] + item.RegularPay_H.Value;
                }

                if (item.OvertimePay_H == null)
                {
                    individualTotals[5] = individualTotals[5] + 0;
                }
                else
                {
                    individualTotals[5] = individualTotals[5] + item.OvertimePay_H.Value;
                }

                if (item.OvertimePay_2_H == null)
                {
                    individualTotals[6] = individualTotals[6] + 0;
                }
                else
                {
                    individualTotals[6] = individualTotals[6] + item.OvertimePay_2_H.Value;
                }

                if (item.OtherPay_D == null)
                {
                    individualTotals[7] = individualTotals[7] + 0;
                }
                else
                {
                    individualTotals[7] = individualTotals[7] + item.OtherPay_D.Value;
                }

                if (item.SickLeave_H == null)
                {
                    individualTotals[8] = individualTotals[8] + 0;
                }
                else
                {
                    individualTotals[8] = individualTotals[8] + item.SickLeave_H.Value;
                }

                if (item.VacationTime_H == null)
                {
                    individualTotals[9] = individualTotals[9] + 0;
                }
                else
                {
                    individualTotals[9] = individualTotals[9] + item.VacationTime_H.Value;
                }

                if (item.OnCallCommission_D == null)
                {
                    individualTotals[10] = individualTotals[10] + 0;
                }
                else
                {
                    individualTotals[10] = individualTotals[10] + item.OnCallCommission_D.Value;
                }

                if (item.OvertimePay_3_H == null)
                {
                    individualTotals[11] = individualTotals[11] + 0;
                }
                else
                {
                    individualTotals[11] = individualTotals[11] + item.OvertimePay_3_H.Value;
                }
            }

            string combinedTotaalsString = individualTotals[0].ToString() + ";" + individualTotals[1].ToString() + ";" + individualTotals[2].ToString() + ";" + individualTotals[3].ToString() + ";" + individualTotals[4].ToString() + ";" + individualTotals[5].ToString() + ";" + individualTotals[6].ToString() + ";" + individualTotals[7].ToString() + ";" + individualTotals[8].ToString() + ";" + individualTotals[9].ToString() + ";" + individualTotals[10].ToString() + ";" + individualTotals[11].ToString();

            return combinedTotaalsString;
        }

        [HttpPost] 
        public ActionResult AssetControlChangeLocation(AssetControlViewModel model)
        {
            return RedirectToAction("AssetControl", new { loc = model.MatchedLoc });
        }

        [HttpPost]
        public ActionResult ValidateEmployeePayroll(EmployeePayrollModel model)
        {
            int payID = Convert.ToInt32(model.SelectedPayrollId.Split(';')[0]);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            foreach (var item in model.ValidationAdjustmentList) {
                //Trace.WriteLine(" This is the location ID - " + item.loc_ID);
                //Trace.WriteLine("This is the form submission adjustment pay - " + item.Adjustment_pay + " adjustment pay1 - " + item.Adjustment_pay1 + " Category - " + item.Category + " Category1 - " + item.Category1 + " plus minus " + item.plusMinus + " plus1 " + item.plusMinus1);
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                int empid = Convert.ToInt32(model.SelectedEmployeeId);

                var empdetailsValid = db.tbl_employee_payroll_submission.Where(x => x.employee_id == empid && x.location_id == item.loc_ID && x.payroll_id == payID).FirstOrDefault();

                double final_regular = empdetailsValid.regular.Value + checkValidateAdjustments(item.plusMinus, item.Adjustment_pay);
                double final_ot = empdetailsValid.ot.Value + checkValidateAdjustments(item.plusMinus1, item.Adjustment_pay1);

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE ");
                    sb.Append("tbl_employee_payroll_submission ");
                    sb.Append("SET adjustmentPay_validate = @apv, plus_minus_validate = @pmv, adjustmentPay_validate1 = @apv1, plus_minus_validate1 = @pmv1 ");
                    sb.Append("WHERE employee_id = @empID and location_id = @loc and payroll_id = @payID  ");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@empID", model.SelectedEmployeeId);
                        command.Parameters.AddWithValue("@loc", item.loc_ID);
                        command.Parameters.AddWithValue("@payID", payID);

                        command.Parameters.AddWithValue("@apv", item.Adjustment_pay);
                        command.Parameters.AddWithValue("@pmv", item.plusMinus);

                        command.Parameters.AddWithValue("@apv1", item.Adjustment_pay1);
                        command.Parameters.AddWithValue("@pmv1", item.plusMinus1);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();

                }

                if (db.tbl_employee_status.Where(x => x.employee_id == empid).Select(x => x.vacation).FirstOrDefault() == 15){
                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();

                        sb.Append("UPDATE ");
                        sb.Append("tbl_employee_payroll_final ");
                        sb.Append("SET RegularPay_H = @reg, OvertimePay_3_H = @ot, OvertimePay_H = 0 ");
                        sb.Append("WHERE employee_id = @empID and location_id = @loc and payroll_id = @payID  ");

                        string sql = sb.ToString();

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@empID", model.SelectedEmployeeId);
                            command.Parameters.AddWithValue("@loc", item.loc_ID);
                            command.Parameters.AddWithValue("@payID", payID);

                            command.Parameters.AddWithValue("@reg", final_regular);
                            command.Parameters.AddWithValue("@ot", final_ot);

                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        connection.Close();

                    }
                }
                else if(db.tbl_employee_status.Where(x => x.employee_id == empid).Select(x => x.vacation).FirstOrDefault() == 10)
                {
                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();

                        sb.Append("UPDATE ");
                        sb.Append("tbl_employee_payroll_final ");
                        sb.Append("SET RegularPay_H = @reg, OvertimePay_2_H = @ot, OvertimePay_H = 0 ");
                        sb.Append("WHERE employee_id = @empID and location_id = @loc and payroll_id = @payID  ");

                        string sql = sb.ToString();

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@empID", model.SelectedEmployeeId);
                            command.Parameters.AddWithValue("@loc", item.loc_ID);
                            command.Parameters.AddWithValue("@payID", payID);

                            command.Parameters.AddWithValue("@reg", final_regular);
                            command.Parameters.AddWithValue("@ot", final_ot);

                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        connection.Close();

                    }
                }

                

            }
            return RedirectToAction("Payroll", "Management", new { locId = model.MatchedLocID, employeeId = model.SelectedEmployeeId, payrollID = payID, ac = "Validate" });
        }

        public double checkValidateAdjustments(string pm, double adj)
        {
            double finalAdjustment = 0;

            if (pm == "-")
            {
                finalAdjustment = finalAdjustment - adj;
            }
            if (pm == "+")
            {
                finalAdjustment = finalAdjustment + adj;
            }

            return finalAdjustment;
        }

        [HttpGet]
        public ActionResult AssetControl(string loc)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            AssetControlViewModel Asset = new AssetControlViewModel();

            //Debug.WriteLine("This is the asset control location that was change" + loc);

            tbl_sehub_access Access = new tbl_sehub_access();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());



            Access = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            if (Access.asset_control == 0)
            {
                return RedirectToAction("LeaveScheduler", "Management");
            }

            if (loc != null)
            {
                Asset.MatchedLoc = loc;
            }
            else
            {
                var emp = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
                Asset.MatchedLoc = emp.loc_ID;
            }

            ViewData["management_dashboard"] = Access.management_dashboard;
            ViewData["management_mystaff"] = Access.my_staff;
            ViewData["management_attendance"] = Access.attendance;
            ViewData["management_assetControl"] = Access.asset_control;
            ViewData["management_vacationScheduler"] = Access.vacation_schedule;
            ViewData["main"] = Access.main;
            ViewData["library"] = Access.library_access;
            ViewData["treadTracker"] = Access.treadTracker;
            ViewData["fleetTVT"] = Access.fleetTVT;
            ViewData["payroll"] = Access.payroll;
            ViewData["settings"] = Access.settings;
            ViewData["tools"] = Access.tools;
            ViewData["plant"] = Access.plant;
            ViewData["employee_folder"] = Access.employee_folder;
            


            
            int permissions = CheckPermissions().asset_control.Value;
            ViewData["AssetAccessLevel"] = permissions;
            //Debug.WriteLine("In AssetControl");

            //Debug.WriteLine("empId:" + empId);
            var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();
            string locationid = "";
            if (result != null)
            {
                locationid = result.loc_ID;
            }
            else
            {
                locationid = "";
            }

            List<tbl_vehicle_info> VehicleDetails = new List<tbl_vehicle_info>();

            VehicleDetails = db.tbl_vehicle_info.OrderBy(x => x.vehicle_short_id).ToList();

            Asset.LocationList = populateLocationsPermissions(empId);

            if (VehicleDetails != null)
            {
                Asset.VehicalInfoList = VehicleDetails;
                //Debug.WriteLine("Vehicle info there are details");
                return View(Asset);
            }
            else
            {
                //Debug.WriteLine("Vehicle info empty");
                return View();
            }

        }

        [HttpGet]
        public ActionResult AddNewVehicle(string value)
        {

            //Debug.WriteLine("In AddNewVehicle");

            AddNewVehicleViewModel obj = new AddNewVehicleViewModel();
            obj.MatchedLocs = populateLocations();
            obj.MatchedEmployeeName = "Branch";
            return PartialView(obj);
        }

        [HttpGet]
        public ActionResult EditVehicleInfo(string value)
        {

            //Debug.WriteLine("In EditVehicleInfo:" + value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var vehicleInfoObj = db.tbl_vehicle_info.Where(x => x.VIN == value).FirstOrDefault();
            string employeeNameValue = "";
            if (vehicleInfoObj.assigned_to is null)
            {
                //Do Nothing
            }
            else
            {
                if (vehicleInfoObj.assigned_to != 0)
                {
                    var employeeTableCheck = db.tbl_employee.Where(x => x.employee_id == vehicleInfoObj.assigned_to).FirstOrDefault();
                    employeeNameValue = employeeTableCheck.full_name;
                }

            }

            AddNewVehicleViewModel obj = new AddNewVehicleViewModel();
            obj.VehicleInfo = vehicleInfoObj;
            obj.MatchedLocs = populateLocations();
            obj.MatchedLocID = vehicleInfoObj.loc_id;
            if (vehicleInfoObj.vehicle_status == 0)
            {
                obj.VehicleStatus = false;
            }
            else if (vehicleInfoObj.vehicle_status > 0) {
                obj.VehicleStatus = true;
            }
            

            //Debug.WriteLine("Full Name:" + employeeNameValue);

            obj.MatchedEmployeeName = employeeNameValue;
            //Debug.WriteLine("In EditVehicleInfo date:" + vehicleInfoObj.inspection_due_date);
            return PartialView(obj);
        }

        public JsonResult GetEmployeeList(string locationId)
        {

            //Debug.WriteLine("In GetEmployeeId requested location:" + locationId);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<tbl_employee> empList = db.tbl_employee.Where(x => x.loc_ID == locationId && x.status == 1).OrderBy(x => x.full_name).ToList();
            JsonResult result = Json(empList, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = 8675309;
            return result;
            //return Json(empList, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetUnAssignedEmployeeCredentials(string locationId)
        {

            //Debug.WriteLine("In GetUnAssignedEmployeeCredentials:" + locationId);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select * FRom tbl_employee where employee_id not in (select employee_id from tbl_employee_credentials) and loc_ID=" + locationId + " and status=1";
                //Debug.WriteLine("Query:" + query);
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
                                Text = sdr["employee_id"].ToString(),
                                Value = sdr["full_name"].ToString()
                            });
                        }

                    }
                    con.Close();
                }
            }
            //var UnAssinedCredentials = (from emp in db.tbl_employee

            //                            join cred in db.tbl_employee_credentials

            //                            on emp.employee_id equals cred.employee_id into empValues

            //                            from ed in empValues.DefaultIfEmpty()

            //                            select new

            //                            {

            //                                emp.employee_id,
            //                                emp.full_name,
            //                                emp.status

            //                            }).ToList();

            List<tbl_employee> emplyAttList = new List<tbl_employee>();
            foreach (var item in items)
            {
                tbl_employee emp_obj = new tbl_employee();


                //Debug.WriteLine("item.employee_id:" + item.Text);
                emp_obj.employee_id = Convert.ToInt32(item.Text);
                emp_obj.full_name = item.Value;
                emplyAttList.Add(emp_obj);


            }
            JsonResult result = Json(emplyAttList, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = 8675309;
            return result;
            //return Json(empList, JsonRequestBehavior.AllowGet);

        }

        private static List<SelectListItem> populateLocations()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select loc_id From tbl_cta_location_info where loc_status=1";
                //Debug.WriteLine("Query:" + query);
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
                                Text = sdr["loc_id"].ToString(),
                                Value = sdr["loc_id"].ToString()
                            });
                        }


                    }
                    con.Close();
                }
            }

            return items;
        }

        private static List<SelectListItem> populateEmployees()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var emp = db.tbl_employee.Where(x => x.status == 1).ToList();

            foreach (var val in emp)
            {
                items.Add(new SelectListItem
                {
                    Text = val.full_name,
                    Value = Convert.ToString(val.employee_id)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateLocationsIncludingAll()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            

            var locs = db.tbl_cta_location_info.Where(x => x.loc_status == 1).ToList();

            foreach (var loc in locs)
            {
                items.Add(new SelectListItem
                {
                    Text = loc.loc_id,
                    Value = loc.loc_id,
                });
            }
            return items;
        }



        private static List<SelectListItem> populateYears()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var emp = db.tbl_employee.Where(x => x.status == 1).ToList();

            int year = DateTime.Now.Year;

            for (int i = 2021 ; i <= year ; i++)
            {
                items.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });

            }

            return items;
        }

        private static List<SelectListItem> NewpopulateEmployees(string location)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var emp = db.tbl_employee.Where(x => x.status == 1).OrderBy(x => x.full_name).ToList(); // && x.loc_ID != location
            foreach (var val in emp)
            {
                items.Add(new SelectListItem
                {
                    Text = val.full_name,
                    Value = Convert.ToString(val.employee_id)
                });
            }
            return items;
        }

        private static List<SelectListItem> populateEmployeesChangeLoc(string location, int PayrollIDInt)
        {

            List<int> items = new List<int>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var Payroll = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == PayrollIDInt).FirstOrDefault();

            
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select distinct employee_id from tbl_attendance_log where time_stamp > '" + Payroll.start_date + "' and time_stamp < '" + Payroll.end_date + "' and loc_id = '" + location + "'";
                //Trace.WriteLine(query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            string emp;
                            emp = Convert.ToString(sdr["employee_id"]);

                            items.Add(Convert.ToInt32(emp));
                        }

                    }
                    con.Close();
                }
            }


            List<SelectListItem> items1 = new List<SelectListItem>();
            foreach (var val in items)
            {
                var EmpSwipedInLocation = db.tbl_employee.Where(x => x.employee_id == val).FirstOrDefault();

                if (EmpSwipedInLocation != null && EmpSwipedInLocation.loc_ID != location)
                {
                    items1.Add(new SelectListItem
                    {
                        Text = EmpSwipedInLocation.full_name,
                        Value = Convert.ToString(val)
                    });
                }

            }
            return items1;
        }

        private static List<SelectListItem> populatePayrollId(string empId, string locId)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            List<SelectListItem> items = new List<SelectListItem>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string todatDate = DateTime.Now.AddDays(-(db.tbl_payroll_settings.Where(x => x.ID == 1).Select(x => x.payroll_submission).FirstOrDefault().Value)).ToString("yyyy-MM-dd");
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT * FRom tbl_employee_payroll_dates where start_date <='" + todatDate + "' order by payroll_id desc";
                //Debug.WriteLine("Query:" + query);
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {

                            string start;
                            string end;

                            start = Convert.ToDateTime(sdr["start_date"].ToString()).ToString("MMM-dd");
                            end = Convert.ToDateTime(sdr["end_date"].ToString()).ToString("MMM-dd");

                            items.Add(new SelectListItem
                            {
                                Text = "20" + sdr["payroll_id"].ToString().Substring(0, 2) + "-" + sdr["payroll_id"].ToString().Substring(sdr["payroll_id"].ToString().Length - 2) + "   " + start + " to " + end,
                                Value = sdr["payroll_id"].ToString() + ";" + empId + ";" + locId
                            });
                        }


                    }
                    con.Close();
                }
            }

            return items;
        }

        [HttpPost]
        public ActionResult AddNewVehicle(AddNewVehicleViewModel model, HttpPostedFileBase vehicleImage)
        {
            

            byte[] imageBytes = null;
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(vehicleImage.FileName);
                //Debug.WriteLine("vehicleImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;
                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
                using (Image image = Image.FromStream(vehicleImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        //Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }
            model.VehicleInfo.assigned_to = Convert.ToInt32(model.MatchedEmployeeID);
            model.VehicleInfo.loc_id = model.MatchedLocID.ToString();
            model.VehicleInfo.vehicle_status = 1;
            model.VehicleInfo.vehicle_image = imageBytes;
            model.VehicleInfo.current_milage = 0;
            model.VehicleInfo.efficiency_liter = 0;
            model.VehicleInfo.efficiency_price = 0;
            string vehicleFirst3 = model.VehicleInfo.vehicle_long_id.ToString().Substring(0, 3);
            string vehicleLast4 = model.VehicleInfo.vehicle_long_id.ToString().Substring(model.VehicleInfo.vehicle_long_id.ToString().Length - 4);
            model.VehicleInfo.vehicle_short_id = vehicleFirst3 + "-" + vehicleLast4;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            db.tbl_vehicle_info.Add(model.VehicleInfo);

            db.SaveChanges();


            return RedirectToAction("AssetControl");
        }

        [HttpPost]
        public ActionResult EditVehicleInfo(AddNewVehicleViewModel model, HttpPostedFileBase vehicleImage)
        {


            byte[] imageBytes = null;
            if (vehicleImage != null && vehicleImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(vehicleImage.FileName);
                //Debug.WriteLine("vehicleImage:" + imageName);
                //string fileName = "C:/Users/mahes/Videos/Docs/Upload This/Resources/sidebar-04/js/" + imageName;
                //var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                //file.SaveAs(path);
                using (Image image = Image.FromStream(vehicleImage.InputStream, true, true))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        //Debug.WriteLine("Image base64:" + base64String);
                    }
                }
            }

            string vehicleFirst3 = model.VehicleInfo.vehicle_long_id.ToString().Substring(0, 3);
            string vehicleLast4 = model.VehicleInfo.vehicle_long_id.ToString().Substring(model.VehicleInfo.vehicle_long_id.ToString().Length - 4);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var result = db.tbl_vehicle_info.Where(a => a.VIN == model.VehicleInfo.VIN).FirstOrDefault();
            if (result != null)
            {
                result.vehicle_long_id = model.VehicleInfo.vehicle_long_id;
                result.vehicle_short_id = vehicleFirst3 + "-" + vehicleLast4;
                result.vehicle_plate = model.VehicleInfo.vehicle_plate;
                result.loc_id = model.MatchedLocID.ToString();
                //if(model.MatchedEmployeeName is null )
                //{
                //    Debug.WriteLine("Level 1:"+ model.MatchedEmployeeName);
                //    if(Convert.ToInt32(model.MatchedEmployeeID) != 0)
                //    {
                //        Debug.WriteLine("Level 2:"+ Convert.ToInt32(model.MatchedEmployeeID));
                //        result.assigned_to = Convert.ToInt32(model.MatchedEmployeeID);
                //    }

                //}               
                //Debug.WriteLine("Level 2:" + Convert.ToInt32(model.MatchedEmployeeID));
                if (Convert.ToInt32(model.MatchedEmployeeID) != 0)
                {

                    result.assigned_to = Convert.ToInt32(model.MatchedEmployeeID);
                }

                result.vehicle_year = model.VehicleInfo.vehicle_year;
                result.vehicle_manufacturer = model.VehicleInfo.vehicle_manufacturer;
                result.vehicle_model = model.VehicleInfo.vehicle_model;
                result.inspection_due_date = model.VehicleInfo.inspection_due_date;
                if (imageBytes != null)
                {
                    result.vehicle_image = imageBytes;
                }

                if(model.VehicleStatus)
                {
                    result.vehicle_status = 1;
                }
                else
                {
                    result.vehicle_status = 0;
                }

            }

            db.SaveChanges();
            return RedirectToAction("AssetControl");
        }

        [HttpGet]
        public ActionResult AddStaff()
        {
            AddNewEmployeeViewModel model = new AddNewEmployeeViewModel();
            model.MatchedLocs = populateLocations();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddStaff(AddNewEmployeeViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_employee empdetails = new tbl_employee();
            empdetails = model.StaffWorkDetails;
            empdetails.full_name = model.StaffWorkDetails.last_name + "," + model.StaffWorkDetails.first_name;
            empdetails.status = 1;
            empdetails.pic_status = 0;
            tbl_employee_personal personalDetails = new tbl_employee_personal();
            personalDetails = model.StaffPersonalDetails;
            personalDetails.employee_id = empdetails.employee_id;

            db.tbl_employee.Add(empdetails);
            db.tbl_employee_personal.Add(personalDetails);
            db.SaveChanges();
            return RedirectToAction("MyStaff", new { LocId = "" });
        }

        protected Image Resize(Image img, int resizedW, int resizedH)
        {
            Bitmap bmp = new Bitmap(resizedW, resizedH);
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.DrawImage(img, 0, 0, resizedW, resizedH);
            graphic.Dispose();
            return (Image)bmp;
        }

        [HttpPost]
        public ActionResult AddEmployee(MyStaffViewModel model, HttpPostedFileBase EmployeeImage)
        {

            byte[] imageBytes = null;
            if (EmployeeImage != null && EmployeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(EmployeeImage.FileName);
                using (Image image = Image.FromStream(EmployeeImage.InputStream, true, true))
                {
                    double height = 170 * image.Height / image.Width;
                    Image img = Resize(image, 170, (int)Math.Round(height));

                    using (MemoryStream m = new MemoryStream())
                    {
                        img.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_employee empdetails = new tbl_employee();

            tbl_employee_status empStatus = new tbl_employee_status();
            tbl_employee_attendance empAttend = new tbl_employee_attendance();

            empAttend.employee_id = model.NewEmployee.employee_id;
            //empAttend.at_work_location = model.NewEmployee.loc_ID;

            empStatus.employee_id = model.NewEmployee.employee_id;
            //empStatus.status = "Active";
            empStatus.date_of_joining = DateTime.Today;
            empStatus.date_of_leaving = null;
            empStatus.vacation = 10;
            empStatus.vacation_buyin = 0;
            empStatus.sick_days = 5;
            empStatus.compensation_type = "Hourly";

            db.tbl_employee_status.Add(empStatus);

            empdetails = model.NewEmployee;
            empdetails.full_name = model.NewEmployee.last_name + ", " + model.NewEmployee.first_name;
            empdetails.status = 1;
            empdetails.pic_status = 0;
            empdetails.profile_pic = imageBytes;
            tbl_employee_personal personalDetails = new tbl_employee_personal();
            personalDetails = model.NewEmployeePersonal;
            personalDetails.employee_id = empdetails.employee_id;

            db.tbl_employee_attendance.Add(empAttend);
            db.tbl_employee.Add(empdetails);
            db.tbl_employee_personal.Add(personalDetails);



            db.SaveChanges();
            return RedirectToAction("MyStaff", new { LocId = "" });
        }

        [HttpGet]
        public ActionResult EditEmployeeInfo(string value)
        {
            //Debug.WriteLine("Inside EditEmployeeInfo:" + value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            MyStaffViewModel model = new MyStaffViewModel();
            int empId = Convert.ToInt32(value);
            var employeeInfoObj = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();
            var employeePersonalInfoObj = db.tbl_employee_personal.Where(x => x.employee_id == empId).FirstOrDefault();
            var employee_start_date = db.tbl_employee_status.Where(x => x.employee_id == empId).FirstOrDefault();

            int empIdUser = Convert.ToInt32(Session["userID"].ToString());
            model.SehubAccess = db.tbl_sehub_access.Where(x => x.employee_id == empIdUser).FirstOrDefault();

            if (employeeInfoObj is null)
            {
                //Do Nothing
            }
            else
            {

                model.NewEmployee = employeeInfoObj;
                if (employeeInfoObj.rfid_number == null)
                {
                    model.NewEmployee.rfid_number = "No RFID";
                }
                else
                {
                    model.NewEmployee.rfid_number = "RFID Paired";
                }
                
                if (employeeInfoObj.status == 1)
                {
                    model.active_status = true;

                }
                else
                {
                    model.active_status = false;
                }
            }
            if (employeePersonalInfoObj is null)
            {

            }
            else
            {
                model.NewEmployeePersonal = employeePersonalInfoObj;
                if (employee_start_date.date_of_joining.HasValue)
                {
                    model.NewEmployeePersonal.employee_start_date = employee_start_date.date_of_joining.Value.ToString("MMMM dd, yyyy"); //public string employee_start_date { get; set; }
                }
            }

            model.CompensationType = db.tbl_employee_status.Where(x => x.employee_id == empId).Select(x => x.compensation_type).FirstOrDefault();

            model.MatchedStaffLocs = populateLocations();
            model.Positions = populatePositions();


            //obj.MatchedLocs = populateLocations();
            //obj.MatchedLocID = vehicleInfoObj.loc_id;        
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult EditEmployeeInfo(MyStaffViewModel model, HttpPostedFileBase EmployeeImage)
        {

            byte[] imageBytes = null;
            if (EmployeeImage != null && EmployeeImage.ContentLength > 0)
            {
                var imageName = Path.GetFileName(EmployeeImage.FileName);
                using (Image image = Image.FromStream(EmployeeImage.InputStream, true, true))
                {
                    double height = 170 * image.Height / image.Width;
                    Image img = Resize(image, 170, (int)Math.Round(height));

                    using (MemoryStream m = new MemoryStream())
                    {
                        img.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();
                    }
                }
            }

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeInfo = db.tbl_employee.Where(a => a.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();
            var PersonalDetails = db.tbl_employee_personal.Where(a => a.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();
            var rfid_info = db.tbl_employee_attendance.Where(a => a.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();
            var statusInfo = db.tbl_employee_status.Where(a => a.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();

            if (EmployeeInfo != null)
            {
                EmployeeInfo.first_name = model.NewEmployee.first_name;
                EmployeeInfo.middle_initial = model.NewEmployee.middle_initial;
                EmployeeInfo.last_name = model.NewEmployee.last_name;
                EmployeeInfo.cta_email = model.NewEmployee.cta_email;
                EmployeeInfo.cta_cell = model.NewEmployee.cta_cell;
                EmployeeInfo.cta_position = model.NewEmployee.cta_position;
                EmployeeInfo.loc_ID = model.NewEmployee.loc_ID;

                int CurrentPayId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).Select(x => x.payroll_Id).FirstOrDefault();
                var Biweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayId).FirstOrDefault();
                var Summery = db.tbl_employee_payroll_summary.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayId).FirstOrDefault();
                var Submission = db.tbl_employee_payroll_submission.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayId).FirstOrDefault();
                var Final = db.tbl_employee_payroll_final.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayId).FirstOrDefault();

                if(Biweekly != null)
                {
                    Biweekly.loc_id = model.NewEmployee.loc_ID;
                }
                if (Summery != null)
                {
                    Summery.loc_id = model.NewEmployee.loc_ID;
                }
                if(Submission != null)
                {
                    Submission.location_id = model.NewEmployee.loc_ID;
                }
                if (Final != null)
                {
                    Final.location_id = model.NewEmployee.loc_ID;
                }
                
                EmployeeInfo.sales_id = model.NewEmployee.sales_id;
                EmployeeInfo.full_name = model.NewEmployee.last_name + ", " + model.NewEmployee.first_name;
                EmployeeInfo.cta_direct_phone = model.NewEmployee.cta_direct_phone;
                if (model.active_status == true)
                {
                    EmployeeInfo.status = 1;
                    var statustbl = db.tbl_employee_status.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();
                    statustbl.date_of_leaving = null;
                }
                else
                {
                    int CurrentPayrollId = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).Select(x => x.payroll_Id).FirstOrDefault();
                    var deletepayrollBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayrollId).FirstOrDefault();
                    var deletepayrollSummery = db.tbl_employee_payroll_summary.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayrollId).FirstOrDefault();
                    var deletepayrollSubmission = db.tbl_employee_payroll_submission.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id) && x.payroll_id == CurrentPayrollId).FirstOrDefault();

                    var statustbl = db.tbl_employee_status.Where(x => x.employee_id.Equals(model.NewEmployee.employee_id)).FirstOrDefault();

                    statusInfo.date_of_leaving = System.DateTime.Today;

                    if (deletepayrollBiweekly != null)
                    {
                        db.tbl_employee_payroll_biweekly.Remove(deletepayrollBiweekly);

                    }
                    if (deletepayrollSummery != null)
                    {
                        db.tbl_employee_payroll_summary.Remove(deletepayrollSummery);
                    }
                    if (deletepayrollSubmission != null)
                    {
                        db.tbl_employee_payroll_submission.Remove(deletepayrollSubmission);
                    }

                    EmployeeInfo.status = 0;
                    EmployeeInfo.rfid_number = null;
                    rfid_info.rfid_number = null;
                }
                if (imageBytes != null)
                {
                    EmployeeInfo.profile_pic = imageBytes;
                }

                if (model.NewEmployee.rfid_number == "No RFID")
                {
                    EmployeeInfo.rfid_number = null;
                    rfid_info.rfid_number = null;
                }

            }
            if (PersonalDetails != null)
            {
                PersonalDetails.personal_email = model.NewEmployeePersonal.personal_email;
                PersonalDetails.home_street1 = model.NewEmployeePersonal.home_street1;
                PersonalDetails.home_street2 = model.NewEmployeePersonal.home_street2;
                PersonalDetails.city = model.NewEmployeePersonal.city;
                PersonalDetails.province = model.NewEmployeePersonal.province;
                PersonalDetails.country = model.NewEmployeePersonal.country;
                PersonalDetails.postal_code = model.NewEmployeePersonal.postal_code;
                PersonalDetails.emergency_contact_name = model.NewEmployeePersonal.emergency_contact_name;
                PersonalDetails.emergency_contact_number = model.NewEmployeePersonal.emergency_contact_number;

            }
            if(statusInfo != null)
            {
                statusInfo.compensation_type = model.CompensationType;
            }


            db.SaveChanges();
            return RedirectToAction("MyStaff", new { LocId = model.NewEmployee.loc_ID });
        }

        [HttpGet]
        public ActionResult MyStaff(string LocId)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            MyStaffViewModel modal = new MyStaffViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());
            var empDetails = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();
            modal.SehubAccess = empDetails;

            if (modal.SehubAccess.my_staff == 0)
            {
                return RedirectToAction("Attendance", "Management");
            }

            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int permissions = CheckPermissions().my_staff.Value;

            string locationid = "";
            if (LocId == "" || LocId is null)
            {
                //Debug.WriteLine("empId:" + empId);
                var result = db.tbl_employee.Where(a => a.employee_id.Equals(empId)).FirstOrDefault();

                if (result != null)
                {
                    locationid = result.loc_ID;
                }
                else
                {
                    locationid = "";
                }

            }
            else
            {
                locationid = LocId;
            }


            var EmployeeDetails = db.tbl_employee.Where(x => x.loc_ID == locationid).OrderBy(x => x.full_name).ToList();


            List<tbl_employee_status> empdetList = new List<tbl_employee_status>();

            foreach(var emp in EmployeeDetails)
            {
                tbl_employee_status empdet = new tbl_employee_status();
                empdet = db.tbl_employee_status.Where(x => x.employee_id == emp.employee_id).FirstOrDefault();
                if(empdet != null)
                {
                    empdetList.Add(empdet);
                }
            }


            //Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {
                modal.positionsTable = db.tbl_position_info.ToList();
                modal.employeeDetails = EmployeeDetails;
                modal.employeeStatusDetails = empdetList;
                modal.MatchedStaffLocs = populateLocationsPermissions(empId);
                modal.MatchedStaffLocID = locationid;
                modal.EmployeePermissions = permissions;


                modal.Positions = populatePositions();

                return View(modal);
            }
            else
            {
                return View(modal);
            }

        }

        public String ContainerNameManagementTraining = "cta-management-training";
        [HttpGet]
        public ActionResult Training(TrainingViewModel model, string loc)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int empId = Convert.ToInt32(Session["userID"].ToString());

            if (loc != null)
            {
                model.location = loc;
            }
            else
            {
                model.location = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.loc_ID).FirstOrDefault();
            }

            model.employeesList = populateEmployees();
            model.locationsList = populateLocations();
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameManagementTraining);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();

            //var blobList = blockBlob.ToList();

            var traiingList = db.tbl_employee.Where(x => x.loc_ID == model.location && x.status == 1).OrderBy(x => x.full_name).ToList();

            List<tbl_management_training> trainList = new List<tbl_management_training>();

            foreach(var train in traiingList)
            {
                tbl_management_training train_temp = new tbl_management_training();

                var trainEmp = db.tbl_management_training.Where(x => x.emp_id == train.employee_id).FirstOrDefault();

                if (trainEmp != null)
                {
                    train_temp.emp_id = trainEmp.emp_id;
                    train_temp.staff_member = trainEmp.staff_member;
                    train_temp.first_aid = trainEmp.first_aid;
                    train_temp.fa_aspiration_date = trainEmp.fa_aspiration_date;
                    CloudBlockBlob fa_blob = container.GetBlockBlobReference(trainEmp.emp_id + "_fa.pdf");
                    if (fa_blob.Exists())
                    {
                        train_temp.fa_certificate = new Uri(fa_blob.Uri.AbsoluteUri).ToString();
                    }
                    train_temp.ohs = trainEmp.ohs;
                    train_temp.ohs_aspiration_date = trainEmp.ohs_aspiration_date;
                    CloudBlockBlob ohs_blob = container.GetBlockBlobReference(trainEmp.emp_id + "_ohs.pdf");
                    if (ohs_blob.Exists())
                    {
                        train_temp.ohs_certificate = new Uri(ohs_blob.Uri.AbsoluteUri).ToString();
                    }
                    train_temp.tia = trainEmp.tia;
                    train_temp.good_year = trainEmp.good_year;
                    train_temp.gy_aspiration_date = trainEmp.gy_aspiration_date;
                    CloudBlockBlob gy_blob = container.GetBlockBlobReference(trainEmp.emp_id + "_gy.pdf");
                    if (gy_blob.Exists())
                    {
                        train_temp.gy_certificate = new Uri(gy_blob.Uri.AbsoluteUri).ToString();
                    }
                    train_temp.other = trainEmp.other;
                    train_temp.other_aspiration_date = trainEmp.other_aspiration_date;
                    CloudBlockBlob other_blob = container.GetBlockBlobReference(trainEmp.emp_id + "_other.pdf");
                    if (other_blob.Exists())
                    {
                        train_temp.other_certificate = new Uri(other_blob.Uri.AbsoluteUri).ToString();
                    }
                }
                else
                {
                    train_temp.emp_id = train.employee_id;
                    train_temp.staff_member = train.full_name;
                }

                trainList.Add(train_temp);
            }

            model.TrainingList = trainList;
            return View(model);
        }

        private static List<SelectListItem> populatePositions()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var config = db.tbl_position_info.ToList();

            foreach (var val in config)
            {
                items.Add(new SelectListItem
                {
                    Text = val.PositionTitle,
                    Value = Convert.ToString(val.PositionTitle)
                });
            }
            return items;
        }

        [HttpPost]
        public ActionResult MyStaff(MyStaffViewModel modal)
        {
            return RedirectToAction("MyStaff", new { LocId = modal.MatchedStaffLocID });
        }

        [HttpGet]
        public ActionResult StaffInfo(string values)
        {
            //Debug.WriteLine("values:" + values);
            int EmpId = Convert.ToInt32(values);
            StaffEditInformationViewModel StaffDetails = new StaffEditInformationViewModel();
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == EmpId).FirstOrDefault();
            var PersonalDetails = db.tbl_employee_personal.Where(x => x.employee_id == EmpId).FirstOrDefault();
            StaffDetails.StaffWorkDetails = EmployeeDetails;
            StaffDetails.StaffPersonalDetails = PersonalDetails;
            if (EmployeeDetails.status == 1)
            {
                StaffDetails.active_status = true;
            }
            else
            {
                StaffDetails.active_status = false;
            }

            if (EmployeeDetails.pic_status == 1)
            {
                StaffDetails.monitor_status = true;
            }
            else
            {
                StaffDetails.monitor_status = false;
            }

            return View(StaffDetails);
        }

        [HttpPost]
        public ActionResult StaffInfoSave(StaffEditInformationViewModel model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeDetails = db.tbl_employee.Where(x => x.employee_id == model.StaffWorkDetails.employee_id).FirstOrDefault();
            var PersonalDetails = db.tbl_employee_personal.Where(x => x.employee_id == model.StaffWorkDetails.employee_id).FirstOrDefault();
            int statusVal = 0;
            int picStatus = 0;
            //Debug.WriteLine(model.active_status);
            if (model.active_status == true)
            {
                statusVal = 1;
            }
            else
            {
                statusVal = 0;
            }

            if (model.monitor_status == true)
            {
                picStatus = 1;
            }
            else
            {
                picStatus = 0;
            }
            if (EmployeeDetails != null)
            {
                EmployeeDetails.first_name = model.StaffWorkDetails.first_name;
                EmployeeDetails.middle_initial = model.StaffWorkDetails.middle_initial;
                EmployeeDetails.last_name = model.StaffWorkDetails.last_name;
                EmployeeDetails.cta_email = model.StaffWorkDetails.cta_email;
                EmployeeDetails.cta_cell = model.StaffWorkDetails.cta_cell;
                EmployeeDetails.cta_position = model.StaffWorkDetails.cta_position;
                EmployeeDetails.loc_ID = model.StaffWorkDetails.loc_ID;
                EmployeeDetails.Date_of_birth = model.StaffWorkDetails.Date_of_birth;
                EmployeeDetails.status = statusVal;
                EmployeeDetails.pic_status = picStatus;

                //Debug.WriteLine("EmployeeDetails in save:" + EmployeeDetails.employee_id);
                //Debug.WriteLine("EmployeeDetails in save:" + EmployeeDetails.first_name);
                //Debug.WriteLine("EmployeeDetails in save:" + statusVal);
                //Debug.WriteLine("EmployeeDetails in save:" + picStatus);
            }

            if (PersonalDetails != null)
            {
                PersonalDetails.personal_email = model.StaffPersonalDetails.personal_email;
                PersonalDetails.primary_phone = model.StaffPersonalDetails.primary_phone;
                PersonalDetails.home_street1 = model.StaffPersonalDetails.home_street1;
                PersonalDetails.home_street2 = model.StaffPersonalDetails.home_street2;
                PersonalDetails.city = model.StaffPersonalDetails.city;
                PersonalDetails.province = model.StaffPersonalDetails.province;
                PersonalDetails.country = model.StaffPersonalDetails.country;
                PersonalDetails.postal_code = model.StaffPersonalDetails.postal_code;
                PersonalDetails.emergency_contact_name = model.StaffPersonalDetails.emergency_contact_name;
                PersonalDetails.emergency_contact_number = model.StaffPersonalDetails.emergency_contact_number;

                //Debug.WriteLine("EmployeeDetails in save:" + PersonalDetails.employee_id);
                //Debug.WriteLine("EmployeeDetails in save:" + PersonalDetails.primary_phone);
            }
            db.SaveChanges();
            return RedirectToAction("StaffInfo", new { values = model.StaffWorkDetails.employee_id });
        }

        public ActionResult PreselectValidation(string payrollID)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int PayrollIDInt = Convert.ToInt32(payrollID);

            List<EmployeePayrollListModel> emplyPayrollListValidation = new List<EmployeePayrollListModel>();

            var empList = db.tbl_employee.Where(x => x.status == 1).ToList();

            foreach (var emp in empList)
            {
                var biweeklyvalidation = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == emp.employee_id && x.payroll_id == PayrollIDInt).ToList();

                if (biweeklyvalidation.Count() > 1)
                {

                    double regularTotal = 0;

                    var corpdata = db.tbl_employee_payroll_submission.Where(x => x.employee_id == emp.employee_id && x.payroll_id == PayrollIDInt);

                    if (corpdata != null)
                    {
                        foreach (var recs in corpdata)
                        {
                            if (recs.regular != null)
                            {
                                regularTotal = regularTotal + recs.regular.Value;
                            }

                        }
                    }


                    //Trace.WriteLine(" This is the Total " + regularTotal);

                    EmployeePayrollListModel empvalid = new EmployeePayrollListModel();
                    empvalid.employeeId = emp.employee_id.ToString();
                    empvalid.fullName = emp.full_name;
                    if (regularTotal <= 80)
                    {
                        empvalid.submissionStatus = "1";
                    }
                    else
                    {
                        empvalid.submissionStatus = "0";
                    }

                    empvalid.submissionStatusCorporate = emp.loc_ID;
                    emplyPayrollListValidation.Add(empvalid);
                }
            }

            int empid = Convert.ToInt32(emplyPayrollListValidation[0].employeeId);

            string loc = db.tbl_employee.Where(x => x.employee_id == empid).Select(x => x.loc_ID).FirstOrDefault();

            return RedirectToAction("Payroll", new { locId = loc, employeeId = emplyPayrollListValidation[0].employeeId, payrollID = payrollID, ac = "Validate" });
        }

        [HttpGet]
        public ActionResult PayrollCorp(string locId, string employeeId, string payrollID)
        {
            string locationid = locId;
            return RedirectToAction("Payroll", new { locId = locationid, employeeId = employeeId, payrollID = payrollID, ac = "Corporate" });
        }

        [HttpGet]
        public ActionResult PayrollBranchDashboard(string locId, string employeeId, string payrollID)
        {
            string locationid = locId;
            if (payrollID == "branch")
            {
                return RedirectToAction("Payroll", new { locId = locationid, employeeId = "", payrollID = "", ac = "branch" });
            }
            else
            {
                return RedirectToAction("Payroll", new { locId = locationid, employeeId = "", payrollID = "", ac = "Corporate" });
            }

        }

        private void LoadLeaveHistory(string employeeID)
        {

            //Debug.WriteLine("In LoadLeave History");
            string year = DateTime.Now.ToString("yyyy").Substring(DateTime.Now.ToString("yyyy").Length - 2);
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select sum(regular) regular_sum,sum(ot) ot_sum From tbl_employee_payroll_summary where employee_id=" + employeeID + " and payroll_id like '" + year + "%'");

                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                ViewData["regualr_hours_year"] = dr["regular_sum"].ToString();
                                ViewData["ot_hours_year"] = dr["ot_sum"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }


        }

        [HttpPost]
        public ActionResult PayrrollChangeLocation(EmployeePayrollModel model, string locID, string payrollID)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date < DateTime.Now && x.end_date > DateTime.Now).FirstOrDefault();

            if(locID == null)
            {
                locID = model.MatchedLocID;
            }

            //Trace.WriteLine(locID);

            if (payrollID == null)
            {

                var CurrentEmployeeList = db.tbl_employee.Where(x => x.loc_ID == locID).ToList();

                List<EmployeePayrollListModel> emplyPayrollList = new List<EmployeePayrollListModel>();
                foreach (var item in CurrentEmployeeList)
                {
                    if (item.status == 1)
                    {
                        var datejoin = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.date_of_joining).FirstOrDefault();

                        var payrollendDate = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == currentPayroll.payroll_Id).Select(x => x.end_date).FirstOrDefault();

                        if (payrollendDate > datejoin) //
                        {
                            EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                                           //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                                                                                           //string[] values = item.ToString().Split(';');
                            obj.employeeId = item.employee_id.ToString();
                            obj.fullName = item.full_name;

                            var empInBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == item.employee_id && x.payroll_id == currentPayroll.payroll_Id && x.loc_id == locID).FirstOrDefault();

                            if (empInBiweekly != null)
                            {
                                obj.submissionStatus = empInBiweekly.recordflag.ToString();
                            }
                            else
                            {
                                obj.submissionStatus = "0";
                            }

                            var empInCorp = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == currentPayroll.payroll_Id && x.location_id == locID).FirstOrDefault();

                            if (empInCorp != null)
                            {
                                obj.submissionStatusCorporate = empInCorp.recordFlag.ToString();
                            }
                            else
                            {
                                obj.submissionStatusCorporate = "0";
                            }

                            emplyPayrollList.Add(obj);
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        var dateleaving = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.date_of_leaving).FirstOrDefault();

                        var payrollStartDate = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == currentPayroll.payroll_Id).Select(x => x.start_date).FirstOrDefault();

                        if (dateleaving > payrollStartDate) //
                        {
                            EmployeePayrollListModel obj = new EmployeePayrollListModel(); // ViewModel
                                                                                           //Debug.WriteLine(item.employee_id.ToString()+" "+ item.full_name+" "+ item.at_work.ToString());
                                                                                           //string[] values = item.ToString().Split(';');
                            obj.employeeId = item.employee_id.ToString();
                            obj.fullName = item.full_name;

                            var empInBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == item.employee_id && x.payroll_id == currentPayroll.payroll_Id && x.loc_id == locID).FirstOrDefault();

                            if (empInBiweekly != null)
                            {
                                obj.submissionStatus = empInBiweekly.recordflag.ToString();
                            }
                            else
                            {
                                obj.submissionStatus = "0";
                            }

                            var empInCorp = db.tbl_employee_payroll_submission.Where(x => x.employee_id == item.employee_id && x.payroll_id == currentPayroll.payroll_Id && x.location_id == locID).FirstOrDefault();

                            if (empInCorp != null)
                            {
                                obj.submissionStatusCorporate = empInCorp.recordFlag.ToString();
                            }
                            else
                            {
                                obj.submissionStatusCorporate = "0";
                            }

                            emplyPayrollList.Add(obj);
                        }
                    }

                }

                int emp = Convert.ToInt32(emplyPayrollList.OrderBy(x => x.fullName).FirstOrDefault().employeeId);

                var firstEmployee = db.tbl_employee.Where(x => x.employee_id == emp).OrderBy(x => x.full_name).FirstOrDefault();
                return RedirectToAction("Payroll", new { locId = locID, employeeId = firstEmployee.employee_id, payrollID = "", ac = "Dashboard" }); //, ac = "Dashboard" 
            }
            else
            {
                string[] values = payrollID.Split(';');
                int payid = Convert.ToInt32(values[0]);

                var db11 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == locID && x.payroll_id == payid) select a).ToList();
                var db22 = (from a in db.tbl_employee select a).ToList();

                var firstEmployee = (from a in db11
                                     join b in db22 on a.employee_id equals b.employee_id
                                     orderby b.full_name
                                     select new { employee_id = a.employee_id, recordflag = a.recordflag }).FirstOrDefault();
                var firstEmployeeprevpayroll = firstEmployee.employee_id.ToString();
                return RedirectToAction("Payroll", new { locId = locID, employeeId = firstEmployeeprevpayroll, payrollID = payid, ac = "Dashboard" }); //, ac = "Dashboard" 
            }

        }

        [HttpPost]
        public ActionResult AddEmployeeChangeLocation(EmployeePayrollModel model)
        {
            //Debug.WriteLine(model.AddEmployeToPayroll);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date < DateTime.Now && x.end_date > DateTime.Now).FirstOrDefault();

            string[] values = model.SelectedPayrollId.Split(';');

            string NewEmp = Convert.ToString(model.AddEmployeToPayroll);
            string loc = model.MatchedLocID;

            int prd = Convert.ToInt32(values[0]);

            var Payroll = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == prd).FirstOrDefault();


            string timeStamp = Convert.ToString(Convert.ToDateTime(Payroll.start_date).AddDays(7));

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO tbl_attendance_log VALUES (@LocID, @EmpID, 'payroll', @TimeStamp)");

                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocID", loc);
                        command.Parameters.AddWithValue("@EmpID", NewEmp);
                        command.Parameters.AddWithValue("@TimeStamp", timeStamp);


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

            return RedirectToAction("Payroll", new { locId = model.MatchedLocID, employeeId = NewEmp, payrollID = "" });

        }

        [HttpGet]
        public ActionResult DeleteEmployeeChangeLocation(string locId, string employeeId, string prid)
        {
            //Debug.WriteLine(employeeId);
            //employeeId = "10577";
            //locId = "347";
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            //var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date < DateTime.Now && x.end_date > DateTime.Now).FirstOrDefault();

            int prd = Convert.ToInt32(prid);

            var Payroll = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == prd).FirstOrDefault();

            string timeStamp = Convert.ToString(Payroll.start_date.Value.AddDays(7));

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("delete from tbl_attendance_log where employee_id = @EmpID and loc_id = @LocID and time_stamp = @TimeStamp and event_id = 'payroll'");


                    string sql = sb.ToString();
                    //Debug.WriteLine(sql);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LocID", locId);
                        command.Parameters.AddWithValue("@EmpID", employeeId);
                        command.Parameters.AddWithValue("@TimeStamp", timeStamp);


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

            var branch_biweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id.ToString() == prid && x.loc_id == locId).FirstOrDefault();
            var branch_summery = db.tbl_employee_payroll_summary.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id.ToString() == prid && x.loc_id == locId).FirstOrDefault();
            var corporate_submission = db.tbl_employee_payroll_submission.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id.ToString() == prid && x.location_id == locId).FirstOrDefault();
            var corporate_final = db.tbl_employee_payroll_final.Where(x => x.employee_id.ToString() == employeeId && x.payroll_id.ToString() == prid && x.location_id == locId).FirstOrDefault();

            if(branch_biweekly != null)
            {
                db.tbl_employee_payroll_biweekly.Remove(branch_biweekly);
            }
            if (branch_summery != null)
            {
                db.tbl_employee_payroll_summary.Remove(branch_summery);
            }
            if (corporate_submission != null)
            {
                db.tbl_employee_payroll_submission.Remove(corporate_submission);
            }
            if (corporate_final != null)
            {
                db.tbl_employee_payroll_final.Remove(corporate_final);
            }
            db.SaveChanges();

            return RedirectToAction("Payroll", new { locId = locId, employeeId = "", payrollID = "" });

        }

        [HttpPost]
        public ActionResult ChangePayrollID(EmployeePayrollModel model)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string[] values = model.SelectedPayrollId.Split(';');

            int payid = Convert.ToInt32(values[0]);

            var checkCurrentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).FirstOrDefault();

            
            var db1 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == model.MatchedLocID && x.payroll_id == payid) select a).ToList();
            var db2 = (from a in db.tbl_employee select a).ToList();

            var firstEmployee = (from a in db1
                             join b in db2 on a.employee_id equals b.employee_id
                             orderby b.full_name
                             select new { employee_id = a.employee_id, recordflag = a.recordflag }).FirstOrDefault();

            if (model.SelectedEmployeeId == null)
            {
                if(firstEmployee != null)
                {
                    return RedirectToAction("Payroll", new { locId = model.MatchedLocID, employeeId = firstEmployee.employee_id, payrollID = values[0], ac = "Dashboard" }); //, ac = "Dashboard" 
                }
                else
                {
                    var defaultLocationEmployee = db.tbl_employee.Where(x => x.loc_ID == model.MatchedLocID && x.status == 1).OrderBy(x => x.full_name).FirstOrDefault();

                    return RedirectToAction("Payroll", new { locId = model.MatchedLocID, employeeId = defaultLocationEmployee.employee_id, payrollID = values[0], ac = "Dashboard" });  //, ac = "Dashboard" 
                }
            }

            return RedirectToAction("Payroll", new { locId = values[2], employeeId = values[1], payrollID = values[0] });

        }

        public ActionResult ShowEmployeePayroll(string value)
        {

            //Debug.WriteLine("In ShowEditCustInfo:" + value);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var EmployeeDetails = db.tbl_employee.Where(x => x.full_name == value).FirstOrDefault();
            string empId = "";
            string locId = "";
            string position = "";
            byte[] profileImg = null;
            if (EmployeeDetails != null)
            {
                empId = EmployeeDetails.employee_id.ToString();
                locId = EmployeeDetails.loc_ID;
                position = EmployeeDetails.cta_position;
                profileImg = EmployeeDetails.profile_pic;
            }
            string base64ProfilePic = "";
            if (profileImg is null)
            {
                base64ProfilePic = "";
            }
            else
            {
                base64ProfilePic = Convert.ToBase64String(profileImg);
            }

            //Debug.WriteLine("Image_base64:" + base64ProfilePic);
            ViewData["ProfileImage"] = "data:image/png;base64," + base64ProfilePic;
            ViewData["EmployeeName"] = value;
            ViewData["EmployeeId"] = empId;
            ViewData["Position"] = position;
            //DateTime todatDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime todatDate = DateTime.Parse("2020-04-30");                                                      
            //Debug.WriteLine("Date time today:" + todatDate);
            var payRollDates = db.tbl_employee_payroll_dates.Where(x => x.start_date <= todatDate && x.end_date >= todatDate).FirstOrDefault();
            //Debug.WriteLine("payRollDates Values:" + payRollDates.payroll_Id);

            LoadPayroll(empId, payRollDates.start_date.ToString(), payRollDates.end_date.ToString(), payRollDates.payroll_Id.ToString(), locId);

            int employeeId = Convert.ToInt32(empId);
            var payrollBiweekly = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == employeeId && x.payroll_id == payRollDates.payroll_Id).FirstOrDefault();
            var payrollSummary = db.tbl_employee_payroll_summary.Where(x => x.employee_id == employeeId && x.payroll_id == payRollDates.payroll_Id).FirstOrDefault();

            PayRollViewModel payrollDetails = new PayRollViewModel();
            if (payrollBiweekly != null)
            {
                EditPayrollBiWeeklyViewModel payrollBiWeekDetails = new EditPayrollBiWeeklyViewModel();
                payrollBiWeekDetails.employee_id = payrollBiweekly.employee_id;
                payrollBiWeekDetails.payroll_id = payrollBiweekly.payroll_id;
                payrollBiWeekDetails.sat_1_reg = payrollBiweekly.sat_1_reg;
                payrollBiWeekDetails.mon_1_reg = payrollBiweekly.mon_1_reg;
                payrollBiWeekDetails.tues_1_reg = payrollBiweekly.tues_1_reg;
                payrollBiWeekDetails.wed_1_reg = payrollBiweekly.wed_1_reg;
                payrollBiWeekDetails.thurs_1_reg = payrollBiweekly.thurs_1_reg;
                payrollBiWeekDetails.fri_1_reg = payrollBiweekly.fri_1_reg;
                payrollBiWeekDetails.sat_2_reg = payrollBiweekly.sat_2_reg;
                payrollBiWeekDetails.mon_2_reg = payrollBiweekly.mon_2_reg;
                payrollBiWeekDetails.tues_2_reg = payrollBiweekly.tues_2_reg;
                payrollBiWeekDetails.wed_2_reg = payrollBiweekly.wed_2_reg;
                payrollBiWeekDetails.thurs_2_reg = payrollBiweekly.thurs_2_reg;
                payrollBiWeekDetails.fri_2_reg = payrollBiweekly.fri_2_reg;
                payrollBiWeekDetails.sat_1_opt = payrollBiweekly.sat_1_opt;
                payrollBiWeekDetails.mon_1_opt = payrollBiweekly.mon_1_opt;
                payrollBiWeekDetails.tues_1_opt = payrollBiweekly.tues_1_opt;
                payrollBiWeekDetails.wed_1_opt = payrollBiweekly.wed_1_opt;
                payrollBiWeekDetails.thurs_1_opt = payrollBiweekly.thurs_1_opt;
                payrollBiWeekDetails.fri_1_opt = payrollBiweekly.fri_1_opt;
                payrollBiWeekDetails.sat_2_opt = payrollBiweekly.sat_2_opt;
                payrollBiWeekDetails.mon_2_opt = payrollBiweekly.mon_2_opt;
                payrollBiWeekDetails.tues_2_opt = payrollBiweekly.tues_2_opt;
                payrollBiWeekDetails.wed_2_opt = payrollBiweekly.wed_2_opt;
                payrollBiWeekDetails.thurs_2_opt = payrollBiweekly.thurs_2_opt;
                payrollBiWeekDetails.fri_2_opt = payrollBiweekly.fri_2_opt;
                payrollBiWeekDetails.sat_1_sel = payrollBiweekly.sat_1_sel;
                payrollBiWeekDetails.mon_1_sel = payrollBiweekly.mon_1_sel;
                payrollBiWeekDetails.tues_1_sel = payrollBiweekly.tues_1_sel;
                payrollBiWeekDetails.wed_1_sel = payrollBiweekly.wed_1_sel;
                payrollBiWeekDetails.thurs_1_sel = payrollBiweekly.thurs_1_sel;
                payrollBiWeekDetails.fri_1_sel = payrollBiweekly.fri_1_sel;
                payrollBiWeekDetails.sat_2_sel = payrollBiweekly.sat_2_sel;
                payrollBiWeekDetails.mon_2_sel = payrollBiweekly.mon_2_sel;
                payrollBiWeekDetails.tues_2_sel = payrollBiweekly.tues_2_sel;
                payrollBiWeekDetails.wed_2_sel = payrollBiweekly.wed_2_sel;
                payrollBiWeekDetails.thurs_2_sel = payrollBiweekly.thurs_2_sel;
                payrollBiWeekDetails.fri_2_sel = payrollBiweekly.fri_2_sel;
                payrollBiWeekDetails.sat_1_sum = payrollBiweekly.sat_1_sum;
                payrollBiWeekDetails.mon_1_sum = payrollBiweekly.mon__1_sum;
                payrollBiWeekDetails.tues_1_sum = payrollBiweekly.tues_1_sum;
                payrollBiWeekDetails.wed_1_sum = payrollBiweekly.wed_1_sum;
                payrollBiWeekDetails.thurs_1_sum = payrollBiweekly.thurs_1_sum;
                payrollBiWeekDetails.fri_1_sum = payrollBiweekly.fri_1_sum;
                payrollBiWeekDetails.sat_2_sum = payrollBiweekly.sat_2_sum;
                payrollBiWeekDetails.mon_2_sum = payrollBiweekly.mon_2_sum;
                payrollBiWeekDetails.tues_2_sum = payrollBiweekly.tues_2_sum;
                payrollBiWeekDetails.wed_2_Sum = payrollBiweekly.wed_2_Sum;
                payrollBiWeekDetails.thurs_2_Sum = payrollBiweekly.thurs_2_Sum;
                payrollBiWeekDetails.fri_2_sum = payrollBiweekly.fri_2_sum;
                payrollBiWeekDetails.bi_week_chkin_avg = payrollBiweekly.bi_week_chkin_avg;
                payrollBiWeekDetails.bi_week_chkout_avg = payrollBiweekly.bi_week_chkout_avg;
                payrollBiWeekDetails.last_updated_by = payrollBiweekly.last_updated_by;
                payrollBiWeekDetails.last_update_date = payrollBiweekly.last_update_date;
                payrollBiWeekDetails.recordflag = payrollBiweekly.recordflag;
                payrollBiWeekDetails.comments = payrollBiweekly.comments;
                payrollBiWeekDetails.timeClock_sat1 = payrollBiweekly.timeClock_sat1;
                payrollBiWeekDetails.timeClock_mon1 = payrollBiweekly.timeClock_mon1;
                payrollBiWeekDetails.timeClock_tues1 = payrollBiweekly.timeClock_tues1;
                payrollBiWeekDetails.timeClock_wed1 = payrollBiweekly.timeClock_wed1;
                payrollBiWeekDetails.timeClock_thurs1 = payrollBiweekly.timeClock_thurs1;
                payrollBiWeekDetails.timeClock_fri1 = payrollBiweekly.timeClock_fri1;
                payrollBiWeekDetails.timeClock_sat2 = payrollBiweekly.timeClock_sat2;
                payrollBiWeekDetails.timeClock_mon2 = payrollBiweekly.timeClock_mon2;
                payrollBiWeekDetails.timeClock_tues2 = payrollBiweekly.timeClock_tues2;
                payrollBiWeekDetails.timeClock_wed2 = payrollBiweekly.timeClock_wed2;
                payrollBiWeekDetails.timeClock_thurs2 = payrollBiweekly.timeClock_thurs2;
                payrollBiWeekDetails.timeClock_fri2 = payrollBiweekly.timeClock_fri2;
                payrollBiWeekDetails.sun_1_reg = payrollBiweekly.sun_1_reg;
                payrollBiWeekDetails.sun_2_reg = payrollBiweekly.sun_2_reg;
                payrollBiWeekDetails.sun_1_opt = payrollBiweekly.sun_1_opt;
                payrollBiWeekDetails.sun_2_opt = payrollBiweekly.sun_2_opt;
                payrollBiWeekDetails.sun_1_sel = payrollBiweekly.sun_1_sel;
                payrollBiWeekDetails.sun_2_sel = payrollBiweekly.sun_2_sel;
                payrollBiWeekDetails.sun_1_sum = payrollBiweekly.sun_1_sum;
                payrollBiWeekDetails.sun_2_sum = payrollBiweekly.sun_2_sum;
                payrollBiWeekDetails.timeClock_sun1 = payrollBiweekly.timeClock_sun1;
                payrollBiWeekDetails.timeClock_sun2 = payrollBiweekly.timeClock_sun2;
                payrollBiWeekDetails.comments = payrollBiweekly.comments;


                if (payrollBiweekly.sat_1_sel == " " || payrollBiweekly.sat_1_sel is null)
                {
                    payrollBiWeekDetails.sat_1_sel_id = 1;
                }
                else
                {
                    var sat1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.sat_1_sel_id = sat1details.category_id;
                    //Debug.WriteLine("payrollBiWeekDetails.sat_1_sel_id:" + payrollBiWeekDetails.sat_1_sel_id);
                }

                if (payrollBiweekly.sun_1_sel == " " || payrollBiweekly.sun_1_sel is null)
                {
                    //Debug.WriteLine("Yes2:" + payrollBiweekly.sun_1_sel + ":");
                    payrollBiWeekDetails.sun_1_sel_id = 1;
                }
                else
                {
                    var sun1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.sun_1_sel_id = sun1details.category_id;


                }

                if (payrollBiweekly.mon_1_sel == " " || payrollBiweekly.mon_1_sel is null)
                {
                    payrollBiWeekDetails.mon_1_sel_id = 1;

                    //Trace.WriteLine("This is the Test 1" + payrollBiWeekDetails.mon_1_sel_id);

                }
                else
                {
                    var mon1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.mon_1_sel_id = mon1details.category_id;

                    //Trace.WriteLine("This is the Test 2" + payrollBiWeekDetails.mon_1_sel_id);

                }

                if (payrollBiweekly.tues_1_sel == " " || payrollBiweekly.tues_1_sel is null)
                {
                    payrollBiWeekDetails.tues_1_sel_id = 1;
                }
                else
                {
                    var tues1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.tues_1_sel_id = tues1details.category_id;
                }

                if (payrollBiweekly.wed_1_sel == " " || payrollBiweekly.wed_1_sel is null)
                {
                    payrollBiWeekDetails.wed_1_sel_id = 1;
                }
                else
                {
                    var wed1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.wed_1_sel_id = wed1details.category_id;
                }

                if (payrollBiweekly.thurs_1_sel == " " || payrollBiweekly.thurs_1_sel is null)
                {
                    payrollBiWeekDetails.thurs_1_sel_id = 1;
                }
                else
                {
                    var thurs1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.thurs_1_sel_id = thurs1details.category_id;
                }

                if (payrollBiweekly.fri_1_sel == " " || payrollBiweekly.fri_1_sel is null)
                {
                    payrollBiWeekDetails.fri_1_sel_id = 1;
                }
                else
                {
                    var fri1details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_1_sel).FirstOrDefault();
                    payrollBiWeekDetails.fri_1_sel_id = fri1details.category_id;
                }

                if (payrollBiweekly.sat_2_sel == " " || payrollBiweekly.sat_2_sel is null)
                {
                    payrollBiWeekDetails.sat_2_sel_id = 1;
                }
                else
                {
                    var sat2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sat_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.sat_2_sel_id = sat2details.category_id;
                }

                if (payrollBiweekly.sun_2_sel == " " || payrollBiweekly.sun_2_sel is null)
                {
                    payrollBiWeekDetails.sun_2_sel_id = 1;
                }
                else
                {
                    var sun2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.sun_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.sun_2_sel_id = sun2details.category_id;
                }

                if (payrollBiweekly.mon_2_sel == " " || payrollBiweekly.mon_2_sel is null)
                {
                    payrollBiWeekDetails.mon_2_sel_id = 1;
                }
                else
                {
                    var mon2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.mon_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.mon_2_sel_id = mon2details.category_id;
                }

                if (payrollBiweekly.tues_2_sel == " " || payrollBiweekly.tues_2_sel is null)
                {
                    payrollBiWeekDetails.tues_2_sel_id = 1;
                }
                else
                {
                    var tues2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.tues_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.tues_2_sel_id = tues2details.category_id;
                }

                if (payrollBiweekly.wed_2_sel == " " || payrollBiweekly.wed_2_sel is null)
                {
                    payrollBiWeekDetails.wed_2_sel_id = 1;
                }
                else
                {
                    var wed2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.wed_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.wed_2_sel_id = wed2details.category_id;
                }

                if (payrollBiweekly.thurs_2_sel == " " || payrollBiweekly.thurs_2_sel is null)
                {
                    payrollBiWeekDetails.thurs_2_sel_id = 1;
                }
                else
                {
                    var thurs2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.thurs_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.thurs_2_sel_id = thurs2details.category_id;
                }

                if (payrollBiweekly.fri_2_sel == " " || payrollBiweekly.fri_2_sel is null)
                {
                    payrollBiWeekDetails.fri_2_sel_id = 1;
                }
                else
                {
                    var fri2details = db.tbl_payroll_category_selection.Where(x => x.category == payrollBiweekly.fri_2_sel).FirstOrDefault();
                    payrollBiWeekDetails.fri_2_sel_id = fri2details.category_id;
                }

                payrollDetails.payBiweek = payrollBiWeekDetails;
            }
            if (payrollSummary != null)
            {
                payrollDetails.paySummary = payrollSummary;
            }
            var selectionList = (from a in db.tbl_payroll_category_selection select a).ToList();

            
            ViewBag.TypeSelectionList = selectionList;

            

            return PartialView(payrollDetails);
        }

        [HttpPost]
        public ActionResult SubmitEmployeePayroll(EmployeePayrollModel model, string locid)
        {
            //Debug.WriteLine("In Payroll Submission:" + model.employeepayroll.payBiweek.employee_id + " " + model.employeepayroll.payBiweek.payroll_id);
            int empID = Convert.ToInt32(model.SelectedEmployeeId); //model.employeepayroll.payBiweek.employee_id
            int payrollId = Convert.ToInt32(model.SelectedPayrollId.Split(';')[0]); //model.employeepayroll.payBiweek.payroll_id
            //Debug.WriteLine("The Payroll-ID that was obtained properly is" + payrollId);

            //Trace.WriteLine("On Payroll Load:" + locid + ":" + empID + ":" + payrollId);

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var BiWeekDetails = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == empID && x.payroll_id == payrollId && x.loc_id == locid).FirstOrDefault();
            var SummaryDetails = db.tbl_employee_payroll_summary.Where(x => x.employee_id == empID && x.payroll_id == payrollId && x.loc_id == locid).FirstOrDefault();
            var SummarySubmission = db.tbl_employee_payroll_submission.Where(x => x.employee_id == empID && x.payroll_id == payrollId).FirstOrDefault();
            var SubmissionDate = db.tbl_payroll_submission_branch.Where(x => x.payroll_id == payrollId && x.location_id == locid).FirstOrDefault();

            //if (SubmissionDate != null)
            //{
            //    SummarySubmission.payroll_id = payrollId;
            //    SummarySubmission.regular = model.employeepayroll.paySummary.regular;
            //    SummarySubmission.ot = model.employeepayroll.paySummary.ot;
            //    SummarySubmission.vacationTime = model.employeepayroll.paySummary.vac;
            //    SummarySubmission.sickTime = model.employeepayroll.paySummary.sick;
            //    SummarySubmission.StatutoryTime = model.employeepayroll.paySummary.stat;
            //    SummarySubmission.recordFlag = 0;
            //}

            if (BiWeekDetails != null && SummaryDetails != null)
            {
                BiWeekDetails.loc_id = locid;
                BiWeekDetails.sat_1_reg = model.employeepayroll.payBiweek.sat_1_reg;
                BiWeekDetails.mon_1_reg = model.employeepayroll.payBiweek.mon_1_reg;
                BiWeekDetails.tues_1_reg = model.employeepayroll.payBiweek.tues_1_reg;
                BiWeekDetails.wed_1_reg = model.employeepayroll.payBiweek.wed_1_reg;
                BiWeekDetails.thurs_1_reg = model.employeepayroll.payBiweek.thurs_1_reg;
                BiWeekDetails.fri_1_reg = model.employeepayroll.payBiweek.fri_1_reg;
                BiWeekDetails.sat_2_reg = model.employeepayroll.payBiweek.sat_2_reg;
                BiWeekDetails.mon_2_reg = model.employeepayroll.payBiweek.mon_2_reg;
                BiWeekDetails.tues_2_reg = model.employeepayroll.payBiweek.tues_2_reg;
                BiWeekDetails.wed_2_reg = model.employeepayroll.payBiweek.wed_2_reg;
                BiWeekDetails.thurs_2_reg = model.employeepayroll.payBiweek.thurs_2_reg;
                BiWeekDetails.fri_2_reg = model.employeepayroll.payBiweek.fri_2_reg;

                BiWeekDetails.sat_1_opt = model.employeepayroll.payBiweek.sat_1_opt;
                BiWeekDetails.mon_1_opt = model.employeepayroll.payBiweek.mon_1_opt;
                BiWeekDetails.tues_1_opt = model.employeepayroll.payBiweek.tues_1_opt;
                BiWeekDetails.wed_1_opt = model.employeepayroll.payBiweek.wed_1_opt;
                BiWeekDetails.thurs_1_opt = model.employeepayroll.payBiweek.thurs_1_opt;
                BiWeekDetails.fri_1_opt = model.employeepayroll.payBiweek.fri_1_opt;
                BiWeekDetails.sat_2_opt = model.employeepayroll.payBiweek.sat_2_opt;
                BiWeekDetails.mon_2_opt = model.employeepayroll.payBiweek.mon_2_opt;
                BiWeekDetails.tues_2_opt = model.employeepayroll.payBiweek.tues_2_opt;
                BiWeekDetails.wed_2_opt = model.employeepayroll.payBiweek.wed_2_opt;
                BiWeekDetails.thurs_2_opt = model.employeepayroll.payBiweek.thurs_2_opt;
                BiWeekDetails.fri_2_opt = model.employeepayroll.payBiweek.fri_2_opt;

                BiWeekDetails.sat_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sat_1_sel_id);
                BiWeekDetails.mon_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.mon_1_sel_id);
                BiWeekDetails.tues_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.tues_1_sel_id);
                BiWeekDetails.wed_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.wed_1_sel_id);
                BiWeekDetails.thurs_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.thurs_1_sel_id);
                BiWeekDetails.fri_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.fri_1_sel_id);
                BiWeekDetails.sat_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sat_2_sel_id);
                BiWeekDetails.mon_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.mon_2_sel_id);
                BiWeekDetails.tues_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.tues_2_sel_id);
                BiWeekDetails.wed_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.wed_2_sel_id);
                BiWeekDetails.thurs_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.thurs_2_sel_id); ;
                BiWeekDetails.fri_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.fri_2_sel_id);

                BiWeekDetails.sat_1_sum = model.employeepayroll.payBiweek.sat_1_sum;
                BiWeekDetails.mon__1_sum = model.employeepayroll.payBiweek.mon_1_sum;
                BiWeekDetails.tues_1_sum = model.employeepayroll.payBiweek.tues_1_sum;
                BiWeekDetails.wed_1_sum = model.employeepayroll.payBiweek.wed_1_sum;
                BiWeekDetails.thurs_1_sum = model.employeepayroll.payBiweek.thurs_1_sum;
                BiWeekDetails.fri_1_sum = model.employeepayroll.payBiweek.fri_1_sum;
                BiWeekDetails.sat_2_sum = model.employeepayroll.payBiweek.sat_2_sum;
                BiWeekDetails.mon_2_sum = model.employeepayroll.payBiweek.mon_2_sum;
                BiWeekDetails.tues_2_sum = model.employeepayroll.payBiweek.tues_2_sum;
                BiWeekDetails.wed_2_Sum = model.employeepayroll.payBiweek.wed_2_Sum;
                BiWeekDetails.thurs_2_Sum = model.employeepayroll.payBiweek.thurs_2_Sum;
                BiWeekDetails.fri_2_sum = model.employeepayroll.payBiweek.fri_2_sum;
                BiWeekDetails.bi_week_chkin_avg = model.employeepayroll.payBiweek.bi_week_chkin_avg;
                BiWeekDetails.bi_week_chkout_avg = model.employeepayroll.payBiweek.bi_week_chkout_avg;
                BiWeekDetails.last_updated_by = model.employeepayroll.payBiweek.last_updated_by;
                BiWeekDetails.last_update_date = model.employeepayroll.payBiweek.last_update_date;
                BiWeekDetails.recordflag = model.employeepayroll.payBiweek.recordflag;
                BiWeekDetails.comments = model.employeepayroll.payBiweek.comments;

                BiWeekDetails.timeClock_sat1 = model.employeepayroll.payBiweek.timeClock_sat1;
                BiWeekDetails.timeClock_mon1 = model.employeepayroll.payBiweek.timeClock_mon1;
                BiWeekDetails.timeClock_tues1 = model.employeepayroll.payBiweek.timeClock_tues1;
                BiWeekDetails.timeClock_wed1 = model.employeepayroll.payBiweek.timeClock_wed1;
                BiWeekDetails.timeClock_thurs1 = model.employeepayroll.payBiweek.timeClock_thurs1;
                BiWeekDetails.timeClock_fri1 = model.employeepayroll.payBiweek.timeClock_fri1;
                BiWeekDetails.timeClock_sat2 = model.employeepayroll.payBiweek.timeClock_sat2;
                BiWeekDetails.timeClock_mon2 = model.employeepayroll.payBiweek.timeClock_mon2;
                BiWeekDetails.timeClock_tues2 = model.employeepayroll.payBiweek.timeClock_tues2;
                BiWeekDetails.timeClock_wed2 = model.employeepayroll.payBiweek.timeClock_wed2;
                BiWeekDetails.timeClock_thurs2 = model.employeepayroll.payBiweek.timeClock_thurs2;
                BiWeekDetails.timeClock_fri2 = model.employeepayroll.payBiweek.timeClock_fri2;

                BiWeekDetails.sun_1_reg = model.employeepayroll.payBiweek.sun_1_reg;
                BiWeekDetails.sun_2_reg = model.employeepayroll.payBiweek.sun_2_reg;
                BiWeekDetails.sun_1_opt = model.employeepayroll.payBiweek.sun_1_opt;
                BiWeekDetails.sun_2_opt = model.employeepayroll.payBiweek.sun_2_opt;
                BiWeekDetails.sun_1_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sun_1_sel_id); ;
                BiWeekDetails.sun_2_sel = SelectedCategoryValue(model.employeepayroll.payBiweek.sun_2_sel_id); ;
                BiWeekDetails.sun_1_sum = model.employeepayroll.payBiweek.sun_1_sum;
                BiWeekDetails.sun_2_sum = model.employeepayroll.payBiweek.sun_2_sum;

                BiWeekDetails.timeClock_sun1 = model.employeepayroll.payBiweek.timeClock_sun1;
                BiWeekDetails.timeClock_sun2 = model.employeepayroll.payBiweek.timeClock_sun2;

                BiWeekDetails.vacation = model.employeepayroll.payBiweek.vacation;
                BiWeekDetails.vacation_buyin = model.employeepayroll.payBiweek.vacation_buyin;
                BiWeekDetails.compensation_type = model.employeepayroll.payBiweek.compensation_type;
                BiWeekDetails.position = model.employeepayroll.payBiweek.position;


                if ((BiWeekDetails.sat_1_reg != null) && (BiWeekDetails.sun_1_reg != null) && (BiWeekDetails.mon_1_reg != null) && (BiWeekDetails.tues_1_reg != null) && (BiWeekDetails.wed_1_reg != null) && (BiWeekDetails.thurs_1_reg != null) && (BiWeekDetails.fri_1_reg != null) && (BiWeekDetails.wed_2_reg != null) && (BiWeekDetails.thurs_2_reg != null) && (BiWeekDetails.fri_2_reg != null) && (BiWeekDetails.mon_2_reg != null) && (BiWeekDetails.tues_2_reg != null) && (BiWeekDetails.sat_2_reg != null) && (BiWeekDetails.sun_2_reg != null))
                {
                    BiWeekDetails.recordflag = 2;

                }
                else
                {
                    BiWeekDetails.recordflag = 1;
                }

                SummaryDetails.regular = model.employeepayroll.paySummary.regular;
                SummaryDetails.ot = model.employeepayroll.paySummary.ot;
                SummaryDetails.sick = model.employeepayroll.paySummary.sick;
                SummaryDetails.vac = model.employeepayroll.paySummary.vac;
                SummaryDetails.brev = model.employeepayroll.paySummary.brev;
                SummaryDetails.record_flag = 2;
                SummaryDetails.stat = model.employeepayroll.paySummary.stat;
                SummaryDetails.split = model.employeepayroll.paySummary.split;
                SummaryDetails.gen_ins = model.employeepayroll.paySummary.gen_ins;
                SummaryDetails.emp_ins = model.employeepayroll.paySummary.emp_ins;
                SummaryDetails.workers_comp = model.employeepayroll.paySummary.workers_comp;
                SummaryDetails.PL = model.employeepayroll.paySummary.PL;
                SummaryDetails.NL = model.employeepayroll.paySummary.NL;


                if(SubmissionDate != null)
                {
                    string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();

                        sb.Append("UPDATE ");
                        sb.Append("tbl_payroll_submission_branch ");
                        sb.Append("SET resubmit = 1");
                        sb.Append("WHERE location_id = @loc AND payroll_id = @payID  ");

                        string sql = sb.ToString();

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@loc", locid);
                            command.Parameters.AddWithValue("@payID", payrollId);

                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        connection.Close();
                    }


                }

            }
            else
            {
                //Trace.WriteLine("No Record");
            }
            db.SaveChanges();
            ViewData["submit_payroll_confirmation"] = "yes";
            return RedirectToAction("Payroll", new { locId = locid, employeeId = empID, payrollID = payrollId }); //return RedirectToAction("Payroll", new { locId = locid, employeeId = "", payrollID = payrollId });
        }

        [HttpGet]
        public ActionResult UnlockCorporate(string locId, string employeeId, string payrollID)
        {
            string locationid = locId;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var deleteOrderDetails =
                    from details in db.tbl_payroll_submission_corporate
                    select details;

            if (deleteOrderDetails != null)
            {
                db.tbl_payroll_submission_corporate.RemoveRange(deleteOrderDetails);
                db.SaveChanges();
            }
            return RedirectToAction("Payroll", new { locId = locationid, ac = "Corporate" });
        }

        [HttpGet]
        public ActionResult UnlockIndividualCorporate(string locId)
        {
            string locationid = locId;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var checkCurrentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).FirstOrDefault();
            int payrollId = checkCurrentPayrollID.payroll_Id;

            var deleteOrderDetails =
                    from details in db.tbl_payroll_submission_corporate.Where(x => x.payroll_id == payrollId)
                    select details;

            if (deleteOrderDetails != null)
            {
                db.tbl_payroll_submission_corporate.RemoveRange(deleteOrderDetails);
                db.SaveChanges();
            }
            return RedirectToAction("Payroll", new { locId = locationid, ac = "Corporate" });
        }

        [HttpGet]
        public ActionResult UnlockIndividualBranch(string locId, string currLocat, string payrollID)
        {

            string[] values = payrollID.Split(';');

            int payid = Convert.ToInt32(values[0]);

            string locationid = locId;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var latestDetails = db.tbl_payroll_submission_branch.Where(x => x.location_id == locId && x.payroll_id == payid).OrderBy(x => x.submission_date).FirstOrDefault();
            
            if (latestDetails != null)
            {
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE ");
                    sb.Append("tbl_payroll_submission_branch ");
                    sb.Append("SET loc_status = 0  ");
                    sb.Append("WHERE location_id = @loc AND payroll_id = @payID  ");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@loc", locId);
                        command.Parameters.AddWithValue("@payID", payid);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }

            var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date < DateTime.Now && x.end_date > DateTime.Now).FirstOrDefault();

            if (payid == currentPayroll.payroll_Id)
            {
                var firstEmployee = db.tbl_employee.Where(x => x.loc_ID == currLocat && x.status == 1).OrderBy(x => x.full_name).FirstOrDefault();
                return RedirectToAction("Payroll", new { locId = currLocat, employeeId = firstEmployee.employee_id, payrollID = payid, ac = "DashboardCorporate" }); //, ac = "Dashboard" 
            }
            else
            {
                var db11 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == currLocat && x.payroll_id == payid) select a).ToList();
                var db22 = (from a in db.tbl_employee select a).ToList();

                var firstEmployee = (from a in db11
                                     join b in db22 on a.employee_id equals b.employee_id
                                     orderby b.full_name
                                     select new { employee_id = a.employee_id, recordflag = a.recordflag }).FirstOrDefault();
                var firstEmployeeprevpayroll = firstEmployee.employee_id.ToString();
                return RedirectToAction("Payroll", new { locId = currLocat, employeeId = firstEmployeeprevpayroll, payrollID = payid, ac = "DashboardCorporate" }); //, ac = "Dashboard" 
            }

        }

        [HttpGet]
        public ActionResult LockIndividualBranch(string locId, string currLocat, string payrollID)
        {

            string[] values = payrollID.Split(';');

            int payid = Convert.ToInt32(values[0]);

            string locationid = locId;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var latestDetails = db.tbl_payroll_submission_branch.Where(x => x.location_id == locId && x.payroll_id == payid);

            if (latestDetails != null)
            {
                string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE ");
                    sb.Append("tbl_payroll_submission_branch ");
                    sb.Append("SET loc_status = 1  ");
                    sb.Append("WHERE location_id = @loc AND payroll_id = @payID  ");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@loc", locId);
                        command.Parameters.AddWithValue("@payID", payid);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }

            var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date < DateTime.Now && x.end_date > DateTime.Now).FirstOrDefault();

            if (payid == currentPayroll.payroll_Id)
            {
                var firstEmployee = db.tbl_employee.Where(x => x.loc_ID == currLocat && x.status == 1).OrderBy(x => x.full_name).FirstOrDefault();
                return RedirectToAction("Payroll", new { locId = currLocat, employeeId = firstEmployee.employee_id, payrollID = payid, ac = "DashboardCorporate" }); //, ac = "Dashboard" 
            }
            else
            {
                var db11 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == currLocat && x.payroll_id == payid) select a).ToList();
                var db22 = (from a in db.tbl_employee select a).ToList();

                var firstEmployee = (from a in db11
                                     join b in db22 on a.employee_id equals b.employee_id
                                     orderby b.full_name
                                     select new { employee_id = a.employee_id, recordflag = a.recordflag }).FirstOrDefault();
                var firstEmployeeprevpayroll = firstEmployee.employee_id.ToString();
                return RedirectToAction("Payroll", new { locId = currLocat, employeeId = firstEmployeeprevpayroll, payrollID = payid, ac = "DashboardCorporate" }); //, ac = "Dashboard" 
            }
        }

        [HttpPost]
        public ActionResult button1_Click(EmployeePayrollModel model, string locId, string prID)
        {
            string[] values = prID.Split(';');

            int payid = Convert.ToInt32(values[0]);

            int UserempId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("The Selected Payroll ID is" + model.SelectedPayrollId);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();


            tbl_payroll_submission_corporate corpSubmission = new tbl_payroll_submission_corporate();

            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
            DateTime nstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

            corpSubmission.submission_date = nstTime;
            corpSubmission.payroll_id = payid;
            corpSubmission.submitter_empId = UserempId;
            corpSubmission.submitter_name = db.tbl_employee.Where(x =>x.employee_id == UserempId).Select(x =>x.full_name).FirstOrDefault();

            db.tbl_payroll_submission_corporate.Add(corpSubmission);
            db.SaveChanges();

            var checkCurrentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).FirstOrDefault();

            //int payrollId = checkCurrentPayrollID.payroll_Id;

            int payrollId;

            if (values[0] != null && values[0] != "")
            {
                payrollId = payid;
            }
            else
            {
                payrollId = checkCurrentPayrollID.payroll_Id;
            }

            string PayrollEndDate = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == payrollId).Select(x => x.end_date).FirstOrDefault().ToString();

            string submitterName = db.tbl_employee.Where(x => x.employee_id == UserempId).Select(x => x.full_name).FirstOrDefault();

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            SqlConnection sqlCon = new SqlConnection(constr);
            sqlCon.Open();

            SqlCommand sqlCmd = new SqlCommand(
                "select payroll_id, end_date, employee_id, location_id,full_name, CommissionPay_D, StatutoryHolidayPay_H, SalaryPay_D, VacationPay_D, RegularPay_H, OvertimePay_H, OvertimePay_2_H, OtherPay_D, SickLeave_H, VacationTime_H, OnCallCommission_D, OvertimePay_3_H  from tbl_employee_payroll_final where payroll_id = " + payrollId + "", sqlCon);
            SqlDataReader reader = sqlCmd.ExecuteReader();

            string path3 = Path.Combine(Server.MapPath("~/Content/Corporate_Submission.csv"));
            StreamWriter sw = new StreamWriter(path3);
            object[] output = new object[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
                output[i] = reader.GetName(i); Debug.WriteLine(output);

            sw.WriteLine(string.Join(",", output));

            while (reader.Read())
            {
                reader.GetValues(output);
                sw.WriteLine(string.Join(",", output));
            }

            sw.Close();
            reader.Close();
            sqlCon.Close();

            string paystr = "20" + payrollId.ToString().Substring(0, 2) + " - " + payrollId.ToString().Substring(payrollId.ToString().Length - 2);

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("payroll@citytire.com", "IT Team")); //jordan.blackwood     harsha.yerramsetty     payroll
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.Subject = "Corporate Payroll Submission";
            msg.Body = "<i><u><b>Corporate Payroll Submission</b></u></i>" +
                "<br /><br />" +
                "Payroll ID: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp  " + "<font color='black'>" + paystr + "</font>" + " <br />" +
                "Payroll End Date: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp " + "<font color='black'>" + Convert.ToDateTime(PayrollEndDate).ToString("dddd, MMMM dd, yyyy") + "</font>" + " <br />" +
                "Submitted By: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp" + "<font color='black'>" + submitterName + "</font>" + " <br />" +
                "Submitted Date-Time: &nbsp &nbsp &nbsp &nbsp" + "<font color='black'>" + nstTime.ToString("dddd, MMMM dd, yyyy h:mm tt") + "</font>" + " <br /><br />" +
                
                "<i><b><font size=1>SEHUB Automated Email Notifications</font></b></i>";
            msg.IsBodyHtml = true;

            string path1 = Path.Combine(Server.MapPath("~/Content/Corporate_Submission.csv"));
            msg.Attachments.Add(new Attachment(path1));

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                //Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            string locationid = locId;

            int firstEmployee = db.tbl_employee.Where(x => x.loc_ID == locationid).OrderBy(x => x.full_name).Select(x => x.employee_id).FirstOrDefault();

            return RedirectToAction("Payroll", "Management", new { locId = locationid, employeeId = firstEmployee, payrollID = payrollId, ac = "DashboardCorporate" });
        }

        [HttpPost]
        public ActionResult SubmitEmployeePayrollCorporate(EmployeePayrollModel model, string locId)
        {


            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            int UserempId = Convert.ToInt32(Session["userID"].ToString());

            int empID = Convert.ToInt32(model.SelectedEmployeeId); //model.employeepayroll.payBiweek.employee_id
            int payrollId = Convert.ToInt32(model.SelectedPayrollId.Split(';')[0]); //model.employeepayroll.payBiweek.payroll_id

            tbl_payroll_submission_branch objCourse1 = new tbl_payroll_submission_branch();

            //Trace.WriteLine(" This is the employee ID : " + empID + "and this is the payrollID : " + payrollId);

            var PayrollCorporateSbmissionDetails = db.tbl_employee_payroll_submission.Where(x => x.employee_id == empID && x.payroll_id == payrollId && x.location_id == locId).FirstOrDefault();
            var PayrollCorporateSbmissionDetailsFinal = db.tbl_employee_payroll_final.Where(x => x.employee_id == empID && x.payroll_id == payrollId && x.location_id == locId).FirstOrDefault();

            PayrollCorporateSbmissionDetails.submission_id = Convert.ToString(empID) + Convert.ToString(payrollId);
            PayrollCorporateSbmissionDetails.payroll_id = payrollId;
            PayrollCorporateSbmissionDetails.employee_id = empID;
            PayrollCorporateSbmissionDetails.submitted_by = Convert.ToString(UserempId);
            PayrollCorporateSbmissionDetails.submission_date = System.DateTime.Today;

            PayrollCorporateSbmissionDetails.regular = model.employeepayrollSubmission.regular;
            PayrollCorporateSbmissionDetails.ot = model.employeepayrollSubmission.ot;
            PayrollCorporateSbmissionDetails.vacationTime = model.employeepayrollSubmission.vacationTime;
            PayrollCorporateSbmissionDetails.sickTime = model.employeepayrollSubmission.sickTime;
            PayrollCorporateSbmissionDetails.StatutoryTime = model.employeepayrollSubmission.StatutoryTime;
            PayrollCorporateSbmissionDetails.PaidLeave = model.employeepayrollSubmission.PaidLeave;
            PayrollCorporateSbmissionDetails.NonPaidLeave = model.employeepayrollSubmission.NonPaidLeave;
            PayrollCorporateSbmissionDetails.commission = model.employeepayrollSubmission.commission;
            PayrollCorporateSbmissionDetails.callCommission = model.employeepayrollSubmission.callCommission;
            PayrollCorporateSbmissionDetails.otherPay = model.employeepayrollSubmission.otherPay;
            PayrollCorporateSbmissionDetails.vacationPay = model.employeepayrollSubmission.vacationPay;
            PayrollCorporateSbmissionDetails.adjustment_type = model.employeepayrollSubmission.adjustment_type;
            PayrollCorporateSbmissionDetails.adjustment_type1 = model.employeepayrollSubmission.adjustment_type1;
            PayrollCorporateSbmissionDetails.adjustment_type2 = model.employeepayrollSubmission.adjustment_type2;
            PayrollCorporateSbmissionDetails.adjustmentPay = model.employeepayrollSubmission.adjustmentPay;
            PayrollCorporateSbmissionDetails.adjustmentPay1 = model.employeepayrollSubmission.adjustmentPay1;
            PayrollCorporateSbmissionDetails.adjustmentPay2 = model.employeepayrollSubmission.adjustmentPay2;
            PayrollCorporateSbmissionDetails.comments = model.employeepayrollSubmission.comments;
            PayrollCorporateSbmissionDetails.plus_minus = model.employeepayrollSubmission.plus_minus;
            PayrollCorporateSbmissionDetails.plus_minus1 = model.employeepayrollSubmission.plus_minus1;
            PayrollCorporateSbmissionDetails.plus_minus2 = model.employeepayrollSubmission.plus_minus2;
 
            string typeForCheck = model.employeepayrollSubmission.adjustment_type;
            string type1ForCheck = model.employeepayrollSubmission.adjustment_type1;
            string type2ForCheck = model.employeepayrollSubmission.adjustment_type2;
            string pmForCheck = model.employeepayrollSubmission.plus_minus;
            string pm1ForCheck = model.employeepayrollSubmission.plus_minus1;
            string pm2ForCheck = model.employeepayrollSubmission.plus_minus2;

            double adjForCheck;
            double adj1ForCheck;
            double adj2ForCheck;

            if (model.employeepayrollSubmission.adjustmentPay.HasValue)
            {
                adjForCheck = model.employeepayrollSubmission.adjustmentPay.Value;
            }
            else
            {
                adjForCheck = 0;
            }

            if (model.employeepayrollSubmission.adjustmentPay1.HasValue)
            {
                adj1ForCheck = model.employeepayrollSubmission.adjustmentPay1.Value;
            }
            else
            {
                adj1ForCheck = 0;
            }
            
            if (model.employeepayrollSubmission.adjustmentPay2.HasValue)
            {
                adj2ForCheck = model.employeepayrollSubmission.adjustmentPay2.Value;
            }
            else
            {
                adj2ForCheck = 0;
            }


            string compensationType = db.tbl_employee_status.Where(x => x.employee_id == empID).Select(x => x.compensation_type).FirstOrDefault();
            
            PayrollCorporateSbmissionDetails.compensation_type = compensationType;
            
            if (PayrollCorporateSbmissionDetails.commission.HasValue && PayrollCorporateSbmissionDetails.callCommission.HasValue && PayrollCorporateSbmissionDetails.otherPay.HasValue && PayrollCorporateSbmissionDetails.vacationPay.HasValue)
            {
                PayrollCorporateSbmissionDetails.recordFlag = 2;
            }
            else
            {
                PayrollCorporateSbmissionDetails.recordFlag = 0;
            }
            
            if(PayrollCorporateSbmissionDetailsFinal != null)
            {
                PayrollCorporateSbmissionDetailsFinal.payroll_id = payrollId;
                PayrollCorporateSbmissionDetailsFinal.employee_id = empID;
                PayrollCorporateSbmissionDetailsFinal.end_date = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == payrollId).Select(x => x.end_date).FirstOrDefault().Value.ToString("dd-mm-yyyy");
                PayrollCorporateSbmissionDetailsFinal.location_id = locId;
                PayrollCorporateSbmissionDetailsFinal.full_name = db.tbl_employee.Where(x => x.employee_id == empID).Select(x => x.full_name).FirstOrDefault().Replace(',', ' ');
                PayrollCorporateSbmissionDetailsFinal.CommissionPay_D = model.employeepayrollSubmission.commission + checkAdjustments("Commission", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck); 
                PayrollCorporateSbmissionDetailsFinal.StatutoryHolidayPay_H = model.employeepayrollSubmission.StatutoryTime + checkAdjustments("Statutory", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                PayrollCorporateSbmissionDetailsFinal.SalaryPay_D = 0;
                PayrollCorporateSbmissionDetailsFinal.VacationPay_D = model.employeepayrollSubmission.vacationPay +  checkAdjustments("Vacation Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck); 
                
                if(model.employeepayrollSubmission.regular != null)
                {
                    PayrollCorporateSbmissionDetailsFinal.RegularPay_H = model.employeepayrollSubmission.regular + checkAdjustments("Regular Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                }
                else
                {
                    PayrollCorporateSbmissionDetailsFinal.RegularPay_H = 0 + checkAdjustments("Regular Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                }

                //Trace.WriteLine("This is the Other Pay" + model.employeepayrollSubmission.otherPay);
                //Trace.WriteLine("This is the adjusted Other pay " + checkAdjustments("Other Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck));

                PayrollCorporateSbmissionDetailsFinal.OvertimePay_H = model.employeepayrollSubmission.ot + checkAdjustments("Over Time", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);

                //Trace.WriteLine( "This is the adjustment " + checkAdjustments("Over Time", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck));

                Trace.WriteLine("This is the OT corp " + model.employeepayrollSubmission.ot + " and the final OT is " + PayrollCorporateSbmissionDetailsFinal.OvertimePay_H);


                PayrollCorporateSbmissionDetailsFinal.OtherPay_D = model.employeepayrollSubmission.otherPay + checkAdjustments("Other Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                PayrollCorporateSbmissionDetailsFinal.SickLeave_H = model.employeepayrollSubmission.sickTime + checkAdjustments("Sick Time", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                PayrollCorporateSbmissionDetailsFinal.VacationTime_H = model.employeepayrollSubmission.vacationTime + checkAdjustments("Vacation", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                PayrollCorporateSbmissionDetailsFinal.OnCallCommission_D = model.employeepayrollSubmission.callCommission + checkAdjustments("On-Call Comission", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);

                if (db.tbl_employee_status.Where(x => x.employee_id == empID).Select(x =>x.vacation).FirstOrDefault() == 10 && PayrollCorporateSbmissionDetailsFinal.OvertimePay_H != null)
                {                    
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_2_H = PayrollCorporateSbmissionDetailsFinal.OvertimePay_H;
                }
                else
                {
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_2_H = 0;
                }

                if(db.tbl_employee_status.Where(x => x.employee_id == empID).Select(x =>x.vacation).FirstOrDefault() == 15 && PayrollCorporateSbmissionDetailsFinal.OvertimePay_H != null)
                {
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_3_H = PayrollCorporateSbmissionDetailsFinal.OvertimePay_H;                    
                }
                else
                {
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_3_H = 0;
                }

                if (PayrollCorporateSbmissionDetailsFinal.RegularPay_H == null)
                {
                    PayrollCorporateSbmissionDetailsFinal.RegularPay_H = 0;
                }

                if (PayrollCorporateSbmissionDetailsFinal.OvertimePay_H == null)
                {
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_H = 0;
                }

                if (PayrollCorporateSbmissionDetailsFinal.OvertimePay_2_H == null)
                {
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_2_H = 0;
                }

                if (PayrollCorporateSbmissionDetailsFinal.OvertimePay_3_H == null)
                {
                    PayrollCorporateSbmissionDetailsFinal.OvertimePay_3_H = 0;
                }

                PayrollCorporateSbmissionDetailsFinal.OvertimePay_H = 0;
            }
            else
            {
                tbl_employee_payroll_final final_table = new tbl_employee_payroll_final();
                final_table.payroll_id = payrollId;
                final_table.employee_id = empID;
                final_table.end_date = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == payrollId).Select(x => x.end_date).FirstOrDefault().Value.ToString("dd-mm-yyyy");
                final_table.location_id = locId;
                final_table.full_name = db.tbl_employee.Where(x => x.employee_id == empID).Select(x => x.full_name).FirstOrDefault().Replace(',', '_');
                final_table.location_id = locId;
                final_table.CommissionPay_D = model.employeepayrollSubmission.commission + checkAdjustments("Commission", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                final_table.StatutoryHolidayPay_H = model.employeepayrollSubmission.StatutoryTime + checkAdjustments("Statutory", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                final_table.SalaryPay_D = 0;
                final_table.VacationPay_D = model.employeepayrollSubmission.vacationPay + checkAdjustments("Vacation Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                //final_table.RegularPay_H = model.employeepayrollSubmission.regular + checkAdjustments("Regular Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);

                if (model.employeepayrollSubmission.regular != null)
                {
                    final_table.RegularPay_H = model.employeepayrollSubmission.regular + checkAdjustments("Regular Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                }
                else
                {
                    final_table.RegularPay_H = 0 + checkAdjustments("Regular Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                }

                //Trace.WriteLine("This is the Other Pay" + model.employeepayrollSubmission.otherPay);
                //Trace.WriteLine("This is the adjusted Other pay " + checkAdjustments("Other Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck));

                final_table.OvertimePay_H = model.employeepayrollSubmission.ot + checkAdjustments("Over Time", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                final_table.OtherPay_D = model.employeepayrollSubmission.otherPay + checkAdjustments("Other Pay", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                final_table.SickLeave_H = model.employeepayrollSubmission.sickTime + checkAdjustments("Sick Time", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                final_table.VacationTime_H = model.employeepayrollSubmission.vacationTime + checkAdjustments("Vacation", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);
                final_table.OnCallCommission_D = model.employeepayrollSubmission.callCommission + checkAdjustments("On-Call Comission", typeForCheck, type1ForCheck, type2ForCheck, pmForCheck, pm1ForCheck, pm2ForCheck, adjForCheck, adj1ForCheck, adj2ForCheck);


                if (db.tbl_employee_status.Where(x => x.employee_id == empID).Select(x => x.vacation).FirstOrDefault() == 10)
                {
                    final_table.OvertimePay_2_H = final_table.OvertimePay_H;
                }
                else
                {
                    final_table.OvertimePay_2_H = 0;
                }

                if (db.tbl_employee_status.Where(x => x.employee_id == empID).Select(x => x.vacation).FirstOrDefault() == 15)
                {
                    final_table.OvertimePay_3_H = final_table.OvertimePay_H;
                }
                else
                {
                    final_table.OvertimePay_3_H = 0;
                }

                if (final_table.RegularPay_H == null)
                {
                    final_table.RegularPay_H = 0;
                }

                if (final_table.OvertimePay_H == null)
                {
                    final_table.OvertimePay_H = 0;
                }

                if (final_table.OvertimePay_2_H == null)
                {
                    final_table.OvertimePay_2_H = 0;
                }

                if (final_table.OvertimePay_3_H == null)
                {
                    final_table.OvertimePay_3_H = 0;
                }

                final_table.OvertimePay_H = 0;

                db.tbl_employee_payroll_final.Add(final_table);

            }

            db.SaveChanges();
            return RedirectToAction("Payroll", new { locId = locId, employeeId = empID, payrollID = payrollId, ac = "Corporate" });
        }

        public double checkAdjustments(string actualType, string type, string type1, string type2, string pm, string pm1, string pm2, double adj, double adj1, double adj2)
        {
            double finalAdjustment = 0;

            if(actualType == type)
            {
                if(pm == "-")
                {
                    finalAdjustment = finalAdjustment - adj;
                }
                if(pm == "+")
                {
                    finalAdjustment = finalAdjustment + adj;
                }

            }

            if(actualType == type1)
            {
                if(pm1 == "-")
                {
                    finalAdjustment = finalAdjustment - adj1;
                }
                if(pm1 == "+")
                {
                    finalAdjustment = finalAdjustment + adj1;
                }

            }

            if(actualType == type2)
            {
                if(pm2 == "-")
                {
                    finalAdjustment = finalAdjustment - adj2;
                }
                if(pm2 == "+")
                {
                    finalAdjustment = finalAdjustment + adj2;
                }

            }


            return finalAdjustment;
        }

        [HttpPost]
        public ActionResult SubmitFromBranchToCorporate(EmployeePayrollModel model, string locId, string pid, string ack)
        {
            int UserempId = Convert.ToInt32(Session["userID"].ToString());
            //Debug.WriteLine("The Selected Payroll ID is" + model.SelectedPayrollId);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string[] checkPayrollID = pid.Split(';');

            int payrollId = Convert.ToInt32(checkPayrollID[0]);

            int NoOfBranchSubmissions = db.tbl_payroll_submission_branch.Where(x => x.payroll_id == payrollId && x.location_id == locId).Count();

            var completedLocationList = db.tbl_payroll_submission_branch.Where(x => x.payroll_id == payrollId).Select(x => x.location_id).Distinct().ToList();

            string paystr = "20" + payrollId.ToString().Substring(0, 2) + " - " + payrollId.ToString().Substring(payrollId.ToString().Length - 2);


            List<string> items = new List<string>();
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            if (NoOfBranchSubmissions > 0)
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE ");
                    sb.Append("tbl_payroll_submission_branch ");
                    sb.Append("SET resubmit = 0");
                    sb.Append("WHERE location_id = @loc AND payroll_id = @payID  ");

                    string sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@loc", locId);
                        command.Parameters.AddWithValue("@payID", payrollId);

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "select loc_id From tbl_cta_location_info where loc_status=1";

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {

                        while (sdr.Read())
                        {
                            items.Add(sdr["loc_id"].ToString());
                        }


                    }
                    con.Close();
                }
            }


            float percentage;
            int compcount;

            if (completedLocationList.Contains(locId))
            {
                compcount = completedLocationList.Count();
            }
            else
            {
                compcount = completedLocationList.Count() + 1;
            }

            percentage = (compcount * 100) / items.Count();

            tbl_payroll_submission_branch objCourse1 = new tbl_payroll_submission_branch();

            //var checkCurrentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).FirstOrDefault();

            //int payrollId = checkCurrentPayrollID.payroll_Id;

            var PrevSubmission = db.tbl_payroll_submission_branch.OrderByDescending(s => s.submission_id).FirstOrDefault();

            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
            DateTime nstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

            if (PrevSubmission != null)
            {
                int tempPrevSub = Convert.ToInt32(PrevSubmission.submission_id);
                objCourse1.submission_id = Convert.ToString(tempPrevSub + 1);
                objCourse1.submitter_empId = UserempId;
                var tempEmpName = db.tbl_employee.Where(x => (x.employee_id == UserempId)).FirstOrDefault();
                if (tempEmpName != null)
                {
                    objCourse1.submitter_name = tempEmpName.full_name;
                }

                objCourse1.submission_date = nstTime;
                objCourse1.payroll_id = payrollId;
                objCourse1.location_id = locId;
                objCourse1.loc_status = 0;
                objCourse1.resubmit = 0;
                //Adds an entity in a pending insert state to this System.Data.Linq.Table<TEntity>and parameter is the entity which to be added  
                db.tbl_payroll_submission_branch.Add(objCourse1);
                // executes the appropriate commands to implement the changes to the database  
                db.SaveChanges();
            }
            else
            {
                objCourse1.submission_id = "1";
                objCourse1.submitter_empId = UserempId;
                var tempEmpName = db.tbl_employee.Where(x => (x.employee_id == UserempId)).FirstOrDefault();
                if (tempEmpName != null)
                {
                    objCourse1.submitter_name = tempEmpName.full_name;
                }

                objCourse1.submission_date = nstTime;
                objCourse1.payroll_id = payrollId;
                objCourse1.location_id = locId;
                objCourse1.loc_status = 0;
                objCourse1.resubmit = 0;
                //Adds an entity in a pending insert state to this System.Data.Linq.Table<TEntity>and parameter is the entity which to be added  
                db.tbl_payroll_submission_branch.Add(objCourse1);
                // executes the appropriate commands to implement the changes to the database  
                db.SaveChanges();
            }


            using (var context = new CityTireAndAutoEntities())
            {
                //var rows = context.tbl_employee.Where(x => (x.loc_ID == locId && x.status != 0));

                //var rows2 = context.tbl_employee_payroll_submission.Where(x => (x.location_id == locId && x.payroll_id == payrollId)).FirstOrDefault();

                var rows4 = context.tbl_employee_payroll_biweekly.Where(x => x.loc_id == locId && x.payroll_id == payrollId);

                int currentPayrollID = db.tbl_employee_payroll_dates.Where(x => x.start_date <= DateTime.Today && x.end_date >= DateTime.Today).Select(x => x.payroll_Id).FirstOrDefault();

                foreach (var item in rows4)
                {
                    var employeePresence = context.tbl_employee_payroll_submission.Where(x => (x.employee_id == item.employee_id && x.location_id == item.loc_id && x.payroll_id == item.payroll_id)).FirstOrDefault();

                    if(employeePresence != null)
                    {
                        CityTireAndAutoEntities OdContext = new CityTireAndAutoEntities();
                        tbl_employee_payroll_submission objCourse = new tbl_employee_payroll_submission();
                        objCourse.submission_id = item.employee_id.ToString() + payrollId;
                        objCourse.employee_id = item.employee_id;
                        objCourse.payroll_id = payrollId;
                        objCourse.location_id = locId;

                        var SummeryDetails = db.tbl_employee_payroll_summary.Where(x => (x.employee_id == item.employee_id && x.payroll_id == payrollId && x.loc_id == item.loc_id)).FirstOrDefault();
                        var subDetails = db.tbl_employee_payroll_submission.Where(x => (x.employee_id == item.employee_id && x.payroll_id == payrollId && x.location_id == item.loc_id)).FirstOrDefault();

                        if (SummeryDetails != null)
                        {
                            string empposit = db.tbl_employee.Where(x => x.employee_id == item.employee_id).Select(x => x.cta_position).FirstOrDefault();

                            int OTeligible = db.tbl_position_info.Where(x => x.PositionTitle == empposit).Select(x => x.OverTimeEligible).FirstOrDefault().Value;

                            string compstype = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault();

                            if(OTeligible == 0 && compstype == "Hourly")
                            {
                                objCourse.regular = SummeryDetails.regular + SummeryDetails.ot;
                                objCourse.ot = 0;

                                double TotalHours = (objCourse.regular + SummeryDetails.vac + SummeryDetails.sick + SummeryDetails.stat).Value;

                                if(TotalHours <= 80)
                                {

                                }
                                else
                                {
                                    objCourse.regular = objCourse.regular + 80 - TotalHours;
                                }


                            }
                            else
                            {
                                objCourse.regular = SummeryDetails.regular;
                                objCourse.ot = SummeryDetails.ot;
                            }

                            objCourse.vacationTime = SummeryDetails.vac;
                            objCourse.sickTime = SummeryDetails.sick;
                            objCourse.StatutoryTime = SummeryDetails.stat;

                            objCourse.commission = subDetails.commission;
                            objCourse.callCommission = subDetails.callCommission;
                            objCourse.otherPay = subDetails.otherPay;
                            objCourse.vacationPay = subDetails.vacationPay;

                            objCourse.recordFlag = 1;

                            objCourse.adjustmentPay = subDetails.adjustmentPay;
                            objCourse.adjustmentPay1 = subDetails.adjustmentPay1;
                            objCourse.adjustmentPay2 = subDetails.adjustmentPay2;
                            objCourse.adjustment_type = subDetails.adjustment_type;
                            objCourse.adjustment_type1 = subDetails.adjustment_type1;
                            objCourse.adjustment_type2 = subDetails.adjustment_type2;
                            objCourse.plus_minus = subDetails.plus_minus;
                            objCourse.plus_minus1 = subDetails.plus_minus1;
                            objCourse.plus_minus2 = subDetails.plus_minus2;
                            objCourse.comments = subDetails.comments;
                            objCourse.compensation_type = subDetails.compensation_type;

                            

                        }

                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("delete from tbl_employee_payroll_submission where employee_id = @employee_id and payroll_id = @Payroll and location_id = @loc");
                            string sql = sb.ToString();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@employee_id", item.employee_id);
                                command.Parameters.AddWithValue("@Payroll", item.payroll_id);
                                command.Parameters.AddWithValue("@loc", item.loc_id);
                                command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }

                            connection.Close();
                        }

                        OdContext.tbl_employee_payroll_submission.Add(objCourse);
                        // executes the appropriate commands to implement the changes to the database  
                        OdContext.SaveChanges();
                    }

                    else
                    {
                        CityTireAndAutoEntities OdContext = new CityTireAndAutoEntities();
                        tbl_employee_payroll_submission objCourse = new tbl_employee_payroll_submission();
                        objCourse.submission_id = item.employee_id.ToString() + payrollId;
                        objCourse.employee_id = item.employee_id;
                        objCourse.payroll_id = payrollId;
                        objCourse.location_id = locId;

                        var SummeryDetails = db.tbl_employee_payroll_summary.Where(x => (x.employee_id == item.employee_id && x.payroll_id == payrollId && x.loc_id == item.loc_id)).FirstOrDefault();

                        if (SummeryDetails != null)
                        {

                            string empposit = db.tbl_employee.Where(x => x.employee_id == item.employee_id).Select(x => x.cta_position).FirstOrDefault();

                            int OTeligible = db.tbl_position_info.Where(x => x.PositionTitle == empposit).Select(x => x.OverTimeEligible).FirstOrDefault().Value;

                            string compstype = db.tbl_employee_status.Where(x => x.employee_id == item.employee_id).Select(x => x.compensation_type).FirstOrDefault();


                            if (OTeligible == 0 && compstype == "Hourly")
                            {
                                objCourse.regular = SummeryDetails.regular + SummeryDetails.ot;
                                objCourse.ot = 0;

                                double TotalHours = (objCourse.regular + SummeryDetails.vac + SummeryDetails.sick + SummeryDetails.stat).Value;

                                if (TotalHours <= 80)
                                {

                                }
                                else
                                {
                                    objCourse.regular = objCourse.regular + 80 - TotalHours;
                                }


                            }
                            else
                            {
                                objCourse.regular = SummeryDetails.regular;
                                objCourse.ot = SummeryDetails.ot;
                            }


                            objCourse.ot = SummeryDetails.ot;
                            objCourse.vacationTime = SummeryDetails.vac;
                            objCourse.sickTime = SummeryDetails.sick;
                            objCourse.StatutoryTime = SummeryDetails.stat;

                            objCourse.commission = 0.0;
                            objCourse.callCommission = 0.0;
                            objCourse.otherPay = 0.0;
                            objCourse.vacationPay = 0.0;

                        }

                        OdContext.tbl_employee_payroll_submission.Add(objCourse);
                        // executes the appropriate commands to implement the changes to the database  
                        OdContext.SaveChanges();
                    }
                }


            }

            string locationid = locId;

            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("payroll@citytire.com", "IT Team")); //jordan.blackwood      harsha.yerramsetty      payroll
            msg.From = new MailAddress("noreply@citytire.com", "Sehub");
            msg.CC.Add(new MailAddress(db.tbl_cta_location_info.Where(x => x.loc_id == locId).Select(x => x.management_email).FirstOrDefault()));
            msg.Subject = "Branch Payroll Submission – " + locId;
            msg.Body = "<i><u><b>Branch Payroll Submission</b></u></i>" +
                "<br /><br />" +
                "Location ID: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp" + "<font color='black'>" + locId + "</font>" + "<br />" +
                "Payroll ID: &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp " + "<font color='black'>" + paystr + "</font>" + " <br />" +
                "Total Branch Progress: &nbsp &nbsp &nbsp " + "<font color='black'>" + percentage + "</font>" + "% &nbsp" + "( " + "<font color='black'>" + compcount + "</font>" + "&nbsp of &nbsp" + "<font color='black'>" + items.Count() + "</font>" + " ) <br /><br />" +
                "<i><b><font size=1>SEHUB Automated Email Notifications</font></b></i>";

            //"There is a branch to corporate payroll submission from " + locId + " for the pay period " + "20" + payrollId.ToString().Substring(0, 2) + "-" + payrollId.ToString().Substring(payrollId.ToString().Length - 2) + "<br /> Location IDs which have submitted  till now from branch to corporate are : " + locationNumber + " for payroll ID : " + "20" + payrollId.ToString().Substring(0, 2) + "-" + payrollId.ToString().Substring(payrollId.ToString().Length - 2);
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("noreply@citytire.com", "U8LH>WpBdXg}");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                //Debug.WriteLine("Message Sent Succesfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }


            var currentPayroll = db.tbl_employee_payroll_dates.Where(x => x.start_date < DateTime.Now && x.end_date > DateTime.Now).FirstOrDefault();

            if (payrollId == currentPayroll.payroll_Id)
            {
                var firstEmployee = db.tbl_employee.Where(x => x.loc_ID == locId && x.status == 1).OrderBy(x => x.full_name).FirstOrDefault();
                return RedirectToAction("Payroll", new { locId = locId, employeeId = firstEmployee.employee_id, payrollID = payrollId, ac = "Dashboard" }); //, ac = "Dashboard" 
            }
            else
            {
                var db11 = (from a in db.tbl_employee_payroll_biweekly.Where(x => x.loc_id == locId && x.payroll_id == payrollId) select a).ToList();
                var db22 = (from a in db.tbl_employee select a).ToList();

                var firstEmployee = (from a in db11
                                     join b in db22 on a.employee_id equals b.employee_id
                                     orderby b.full_name
                                     select new { employee_id = a.employee_id, recordflag = a.recordflag }).FirstOrDefault();
                var firstEmployeeprevpayroll = firstEmployee.employee_id.ToString();
                return RedirectToAction("Payroll", new { locId = locId, employeeId = firstEmployeeprevpayroll, payrollID = payrollId, ac = "Dashboard" }); //, ac = "Dashboard" 
            }

        }

        public string SelectedCategoryValue(int value)
        {

            //Trace.WriteLine("This is the value of category" + value);

            string returnvalue = null;
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            var details = db.tbl_payroll_category_selection.Where(x => x.category_id == value).FirstOrDefault();
            if (details != null)
            {
                returnvalue = details.category;
            }
            else
            {
                returnvalue = "";
            }
            return returnvalue;
        }

        private void loadattendanceInformation(string employeeid, string locID)
        {

            string todayDate = DateTime.Today.AddDays(12).ToString("yyyy-MM-dd");
            string currentPayRollId = "";
            string startDate = "";
            string endDate = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select * FRom  tbl_employee_payroll_dates where start_date <=@SDate and end_date >= @SDate ");
                    string sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@SDate", todayDate);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                currentPayRollId = dr["payroll_id"].ToString();
                                startDate = dr["start_date"].ToString();
                                endDate = dr["end_date"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }

            string retrunVal = loadTimeCalculations(employeeid, startDate);
            string[] valuesToBeAssigned = retrunVal.Split(';');
            ViewData["avgInHoursCurrent"] = valuesToBeAssigned[0];
            ViewData["avgOutHoursCurrent"] = valuesToBeAssigned[1];

            LoadAttendanceHelper(employeeid, startDate, endDate, currentPayRollId, locID);
        }

        private void LoadAttendanceHelper(string management_emp_id, string sdate, string edate, string currPayId, string locID)
        {

            //Trace.WriteLine(currPayId);

            double totalDays = (DateTime.Today - Convert.ToDateTime(sdate)).TotalDays;
            string startDate = sdate;
            string endDate = edate;
            string payrollid = currPayId;
            if (totalDays <= 3)
            {
                startDate = DateTime.Today.AddDays(-14 - totalDays).ToString();
                endDate = DateTime.Today.AddDays(-totalDays).ToString();
            }
            ViewData["payId"] = payrollid;
            ViewData["payFrom"] = Convert.ToDateTime(startDate).ToString("MMMM dd, yyyy");
            ViewData["payTo"] = Convert.ToDateTime(endDate).ToString("MMMM dd, yyyy");
            //Debug.WriteLine("employee_id:" + management_emp_id);
            System.Collections.ArrayList instantArrayList = new System.Collections.ArrayList();
            instantArrayList.Clear();
            string[,] BiWeeklyHours = new string[14, 2];
            //Debug.WriteLine("todayDate :" + Convert.ToDateTime(startDate).ToString("yyyy-MM-dd"));
            DateTime convStartDate = Convert.ToDateTime(Convert.ToDateTime(startDate).ToString("yyyy-MM-dd"));
            ViewData["start_date"] = Convert.ToDateTime(convStartDate).ToString("MMM dd,yyyy");
            ViewData["end_date"] = Convert.ToDateTime(convStartDate.AddDays(13)).ToString("MMM dd,yyyy");

            
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb.Append("Select convert(varchar(5),time_stamp, 108) as hours from tbl_attendance_log where  CONVERT(DATE, time_stamp) = @DailyWise and employee_id = @empId and loc_id = @locID");

                    string sql = sb.ToString();

                    for (int day = 0; day <= 13; day++)
                    {
                        //Debug.WriteLine("Entering looop:" + day);

                        DateTime ConvrunDate = convStartDate.AddDays(day);
                        string runDate = Convert.ToDateTime(ConvrunDate).ToString("yyyy-MM-dd");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@DailyWise", runDate);
                            command.Parameters.AddWithValue("@empId", management_emp_id);
                            command.Parameters.AddWithValue("@locID", locID);

                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                    instantArrayList.Add((dr["hours"].ToString()));
                            }
                            command.Parameters.Clear();
                        }
                        //Debug.WriteLine("instantArrayList.Count" + instantArrayList.Count);
                        if (instantArrayList.Count == 0)
                        {
                            BiWeeklyHours[day, 0] = "0:00";
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else if (instantArrayList.Count == 1)
                        {
                            //Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else
                        {
                            //Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            var comp1 = (int)TimeSpan.Parse(instantArrayList[0].ToString()).TotalMinutes;
                            var comp2 = (int)TimeSpan.Parse(instantArrayList[instantArrayList.Count - 1].ToString()).TotalMinutes;
                            if (comp1 > comp2)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[instantArrayList.Count - 1].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[0].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "16:30";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "16:30";
                                }

                            }
                            else if (comp2 > comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "16:30";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "16:30";
                                }
                            }
                            else if (comp2 == comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                            }

                        }

                        instantArrayList.Clear();
                    }


                    connection.Close();
                }
            }
            catch (Exception e2)
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet :" + e2);
            }
            //

            string[,] dailyHours = new string[14, 2];
            for (int i = 0; i <= 13; i++)
            {
                var firstSignIn = BiWeeklyHours[i, 0];
                var LastSignIn = BiWeeklyHours[i, 1];


                var leftMinutes = (int)TimeSpan.Parse(firstSignIn).TotalMinutes;
                var rightMinutes = (int)TimeSpan.Parse(LastSignIn).TotalMinutes;

                if (leftMinutes != 0 && rightMinutes != 0)
                {
                    dailyHours[i, 0] = (Math.Abs((leftMinutes - rightMinutes)) / 60).ToString();
                    dailyHours[i, 1] = (Math.Abs((leftMinutes - rightMinutes)) % 60).ToString();
                }
                else
                {
                    dailyHours[i, 0] = "";
                    dailyHours[i, 1] = "";
                }
            }

            string[] totalString = new string[14];
            for (int i = 0; i <= 13; i++)
            {
                if (dailyHours[i, 0] == "" && dailyHours[i, 1] == "")
                {
                    totalString[i] = "";
                }
                else
                {
                    if (dailyHours[i, 0].Length == 1)
                    {
                        dailyHours[i, 0] = "0" + dailyHours[i, 0][0];
                    }
                    if (dailyHours[i, 1].Length == 1)
                    {
                        dailyHours[i, 1] = "0" + dailyHours[i, 1][0];
                    }
                    //Debug.WriteLine("666:" + dailyHours[i, 0]);
                    float totHrs = (float)Convert.ToDouble(dailyHours[i, 0]);
                    //Debug.WriteLine("666:" + totHrs.ToString());
                    float totMins = (float)Convert.ToDouble(dailyHours[i, 1]);
                    totalString[i] = dailyHours[i, 0] + ":" + dailyHours[i, 1];
                }
            }

            ViewData["TOT1"] = totalString[0];
            ViewData["TOT2"] = totalString[1];
            ViewData["TOT3"] = totalString[2];
            ViewData["TOT4"] = totalString[3];
            ViewData["TOT5"] = totalString[4];
            ViewData["TOT6"] = totalString[5];
            ViewData["TOT7"] = totalString[6];
            ViewData["TOT8"] = totalString[7];
            ViewData["TOT9"] = totalString[8];
            ViewData["TOT10"] = totalString[9];
            ViewData["TOT11"] = totalString[10];
            ViewData["TOT12"] = totalString[11];
            ViewData["TOT13"] = totalString[12];
            ViewData["TOT14"] = totalString[13];

            for (int i = 0; i <= 13; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (BiWeeklyHours[i, j] == "0:00")
                    {
                        BiWeeklyHours[i, j] = "";
                    }
                }
            }

            ViewData["IN1"] = BiWeeklyHours[0, 0];
            ViewData["IN2"] = BiWeeklyHours[1, 0];
            ViewData["IN3"] = BiWeeklyHours[2, 0];
            ViewData["IN4"] = BiWeeklyHours[3, 0];
            ViewData["IN5"] = BiWeeklyHours[4, 0];
            ViewData["IN6"] = BiWeeklyHours[5, 0];
            ViewData["IN7"] = BiWeeklyHours[6, 0];
            ViewData["IN8"] = BiWeeklyHours[7, 0];
            ViewData["IN9"] = BiWeeklyHours[8, 0];
            ViewData["IN10"] = BiWeeklyHours[9, 0];
            ViewData["IN11"] = BiWeeklyHours[10, 0];
            ViewData["IN12"] = BiWeeklyHours[11, 0];
            ViewData["IN13"] = BiWeeklyHours[12, 0];
            ViewData["IN14"] = BiWeeklyHours[13, 0];
            //this.OUT0.Text = "0:00";
            //this.OUT1.Text = "0:00";
            ViewData["OUT1"] = BiWeeklyHours[0, 1];
            ViewData["OUT2"] = BiWeeklyHours[1, 1];
            ViewData["OUT3"] = BiWeeklyHours[2, 1];
            ViewData["OUT4"] = BiWeeklyHours[3, 1];
            ViewData["OUT5"] = BiWeeklyHours[4, 1];
            ViewData["OUT6"] = BiWeeklyHours[5, 1];
            ViewData["OUT7"] = BiWeeklyHours[6, 1];
            ViewData["OUT8"] = BiWeeklyHours[7, 1];
            ViewData["OUT9"] = BiWeeklyHours[8, 1];
            ViewData["OUT10"] = BiWeeklyHours[9, 1];
            ViewData["OUT11"] = BiWeeklyHours[10, 1];
            ViewData["OUT12"] = BiWeeklyHours[11, 1];
            ViewData["OUT13"] = BiWeeklyHours[12, 1];
            ViewData["OUT14"] = BiWeeklyHours[13, 1];
            //End of Table 3

        }

        [HttpPost]
        public ActionResult ChangeLocAttendance(AttendanceModel model)
        {
            return RedirectToAction("Attendance", new { locId = model.MatchedLocID, employeeId = "" });
        }

        [HttpPost]
        public ActionResult ChangeLocTraining(TrainingViewModel model)
        {
            return RedirectToAction("Training", new { loc = model.location});
        }



        [HttpGet]
        public ActionResult NewHire()
        {
            return View();
        }

        [HttpGet]
        public ActionResult VacationSchedule()
        {
            return View();
        }

        private void loadattendanceInfo(string employeeid)
        {

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int empid = Convert.ToInt32(employeeid);

            string location_id = db.tbl_employee.Where(x => x.employee_id == empid).Select(x => x.loc_ID).FirstOrDefault();

            string todayDate = DateTime.Today.ToString("yyyy-MM-dd");
            string currentPayRollId = "";
            string startDate = "";
            string endDate = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select * FRom  tbl_employee_payroll_dates where start_date <=@SDate and end_date >= @SDate ");
                    string sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@SDate", todayDate);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                currentPayRollId = dr["payroll_id"].ToString();
                                startDate = dr["start_date"].ToString();
                                endDate = dr["end_date"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }

            string retrunVal = loadTimeCalculations(employeeid, startDate);
            string[] valuesToBeAssigned = retrunVal.Split(';');
            ViewData["avgInHoursCurrent"] = valuesToBeAssigned[0];
            ViewData["avgOutHoursCurrent"] = valuesToBeAssigned[1];
            ViewData["BiweekHours"] = valuesToBeAssigned[2];

            string status = checkIfAverageExists(employeeid, currentPayRollId);

            if (status == "NoAverage")
            {
                int last_payroll = Int32.Parse(currentPayRollId) - 1;
                string Previousstatus = checkIfAverageExists(employeeid, last_payroll.ToString());
                float totalAverageInHrs = 0;
                float totalAverageOutHrs = 0;

                System.Collections.ArrayList arrayLIST = new System.Collections.ArrayList();
                arrayLIST.Clear();
                try
                {

                    using (SqlConnection connection = new SqlConnection(constr))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("select * From tbl_employee_payroll_dates where payroll_Id < @payid");
                        string sql = sb.ToString();

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {

                            command.Parameters.AddWithValue("@payid", currentPayRollId);
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    arrayLIST.Add((dr["start_date"].ToString()));
                                }

                            }
                            command.Parameters.Clear();
                        }
                        connection.Close();
                    }
                }
                catch (SqlException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
                if (Previousstatus == "NoAverage")
                {
                    int InCount = 0;

                    int OutCount = 0;
                    foreach (object o in arrayLIST)
                    {
                        string retrunValues = loadTimeCalculations(employeeid, o.ToString());

                        string[] returnVals = retrunValues.Split(';');
                        string inVal = returnVals[0];
                        string outVal = returnVals[1];
                        if (inVal != "No Data")
                        {
                            int comp1 = (int)TimeSpan.Parse(inVal).TotalMinutes;
                            totalAverageInHrs = totalAverageInHrs + comp1;
                            InCount = InCount + 1;
                        }
                        if (outVal != "No Data")
                        {
                            int comp1 = (int)TimeSpan.Parse(outVal).TotalMinutes;
                            totalAverageOutHrs = totalAverageOutHrs + comp1;
                            OutCount = OutCount + 1;
                        }
                    }
                    totalAverageInHrs = totalAverageInHrs / InCount;
                    totalAverageOutHrs = totalAverageOutHrs / OutCount;

                    string inHrs = ((int)totalAverageInHrs / 60).ToString();
                    string inMins = ((int)totalAverageInHrs % 60).ToString();
                    string outHrs = ((int)totalAverageOutHrs / 60).ToString();
                    string outMins = ((int)totalAverageOutHrs % 60).ToString();

                    if (inHrs.Length < 2)
                    {

                        inHrs = "0" + inHrs;
                    }
                    if (inMins.Length < 2)
                    {
                        inMins = "0" + inMins;
                    }
                    if (outHrs.Length < 2)
                    {
                        outHrs = "0" + outHrs;
                    }
                    if (outMins.Length < 2)
                    {
                        outMins = "0" + outMins;
                    }

                    if (Convert.ToInt32(inHrs) < 0)
                    {
                        ViewData["avgInHoursOverall"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgInHoursOverall"] = inHrs + ":" + inMins;
                    }

                    if (Convert.ToInt32(outHrs) < 0)
                    {
                        ViewData["avgOutHoursOverall.Text"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgOutHoursOverall"] = outHrs + ":" + outMins;
                    }

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("insert into tbl_attendance_management values(@employee_id,@last_payroll,@Average_In, @Average_out)");
                            string sql = sb.ToString();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@employee_id", employeeid);
                                command.Parameters.AddWithValue("@last_payroll", last_payroll);
                                command.Parameters.AddWithValue("@Average_In", inHrs + ":" + inMins);
                                command.Parameters.AddWithValue("@Average_out", outHrs + ":" + outMins);
                                command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }

                            connection.Close();
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
                    }
                }
                else
                {
                    string date = "";
                    try
                    {

                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("select * From tbl_employee_payroll_dates where payroll_Id = @payid");
                            string sql = sb.ToString();

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@payid", last_payroll);
                                using (SqlDataReader dr = command.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        date = ((dr["start_date"].ToString()));
                                    }

                                }
                                command.Parameters.Clear();
                            }
                            connection.Close();
                        }
                    }
                    catch (SqlException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }



                    string[] previous = Previousstatus.Split(';');
                    string[] PrevIn = previous[0].Split(':');
                    string[] PrevOut = previous[1].Split(':');
                    string retrunValues = loadTimeCalculations(employeeid, date);
                    string[] returnVals = retrunValues.Split(';');
                    string inVal = returnVals[0];
                    string outVal = returnVals[1];
                    if (inVal != "No Data")
                    {
                        int comp1 = (int)TimeSpan.Parse(inVal).TotalMinutes;
                        totalAverageInHrs = totalAverageInHrs + comp1;

                    }
                    if (outVal != "No Data")
                    {
                        int comp1 = (int)TimeSpan.Parse(outVal).TotalMinutes;
                        totalAverageOutHrs = totalAverageOutHrs + comp1;

                    }
                    totalAverageInHrs = (totalAverageInHrs + (int)TimeSpan.Parse(PrevIn[0] + ":" + PrevIn[1]).TotalMinutes) / (arrayLIST.Capacity + 1);
                    totalAverageOutHrs = (totalAverageOutHrs + (int)TimeSpan.Parse(PrevOut[0] + ":" + PrevOut[1]).TotalMinutes) / (arrayLIST.Capacity + 1);

                    string inHrs = ((int)totalAverageInHrs / 60).ToString();
                    string inMins = ((int)totalAverageInHrs % 60).ToString();
                    string outHrs = ((int)totalAverageOutHrs / 60).ToString();
                    string outMins = ((int)totalAverageOutHrs % 60).ToString();

                    if (inHrs.Length < 2)
                    {

                        inHrs = "0" + inHrs;
                    }
                    if (inMins.Length < 2)
                    {
                        inMins = "0" + inMins;
                    }
                    if (outHrs.Length < 2)
                    {
                        outHrs = "0" + outHrs;
                    }
                    if (outMins.Length < 2)
                    {
                        outMins = "0" + outMins;
                    }

                    if (Convert.ToInt32(inHrs) < 0)
                    {
                        ViewData["avgInHoursOverall"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgInHoursOverall"] = inHrs + ":" + inMins;
                    }

                    if (Convert.ToInt32(outHrs) < 0)
                    {
                        ViewData["avgOutHoursOverall"] = "No Data";
                    }
                    else
                    {
                        ViewData["avgOutHoursOverall"] = outHrs + ":" + outMins;
                    }

                    try
                    {



                        using (SqlConnection connection = new SqlConnection(constr))
                        {
                            connection.Open();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("update tbl_attendance_management set last_payroll=@last_payroll,Average_In=@Average_In,Average_out=@Average_out Where employee_id=@employee_id and last_payroll=@previousPayroll");
                            string sql = sb.ToString();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {

                                command.Parameters.AddWithValue("@employee_id", employeeid);
                                command.Parameters.AddWithValue("@previousPayroll", last_payroll - 1);
                                command.Parameters.AddWithValue("@last_payroll", last_payroll);
                                command.Parameters.AddWithValue("@Average_In", inHrs + ":" + inMins);
                                command.Parameters.AddWithValue("@Average_out", outHrs + ":" + outMins);
                                command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }

                            connection.Close();
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
                    }
                }

            }
            else
            {
                string[] previous = status.Split(';');
                string[] PrevIn = previous[0].Split(':');
                string[] PrevOut = previous[1].Split(':');
                ViewData["avgInHoursOverall"] = PrevIn[0] + ":" + PrevIn[1];
                ViewData["avgOutHoursOverall"] = PrevOut[0] + ":" + PrevOut[1];
            }
            LoadPayroll(employeeid, startDate, endDate, currentPayRollId, location_id);

        }

        private string loadTimeCalculations(string employeeid, string startDate)
        {
            System.Collections.ArrayList instantArrayList = new System.Collections.ArrayList();
            instantArrayList.Clear();
            string[,] BiWeeklyHours = new string[14, 2];
            string returnThis = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb.Append("Select convert(varchar(5),time_stamp, 108) as hours from tbl_attendance_log where  CONVERT(DATE, time_stamp) = @DailyWise and employee_id = @empId ");

                    string sql = sb.ToString();

                    for (int day = 0; day <= 13; day++)
                    {
                        DateTime ConvrunDate = Convert.ToDateTime(startDate).AddDays(day);
                        string runDate = Convert.ToDateTime(ConvrunDate).ToString("yyyy-MM-dd");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@DailyWise", runDate);
                            command.Parameters.AddWithValue("@empId", employeeid);
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                    instantArrayList.Add((dr["hours"].ToString()));
                            }
                            command.Parameters.Clear();
                        }

                        if (instantArrayList.Count == 0)
                        {
                            BiWeeklyHours[day, 0] = "0:00";
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else if (instantArrayList.Count == 1)
                        {
                            BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else
                        {
                            var comp1 = (int)TimeSpan.Parse(instantArrayList[0].ToString()).TotalMinutes;
                            var comp2 = (int)TimeSpan.Parse(instantArrayList[instantArrayList.Count - 1].ToString()).TotalMinutes;
                            if (comp1 > comp2)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[instantArrayList.Count - 1].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[0].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "17:00";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "17:00";
                                }

                            }
                            else if (comp2 > comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "17:00";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "17:00";
                                }
                            }
                            else if (comp2 == comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                            }

                        }

                        instantArrayList.Clear();
                    }


                    connection.Close();
                }
            }
            catch (Exception e2)
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet :" + e2);
            }

            string[,] dailyHours = new string[14, 2];
            float checkinTotal = 0;
            int checkInCount = 0;
            float checkOutTotal = 0;
            int checkOutCount = 0;
            for (int i = 0; i <= 13; i++)
            {
                var firstSignIn = BiWeeklyHours[i, 0];
                var LastSignIn = BiWeeklyHours[i, 1];

                if (firstSignIn == null || firstSignIn == "")
                {
                    firstSignIn = "00:00";
                }
                if (LastSignIn == null || LastSignIn == "")
                {
                    LastSignIn = "00:00";
                }
                var leftMinutes = (int)TimeSpan.Parse(firstSignIn).TotalMinutes;
                var rightMinutes = (int)TimeSpan.Parse(LastSignIn).TotalMinutes;

                if (leftMinutes != 0 && rightMinutes != 0)
                {

                    checkinTotal = checkinTotal + leftMinutes;
                    checkOutTotal = checkOutTotal + rightMinutes;
                    dailyHours[i, 0] = (Math.Abs((leftMinutes - rightMinutes)) / 60).ToString();
                    dailyHours[i, 1] = (Math.Abs((leftMinutes - rightMinutes)) % 60).ToString();
                    checkInCount = checkInCount + 1;
                    checkOutCount = checkOutCount + 1;
                }
                else
                {
                    dailyHours[i, 0] = "";
                    dailyHours[i, 1] = "";
                }

                if (leftMinutes != 0 && rightMinutes == 0)
                {

                    checkinTotal = checkinTotal + leftMinutes;
                    checkInCount = checkInCount + 1;
                }
            }
            float checkInAvgHours = Math.Abs((checkinTotal / checkInCount)) / 60;
            float checkInAvgMins = Math.Abs((checkinTotal / checkInCount)) % 60;

            float checkOutAvgHours = Math.Abs((checkOutTotal / checkOutCount)) / 60;
            float checkOutAvgMins = Math.Abs((checkOutTotal / checkOutCount)) % 60;
            if (checkInCount > 0)
            {
                string sInHours = ((int)checkInAvgHours).ToString();
                string sOutMins = ((int)checkInAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }
                returnThis = sInHours + ":" + sOutMins;


            }
            else
            {
                returnThis = "No Data";


            }
            if (checkOutCount > 0)
            {
                string sInHours = ((int)checkOutAvgHours).ToString();
                string sOutMins = ((int)checkOutAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }
                returnThis = returnThis + ";" + sInHours + ":" + sOutMins;

            }
            else
            {
                returnThis = returnThis + ";" + "No Data";
            }

            string[] totalString = new string[14];
            double[] BiWeekString = new double[14];
            string[] BiWeekDates = new string[14];
            float totHrs = 0;
            float totMins = 0;
            for (int i = 0; i <= 13; i++)
            {
                BiWeekDates[i] = Convert.ToDateTime(Convert.ToDateTime(startDate).AddDays(i)).ToString("dd-MM");
                if (dailyHours[i, 0] == "" && dailyHours[i, 1] == "")
                {
                    totalString[i] = "";
                    BiWeekString[i] = 0;
                }
                else
                {
                    if (dailyHours[i, 0].Length == 1)
                    {
                        dailyHours[i, 0] = "0" + dailyHours[i, 0][0];
                    }
                    if (dailyHours[i, 1].Length == 1)
                    {
                        dailyHours[i, 1] = "0" + dailyHours[i, 1][0];
                    }
                    totHrs = (float)Convert.ToDouble(dailyHours[i, 0]) + totHrs;
                    totMins = (float)Convert.ToDouble(dailyHours[i, 1]) + totMins;
                    totalString[i] = dailyHours[i, 0] + ":" + dailyHours[i, 1];
                    double mintValCalc = Convert.ToInt32(dailyHours[i, 1]) / 60;
                    BiWeekString[i] = Convert.ToDouble(dailyHours[i, 0]) + mintValCalc;
                }
            }

            ViewData["BarChartValues"] = BiWeekString;
            ViewData["BarChartDates"] = BiWeekDates;
            float mins = 0;
            if (totMins > 60)
            {
                mins = totMins % 60;
                totHrs = totHrs + (int)(totMins / 60);
            }

            returnThis = returnThis + ";" + totHrs.ToString();

            return returnThis;

        }

        private string checkIfAverageExists(string empId, string currentPayRollId)
        {
            int last_payroll = Int32.Parse(currentPayRollId) - 1;
            string employeeId = "";
            string returnString = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select * from tbl_attendance_management where employee_id = @employee_id and Last_payroll = @lastPayroll");
                    string sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@employee_id", empId);
                        command.Parameters.AddWithValue("@lastPayroll", last_payroll);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                employeeId = dr["employee_id"].ToString();
                                returnString = dr["Average_In"].ToString() + ";" + dr["Average_out"].ToString();
                            }
                        }
                        command.Parameters.Clear();
                    }

                    connection.Close();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet");
            }
            if (employeeId == null || employeeId == "")
            {
                returnString = "NoAverage";
            }

            return returnString;
        }

        public double getDataEmployee(string employeeId)
        {

            string clockCount = "";
            string manualCount = "";
            string clockInCount = "";
            string manualInCount = "";
            string clockOutCount = "";
            string manualOutCount = "";
            string autoOutCount = "";
            string atWork = "";
            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {

                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sbX = new StringBuilder();
                    StringBuilder sbY = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    StringBuilder sb3 = new StringBuilder();
                    StringBuilder sb4 = new StringBuilder();
                    StringBuilder sb5 = new StringBuilder();
                    StringBuilder sb6 = new StringBuilder();
                    sbX.Append("select count(employee_id) as empCount FRom tbl_attendance_log  where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id not in ('manualIN', 'manualOUT')");
                    sbY.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id in ('manualIN', 'manualOUT')");
                    sb1.Append("select count(employee_id) as empCount FRom tbl_attendance_log  where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'manualIN'");
                    sb2.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'clockIN'");
                    sb3.Append("select count(employee_id) as empCount FRom tbl_attendance_log  where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'manualOUT'");
                    sb4.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'clockOUT'");
                    sb5.Append("select count(employee_id) as empCount FRom tbl_attendance_log where employee_id = @employeeId and time_stamp > '2019-06-13' and event_id = 'autoOUT'");
                    sb6.Append("select at_work FRom tbl_employee_attendance where employee_id=@employeeId");

                    string sqlX = sbX.ToString();
                    string sqlY = sbY.ToString();
                    string sql1 = sb1.ToString();
                    string sql2 = sb2.ToString();
                    string sql3 = sb3.ToString();
                    string sql4 = sb4.ToString();
                    string sql5 = sb5.ToString();
                    string sql6 = sb6.ToString();
                    using (SqlCommand command = new SqlCommand(sqlX, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                clockCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sqlY, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                manualCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql1, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                manualInCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                clockInCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql3, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                manualOutCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql4, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                clockOutCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }
                    using (SqlCommand command = new SqlCommand(sql5, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                autoOutCount = dr["empCount"].ToString();

                            }
                        }
                        command.Parameters.Clear();
                    }

                    using (SqlCommand command = new SqlCommand(sql6, connection))
                    {

                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                atWork = dr["at_work"].ToString();


                            }
                        }
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            int a = Int32.Parse(clockCount);
            int b = Int32.Parse(manualCount);
            double percetage = 0.00;
            int sum = a + b;
            if (sum == 0)
            {

            }
            else
            {
                percetage = Math.Round(((double)a / (double)(sum)) * 100);
            }
            // Debug.WriteLine("Percentage :" + percetage);

            ////Calculating for individual employee
            int cIn = Int32.Parse(clockInCount);
            int mIn = Int32.Parse(manualInCount);
            double clockInPer = 0.00;

            if (cIn + mIn == 0)
            {

            }
            else
            {
                clockInPer = Math.Round(((double)cIn / (double)(cIn + mIn)) * 100);
            }

            ViewData["StackedClockInFob"] = clockInPer;
            ViewData["StackedClockInManual"] = 100 - clockInPer;
            //Debug.WriteLine("clockInPer:" + clockInPer);
            int cOut = Int32.Parse(clockOutCount);
            int mOut = Int32.Parse(manualOutCount);
            int aOut = Int32.Parse(autoOutCount);
            double clockOutPer = 0.00;
            double manualOutPer = 0.00;
            double autoOutPer = 0.00;

            if (cOut + mOut + aOut == 0)
            {

            }
            else
            {
                clockOutPer = Math.Round(((double)cOut / (double)(cOut + mOut + aOut)) * 100);
                manualOutPer = Math.Round(((double)mOut / (double)(cOut + mOut + aOut)) * 100);
                autoOutPer = Math.Round(((double)aOut / (double)(cOut + mOut + aOut)) * 100);
            }

            ViewData["StackedClockOutFob"] = clockOutPer;
            ViewData["StackedClockOutManual"] = manualOutPer;
            ViewData["StackedClockOutAuto"] = autoOutPer;

            return percetage;
        }

        private void LoadPayroll(string management_emp_id, string sdate, string edate, string currPayId , string location_id)
        {
            //Trace.WriteLine(" This is the Location" + location_id);  
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();            


            double totalDays = (DateTime.Today - Convert.ToDateTime(sdate)).TotalDays;
            double remainingDays = (Convert.ToDateTime(edate) - DateTime.Today).TotalDays;
            string startDate = sdate;
            string endDate = edate;
            string PrevPayRollId;
            string currentPayRollId = currPayId;
            string payrollid = currPayId;
            ViewData["RemainingDays"] = remainingDays;

            //Debug.WriteLine("*************TotalDays*****************************:" + totalDays);
            //Debug.WriteLine("*************remainingDays*****************************:" + remainingDays);
            if (totalDays <= 3)
            {
                //Debug.WriteLine("*************Changed dates*****************************:" + DateTime.Today.AddDays(-14- totalDays).ToString());
                startDate = DateTime.Today.AddDays(-14 - totalDays).ToString();
                endDate = DateTime.Today.AddDays(-totalDays).ToString();
                currentPayRollId = (Convert.ToInt32(currentPayRollId) - 1).ToString();
                PrevPayRollId = (Convert.ToInt32(currentPayRollId) - 1).ToString();

            }
            ViewData["payId"] = payrollid;
            ViewData["payFrom"] = Convert.ToDateTime(startDate).ToString("MMMM dd, yyyy");
            ViewData["payTo"] = Convert.ToDateTime(endDate).ToString("MMMM dd, yyyy");
            //End of getting paydates from database
            //Accessing Clock In and ClockOut timings for an employee
            //Debug.WriteLine("employee_id:" + management_emp_id);
            System.Collections.ArrayList instantArrayList = new System.Collections.ArrayList();
            instantArrayList.Clear();
            string[,] BiWeeklyHours = new string[14, 2];
            //Debug.WriteLine("todayDate :" + Convert.ToDateTime(startDate).ToString("yyyy-MM-dd"));
            //This code for date columns on the top of the table
            DateTime convStartDate = Convert.ToDateTime(Convert.ToDateTime(startDate).ToString("yyyy-MM-dd"));

            DateTime convStartDate1 = Convert.ToDateTime(Convert.ToDateTime(sdate).ToString("yyyy-MM-dd"));

            ViewData["start_date"] = Convert.ToDateTime(sdate).ToString("MMM dd,yyyy");

            ViewData["sun_1_date"] = Convert.ToDateTime(sdate).ToString("dd-MMM");
            ViewData["mun_1_date"] = Convert.ToDateTime(convStartDate1.AddDays(1)).ToString("dd-MMM");
            ViewData["tues_1_date"] = Convert.ToDateTime(convStartDate1.AddDays(2)).ToString("dd-MMM");
            ViewData["wed_1_date"] = Convert.ToDateTime(convStartDate1.AddDays(3)).ToString("dd-MMM");
            ViewData["thur_1_date"] = Convert.ToDateTime(convStartDate1.AddDays(4)).ToString("dd-MMM");
            ViewData["fri_1_date"] = Convert.ToDateTime(convStartDate1.AddDays(5)).ToString("dd-MMM");
            ViewData["sat_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(6)).ToString("dd-MMM");
            ViewData["sun_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(7)).ToString("dd-MMM");
            ViewData["mon_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(8)).ToString("dd-MMM");
            ViewData["tues_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(9)).ToString("dd-MMM");
            ViewData["wed_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(10)).ToString("dd-MMM");
            ViewData["thurs_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(11)).ToString("dd-MMM");
            ViewData["fri_2_date"] = Convert.ToDateTime(convStartDate1.AddDays(12)).ToString("dd-MMM");
            ViewData["sat_1_date"] = Convert.ToDateTime(convStartDate1.AddDays(13)).ToString("dd-MMM");

            ViewData["endDate"] = Convert.ToDateTime(convStartDate1.AddDays(19));

            ViewData["end_date"] = Convert.ToDateTime(edate).ToString("MMM dd,yyyy");

            ViewData["finalDay"] = Convert.ToDateTime(edate);

            string constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    sb.Append("Select convert(varchar(5),time_stamp, 108) as hours from tbl_attendance_log where  CONVERT(DATE, time_stamp) = @DailyWise and employee_id = @empId and loc_id = @locID");

                    string sql = sb.ToString();

                    for (int day = 0; day <= 13; day++)
                    {
                        //Debug.WriteLine("Entering looop:" + day);

                        DateTime ConvrunDate = convStartDate1.AddDays(day);
                        string runDate = Convert.ToDateTime(ConvrunDate).ToString("yyyy-MM-dd");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@DailyWise", runDate);
                            command.Parameters.AddWithValue("@empId", management_emp_id);
                            command.Parameters.AddWithValue("@locID", location_id);
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while (dr.Read())
                                    instantArrayList.Add((dr["hours"].ToString()));
                            }
                            command.Parameters.Clear();
                        }
                        //Debug.WriteLine("instantArrayList.Count" + instantArrayList.Count);
                        if (instantArrayList.Count == 0)
                        {
                            BiWeeklyHours[day, 0] = "0:00";
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else if (instantArrayList.Count == 1)
                        {
                            //Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                            BiWeeklyHours[day, 1] = "0:00";
                        }
                        else
                        {
                            //Debug.WriteLine(" instantArrayList[0].ToString()" + instantArrayList[0].ToString());
                            var comp1 = (int)TimeSpan.Parse(instantArrayList[0].ToString()).TotalMinutes;
                            var comp2 = (int)TimeSpan.Parse(instantArrayList[instantArrayList.Count - 1].ToString()).TotalMinutes;
                            if (comp1 > comp2)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[instantArrayList.Count - 1].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[0].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "16:30";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "16:30";
                                }

                            }
                            else if (comp2 > comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                                if (BiWeeklyHours[day, 0] == "20:00")
                                {
                                    BiWeeklyHours[day, 0] = "16:30";
                                }
                                if (BiWeeklyHours[day, 1] == "20:00")
                                {
                                    BiWeeklyHours[day, 1] = "16:30";
                                }
                            }
                            else if (comp2 == comp1)
                            {
                                BiWeeklyHours[day, 0] = instantArrayList[0].ToString();
                                BiWeeklyHours[day, 1] = instantArrayList[instantArrayList.Count - 1].ToString();
                            }

                        }

                        instantArrayList.Clear();
                    }


                    connection.Close();
                }
            }
            catch (Exception e2)
            {
                System.Diagnostics.Debug.WriteLine("An Error occured in Managemnet :" + e2);
            }

            //Debug.WriteLine(" instantArrayList's capacity :" + instantArrayList.Capacity);

            string[,] dailyHours = new string[14, 2];
            float checkinTotal = 0;
            int checkInCount = 0;
            float checkOutTotal = 0;
            int checkOutCount = 0;
            for (int i = 0; i <= 13; i++)
            {
                //Debug.WriteLine("............................................");
                //Debug.WriteLine(" i,0 :" + i + ":" + BiWeeklyHours[i, 0]);
                //Debug.WriteLine("i,1" + BiWeeklyHours[i, 1]);

                var firstSignIn = BiWeeklyHours[i, 0];
                var LastSignIn = BiWeeklyHours[i, 1];


                var leftMinutes = (int)TimeSpan.Parse(firstSignIn).TotalMinutes;
                var rightMinutes = (int)TimeSpan.Parse(LastSignIn).TotalMinutes;
                //Debug.WriteLine("Overall Minutes left" + leftMinutes);
                //Debug.WriteLine("Overall minutes right" + rightMinutes);

                if (leftMinutes != 0 && rightMinutes != 0)
                {
                    //Debug.WriteLine("Overall Hours to database :" + (leftMinutes - rightMinutes) / 60);
                    //Debug.WriteLine("Overall Minutes to database :" + (leftMinutes - rightMinutes) % 60);
                    checkinTotal = checkinTotal + leftMinutes;
                    checkOutTotal = checkOutTotal + rightMinutes;
                    dailyHours[i, 0] = (Math.Abs((leftMinutes - rightMinutes)) / 60).ToString();
                    dailyHours[i, 1] = (Math.Abs((leftMinutes - rightMinutes)) % 60).ToString();
                    checkInCount = checkInCount + 1;
                    checkOutCount = checkOutCount + 1;
                }
                else
                {
                    //dailyHours[i, 0] ="0";
                    //dailyHours[i, 1] ="00";
                    dailyHours[i, 0] = "";
                    dailyHours[i, 1] = "";
                }

                if (leftMinutes != 0 && rightMinutes == 0)
                {

                    checkinTotal = checkinTotal + leftMinutes;
                    checkInCount = checkInCount + 1;
                }



            }
            //Debug.WriteLine("checkinTotal :" + checkinTotal);
            //Debug.WriteLine("checkOutTotal:" + checkOutTotal);

            //Calculating average hours
            float checkInAvgHours = Math.Abs((checkinTotal / checkInCount)) / 60;
            float checkInAvgMins = Math.Abs((checkinTotal / checkInCount)) % 60;

            float checkOutAvgHours = Math.Abs((checkOutTotal / checkOutCount)) / 60;
            float checkOutAvgMins = Math.Abs((checkOutTotal / checkOutCount)) % 60;
            string ChkInAvg = "";
            string ChkoutAvg = "";
            if (checkInCount > 0)
            {
                string sInHours = ((int)checkInAvgHours).ToString();
                string sOutMins = ((int)checkInAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }

                ChkInAvg = sInHours + ":" + sOutMins;
                ViewData["ChkInAvgDisp"] = "Clock-In : " + sInHours + ":" + sOutMins;
                //this.IN1.Text = "Check-In (" + sInHours + ":" + sOutMins + ")";
            }
            else
            {
                ChkInAvg = "";
                ViewData["ChkInAvgDisp"] = "Clock-In : ";
                //this.IN1.Text = "Check-In ( )";
            }
            if (checkOutCount > 0)
            {
                string sInHours = ((int)checkOutAvgHours).ToString();
                string sOutMins = ((int)checkOutAvgMins).ToString();
                if (sInHours.Length == 1)
                {
                    sInHours = "0" + sInHours;
                }
                if (sOutMins.Length == 1)
                {
                    sOutMins = "0" + sOutMins;
                }

                ChkoutAvg = sInHours + ":" + sOutMins;
                ViewData["ChkoutAvgDisp"] = "Clock-Out : " + sInHours + ":" + sOutMins;
                //this.OUT1.Text = "Check-out (" + sInHours + ":" + sOutMins + ")";
            }
            else
            {
                ChkoutAvg = "";
                ViewData["ChkoutAvgDisp"] = "Clock-Out : ";
                //this.OUT1.Text = "Check-out ( )";
            }

            string[] totalString = new string[14];
            for (int i = 0; i <= 13; i++)
            {
                if (dailyHours[i, 0] == "" && dailyHours[i, 1] == "")
                {
                    totalString[i] = "";
                }
                else
                {
                    if (dailyHours[i, 0].Length == 1)
                    {
                        dailyHours[i, 0] = "0" + dailyHours[i, 0][0];
                    }
                    if (dailyHours[i, 1].Length == 1)
                    {
                        dailyHours[i, 1] = "0" + dailyHours[i, 1][0];
                    }
                    //Debug.WriteLine("666:" + dailyHours[i, 0]);
                    float totHrs = (float)Convert.ToDouble(dailyHours[i, 0]);
                    //Debug.WriteLine("666:" + totHrs.ToString());
                    float totMins = (float)Convert.ToDouble(dailyHours[i, 1]);
                    totalString[i] = dailyHours[i, 0] + ":" + dailyHours[i, 1];
                }
            }

            var totAvgIn = "";
            var totAvgOut = "";
            if (ChkInAvg == "")
            {
                totAvgIn = "0";

            }
            else
            {
                totAvgIn = ChkInAvg;
            }
            if (ChkoutAvg == "")
            {
                totAvgOut = "0";
            }
            else
            {
                totAvgOut = ChkoutAvg;
            }

            var totleftMinutes = (int)TimeSpan.Parse(totAvgIn).TotalMinutes;
            var totrightMinutes = (int)TimeSpan.Parse(totAvgOut).TotalMinutes;
            string TotAvgHours = (Math.Abs((totleftMinutes - totrightMinutes)) / 60).ToString();
            string TotAvgMins = (Math.Abs((totleftMinutes - totrightMinutes)) % 60).ToString();
            //Debug.WriteLine("totalHrsOnLoad aBG HOURS" + TotAvgHours + "dcsdcsd :" + TotAvgMins);
            if (TotAvgHours.Length == 1)
            {
                TotAvgHours = "0" + TotAvgHours;
            }
            if (TotAvgMins.Length == 1)
            {
                TotAvgMins = "0" + TotAvgMins;
            }

            ViewData["OverallAvgDisp"] = "Total Avg : " + TotAvgHours + ":" + TotAvgMins;


            ViewData["TOT1"] = totalString[0];
            ViewData["TOT2"] = totalString[1];
            ViewData["TOT3"] = totalString[2];
            ViewData["TOT4"] = totalString[3];
            ViewData["TOT5"] = totalString[4];
            ViewData["TOT6"] = totalString[5];
            ViewData["TOT7"] = totalString[6];
            ViewData["TOT8"] = totalString[7];
            ViewData["TOT9"] = totalString[8];
            ViewData["TOT10"] = totalString[9];
            ViewData["TOT11"] = totalString[10];
            ViewData["TOT12"] = totalString[11];
            ViewData["TOT13"] = totalString[12];
            ViewData["TOT14"] = totalString[13];

            //End of Table 1

            //Assigning Values to Table 3

            //this.IN0.Text ="0:00";
            //this.IN1.Text = "0:00";
            for (int i = 0; i <= 13; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (BiWeeklyHours[i, j] == "0:00")
                    {
                        BiWeeklyHours[i, j] = "";
                    }
                }
            }

            ViewData["IN1"] = BiWeeklyHours[0, 0];
            ViewData["IN2"] = BiWeeklyHours[1, 0];
            ViewData["IN3"] = BiWeeklyHours[2, 0];
            ViewData["IN4"] = BiWeeklyHours[3, 0];
            ViewData["IN5"] = BiWeeklyHours[4, 0];
            ViewData["IN6"] = BiWeeklyHours[5, 0];
            ViewData["IN7"] = BiWeeklyHours[6, 0];
            ViewData["IN8"] = BiWeeklyHours[7, 0];
            ViewData["IN9"] = BiWeeklyHours[8, 0];
            ViewData["IN10"] = BiWeeklyHours[9, 0];
            ViewData["IN11"] = BiWeeklyHours[10, 0];
            ViewData["IN12"] = BiWeeklyHours[11, 0];
            ViewData["IN13"] = BiWeeklyHours[12, 0];
            ViewData["IN14"] = BiWeeklyHours[13, 0];
            //this.OUT0.Text = "0:00";
            //this.OUT1.Text = "0:00";
            ViewData["OUT1"] = BiWeeklyHours[0, 1];
            ViewData["OUT2"] = BiWeeklyHours[1, 1];
            ViewData["OUT3"] = BiWeeklyHours[2, 1];
            ViewData["OUT4"] = BiWeeklyHours[3, 1];
            ViewData["OUT5"] = BiWeeklyHours[4, 1];
            ViewData["OUT6"] = BiWeeklyHours[5, 1];
            ViewData["OUT7"] = BiWeeklyHours[6, 1];
            ViewData["OUT8"] = BiWeeklyHours[7, 1];
            ViewData["OUT9"] = BiWeeklyHours[8, 1];
            ViewData["OUT10"] = BiWeeklyHours[9, 1];
            ViewData["OUT11"] = BiWeeklyHours[10, 1];
            ViewData["OUT12"] = BiWeeklyHours[11, 1];
            ViewData["OUT13"] = BiWeeklyHours[12, 1];
            ViewData["OUT14"] = BiWeeklyHours[13, 1];
            //End of Table 3


            //Checking wheather ,is there any record in tbl_employee_payroll_biweekly. If there is any record, then no need to insert otherwise insert null values
            

            int emid = Convert.ToInt32(management_emp_id);
            int pyid = Convert.ToInt32(payrollid);

            //Trace.WriteLine("employee_id is " + emid +" payroll_id is " + pyid + " loc_id is "  + location_id);

            var recordFlag = db.tbl_employee_payroll_biweekly.Where(x => x.employee_id == emid && x.payroll_id == pyid && x.loc_id == location_id).FirstOrDefault();
            //Debug.WriteLine("is there any values in tbl_employee_payroll_biweekly :" + recordFlag);
            //End of checking


            //If there is no record, then insert null values to the database upon opening the employee and if there is any record try to fetch details of the employee.
            if (recordFlag != null) //
            {
                //Trace.WriteLine("*********** The record  exist ************");
            }
            else
            {
                //Trace.WriteLine("*********************** The record doesnt exist ***********************");

                tbl_employee_payroll_biweekly newbiweekly = new tbl_employee_payroll_biweekly();
                newbiweekly.employee_id = Convert.ToInt32(emid);
                newbiweekly.payroll_id = Convert.ToInt32(pyid);
                newbiweekly.last_updated_by = Convert.ToInt32(Session["userID"]);
                newbiweekly.last_update_date = DateTime.Now;
                newbiweekly.recordflag = 1;
                newbiweekly.loc_id = location_id;

                tbl_employee_payroll_summary newsummery = new tbl_employee_payroll_summary();
                newsummery.employee_id = Convert.ToInt32(management_emp_id);
                newsummery.payroll_id = Convert.ToInt32(payrollid);
                newsummery.updated_by = Convert.ToInt32(Session["userID"]);
                newsummery.updated_date = DateTime.Now;
                newsummery.record_flag = 1;
                newsummery.loc_id = location_id;


                db.tbl_employee_payroll_biweekly.Add(newbiweekly);
                db.tbl_employee_payroll_summary.Add(newsummery);

                db.SaveChanges();
            }

        }

        public String ContainerNameEmployeeFiles = "cta-employee-files";
        [HttpGet]
        public ActionResult EmployeeFiles(string locId, string employeeId)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            int empId = Convert.ToInt32(Session["userID"].ToString());
            int emplID = Convert.ToInt32(Session["userID"].ToString());
            int empIdSession = Convert.ToInt32(Session["userID"].ToString());
            if (employeeId != null)
            {
                empId = Convert.ToInt32(employeeId);
            }
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            FileURL FileUrl = new FileURL();
            var empDetails1 = db.tbl_sehub_access.Where(x => x.employee_id == empIdSession).FirstOrDefault();

            FileUrl.Permission = Convert.ToInt32(empDetails1.library_access);
            FileUrl.SehubAccess = empDetails1;

            var empDetails = db.tbl_employee.Where(x => x.employee_id == empId).FirstOrDefault();

            List<tbl_employee> employeeListLocation = new List<tbl_employee>();

            if (locId == null)
            {
                employeeListLocation = db.tbl_employee.Where(x => x.loc_ID == empDetails.loc_ID && x.status == 1).OrderBy(x => x.full_name).ToList();
            }
            else
            {
                employeeListLocation = db.tbl_employee.Where(x => x.loc_ID == locId && x.status == 1).OrderBy(x => x.full_name).ToList();
            }

            List<EmployeeAttendanceListModel> EmpDetailsList = new List<EmployeeAttendanceListModel>();
            foreach (var emp in employeeListLocation)
            {
                EmployeeAttendanceListModel empdetails = new EmployeeAttendanceListModel();
                empdetails.employeeId = emp.employee_id.ToString();
                empdetails.fullName = emp.full_name;

                EmpDetailsList.Add(empdetails);
            }

            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            // Retrieve reference to a blob ie "picture.jpg".
            var blockBlob = container.ListBlobs();

            var blobList = blockBlob.ToList();

            var URLNames = new List<KeyValuePair<string, string>>();

            foreach (var blob in blockBlob)
            {
                var newUri = new Uri(blob.Uri.AbsoluteUri);
                var blobFileName = blob.Uri.Segments.Last();
                blobFileName = blobFileName.Replace("%20", " ");
                blobFileName = blobFileName.Replace(".pdf", " ");

                if (blobFileName.Contains(empDetails.employee_id.ToString()))
                {
                    blobFileName = blobFileName.Replace(empDetails.employee_id.ToString() + "_", "");
                    URLNames.Add(new KeyValuePair<string, string>(newUri.ToString(), blobFileName));
                }
            }

            FileUrl.URLName = URLNames;
            FileUrl.employeeList = EmpDetailsList;
            FileUrl.Location_ID = empDetails.loc_ID;
            FileUrl.LocationsList = populateLocationsPermissions(emplID);
            FileUrl.SelectedEmployeeId = empId.ToString();

            return View(FileUrl);
        }

        [HttpPost]                                                                      
        public ActionResult UploadEmployeeFiles(HttpPostedFileBase CompanyDocument, FileURL model)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.employee_id).FirstOrDefault().ToString();

            string imageName;

            if (model.SelectedEmployeeId != null)
            {
                imageName = model.SelectedEmployeeId + "_" + Path.GetFileName(CompanyDocument.FileName);
            }
            else
            {
                imageName = empID + "_" + Path.GetFileName(CompanyDocument.FileName);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            CloudBlockBlob Blob = container.GetBlockBlobReference(imageName);

            Blob.Properties.ContentType = CompanyDocument.ContentType;

            Blob.UploadFromStream(CompanyDocument.InputStream);

            return RedirectToAction("EmployeeFiles", "Management");
        }

        public ActionResult DeleteEmployeeFiles(string fileName, string employeeID)
        {
            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.employee_id).FirstOrDefault().ToString();

            if(employeeID == null)
            {
                fileName = empID + "_" + fileName.Remove(fileName.Length - 1) + ".pdf";
            }
            else
            {
                fileName = employeeID + "_" + fileName.Remove(fileName.Length - 1) + ".pdf";
            }

            Debug.WriteLine(fileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();

            return RedirectToAction("EmployeeFiles");
        }

        [HttpPost]
        public ActionResult RenameEmployeeFiles(string currentFileName, string newFileName, FileURL model)
        {

            int empId = Convert.ToInt32(Session["userID"].ToString());
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID;

            if(model.SelectedEmployeeId == null)
            {
                empID = db.tbl_employee.Where(x => x.employee_id == empId).Select(x => x.employee_id).FirstOrDefault().ToString();
            }
            else
            {
                empID = model.SelectedEmployeeId;
            }

            currentFileName = empID + "_" + currentFileName.Remove(currentFileName.Length - 1) + ".pdf";
            Debug.WriteLine(currentFileName);
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            // Your code ------ 
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["BlobConnection"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerNameEmployeeFiles);

            var blob = container.GetBlockBlobReference(currentFileName);
            var blob1 = container.GetBlockBlobReference(empID + "_" + model.RenameString + ".pdf");

            blob1.StartCopy(blob);

            blob.DeleteIfExists();

            return RedirectToAction("EmployeeFiles");
        }

        [HttpPost]
        public ActionResult ChangeLocEmployeeFiles(FileURL model)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            string empID = db.tbl_employee.Where(x => x.loc_ID == model.Location_ID && x.status == 1).OrderBy(x => x.full_name).Select(x => x.employee_id).FirstOrDefault().ToString();

            return RedirectToAction("EmployeeFiles", new { locId = model.Location_ID, employeeId = empID });
        }

        public ActionResult aco()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            DateTime date = new DateTime(2022, 06, 21);
            DateTime startDate = new DateTime(2021, 04, 25);

            var clkins = db.tbl_attendance_log.Where(x => x.event_id == "clockIN" && x.time_stamp < date && x.time_stamp > startDate).ToList();
            //Trace.WriteLine(clkins.Count());
            foreach (var evnt in clkins)
            {
                DateTime evnt_start = evnt.time_stamp.Date;
                DateTime evnt_end = evnt.time_stamp.Date.AddDays(1);

                if (db.tbl_attendance_log.Where(x => x.loc_id == evnt.loc_id && x.employee_id == evnt.employee_id && x.time_stamp > evnt_start && x.time_stamp < evnt_end).OrderByDescending(x => x.time_stamp).Select(x => x.event_id).FirstOrDefault() == "clockIN")
                {                    
                    tbl_attendance_log newAutoOut = new tbl_attendance_log();
                    newAutoOut.loc_id = evnt.loc_id;
                    newAutoOut.employee_id = evnt.employee_id;
                    newAutoOut.event_id = "autoOUT";
                    newAutoOut.time_stamp = evnt_start.AddHours(16).AddMinutes(30);
                    if(db.tbl_attendance_log.Where(x => x.loc_id == newAutoOut.loc_id && x.employee_id == newAutoOut.employee_id && x.event_id == "autoOUT" && x.time_stamp == newAutoOut.time_stamp).Any())
                    {

                    }
                    else
                    {
                        db.tbl_attendance_log.Add(newAutoOut);
                        db.SaveChanges();
                    }
                    
                }
            }


            return RedirectToAction("Dashboard", "Settings");
        }

        public JsonResult GetVacationEvents()
        {
            CityTireAndAutoEntities dc = new CityTireAndAutoEntities();

            var events = dc.tbl_Calendar_events.ToList();

            var vacations = dc.tbl_vacation_schedule.ToList();

            foreach (var item in vacations)
            {
                if (dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.status).FirstOrDefault() == 1)
                {
                    tbl_Calendar_events eve = new tbl_Calendar_events();
                    eve.subject = "Vacation";
                    eve.start_date = item.start_date.AddDays(1);
                    eve.end_date = item.end_date.Value.AddDays(1);
                    eve.Description = dc.tbl_employee.Where(x => x.employee_id == item.empid).Select(x => x.full_name).FirstOrDefault() + " " + item.leave_type;
                    events.Add(eve);
                }
            }

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

    }
}