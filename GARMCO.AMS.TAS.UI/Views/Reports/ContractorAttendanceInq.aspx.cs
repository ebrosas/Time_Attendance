using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.DAL.Employee;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    public partial class ContractorAttendanceInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoDateRange,
            InvalidDateRange,
            InvalidYear,
            NoRecordToPrint
        }
        #endregion

        #region Properties
        public bool IsExcelDownload { get; set; }

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

        private Dictionary<string, object> ContractorAttendanceInqStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ContractorAttendanceInqStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ContractorAttendanceInqStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ContractorAttendanceInqStorage"] = value;
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

        private List<ContractorAttendance> ContractorAttendanceList
        {
            get
            {
                List<ContractorAttendance> list = ViewState["ContractorAttendanceList"] as List<ContractorAttendance>;
                if (list == null)
                    ViewState["ContractorAttendanceList"] = list = new List<ContractorAttendance>();

                return list;
            }
            set
            {
                ViewState["ContractorAttendanceList"] = value;
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

        private int CurrentTotalRecord
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentTotalRecord"]);
            }
            set
            {
                ViewState["CurrentTotalRecord"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTRATEND.ToString());

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
                FillCostCenterCombo(false);
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
                this.Master.FormTitle = UIHelper.PAGE_CONTRACTOR_ATTENDANCE_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_CONTRACTOR_ATTENDANCE_INQUIRY_TITLE), true);
                    }
                }
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ContractorAttendanceInqStorage.Count > 0)
                {
                    if (this.ContractorAttendanceInqStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtContractorNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("ContractorAttendanceInqStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString() ||
                    formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show last inquiry data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.ContractorAttendanceInqStorage.Clear();

                    // Refresh query string value
                    //this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);

                    // Check if need to invoke method to load data in the grid
                    //if (this.ReloadGridData)
                    //    this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    // Initialize controls
                    //this.dtpStartDate.SelectedDate = DateTime.Now.AddDays(-1);
                    //this.dtpEndDate.SelectedDate = DateTime.Now;
                    this.txtContractorName.Focus();

                    // Fill data to the grid
                    //this.btnSearch_Click(this.btnSearch, new EventArgs());
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

            //if (!this.IsExcelDownload)
            //{
            //    // Fill data to the grid
            //    GetContractorAttendance(true);
            //}
            //else
            //    this.IsExcelDownload = false;

            RebindDataToGrid();
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            //if (this.CurrentPageSize != e.NewPageSize)
            //{
            //    // Store page size to session
            //    this.CurrentPageSize = e.NewPageSize;
            //    this.CurrentPageIndex = 1;

            //    if (!this.IsExcelDownload)
            //    {
            //        // Fill data to the grid
            //        GetContractorAttendance(true);
            //    }
            //    else
            //        this.IsExcelDownload = false;
            //}

            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            RebindDataToGrid();
        }

        protected void gridSearchResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ContractorAttendanceList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.ContractorAttendanceList;
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
            if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
            {
                // Set the flag
                this.IsExcelDownload = true;

                this.gridSearchResults.AllowPaging = false;
                RebindDataToGrid();

                #region Initialize grid columns for export
                this.gridSearchResults.MasterTableView.GetColumn("SwipeIn").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("SwipeOut").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("SwipeInExcel").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("SwipeOutExcel").Visible = true;
                #endregion

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

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridPagerItem)
            {
                RadComboBox myPageSizeCombo = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
                if (myPageSizeCombo != null)
                {
                    // Clear default items
                    myPageSizeCombo.Items.Clear();

                    // Add new items
                    string[] arrayPageSize = { "10", "20", "50", "100", "200", "500", "1000" };
                    foreach (string item in arrayPageSize)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem(item);
                        cboItem.Attributes.Add("ownerTableViewId", gridSearchResults.MasterTableView.ClientID);

                        // Add to the grid combo
                        myPageSizeCombo.Items.Add(cboItem);
                    }

                    // Get the default size
                    RadComboBoxItem cboItemDefault = myPageSizeCombo.FindItemByText(e.Item.OwnerTableView.PageSize.ToString());
                    if (cboItemDefault != null)
                        cboItemDefault.Selected = true;
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.ContractorAttendanceList.Count > 0)
            {
                int totalRecords = this.ContractorAttendanceList.FirstOrDefault().TotalRecords;
                //if (totalRecords > 0)
                //    this.gridSearchResults.VirtualItemCount = totalRecords;
                //else
                //    this.gridSearchResults.VirtualItemCount = 1;

                this.gridSearchResults.DataSource = this.ContractorAttendanceList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<ContractorAttendance>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtContractorNo.Text = string.Empty;
            this.txtContractorName.Text = string.Empty;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;

            this.chkPayPeriod.Checked = true;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            // Cler collections
            this.ContractorAttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["CurrentTotalRecord"] = null;

            // Reset the grid
            //this.gridSearchResults.VirtualItemCount = 1;
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check date range
            if (this.dtpStartDate.SelectedDate == null &&
                this.dtpEndDate.SelectedDate == null)
            {
                //this.txtGeneric.Text = ValidationErrorType.NoDateRange.ToString();
                //this.ErrorType = ValidationErrorType.NoDateRange;
                //this.cusValStartDate.Validate();
                //errorCount++;
            }
            else
            {
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

            GetContractorAttendance(true);
        }
                
        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            #region Perform Validation                        
            int errorCount = 0;
            if (this.ContractorAttendanceList.Count == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoRecordToPrint.ToString();
                this.ErrorType = ValidationErrorType.NoRecordToPrint;
                this.cusValButton.Validate();
                errorCount++;
            }

            if (errorCount > 0)
                return;
            #endregion

            #region Display the report
            StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

            #region Display in the report data in the grid's current page 
            //Session["ContractorAttendanceReportSource"] = this.ContractorAttendanceList;

            //string dateFilter = string.Empty;
            //if (this.dtpStartDate.SelectedDate != null && this.dtpEndDate.SelectedDate != null)
            //{
            //    dateFilter = string.Format("From {0} to {1}",
            //        this.dtpStartDate.SelectedDate.Value.ToString("dd-MMM-yyyy"),
            //        this.dtpEndDate.SelectedDate.Value.ToString("dd-MMM-yyyy"));
            //}

            //Response.Redirect
            //(
            //    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}",
            //    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
            //    UIHelper.ReportTypes.ContractorAttendanceReport.ToString(),
            //    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
            //    UIHelper.PAGE_CONTRACTOR_ATTENDANCE_INQUIRY,
            //    "DateFilterString",
            //    dateFilter
            //),
            //false);
            #endregion

            #region Display in the report all data in the grid
            List<ContractorAttendance> reportSource = GetReportData();
            if (reportSource != null)
            {
                Session["ContractorAttendanceReportSource"] = reportSource;

                // Show the report
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}",
                    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                    UIHelper.ReportTypes.ContractorAttendanceReport.ToString(),
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_CONTRACTOR_ATTENDANCE_INQUIRY
                ),
                false);
            }
            else
                DisplayFormLevelError("Unable to view the report due to empty database record!");
            #endregion

            #endregion
        }

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            DateTime? startDate = this.dtpStartDate.SelectedDate;
            DateTime? endDate = this.dtpEndDate.SelectedDate;
            int contractorNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
            string contractorName = this.txtContractorName.Text.Trim();
            string costCenter = this.cboCostCenter.SelectedValue;

            #region Perform Data Validation
            // Check date range
            //if (startDate != null && endDate != null)
            //{
            //    if (startDate > endDate)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidDateRange;
            //        this.cusValStartDate.Validate();
            //        errorCount++;
            //    }
            //}

            if (this.ContractorAttendanceList.Count == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoRecordToPrint.ToString();
                this.ErrorType = ValidationErrorType.NoRecordToPrint;
                this.cusValButton.Validate();
                errorCount++;
            }

            if (errorCount > 0)
            {
                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }
            #endregion

            // Display the report in excel sheet file
            ExportAttendanceToExcel(startDate, endDate, contractorNo, contractorName, costCenter);
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
                else if (this.ErrorType == ValidationErrorType.NoDateRange)
                {
                    validator.ErrorMessage = "Start Date and End Date are required.";
                    validator.ToolTip = "Start Date and End Date are required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that the Effective Date is less than the Ending Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that the Effective Date is less than the Ending Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidYear)
                {
                    validator.ErrorMessage = "The specified payroll year should not be greater than the current year.";
                    validator.ToolTip = "The specified payroll year should not be greater than the current year.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToPrint)
                {
                    validator.ErrorMessage = "Unable to display the report because no record is found on the grid.";
                    validator.ToolTip = "Unable to display the report because no record is found on the grid.";
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
                //this.dtpStartDate.SelectedDate = null;
                //this.dtpEndDate.SelectedDate = null;

                this.cboMonth.SelectedIndex = -1;
                this.cboMonth.Text = string.Empty;
                this.txtYear.Text = string.Empty;
                this.dtpStartDate.Focus();
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

        protected void txtYear_TextChanged(object sender, EventArgs e)
        {
            this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtContractorNo.Text = string.Empty;
            this.txtContractorName.Text = string.Empty;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;

            this.chkPayPeriod.Checked = true;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            //this.gridSearchResults.VirtualItemCount = 1;
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
            this.ContractorAttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CurrentTotalRecord"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["CurrentContractorAttendance"] = null;
            Session.Remove("CurrentContractorAttendance");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ContractorAttendanceInqStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ContractorAttendanceInqStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ContractorAttendanceInqStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.ContractorAttendanceInqStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.ContractorAttendanceInqStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.ContractorAttendanceInqStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.ContractorAttendanceInqStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.ContractorAttendanceInqStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.ContractorAttendanceInqStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.ContractorAttendanceInqStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.ContractorAttendanceInqStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.ContractorAttendanceInqStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.ContractorAttendanceInqStorage.ContainsKey("ContractorAttendanceList"))
                this.ContractorAttendanceList = this.ContractorAttendanceInqStorage["ContractorAttendanceList"] as List<ContractorAttendance>;
            else
                this.ContractorAttendanceList = null;

            if (this.ContractorAttendanceInqStorage.ContainsKey("CurrentTotalRecord"))
                this.CurrentTotalRecord = UIHelper.ConvertObjectToInt(this.ContractorAttendanceInqStorage["CurrentTotalRecord"]);
            else
                this.CurrentTotalRecord = 0;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ContractorAttendanceInqStorage.ContainsKey("txtContractorNo"))
                this.txtContractorNo.Text = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["txtContractorNo"]);
            else
                this.txtContractorNo.Text = string.Empty;

            if (this.ContractorAttendanceInqStorage.ContainsKey("txtContractorName"))
                this.txtContractorName.Text = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["txtContractorName"]);
            else
                this.txtContractorName.Text = string.Empty;

            if (this.ContractorAttendanceInqStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.ContractorAttendanceInqStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ContractorAttendanceInqStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.ContractorAttendanceInqStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ContractorAttendanceInqStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.ContractorAttendanceInqStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.ContractorAttendanceInqStorage.ContainsKey("cboMonth"))
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorAttendanceInqStorage["cboMonth"]);
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.ContractorAttendanceInqStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.ContractorAttendanceInqStorage["chkPayPeriod"]);
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
            if (this.CurrentPageIndex > 0)
            {
                this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex - 1;
                this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex - 1;
            }
            else
            {
                this.gridSearchResults.CurrentPageIndex = 1;
                this.gridSearchResults.MasterTableView.CurrentPageIndex = 1;
            }

            //this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex;
            //this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ContractorAttendanceInqStorage.Clear();
            this.ContractorAttendanceInqStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ContractorAttendanceInqStorage.Add("txtContractorNo", this.txtContractorNo.Text.Trim());
            this.ContractorAttendanceInqStorage.Add("txtContractorName", this.txtContractorName.Text.Trim());
            this.ContractorAttendanceInqStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.ContractorAttendanceInqStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.ContractorAttendanceInqStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.ContractorAttendanceInqStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.ContractorAttendanceInqStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.ContractorAttendanceInqStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            #endregion

            #region Save Query String values to collection
            this.ContractorAttendanceInqStorage.Add("CallerForm", this.CallerForm);
            this.ContractorAttendanceInqStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.ContractorAttendanceInqStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.ContractorAttendanceInqStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.ContractorAttendanceInqStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.ContractorAttendanceInqStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.ContractorAttendanceInqStorage.Add("ContractorAttendanceList", this.ContractorAttendanceList);
            this.ContractorAttendanceInqStorage.Add("CurrentTotalRecord", this.CurrentTotalRecord);

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

        private void ExportAttendanceToExcel(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName, string costCenter)
        {
            int recordsProcessed = 0;
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Get attendance records from the database
                DALProxy proxy = new DALProxy();
                List<ContractorAttendanceExcel> exportDataList = new List<ContractorAttendanceExcel>();

                //var source = proxy.GetContractorAttendanceAll(startDate, endDate, contractorNo, contractorName, costCenter, ref error, ref innerError);
                var source = proxy.GetContractorAttendanceExcel(startDate, endDate, contractorNo, contractorName, costCenter, ref error, ref innerError);
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
                        exportDataList.AddRange(source);
                    }
                }
                #endregion

                if (exportDataList != null && exportDataList.Count > 0)
                {
                    DataTable dt = UIHelper.ContractorAttendanceToDataTable(exportDataList);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string fileName = string.Empty;

                        // Get the record count
                        recordsProcessed = dt.Rows.Count;

                        #region Set the Excel file name and path
                        string appPath = Environment.CurrentDirectory;
                        if (Environment.CurrentDirectory.IndexOf("\\bin") > -1)
                            appPath = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("\\bin"));

                        // Retrieve the folder where the files will be saved
                        string exportFolder = Server.MapPath(ConfigurationManager.AppSettings["DownloadPath"]);

                        if (startDate != null && endDate != null)
                        {
                            fileName = string.Format(@"{0}\ContractorAttendanceReport_{1}-{2}.xlsx",
                                exportFolder,
                                Convert.ToDateTime(startDate).ToString("ddMMMyy"),
                                Convert.ToDateTime(endDate).ToString("ddMMMyy"));
                        }
                        else
                        {
                            fileName = string.Format(@"{0}\ContractorAttendanceReport_{1}.xlsx",
                                exportFolder,
                                DateTime.Now.ToFileTime().ToString());
                        }
                        #endregion

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            #region Remove existing files
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            #endregion

                            #region Build the Excel Sheet file using EPPlus
                            string reportTitle = "Contractor Attendance Report";
                            string sheetTitle = "Raw data";
                            if (startDate != null && endDate != null)
                            {
                                sheetTitle = string.Format("From {0} to {1}",
                                    Convert.ToDateTime(startDate).ToString("ddMMMyyyy"),
                                    Convert.ToDateTime(endDate).ToString("ddMMMyyyy"));
                            }

                            CreateExcelSheet(dt, fileName, sheetTitle, reportTitle);
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        
        private void CreateExcelSheet(DataTable dt, string destination, string sheetTitle, string reportTitle)
        {
            try
            {
                FileInfo fi = new FileInfo(destination);

                using (ExcelPackage package = new ExcelPackage())
                {
                    // Add new worksheet to the report
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetTitle);

                    #region Set the title and format it
                    worksheet.Cells["A1"].Value = reportTitle;

                    using (ExcelRange range = worksheet.Cells["A1:R1"])
                    {
                        range.Merge = true;
                        range.Style.Font.SetFromFont(new System.Drawing.Font("Calibri", 20, FontStyle.Regular));
                        range.Style.Font.Color.SetColor(Color.AntiqueWhite);
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 36, 62));
                    }
                    #endregion

                    #region Format the Header row
                    using (ExcelRange range = worksheet.Cells["A3:R3"])
                    {
                        range.Style.Font.SetFromFont(new System.Drawing.Font("Calibri", 12, FontStyle.Bold));
                        range.Style.Font.Color.SetColor(Color.AntiqueWhite);
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                        range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));

                        #region Add column headings
                        int rowIndex = 3;
                        int colIndex = 1;

                        foreach (DataColumn column in dt.Columns)
                        {
                            string columnName = string.Empty;

                            switch (column.ColumnName)
                            {
                                case "EmpNo":
                                    columnName = "Contractor ID";
                                    break;

                                case "EmpName":
                                    columnName = "Contractor Name";
                                    break;

                                case "CPRNo":
                                    columnName = "CPR No.";
                                    break;

                                case "JobTitle":
                                    columnName = "Job Title";
                                    break;

                                case "EmployerName":
                                    columnName = "Employer Name";
                                    break;

                                case "CostCenter":
                                    columnName = "Cost Center";
                                    break;

                                case "CostCenterName":
                                    columnName = "Department";
                                    break;

                                case "ReaderName":
                                    columnName = "Gate";
                                    break;

                                case "SwipeDate":
                                    columnName = "Date";
                                    break;

                                case "SwipeIn":
                                    columnName = "In";
                                    break;

                                case "SwipeOut":
                                    columnName = "Out";
                                    break;

                                case "WorkHour":
                                    columnName = "Work Hour";
                                    break;

                                case "NetHour":
                                    columnName = "Net Hour";
                                    break;

                                case "OvertimeHour":
                                    columnName = "Overtime";
                                    break;

                                case "StatusDesc":
                                    columnName = "Status";
                                    break;

                                case "ContractStartDate":
                                    columnName = "Start Date";
                                    break;

                                case "ContractEndDate":
                                    columnName = "Expiry Date";
                                    break;

                                case "CreatedDate":
                                    columnName = "Printed Date";
                                    break;
                            }

                            worksheet.Cells[rowIndex, colIndex].Value = columnName;
                            colIndex++;
                        }
                        #endregion
                    }
                    #endregion

                    #region Load the data from the data table
                    worksheet.Cells["A4"].LoadFromDataTable(dt, false, OfficeOpenXml.Table.TableStyles.Medium9);

                    // Format the columns to have a better look
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    worksheet.Cells["A:A"].Style.Numberformat.Format = "0";             //EmpNo
                    worksheet.Cells["C:C"].Style.Numberformat.Format = "0";             //CPRNo
                    worksheet.Cells["I:I"].Style.Numberformat.Format = "dd-MMM-yyyy";   //SwipeDate
                    worksheet.Cells["J:J"].Style.Numberformat.Format = "HH:mm";         //SwipeIn
                    worksheet.Cells["K:K"].Style.Numberformat.Format = "HH:mm";         //SwipeOut
                    worksheet.Cells["L:L"].Style.Numberformat.Format = "0.00";          //WorkHour
                    worksheet.Cells["M:M"].Style.Numberformat.Format = "0.00";          //NetHour
                    worksheet.Cells["N:N"].Style.Numberformat.Format = "0.00";          //OvertimeHour
                    worksheet.Cells["P:P"].Style.Numberformat.Format = "dd-MMM-yyyy";   //ContractStartDate
                    worksheet.Cells["Q:Q"].Style.Numberformat.Format = "dd-MMM-yyyy";   //ContractEndDate
                    worksheet.Cells["R:R"].Style.Numberformat.Format = "dd-MMM-yyyy";   //CreatedDate

                    // Set cell text alignment
                    worksheet.Cells["A:R"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                    // Set the column width
                    worksheet.Column(9).Width = 15;     //SwipeDate
                    worksheet.Column(16).Width = 15;    //ContractStartDate
                    worksheet.Column(17).Width = 15;    //ContractEndDate
                    worksheet.Column(18).Width = 15;    //CreatedDate
                    
                    #endregion

                    // Save the output file
                    package.SaveAs(fi);

                    // Open the excel sheet file
                    string urlPath = string.Format("{0}?filename={1}&fileType={2}",
                        UIHelper.PAGE_FILE_HANDLER.Replace("~", string.Empty),
                        fi.Name,
                        UIHelper.CONST_EXCEL_FILE_TYPE);

                    string script = string.Format("DisplayAttachment('{0}');", urlPath);
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ShowAttachment", script.ToString(), true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Database Access
        private void GetContractorAttendance(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                int contractorNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
                string contractorName = this.txtContractorName.Text.Trim();
                string costCenter = this.cboCostCenter.SelectedValue;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                //this.gridSearchResults.VirtualItemCount = 1;

                // Reset record count
                this.CurrentTotalRecord = 0;
                #endregion

                #region Fill data to the collection
                List<ContractorAttendance> gridSource = new List<ContractorAttendance>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ContractorAttendanceList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetContractorAttendance(startDate, endDate, contractorNo, contractorName,  costCenter,  this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                this.ContractorAttendanceList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.ContractorAttendanceList.Count > 0)
                {
                    this.CurrentTotalRecord = this.ContractorAttendanceList.FirstOrDefault().TotalRecords;
                    //if (this.CurrentTotalRecord > 0)
                    //    this.gridSearchResults.VirtualItemCount = this.CurrentTotalRecord;
                    //else
                    //    this.gridSearchResults.VirtualItemCount = 1;

                    this.gridSearchResults.DataSource = this.ContractorAttendanceList;
                    this.gridSearchResults.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", this.CurrentTotalRecord.ToString("#,###"));
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

        private List<ContractorAttendance> GetReportData()
        {
            List<ContractorAttendance> reportData = null;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                int contractorNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
                string contractorName = this.txtContractorName.Text.Trim();
                string costCenter = this.cboCostCenter.SelectedValue;

                #region Get database records
                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetContractorAttendanceReport(startDate, endDate, contractorNo, contractorName, costCenter, 1, this.CurrentTotalRecord, ref error, ref innerError);
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
                        reportData = new List<ContractorAttendance>();
                        reportData.AddRange(rawData);
                    }
                }
                #endregion

                return reportData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillCostCenterCombo(bool filterByAllowedCC = true)
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

            if (filterByAllowedCC)
            {
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
            }
            else
            {
                #region No filtering for cost center
                foreach (DataRow rw in source)
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

                //Set the flag
                enableEmpSearch = true;
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