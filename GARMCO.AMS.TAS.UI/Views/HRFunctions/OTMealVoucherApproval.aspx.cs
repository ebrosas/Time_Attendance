using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.DAL.Employee;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class OTMealVoucherApproval : BaseWebForm, IFormExtension
    {
        #region Private Data Members
        private RadGrid _gridTimesheet = null;
        #endregion

        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoStartDate,
            NoDataFilterOption,
            InvalidDateRange,
            InvalidYear
        }

        private enum FetchOTBudgetStatisticType
        {
            GetOTBudgetAmount,
            GetOTActualAmount,
            GetOTTotalBudgetAmount,
            GetOTTotalActualAmount,
            GetOTTotalBudgetAndActualAmount,
            GetFiscalYearList,
            GetOTBudgetBreakdownByCostCenter,
            GetOTActualBreakdownByCostCenter,
            GetAllCostCenterByFiscalYear,
            GetOTBudgetAndActualHours,
            GetOTBudgetBreakdownByHour,
            GetOTActualBreakdownByHour,
            GetOTBudgetBreakdownByCostCenterHour,
            GetOTActualBreakdownByCostCenterHour
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

        private Dictionary<string, object> OvertimeApprovalStorage
        {
            get
            {
                Dictionary<string, object> list = Session["OvertimeApprovalStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["OvertimeApprovalStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["OvertimeApprovalStorage"] = value;
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

        private List<EmployeeAttendanceEntity> OTRequisitionList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["OTRequisitionList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["OTRequisitionList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["OTRequisitionList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> OTRequisitionListOrig
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["OTRequisitionListOrig"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["OTRequisitionListOrig"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["OTRequisitionListOrig"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> OTRequisitionApprovalList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["OTRequisitionApprovalList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["OTRequisitionApprovalList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["OTRequisitionApprovalList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> CheckedOTRequisitionList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["CheckedOTRequisitionList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["CheckedOTRequisitionList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedOTRequisitionList"] = value;
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

        private bool IsOTWFApprove
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTWFApprove"]);
            }
            set
            {
                ViewState["IsOTWFApprove"] = value;
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

        private bool IsOTWFApprovalHeaderClicked
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTWFApprovalHeaderClicked"]);
            }
            set
            {
                ViewState["IsOTWFApprovalHeaderClicked"] = value;
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

        private bool IsHRValidator
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsHRValidator"]);
            }
            set
            {
                ViewState["IsHRValidator"] = value;
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

        private List<OvertimeBudgetEntity> FiscalYearComboList
        {
            get
            {
                List<OvertimeBudgetEntity> list = ViewState["FiscalYearComboList"] as List<OvertimeBudgetEntity>;
                if (list == null)
                    ViewState["FiscalYearComboList"] = list = new List<OvertimeBudgetEntity>();

                return list;
            }
            set
            {
                ViewState["FiscalYearComboList"] = value;
            }
        }

        private bool IsOTBudgetAdmin
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTBudgetAdmin"]);
            }
            set
            {
                ViewState["IsOTBudgetAdmin"] = value;
            }
        }

        private OvertimeBudgetEntity OvertimeBudgetData
        {
            get
            {
                return ViewState["OvertimeBudgetData"] as OvertimeBudgetEntity;
            }
            set
            {
                ViewState["OvertimeBudgetData"] = value;
            }
        }

        private OvertimeBudgetEntity OvertimeActualsData
        {
            get
            {
                return ViewState["OvertimeActualsData"] as OvertimeBudgetEntity;
            }
            set
            {
                ViewState["OvertimeActualsData"] = value;
            }
        }

        private List<OvertimeBudgetEntity> OvertimeCostCenterList
        {
            get
            {
                List<OvertimeBudgetEntity> list = ViewState["OvertimeCostCenterList"] as List<OvertimeBudgetEntity>;
                if (list == null)
                    ViewState["OvertimeCostCenterList"] = list = new List<OvertimeBudgetEntity>();

                return list;
            }
            set
            {
                ViewState["OvertimeCostCenterList"] = value;
            }
        }

        private List<OvertimeBudgetEntity> OvertimeBudgetList
        {
            get
            {
                List<OvertimeBudgetEntity> list = ViewState["OvertimeBudgetList"] as List<OvertimeBudgetEntity>;
                if (list == null)
                    ViewState["OvertimeBudgetList"] = list = new List<OvertimeBudgetEntity>();

                return list;
            }
            set
            {
                ViewState["OvertimeBudgetList"] = value;
            }
        }

        private List<OvertimeBudgetEntity> OvertimeActualList
        {
            get
            {
                List<OvertimeBudgetEntity> list = ViewState["OvertimeActualList"] as List<OvertimeBudgetEntity>;
                if (list == null)
                    ViewState["OvertimeActualList"] = list = new List<OvertimeBudgetEntity>();

                return list;
            }
            set
            {
                ViewState["OvertimeActualList"] = value;
            }
        }

        private List<string> AllocatedCostCenterList
        {
            get
            {
                List<string> list = ViewState["AllocatedCostCenterList"] as List<string>;
                if (list == null)
                    ViewState["AllocatedCostCenterList"] = list = new List<string>();

                return list;
            }
            set
            {
                ViewState["AllocatedCostCenterList"] = value;
            }
        }

        private bool CanViewOTStatistic
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["CanViewOTStatistic"]);
            }
            set
            {
                ViewState["CanViewOTStatistic"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.OTAPPROVE.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_OVERTIME_APPROVAL_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMPLOYEE_OVERTIME_APPROVAL_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Enabled = this.Master.IsEditAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.OvertimeApprovalStorage.Count > 0)
                {
                    if (this.OvertimeApprovalStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        string originatorButton = this.OvertimeApprovalStorage.ContainsKey("SourceControl")
                           ? UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["SourceControl"]) : string.Empty;

                        switch (originatorButton)
                        {
                            case "btnFindEmployee":
                                this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;

                            case "btnFindAssignee":
                                this.txtAssigneeEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;
                        }
                    }

                    // Clear data storage
                    Session.Remove("OvertimeApprovalStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("OvertimeApprovalStorage");

                    // Check if need to refresh data in the grid
                    if (this.ReloadGridData)
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();

                    #region Initialize system flags

                    DALProxy proxy = new DALProxy();
                    string error = string.Empty;
                    string innerError = string.Empty;
                    int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    #region Check if current user is the HR Validator
                    bool? isHrApprover = proxy.CheckIfHRApprover(currentUserEmpNo, ref error, ref innerError);
                    if (isHrApprover.HasValue)
                        this.IsHRValidator = Convert.ToBoolean(isHrApprover);
                    else
                        this.IsHRValidator = false;

                    //var rawData = proxy.GetWorkflowActionMember(0, UIHelper.DistributionGroupCodes.OTHRVALIDR.ToString(), string.Empty, ref error, ref innerError);
                    //if (rawData != null)
                    //{
                    //    EmployeeDetail hrValidator = rawData
                    //        .Where(a => a.EmpNo == currentUserEmpNo)
                    //        .FirstOrDefault();
                    //    this.IsHRValidator = hrValidator != null;
                    //}
                    #endregion

                    #region Check if current user is member of OT Budget Administrator group
                    error = innerError = string.Empty;
                    this.IsOTBudgetAdmin = UIHelper.ConvertObjectToBolean(proxy.IsOTBudgetAdmin(currentUserEmpNo, ref error, ref innerError));
                    #endregion

                    #region Get the allocated cost centers wherein the employee is either the Superintendent or CC Manager
                    error = innerError = string.Empty;
                    this.AllocatedCostCenterList = proxy.GetAllocatedCostCenter(currentUserEmpNo, ref error, ref innerError);
                    #endregion

                    #endregion

                    FillComboData();

                    #region Initialize controls
                    //this.chkPayPeriod.Checked = true;
                    //this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

                    //int month = DateTime.Now.Month;
                    //if (DateTime.Now.Day >= 16)
                    //    month = month + 1;

                    //this.txtYear.Text = DateTime.Now.Year.ToString();
                    //this.cboMonth.SelectedValue = month.ToString();
                    //this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                    //this.cboMonth.Focus();

                    this.rblAssignedTo.SelectedValue = "1";     // Me
                    this.rblAssignedTo_SelectedIndexChanged(this.rblAssignedTo, new EventArgs());

                    // Show the 12-hour filter option for HR and System Administrators only
                    this.tblShow12HourShift.Style[HtmlTextWriterStyle.Display] = this.IsHRValidator || this.Master.IsSystemAdmin ? string.Empty : "none";
                    #endregion
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Parent Grid Events
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
            if (this.OTRequisitionList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.OTRequisitionList;
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
                                this.OTRequisitionList.Count > 0)
                            {
                                EmployeeAttendanceEntity selectedOTRecord = this.OTRequisitionList
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
                            #region "View History" link button 
                            // Save session values
                            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                            // Get the data key value
                            int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));

                            if (autoID > 0 &&
                                this.OTRequisitionList.Count > 0)
                            {
                                EmployeeAttendanceEntity selectedOTRecord = this.OTRequisitionList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedOTRecord != null)
                                {
                                    // Save the currently selected record
                                    Session["CurrentOvertimeRequest"] = selectedOTRecord;

                                    Response.Redirect
                                    (
                                        String.Format(UIHelper.PAGE_OVERTIME_APPROVAL_HISTORY + "?{0}={1}",
                                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                                        UIHelper.PAGE_OVERTIME_APPROVAL
                                    ),
                                    false);
                                }
                            }
                            #endregion
                        }
                        else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewAttendanceLinkButton"].Controls[0] as LinkButton).Text.Trim())
                        {
                            #region "View Attendance" link button 
                            // Save session values
                            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                            // Get the data key value
                            int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));

                            if (autoID > 0 &&
                                this.OTRequisitionList.Count > 0)
                            {
                                EmployeeAttendanceEntity selectedOTRecord = this.OTRequisitionList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedOTRecord != null &&
                                    selectedOTRecord.DT != null)
                                {
                                    DateTime? startDate = null;
                                    DateTime? endDate = null;
                                    string costCenter = selectedOTRecord.CostCenter;
                                    string costCenterFullName = selectedOTRecord.CostCenterFullName;
                                    int empNo = selectedOTRecord.EmpNo;

                                    int monthNum = selectedOTRecord.DT.Value.Month;
                                    if (selectedOTRecord.DT.Value.Day >= 16)
                                        monthNum = monthNum + 1;

                                    int yearNum = selectedOTRecord.DT.Value.Year;
                                    if (monthNum > 12)
                                    {
                                        monthNum = 1;
                                        yearNum = yearNum + 1;
                                    }

                                    #region Set the payroll period
                                    switch (monthNum)
                                    {
                                        case 1: // January
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/12/{0}", yearNum - 1));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/01/{0}", yearNum));
                                            break;

                                        case 2: // Feburary
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/01/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/02/{0}", yearNum));
                                            break;

                                        case 3: // March
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/02/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/03/{0}", yearNum));
                                            break;

                                        case 4: // April
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/03/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/04/{0}", yearNum));
                                            break;

                                        case 5: // May
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/04/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/05/{0}", yearNum));
                                            break;

                                        case 6: // June
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/05/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/06/{0}", yearNum));
                                            break;

                                        case 7: // July
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/06/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/07/{0}", yearNum));
                                            break;

                                        case 8: // August
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/07/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/08/{0}", yearNum));
                                            break;

                                        case 9: // September
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/08/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/09/{0}", yearNum));
                                            break;

                                        case 10: // October
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/09/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/10/{0}", yearNum));
                                            break;

                                        case 11: // November
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/10/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/11/{0}", yearNum));
                                            break;

                                        case 12: // December
                                            startDate = UIHelper.ConvertObjectToDate(string.Format("16/11/{0}", yearNum));
                                            endDate = UIHelper.ConvertObjectToDate(string.Format("15/12/{0}", yearNum));
                                            break;
                                    }
                                    #endregion

                                    if (startDate.HasValue && endDate.HasValue)
                                    {
                                        List<EmployeeAttendanceEntity> attendanceList = GetAttendanceHistory(startDate, endDate, costCenter, empNo);
                                        if (attendanceList != null)
                                        {
                                            // Save report data to session
                                            Session["EmpAttendanceHistoryReportSource"] = attendanceList;

                                            // Show the report with workplace swipes information
                                            Response.Redirect
                                            (
                                                String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}",
                                                UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                                                UIHelper.ReportTypes.EmployeeAttendanceHistoryReport.ToString(),
                                                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                                                UIHelper.PAGE_OVERTIME_APPROVAL,
                                                UIHelper.QUERY_STRING_COSTCENTER_KEY,
                                                string.Format("Cost Center: {0}", Server.UrlEncode(costCenterFullName)),
                                                UIHelper.QUERY_STRING_STARTDATE_KEY,
                                                Convert.ToDateTime(startDate).Date.ToString(),
                                                UIHelper.QUERY_STRING_ENDDATE_KEY,
                                                Convert.ToDateTime(endDate).Date.ToString()
                                            ),
                                            false);
                                        }
                                    }
                                    else
                                        DisplayFormLevelError("Unable to view the attendance report because the payroll period could not be determined!");
                                }
                            }
                            #endregion
                        }
                        else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                        {
                            #region "Edit" link button
                            //// Enable "OT Approved" field
                            //RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                            //if (cboOTApprovalType != null)
                            //    cboOTApprovalType.Enabled = true;

                            //// Enable "Meal VoucherApproved" field
                            //RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            //if (cboMealVoucherEligibility != null)
                            //    cboMealVoucherEligibility.Enabled = true;

                            //// Enable "OT Duration" field
                            //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            //if (txtDuration != null)
                            //    txtDuration.Enabled = true;

                            //// Enable "OT Reason" field
                            //RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                            //if (cboOTReason != null)
                            //    cboOTReason.Enabled = true;

                            //// Enable "Remarks" field
                            //TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                            //if (txtRemarks != null)
                            //    txtRemarks.Enabled = true;

                            //// Select the row
                            //item.Selected = true;

                            //// Toggle the link title
                            //if ((item["EditLinkButton"].Controls[0] as LinkButton).Text == "Edit")
                            //    (item["EditLinkButton"].Controls[0] as LinkButton).Text = "Cancel";
                            //else
                            //    (item["EditLinkButton"].Controls[0] as LinkButton).Text = "Edit";
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
                RebindDataToGrid();

                #region Initialize grid columns for export
                this.gridSearchResults.MasterTableView.GetColumn("DT").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("EmpNo").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("GradeCode").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTType").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("AttendanceRemarks").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("CurrentlyAssignedFullName").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ShiftPatCode").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ShiftCode").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ActualShiftCode").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTRequestNo").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("CostCenter").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("CheckboxSelectColumn").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ViewHistoryLinkButton").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ViewAttendanceLinkButton").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("AttendanceHistoryButton").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("HistoryButton").Visible = false;                
                this.gridSearchResults.MasterTableView.GetColumn("OTReason").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTApprovalDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTDurationHour").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("IsOTDueToShiftSpan").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateTime").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("dtIN").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("dtOUT").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTStartTime").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTEndTime").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("RequiredWorkDuration").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("TotalWorkDuration").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("EmpName").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("StatusDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("DistListDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateFullName").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("MealVoucherEligibility").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("OTWFApprovalDesc").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ApproverRemarks").Visible = false;                

                this.gridSearchResults.MasterTableView.GetColumn("DTExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("EmpNoExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("GradeCodeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTTypeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("AttendanceRemarksExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("CurrentlyAssignedFullNameExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("ShiftPatCodeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("ShiftCodeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("ActualShiftCodeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTRequestNoExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("CostCenterExport").Visible = true;                
                this.gridSearchResults.MasterTableView.GetColumn("OTReasonExport").Visible = true;
                //this.gridSearchResults.MasterTableView.GetColumn("OTApprovalDescExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTDurationHourExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("LastUpdateTimeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("dtINExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("dtOUTExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTStartTimeExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("OTEndTimeExport").Visible = true;
                //this.gridSearchResults.MasterTableView.GetColumn("RequiredWorkDurationExport").Visible = true;
                //this.gridSearchResults.MasterTableView.GetColumn("TotalWorkDurationExport").Visible = true;
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
                this.gridSearchResults.Rebind();
                #endregion
            }
            else if (e.CommandName.Equals(RadGrid.ExpandCollapseCommandName))
            {
                #region Selected row in the grid is expanded
                if (!e.Item.Expanded)
                {
                    GridDataItem item = e.Item as GridDataItem;
                    if (item != null)
                    {
                        #region Collapse other items
                        foreach (GridItem otherItem in e.Item.OwnerTableView.Items)
                        {
                            if (otherItem.Expanded && otherItem != item)
                                otherItem.Expanded = false;
                        }
                        #endregion

                        // Sets the order detail grid view
                        this._gridTimesheet = item.ChildItem.FindControl("gridTimesheet") as RadGrid;

                        int empNo = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex]["EmpNo"].Text);                        
                        DateTime? startDate = UIHelper.ConvertObjectToDate(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex]["DT"].Text);
                        DateTime? endDate = UIHelper.ConvertObjectToDate(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex]["DT"].Text);
                        //if (endDate.HasValue)
                        //    startDate = Convert.ToDateTime(endDate).AddDays(-1);

                        GetAttendanceHistory(empNo, startDate, endDate);
                    }
                }
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
                    #region Initialize variables                               
                    EmployeeAttendanceEntity currentOTRecord = null;
                    int autoID = UIHelper.ConvertObjectToInt(item["AutoID"].Text);      // Get the data key value
                    if (autoID > 0)
                    {
                        currentOTRecord = this.OTRequisitionList.Where(a => a.AutoID == autoID).FirstOrDefault();
                    }

                    string statusHandlingCode = UIHelper.ConvertObjectToString(item["StatusHandlingCode"].Text);
                    string statusCode = UIHelper.ConvertObjectToString(item["StatusCode"].Text);
                    string currentDistListCode = UIHelper.ConvertObjectToString(item["DistListCode"].Text);
                    bool isOTProcessed = UIHelper.ConvertObjectToBolean(item["IsOTAlreadyProcessed"].Text);
                    int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);
                    int currentlyAssignedEmpNo = UIHelper.ConvertObjectToInt(item["CurrentlyAssignedEmpNo"].Text);
                    bool isEditMode = UIHelper.ConvertObjectToBolean(item["IsEditMode"].Text);
                    bool isCallOut = UIHelper.ConvertObjectToBolean(item["IsCallOut"].Text);
                    bool isOTExceedOrig = UIHelper.ConvertObjectToBolean(item["IsOTExceedOrig"].Text);
                    bool isOTRamadanExceedLimit = UIHelper.ConvertObjectToBolean(item["IsOTRamadanExceedLimit"].Text);
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                    RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                    RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                    RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                    TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                    TextBox txtApproverComments = item["ApproverRemarks"].Controls[1] as TextBox;
                    RadLabel lblDuration = (RadLabel)item["OTDurationHour"].FindControl("lblDuration");
                    #endregion

                    #region Process "OT Approved?" Header
                    foreach (GridHeaderItem headerItem in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Header))
                    {
                        CheckBox chkOTApprove = (CheckBox)headerItem["OTApprovalDesc"].Controls[1]; // Get the header checkbox 
                        if (chkOTApprove != null)
                        {
                            chkOTApprove.Checked = this.IsOTApprove;
                            chkOTApprove.Enabled = this.IsHRValidator;
                        }
                    }
                    #endregion

                    #region Process workflow "OT Approved?" Header
                    foreach (GridHeaderItem headerItem in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Header))
                    {
                        CheckBox chkOTWFApprove = (CheckBox)headerItem["OTWFApprovalDesc"].Controls[1]; // Get the header checkbox 
                        if (chkOTWFApprove != null)
                        {
                            chkOTWFApprove.Checked = this.IsOTWFApprove;
                        }
                    }
                    #endregion

                    #region Process "OT Approved?" column                   
                    if (cboOTApprovalType != null)
                    {
                        if (cboOTApprovalType.Items.Count > 0)
                            cboOTApprovalType.SelectedValue = UIHelper.ConvertObjectToString(item["OTApprovalCode"].Text).Replace("&nbsp;", "");

                        //if (cboOTApprovalType.SelectedValue == "Y")
                        //    cboOTApprovalType.ForeColor = System.Drawing.Color.DarkGreen;
                        //else if (cboOTApprovalType.SelectedValue == "N")
                        //    cboOTApprovalType.ForeColor = System.Drawing.Color.Red;
                        //else
                        //    cboOTApprovalType.ForeColor = System.Drawing.Color.Orange;
                    }
                    #endregion

                    #region Process "Approve?" column
                    RadComboBox cboOTWFApprovalType = (RadComboBox)item["OTWFApprovalDesc"].FindControl("cboOTWFApprovalType");
                    if (cboOTWFApprovalType != null)
                    {                        
                        if (cboOTWFApprovalType.Items.Count > 0)
                            cboOTWFApprovalType.SelectedValue = UIHelper.ConvertObjectToString(item["OTWFApprovalCode"].Text).Replace("&nbsp;", "");

                        if (cboOTWFApprovalType.SelectedValue == "Y")
                        {
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.YellowGreen;
                            if (currentOTRecord != null)
                                currentOTRecord.IsRemarksRequired = false;
                        }
                        else if (cboOTWFApprovalType.SelectedValue == "N")
                        {
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.Red;
                            if (currentOTRecord != null)
                                currentOTRecord.IsRemarksRequired = true;
                        }
                        else
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.Orange;

                        if (txtApproverComments != null)
                        {
                            if (cboOTWFApprovalType.SelectedValue == "N")
                            {
                                txtApproverComments.BackColor = System.Drawing.Color.Yellow;
                                txtApproverComments.Enabled = true;
                            }
                            else if (cboOTWFApprovalType.SelectedValue == "Y")
                            {
                                txtApproverComments.BackColor = System.Drawing.Color.White;
                                txtApproverComments.Enabled = true;
                            }
                            else
                            {
                                txtApproverComments.BackColor = System.Drawing.Color.Gray;
                                txtApproverComments.Enabled = false;
                                txtApproverComments.ToolTip = "(Note: Approver Comments is disabled if OT request is on-hold.)";
                            }
                        }
                    }
                    #endregion

                    #region Process "Meal Voucher Approved?"                    
                    if (cboMealVoucherEligibility != null &&
                        cboMealVoucherEligibility.Items.Count > 0)
                    {
                        cboMealVoucherEligibility.SelectedValue = UIHelper.ConvertObjectToString(item["MealVoucherEligibilityCode"].Text).Replace("&nbsp;", "");
                    }
                    #endregion

                    #region Process "OT reason"                    
                    if (cboOTReason != null)
                    {
                        cboOTReason.SelectedValue = UIHelper.ConvertObjectToString(item["OTReasonCode"].Text).Replace("&nbsp;", "");
                    }

                    #region Enable/disable controls based on the value of "OT Approved"
                    if (cboOTApprovalType != null)
                    {
                        // Disable "OT Duration"
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = false;

                            // Set the maximum input value
                            txtDuration.MaxValue = isCallOut 
                                ? UIHelper.ConvertObjectToDouble(item["OTDurationHourClone"].Text)
                                : UIHelper.ConvertObjectToDouble(item["OTDurationHourOrig"].Text);
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
                            selectedRecord = this.OTRequisitionList
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
                            if (txtDuration != null)
                                txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";

                            // Enable "OT Reason"
                            //RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
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
                            if (txtDuration != null)
                            {
                                txtDuration.Enabled = false;
                                if (this.SelectedOvertimeRecord != null)
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                            }

                            // Disable "OT Reason"
                            //RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
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

                    #region Set background color 
                    if (statusHandlingCode == "Open")
                    {
                        item.BackColor = System.Drawing.Color.FromName("#8cccff");
                        if (isOTExceedOrig)
                        {
                            item.ForeColor = System.Drawing.Color.Red;
                            item["OTStartTime"].ForeColor = System.Drawing.Color.Red;
                            item["OTEndTime"].ForeColor = System.Drawing.Color.Red;
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

                    // Set the font color to red if overtime exceeds 2 hours during Ramadan for Muslims
                    if (isOTRamadanExceedLimit)
                    {
                        item.ForeColor = System.Drawing.Color.Red;
                        item["EmpNo"].ForeColor = System.Drawing.Color.Red;
                        item["OTStartTime"].ForeColor = System.Drawing.Color.Red;
                        item["OTEndTime"].ForeColor = System.Drawing.Color.Red;
                        item["ArrivalSchedule"].ForeColor = System.Drawing.Color.Red;
                        item["ActualShiftCode"].ForeColor = System.Drawing.Color.Red;
                        item["ActualShiftCode"].Font.Bold = false;

                        if (txtDuration != null)
                            txtDuration.ToolTip = UIHelper.CONST_OT_DURATION_RAMADAN;
                    }
                    #endregion

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
                            imgViewHistory.Enabled = true;
                            imgViewHistory.ToolTip = "View approval history";
                        }
                        else
                        {
                            imgViewHistory.Enabled = false;
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

                    #region Enable/Disable selection checkbox
                    System.Web.UI.WebControls.CheckBox chkSelect = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    if (chkSelect != null)
                    {
                        if (statusHandlingCode == UIHelper.STATUS_HANDLING_CODE_OPEN)
                        {
                            if (this.rblAssignedTo.SelectedValue == "0")        // All
                                chkSelect.Enabled = currentlyAssignedEmpNo != UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]) || Master.IsSystemAdmin;
                            else if (this.rblAssignedTo.SelectedValue == "1")    // Me
                                chkSelect.Enabled = currentlyAssignedEmpNo == UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                            else     // Others
                                chkSelect.Enabled = currentlyAssignedEmpNo != UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]) || Master.IsSystemAdmin;
                        }
                        else
                            chkSelect.Enabled = false;

                        if (!chkSelect.Enabled)
                            chkSelect.Checked = false;
                    }
                    #endregion

                    #region Check if the currently assigned person is HR Validator
                    if (currentDistListCode == UIHelper.DistributionGroupCodes.OTHRVALIDR.ToString() && 
                        userEmpNo == currentlyAssignedEmpNo)
                    {
                        #region Initialize controls
                        // Enable "OT Approved" field
                        if (cboOTApprovalType != null)
                        {
                            cboOTApprovalType.Enabled = true;

                            if (cboOTApprovalType.SelectedValue == "Y")
                            {
                                if (txtDuration != null)
                                    txtDuration.Enabled = true;
                            }
                        }

                        // Enable "Meal VoucherApproved" field
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = true;

                        // Enable "OT Reason" field
                        if (cboOTReason != null)
                            cboOTReason.Enabled = true;

                        // Enable "Remarks" field
                        //if (txtRemarks != null)
                        //    txtRemarks.Enabled = true;

                        // Flag the current record for HR Validation
                        if (autoID > 0 && this.OTRequisitionList.Count > 0)
                        {
                            EmployeeAttendanceEntity currentRecord = this.OTRequisitionList
                                 .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (currentRecord != null)
                                currentRecord.IsForHRValidation = true;
                        }

                        // Remove "0" (No action) item in the OT approval type combobox
                        if (cboOTApprovalType != null &&
                            cboOTApprovalType.Items.Count > 0)
                        {
                            RadComboBoxItem noActionItem = cboOTApprovalType.Items
                                .Where(a => a.Value == "0")
                                .FirstOrDefault();
                            if (noActionItem != null)
                                cboOTApprovalType.Items.Remove(noActionItem);
                        }
                        #endregion

                        #region Reload OT Reason combobox
                        if (cboOTApprovalType.SelectedValue == "Y")
                            FillOvertimeReasonCombo(true, 1);
                        else
                            FillOvertimeReasonCombo(true, 2);
                        #endregion
                    }
                    else
                    {
                        // Disable "Meal Voucher Approved?" field if current user is not HR Validator or workflow not currently assigne for HR validation
                        if (cboMealVoucherEligibility != null &&
                            cboMealVoucherEligibility.Enabled == true)
                        {
                            cboMealVoucherEligibility.Enabled = false;
                        }

                        //if (editLink != null)
                        //{
                        //    editLink.Enabled = false;
                        //    editLink.ForeColor = System.Drawing.Color.Silver;
                        //}

                        #region Enable "Remarks" field for Managers only (Grade 12 and above)                                                
                        if (this.btnSubmitApproval.Visible == true &&
                            this.Master.CurrentUserPayGrade >= 12 &&
                            currentDistListCode != UIHelper.DistributionGroupCodes.OTHRAPROVE.ToString())
                        {
                            // Enable "Remarks" field
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;
                        }
                        #endregion
                    }
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
                GridColumn dynamicColumn = null;

                #region Show/hide Cancel button 
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CancelButton").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
                #endregion

                #region Show/Hide Edit link
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "EditLinkButton").FirstOrDefault();
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

                #region Show/hide "Status" field 
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "StatusDesc").FirstOrDefault();
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

                #region Show/hide "Currently Assigned To" field 
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CurrentlyAssignedFullName").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSHOWALL.ToString())
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
                #endregion

                #region Show/hide "Approver Comments" field 
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ApproverRemarks").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.rblAssignedTo.SelectedValue == "1")    // Currently Assigned to Me
                        dynamicColumn.Visible = true;
                    else
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show/hide "Approve?" field 
                dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "OTWFApprovalDesc").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    if (this.rblAssignedTo.SelectedValue == "1")    // Currently Assigned to Me
                        dynamicColumn.Visible = true;
                    else
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show other fields when current user is HR Validator
                if (this.IsHRValidator)
                {
                    // Show Edit link
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "EditImageLink").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.Visible = true;

                    //// Show Undo link
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "UndoImageLink").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.Visible = true;

                    //// Show Save link
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "SaveImageLink").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.Visible = true;

                    // Show "ShiftPatCode" field
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ShiftPatCode").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.Visible = true;

                    //// Show "ShiftCode" field
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ShiftCode").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.Visible = true;

                    //// Show "ActualShiftCode" field
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ActualShiftCode").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.Visible = true;

                    // Set the "Validate?" field
                    //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "OTWFApprovalDesc").FirstOrDefault();
                    //if (dynamicColumn != null)
                    //    dynamicColumn.HeaderText = "Validate?";
                }
                #endregion

                #region Hide unnecessary fields for non-HR approvers
                if (!this.IsHRValidator &&
                    !this.Master.IsSystemAdmin)
                {
                    // Hide Total Work Duration
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "TotalWorkDuration").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Required Work Duration
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "RequiredWorkDuration").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Meal Voucher Approved?
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "MealVoucherEligibility").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Is Shift Span?
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "IsOTDueToShiftSpan").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Status
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "StatusDesc").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Currently Assigned To
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CurrentlyAssignedFullName").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Approval Level
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "DistListDesc").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Last Updated By
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "LastUpdateFullName").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Last Updated Time
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "LastUpdateTime").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Shift Pat. Code
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ShiftPatCode").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide View Attendance link                    
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ViewAttendanceLinkButton").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "AttendanceHistoryButton").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;
                }
                #endregion

                #region Show/Hide Command Items
                if (!this.IsHRValidator &&
                    !this.Master.IsSystemAdmin &&
                    this.Master.CurrentUserPayGrade < 12)
                {
                    GridCommandItem item = (GridCommandItem)this.gridSearchResults.MasterTableView.GetItems(GridItemType.CommandItem)[0];
                    if (item != null)
                        item.Display = false;
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
            if (this.OTRequisitionList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.OTRequisitionList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.OTRequisitionList.Count.ToString("#,###"));
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

        #region Child Grid Events
        private void RebindDataToTimesheetGrid()
        {
            if (this._gridTimesheet != null &&
                this.AttendanceList.Count > 0)
            {
                this._gridTimesheet.DataSource = this.AttendanceList;
                this._gridTimesheet.DataBind();
            }
            else
                InitializeDataToTimesheetGrid();
        }

        private void InitializeDataToTimesheetGrid()
        {
            if (this._gridTimesheet != null)
            {
                this._gridTimesheet.DataSource = new List<EmployeeAttendanceEntity>();
                this._gridTimesheet.DataBind();
            }
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset controls
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());
            this.chkShow12HourShift.Checked = false;

            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboCostCenter.ClearCheckedItems();
            this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTUNPROCSD.ToString();

            this.rblAssignedTo.SelectedValue = "1";     // Me
            this.rblAssignedTo_SelectedIndexChanged(this.rblAssignedTo, new EventArgs());

            if (this.IsOTBudgetAdmin)
            {
                #region Reset overtime statistics controls
                this.chkShowBreakdown.Checked = false;
                this.cboFiscalYear.SelectedValue = DateTime.Now.Year.ToString();
                this.cboFiscalYear_SelectedIndexChanged(this.cboFiscalYear, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFiscalYear.Text, string.Empty, this.cboFiscalYear.SelectedValue, string.Empty));
                #endregion
            }
            #endregion

            // Cler collections
            this.OTRequisitionList.Clear();
            this.OTRequisitionApprovalList.Clear();
            this.CheckedOTRequisitionList.Clear();
            this.OTRequisitionListOrig.Clear();

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
            ViewState["IsOTWFApprove"] = null;
            ViewState["IsOTApprovalHeaderClicked"] = null;
            ViewState["IsOTWFApprovalHeaderClicked"] = null;

            // Reset the grid
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();

            // Reload the data
            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            //int errorCount = 0;

            //// Check date range
            //if (this.dtpStartDate.SelectedDate == null)
            //{
            //    this.txtGeneric.Text = ValidationErrorType.NoStartDate.ToString();
            //    this.ErrorType = ValidationErrorType.NoStartDate;
            //    this.cusValStartDate.Validate();
            //    errorCount++;
            //}
            //else
            //{
            //    if (this.dtpStartDate.SelectedDate != null &&
            //        this.dtpEndDate.SelectedDate != null)
            //    {
            //        if (this.dtpStartDate.SelectedDate > this.dtpEndDate.SelectedDate)
            //        {
            //            this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
            //            this.ErrorType = ValidationErrorType.InvalidDateRange;
            //            this.cusValStartDate.Validate();
            //            errorCount++;
            //        }
            //    }
            //}

            //if (errorCount > 0)
            //{
            //    // Set focus to the top panel
            //    Page.SetFocus(this.lnkMoveUp.ClientID);
            //    return;
            //}
            #endregion

            if (!this.ReloadGridData)
            {
                // Reset page index
                this.gridSearchResults.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridSearchResults.PageSize;
            }

            GetOvertimeRequest(true);

            if (this.CanViewOTStatistic)
                GetOTStatisticsData();
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_APPROVAL
            ),
            false);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (this.OTRequisitionList.Count == 0)
                return;

            try
            {
                int errorCount = 0;
                StringBuilder sb = new StringBuilder();
                List<EmployeeAttendanceEntity> OTRequisitionList = new List<EmployeeAttendanceEntity>();

                #region Build the collection and populate overtime record
                foreach (GridDataItem item in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Item, GridItemType.AlternatingItem))
                {
                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
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
                                        sb.AppendLine(string.Format(@"OT Reason for Employee No. {0} is mandatory. Please specify the overtime reason then try to save again!<br />", selectedRecord.EmpNo));
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
                                    newItem.AttendanceRemarks = txtRemarks.Text.Trim();
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
                                        DateTime otStart = Convert.ToDateTime(selectedRecord.OTStartTime);
                                        DateTime otEnd = Convert.ToDateTime(selectedRecord.OTEndTime);

                                        actualOTDurationMin = (new DateTime(otEnd.Year, otEnd.Month, otEnd.Day, otEnd.Hour, otEnd.Minute, 0) - new DateTime(otStart.Year, otStart.Month, otStart.Day, otStart.Hour, otStart.Minute, 0)).TotalMinutes;
                                        //actualOTDurationMin = (Convert.ToDateTime(selectedRecord.OTEndTime) - Convert.ToDateTime(selectedRecord.OTStartTime)).TotalMinutes;
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
                                        sb.AppendLine(string.Format(@"The specified overtime duration for Employee No. {0} is incorrect. Take note that duration should be equal or less than the actual duration based on the value of OT Start Time and OT End Time.<br />", selectedRecord.EmpNo));
                                    }
                                }
                                #endregion

                                // Add item to the collection
                                OTRequisitionList.Add(newItem);
                            }
                        }
                    }
                }
                #endregion

                #region Check for errors
                if (errorCount > 0)
                {
                    DisplayFormLevelError(sb.ToString().Trim());
                    return;
                }
                #endregion

                if (OTRequisitionList.Count > 0)
                    SaveOvertime(OTRequisitionList);
                else
                    DisplayFormLevelError("Unable to proceed because no overtime record has been selected or processed for approval.");
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

        protected void btnFindAssignee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_APPROVAL
            ),
            false);
        }

        protected void btnSubmitApproval_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.OTRequisitionList.Count == 0)
                {
                    throw new Exception("Could not proceed because there are no selected records in the grid that require approval.");
                }
                else
                {
                    List<EmployeeAttendanceEntity> approvedOTList = new List<EmployeeAttendanceEntity>();
                    List<EmployeeAttendanceEntity> rejectededOTList = new List<EmployeeAttendanceEntity>();
                    List<EmployeeAttendanceEntity> holdOTList = new List<EmployeeAttendanceEntity>();
                    int errorCount = 0;
                    StringBuilder sb = new StringBuilder();

                    #region Loop through each row in the grid to set the value of "IsApproved" and "ApproverRemarks" fields
                    GridDataItemCollection gridData = this.gridSearchResults.MasterTableView.Items;
                    if (gridData.Count > 0)
                    {
                        foreach (GridDataItem item in gridData)
                        {
                            // Get the data key value
                            long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                            if (autoID > 0)
                            {
                                EmployeeAttendanceEntity selectedOTRequest = this.OTRequisitionList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedOTRequest != null)
                                {
                                    #region Check if OT Reason is specified
                                    RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                                    if (cboOTReason != null)
                                    {
                                        if (string.IsNullOrEmpty(cboOTReason.SelectedValue) ||
                                            cboOTReason.SelectedValue.Replace("&nbsp;", "").Trim() == string.Empty ||
                                            cboOTReason.SelectedValue == "0")
                                        {
                                            errorCount += 1;
                                            sb.AppendLine(string.Format(@"Overtime reason for Requisition No. {0} is mandatory. Please specify the reason then try to submit again!<br />", selectedOTRequest.OTRequestNo));
                                        }
                                    }
                                    #endregion

                                    #region Get the approver remarks
                                    System.Web.UI.WebControls.TextBox txtRemarks = item["ApproverRemarks"].Controls[1] as System.Web.UI.WebControls.TextBox;
                                    if (txtRemarks != null)
                                    {
                                        selectedOTRequest.ApproverRemarks = txtRemarks.Text.Trim();
                                    }
                                    #endregion

                                    #region Set value for "AttendanceRemarks"
                                    TextBox txtAttendanceRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                                    if (txtAttendanceRemarks != null &&
                                        txtAttendanceRemarks.Enabled)
                                        selectedOTRequest.AttendanceRemarks = txtAttendanceRemarks.Text.Trim();
                                    #endregion

                                    #region Get the approval type
                                    RadComboBox cboOTWFApprovalType = null;

                                    try
                                    {
                                        cboOTWFApprovalType = (RadComboBox)item["OTWFApprovalDesc"].FindControl("cboOTWFApprovalType");
                                        if (cboOTWFApprovalType != null)
                                        {
                                            if (cboOTWFApprovalType.SelectedValue == "Y")
                                                selectedOTRequest.IsApproved = true;
                                            else if (cboOTWFApprovalType.SelectedValue == "N")
                                                selectedOTRequest.IsApproved = false;
                                            //else
                                            //    continue;   // Skip to the next record since approval is not set to either "Yes" or "No"
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        selectedOTRequest.IsApproved = true;
                                    }

                                    // Add item to the designated collection
                                    if (cboOTWFApprovalType != null)
                                    {
                                        if (cboOTWFApprovalType.SelectedValue == "Y")
                                            approvedOTList.Add(selectedOTRequest);
                                        else if (cboOTWFApprovalType.SelectedValue == "N")
                                            rejectededOTList.Add(selectedOTRequest);
                                        else
                                            holdOTList.Add(selectedOTRequest);
                                    }
                                    #endregion

                                    #region Set the flag whether OT details have been modified by HR
                                    if (selectedOTRequest.IsOTApprovalCodeModified ||
                                        selectedOTRequest.IsMealVoucherEligibilityModified ||
                                        selectedOTRequest.IsOTReasonModified ||
                                        selectedOTRequest.IsAttendanceRemarksModified ||
                                        selectedOTRequest.IsOTDurationModified)
                                    {
                                        selectedOTRequest.IsModifiedByHR = true;
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion

                    if (errorCount > 0)
                    {
                        throw new Exception(sb.ToString().Trim());
                    }
                    else
                    {
                        if (approvedOTList.Count == 0 &&
                            rejectededOTList.Count == 0 &&
                            holdOTList.Count == 0)
                        {
                            throw new Exception("Could not proceed because there are no selected records in the grid that require approval.");
                        }
                        else
                        {
                            #region Process the rejected overtime requisitions
                            if (rejectededOTList.Count > 0)
                            {
                                #region Check if justification is specified
                                errorCount = 0;
                                sb.Clear();

                                foreach (EmployeeAttendanceEntity item in rejectededOTList)
                                {
                                    if (item.IsRemarksRequired &&
                                        string.IsNullOrEmpty(item.ApproverRemarks))
                                    {
                                        sb.AppendLine(string.Format(@"Could not reject Requisition No. {0}. Please specify the justification for rejection in the <b>Approver Comments</b> field! <br />",
                                            item.OTRequestNo));
                                        errorCount++;
                                    }
                                }
                                #endregion

                                if (errorCount > 0)
                                {
                                    throw new Exception(sb.ToString().Trim());
                                }
                                else
                                    ProcessRejectionAction(rejectededOTList);
                            }
                            #endregion

                            #region Process the approved overtime requisitions
                            if (approvedOTList.Count > 0)
                            {
                                ProcessApprovalAction(approvedOTList);
                            }
                            #endregion

                            #region Process the overtime requisitions to be hold
                            if (holdOTList.Count > 0)
                            {
                                HoldOvertimeRequest(holdOTList);
                            }
                            #endregion

                            // Refresh the grid
                            this.btnSearch_Click(this.btnSearch, new EventArgs());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnReassign_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform validation
                if (this.OTRequisitionList.Count == 0)
                {
                    throw new Exception("Could not proceed because there are selected records in the grid that need to be reassigned.");
                }
                #endregion

                #region Get the selected records
                // Reset collection
                this.CheckedOTRequisitionList.Clear();

                System.Web.UI.WebControls.CheckBox chkSelectColumn = null;
                foreach (GridDataItem item in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Item, GridItemType.AlternatingItem))
                {
                    chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    if (chkSelectColumn != null && chkSelectColumn.Checked)
                    {
                        // Get the data key value
                        long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                        if (autoID > 0)
                        {
                            EmployeeAttendanceEntity selectedOTRecord = this.OTRequisitionList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedOTRecord != null)
                            {
                                this.CheckedOTRequisitionList.Add(selectedOTRecord);
                            }
                        }
                    }
                }
                #endregion

                if (this.CheckedOTRequisitionList.Count > 0)
                {
                    StringBuilder script = new StringBuilder();
                    script.Append("ConfirmButtonAction('");
                    script.Append(string.Concat(this.btnReassignDummy.ClientID, "','"));
                    script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                    script.Append(UIHelper.CONST_REASSIGN_CONFIRMATION + "');");

                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Reassignment Confirmation", script.ToString(), true);
                }
                else
                {
                    throw new Exception("Could not proceed because there are no selected records in the grid that need to be reassigned.");
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnReassignDummy_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReassignmentForm, (sender as RadButton).ID);

            // Set the session
            Session["OTRequestReassignList"] = this.CheckedOTRequisitionList;

            int approverEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_REASSIGNMENT_FORM + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_APPROVAL,
                "CurrentApproverEmpNo",
                approverEmpNo
            ),
            false);
        }

        protected void btnAssignToMe_Click(object sender, EventArgs e)
        {
            try
            {
                #region Get the selected swipes
                // Reset collection
                this.CheckedOTRequisitionList.Clear();

                System.Web.UI.WebControls.CheckBox chkSelectColumn = null;
                foreach (GridDataItem item in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Item, GridItemType.AlternatingItem))
                {
                    chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    if (chkSelectColumn != null && chkSelectColumn.Checked)
                    {
                        // Get the data key value
                        long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                        if (autoID > 0)
                        {
                            EmployeeAttendanceEntity selectedOTRequest = this.OTRequisitionList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedOTRequest != null)
                            {
                                this.CheckedOTRequisitionList.Add(selectedOTRequest);
                            }
                        }
                    }
                }
                #endregion

                if (this.CheckedOTRequisitionList.Count > 0)
                {
                    StringBuilder script = new StringBuilder();
                    script.Append("ConfirmButtonAction('");
                    script.Append(string.Concat(this.btnAssignToMeDummy.ClientID, "','"));
                    script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                    script.Append(UIHelper.CONST_ASSIGN_TOME_CONFIRMATION + "');");
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Reassignment Confirmation", script.ToString(), true);
                }
                else
                {
                    throw new Exception("Could not proceed because there are no selected records in the grid for reassignment.");
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnAssignToMeDummy_Click(object sender, EventArgs e)
        {
            ProcessAssignToMeAction(this.CheckedOTRequisitionList);
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
                    #region Check if selected overtime reason is a callout
                    GridDataItem item = cboOTReason.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        #region Save currently selected record to session
                        // Reset session
                        this.SelectedOvertimeRecord = null;

                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        // Save current selected datagrid row
                        if (autoID > 0 && this.OTRequisitionList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null)
                            {
                                #region Check if record has been modified by HR Validator
                                if (selectedRecord.IsForHRValidation)
                                {
                                    selectedRecord.IsOTReasonModified = CheckIfRecordHasChanged(cboOTReason.SelectedValue, "OTReasonCode", selectedRecord.OTRequestNo, this.OTRequisitionListOrig);
                                    selectedRecord.OTReasonCode = cboOTReason.SelectedValue;
                                    selectedRecord.OTReason = cboOTReason.Text;
                                }
                                #endregion

                                // Save current record to session
                                this.SelectedOvertimeRecord = selectedRecord;
                            }
                        }
                        #endregion

                        int otDurationOrig = UIHelper.ConvertObjectToInt(item["OTDurationHourOrig"].Text);
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
                                            txtDuration.Value = txtDuration.Value + callOutValue;
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

                                this.txtDuration_TextChanged(txtDuration, new EventArgs());
                            }
                        }
                    }                    
                    #endregion
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
                    if (autoID > 0 && this.OTRequisitionList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            #region Check if record has been modified by HR Validator
                            if (selectedRecord.IsForHRValidation)
                            {
                                selectedRecord.IsOTApprovalCodeModified = CheckIfRecordHasChanged(cboOTApprovalType.SelectedValue, "OTApprovalCode", selectedRecord.OTRequestNo, this.OTRequisitionListOrig); 
                                selectedRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                                selectedRecord.OTApprovalDesc = cboOTApprovalType.Text;
                            }
                            #endregion

                            // Save current record to session
                            this.SelectedOvertimeRecord = selectedRecord;
                        }
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
                                txtDuration.Value = 0;
                            }
                            else
                            {
                                // Calculate OT duration
                                txtDuration.Value = this.SelectedOvertimeRecord.OTDurationHourOrig;
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
                                txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
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

        protected void cboOTWFApprovalType_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            RadComboBox cboOTWFApprovalType = (RadComboBox)sender;
            if (cboOTWFApprovalType != null)
            {
                GridDataItem item = cboOTWFApprovalType.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Reset session
                    this.SelectedOvertimeRecord = null;

                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                    // Save current selected datagrid row
                    if (autoID > 0 && 
                        this.OTRequisitionList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            // Save to session
                            this.SelectedOvertimeRecord = selectedRecord;

                            selectedRecord.IsRemarksRequired = cboOTWFApprovalType.SelectedValue == "N" ? true : false;
                        }
                    }

                    #region Initialize variables
                    TextBox txtRemarks = item["ApproverRemarks"].Controls[1] as TextBox;
                    RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                    RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                    RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                    RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                    #endregion

                    #region Set the backcolor of the Remarks field                    
                    if (txtRemarks != null)
                    {
                        if (cboOTWFApprovalType.SelectedValue == "N")
                        {
                            txtRemarks.BackColor = System.Drawing.Color.Yellow;
                            txtRemarks.Enabled = true;
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.Red;
                        }
                        else if (cboOTWFApprovalType.SelectedValue == "Y")
                        {
                            txtRemarks.BackColor = System.Drawing.Color.White;
                            txtRemarks.Enabled = true;
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.YellowGreen;
                        }
                        else 
                        {
                            txtRemarks.BackColor = System.Drawing.Color.Gray;
                            txtRemarks.Enabled = false;
                            txtRemarks.Text = string.Empty;
                            txtRemarks.ToolTip = "(Note: Approver Comments is disabled if OT request is set on-hold.)";
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.Orange;
                        }
                    }
                    #endregion

                    #region Enable/Disable fields for HR Validator
                    if (this.SelectedOvertimeRecord != null &&
                        this.SelectedOvertimeRecord.IsForHRValidation)
                    {
                        if (cboOTWFApprovalType.SelectedValue == "0")
                        {
                            // Disable "OT Approved" field
                            if (cboOTApprovalType != null)
                                cboOTApprovalType.Enabled = false;

                            // Disable "Meal VoucherApproved" field
                            if (cboMealVoucherEligibility != null)
                                cboMealVoucherEligibility.Enabled = false;

                            // Disable "OT Duration" field
                            if (txtDuration != null)
                                txtDuration.Enabled = false;

                            // Disable "OT Reason" field
                            if (cboOTReason != null)
                                cboOTReason.Enabled = false;

                            // Disable "Remarks" field
                            if (txtRemarks != null)
                                txtRemarks.Enabled = false;
                        }
                        else
                        {
                            // Enable "OT Approved" field
                            if (cboOTApprovalType != null)
                                cboOTApprovalType.Enabled = true;

                            // Enable "Meal VoucherApproved" field
                            if (cboMealVoucherEligibility != null)
                                cboMealVoucherEligibility.Enabled = true;

                            // Enable "OT Duration" field
                            if (txtDuration != null)
                                txtDuration.Enabled = true;

                            // Enable "OT Reason" field
                            if (cboOTReason != null)
                                cboOTReason.Enabled = true;

                            // Enable "Remarks" field
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;
                        }
                    }
                    #endregion
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
                        #region Save selected record to session
                        // Reset session
                        this.SelectedOvertimeRecord = null;

                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        // Save current selected datagrid row
                        if (autoID > 0 && this.OTRequisitionList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null)
                            {
                                #region Check if record has been modified by HR Validator
                                if (selectedRecord.IsForHRValidation)
                                {
                                    selectedRecord.IsMealVoucherEligibilityModified = CheckIfRecordHasChanged(cboMealVoucherEligibility.SelectedValue, "MealVoucherEligibility", selectedRecord.OTRequestNo, this.OTRequisitionListOrig);
                                    selectedRecord.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                    selectedRecord.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                                }
                                #endregion

                                // Save to session
                                this.SelectedOvertimeRecord = selectedRecord;
                            }
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

                    if (this.OTRequisitionList.Count > 0)
                    {
                        foreach (EmployeeAttendanceEntity item in this.OTRequisitionList)
                        {
                            if (!item.IsOTAlreadyProcessed)
                            {
                                item.OTApprovalDesc = chkOTApprove.Checked == true ? "Yes" : "-";
                                item.OTApprovalCode = chkOTApprove.Checked == true ? "Y" : "-";
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

        protected void chkOTWFApprove_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkOTWFApprove = sender as CheckBox;
                if (chkOTWFApprove != null)
                {
                    // Save to session
                    this.IsOTWFApprove = UIHelper.ConvertObjectToBolean(chkOTWFApprove.Checked);
                    this.IsOTWFApprovalHeaderClicked = true;

                    if (this.OTRequisitionList.Count > 0)
                    {
                        foreach (EmployeeAttendanceEntity item in this.OTRequisitionList)
                        {
                            if (!item.IsOTWFProcessed)
                            {
                                item.OTWFApprovalDesc = chkOTWFApprove.Checked == true ? "Yes" : "No";
                                item.OTWFApprovalCode = chkOTWFApprove.Checked == true ? "Y" : "N";
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
            switch (selectedDisplayOption)
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
                    if (autoID > 0 && this.OTRequisitionList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
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
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
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
                                txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
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
                    this.OTRequisitionList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
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
                    this.OTRequisitionList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedOTRecord = this.OTRequisitionList
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
                            UIHelper.PAGE_OVERTIME_APPROVAL
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

        protected void imgViewAttendance_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex].GetDataKeyValue("AutoID"));

                // Save current selected datagrid row
                if (autoID > 0 &&
                    this.OTRequisitionList.Count > 0)
                {
                    // Save session values
                    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    if (autoID > 0 &&
                        this.OTRequisitionList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedOTRecord = this.OTRequisitionList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedOTRecord != null &&
                            selectedOTRecord.DT != null)
                        {
                            DateTime? startDate = null;
                            DateTime? endDate = null;
                            string costCenter = selectedOTRecord.CostCenter;
                            string costCenterFullName = selectedOTRecord.CostCenterFullName;
                            int empNo = selectedOTRecord.EmpNo;
                            int dayNum = selectedOTRecord.DT.Value.Day;
                            int monthNum = selectedOTRecord.DT.Value.Month;
                            if (selectedOTRecord.DT.Value.Day >= 16)
                                monthNum = monthNum + 1;

                            int yearNum = selectedOTRecord.DT.Value.Year;
                            if (monthNum > 12)
                            {
                                monthNum = 1;
                                yearNum = yearNum + 1;
                            }

                            #region Set the payroll period
                            switch (monthNum)
                            {
                                case 1: // January
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/12/{0}", yearNum - 1));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/01/{0}", yearNum));
                                    break;

                                case 2: // Feburary
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/01/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/02/{0}", yearNum));
                                    break;

                                case 3: // March
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/02/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/03/{0}", yearNum));
                                    break;

                                case 4: // April
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/03/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/04/{0}", yearNum));
                                    break;

                                case 5: // May
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/04/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/05/{0}", yearNum));
                                    break;

                                case 6: // June
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/05/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/06/{0}", yearNum));
                                    break;

                                case 7: // July
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/06/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/07/{0}", yearNum));
                                    break;

                                case 8: // August
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/07/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/08/{0}", yearNum));
                                    break;

                                case 9: // September
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/08/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/09/{0}", yearNum));
                                    break;

                                case 10: // October
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/09/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/10/{0}", yearNum));
                                    break;

                                case 11: // November
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/10/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/11/{0}", yearNum));
                                    break;

                                case 12: // December
                                    startDate = UIHelper.ConvertObjectToDate(string.Format("16/11/{0}", yearNum));
                                    endDate = UIHelper.ConvertObjectToDate(string.Format("15/12/{0}", yearNum));
                                    break;
                            }
                            #endregion

                            if (startDate.HasValue && endDate.HasValue)
                            {
                                List<EmployeeAttendanceEntity> attendanceList = GetAttendanceHistory(startDate, endDate, costCenter, empNo);
                                if (attendanceList != null)
                                {
                                    // Save report data to session
                                    Session["EmpAttendanceHistoryReportSource"] = attendanceList;

                                    // Show the report with workplace swipes information
                                    Response.Redirect
                                    (
                                        String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}",
                                        UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                                        UIHelper.ReportTypes.EmployeeAttendanceHistoryReport.ToString(),
                                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                                        UIHelper.PAGE_OVERTIME_APPROVAL,
                                        UIHelper.QUERY_STRING_COSTCENTER_KEY,
                                        string.Format("Cost Center: {0}", Server.UrlEncode(costCenterFullName)),
                                        UIHelper.QUERY_STRING_STARTDATE_KEY,
                                        Convert.ToDateTime(startDate).Date.ToString(),
                                        UIHelper.QUERY_STRING_ENDDATE_KEY,
                                        Convert.ToDateTime(endDate).Date.ToString()
                                    ),
                                    false);
                                }
                            }
                            else
                                throw new Exception("Unable to view the attendance report because the payroll period could not be determined!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void imgEdit_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex].GetDataKeyValue("AutoID"));

                // Save current selected datagrid row
                if (autoID > 0 &&
                    this.OTRequisitionList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (selectedRecord != null)
                    {
                        // Set edit mode flag
                        selectedRecord.IsEditMode = true;

                        // Save the currently selected record
                        this.SelectedOvertimeRecord = selectedRecord;

                        GridDataItem item = this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex];
                        if (item != null)
                        {
                            #region Enable datagrid fields
                            // Enable "OT Approved" field
                            RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                            if (cboOTApprovalType != null)
                                cboOTApprovalType.Enabled = true;

                            // Enable "Meal VoucherApproved" field
                            RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            if (cboMealVoucherEligibility != null)
                                cboMealVoucherEligibility.Enabled = true;

                            // Enable "OT Duration" field
                            RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                                txtDuration.Enabled = true;

                            // Enable "OT Reason" field
                            RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                            if (cboOTReason != null)
                                cboOTReason.Enabled = true;

                            // Enable "Remarks" field
                            TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;

                            // Select the row
                            item.Selected = true;
                            #endregion

                            #region Show Undo and Save buttons, hide Edit button
                            (sender as ImageButton).Visible = false;

                            //ImageButton imgUndo = (ImageButton)item["UndoImageLink"].FindControl("imgUndo");
                            //if (imgUndo != null)
                            //{
                            //    imgUndo.Visible = true;
                            //}

                            //ImageButton imgSave = (ImageButton)item["SaveImageLink"].FindControl("imgSave");
                            //if (imgSave != null)
                            //{
                            //    imgSave.Visible = true;
                            //}
                            #endregion
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

        protected void imgUndo_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                // Get the data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex].GetDataKeyValue("AutoID"));

                // Save current selected datagrid row
                if (autoID > 0 &&
                    this.OTRequisitionList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (selectedRecord != null)
                    {
                        // Save the currently selected record
                        this.SelectedOvertimeRecord = selectedRecord;

                        GridDataItem item = this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex];
                        if (item != null)
                        {
                            #region Initialize datagrid fields
                            // Disable "OT Approved" field
                            RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                            if (cboOTApprovalType != null)
                                cboOTApprovalType.Enabled = false;

                            // Disable "Meal VoucherApproved" field
                            RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            if (cboMealVoucherEligibility != null)
                                cboMealVoucherEligibility.Enabled = false;

                            // Disable "OT Duration" field
                            RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                                txtDuration.Enabled = false;

                            // Disable "OT Reason" field
                            RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                            if (cboOTReason != null)
                                cboOTReason.Enabled = false;

                            // Disable "Remarks" field
                            TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                            if (txtRemarks != null)
                                txtRemarks.Enabled = false;

                            // Select the row
                            item.Selected = true;
                            #endregion

                            #region Hide Undo and Save buttons, show Edit button
                            (sender as ImageButton).Visible = true;

                            ImageButton imgUndo = (ImageButton)item["UndoImageLink"].FindControl("imgUndo");
                            if (imgUndo != null)
                            {
                                imgUndo.Visible = false;
                            }

                            ImageButton imgSave = (ImageButton)item["SaveImageLink"].FindControl("imgSave");
                            if (imgSave != null)
                            {
                                imgSave.Visible = false;
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

        protected void imgSave_Click(object sender, ImageClickEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void rblAssignedTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rblAssignedTo.SelectedValue == "0")    // All
            {
                this.txtAssigneeEmpNo.Visible = false;
                this.btnFindAssignee.Visible = false;

                // Initialize buttons
                this.btnSubmitApproval.Visible = false;
                this.btnReassign.Visible = true;
                this.btnAssignToMe.Visible = true;
            }
            else if (this.rblAssignedTo.SelectedValue == "2")    // Others
            {                
                this.btnFindAssignee.Visible = true;
                this.txtAssigneeEmpNo.Visible = true;
                this.txtAssigneeEmpNo.Text = string.Empty;
                this.txtAssigneeEmpNo.Focus();

                // Initialize buttons
                this.btnSubmitApproval.Visible = false;
                this.btnReassign.Visible = false;
                this.btnAssignToMe.Visible = true;
            }
            else
            {
                this.txtAssigneeEmpNo.Visible = false;
                this.btnFindAssignee.Visible = false;

                // Initialize buttons
                this.btnSubmitApproval.Visible = true;
                this.btnReassign.Visible = true;
                this.btnAssignToMe.Visible = false;
            }

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void rblApproval_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButtonList rblApproval = sender as RadioButtonList;
                if (rblApproval != null)
                {
                    GridDataItem item = rblApproval.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        //// Get the data key value
                        //long swipeID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("SwipeID"));
                        //if (swipeID > 0)
                        //{
                        //    EmployeeAttendance selectedSwipe = this.MissingSwipeList
                        //        .Where(a => a.SwipeID == swipeID)
                        //        .FirstOrDefault();
                        //    if (selectedSwipe != null)
                        //    {
                        //        selectedSwipe.IsRemarksRequired = rblApproval.SelectedValue == "valNo" ? true : false;
                        //    }
                        //}

                        //// Set the backcolor of the Remarks field
                        //System.Web.UI.WebControls.TextBox txtRemarks = item["ApproverRemarks"].Controls[1] as System.Web.UI.WebControls.TextBox;
                        //if (txtRemarks != null)
                        //{
                        //    txtRemarks.BackColor = rblApproval.SelectedValue == "valNo" ? System.Drawing.Color.Yellow : System.Drawing.Color.White;
                        //}
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
                if (txtDuration != null)
                {
                    GridDataItem item = txtDuration.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        #region Save selected record to session
                        // Reset session
                        this.SelectedOvertimeRecord = null;

                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");

                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        // Save current selected datagrid row
                        if (autoID > 0 && this.OTRequisitionList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null &&
                                selectedRecord.IsForHRValidation)
                            {
                                #region Check if record has been modified by HR Validator
                                if (selectedRecord.IsForHRValidation)
                                {
                                    #region Validate the specified duration
                                    bool isValidOTDuration = false;
                                    int specifiedOTDurationHour = UIHelper.ConvertObjectToInt(txtDuration.Value);
                                    int specifiedOTDurationMin = 0;
                                    double actualOTDurationMin = 0;
                                    int hours = 0;
                                    int minutes = 0;
                                    StringBuilder sb = new StringBuilder();

                                    // Convert hour duration into minutes
                                    hours = Math.DivRem(specifiedOTDurationHour, 100, out minutes);
                                    specifiedOTDurationMin = (hours * 60) + minutes;

                                    #region Validate overtime duration
                                    //if (selectedRecord.OTStartTime != null &&
                                    //    selectedRecord.OTEndTime != null)
                                    //{
                                    //    DateTime otStart = Convert.ToDateTime(selectedRecord.OTStartTime);
                                    //    DateTime otEnd = Convert.ToDateTime(selectedRecord.OTEndTime);

                                    //    actualOTDurationMin = (new DateTime(otEnd.Year, otEnd.Month, otEnd.Day, otEnd.Hour, otEnd.Minute, 0) - new DateTime(otStart.Year, otStart.Month, otStart.Day, otStart.Hour, otStart.Minute, 0)).TotalMinutes;
                                    //}

                                    if (selectedRecord.OTStartTimeOrig != null &&
                                        selectedRecord.OTEndTimeOrig != null)
                                    {
                                        DateTime otStart = Convert.ToDateTime(selectedRecord.OTStartTimeOrig);
                                        DateTime otEnd = Convert.ToDateTime(selectedRecord.OTEndTimeOrig);

                                        actualOTDurationMin = (new DateTime(otEnd.Year, otEnd.Month, otEnd.Day, otEnd.Hour, otEnd.Minute, 0) - new DateTime(otStart.Year, otStart.Month, otStart.Day, otStart.Hour, otStart.Minute, 0)).TotalMinutes;
                                    }
                                    #endregion

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
                                    }

                                    if (isValidOTDuration)
                                    {
                                        int maxOTMinutes = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["MaxOTMinutes"]);

                                        selectedRecord.IsOTDurationModified = CheckIfRecordHasChanged(specifiedOTDurationMin.ToString(), "OTDurationMinute", selectedRecord.OTRequestNo, this.OTRequisitionListOrig);
                                        selectedRecord.OTDurationHour = specifiedOTDurationHour;
                                        selectedRecord.OTDurationMinute = specifiedOTDurationMin;

                                        #region Check if overtime duration is greater than or equals to the limit set in the config file
                                        if (maxOTMinutes > 0 &&
                                            txtDuration.Enabled)
                                        {
                                            if (selectedRecord.OTDurationMinute >= maxOTMinutes)
                                            {
                                                throw new Exception(string.Format(@"The overtime duration of Employee No. {0} on {1} is not allowed. Take note that duration should not be equal or greater than {2} hours.<br />",
                                                    selectedRecord.EmpNo,
                                                    Convert.ToDateTime(selectedRecord.DT).ToString("dd-MMM-yyyy"),
                                                    maxOTMinutes / 60));
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        throw new Exception(string.Format(@"The specified overtime duration for Employee No. {0} is incorrect. Take note that duration should be equal or less than the actual duration based on the value of OT Start Time and OT End Time.<br />", selectedRecord.EmpNo));
                                    }
                                    #endregion
                                }
                                #endregion

                                // Save current record to session
                                this.SelectedOvertimeRecord = selectedRecord;
                            }
                        }
                        #endregion

                        #region Set OT duration value into 24-hour time format
                        RadLabel lblDuration = (RadLabel)item["OTDurationHour"].FindControl("lblDuration");
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
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void txtRemarks_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox txtRemarks = (TextBox)sender;
                if (txtRemarks != null)
                {
                    GridDataItem item = txtRemarks.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        #region Save selected record to session
                        // Reset session
                        this.SelectedOvertimeRecord = null;

                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        // Save current selected datagrid row
                        if (autoID > 0 && this.OTRequisitionList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null &&
                                selectedRecord.IsForHRValidation)
                            {
                                #region Check if record has been modified by HR Validator
                                if (selectedRecord.IsForHRValidation)
                                {
                                    selectedRecord.IsAttendanceRemarksModified = CheckIfRecordHasChanged(txtRemarks.Text.Trim(), "AttendanceRemarks", selectedRecord.OTRequestNo, this.OTRequisitionListOrig);
                                    selectedRecord.AttendanceRemarks = txtRemarks.Text.Trim();
                                }
                                #endregion

                                // Save current record to session
                                this.SelectedOvertimeRecord = selectedRecord;
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

        protected void cboFiscalYear_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                #region Reset gauge and chart data
                this.tdTotalBudget.InnerHtml = "- Not found -";
                this.tdTotalConsumed.InnerHtml = "- Not found -";
                this.tdTotalBalance.InnerHtml = "- Not found -";

                this.gaugeOTBudget.Pointer.Value = 0;
                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "black";
                ResetOvertimeChart();
                #endregion

                int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
                string costCenter = string.Empty;

                if (this.cboCostCenter.CheckedItems.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (RadComboBoxItem item in this.cboCostCenter.CheckedItems)
                    {
                        if (item.Value == UIHelper.CONST_COMBO_EMTYITEM_ID ||
                            string.IsNullOrEmpty(item.Value))
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.Value);
                        else
                            sb.Append(string.Format(",{0}", item.Value));
                    }

                    costCenter = sb.ToString().Trim();
                }

                if (fiscalYear > 0)
                {
                    if (this.cboUnitType.SelectedValue == "valAmount")
                        GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTTotalBudgetAndActualAmount, fiscalYear, costCenter);
                    else
                        GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTBudgetAndActualHours, fiscalYear, costCenter);
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkShowBreakdown_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkShowBreakdown.Checked)
            {
                this.tdOTBudgetBreakdown.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.rblOTBreakdownType_SelectedIndexChanged(this.rblOTBreakdownType, new EventArgs());
            }
            else
            {
                ResetOvertimeChart();
                this.tdOTBudgetBreakdown.Style[HtmlTextWriterStyle.Display] = "none";
            }
        }

        protected void rblOTBreakdownType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
            string unitType = this.cboUnitType.SelectedValue;
            string costCenter = string.Empty; //this.cboCostCenter.SelectedValue != UIHelper.CONST_COMBO_EMTYITEM_ID ? cboCostCenter.SelectedValue : string.Empty;

            #region Get the checked cost centers
            if (this.cboCostCenter.CheckedItems.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (RadComboBoxItem item in this.cboCostCenter.CheckedItems)
                {
                    if (item.Value == UIHelper.CONST_COMBO_EMTYITEM_ID ||
                        string.IsNullOrEmpty(item.Value))
                        continue;

                    if (sb.Length == 0)
                        sb.Append(item.Value);
                    else
                        sb.Append(string.Format(",{0}", item.Value));
                }

                costCenter = sb.ToString().Trim();
            }
            #endregion

            if (this.rblOTBreakdownType.SelectedValue == "valCostCenter")
            {
                #region Show overtime breakdown by cost center
                this.tdOTBreakdownByPeriod.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdOTBreakdownByCostCenter.Style[HtmlTextWriterStyle.Display] = string.Empty;
                //this.trCostCenterFilter.Style[HtmlTextWriterStyle.Display] = "none";

                if (fiscalYear > 0)
                {
                    PopulateChartCostCenter(fiscalYear, costCenter);
                    PopulateOTBudgetByCostCenter(fiscalYear, unitType, costCenter);
                    PopulateOTActualsByCostCenter(fiscalYear, unitType, costCenter);

                    if (unitType == "valAmount")
                    {
                        this.chartOTBudgetCostCenter.PlotArea.YAxis.TitleAppearance.Text = "Amount (BD)";
                        if (this.IsOTBudgetAdmin)
                            this.chartOTBudgetCostCenter.PlotArea.YAxis.MaxValue = 50000;
                        else
                            this.chartOTBudgetCostCenter.PlotArea.YAxis.MaxValue = 30000;
                    }
                    else
                    {
                        this.chartOTBudgetCostCenter.PlotArea.YAxis.TitleAppearance.Text = "Hours";
                        if (this.IsOTBudgetAdmin)
                            this.chartOTBudgetCostCenter.PlotArea.YAxis.MaxValue = 20000;
                        else
                            this.chartOTBudgetCostCenter.PlotArea.YAxis.MaxValue = 10000;
                    }
                }
                #endregion
            }
            else
            {
                #region Show overtime breakdown by period
                this.tdOTBreakdownByPeriod.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.tdOTBreakdownByCostCenter.Style[HtmlTextWriterStyle.Display] = "none";
                //this.trCostCenterFilter.Style[HtmlTextWriterStyle.Display] = string.Empty;

                if (fiscalYear > 0)
                {
                    PopulateOTBudgetByMonth(fiscalYear, unitType, costCenter);
                    PopulateOTActualsByMonth(fiscalYear, unitType, costCenter);

                    if (unitType == "valAmount")
                    {
                        this.chartOTBudget.PlotArea.YAxis.TitleAppearance.Text = "Amount (BD)";
                        if (this.IsOTBudgetAdmin)
                            this.chartOTBudget.PlotArea.YAxis.MaxValue = 100000;
                        else
                            this.chartOTBudget.PlotArea.YAxis.MaxValue = 50000;
                    }
                    else
                    {
                        this.chartOTBudget.PlotArea.YAxis.TitleAppearance.Text = "Hours";
                        if (this.IsOTBudgetAdmin)
                            this.chartOTBudget.PlotArea.YAxis.MaxValue = 30000;
                        else
                            this.chartOTBudget.PlotArea.YAxis.MaxValue = 10000;
                    }
                }
                #endregion
            }
        }

        protected void cboUnitType_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            this.cboFiscalYear_SelectedIndexChanged(this.cboFiscalYear, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFiscalYear.Text, string.Empty, this.cboFiscalYear.SelectedValue, string.Empty));
        }

        protected void cboOTCostCenter_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
                string unitType = this.cboUnitType.SelectedValue;
                string costCenter = this.cboOTCostCenter.SelectedValue != UIHelper.CONST_COMBO_EMTYITEM_ID ? cboOTCostCenter.SelectedValue : string.Empty;

                PopulateOTBudgetByMonth(fiscalYear, unitType, costCenter);
                PopulateOTActualsByMonth(fiscalYear, unitType, costCenter);

                if (unitType == "valAmount")
                {
                    this.chartOTBudget.PlotArea.YAxis.TitleAppearance.Text = "Amount (BD)";
                    if (this.IsOTBudgetAdmin)
                        this.chartOTBudget.PlotArea.YAxis.MaxValue = 100000;
                    else
                        this.chartOTBudget.PlotArea.YAxis.MaxValue = 50000;
                }
                else
                {
                    this.chartOTBudget.PlotArea.YAxis.TitleAppearance.Text = "Hours";
                    if (this.IsOTBudgetAdmin)
                        this.chartOTBudget.PlotArea.YAxis.MaxValue = 30000;
                    else
                        this.chartOTBudget.PlotArea.YAxis.MaxValue = 10000;
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboCostCenter_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            //try
            //{
            //    int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
            //    string unitType = this.cboUnitType.SelectedValue;
            //    string costCenter = this.cboCostCenter.SelectedValue != UIHelper.CONST_COMBO_EMTYITEM_ID ? cboCostCenter.SelectedValue : string.Empty;

            //    if (this.cboUnitType.SelectedValue == "valAmount")
            //        GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTTotalBudgetAndActualAmount, fiscalYear, costCenter);
            //    else
            //        GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTBudgetAndActualHours, fiscalYear, costCenter);
            //}
            //catch (Exception ex)
            //{
            //    DisplayFormLevelError(ex.Message.ToString());
            //}
        }

        protected void cboCostCenter_ItemChecked(object sender, RadComboBoxItemEventArgs e)
        {
            try
            {
                int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
                string unitType = this.cboUnitType.SelectedValue;
                string costCenter = string.Empty;

                if (this.cboCostCenter.CheckedItems.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (RadComboBoxItem item in this.cboCostCenter.CheckedItems)
                    {
                        if (item.Value == UIHelper.CONST_COMBO_EMTYITEM_ID ||
                            string.IsNullOrEmpty(item.Value))
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.Value);
                        else
                            sb.Append(string.Format(",{0}", item.Value));
                    }

                    costCenter = sb.ToString().Trim();
                }

                if (this.cboUnitType.SelectedValue == "valAmount")
                    GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTTotalBudgetAndActualAmount, fiscalYear, costCenter);
                else
                    GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTBudgetAndActualHours, fiscalYear, costCenter);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboCostCenter_CheckAllCheck(object sender, RadComboBoxCheckAllCheckEventArgs e)
        {
            try
            {
                int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
                string unitType = this.cboUnitType.SelectedValue;
                string costCenter = string.Empty;

                if (this.cboCostCenter.CheckedItems.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (RadComboBoxItem item in this.cboCostCenter.CheckedItems)
                    {
                        if (item.Value == UIHelper.CONST_COMBO_EMTYITEM_ID ||
                            string.IsNullOrEmpty(item.Value))
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.Value);
                        else
                            sb.Append(string.Format(",{0}", item.Value));
                    }

                    costCenter = sb.ToString().Trim();
                }

                if (this.cboUnitType.SelectedValue == "valAmount")
                    GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTTotalBudgetAndActualAmount, fiscalYear, costCenter);
                else
                    GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTBudgetAndActualHours, fiscalYear, costCenter);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkShow12HourShift_CheckedChanged(object sender, EventArgs e)
        {
            GetOvertimeRequest(true);
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls            
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());
            this.chkShow12HourShift.Checked = false;

            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboCostCenter.ClearCheckedItems();
            this.cboFilterOption.SelectedValue = UIHelper.OvertimeFilter.OTUNPROCSD.ToString();

            // Overtime Statistics controls
            this.cboFiscalYear.SelectedIndex = -1;
            this.cboFiscalYear.Text = string.Empty;
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
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.OTRequisitionList.Clear();
            this.OTReasonList.Clear();
            this.OvertimeFilterOptionList.Clear();
            this.CheckedOTRequisitionList.Clear();
            this.OTRequisitionApprovalList.Clear();
            this.OTRequisitionListOrig.Clear();
            this.CostCenterList.Clear();
            this.FiscalYearComboList.Clear();
            this.OvertimeCostCenterList.Clear();
            this.OvertimeBudgetList.Clear();
            this.OvertimeActualList.Clear();
            this.AllocatedCostCenterList.Clear();
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
            ViewState["IsOTWFApprove"] = null;
            ViewState["IsOTApprovalHeaderClicked"] = null;
            ViewState["IsOTWFApprovalHeaderClicked"] = null;
            ViewState["IsHRValidator"] = null;
            ViewState["IsOTBudgetAdmin"] = null;
            ViewState["OvertimeBudgetData"] = null;
            ViewState["OvertimeActualsData"] = null;
            ViewState["CanViewOTStatistic"] = null;            

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.OvertimeApprovalStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.OvertimeApprovalStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.OvertimeApprovalStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.OvertimeApprovalStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("OTRequisitionList"))
                this.OTRequisitionList = this.OvertimeApprovalStorage["OTRequisitionList"] as List<EmployeeAttendanceEntity>;
            else
                this.OTRequisitionList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("OTReasonList"))
                this.OTReasonList = this.OvertimeApprovalStorage["OTReasonList"] as List<UDCEntity>;
            else
                this.OTReasonList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("SelectedOvertimeRecord"))
                this.SelectedOvertimeRecord = this.OvertimeApprovalStorage["SelectedOvertimeRecord"] as EmployeeAttendanceEntity;
            else
                this.SelectedOvertimeRecord = null;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTApprove"))
                this.IsOTApprove = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTApprove"]);
            else
                this.IsOTApprove = false;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTWFApprove"))
                this.IsOTWFApprove = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTWFApprove"]);
            else
                this.IsOTWFApprove = false;

            if (this.OvertimeApprovalStorage.ContainsKey("IsHRValidator"))
                this.IsHRValidator = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsHRValidator"]);
            else
                this.IsHRValidator = false;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTApprovalHeaderClicked"))
                this.IsOTApprovalHeaderClicked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTApprovalHeaderClicked"]);
            else
                this.IsOTApprovalHeaderClicked = false;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTWFApprovalHeaderClicked"))
                this.IsOTWFApprovalHeaderClicked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTWFApprovalHeaderClicked"]);
            else
                this.IsOTWFApprovalHeaderClicked = false;

            if (this.OvertimeApprovalStorage.ContainsKey("OvertimeFilterOptionList"))
                this.OvertimeFilterOptionList = this.OvertimeApprovalStorage["OvertimeFilterOptionList"] as List<UserDefinedCodes>;
            else
                this.OvertimeFilterOptionList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("CheckedOTRequisitionList"))
                this.CheckedOTRequisitionList = this.OvertimeApprovalStorage["CheckedOTRequisitionList"] as List<EmployeeAttendanceEntity>;
            else
                this.CheckedOTRequisitionList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("OTRequisitionApprovalList"))
                this.OTRequisitionApprovalList = this.OvertimeApprovalStorage["OTRequisitionApprovalList"] as List<EmployeeAttendanceEntity>;
            else
                this.OTRequisitionApprovalList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("OTRequisitionListOrig"))
                this.OTRequisitionListOrig = this.OvertimeApprovalStorage["OTRequisitionListOrig"] as List<EmployeeAttendanceEntity>;
            else
                this.OTRequisitionListOrig = null;

            if (this.OvertimeApprovalStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.OvertimeApprovalStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("FiscalYearComboList"))
                this.FiscalYearComboList = this.OvertimeApprovalStorage["FiscalYearComboList"] as List<OvertimeBudgetEntity>;
            else
                this.FiscalYearComboList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTBudgetAdmin"))
                this.IsOTBudgetAdmin = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTBudgetAdmin"]);
            else
                this.IsOTBudgetAdmin = false;

            if (this.OvertimeApprovalStorage.ContainsKey("AllocatedCostCenterList"))
                this.AllocatedCostCenterList = this.OvertimeApprovalStorage["AllocatedCostCenterList"] as List<string>;
            else
                this.AllocatedCostCenterList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("CanViewOTStatistic"))
                this.CanViewOTStatistic = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["CanViewOTStatistic"]);
            else
                this.CanViewOTStatistic = false;

            if (this.OvertimeApprovalStorage.ContainsKey("AttendanceList"))
                this.AttendanceList = this.OvertimeApprovalStorage["AttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceList = null;

            if (this.IsOTBudgetAdmin)
            {
                if (this.OvertimeApprovalStorage.ContainsKey("OvertimeBudgetData"))
                    this.OvertimeBudgetData = this.OvertimeApprovalStorage["OvertimeBudgetData"] as OvertimeBudgetEntity;
                else
                    this.OvertimeBudgetData = null;

                if (this.OvertimeApprovalStorage.ContainsKey("OvertimeActualsData"))
                    this.OvertimeActualsData = this.OvertimeApprovalStorage["OvertimeActualsData"] as OvertimeBudgetEntity;
                else
                    this.OvertimeActualsData = null;

                if (this.OvertimeApprovalStorage.ContainsKey("OvertimeCostCenterList"))
                    this.OvertimeCostCenterList = this.OvertimeApprovalStorage["OvertimeCostCenterList"] as List<OvertimeBudgetEntity>;
                else
                    this.OvertimeCostCenterList = null;

                if (this.OvertimeApprovalStorage.ContainsKey("OvertimeBudgetList"))
                    this.OvertimeBudgetList = this.OvertimeApprovalStorage["OvertimeBudgetList"] as List<OvertimeBudgetEntity>;
                else
                    this.OvertimeBudgetList = null;

                if (this.OvertimeApprovalStorage.ContainsKey("OvertimeActualList"))
                    this.OvertimeActualList = this.OvertimeApprovalStorage["OvertimeActualList"] as List<OvertimeBudgetEntity>;
                else
                    this.OvertimeActualList = null;
            }

            FillComboData(false);
            #endregion

            #region Restore control values  

            if (this.OvertimeApprovalStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.OvertimeApprovalStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.OvertimeApprovalStorage.ContainsKey("cboCostCenterCheckedItems"))
            {
                try
                {
                    dynamic checkedItems = this.OvertimeApprovalStorage["cboCostCenterCheckedItems"];
                    if (checkedItems != null)
                    {
                        this.cboCostCenter.ClearCheckedItems();
                        foreach (RadComboBoxItem item in checkedItems)
                        {
                            RadComboBoxItem filteredItem = this.cboCostCenter.Items.Where(a => a.Value == item.Value).FirstOrDefault();
                            if (filteredItem != null)
                                filteredItem.Checked = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.cboCostCenter.ClearCheckedItems();
                }
            }
            else
            {
                this.cboCostCenter.ClearCheckedItems();
                this.cboCostCenter.SelectedIndex = -1;
                this.cboCostCenter.Text = string.Empty;
            }

            

            if (this.OvertimeApprovalStorage.ContainsKey("cboMonth"))
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboMonth"]);
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.OvertimeApprovalStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.OvertimeApprovalStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OvertimeApprovalStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.OvertimeApprovalStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OvertimeApprovalStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.OvertimeApprovalStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["chkPayPeriod"]);
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

            if (this.OvertimeApprovalStorage.ContainsKey("cboFilterOption"))
                this.cboFilterOption.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboFilterOption"]);
            else
            {
                this.cboFilterOption.Text = string.Empty;
                this.cboFilterOption.SelectedIndex = -1;
            }

            if (this.OvertimeApprovalStorage.ContainsKey("rblAssignedTo"))
                this.rblAssignedTo.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["rblAssignedTo"]);
            else
                this.rblAssignedTo.ClearSelection();

            if (this.rblAssignedTo.SelectedValue == "0")    // All
            {
                this.txtAssigneeEmpNo.Visible = false;
                this.btnFindAssignee.Visible = false;

                // Initialize buttons
                this.btnSubmitApproval.Visible = false;
                this.btnReassign.Visible = true;
                this.btnAssignToMe.Visible = true;
            }
            else if (this.rblAssignedTo.SelectedValue == "2")    // Others
            {
                this.btnFindAssignee.Visible = true;
                this.txtAssigneeEmpNo.Visible = true;
                this.txtAssigneeEmpNo.Text = string.Empty;
                this.txtAssigneeEmpNo.Focus();

                // Initialize buttons
                this.btnSubmitApproval.Visible = false;
                this.btnReassign.Visible = false;
                this.btnAssignToMe.Visible = true;
            }
            else
            {
                this.txtAssigneeEmpNo.Visible = false;
                this.btnFindAssignee.Visible = false;

                // Initialize buttons
                this.btnSubmitApproval.Visible = true;
                this.btnReassign.Visible = true;
                this.btnAssignToMe.Visible = false;
            }

            if (this.OvertimeApprovalStorage.ContainsKey("rblOTBreakdownType"))
                this.rblOTBreakdownType.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["rblOTBreakdownType"]);
            else
                this.rblOTBreakdownType.ClearSelection();

            if (this.OvertimeApprovalStorage.ContainsKey("chkShow12HourShift"))
                this.chkShow12HourShift.Checked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["chkShow12HourShift"]);
            else
                this.chkShow12HourShift.Checked = false;

            #region Restore overtime budget details
            if (this.CanViewOTStatistic)
            {
                if (this.OvertimeApprovalStorage.ContainsKey("cboUnitType"))
                    this.cboUnitType.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboUnitType"]);
                else
                {
                    this.cboUnitType.SelectedIndex = 0;
                }

                if (this.OvertimeApprovalStorage.ContainsKey("cboFiscalYear"))
                    this.cboFiscalYear.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboFiscalYear"]);
                else
                {
                    this.cboFiscalYear.Text = string.Empty;
                    this.cboFiscalYear.SelectedIndex = -1;
                }
                this.cboFiscalYear_SelectedIndexChanged(this.cboFiscalYear, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFiscalYear.Text, string.Empty, this.cboFiscalYear.SelectedValue, string.Empty));

                if (this.OvertimeApprovalStorage.ContainsKey("tdTotalBudget"))
                    this.tdTotalBudget.InnerHtml = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["tdTotalBudget"]);
                else
                    this.tdTotalBudget.InnerHtml = string.Empty;

                if (this.OvertimeApprovalStorage.ContainsKey("tdTotalConsumed"))
                    this.tdTotalConsumed.InnerHtml = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["tdTotalConsumed"]);
                else
                    this.tdTotalConsumed.InnerHtml = string.Empty;

                if (this.OvertimeApprovalStorage.ContainsKey("tdTotalBalance"))
                    this.tdTotalBalance.InnerHtml = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["tdTotalBalance"]);
                else
                    this.tdTotalBalance.InnerHtml = string.Empty;

                if (this.OvertimeApprovalStorage.ContainsKey("gaugeOTBudget"))
                    this.gaugeOTBudget.Pointer.Value = UIHelper.ConvertObjectToDecimal(this.OvertimeApprovalStorage["gaugeOTBudget"]);
                else
                    this.gaugeOTBudget.Pointer.Value = 0;

                if (this.OvertimeApprovalStorage.ContainsKey("chkShowBreakdown"))
                    this.chkShowBreakdown.Checked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["chkShowBreakdown"]);
                else
                    this.chkShowBreakdown.Checked = false;
                this.chkShowBreakdown_CheckedChanged(this.chkShowBreakdown, new EventArgs());
            }
            #endregion

            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex > 0 ? this.CurrentPageIndex - 1 : 0;
            this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex > 0 ? this.CurrentPageIndex - 1 : 0;
            this.gridSearchResults.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag, string sourceControl = "")
        {
            this.OvertimeApprovalStorage.Clear();
            this.OvertimeApprovalStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.OvertimeApprovalStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.OvertimeApprovalStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.OvertimeApprovalStorage.Add("cboCostCenterCheckedItems", this.cboCostCenter.CheckedItems);
            this.OvertimeApprovalStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            this.OvertimeApprovalStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.OvertimeApprovalStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.OvertimeApprovalStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.OvertimeApprovalStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.OvertimeApprovalStorage.Add("cboFilterOption", this.cboFilterOption.SelectedValue);
            this.OvertimeApprovalStorage.Add("rblAssignedTo", this.rblAssignedTo.SelectedValue);
            this.OvertimeApprovalStorage.Add("chkShow12HourShift", this.chkShow12HourShift.Checked);

            #region Store overtime budget details
            if (this.CanViewOTStatistic)
            {
                this.OvertimeApprovalStorage.Add("cboFiscalYear", this.cboFiscalYear.SelectedValue);
                this.OvertimeApprovalStorage.Add("cboUnitType", this.cboUnitType.SelectedValue);
                this.OvertimeApprovalStorage.Add("tdTotalBudget", this.tdTotalBudget.InnerHtml);
                this.OvertimeApprovalStorage.Add("tdTotalConsumed", this.tdTotalConsumed.InnerHtml);
                this.OvertimeApprovalStorage.Add("tdTotalBalance", this.tdTotalBalance.InnerHtml);
                this.OvertimeApprovalStorage.Add("gaugeOTBudget", this.gaugeOTBudget.Pointer.Value);
                this.OvertimeApprovalStorage.Add("chkShowBreakdown", this.chkShowBreakdown.Checked);
                this.OvertimeApprovalStorage.Add("rblOTBreakdownType", this.rblOTBreakdownType.SelectedValue);
            }
            #endregion
            #endregion

            #region Save Query String values to collection
            this.OvertimeApprovalStorage.Add("CallerForm", this.CallerForm);
            this.OvertimeApprovalStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.OvertimeApprovalStorage.Add("SourceControl", sourceControl);
            this.OvertimeApprovalStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.OvertimeApprovalStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.OvertimeApprovalStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.OvertimeApprovalStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.OvertimeApprovalStorage.Add("OTRequisitionList", this.OTRequisitionList);
            this.OvertimeApprovalStorage.Add("OTReasonList", this.OTReasonList);
            this.OvertimeApprovalStorage.Add("SelectedOvertimeRecord", this.SelectedOvertimeRecord);
            this.OvertimeApprovalStorage.Add("IsOTApprove", this.IsOTApprove);
            this.OvertimeApprovalStorage.Add("IsOTWFApprove", this.IsOTWFApprove);
            this.OvertimeApprovalStorage.Add("IsHRValidator", this.IsHRValidator);
            this.OvertimeApprovalStorage.Add("IsOTApprovalHeaderClicked", this.IsOTApprovalHeaderClicked);
            this.OvertimeApprovalStorage.Add("IsOTWFApprovalHeaderClicked", this.IsOTWFApprovalHeaderClicked);
            this.OvertimeApprovalStorage.Add("OvertimeFilterOptionList", this.OvertimeFilterOptionList);
            this.OvertimeApprovalStorage.Add("CheckedOTRequisitionList", this.CheckedOTRequisitionList);
            this.OvertimeApprovalStorage.Add("OTRequisitionApprovalList", this.OTRequisitionApprovalList);
            this.OvertimeApprovalStorage.Add("OTRequisitionListOrig", this.OTRequisitionListOrig);
            this.OvertimeApprovalStorage.Add("CostCenterList", this.CostCenterList);
            this.OvertimeApprovalStorage.Add("FiscalYearComboList", this.FiscalYearComboList);
            this.OvertimeApprovalStorage.Add("AllocatedCostCenterList", this.AllocatedCostCenterList);
            this.OvertimeApprovalStorage.Add("CanViewOTStatistic", this.CanViewOTStatistic);
            this.OvertimeApprovalStorage.Add("AttendanceList", this.AttendanceList);

            this.OvertimeApprovalStorage.Add("IsOTBudgetAdmin", this.IsOTBudgetAdmin);
            if (this.IsOTBudgetAdmin)
            {
                this.OvertimeApprovalStorage.Add("OvertimeBudgetData", this.OvertimeBudgetData);
                this.OvertimeApprovalStorage.Add("OvertimeActualsData", this.OvertimeActualsData);
                this.OvertimeApprovalStorage.Add("OvertimeCostCenterList", this.OvertimeCostCenterList);
                this.OvertimeApprovalStorage.Add("OvertimeBudgetList", this.OvertimeBudgetList);
                this.OvertimeApprovalStorage.Add("OvertimeActualList", this.OvertimeActualList);
            }
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
            FillCostCenterCombo(reloadFromDB);

            if (this.IsOTBudgetAdmin ||
                this.AllocatedCostCenterList.Count > 0)
            {
                // Set the flag that determines whether user can view the overtime statistics (Note: Only department managers and members of "OTBUDGTADM" distribution group can view OT statistics)
                this.CanViewOTStatistic = true;

                this.tdOTBudgetSummary.Style[HtmlTextWriterStyle.Display] = string.Empty;
                FillDataToFiscalYearCombo(reloadFromDB, string.Empty, DateTime.Now.Year.ToString());

                // Set the default breakdown type
                this.rblOTBreakdownType.SelectedValue = "valPeriod";

                if (this.IsOTBudgetAdmin ||
                    this.CostCenterList.Count > 13)
                {
                    this.chartOTBudgetCostCenter.Height = new Unit(1000, UnitType.Pixel);
                }
                else
                    this.chartOTBudgetCostCenter.Height = new Unit(580, UnitType.Pixel);
            }
            else
            {
                this.CanViewOTStatistic = false;
                this.tdOTBudgetSummary.Style[HtmlTextWriterStyle.Display] = "none";
            }
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

        private void ProcessAssignToMeAction(List<EmployeeAttendanceEntity> selectedRequisitionList)
        {
            try
            {
                if (selectedRequisitionList.Count == 0)
                    return;

                #region Initialize variables                                
                string error = string.Empty;
                string innerError = string.Empty;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string userEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                string userEmailAddress = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]);
                DALProxy proxy = new DALProxy();
                #endregion

                int counter = 0;
                foreach (EmployeeAttendanceEntity item in selectedRequisitionList)
                {
                    DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.ReassignToOtherApprover),
                        item.OTRequestNo, item.AutoID, userID, userEmpNo, userEmpName, userEmpNo, userEmpName, null, null, item.RequestSubmissionDate, null, 
                        ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) ||
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }

                    // Increment the counter
                    counter++;
                }

                #region Show success notification and refresh the grid
                this.rblAssignedTo.SelectedValue = "1";     // Me
                this.rblAssignedTo_SelectedIndexChanged(this.rblAssignedTo, new EventArgs());

                // Refresh the grid
                //this.btnSearch_Click(this.btnSearch, new EventArgs());
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ProcessApprovalAction(List<EmployeeAttendanceEntity> selectedRequisitionList)
        {
            try
            {
                // Get WCF Instance
                if (selectedRequisitionList.Count == 0)
                    return;

                #region Initialize variables                                
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int assigneeEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string assigneeEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string assigneeUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                StringBuilder sb = new StringBuilder();
                bool isWFCompleted = false;
                bool useMultithread = UIHelper.ConvertNumberToBolean(ConfigurationManager.AppSettings["UseMultithread"]);
                string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_OVERTIME_ENTRY.Replace("~", string.Empty));
                #endregion

                #region Update the database
                int emailCounter = 0;
                foreach (EmployeeAttendanceEntity item in selectedRequisitionList)
                {
                    DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.GetNextWFActivity),
                        item.OTRequestNo, item.AutoID, assigneeUserID, assigneeEmpNo, assigneeEmpName, assigneeEmpNo, assigneeEmpName, 
                        true, item.ApproverRemarks, item.RequestSubmissionDate, item.AttendanceRemarks, ref error, ref innerError);
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
                        if (workflowResult != null)
                        {
                            #region Check if overtime details have been modified
                            if (item.IsModifiedByHR)
                            {
                                DatabaseSaveResult dbResult = proxy.SubmitOvertimeChanges(item.OTRequestNo, item.OTReasonCode, item.AttendanceRemarks, assigneeEmpNo, assigneeEmpName, assigneeUserID,
                                    item.OTApprovalCode, item.MealVoucherEligibilityCode, item.OTDurationMinute, ref error, ref innerError);
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
                                    if (dbResult != null)
                                    {
                                        #region Notify the originator about the changes in the overtime request

                                        #region Build email contents
                                        sb.Clear();
                                        sb.AppendLine(string.Format(@"1. <b>Employee Name:</b> {0}", item.EmpName));
                                        sb.AppendLine(@"<br /> <br />");
                                        sb.AppendLine(string.Format(@"2. <b>Position:</b> {0}", !string.IsNullOrEmpty(item.Position) ? item.Position : "Unknown"));
                                        sb.AppendLine(@"<br /> <br />");
                                        sb.AppendLine(string.Format(@"3. <b>Cost Center:</b> {0}", item.CostCenterFullName));
                                        sb.AppendLine(@"<br /> <br />");
                                        sb.AppendLine(string.Format(@"4. <b>Date:</b> {0}", UIHelper.ConvertObjectToDateString(item.DT)));
                                        sb.AppendLine(@"<br /> <br />");

                                        if (item.IsMealVoucherEligibilityModified)
                                            sb.AppendLine(string.Format(@"5. <b>Meal Voucher Approved?:</b> <font color='Red'>{0}</font>", item.MealVoucherEligibility));
                                        else
                                            sb.AppendLine(string.Format(@"5. <b>Meal Voucher Approved?:</b> {0}", item.MealVoucherEligibility));

                                        sb.AppendLine(@"<br /> <br />");

                                        if (item.IsOTApprovalCodeModified)
                                            sb.AppendLine(string.Format(@"6. <b>OT Approved?:</b> <font color='Red'>{0}</font>", item.OTApprovalDesc));
                                        else
                                            sb.AppendLine(string.Format(@"6. <b>OT Approved?:</b> {0}", item.OTApprovalDesc));

                                        sb.AppendLine(@"<br /> <br />");

                                        if (item.OTApprovalCode == "Y")
                                        {
                                            if (item.IsOTDurationModified)
                                            {
                                                sb.AppendLine(string.Format(@"7. <b>OT Start Time:</b> <font color='Red'>{0}</font>", UIHelper.ConvertObjectToTimeString(dbResult.OTStartTime)));
                                                sb.AppendLine(@"<br /> <br />");
                                                sb.AppendLine(string.Format(@"8. <b>OT End Time:</b> <font color='Red'>{0}</font>", UIHelper.ConvertObjectToTimeString(dbResult.OTEndTime)));
                                                sb.AppendLine(@"<br /> <br />");
                                                sb.AppendLine(string.Format(@"9. <b>OT Duration:</b> <font color='Red'>{0}</font>", UIHelper.ConvertMinutesToHour(UIHelper.ConvertObjectToInt(item.OTDurationMinute))));
                                            }
                                            else
                                            {
                                                sb.AppendLine(string.Format(@"7. <b>OT Start Time:</b> {0}", UIHelper.ConvertObjectToTimeString(dbResult.OTStartTime)));
                                                sb.AppendLine(@"<br /> <br />");
                                                sb.AppendLine(string.Format(@"8. <b>OT End Time:</b> {0}", UIHelper.ConvertObjectToTimeString(dbResult.OTEndTime)));
                                                sb.AppendLine(@"<br /> <br />");
                                                sb.AppendLine(string.Format(@"9. <b>OT Duration:</b> {0}", UIHelper.ConvertMinutesToHour(UIHelper.ConvertObjectToInt(item.OTDurationMinute))));
                                            }

                                            sb.AppendLine(@"<br /> <br />");
                                            sb.AppendLine(string.Format(@"10. <b>OT Type:</b> {0}", dbResult.OTType));                                            
                                        }
                                        else
                                        {
                                            sb.AppendLine(string.Format(@"7. <b>OT Start Time:</b> {0}", "N/A"));
                                            sb.AppendLine(@"<br /> <br />");
                                            sb.AppendLine(string.Format(@"8. <b>OT End Time:</b> {0}", "N/A"));
                                            sb.AppendLine(@"<br /> <br />");
                                            sb.AppendLine(string.Format(@"9. <b>OT Duration:</b> {0}", "0"));
                                            sb.AppendLine(@"<br /> <br />");
                                            sb.AppendLine(string.Format(@"10. <b>OT Type:</b> {0}", "N/A"));
                                        }

                                        sb.AppendLine(@"<br /> <br />");
                                        if (item.IsOTReasonModified)
                                            sb.AppendLine(string.Format(@"11. <b>OT Reason:</b> <font color='Red'>{0}</font>", item.OTReason));
                                        else
                                            sb.AppendLine(string.Format(@"11. <b>OT Reason:</b> {0}", item.OTReason));

                                        sb.AppendLine(@"<br /> <br />");
                                        sb.AppendLine(string.Format(@"12. <b>Remarks:</b> {0}", item.AttendanceRemarks));
                                        #endregion

                                        if (useMultithread)
                                        {
                                            // Send email in a separate thread                                                                                    
                                            Task.Factory.StartNew(() => SendNotificationToCreatorAboutOTChanges(item, sb.ToString().Trim(), assigneeEmpNo, dynamicEndpointAddress, assigneeEmpNo, assigneeUserID, true));
                                        }
                                        else
                                        {
                                            // Send email using the main application thread
                                            SendNotificationToCreatorAboutOTChanges(item, sb.ToString().Trim(), assigneeEmpNo, dynamicEndpointAddress, assigneeEmpNo, assigneeUserID);
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #endregion

                            #region Send notification to the creator if workflow is completed
                            if (workflowResult.IsWorkflowCompleted)
                            {
                                // Set the flag
                                isWFCompleted = true;

                                #region Build email contents                                                                
                                sb.Clear();
                                sb.AppendLine(string.Format("Please be informed that Overtime Requisition No. <b>{0}</b> has been processed completely and considered closed. Below summarize the details.", item.OTRequestNo));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"1. <b>Employee Name:</b> {0}", item.EmpName));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"2. <b>Position:</b> {0}", !string.IsNullOrEmpty(item.Position) ? item.Position : "Unknown"));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"3. <b>Cost Center:</b> {0}", item.CostCenterFullName));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"4. <b>Date:</b> {0}", UIHelper.ConvertObjectToDateString(item.DT)));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"5. <b>Meal Voucher Approved?:</b> {0}", item.MealVoucherEligibility));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"6. <b>OT Approved?:</b> {0}", item.OTApprovalDesc));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"7. <b>OT Start Time:</b> {0}", item.OTApprovalCode == "Y" ? UIHelper.ConvertObjectToTimeString(item.OTStartTime) : "-"));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"8. <b>OT End Time:</b> {0}", item.OTApprovalCode == "Y" ? UIHelper.ConvertObjectToTimeString(item.OTEndTime) : "-"));
                                sb.AppendLine(@"<br /> <br />");
                                sb.AppendLine(string.Format(@"9. <b>OT Duration:</b> {0}", item.OTApprovalCode == "Y" ? UIHelper.ConvertMinutesToHour(UIHelper.ConvertObjectToInt(item.OTDurationMinute)) : "0"));
                                #endregion

                                if (useMultithread)
                                {
                                    // Send email in a separate thread                                        
                                    Task.Factory.StartNew(() => SendNotificationToCreatorWFCompleted(item, sb.ToString().Trim(), dynamicEndpointAddress, assigneeEmpNo, assigneeUserID, true));
                                }
                                else
                                {
                                    // Send email using the main application thread
                                    SendNotificationToCreatorWFCompleted(item, sb.ToString().Trim(), dynamicEndpointAddress, assigneeEmpNo, assigneeUserID);
                                }
                            }
                            #endregion
                        }
                    }

                    // Increment the counter
                    emailCounter++;
                }
                #endregion

                #region Send notification to the assigned approver if workflow is not yet completed. (Note: Sending of system notification to the approver has been commented as per Helpdesk No. 85517)
                //if (!isWFCompleted)
                //{
                //    if (ProcessWorkflowEmail())
                //    {
                //        // Show success notification and refresh the grid
                //        //UIHelper.DisplayJavaScriptMessage(this, "Selected requisitions have been approved sucessfully!");
                //        //this.btnSearch_Click(this.btnSearch, new EventArgs());
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ProcessRejectionAction(List<EmployeeAttendanceEntity> rejectedOTList)
        {
            try
            {
                if (rejectedOTList.Count == 0)
                    return;

                #region Initialize variables                                
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int assigneeEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string assigneeEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string assigneeUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                bool useMultithread = UIHelper.ConvertNumberToBolean(ConfigurationManager.AppSettings["UseMultithread"]);
                string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_OVERTIME_ENTRY.Replace("~", string.Empty));
                #endregion

                #region Update the workflow
                foreach (EmployeeAttendanceEntity item in rejectedOTList)
                {
                    DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.GetNextWFActivity),
                        item.OTRequestNo, item.AutoID, assigneeUserID, assigneeEmpNo, assigneeEmpName, assigneeEmpNo, assigneeEmpName,
                        false, item.ApproverRemarks, item.RequestSubmissionDate, item.AttendanceRemarks, ref error, ref innerError);
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
                        StringBuilder sb = new StringBuilder();
                        string approverName = !string.IsNullOrEmpty(item.DistListDesc) ? item.DistListDesc : string.Format("({0}) {1}", assigneeEmpNo, assigneeEmpName);

                        #region Build the email content     
                        sb.AppendLine(string.Format(@"1. <b>Employee Name:</b> {0}", item.EmpName));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"2. <b>Position:</b> {0}", !string.IsNullOrEmpty(item.Position) ? item.Position : "Unknown"));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"3. <b>Cost Center:</b> {0}", item.CostCenterFullName));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"4. <b>Date:</b> {0}", UIHelper.ConvertObjectToDateString(item.DT)));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"5. <b>OT Start Time:</b> {0}", UIHelper.ConvertObjectToTimeString(item.OTStartTime)));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"6. <b>OT End Time:</b> {0}", UIHelper.ConvertObjectToTimeString(item.OTEndTime)));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"7. <b>OT Duration:</b> {0}", UIHelper.ConvertMinutesToHour(UIHelper.ConvertObjectToInt(item.OTDurationMinute))));
                        #endregion

                        if (useMultithread)
                        {
                            // Send email using separate thread
                            SendRejectionEmail(item, sb.ToString().Trim(), item.ApproverRemarks, approverName, assigneeEmpNo,
                                dynamicEndpointAddress, assigneeEmpNo, assigneeUserID, true);
                        }
                        else
                        {
                            // Send email using the main application thread
                            SendRejectionEmail(item, sb.ToString().Trim(), item.ApproverRemarks, approverName, assigneeEmpNo,
                                dynamicEndpointAddress, assigneeEmpNo, assigneeUserID);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void HoldOvertimeRequest(List<EmployeeAttendanceEntity> otRequisitionList)
        {
            try
            {
                if (otRequisitionList.Count == 0)
                    return;

                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;

                proxy.HoldOvertimeRequest(otRequisitionList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) ||
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool CheckIfRecordHasChanged(string fieldValue, string fieldToSearch, long otRequestNo, List<EmployeeAttendanceEntity> overtimeList)
        {
            bool result = false;

            try
            {
                if (otRequestNo > 0 && overtimeList.Count > 0)
                {
                    EmployeeAttendanceEntity searchRecord = overtimeList
                        .Where(a => a.OTRequestNo == otRequestNo)
                        .FirstOrDefault();
                    if (searchRecord != null)
                    {
                        if (fieldToSearch == "OTApprovalCode")
                            result = searchRecord.OTApprovalCode != fieldValue;
                        else if (fieldToSearch == "OTReasonCode")
                            result = searchRecord.OTReasonCode != fieldValue;
                        else if (fieldToSearch == "MealVoucherEligibility")
                            result = searchRecord.MealVoucherEligibilityCode != fieldValue;
                        else if (fieldToSearch == "AttendanceRemarks")
                            result = searchRecord.AttendanceRemarks != fieldValue;
                        else if (fieldToSearch == "OTDurationMinute")
                            result = searchRecord.OTDurationMinute.ToString() != fieldValue;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ResetOvertimeChart()
        {
            // Reset overtime budget series
            BarSeries barOTBudget = this.chartOTBudget.PlotArea.Series[0] as BarSeries;
            if (barOTBudget != null)
            {
                foreach (CategorySeriesItem item in barOTBudget.SeriesItems)
                {
                    item.Y = 0;
                }
            }

            // Reset overtime actuals series
            BarSeries barOTActuals = this.chartOTBudget.PlotArea.Series[1] as BarSeries;
            if (barOTActuals != null)
            {
                foreach (CategorySeriesItem item in barOTActuals.SeriesItems)
                {
                    item.Y = 0;
                }
            }
        }

        private void GetOTStatisticsData()
        {
            try
            {
                int fiscalYear = UIHelper.ConvertObjectToInt(this.cboFiscalYear.SelectedValue);
                string unitType = this.cboUnitType.SelectedValue;
                string costCenter = string.Empty;

                if (this.cboCostCenter.CheckedItems.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (RadComboBoxItem item in this.cboCostCenter.CheckedItems)
                    {
                        if (item.Value == UIHelper.CONST_COMBO_EMTYITEM_ID ||
                            string.IsNullOrEmpty(item.Value))
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.Value);
                        else
                            sb.Append(string.Format(",{0}", item.Value));
                    }

                    costCenter = sb.ToString().Trim();
                }

                if (this.cboUnitType.SelectedValue == "valAmount")
                    GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTTotalBudgetAndActualAmount, fiscalYear, costCenter);
                else
                    GetOTBudgetDetail(FetchOTBudgetStatisticType.GetOTBudgetAndActualHours, fiscalYear, costCenter);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion

        #region Database Access
        private void GetOvertimeRequest(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables          
                int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                byte assignTypeID = UIHelper.ConvertObjectToByte(this.rblAssignedTo.SelectedValue);                
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                bool show12HourShift = this.chkShow12HourShift.Checked;

                //string costCenter = this.cboCostCenter.SelectedValue;
                //if (costCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                //    costCenter = string.Empty;

                #region Get the checked cost centers
                string costCenter = string.Empty;
                if (this.cboCostCenter.CheckedItems.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (RadComboBoxItem item in this.cboCostCenter.CheckedItems)
                    {
                        if (item.Value == UIHelper.CONST_COMBO_EMTYITEM_ID ||
                            string.IsNullOrEmpty(item.Value))
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.Value);
                        else
                            sb.Append(string.Format(",{0}", item.Value));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                int assignedToEmpNo = UIHelper.ConvertObjectToInt(this.txtAssigneeEmpNo.Text);
                if (assignedToEmpNo.ToString().Length == 4)
                {
                    assignedToEmpNo += 10000000;

                    // Display Emp. No.
                    this.txtAssigneeEmpNo.Text = assignedToEmpNo.ToString();
                }

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";

                // Reset session variables
                ViewState["IsOTApprove"] = null;
                ViewState["IsOTWFApprove"] = null;
                ViewState["IsOTApprovalHeaderClicked"] = null;
                ViewState["IsOTWFApprovalHeaderClicked"] = null;
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.OTRequisitionList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetAssignedOvertimeRequest(currentUserEmpNo, assignTypeID, assignedToEmpNo, startDate, endDate, costCenter, empNo, show12HourShift, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (rawData != null && 
                            rawData.Count() > 0)
                        {
                            gridSource.AddRange(rawData);
                        }
                    }
                }

                // Store collection to session
                this.OTRequisitionList = gridSource;
                this.OTRequisitionListOrig = gridSource;
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
                List<CostCenterEntity> costCenterList2 = new List<CostCenterEntity>();

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
                            // Fill the dummy collection
                            costCenterList2.AddRange(comboSource.ToList());

                            #region Add blank item
                            //comboSource.Insert(0, new CostCenterEntity()
                            //{
                            //    CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                            //    CostCenterName = string.Empty,
                            //    CostCenterFullName = string.Empty
                            //});

                            costCenterList2.Insert(0, new CostCenterEntity()
                            {
                                CostCenter = string.Empty,
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

                    this.cboOTCostCenter.DataSource = costCenterList2;
                    this.cboOTCostCenter.DataTextField = "CostCenter";
                    this.cboOTCostCenter.DataValueField = "CostCenter";
                    this.cboOTCostCenter.DataBind();

                    // Enable employee search button 
                    this.btnFindEmployee.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillCostCenterComboOld(bool reloadFromDB = true)
        {
            try
            {
                // Initialize controls
                this.cboCostCenter.Items.Clear();
                this.cboCostCenter.Text = string.Empty;
                this.btnFindEmployee.Enabled = false;

                string userCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                List<CostCenterEntity> comboSource = new List<CostCenterEntity>();
                List<CostCenterEntity> filteredComboSource = null;

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
                            if (!this.IsOTBudgetAdmin && 
                                this.AllocatedCostCenterList.Count > 0)
                            {
                                #region Filter the list based on the allocated cost center for the current user
                                filteredComboSource = new List<CostCenterEntity>();
                                foreach (CostCenterEntity item in comboSource)
                                {
                                    if (this.AllocatedCostCenterList.Contains(item.CostCenter))
                                        filteredComboSource.Add(item);
                                }

                                if (filteredComboSource.Count > 0)
                                {
                                    // Add blank item
                                    filteredComboSource.Insert(0, new CostCenterEntity()
                                    {
                                        CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                                        CostCenterName = string.Empty,
                                        CostCenterFullName = string.Empty
                                    });

                                    // Store to session
                                    this.CostCenterList = filteredComboSource;
                                }
                                #endregion
                            }
                            else
                            {
                                #region Add blank item                                    
                                comboSource.Insert(0, new CostCenterEntity()
                                {
                                    CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                                    CostCenterName = string.Empty,
                                    CostCenterFullName = string.Empty
                                });
                                #endregion

                                // Store to session
                                this.CostCenterList = comboSource;
                            }
                        }
                    }
                }

                if (this.CostCenterList.Count > 0)
                {
                    this.cboCostCenter.DataSource = this.CostCenterList;
                    this.cboCostCenter.DataTextField = "CostCenterFullName";
                    this.cboCostCenter.DataValueField = "CostCenter";
                    this.cboCostCenter.DataBind();

                    //this.cboOTCostCenter.DataSource = this.CostCenterList.Where(a => a.CostCenter != UIHelper.CONST_COMBO_EMTYITEM_ID).ToList();
                    //this.cboOTCostCenter.DataTextField = "CostCenterName";
                    //this.cboOTCostCenter.DataValueField = "CostCenter";
                    //this.cboOTCostCenter.DataBind();                                       

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
                ShowErrorMessage(ex);
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

        private void SaveOvertime(List<EmployeeAttendanceEntity> OTRequisitionList)
        {
            try
            {
                if (OTRequisitionList.Count == 0)
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
                long otRequestNo = 0;
                #endregion

                #region Save to database
                List<EmployeeAttendanceEntity> otProcessedList = new List<EmployeeAttendanceEntity>();
                int recordCounter = 0;

                foreach (EmployeeAttendanceEntity item in OTRequisitionList)
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
                        recordCounter++;

                        #region Refresh the collection
                        EmployeeAttendanceEntity itemToRemove = this.OTRequisitionList
                            .Where(a => a.AutoID == item.AutoID)
                            .FirstOrDefault();
                        if (itemToRemove != null)
                            this.OTRequisitionList.Remove(itemToRemove);
                        #endregion

                        #region Submit OT approval in a separate process
                        //if (dbResult != null)
                        //{
                        //    if (dbResult.HasError &&
                        //        !string.IsNullOrEmpty(dbResult.ErrorDesc))
                        //    {
                        //        throw new Exception(dbResult.ErrorDesc);
                        //    }
                        //    else
                        //    {
                        //        // Get the identity seed value
                        //        otRequestNo = dbResult.OTRequestNo;

                        //        DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.CreateWorkflow),
                        //            otRequestNo, autoID, userID, userEmpNo, userEmpName, null, null, null, null, DateTime.Now.Date, ref error, ref innerError);
                        //        if (!string.IsNullOrEmpty(error) ||
                        //            !string.IsNullOrEmpty(innerError))
                        //        {
                        //            if (!string.IsNullOrEmpty(innerError))
                        //                throw new Exception(innerError, new Exception(innerError));
                        //            else
                        //                throw new Exception(error);
                        //        }
                        //    }
                        //}
                        #endregion

                        // Add the processed overtime record to the collection
                        otProcessedList.Add(item);
                    }
                }
                #endregion

                #region Initiate the workflow, send notification to the first approver
                if (ProcessWorkflowEmail())
                {
                    #region Refresh the OT collection by removing the processed records
                    if (otProcessedList.Count > 0)
                    {
                        foreach (EmployeeAttendanceEntity item in otProcessedList)
                        {
                            EmployeeAttendanceEntity itemToRemove = this.OTRequisitionList
                                .Where(a => a.OTRequestNo == item.OTRequestNo)
                                .FirstOrDefault();
                            if (itemToRemove != null)
                                this.OTRequisitionList.Remove(itemToRemove);
                        }
                    }

                    // Refresh the grid
                    RebindDataToGrid();
                    #endregion

                    //this.ReloadGridData = true;
                    //this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
                #endregion
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

        private List<EmployeeAttendanceEntity> GetAttendanceHistory(DateTime? startDate, DateTime? endDate, string costCenter, int empNo)
        {
            List<EmployeeAttendanceEntity> attendanceList = null;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetEmployeeAttendanceHistory(startDate, endDate, costCenter, empNo, ref error, ref innerError);
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
                        attendanceList = new List<EmployeeAttendanceEntity>();
                        attendanceList.AddRange(rawData);
                    }
                }

                return attendanceList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillDataToFiscalYearCombo(bool reloadFromDB, string costCenter = "", string defaultValue = "")
        {
            try
            {
                List<OvertimeBudgetEntity> comboSource = new List<OvertimeBudgetEntity>();

                if (this.FiscalYearComboList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.FiscalYearComboList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetOvertimeBudgetStatistics(Convert.ToByte(FetchOTBudgetStatisticType.GetFiscalYearList), 0, costCenter, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                    {
                        comboSource.AddRange(rawData.ToList());

                        // Add blank item
                        comboSource.Insert(0, new OvertimeBudgetEntity() { FiscalYear = 0, FiscalYearDesc = string.Empty });
                    }
                }

                // Save to session
                this.FiscalYearComboList = comboSource;

                #region Bind data to combobox
                this.cboFiscalYear.DataSource = this.FiscalYearComboList;
                this.cboFiscalYear.DataTextField = "FiscalYearDesc";
                this.cboFiscalYear.DataValueField = "FiscalYear";
                this.cboFiscalYear.DataBind();

                if (this.cboFiscalYear.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboFiscalYear.SelectedValue = defaultValue;
                    this.cboFiscalYear_SelectedIndexChanged(this.cboFiscalYear, new RadComboBoxSelectedIndexChangedEventArgs(this.cboFiscalYear.Text, string.Empty, this.cboFiscalYear.SelectedValue, string.Empty));
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetOTBudgetDetail(FetchOTBudgetStatisticType loadType, int fiscalYear, string costCenter = "")
        {
            try
            {
                // Initialize variables and controls                
                string error = string.Empty;
                string innerError = string.Empty;

                #region Check if the current user is a Superintendent or CC Manager of certain cost center                                
                //if (!this.IsOTBudgetAdmin && 
                //    this.AllocatedCostCenterList.Count > 0)
                //{
                //    StringBuilder sb = new StringBuilder();

                //    foreach (string item in this.AllocatedCostCenterList)
                //    {
                //        if (sb.Length == 0)
                //            sb.Append(item);
                //        else
                //            sb.Append(string.Format(",{0}", item));
                //    }

                //    costCenter = sb.ToString().Trim();
                //}
                #endregion

                #region Get the cost center list where the user has access
                if (!this.IsOTBudgetAdmin &&
                   this.CostCenterList.Count > 0 &&
                   string.IsNullOrEmpty(costCenter))
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CostCenterEntity item in this.CostCenterList)
                    {
                        if (item.CostCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.CostCenter);
                        else
                            sb.Append(string.Format(",{0}", item.CostCenter));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetOvertimeBudgetStatistics(Convert.ToByte(loadType), fiscalYear, costCenter, ref error, ref innerError);
                if (rawData != null && rawData.Count() > 0)
                {
                    OvertimeBudgetEntity otBudgetDetail = rawData.FirstOrDefault();
                    if (otBudgetDetail != null)
                    {
                        this.chkShowBreakdown.Enabled = true;

                        if (loadType == FetchOTBudgetStatisticType.GetOTTotalBudgetAndActualAmount)
                        {
                            #region Render budget in terms of amount                                                        
                            this.tdTotalBudget.InnerHtml = string.Format("{0:#,0.000} BD", otBudgetDetail.TotalBudgetAmount);
                            this.tdTotalConsumed.InnerHtml = string.Format("{0:#,0.000} BD", otBudgetDetail.TotalActualAmount);
                            this.tdTotalBalance.InnerHtml = string.Format("{0:#,0.000} BD", otBudgetDetail.TotalBalanceAmount);

                            if (otBudgetDetail.TotalBalanceAmount < 0)
                                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "red";
                            else if (otBudgetDetail.TotalBalanceAmount > 0)
                                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "green";
                            else
                                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "black";

                            // Set the gauge pointer value
                            if (otBudgetDetail.TotalBudgetAmount > 0)
                            {
                                this.gaugeOTBudget.Pointer.Value = (otBudgetDetail.TotalActualAmount / otBudgetDetail.TotalBudgetAmount) * 100;
                            }
                            #endregion
                        }
                        else
                        {
                            #region Render budget in terms of work hours                                                        
                            this.tdTotalBudget.InnerHtml = string.Format("{0:#,0.00} hrs.", otBudgetDetail.TotalBudgetHour);
                            this.tdTotalConsumed.InnerHtml = string.Format("{0:#,0.00} hrs.", otBudgetDetail.TotalActualHour);
                            this.tdTotalBalance.InnerHtml = string.Format("{0:#,0.00} hrs.", otBudgetDetail.TotalBalanceHour);

                            if (otBudgetDetail.TotalBalanceHour < 0)
                                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "red";
                            else if (otBudgetDetail.TotalBalanceHour > 0)
                                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "green";
                            else
                                this.tdTotalBalance.Style[HtmlTextWriterStyle.Color] = "black";

                            // Set the gauge pointer value
                            if (otBudgetDetail.TotalBudgetHour > 0)
                            {
                                this.gaugeOTBudget.Pointer.Value = (otBudgetDetail.TotalActualHour / otBudgetDetail.TotalBudgetHour) * 100;
                            }
                            #endregion
                        }

                        this.chkShowBreakdown_CheckedChanged(this.chkShowBreakdown, new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PopulateOTBudgetByMonth(int fiscalYear, string unitType, string costCenter = "", bool reloadData = true)
        {
            try
            {
                #region Initialize variables and controls                
                string error = string.Empty;
                string innerError = string.Empty;

                #region Check if the current user is a Superintendent or CC Manager of certain cost center                                
                //if (!this.IsOTBudgetAdmin && 
                //    this.AllocatedCostCenterList.Count > 0)
                //{
                //    StringBuilder sb = new StringBuilder();

                //    foreach (string item in this.AllocatedCostCenterList)
                //    {
                //        if (sb.Length == 0)
                //            sb.Append(item);
                //        else
                //            sb.Append(string.Format(",{0}", item));
                //    }

                //    costCenter = sb.ToString().Trim();
                //}
                #endregion

                #region Get the cost center list where the user has access
                if (!this.IsOTBudgetAdmin &&
                   this.CostCenterList.Count > 0 &&
                   string.IsNullOrEmpty(costCenter))
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CostCenterEntity item in this.CostCenterList)
                    {
                        if (item.CostCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.CostCenter);
                        else
                            sb.Append(string.Format(",{0}", item.CostCenter));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                if (reloadData)
                    this.OvertimeBudgetData = null;

                BarSeries barOTBudget = this.chartOTBudget.PlotArea.Series[0] as BarSeries;
                if (barOTBudget != null)
                {
                    foreach (CategorySeriesItem item in barOTBudget.SeriesItems)
                    {
                        item.Y = 0;
                    }
                }
                #endregion

                #region Fetch data from DB                                
                if (reloadData)
                {
                    DALProxy proxy = new DALProxy();
                    byte loadType = 0;
                    if (unitType == "valAmount")
                    {
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTBudgetAmount);
                        barOTBudget.TooltipsAppearance.DataFormatString = "Budget: {0:#,0.000} BD";
                        barOTBudget.LabelsAppearance.DataFormatString = "{0:#,0.000}";
                    }
                    else
                    {
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTBudgetBreakdownByHour);
                        barOTBudget.TooltipsAppearance.DataFormatString = "Budget: {0:#,0.00} hrs.";
                        barOTBudget.LabelsAppearance.DataFormatString = "{0:#,0.00}";
                    }

                    var rawData = proxy.GetOvertimeBudgetStatistics(loadType, fiscalYear, costCenter, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                        this.OvertimeBudgetData = rawData.FirstOrDefault();
                }
                #endregion

                #region Populate data in the chart series           
                if (this.OvertimeBudgetData != null &&
                    barOTBudget != null)
                {
                    int counter = 1;

                    foreach (CategorySeriesItem item in barOTBudget.SeriesItems)
                    {
                        if (counter == 1)
                            item.Y = this.OvertimeBudgetData.JanBudget;
                        else if (counter == 2)
                            item.Y = this.OvertimeBudgetData.FebBudget;
                        else if (counter == 3)
                            item.Y = this.OvertimeBudgetData.MarBudget;
                        else if (counter == 4)
                            item.Y = this.OvertimeBudgetData.AprBudget;
                        else if (counter == 5)
                            item.Y = this.OvertimeBudgetData.MayBudget;
                        else if (counter == 6)
                            item.Y = this.OvertimeBudgetData.JunBudget;
                        else if (counter == 7)
                            item.Y = this.OvertimeBudgetData.JulBudget;
                        else if (counter == 8)
                            item.Y = this.OvertimeBudgetData.AugBudget;
                        else if (counter == 9)
                            item.Y = this.OvertimeBudgetData.SepBudget;
                        else if (counter == 10)
                            item.Y = this.OvertimeBudgetData.OctBudget;
                        else if (counter == 11)
                            item.Y = this.OvertimeBudgetData.NovBudget;
                        else if (counter == 12)
                            item.Y = this.OvertimeBudgetData.DecBudget;

                        counter++;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PopulateOTActualsByMonth(int fiscalYear, string unitType, string costCenter = "", bool reloadData = true)
        {
            try
            {
                #region Initialize variables and controls                
                string error = string.Empty;
                string innerError = string.Empty;

                #region Check if the current user is a Superintendent or CC Manager of certain cost center                                
                //if (!this.IsOTBudgetAdmin && 
                //    this.AllocatedCostCenterList.Count > 0)
                //{
                //    StringBuilder sb = new StringBuilder();

                //    foreach (string item in this.AllocatedCostCenterList)
                //    {
                //        if (sb.Length == 0)
                //            sb.Append(item);
                //        else
                //            sb.Append(string.Format(",{0}", item));
                //    }

                //    costCenter = sb.ToString().Trim();
                //}
                #endregion

                #region Get the cost center list where the user has access
                if (!this.IsOTBudgetAdmin &&
                   this.CostCenterList.Count > 0 &&
                   string.IsNullOrEmpty(costCenter))
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CostCenterEntity item in this.CostCenterList)
                    {
                        if (item.CostCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.CostCenter);
                        else
                            sb.Append(string.Format(",{0}", item.CostCenter));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                if (reloadData)
                    this.OvertimeActualsData = null;

                BarSeries barOTActuals = this.chartOTBudget.PlotArea.Series[1] as BarSeries;
                if (barOTActuals != null)
                {
                    foreach (CategorySeriesItem item in barOTActuals.SeriesItems)
                    {
                        item.Y = 0;
                    }
                }
                #endregion

                #region Fetch data from DB                                
                if (reloadData)
                {
                    DALProxy proxy = new DALProxy();
                    byte loadType = 0;
                    if (unitType == "valAmount")
                    {
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTActualAmount);
                        barOTActuals.TooltipsAppearance.DataFormatString = "Actual: {0:#,0.000} BD";
                        barOTActuals.LabelsAppearance.DataFormatString = "{0:#,0.000}";
                    }
                    else
                    {
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTActualBreakdownByHour);
                        barOTActuals.TooltipsAppearance.DataFormatString = "Actual: {0:#,0.00} hrs.";
                        barOTActuals.LabelsAppearance.DataFormatString = "{0:#,0.00}";
                    }

                    var rawData = proxy.GetOvertimeBudgetStatistics(loadType, fiscalYear, costCenter, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                        this.OvertimeActualsData = rawData.FirstOrDefault();
                }
                #endregion

                #region Populate data in the chart series                                
                if (this.OvertimeActualsData != null 
                    && barOTActuals != null)
                {
                    int counter = 1;
                    decimal xValue = 0;

                    foreach (CategorySeriesItem item in barOTActuals.SeriesItems)
                    {
                        xValue = 0;

                        if (counter == 1)
                        {
                            item.Y = this.OvertimeActualsData.JanActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.JanBudget;
                        }
                        else if (counter == 2)
                        {
                            item.Y = this.OvertimeActualsData.FebActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.FebBudget;
                        }
                        else if (counter == 3)
                        {
                            item.Y = this.OvertimeActualsData.MarActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.MarBudget;
                        }
                        else if (counter == 4)
                        {
                            item.Y = this.OvertimeActualsData.AprActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.AprBudget;
                        }
                        else if (counter == 5)
                        {
                            item.Y = this.OvertimeActualsData.MayActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.MayBudget;
                        }
                        else if (counter == 6)
                        {
                            item.Y = this.OvertimeActualsData.JunActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.JunBudget;
                        }
                        else if (counter == 7)
                        {
                            item.Y = this.OvertimeActualsData.JulActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.JulBudget;
                        }
                        else if (counter == 8)
                        {
                            item.Y = this.OvertimeActualsData.AugActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.AugBudget;
                        }
                        else if (counter == 9)
                        {
                            item.Y = this.OvertimeActualsData.SepActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.SepBudget;
                        }
                        else if (counter == 10)
                        {
                            item.Y = this.OvertimeActualsData.OctActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.OctBudget;
                        }
                        else if (counter == 11)
                        {
                            item.Y = this.OvertimeActualsData.NovActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.NovBudget;
                        }
                        else if (counter == 12)
                        {
                            item.Y = this.OvertimeActualsData.DecActual;
                            if (this.OvertimeBudgetData != null)
                                xValue = this.OvertimeBudgetData.DecBudget;
                        }

                        #region Check if actual amount is greater than the budget
                        if (item.Y > xValue)
                            item.BackgroundColor = System.Drawing.Color.Red;
                        else
                            item.BackgroundColor = System.Drawing.Color.Green;
                        #endregion

                        // Increment the counter
                        counter++;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PopulateChartCostCenter(int fiscalYear, string costCenter = "", bool reloadData = true)
        {
            try
            {
                #region Initialize variables and controls                
                string error = string.Empty;
                string innerError = string.Empty;                

                #region Check if the current user is a Superintendent or CC Manager of certain cost center                                
                //if (!this.IsOTBudgetAdmin && 
                //    this.AllocatedCostCenterList.Count > 0)
                //{
                //    StringBuilder sb = new StringBuilder();

                //    foreach (string item in this.AllocatedCostCenterList)
                //    {
                //        if (sb.Length == 0)
                //            sb.Append(item);
                //        else
                //            sb.Append(string.Format(",{0}", item));
                //    }

                //    costCenter = sb.ToString().Trim();
                //}
                #endregion

                #region Get the cost center list where the user has access
                if (!this.IsOTBudgetAdmin &&
                   this.CostCenterList.Count > 0 &&
                   string.IsNullOrEmpty(costCenter))
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CostCenterEntity item in this.CostCenterList)
                    {
                        if (item.CostCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.CostCenter);
                        else
                            sb.Append(string.Format(",{0}", item.CostCenter));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                if (reloadData)
                    this.OvertimeCostCenterList.Clear();

                this.chartOTBudgetCostCenter.PlotArea.XAxis.Items.Clear();
                #endregion

                #region Fetch data from DB                                
                if (reloadData)
                {
                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetOvertimeBudgetStatistics(Convert.ToByte(FetchOTBudgetStatisticType.GetAllCostCenterByFiscalYear), fiscalYear, costCenter, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                        this.OvertimeCostCenterList = rawData.ToList();
                }
                #endregion

                #region Populate data in the chart series           
                if (this.OvertimeCostCenterList.Count > 0)
                {
                    foreach (OvertimeBudgetEntity item in this.OvertimeCostCenterList)
                    {
                        this.chartOTBudgetCostCenter.PlotArea.XAxis.Items.Add(new AxisItem(item.CostCenter));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PopulateOTBudgetByCostCenter(int fiscalYear, string unitType, string costCenter = "", bool reloadData = true)
        {
            try
            {
                #region Initialize variables and controls                
                string error = string.Empty;
                string innerError = string.Empty;                                

                #region Check if the current user is a Superintendent or CC Manager of certain cost center                                
                //if (!this.IsOTBudgetAdmin && 
                //    this.AllocatedCostCenterList.Count > 0)
                //{
                //    StringBuilder sb = new StringBuilder();

                //    foreach (string item in this.AllocatedCostCenterList)
                //    {
                //        if (sb.Length == 0)
                //            sb.Append(item);
                //        else
                //            sb.Append(string.Format(",{0}", item));
                //    }

                //    costCenter = sb.ToString().Trim();
                //}
                #endregion

                #region Get the cost center list where the user has access
                if (!this.IsOTBudgetAdmin &&
                   this.CostCenterList.Count > 0 &&
                   string.IsNullOrEmpty(costCenter))
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CostCenterEntity item in this.CostCenterList)
                    {
                        if (item.CostCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.CostCenter);
                        else
                            sb.Append(string.Format(",{0}", item.CostCenter));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                // Remove all series item
                this.chartOTBudgetCostCenter.PlotArea.Series[0].Items.Clear();

                if (reloadData)
                    this.OvertimeBudgetList = null;
                #endregion

                #region Fetch data from DB                                
                if (reloadData)
                {
                    DALProxy proxy = new DALProxy();
                    byte loadType = 0;
                    if (unitType == "valAmount")
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTBudgetBreakdownByCostCenter);
                    else
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTBudgetBreakdownByCostCenterHour);

                    var rawData = proxy.GetOvertimeBudgetStatistics(loadType, fiscalYear, costCenter, ref error, ref innerError);
                    if (rawData != null)
                        this.OvertimeBudgetList = rawData.ToList();
                }
                #endregion

                #region Populate data in the chart series           
                if (this.OvertimeBudgetList.Count > 0)
                {
                    foreach (AxisItem item in this.chartOTBudgetCostCenter.PlotArea.XAxis.Items)
                    {
                        OvertimeBudgetEntity axisItemValue = this.OvertimeBudgetList
                            .Where(a => a.CostCenter == item.LabelText)
                            .FirstOrDefault();
                        if (axisItemValue != null)
                            this.chartOTBudgetCostCenter.PlotArea.Series[0].Items.Add(new SeriesItem(axisItemValue.TotalBudgetAmount));
                    }

                    BarSeries barOTBudget = this.chartOTBudgetCostCenter.PlotArea.Series[0] as BarSeries;
                    if (unitType == "valAmount")
                    {
                        barOTBudget.TooltipsAppearance.DataFormatString = "Budget: {0:#,0.000} BD";
                        barOTBudget.LabelsAppearance.DataFormatString = "{0:#,0.000}";
                    }
                    else
                    {
                        barOTBudget.TooltipsAppearance.DataFormatString = "Budget: {0:#,0.00} hrs.";
                        barOTBudget.LabelsAppearance.DataFormatString = "{0:#,0.00}";
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PopulateOTActualsByCostCenter(int fiscalYear, string unitType, string costCenter = "", bool reloadData = true)
        {
            try
            {
                #region Initialize variables and controls                
                string error = string.Empty;
                string innerError = string.Empty;                

                #region Check if the current user is a Superintendent or CC Manager of certain cost center                                
                //if (!this.IsOTBudgetAdmin && 
                //    this.AllocatedCostCenterList.Count > 0)
                //{
                //    StringBuilder sb = new StringBuilder();

                //    foreach (string item in this.AllocatedCostCenterList)
                //    {
                //        if (sb.Length == 0)
                //            sb.Append(item);
                //        else
                //            sb.Append(string.Format(",{0}", item));
                //    }

                //    costCenter = sb.ToString().Trim();
                //}
                #endregion

                #region Get the cost center list where the user has access
                if (!this.IsOTBudgetAdmin &&
                   this.CostCenterList.Count > 0 &&
                   string.IsNullOrEmpty(costCenter))
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CostCenterEntity item in this.CostCenterList)
                    {
                        if (item.CostCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                            continue;

                        if (sb.Length == 0)
                            sb.Append(item.CostCenter);
                        else
                            sb.Append(string.Format(",{0}", item.CostCenter));
                    }

                    costCenter = sb.ToString().Trim();
                }
                #endregion

                // Remove all series item
                this.chartOTBudgetCostCenter.PlotArea.Series[1].Items.Clear();

                if (reloadData)
                    this.OvertimeActualList = null;
                #endregion

                #region Fetch data from DB                                
                if (reloadData)
                {
                    DALProxy proxy = new DALProxy();
                    byte loadType = 0;
                    if (unitType == "valAmount")
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTActualBreakdownByCostCenter);
                    else
                        loadType = Convert.ToByte(FetchOTBudgetStatisticType.GetOTActualBreakdownByCostCenterHour);

                    var rawData = proxy.GetOvertimeBudgetStatistics(loadType, fiscalYear, costCenter, ref error, ref innerError);
                    if (rawData != null)
                        this.OvertimeActualList = rawData.ToList();
                }
                #endregion

                #region Populate data in the chart series           
                if (this.OvertimeActualList.Count > 0)
                {
                    SeriesItem seriesItem = new SeriesItem();
                    bool isOverBudget = false;

                    foreach (AxisItem item in this.chartOTBudgetCostCenter.PlotArea.XAxis.Items)
                    {
                        OvertimeBudgetEntity axisItemValue = this.OvertimeActualList
                            .Where(a => a.CostCenter == item.LabelText)
                            .FirstOrDefault();
                        if (axisItemValue != null)
                        {
                            // Reset flag value
                            isOverBudget = false;

                            seriesItem = new SeriesItem();
                            seriesItem.YValue = axisItemValue.TotalActualAmount;

                            #region Check if actual amount is greater than the budget
                            if (this.OvertimeBudgetList.Count > 0)
                            {
                                OvertimeBudgetEntity axisItemBudgetValue = this.OvertimeBudgetList
                                   .Where(a => a.CostCenter == item.LabelText)
                                   .FirstOrDefault();
                                if (axisItemBudgetValue != null)
                                {
                                    if (axisItemValue.TotalActualAmount > axisItemBudgetValue.TotalBudgetAmount)
                                        isOverBudget = true;
                                }
                            }
                            #endregion

                            seriesItem.BackgroundColor = isOverBudget ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                            this.chartOTBudgetCostCenter.PlotArea.Series[1].Items.Add(seriesItem);
                        }
                    }

                    BarSeries barOTActual = this.chartOTBudgetCostCenter.PlotArea.Series[1] as BarSeries;
                    if (unitType == "valAmount")
                    {
                        barOTActual.TooltipsAppearance.DataFormatString = "Actual: {0:#,0.000} BD";
                        barOTActual.LabelsAppearance.DataFormatString = "{0:#,0.000}";
                    }
                    else
                    {
                        barOTActual.TooltipsAppearance.DataFormatString = "Actual: {0:#,0.00} hrs.";
                        barOTActual.LabelsAppearance.DataFormatString = "{0:#,0.00}";
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetAttendanceHistory(int empNo, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                // Reset collection
                this.AttendanceList.Clear();

                DALProxy proxy = new DALProxy();
                var source = proxy.GetEmployeeAttendanceHistoryCompact(startDate, endDate, string.Empty, empNo, ref error, ref innerError);
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
                        this.AttendanceList.AddRange(source);
                    }
                }

                // Bind data to the grid
                RebindDataToTimesheetGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Workflow Methods
        private bool ProcessWorkflowEmail()
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
                string originatorEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]);
                string originatorName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string otStartTime = string.Empty;
                string otEndTime = string.Empty;
                string otType = string.Empty;
                string isOTApproved = string.Empty;
                string isMealVoucherApproved = string.Empty;
                #endregion

                var rawData = proxy.GetWFEmailDueForDelivery(1, createdByEmpNo, 0, null, null, ref error, ref innerError);
                if (rawData != null)
                {
                    List<WorkflowEmailDeliveryEntity> recipientList = rawData.ToList();
                    foreach (WorkflowEmailDeliveryEntity recipient in recipientList)
                    {
                        #region Send email to each assigned approver
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

                                foreach (WorkflowEmailDeliveryEntity item in emailDeliveryList)
                                {
                                    #region Build the email content
                                    if (item.OTApproved == "Y") // OT is Approved
                                    {
                                        isOTApproved = "Yes";
                                        otStartTime = UIHelper.ConvertObjectToTimeString(item.OTStartTime);
                                        otEndTime = UIHelper.ConvertObjectToTimeString(item.OTEndTime);
                                        otType = item.OTType;
                                    }
                                    else if (item.OTApproved == "N")    // OT is rejected
                                    {
                                        isOTApproved = "No";
                                        otStartTime = "-";
                                        otEndTime = "-";
                                        otType = "-";
                                    }
                                    else
                                    {
                                        isOTApproved = "No action";
                                        otStartTime = "-";
                                        otEndTime = "-";
                                        otType = "-";
                                    }

                                    if (item.MealVoucherEligibility == "YA")
                                        isMealVoucherApproved = "Yes";
                                    else if (item.MealVoucherEligibility == "N")
                                        isMealVoucherApproved = "No";
                                    else
                                        isMealVoucherApproved = "No action";

                                    if (sb.Length > 0)
                                        sb.AppendLine(@"<br />");

                                    sb.AppendLine(string.Format(@"{0}. <b>Requisition No.:</b> {1}; " + 
                                                    "<b>Employee Name:</b> {2}; " +
                                                    "<b>Position:</b> {3}; " +
                                                    "<b>Cost Center:</b> {4}; " +
                                                    //"<b>Pay Grade:</b> {5}; " +
                                                    //"<b>Shift Pat.:</b> {6}; " +
                                                    //"<b>Sched. Shift:</b> {7}; " +
                                                    //"<b>Actual Shift:</b> {8}; " +
                                                    "<b>Date:</b> {5}; " +
                                                    "<b>Meal Voucher Approved:</b> {6}; " +
                                                    "<b>OT Approved:</b> {7}; " +
                                                    "<b>OT Start Time:</b>" + "<font color=" + "red" + ">" + " {8}</font>; " +
                                                    "<b>OT End Time:</b>" + "<font color=" + "red" + ">" + " {9}</font>; " +
                                                    //"<b>OT Type:</b> {14}; " +
                                                    "<b>Remarks:</b> {10}",
                                                counter,
                                                item.OTRequestNo,
                                                !string.IsNullOrEmpty(item.EmpFullName) ? item.EmpFullName : "Not defined",
                                                !string.IsNullOrEmpty(item.Position) ? item.Position : "Not defined",
                                                !string.IsNullOrEmpty(item.CostCenterFullName) ? item.CostCenterFullName : "Not defined",
                                                //item.PayGrade,
                                                //!string.IsNullOrEmpty(item.ShiftPatCode) ? item.ShiftPatCode : "Not defined",
                                                //!string.IsNullOrEmpty(item.ShiftCode) ? item.ShiftCode : "Not defined",
                                                //!string.IsNullOrEmpty(item.ActualShiftCode) ? item.ShiftCode : "Not defined",
                                                UIHelper.ConvertObjectToDateString(item.DT),                                                
                                                isMealVoucherApproved,
                                                isOTApproved,
                                                otStartTime,
                                                otEndTime,
                                                //otType,
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
                                    if (SendEmailToApprover(originatorName, originatorEmail, recipient, emailBody))
                                    {
                                        error = string.Empty;
                                        innerError = string.Empty;

                                        proxy.CloseEmailDelivery(emailDeliveryList, ref error, ref innerError);
                                        if (!string.IsNullOrEmpty(error))
                                        {
                                            throw new Exception(error);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
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
        private bool SendEmailToApprover(string originatorName, string originatorEmail, WorkflowEmailDeliveryEntity emailData, string emailBody)
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
                    return false;

                //Check the collection
                if (emailData == null)
                    return false;
                #endregion

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                int retError = 0;
                string errorMsg = string.Empty;
                string error = string.Empty;
                string innerError = string.Empty;
                string recipientEmail = string.Empty;
                string recipientName = "Colleague";
                string distListCode = string.Empty;
                EmployeeDetail empInfo = new EmployeeDetail();
                bool useMultithread = UIHelper.ConvertNumberToBolean(ConfigurationManager.AppSettings["UseMultithread"]);
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

                // Get the name of the primary recipient
                if (!string.IsNullOrEmpty(emailData.CurrentlyAssignedEmpName))
                    recipientName = UIHelper.ConvertStringToTitleCase(emailData.CurrentlyAssignedEmpName);

                if (!string.IsNullOrEmpty(emailData.CurrentlyAssignedEmpEmail) &&
                    !string.IsNullOrEmpty(emailData.CurrentlyAssignedEmpName))
                {
                    toList.Add(new MailAddress(emailData.CurrentlyAssignedEmpEmail, UIHelper.ConvertStringToTitleCase(emailData.CurrentlyAssignedEmpName)));
                }
                else
                {
                    if (emailData.CurrentlyAssignedEmpNo > 0)
                    {
                        empInfo = UIHelper.GetEmployeeEmailInfo(emailData.CurrentlyAssignedEmpNo);
                        if (empInfo != null)
                        {
                            if (!string.IsNullOrEmpty(empInfo.EmpName) &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                toList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
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
                                        empInfo = UIHelper.GetEmployeeEmailInfo(emp.EmpNo);
                                        if (empInfo != null &&
                                            !string.IsNullOrEmpty(empInfo.EmpEmail))
                                        {
                                            ccList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
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
                                        empInfo = UIHelper.GetEmployeeEmailInfo(emp.EmpNo);
                                        if (empInfo != null &&
                                            !string.IsNullOrEmpty(empInfo.EmpEmail))
                                        {
                                            ccList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
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

                #region Send cc copy to the current approver
                //if (ccList == null)
                //    ccList = new List<MailAddress>();

                //ccList.Add(new MailAddress(originatorEmail, UIHelper.ConvertStringToTitleCase(originatorName)));
                #endregion

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
                string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                        UIHelper.PAGE_OVERTIME_APPROVAL.Replace("~", string.Empty));

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
                    if (useMultithread)
                    {
                        int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);

                        // Send email in separate thread
                        Task.Factory.StartNew(() => SendEmailMultithread(toList, ccList, bccList, from, subject, htmLBody, attachmentList, mailServer, emailData.OTRequestNo, userEmpNo, userID));
                    }
                    else
                    {
                        #region Send email using the main application thread
                        retError = 0;
                        errorMsg = string.Empty;
                        SendEmail(toList, ccList, bccList, from, subject, htmLBody, attachmentList, mailServer, ref errorMsg, ref retError);
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            throw new Exception(errorMsg);
                        }
                        #endregion
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SendEmailMultithread(List<MailAddress> toList, List<MailAddress> ccList, List<MailAddress> bccList, MailAddress from, string subject,
            string htmLBody, List<Attachment> attachmentList, string mailServer, long otRequestNo, int userEmpNo, string userID)
        {
            try
            {
                int retError = 0;
                string errorMsg = string.Empty;

                SendEmail(toList, ccList, bccList, from, subject, htmLBody, attachmentList, mailServer, ref errorMsg, ref retError);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    throw new Exception(errorMsg);
                }
            }
            catch (Exception ex)
            {
                DALProxy proxy = new DALProxy();
                proxy.InsertSystemErrorLog(Convert.ToByte(UIHelper.SaveType.Insert), 0, otRequestNo, Convert.ToByte(UIHelper.SystemErrorCode.MultithreadingError), ex.Message.ToString(), userEmpNo, userID);
            }
        }

        private void SendRejectionEmail(EmployeeAttendanceEntity overtimeRequest, string emailBody, string rejectionRemarks, string approverName, int approverNo,
            string dynamicEndpointAddress, int userEmpNo, string userID, bool isMultithread = false)
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
                    return;

                //Check the collection
                if (overtimeRequest == null)
                    return;
                #endregion

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                int retError = 0;
                string errorMsg = string.Empty;
                string error = string.Empty;
                string innerError = string.Empty;
                string recipientEmail = string.Empty;
                string recipientName = "Colleague";
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

                // Set the primary email recipient to the creator of the request
                if (!string.IsNullOrEmpty(overtimeRequest.CreatedByEmpName) &&
                    !string.IsNullOrEmpty(overtimeRequest.CreatedByEmail))
                {
                    recipientName = UIHelper.ConvertStringToTitleCase(overtimeRequest.CreatedByEmpName);
                    toList.Add(new MailAddress(overtimeRequest.CreatedByEmail, UIHelper.ConvertStringToTitleCase(overtimeRequest.CreatedByEmpName)));
                }
                else
                {
                    if (overtimeRequest.CreatedByEmpNo > 0)
                    {
                        empInfo = UIHelper.GetEmployeeEmailInfo(overtimeRequest.CreatedByEmpNo);
                        if (empInfo != null)
                        {
                            if (!string.IsNullOrEmpty(empInfo.EmpName) &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                recipientName = UIHelper.ConvertStringToTitleCase(empInfo.EmpName);
                                toList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                            }
                        }
                    }
                }
                #endregion

                #region Set the Cc Recipients
                var rawData = proxy.GetRequestApprovers(overtimeRequest.OTRequestNo, overtimeRequest.AutoID, overtimeRequest.RequestSubmissionDate, ref error, ref innerError);
                if (rawData != null)
                {
                    // Initialize collection
                    ccList = new List<MailAddress>();

                    foreach (EmployeeDetail item in rawData.ToList())
                    {
                        // Skip if approver is the same as the current approver who rejected the request
                        if (item.EmpNo == approverNo)
                            continue;

                        if (!string.IsNullOrEmpty(item.EmpEmail) &&
                            !string.IsNullOrEmpty(item.EmpName))
                        {
                            if (ccList.Count > 0)
                            {
                                if (ccList.Where(a => a.Address.Trim() == item.EmpEmail).FirstOrDefault() == null)
                                    ccList.Add(new MailAddress(item.EmpEmail, UIHelper.ConvertStringToTitleCase(item.EmpName)));
                            }
                            else
                                ccList.Add(new MailAddress(item.EmpEmail, UIHelper.ConvertStringToTitleCase(item.EmpName)));
                        }
                    }
                }
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



                // Exit if primary mail recipient is null
                //if (toList == null || toList.Count == 0)
                //    return false;

                #region Build URL address
                //string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                //        UIHelper.PAGE_OVERTIME_ENTRY.Replace("~", string.Empty));

                string queryString = string.Format("?IsLoadRequest={0}&EmpNo={1}&StartDate={2}&DisplayOption={3}&OTRequestNo={4}",
                        true.ToString(),
                        overtimeRequest.EmpNo,
                        overtimeRequest.DT.HasValue ? Convert.ToDateTime(overtimeRequest.DT).ToString("dd/MM/yyyy") : string.Empty,
                        UIHelper.OvertimeFilter.OTREJECTED.ToString(),
                        overtimeRequest.OTRequestNo);

                StringBuilder url = new StringBuilder();
                url.Append(string.Concat(dynamicEndpointAddress, queryString.Trim()));
                #endregion

                #region Set Message Body
                string body = String.Empty;
                string htmLBody = string.Empty;
                string appPath = Server.MapPath(UIHelper.CONST_REJECTION_EMAIL_TEMPLATE);
                string adminName = ConfigurationManager.AppSettings["AdminName"];

                // Build the message body
                body = String.Format(UIHelper.RetrieveXmlMessage(appPath),
                    recipientName,
                    emailBody,
                    url.ToString().Trim(),
                    adminName,
                    string.Format("<font color=Red>{0}</font>", rejectionRemarks),
                    approverName,
                    overtimeRequest.OTRequestNo
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
                    proxy.InsertSystemErrorLog(Convert.ToByte(UIHelper.SaveType.Insert), 0, overtimeRequest.OTRequestNo, Convert.ToByte(UIHelper.SystemErrorCode.MultithreadingError), ex.Message.ToString(), userEmpNo, userID);
                }
                else
                    throw ex;
            }
        }

        private void SendNotificationToCreatorAboutOTChanges(EmployeeAttendanceEntity overtimeRequest, string emailBody, int approverNo, 
            string dynamicEndpointAddress, int userEmpNo, string userID, bool isMultithread = false)
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
                    return;

                //Check the collection
                if (overtimeRequest == null)
                    return;
                #endregion

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                int retError = 0;
                string errorMsg = string.Empty;
                string error = string.Empty;
                string innerError = string.Empty;
                string recipientEmail = string.Empty;
                string recipientName = "Colleague";
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

                // Set the creator of the request to be the primary email recipient 
                if (!string.IsNullOrEmpty(overtimeRequest.CreatedByEmpName) &&
                    !string.IsNullOrEmpty(overtimeRequest.CreatedByEmail))
                {
                    recipientName = UIHelper.ConvertStringToTitleCase(overtimeRequest.CreatedByEmpName);
                    toList.Add(new MailAddress(overtimeRequest.CreatedByEmail, UIHelper.ConvertStringToTitleCase(overtimeRequest.CreatedByEmpName)));
                }
                else
                {
                    if (overtimeRequest.CreatedByEmpNo > 0)
                    {
                        empInfo = UIHelper.GetEmployeeEmailInfo(overtimeRequest.CreatedByEmpNo);
                        if (empInfo != null)
                        {
                            if (!string.IsNullOrEmpty(empInfo.EmpName) &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                recipientName = UIHelper.ConvertStringToTitleCase(empInfo.EmpName);
                                toList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                            }
                        }
                    }
                }
                #endregion

                #region Send cc copy to all approvers who have approved the overtime request
                var rawData = proxy.GetRequestApprovers(overtimeRequest.OTRequestNo, overtimeRequest.AutoID, overtimeRequest.RequestSubmissionDate, ref error, ref innerError);
                if (rawData != null)
                {
                    // Initialize collection
                    ccList = new List<MailAddress>();

                    foreach (EmployeeDetail item in rawData.ToList())
                    {
                        // Skip if approver is the same as the current approver who rejected the request
                        if (item.EmpNo == approverNo)
                            continue;

                        if (!string.IsNullOrEmpty(item.EmpEmail) &&
                            !string.IsNullOrEmpty(item.EmpName))
                        {
                            if (ccList.Count > 0)
                            {
                                if (ccList.Where(a => a.Address.Trim() == item.EmpEmail).FirstOrDefault() == null)
                                    ccList.Add(new MailAddress(item.EmpEmail, UIHelper.ConvertStringToTitleCase(item.EmpName)));
                            }
                            else
                                ccList.Add(new MailAddress(item.EmpEmail, UIHelper.ConvertStringToTitleCase(item.EmpName)));
                        }
                    }
                }
                #endregion

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

                // Exit if primary mail recipient is null
                //if (toList == null || toList.Count == 0)
                //    return false;

                #region Build URL address
                //string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                //        UIHelper.PAGE_OVERTIME_ENTRY.Replace("~", string.Empty));

                string queryString = string.Format("?IsLoadRequest={0}&EmpNo={1}&StartDate={2}&DisplayOption={3}&OTRequestNo={4}",
                        true.ToString(),
                        overtimeRequest.EmpNo,
                        overtimeRequest.DT.HasValue ? Convert.ToDateTime(overtimeRequest.DT).ToString("dd/MM/yyyy") : string.Empty,
                        UIHelper.OvertimeFilter.OTSUBMITED.ToString(),
                        overtimeRequest.OTRequestNo);

                StringBuilder url = new StringBuilder();
                url.Append(string.Concat(dynamicEndpointAddress, queryString.Trim()));
                #endregion

                #region Set Message Body
                string body = String.Empty;
                string htmLBody = string.Empty;
                string appPath = Server.MapPath(UIHelper.CONST_MODIFIED_OTDETAILS_EMAIL_TEMPLATE);
                string adminName = ConfigurationManager.AppSettings["AdminName"];

                // Build the message body
                body = String.Format(UIHelper.RetrieveXmlMessage(appPath),
                        recipientName,
                        overtimeRequest.OTRequestNo,
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
                    proxy.InsertSystemErrorLog(Convert.ToByte(UIHelper.SaveType.Insert), 0, overtimeRequest.OTRequestNo, Convert.ToByte(UIHelper.SystemErrorCode.MultithreadingError), ex.Message.ToString(), userEmpNo, userID);
                }
                else
                    throw ex;
            }
        }

        private void SendNotificationToCreatorWFCompleted(EmployeeAttendanceEntity overtimeRequest, string emailBody, string dynamicEndpointAddress,
            int userEmpNo, string userID, bool isMultithread = false)
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
                    return;

                //Check the collection
                if (overtimeRequest == null)
                    return;
                #endregion

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                int retError = 0;
                string errorMsg = string.Empty;
                string error = string.Empty;
                string innerError = string.Empty;
                string recipientEmail = string.Empty;
                string recipientName = "Colleague";
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

                // Set the creator of the request to be the primary email recipient 
                if (!string.IsNullOrEmpty(overtimeRequest.CreatedByEmpName) &&
                    !string.IsNullOrEmpty(overtimeRequest.CreatedByEmail))
                {
                    recipientName = UIHelper.ConvertStringToTitleCase(overtimeRequest.CreatedByEmpName);
                    toList.Add(new MailAddress(overtimeRequest.CreatedByEmail, UIHelper.ConvertStringToTitleCase(overtimeRequest.CreatedByEmpName)));
                }
                else
                {
                    if (overtimeRequest.CreatedByEmpNo > 0)
                    {
                        empInfo = UIHelper.GetEmployeeEmailInfo(overtimeRequest.CreatedByEmpNo);
                        if (empInfo != null)
                        {
                            if (!string.IsNullOrEmpty(empInfo.EmpName) &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                recipientName = UIHelper.ConvertStringToTitleCase(empInfo.EmpName);
                                toList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                            }
                        }
                    }
                }
                #endregion

                #region Send cc copy to all approvers who have approved the overtime request
                //var rawData = proxy.GetRequestApprovers(overtimeRequest.OTRequestNo, overtimeRequest.AutoID, overtimeRequest.RequestSubmissionDate, ref error, ref innerError);
                //if (rawData != null)
                //{
                //    // Initialize collection
                //    ccList = new List<MailAddress>();

                //    foreach (EmployeeDetail item in rawData.ToList())
                //    {
                //        // Skip if approver is the same as the current approver who rejected the request
                //        if (item.EmpNo == approverNo)
                //            continue;

                //        if (!string.IsNullOrEmpty(item.EmpEmail) &&
                //            !string.IsNullOrEmpty(item.EmpName))
                //        {
                //            if (ccList.Count > 0)
                //            {
                //                if (ccList.Where(a => a.Address.Trim() == item.EmpEmail).FirstOrDefault() == null)
                //                    ccList.Add(new MailAddress(item.EmpEmail, UIHelper.ConvertStringToTitleCase(item.EmpName)));
                //            }
                //            else
                //                ccList.Add(new MailAddress(item.EmpEmail, UIHelper.ConvertStringToTitleCase(item.EmpName)));
                //        }
                //    }
                //}
                #endregion

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

                // Exit if primary mail recipient is null
                //if (toList == null || toList.Count == 0)
                //    return false;

                #region Build URL address
                //string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                //        UIHelper.PAGE_OVERTIME_ENTRY.Replace("~", string.Empty));

                string queryString = string.Format("?IsLoadRequest={0}&EmpNo={1}&StartDate={2}&DisplayOption={3}&OTRequestNo={4}",
                        true.ToString(),
                        overtimeRequest.EmpNo,
                        overtimeRequest.DT.HasValue ? Convert.ToDateTime(overtimeRequest.DT).ToString("dd/MM/yyyy") : string.Empty,
                        UIHelper.OvertimeFilter.OTAPPROVED.ToString(),
                        overtimeRequest.OTRequestNo);

                StringBuilder url = new StringBuilder();
                url.Append(string.Concat(dynamicEndpointAddress, queryString.Trim()));
                #endregion

                #region Set Message Body
                string body = String.Empty;
                string htmLBody = string.Empty;
                string appPath = Server.MapPath(UIHelper.CONST_WFCOMPLETION_EMAIL_TEMPLATE);
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
                    proxy.InsertSystemErrorLog(Convert.ToByte(UIHelper.SaveType.Insert), 0, overtimeRequest.OTRequestNo, Convert.ToByte(UIHelper.SystemErrorCode.MultithreadingError), ex.Message.ToString(), userEmpNo, userID);
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