using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.DAL.Employee;
using GARMCO.Common.Object;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    public partial class ManualAttendance : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoSpecifiedEmpNo,
            NoEmpNo,
            NoDateIn,
            NoDateOut,
            NoTimeIn,
            NoTimeOut,
            InvalidSwipeDateRange,
            SpecifiedIDNotEmployee
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

        private Dictionary<string, object> ManualAttendanceStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ManualAttendanceStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ManualAttendanceStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ManualAttendanceStorage"] = value;
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

        private List<EmployeeAttendanceEntity> ManualAttendanceList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["ManualAttendanceList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["ManualAttendanceList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["ManualAttendanceList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> EmployeeAttendanceHistoryList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["EmployeeAttendanceHistoryList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["EmployeeAttendanceHistoryList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["EmployeeAttendanceHistoryList"] = value;
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
                    pageSize = this.gridAttendanceHistoryAll.MasterTableView.PageSize;

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

        private EmployeeAttendanceEntity CurrentManualAttendance
        {
            get
            {
                return ViewState["CurrentManualAttendance"] as EmployeeAttendanceEntity;
            }
            set
            {
                ViewState["CurrentManualAttendance"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.MANUALSWIP.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_MANUAL_ATTENDANCE_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (SecurityUserList.Count > 0 &&
                        SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() == null)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_MANUAL_ATTENDANCE_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSwipeIn.Enabled = this.btnSwipeOut.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnGet.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ManualAttendanceStorage.Count > 0)
                {
                    if (this.ManualAttendanceStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    string callerControlName = this.ManualAttendanceStorage.ContainsKey("CallerControlName")
                        ? UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["CallerControlName"]) : string.Empty;

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        switch(callerControlName)
                        {
                            case "btnFindEmployee":
                                this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                                this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                                this.litCostCenter.Text = string.Format("{0} - {1}",
                                    UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY]) != string.Empty ? UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY]) : UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                                    UIHelper.ConvertObjectToString(Server.UrlDecode(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY])));
                                break;

                            case "btnFindEmpHistory":
                                this.txtEmpNoHistory.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;
                        }                        
                    }

                    // Clear data storage
                    Session.Remove("ManualAttendanceStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("ManualAttendanceStorage");

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

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events

        #region Manual Attendance Grid Events
        protected void gridAttendanceHistoryAll_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetManualAttendanceHistory(true);
        }

        protected void gridAttendanceHistoryAll_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetManualAttendanceHistory(true);
        }

        protected void gridAttendanceHistoryAll_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ManualAttendanceList.Count > 0)
            {
                this.gridAttendanceHistoryAll.DataSource = this.ManualAttendanceList;
                this.gridAttendanceHistoryAll.DataBind();

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
                        sortExpr.SortOrder = this.gridAttendanceHistoryAll.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridAttendanceHistoryAll.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridAttendanceHistoryAll_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Open the Manual Timesheet data entry form
                    //dynamic itemObj = e.CommandSource;
                    //string itemText = itemObj.Text;

                    //// Get data key value
                    //long autoID = UIHelper.ConvertObjectToLong(this.gridAttendanceHistoryAll.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    //if (autoID > 0 && this.ManualAttendanceList.Count > 0)
                    //{
                    //    EmployeeAttendanceEntity selectedRecord = this.ManualAttendanceList
                    //        .Where(a => a.AutoID == autoID)
                    //        .FirstOrDefault();
                    //    if (selectedRecord != null && autoID > 0)
                    //    {
                    //        // Save to session
                    //        Session["SelectedManualTimesheet"] = selectedRecord;
                    //    }
                    //}

                    //#region Determine type of employee
                    //bool isContractor = false;
                    //Label lblIsContractor = item["IsContractor"].FindControl("lblIsContractor") as Label;
                    //if (lblIsContractor != null)
                    //{
                    //    isContractor = lblIsContractor.Text == "Yes" ? true : false;
                    //}
                    //#endregion

                    //if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    //{
                    //    #region Edit link is clicked
                    //    // Save session values
                    //    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //    Response.Redirect
                    //   (
                    //       String.Format(UIHelper.PAGE_MANUAL_TIMESHEET_ENTRY + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                    //       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //       UIHelper.PAGE_MANUAL_TIMESHEET_INQ,
                    //       UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //       autoID,
                    //       UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //       Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString(),
                    //       "IsContractor",
                    //       isContractor.ToString()
                    //   ),
                    //   false);
                    //    #endregion
                    //}
                    //else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    //{
                    //    #region View link is clicked
                    //    // Save session values
                    //    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //    Response.Redirect
                    //   (
                    //       String.Format(UIHelper.PAGE_MANUAL_TIMESHEET_ENTRY + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                    //       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //       UIHelper.PAGE_MANUAL_TIMESHEET_INQ,
                    //       UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //       autoID,
                    //       UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //       Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString(),
                    //       "IsContractor",
                    //       isContractor.ToString()
                    //   ),
                    //   false);
                    //    #endregion
                    //}
                    #endregion
                }
            }
        }

        protected void gridAttendanceHistoryAll_ItemDataBound(object sender, GridItemEventArgs e)
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
            if (this.ManualAttendanceList.Count > 0)
            {
                int totalRecords = this.ManualAttendanceList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridAttendanceHistoryAll.VirtualItemCount = totalRecords;
                else
                    this.gridAttendanceHistoryAll.VirtualItemCount = 1;

                this.gridAttendanceHistoryAll.DataSource = this.ManualAttendanceList;
                this.gridAttendanceHistoryAll.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridAttendanceHistoryAll.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridAttendanceHistoryAll.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Employee Attendance History Events
        protected void gridEmpAttendanceHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindEmployeeAttendanceGrid();
        }

        protected void gridEmpAttendanceHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindEmployeeAttendanceGrid();
        }

        protected void gridEmpAttendanceHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.EmployeeAttendanceHistoryList.Count > 0)
            {
                this.gridEmpAttendanceHistory.DataSource = this.EmployeeAttendanceHistoryList;
                this.gridEmpAttendanceHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridEmpAttendanceHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridEmpAttendanceHistory.Rebind();
            }
            else
                InitializeEmployeeAttendanceGrid();
        }

        protected void gridEmpAttendanceHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridEmpAttendanceHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindEmployeeAttendanceGrid()
        {
            if (this.EmployeeAttendanceHistoryList.Count > 0)
            {
                this.gridEmpAttendanceHistory.DataSource = this.EmployeeAttendanceHistoryList;
                this.gridEmpAttendanceHistory.DataBind();
            }
            else
                InitializeEmployeeAttendanceGrid();
        }

        private void InitializeEmployeeAttendanceGrid()
        {
            this.gridEmpAttendanceHistory.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridEmpAttendanceHistory.DataBind();
        }
        #endregion
        
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.dtpDateIn.SelectedDate = null;
            this.dtpDateOut.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;

            // Cler collections
            this.ManualAttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Reset the grid
            this.gridAttendanceHistoryAll.VirtualItemCount = 1;
            this.gridAttendanceHistoryAll.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridAttendanceHistoryAll.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridAttendanceHistoryAll.PageSize;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            if (errorCount > 0)
            {
                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }

            #endregion

            // Reset page index
            this.gridAttendanceHistoryAll.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridAttendanceHistoryAll.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridAttendanceHistoryAll.PageSize;

            GetManualAttendanceHistory(true);
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
                this.litEmpName.Text = UIHelper.CONST_NOT_DEFINED;
                this.litPosition.Text = UIHelper.CONST_NOT_DEFINED;
                this.litCostCenter.Text = UIHelper.CONST_NOT_DEFINED;
                #endregion

                #region Get the employee information
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                // Check if valid employee no.
                if (empNo < 10000000)
                {
                    this.txtGeneric.Text = ValidationErrorType.SpecifiedIDNotEmployee.ToString();
                    this.ErrorType = ValidationErrorType.SpecifiedIDNotEmployee;
                    this.cusValEmpNo.Validate();
                    return;
                }

                //string error = string.Empty;
                //string innerError = string.Empty;

                //EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
                //if (empInfo != null)
                //{
                //    if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                //    {
                //        #region Check if cost center exist in the allowed cost center list
                //        //if (this.Master.AllowedCostCenterList.Count > 0)
                //        //{
                //        //    string allowedCC = this.Master.AllowedCostCenterList
                //        //        .Where(a => a == UIHelper.ConvertObjectToString(empInfo.CostCenter))
                //        //        .FirstOrDefault();
                //        //    if (!string.IsNullOrEmpty(allowedCC))
                //        //    {
                //        this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
                //        this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
                //        this.litCostCenter.Text = string.Format("{0} - {1}",
                //            empInfo.CostCenter,
                //            empInfo.CostCenterName);
                //        //    }
                //        //    else
                //        //    {
                //        //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                //        //    }
                //        //}
                //        #endregion
                //    }
                //    else
                //    {
                //        #region Get employee info from the employee master
                //        DALProxy proxy = new DALProxy();
                //        var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                //        if (rawData != null)
                //        {
                //            //if (this.Master.AllowedCostCenterList.Count > 0)
                //            //{
                //            //    string allowedCC = this.Master.AllowedCostCenterList
                //            //        .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
                //            //        .FirstOrDefault();
                //            //    if (!string.IsNullOrEmpty(allowedCC))
                //            //    {
                //            this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                //            this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                //            this.litCostCenter.Text = string.Format("{0} - {1}",
                //               rawData.CostCenter,
                //               rawData.CostCenterName);
                //            //    }
                //            //    else
                //            //    {
                //            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                //            //    }
                //            //}
                //        }
                //        #endregion
                //    }
                //}
                #endregion

                // Get the employee information
                GetEmployeeInfo(empNo);

                // Get the attendance history
                GetEmployeeAttendanceHistory(true);

                #region Get the employee's last swipe status                
                //DateTime swipeDate = DateTime.Now.Date;
                //GetEmployeeLastSwipeStatus(empNo, swipeDate);

                //if (this.EmployeeSwipeStatus == UIHelper.SwipeTypes.IN)
                //{
                //    this.rblSwipeIn.Checked = false;
                //    this.rblSwipeOut.Checked = true;
                //    this.rblSwipeOut_CheckedChanged(this.rblSwipeOut, new EventArgs());
                //}
                //else if (this.EmployeeSwipeStatus == UIHelper.SwipeTypes.OUT)
                //{                    
                //    this.rblSwipeOut.Checked = false;
                //    this.rblSwipeIn.Checked = true;
                //    this.rblSwipeIn_CheckedChanged(this.rblSwipeIn, new EventArgs());
                //}
                //else
                //{
                //    this.rblSwipeOut.Checked = false;
                //    this.rblSwipeIn.Checked = false;
                //    this.dtpDateIn.SelectedDate = null;
                //    this.dtpTimeIn.SelectedDate = null;
                //    this.dtpDateOut.SelectedDate = null;
                //    this.dtpTimeOut.SelectedDate = null;

                //    this.dtpDateIn.Enabled = this.dtpTimeIn.Enabled = false;
                //    this.dtpDateOut.Enabled = this.dtpTimeOut.Enabled = false;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, "btnFindEmployee");

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_MANUAL_TIMESHEET_INQ
            ),
            false);
        }

        protected void btnFindEmpHistory_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, "btnFindEmpHistory");

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_MANUAL_TIMESHEET_INQ
            ),
            false);
        }

        protected void btnSwipeIn_Click(object sender, EventArgs e)
        {
            try
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
                }

                if (empNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Date In 
                if (this.dtpDateIn.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDateIn.ToString();
                    this.ErrorType = ValidationErrorType.NoDateIn;
                    this.cusValSwipeIn.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Time In 
                if (this.dtpTimeIn.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoTimeIn.ToString();
                    this.ErrorType = ValidationErrorType.NoTimeIn;
                    this.cusValSwipeIn.Validate();
                    errorCount++;
                }
                #endregion

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }

                #endregion

                #region Save to database
                EmployeeAttendanceEntity attendanceData = new EmployeeAttendanceEntity()
                {
                    AutoID = 0,
                    dtOUT = null,
                    TimeOut = null,
                    EmpNo = empNo,
                    dtIN = this.dtpDateIn.SelectedDate,                    
                    TimeIn = this.dtpTimeIn.SelectedDate,                    
                    CreatedUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                    CreatedTime = DateTime.Now
                };

                SaveChanges(UIHelper.SaveType.Insert, attendanceData);
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnSwipeOut_Click(object sender, EventArgs e)
        {
            try
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
                }

                if (empNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Date Out 
                if (this.dtpDateOut.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDateOut.ToString();
                    this.ErrorType = ValidationErrorType.NoDateOut;
                    this.cusValSwipeOut.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Time In 
                if (this.dtpTimeOut.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoTimeOut.ToString();
                    this.ErrorType = ValidationErrorType.NoTimeOut;
                    this.cusValSwipeOut.Validate();
                    errorCount++;
                }
                #endregion

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }

                #endregion

                #region Save to database
                UIHelper.SaveType saveType = this.CurrentManualAttendance.AutoID > 0 ? UIHelper.SaveType.Update : UIHelper.SaveType.Insert;

                if (saveType == UIHelper.SaveType.Insert)
                {
                    EmployeeAttendanceEntity attendanceData = new EmployeeAttendanceEntity()
                    {
                        AutoID = this.CurrentManualAttendance.AutoID,
                        EmpNo = empNo,
                        dtOUT = this.dtpDateOut.SelectedDate,
                        TimeOut = this.dtpTimeOut.SelectedDate,
                        CreatedUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                        CreatedTime = DateTime.Now
                    };

                    SaveChanges(saveType, attendanceData);
                }
                else if (saveType == UIHelper.SaveType.Update)
                {
                    EmployeeAttendanceEntity attendanceData = new EmployeeAttendanceEntity()
                    {
                        AutoID = this.CurrentManualAttendance.AutoID,
                        EmpNo = empNo,
                        dtOUT = this.dtpDateOut.SelectedDate,
                        TimeOut = this.dtpTimeOut.SelectedDate,
                        LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                        LastUpdateTime = DateTime.Now
                    };

                    SaveChanges(saveType, attendanceData);
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            #region Reset controls
            // Employee Information section
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = UIHelper.CONST_NOT_DEFINED;
            this.litPosition.Text = UIHelper.CONST_NOT_DEFINED;
            this.litCostCenter.Text = UIHelper.CONST_NOT_DEFINED;
            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;

            // Attendance Action section            
            this.dtpDateIn.SelectedDate = null;
            this.dtpDateOut.SelectedDate = null;
            this.dtpTimeIn.SelectedDate = null;
            this.dtpTimeOut.SelectedDate = null;
            this.dtpDateIn.Enabled = false;
            this.dtpDateOut.Enabled = false;
            this.dtpTimeIn.Enabled = false;
            this.dtpTimeOut.Enabled = false;
            this.rblSwipeIn.Checked = false;
            this.rblSwipeOut.Checked = false;
            this.rblSwipeIn.Enabled = false;
            this.rblSwipeOut.Enabled = false;
            //this.rblSwipeIn_CheckedChanged(this.rblSwipeIn, new EventArgs());

            // Hide buttons
            this.btnSwipeIn.Visible = false;
            this.btnSwipeOut.Visible = false;
            #endregion

            // Clear sessions
            ViewState["EmployeeSwipeStatus"] = null;
            this.EmployeeAttendanceHistoryList.Clear();

            // Reset the grid
            this.gridEmpAttendanceHistory.CurrentPageIndex = 0;
            InitializeEmployeeAttendanceGrid();

            // Refresh Manual Attendance History grid
            GetManualAttendanceHistory(true);
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
                else if (this.ErrorType == ValidationErrorType.NoDateIn)
                {
                    validator.ErrorMessage = "Swipe in date is required.";
                    validator.ToolTip = "Swipe in date In is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateOut)
                {
                    validator.ErrorMessage = "Swipe out date is required.";
                    validator.ToolTip = "Swipe out date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeIn)
                {
                    validator.ErrorMessage = "Time In is required.";
                    validator.ToolTip = "Time In is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeOut)
                {
                    validator.ErrorMessage = "Time Out is required.";
                    validator.ToolTip = "Time Out is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidSwipeDateRange)
                {
                    validator.ErrorMessage = "Swipe Date range is invalid. Make sure that the start date is less than the end date.";
                    validator.ToolTip = "Swipe Date range is invalid. Make sure that the start date is less than the end date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.SpecifiedIDNotEmployee)
                {
                    validator.ErrorMessage = "The specified ID is not an employee. Please enter a valid employee number!";
                    validator.ToolTip = "The specified ID is not an employee. Please enter a valid employee number!";
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

        protected void rblSwipeIn_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rblSwipeIn.Checked)
            {
                this.rblSwipeOut.Checked = false;
                this.btnSwipeIn.Visible = true;
                this.btnSwipeOut.Visible = false;
                //this.dtpTimeIn.Enabled = true;
                this.dtpTimeOut.Enabled = false;

                // Set the Swipe date
                this.dtpDateIn.SelectedDate = DateTime.Now;
                this.dtpTimeIn.SelectedDate = DateTime.Now;
                this.dtpDateOut.SelectedDate = null;
                this.dtpTimeOut.SelectedDate = null;

                // Set the focus
                this.dtpTimeIn.Focus();
            }
        }

        protected void rblSwipeOut_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rblSwipeOut.Checked)
            {
                this.rblSwipeIn.Checked = false;
                this.btnSwipeIn.Visible = false;
                this.btnSwipeOut.Visible = true;
                this.dtpTimeIn.Enabled = false;
                //this.dtpTimeOut.Enabled = true;

                // Set the Swipe date
                this.dtpDateOut.SelectedDate = DateTime.Now;
                this.dtpTimeOut.SelectedDate = DateTime.Now;
                this.dtpDateIn.SelectedDate = null;
                this.dtpTimeIn.SelectedDate = null;

                // Set the focus
                this.dtpTimeOut.Focus();
            }
        }

        protected void chkSearchFilter_CheckedChanged(object sender, EventArgs e)
        {
            this.panManualAttendanceFilter.Style[HtmlTextWriterStyle.Display] = this.chkSearchFilter.Checked ? string.Empty : "none";
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            // Employee Information section
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = UIHelper.CONST_NOT_DEFINED;
            this.litPosition.Text = UIHelper.CONST_NOT_DEFINED;
            this.litCostCenter.Text = UIHelper.CONST_NOT_DEFINED;
            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;

            // Attendance Action section            
            this.dtpDateIn.SelectedDate = null;
            this.dtpDateOut.SelectedDate = null;
            this.dtpTimeIn.SelectedDate = null;
            this.dtpTimeOut.SelectedDate = null;
            this.dtpDateIn.Enabled = false;
            this.dtpDateOut.Enabled = false;
            this.dtpTimeIn.Enabled = false;
            this.dtpTimeOut.Enabled = false;
            this.rblSwipeIn.Checked = false;
            this.rblSwipeOut.Checked = false;
            this.rblSwipeIn.Enabled = false;
            this.rblSwipeOut.Enabled = false;
            //this.rblSwipeIn_CheckedChanged(this.rblSwipeIn, new EventArgs());

            // Hide buttons
            this.btnSwipeIn.Visible = false;
            this.btnSwipeOut.Visible = false;

            // Manual Attendance History section
            this.txtEmpNoHistory.Text = string.Empty;
            this.dtpSwipeStartDate.SelectedDate = null;
            this.dtpSwipeEndDate.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.chkSearchFilter.Checked = false;
            this.chkSearchFilter_CheckedChanged(this.chkSearchFilter, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridAttendanceHistoryAll.VirtualItemCount = 1;
            this.gridAttendanceHistoryAll.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridAttendanceHistoryAll.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridAttendanceHistoryAll.PageSize;

            InitializeDataToGrid();
            InitializeEmployeeAttendanceGrid();
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
            this.ManualAttendanceList.Clear();
            this.EmployeeAttendanceHistoryList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["EmployeeSwipeStatus"] = null;
            ViewState["CurrentManualAttendance"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ManualAttendanceStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ManualAttendanceStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ManualAttendanceStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.ManualAttendanceStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.ManualAttendanceStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.ManualAttendanceStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.ManualAttendanceStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.ManualAttendanceStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.ManualAttendanceStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.ManualAttendanceStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.ManualAttendanceStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.ManualAttendanceStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.ManualAttendanceStorage.ContainsKey("ManualAttendanceList"))
                this.ManualAttendanceList = this.ManualAttendanceStorage["ManualAttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.ManualAttendanceList = null;

            if (this.ManualAttendanceStorage.ContainsKey("EmployeeAttendanceHistoryList"))
                this.EmployeeAttendanceHistoryList = this.ManualAttendanceStorage["EmployeeAttendanceHistoryList"] as List<EmployeeAttendanceEntity>;
            else
                this.EmployeeAttendanceHistoryList = null;

            if (this.ManualAttendanceStorage.ContainsKey("CurrentManualAttendance"))
                this.CurrentManualAttendance = this.ManualAttendanceStorage["CurrentManualAttendance"] as EmployeeAttendanceEntity;
            else
                this.CurrentManualAttendance = null;

            FillComboData(false);
            #endregion

            #region Restore control values  
            // Employee Information section          
            if (this.ManualAttendanceStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.ManualAttendanceStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.ManualAttendanceStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.ManualAttendanceStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.ManualAttendanceStorage.ContainsKey("imgPhoto"))
                this.imgPhoto.ImageUrl = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["imgPhoto"]);
            else
                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;

            // Attendance Action section
            if (this.ManualAttendanceStorage.ContainsKey("rblSwipeIn"))
                this.rblSwipeIn.Checked = UIHelper.ConvertObjectToBolean(this.ManualAttendanceStorage["rblSwipeIn"]);
            else
                this.rblSwipeIn.Checked = false;

            if (this.ManualAttendanceStorage.ContainsKey("rblSwipeOut"))
                this.rblSwipeOut.Checked = UIHelper.ConvertObjectToBolean(this.ManualAttendanceStorage["rblSwipeOut"]);
            else
                this.rblSwipeOut.Checked = false;

            if (this.ManualAttendanceStorage.ContainsKey("dtpDateIn"))
                this.dtpDateIn.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualAttendanceStorage["dtpDateIn"]);
            else
                this.dtpDateIn.SelectedDate = null;

            if (this.ManualAttendanceStorage.ContainsKey("dtpTimeIn"))
                this.dtpTimeIn.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualAttendanceStorage["dtpTimeIn"]);
            else
                this.dtpTimeIn.SelectedDate = null;

            if (this.ManualAttendanceStorage.ContainsKey("dtpDateOut"))
                this.dtpDateOut.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualAttendanceStorage["dtpDateOut"]);
            else
                this.dtpDateOut.SelectedDate = null;

            if (this.ManualAttendanceStorage.ContainsKey("dtpTimeOut"))
                this.dtpTimeOut.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualAttendanceStorage["dtpTimeOut"]);
            else
                this.dtpTimeOut.SelectedDate = null;

            // Manual Attendance History section
            if (this.ManualAttendanceStorage.ContainsKey("txtEmpNoHistory"))
                this.txtEmpNoHistory.Text = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["txtEmpNoHistory"]);
            else
                this.txtEmpNoHistory.Text = string.Empty;

            if (this.ManualAttendanceStorage.ContainsKey("dtpSwipeStartDate"))
                this.dtpSwipeStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualAttendanceStorage["dtpSwipeStartDate"]);
            else
                this.dtpSwipeStartDate.SelectedDate = null;

            if (this.ManualAttendanceStorage.ContainsKey("dtpSwipeEndDate"))
                this.dtpSwipeEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualAttendanceStorage["dtpSwipeEndDate"]);
            else
                this.dtpSwipeEndDate.SelectedDate = null;

            if (this.ManualAttendanceStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();
            RebindEmployeeAttendanceGrid();

            // Set the grid attributes
            this.gridAttendanceHistoryAll.CurrentPageIndex = this.CurrentPageIndex;
            this.gridAttendanceHistoryAll.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridAttendanceHistoryAll.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridAttendanceHistoryAll.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag, string controlName = "")
        {
            this.ManualAttendanceStorage.Clear();
            this.ManualAttendanceStorage.Add("FormFlag", formFlag.ToString());

            this.ManualAttendanceStorage.Add("CallerControlName", controlName);

            #region Save control values to session
            // Employee Information section
            this.ManualAttendanceStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.ManualAttendanceStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.ManualAttendanceStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.ManualAttendanceStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.ManualAttendanceStorage.Add("imgPhoto", this.imgPhoto.ImageUrl);

            // Attendance Action section
            this.ManualAttendanceStorage.Add("rblSwipeIn", this.rblSwipeIn.Checked);
            this.ManualAttendanceStorage.Add("rblSwipeOut", this.rblSwipeOut.Checked);
            this.ManualAttendanceStorage.Add("dtpDateIn", this.dtpDateIn.SelectedDate);
            this.ManualAttendanceStorage.Add("dtpTimeIn", this.dtpTimeIn.SelectedDate);
            this.ManualAttendanceStorage.Add("dtpDateOut", this.dtpDateOut.SelectedDate);
            this.ManualAttendanceStorage.Add("dtpTimeOut", this.dtpDateOut.SelectedDate);

            // Manual Attendance History section
            this.ManualAttendanceStorage.Add("txtEmpNoHistory", this.txtEmpNoHistory.Text.Trim());
            this.ManualAttendanceStorage.Add("dtpSwipeStartDate", this.dtpSwipeStartDate.SelectedDate);
            this.ManualAttendanceStorage.Add("dtpSwipeEndDate", this.dtpSwipeEndDate.SelectedDate);
            this.ManualAttendanceStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.ManualAttendanceStorage.Add("CallerForm", this.CallerForm);
            this.ManualAttendanceStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.ManualAttendanceStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.ManualAttendanceStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.ManualAttendanceStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.ManualAttendanceStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.ManualAttendanceStorage.Add("ManualAttendanceList", this.ManualAttendanceList);
            this.ManualAttendanceStorage.Add("EmployeeAttendanceHistoryList", this.EmployeeAttendanceHistoryList);
            this.ManualAttendanceStorage.Add("CurrentManualAttendance", this.CurrentManualAttendance);
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

        private void LoadEmployeePhoto(int empNo, string empPhotoPath)
        {
            try
            {
                bool isPhotoFound = false;
                string imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", empPhotoPath, empNo);
                string imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", empPhotoPath, empNo);

                #region Begin searching for bitmap photo                                
                if (File.Exists(imageFullPath_BMP))
                {
                    this.imgPhoto.ImageUrl = imageFullPath_BMP;
                    isPhotoFound = true;
                }
                else
                {
                    if (empNo > 10000000)
                    {
                        imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", empPhotoPath, empNo - 10000000);
                        if (File.Exists(imageFullPath_BMP))
                        {
                            this.imgPhoto.ImageUrl = imageFullPath_BMP;
                            isPhotoFound = true;
                        }
                        else
                        {
                            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                        }
                    }
                    else
                    {
                        this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                    }
                }
                #endregion

                if (!isPhotoFound)
                {
                    #region Search for JPEG photo
                    if (File.Exists(imageFullPath_JPG))
                    {
                        this.imgPhoto.ImageUrl = imageFullPath_JPG;
                        isPhotoFound = true;
                    }
                    else
                    {
                        if (empNo > 10000000)
                        {
                            imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", empPhotoPath, empNo - 10000000);
                            if (File.Exists(imageFullPath_JPG))
                            {
                                this.imgPhoto.ImageUrl = imageFullPath_JPG;
                                isPhotoFound = true;
                            }
                            else
                            {
                                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                            }
                        }
                        else
                        {
                            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Database Access
        private void GetManualAttendanceHistory(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNoHistory.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNoHistory.Text = empNo.ToString();
                }

                int autoID = 0;
                string costCenter = this.cboCostCenter.SelectedValue;
                DateTime? dateIn = this.dtpSwipeStartDate.SelectedDate;
                DateTime? dateOut = this.dtpSwipeEndDate.SelectedDate;

                // Initialize record count
                this.lblRecordCountAll.Text = "0 record found";
                this.gridAttendanceHistoryAll.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ManualAttendanceList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetManualTimesheetEntry(autoID, empNo, costCenter, dateIn, dateOut, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                this.ManualAttendanceList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.ManualAttendanceList.Count > 0)
                {
                    int totalRecords = this.ManualAttendanceList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridAttendanceHistoryAll.VirtualItemCount = totalRecords;
                    else
                        this.gridAttendanceHistoryAll.VirtualItemCount = 1;

                    this.gridAttendanceHistoryAll.DataSource = this.ManualAttendanceList;
                    this.gridAttendanceHistoryAll.DataBind();

                    //Display the record count
                    this.lblRecordCountAll.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
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

        private void GetEmployeeAttendanceHistory(bool reloadDataFromDB = false)
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

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridEmpAttendanceHistory.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.EmployeeAttendanceHistoryList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetManualTimesheetEntry(0, empNo, null, null, null, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                this.EmployeeAttendanceHistoryList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.EmployeeAttendanceHistoryList.Count > 0)
                {
                    int totalRecords = this.EmployeeAttendanceHistoryList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridEmpAttendanceHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridEmpAttendanceHistory.VirtualItemCount = 1;

                    this.gridEmpAttendanceHistory.DataSource = this.EmployeeAttendanceHistoryList;
                    this.gridEmpAttendanceHistory.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeEmployeeAttendanceGrid();
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

        private void GetEmployeeLastSwipeStatus(int empNo, DateTime swipeDate)
        {
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                int swipeStatusID = proxy.GetLastSwipeStatus(empNo, swipeDate, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    //if (swipeStatusID == Convert.ToInt32(UIHelper.SwipeTypes.IN))
                    //    this.EmployeeSwipeStatus = UIHelper.SwipeTypes.IN;
                    //else if (swipeStatusID == Convert.ToInt32(UIHelper.SwipeTypes.OUT))
                    //    this.EmployeeSwipeStatus = UIHelper.SwipeTypes.OUT;
                    //else
                    //    this.EmployeeSwipeStatus = UIHelper.SwipeTypes.Unknown;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetEmployeeInfo(int empNo)
        {
            try
            {
                #region Initialize controls
                this.dtpDateIn.SelectedDate = null;
                this.dtpDateOut.SelectedDate = null;
                this.dtpTimeIn.SelectedDate = null;
                this.dtpTimeOut.SelectedDate = null;
                this.dtpDateIn.Enabled = false;
                this.dtpDateOut.Enabled = false;
                this.dtpTimeIn.Enabled = false;
                this.dtpTimeOut.Enabled = false;
                this.rblSwipeIn.Checked = false;
                this.rblSwipeOut.Checked = false;
                this.rblSwipeIn.Enabled = false;
                this.rblSwipeOut.Enabled = false;
                #endregion

                string error = string.Empty;
                string innerError = string.Empty;
                DALProxy proxy = new DALProxy();

                EmployeeAttendanceEntity rawData = proxy.GetEmployeeDetailForManualAttendance(empNo, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (rawData != null)
                    {
                        // Save to session
                        this.CurrentManualAttendance = rawData;

                        #region Get the employee photo                                
                        string empPhotoPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["EmployeePhotoPath"]);
                        if (!string.IsNullOrEmpty(empPhotoPath))
                        {
                            LoadEmployeePhoto(empNo, empPhotoPath);
                        }
                        #endregion

                        #region Bind data to controls
                        this.litEmpName.Text = this.CurrentManualAttendance.EmpName;
                        this.litPosition.Text = this.CurrentManualAttendance.Position;
                        this.litCostCenter.Text = this.CurrentManualAttendance.CostCenterFullName;
                        this.rblSwipeIn.Enabled = this.rblSwipeOut.Enabled = true;
                        #endregion

                        #region Determine the last attendance status
                        if (this.CurrentManualAttendance.SwipeCode == UIHelper.SwipeStatus.CheckOUT.ToString())
                        {
                            // The last employee swipe record is a checked out, then this will be a new check-in
                            this.rblSwipeIn.Checked = true;
                            this.rblSwipeIn_CheckedChanged(this.rblSwipeIn, new EventArgs());

                            // Set the entity 
                            this.CurrentManualAttendance.AutoID = 0;
                            this.CurrentManualAttendance.dtOUT = null;
                            this.CurrentManualAttendance.TimeOut = null;
                        }
                        else
                        {
                            // Attendance will be a check-out
                            this.rblSwipeOut.Checked = true;
                            this.rblSwipeOut_CheckedChanged(this.rblSwipeOut, new EventArgs());

                            // Display the last check-in time
                            //this.dtpDateIn.SelectedDate = this.CurrentManualAttendance.dtIN;
                            //this.dtpTimeIn.SelectedDate = this.CurrentManualAttendance.dtIN;
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, EmployeeAttendanceEntity attendanceData)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                DALProxy proxy = new DALProxy();

                proxy.SaveManualAttendance(Convert.ToInt32(saveType), attendanceData, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    #region Clear the form
                    //this.rblSwipeOut.Checked = false;
                    //this.rblSwipeIn.Checked = false;
                    //this.dtpDateIn.SelectedDate = null;
                    //this.dtpTimeIn.SelectedDate = null;
                    //this.dtpDateOut.SelectedDate = null;
                    //this.dtpTimeOut.SelectedDate = null;
                    //this.dtpDateIn.Enabled = this.dtpTimeIn.Enabled = false;
                    //this.dtpDateOut.Enabled = this.dtpTimeOut.Enabled = false;
                    //this.btnSwipeIn.Visible = false;
                    //this.btnSwipeOut.Visible = false;

                    // Refresh employee information
                    GetEmployeeInfo(attendanceData.EmpNo);

                    // Refresh Employee Attendance History grid
                    GetEmployeeAttendanceHistory(true);

                    // Clear the form
                    //this.btnClear_Click(this.btnClear, new EventArgs());

                    // Refresh Manual Attendance History grid
                    GetManualAttendanceHistory(true);
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion                
    }
}