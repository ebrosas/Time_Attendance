using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.DAL.Employee;
using Telerik.Web.UI;
using System.Configuration;
using GARMCO.Common.Object;

namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    public partial class EmpAbsencesReportFilter : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoDateDuration,
            NoDateFrom,
            NoDateTo,
            InvalidDateRange,
            NoSpecifiedEmpNo            
        }
        #endregion

        #region Properties
        public string FormAccess
        {
            get
            {
                string userFormAccess = GAPConstants.FORM_ACCESS_DEFAULT;
                if (!String.IsNullOrEmpty(this.hidFormAccess.Value))
                    userFormAccess = this.hidFormAccess.Value;

                return userFormAccess;
            }

            set
            {
                this.hidFormAccess.Value = value;
            }
        }

        public string SearchUrl
        {
            get
            {
                return this.hidSearchUrl.Value;
            }

            set
            {
                this.hidSearchUrl.Value = value;
            }
        }

        private Dictionary<string, object> EmpAbsencesReportStorage
        {
            get
            {
                Dictionary<string, object> list = Session["EmpAbsencesReportStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["EmpAbsencesReportStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["EmpAbsencesReportStorage"] = value;
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

        private ValidationErrorType ErrorType
        {
            get
            {
                ValidationErrorType result = ValidationErrorType.NoError;
                if (ViewState["ErrorType"] != null)
                {
                    try
                    {
                        result = (ValidationErrorType)Enum.Parse(typeof(ValidationErrorType), UIHelper.ConvertObjectToString(ViewState["ErrorType"]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            set
            {
                ViewState["ErrorType"] = value;
            }
        }

        private string CustomErrorMsg
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CustomErrorMsg"]);
            }
            set
            {
                ViewState["CustomErrorMsg"] = value;
            }
        }

        private List<string> AllowedCostCenterList
        {
            get
            {
                List<string> list = Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string>;
                if (list == null)
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = list = new List<string>();

                return list;
            }
        }

        private List<CostCenterEntity> CostCenterList
        {
            get
            {
                List<CostCenterEntity> list = ViewState["CostCenterList"] as List<CostCenterEntity>;
                if (list == null)
                    ViewState["CostCenterList"] = list = new List<CostCenterEntity>();

                return list;
            }
            set
            {
                ViewState["CostCenterList"] = value;
            }
        }

        private List<EmployeeAbsentEntity> ReportDataList
        {
            get
            {
                List<EmployeeAbsentEntity> list = ViewState["ReportDataList"] as List<EmployeeAbsentEntity>;
                if (list == null)
                    ViewState["ReportDataList"] = list = new List<EmployeeAbsentEntity>();

                return list;
            }
            set
            {
                ViewState["ReportDataList"] = value;
            }
        }

        private int EmployeeNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["EmployeeNo"]);
            }
            set
            {
                ViewState["EmployeeNo"] = value;
            }
        }
        #endregion

        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            base.IsRetrieveUserInfo = true;
            base.OnInit(e);

            if (!this.IsPostBack)
            {
                if (this.Master.IsSessionExpired)
                    Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);

                this.Master.SetPageForm(UIHelper.FormAccessCodes.ABSSUMYRPT.ToString());

                // Checks if search url is specified
                int index = Request.QueryString.ToString().IndexOf("searchUrl=");
                if (index > -1)
                    this.SearchUrl = Server.UrlDecode(Request.QueryString.ToString().Substring(index + 10));
            }

            #region Check culture info
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            }
            #endregion
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                FillCostCenterCombo();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                #region Set Page Title and Display Login user
                StringBuilder sb = new StringBuilder();
                string position = UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_POSITION_DESC]));
                string costCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                string costCenterDesc = UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER_NAME]));
                string extension = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EXT]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);

                if (!string.IsNullOrEmpty(userID))
                {
                    sb.Append(string.Format(@"User ID: GARMCO\{0} <br />", userID));
                }

                //if (!string.IsNullOrEmpty(position))
                //{
                //    sb.Append(string.Format("Position: {0} <br />", position));
                //}

                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Format("Welcome {0}",
                   UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]), UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));

                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_ABSENCES_SUMMARY_REPORT_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMPLOYEE_ABSENCES_SUMMARY_REPORT_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnShowReport.Enabled = this.Master.IsPrintAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnShowReport.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.EmpAbsencesReportStorage.Count > 0)
                {
                    if (this.EmpAbsencesReportStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.EmpAbsencesReportStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show last inquiry data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.EmpAbsencesReportStorage.Clear();
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);

                        // Save Employee No. to session
                        this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("EmpAbsencesReportStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    this.chkPayPeriod.Checked = true;
                    this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

                    int month = DateTime.Now.Month;
                    if (DateTime.Now.Day >= 16)
                        month = month + 1;

                    this.txtYear.Text = DateTime.Now.Year.ToString();
                    if (month > 12)
                    {
                        month = 1;
                        this.txtYear.Text = (DateTime.Now.Year + 1).ToString();
                    }

                    this.cboMonth.SelectedValue = month.ToString();
                    this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                    this.cboMonth.Focus();
                    #endregion
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();

            #region Initialize date range
            //this.chkPayPeriod.Checked = true;
            //this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            //int month = DateTime.Now.Month;
            //if (DateTime.Now.Day >= 16)
            //    month = month + 1;

            //this.txtYear.Text = DateTime.Now.Year.ToString();
            //if (month > 12)
            //{
            //    month = 1;
            //    this.txtYear.Text = (DateTime.Now.Year + 1).ToString();
            //}

            //this.cboMonth.SelectedValue = month.ToString();
            //this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
            //this.cboMonth.Focus();
            #endregion
        }

        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                #region Check date range
                if (this.dtpDateFrom.SelectedDate == null &&
                    this.dtpDateTo.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDateDuration.ToString();
                    this.ErrorType = ValidationErrorType.NoDateDuration;
                    this.cusValDateDuration.Validate();
                    errorCount++;
                }
                else if (this.dtpDateFrom.SelectedDate != null &&
                    this.dtpDateTo.SelectedDate != null)
                {
                    if (this.dtpDateFrom.SelectedDate.Value > this.dtpDateTo.SelectedDate.Value)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                        this.ErrorType = ValidationErrorType.InvalidDateRange;
                        this.cusValDateDuration.Validate();
                        errorCount++;
                    }
                }
                else
                {
                    if (this.dtpDateFrom.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoDateFrom.ToString();
                        this.ErrorType = ValidationErrorType.NoDateFrom;
                        this.cusValDateDuration.Validate();
                        errorCount++;
                    }
                    else if (this.dtpDateTo.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoDateTo.ToString();
                        this.ErrorType = ValidationErrorType.NoDateTo;
                        this.cusValDateDuration.Validate();
                        errorCount++;
                    }
                }

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                #endregion

                GetReportData(true);

                #region Display in the report all data in the grid
                if (this.ReportDataList != null &&
                    this.ReportDataList.Count > 0)
                {
                    // Save report data to session
                    Session["EmployeeAbsencesReportSource"] = this.ReportDataList;

                    // Determine the date range
                    string startDate = this.dtpDateFrom.SelectedDate.Value.ToString();
                    string endDate = this.dtpDateTo.SelectedDate.Value.ToString();
                    string costCenter = string.Format("Cost Center: {0}",
                        this.cboCostCenter.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID ? "All" : this.cboCostCenter.Text);

                    StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                    // Show the report
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}",
                        UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                        UIHelper.ReportTypes.EmployeeAbsencesSummaryReport.ToString(),
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_EMPLOYEE_ABSENCES_REPORT_FILTER,
                        UIHelper.QUERY_STRING_STARTDATE_KEY,
                        startDate,
                        UIHelper.QUERY_STRING_ENDDATE_KEY,
                        endDate,
                        UIHelper.QUERY_STRING_COSTCENTER_KEY,
                        costCenter
                    ),
                    false);
                }
                else
                    throw new Exception("No matching records were found in the database. Please modify the search criterias then try to view the report again!");
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnGet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform data validation
                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSpecifiedEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoSpecifiedEmpNo;
                    this.cusValEmpNo.Validate();
                    return;
                }
                #endregion

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                string error = string.Empty;
                string innerError = string.Empty;

                EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
                if (empInfo != null)
                {
                    if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                    {
                        #region Check if cost center exist in the allowed cost center list
                        // Save Employee No. to session
                        this.EmployeeNo = empNo;
                        #endregion
                    }
                    else
                    {
                        #region Get employee info from the employee master
                        DALProxy proxy = new DALProxy();
                        var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                        if (rawData != null)
                        {
                            // Save Employee No. to session
                            this.EmployeeNo = empNo;
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_EMPLOYEE_ABSENCES_REPORT_FILTER
            ),
            false);
        }
        #endregion

        #region Page Control Events
        protected void cusGenericValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            CustomValidator validator = source as CustomValidator;

            try
            {
                if (this.ErrorType == ValidationErrorType.CustomFormError)
                {
                    validator.ErrorMessage = this.CustomErrorMsg != string.Empty ? this.CustomErrorMsg : "Unhandled Error Occured";
                    validator.ToolTip = this.CustomErrorMsg != string.Empty ? this.CustomErrorMsg : "Unhandled Error Occured";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateDuration)
                {
                    validator.ErrorMessage = "Date duration is required and should not be left blank or unspecified.";
                    validator.ToolTip = "Date duration is required and should not be left blank or unspecified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateFrom)
                {
                    validator.ErrorMessage = "Start date is required and should not be left blank or unspecified.";
                    validator.ToolTip = "Start date is required and should not be left blank or unspecified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateTo)
                {
                    validator.ErrorMessage = "End date is required and should not be left blank or unspecified.";
                    validator.ToolTip = "End date is required and should not be left blank or unspecified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date duration is invalid.";
                    validator.ToolTip = "The specified date duration is invalid.";
                    args.IsValid = false;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                this.txtGeneric.Text = string.Empty;
                this.ErrorType = ValidationErrorType.NoError;
            }
        }

        protected void chkPayPeriod_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriod.Checked)
            {
                this.cboMonth.Enabled = true;
                this.txtYear.Enabled = true;
                this.dtpDateFrom.Enabled = false;
                this.dtpDateTo.Enabled = false;

                #region Set the current pay period
                int month = DateTime.Now.Month;
                if (DateTime.Now.Day >= 16)
                    month = month + 1;

                this.txtYear.Text = DateTime.Now.Year.ToString();

                if (month > 12)
                {
                    month = 1;
                    this.txtYear.Text = (DateTime.Now.Year + 1).ToString();
                }

                this.cboMonth.SelectedValue = month.ToString();
                this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                this.cboMonth.Focus();
                #endregion
            }
            else
            {
                this.cboMonth.Enabled = false;
                this.txtYear.Enabled = false;
                this.dtpDateFrom.Enabled = true;
                this.dtpDateTo.Enabled = true;
                this.cboMonth.SelectedIndex = -1;
                this.cboMonth.Text = string.Empty;
                this.txtYear.Text = string.Empty;
                this.dtpDateFrom.Focus();
            }
        }

        protected void cboMonth_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpDateFrom.SelectedDate = this.dtpDateTo.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYear.Text == string.Empty)
            {
                this.txtYear.Text = DateTime.Now.Year.ToString();
            }

            int month = UIHelper.ConvertObjectToInt(this.cboMonth.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYear.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpDateFrom.SelectedDate = startDate;
            this.dtpDateTo.SelectedDate = endDate;
        }

        protected void txtYear_TextChanged(object sender, EventArgs e)
        {
            this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.txtYear.Text = string.Empty;
            this.dtpDateFrom.SelectedDate = null;
            this.dtpDateTo.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.chkPayPeriod.Checked = false;
            #endregion

            // Clear collections
            KillSessions();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.CostCenterList.Clear();
            this.ReportDataList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CallerForm"] = null;
            ViewState["EmployeeNo"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.EmpAbsencesReportStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.EmpAbsencesReportStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.EmpAbsencesReportStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.EmpAbsencesReportStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.EmpAbsencesReportStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            if (this.EmpAbsencesReportStorage.ContainsKey("ReportDataList"))
                this.ReportDataList = this.EmpAbsencesReportStorage["ReportDataList"] as List<EmployeeAbsentEntity>;
            else
                this.ReportDataList = null;

            if (this.EmpAbsencesReportStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.EmpAbsencesReportStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.EmpAbsencesReportStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.EmpAbsencesReportStorage["chkPayPeriod"]);
            else
                this.chkPayPeriod.Checked = false;

            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            if (this.EmpAbsencesReportStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.EmpAbsencesReportStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.EmpAbsencesReportStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.EmpAbsencesReportStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.EmpAbsencesReportStorage.ContainsKey("cboMonth"))
            {
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.EmpAbsencesReportStorage["cboMonth"]);
            }
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.EmpAbsencesReportStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.EmpAbsencesReportStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.EmpAbsencesReportStorage.ContainsKey("dtpDateFrom"))
                this.dtpDateFrom.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpAbsencesReportStorage["dtpDateFrom"]);
            else
                this.dtpDateFrom.SelectedDate = null;

            if (this.EmpAbsencesReportStorage.ContainsKey("dtpDateTo"))
                this.dtpDateTo.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpAbsencesReportStorage["dtpDateTo"]);
            else
                this.dtpDateTo.SelectedDate = null;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.EmpAbsencesReportStorage.Clear();
            this.EmpAbsencesReportStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.EmpAbsencesReportStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.EmpAbsencesReportStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.EmpAbsencesReportStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.EmpAbsencesReportStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.EmpAbsencesReportStorage.Add("dtpDateFrom", this.dtpDateFrom.SelectedDate);
            this.EmpAbsencesReportStorage.Add("dtpDateTo", this.dtpDateTo.SelectedDate);
            this.EmpAbsencesReportStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            #endregion

            #region Save Query String values to collection
            this.EmpAbsencesReportStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.EmpAbsencesReportStorage.Add("CostCenterList", this.CostCenterList);
            this.EmpAbsencesReportStorage.Add("ReportDataList", this.ReportDataList);
            this.EmpAbsencesReportStorage.Add("EmployeeNo", this.EmployeeNo);
            #endregion
        }

        private void DisplayFormLevelError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return;

            this.CustomErrorMsg = errorMsg;
            this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
            this.ErrorType = ValidationErrorType.CustomFormError;
            this.cusValButton.Validate();
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            //FillCostCenterCombo(true);
        }

        private void GetPayPeriod(int year, int month, ref DateTime? startDate, ref DateTime? endDate)
        {
            try
            {
                switch (month)
                {
                    case 1:     // January
                        startDate = UIHelper.ConvertObjectToDate(string.Format("16/{0}/{1}", month + 11, year - 1));
                        endDate = UIHelper.ConvertObjectToDate(string.Format("15/{0}/{1}", month, year));
                        break;

                    case 2:     // February
                    case 3:     // March
                    case 4:     // April
                    case 5:     // May
                    case 6:     // June
                    case 7:     // July
                    case 8:     // August
                    case 9:     // September
                    case 10:    // October
                    case 11:    // November
                    case 12:    // December
                        startDate = UIHelper.ConvertObjectToDate(string.Format("16/{0}/{1}", month - 1, year));
                        endDate = UIHelper.ConvertObjectToDate(string.Format("15/{0}/{1}", month, year));
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion

        #region Database Access
        private void GetReportData(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables                               
                DateTime? startDate = this.dtpDateFrom.SelectedDate;
                DateTime? endDate = this.dtpDateTo.SelectedDate;
                string costCenter = this.cboCostCenter.Text;
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }
                #endregion

                #region Fill data to the collection
                List<EmployeeAbsentEntity> reportData = new List<EmployeeAbsentEntity>();
                if (!reloadDataFromDB)
                {
                    reportData = this.ReportDataList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    List<EmployeeAbsentEntity> rawData = proxy.GetEmployeeAbsences(startDate, endDate, costCenter, empNo, ref error, ref innerError);
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
                            reportData.AddRange(rawData);
                        }
                    }
                }

                // Store collection to session
                this.ReportDataList = reportData;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillCostCenterCombo()
        {
            DataView dv = this.objCostCenter.Select() as DataView;
            if (dv == null || dv.Count == 0)
                return;

            DataRow[] source = new DataRow[dv.Count];
            dv.Table.Rows.CopyTo(source, 0);
            EmployeeDAL.CostCenterDataTable filteredDT = new EmployeeDAL.CostCenterDataTable();

            #region Add default selection item
            EmployeeDAL.CostCenterRow defaultRow = filteredDT.NewCostCenterRow();
            defaultRow.CostCenter = String.Empty;
            defaultRow.CostCenterName = "Please select a Cost Center...";
            defaultRow.Company = String.Empty;
            defaultRow.SuperintendentNo = 0;
            defaultRow.SuperintendentName = String.Empty;
            defaultRow.ManagerNo = 0;
            defaultRow.ManagerName = String.Empty;
            filteredDT.Rows.Add(defaultRow);
            #endregion

            if (this.AllowedCostCenterList.Count > 0)
            {
                #region Filter list based on allowed cost center
                foreach (string filter in this.AllowedCostCenterList)
                {
                    DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
                    foreach (DataRow rw in rows)
                    {
                        EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                        row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                        row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                        row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);
                        filteredDT.Rows.Add(row);
                    }
                }
                #endregion
            }
            else if (this.AllowedCostCenterList.Count == 0 && 
                UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty)
            {
                this.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

                #region Filter list based on allowed cost center
                foreach (string filter in this.AllowedCostCenterList)
                {
                    DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
                    foreach (DataRow rw in rows)
                    {
                        EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                        row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                        row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                        row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);
                        filteredDT.Rows.Add(row);
                    }
                }
                #endregion
            }
            else
            {
                #region No filtering for cost center
                //foreach (DataRow rw in source)
                //{
                //    EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                //    row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                //    row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                //    row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                //    row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                //    row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                //    row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                //    row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);
                //    filteredDT.Rows.Add(row);
                //}
                #endregion
            }

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();
            }
        }
        #endregion                
    }
}