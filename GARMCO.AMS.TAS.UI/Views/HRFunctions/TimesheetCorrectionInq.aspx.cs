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
using GARMCO.Common.Object;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class TimesheetCorrectionInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            InvalidDateRange,
            NoDateFrom,
            NoSpecifiedEmpNo,
            NoEmpNo,
            InvalidYear
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

        private Dictionary<string, object> TimesheetTranHistoryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["TimesheetTranHistoryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["TimesheetTranHistoryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["TimesheetTranHistoryStorage"] = value;
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

        private List<EmployeeAttendanceEntity> DataList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["DataList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["DataList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["DataList"] = value;
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

        private bool ReloadGridData
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["ReloadGridData"]);
            }
            set
            {
                ViewState["ReloadGridData"] = value;
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

        private DateTime? PayPeriodStartDate
        {
            get
            {
                return UIHelper.ConvertObjectToDate(ViewState["PayPeriodStartDate"]);
            }
            set
            {
                ViewState["PayPeriodStartDate"] = value;
            }
        }

        private DateTime? PayPeriodEndDate
        {
            get
            {
                return UIHelper.ConvertObjectToDate(ViewState["PayPeriodEndDate"]);
            }
            set
            {
                ViewState["PayPeriodEndDate"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.TSCOREKINQ.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_TIMESHEET_CORRECTION_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_TIMESHEET_CORRECTION_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnNew.Enabled = this.Master.IsCreateAllowed;
                //this.btnDelete.Enabled = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.TimesheetTranHistoryStorage.Count > 0)
                {
                    if (this.TimesheetTranHistoryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);

                        // Save Employee No. to session
                        this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("TimesheetTranHistoryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("TimesheetTranHistoryStorage");

                    // Check if need to refresh data in the grid
                    if (this.ReloadGridData)
                    {
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                        this.ReloadGridData = false;
                    }
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    int month = DateTime.Now.Month;
                    int year = DateTime.Now.Year;
                    DateTime? startDate = null;
                    DateTime? endDate = null;
                    GetPayPeriod(year, month, ref startDate, ref endDate);

                    this.PayPeriodStartDate = startDate;
                    this.PayPeriodEndDate = endDate;

                    // Set the start and end date to the current date
                    this.dtpDateFrom.SelectedDate = DateTime.Now;
                    this.dtpDateTo.SelectedDate = DateTime.Now;

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

                    // Set focus to Employee no.
                    this.txtEmpNo.Focus();
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

            // Fill data to the grid
            GetTimesheetCorrection(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetTimesheetCorrection(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridSearchResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.DataList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.DataList;
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
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Open the Timesheet History form
                    dynamic itemObj = e.CommandSource;
                    string itemText = itemObj.Text;

                    // Get data key value
                    long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    int empNo = UIHelper.ConvertObjectToInt(item["EmpNo"].Text);
                    DateTime startDate = this.dtpDateFrom.SelectedDate.Value.Date;
                    DateTime endDate = this.dtpDateTo.SelectedDate.HasValue ? this.dtpDateTo.SelectedDate.Value.Date : this.dtpDateFrom.SelectedDate.Value.Date;

                    if (autoID > 0 && this.DataList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.DataList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null && autoID > 0)
                        {
                            #region Add employee information
                            selectedRecord.EmpName = (item["EmpName"].FindControl("litEmpName") as Literal).Text;
                            selectedRecord.Position = (item["Position"].FindControl("litPosition") as Literal).Text;
                            selectedRecord.CostCenterFullName = (item["CostCenterFullName"].FindControl("litCostCenterFullName") as Literal).Text;
                            #endregion

                            // Save to session
                            Session["SelectedTimesheetRecord"] = selectedRecord;
                        }
                    }

                    if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region View link is clicked
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                       (
                           String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY + "?{0}={1}&{2}={3}",
                           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                           UIHelper.PAGE_TIMESHEET_CORRECTION_INQUIRY,
                           UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                           Convert.ToInt32(UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord).ToString()
                       ),
                       false);
                        #endregion
                    }
                    else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region Edit link is clicked

                        // Check if Timesheet Processing is in progress
                        string error = string.Empty;
                        string innerError = string.Empty;
                        DALProxy proxy = new DALProxy();

                        bool isTimesheetProcess = proxy.CheckIfTimesheetProcessingInProgress(ref error, ref innerError);
                        if (isTimesheetProcess)
                        {
                            DisplayFormLevelError("Unable to perform correction because Timesheet Processing is still in progress. Please try again after few minutes!");
                        }
                        else
                        {
                            // Save session values
                            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                            Response.Redirect
                           (
                               String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                               UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                               UIHelper.PAGE_TIMESHEET_CORRECTION_INQUIRY,
                               UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                               Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString(),
                               UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                               autoID
                           ),
                           false);
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set column background color to red if CorrectionCode is not null
                    string correctionCode = UIHelper.ConvertObjectToString(item["CorrectionCode"].Text.Replace("&nbsp;", string.Empty));
                    if (!string.IsNullOrEmpty(correctionCode))
                    {
                        item["CorrectionCode"].BackColor = System.Drawing.Color.Red;
                        item["CorrectionCode"].ForeColor = System.Drawing.Color.White;
                        item["CorrectionCode"].Font.Bold = true;
                        item["CorrectionCode"].ToolTip = UIHelper.ConvertObjectToString(item["CorrectionDesc"].Text);
                    }
                    #endregion

                    #region Set row background color to green for backdated dates
                    DateTime? attendanceDate = UIHelper.ConvertObjectToDate(item["DT"].Text);

                    if (attendanceDate.HasValue && 
                        this.PayPeriodStartDate.HasValue && 
                        attendanceDate < this.PayPeriodStartDate &&
                        !string.IsNullOrEmpty(correctionCode))
                    {
                        item.BackColor = System.Drawing.Color.LightGreen;
                    }
                    #endregion

                    #region Set font color for all death related timesheet correction codes
                    correctionCode = UIHelper.ConvertObjectToString(item["CorrectionCode"].Text.Replace("&nbsp;", string.Empty));
                    if (correctionCode == "RAD0" ||
                        correctionCode == "RAD1" ||
                        correctionCode == "RAD2" ||
                        correctionCode == "RAD3" ||
                        correctionCode == "RAD4")
                    {
                        item["CorrectionDesc"].ForeColor = System.Drawing.Color.Red;
                        item["CorrectionCode"].ToolTip = UIHelper.ConvertObjectToString(item["CorrectionCodeDesc"].Text);
                    }
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.DataList.Count > 0)
            {
                int totalRecords = this.DataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridSearchResults.VirtualItemCount = totalRecords;
                else
                    this.gridSearchResults.VirtualItemCount = 1;

                this.gridSearchResults.DataSource = this.DataList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
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
            this.txtEmpNo.Text = string.Empty;
            this.txtYear.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.dtpDateFrom.SelectedDate = null;
            this.dtpDateTo.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;

            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            // Set focus to Employee no.
            this.txtEmpNo.Focus();

            // Cler collections
            this.DataList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Reset the grid
            this.gridSearchResults.VirtualItemCount = 1;
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
            #endregion
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            #region Check selected employee 
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            if (empNo.ToString().Length == 4)
            {
                empNo += 10000000;

                // Display the formatted Emp. No.
                this.txtEmpNo.Text = empNo.ToString();

                if (this.EmployeeNo == 0)
                    this.btnGet_Click(this.btnGet, new EventArgs());
            }

            //if (empNo == 0)
            //{
            //    this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
            //    this.ErrorType = ValidationErrorType.NoEmpNo;
            //    this.cusValEmpNo.Validate();
            //    errorCount++;

            //    this.litEmpName.Text = "Not defined";
            //    this.litPosition.Text = "Not defined";
            //}
            #endregion

            #region Check date range
            if (this.dtpDateFrom.SelectedDate != null &&
                this.dtpDateTo.SelectedDate != null)
            {
                if (this.dtpDateFrom.SelectedDate > this.dtpDateTo.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateRange;
                    this.cusValDateFrom.Validate();
                    errorCount++;
                }
            }
            else
            {
                if (this.dtpDateFrom.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDateFrom.ToString();
                    this.ErrorType = ValidationErrorType.NoDateFrom;
                    this.cusValDateFrom.Validate();
                    errorCount++;
                }
            }
            #endregion

            if (errorCount > 0)
            {
                InitializeDataToGrid();

                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }

            #endregion

            if (!this.ReloadGridData)
            {
                // Reset page index
                this.gridSearchResults.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridSearchResults.PageSize;
            }

            GetTimesheetCorrection(true);

            // Set focus to Date From
            this.dtpDateFrom.Focus();
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

                #region Initialize control values and variables
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
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
                        //if (this.Master.AllowedCostCenterList.Count > 0)
                        //{
                        //    string allowedCC = this.Master.AllowedCostCenterList
                        //        .Where(a => a == UIHelper.ConvertObjectToString(empInfo.CostCenter))
                        //        .FirstOrDefault();
                        //    if (!string.IsNullOrEmpty(allowedCC))
                        //    {
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
                        //    }
                        //    else
                        //    {
                        //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                        //    }
                        //}

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
                            //if (this.Master.AllowedCostCenterList.Count > 0)
                            //{
                            //    string allowedCC = this.Master.AllowedCostCenterList
                            //        .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
                            //        .FirstOrDefault();
                            //    if (!string.IsNullOrEmpty(allowedCC))
                            //    {
                            this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                            this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);

                            //    }
                            //    else
                            //    {
                            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                            //    }
                            //}

                            // Save Employee No. to session
                            this.EmployeeNo = empNo;
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_TIMESHEET_CORRECTION_INQUIRY
            ),
            false);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
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
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedEmpNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateFrom)
                {
                    validator.ErrorMessage = "Date Duration is required.";
                    validator.ToolTip = "Date Duration is required.";
                    args.IsValid = false;
                }                               
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that the start date is less than the end date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that the start date is less than the end date.";
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
            this.dtpDateFrom.SelectedDate = this.dtpDateTo.SelectedDate = null;

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

            this.dtpDateFrom.SelectedDate = startDate;
            this.dtpDateTo.SelectedDate = endDate;

            #region Fill data in the grid
            //if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) > 0 &&
            //    this.dtpDateFrom.SelectedDate != null &&
            //    this.dtpDateTo.SelectedDate != null)
            //{
            //    this.btnSearch_Click(this.btnSearch, new EventArgs());
            //}
            //else
            //{
            //    this.litEmpName.Text = "Not defined";
            //    this.litPosition.Text = "Not defined";
            //    InitializeDataToGrid();
            //}
            #endregion
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
                this.dtpDateFrom.SelectedDate = null;
                this.dtpDateTo.SelectedDate = null;
                this.dtpDateFrom.Focus();
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
            this.txtEmpNo.Text = string.Empty;
            this.txtYear.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
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

            // Reset the grid
            this.gridSearchResults.VirtualItemCount = 1;
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
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.DataList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["EmployeeNo"] = null;
            ViewState["CallerForm"] = null;
            ViewState["PayPeriodStartDate"] = null;
            ViewState["PayPeriodEndDate"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["SelectedTimesheetRecord"] = null;
            Session.Remove("SelectedTimesheetRecord");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.TimesheetTranHistoryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.TimesheetTranHistoryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.TimesheetTranHistoryStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.TimesheetTranHistoryStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.TimesheetTranHistoryStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.TimesheetTranHistoryStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.TimesheetTranHistoryStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.TimesheetTranHistoryStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.TimesheetTranHistoryStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.TimesheetTranHistoryStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.TimesheetTranHistoryStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.TimesheetTranHistoryStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.TimesheetTranHistoryStorage.ContainsKey("DataList"))
                this.DataList = this.TimesheetTranHistoryStorage["DataList"] as List<EmployeeAttendanceEntity>;
            else
                this.DataList = null;

            if (this.TimesheetTranHistoryStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.TimesheetTranHistoryStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            if (this.TimesheetTranHistoryStorage.ContainsKey("PayPeriodStartDate"))
                this.PayPeriodStartDate = UIHelper.ConvertObjectToDate(this.TimesheetTranHistoryStorage["PayPeriodStartDate"]);
            else
                this.PayPeriodStartDate = null;

            if (this.TimesheetTranHistoryStorage.ContainsKey("PayPeriodEndDate"))
                this.PayPeriodEndDate = UIHelper.ConvertObjectToDate(this.TimesheetTranHistoryStorage["PayPeriodEndDate"]);
            else
                this.PayPeriodEndDate = null;

            FillComboData(false);
            #endregion

            #region Restore control values     
            if (this.TimesheetTranHistoryStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.TimesheetTranHistoryStorage["chkPayPeriod"]);
            else
                this.chkPayPeriod.Checked = false;

            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            if (this.TimesheetTranHistoryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.TimesheetTranHistoryStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.TimesheetTranHistoryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.TimesheetTranHistoryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.TimesheetTranHistoryStorage.ContainsKey("cboMonth"))
            {
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["cboMonth"]);
                //this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs
                //    (this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
            }
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.TimesheetTranHistoryStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetTranHistoryStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.TimesheetTranHistoryStorage.ContainsKey("dtpDateFrom"))
                this.dtpDateFrom.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetTranHistoryStorage["dtpDateFrom"]);
            else
                this.dtpDateFrom.SelectedDate = null;

            if (this.TimesheetTranHistoryStorage.ContainsKey("dtpDateTo"))
                this.dtpDateTo.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetTranHistoryStorage["dtpDateTo"]);
            else
                this.dtpDateTo.SelectedDate = null;
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex > 0 ? this.CurrentPageIndex - 1 : 0;
            this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex > 0 ? this.CurrentPageIndex - 1 : 0;
            this.gridSearchResults.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.TimesheetTranHistoryStorage.Clear();
            this.TimesheetTranHistoryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.TimesheetTranHistoryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.TimesheetTranHistoryStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.TimesheetTranHistoryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.TimesheetTranHistoryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.TimesheetTranHistoryStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.TimesheetTranHistoryStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.TimesheetTranHistoryStorage.Add("dtpDateFrom", this.dtpDateFrom.SelectedDate);
            this.TimesheetTranHistoryStorage.Add("dtpDateTo", this.dtpDateTo.SelectedDate);
            this.TimesheetTranHistoryStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            #endregion

            #region Save Query String values to collection
            this.TimesheetTranHistoryStorage.Add("CallerForm", this.CallerForm);
            this.TimesheetTranHistoryStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.TimesheetTranHistoryStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.TimesheetTranHistoryStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.TimesheetTranHistoryStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.TimesheetTranHistoryStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.TimesheetTranHistoryStorage.Add("DataList", this.DataList);
            this.TimesheetTranHistoryStorage.Add("EmployeeNo", this.EmployeeNo);
            this.TimesheetTranHistoryStorage.Add("PayPeriodStartDate", this.PayPeriodStartDate);
            this.TimesheetTranHistoryStorage.Add("PayPeriodEndDate", this.PayPeriodEndDate);
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

        private void GetTimesheetCorrection(bool reloadDataFromDB = false)
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

                    if (this.EmployeeNo == 0)
                        this.btnGet_Click(this.btnGet, new EventArgs());
                }

                string costCenter = this.cboCostCenter.SelectedValue;
                DateTime? startDate = this.dtpDateFrom.SelectedDate;
                DateTime? endDate = this.dtpDateTo.SelectedDate;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridSearchResults.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.DataList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetCorrection(costCenter, empNo, startDate, endDate, 0, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                this.DataList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.DataList.Count > 0)
                {
                    int totalRecords = this.DataList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridSearchResults.VirtualItemCount = totalRecords;
                    else
                        this.gridSearchResults.VirtualItemCount = 1;

                    this.gridSearchResults.DataSource = this.DataList;
                    this.gridSearchResults.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeDataToGrid();
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