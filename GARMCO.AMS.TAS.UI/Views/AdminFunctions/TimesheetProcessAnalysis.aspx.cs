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

namespace GARMCO.AMS.TAS.UI.Views.AdminFunctions
{
    public partial class TimesheetProcessAnalysis : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoProcessDate
        }

        private enum LogDetailType
        {
            NotDefined,
            GetSPULogs,
            GetTimesheetLogs,
            GetShiftPointerCount
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

        private Dictionary<string, object> ServiceLogDetailStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ServiceLogDetailStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ServiceLogDetailStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ServiceLogDetailStorage"] = value;
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

        private List<ServiceLogDetail> SPULogDetailList
        {
            get
            {
                List<ServiceLogDetail> list = ViewState["SPULogDetailList"] as List<ServiceLogDetail>;
                if (list == null)
                    ViewState["SPULogDetailList"] = list = new List<ServiceLogDetail>();

                return list;
            }
            set
            {
                ViewState["SPULogDetailList"] = value;
            }
        }

        private List<ServiceLogDetail> TimesheetLogDetailList
        {
            get
            {
                List<ServiceLogDetail> list = ViewState["TimesheetLogDetailList"] as List<ServiceLogDetail>;
                if (list == null)
                    ViewState["TimesheetLogDetailList"] = list = new List<ServiceLogDetail>();

                return list;
            }
            set
            {
                ViewState["TimesheetLogDetailList"] = value;
            }
        }

        private List<ServiceLogDetail> ShiftPointerLogDetailList
        {
            get
            {
                List<ServiceLogDetail> list = ViewState["ShiftPointerLogDetailList"] as List<ServiceLogDetail>;
                if (list == null)
                    ViewState["ShiftPointerLogDetailList"] = list = new List<ServiceLogDetail>();

                return list;
            }
            set
            {
                ViewState["ShiftPointerLogDetailList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.SVCLOGDETL.ToString());

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

                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Format("Welcome {0}",
                   UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]), UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));

                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_SERVICE_LOG_DETAIL_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_SERVICE_LOG_DETAIL_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnNew.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ServiceLogDetailStorage.Count > 0)
                {
                    if (this.ServiceLogDetailStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ServiceLogDetailStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("ServiceLogDetailStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    GetAttendanceDateFlags();

                    // Initialize controls
                    this.dtpProcessDate.MaxDate = DateTime.Now;
                    this.dtpProcessDate.SelectedDate = DateTime.Now;

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events

        #region SPU Log Detail Grid
        protected void gridSPULog_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToSPULogGrid();
        }

        protected void gridSPULog_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToSPULogGrid();
        }

        protected void gridSPULog_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.SPULogDetailList.Count > 0)
            {
                this.gridSPULog.DataSource = this.SPULogDetailList;
                this.gridSPULog.DataBind();

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
                        sortExpr.SortOrder = this.gridSPULog.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridSPULog.Rebind();
            }
            else
                InitializeDataToSPULogGrid();
        }

        protected void gridSPULog_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Open the Shift Pattern Entry page
                    //// Save session values
                    //StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //// Get data key value
                    //long autoID = UIHelper.ConvertObjectToLong(this.gridSPULog.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    //if (this.SPULogDetailList.Count > 0)
                    //{
                    //    ServiceLogDetail selectedRecord = this.SPULogDetailList
                    //        .Where(a => a.AutoID == autoID)
                    //        .FirstOrDefault();
                    //    if (selectedRecord != null && autoID > 0)
                    //    {
                    //        // Save to session
                    //        Session["SelectedResignedEmpRecord"] = selectedRecord;
                    //    }
                    //}

                    //// Redirect to Employee Training Entry page
                    //Response.Redirect
                    //(
                    //    String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY + "?{0}={1}&{2}={3}",
                    //    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //    UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ,
                    //    UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //    autoID
                    //),
                    //false);
                    #endregion
                }
            }
        }

        protected void gridSPULog_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToSPULogGrid()
        {
            if (this.SPULogDetailList.Count > 0)
            {
                this.gridSPULog.DataSource = this.SPULogDetailList;
                this.gridSPULog.DataBind();
            }
            else
                InitializeDataToSPULogGrid();
        }

        private void InitializeDataToSPULogGrid()
        {
            this.gridSPULog.DataSource = new List<ServiceLogDetail>();
            this.gridSPULog.DataBind();
        }
        #endregion

        #region Timesheet Log Detail Grid
        protected void gridTimesheetLog_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToTimesheetLogGrid();
        }

        protected void gridTimesheetLog_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToTimesheetLogGrid();
        }

        protected void gridTimesheetLog_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.TimesheetLogDetailList.Count > 0)
            {
                this.gridTimesheetLog.DataSource = this.TimesheetLogDetailList;
                this.gridTimesheetLog.DataBind();

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
                        sortExpr.SortOrder = this.gridTimesheetLog.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridTimesheetLog.Rebind();
            }
            else
                InitializeDataToTimesheetLogGrid();
        }

        protected void gridTimesheetLog_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                   
                }
            }
        }

        protected void gridTimesheetLog_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToTimesheetLogGrid()
        {
            if (this.TimesheetLogDetailList.Count > 0)
            {
                this.gridTimesheetLog.DataSource = this.TimesheetLogDetailList;
                this.gridTimesheetLog.DataBind();
            }
            else
                InitializeDataToTimesheetLogGrid();
        }

        private void InitializeDataToTimesheetLogGrid()
        {
            this.gridTimesheetLog.DataSource = new List<ServiceLogDetail>();
            this.gridTimesheetLog.DataBind();
        }
        #endregion

        #region Shift Pointer Grid
        protected void gridShiftPointer_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToShiftPointerGrid();
        }

        protected void gridShiftPointer_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToShiftPointerGrid();
        }

        protected void gridShiftPointer_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ShiftPointerLogDetailList.Count > 0)
            {
                this.gridShiftPointer.DataSource = this.ShiftPointerLogDetailList;
                this.gridShiftPointer.DataBind();

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
                        sortExpr.SortOrder = this.gridShiftPointer.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridShiftPointer.Rebind();
            }
            else
                InitializeDataToShiftPointerGrid();
        }

        protected void gridShiftPointer_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                   
                }
            }
        }

        protected void gridShiftPointer_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToShiftPointerGrid()
        {
            if (this.ShiftPointerLogDetailList.Count > 0)
            {
                this.gridShiftPointer.DataSource = this.ShiftPointerLogDetailList;
                this.gridShiftPointer.DataBind();
            }
            else
                InitializeDataToShiftPointerGrid();
        }

        private void InitializeDataToShiftPointerGrid()
        {
            this.gridShiftPointer.DataSource = new List<ServiceLogDetail>();
            this.gridShiftPointer.DataBind();
        }
        #endregion

        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.dtpProcessDate.SelectedDate = null;
            this.tabLogDetail.Tabs[0].Selected = true;

            // Clear collections
            this.SPULogDetailList.Clear();
            this.TimesheetLogDetailList.Clear();
            this.ShiftPointerLogDetailList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Reset the grid
            this.gridSPULog.CurrentPageIndex = 0;
            this.gridTimesheetLog.CurrentPageIndex = 0;
            this.gridShiftPointer.CurrentPageIndex = 0;

            InitializeDataToSPULogGrid();
            InitializeDataToTimesheetLogGrid();
            InitializeDataToShiftPointerGrid();
            #endregion
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                // Check Process Date
                if (this.dtpProcessDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoProcessDate.ToString();
                    this.ErrorType = ValidationErrorType.NoProcessDate;
                    this.cusValProcessDate.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }

                #endregion

                // Reset the grid's page index
                this.gridSPULog.CurrentPageIndex = 0;
                this.gridTimesheetLog.CurrentPageIndex = 0;
                this.gridShiftPointer.CurrentPageIndex = 0;

                DateTime? processDate = this.dtpProcessDate.SelectedDate;
                LogDetailType logType = LogDetailType.NotDefined;

                if (this.tabLogDetail.SelectedTab.Value == "valSPUTab")
                    logType = LogDetailType.GetSPULogs;
                else if (this.tabLogDetail.SelectedTab.Value == "valTimesheetProcessTab")
                    logType = LogDetailType.GetTimesheetLogs;
                else if (this.tabLogDetail.SelectedTab.Value == "valShiftPointerTab")
                    logType = LogDetailType.GetShiftPointerCount;

                GetLogDetails(logType, processDate);
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
                else if (this.ErrorType == ValidationErrorType.NoProcessDate)
                {
                    validator.ErrorMessage = "Process Date is required.";
                    validator.ToolTip = "Process Date is required.";
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

        protected void tabLogDetail_TabClick(object sender, RadTabStripEventArgs e)
        {
            try
            {
                DateTime? processDate = this.dtpProcessDate.SelectedDate;
                LogDetailType logType = LogDetailType.NotDefined;

                if (this.tabLogDetail.SelectedTab.Value == "valSPUTab")
                    logType = LogDetailType.GetSPULogs;
                else if (this.tabLogDetail.SelectedTab.Value == "valTimesheetProcessTab")
                    logType = LogDetailType.GetTimesheetLogs;
                else if (this.tabLogDetail.SelectedTab.Value == "valShiftPointerTab")
                    logType = LogDetailType.GetShiftPointerCount;

                GetLogDetails(logType, processDate);
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
            this.litLastSPURun.Text = string.Empty;
            this.litSwipeLastProcess.Text = string.Empty;
            this.dtpProcessDate.SelectedDate = null;            
            this.tabLogDetail.Tabs[0].Selected = true;
            #endregion

            // Clear collections
            this.SPULogDetailList.Clear();
            this.TimesheetLogDetailList.Clear();
            this.ShiftPointerLogDetailList.Clear();

            KillSessions();

            // Reset the grid
            this.gridSPULog.CurrentPageIndex = 0;
            this.gridTimesheetLog.CurrentPageIndex = 0;
            this.gridShiftPointer.CurrentPageIndex = 0;

            InitializeDataToSPULogGrid();
            InitializeDataToTimesheetLogGrid();
            InitializeDataToShiftPointerGrid();
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
            this.SPULogDetailList.Clear();

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
            if (this.ServiceLogDetailStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ServiceLogDetailStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ServiceLogDetailStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.ServiceLogDetailStorage.ContainsKey("SPULogDetailList"))
                this.SPULogDetailList = this.ServiceLogDetailStorage["SPULogDetailList"] as List<ServiceLogDetail>;
            else
                this.SPULogDetailList = null;

            if (this.ServiceLogDetailStorage.ContainsKey("TimesheetLogDetailList"))
                this.TimesheetLogDetailList = this.ServiceLogDetailStorage["TimesheetLogDetailList"] as List<ServiceLogDetail>;
            else
                this.TimesheetLogDetailList = null;

            if (this.ServiceLogDetailStorage.ContainsKey("ShiftPointerLogDetailList"))
                this.ShiftPointerLogDetailList = this.ServiceLogDetailStorage["ShiftPointerLogDetailList"] as List<ServiceLogDetail>;
            else
                this.ShiftPointerLogDetailList = null;


            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ServiceLogDetailStorage.ContainsKey("dtpProcessDate"))
                this.dtpProcessDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ServiceLogDetailStorage["dtpProcessDate"]);
            else
                this.dtpProcessDate.SelectedDate = null;
            #endregion

            // Refresh the grid
            RebindDataToSPULogGrid();

            // Set the grid attributes
            //this.gridSPULog.CurrentPageIndex = 0;
            //this.gridSPULog.MasterTableView.CurrentPageIndex = 0;
            //this.gridSPULog.MasterTableView.PageSize = 0;
            //this.gridSPULog.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ServiceLogDetailStorage.Clear();
            this.ServiceLogDetailStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ServiceLogDetailStorage.Add("dtpProcessDate", this.dtpProcessDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.ServiceLogDetailStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.ServiceLogDetailStorage.Add("SPULogDetailList", this.SPULogDetailList);
            this.ServiceLogDetailStorage.Add("TimesheetLogDetailList", this.TimesheetLogDetailList);
            this.ServiceLogDetailStorage.Add("ShiftPointerLogDetailList", this.ShiftPointerLogDetailList);
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
        private void GetLogDetails(LogDetailType logType, DateTime? processDate)
        {
            try
            {
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;

                // Initialize collections
                this.SPULogDetailList.Clear();
                this.TimesheetLogDetailList.Clear();
                this.ShiftPointerLogDetailList.Clear();

                switch (logType)
                {
                    case LogDetailType.GetSPULogs:
                        #region Get SPU logs
                        var rawDataSPU = proxy.GetTimesheetAndSPULogDetail(Convert.ToByte(logType), processDate, ref error, ref innerError);
                        if (!string.IsNullOrEmpty(error) || 
                            !string.IsNullOrEmpty(innerError))
                        {
                            if (!string.IsNullOrEmpty(innerError))
                                throw new Exception(error, new Exception(innerError));
                            else
                                throw new Exception(error);
                        }
                        else
                        {
                            if (rawDataSPU != null)
                            {
                                // Save to session
                                this.SPULogDetailList.AddRange(rawDataSPU.ToList());
                            }
                        }

                        // Bind data to the grid
                        RebindDataToSPULogGrid();

                        break;
                    #endregion

                    case LogDetailType.GetTimesheetLogs:
                        #region Get SPU logs
                        var rawDataTimesheet = proxy.GetTimesheetAndSPULogDetail(Convert.ToByte(logType), processDate, ref error, ref innerError);
                        if (!string.IsNullOrEmpty(error) || 
                            !string.IsNullOrEmpty(innerError))
                        {
                            if (!string.IsNullOrEmpty(innerError))
                                throw new Exception(error, new Exception(innerError));
                            else
                                throw new Exception(error);
                        }
                        else
                        {
                            if (rawDataTimesheet != null)
                            {
                                // Save to session
                                this.TimesheetLogDetailList.AddRange(rawDataTimesheet.ToList());
                            }
                        }

                        // Bind data to the grid
                        RebindDataToTimesheetLogGrid();

                        break;
                    #endregion

                    case LogDetailType.GetShiftPointerCount:
                        #region Get Shift Pointer count
                        var rawDataShiftPointer = proxy.GetTimesheetAndSPULogDetail(Convert.ToByte(logType), processDate, ref error, ref innerError);
                        if (!string.IsNullOrEmpty(error) ||
                            !string.IsNullOrEmpty(innerError))
                        {
                            if (!string.IsNullOrEmpty(innerError))
                                throw new Exception(error, new Exception(innerError));
                            else
                                throw new Exception(error);
                        }
                        else
                        {
                            if (rawDataShiftPointer != null)
                            {
                                // Save to session
                                this.ShiftPointerLogDetailList.AddRange(rawDataShiftPointer.ToList());
                            }
                        }

                        // Bind data to the grid
                        RebindDataToShiftPointerGrid();

                        break;
                        #endregion       
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetAttendanceDateFlags()
        {
            try
            {
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;

                var rawData = proxy.GetSystemValues(ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) ||
                    !string.IsNullOrEmpty(innerError))
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
                        // Bind data to controls
                        this.litLastSPURun.Text = rawData.ShiftPatternLastUpdated.HasValue ? Convert.ToDateTime(rawData.ShiftPatternLastUpdated).ToString("dd/MM/yyyy HH:mm:ss") : "Not defined";
                        this.litSwipeLastProcess.Text = rawData.SwipeLastProcessDate.HasValue ? Convert.ToDateTime(rawData.SwipeLastProcessDate).ToString("dd/MM/yyyy HH:mm:ss") : "Not defined";
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion                
    }
}