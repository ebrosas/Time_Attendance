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

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class VisitorPassInquiry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToDelete,
            NoRecordToPrint,
            NoRecordOnGrid,
            InvalidDateDuration,
            NoSelectedRecordToDelete
        }

        private enum BlockUserOption
        {
            valAll,
            valYes,
            valNo
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

        private Dictionary<string, object> VisitorInquiryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["VisitorInquiryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["VisitorInquiryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["VisitorInquiryStorage"] = value;
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

        private List<VisitorPassEntity> VisitorPassList
        {
            get
            {
                List<VisitorPassEntity> list = ViewState["VisitorPassList"] as List<VisitorPassEntity>;
                if (list == null)
                    ViewState["VisitorPassList"] = list = new List<VisitorPassEntity>();

                return list;
            }
            set
            {
                ViewState["VisitorPassList"] = value;
            }
        }

        private List<VisitorPassEntity> CheckedVisitorList
        {
            get
            {
                List<VisitorPassEntity> list = ViewState["CheckedVisitorList"] as List<VisitorPassEntity>;
                if (list == null)
                    ViewState["CheckedVisitorList"] = list = new List<VisitorPassEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedVisitorList"] = value;
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
                    pageSize = this.gridVisitor.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
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

        private List<VisitorPassEntity> DataList
        {
            get
            {
                List<VisitorPassEntity> list = ViewState["DataList"] as List<VisitorPassEntity>;
                if (list == null)
                    ViewState["DataList"] = list = new List<VisitorPassEntity>();

                return list;
            }
            set
            {
                ViewState["DataList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.VPASSINQ.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_VISITOR_PASS_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (SecurityUserList.Count > 0 &&
                        SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() == null)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_VISITOR_PASS_INQUIRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSearch.Enabled = this.Master.IsRetrieveAllowed;
                this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                this.btnNewRecord.Visible = this.Master.IsCreateAllowed;
                this.btnPrint.Visible = this.Master.IsPrintAllowed;
                //this.btnExportToExcel.Visible = this.Master.IsPrintAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.VisitorInquiryStorage.Count > 0)
                {
                    if (this.VisitorInquiryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        string originatorButton = this.VisitorInquiryStorage.ContainsKey("SourceControl")
                           ? UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["SourceControl"]) : string.Empty;

                        switch (originatorButton)
                        {
                            case "btnFindEmployee":
                                this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                                break;

                            case "btnFindOtherEmp":
                                this.txtOtherEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                                break;
                        }
                    }

                    // Clear data storage
                    this.VisitorInquiryStorage.Clear();
                    #endregion
                }
                else if (formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString() ||
                    formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show last inquiry data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.VisitorInquiryStorage.Clear();

                    // Refresh query string value
                    this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);

                    // Check if need to invoke method to load data in the grid
                    if (this.ReloadGridData)
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    // Begin searching for records
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset Controls
            this.txtVisitorName.Text = string.Empty;
            this.txtIDNumber.Text = string.Empty;
            this.txtVisitorCardNo.Text = string.Empty;
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.dtpVisitStartDate.SelectedDate = null;
            this.dtpVisitEndDate.SelectedDate = null;
            this.rblBlockOption.SelectedValue = BlockUserOption.valAll.ToString();
            this.txtOtherEmpNo.Text = string.Empty;
            this.rblCreatedBy.SelectedValue = Convert.ToInt32(UIHelper.CreatedByOptions.All).ToString();
            #endregion

            // Clear collections
            this.VisitorPassList.Clear();
            this.CheckedVisitorList.Clear();
            this.DataList.Clear();

            // Reset the grid
            this.gridVisitor.VirtualItemCount = 1;
            this.gridVisitor.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridVisitor.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridVisitor.PageSize;

            KillSessions();
            InitializeGrid();
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
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["ReloadGridData"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Grid Events and Methods
        protected void gridVisitor_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetVistorPassLog(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridVisitor_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetVistorPassLog(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridVisitor_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.DataList.Count > 0)
            {
                gridVisitor.DataSource = this.DataList;
                gridVisitor.DataBind();

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
                        sortExpr.SortOrder = gridVisitor.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                gridVisitor.Rebind();
            }
            else
                InitializeGrid();
        }

        protected void gridVisitor_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    if (UIHelper.ConvertObjectToString(e.CommandArgument) == "PrintButton")
                    {
                        #region Print button
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        // Get the data key value
                        long logID = UIHelper.ConvertObjectToLong(this.gridVisitor.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("LogID"));

                        if (this.DataList.Count > 0)
                        {
                            VisitorPassEntity selectedVisitorRecord = this.DataList
                                .Where(a => a.LogID == logID)
                                .FirstOrDefault();

                            if (selectedVisitorRecord != null && logID > 0)
                            {
                                // Save to session
                                Session["VisitorLogReportSource"] = selectedVisitorRecord;
                            }
                        }

                        // Show the report
                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}",
                            UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                            UIHelper.ReportTypes.VisitorLogReport.ToString(),
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_VISITOR_PASS_INQUIRY
                        ),
                        false);
                        #endregion
                    }
                    else
                    {
                        #region Visitior link is clicked
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        // Initialize variables
                        int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);
                        int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                        string formLoadType = string.Empty;
                        if (createdByEmpNo == currentUserEmpNo ||
                            this.Master.IsVisitorPassSystemAdmin)
                        {
                            formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString();
                        }
                        else
                            formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString();

                        // Get data key value
                        long logID = UIHelper.ConvertObjectToLong(this.gridVisitor.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("LogID"));
                        if (this.DataList.Count > 0)
                        {
                            VisitorPassEntity selectedVisitorRecord = this.DataList
                                .Where(a => a.LogID == logID)
                                .FirstOrDefault();

                            if (selectedVisitorRecord != null && logID > 0)
                            {
                                // Save to session
                                Session["SelectedVisitorPassRecord"] = selectedVisitorRecord;
                            }
                        }

                        // Redirect to Employee Training Entry page
                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_VISITOR_PASS_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_VISITOR_PASS_INQUIRY,
                            UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                            logID,
                            UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                            formLoadType
                        ),
                        false);
                        #endregion
                    }
                }
            }
        }

        protected void gridVisitor_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                    int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);

                    #region Enable/disable select checkbox
                    CheckBox chkSelect = item["CheckboxSelectColumn"].Controls[0] as CheckBox;
                    if (chkSelect != null)
                    {
                        // Checks if the current user is the one who created the record
                        chkSelect.Enabled = currentUserEmpNo == createdByEmpNo || this.Master.IsVisitorPassSystemAdmin;
                    }
                    #endregion

                    #region Set background color or blocked records
                    bool isBlocked = false;
                    Label lblIsBlock = item["IsBlock"].FindControl("lblIsBlock") as Label;
                    if (lblIsBlock != null)
                    {
                        isBlocked = lblIsBlock.Text == "Yes" ? true : false;
                        if (isBlocked)
                        {
                            item.BackColor = System.Drawing.Color.FromName("#ff3300");
                        }
                    }
                    #endregion
                }
            }
        }

        protected void gridVisitor_PreRender(object sender, EventArgs e)
        {
            try
            {
                GridColumn dynamicColumn = this.gridVisitor.MasterTableView.RenderColumns.Where(a => a.UniqueName == "VisitorCardNo").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    dynamicColumn.ItemStyle.Font.Bold = true;
                    dynamicColumn.ItemStyle.ForeColor = System.Drawing.Color.Purple;
                }

                #region Show/Hide checkbox selection column
                //dynamicColumn = this.gridVisitor.MasterTableView.RenderColumns.Where(a => a.UniqueName == "CheckboxSelectColumn").FirstOrDefault();
                //if (dynamicColumn != null)
                //{
                //    dynamicColumn.Visible = this.Master.IsTrainingAdmin;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void RebindDataToGrid()
        {
            if (this.DataList.Count > 0)
            {
                int totalRecords = this.DataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridVisitor.VirtualItemCount = totalRecords;
                else
                    this.gridVisitor.VirtualItemCount = 1;

                this.gridVisitor.DataSource = this.DataList;
                this.gridVisitor.DataBind();
            }
            else
                InitializeGrid();
        }

        private void InitializeGrid()
        {
            this.gridVisitor.DataSource = new List<VisitorPassEntity>();
            this.gridVisitor.DataBind();
        }
        #endregion

        #region Action Buttons
        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_VISITOR_PASS_INQUIRY
            ),
            false);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            int errorCount = 0;

            // Check Date Duration
            if (this.dtpVisitStartDate.SelectedDate == null && this.dtpVisitEndDate.SelectedDate != null ||
                this.dtpVisitStartDate.SelectedDate != null && this.dtpVisitEndDate.SelectedDate == null)
            {
                this.txtGeneric.Text = ValidationErrorType.InvalidDateDuration.ToString();
                this.ErrorType = ValidationErrorType.InvalidDateDuration;
                this.cusValVisitDate.Validate();
                errorCount++;
            }

            if (errorCount > 0)
                return;
            #endregion

            // Reset page index
            this.gridVisitor.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridVisitor.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridVisitor.PageSize;

            GetVistorPassLog(true);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset Controls
            this.txtVisitorName.Text = string.Empty;
            this.txtIDNumber.Text = string.Empty;
            this.txtVisitorCardNo.Text = string.Empty;
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.dtpVisitStartDate.SelectedDate = null;
            this.dtpVisitEndDate.SelectedDate = null;
            this.rblBlockOption.SelectedValue = BlockUserOption.valAll.ToString();
            this.txtOtherEmpNo.Text = string.Empty;

            this.rblCreatedBy.SelectedValue = Convert.ToInt32(UIHelper.CreatedByOptions.All).ToString();
            this.rblCreatedBy_SelectedIndexChanged(this.rblCreatedBy, new EventArgs());
            #endregion

            // Clear collections
            this.VisitorPassList.Clear();
            this.CheckedVisitorList.Clear();
            this.DataList.Clear();

            // Reset the grid
            this.gridVisitor.VirtualItemCount = 1;
            this.gridVisitor.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridVisitor.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridVisitor.PageSize;

            KillSessions();
            InitializeGrid();
            FillComboData(false);

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnNewRecord_Click(object sender, EventArgs e)
        {
            #region Redirect to Visitor Pass Entry page
            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            // Redirect to Employee Training Entry page
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_VISITOR_PASS_ENTRY + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_VISITOR_PASS_INQUIRY,
                UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString()
            ),
            false);
            #endregion
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedVisitorList.Clear();

            #region Loop through each record in the grid
            GridDataItemCollection gridData = this.gridVisitor.MasterTableView.Items;
            if (gridData.Count > 0)
            {
                foreach (GridDataItem item in gridData)
                {
                    System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    int logID = UIHelper.ConvertObjectToInt(this.gridVisitor.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("LogID"));

                    if (chkSelectColumn != null)
                    {
                        if (chkSelectColumn.Checked)
                        {
                            if (this.DataList.Count > 0 && logID > 0)
                            {
                                VisitorPassEntity selectedRecord = this.DataList
                                    .Where(a => a.LogID == logID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedVisitorList.Count == 0)
                                    {
                                        this.CheckedVisitorList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedVisitorList.Count > 0 &&
                                        this.CheckedVisitorList.Where(a => a.LogID == selectedRecord.LogID).FirstOrDefault() == null)
                                    {
                                        this.CheckedVisitorList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record already exist in the selected item collection
                            if (logID > 0)
                            {
                                VisitorPassEntity selectedRecord = this.DataList
                                    .Where(a => a.LogID == logID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedVisitorList.Count > 0
                                        && this.CheckedVisitorList.Where(a => a.LogID == selectedRecord.LogID).FirstOrDefault() != null)
                                    {
                                        VisitorPassEntity itemToDelete = this.CheckedVisitorList
                                            .Where(a => a.LogID == selectedRecord.LogID)
                                            .FirstOrDefault();
                                        if (itemToDelete != null)
                                        {
                                            this.CheckedVisitorList.Remove(itemToDelete);
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
            if (this.CheckedVisitorList.Count == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoSelectedRecordToDelete.ToString();
                this.ErrorType = ValidationErrorType.NoSelectedRecordToDelete;
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

        protected void btnDeleteDummy_Click(object sender, EventArgs e)
        {
            if (this.CheckedVisitorList == null ||
                this.CheckedVisitorList.Count == 0)
                return;

            if (DeleteVisitorRecord(this.CheckedVisitorList))
            {
                // Refresh data in the grid
                this.btnSearch_Click(this.btnSearch, new EventArgs());
            }
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            #region Perform Validation                        
            int errorCount = 0;
            if (this.DataList.Count == 0)
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

            // Pass data to the session
            Session["VisitorSummaryReportSource"] = GetVistorPassForReport();

            // Show the report
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                UIHelper.ReportTypes.VisitorPassSummaryReport.ToString(),
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_VISITOR_PASS_INQUIRY
            ),
            false);
            #endregion
        }

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {

        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnFindOtherEmp_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo, (sender as RadButton).ID);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_VISITOR_PASS_INQUIRY
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
                else if (this.ErrorType == ValidationErrorType.InvalidDateDuration)
                {
                    validator.ErrorMessage = "Date duration is invalid. Start Date must be less than or equal to End Date.";
                    validator.ToolTip = "Date duration is invalid. Start Date must be less than or equal to End Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToPrint)
                {
                    validator.ErrorMessage = "Unable to display the report because no record is found on the grid.";
                    validator.ToolTip = "Unable to display the report because no record is found on the grid.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Unable to delete because no record has been selected on the grid.";
                    validator.ToolTip = "Unable to delete because no record has been selected on the grid.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSelectedRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the records to delete from the grid.";
                    validator.ToolTip = "Please select the records to delete from the grid.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordOnGrid)
                {
                    validator.ErrorMessage = "Could not export data to Excel because no records were found on the grid.";
                    validator.ToolTip = "Could not export data to Excel because no records were found on the grid.";
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

        protected void cboCostCenter_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            try
            {
                if (this.cboCostCenter.DataSource != null)
                    return;

                DataView dv = this.objCostCenter.Select() as DataView;
                if (dv == null || dv.Count == 0)
                    return;

                DataRow[] source = new DataRow[dv.Count];
                dv.Table.Rows.CopyTo(source, 0);
                EmployeeDAL.CostCenterDataTable filteredDT = new EmployeeDAL.CostCenterDataTable();
                List<string> AllowedCostCenterList = Session[UIHelper.CONST_ALLOWED_COSTCENTER] != null ? Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string> : null;

                if (AllowedCostCenterList != null && AllowedCostCenterList.Count > 0)
                {
                    foreach (string filter in AllowedCostCenterList)
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
                            filteredDT.Rows.Add(row);
                        }
                    }
                }
                else
                {
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
                        filteredDT.Rows.Add(row);
                    }
                }

                if (filteredDT.Rows.Count > 0)
                {
                    this.cboCostCenter.DataTextField = "CostCenterName";
                    this.cboCostCenter.DataValueField = "CostCenter";
                    this.cboCostCenter.DataSource = filteredDT;
                    this.cboCostCenter.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void lnkVisitorName_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkVisitorName_Click = sender as LinkButton;
                GridDataItem item = lnkVisitorName_Click.NamingContainer as GridDataItem;
                if (item != null)
                {
                    // Initialize variables
                    int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);
                    int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    string formLoadType = string.Empty;
                    if (createdByEmpNo == currentUserEmpNo ||
                        this.Master.IsVisitorPassSystemAdmin)
                    {
                        formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString();
                    }
                    else
                        formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString();

                    // Get data key value
                    long logID = UIHelper.ConvertObjectToLong(this.gridVisitor.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("LogID"));
                    if (this.DataList.Count > 0)
                    {
                        VisitorPassEntity selectedRecord = this.DataList
                            .Where(a => a.LogID == logID)
                            .FirstOrDefault();

                        if (selectedRecord != null && logID > 0)
                        {
                            // Save to session
                            Session["SelectedVisitorPassRecord"] = selectedRecord;
                        }
                    }

                    // Save session values
                    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    // Redirect to Employee Training Entry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_VISITOR_PASS_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_VISITOR_PASS_INQUIRY,
                        UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                        logID,
                        UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                        formLoadType
                    ),
                    false);
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void rblCreatedBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rblCreatedBy.SelectedValue == "2")    // Others
            {
                this.txtOtherEmpNo.Visible = true;
                this.btnFindOtherEmp.Visible = true;
                this.txtOtherEmpNo.Focus();
            }
            else
            {
                this.txtOtherEmpNo.Visible = false;
                this.btnFindOtherEmp.Visible = false;
            }
        }
        #endregion

        #region Private Methods
        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag, string sourceControl = "")
        {
            this.VisitorInquiryStorage.Clear();
            this.VisitorInquiryStorage.Add("FormFlag", formFlag.ToString());

            #region Store control values to the collection            
            this.VisitorInquiryStorage.Add("txtVisitorName", this.txtVisitorName.Text.Trim());
            this.VisitorInquiryStorage.Add("txtIDNumber", this.txtIDNumber.Text.Trim());
            this.VisitorInquiryStorage.Add("txtVisitorCardNo", this.txtVisitorCardNo.Text.Trim());
            this.VisitorInquiryStorage.Add("dtpVisitStartDate", this.dtpVisitStartDate.SelectedDate);
            this.VisitorInquiryStorage.Add("dtpVisitEndDate", this.dtpVisitEndDate.SelectedDate);
            this.VisitorInquiryStorage.Add("cboCostCenter", this.cboCostCenter.Text);
            this.VisitorInquiryStorage.Add("txtEmpNo", this.txtEmpNo.Text);
            this.VisitorInquiryStorage.Add("litEmpName", this.litEmpName.Text);
            this.VisitorInquiryStorage.Add("rblBlockOption", this.rblBlockOption.SelectedValue);
            this.VisitorInquiryStorage.Add("rblCreatedBy", this.rblCreatedBy.SelectedValue);
            this.VisitorInquiryStorage.Add("txtOtherEmpNo", this.txtOtherEmpNo.Text);
            #endregion

            #region Store query string values to the collection
            this.VisitorInquiryStorage.Add("CallerForm", this.CallerForm);
            this.VisitorInquiryStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to the collection
            this.VisitorInquiryStorage.Add("SourceControl", sourceControl);
            this.VisitorInquiryStorage.Add("VisitorPassList", this.VisitorPassList);
            this.VisitorInquiryStorage.Add("CheckedVisitorList", this.CheckedVisitorList);
            this.VisitorInquiryStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.VisitorInquiryStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.VisitorInquiryStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.VisitorInquiryStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.VisitorInquiryStorage.Add("DataList", this.DataList);
            #endregion
        }

        private void RestoreDataFromCollection()
        {
            if (this.VisitorInquiryStorage.Count == 0)
                return;

            #region Restore query string values
            if (this.VisitorInquiryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.VisitorInquiryStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.VisitorInquiryStorage.ContainsKey("VisitorPassList"))
                this.VisitorPassList = this.VisitorInquiryStorage["VisitorPassList"] as List<VisitorPassEntity>;
            else
                this.VisitorPassList = null;

            if (this.VisitorInquiryStorage.ContainsKey("CheckedVisitorList"))
                this.CheckedVisitorList = this.VisitorInquiryStorage["CheckedVisitorList"] as List<VisitorPassEntity>;
            else
                this.CheckedVisitorList = null;

            if (this.VisitorInquiryStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.VisitorInquiryStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.VisitorInquiryStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.VisitorInquiryStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.VisitorInquiryStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.VisitorInquiryStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.VisitorInquiryStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.VisitorInquiryStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.VisitorInquiryStorage.ContainsKey("DataList"))
                this.DataList = this.VisitorInquiryStorage["DataList"] as List<VisitorPassEntity>;
            else
                this.DataList = null;

            // Reload combo data
            FillComboData(false);
            #endregion

            #region Restore control values
            if (this.VisitorInquiryStorage.ContainsKey("txtVisitorName"))
                this.txtVisitorName.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["txtVisitorName"]);
            else
                this.txtVisitorName.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("txtIDNumber"))
                this.txtIDNumber.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["txtIDNumber"]);
            else
                this.txtIDNumber.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("txtVisitorCardNo"))
                this.txtVisitorCardNo.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["txtVisitorCardNo"]);
            else
                this.txtVisitorCardNo.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("dtpVisitStartDate"))
                this.dtpVisitStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorInquiryStorage["dtpVisitStartDate"]);
            else
                this.dtpVisitStartDate.SelectedDate = null;

            if (this.VisitorInquiryStorage.ContainsKey("dtpVisitEndDate"))
                this.dtpVisitEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.VisitorInquiryStorage["dtpVisitEndDate"]);
            else
                this.dtpVisitEndDate.SelectedDate = null;

            if (this.VisitorInquiryStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["cboCostCenter"]);
            else
                this.cboCostCenter.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("rblBlockOption"))
                this.rblBlockOption.SelectedValue = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["rblBlockOption"]);
            else
                this.rblBlockOption.SelectedValue = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("txtOtherEmpNo"))
                this.txtOtherEmpNo.Text = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["txtOtherEmpNo"]);
            else
                this.txtOtherEmpNo.Text = string.Empty;

            if (this.VisitorInquiryStorage.ContainsKey("rblCreatedBy"))
                this.rblCreatedBy.SelectedValue = UIHelper.ConvertObjectToString(this.VisitorInquiryStorage["rblCreatedBy"]);
            else
                this.rblCreatedBy.ClearSelection();

            this.rblCreatedBy_SelectedIndexChanged(this.rblCreatedBy, new EventArgs());
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridVisitor.CurrentPageIndex = this.CurrentPageIndex;
            this.gridVisitor.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridVisitor.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridVisitor.MasterTableView.DataBind();
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            
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

            //if (this.AllowedCostCenterList.Count > 0)
            //{
            //    #region Filter list based on allowed cost center
            //    foreach (string filter in this.AllowedCostCenterList)
            //    {
            //        DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
            //        foreach (DataRow rw in rows)
            //        {
            //            EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
            //            row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
            //            row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
            //            row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);

            //            // Add record to the collection
            //            filteredDT.Rows.Add(row);
            //        }
            //    }

            //    // Set the flag
            //    enableEmpSearch = true;
            //    #endregion
            //}
            //else if (this.AllowedCostCenterList.Count == 0 && UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty)
            //{
            //    #region Filter list based on user's cost center
            //    this.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

            //    foreach (string filter in this.AllowedCostCenterList)
            //    {
            //        DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
            //        foreach (DataRow rw in rows)
            //        {
            //            EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
            //            row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
            //            row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
            //            row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);

            //            // Add record to the collection
            //            filteredDT.Rows.Add(row);
            //        }
            //    }

            //    //// Set the flag
            //    enableEmpSearch = true;
            //    #endregion
            //}
            //else
            //{
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
            //}

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();

                // Enable/Disable employee search button
                this.btnFindEmployee.Enabled = enableEmpSearch;
            }
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

        private void BeginSearch(bool isDirty = false)
        {
            int errorCount = 0;

            try
            {
                #region Perform data validation
                // Check Date Duration
                if (this.dtpVisitStartDate.SelectedDate == null && this.dtpVisitEndDate.SelectedDate != null ||
                    this.dtpVisitStartDate.SelectedDate != null && this.dtpVisitEndDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateDuration.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateDuration;
                    this.cusValVisitDate.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                    return;
                #endregion

                #region Initialize variables               
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                long logID = 0;
                string visitorName = this.txtVisitorName.Text.Trim();
                string idNumber = this.txtIDNumber.Text.Trim();
                int visitorCardNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                int visitEmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                string vistiCostCenter = this.cboCostCenter.Text.Trim();
                DateTime? startDate = this.dtpVisitStartDate.SelectedDate;
                DateTime? endDate = this.dtpVisitEndDate.SelectedDate;                
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                byte blockOption = 0;
                if (this.rblBlockOption.SelectedValue == BlockUserOption.valYes.ToString())
                    blockOption = 1;
                else if (this.rblBlockOption.SelectedValue == BlockUserOption.valNo.ToString())
                    blockOption = 2;
                #endregion

                #region Initialize record count
                this.lblRecordCount.Text = "No record found";
                this.gridVisitor.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<VisitorPassEntity> gridSource = new List<VisitorPassEntity>();
                if (this.VisitorPassList.Count > 0 && !isDirty)
                {
                    gridSource = this.VisitorPassList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    var source = dataProxy.GetVisitorPassLog(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, vistiCostCenter, startDate, endDate, blockOption, userEmpNo, ref error, ref innerError);
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
                this.VisitorPassList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.VisitorPassList.Count > 0)
                {
                    int totalRecords = this.VisitorPassList.Count;
                    if (totalRecords > 0)
                        this.gridVisitor.VirtualItemCount = totalRecords;
                    else
                        this.gridVisitor.VirtualItemCount = 1;

                    this.gridVisitor.DataSource = this.VisitorPassList;
                    this.gridVisitor.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeGrid();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetVistorPassLog(bool reloadDataFromDB = false)
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

                long logID = 0;
                string visitorName = this.txtVisitorName.Text.Trim();
                string idNumber = this.txtIDNumber.Text.Trim();
                int visitorCardNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                int visitEmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                string vistiCostCenter = this.cboCostCenter.Text.Trim();
                DateTime? startDate = this.dtpVisitStartDate.SelectedDate;
                DateTime? endDate = this.dtpVisitEndDate.SelectedDate;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                byte createdByTypeID = UIHelper.ConvertObjectToByte(this.rblCreatedBy.SelectedValue);

                int createdByOtherEmpNo = UIHelper.ConvertObjectToInt(this.txtOtherEmpNo.Text);
                if (createdByOtherEmpNo.ToString().Length == 4)
                {
                    createdByOtherEmpNo += 10000000;

                    // Display Currently Assigned to Emp. No.
                    this.txtOtherEmpNo.Text = createdByOtherEmpNo.ToString();
                }

                byte blockOption = 0;
                if (this.rblBlockOption.SelectedValue == BlockUserOption.valYes.ToString())
                    blockOption = 1;
                else if (this.rblBlockOption.SelectedValue == BlockUserOption.valNo.ToString())
                    blockOption = 2;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridVisitor.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<VisitorPassEntity> gridSource = new List<VisitorPassEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.DataList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    var source = dataProxy.GetVisitorPassLogV2(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, vistiCostCenter, 
                        startDate, endDate, blockOption, userEmpNo, createdByOtherEmpNo, createdByTypeID,
                        this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                        this.gridVisitor.VirtualItemCount = totalRecords;
                    else
                        this.gridVisitor.VirtualItemCount = 1;

                    this.gridVisitor.DataSource = this.DataList;
                    this.gridVisitor.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeGrid();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private List<VisitorPassEntity> GetVistorPassForReport()
        {
            try
            {
                #region Initialize variables      
                List<VisitorPassEntity> visitorList = new List<VisitorPassEntity>();
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                long logID = 0;
                string visitorName = this.txtVisitorName.Text.Trim();
                string idNumber = this.txtIDNumber.Text.Trim();
                int visitorCardNo = UIHelper.ConvertObjectToInt(this.txtVisitorCardNo.Text);
                int visitEmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                string vistiCostCenter = this.cboCostCenter.Text.Trim();
                DateTime? startDate = this.dtpVisitStartDate.SelectedDate;
                DateTime? endDate = this.dtpVisitEndDate.SelectedDate;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                byte createdByTypeID = UIHelper.ConvertObjectToByte(this.rblCreatedBy.SelectedValue);

                int createdByOtherEmpNo = UIHelper.ConvertObjectToInt(this.txtOtherEmpNo.Text);
                if (createdByOtherEmpNo.ToString().Length == 4)
                {
                    createdByOtherEmpNo += 10000000;

                    // Display Currently Assigned to Emp. No.
                    this.txtOtherEmpNo.Text = createdByOtherEmpNo.ToString();
                }

                byte blockOption = 0;
                if (this.rblBlockOption.SelectedValue == BlockUserOption.valYes.ToString())
                    blockOption = 1;
                else if (this.rblBlockOption.SelectedValue == BlockUserOption.valNo.ToString())
                    blockOption = 2;
                #endregion

                #region Fill data to the collection
                // Get WCF Instance
                if (dataProxy == null)
                    return null;

                string error = string.Empty;
                string innerError = string.Empty;

                var source = dataProxy.GetVisitorPassLog(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, vistiCostCenter,
                    startDate, endDate, blockOption, createdByOtherEmpNo, ref error, ref innerError);
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
                    if (source != null 
                        && source.Count() > 0)
                    {
                        visitorList.AddRange(source);
                    }
                }
                #endregion

                return visitorList;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
                return null;
            }
        }
        #endregion

        #region Database Access
        protected void objCostCenter_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            EmployeeDAL.CostCenterDataTable dataTable = e.ReturnValue as
                EmployeeDAL.CostCenterDataTable;

            // Checks if found
            if (dataTable != null)
            {
                #region Create a new record
                //EmployeeDAL.CostCenterRow row = dataTable.NewCostCenterRow();

                //row.CostCenter = String.Empty;
                //row.CostCenterName = "Please select a Cost Center...";
                //row.Company = String.Empty;
                //row.SuperintendentNo = 0;
                //row.SuperintendentName = String.Empty;
                //row.ManagerNo = 0;
                //row.ManagerName = String.Empty;

                //dataTable.Rows.InsertAt(row, 0);
                #endregion
            }
        }

        private bool DeleteVisitorRecord(List<VisitorPassEntity> recordToDeleteList)
        {
            if (dataProxy == null || recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                dataProxy.DeleteVisitorPassMultipleRecord(recordToDeleteList, ref error, ref innerError);
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
        #endregion                        
    }
}