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

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class AspirePayrollInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoStartDate,
            NoEndDate,
            InvalidYear,
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

        private Dictionary<string, object> AspireReportStorage
        {
            get
            {
                Dictionary<string, object> list = Session["AspireReportStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["AspireReportStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["AspireReportStorage"] = value;
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

        private int CurrentStartRowIndex
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentStartRowIndex"]);
            }
            set
            {
                ViewState["CurrentStartRowIndex"] = value;
            }
        }

        private int CurrentMaximumRows
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentMaximumRows"]);
            }
            set
            {
                ViewState["CurrentMaximumRows"] = value;
            }
        }

        private int CurrentPageIndex
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentPageIndex"]);
            }
            set
            {
                ViewState["CurrentPageIndex"] = value;
            }
        }

        private int CurrentPageSize
        {
            get
            {
                int pageSize = UIHelper.ConvertObjectToInt(ViewState["CurrentPageSize"]);
                if (pageSize == 0)
                    pageSize = this.gridSearchResults.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.ASPIREREPT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_ASPIRE_EMPLOYEES_PAYROLL_REPORT_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_ASPIRE_EMPLOYEES_PAYROLL_REPORT_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnShowReport.Enabled = this.Master.IsPrintAllowed;
                this.btnSearch.Enabled = this.Master.IsRetrieveAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.AspireReportStorage.Count > 0)
                {
                    if (this.AspireReportStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.AspireReportStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("AspireReportStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    this.rblStatus.SelectedValue = "valAll";
                    this.chkPayPeriod.Checked = true;
                    this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());
                    #endregion

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridSearchResults_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            RebindDataToGrid();
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            RebindDataToGrid();
        }

        protected void gridSearchResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.AttendanceList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.AttendanceList;
                this.gridSearchResults.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridSearchResults.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridSearchResults.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridSearchResults_ItemCommand(object sender, GridCommandEventArgs e)
        {
            try
            {
                if (e.CommandName.Equals(RadGrid.SelectCommandName))
                {
                    #region Process View link
                    //GridDataItem item = e.Item as GridDataItem;
                    //if (item != null)
                    //{
                    //    dynamic itemObj = e.CommandSource;
                    //    string itemText = itemObj.Text;

                    //    // Get data key value
                    //    long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    //    if (autoID > 0 && this.AttendanceList.Count > 0)
                    //    {
                    //        EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                    //            .Where(a => a.AutoID == autoID)
                    //            .FirstOrDefault();
                    //        if (selectedRecord != null && autoID > 0)
                    //        {
                    //            // Save to session
                    //            Session["SelectedEmpShiftPattern"] = selectedRecord;
                    //        }
                    //    }

                    //    if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    //    {
                    //        #region View link is clicked
                    //        // Save session values
                    //        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //        Response.Redirect
                    //       (
                    //           String.Format(UIHelper.PAGE_CURRENT_SHIFT_PATTERN_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //           UIHelper.PAGE_EMPLOYEE_DIRECTORY,
                    //           UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //           autoID,
                    //           UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //           Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString()
                    //       ),
                    //       false);
                    //        #endregion
                    //    }
                    //}
                    #endregion
                }
                else if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                    e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                    e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                    e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
                {
                    this.gridSearchResults.AllowPaging = false;
                    RebindDataToGrid();

                    this.gridSearchResults.ExportSettings.Excel.Format = GridExcelExportFormat.Biff;
                    this.gridSearchResults.ExportSettings.IgnorePaging = true;
                    this.gridSearchResults.ExportSettings.ExportOnlyData = true;
                    this.gridSearchResults.ExportSettings.OpenInNewWindow = true;
                    this.gridSearchResults.ExportSettings.UseItemStyles = true;

                    this.gridSearchResults.AllowPaging = true;
                    this.gridSearchResults.Rebind();
                }
                else if (e.CommandName.Equals(RadGrid.RebindGridCommandName))
                {
                    RebindDataToGrid();
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.AttendanceList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.AttendanceList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.AttendanceList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.rblStatus.SelectedValue = "valAll";
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.chkPayPeriod.Checked = true;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            // Cler collections
            this.AttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Reset the grid
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
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

            // Reset page index
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            GetAspireReportData(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_EMPLOYEE_DIRECTORY
            ),
            false);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                this.gridSearchResults.ExportSettings.Excel.Format = (GridExcelExportFormat)Enum.Parse(typeof(GridExcelExportFormat), "Xlsx");
                this.gridSearchResults.ExportSettings.IgnorePaging = true;
                this.gridSearchResults.ExportSettings.ExportOnlyData = true;
                this.gridSearchResults.ExportSettings.OpenInNewWindow = true;
                this.gridSearchResults.MasterTableView.ExportToExcel();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            if (this.AttendanceList.Count > 0)
            {
                StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                // Save report data to session
                Session["AspirePayrollReportSource"] = this.AttendanceList;

                // Determine the date range
                string startDate = this.dtpStartDate.SelectedDate.Value.ToString();
                string endDate = this.dtpEndDate.SelectedDate.Value.ToString();

                // Show the report
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                    UIHelper.ReportTypes.AspirePayrollReport.ToString(),
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_ASPIRE_PAYROLL_REPORT,
                    UIHelper.QUERY_STRING_STARTDATE_KEY,
                    startDate,
                    UIHelper.QUERY_STRING_ENDDATE_KEY,
                    endDate
                ),
                false);
            }
            else
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

                GetAspireReportData(true);

                #region Show the report
                if (this.AttendanceList != null &&
                    this.AttendanceList.Count > 0)
                {
                    StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                    // Save report data to session
                    Session["AspirePayrollReportSource"] = this.AttendanceList;

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
                        UIHelper.PAGE_ASPIRE_PAYROLL_REPORT,
                        UIHelper.QUERY_STRING_STARTDATE_KEY,
                        startDate,
                        UIHelper.QUERY_STRING_ENDDATE_KEY,
                        endDate
                    ),
                    false);
                }
                else
                    DisplayFormLevelError("No matching record was found in the database. Please modify the search criteria then try to view the report again!");
                #endregion
            }
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
                else if (this.ErrorType == ValidationErrorType.InvalidYear)
                {
                    validator.ErrorMessage = "The specified payroll year should not be greater than the current year.";
                    validator.ToolTip = "The specified payroll year should not be greater than the current year.";
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

        protected void cboMonth_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpStartDate.SelectedDate = this.dtpEndDate.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYear.Text == string.Empty)
            {
                this.txtYear.Text = DateTime.Now.Year.ToString();
            }
            //else
            //{
            //    // Check if greater than current year
            //    if (this.txtYear.Value > DateTime.Today.Year)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidYear.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidYear;
            //        this.cusValPayrollYear.Validate();
            //        this.txtYear.Focus();
            //        return;
            //    }
            //}

            int month = UIHelper.ConvertObjectToInt(this.cboMonth.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYear.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpStartDate.SelectedDate = startDate;
            this.dtpEndDate.SelectedDate = endDate;
        }

        protected void chkPayPeriod_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriod.Checked)
            {
                this.cboMonth.Enabled = true;
                this.txtYear.Enabled = true;
                this.dtpStartDate.Enabled = false;
                this.dtpEndDate.Enabled = false;

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
                this.dtpStartDate.Enabled = true;
                this.dtpEndDate.Enabled = true;

                this.cboMonth.SelectedIndex = -1;
                this.cboMonth.Text = string.Empty;
                this.txtYear.Text = string.Empty;
                this.dtpStartDate.Focus();
            }
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
            this.rblStatus.ClearSelection();
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.chkPayPeriod.Checked = true;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
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
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
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

        private void RestoreDataFromCollection()
        {
            if (this.AspireReportStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.AspireReportStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.AspireReportStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.AspireReportStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.AspireReportStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.AspireReportStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.AspireReportStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.AspireReportStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.AspireReportStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.AspireReportStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.AspireReportStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.AspireReportStorage.ContainsKey("AttendanceList"))
                this.AttendanceList = this.AspireReportStorage["AttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceList = null;

            FillComboData(false);
            #endregion

            #region Restore control values        
            if (this.AspireReportStorage.ContainsKey("rblStatus"))
                this.rblStatus.SelectedValue = UIHelper.ConvertObjectToString(this.AspireReportStorage["rblStatus"]);
            else
                this.rblStatus.ClearSelection();

            if (this.AspireReportStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.AspireReportStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.AspireReportStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.AspireReportStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.AspireReportStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.AspireReportStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.AspireReportStorage.ContainsKey("cboMonth"))
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.AspireReportStorage["cboMonth"]);
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.AspireReportStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.AspireReportStorage["chkPayPeriod"]);
            else
                this.chkPayPeriod.Checked = false;

            if (this.chkPayPeriod.Checked)
            {
                this.cboMonth.Enabled = true;
                this.txtYear.Enabled = true;
                this.dtpStartDate.Enabled = false;
                this.dtpEndDate.Enabled = false;
            }
            else
            {
                this.cboMonth.Enabled = false;
                this.txtYear.Enabled = false;
                this.dtpStartDate.Enabled = true;
                this.dtpEndDate.Enabled = true;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.AspireReportStorage.Clear();
            this.AspireReportStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session            
            this.AspireReportStorage.Add("rblStatus", this.rblStatus.SelectedValue);
            this.AspireReportStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.AspireReportStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.AspireReportStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.AspireReportStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.AspireReportStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            #endregion

            #region Save Query String values to collection
            this.AspireReportStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.AspireReportStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.AspireReportStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.AspireReportStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.AspireReportStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.AspireReportStorage.Add("AttendanceList", this.AttendanceList);
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
        private void GetAspireReportData(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;

                int processType = 0;
                if (this.rblStatus.SelectedValue == "valNotYetProcessed")
                    processType = 1;
                else if (this.rblStatus.SelectedValue == "valProcessed")
                    processType = 2;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
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
                    var source = proxy.GetAspirePayrolReport(startDate, endDate, processType, ref error, ref innerError);
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

                // Bind data to the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        #endregion
               
    }
}