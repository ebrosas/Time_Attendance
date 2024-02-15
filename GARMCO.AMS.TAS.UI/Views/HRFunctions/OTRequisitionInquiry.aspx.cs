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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class OTRequisitionInquiry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoStartDate,
            NoDataFilterOption,
            NoSelectedOTToCancel,
            InvalidDateRange,
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

        private Dictionary<string, object> OTRequisitionInqStorage
        {
            get
            {
                Dictionary<string, object> list = Session["OTRequisitionInqStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["OTRequisitionInqStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["OTRequisitionInqStorage"] = value;
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

        private List<EmployeeAttendanceEntity> SelectedOTRequisitionList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["SelectedOTRequisitionList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["SelectedOTRequisitionList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["SelectedOTRequisitionList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.OTINQUIRY.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_OVERTIME_REQUISITION_INQ_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_OVERTIME_REQUISITION_INQ_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Enabled = this.Master.IsEditAllowed;
                this.btnReassign.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.OTRequisitionInqStorage.Count > 0)
                {
                    if (this.OTRequisitionInqStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        string originatorButton = this.OTRequisitionInqStorage.ContainsKey("SourceControl")
                           ? UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["SourceControl"]) : string.Empty;

                        switch (originatorButton)
                        {
                            case "btnFindEmployee":
                                this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;

                            case "btnFindAssignee":
                                this.txtAssigneeEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;

                            case "btnFindCreator":
                                this.txtCreatedByEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;
                        }
                    }

                    // Clear data storage
                    Session.Remove("OTRequisitionInqStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("OTRequisitionInqStorage");

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

                    #region Check if current user is the HR Validator
                    DALProxy proxy = new DALProxy();
                    string error = string.Empty;
                    string innerError = string.Empty;
                    int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    bool? isHrApprover = proxy.CheckIfHRApprover(currentUserEmpNo, ref error, ref innerError);
                    if (isHrApprover.HasValue)
                        this.IsHRValidator = Convert.ToBoolean(isHrApprover);
                    else
                        this.IsHRValidator = false;
                    #endregion

                    // Populate data to the grid
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
                                    script.Append(string.Concat(this.btnCancelDummyGrid.ClientID, "','"));
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
                                        UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY
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
                                                UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY,
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
                    }
                }
            }
            else if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
            {
                this.gridSearchResults.AllowPaging = false;
                RebindDataToGrid();

                //foreach (GridDataItem item in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Item, GridItemType.AlternatingItem))
                //{
                //    item.BackColor = System.Drawing.Color.Transparent;
                //    item.ForeColor = System.Drawing.Color.Black;
                //}

                #region Initialize grid columns for export
                this.gridSearchResults.MasterTableView.GetColumn("CheckboxSelectColumn").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("ViewHistoryLinkButton").Visible = false;
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
                this.gridSearchResults.MasterTableView.GetColumn("CreatedByFullName").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("CreatedDate").Visible = false;
                this.gridSearchResults.MasterTableView.GetColumn("MealVoucherEligibility").Visible = false;

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
                this.gridSearchResults.MasterTableView.GetColumn("CreatedByFullNameExport").Visible = true;
                this.gridSearchResults.MasterTableView.GetColumn("CreatedDateExport").Visible = true;
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
                    int currentlyAssignedEmpNo = UIHelper.ConvertObjectToInt(item["CurrentlyAssignedEmpNo"].Text);
                    bool isOTRamadanExceedLimit = UIHelper.ConvertObjectToBolean(item["IsOTRamadanExceedLimit"].Text);
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    // Initialize control variables
                    RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                    RadLabel lblDuration = (RadLabel)item["OTDurationHour"].FindControl("lblDuration");

                    #region Process "OT Approved?" Header
                    foreach (GridHeaderItem headerItem in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Header))
                    {
                        CheckBox chkOTApprove = (CheckBox)headerItem["OTApprovalDesc"].Controls[1]; // Get the header checkbox 
                        if (chkOTApprove != null)
                            chkOTApprove.Checked = this.IsOTApprove;
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

                    #region Process "OT Approved?"
                    RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                    if (cboOTApprovalType != null)
                    {
                        if (cboOTApprovalType.Items.Count > 0)
                            cboOTApprovalType.SelectedValue = UIHelper.ConvertObjectToString(item["OTApprovalCode"].Text).Replace("&nbsp;", "");

                        if (cboOTApprovalType.SelectedValue == "Y")
                            cboOTApprovalType.ForeColor = System.Drawing.Color.DarkGreen;
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
                    RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                    if (cboOTReason != null)
                    {
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

                            // Set the maximum input value
                            txtDuration.MaxValue = UIHelper.ConvertObjectToDouble(item["OTDurationHourClone"].Text);
                        }

                        // Disable "OT Reason"
                        if (cboOTReason != null)
                            cboOTReason.Enabled = false;

                        // Disable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
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
                            //if (cboMealVoucherEligibility != null)
                            //{
                            //    if (cboMealVoucherEligibility.SelectedValue == "YA" ||
                            //        cboMealVoucherEligibility.SelectedValue == "N")
                            //    {
                            //        cboMealVoucherEligibility.Enabled = false;
                            //    }
                            //    else
                            //        cboMealVoucherEligibility.Enabled = true;
                            //}
                        }
                        else
                        {
                            //if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                            //   this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                            //   this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString())
                            //{
                            //    cboOTApprovalType.Enabled = false;
                            //}
                            //else
                            //{
                            //    // Enable "OT Approved?"
                            //    cboOTApprovalType.Enabled = true;
                            //}

                            // Dsiable "Meal Voucher Approved?"
                            //if (cboMealVoucherEligibility != null)
                            //    cboMealVoucherEligibility.Enabled = false;
                        }
                    }
                    #endregion

                    #endregion

                    #region Enable/disable other controls based on OT approval value
                    if (this.IsOTApprovalHeaderClicked && !isOTProcessed)
                    {
                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(item["AutoID"].Text);

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
                            //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                                txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";

                            // Enable "OT Reason"
                            //RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                            if (cboOTReason != null)
                                cboOTReason.Enabled = true;

                            // Enable "Remarks"
                            TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
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
                            TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
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
                        if (statusHandlingCode == UIHelper.STATUS_HANDLING_CODE_OPEN)
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

                            //RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                                txtDuration.ToolTip = UIHelper.CONST_OT_DURATION_RAMADAN;
                        }
                        #endregion
                    }

                    #region Enable/disable Cancel button 
                    ImageButton imgCancel = (ImageButton)item["CancelButton"].FindControl("imgCancelOT");
                    if (imgCancel != null)
                    {
                        if (statusHandlingCode == UIHelper.STATUS_HANDLING_CODE_CLOSED)
                        {
                            if (Master.IsSystemAdmin || this.IsHRValidator)
                            {                                
                                imgCancel.Enabled = true;
                                imgCancel.ImageUrl = @"~/Images/delete_enabled_icon.png";
                                imgCancel.ToolTip = "Cancel overtime request";
                            }
                        }
                        else
                        {
                            if (userEmpNo == createdByEmpNo ||
                                (this.Master.IsSystemAdmin || this.IsHRValidator))
                            {
                                imgCancel.Enabled = statusHandlingCode == UIHelper.STATUS_HANDLING_CODE_OPEN;
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
                    }
                    #endregion

                    #region Enable/disable "View history" link
                    ImageButton imgViewHistory = (ImageButton)item["HistoryButton"].FindControl("imgViewHistory"); // item["HistoryButton"].Controls[0] as ImageButton;
                    if (imgViewHistory != null)
                    {
                        //if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                        //    this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                        //    this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                        //    this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                        //    !string.IsNullOrEmpty(statusHandlingCode))
                        //{
                            imgViewHistory.Enabled = true;
                            imgViewHistory.ToolTip = "View approval history";
                        //}
                        //else
                        //{
                        //    imgViewHistory.Enabled = false;
                        //    imgViewHistory.ToolTip = "Control is disabled";
                        //}
                    }

                    //LinkButton viewHistoryLink = item["ViewHistoryLinkButton"].Controls[0] as LinkButton;
                    //if (viewHistoryLink != null)
                    //{
                    //    if (this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTSUBMITED.ToString() ||
                    //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTAPPROVED.ToString() ||
                    //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTREJECTED.ToString() ||
                    //        this.cboFilterOption.SelectedValue == UIHelper.OvertimeFilter.OTCANCELED.ToString() ||
                    //        statusHandlingCode == UIHelper.STATUS_HANDLING_CODE_OPEN)
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
                    CheckBox chkSelect = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    if (chkSelect != null)
                    {
                        if (statusHandlingCode == UIHelper.STATUS_HANDLING_CODE_OPEN)
                            chkSelect.Enabled = currentlyAssignedEmpNo == UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]) || Master.IsSystemAdmin || this.IsHRValidator;
                        else if (this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATAPPRVE.ToString())                            
                            chkSelect.Enabled = Master.IsSystemAdmin || this.IsHRValidator;
                        else
                            chkSelect.Enabled = false;

                        if (!chkSelect.Enabled)
                            chkSelect.Checked = false;
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
                //    if (this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATOPEN.ToString())
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
                #endregion

                #region Show/hide "Approval Level" field 
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "DistListDesc").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATOPEN.ToString())
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
                #endregion

                #region Show/hide "Approver Comments" field 
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "ApproverRemarks").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.rblAssignedTo.SelectedValue == "1")    // Currently Assigned to Me
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
                #endregion

                #region Show/hide "Approve?" field 
                //dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "OTWFApprovalDesc").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    if (this.rblAssignedTo.SelectedValue == "1")    // Currently Assigned to Me
                //        dynamicColumn.Visible = true;
                //    else
                //        dynamicColumn.Visible = false;
                //}
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

                    // Hide Created By
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CreatedByFullName").FirstOrDefault();
                    if (dynamicColumn != null)
                        dynamicColumn.Visible = false;

                    // Hide Created Date
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CreatedDate").FirstOrDefault();
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

                    // Hide Cancel link button
                    dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CancelButton").FirstOrDefault();
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

            this.txtRequisitionNo.Text = string.Empty;
            this.txtEmpNo.Text = string.Empty;
            this.txtAssigneeEmpNo.Text = string.Empty;
            this.txtCreatedByEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboStatus.SelectedValue = UIHelper.ApprovalStatus.STATOPEN.ToString();
            this.cboStatus.Text = "Open";
            this.btnCancelRequest.Visible = false;
            #endregion

            // Cler collections
            this.OTRequisitionList.Clear();
            this.OTRequisitionApprovalList.Clear();
            this.CheckedOTRequisitionList.Clear();

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
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check date range
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

            if (errorCount > 0)
            {
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

            GetOvertimeRequisition(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY
            ),
            false);
        }

        protected void btnFindCreator_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY
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

        protected void btnFindAssignee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY
            ),
            false);
        }

        protected void btnSubmitApproval_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.OTRequisitionList.Count == 0)
                {
                    DisplayFormLevelError("Could not proceed because there are no records currently selected in the grid.");
                }
                else
                {
                    List<EmployeeAttendanceEntity> approvedOTList = new List<EmployeeAttendanceEntity>();
                    List<EmployeeAttendanceEntity> rejectededOTList = new List<EmployeeAttendanceEntity>();

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
                                    // Get the approver remarks
                                    System.Web.UI.WebControls.TextBox txtRemarks = item["ApproverRemarks"].Controls[1] as System.Web.UI.WebControls.TextBox;
                                    if (txtRemarks != null)
                                    {
                                        selectedOTRequest.ApproverRemarks = txtRemarks.Text.Trim();
                                    }

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
                                            else
                                                continue;   // Skip to the next record since approval is not set to either "Yes" or "No"
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
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion

                    if (approvedOTList.Count == 0 &&
                        rejectededOTList.Count == 0)
                    {
                        throw new Exception("Could not proceed because there are no selected overtime request that has been approved or rejected.");
                    }
                    else
                    {
                        #region Process the approved overtime requisitions
                        if (approvedOTList.Count > 0)
                        {
                            ProcessApprovalAction(approvedOTList);
                        }
                        #endregion

                        #region Process the rejected overtime requisitions
                        if (rejectededOTList.Count > 0)
                        {
                            #region Check if justification is specified
                            int errorCount = 0;
                            StringBuilder sb = new StringBuilder();

                            foreach (EmployeeAttendanceEntity item in rejectededOTList)
                            {
                                if (item.IsRemarksRequired &&
                                    string.IsNullOrEmpty(item.ApproverRemarks))
                                {                                    
                                    sb.AppendLine(string.Format(@"Could not reject overtime request for employee no. {0} on {1} because there is no justification defined in the <b>Approver Comments</b> field. <br />", 
                                        item.EmpNo,
                                        Convert.ToDateTime(item.DT).ToString("dd-MMM-yyyy")));
                                    errorCount++;
                                }
                            }
                            #endregion

                            if (errorCount > 0)
                                throw new Exception(sb.ToString().Trim());
                            else
                                ProcessRejectionAction(rejectededOTList);
                        }
                        #endregion

                        // Refresh the grid
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
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
                    DisplayFormLevelError("Could not proceed because there are no records currently shown in the grid.");
                    return;
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
                    DisplayFormLevelError("Could not proceed because no records have been selected in the grid.");
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
                UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY,
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
                    DisplayFormLevelError("Could not proceed because there are no records currently selected in the grid.");
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

        protected void btnCancelDummy_Click(object sender, EventArgs e)
        {
            CancelOvertimeRequest(this.SelectedOTRequisitionList);
        }

        protected void btnCancelDummyGrid_Click(object sender, EventArgs e)
        {
            if (this.SelectedOvertimeRecord != null)
            {
                if (this.SelectedOvertimeRecord.StatusHandlingCode == UIHelper.STATUS_HANDLING_CODE_CLOSED)
                    CancelOvertimeRequest(new List<EmployeeAttendanceEntity>() { this.SelectedOvertimeRecord });
                else
                    CancelOvertimeRequest(this.SelectedOvertimeRecord);
            }
        }

        protected void btnCancelRequest_Click(object sender, EventArgs e)
        {
            try
            {
                #region Get the selected records
                // Reset collection
                this.SelectedOTRequisitionList.Clear();

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
                                this.SelectedOTRequisitionList.Add(selectedOTRecord);
                            }
                        }
                    }
                }
                #endregion

                if (this.SelectedOTRequisitionList.Count > 0)
                {
                    StringBuilder script = new StringBuilder();
                    script.Append("ConfirmButtonAction('");
                    script.Append(string.Concat(this.btnCancelDummy.ClientID, "','"));
                    script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                    script.Append(UIHelper.CONST_CANCEL_OT_REQUISITION_CONFIRMATION + "');");

                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Cancel OT Confirmation", script.ToString(), true);
                }
                else
                {
                    throw new Exception("Unable to perform cancellation because there are no overtime requests currently selected in the grid.");
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
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
                else if (this.ErrorType == ValidationErrorType.NoSelectedOTToCancel)
                {
                    validator.ErrorMessage = "Please select the overtime request to be cancelled on the grid.";
                    validator.ToolTip = "Please select the overtime request to be cancelled on the grid.";
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
                if (cboOTReason != null &&
                    this.OTReasonList != null)
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

                                    //if (txtDuration.Value >= 0 &&
                                    //    txtDuration.Value != otDurationOrig)
                                    //{
                                    //    txtDuration.Value = (txtDuration.Value * 100) + callOutValue;
                                    //}
                                    //else
                                    txtDuration.Value = otDurationOrig + callOutValue;
                                }
                                else
                                {
                                    txtDuration.MaxValue = otDurationOrig;

                                    //if (txtDuration.Value >= 0 &&
                                    //    txtDuration.Value != otDurationOrig)
                                    //{
                                    //    txtDuration.Value = txtDuration.Value;
                                    //}
                                    //else
                                    txtDuration.Value = otDurationOrig;
                                }
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

                    // Set the backcolor of the Remarks field
                    System.Web.UI.WebControls.TextBox txtRemarks = item["ApproverRemarks"].Controls[1] as System.Web.UI.WebControls.TextBox;
                    if (txtRemarks != null)
                    {
                        if (cboOTWFApprovalType.SelectedValue == "N")
                        {
                            txtRemarks.BackColor = System.Drawing.Color.Yellow;
                            txtRemarks.Enabled = true;
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.YellowGreen;
                        }
                        else if (cboOTWFApprovalType.SelectedValue == "Y")
                        {
                            txtRemarks.BackColor = System.Drawing.Color.White;
                            txtRemarks.Enabled = true;
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.Red;
                        }
                        else 
                        {
                            txtRemarks.BackColor = System.Drawing.Color.Gray;
                            txtRemarks.Enabled = false;
                            txtRemarks.ToolTip = "This field is disabled when there is no selected approval type.";
                            cboOTWFApprovalType.ForeColor = System.Drawing.Color.Orange;
                        }
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
                        if (autoID > 0 && this.OTRequisitionList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
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
                                item.OTWFApprovalDesc = chkOTWFApprove.Checked == true ? "Yes" : "-";
                                item.OTWFApprovalCode = chkOTWFApprove.Checked == true ? "Y" : "-";
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

                int otRequestNo = 0;
                GridDataItem item = this.gridSearchResults.MasterTableView.Items[((sender as ImageButton).Parent.Parent as GridDataItem).ItemIndex];
                if (item != null)
                    otRequestNo = UIHelper.ConvertObjectToInt(item["OTRequestNo"].Text);

                // Save current selected datagrid row
                if (autoID > 0 &&
                    otRequestNo > 0 &&
                    this.OTRequisitionList.Count > 0)
                {
                    EmployeeAttendanceEntity selectedRecord = this.OTRequisitionList
                        .Where(a => a.AutoID == autoID && a.OTRequestNo == otRequestNo)
                        .FirstOrDefault();
                    if (selectedRecord != null)
                    {
                        // Save the currently selected record
                        this.SelectedOvertimeRecord = selectedRecord;

                        // Display confirmation message
                        StringBuilder script = new StringBuilder();
                        script.Append("ConfirmButtonAction('");
                        script.Append(string.Concat(this.btnCancelDummyGrid.ClientID, "','"));
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
                            UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY
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
                                        UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY,
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

        protected void cboStatus_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            this.btnSearch_Click(this.btnSearch, new EventArgs());

            // Enable/disable the "Reassign to Others" button
            this.btnReassign.Enabled = this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATOPEN.ToString() || this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATALL.ToString();

            if (this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATAPPRVE.ToString() &&
                (this.IsHRValidator || this.Master.IsSystemAdmin))
            {
                this.btnCancelRequest.Visible = true;
                this.btnReassign.Visible = false;
            }
            else if (this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATREJECT.ToString() ||
                this.cboStatus.SelectedValue == UIHelper.ApprovalStatus.STATCANCEL.ToString())
            {
                this.btnCancelRequest.Visible = false;
                this.btnReassign.Visible = false;
            }
            else
            {
                this.btnCancelRequest.Visible = false;
                this.btnReassign.Visible = true;
            }
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

            this.txtRequisitionNo.Text = string.Empty;
            this.txtEmpNo.Text = string.Empty;
            this.txtAssigneeEmpNo.Text = string.Empty;
            this.txtCreatedByEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboStatus.Text = string.Empty;
            this.cboStatus.SelectedIndex = -1;
            this.btnCancelRequest.Visible = false;
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
            this.CostCenterList.Clear();
            this.SelectedOTRequisitionList.Clear();

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

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.OTRequisitionInqStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.OTRequisitionInqStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.OTRequisitionInqStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.OTRequisitionInqStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.OTRequisitionInqStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.OTRequisitionInqStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.OTRequisitionInqStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.OTRequisitionInqStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.OTRequisitionInqStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.OTRequisitionInqStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.OTRequisitionInqStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.OTRequisitionInqStorage.ContainsKey("OTRequisitionList"))
                this.OTRequisitionList = this.OTRequisitionInqStorage["OTRequisitionList"] as List<EmployeeAttendanceEntity>;
            else
                this.OTRequisitionList = null;

            if (this.OTRequisitionInqStorage.ContainsKey("OTReasonList"))
                this.OTReasonList = this.OTRequisitionInqStorage["OTReasonList"] as List<UDCEntity>;
            else
                this.OTReasonList = null;

            if (this.OTRequisitionInqStorage.ContainsKey("SelectedOvertimeRecord"))
                this.SelectedOvertimeRecord = this.OTRequisitionInqStorage["SelectedOvertimeRecord"] as EmployeeAttendanceEntity;
            else
                this.SelectedOvertimeRecord = null;

            if (this.OTRequisitionInqStorage.ContainsKey("IsOTApprove"))
                this.IsOTApprove = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["IsOTApprove"]);
            else
                this.IsOTApprove = false;

            if (this.OTRequisitionInqStorage.ContainsKey("IsOTWFApprove"))
                this.IsOTWFApprove = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["IsOTWFApprove"]);
            else
                this.IsOTWFApprove = false;

            if (this.OTRequisitionInqStorage.ContainsKey("IsOTApprovalHeaderClicked"))
                this.IsOTApprovalHeaderClicked = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["IsOTApprovalHeaderClicked"]);
            else
                this.IsOTApprovalHeaderClicked = false;

            if (this.OTRequisitionInqStorage.ContainsKey("IsOTWFApprovalHeaderClicked"))
                this.IsOTWFApprovalHeaderClicked = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["IsOTWFApprovalHeaderClicked"]);
            else
                this.IsOTWFApprovalHeaderClicked = false;

            if (this.OTRequisitionInqStorage.ContainsKey("OvertimeFilterOptionList"))
                this.OvertimeFilterOptionList = this.OTRequisitionInqStorage["OvertimeFilterOptionList"] as List<UserDefinedCodes>;
            else
                this.OvertimeFilterOptionList = null;

            if (this.OTRequisitionInqStorage.ContainsKey("CheckedOTRequisitionList"))
                this.CheckedOTRequisitionList = this.OTRequisitionInqStorage["CheckedOTRequisitionList"] as List<EmployeeAttendanceEntity>;
            else
                this.CheckedOTRequisitionList = null;

            if (this.OTRequisitionInqStorage.ContainsKey("OTRequisitionApprovalList"))
                this.OTRequisitionApprovalList = this.OTRequisitionInqStorage["OTRequisitionApprovalList"] as List<EmployeeAttendanceEntity>;
            else
                this.OTRequisitionApprovalList = null;

            if (this.OTRequisitionInqStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.OTRequisitionInqStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            if (this.OTRequisitionInqStorage.ContainsKey("IsHRValidator"))
                this.IsHRValidator = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["IsHRValidator"]);
            else
                this.IsHRValidator = false;

            if (this.OTRequisitionInqStorage.ContainsKey("SelectedOTRequisitionList"))
                this.SelectedOTRequisitionList = this.OTRequisitionInqStorage["SelectedOTRequisitionList"] as List<EmployeeAttendanceEntity>;
            else
                this.SelectedOTRequisitionList = null;

            FillComboData(false);
            #endregion

            #region Restore control values  
            if (this.OTRequisitionInqStorage.ContainsKey("txtRequisitionNo"))
                this.txtRequisitionNo.Text = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["txtRequisitionNo"]);
            else
                this.txtRequisitionNo.Text = string.Empty;

            if (this.OTRequisitionInqStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.OTRequisitionInqStorage.ContainsKey("txtAssigneeEmpNo"))
                this.txtAssigneeEmpNo.Text = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["txtAssigneeEmpNo"]);
            else
                this.txtAssigneeEmpNo.Text = string.Empty;

            if (this.OTRequisitionInqStorage.ContainsKey("txtCreatedByEmpNo"))
                this.txtCreatedByEmpNo.Text = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["txtCreatedByEmpNo"]);
            else
                this.txtCreatedByEmpNo.Text = string.Empty;

            if (this.OTRequisitionInqStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.OTRequisitionInqStorage.ContainsKey("cboMonth"))
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["cboMonth"]);
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.OTRequisitionInqStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.OTRequisitionInqStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OTRequisitionInqStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.OTRequisitionInqStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OTRequisitionInqStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.OTRequisitionInqStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.OTRequisitionInqStorage["chkPayPeriod"]);
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

            if (this.OTRequisitionInqStorage.ContainsKey("cboStatus"))
                this.cboStatus.SelectedValue = UIHelper.ConvertObjectToString(this.OTRequisitionInqStorage["cboStatus"]);
            else
            {
                this.cboStatus.Text = string.Empty;
                this.cboStatus.SelectedIndex = -1;
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

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag, string sourceControl = "")
        {
            this.OTRequisitionInqStorage.Clear();
            this.OTRequisitionInqStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.OTRequisitionInqStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.OTRequisitionInqStorage.Add("txtRequisitionNo", this.txtRequisitionNo.Text);
            this.OTRequisitionInqStorage.Add("txtAssigneeEmpNo", this.txtAssigneeEmpNo.Text);
            this.OTRequisitionInqStorage.Add("txtCreatedByEmpNo", this.txtCreatedByEmpNo.Text);
            this.OTRequisitionInqStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.OTRequisitionInqStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            this.OTRequisitionInqStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.OTRequisitionInqStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.OTRequisitionInqStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.OTRequisitionInqStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.OTRequisitionInqStorage.Add("cboStatus", this.cboStatus.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.OTRequisitionInqStorage.Add("CallerForm", this.CallerForm);
            this.OTRequisitionInqStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.OTRequisitionInqStorage.Add("SourceControl", sourceControl);
            this.OTRequisitionInqStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.OTRequisitionInqStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.OTRequisitionInqStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.OTRequisitionInqStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.OTRequisitionInqStorage.Add("OTRequisitionList", this.OTRequisitionList);
            this.OTRequisitionInqStorage.Add("OTReasonList", this.OTReasonList);
            this.OTRequisitionInqStorage.Add("SelectedOvertimeRecord", this.SelectedOvertimeRecord);
            this.OTRequisitionInqStorage.Add("IsOTApprove", this.IsOTApprove);
            this.OTRequisitionInqStorage.Add("IsOTWFApprove", this.IsOTWFApprove);
            this.OTRequisitionInqStorage.Add("IsOTApprovalHeaderClicked", this.IsOTApprovalHeaderClicked);
            this.OTRequisitionInqStorage.Add("IsOTWFApprovalHeaderClicked", this.IsOTWFApprovalHeaderClicked);
            this.OTRequisitionInqStorage.Add("OvertimeFilterOptionList", this.OvertimeFilterOptionList);
            this.OTRequisitionInqStorage.Add("CheckedOTRequisitionList", this.CheckedOTRequisitionList);
            this.OTRequisitionInqStorage.Add("OTRequisitionApprovalList", this.OTRequisitionApprovalList);
            this.OTRequisitionInqStorage.Add("CostCenterList", this.CostCenterList);
            this.OTRequisitionInqStorage.Add("IsHRValidator", this.IsHRValidator);
            this.OTRequisitionInqStorage.Add("SelectedOTRequisitionList", this.SelectedOTRequisitionList);
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
            FillStatusCombo(reloadFromDB, UIHelper.UDCSorterColumn.UDCSequenceNo, UIHelper.ApprovalStatus.STATOPEN.ToString());
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
                        item.OTRequestNo, item.AutoID, userID, userEmpNo, userEmpName, userEmpNo, userEmpName, null, null, item.RequestSubmissionDate, null, ref error, ref innerError);
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
                //UIHelper.DisplayJavaScriptMessage(this, "The selected overtime requisitions have been assigned to you successfully!");
                //this.btnReset_Click(this.btnReset, new EventArgs());
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
                #endregion

                #region Update the database
                int emailCounter = 0;
                foreach (EmployeeAttendanceEntity item in selectedRequisitionList)
                {
                    DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.GetNextWFActivity),
                        item.OTRequestNo, item.AutoID, item.CreatedByUserID, item.CreatedByEmpNo, item.CreatedByEmpName, assigneeEmpNo, assigneeEmpName, 
                        true, item.ApproverRemarks, item.RequestSubmissionDate, null, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) ||
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }

                    // Increment the counter
                    emailCounter++;
                }
                #endregion

                #region Send email to the approver
                if (ProcessWorkflowEmail())
                {
                    // Show success notification and refresh the grid
                    //UIHelper.DisplayJavaScriptMessage(this, "Selected requisitions have been approved sucessfully!");
                    //this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
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
                // Get WCF Instance
                if (rejectedOTList.Count == 0)
                    return;

                #region Initialize variables                                
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int assigneeEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string assigneeEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                #endregion

                #region Update the workflow
                foreach (EmployeeAttendanceEntity item in rejectedOTList)
                {
                    DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.GetNextWFActivity),
                        item.OTRequestNo, item.AutoID, item.CreatedByUserID, item.CreatedByEmpNo, item.CreatedByEmpName, assigneeEmpNo, assigneeEmpName,
                        false, item.ApproverRemarks, item.RequestSubmissionDate, null, ref error, ref innerError);
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
                        string approverName = string.Format("({0}) {1}", assigneeEmpNo, assigneeEmpName);

                        #region Build the email content
                        //sb.AppendLine(string.Format(@"<b>Employee Name:</b> {0}; " +
                        //                "<b>Position:</b> {1}; " +
                        //                "<b>Cost Center:</b> {2}; " +
                        //                "<b>Pay Grade:</b> {3}; " +
                        //                "<b>Shift Pat.:</b> {4}; " +
                        //                "<b>Sched. Shift:</b> {5}; " +
                        //                "<b>Actual Shift:</b> {6}; " +
                        //                "<b>Date:</b> {7}; " +
                        //                "<b>OT Start Time:</b>" + "<font color=" + "red" + ">" + " {8}</font>; " +
                        //                "<b>OT End Time:</b>" + "<font color=" + "red" + ">" + " {9}</font>; " +
                        //                "<b>OT Type:</b> {10}",
                        //    item.EmpFullName,
                        //    !string.IsNullOrEmpty(item.Position) ? item.Position : "Unknown",
                        //    item.CostCenterFullName,
                        //    item.PayGrade,
                        //    !string.IsNullOrEmpty(item.ShiftPatCode) ? item.ShiftPatCode : "Unknown",
                        //    !string.IsNullOrEmpty(item.ShiftCode) ? item.ShiftCode : "Unknown",
                        //    !string.IsNullOrEmpty(item.ActualShiftCode) ? item.ShiftCode : "Unknown",
                        //    UIHelper.ConvertObjectToDateString(item.DT),
                        //    UIHelper.ConvertObjectToTimeString(item.OTStartTime),
                        //    UIHelper.ConvertObjectToTimeString(item.OTEndTime),
                        //    item.OTType));

                        sb.AppendLine(string.Format(@"1. <b>Employee Name:</b> {0}", item.EmpName));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"2. <b>Position:</b> {0}", !string.IsNullOrEmpty(item.Position) ? item.Position : "Unknown"));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"3. <b>Cost Center:</b> {0}", item.CostCenterFullName));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"4. <b>Pay Grade:</b> {0}", item.PayGrade));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"5. <b>Shift Pattern:</b> {0}", !string.IsNullOrEmpty(item.ShiftPatCode) ? item.ShiftPatCode : "Unknown"));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"6. <b>Sched. Shift:</b> {0}", !string.IsNullOrEmpty(item.ShiftCode) ? item.ShiftCode : "Unknown"));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"7. <b>Actual Shift:</b> {0}", !string.IsNullOrEmpty(item.ActualShiftCode) ? item.ActualShiftCode : "Unknown"));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"8. <b>Date:</b> {0}", UIHelper.ConvertObjectToDateString(item.DT)));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"9. <b>OT Start Time:</b> {0}", UIHelper.ConvertObjectToTimeString(item.OTStartTime)));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"10. <b>OT End Time:</b> {0}", UIHelper.ConvertObjectToTimeString(item.OTEndTime)));
                        sb.AppendLine(@"<br /> <br />");
                        sb.AppendLine(string.Format(@"11. <b>OT Type:</b> {0}", item.OTType));                        
                        #endregion

                        SendRejectionEmail(item, sb.ToString().Trim(), item.ApproverRemarks, approverName, assigneeEmpNo);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Database Access
        private void GetOvertimeRequisition(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables      
                long otRequestNo = UIHelper.ConvertObjectToLong(this.txtRequisitionNo.Text);

                string costCenter = this.cboCostCenter.SelectedValue;
                if (costCenter == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    costCenter = string.Empty;

                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                string statusCode = this.cboStatus.Text.Trim();

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                int createdByEmpNo = UIHelper.ConvertObjectToInt(this.txtCreatedByEmpNo.Text);
                if (createdByEmpNo.ToString().Length == 4)
                {
                    createdByEmpNo += 10000000;

                    // Display Emp. No.
                    this.txtCreatedByEmpNo.Text = createdByEmpNo.ToString();
                }

                int assignedToEmpNo = UIHelper.ConvertObjectToInt(this.txtAssigneeEmpNo.Text);
                if (assignedToEmpNo.ToString().Length == 4)
                {
                    assignedToEmpNo += 10000000;

                    // Display Emp. No.
                    this.txtAssigneeEmpNo.Text = assignedToEmpNo.ToString();
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
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    var rawData = proxy.GetOvertimeRequisition(userEmpNo, otRequestNo, empNo, costCenter, createdByEmpNo, assignedToEmpNo, startDate, endDate, statusCode, ref error, ref innerError);
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
                ShowErrorMessage(ex);
            }
        }

        private void FillStatusCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
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
                    var source = proxy.GetUserDefinedCode(UIHelper.UDCGroupCodes.SWIPSTATUS.ToString(), ref error, ref innerError);
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
                this.cboStatus.DataSource = comboSource;
                this.cboStatus.DataTextField = "UDCDesc1";
                this.cboStatus.DataValueField = "UDCCode";
                this.cboStatus.DataBind();

                if (this.cboStatus.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboStatus.SelectedValue = defaultValue;
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
                    }
                }
                #endregion

                #region Initiate the workflow, send notification to the first approver
                if (ProcessWorkflowEmail())
                {
                    // Show success notification and refresh the grid
                    //UIHelper.DisplayJavaScriptMessage(this, "The selected overtime records have been submitted for approval sucessfully!");

                    RebindDataToGrid();

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

        private void CancelOvertimeRequest(List<EmployeeAttendanceEntity> otRequestList)
        {
            try
            {
                #region Initialize variables
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string userEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                #endregion

                foreach (EmployeeAttendanceEntity item in otRequestList)
                {
                    #region Cancel record in the database
                    error = innerError = string.Empty;
                    DatabaseSaveResult dbResult = proxy.ManageOvertimeRequest(2, item.OTRequestNo, userEmpNo, userEmpName, userID, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || 
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }
                    #endregion
                }

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
                                    sb.AppendLine(string.Format(@"{0}. <b>Employee Name:</b> {1}; " +
                                                    "<b>Position:</b> {2}; " +
                                                    "<b>Cost Center:</b> {3}; " +
                                                    "<b>Pay Grade:</b> {4}; " +
                                                    "<b>Shift Pat.:</b> {5}; " +
                                                    "<b>Sched. Shift:</b> {6}; " +
                                                    "<b>Actual Shift:</b> {7}; " +
                                                    "<b>Date:</b> {8}; " +
                                                    "<b>OT Start Time:</b>" + "<font color=" + "red" + ">" + " {9}</font>; " +
                                                    "<b>OT End Time:</b>" + "<font color=" + "red" + ">" + " {10}</font>; " +
                                                    "<b>OT Type:</b> {11}",
                                                counter,
                                                item.EmpFullName,
                                                !string.IsNullOrEmpty(item.Position) ? item.Position : "Unknown",
                                                item.CostCenterFullName,
                                                item.PayGrade,
                                                !string.IsNullOrEmpty(item.ShiftPatCode) ? item.ShiftPatCode : "Unknown",
                                                !string.IsNullOrEmpty(item.ShiftCode) ? item.ShiftCode : "Unknown",
                                                !string.IsNullOrEmpty(item.ActualShiftCode) ? item.ShiftCode : "Unknown",
                                                UIHelper.ConvertObjectToDateString(item.DT),
                                                UIHelper.ConvertObjectToTimeString(item.OTStartTime),
                                                UIHelper.ConvertObjectToTimeString(item.OTEndTime),
                                                item.OTType));
                                    sb.AppendLine(@"<br /> <br />");
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
                    recipientName = UIHelper.GetUserFirstName(emailData.CurrentlyAssignedEmpName);

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
                        UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY.Replace("~", string.Empty));

                string queryString = string.Format("?{0}={1}&{2}={3}",
                       UIHelper.QUERY_STRING_IS_ASSIGNED_KEY,
                       true.ToString(),
                       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                       UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY);

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

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool SendTestEmailMemo()
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
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

                empInfo = UIHelper.GetEmployeeEmailInfo(10003632);
                if (empInfo != null &&
                    !string.IsNullOrEmpty(empInfo.EmpEmail))
                {
                    toList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                }
                #endregion

                #endregion

                #region Build URL address
                string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                        UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY.Replace("~", string.Empty));

                string queryString = string.Format("?{0}={1}&{2}={3}",
                       UIHelper.QUERY_STRING_IS_ASSIGNED_KEY,
                       true.ToString(),
                       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                       UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY);

                StringBuilder url = new StringBuilder();
                url.Append(string.Concat(dynamicEndpointAddress, queryString.Trim()));
                #endregion

                #region Set Message Body
                string body = String.Empty;
                string htmLBody = string.Empty;
                string appPath = Server.MapPath(UIHelper.CONST_MEMO_TEMPLATE);
                string adminName = ConfigurationManager.AppSettings["AdminName"];

                // Build the message body
                body = String.Format(UIHelper.RetrieveXmlMessage(appPath),
                    recipientName,
                    adminName
                    ).Replace("&lt;", "<").Replace("&gt;", ">");

                // Format the message contents
                htmLBody = string.Format("<HTML><BODY><p style='border-spacing: 0px; font-weight: normal; font-family: Arial; font-size: 12pt;'>{0}</p></BODY></HTML>", body);
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

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool SendRejectionEmail(EmployeeAttendanceEntity overtimeRequest, string emailBody, string rejectionRemarks, string approverName, int approverNo)
        {
            try
            {
                #region Perform Validation
                //Check mail server
                string mailServer = ConfigurationManager.AppSettings["MailServer"];
                if (string.IsNullOrEmpty(mailServer))
                    return false;

                //Check the collection
                if (overtimeRequest == null)
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
                    recipientName = UIHelper.GetUserFirstName(overtimeRequest.CreatedByEmpName);
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
                                recipientName = UIHelper.GetUserFirstName(empInfo.EmpName);
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
                string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                        UIHelper.PAGE_OVERTIME_ENTRY.Replace("~", string.Empty));

                string queryString = string.Format("?{0}={1}&{2}={3}",
                       UIHelper.QUERY_STRING_IS_ASSIGNED_KEY,
                       true.ToString(),
                       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                       UIHelper.PAGE_OVERTIME_ENTRY);

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

                return true;
            }
            catch (Exception ex)
            {
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