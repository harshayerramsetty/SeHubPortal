using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SeHubPortal.Models;
using SeHubPortal.ViewModel;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;

namespace SeHubPortal.Controllers
{
    public class PayrollDepositsController : Controller
    {
        // GET: Payroll
        public ActionResult Details()
        {
            return View();
        }

        private static List<SelectListItem> populateLocationsPermissions(int empId)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var locaList = db.tbl_cta_location_info.ToList();

            var sehubloc = db.tbl_sehub_access.Where(x => x.employee_id == empId).FirstOrDefault();

            items.Add(new SelectListItem
            {
                Text = "All",
                Value = "All"
            });

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


            return items;
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
        public ActionResult EmployeeDeductions(MyStaffViewModel modal)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            return RedirectToAction("EmployeeDeductions", new { LocId = modal.MatchedStaffLocID, payroll_id = modal.SelectedPayrollId });
        }

        [HttpPost]
        public ActionResult SavePaystub(PaystubViewModel modal)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            tbl_payroll_employee_paystubs paystub = new tbl_payroll_employee_paystubs();

            var empDetails = db.tbl_employee.Where(x => x.employee_id == modal.empid).FirstOrDefault();

            string paystupPDFstring = "";

            paystupPDFstring += "Employee ID --> " + empDetails.employee_id+ ";";
            paystupPDFstring += "Payroll ID --> " + modal.pid+ ";";
            paystupPDFstring += "Name --> " + empDetails.full_name+ ";";
            paystupPDFstring += "Position --> " + empDetails.cta_position+ ";";
            paystupPDFstring += "Email --> " + empDetails.cta_email+ ";";
            paystupPDFstring += "Location --> " + empDetails.loc_ID+ ";";



            paystupPDFstring += "Working Hours --> " + modal.deductions.hoursWorked.ToString() + ";";
            paystupPDFstring += "Gross Earnings --> " + modal.deductions.gross_earnings.ToString() + ";";
            paystupPDFstring += "Federal Tax --> " + modal.deductions.federal_tax.ToString() + ";";
            paystupPDFstring += "Provincial Tax --> " + modal.deductions.provincial_tax.ToString() + ";";
            paystupPDFstring += "CPP --> " + modal.deductions.CPP.ToString() + ";";
            paystupPDFstring += "EI --> " + modal.deductions.EI.ToString() + ";";
            paystupPDFstring += "Group INS --> " + modal.deductions.group_ins.ToString() + ";";
            paystupPDFstring += "Total Deductions --> " + modal.deductions.total_deductions.ToString() + ";";
            paystupPDFstring += "Net Pay --> " + modal.deductions.net_pay.ToString() + ";";

            EmailPaystub(paystupPDFstring);

            paystub.emp_id = Convert.ToInt32(modal.empid);
            paystub.payroll_id = Convert.ToInt32(modal.pid);
            paystub.hours_worked = Convert.ToDouble(modal.deductions.hoursWorked);
            paystub.gross_earnings = Convert.ToDouble(modal.deductions.gross_earnings);
            paystub.federal_tax = Convert.ToDouble(modal.deductions.federal_tax);
            paystub.provincial_tax = Convert.ToDouble(modal.deductions.provincial_tax);
            paystub.cpp = Convert.ToDouble(modal.deductions.CPP);
            paystub.ei = Convert.ToDouble(modal.deductions.EI);
            paystub.group_ins = Convert.ToDouble(modal.deductions.group_ins);
            paystub.total_deduction = Convert.ToDouble(modal.deductions.total_deductions);
            paystub.net_pay = Convert.ToDouble(modal.deductions.net_pay);

            db.tbl_payroll_employee_paystubs.Add(paystub);
            db.SaveChanges();

