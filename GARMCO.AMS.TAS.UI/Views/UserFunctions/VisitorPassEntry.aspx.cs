using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class VisitorPassEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoVisitDate,
            NoVisitorName,
            NoIDNumber,
            NoVisitorCardNo,
            NoPersonToVisitEmpNo,
            NoRemarks,
            NoDateIn,
            NoDateOut,
            NoTimeIn,
            NoTimeOut,
            NoTimeInAndOut,
            NoSwipeDate,
            NoSwipeTime,
            NoSwipeType,
            InvalidDateRange,
            InvalidTimeIn,
            InvalidTimeOut,
            InvalidSwipeDate,
            InvalidSwipeTime,
            NoRecordToUpdate,
            NoRecordToDelete,
            NoRecordToPrint,
            DateDifferenceExceedLimit,
            SwipeDateLessThanVisitDate,
            SpecifiedIDNotVisitor
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

        private Dictionary<string, object> VisitorPassEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["VisitorPassEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["VisitorPassEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["VisitorPassEntryStorage"] = value;
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

        private int VisitEmployeeNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["VisitEmployeeNo"]);
            }
            set
            {
                ViewState["VisitEmployeeNo"] = value;
            }
        }

        private UIHelper.DataLoadTypes CurrentFormLoadType
        {
            get
            {
                UIHelper.DataLoadTypes result = UIHelper.DataLoadTypes.OpenReadonlyRecord;
                if (ViewState["CurrentFormLoadType"] != null)
                {
                    try
                    {
                        result = (UIHelper.DataLoadTypes)Enum.Parse(typeof(UIHelper.DataLoadTypes), UIHelper.ConvertObjectToString(ViewState["CurrentFormLoadType"]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            set
            {
                ViewState["CurrentFormLoadType"] = value;
            }
        }

        private VisitorPassEntity CurrentVisitorPass
        {
            get
            {
                return ViewState["CurrentVisitorPass"] as VisitorPassEntity;
            }
            set
            {
                ViewState["CurrentVisitorPass"] = value;
            }
        }

        private long LogID
        {
            get
            {
                return UIHelper.ConvertObjectToLong(ViewState["LogID"]);
            }
            set
            {
                ViewState["LogID"] = value;
            }
        }

        private bool IsBlockedVisitor
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsBlockedVisitor"]);
            }
            set
            {
                ViewState["IsBlockedVisitor"] = value;
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
                    pageSize = this.gridSwipeHistory.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
            }
        }

        private List<VisitorSwipeEntity> SwipeDataList
        {
            get
            {
                List<VisitorSwipeEntity> list = ViewState["SwipeDataList"] as List<VisitorSwipeEntity>;
                if (list == null)
                    ViewState["SwipeDataList"] = list = new List<VisitorSwipeEntity>();

                return list;
            }
            set
            {
                ViewState["SwipeDataList"] = value;
            }
        }

        private List<VisitorSwipeEntity> CheckedSwipeDataList
        {
            get
            {
                List<VisitorSwipeEntity> list = ViewState["CheckedSwipeDataList"] as List<VisitorSwipeEntity>;
                if (list == null)
                    ViewState["CheckedSwipeDataList"] = list = new List<VisitorSwipeEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedSwipeDataList"] = value;
            }
        }

        private VisitorSwipeEntity SelectedSwipeRecord
        {
            get
            {
                return ViewState["SelectedSwipeRecord"] as VisitorSwipeEntity;
            }
            set
            {
                ViewState["SelectedSwipeRecord"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.VPASSENTRY.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_VISITOR_PASS_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (SecurityUserList.Count > 0 &&
                        SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() == null)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_VISITOR_PASS_ENTRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Enabled = this.Master.IsCreateAllowed;
                this.btnDelete.Enabled = this.Master.IsDeleteAllowed;
                this.btnViewReport.Enabled = this.Master.IsPrintAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnGet.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.VisitorPassEntryStorage.Count > 0)
                {
                    if (this.VisitorPassEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["FormFlag"]);
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
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY]) != string.Empty ? UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY]) : UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                            UIHelper.ConvertObjectToString(Server.UrlDecode(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY])));
                        this.litCCManager.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_MANAGER_KEY]);
                        this.litSupervisor.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_SUPERVISOR_KEY]);
                        this.litExtNo.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EXTENSION_KEY]);

                        // Set the session variables
                        this.VisitEmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    this.VisitorPassEntryStorage.Clear();
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.VisitorPassEntryStorage.Clear();
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();

                    #region Check if need to load existing record
                    if (this.LogID > 0)
                    {
                        GetVisitorPassData(this.LogID);
                    }
                    #endregion      
                }

                InitializeControls(this.CurrentFormLoadType);

                //this.btnBack.Visible = !string.IsNullOrEmpty(this.CallerForm);
            }                        

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridSwipeHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetVisitorSwipeHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridSwipeHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetVisitorSwipeHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridSwipeHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.SwipeDataList.Count > 0)
            {
                this.gridSwipeHistory.DataSource = this.SwipeDataList;
                this.gridSwipeHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridSwipeHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridSwipeHistory.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridSwipeHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Perform Command action
                    if (UIHelper.ConvertObjectToString(e.CommandArgument) == "DeleteButton")
                    {
                        #region Delete button is clicked
                        StringBuilder script = new StringBuilder();
                        script.Append("ConfirmButtonAction('");
                        script.Append(string.Concat(this.btnRemoveGridItem.ClientID, "','"));
                        script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                        script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Action Confirmation", script.ToString(), true);

                        // Save the selected grid item to session
                        DateTime? swipeTime = UIHelper.ConvertObjectToDate(this.gridSwipeHistory.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("SwipeTime"));
                        DateTime? swipeDate = UIHelper.ConvertObjectToDate(item["SwipeDate"].Text);
                        long swipeID = UIHelper.ConvertObjectToLong(item["SwipeID"].Text);

                        if (swipeID > 0)
                        {
                            this.SelectedSwipeRecord = this.SwipeDataList
                                .Where(a => a.SwipeID == swipeID)
                                .FirstOrDefault();
                        }
                        else
                        {
                            if (swipeTime.HasValue && swipeDate.HasValue)
                            {
                                VisitorSwipeEntity selectedRecord = this.SwipeDataList
                                    .Where(a => a.SwipeDate == swipeDate && a.SwipeTime == swipeTime)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Save to session
                                    this.SelectedSwipeRecord = selectedRecord;
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Initialize variables and objects
                        dynamic itemObj = e.CommandSource;
                        string itemText = itemObj.Text;

                        // Get data key value
                        DateTime? swipeTime = UIHelper.ConvertObjectToDate(this.gridSwipeHistory.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("SwipeTime"));
                        DateTime? swipeDate = UIHelper.ConvertObjectToDate(item["SwipeDate"].Text);
                        long swipeID = UIHelper.ConvertObjectToLong(item["SwipeID"].Text);

                        if (swipeID > 0)
                        {
                            VisitorSwipeEntity selectedRecord = this.SwipeDataList
                                .Where(a => a.SwipeID == swipeID)
                                .FirstOrDefault();
                            if (selectedRecord != null)
                            {
                                // Save to session
                                this.SelectedSwipeRecord = selectedRecord;
                            }
                        }
                        else
                        {
                            if (swipeTime.HasValue && swipeDate.HasValue)
                            {
                                VisitorSwipeEntity selectedRecord = this.SwipeDataList
                                    .Where(a => a.SwipeDate == swipeDate && a.SwipeTime == swipeTime)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Save to session
                                    this.SelectedSwipeRecord = selectedRecord;
                                }
                            }
                        }
                        #endregion

                        if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                        {
                            #region Enable selected record for editing
                            if (this.SelectedSwipeRecord != null)
                            {
                                // Bind data to controls
                                this.txtSwipeID.Value = this.SelectedSwipeRecord.SwipeID;
                                this.dtpSwipeDate.SelectedDate = this.SelectedSwipeRecord.SwipeDate;
                                this.dtpSwipeTime.SelectedDate = this.SelectedSwipeRecord.SwipeTime;
                                this.cboSwipeType.SelectedValue = this.SelectedSwipeRecord.SwipeTypeCode;

                                // Setup buttons
                                this.btnAddSwipe.Enabled = false;
                                this.btnUpdateSwipe.Enabled = true;

                                // Set focus to swipe time
                                this.dtpSwipeTime.Focus();
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
        }

        protected void gridSwipeHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    // Initialize variables
                    long swipeID = UIHelper.ConvertObjectToLong(item["SwipeID"].Text);
                    long logID = UIHelper.ConvertObjectToLong(item["LogID"].Text);
                    int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);

                    #region Enable/Disable Edit link
                    System.Web.UI.WebControls.LinkButton editLink = item["EditLinkButton"].Controls[0] as System.Web.UI.WebControls.LinkButton;
                    if (editLink != null)
                    {
                        if (this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord ||
                            (
                                this.CurrentFormLoadType == UIHelper.DataLoadTypes.EditExistingRecord && 
                                (swipeID > 0 || (swipeID == 0 && logID > 0))
                            ))
                        {
                            editLink.Enabled = true;
                            editLink.ForeColor = System.Drawing.Color.Blue;
                        }
                        else
                        {
                            editLink.Enabled = false;
                            editLink.ForeColor = System.Drawing.Color.Gray;
                        }
                    }
                    #endregion

                    #region Enable/Disable Delete link
                    System.Web.UI.WebControls.ImageButton deleteLink = item["DeleteButton"].Controls[0] as System.Web.UI.WebControls.ImageButton;
                    if (deleteLink != null)
                    {
                        if (this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord ||
                            (
                                this.CurrentFormLoadType == UIHelper.DataLoadTypes.EditExistingRecord &&
                                (swipeID > 0 || (swipeID == 0 && logID > 0))
                            ))
                        {
                            deleteLink.Enabled = true;
                            deleteLink.ImageUrl = @"~/Images/delete_enabled_icon.png";
                            deleteLink.ToolTip = "Delete selected record";
                        }
                        else
                        {
                            deleteLink.Enabled = false;
                            deleteLink.ImageUrl = @"~/Images/delete_disabled_icon.png";
                            deleteLink.ToolTip = "Delete functionality is disabled";
                        }
                    }
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.SwipeDataList.Count > 0)
            {
                int totalRecords = this.SwipeDataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridSwipeHistory.VirtualItemCount = totalRecords;
                else
                    this.gridSwipeHistory.VirtualItemCount = 1;

                this.gridSwipeHistory.DataSource = this.SwipeDataList.OrderBy(a => a.SwipeTime);
                this.gridSwipeHistory.DataBind();
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSwipeHistory.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridSwipeHistory.DataBind();
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            // Visitor Information section
            this.dtpVisitDate.SelectedDate = null;
            this.txtVisitorName.Text = string.Empty;
            this.txtIDNumber.Text = string.Empty;
            this.txtVisitorCardNo.Text = string.Empty;

            // Person to Visit Information section
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.litSupervisor.Text = "Not defined";
            this.litCCManager.Text = "Not defined";
            this.litExtNo.Text = "Not defined";

            // Log Information section
            this.txtSwipeID.Text = string.Empty;
            this.dtpSwipeDate.SelectedDate = null;
            this.dtpSwipeTime.SelectedDate = null;
            this.cboSwipeType.SelectedIndex = -1;
            this.cboSwipeType.Text = string.Empty;
            this.txtRemarks.Text = string.Empty;
            this.chkBlockVisitor.Checked = false;

            //this.dtpDateOut.SelectedDate = null;
            //this.dtpTimeOut.Text = string.Empty;
            //this.chkSwipeIn.Checked = false;
            //this.chkSwipeIn_CheckedChanged(this.chkSwipeIn, new EventArgs());
            //this.chkSwipeOut.Checked = false;
            //this.chkSwipeOut_CheckedChanged(this.chkSwipeOut, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridSwipeHistory.VirtualItemCount = 1;
            this.gridSwipeHistory.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSwipeHistory.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSwipeHistory.PageSize;

            InitializeDataToGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.LogID = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY]);

            #region Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_FORM_LOAD_TYPE]);
            if (formLoadType != string.Empty)
            {
                UIHelper.DataLoadTypes loadType = UIHelper.DataLoadTypes.OpenReadonlyRecord;
                try
                {
                    loadType = (UIHelper.DataLoadTypes)Enum.Parse(typeof(UIHelper.DataLoadTypes), formLoadType);
                }
                catch (Exception)
                {
                }
                this.CurrentFormLoadType = loadType;
            }
            #endregion
        }

        public void KillSessions()
        {
            // Clear collections
            this.SwipeDataList.Clear();
            this.CheckedSwipeDataList.Clear();

            ViewState["CustomErrorMsg"] = null;
            ViewState["VisitEmployeeNo"] = null;
            ViewState["CurrentVisitorPass"] = null;
            ViewState["LogID"] = null;
            ViewState["IsBlockedVisitor"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["SelectedSwipeRecord"] = null;

            // Clear all Viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.VisitorPassEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.VisitorPassEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("LogID"))
                this.LogID = UIHelper.ConvertObjectToLong(this.VisitorPassEntryStorage["LogID"]);
            else
                this.LogID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["CurrentFormLoadType"]);
            if (formLoadType != string.Empty)
            {
                UIHelper.DataLoadTypes loadType = UIHelper.DataLoadTypes.OpenReadonlyRecord;
                try
                {
                    loadType = (UIHelper.DataLoadTypes)Enum.Parse(typeof(UIHelper.DataLoadTypes), formLoadType);
                }
                catch (Exception)
                {
                }
                this.CurrentFormLoadType = loadType;
            }
            #endregion

            #region Restore session values
            if (this.VisitorPassEntryStorage.ContainsKey("CurrentVisitorPass"))
                this.CurrentVisitorPass = this.VisitorPassEntryStorage["CurrentVisitorPass"] as VisitorPassEntity;
            else
                this.CurrentVisitorPass = null;

            if (this.VisitorPassEntryStorage.ContainsKey("VisitEmployeeNo"))
                this.VisitEmployeeNo = UIHelper.ConvertObjectToInt(this.VisitorPassEntryStorage["VisitEmployeeNo"]);
            else
                this.VisitEmployeeNo = 0;

            if (this.VisitorPassEntryStorage.ContainsKey("IsBlockedVisitor"))
                this.IsBlockedVisitor = UIHelper.ConvertObjectToBolean(this.VisitorPassEntryStorage["IsBlockedVisitor"]);
            else
                this.IsBlockedVisitor = false;

            if (this.VisitorPassEntryStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.VisitorPassEntryStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.VisitorPassEntryStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.VisitorPassEntryStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.VisitorPassEntryStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.VisitorPassEntryStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.VisitorPassEntryStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.VisitorPassEntryStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.VisitorPassEntryStorage.ContainsKey("SwipeDataList"))
                this.SwipeDataList = this.VisitorPassEntryStorage["SwipeDataList"] as List<VisitorSwipeEntity>;
            else
                this.SwipeDataList = null;

            if (this.VisitorPassEntryStorage.ContainsKey("CheckedSwipeDataList"))
                this.CheckedSwipeDataList = this.VisitorPassEntryStorage["CheckedSwipeDataList"] as List<VisitorSwipeEntity>;
            else
                this.CheckedSwipeDataList = null;

            if (this.VisitorPassEntryStorage.ContainsKey("SelectedSwipeRecord"))
                this.SelectedSwipeRecord = this.VisitorPassEntryStorage["SelectedSwipeRecord"] as VisitorSwipeEntity;
            else
                this.SelectedSwipeRecord = null;
            #endregion

            #region Restore control values    

            #region Visitor Information section
            if (this.VisitorPassEntryStorage.ContainsKey("txtVisitorName"))
                this.txtVisitorName.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["txtVisitorName"]);
            else
                this.txtVisitorName.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("txtIDNumber"))
                this.txtIDNumber.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["txtIDNumber"]);
            else
                this.txtIDNumber.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("txtVisitorCardNo"))
                this.txtVisitorCardNo.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["txtVisitorCardNo"]);
            else
                this.txtVisitorCardNo.Text = string.Empty;
            #endregion

            #region Person to Visit Information section 
            if (this.VisitorPassEntryStorage.ContainsKey("dtpVisitDate"))
                this.dtpVisitDate.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorPassEntryStorage["dtpVisitDate"]);
            else
                this.dtpVisitDate.SelectedDate = null;

            if (this.VisitorPassEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("litSupervisor"))
                this.litSupervisor.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["litSupervisor"]);
            else
                this.litSupervisor.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("litCCManager"))
                this.litCCManager.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["litCCManager"]);
            else
                this.litCCManager.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("litExtNo"))
                this.litExtNo.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["litExtNo"]);
            else
                this.litExtNo.Text = string.Empty;
            #endregion

            #region Swipe History section
            if (this.VisitorPassEntryStorage.ContainsKey("txtSwipeID"))
                this.txtSwipeID.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["txtSwipeID"]);
            else
                this.txtSwipeID.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("dtpSwipeDate"))
                this.dtpSwipeDate.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorPassEntryStorage["dtpSwipeDate"]);
            else
                this.dtpSwipeDate.SelectedDate = null;

            if (this.VisitorPassEntryStorage.ContainsKey("dtpDateOut"))
                this.dtpDateOut.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorPassEntryStorage["dtpDateOut"]);
            else
                this.dtpDateOut.SelectedDate = null;

            if (this.VisitorPassEntryStorage.ContainsKey("dtpSwipeTime"))
                this.dtpSwipeTime.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorPassEntryStorage["dtpSwipeTime"]);
            else
                this.dtpSwipeTime.SelectedDate = null;

            if (this.VisitorPassEntryStorage.ContainsKey("dtpTimeOut"))
                this.dtpTimeOut.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorPassEntryStorage["dtpTimeOut"]);
            else
                this.dtpTimeOut.SelectedDate = null;

            if (this.VisitorPassEntryStorage.ContainsKey("chkSwipeIn"))
                this.chkSwipeIn.Checked = UIHelper.ConvertObjectToBolean(this.VisitorPassEntryStorage["chkSwipeIn"]);
            else
                this.chkSwipeIn.Checked = false;

            //this.chkSwipeIn_CheckedChanged(this.chkSwipeIn, new EventArgs());

            if (this.VisitorPassEntryStorage.ContainsKey("chkSwipeOut"))
                this.chkSwipeOut.Checked = UIHelper.ConvertObjectToBolean(this.VisitorPassEntryStorage["chkSwipeOut"]);
            else
                this.chkSwipeOut.Checked = false;

            //this.chkSwipeOut_CheckedChanged(this.chkSwipeOut, new EventArgs());

            if (this.VisitorPassEntryStorage.ContainsKey("cboSwipeType"))
                this.cboSwipeType.SelectedValue = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["cboSwipeType"]);
            else
            {
                this.cboSwipeType.SelectedIndex = -1;
                this.cboSwipeType.Text = string.Empty;
            }
            #endregion

            #region Other Details section
            if (this.VisitorPassEntryStorage.ContainsKey("txtRemarks"))
                this.txtRemarks.Text = UIHelper.ConvertObjectToString(this.VisitorPassEntryStorage["txtRemarks"]);
            else
                this.txtRemarks.Text = string.Empty;

            if (this.VisitorPassEntryStorage.ContainsKey("chkBlockVisitor"))
                this.chkBlockVisitor.Checked = UIHelper.ConvertObjectToBolean(this.VisitorPassEntryStorage["chkBlockVisitor"]);
            else
                this.chkBlockVisitor.Checked = false;
            #endregion

            #endregion

            #region Reset the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridSwipeHistory.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSwipeHistory.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSwipeHistory.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSwipeHistory.MasterTableView.DataBind();
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.VisitorPassEntryStorage.Clear();
            this.VisitorPassEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            // Visitor Information section
            this.VisitorPassEntryStorage.Add("dtpVisitDate", this.dtpVisitDate.SelectedDate);
            this.VisitorPassEntryStorage.Add("txtVisitorName", this.txtVisitorName.Text.Trim());
            this.VisitorPassEntryStorage.Add("txtIDNumber", this.txtIDNumber.Text.Trim());
            this.VisitorPassEntryStorage.Add("txtVisitorCardNo", this.txtVisitorCardNo.Text.Trim());

            // Person to Visit Information section   
            this.VisitorPassEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text);
            this.VisitorPassEntryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.VisitorPassEntryStorage.Add("litPosition", this.litPosition.Text.Trim());            
            this.VisitorPassEntryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.VisitorPassEntryStorage.Add("litSupervisor", this.litSupervisor.Text.Trim());
            this.VisitorPassEntryStorage.Add("litCCManager", this.litCCManager.Text.Trim());
            this.VisitorPassEntryStorage.Add("litExtNo", this.litExtNo.Text.Trim());

            // Swipes History section
            this.VisitorPassEntryStorage.Add("txtSwipeID", this.txtSwipeID.Text);
            this.VisitorPassEntryStorage.Add("dtpSwipeDate", this.dtpSwipeDate.SelectedDate);
            this.VisitorPassEntryStorage.Add("dtpDateOut", this.dtpDateOut.SelectedDate);
            this.VisitorPassEntryStorage.Add("dtpSwipeTime", this.dtpSwipeTime.SelectedDate);
            this.VisitorPassEntryStorage.Add("dtpTimeOut", this.dtpTimeOut.SelectedDate);
            this.VisitorPassEntryStorage.Add("chkSwipeIn", this.chkSwipeIn.Checked);
            this.VisitorPassEntryStorage.Add("chkSwipeOut", this.chkSwipeOut.Checked);
            this.VisitorPassEntryStorage.Add("cboSwipeType", this.cboSwipeType.SelectedValue);

            // Other Details section
            this.VisitorPassEntryStorage.Add("txtRemarks", this.txtRemarks.Text.Trim());
            this.VisitorPassEntryStorage.Add("chkBlockVisitor", this.chkBlockVisitor.Checked);
            #endregion

            #region Save Query String values to collection
            this.VisitorPassEntryStorage.Add("CallerForm", this.CallerForm);
            this.VisitorPassEntryStorage.Add("LogID", this.LogID);
            this.VisitorPassEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.VisitorPassEntryStorage.Add("SwipeDataList", this.SwipeDataList);
            this.VisitorPassEntryStorage.Add("CheckedSwipeDataList", this.CheckedSwipeDataList);
            this.VisitorPassEntryStorage.Add("CurrentVisitorPass", this.CurrentVisitorPass);
            this.VisitorPassEntryStorage.Add("VisitEmployeeNo", this.VisitEmployeeNo);
            this.VisitorPassEntryStorage.Add("IsBlockedVisitor", this.IsBlockedVisitor);
            this.VisitorPassEntryStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.VisitorPassEntryStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.VisitorPassEntryStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.VisitorPassEntryStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.VisitorPassEntryStorage.Add("SelectedSwipeRecord", this.SelectedSwipeRecord);
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

        private void InitializeControls(UIHelper.DataLoadTypes formLoadType)
        {
            // Initialize buttons
            this.btnGet.Enabled = false;
            this.btnFindEmployee.Enabled = false;
            this.btnCheckVisit.Visible = false;
            this.btnSave.Visible = false;
            this.btnDelete.Visible = false;
            this.btnReset.Visible = false;
            this.btnViewReport.Visible = false;

            switch (formLoadType)
            {
                case UIHelper.DataLoadTypes.CreateNewRecord:
                    #region Create new record
                    // Setup controls 
                    this.txtEmpNo.Enabled = true;
                    this.txtVisitorName.Enabled = true;
                    this.txtIDNumber.Enabled = true;
                    this.txtVisitorCardNo.Enabled = true;
                    this.dtpVisitDate.Enabled = true;
                    this.dtpVisitDate.SelectedDate = DateTime.Now;
                    this.dtpVisitDate_SelectedDateChanged(this.dtpVisitDate, new Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs(null, this.dtpVisitDate.SelectedDate));

                    // Setup buttons
                    this.btnGet.Enabled = true;
                    this.btnFindEmployee.Enabled = true;
                    this.btnCheckVisit.Visible = true;
                    this.btnSave.Visible = true;
                    this.btnDelete.Visible = false;
                    this.btnReset.Visible = true;
                    this.btnViewReport.Visible = false;

                    // Swipe buttons
                    this.btnAddSwipe.Enabled = true;
                    this.btnUpdateSwipe.Enabled = false;

                    // Setup panels
                    //this.trSwipeHistoryTimeEntry.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    //this.trSwipeHistoryButton.Style[HtmlTextWriterStyle.Display] = "none";
                    //this.tblGrid.Style[HtmlTextWriterStyle.Display] = "none";
                    //this.trCheckOffense.Style[HtmlTextWriterStyle.Display] = string.Empty;

                    // Set focus to Visitor Name
                    this.txtVisitorName.Focus();

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing record
                    // Setup controls 
                    this.dtpVisitDate.Enabled = false;
                    this.txtVisitorName.Enabled = false;
                    this.txtVisitorCardNo.Enabled = false;
                    this.txtIDNumber.Enabled = false;                    
                    this.txtEmpNo.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Visible = true;
                    this.btnDelete.Visible = true;
                    this.btnReset.Visible = true;
                    this.btnViewReport.Visible = true;

                    // Swipe buttons
                    this.btnAddSwipe.Enabled = true;
                    this.btnUpdateSwipe.Enabled = false;

                    // Setup panels
                    //this.trSwipeHistoryTimeEntry.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    //this.trSwipeHistoryButton.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    //this.tblGrid.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    //this.trCheckOffense.Style[HtmlTextWriterStyle.Display] = "none";
                    break;
                    #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing record (read-only)
                    // Setup controls 
                    this.dtpVisitDate.Enabled = false;
                    this.txtVisitorName.Enabled = false;
                    this.txtVisitorCardNo.Enabled = false;
                    this.txtIDNumber.Enabled = false;
                    this.txtEmpNo.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnCheckVisit.Visible = false;
                    this.btnSave.Visible = false;
                    this.btnDelete.Visible = false;
                    this.btnReset.Visible = false;
                    this.btnViewReport.Visible = true;

                    // Swipe buttons
                    this.btnAddSwipe.Enabled = true;
                    this.btnUpdateSwipe.Enabled = false;

                    // Setup panels
                    //this.trSwipeHistoryTimeEntry.Style[HtmlTextWriterStyle.Display] = "none";
                    //this.trSwipeHistoryButton.Style[HtmlTextWriterStyle.Display] = "none";
                    //this.tblGrid.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    //this.trCheckOffense.Style[HtmlTextWriterStyle.Display] = "none";
                    break;
                    #endregion
            }
        }
        #endregion

        #region Action Buttons
        protected void btnGet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Initialize control, variables and perform data validation                
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                this.litSupervisor.Text = "Not defined";
                this.litCCManager.Text = "Not defined";
                this.litExtNo.Text = "Not defined";

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoPersonToVisitEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoPersonToVisitEmpNo;
                    this.cusValEmpNo.Validate();
                    return;
                }
                
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }
                #endregion

                #region Get employee information
                string error = string.Empty;
                string innerError = string.Empty;

                if (dataProxy == null)
                    return;

                var rawData = dataProxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                if (rawData != null)
                {
                    this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                    this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                    this.litExtNo.Text = UIHelper.ConvertObjectToString(rawData.PhoneExtension);
                    this.litCostCenter.Text = string.Format("{0} - {1}", rawData.CostCenter, rawData.CostCenterName);
                    this.litSupervisor.Text = rawData.SupervisorFullName;
                    this.litCCManager.Text = rawData.ManagerFullName;

                    // Save to session
                    this.VisitEmployeeNo = empNo;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_VISITOR_PASS_ENTRY
            ),
            false);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                UIHelper.SaveType saveType = this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord
                    ? UIHelper.SaveType.Insert
                    : UIHelper.SaveType.Update;

                if (saveType == UIHelper.SaveType.Insert)
                {
                    #region Perform Insert Operation

                    #region Perform data validation
                    // Check if visitor is blocked
                    if (this.chkBlockVisitor.Checked)
                    {
                        if (this.txtRemarks.Text == string.Empty)
                        {
                            this.txtGeneric.Text = ValidationErrorType.NoRemarks.ToString();
                            this.ErrorType = ValidationErrorType.NoRemarks;
                            this.cusValRemarks.Validate();
                            errorCount++;
                        }
                    }

                    // Check Visit Date
                    if (this.dtpVisitDate.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoVisitDate.ToString();
                        this.ErrorType = ValidationErrorType.NoVisitDate;
                        this.cusValVisitDate.Validate();
                        errorCount++;
                    }

                    // Check Visitor Name
                    if (this.txtVisitorName.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoVisitorName.ToString();
                        this.ErrorType = ValidationErrorType.NoVisitorName;
                        this.cusValVisitorName.Validate();
                        errorCount++;
                    }

                    // Check ID Number
                    if (this.txtIDNumber.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoIDNumber.ToString();
                        this.ErrorType = ValidationErrorType.NoIDNumber;
                        this.cusValIDNumber.Validate();
                        errorCount++;
                    }

                    // Check Visitor Card No.
                    if (this.txtVisitorCardNo.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoVisitorCardNo.ToString();
                        this.ErrorType = ValidationErrorType.NoVisitorCardNo;
                        this.cusValVisitorCardNo.Validate();
                        errorCount++;
                    }

                    // Check Person to Visit Emp. No.
                    //if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                    if (this.VisitEmployeeNo == 0)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoPersonToVisitEmpNo.ToString();
                        this.ErrorType = ValidationErrorType.NoPersonToVisitEmpNo;
                        this.cusValEmpNo.Validate();
                        errorCount++;
                    }

                    #region Check if the specified person is an employee
                    int empNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                    if (empNo.ToString().Length == 4)
                    {
                        empNo += 10000000;

                        // Display the formatted Emp. No.
                        this.txtVisitorCardNo.Text = empNo.ToString();
                    }

                    if (empNo >= 10000000)
                    {
                        this.txtGeneric.Text = ValidationErrorType.SpecifiedIDNotVisitor.ToString();
                        this.ErrorType = ValidationErrorType.SpecifiedIDNotVisitor;
                        this.cusValVisitorCardNo.Validate();
                        return;
                    }
                    #endregion

                    #endregion

                    if (errorCount > 0)
                    {
                        // Set focus to the top panel
                        Page.SetFocus(this.lnkMoveUp.ClientID);
                    }
                    else
                    {
                        // Initialize flag
                        this.IsBlockedVisitor = false;

                        if (!this.chkBlockVisitor.Checked)
                        {
                            // Check if visitor has been blocked by Security before
                            this.btnCheckVisit_Click(this.btnCheckVisit, new EventArgs());
                        }

                        if (!this.IsBlockedVisitor)
                        {
                            #region Build the Visitor Pass entity
                            VisitorPassEntity visitorPassData = new VisitorPassEntity()
                            {
                                VisitorName = this.txtVisitorName.Text.Trim(),
                                IDNumber = this.txtIDNumber.Text.Trim(),
                                VisitorCardNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text),
                                VisitEmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text),
                                VisitDate = this.dtpVisitDate.SelectedDate,
                                VisitTimeIn = this.dtpSwipeTime.SelectedDate,
                                VisitTimeOut = this.dtpTimeOut.SelectedDate,
                                Remarks = this.txtRemarks.Text.Trim(),
                                IsBlock = this.chkBlockVisitor.Checked,
                                CreatedDate = DateTime.Now,
                                CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                                CreatedByEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                                CreatedByEmpEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]),
                                CreatedByUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])
                            };

                            // Save the swipe logs
                            visitorPassData.VisitorSwipeList = this.SwipeDataList;
                            #endregion

                            // Save to database
                            SaveChanges(saveType, visitorPassData);
                        }
                    }
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Update)
                {
                    #region Perform Update Operation
                    if (this.CurrentVisitorPass == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoRecordToUpdate.ToString();
                        this.ErrorType = ValidationErrorType.NoRecordToUpdate;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    else
                    {
                        // Check if visitor is blocked
                        if (this.chkBlockVisitor.Checked)
                        {
                            if (this.txtRemarks.Text == string.Empty)
                            {
                                this.txtGeneric.Text = ValidationErrorType.NoRemarks.ToString();
                                this.ErrorType = ValidationErrorType.NoRemarks;
                                this.cusValRemarks.Validate();
                                errorCount++;
                            }
                        }
                    }

                    if (errorCount > 0)
                    {
                        // Set focus to the top panel
                        Page.SetFocus(this.lnkMoveUp.ClientID);
                    }
                    else
                    {
                        #region Update the Visitor Pass entity
                        this.CurrentVisitorPass.VisitorName = this.txtVisitorName.Text.Trim();
                        this.CurrentVisitorPass.IDNumber = this.txtIDNumber.Text.Trim();
                        this.CurrentVisitorPass.VisitorCardNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                        this.CurrentVisitorPass.VisitEmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                        this.CurrentVisitorPass.VisitDate = this.dtpVisitDate.SelectedDate;
                        this.CurrentVisitorPass.VisitTimeIn = this.dtpSwipeTime.SelectedDate;
                        this.CurrentVisitorPass.VisitTimeOut = this.dtpTimeOut.SelectedDate;
                        this.CurrentVisitorPass.Remarks = this.txtRemarks.Text.Trim();
                        this.CurrentVisitorPass.IsBlock = this.chkBlockVisitor.Checked;
                        this.CurrentVisitorPass.LastUpdateTime = DateTime.Now;
                        this.CurrentVisitorPass.LastUpdateEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        this.CurrentVisitorPass.LastUpdateEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                        this.CurrentVisitorPass.LastUpdateEmpEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]);
                        this.CurrentVisitorPass.LastUpdateUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);

                        // Save the swipe logs
                        this.CurrentVisitorPass.VisitorSwipeList = this.SwipeDataList;
                        #endregion

                        // Save to database
                        SaveChanges(saveType, this.CurrentVisitorPass);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());                
            }
        }

        protected void btnSave2_Click(object sender, EventArgs e)
        {
            UIHelper.SaveType saveType = this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord
                   ? UIHelper.SaveType.Insert
                   : UIHelper.SaveType.Update;

            #region Build the Visitor Pass entity
            VisitorPassEntity visitorPassData = new VisitorPassEntity()
            {
                VisitorName = this.txtVisitorName.Text.Trim(),
                IDNumber = this.txtIDNumber.Text.Trim(),
                VisitorCardNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text),
                VisitEmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text),
                VisitDate = this.dtpVisitDate.SelectedDate,
                VisitTimeIn = this.dtpSwipeTime.SelectedDate,
                VisitTimeOut = this.dtpTimeOut.SelectedDate,
                Remarks = this.txtRemarks.Text.Trim(),
                IsBlock = this.chkBlockVisitor.Checked,
                CreatedDate = DateTime.Now,
                CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                CreatedByEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                CreatedByEmpEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]),
                CreatedByUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])
            };

            // Save the swipe logs
            visitorPassData.VisitorSwipeList = this.SwipeDataList;
            #endregion

            // Save to database
            SaveChanges(saveType, visitorPassData);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            // Check if there is selected record to delete
            if (this.CurrentVisitorPass == null)
            {
                this.txtGeneric.Text = ValidationErrorType.NoRecordToDelete.ToString();
                this.ErrorType = ValidationErrorType.NoRecordToDelete;
                this.cusValButton.Validate();
                return;
            }
            #endregion

            StringBuilder script = new StringBuilder();
            script.Append("ConfirmRecordDeletion('");
            script.Append(string.Concat(this.btnDeleteDummy.ClientID, "','"));
            script.Append(string.Concat(this.btnReset.ClientID, "','"));
            script.Append(UIHelper.CONST_DELETE_SINGLE_RECORD_CONFIRMATION + "');");

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Deletion Confirmation", script.ToString(), true);
        }

        protected void btnDeleteDummy_Click(object sender, EventArgs e)
        {
            try
            {
                #region Delete database record
                string error = string.Empty;
                string innerError = string.Empty;

                // Get WCF Instance
                if (dataProxy == null)
                    return;

                dataProxy.InsertUpdateDeleteVisitorPassLog(Convert.ToInt32(UIHelper.SaveType.Delete), this.CurrentVisitorPass, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Employee Training Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_VISITOR_PASS_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                        true.ToString()
                    ),
                    false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset controls
            // Visitor Information section
            this.dtpVisitDate.SelectedDate = null;
            this.txtVisitorName.Text = string.Empty;
            this.txtIDNumber.Text = string.Empty;
            this.txtVisitorCardNo.Text = string.Empty;

            // Person to Visit Information section
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.litSupervisor.Text = "Not defined";
            this.litCCManager.Text = "Not defined";
            this.litExtNo.Text = "Not defined";

            // Log Information section
            this.txtSwipeID.Text = string.Empty;
            this.dtpSwipeDate.SelectedDate = null;
            this.dtpSwipeTime.SelectedDate = null;
            this.cboSwipeType.SelectedIndex = -1;
            this.cboSwipeType.Text = string.Empty;
            this.txtRemarks.Text = string.Empty;
            this.chkBlockVisitor.Checked = false;

            //this.dtpDateOut.SelectedDate = null;
            //this.dtpTimeOut.Text = string.Empty;
            //this.chkSwipeIn.Checked = false;
            //this.chkSwipeIn_CheckedChanged(this.chkSwipeIn, new EventArgs());
            //this.chkSwipeOut.Checked = false;
            //this.chkSwipeOut_CheckedChanged(this.chkSwipeOut, new EventArgs());

            // Reset the grid
            this.gridSwipeHistory.VirtualItemCount = 1;
            this.gridSwipeHistory.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSwipeHistory.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSwipeHistory.PageSize;

            InitializeDataToGrid();
            #endregion

            KillSessions();
            InitializeControls(UIHelper.DataLoadTypes.CreateNewRecord);
        }

        protected void btnViewReport_Click(object sender, EventArgs e)
        {
            #region Perform Validation
            // Check if there is training record currently loaded
            if (this.CurrentVisitorPass == null)
            {
                this.txtGeneric.Text = ValidationErrorType.NoRecordToPrint.ToString();
                this.ErrorType = ValidationErrorType.NoRecordToPrint;
                this.cusValButton.Validate();
                return;
            }
            #endregion

            // Save session state
            StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

            // Store collection to session
            Session["VisitorLogReportSource"] = this.CurrentVisitorPass;

            // Show the report
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                UIHelper.ReportTypes.VisitorLogReport.ToString(),
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_VISITOR_PASS_ENTRY
            ),
            false);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerForm))
            {
                Response.Redirect
                (
                    String.Format(this.CallerForm + "?{0}={1}",
                        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,
                        true.ToString()
                ),
                false);
            }
            else
            {
                Response.Redirect
               (
                   String.Format(UIHelper.PAGE_VISITOR_PASS_INQUIRY + "?{0}={1}",
                       UIHelper.QUERY_STRING_RELOAD_DATA_KEY,
                       true.ToString()
               ),
               false);
            }
        }

        protected void btnCheckVisit_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            int errorCount = 0;

            string visitorName = string.Empty; //this.txtVisitorName.Text.Trim();
            string idNumber = this.txtIDNumber.Text.Trim();
            int visitorCardNo = 0; //this.txtVisitorCardNo.Text.Trim();

            // Check Visitor Name
            //if (string.IsNullOrEmpty(visitorName))
            //{
            //    this.txtGeneric.Text = ValidationErrorType.NoVisitorName.ToString();
            //    this.ErrorType = ValidationErrorType.NoVisitorName;
            //    this.cusValVisitorName.Validate();
            //    errorCount++;
            //}

            // Check ID Number
            if (string.IsNullOrEmpty(idNumber))
            {
                this.txtGeneric.Text = ValidationErrorType.NoIDNumber.ToString();
                this.ErrorType = ValidationErrorType.NoIDNumber;
                this.cusValIDNumber.Validate();
                errorCount++;
            }

            // Check Visitor Card No.
            //if (string.IsNullOrEmpty(visitorCardNo))
            //{
            //    this.txtGeneric.Text = ValidationErrorType.NoVisitorCardNo.ToString();
            //    this.ErrorType = ValidationErrorType.NoVisitorCardNo;
            //    this.cusValVisitorCardNo.Validate();
            //    errorCount++;
            //}

            if (errorCount > 0)
            {
                this.IsBlockedVisitor = true;
                return;
            }
            #endregion

            // Load the data
            GetVisitorPassData(0, visitorName, idNumber, visitorCardNo, 0, string.Empty, null, null, true);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            // Set the session flag
            this.CurrentFormLoadType = UIHelper.DataLoadTypes.EditExistingRecord;

            InitializeControls(this.CurrentFormLoadType);

            #region Bind data to controls
            if (this.CurrentVisitorPass != null)
            {
                // Visitor Information section
                this.txtVisitorName.Text = this.CurrentVisitorPass.VisitorName;
                this.txtIDNumber.Text = this.CurrentVisitorPass.IDNumber;
                this.txtVisitorCardNo.Value = this.CurrentVisitorPass.VisitorCardNo;

                // Person to Visit Information section
                this.txtEmpNo.Text = this.CurrentVisitorPass.VisitEmpNo > 0 ? this.CurrentVisitorPass.VisitEmpNo.ToString() : string.Empty;
                this.litEmpName.Text = this.CurrentVisitorPass.VisitEmpName;
                this.litPosition.Text = this.CurrentVisitorPass.VisitEmpPosition;
                this.litCostCenter.Text = this.CurrentVisitorPass.VisitEmpFullCostCenter;
                this.litSupervisor.Text = this.CurrentVisitorPass.VisitEmpSupervisorFullName;
                this.litCCManager.Text = this.CurrentVisitorPass.VisitEmpManagerFullName;
                this.litExtNo.Text = this.CurrentVisitorPass.VisitEmpExtension;

                // Log Information section
                this.dtpSwipeTime.SelectedDate = this.CurrentVisitorPass.VisitTimeIn;
                this.dtpTimeOut.SelectedDate = this.CurrentVisitorPass.VisitTimeOut;
                this.txtRemarks.Text = this.CurrentVisitorPass.Remarks;
                this.chkBlockVisitor.Checked = UIHelper.ConvertObjectToBolean(this.CurrentVisitorPass.IsBlock);

                // Swipes History section
                GetVisitorSwipeHistory(true);
            }
            #endregion
        }

        protected void btnRemoveGridItem_Click(object sender, EventArgs e)
        {
            if (this.SwipeDataList.Count > 0 &&
                this.SelectedSwipeRecord != null)
            {
                VisitorSwipeEntity itemToRemove = this.SwipeDataList
                    .Where(a => a.SwipeID == this.SelectedSwipeRecord.SwipeID)
                    .FirstOrDefault();
                if (itemToRemove != null)
                {
                    this.SwipeDataList.Remove(itemToRemove);

                    // Refresh the grid
                    this.btnResetSwipe_Click(this.btnResetSwipe, new EventArgs());
                }
            }
        }

        protected void btnDeleteSwipe_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedSwipeDataList.Clear();

            #region Loop through each record in the grid
            GridDataItemCollection gridData = this.gridSwipeHistory.MasterTableView.Items;
            if (gridData.Count > 0)
            {
                foreach (GridDataItem item in gridData)
                {
                    System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    int swipeID = UIHelper.ConvertObjectToInt(this.gridSwipeHistory.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("SwipeID"));

                    if (chkSelectColumn != null)
                    {
                        if (chkSelectColumn.Checked)
                        {
                            if (this.SwipeDataList.Count > 0 && swipeID > 0)
                            {
                                VisitorSwipeEntity selectedRecord = this.SwipeDataList
                                    .Where(a => a.SwipeID == swipeID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedSwipeDataList.Count == 0)
                                    {
                                        this.CheckedSwipeDataList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedSwipeDataList.Count > 0 &&
                                        this.CheckedSwipeDataList.Where(a => a.SwipeID == selectedRecord.SwipeID).FirstOrDefault() == null)
                                    {
                                        this.CheckedSwipeDataList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record exist in the selected item collection
                            if (swipeID > 0)
                            {
                                VisitorSwipeEntity selectedRecord = this.SwipeDataList
                                    .Where(a => a.SwipeID == swipeID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedSwipeDataList.Count > 0
                                        && this.CheckedSwipeDataList.Where(a => a.SwipeID == selectedRecord.SwipeID).FirstOrDefault() != null)
                                    {
                                        VisitorSwipeEntity itemToDelete = this.CheckedSwipeDataList
                                            .Where(a => a.SwipeID == selectedRecord.SwipeID)
                                            .FirstOrDefault();
                                        if (itemToDelete != null)
                                        {
                                            this.CheckedSwipeDataList.Remove(itemToDelete);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion

            #region Display confirmation message
            // Check for selected swipe records to submit for approval
            if (this.CheckedSwipeDataList.Count == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoRecordToDelete.ToString();
                this.ErrorType = ValidationErrorType.NoRecordToDelete;
                this.cusValButton.Validate();

                // Refresh the grid
                RebindDataToGrid();
            }
            else
            {
                StringBuilder script = new StringBuilder();
                script.Append("ConfirmButtonAction('");
                script.Append(string.Concat(this.btnDeleteDummy.ClientID, "','"));
                script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Delete Confirmation", script.ToString(), true);
            }
            #endregion
        }

        protected void btnDeleteSwipeDummy_Click(object sender, EventArgs e)
        {
            if (this.CheckedSwipeDataList == null ||
                this.CheckedSwipeDataList.Count == 0)
                return;

            if (DeleteSwipeRecord(this.CheckedSwipeDataList))
            {
                // Refresh data in the grid
                GetVisitorSwipeHistory(true);
            }
        }

        protected void btnResetSwipe_Click(object sender, EventArgs e)
        {
            this.txtSwipeID.Text = string.Empty;
            this.dtpSwipeDate.SelectedDate = this.dtpVisitDate.SelectedDate;
            this.dtpSwipeTime.SelectedDate = null;
            this.cboSwipeType.SelectedIndex = -1;
            this.cboSwipeType.Text = string.Empty;

            this.dtpDateOut.SelectedDate = null;
            this.dtpTimeOut.SelectedDate = null;

            // Reset buttons
            this.btnAddSwipe.Enabled = true;
            this.btnUpdateSwipe.Enabled = false;

            // Reset session
            this.SelectedSwipeRecord = null;

            RebindDataToGrid();
        }

        protected void btnAddSwipe_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Perform Data Validation

                #region Check Visitor Card No.
                //int empNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                //if (empNo == 0)
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoVisitorCardNo.ToString();
                //    this.ErrorType = ValidationErrorType.NoVisitorCardNo;
                //    this.cusValVisitorCardNo.Validate();
                //    errorCount++;
                //}
                #endregion

                #region Check Visit Date
                if (this.dtpVisitDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoVisitDate.ToString();
                    this.ErrorType = ValidationErrorType.NoVisitDate;
                    this.cusValVisitDate.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Swipe Type
                if (string.IsNullOrEmpty(this.cboSwipeType.Text.Trim()))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeType.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeType;
                    this.cusValSwipeType.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Swipe Date
                if (this.dtpSwipeDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeDate.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeDate;
                    this.cusValSwipeDate.Validate();
                    errorCount++;
                }
                else
                {
                    if (this.cboSwipeType.SelectedValue == "valOUT")
                    {
                        if (this.dtpVisitDate.SelectedDate.HasValue &&
                            this.dtpSwipeDate.SelectedDate < this.dtpVisitDate.SelectedDate)
                        {
                            this.txtGeneric.Text = ValidationErrorType.SwipeDateLessThanVisitDate.ToString();
                            this.ErrorType = ValidationErrorType.SwipeDateLessThanVisitDate;
                            this.cusValSwipeDate.Validate();
                            errorCount++;
                        }
                    }

                    if (this.dtpVisitDate.SelectedDate.HasValue && 
                        Math.Abs((Convert.ToDateTime(this.dtpVisitDate.SelectedDate) - Convert.ToDateTime(this.dtpSwipeDate.SelectedDate)).TotalDays) > 1)
                    {
                        this.txtGeneric.Text = ValidationErrorType.DateDifferenceExceedLimit.ToString();
                        this.ErrorType = ValidationErrorType.DateDifferenceExceedLimit;
                        this.cusValSwipeDate.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Check Swipe Time
                if (this.dtpSwipeTime.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeTime.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeTime;
                    this.cusValSwipeTime.Validate();
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

                #region Add new swipe record
                VisitorSwipeEntity newSwipeRecord = new VisitorSwipeEntity()
                {
                    LogID = this.CurrentVisitorPass != null ? this.CurrentVisitorPass.LogID : 0,
                    SwipeDate = this.dtpSwipeDate.SelectedDate,
                    SwipeTime = this.dtpSwipeDate.SelectedDate.Value.Date
                        .AddHours(this.dtpSwipeTime.SelectedDate.Value.Hour)
                        .AddMinutes(this.dtpSwipeTime.SelectedDate.Value.Minute)
                        .AddSeconds(this.dtpSwipeTime.SelectedDate.Value.Second),
                    SwipeTypeCode = this.cboSwipeType.SelectedValue,
                    SwipeTypeDesc = this.cboSwipeType.Text,
                    CreatedDate = DateTime.Now,
                    CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                    CreatedByEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                    CreatedByUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]),
                    CreatedByEmpEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]),
                    TotalRecords = this.SwipeDataList.Count == 0 ? 1 : this.SwipeDataList.Count + 1,
                    SwipeLocation = "Manual Swipe",
                    SwipeCode = UIHelper.SwipeCode.MANUAL.ToString()
                };

                // Add item to the collection
                this.SwipeDataList.Add(newSwipeRecord);
                #endregion

                // Clear controls
                this.btnResetSwipe_Click(this.btnResetSwipe, new EventArgs());

                // Refresh data in the grid
                //RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnUpdateSwipe_Click(object sender, EventArgs e)
        {
            int errorCount = 0;

            try
            {
                #region Perform Data Validation

                #region Check Visitor Card No.
                int empNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                if (empNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoVisitorCardNo.ToString();
                    this.ErrorType = ValidationErrorType.NoVisitorCardNo;
                    this.cusValVisitorCardNo.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Swipe Date
                if (this.dtpSwipeDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeDate.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeDate;
                    this.cusValSwipeDate.Validate();
                    errorCount++;
                }
                else
                {
                    if (this.cboSwipeType.SelectedValue == "valOUT")
                    {
                        if (this.dtpVisitDate.SelectedDate.HasValue &&
                            this.dtpSwipeDate.SelectedDate < this.dtpVisitDate.SelectedDate)
                        {
                            this.txtGeneric.Text = ValidationErrorType.SwipeDateLessThanVisitDate.ToString();
                            this.ErrorType = ValidationErrorType.SwipeDateLessThanVisitDate;
                            this.cusValSwipeDate.Validate();
                            errorCount++;
                        }
                    }

                    if (this.dtpVisitDate.SelectedDate.HasValue &&
                        Math.Abs((Convert.ToDateTime(this.dtpVisitDate.SelectedDate) - Convert.ToDateTime(this.dtpSwipeDate.SelectedDate)).TotalDays) > 1)
                    {
                        this.txtGeneric.Text = ValidationErrorType.DateDifferenceExceedLimit.ToString();
                        this.ErrorType = ValidationErrorType.DateDifferenceExceedLimit;
                        this.cusValSwipeDate.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Check Swipe Time
                if (this.dtpSwipeTime.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeTime.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeTime;
                    this.cusValSwipeDate.Validate();
                    errorCount++;
                }
                #endregion

                #region Check Swipe Type
                if (string.IsNullOrEmpty(this.cboSwipeType.SelectedValue))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeType.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeType;
                    this.cusValSwipeType.Validate();
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

                #region Update matching record in the grid's data collection
                VisitorSwipeEntity recordToUpdate = this.SwipeDataList
                    .Where(a => a.SwipeDate == this.SelectedSwipeRecord.SwipeDate
                        && a.SwipeTime == this.SelectedSwipeRecord.SwipeTime
                        && UIHelper.ConvertObjectToString(a.SwipeTypeCode) == UIHelper.ConvertObjectToString(this.SelectedSwipeRecord.SwipeTypeCode))
                    .FirstOrDefault();
                if (recordToUpdate != null)
                {
                    recordToUpdate.SwipeDate = this.dtpSwipeDate.SelectedDate;
                    recordToUpdate.SwipeTime = this.dtpSwipeDate.SelectedDate.Value.Date
                       .AddHours(this.dtpSwipeTime.SelectedDate.Value.Hour)
                       .AddMinutes(this.dtpSwipeTime.SelectedDate.Value.Minute)
                       .AddSeconds(this.dtpSwipeTime.SelectedDate.Value.Second);
                    recordToUpdate.SwipeTypeCode = this.cboSwipeType.SelectedValue;
                    recordToUpdate.SwipeTypeDesc = this.cboSwipeType.Text;
                    recordToUpdate.LastUpdateTime = DateTime.Now;
                    recordToUpdate.LastUpdateEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                    recordToUpdate.LastUpdateEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                    recordToUpdate.LastUpdateUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                    recordToUpdate.LastUpdateEmpEmail = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]);
                }
                #endregion

                // Refresh the grid
                RebindDataToGrid();

                // Clear controls
                this.btnResetSwipe_Click(this.btnResetSwipe, new EventArgs());
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
                else if (this.ErrorType == ValidationErrorType.NoVisitDate)
                {
                    validator.ErrorMessage = "Visitor Date is required.";
                    validator.ToolTip = "Visitor Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoVisitorName)
                {
                    validator.ErrorMessage = "Visitor Name is required.";
                    validator.ToolTip = "Visitor Name is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoIDNumber)
                {
                    validator.ErrorMessage = "ID Number is required.";
                    validator.ToolTip = "ID Number is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoVisitorCardNo)
                {
                    validator.ErrorMessage = "GARMCO Visitor Card No. is required.";
                    validator.ToolTip = "GARMCO Visitor Card No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoPersonToVisitEmpNo)
                {
                    validator.ErrorMessage = "Person to visit employee no. is required. Make sure to enter the employee no. then click the Get button.";
                    validator.ToolTip = "Person to visit employee no. is required. Make sure to enter the employee no. then click the Get button.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidTimeIn)
                {
                    validator.ErrorMessage = "The specified Time In is invalid.";
                    validator.ToolTip = "The specified Time In is invalid.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidTimeOut)
                {
                    validator.ErrorMessage = "The specified Time Out is invalid.";
                    validator.ToolTip = "The specified Time Out is invalid.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToUpdate)
                {
                    validator.ErrorMessage = "Unable to perform update operation because no record has been opened.";
                    validator.ToolTip = "Unable to perform update operation because no record has been opened.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Unable to perform delete operation because no record has been opened.";
                    validator.ToolTip = "Unable to perform delete operation because no record has been opened.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRemarks)
                {
                    validator.ErrorMessage = "Remarks must be supplied if visitor needs to be blocked.";
                    validator.ToolTip = "Remarks must be supplied if visitor needs to be blocked.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateIn)
                {
                    validator.ErrorMessage = "Date In is required.";
                    validator.ToolTip = "Date In is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateOut)
                {
                    validator.ErrorMessage = "Date In is required.";
                    validator.ToolTip = "Date In is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeIn)
                {
                    validator.ErrorMessage = "Time In is required if Date In is specified.";
                    validator.ToolTip = "Time In is required if Date In is specified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeOut)
                {
                    validator.ErrorMessage = "Time Out is required if Date Out is specified.";
                    validator.ToolTip = "Time Out is required if Date Out is specified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeInAndOut)
                {
                    validator.ErrorMessage = "Both Swipe In and Swipe Out dates cannot be null.";
                    validator.ToolTip = "Both Swipe In and Swipe Out dates cannot be null.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "Date In should be less than Date Out.";
                    validator.ToolTip = "Date In should be less than Date Out.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.DateDifferenceExceedLimit)
                {
                    validator.ErrorMessage = "The difference between Visit Date and Swipe date should not exceed 1 day.";
                    validator.ToolTip = "The difference between Visit Date and Swipe date should not exceed 1 day.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidSwipeDate)
                {
                    validator.ErrorMessage = "The specified Swipe Date is invalid. It should not be greater than the Visit Date.";
                    validator.ToolTip = "The specified Swipe Date is invalid. It should not be greater than the Visit Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidSwipeTime)
                {
                    validator.ErrorMessage = "Invalid swipe time. The swipe-in time should be less than the swipe-out time.";
                    validator.ToolTip = "Invalid swipe time. The swipe-in time should be less than the swipe-out time.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSwipeDate)
                {
                    validator.ErrorMessage = "Swipe Date is required.";
                    validator.ToolTip = "Swipe Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSwipeTime)
                {
                    validator.ErrorMessage = "Swipe Time is required.";
                    validator.ToolTip = "Swipe Time is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSwipeType)
                {
                    validator.ErrorMessage = "Swipe Type is required.";
                    validator.ToolTip = "Swipe Type is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.SwipeDateLessThanVisitDate)
                {
                    validator.ErrorMessage = "Swipe Date should not be less than Visit Date if swipe type is OUT.";
                    validator.ToolTip = "Swipe Date should not be less than Visit Date if swipe type is OUT.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToPrint)
                {
                    validator.ErrorMessage = "Unable to view the report because there is no visit log record currently opened.";
                    validator.ToolTip = "Unable to show the report because there is no visit log record currently opened.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.SpecifiedIDNotVisitor)
                {
                    validator.ErrorMessage = "The specified ID refers to an employee. Please enter a valid visitor card number!";
                    validator.ToolTip = "The specified ID refers to an employee. Please enter a valid visitor card number!";
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

        protected void dtpVisitDate_SelectedDateChanged(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
        {
            this.dtpSwipeDate.SelectedDate = this.dtpVisitDate.SelectedDate;
            this.dtpDateOut.SelectedDate = this.dtpVisitDate.SelectedDate;
        }

        protected void chkSwipeIn_CheckedChanged(object sender, EventArgs e)
        {
            this.dtpSwipeDate.Enabled = this.dtpSwipeTime.Enabled = this.chkSwipeIn.Checked;

            if (!this.dtpSwipeDate.Enabled)
                this.dtpSwipeDate.SelectedDate = null;
            else
                this.dtpSwipeDate.Focus();

            if (!this.dtpSwipeTime.Enabled)
                this.dtpSwipeTime.SelectedDate = null;
        }

        protected void chkSwipeOut_CheckedChanged(object sender, EventArgs e)
        {
            this.dtpDateOut.Enabled = this.dtpTimeOut.Enabled = this.chkSwipeOut.Checked;

            if (!this.dtpDateOut.Enabled)
                this.dtpDateOut.SelectedDate = null;
            else
            {
                if (this.dtpSwipeDate.SelectedDate != null)
                    this.dtpDateOut.SelectedDate = this.dtpSwipeDate.SelectedDate;

                this.dtpDateOut.Focus();
            }

            if (!this.dtpTimeOut.Enabled)
                this.dtpTimeOut.SelectedDate = null;
        }

        protected void txtEmpNo_TextChanged(object sender, EventArgs e)
        {
            this.btnGet_Click(this.btnGet, new EventArgs());
        }
        #endregion

        #region Database Access
        private void GetVisitorPassData(long logID = 0, string visitorName = "", string idNumber = "", int visitorCardNo = 0, int visitEmpNo = 0, string vistiCostCenter = "",
            DateTime? startDate = null, DateTime? endDate = null, bool? isBlock = null, int createdByEmpNo = 0)
        {
            try
            {
                #region Get visitor pass record from the database
                if (Session["SelectedVisitorPass"] != null)
                {
                    this.CurrentVisitorPass = Session["SelectedVisitorPass"] as VisitorPassEntity;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;
                    
                    byte blockOption = 0;
                    if (isBlock.HasValue && Convert.ToBoolean(isBlock) == true)
                        blockOption = 1;

                    var rawData = dataProxy.GetVisitorPassLog(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, vistiCostCenter, startDate, endDate, blockOption, createdByEmpNo, ref error, ref innerError);
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
                            this.CurrentVisitorPass = rawData.FirstOrDefault();
                        }
                    }
                }
                #endregion

                if (this.CurrentVisitorPass != null)
                {
                    if (isBlock == true)
                    {
                        // Set the flag
                        this.IsBlockedVisitor = true;

                        #region Show user about the visitor's previous offense
                        StringBuilder script = new StringBuilder();
                        string confirmationMsg = string.Format(@"The specified visitor with ID Number {0} has been blocked by Security Department on {1}. Do you want to proceed saving the data or view the previous visit record? \n\n(Note: Please click Ok to save the data, otherwise click Cancel to view the previous record.)",
                            this.CurrentVisitorPass.IDNumber,
                            Convert.ToDateTime(this.CurrentVisitorPass.VisitDate).ToString("dd-MMM-yyyy"));

                        script.Append("ConfirmButtonAction('");
                        script.Append(string.Concat(this.btnSave2.ClientID, "','"));
                        script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                        script.Append(confirmationMsg + "');");
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Blocked User Information", script.ToString(), true);
                        #endregion
                    }
                    else
                    {
                        #region Bind data to controls
                        // Visitor Information section
                        this.dtpVisitDate.SelectedDate = this.CurrentVisitorPass.VisitDate;
                        this.txtVisitorName.Text = this.CurrentVisitorPass.VisitorName;
                        this.txtIDNumber.Text = this.CurrentVisitorPass.IDNumber;
                        this.txtVisitorCardNo.Value = this.CurrentVisitorPass.VisitorCardNo;

                        // Person to Visit Information section
                        this.txtEmpNo.Text = this.CurrentVisitorPass.VisitEmpNo > 0 ? this.CurrentVisitorPass.VisitEmpNo.ToString() : string.Empty;
                        this.litEmpName.Text = this.CurrentVisitorPass.VisitEmpName;
                        this.litPosition.Text = this.CurrentVisitorPass.VisitEmpPosition;
                        this.litCostCenter.Text = this.CurrentVisitorPass.VisitEmpFullCostCenter;
                        this.litSupervisor.Text = this.CurrentVisitorPass.VisitEmpSupervisorFullName;
                        this.litCCManager.Text = this.CurrentVisitorPass.VisitEmpManagerFullName;
                        this.litExtNo.Text = this.CurrentVisitorPass.VisitEmpExtension;

                        // Swipes History section
                        GetVisitorSwipeHistory(true);

                        // Other Details section
                        this.txtRemarks.Text = this.CurrentVisitorPass.Remarks;
                        this.chkBlockVisitor.Checked = UIHelper.ConvertObjectToBolean(this.CurrentVisitorPass.IsBlock);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, VisitorPassEntity visitorPassData)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                // Get WCF Instance
                if (dataProxy == null)
                    return;

                dataProxy.InsertUpdateDeleteVisitorPassLog(Convert.ToInt32(saveType), visitorPassData, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Employee Training Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_VISITOR_PASS_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                        true.ToString()
                    ),
                    false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.CurrentVisitorPass = null;
                throw new Exception(ex.Message.ToString());
            }
        }

        private void GetVisitorSwipeHistory(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                int empNo = UIHelper.ConvertObjectToInt(this.CurrentVisitorPass.VisitorCardNo);
                DateTime? startDate = this.CurrentVisitorPass.VisitDate;
                DateTime? endDate = this.CurrentVisitorPass.VisitDate;

                //if (this.CurrentVisitorPass.VisitDate.HasValue)
                //    endDate = this.CurrentVisitorPass.VisitDate.Value.AddDays(1);

                // Initialize grid
                this.gridSwipeHistory.VirtualItemCount = 1;

                // Clear collection
                this.SwipeDataList.Clear();
                this.CheckedSwipeDataList.Clear();
                #endregion

                if (empNo > 0)
                {
                    #region Fill data to the collection
                    List<VisitorSwipeEntity> gridSource = new List<VisitorSwipeEntity>();
                    if (!reloadDataFromDB)
                    {
                        gridSource = this.SwipeDataList;
                    }
                    else
                    {
                        // Get WCF Instance
                        if (dataProxy == null)
                            return;

                        string error = string.Empty;
                        string innerError = string.Empty;

                        var source = dataProxy.GetVisitorSwipeHistory(empNo, startDate, endDate, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                    this.SwipeDataList = gridSource;
                    #endregion
                }

                #region Bind data to the grid
                if (this.SwipeDataList.Count > 0)
                {
                    int totalRecords = this.SwipeDataList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridSwipeHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridSwipeHistory.VirtualItemCount = 1;

                    this.gridSwipeHistory.DataSource = this.SwipeDataList;
                    this.gridSwipeHistory.DataBind();
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

        private bool DeleteSwipeRecord(List<VisitorSwipeEntity> recordToDeleteList)
        {
            if (dataProxy == null || recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                dataProxy.InsertUpdateDeleteVisitorSwipeLog(Convert.ToInt32(UIHelper.SaveType.Delete), recordToDeleteList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
                return false;
            }
        }

        private bool SaveVisitorSwipe(UIHelper.SaveType saveType, List<VisitorSwipeEntity> dataList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                // Get WCF Instance
                if (dataProxy == null || dataList == null)
                    return false;

                dataProxy.InsertUpdateDeleteVisitorSwipeLog(Convert.ToInt32(saveType), dataList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        #endregion
                
    }
}