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
using System.Net.Mail;
using GARMCO.Common.Object;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class OvertimeMealVoucherEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoStartDateUnprocessed,
            NoStartDateAllRecords,
            NoDataFilterOption,
            InvalidDateRange,
            InvalidYear,
            OngoingPayrollProcess
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

        private Dictionary<string, object> OvertimeEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["OvertimeEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["OvertimeEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["OvertimeEntryStorage"] = value;
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

        private List<UDCEntity> OTReasonList
        {
            get
            {
                List<UDCEntity> list = ViewState["OTReasonList"] as List<UDCEntity>;
                if (list == null)
                    ViewState["OTReasonList"] = list = new List<UDCEntity>();

                return list;
            }
            set
            {
                ViewState["OTReasonList"] = value;
            }
        }

        private EmployeeAttendanceEntity SelectedOvertimeRecord
        {
            get
            {
                return ViewState["SelectedOvertimeRecord"] as EmployeeAttendanceEntity;
            }
            set
            {
                ViewState["SelectedOvertimeRecord"] = value;
            }
        }

        private bool IsOTApprove
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTApprove"]);
            }
            set
            {
                ViewState["IsOTApprove"] = value;
            }
        }

        private bool IsOTApprovalHeaderClicked
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTApprovalHeaderClicked"]);
            }
            set
            {
                ViewState["IsOTApprovalHeaderClicked"] = value;
            }
        }

        private List<UserDefinedCodes> OvertimeFilterOptionList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["OvertimeFilterOptionList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["OvertimeFilterOptionList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["OvertimeFilterOptionList"] = value;
            }
        }

        private int EmpNoParam
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["EmpNoParam"]);
            }
            set
            {
                ViewState["EmpNoParam"] = value;
            }
        }

        private DateTime? StartDateParam
        {
            get
            {
                return UIHelper.ConvertObjectToDate(ViewState["StartDateParam"]);
            }
            set
            {
                ViewState["StartDateParam"] = value;
            }
        }

        private string DisplayOptionParam
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["DisplayOptionParam"]);
            }
            set
            {
                ViewState["DisplayOptionParam"] = value;
            }
        }

        private bool IsLoadRequest
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsLoadRequest"]);
            }
            set
            {
                ViewState["IsLoadRequest"] = value;
            }
        }

        private long OTRequestNoParam
        {
            get
            {
                return UIHelper.ConvertObjectToLong(ViewState["OTRequestNoParam"]);
            }
            set
            {
                ViewState["OTRequestNoParam"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.OTENTRY.ToString());

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
                //FillCostCenterCombo();
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
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_OVERTIME_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    // Check is system is opened from the email
                    bool isLoadRequest = UIHelper.ConvertObjectToBolean(Request.QueryString["IsLoadRequest"]);
                    if (!isLoadRequest)
                    {
                        if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                        {
                            Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMPLOYEE_OVERTIME_ENTRY_TITLE), true);
                        }
                    }
                }
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.OvertimeEntryStorage.Count > 0)
                {
                    if (this.OvertimeEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["FormFlag"]);
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
                    Session.Remove("OvertimeEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("OvertimeEntryStorage");

                    // Check if need to refresh data in the grid
                    if (this.ReloadGridData)
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    if (this.IsLoadRequest)
                    {
                        #region Set control based on the value passed from the query string
                        if (this.EmpNoParam > 0)
                            this.txtEmpNo.Value = this.EmpNoParam;

                        if (this.StartDateParam.HasValue)
                            this.dtpStartDate.SelectedDate = this.StartDateParam;

                        if (!string.IsNullOrEmpty(this.DisplayOptionParam))
                            this.cboFilterOption.SelectedValue = this.DisplayOptionParam;
                        #endregion
                    }
                    else
                    {
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
                    }
                    #endregion

                    // Populate data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Visible = this.Master.IsEditAllowed;
                #endregion
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
            GetOvertimeRecord(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetOvertimeRecord(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
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
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    if (e.CommandSource.GetType() == typeof(ImageButton))
                    {
                        if (UIHelper.ConvertObjectToString(e.CommandArgument) == "CancelButton")
                        {
                            #region Cancel button is clicked
                            // Get the data key value
                            int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                            // Save current selected datagrid row
                            if (autoID > 0 && 
                                this.AttendanceList.Count > 0)
                            {
                                EmployeeAttendanceEntity selectedOTRecord = this.AttendanceList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedOTRecord != null)
                                {
                                    // Save the currently selected record
                                    this.SelectedOvertimeRecord = selectedOTRecord;

                                    // Display confirmation message
                                    StringBuilder script = new StringBuilder();
                                    script.Append("ConfirmButtonAction('");
                                    script.Append(string.Concat(this.btnCancelDummy.ClientID, "','"));
                                    script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                                    script.Append(UIHelper.CONST_CANCEL_OVERTIME_CONFIRMATION + "');");
                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Delete Confirmation", script.ToString(), true);
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        dynamic itemObj = e.CommandSource;
                        string itemText = itemObj.Text;

                        if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewHistoryLinkButton"].Controls[0] as LinkButton).Text.Trim())
                        {
                            #region View History 
                            // Save session values
                            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                            // Get the data key value
                            int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));

                            if (autoID > 0 &&
                                this.AttendanceList.Count > 0)
                            {
                                EmployeeAttendanceEntity selectedOTRecord = this.AttendanceList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedOTRecord != null)
                                {
                                    // Save the currently selected record
                                    Session["CurrentOvertimeRequest"] = selectedOTRecord;

                                    Response.Redirect
                                    (
                                        String.Format(UIHelper.PAGE_OVERTIME_APPROVAL_HISTORY + "?{0}={1}&IsLoadRequest={2}",
                                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                                        UIHelper.PAGE_OVERTIME_ENTRY,
                                        this.IsLoadRequest.ToString()
                                    ),
                                    false);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            else if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
            {
                #region Export command                                
                this.gridSearchResults.AllowPaging = false;
                this.gridSearchResults.AllowCustomPaging = false;

                RebindDataToGrid();

                #region Initialize grid columns for export
                this.gridSearchResults.MasterTableView.GetColumn("CancelButton").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ViewHistoryLinkButton").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("HistoryButton").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTReason").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTApprovalDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTDurationHour").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("IsOTDueToShiftSpan").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateTime").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("dtINLastRow").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("dtOUT").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTStartTime").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTEndTime").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("RequiredWorkDuration").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("TotalWorkDuration").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("EmpName").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("StatusDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("DistListDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateFullName").Visible = false;                

                this.gridSearchResults.MasterTableView.GetColumn("OTReasonExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTApprovalDescExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTDurationHourExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateTimeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("dtINExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("dtOUTExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTStartTimeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTEndTimeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("RequiredWorkDurationExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("TotalWorkDurationExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("EmpNameExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("StatusDescExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("DistListDescExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateFullNameExport").Visible = true;
                #endregion

                this.gridSearchResults.ExportSettings.Excel.Format = GridExcelExportFormat.Biff;
                this.gridSearchResults.ExportSettings.IgnorePaging = true;
                this.gridSearchResults.ExportSettings.ExportOnlyData = true;
                this.gridSearchResults.ExportSettings.OpenInNewWindow = true;
                this.gridSearchResults.ExportSettings.UseItemStyles = true;

                this.gridSearchResults.AllowPaging = true;
                this.gridSearchResults.AllowCustomPaging = true;
                this.gridSearchResults.Rebind();
                #endregion
            }
            else if (e.CommandName.Equals(RadGrid.RebindGridCommandName))
            {
                RebindDataToGrid();
            }
        }

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            #region Customize the grid pager items                        
            if (e.Item is GridPagerItem)
            {
                RadComboBox myPageSizeCombo = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
                if (myPageSizeCombo != null)
                {
                    // Clear default items
                    myPageSizeCombo.Items.Clear();

                    // Add new items
                    string[] arrayPageSize = { "10", "20", "30", "40", "50" };
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
            #endregion

            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    string statusHandlingCode = UIHelper.ConvertObjectToString(item["StatusHandlingCode"].Text);
                    string statusCode = UIHelper.ConvertObjectToString(item["StatusCode"].Text);
                    bool isOTProcessed = UIHelper.ConvertObjectToBolean(item["IsOTAlreadyProcessed"].Text);
                    bool isArrivedEarly = UIHelper.ConvertObjectToBolean(item["IsArrivedEarly"].Text);
                    bool isOTExceedOrig = UIHelper.ConvertObjectToBolean(item["IsOTExceedOrig"].Text);                    
                    int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                    string[] callOutArray = ConfigurationManager.AppSettings["OTCallOut"].Split(',');
                    bool isPublicHoliday = UIHelper.ConvertObjectToBolean(item["IsPublicHoliday"].Text);
                    bool isRamadan = UIHelper.ConvertObjectToBolean(item["IsRamadan"].Text);
                    bool IsOTRamadanExceedLimit = UIHelper.ConvertObjectToBolean(item["IsOTRamadanExceedLimit"].Text);

                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(item["AutoID"].Text);

                    // Initialize control variables
                    TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                    RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                    RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                    RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                    RadLabel lblDuration = (RadLabel)item["OTDurationHour"].FindControl("lblDuration");

                    #region Process "OT Approved?" Header
                    foreach (GridHeaderItem headerItem in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Header))
                    {
                        CheckBox chkOTApprove = (CheckBox)headerItem["OTApprovalDesc"].Controls[1]; // Get the header checkbox 
                        if (chkOTApprove != null)
                        {
                            chkOTApprove.Checked = this.IsOTApprove;
                        }

                        if (chkOTApprove.Checked)
                        {
                            if (isPublicHoliday)
                            {
                                if (txtRemarks != null)
                                    txtRemarks.Text = UIHelper.CONST_PUBLIC_HOLIDAY_DESC;

                                if (cboOTReason != null)
                                {
                                    cboOTReason.Text = UIHelper.CONST_PUBLIC_HOLIDAY_DESC;
                                    cboOTReason.SelectedValue = UIHelper.CONST_PUBLIC_HOLIDAY_CODE;
                                }
                            }
                            else if (isRamadan)
                            {
                                if (txtRemarks != null)
                                    txtRemarks.Text = UIHelper.CONST_RAMADAN_OT_REASON_DESC;

                                if (cboOTReason != null)
                                {
                                    cboOTReason.Text = UIHelper.CONST_RAMADAN_OT_REASON_DESC;
                                    cboOTReason.SelectedValue = UIHelper.CONST_RAMADAN_OT_REASON_CODE;
                                }
                            }
                            else
                            {
                                if (txtRemarks != null)
                                    txtRemarks.Text = string.Empty;

                                if (cboOTReason != null)
                                {
                                    cboOTReason.Text = string.Empty;
                                    cboOTReason.SelectedValue = null;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Process "OT Approved?" column                    
                    if (cboOTApprovalType != null)                          
                    {
                        if (cboOTApprovalType.Items.Count > 0)
                            cboOTApprovalType.SelectedValue = UIHelper.ConvertObjectToString(item["OTApprovalCode"].Text).Replace("&nbsp;", "");

                        if (cboOTApprovalType.SelectedValue == "Y")
                            cboOTApprovalType.ForeColor = System.Drawing.Color.YellowGreen;
                        else if (cboOTApprovalType.SelectedValue == "N")
                            cboOTApprovalType.ForeColor = System.Drawing.Color.Red;
                        else
                            cboOTApprovalType.ForeColor = System.Drawing.Color.Orange;
                    }
                    #endregion

                    #region Process "Meal Voucher Approved?"
                    RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                    if (cboMealVoucherEligibility != null && 
                        cboMealVoucherEligibility.Items.Count > 0)
                    {
                        cboMealVoucherEligibility.SelectedValue = UIHelper.ConvertObjectToString(item["MealVoucherEligibilityCode"].Text).Replace("&nbsp;", "");                                                                        
                    }
                    #endregion

                    #region Process "OT reason"                    
                    if (cboOTReason != null)
                    {
                        if (string.IsNullOrEmpty(cboOTReason.SelectedValue))
                            cboOTReason.SelectedValue = UIHelper.ConvertObjectToString(item["OTReasonCode"].Text).Replace("&nbsp;", "");
                    }

                    #region Enable/disable controls based on the value of "OT Approved"
                    if (cboOTApprovalType != null)
                    {
                        // Disable "OT Duration"
                        //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = false;

                            if (callOutArray.Where(a => a.Trim() == cboOTReason.SelectedValue.Trim()).FirstOrDefault() != null)
                            {
                                // Set the maximum input value equal to the callout overtime duration
                                txtDuration.MaxValue = UIHelper.ConvertObjectToDouble(item["OTDurationHourOrig"].Text);
                            }
                            else
                            {
                                // Set the maximum input value
                                txtDuration.MaxValue = UIHelper.ConvertObjectToDouble(item["OTDurationHourClone"].Text);
                            }
                        }

                        // Disable "OT Reason"
                        if (cboOTReason != null)
                            cboOTReason.Enabled = false;

                        // Disable "Remarks"                        
                        if (txtRemarks != null)
                            txtRemarks.Enabled = false;

                        if (cboOTApprovalType.SelectedValue == "Y" ||
                            cboOTApprovalType.SelectedValue == "N")
                        {
                            if (!this.IsOTApprovalHeaderClicked || isOTProcessed)
                            {
                                // Disable "OT Approved?"
                                cboOTApprovalType.Enabled = false;
                            }

                            // Enable/disable "Meal Voucher Approved?"
                            if (cboMealVoucherEligibility != null)
                            {
                                if (cboMealVoucherEligibility.SelectedValue == "YA" ||
                                    cboMealVoucherEligibility.SelectedValue == "N")
                                {
                                    cboMealVoucherEligibility.Enabled = false;
                                }
                                else
                                    cboMealVoucherEligibility.Enabled = true;
                            }
                        }
                        else
                        {
                            if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                               this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                               this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString())
                            {
                                cboOTApprovalType.Enabled = false;
                            }
                            else
                            {
                                // Enable "OT Approved?"
                                cboOTApprovalType.Enabled = true;
                            }

                            // Dsiable "Meal Voucher Approved?"
                            if (cboMealVoucherEligibility != null)
                                cboMealVoucherEligibility.Enabled = false;
                        }
                    }
                    #endregion

                    #endregion

                    #region Enable/disable other controls based on OT approval value
                    if (this.IsOTApprovalHeaderClicked && !isOTProcessed)
                    {                        
                        EmployeeAttendanceEntity selectedRecord = null;
                        if (autoID > 0)
                        {
                            selectedRecord = this.AttendanceList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                        }

                        if (cboOTApprovalType.SelectedValue == "Y" ||
                            cboOTApprovalType.SelectedValue == "N")
                        {
                            #region Enable other template controls
                            // Enable "Meal Voucher Approved?"
                            //RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            //if (cboMealVoucherEligibility != null)
                            //    cboMealVoucherEligibility.Enabled = true;

                            // Enable "OT Duration"
                            //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                                txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";

                            // Enable "OT Reason"
                            if (cboOTReason != null)
                                cboOTReason.Enabled = true;

                            // Enable "Remarks"
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;
                            #endregion

                            #region Update data in the collection                        
                            if (selectedRecord != null)
                            {
                                // Turn on the flag to save changes in the current row
                                selectedRecord.IsDirty = true;
                            }
                            #endregion

                            #region Reload data to OT Reason combobox
                            if (cboOTApprovalType.SelectedValue == "Y")
                                FillOvertimeReasonCombo(true, 1);
                            else
                                FillOvertimeReasonCombo(true, 2);
                            #endregion
                        }
                        else
                        {
                            #region Disable other template controls
                            // Disable "Meal Voucher Approved?"
                            //RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            //if (cboMealVoucherEligibility != null)
                            //    cboMealVoucherEligibility.Enabled = false;

                            // Disable "OT Duration"
                            //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                            {
                                txtDuration.Enabled = false;
                                if (this.SelectedOvertimeRecord != null)
                                {
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                                    this.txtDuration_TextChanged(txtDuration, new EventArgs());
                                }
                            }

                            // Disable "OT Reason"
                            if (cboOTReason != null)
                            {
                                cboOTReason.Enabled = false;
                                cboOTReason.SelectedIndex = -1;
                                cboOTReason.Text = string.Empty;
                            }

                            // Disable "Remarks"
                            if (txtRemarks != null)
                            {
                                txtRemarks.Enabled = false;
                                txtRemarks.Text = string.Empty;
                            }
                            #endregion

                            #region Update data in the collection                        
                            if (selectedRecord != null)
                            {
                                // Turn off the flag to skip saving changes in the current row
                                selectedRecord.IsDirty = false;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    if (!this.gridSearchResults.IsExporting)
                    {
                        #region Set background color 
                        if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                        {
                            if (statusHandlingCode == "Open")
                            {
                                item.BackColor = System.Drawing.Color.FromName("#8cccff");
                                if (isOTExceedOrig)
                                {
                                    item.ForeColor = System.Drawing.Color.Red;
                                    item["ArrivalSchedule"].ForeColor = System.Drawing.Color.Red;
                                    item["ActualShiftCode"].ForeColor = System.Drawing.Color.Red;
                                    item["ActualShiftCode"].Font.Bold = false;
                                }
                                else
                                {
                                    item.ForeColor = System.Drawing.Color.Black;
                                    item["OTStartTime"].ForeColor = System.Drawing.Color.Black;
                                    item["OTEndTime"].ForeColor = System.Drawing.Color.Black;
                                    item["ArrivalSchedule"].ForeColor = System.Drawing.Color.Black;
                                    item["ActualShiftCode"].ForeColor = System.Drawing.Color.Black;
                                    item["ActualShiftCode"].Font.Bold = false;
                                }
                            }
                            else if (statusHandlingCode == "Rejected")
                            {
                                item.BackColor = System.Drawing.Color.FromName("#ff3300");
                                item.ForeColor = System.Drawing.Color.Yellow;
                                item["OTStartTime"].ForeColor = System.Drawing.Color.Yellow;
                                item["OTEndTime"].ForeColor = System.Drawing.Color.Yellow;
                                item["ArrivalSchedule"].ForeColor = System.Drawing.Color.Yellow;
                                item["ActualShiftCode"].ForeColor = System.Drawing.Color.Yellow;
                                item["ActualShiftCode"].Font.Bold = false;
                            }
                            else if (statusHandlingCode == "Cancelled")
                            {
                                item.BackColor = System.Drawing.Color.FromName("#ff6600");
                                item.ForeColor = System.Drawing.Color.Black;
                                item["OTStartTime"].ForeColor = System.Drawing.Color.Black;
                                item["OTEndTime"].ForeColor = System.Drawing.Color.Black;
                                item["ArrivalSchedule"].ForeColor = System.Drawing.Color.Black;
                                item["ActualShiftCode"].ForeColor = System.Drawing.Color.Black;
                                item["ActualShiftCode"].Font.Bold = false;
                            }
                            else if (statusHandlingCode == "Closed" ||
                                statusHandlingCode == "Approved")
                            {
                                item.BackColor = System.Drawing.Color.FromName("#99ff66");
                            }
                        }
                        else if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTUNPROCSD.ToString())
                        {
                            #region Show Unprocessed Overtime                                                        
                            if (isArrivedEarly)
                            {
                                //item.BackColor = System.Drawing.Color.FromName("#ffcc66");
                                item.ForeColor = System.Drawing.Color.DarkOrange;
                                item["OTStartTime"].ForeColor = System.Drawing.Color.DarkOrange;
                                item["OTEndTime"].ForeColor = System.Drawing.Color.DarkOrange;
                                item["ArrivalSchedule"].ForeColor = System.Drawing.Color.DarkOrange;
                                item["ActualShiftCode"].ForeColor = System.Drawing.Color.DarkOrange;
                                //item["ActualShiftCode"].Font.Bold = false;
                            }

                            // Set the font color to red if overtime exceeds 2 hours during Ramadan for Muslims
                            if (IsOTRamadanExceedLimit)
                            {
                                item.ForeColor = System.Drawing.Color.Red;
                                item["EmpNo"].ForeColor = System.Drawing.Color.Red;
                                item["ActualShiftCode"].ForeColor = System.Drawing.Color.Red;
                                item["ActualShiftCode"].Font.Bold = false;

                                //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                                if (txtDuration != null)
                                    txtDuration.ToolTip = UIHelper.CONST_OT_DURATION_RAMADAN;
                            }
                            #endregion
                        }
                        #endregion
                    }

                    #region Enable/disable Cancel button 
                    ImageButton imgCancel = (ImageButton)item["CancelButton"].FindControl("imgCancelOT");
                    if (imgCancel != null)
                    {
                        if (userEmpNo == createdByEmpNo)
                        {
                            imgCancel.Enabled = statusHandlingCode == "Open";
                            if (imgCancel.Enabled)
                            {
                                imgCancel.ImageUrl = @"~/Images/delete_enabled_icon.png";
                                imgCancel.ToolTip = "Cancel overtime request";
                            }
                            else
                            {
                                imgCancel.ImageUrl = @"~/Images/delete_disabled_icon.png";
                                imgCancel.ToolTip = "Cancelling overtime request is disabled";
                            }
                        }
                        else
                        {
                            imgCancel.Enabled = false;
                            imgCancel.ImageUrl = @"~/Images/delete_disabled_icon.png";
                            imgCancel.ToolTip = "Cancelling overtime request is disabled";
                        }
                    }
                    #endregion

                    #region Enable/disable "View history" link
                    ImageButton imgViewHistory = (ImageButton)item["HistoryButton"].FindControl("imgViewHistory"); // item["HistoryButton"].Controls[0] as ImageButton;
                    if (imgViewHistory != null)
                    {
                        if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                            this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                            !string.IsNullOrEmpty(statusHandlingCode))
                        {
                            imgViewHistory.Visible = true;
                            imgViewHistory.ToolTip = "View approval history";
                        }
                        else
                        {
                            imgViewHistory.Visible = false;
                            imgViewHistory.ToolTip = "Control is disabled";
                        }
                    }

                    //LinkButton viewHistoryLink = item["ViewHistoryLinkButton"].Controls[0] as LinkButton;
                    //if (viewHistoryLink != null)
                    //{
                    //    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                    //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                    //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                    //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                    //        statusHandlingCode == "Open")
                    //    {
                    //        viewHistoryLink.Enabled = true;
                    //        viewHistoryLink.ForeColor = System.Drawing.Color.Blue;
                    //    }
                    //    else
                    //    {
                    //        viewHistoryLink.Enabled = false;
                    //        viewHistoryLink.ForeColor = System.Drawing.Color.Gray;
                    //    }
                    //}
                    #endregion

                    #region Set OT duration value into 24-hour time format
                    if (txtDuration != null &&
                        lblDuration != null)
                    {
                        decimal otDuration = UIHelper.ConvertObjectToDecimal(txtDuration.Value);

                        if (otDuration > 0)
                        {
                            if (otDuration > 0 && otDuration < 10)
                            {
                                txtDuration.ToolTip = string.Format("Duration: 00:0{0}", otDuration);
                                lblDuration.Text = string.Format("00:0{0}", otDuration);
                            }
                            else if (otDuration >= 10 && otDuration < 60)
                            {
                                txtDuration.ToolTip = string.Format("Duration: 00:{0}", otDuration);
                                lblDuration.Text = string.Format("00:{0}", otDuration);
                            }
                            else if (otDuration == 60)
                            {
                                txtDuration.ToolTip = "Duration: 01:00";
                                lblDuration.Text = "01:00";
                            }
                            else if (otDuration > 60 && otDuration < 100)
                            {
                                var quotient = Math.Floor(otDuration / 60);
                                var remainder = otDuration % 60;

                                if (remainder < 10)
                                {
                                    txtDuration.ToolTip = string.Format("Duration: 0{0}:0{1}", quotient, remainder);
                                    lblDuration.Text = string.Format("0{0}:0{1}", quotient, remainder);
                                }
                                else
                                {
                                    txtDuration.ToolTip = string.Format("Duration: 0{0}:{1}", quotient, remainder);
                                    lblDuration.Text = string.Format("0{0}:{1}", quotient, remainder);
                                }
                            }
                            else
                            {
                                if (otDuration.ToString().Length == 3)
                                {
                                    txtDuration.ToolTip = "Duration: " + string.Concat("0", otDuration.ToString()).Insert(2, ":");
                                    lblDuration.Text = string.Concat("0", otDuration.ToString()).Insert(2, ":");
                                }
                                else
                                {
                                    txtDuration.ToolTip = "Duration: " + otDuration.ToString().Insert(2, ":");
                                    lblDuration.Text = otDuration.ToString().Insert(2, ":");
                                }
                            }

                            lblDuration.Visible = otDuration.ToString().Length <= 2;
                        }
                    }
                    #endregion
                }
            }
        }

        protected void gridSearchResults_PreRender(object sender, EventArgs e)
        {
            try
            {
                #region Show/hide Cancel button 
                GridColumn dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CancelButton").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                        dynamicColumn.Visible = true;
                    else
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show/Hide View History button
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "HistoryButton").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}

                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ViewHistoryLinkButton").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
                #endregion

                #region Show/hide "Status" field 
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "StatusDesc").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                        dynamicColumn.Visible = true;
                    else
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show/hide "Currently Assigned To" field 
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CurrentlyAssignedFullName").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                        dynamicColumn.Visible = true;
                    else
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show/hide "Approval Level" field 
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "DistListDesc").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                        dynamicColumn.Visible = true;
                    else
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show/hide "Requisition No." field 
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "OTRequestNo").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTUNPROCSD.ToString())
                        dynamicColumn.Visible = false;
                    else
                        dynamicColumn.Visible = true;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void RebindDataToGrid()
        {
            if (this.AttendanceList.Count > 0)
            {
                int totalRecords = this.AttendanceList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridSearchResults.VirtualItemCount = totalRecords;
                else
                    this.gridSearchResults.VirtualItemCount = 1;

                this.gridSearchResults.DataSource = this.AttendanceList;
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
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            this.txtEmpNo.Text = string.Empty;            
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTUNPROCSD.ToString();
            this.btnSave.Enabled = true;

            // Cler collections
            this.AttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["SelectedOvertimeRecord"] = null;
            ViewState["IsOTApprove"] = null;
            ViewState["IsOTApprovalHeaderClicked"] = null;

            // Reset the grid
            this.gridSearchResults.VirtualItemCount = 1;
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

            // Check Data Filter Option
            if (string.IsNullOrEmpty(this.cboFilterOption.SelectedValue))
            {
                this.txtGeneric.Text = ValidationErrorType.NoDataFilterOption.ToString();
                this.ErrorType = ValidationErrorType.NoDataFilterOption;
                this.cusValFilterOption.Validate();
                errorCount++;
            }

            // Check date range
            if (this.dtpStartDate.SelectedDate == null)
            {
                //if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTUNPROCSD.ToString())        // Start Date is required if Display Option is set to "Show unprocessed overtime"
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoStartDateUnprocessed.ToString();
                //    this.ErrorType = ValidationErrorType.NoStartDateUnprocessed;
                //    this.cusValStartDate.Validate();
                //    errorCount++;
                //}
                //else if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())    // Start Date is required if Display Option is set to "Show All"
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoStartDateAllRecords.ToString();
                //    this.ErrorType = ValidationErrorType.NoStartDateAllRecords;
                //    this.cusValStartDate.Validate();
                //    errorCount++;
                //}
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

            GetOvertimeRecord(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_ENTRY
            ),
            false);
        }
        
        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (this.AttendanceList.Count == 0)
                return;

            try
            {
                #region Check if payroll processing is currently on-going
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;

                bool isPayrollProcessing = proxy.CheckIfPayrollProcessing(ref error, ref innerError);
                if (isPayrollProcessing)
                {
                    this.txtGeneric.Text = ValidationErrorType.OngoingPayrollProcess.ToString();
                    this.ErrorType = ValidationErrorType.OngoingPayrollProcess;
                    this.cusValButton.Validate();
                    return;
                }
                #endregion

                #region Check the selected grid's page size
                //if (this.gridSearchResults.PageSize > 50)
                //{
                //    throw new Exception("The maximum number of overtime request that can be submitted for processing is 50. Please set the grid page size to 50 or below then try to save again!");
                //}
                #endregion

                int errorCount = 0;
                StringBuilder sb = new StringBuilder();
                List<EmployeeAttendanceEntity> attendanceList = new List<EmployeeAttendanceEntity>();

                #region Build the collection and populate overtime record
                foreach (GridDataItem item in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Item, GridItemType.AlternatingItem))
                {
                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            if (selectedRecord.IsDirty)
                            {
                                EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity();

                                // Store the identity key
                                newItem.AutoID = autoID;

                                #region Set value for "OTApprovalCode", "OTApprovalDesc"
                                RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                                if (cboOTApprovalType != null)
                                {
                                    newItem.OTApprovalCode = cboOTApprovalType.SelectedValue;
                                    newItem.OTApprovalDesc = cboOTApprovalType.Text;
                                }
                                #endregion

                                #region Set value for "MealVoucherEligibilityCode", "MealVoucherEligibility"
                                RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                                if (cboMealVoucherEligibility != null)
                                {
                                    newItem.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                    newItem.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                                }
                                #endregion

                                #region Set value for "OTReasonCode", "OTReason"
                                RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                                if (cboOTReason != null)
                                {
                                    if (string.IsNullOrEmpty(cboOTReason.SelectedValue) ||
                                        cboOTReason.SelectedValue.Replace("&nbsp;", "").Trim() == string.Empty ||
                                        cboOTReason.SelectedValue == "0")
                                    {
                                        errorCount += 1;
                                        sb.AppendLine(string.Format(@"OT Reason for Employee No. {0} is mandatory. Please specify the overtime reason then try to save again!<br />",  selectedRecord.EmpNo));
                                    }
                                    else
                                    {
                                        newItem.OTReasonCode = cboOTReason.SelectedValue;
                                        newItem.OTReason = cboOTReason.Text;
                                    }
                                }
                                #endregion

                                #region Set value for "AttendanceRemarks"
                                TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                                if (txtRemarks != null)
                                {
                                    if (string.IsNullOrEmpty(txtRemarks.Text))
                                    {
                                        errorCount += 1;
                                        sb.AppendLine(string.Format(@"Please specify the overtime remarks for Employee No. {0}. <br />", selectedRecord.EmpNo));
                                    }
                                    else
                                    {
                                        newItem.AttendanceRemarks = txtRemarks.Text.Trim();
                                    }
                                }
                                #endregion

                                #region Set value for "OTDurationHour"
                                RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                                if (txtDuration != null)
                                {
                                    bool isValidOTDuration = false;
                                    int specifiedOTDurationHour = UIHelper.ConvertObjectToInt(txtDuration.Value);
                                    int specifiedOTDurationMin = 0;
                                    double actualOTDurationMin = 0;
                                    int hours = 0;
                                    int minutes = 0;

                                    // Convert hour duration into minutes
                                    hours = Math.DivRem(specifiedOTDurationHour, 100, out minutes);
                                    specifiedOTDurationMin = (hours * 60) + minutes;

                                    // Validate overtime duration
                                    if (selectedRecord.OTStartTime != null &&
                                        selectedRecord.OTEndTime != null)
                                    {
                                        DateTime otStart = Convert.ToDateTime(selectedRecord.dtIN);
                                        DateTime otEnd = Convert.ToDateTime(selectedRecord.dtOUT);

                                        actualOTDurationMin = (new DateTime(otEnd.Year, otEnd.Month, otEnd.Day, otEnd.Hour, otEnd.Minute, 0) - new DateTime(otStart.Year, otStart.Month, otStart.Day, otStart.Hour, otStart.Minute, 0)).TotalMinutes;
                                        if (actualOTDurationMin < 0)
                                        {
                                            //otStart = Convert.ToDateTime(selectedRecord.OTStartTime);
                                            //otEnd = Convert.ToDateTime(selectedRecord.OTEndTime);

                                            actualOTDurationMin = 1440 + (new DateTime(otEnd.Year, otEnd.Month, otEnd.Day, otEnd.Hour, otEnd.Minute, 0) - new DateTime(otStart.Year, otStart.Month, otStart.Day, otStart.Hour, otStart.Minute, 0)).TotalMinutes;
                                        }
                                    }

                                    if (specifiedOTDurationMin > 0 && actualOTDurationMin > 0)
                                    {
                                        if (specifiedOTDurationMin > Convert.ToInt32(actualOTDurationMin))
                                        {
                                            #region Check if overtime reason is a callout
                                            string[] callOutArray = ConfigurationManager.AppSettings["OTCallOut"].Split(',');
                                            if (callOutArray != null)
                                            {
                                                if (callOutArray.Where(a => a.Trim() == cboOTReason.SelectedValue.Trim()).FirstOrDefault() != null)
                                                    isValidOTDuration = true;
                                            }
                                            #endregion
                                        }                                        
                                        else
                                            isValidOTDuration = true;
                                    }
                                    else if (specifiedOTDurationMin == 0 && actualOTDurationMin > 0)
                                    {
                                        // Note: Allow zero minutes overtime duration
                                        isValidOTDuration = true;   
                                        
                                        // Disallow zero minutes overtime duration (Note: Code is temporaty commented)
                                        //if (txtDuration.Enabled)
                                        //{
                                        //    isValidOTDuration = false;
                                        //    errorCount += 1;
                                        //    sb.AppendLine(string.Format(@"OT Duration for Employee No. {0} is mandatory if overtime is approved. Take note that duration should be greater than zero.<br />", selectedRecord.EmpNo));
                                        //}
                                    }

                                    if (isValidOTDuration)
                                    {
                                        int maxOTMinutes = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["MaxOTMinutes"]);

                                        newItem.OTDurationHour = specifiedOTDurationHour;
                                        newItem.OTDurationMinute = specifiedOTDurationMin;

                                        #region Check if overtime duration is greater than or equals to the limit set in the config file
                                        if (maxOTMinutes > 0 &&
                                            txtDuration.Enabled)
                                        {
                                            if (newItem.OTDurationMinute >= maxOTMinutes)
                                            {
                                                errorCount += 1;
                                                sb.AppendLine(string.Format(@"The overtime duration of Employee No. {0} on {1} is not allowed. Take note that duration should not be equal or greater than {2} hours.<br />", 
                                                    selectedRecord.EmpNo,
                                                    Convert.ToDateTime(selectedRecord.DT).ToString("dd-MMM-yyyy"),
                                                    maxOTMinutes / 60));
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        errorCount += 1;
                                        sb.AppendLine(string.Format(@"The specified overtime duration for Employee No. {0} is invalid. Take note that duration should not exceed the total work duration.<br />", selectedRecord.EmpNo));
                                    }
                                }
                                #endregion

                                #region Set other workflow related fields
                                newItem.EmpNo = selectedRecord.EmpNo;
                                newItem.DT = selectedRecord.DT;
                                #endregion

                                // Add item to the collection
                                attendanceList.Add(newItem);
                            }
                        }
                    }
                }
                #endregion

                #region Check for errors
                if (errorCount > 0)
                {
                    throw new Exception(sb.ToString().Trim());
                }
                #endregion

                if (attendanceList.Count > 0)
                {
                    string errorMsg = string.Empty;
                    SaveOvertime(attendanceList, ref errorMsg);

                    if (!string.IsNullOrEmpty(errorMsg))
                        throw new Exception(errorMsg);
                }
                else
                    throw new Exception("Could not proceed because there are no selected overtime records that require approval.");
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnCancelDummy_Click(object sender, EventArgs e)
        {
            if (this.SelectedOvertimeRecord != null)
            {
                CancelOvertimeRequest(this.SelectedOvertimeRecord);
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
                else if (this.ErrorType == ValidationErrorType.NoStartDateUnprocessed)
                {
                    validator.ErrorMessage = "Start Date is required when viewing unprocessed overtime records.";
                    validator.ToolTip = "Start Date is required when viewing unprocessed overtime records.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoStartDateAllRecords)
                {
                    validator.ErrorMessage = "Start Date is required when viewing all records.";
                    validator.ToolTip = "Start Date is required when viewing all records.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDataFilterOption)
                {
                    validator.ErrorMessage = "Please select a data filter option!";
                    validator.ToolTip = "Please select a data filter option!";
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
                else if (this.ErrorType == ValidationErrorType.OngoingPayrollProcess)
                {
                    validator.ErrorMessage = "Payroll Processing is currently on-going. You cannot submit an overtime request until the payroll process is completed. Sorry for the inconvenience, please try again later!";
                    validator.ToolTip = "Payroll Processing is currently on-going. You cannot submit an overtime request until the payroll process is completed. Sorry for the inconvenience, please try again later!";
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
                //this.dtpStartDate.SelectedDate = null;
                //this.dtpEndDate.SelectedDate = null;

                this.cboMonth.SelectedIndex = -1;
                this.cboMonth.Text = string.Empty;
                this.txtYear.Text = string.Empty;
                this.dtpStartDate.Focus();
            }
        }

        protected void cboOTReason_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            try
            {
                RadComboBox cboOTReason = (RadComboBox)sender;
                GridDataItem gridItem = cboOTReason.Parent.Parent as GridDataItem;
                if (gridItem != null)
                {
                    RadComboBox cboOTApprovalType = (RadComboBox)gridItem["OTDurationHour"].FindControl("cboOTApprovalType");
                    if (cboOTApprovalType != null)
                    {
                        if (cboOTApprovalType.SelectedValue == "Y")
                            FillOvertimeReasonCombo(true, 1);
                        else
                            FillOvertimeReasonCombo(true, 2);
                    }
                }

                if (this.OTReasonList != null)
                {
                    // Clear combobox items
                    cboOTReason.Items.Clear();

                    foreach (UDCEntity item in this.OTReasonList)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem();
                        cboItem.Text = item.Description;
                        cboItem.Value = item.Code;
                        cboItem.Attributes.Add(item.Code, item.Description);

                        // Add item to combobox
                        cboOTReason.Items.Add(cboItem);
                        cboItem.DataBind();
                    }

                    if (this.SelectedOvertimeRecord != null &&
                        !string.IsNullOrEmpty(this.SelectedOvertimeRecord.SelectedOTReasonCode))
                    {
                        cboOTReason.SelectedValue = this.SelectedOvertimeRecord.SelectedOTReasonCode;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboOTReason_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                RadComboBox cboOTReason = sender as RadComboBox;
                if (cboOTReason != null &&
                    !string.IsNullOrEmpty(cboOTReason.SelectedValue))
                {
                    GridDataItem item = cboOTReason.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        #region Check if the selected OT Reason is not public holiday
                        if (autoID > 0 && this.AttendanceList.Count > 0)
                        {
                            this.SelectedOvertimeRecord = this.AttendanceList.Where(a => a.AutoID == autoID).FirstOrDefault();
                            if (this.SelectedOvertimeRecord != null)
                            {
                                TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                                if (txtRemarks != null)
                                {
                                    if (this.SelectedOvertimeRecord.IsPublicHoliday)
                                    {
                                        #region Public Holidays
                                        if (cboOTReason.SelectedValue != UIHelper.CONST_PUBLIC_HOLIDAY_CODE &&
                                        txtRemarks.Text == UIHelper.CONST_PUBLIC_HOLIDAY_DESC)
                                        {
                                            txtRemarks.Text = string.Empty;
                                        }
                                        else if (cboOTReason.SelectedValue == UIHelper.CONST_PUBLIC_HOLIDAY_CODE &&
                                            txtRemarks.Text == string.Empty)
                                        {
                                            txtRemarks.Text = UIHelper.CONST_PUBLIC_HOLIDAY_DESC;
                                        }
                                        #endregion
                                    }
                                    else if (this.SelectedOvertimeRecord.IsRamadan)
                                    {
                                        #region Ramadan
                                        if (cboOTReason.SelectedValue != UIHelper.CONST_RAMADAN_OT_REASON_CODE &&
                                            txtRemarks.Text == UIHelper.CONST_RAMADAN_OT_REASON_DESC)
                                        {
                                            txtRemarks.Text = string.Empty;
                                        }
                                        else if (cboOTReason.SelectedValue == UIHelper.CONST_RAMADAN_OT_REASON_CODE &&
                                            txtRemarks.Text == string.Empty)
                                        {
                                            txtRemarks.Text = UIHelper.CONST_RAMADAN_OT_REASON_DESC;
                                        }
                                        #endregion
                                    }
                                }                                
                            }                            
                        }
                        #endregion

                        #region Check if selected overtime reason is a callout
                        int otDurationOrig = UIHelper.ConvertObjectToInt(item["OTDurationHourClone"].Text);
                        int callOutValue = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["OTCallOutValue"]);                        
                        string[] callOutArray = ConfigurationManager.AppSettings["OTCallOut"].Split(',');

                        if (callOutArray != null)
                        {
                            RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                            {
                                if (callOutArray.Where(a => a.Trim() == cboOTReason.SelectedValue.Trim()).FirstOrDefault() != null)
                                {
                                    txtDuration.MaxValue = otDurationOrig + callOutValue;

                                    if (txtDuration.Value >= 0 &&
                                        txtDuration.Value != otDurationOrig)
                                    {
                                        if ((txtDuration.Value + callOutValue) < txtDuration.MaxValue)
                                            txtDuration.Value = txtDuration.Value  + callOutValue;
                                    }
                                    else
                                        txtDuration.Value = otDurationOrig + callOutValue;
                                }
                                else
                                {
                                    txtDuration.MaxValue = otDurationOrig;

                                    if (UIHelper.ConvertObjectToInt(txtDuration.Text) == 0)
                                        txtDuration.Value = otDurationOrig;
                                }
                            }
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

        protected void cboOTApprovalType_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            RadComboBox cboOTApprovalType = (RadComboBox)sender;
            if (cboOTApprovalType != null)
            {
                GridDataItem item = cboOTApprovalType.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Reset session
                    this.SelectedOvertimeRecord = null;

                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                    // Save current selected datagrid row
                    if (autoID > 0 && this.AttendanceList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                            this.SelectedOvertimeRecord = selectedRecord;
                    }

                    if (cboOTApprovalType.SelectedValue == "Y" ||
                        cboOTApprovalType.SelectedValue == "N")
                    {
                        #region Enable other template controls
                        // Enable "Meal Voucher Approved?"
                        RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = true;

                        // Enable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";
                            if (!txtDuration.Enabled)
                            {
                                if (this.SelectedOvertimeRecord != null)
                                {
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                                    this.txtDuration_TextChanged(txtDuration, new EventArgs());
                                }
                            }
                        }

                        // Enable "OT Reason"
                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                        if (cboOTReason != null)
                        {
                            cboOTReason.Enabled = true;
                            cboOTReason.SelectedIndex = -1;
                            cboOTReason.Text = string.Empty;
                        }

                        // Enable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                            txtRemarks.Enabled = true;
                        #endregion

                        #region Update data in the collection                        
                        if (this.SelectedOvertimeRecord != null)
                        {
                            // Turn on the flag to save changes in the current row
                            this.SelectedOvertimeRecord.IsDirty = true;

                            // Set the value for "OTApprovalCode" and "OTApprovalDesc" fields
                            //this.SelectedOvertimeRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                            //this.SelectedOvertimeRecord.OTApprovalDesc = cboOTApprovalType.Text;
                        }
                        #endregion

                        #region Reload data to OT Reason combobox
                        if (cboOTApprovalType.SelectedValue == "Y")
                        {
                            FillOvertimeReasonCombo(true, 1);

                            if (this.SelectedOvertimeRecord != null)
                            {
                                if (this.SelectedOvertimeRecord.IsPublicHoliday)
                                {
                                    #region Set the OT Reason and Remarks during public holidays
                                    this.SelectedOvertimeRecord.SelectedOTReasonCode = UIHelper.CONST_PUBLIC_HOLIDAY_CODE;

                                    // Set the OT reason
                                    if (cboOTReason != null)
                                    {
                                        cboOTReason.Text = UIHelper.CONST_PUBLIC_HOLIDAY_DESC;
                                        cboOTReason.SelectedValue = UIHelper.CONST_PUBLIC_HOLIDAY_CODE;
                                    }

                                    // Set the remarks
                                    if (txtRemarks != null)
                                        txtRemarks.Text = UIHelper.CONST_PUBLIC_HOLIDAY_DESC;
                                    #endregion
                                }
                                else if (this.SelectedOvertimeRecord.IsRamadan)
                                {
                                    #region Set the OT Reason and Remarks during Ramadan
                                    this.SelectedOvertimeRecord.SelectedOTReasonCode = UIHelper.CONST_RAMADAN_OT_REASON_CODE;

                                    // Set the OT reason
                                    if (cboOTReason != null)
                                    {
                                        cboOTReason.Text = UIHelper.CONST_RAMADAN_OT_REASON_DESC;
                                        cboOTReason.SelectedValue = UIHelper.CONST_RAMADAN_OT_REASON_CODE;
                                    }

                                    // Set the remarks
                                    if (txtRemarks != null)
                                        txtRemarks.Text = UIHelper.CONST_RAMADAN_OT_REASON_DESC;
                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            FillOvertimeReasonCombo(true, 2);

                            if (txtRemarks != null)
                                txtRemarks.Text = string.Empty;
                        }
                        #endregion

                        #region Set the font color                                                 
                        if (cboOTApprovalType.SelectedValue == "Y")
                            cboOTApprovalType.ForeColor = System.Drawing.Color.YellowGreen;
                        else if (cboOTApprovalType.SelectedValue == "N")
                            cboOTApprovalType.ForeColor = System.Drawing.Color.Red;
                        #endregion
                    }
                    else
                    {
                        #region Disable other template controls
                        // Disable "Meal Voucher Approved?"
                        RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = false;

                        // Disable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = false;
                            if (this.SelectedOvertimeRecord != null)
                            {
                                txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                                this.txtDuration_TextChanged(txtDuration, new EventArgs());
                            }
                        }

                        // Disable "OT Reason"
                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                        if (cboOTReason != null)
                        {
                            cboOTReason.Enabled = false;
                            cboOTReason.SelectedIndex = -1;
                            cboOTReason.Text = string.Empty;
                        }

                        // Disable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                        {
                            txtRemarks.Enabled = false;
                            txtRemarks.Text = string.Empty;
                        }
                        #endregion

                        #region Update data in the collection                        
                        if (this.SelectedOvertimeRecord != null)
                        {
                            // Turn off the flag to skip saving changes in the current row
                            this.SelectedOvertimeRecord.IsDirty = false;

                            // Set the value for "OTApprovalCode" and "OTApprovalDesc" fields
                            //this.SelectedOvertimeRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                            //this.SelectedOvertimeRecord.OTApprovalDesc = cboOTApprovalType.Text;
                        }
                        #endregion

                        cboOTApprovalType.ForeColor = System.Drawing.Color.Orange;
                    }
                }
            }
        }

        protected void cboMealVoucherEligibility_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                RadComboBox cboMealVoucherEligibility = (RadComboBox)sender;
                if (cboMealVoucherEligibility != null)
                {
                    GridDataItem item = cboMealVoucherEligibility.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        #region Get the selected record
                        // Reset session
                        this.SelectedOvertimeRecord = null;

                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        // Save current selected datagrid row
                        if (autoID > 0 && this.AttendanceList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null)
                                this.SelectedOvertimeRecord = selectedRecord;
                        }
                        #endregion

                        // Initialize template controls
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");

                        if (cboMealVoucherEligibility.SelectedValue == "YA" ||
                            cboMealVoucherEligibility.SelectedValue == "N")
                        {
                            #region Enable other template controls
                            // Enable "Remarks"
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;
                            #endregion

                            #region Update data in the collection                        
                            if (this.SelectedOvertimeRecord != null)
                            {
                                // Turn on the flag to save changes in the current row
                                this.SelectedOvertimeRecord.IsDirty = true;

                                // Set the value for "MealVoucherEligibilityCode" and "MealVoucherEligibility" fields
                                //this.SelectedOvertimeRecord.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                //this.SelectedOvertimeRecord.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                            }
                            #endregion
                        }
                        else
                        {
                            #region Disable other template controls
                            // Disable "Remarks"
                            if (txtRemarks != null)
                            {
                                txtRemarks.Enabled = false;
                                //txtRemarks.Text = string.Empty;
                            }
                            #endregion

                            #region Update data in the collection                        
                            if (this.SelectedOvertimeRecord != null)
                            {
                                // Turn off the flag to skip saving changes in the current row
                                this.SelectedOvertimeRecord.IsDirty = true;

                                // Set the value for "MealVoucherEligibilityCode" and "MealVoucherEligibility" fields
                                //this.SelectedOvertimeRecord.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                //this.SelectedOvertimeRecord.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkOTApprove_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkOTApprove = sender as CheckBox;
                if (chkOTApprove != null)
                {
                    // Save to session
                    this.IsOTApprove = UIHelper.ConvertObjectToBolean(chkOTApprove.Checked);
                    this.IsOTApprovalHeaderClicked = true;

                    if (this.AttendanceList.Count > 0)
                    {
                        foreach (EmployeeAttendanceEntity item in this.AttendanceList)
                        {
                            if (!item.IsOTAlreadyProcessed)
                            {
                                item.OTApprovalDesc = chkOTApprove.Checked == true ? "Yes" : "No";
                                item.OTApprovalCode = chkOTApprove.Checked == true ? "Y" : "N";

                                if (item.IsPublicHoliday)
                                    item.OTReasonCode =  UIHelper.CONST_PUBLIC_HOLIDAY_CODE;
                                else if (item.IsRamadan)
                                    item.OTReasonCode = UIHelper.CONST_RAMADAN_OT_REASON_CODE;
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void txtYear_TextChanged(object sender, EventArgs e)
        {
            this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
        }

        protected void cboFilterOption_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            UIHelper.OvertimeFilter selectedDisplayOption = (UIHelper.OvertimeFilter)Enum.Parse(typeof(UIHelper.OvertimeFilter), this.cboFilterOption.SelectedValue);
            switch(selectedDisplayOption)
            {
                case UIHelper.OvertimeFilter.OTUNPROCSD:
                case UIHelper.OvertimeFilter.OTSHOWALL:
                    this.btnSave.Enabled = true;
                    break;

                default:
                    this.btnSave.Enabled = false;
                    break;
            }

            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void chkAllowOT_CheckedChanged(object sender, EventArgs e)
        {
            RadComboBox cboOTApprovalType = (RadComboBox)sender;
            if (cboOTApprovalType != null)
            {
                GridDataItem item = cboOTApprovalType.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Reset session
                    this.SelectedOvertimeRecord = null;

                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                    // Save current selected datagrid row
                    if (autoID > 0 && this.AttendanceList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                            this.SelectedOvertimeRecord = selectedRecord;
                    }

                    if (cboOTApprovalType.SelectedValue == "Y" ||
                        cboOTApprovalType.SelectedValue == "N")
                    {
                        #region Enable other template controls
                        // Enable "Meal Voucher Approved?"
                        RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = true;

                        // Enable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";
                            if (!txtDuration.Enabled)
                            {
                                if (this.SelectedOvertimeRecord != null)
                                {
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                                    this.txtDuration_TextChanged(txtDuration, new EventArgs());
                                }
                            }
                        }

                        // Enable "OT Reason"
                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                        if (cboOTReason != null)
                        {
                            cboOTReason.Enabled = true;
                            cboOTReason.SelectedIndex = -1;
                            cboOTReason.Text = string.Empty;
                        }

                        // Enable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                            txtRemarks.Enabled = true;
                        #endregion

                        #region Update data in the collection                        
                        if (this.SelectedOvertimeRecord != null)
                        {
                            // Turn on the flag to save changes in the current row
                            this.SelectedOvertimeRecord.IsDirty = true;

                            // Set the value for "OTApprovalCode" and "OTApprovalDesc" fields
                            //this.SelectedOvertimeRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                            //this.SelectedOvertimeRecord.OTApprovalDesc = cboOTApprovalType.Text;
                        }
                        #endregion

                        #region Reload data to OT Reason combobox
                        if (cboOTApprovalType.SelectedValue == "Y")
                            FillOvertimeReasonCombo(true, 1);
                        else
                            FillOvertimeReasonCombo(true, 2);
                        #endregion
                    }
                    else
                    {
                        #region Disable other template controls
                        // Disable "Meal Voucher Approved?"
                        RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = false;

                        // Disable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = false;
                            if (this.SelectedOvertimeRecord != null)
                            {
                                txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                                this.txtDuration_TextChanged(txtDuration, new EventArgs());
                            }
                        }

                        // Disable "OT Reason"
                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                        if (cboOTReason != null)
                        {
                            cboOTReason.Enabled = false;
                            cboOTReason.SelectedIndex = -1;
                            cboOTReason.Text = string.Empty;
                        }

                        // Disable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                        {
                            txtRemarks.Enabled = false;
                            txtRemarks.Text = string.Empty;
                        }
                        #endregion

                        #region Update data in the collection                        
                        if (this.SelectedOvertimeRecord != null)
                        {
                            // Turn off the flag to skip saving changes in the current row
                            this.SelectedOvertimeRecord.IsDirty = false;

                            // Set the value for "OTApprovalCode" and "OTApprovalDesc" fields
                            //this.SelectedOvertimeRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                            //this.SelectedOvertimeRecord.OTApprovalDesc = cboOTApprovalType.Text;
                        }
                        #endregion
                    }
                }
            }
        }

        protected void imgCancelOT_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex].GetDataKeyValue("AutoID"));

                // Save current selected datagrid row
                if (autoID > 0 &&
                    this.AttendanceList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (selectedRecord != null)
                    {
                        // Save the currently selected record
                        this.SelectedOvertimeRecord = selectedRecord;

                        // Display confirmation message
                        StringBuilder script = new StringBuilder();
                        script.Append("ConfirmButtonAction('");
                        script.Append(string.Concat(this.btnCancelDummy.ClientID, "','"));
                        script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                        script.Append(UIHelper.CONST_CANCEL_OVERTIME_CONFIRMATION + "');");
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Delete Confirmation", script.ToString(), true);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void imgViewHistory_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex].GetDataKeyValue("AutoID"));

                // Save current selected datagrid row
                if (autoID > 0 &&
                    this.AttendanceList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedOTRecord = this.AttendanceList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (selectedOTRecord != null)
                    {
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        // Save the currently selected record
                        Session["CurrentOvertimeRequest"] = selectedOTRecord;

                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_OVERTIME_APPROVAL_HISTORY + "?{0}={1}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_OVERTIME_ENTRY
                        ),
                        false);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void txtDuration_TextChanged(object sender, EventArgs e)
        {
            try
            {
                RadNumericTextBox txtDuration = (RadNumericTextBox)sender;
                GridDataItem gridItem = txtDuration.Parent.Parent as GridDataItem;

                if (gridItem != null)
                {
                    RadLabel lblDuration = (RadLabel)gridItem["OTDurationHour"].FindControl("lblDuration");
                    if (lblDuration != null)
                    {
                        decimal otDuration = UIHelper.ConvertObjectToDecimal(txtDuration.Value);

                        if (otDuration > 0 && otDuration < 10)
                        {
                            txtDuration.ToolTip = string.Format("Duration: 00:0{0}", otDuration);
                            lblDuration.Text = string.Format("00:0{0}", otDuration);
                        }
                        else if (otDuration >= 10 && otDuration < 60)
                        {
                            txtDuration.ToolTip = string.Format("Duration: 00:{0}", otDuration);
                            lblDuration.Text = string.Format("00:{0}", otDuration);
                        }
                        else if (otDuration == 60)
                        {
                            txtDuration.ToolTip = "Duration: 01:00";
                            lblDuration.Text = "01:00";
                        }
                        else if (otDuration > 60 && otDuration < 100)
                        {
                            var quotient = Math.Floor(otDuration / 60);
                            var remainder = otDuration % 60;

                            if (remainder < 10)
                            {
                                txtDuration.ToolTip = string.Format("Duration: 0{0}:0{1}", quotient, remainder);
                                lblDuration.Text = string.Format("0{0}:0{1}", quotient, remainder);
                            }
                            else
                            {
                                txtDuration.ToolTip = string.Format("Duration: 0{0}:{1}", quotient, remainder);
                                lblDuration.Text = string.Format("0{0}:{1}", quotient, remainder);
                            }
                        }
                        else
                        {
                            if (otDuration.ToString().Length == 3)
                            {
                                txtDuration.ToolTip = "Duration: " + string.Concat("0", otDuration.ToString()).Insert(2, ":");
                                lblDuration.Text = string.Concat("0", otDuration.ToString()).Insert(2, ":");
                            }
                            else
                            {
                                txtDuration.ToolTip = "Duration: " + otDuration.ToString().Insert(2, ":");
                                lblDuration.Text = otDuration.ToString().Insert(2, ":");
                            }
                        }

                        lblDuration.Visible = otDuration.ToString().Length <= 2;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls            
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.cboFilterOption.Text = string.Empty;
            this.cboFilterOption.SelectedIndex = -1;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            this.txtEmpNo.Text = string.Empty;
            //this.cboCostCenter.Text = string.Empty;
            //this.cboCostCenter.SelectedIndex = -1;
            this.btnSave.Enabled = true;
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
            this.IsLoadRequest = UIHelper.ConvertObjectToBolean(Request.QueryString["IsLoadRequest"]);
            this.OTRequestNoParam = UIHelper.ConvertObjectToLong(Request.QueryString["OTRequestNo"]);
            this.EmpNoParam = UIHelper.ConvertObjectToInt(Request.QueryString["EmpNo"]);
            this.StartDateParam = UIHelper.ConvertObjectToDate(Request.QueryString["StartDate"]);
            this.DisplayOptionParam = UIHelper.ConvertObjectToString(Request.QueryString["DisplayOption"]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.AttendanceList.Clear();
            this.OTReasonList.Clear();
            this.OvertimeFilterOptionList.Clear();
            this.CostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["SelectedOvertimeRecord"] = null;
            ViewState["IsOTApprove"] = null;
            ViewState["IsOTApprovalHeaderClicked"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.OvertimeEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.OvertimeEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.OvertimeEntryStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.OvertimeEntryStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;

            if (this.OvertimeEntryStorage.ContainsKey("IsLoadRequest"))
                this.IsLoadRequest = UIHelper.ConvertObjectToBolean(this.OvertimeEntryStorage["IsLoadRequest"]);
            else
                this.IsLoadRequest = false;

            if (this.OvertimeEntryStorage.ContainsKey("EmpNoParam"))
                this.EmpNoParam = UIHelper.ConvertObjectToInt(this.OvertimeEntryStorage["EmpNoParam"]);
            else
                this.EmpNoParam = 0;

            if (this.OvertimeEntryStorage.ContainsKey("StartDateParam"))
                this.StartDateParam = UIHelper.ConvertObjectToDate(this.OvertimeEntryStorage["StartDateParam"]);
            else
                this.StartDateParam = null;

            if (this.OvertimeEntryStorage.ContainsKey("DisplayOptionParam"))
                this.DisplayOptionParam = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["DisplayOptionParam"]);
            else
                this.DisplayOptionParam = string.Empty;

            if (this.OvertimeEntryStorage.ContainsKey("OTRequestNoParam"))
                this.OTRequestNoParam = UIHelper.ConvertObjectToLong(this.OvertimeEntryStorage["OTRequestNoParam"]);
            else
                this.OTRequestNoParam = 0;
            #endregion

            #region Restore session values
            if (this.OvertimeEntryStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.OvertimeEntryStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.OvertimeEntryStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.OvertimeEntryStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.OvertimeEntryStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.OvertimeEntryStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.OvertimeEntryStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.OvertimeEntryStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.OvertimeEntryStorage.ContainsKey("AttendanceList"))
                this.AttendanceList = this.OvertimeEntryStorage["AttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceList = null;

            if (this.OvertimeEntryStorage.ContainsKey("OTReasonList"))
                this.OTReasonList = this.OvertimeEntryStorage["OTReasonList"] as List<UDCEntity>;
            else
                this.OTReasonList = null;

            if (this.OvertimeEntryStorage.ContainsKey("SelectedOvertimeRecord"))
                this.SelectedOvertimeRecord = this.OvertimeEntryStorage["SelectedOvertimeRecord"] as EmployeeAttendanceEntity;
            else
                this.SelectedOvertimeRecord = null;

            if (this.OvertimeEntryStorage.ContainsKey("IsOTApprove"))
                this.IsOTApprove = UIHelper.ConvertObjectToBolean(this.OvertimeEntryStorage["IsOTApprove"]);
            else
                this.IsOTApprove = false;

            if (this.OvertimeEntryStorage.ContainsKey("IsOTApprovalHeaderClicked"))
                this.IsOTApprovalHeaderClicked = UIHelper.ConvertObjectToBolean(this.OvertimeEntryStorage["IsOTApprovalHeaderClicked"]);
            else
                this.IsOTApprovalHeaderClicked = false;

            if (this.OvertimeEntryStorage.ContainsKey("OvertimeFilterOptionList"))
                this.OvertimeFilterOptionList = this.OvertimeEntryStorage["OvertimeFilterOptionList"] as List<UserDefinedCodes>;
            else
                this.OvertimeFilterOptionList = null;

            if (this.OvertimeEntryStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.OvertimeEntryStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            FillComboData(false);
            #endregion

            #region Restore control values  

            if (this.OvertimeEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.OvertimeEntryStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.OvertimeEntryStorage.ContainsKey("cboMonth"))
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["cboMonth"]);
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.OvertimeEntryStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.OvertimeEntryStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OvertimeEntryStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.OvertimeEntryStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OvertimeEntryStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.OvertimeEntryStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.OvertimeEntryStorage["chkPayPeriod"]);
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

            if (this.OvertimeEntryStorage.ContainsKey("cboFilterOption"))
                this.cboFilterOption.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeEntryStorage["cboFilterOption"]);
            else
            {
                this.cboFilterOption.Text = string.Empty;
                this.cboFilterOption.SelectedIndex = -1;
            }
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
            this.OvertimeEntryStorage.Clear();
            this.OvertimeEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.OvertimeEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.OvertimeEntryStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.OvertimeEntryStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            this.OvertimeEntryStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.OvertimeEntryStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.OvertimeEntryStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.OvertimeEntryStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.OvertimeEntryStorage.Add("cboFilterOption", this.cboFilterOption.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.OvertimeEntryStorage.Add("CallerForm", this.CallerForm);
            this.OvertimeEntryStorage.Add("ReloadGridData", this.ReloadGridData);
            this.OvertimeEntryStorage.Add("IsLoadRequest", this.IsLoadRequest);
            this.OvertimeEntryStorage.Add("EmpNoParam", this.EmpNoParam);
            this.OvertimeEntryStorage.Add("StartDateParam", this.StartDateParam);
            this.OvertimeEntryStorage.Add("DisplayOptionParam", this.DisplayOptionParam);
            this.OvertimeEntryStorage.Add("OTRequestNoParam", this.OTRequestNoParam);
            #endregion

            #region Store session data to collection
            this.OvertimeEntryStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.OvertimeEntryStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.OvertimeEntryStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.OvertimeEntryStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.OvertimeEntryStorage.Add("AttendanceList", this.AttendanceList);
            this.OvertimeEntryStorage.Add("OTReasonList", this.OTReasonList);
            this.OvertimeEntryStorage.Add("SelectedOvertimeRecord", this.SelectedOvertimeRecord);
            this.OvertimeEntryStorage.Add("IsOTApprove", this.IsOTApprove);
            this.OvertimeEntryStorage.Add("IsOTApprovalHeaderClicked", this.IsOTApprovalHeaderClicked);
            this.OvertimeEntryStorage.Add("OvertimeFilterOptionList", this.OvertimeFilterOptionList);
            this.OvertimeEntryStorage.Add("CostCenterList", this.CostCenterList);
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
            FillOvertimeReasonCombo(reloadFromDB);
            FillOvertimeFilterOptionCombo(reloadFromDB, UIHelper.UDCSorterColumn.UDCSequenceNo, UIHelper.OvertimeFilter.OTUNPROCSD.ToString());
            FillCostCenterCombo(reloadFromDB);
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
        private void GetOvertimeRecord(bool reloadDataFromDB = false)
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
                if (costCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    costCenter = string.Empty;

                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                UIHelper.OvertimeFilter dataFilterType = (UIHelper.OvertimeFilter)Enum.Parse(typeof(UIHelper.OvertimeFilter), this.cboFilterOption.SelectedValue);                

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridSearchResults.VirtualItemCount = 1;

                // Reset session variables
                ViewState["IsOTApprove"] = null;
                ViewState["IsOTApprovalHeaderClicked"] = null;
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
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetOvertimeByPeriod(userEmpNo, Convert.ToByte(dataFilterType), startDate, endDate, costCenter, empNo, this.OTRequestNoParam,
                        this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
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

                //Bind data to the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillCostCenterCombo(bool reloadFromDB = true)
        {
            try
            {
                // Initialize controls
                this.cboCostCenter.Items.Clear();
                this.cboCostCenter.Text = string.Empty;
                this.btnFindEmployee.Enabled = false;

                List<CostCenterEntity> comboSource = new List<CostCenterEntity>();
                if (this.CostCenterList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.CostCenterList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                    DALProxy proxy = new DALProxy();

                    var rawData = proxy.GetCostCenterOTAllowed(userEmpNo, ref error, ref innerError);
                    if (rawData != null)
                    {
                        comboSource.AddRange(rawData.ToList());

                        if (comboSource.Count > 0)
                        {
                            #region Add blank item
                            comboSource.Insert(0, new CostCenterEntity()
                            {
                                CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                                CostCenterName = string.Empty,
                                CostCenterFullName = string.Empty
                            });
                            #endregion
                        }

                        // Store to session
                        this.CostCenterList = comboSource;
                    }
                }

                if (this.CostCenterList.Count > 0)
                {
                    this.cboCostCenter.DataSource = this.CostCenterList;
                    this.cboCostCenter.DataTextField = "CostCenterFullName";
                    this.cboCostCenter.DataValueField = "CostCenter";
                    this.cboCostCenter.DataBind();

                    // Enable employee search button 
                    this.btnFindEmployee.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillOvertimeReasonCombo(bool reloadFromDB = true, byte loadType = 0)
        {
            try
            {
                List<UDCEntity> comboSource = new List<UDCEntity>();
                if (this.OTReasonList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.OTReasonList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetOvertimeReasons(loadType, ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        comboSource.AddRange(source.ToList());
                    }
                }

                // Store to session
                this.OTReasonList = comboSource;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillOvertimeFilterOptionCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.OvertimeFilterOptionList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.OvertimeFilterOptionList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetUserDefinedCode(UIHelper.UDCGroupCodes.TSOTFILTER.ToString(), ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        rawData.AddRange(source.ToList());

                        // Add blank item
                        //rawData.Insert(0, new UserDefinedCodes() { UDCDesc1 = string.Empty, UDCCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
                    }
                }

                #region Sort the list
                if (rawData != null && rawData.Count() > 0)
                {
                    switch (sorter)
                    {
                        case UIHelper.UDCSorterColumn.UDCDesc1:
                            comboSource.AddRange(rawData.OrderBy(o => o.UDCDesc1).ToList());
                            break;

                        case UIHelper.UDCSorterColumn.UDCDesc2:
                            comboSource.AddRange(rawData.OrderBy(o => o.UDCDesc2).ToList());
                            break;

                        case UIHelper.UDCSorterColumn.UDCID:
                            comboSource.AddRange(rawData.OrderBy(o => o.UDCID).ToList());
                            break;

                        case UIHelper.UDCSorterColumn.UDCSequenceNo:
                            comboSource.AddRange(rawData.OrderBy(o => o.UDCAmount).ToList());
                            break;
                    }
                }
                #endregion

                // Store to session
                this.OvertimeFilterOptionList = comboSource;

                #region Bind data to combobox
                this.cboFilterOption.DataSource = comboSource;
                this.cboFilterOption.DataTextField = "UDCDesc1";
                this.cboFilterOption.DataValueField = "UDCCode";
                this.cboFilterOption.DataBind();

                if (this.cboFilterOption.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboFilterOption.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveOvertime(List<EmployeeAttendanceEntity> attendanceList, ref string errorMsg)
        {
            try
            {
                if (attendanceList.Count == 0)
                    return;

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int autoID = 0;
                string otReasonCode = null;
                string comment = null;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string userEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                string otApprovalCode = null;
                string mealVoucherApprovalCode = null;
                int otDuration = 0;
                StringBuilder sbError = new StringBuilder();
                Dictionary<long, int> autoClosedOTList = new Dictionary<long, int>();
                #endregion

                #region Save to database
                int recordCounter = 0;
                foreach (EmployeeAttendanceEntity item in attendanceList)
                {
                    autoID = item.AutoID;
                    otReasonCode = item.OTReasonCode;
                    comment = item.AttendanceRemarks;
                    otApprovalCode = item.OTApprovalCode;
                    mealVoucherApprovalCode = item.MealVoucherEligibilityCode;
                    otDuration = UIHelper.ConvertObjectToInt(item.OTDurationMinute);

                    DatabaseSaveResult dbResult = proxy.SaveEmployeeOvertimeByClerk(autoID, otReasonCode, comment, userEmpNo, userEmpName, userID, otApprovalCode, mealVoucherApprovalCode, otDuration, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || 
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }                                        
                    else
                    {
                        if (dbResult != null && 
                            dbResult.HasError)
                        {
                            if (!string.IsNullOrEmpty(dbResult.ErrorDesc))
                            {
                                if (sbError.Length == 0)
                                {
                                    sbError.AppendLine(string.Format("Could not save overtime request for Employee No. {0} on {1} due to the following error: {2}",
                                           item.EmpNo,
                                           Convert.ToDateTime(item.DT).ToString("dd-MMM-yyyy"),
                                           dbResult.ErrorDesc));
                                }
                                else
                                {
                                    sbError.AppendLine(string.Format("<br /> Could not save overtime request for Employee No. {0} on {1} due to the following error: {2}",
                                           item.EmpNo,
                                           Convert.ToDateTime(item.DT).ToString("dd-MMM-yyyy"),
                                           dbResult.ErrorDesc));
                                }
                            }
                        }
                        else
                        {
                            recordCounter++;

                            #region Add to the collection if the workflow of the OT request has been closed automatically
                            if (dbResult.IsWorkflowCompleted)
                            {
                                autoClosedOTList.Add(dbResult.OTRequestNo, item.EmpNo);
                            }
                            #endregion

                            #region Refresh the collection
                            EmployeeAttendanceEntity itemToRemove = this.AttendanceList
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (itemToRemove != null)
                                this.AttendanceList.Remove(itemToRemove);
                            #endregion
                        }
                    }
                }

                if (sbError.Length > 0)
                    errorMsg = sbError.ToString().Trim();
                #endregion

                #region Disable sending of system notification to the approver upon submission
                if (autoClosedOTList.Count > 0)
                {
                    // View the approved requisitions
                    this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTAPPROVED.ToString();
                    this.cboFilterOption_SelectedIndexChanged(this.cboFilterOption, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFilterOption.Text, string.Empty, this.cboFilterOption.SelectedValue, string.Empty));
                }
                else
                {
                    // View the submitted requisitions
                    this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTSUBMITED.ToString();
                    this.cboFilterOption_SelectedIndexChanged(this.cboFilterOption, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFilterOption.Text, string.Empty, this.cboFilterOption.SelectedValue, string.Empty));
                }
                #endregion

                #region Initiate the workflow, send notification to the first approver (Note: Sending of system notification to the approver has been commented as per Helpdesk No. 85517)
                //int noOfRequisition = 0;
                //if (ProcessWorkflowEmail(ref noOfRequisition))
                //{
                //    if (noOfRequisition > 0)
                //    {
                //        // View the submitted requisitions
                //        this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTSUBMITED.ToString();
                //        this.cboFilterOption_SelectedIndexChanged(this.cboFilterOption, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFilterOption.Text, string.Empty, this.cboFilterOption.SelectedValue, string.Empty));
                //    }
                //    else
                //    {
                //        if (autoClosedOTList.Count > 0)
                //        {
                //            // View the approved requisitions
                //            this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTAPPROVED.ToString();
                //            this.cboFilterOption_SelectedIndexChanged(this.cboFilterOption, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFilterOption.Text, string.Empty, this.cboFilterOption.SelectedValue, string.Empty));
                //        }
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            finally
            {
                this.ReloadGridData = false;
            }
        }

        private void CancelOvertimeRequest(EmployeeAttendanceEntity selectedOTRecord)
        {
            try
            {
                if (selectedOTRecord == null)
                    return;

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;                
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string userEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                #endregion

                #region Cancel record in the database
                DatabaseSaveResult dbResult = proxy.ManageOvertimeRequest(1, selectedOTRecord.OTRequestNo, userEmpNo, userEmpName, userID, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                #endregion

                this.ReloadGridData = true;
                this.btnSearch_Click(this.btnSearch, new EventArgs());
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
            finally
            {
                this.ReloadGridData = false;
            }
        }
        #endregion

        #region Workflow Methods
        private bool ProcessWorkflowEmail(ref int noOfRequisition)
        {
            try
            {
                #region Initialize variables                                
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                StringBuilder sb = new StringBuilder();
                string emailBody = string.Empty;
                int counter = 0;
                int createdByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string createdByUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                string originatorEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]);
                string originatorName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                bool useMultithread = UIHelper.ConvertNumberToBolean(ConfigurationManager.AppSettings["UseMultithread"]);
                string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_OVERTIME_APPROVAL.Replace("~", string.Empty));
                #endregion

                var rawData = proxy.GetWFEmailDueForDelivery(1, createdByEmpNo, 0, null, null, ref error, ref innerError);
                if (rawData != null)
                {
                    #region Loop through each assignee to send the system notification                                        
                    List<WorkflowEmailDeliveryEntity> recipientList = rawData.ToList();
                    foreach (WorkflowEmailDeliveryEntity recipient in recipientList)
                    {                        
                        var rawData2 = proxy.GetWFEmailDueForDelivery(2, createdByEmpNo, recipient.CurrentlyAssignedEmpNo, null, null, ref error, ref innerError);
                        if (rawData2 != null)
                        {
                            List<WorkflowEmailDeliveryEntity> emailDeliveryList = rawData2.ToList();
                            if (emailDeliveryList.Count > 0)
                            {
                                // Reset variables
                                emailBody = string.Empty;
                                counter = 1;
                                sb.Clear();

                                string otApprovalDesc = string.Empty;
                                string mealVoucherApprovalDesc = string.Empty;

                                #region Send email to each approver
                                foreach (WorkflowEmailDeliveryEntity item in emailDeliveryList)
                                {
                                    #region Build the email content
                                    if (item.OTApproved == "Y")
                                        otApprovalDesc = "Yes";
                                    else if (item.OTApproved == "N")
                                        otApprovalDesc = "No";
                                    else
                                        otApprovalDesc = "No action";

                                    if (item.MealVoucherEligibility == "YA")
                                        mealVoucherApprovalDesc = "Yes";
                                    else if (item.MealVoucherEligibility == "N")
                                        mealVoucherApprovalDesc = "No";
                                    else
                                        mealVoucherApprovalDesc = "No action";

                                    if (sb.Length > 0)
                                        sb.AppendLine(@"<br />");

                                    sb.AppendLine(string.Format(@"{0}. " +
                                                    "<b>Requisition No.:</b> {1}; " +
                                                    "<b>Employee Name:</b> {2}; " +
                                                    "<b>Position:</b> {3}; " +
                                                    "<b>Cost Center:</b> {4}; " +
                                                    "<b>Date:</b> {5}; " +
                                                    "<b>Meal Voucher Approved?:</b> {6}; " +
                                                    "<b>OT Approved?:</b> {7}; " +
                                                    "<b>OT Start Time:</b>" + "<font color=" + "red" + ">" + " {8}</font>; " +
                                                    "<b>OT End Time:</b>" + "<font color=" + "red" + ">" + " {9}</font>; " +
                                                    //"<b>OT Type:</b> {10}; " +
                                                    "<b>Remarks:</b> {10}",
                                               counter,
                                               item.OTRequestNo,
                                               item.EmpFullName,
                                               !string.IsNullOrEmpty(item.Position) ? item.Position : "Not defined",
                                               item.CostCenterFullName,
                                               item.DT.HasValue ? UIHelper.ConvertObjectToDateString(item.DT) : "Not defined",
                                               mealVoucherApprovalDesc,
                                               otApprovalDesc,                                               
                                               item.OTStartTime.HasValue ? UIHelper.ConvertObjectToTimeString(item.OTStartTime) : "-",
                                               item.OTEndTime.HasValue ? UIHelper.ConvertObjectToTimeString(item.OTEndTime) : "-",
                                               item.OTComment));

                                    sb.AppendLine(@"<br />");
                                    #endregion

                                    #region Set the last update info
                                    item.LastUpdateEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                    item.LastUpdateEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                                    item.LastUpdateTime = DateTime.Now;
                                    #endregion

                                    counter++;
                                }

                                if (sb.Length > 0)
                                    emailBody = sb.ToString().Trim();

                                if (!string.IsNullOrEmpty(emailBody))
                                {
                                    if (useMultithread)
                                    {
                                        // Send email in separate thread                                        
                                        Task.Factory.StartNew(() => SendEmailToApprover(originatorName, originatorEmail, recipient, emailBody, dynamicEndpointAddress, createdByEmpNo, createdByUserID, true));
                                    }
                                    else
                                    {
                                        SendEmailToApprover(originatorName, originatorEmail, recipient, emailBody, dynamicEndpointAddress, createdByEmpNo, createdByUserID);
                                    }
                                }
                                #endregion

                                #region Close the email delivery records                                                                
                                error = innerError = string.Empty;
                                proxy.CloseEmailDelivery(emailDeliveryList, ref error, ref innerError);
                                if (!string.IsNullOrEmpty(error))
                                {
                                    throw new Exception(error);
                                }
                                #endregion

                                noOfRequisition++;
                            }
                        }                        
                    }
                    #endregion
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Email Communications       
        private void SendEmailToApprover(string originatorName, string originatorEmail, WorkflowEmailDeliveryEntity emailData, string emailBody, string dynamicEndpointAddress, int userEmpNo, string userID, bool isMultithread = false)
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
                    return;

                //Check the collection
                if (emailData == null)
                    return;
                #endregion

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                int retError = 0;
                string errorMsg = string.Empty;
                string error = string.Empty;
                string innerError = string.Empty;
                string recipientEmail = string.Empty;
                string recipientName = string.Empty;
                //EmployeeInfo empInfo = new EmployeeInfo();
                string distListCode = string.Empty;
                EmployeeDetail empInfo = new EmployeeDetail();
                #endregion

                #region Set the From, Subject, and primary recipients
                string adminAlias = ConfigurationManager.AppSettings["AdminEmailAlias"];
                MailAddress from = new MailAddress(ConfigurationManager.AppSettings["AdminEmail"], !string.IsNullOrEmpty(adminAlias) ? adminAlias : "TAS Admin");
                string subject = "TAS - Overtime Online Approval";
                #endregion

                #region Set the Mail Recipients
                List<MailAddress> toList = null;
                List<MailAddress> ccList = null;
                List<MailAddress> bccList = null;

                #region Set the To recipients
                // Initialize the collection
                toList = new List<MailAddress>();

                if (!string.IsNullOrEmpty(emailData.CurrentlyAssignedEmpEmail) &&
                    !string.IsNullOrEmpty(emailData.CurrentlyAssignedEmpName))
                {
                    toList.Add(new MailAddress(emailData.CurrentlyAssignedEmpEmail, UIHelper.ConvertStringToTitleCase(emailData.CurrentlyAssignedEmpName)));
                    recipientName = UIHelper.ConvertStringToTitleCase(emailData.CurrentlyAssignedEmpName);
                }
                else
                {
                    if (emailData.CurrentlyAssignedEmpNo > 0)
                    {
                        var rawData = proxy.GetEmployeeEmailInfo(emailData.CurrentlyAssignedEmpNo, string.Empty, ref error, ref innerError);
                        if (rawData != null)
                        {
                            empInfo = rawData.FirstOrDefault();
                            if (empInfo != null &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                toList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                                recipientName = UIHelper.ConvertStringToTitleCase(empInfo.EmpName);
                            }
                        }
                    }
                }
                #endregion

                #region Set the Cc Recipients
                if (emailData.EmailCCRecipientType == Convert.ToInt32(UIHelper.EmailRecipientType.BuiltinGroup))
                {
                    #region Get the built-in group member
                    distListCode = emailData.EmailCCRecipient;
                    if (!string.IsNullOrEmpty(distListCode))
                    {
                        var rawData = proxy.GetWorkflowActionMember(emailData.CurrentlyAssignedEmpNo, distListCode, emailData.CostCenter, ref error, ref innerError);
                        if (rawData != null)
                        {
                            // Initialize collection
                            ccList = new List<MailAddress>();

                            foreach (EmployeeDetail emp in rawData.ToList())
                            {
                                if (string.IsNullOrEmpty(emp.EmpEmail))
                                {
                                    if (emp.EmpNo > 0)
                                    {
                                        var rawEmail = proxy.GetEmployeeEmailInfo(emp.EmpNo, string.Empty, ref error, ref innerError);
                                        if (rawEmail != null)
                                        {
                                            empInfo = rawEmail.FirstOrDefault();
                                            if (empInfo != null &&
                                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                                            {
                                                ccList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ccList.Add(new MailAddress(emp.EmpEmail, UIHelper.ConvertStringToTitleCase(emp.EmpName)));
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (emailData.EmailCCRecipientType == Convert.ToInt32(UIHelper.EmailRecipientType.DistributionList))
                {
                    #region Get the built-in group member
                    distListCode = emailData.EmailCCRecipient;
                    if (!string.IsNullOrEmpty(distListCode))
                    {
                        var rawData = proxy.GetWorkflowActionMember(0, distListCode, "ALL", ref error, ref innerError);
                        if (rawData != null)
                        {
                            // Initialize collection
                            ccList = new List<MailAddress>();

                            foreach (EmployeeDetail emp in rawData.ToList())
                            {
                                if (string.IsNullOrEmpty(emp.EmpEmail))
                                {
                                    if (emp.EmpNo > 0)
                                    {
                                        var rawEmail = proxy.GetEmployeeEmailInfo(emp.EmpNo, string.Empty, ref error, ref innerError);
                                        if (rawEmail != null)
                                        {
                                            empInfo = rawEmail.FirstOrDefault();
                                            if (empInfo != null &&
                                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                                            {
                                                ccList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ccList.Add(new MailAddress(emp.EmpEmail, UIHelper.ConvertStringToTitleCase(emp.EmpName)));
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (emailData.EmailCCRecipientType == Convert.ToInt32(UIHelper.EmailRecipientType.IndividualEmployee))
                {
                    #region Individual employee email
                    if (!string.IsNullOrEmpty(emailData.EmailCCRecipient))
                    {
                        // Initialize collection
                        ccList = new List<MailAddress>();

                        string[] emailArray = emailData.EmailCCRecipient.Split(';');
                        if (emailArray != null && emailArray.Count() > 0)
                        {
                            foreach (string emailAddress in emailArray)
                            {
                                if (!string.IsNullOrEmpty(emailAddress))
                                    ccList.Add(new MailAddress(emailAddress, emailAddress));
                            }
                        }
                    }
                    #endregion
                }

                if (ccList == null)
                    ccList = new List<MailAddress>();

                ccList.Add(new MailAddress(originatorEmail, UIHelper.ConvertStringToTitleCase(originatorName)));
                #endregion

                #region Set the Bcc recipients (For tracking purpose)
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AdminBCCRecipients"]))
                {
                    string[] recipients = ConfigurationManager.AppSettings["AdminBCCRecipients"].Split(',');
                    if (recipients != null && recipients.Count() > 0)
                    {
                        bccList = new List<MailAddress>();
                        foreach (string recipient in recipients)
                        {
                            if (recipient.Length > 0)
                                bccList.Add(new MailAddress(recipient, recipient));
                        }
                    }
                }
                #endregion

                #endregion

                // Exit if Mail-to recipient is null
                //if (toList == null || toList.Count == 0)
                //    return false;

                #region Build URL address
                //string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                //        UIHelper.PAGE_OVERTIME_APPROVAL.Replace("~", string.Empty));

                string queryString = string.Format("?IsAssignedKey={0}", true.ToString());

                StringBuilder url = new StringBuilder();
                url.Append(string.Concat(dynamicEndpointAddress, queryString.Trim()));
                #endregion

                #region Set Message Body
                string body = String.Empty;
                string htmLBody = string.Empty;
                string appPath = Server.MapPath(UIHelper.CONST_APPROVER_EMAIL_TEMPLATE);
                string adminName = ConfigurationManager.AppSettings["AdminName"];

                // Build the message body
                body = String.Format(UIHelper.RetrieveXmlMessage(appPath),
                    recipientName,
                    emailBody,
                    url.ToString().Trim(),
                    adminName
                    ).Replace("&lt;", "<").Replace("&gt;", ">");

                // Format the message contents
                htmLBody = string.Format("<HTML><BODY><p>{0}</p></BODY></HTML>", body);
                #endregion

                #region Create attachment
                List<Attachment> attachmentList = null;
                #endregion

                #region Send the e-mail
                if (!string.IsNullOrEmpty(htmLBody))
                {
                    retError = 0;
                    errorMsg = string.Empty;
                    SendEmail(toList, ccList, bccList, from, subject, htmLBody, attachmentList, mailServer, ref errorMsg, ref retError);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        throw new Exception(errorMsg);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (isMultithread)
                {
                    DALProxy proxy = new DALProxy();
                    proxy.InsertSystemErrorLog(Convert.ToByte(UIHelper.SaveType.Insert), 0, emailData.OTRequestNo, Convert.ToByte(UIHelper.SystemErrorCode.MultithreadingError), ex.Message.ToString(), userEmpNo, userID);
                }
                else
                    throw ex;
            }
        }

        private void SendEmail(List<MailAddress> toList, List<MailAddress> ccList, List<MailAddress> bccList, MailAddress from,
            string subject, string body, List<Attachment> attachmentList, string smtpConn, ref string errorMsg, ref int retError)
        {
            errorMsg = String.Empty;
            retError = 0;

            try
            {
                bool isTestMode = UIHelper.ConvertNumberToBolean(ConfigurationManager.AppSettings["EmailTestMode"]);
                int indexLoc = 0;
                string newEmailAddress = string.Empty;

                // Create an email object
                MailMessage email = new MailMessage();

                #region Add all the recipients and originator
                if (toList != null)
                {
                    foreach (MailAddress to in toList)
                    {
                        if (isTestMode)
                        {
                            #region Append underscore to the email address if in test mode
                            if (!string.IsNullOrEmpty(to.Address))
                            {
                                indexLoc = to.Address.IndexOf("@");
                                if (indexLoc > 0)
                                {
                                    newEmailAddress = to.Address.Replace(to.Address.Substring(indexLoc + 1),
                                        string.Concat("_", to.Address.Substring(indexLoc + 1)));

                                    // Add email address
                                    email.To.Add(new MailAddress(newEmailAddress, to.DisplayName));
                                }
                                else
                                    email.To.Add(to);
                            }
                            #endregion
                        }
                        else
                            email.To.Add(to);
                    }
                }

                if (ccList != null)
                {
                    foreach (MailAddress cc in ccList)
                    {
                        if (isTestMode)
                        {
                            #region Append underscore to the email address if in test mode
                            if (!string.IsNullOrEmpty(cc.Address))
                            {
                                indexLoc = cc.Address.IndexOf("@");
                                if (indexLoc > 0)
                                {
                                    newEmailAddress = cc.Address.Replace(cc.Address.Substring(indexLoc + 1),
                                        string.Concat("_", cc.Address.Substring(indexLoc + 1)));

                                    // Add email address
                                    email.CC.Add(new MailAddress(newEmailAddress, cc.DisplayName));
                                }
                                else
                                    email.CC.Add(cc);
                            }
                            #endregion
                        }
                        else
                            email.CC.Add(cc);
                    }
                }

                if (bccList != null)
                {
                    foreach (MailAddress bcc in bccList)
                    {
                        email.Bcc.Add(bcc);
                    }
                }

                email.From = from;
                #endregion

                #region Set the subject and body
                // Deserialize the subject
                RadEditor txtStorage = new RadEditor();
                txtStorage.Content = subject;
                email.Subject = txtStorage.Text.Trim();

                StringBuilder bodyList = new StringBuilder();
                bodyList.Append("<div style='font-family: Tahoma; font-size: 10pt'>");
                bodyList.Append(body);
                bodyList.Append("</div>");
                email.Body = bodyList.ToString();
                email.IsBodyHtml = true;
                #endregion

                #region Add attachments
                if (attachmentList != null)
                {
                    foreach (Attachment attach in attachmentList)
                        email.Attachments.Add(attach);
                }
                #endregion

                // Create an smtp client and send the mail message
                SmtpClient smtpClient = new SmtpClient(smtpConn);
                smtpClient.UseDefaultCredentials = true;

                // Send the mail message
                smtpClient.Send(email);

            }

            catch (Exception error)
            {
                errorMsg = error.Message;
                retError = -1;
            }
        }
        #endregion
               
    }
}