            return RedirectToAction("EmployeeDeductions", new { LocId = "", payroll_id = modal.pid });
        }

        [HttpPost]
        public ActionResult ChangePayrollID(MyStaffViewModel modal)
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }
            return RedirectToAction("EmployeeDeductions", new { LocId = modal.MatchedStaffLocID, payroll_id = modal.SelectedPayrollId });
        }

        private static List<SelectListItem> populatePayrollId()
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            List<SelectListItem> items = new List<SelectListItem>();

            var payroll_ids = db.tbl_employee_payroll_dates.Where(x => x.end_date < System.DateTime.Today).OrderByDescending(x => x.start_date).Select(x => x.payroll_Id).ToList();

            foreach (var payroll in payroll_ids)
            {
                items.Add(new SelectListItem
                {
                    Text = payroll.ToString(),
                    Value = payroll.ToString()
                });
            }

            return items;
        }

        [HttpGet]
        public ActionResult EmployeeDeductions(string LocId, int payroll_id)
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
            modal.PayrollIdList = populatePayrollId();
            if (modal.SehubAccess.my_staff == 0)
            {
                return RedirectToAction("Attendance", "Management");
            }

            if (Session["userID"] == null)
            {
                return RedirectToAction("SignIn", "Login");
            }

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

            if (payroll_id == 0)
            {
                var CurrentPayroll = db.tbl_employee_payroll_dates.Where(x => x.end_date <= DateTime.Today).OrderByDescending(x => x.start_date).FirstOrDefault();
                payroll_id = CurrentPayroll.payroll_Id;                
            }

            modal.SelectedPayrollId = payroll_id.ToString();

            var EmployeeDetails = new List<tbl_employee>();

            if (locationid == "All")
            {
                EmployeeDetails = db.tbl_employee.Where(x => x.full_name != "Auto Technician" && x.status == 1).OrderBy(x => x.full_name).ToList();
            }
            else
            {
                EmployeeDetails = db.tbl_employee.Where(x => x.loc_ID == locationid && x.full_name != "Auto Technician" && x.status == 1).OrderBy(x => x.full_name).ToList();
            }

            List<tbl_employee_status> empdetList = new List<tbl_employee_status>();
            List<PayrollDeductionsViewModel> deductions = new List<PayrollDeductionsViewModel>();
            List<PayrollDeductionsViewModel> deductionsYTD = new List<PayrollDeductionsViewModel>();

            foreach (var emp in EmployeeDetails)
            {
                tbl_employee_status empdet = new tbl_employee_status();
                PayrollDeductionsViewModel deduct = new PayrollDeductionsViewModel();
                //empdet = db.tbl_employee_status.Where(x => x.employee_id == emp.employee_id).FirstOrDefault();
                var payrollInfo = db.tbl_payroll_employee_specifications.Where(x => x.emp_id == emp.employee_id).FirstOrDefault();                

                if (payrollInfo != null)
                {
                    deduct = CalculateTaxDeductions(emp.employee_id, payroll_id);
                    deductions.Add(deduct);
                    empdetList.Add(empdet);
                }
                
            }


            //Debug.WriteLine("locationid:" + locationid);
            if (EmployeeDetails != null)
            {
                modal.positionsTable = db.tbl_position_info.ToList();
                modal.employeeDetails = EmployeeDetails;
                modal.employeeStatusDetails = empdetList;
                modal.Deductions = deductions;
                modal.DeductionsYTD = deductionsYTD;
                modal.MatchedStaffLocs = populateLocationsPermissions(empId);
                modal.MatchedStaffLocID = locationid;


                modal.Positions = populatePositions();

                return View(modal);
            }
            else
            {
                return View(modal);
            }

        }

        static PayrollDeductionsViewModel CalculateTaxDeductions(int emp_id, int payroll_id)
        {

            Trace.WriteLine(emp_id + " " + payroll_id);
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            int year = db.tbl_employee_payroll_dates.Where(x => x.payroll_Id == payroll_id).Select(x => x.start_date.Value.Year).FirstOrDefault();

            var finalPayroll = db.tbl_employee_payroll_final.Where(x => x.employee_id == emp_id && x.payroll_id == payroll_id).FirstOrDefault();

            var payrollInfo = db.tbl_payroll_employee_specifications.Where(x => x.emp_id == emp_id).FirstOrDefault();

            PayrollDeductionsViewModel deductions = new PayrollDeductionsViewModel();
            deductions.emp_id = emp_id;
            deductions.emp_name = db.tbl_employee.Where(x => x.employee_id == emp_id).Select(x => x.full_name).FirstOrDefault();

            if (finalPayroll != null)
            {
                double hoursWorked = finalPayroll.CommissionPay_D.Value + finalPayroll.StatutoryHolidayPay_H.Value + finalPayroll.VacationTime_H.Value + finalPayroll.RegularPay_H.Value + finalPayroll.OvertimePay_H.Value + finalPayroll.OvertimePay_2_H.Value + finalPayroll.OvertimePay_3_H.Value + finalPayroll.OtherPay_D.Value + finalPayroll.SickLeave_H.Value + finalPayroll.OnCallCommission_D.Value;

                double hourlyRate = payrollInfo.payRat_per_hour.Value;

                double federalTaxExemption = payrollInfo.federal_tax_exemption.Value;

                double provincialTaxExemption = payrollInfo.provincial_tax_exemption.Value;

                double cppExemption = payrollInfo.cpp_exemption.Value;

                double groupINS = payrollInfo.group_ins.Value;

                double grossPay = hoursWorked * hourlyRate;

                double federalTax = CalculateFederalTax(grossPay, federalTaxExemption, year);
                double provincialTax = CalculateProvincialTax(grossPay, provincialTaxExemption, year);
                double cpp = CalculateCPP(grossPay, cppExemption, year);

                double ei = 0;

                if (payrollInfo.ei_eligible.Value)
                {
                    ei = CalculateEI(grossPay, year);
                }

                double rrsp = 0;

                if (payrollInfo.rrsp_voluntary.Value)
                {
                    rrsp = payrollInfo.pensionPlan_contribution.Value;
                }

                double otherPension = 0;

                if (payrollInfo.pensionPlan_additional.HasValue)
                {
                    otherPension = payrollInfo.pensionPlan_additional.Value;
                }

                double totalDeductions = federalTax + provincialTax + cpp + ei + groupINS + rrsp + otherPension;
                double netPay = grossPay - totalDeductions;

                deductions.hoursWorked = Math.Round(hoursWorked, 2).ToString("N2");
                deductions.gross_earnings = Math.Round(grossPay, 2).ToString("N2");
                deductions.federal_tax = Math.Round(federalTax, 2).ToString("N2");
                deductions.provincial_tax = Math.Round(provincialTax, 2).ToString("N2");
                deductions.CPP = Math.Round(cpp, 2).ToString("N2");
                deductions.EI = Math.Round(ei, 2).ToString("N2");
                deductions.group_ins = Math.Round(groupINS, 2).ToString("N2");
                deductions.pension_plan = Math.Round(rrsp, 2).ToString("N2");
                deductions.other_pension_plan = Math.Round(otherPension, 2).ToString("N2");
                deductions.total_deductions = Math.Round(totalDeductions, 2).ToString("N2");
                deductions.net_pay = Math.Round(netPay, 2).ToString("N2");

            }
            return deductions;
        }

        static double CalculateFederalTax(double grossPay, double federalTaxExemption, int year)
        {
            double federalTax = 0.0;
            double grossPayPerYear = (grossPay * 26);
            grossPayPerYear -= federalTaxExemption;

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var federalBrackets = db.tbl_source_payroll_federal_tax_brackets.Where(x => x.year == year && x.lower_income_bracket < grossPayPerYear).OrderBy(x => x.tax_rate).ToList();

            double tempUpper = 0;

            foreach (var brkt in federalBrackets)
            {
                if (brkt.lower_income_bracket < grossPayPerYear && grossPayPerYear < brkt.upper_income_bracket)
                {
                    federalTax += (grossPayPerYear - tempUpper) * brkt.tax_rate;
                }
                else
                {
                    federalTax += brkt.upper_income_bracket * brkt.tax_rate;
                    tempUpper = brkt.upper_income_bracket;
                }
            }

            federalTax /= 26;

            return federalTax;
        }

        static double CalculateProvincialTax(double grossPay, double provincialTaxExemption, int year)
        {
            double provincialTax = 0.0;
            double grossPayPerYear = (grossPay * 26);
            grossPayPerYear -= provincialTaxExemption;

            CityTireAndAutoEntities db = new CityTireAndAutoEntities();

            var provincialBrackets = db.tbl_source_payroll_provincial_tax_brackets.Where(x => x.year == year && x.lower_income_bracket < grossPayPerYear).OrderBy(x => x.tax_rate).ToList();

            double tempUpper = 0;

            foreach(var brkt in provincialBrackets)
            {
                Trace.WriteLine(brkt.upper_income_bracket + " upper");

                if (brkt.lower_income_bracket < grossPayPerYear && grossPayPerYear < brkt.upper_income_bracket)
                {
                    provincialTax += (grossPayPerYear - tempUpper) * brkt.tax_rate;
                }
                else
                {
                    provincialTax += brkt.upper_income_bracket * brkt.tax_rate;
                    tempUpper = brkt.upper_income_bracket;
                }
                Trace.WriteLine(provincialTax + " Tax");
            }


            provincialTax /= 26;

            return provincialTax;
        }

        static double CalculateCPP(double grossPay, double cppExemption, int year)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            double grossPayPerYear = grossPay * 26;
            double PensionableIncome = grossPayPerYear - cppExemption;
            double cppAnnual = PensionableIncome * db.tbl_source_payroll_cpp_brackets.Where(x => x.year == year).Select(x => x.rate).FirstOrDefault();
            double cpp = cppAnnual / 26;

            return cpp;
        }

        static double CalculateEI(double grossPay, int year)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            double grossPayPerYear = grossPay * 26;
            double ei = 0.0;
            ei = (grossPayPerYear * db.tbl_source_payroll_EI_brackets.Where(x => x.year == year).Select(x => x.rate).FirstOrDefault()) / 26;
            return ei;
        }

        private MemoryStream PDFGenerate(string message)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            MemoryStream output = new MemoryStream();
            Document pdfDoc = new Document(PageSize.A4, 25, 10, 25, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, output);
            pdfDoc.Open();

            byte[] imageBytes = db.tbl_employee.Where(x => x.employee_id == 10901).Select(x => x.profile_pic).FirstOrDefault();

            // Create an iTextSharp image from the byte array
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

            // Set the image scale to fit within the document bounds
            image.ScaleToFit(80F, 80F);
            image.SetAbsolutePosition(450, 650);

            byte[] file;
            file = System.IO.File.ReadAllBytes(Path.Combine(Server.MapPath("~/Content/dashboard-transparent.png")));
            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(file);
            jpg.ScaleToFit(300F, 200F);
            
            jpg.SetAbsolutePosition(250, 750);
            
            pdfDoc.Add(image);
            pdfDoc.Add(jpg);


            foreach (string tax in message.Split(';'))
            {
                Paragraph Text = new Paragraph(tax);
                pdfDoc.Add(Text);
            }
            
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            output.Position = 0;
            return output;
        }

        public void EmailPaystub(string paystupPDFstring)
        {
            MemoryStream file = new MemoryStream(PDFGenerate(paystupPDFstring).ToArray());
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("harsha.yerramsetty@citytire.com", "IT Team")); //jordan.blackwood
            msg.CC.Add(new MailAddress("jordan.blackwood@citytire.com", "IT Team"));
            msg.From = new MailAddress("sehub@citytire.com", "Sehub");
            msg.Subject = "Paystub";
            msg.Attachments.Add(new Attachment(file, "paystub.pdf"));
            msg.Body = "Paystub";
            msg.IsBodyHtml = true;
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
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        public ActionResult populatePaystub(int emp_id, int pay_id)
        {
            CityTireAndAutoEntities db = new CityTireAndAutoEntities();
            PaystubViewModel model = new PaystubViewModel();
            PayrollDeductionsViewModel deductions = new PayrollDeductionsViewModel();

            deductions = CalculateTaxDeductions(emp_id, pay_id);

            var emp_details = db.tbl_employee.Where(x => x.employee_id == emp_id).FirstOrDefault();

            model.empid = emp_id;
            model.pid = pay_id;
            model.name = emp_details.full_name;
            model.position = emp_details.cta_position;
            model.loc = emp_details.loc_ID;
            model.email = emp_details.cta_email;

            model.deductions = deductions;
            return PartialView(model);
        }

        public void sampleEmail()
        {
            string htmlString = "<html> <body> <div style='display: flex; width: 100%; opacity: 0.5;'> <div style='flex:3; text-align: right;'></div> <div style='flex:3; display: flex; justify-content: center; align-items: center;'> <img style='width: 100%; object-fit: contain;' src='https://citytire.com/images/logo.png'> </div> <div style='flex:3; text-align: right; font-size: 6px;'> <div> <u> <b> Head Office </b> </u> </div> 1123 Topsail Road, P.O. Box 549 <br> Mount Pearl, NL A1N 2W4 <br> Tel: (709) 364-6808 • Fax: (709) 364-0074 <br> 24 Hour: (709) 368-5971 <br> Email: cta@citytire.com • www.citytire.com </div> </div> </body> </html>";

            MemoryStream output = new MemoryStream();
            Document document = new Document(PageSize.A4, 25, 10, 25, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(document, output);

            // Open the document for writing
            document.Open();

            // Parse the HTML string and add it to the document
            HTMLWorker worker = new HTMLWorker(document);
            worker.Parse(new StringReader(htmlString));

            // Close the document
            document.Close();

            MemoryStream file = new MemoryStream(output.ToArray());
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("harsha.yerramsetty@citytire.com", "IT Team")); //jordan.blackwood
            //msg.CC.Add(new MailAddress("jordan.blackwood@citytire.com", "IT Team"));
            msg.From = new MailAddress("harsha.yerramsetty@citytire.com", "Sehub");
            msg.Subject = "Paystub";
            msg.Attachments.Add(new Attachment(file, "paystub.pdf"));
            msg.Body = "Paystub";
            msg.IsBodyHtml = true;
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("harsha.yerramsetty@citytire.com", "MuN2020$r!");
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                Debug.WriteLine("Message Sent Succesfully");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        } 
    }
}