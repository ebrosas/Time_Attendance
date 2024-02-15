using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Reporting;
using Telerik.ReportViewer.WebForms;

namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    public partial class ReportViewer : BaseWebForm, IFormExtension
    {
        #region Properties
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        private string CostCenter { get; set; }
        private string ReportTitle { get; set; }

        private UIHelper.ReportTypes CurrentReportType
        {
            get
            {
                UIHelper.ReportTypes result = UIHelper.ReportTypes.NotDefined;
                if (ViewState["CurrentReportType"] != null)
                {
                    try
                    {
                        result = (UIHelper.ReportTypes)Enum.Parse(typeof(UIHelper.ReportTypes), UIHelper.ConvertObjectToString(ViewState["CurrentReportType"]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            set
            {
                ViewState["CurrentReportType"] = value;
            }
        }

        private string CallerForm
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CallerForm"]);
            }
            set
            {
                ViewState["CallerForm"] = value;
            }
        }

        private string DateFilterString
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["DateFilterString"]);
            }
            set
            {
                ViewState["DateFilterString"] = value;
            }
        }

        private VisitorPassEntity VisitorLogReportSource
        {
            get
            {
                VisitorPassEntity reportSource = Session["VisitorLogReportSource"] as VisitorPassEntity;
                return reportSource;
            }
        }

        private List<VisitorSwipeEntity> SwipeDataList
        {
            get
            {
                List<VisitorSwipeEntity> list = ViewState["SwipeDataList"] as List<VisitorSwipeEntity>;
                if (list == null)
                    ViewState["SwipeDataList"] = list = new List<VisitorSwipeEntity>();

                return list;
            }
            set
            {
                ViewState["SwipeDataList"] = value;
            }
        }

        private List<VisitorPassEntity> VisitorSummaryReportSource
        {
            get
            {
                List<VisitorPassEntity> list = Session["VisitorSummaryReportSource"] as List<VisitorPassEntity>;
                if (list == null)
                    Session["VisitorSummaryReportSource"] = list = new List<VisitorPassEntity>();

                return list;
            }
        }

        private List<ContractorAttendance> ContractorAttendanceReportSource
        {
            get
            {
                List<ContractorAttendance> list = Session["ContractorAttendanceReportSource"] as List<ContractorAttendance>;
                if (list == null)
                    Session["ContractorAttendanceReportSource"] = list = new List<ContractorAttendance>();

                return list;
            }
        }

        private List<ShiftProjectionEntity> ShiftProjectionReportSource
        {
            get
            {
                List<ShiftProjectionEntity> list = Session["ShiftProjectionReportSource"] as List<ShiftProjectionEntity>;
                if (list == null)
                    Session["ShiftProjectionReportSource"] = list = new List<ShiftProjectionEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> EmpAttendanceHistoryReportSource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["EmpAttendanceHistoryReportSource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["EmpAttendanceHistoryReportSource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<DutyROTAEntity> DutyROTAReportSource
        {
            get
            {
                List<DutyROTAEntity> list = Session["DutyROTAReportSource"] as List<DutyROTAEntity>;
                if (list == null)
                    Session["DutyROTAReportSource"] = list = new List<DutyROTAEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> DailyAttendanceNonSalarySource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["DailyAttendanceNonSalarySource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["DailyAttendanceNonSalarySource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> DailyAttendanceSalarySource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["DailyAttendanceSalarySource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["DailyAttendanceSalarySource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> AbsenceReasonReportSource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["AbsenceReasonReportSource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["AbsenceReasonReportSource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> LateDutyRotaReportSource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["LateDutyRotaReportSource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["LateDutyRotaReportSource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> DILReportSource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["DILReportSource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["DILReportSource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> WeeklyOvertimeReportSource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["WeeklyOvertimeReportSource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["WeeklyOvertimeReportSource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<EmployeeAttendanceEntity> AspirePayrollReportSource
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["AspirePayrollReportSource"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["AspirePayrollReportSource"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private List<AttendanceStatisticsEntity> PunctualityReportSource
        {
            get
            {
                List<AttendanceStatisticsEntity> list = Session["PunctualityReportSource"] as List<AttendanceStatisticsEntity>;
                if (list == null)
                    Session["PunctualityReportSource"] = list = new List<AttendanceStatisticsEntity>();

                return list;
            }
        }

        private List<PunctualityEntity> WeeklyPunctualityReportSource
        {
            get
            {
                List<PunctualityEntity> list = Session["WeeklyPunctualityReportSource"] as List<PunctualityEntity>;
                if (list == null)
                    Session["WeeklyPunctualityReportSource"] = list = new List<PunctualityEntity>();

                return list;
            }
        }

        private List<PunctualityEntity> UnpunctualEmployeeReportSource
        {
            get
            {
                List<PunctualityEntity> list = Session["UnpunctualEmployeeReportSource"] as List<PunctualityEntity>;
                if (list == null)
                    Session["UnpunctualEmployeeReportSource"] = list = new List<PunctualityEntity>();

                return list;
            }
        }

        private List<EmployeeAbsentEntity> EmployeeAbsencesReportSource
        {
            get
            {
                List<EmployeeAbsentEntity> list = Session["EmployeeAbsencesReportSource"] as List<EmployeeAbsentEntity>;
                if (list == null)
                    Session["EmployeeAbsencesReportSource"] = list = new List<EmployeeAbsentEntity>();

                return list;
            }
        }
        #endregion

        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            base.IsRetrieveUserInfo = true;
            base.OnInit(e);

            //if (!this.IsPostBack)
            //{
            //    if (this.Master.IsSessionExpired)
            //        Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);

            //    this.Master.SetPageForm(UIHelper.FormAccessCodes.TASREPORTS.ToString());
            //}

            #region Check culture info
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            }
            #endregion
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                #region Set Page Title and Display Login user
                //StringBuilder sb = new StringBuilder();
                //string position = UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_POSITION_DESC]));
                //string costCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                //string costCenterDesc = UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER_NAME]));
                //string extension = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EXT]);

                //if (!string.IsNullOrEmpty(position))
                //{
                //    sb.Append(string.Format("Position: {0} <br />", position));
                //}
                //if (!string.IsNullOrEmpty(costCenter) && !string.IsNullOrEmpty(costCenterDesc))
                //{
                //    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                //}

                //this.Master.LogOnUser = string.Concat("Welcome, ", UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])));
                //this.Master.LogOnUserInfo = sb.ToString().Trim();
                //this.Master.FormTitle = UIHelper.PAGE_REPORT_VIEWER_TITLE;
                #endregion

                #region Check if user has permission to access the page
                //if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                //{
                //    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                //    {
                //        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_REPORT_VIEWER_TITLE), true);
                //    }
                //}
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnAdd.Visible = this.Master.IsCreateAllowed;
                //this.btnSave.Visible = this.Master.IsEditAllowed;
                //this.btnRemove.Visible = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                //this.Master.DefaultButton = this.btnClose.UniqueID;
                #endregion

                ClearForm();
                ProcessQueryString();
                DisplayReport();
            }

            //ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Private Methods
        private void DisplayReport()
        {
            #region Initialize variables
            DALProxy proxy = new DALProxy();
            InstanceReportSource instanceRepSource = new InstanceReportSource();
            Report report = null;
            string error = string.Empty;
            string innerError = string.Empty;
            string dnsName = string.Empty;
            string machineName = string.Empty;
            #endregion

            try
            {
                // Get the PC Name
                machineName = GetMachineName(ref dnsName);

                switch (this.CurrentReportType)
                {
                    case UIHelper.ReportTypes.VisitorLogReport:
                        #region Display Visitor Log Report
                        VisitorLogReport visitorLogReport = new VisitorLogReport();
                        report = visitorLogReport;

                        if (this.VisitorLogReportSource != null)
                        {
                            #region Get swipe records
                            var rawData = proxy.GetVisitorSwipeHistory(this.VisitorLogReportSource.VisitorCardNo, this.VisitorLogReportSource.VisitDate,
                                this.VisitorLogReportSource.VisitDate, 0, 0, ref error, ref innerError);
                            if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                            {
                                if (!string.IsNullOrEmpty(innerError))
                                    throw new Exception(error, new Exception(innerError));
                                else
                                    throw new Exception(error);
                            }
                            else
                            {
                                if (rawData != null && rawData.Count() > 0)
                                {
                                    // Store collection to session
                                    this.SwipeDataList.AddRange(rawData);
                                }
                            }
                            #endregion

                            #region Pass data to the report and display it
                            //this.VisitorLogReportSource.VisitorSwipeList = this.SwipeDataList;

                            // Set data source
                            visitorLogReport.DataSource = this.VisitorLogReportSource;
                            visitorLogReport.SwipeDataList = this.SwipeDataList;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            #endregion

                            // Show the report
                            this.repViewer.Report = report;
                            this.repViewer.ViewMode = Telerik.ReportViewer.WebForms.ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.VisitorPassSummaryReport:
                        #region Display Visitor Pass Summary Report
                        VisitorPassSummaryReport visitorSummaryReport = new VisitorPassSummaryReport();
                        report = visitorSummaryReport;

                        if (this.VisitorSummaryReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            visitorSummaryReport.DataSource = this.VisitorSummaryReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["TotalRecords"].Value = this.VisitorSummaryReportSource.Count;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.ContractorAttendanceReport:
                        #region Display Contractor Attendace Report
                        ContractorAttendanceReport contractorReport = new ContractorAttendanceReport();
                        report = contractorReport;

                        if (this.ContractorAttendanceReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            contractorReport.DataSource = this.ContractorAttendanceReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["TotalRecords"].Value = this.ContractorAttendanceReportSource.Count;
                            report.ReportParameters["DateFilter"].Value = this.DateFilterString;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                        #endregion

                    case UIHelper.ReportTypes.ShiftProjectionReport:
                        #region Display Shift Projection Report
                        ShiftProjectionReport shiftProjectionReport = new ShiftProjectionReport();
                        report = shiftProjectionReport;

                        if (this.ShiftProjectionReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            shiftProjectionReport.DataSource = this.ShiftProjectionReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["CostCenter"].Value = this.CostCenter;
                            report.ReportParameters["DateFrom"].Value = this.StartDate.HasValue ? Convert.ToDateTime(this.StartDate).ToString("MMMM - yyyy") : string.Empty;
                            report.ReportParameters["DateTo"].Value = this.EndDate.HasValue ? Convert.ToDateTime(this.EndDate).ToString("MMMM - yyyy") : string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.EmployeeAttendanceHistoryReport:
                        #region Display Employee Attendance History Report
                        EmployeeAttendanceHistoryReport attendanceHistoryReport = new EmployeeAttendanceHistoryReport();
                        report = attendanceHistoryReport;

                        if (this.EmpAttendanceHistoryReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            attendanceHistoryReport.DataSource = this.EmpAttendanceHistoryReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Date Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));                                    
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Date Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }                                   
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.EmployeeWorkplaceAttendanceReport:
                        #region Display Employee Attendance History Report (With workplace swipes information)
                        EmployeeAttendanceWorkplace attendanceReport = new EmployeeAttendanceWorkplace();
                        report = attendanceReport;

                        if (this.EmpAttendanceHistoryReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            attendanceReport.DataSource = this.EmpAttendanceHistoryReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Date Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Date Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                        #endregion

                    case UIHelper.ReportTypes.DutyROTAReport:
                        #region Display Duty ROTA Report
                        DutyROTAReport dutyROTAReport = new DutyROTAReport();
                        report = dutyROTAReport;

                        if (this.DutyROTAReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dutyROTAReport.DataSource = this.DutyROTAReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.DailyAttendanceReportNonSalaryStaff:
                        #region Display Daily Attendance Report for Non-Salary Staff
                        DailyAttendanceReportNS dailyAttendanceReportNS = new DailyAttendanceReportNS();
                        report = dailyAttendanceReportNS;

                        if (this.DailyAttendanceNonSalarySource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dailyAttendanceReportNS.DataSource = this.DailyAttendanceNonSalarySource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["EmployeeType"].Value = "(NON-SALARY STAFF)";                            
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                        #endregion

                    case UIHelper.ReportTypes.DailyAttendanceReportSalaryStaff:
                        #region Display Daily Attendance Report for Salary Staff
                        //DailyAttendanceReportSS dailyAttendanceSalaryStaff = new DailyAttendanceReportSS();
                        DailyAttendanceReportNS dailyAttendanceReportSS = new DailyAttendanceReportNS();
                        report = dailyAttendanceReportSS;

                        if (this.DailyAttendanceSalarySource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dailyAttendanceReportSS.DataSource = this.DailyAttendanceSalarySource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["EmployeeType"].Value = "(SALARY STAFF)";
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.DailyAttendanceReportSalaryStaffOnly:
                        #region Display Daily Attendance Report for Salary Staff
                        DailyAttendanceReportSS dailyAttendanceSalaryStaffOnly = new DailyAttendanceReportSS();
                        report = dailyAttendanceSalaryStaffOnly;

                        if (this.DailyAttendanceSalarySource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dailyAttendanceSalaryStaffOnly.DataSource = this.DailyAttendanceSalarySource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.DailyAttendanceReportAll:
                        #region Display Daily Attendance Report for Salary and Non-Salary Staff
                        // Create report book
                        ReportBook reportBook = new ReportBook();

                        #region Display Daily Attendance Report for Non-Salary Staff
                        DailyAttendanceReportNS dailyAttendanceNonSalary = new DailyAttendanceReportNS();
                        report = dailyAttendanceNonSalary;

                        if (this.DailyAttendanceNonSalarySource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dailyAttendanceNonSalary.DataSource = this.DailyAttendanceNonSalarySource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["EmployeeType"].Value = "(NON-SALARY STAFF)";
                            #endregion

                            // Add report to the report book collection                            
                            reportBook.Reports.Add(report);
                            #endregion
                        }
                        #endregion

                        #region Display Daily Attendance Report for Salary Staff
                        //DailyAttendanceReportSS dailyAttendanceSalaryStaff = new DailyAttendanceReportSS();
                        DailyAttendanceReportNS dailyAttendanceSalaryStaff = new DailyAttendanceReportNS();
                        report = dailyAttendanceSalaryStaff;

                        if (this.DailyAttendanceSalarySource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dailyAttendanceSalaryStaff.DataSource = this.DailyAttendanceSalarySource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["EmployeeType"].Value = "(SALARY STAFF)";
                            #endregion

                            // Add report to the report book collection                            
                            reportBook.Reports.Add(report);
                            #endregion
                        }
                        #endregion

                        // Display all reports
                        if (reportBook.Reports.Count > 0)
                        {
                            this.repViewer.Report = reportBook;
                            this.repViewer.ViewMode = Telerik.ReportViewer.WebForms.ViewMode.PrintPreview;
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.AbsenceReasonReport:
                        #region Display Absence Reason Report
                        AbsenceReasonReport absenceReport = new AbsenceReasonReport();
                        report = absenceReport;

                        if (this.AbsenceReasonReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            absenceReport.DataSource = this.AbsenceReasonReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.LateDutyRotaReport:
                        #region Display DIL Due to Late Entry of Duty Rota Report
                        rptLateDutyRotaReport lateDutyRotaReport = new rptLateDutyRotaReport();
                        report = lateDutyRotaReport;

                        if (this.LateDutyRotaReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            lateDutyRotaReport.DataSource = this.LateDutyRotaReportSource;
                            lateDutyRotaReport.ReportDataList = this.LateDutyRotaReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.DayInLieuReport:
                        #region Display Day In Lieu Report
                        DILReport dilReport = new DILReport();
                        report = dilReport;

                        if (this.DILReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            dilReport.DataSource = this.DILReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.WeeklyOvertimeReport:
                        #region Display Weekly Overtime Report
                        WeeklyOvertimeReport weeklyOvertimeReport = new WeeklyOvertimeReport();
                        report = weeklyOvertimeReport;

                        if (this.WeeklyOvertimeReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            weeklyOvertimeReport.DataSource = this.WeeklyOvertimeReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.AspirePayrollReport:
                        #region Display Aspire Employee Payroll Report
                        AspirePayrollReport aspirePayrollReport = new AspirePayrollReport();
                        report = aspirePayrollReport;

                        if (this.AspirePayrollReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            aspirePayrollReport.DataSource = this.AspirePayrollReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.PunctualityReport:
                        #region Display Punctuality Report
                        PunctualityReport punctualityReport = new PunctualityReport();
                        report = punctualityReport;

                        if (this.PunctualityReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            punctualityReport.DataSource = this.PunctualityReportSource;
                            punctualityReport.ReportDataList = this.PunctualityReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);
                            report.ReportParameters["CostCenter"].Value = this.CostCenter;
                            report.ReportParameters["ReportTitle"].Value = this.ReportTitle;

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: From {0} To {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.PunctualitySummaryReport:
                        #region Display Weekly Employee Punctuality Report
                        WeeklyPunctualityReport weeklyPunctualityReport = new WeeklyPunctualityReport();
                        report = weeklyPunctualityReport;

                        if (this.WeeklyPunctualityReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            weeklyPunctualityReport.DataSource = this.WeeklyPunctualityReportSource;
                            weeklyPunctualityReport.ReportDataList = this.WeeklyPunctualityReportSource;

                            #region Set the parameters
                            int lateAttendanceThreshold = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["LateAttendanceThreshold"]);
                            int earlyLeavingThreshold = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["EarlyLeavingThreshold"]);
                            int punctualityOccurence = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["PunctualityOccurence"]);

                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);                            

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0} to {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;

                            report.ReportParameters["LateAttendanceThreshold"].Value = lateAttendanceThreshold;
                            report.ReportParameters["EarlyLeavingThreshold"].Value = earlyLeavingThreshold;
                            report.ReportParameters["PunctualityOccurence"].Value = punctualityOccurence;                            
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.UnpuntualEmployeeSummary:
                        #region Display Unpunctual Employee Summary Report
                        PunctualityByPeriodReport unpunctualEmployeeReport = new PunctualityByPeriodReport();
                        report = unpunctualEmployeeReport;

                        if (this.UnpunctualEmployeeReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            unpunctualEmployeeReport.DataSource = this.UnpunctualEmployeeReportSource;
                            unpunctualEmployeeReport.ReportDataList = this.UnpunctualEmployeeReportSource;

                            #region Set the parameters
                            int lateAttendanceThreshold = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["LateAttendanceThreshold"]);
                            int earlyLeavingThreshold = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["EarlyLeavingThreshold"]);
                            int punctualityOccurence = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["PunctualityOccurence"]);

                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["MachineName"].Value = string.Format(@"{0}\{1}", dnsName, machineName);

                            if (this.StartDate.HasValue && this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["DateFilter"].Value = string.Format("Period: {0} to {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["DateFilter"].Value = string.Empty;

                            report.ReportParameters["LateAttendanceThreshold"].Value = lateAttendanceThreshold;
                            report.ReportParameters["EarlyLeavingThreshold"].Value = earlyLeavingThreshold;
                            report.ReportParameters["PunctualityOccurence"].Value = punctualityOccurence;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                    #endregion

                    case UIHelper.ReportTypes.EmployeeAbsencesSummaryReport:
                        #region Display Employee Absences Summary Report
                        EmployeeAbsencesReport absencesReport = new EmployeeAbsencesReport();
                        report = absencesReport;

                        if (this.EmployeeAbsencesReportSource.Count > 0)
                        {
                            #region Pass data to the report and display it
                            // Set data source
                            absencesReport.DataSource = this.EmployeeAbsencesReportSource;
                            absencesReport.ReportDataList = this.EmployeeAbsencesReportSource;

                            #region Set the parameters
                            report.ReportParameters["UserID"].Value = this.Page.User.Identity.Name;
                            report.ReportParameters["TotalAbsences"].Value = this.EmployeeAbsencesReportSource.Count;
                            report.ReportParameters["MachineName"].Value = machineName;

                            if (this.StartDate.HasValue && 
                                this.EndDate.HasValue)
                            {
                                if (this.StartDate == this.EndDate)
                                {
                                    report.ReportParameters["Period"].Value = string.Format("Period Covered: {0}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"));
                                }
                                else
                                {
                                    report.ReportParameters["Period"].Value = string.Format("Period Covered: {0} to {1}",
                                        Convert.ToDateTime(this.StartDate).ToString("dd-MMM-yyyy"),
                                        Convert.ToDateTime(this.EndDate).ToString("dd-MMM-yyyy"));
                                }
                            }
                            else
                                report.ReportParameters["Period"].Value = string.Empty;
                            #endregion

                            // Show the report
                            instanceRepSource.ReportDocument = report;
                            this.repViewer.ReportSource = instanceRepSource;
                            this.repViewer.ViewMode = ViewMode.PrintPreview;
                            #endregion
                        }

                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
            finally
            {
                instanceRepSource = null;
            }
        }

        private string GetMachineName(ref string dnsName)
        {
            try
            {
                string[] computerInfo = System.Net.Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName.Split(new Char[] { '.' });
                String machineName = System.Environment.MachineName;

                if (computerInfo.Length > 0)
                    dnsName = computerInfo[1].ToString();

                return machineName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region Action Buttons
        protected void btnClose_Click(object sender, EventArgs e)
        {
            if (this.CallerForm != string.Empty)
            {
                Response.Redirect
                (
                    String.Format(this.CallerForm + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_REPORT_VIEWER
                ),
                false);
            }
            else
            {
                Response.Redirect(UIHelper.PAGE_HOME, false);
            }
        }
        #endregion

        #region Interface Implementation
        public void ClearForm()
        {
            KillSessions();
        }

        public void AddControlsAttribute()
        {

        }

        public void SetButtonsVisibility()
        {

        }

        public void FillComboData()
        {

        }

        public void ProcessQueryString()
        {
            this.CurrentReportType = UIHelper.ReportTypes.NotDefined;
            try
            {
                this.CurrentReportType = (UIHelper.ReportTypes)Enum.Parse(typeof(UIHelper.ReportTypes), UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_REPORT_TYPE_KEY]));
            }
            catch (Exception)
            {
            }

            this.CallerForm = !string.IsNullOrEmpty(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY])
                ? UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY])
                : this.CallerForm;

            this.DateFilterString = UIHelper.ConvertObjectToString(Request.QueryString["DateFilterString"]);

            // Query strings used in "Shift Projection Report"
            this.CostCenter = Server.UrlDecode(UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]));
            this.StartDate = UIHelper.ConvertObjectToDate(Request.QueryString[UIHelper.QUERY_STRING_STARTDATE_KEY]);
            this.EndDate = UIHelper.ConvertObjectToDate(Request.QueryString[UIHelper.QUERY_STRING_ENDDATE_KEY]);
            this.ReportTitle = Server.UrlDecode(UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_REPORT_TITLE_KEY]));
        }

        public void KillSessions()
        {
            ViewState["CurrentReportType"] = null;
        }
        #endregion
    }
}