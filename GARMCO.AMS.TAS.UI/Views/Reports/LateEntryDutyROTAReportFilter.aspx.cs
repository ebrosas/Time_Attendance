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
    public partial class LateEntryDutyROTAReportFilter : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
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

        private Dictionary<string, object> LateDutyRotaReportSource
        {
            get
            {
                Dictionary<string, object> list = Session["LateDutyRotaReportSource"] as Dictionary<string, object>;
                if (list == null)
                    Session["LateDutyRotaReportSource"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["LateDutyRotaReportSource"] = value;
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

        private List<EmployeeAttendanceEntity> AttendanceList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["AttendanceList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["AttendanceList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["AttendanceList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.LATEDUROTA.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_LATE_DUTY_ROTA_REPORT_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_LATE_DUTY_ROTA_REPORT_TITLE), true);
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
                if (this.LateDutyRotaReportSource.Count > 0)
                {
                    if (this.LateDutyRotaReportSource.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.LateDutyRotaReportSource["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("LateDutyRotaReportSource");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.LateDutyRotaReportSource.Clear();
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
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
            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;

            // Cler collections
            this.AttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            #endregion
        }

        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

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
                if (this.dtpStartDate.SelectedDate != null &&
                    this.dtpEndDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEndDate.ToString();
                    this.ErrorType = ValidationErrorType.NoEndDate;
                    this.cusValEndDate.Validate();
                    errorCount++;
                }
                else if (this.dtpStartDate.SelectedDate == null &&
                   this.dtpEndDate.SelectedDate != null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoStartDate.ToString();
                    this.ErrorType = ValidationErrorType.NoStartDate;
                    this.cusValStartDate.Validate();
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

            #region Display in the report all data in the grid
            if (this.AttendanceList != null &&
                this.AttendanceList.Count > 0)
            {
                StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                // Save report data to session
                Session["LateDutyRotaReportSource"] = this.AttendanceList;

                // Determine the date range
                string startDate = this.dtpStartDate.SelectedDate.HasValue ? this.dtpStartDate.SelectedDate.Value.ToString() : string.Empty;
                string endDate = this.dtpEndDate.SelectedDate.HasValue ? this.dtpEndDate.SelectedDate.Value.ToString() : string.Empty;

                // Show the report
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                    UIHelper.ReportTypes.LateDutyRotaReport.ToString(),
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_LATE_DUTY_ROTA_REPORT,
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

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_LATE_DUTY_ROTA_REPORT
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
                else if (this.ErrorType == ValidationErrorType.NoStartDate)
                {
                    validator.ErrorMessage = "Start Date is required if End Date is specified.";
                    validator.ToolTip = "Start Date is required if End Date is specified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEndDate)
                {
                    validator.ErrorMessage = "End Date is required if Start Date is specified.";
                    validator.ToolTip = "End Date is required if Start Date is specified.";
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
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
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
            this.AttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CallerForm"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.LateDutyRotaReportSource.Count == 0)
                return;

            #region Restore Query String values
            if (this.LateDutyRotaReportSource.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.LateDutyRotaReportSource["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.LateDutyRotaReportSource.ContainsKey("AttendanceList"))
                this.AttendanceList = this.LateDutyRotaReportSource["AttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceList = null;
            
            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.LateDutyRotaReportSource.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.LateDutyRotaReportSource["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.LateDutyRotaReportSource.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.LateDutyRotaReportSource["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.LateDutyRotaReportSource.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.LateDutyRotaReportSource["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.LateDutyRotaReportSource.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.LateDutyRotaReportSource["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.LateDutyRotaReportSource.Clear();
            this.LateDutyRotaReportSource.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.LateDutyRotaReportSource.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.LateDutyRotaReportSource.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.LateDutyRotaReportSource.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.LateDutyRotaReportSource.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.LateDutyRotaReportSource.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.LateDutyRotaReportSource.Add("AttendanceList", this.AttendanceList);
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
            
        }
        #endregion

        #region Database Access
        private void GetReportData(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                string costCenter = this.cboCostCenter.SelectedValue;
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.AttendanceList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetDILDueToLateEntryOfDutyROTA(startDate, endDate, costCenter, empNo, ref error, ref innerError);
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
                            gridSource.AddRange(source);
                        }
                    }
                }

                // Store collection to session
                this.AttendanceList = gridSource;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
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
            bool enableEmpSearch = false;

            #region Add default selection item
            EmployeeDAL.CostCenterRow defaultRow = filteredDT.NewCostCenterRow();
            defaultRow.CostCenter = String.Empty;
            defaultRow.CostCenterName = "Please select a Cost Center...";
            defaultRow.Company = String.Empty;
            defaultRow.SuperintendentNo = 0;
            defaultRow.SuperintendentName = String.Empty;
            defaultRow.ManagerNo = 0;
            defaultRow.ManagerName = String.Empty;

            // Add record to the collection
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

                        // Add record to the collection
                        filteredDT.Rows.Add(row);
                    }
                }

                // Set the flag
                enableEmpSearch = true;
                #endregion
            }
            else if (this.AllowedCostCenterList.Count == 0 && UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty)
            {
                #region Filter list based on user's cost center
                this.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

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

                        // Add record to the collection
                        filteredDT.Rows.Add(row);
                    }
                }

                //// Set the flag
                enableEmpSearch = true;
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

                //    // Add record to the collection
                //    filteredDT.Rows.Add(row);
                //}

                ////Set the flag
                //enableEmpSearch = true;
                #endregion
            }

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();
            }

            // Enable/Disable employee search button 
            this.btnFindEmployee.Enabled = enableEmpSearch;
        }
        #endregion                
    }
}