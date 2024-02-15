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

namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    public partial class WeeklyOvertimeReportFilter : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoCostCenter,
            NoStartDate,
            NoEndDate,
            InvalidDateRange
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

        private Dictionary<string, object> WeeklyOvertimeStorage
        {
            get
            {
                Dictionary<string, object> list = Session["WeeklyOvertimeStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["WeeklyOvertimeStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["WeeklyOvertimeStorage"] = value;
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

        private List<EmployeeAttendanceEntity> ReportDataList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["ReportDataList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["ReportDataList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["ReportDataList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.WEEKOTRPT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_WEEKLY_OVERTIME_REPORT_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_WEEKLY_OVERTIME_REPORT_TITLE), true);
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
                if (this.WeeklyOvertimeStorage.Count > 0)
                {
                    if (this.WeeklyOvertimeStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.WeeklyOvertimeStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.WeeklyOvertimeStorage.Clear();
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls  
                    this.dtpEndDate.MaxDate = DateTime.Now;
                    this.dtpEndDate.SelectedDate = DateTime.Now;
                    this.dtpStartDate.SelectedDate = DateTime.Now.AddDays(-7);

                    // Select all cost centers
                    this.chkSelectAll.Checked = true;
                    this.chkSelectAll_CheckedChanged(this.chkSelectAll, new EventArgs());
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
            #region Clear the form
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.lbCostCenter.ClearSelection();
            this.lbCostCenter.ClearChecked();
            this.chkSelectAll.Checked = true;
            this.chkSelectAll_CheckedChanged(this.chkSelectAll, new EventArgs());

            // Cler collections
            this.ReportDataList.Clear();
            this.CostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            #endregion
        }

        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check selected Cost Center
            if (this.lbCostCenter.Items.Count > 0 &&
                this.lbCostCenter.CheckedItems.Count == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoCostCenter.ToString();
                this.ErrorType = ValidationErrorType.NoCostCenter;
                this.cusValButton.Validate();
                errorCount++;
            }

            if (this.dtpStartDate.SelectedDate != null &&
                    this.dtpEndDate.SelectedDate != null)
            {
                if (this.dtpStartDate.SelectedDate > this.dtpEndDate.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateRange;
                    this.cusValStartDate.Validate();
                    errorCount++;
                }
            }
            else
            {
                // Check Start Date
                if (this.dtpStartDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoStartDate.ToString();
                    this.ErrorType = ValidationErrorType.NoStartDate;
                    this.cusValStartDate.Validate();
                    errorCount++;
                }

                // Check End Date
                if (this.dtpEndDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEndDate.ToString();
                    this.ErrorType = ValidationErrorType.NoEndDate;
                    this.cusValEndDate.Validate();
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

            GetReportData(true);

            #region Show the Daily Attendance Report
            if (this.ReportDataList != null &&
                this.ReportDataList.Count > 0)
            {
                StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                // Save report data to session
                Session["WeeklyOvertimeReportSource"] = this.ReportDataList;

                // Determine the date range
                string startDate = this.dtpStartDate.SelectedDate.Value.ToString();
                string endDate = this.dtpEndDate.SelectedDate.Value.ToString();

                // Show the report
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                    UIHelper.ReportTypes.WeeklyOvertimeReport.ToString(),
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_WEEKLY_OVERTIME_REPORT,
                    UIHelper.QUERY_STRING_STARTDATE_KEY,
                    startDate,
                    UIHelper.QUERY_STRING_ENDDATE_KEY,
                    endDate
                ),
                false);
            }
            else
                DisplayFormLevelError("No matching record was found in the database. Please modify the search criteria then try viewing the report again!");
            #endregion
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
                else if (this.ErrorType == ValidationErrorType.NoCostCenter)
                {
                    validator.ErrorMessage = "Please select a cost center from the list!";
                    validator.ToolTip = "Please select a cost center from the list!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoStartDate)
                {
                    validator.ErrorMessage = "Start Date is required.";
                    validator.ToolTip = "Start Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEndDate)
                {
                    validator.ErrorMessage = "End Date is required.";
                    validator.ToolTip = "End Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
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

        protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.lbCostCenter.Items.Count > 0)
            {
                if (chkSelectAll.Checked)
                {
                    foreach (RadListBoxItem item in this.lbCostCenter.Items)
                    {
                        item.Checked = true;
                    }
                }
                else
                {
                    this.lbCostCenter.ClearChecked();
                    this.lbCostCenter.ClearSelection();
                }
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.lbCostCenter.ClearSelection();
            this.lbCostCenter.ClearChecked();
            this.chkSelectAll.Checked = false;

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
            this.ReportDataList.Clear();
            this.CostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CallerForm"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void FillComboData(bool reloadFromDB = true)
        {
            FillCostCenterCombo(reloadFromDB);
        }

        private void RestoreDataFromCollection()
        {
            if (this.WeeklyOvertimeStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.WeeklyOvertimeStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.WeeklyOvertimeStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.WeeklyOvertimeStorage.ContainsKey("ReportDataList"))
                this.ReportDataList = this.WeeklyOvertimeStorage["ReportDataList"] as List<EmployeeAttendanceEntity>;
            else
                this.ReportDataList = null;

            if (this.WeeklyOvertimeStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.WeeklyOvertimeStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            FillComboData(false);
            #endregion

            #region Restore control values        
            if (this.WeeklyOvertimeStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.WeeklyOvertimeStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.WeeklyOvertimeStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.WeeklyOvertimeStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.WeeklyOvertimeStorage.ContainsKey("lbCostCenter"))
            {
                if (this.lbCostCenter.Items.Count > 0)
                {
                    dynamic selectedItems = this.WeeklyOvertimeStorage["lbCostCenter"];
                    //List<RadListBoxItem> selectedItems = this.WeeklyOvertimeStorage["lbCostCenter"] as List<RadListBoxItem>;
                    if (selectedItems != null &&
                        selectedItems.Count > 0)
                    {
                        foreach (var item in selectedItems)
                        {
                            RadListBoxItem foundItem = this.lbCostCenter.Items.Where(a => a.Value == item.Value).FirstOrDefault();
                            if (foundItem != null)
                                foundItem.Checked = true;
                                //this.lbCostCenter.Items[foundItem.Index].Checked = true;
                        }
                    }
                }
            }
            else
            {
                this.lbCostCenter.ClearChecked();
                this.lbCostCenter.ClearSelection();
            }

            if (this.WeeklyOvertimeStorage.ContainsKey("chkSelectAll"))
                this.chkSelectAll.Checked = UIHelper.ConvertObjectToBolean(this.WeeklyOvertimeStorage["chkSelectAll"]);
            else
                this.chkSelectAll.Checked = false;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.WeeklyOvertimeStorage.Clear();
            this.WeeklyOvertimeStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.WeeklyOvertimeStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.WeeklyOvertimeStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.WeeklyOvertimeStorage.Add("lbCostCenter", this.lbCostCenter.CheckedItems);
            this.WeeklyOvertimeStorage.Add("chkSelectAll", this.chkSelectAll.Checked);
            #endregion

            #region Save Query String values to collection
            this.WeeklyOvertimeStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.WeeklyOvertimeStorage.Add("ReportDataList", this.ReportDataList);
            this.WeeklyOvertimeStorage.Add("CostCenterList", this.CostCenterList);
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
        #endregion

        #region Database Access
        private void FillCostCenterCombo(bool reloadFromDB = true)
        {
            try
            {
                // Initialize listbox
                this.lbCostCenter.Items.Clear();

                List<CostCenterEntity> comboSource = new List<CostCenterEntity>();
                List<CostCenterEntity> filteredComboSource = new List<CostCenterEntity>();

                if (this.CostCenterList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.CostCenterList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetCostCenterList(1, ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        comboSource.AddRange(source.ToList());
                    }
                }

                #region Check for Allowed Cost Center
                if (this.AllowedCostCenterList.Count > 0)
                {
                    #region Filter list based on allowed cost center
                    foreach (string filter in this.AllowedCostCenterList)
                    {
                        foreach (CostCenterEntity item in comboSource)
                        {
                            if (item.CostCenter == filter)
                            {
                                filteredComboSource.Add(item);
                            }
                        }
                    }
                    #endregion
                }
                else if (this.AllowedCostCenterList.Count == 0 && UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty)
                {
                    #region Filter list based on user's cost center
                    this.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

                    foreach (string filter in this.AllowedCostCenterList)
                    {
                        foreach (CostCenterEntity item in comboSource)
                        {
                            if (item.CostCenter == filter)
                            {
                                filteredComboSource.Add(item);
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                // Store to session
                this.CostCenterList = filteredComboSource;

                #region Bind data to combobox
                this.lbCostCenter.DataSource = this.CostCenterList;
                this.lbCostCenter.DataTextField = "CostCenterFullName";
                this.lbCostCenter.DataValueField = "CostCenter";
                this.lbCostCenter.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void GetReportData(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;

                string costCenterList = string.Empty;
                StringBuilder sb = new StringBuilder();
                if (this.lbCostCenter.CheckedItems.Count > 0)
                {
                    foreach (var item in this.lbCostCenter.CheckedItems)
                    {
                        if (sb.Length == 0)
                            sb.Append(item.Value.Trim());
                        else
                            sb.Append(string.Format(",{0}", item.Value.Trim()));
                    }

                    costCenterList = sb.ToString().Trim();
                }
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> reportDataList = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    reportDataList = this.ReportDataList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetWeeklyOvertimeReportData(startDate, endDate, costCenterList, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(error, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (source != null && source.Count() > 0)
                        {
                            reportDataList.AddRange(source);
                        }
                    }
                }

                // Store collection to session
                this.ReportDataList = reportDataList;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion
    }
}