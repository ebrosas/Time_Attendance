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
    public partial class DutyROTAInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoStartDate,
            NoEndDate,
            NoDutyType,
            InvalidDateRange,
            NoRecordToDelete
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

        private Dictionary<string, object> DutyROTAInqStorage
        {
            get
            {
                Dictionary<string, object> list = Session["DutyROTAInqStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["DutyROTAInqStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["DutyROTAInqStorage"] = value;
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

        private List<DutyROTAEntity> DutyROTAList
        {
            get
            {
                List<DutyROTAEntity> list = ViewState["DutyROTAList"] as List<DutyROTAEntity>;
                if (list == null)
                    ViewState["DutyROTAList"] = list = new List<DutyROTAEntity>();

                return list;
            }
            set
            {
                ViewState["DutyROTAList"] = value;
            }
        }

        private List<DutyROTAEntity> CheckedDutyROTAList
        {
            get
            {
                List<DutyROTAEntity> list = ViewState["CheckedDutyROTAList"] as List<DutyROTAEntity>;
                if (list == null)
                    ViewState["CheckedDutyROTAList"] = list = new List<DutyROTAEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedDutyROTAList"] = value;
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

        private List<DutyROTAEntity> DutyTypeList
        {
            get
            {
                List<DutyROTAEntity> list = ViewState["DutyTypeList"] as List<DutyROTAEntity>;
                if (list == null)
                    ViewState["DutyTypeList"] = list = new List<DutyROTAEntity>();

                return list;
            }
            set
            {
                ViewState["DutyTypeList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.DROTAINQ.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_DUTY_ROTA_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_DUTY_ROTA_INQUIRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnNew.Enabled = this.Master.IsCreateAllowed;
                this.btnDelete.Enabled = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.DutyROTAInqStorage.Count > 0)
                {
                    if (this.DutyROTAInqStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.DutyROTAInqStorage["FormFlag"]);
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
                    Session.Remove("DutyROTAInqStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("DutyROTAInqStorage");

                    // Check if need to refresh data in the grid
                    if (this.ReloadGridData)
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("DutyROTAInqStorage");
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
        protected void gridSearchResults_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetDutyROTA(true);
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetDutyROTA(true);
        }

        protected void gridSearchResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.DutyROTAList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.DutyROTAList;
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
                    #region Open the Manual Timesheet data entry form
                    dynamic itemObj = e.CommandSource;
                    string itemText = itemObj.Text;

                    // Get data key value
                    long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0 && this.DutyROTAList.Count > 0)
                    {
                        DutyROTAEntity selectedRecord = this.DutyROTAList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null && autoID > 0)
                        {
                            // Save to session
                            Session["SelectedDutyROTA"] = selectedRecord;
                        }
                    }

                    if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region Edit link is clicked
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                       (
                           String.Format(UIHelper.PAGE_DUTY_ROTA_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                           UIHelper.PAGE_DUTY_ROTA_INQ,
                           UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                           autoID,
                           UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                           Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString()
                       ),
                       false);
                        #endregion
                    }
                    else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region View link is clicked
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                       (
                           String.Format(UIHelper.PAGE_DUTY_ROTA_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                           UIHelper.PAGE_DUTY_ROTA_INQ,
                           UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                           autoID,
                           UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                           Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString()
                       ),
                       false);
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
                    #region Enable Edit link based on user permission
                    LinkButton editLink = item["EditLinkButton"].Controls[0] as LinkButton;
                    if (editLink != null)
                    {
                        string currentUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        string currentUserCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                        string lastUpdateUserID = string.Empty;
                        string costCenter = UIHelper.ConvertObjectToString(item["CostCenter"].Text);

                        Literal litLastUpdateUser = item["LastUpdateUser"].FindControl("litLastUpdateUser") as Literal;
                        if (litLastUpdateUser != null)
                        {
                            int idx = litLastUpdateUser.Text.Trim().LastIndexOf(@"\");
                            if (idx > 0)
                                lastUpdateUserID = litLastUpdateUser.Text.Trim().Substring(idx + 1);
                        }

                        // Enable Edit link if the following conditions are met:
                        // The value of "LastUpdateUser" field equals to the current logged-on user id
                        // The current logged-on user belongs to the same cost center of the affected employee
                        // Current user belongs to HR department 
                        if ((!string.IsNullOrEmpty(lastUpdateUserID) && lastUpdateUserID == currentUserID) ||
                            (costCenter == currentUserCostCenter) ||
                            (currentUserCostCenter == "7500"))
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
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.DutyROTAList.Count > 0)
            {
                int totalRecords = this.DutyROTAList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridSearchResults.VirtualItemCount = totalRecords;
                else
                    this.gridSearchResults.VirtualItemCount = 1;

                this.gridSearchResults.DataSource = this.DutyROTAList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<DutyROTAEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.dtpEffectiveDate.SelectedDate = null;
            this.dtpEndingDate.SelectedDate = null;
            this.cboDutyType.Text = string.Empty;
            this.cboDutyType.SelectedIndex = -1;

            // Cler collections
            this.DutyROTAList.Clear();
            this.CheckedDutyROTAList.Clear();

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
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            GetDutyROTA(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_DUTY_ROTA_INQ
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            // Redirect to Employee Training Entry page
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_DUTY_ROTA_ENTRY + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_DUTY_ROTA_INQ,
                UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString()
            ),
            false);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedDutyROTAList.Clear();

            #region Loop through each record in the grid
            GridDataItemCollection gridData = this.gridSearchResults.MasterTableView.Items;
            if (gridData.Count > 0)
            {
                foreach (GridDataItem item in gridData)
                {
                    System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                    if (chkSelectColumn != null)
                    {
                        if (chkSelectColumn.Checked)
                        {
                            if (this.DutyROTAList.Count > 0 && autoID > 0)
                            {
                                DutyROTAEntity selectedRecord = this.DutyROTAList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedDutyROTAList.Count == 0)
                                    {
                                        this.CheckedDutyROTAList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedDutyROTAList.Count > 0 &&
                                        this.CheckedDutyROTAList.Where(a => a.AutoID == selectedRecord.AutoID).FirstOrDefault() == null)
                                    {
                                        this.CheckedDutyROTAList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record exist in the selected item collection
                            if (autoID > 0)
                            {
                                DutyROTAEntity selectedRecord = this.DutyROTAList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedDutyROTAList.Count > 0
                                        && this.CheckedDutyROTAList.Where(a => a.AutoID == selectedRecord.AutoID).FirstOrDefault() != null)
                                    {
                                        DutyROTAEntity itemToDelete = this.CheckedDutyROTAList
                                            .Where(a => a.AutoID == selectedRecord.AutoID)
                                            .FirstOrDefault();
                                        if (itemToDelete != null)
                                        {
                                            this.CheckedDutyROTAList.Remove(itemToDelete);
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
            if (this.CheckedDutyROTAList.Count == 0)
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

        protected void btnDeleteDummy_Click(object sender, EventArgs e)
        {
            if (this.CheckedDutyROTAList == null ||
                this.CheckedDutyROTAList.Count == 0)
                return;

            if (DeleteDutyROTA(this.CheckedDutyROTAList))
            {
                // Refresh data in the grid
                this.btnSearch_Click(this.btnSearch, new EventArgs());
            }
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnPrintReport_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                //// Check Duty Type
                //if (string.IsNullOrEmpty(this.cboDutyType.SelectedValue) ||
                //    this.cboDutyType.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID)
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoDutyType.ToString();
                //    this.ErrorType = ValidationErrorType.NoDutyType;
                //    this.cusValDutyType.Validate();
                //    errorCount++;
                //}

                // Check Effective Date and Ending Date
                if (this.dtpEffectiveDate.SelectedDate != null &&
                this.dtpEndingDate.SelectedDate != null)
                {
                    if (this.dtpEffectiveDate.SelectedDate > this.dtpEndingDate.SelectedDate)
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
                    if (this.dtpEffectiveDate.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoStartDate.ToString();
                        this.ErrorType = ValidationErrorType.NoStartDate;
                        this.cusValStartDate.Validate();
                        errorCount++;
                    }

                    // Check End Date
                    if (this.dtpEndingDate.SelectedDate == null)
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

                #region Initialize variables               
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                int autoID = 0;
                string dutyType = this.cboDutyType.SelectedValue;
                if (dutyType == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    dutyType = string.Empty;

                DateTime? effectiveDate = this.dtpEffectiveDate.SelectedDate;
                DateTime? endingDate = this.dtpEndingDate.SelectedDate;
                #endregion

                #region Retrieve database records then show the report
                List<DutyROTAEntity> reportDataSource = new List<DutyROTAEntity>();
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var source = proxy.GetDutyROTAEntry(autoID, empNo, effectiveDate, endingDate, dutyType, 1, this.gridSearchResults.VirtualItemCount, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    reportDataSource.AddRange(source.ToList());

                    // Save report data to session
                    Session["DutyROTAReportSource"] = reportDataSource;
                    StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                    // Show the report
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                        UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                        UIHelper.ReportTypes.DutyROTAReport.ToString(),
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_DUTY_ROTA_INQ,
                        UIHelper.QUERY_STRING_STARTDATE_KEY,
                        effectiveDate.HasValue ? effectiveDate.Value.ToString() : string.Empty,
                        UIHelper.QUERY_STRING_ENDDATE_KEY,
                        endingDate.HasValue ? endingDate.Value.ToString() : string.Empty
                    ),
                    false);
                }
                #endregion
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
                    validator.ErrorMessage = "Effective Date is required when viewing the report.";
                    validator.ToolTip = "Effective Date is required when viewing the report.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEndDate)
                {
                    validator.ErrorMessage = "Ending Date is required when viewing the report.";
                    validator.ToolTip = "Ending Date is required when viewing the report.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDutyType)
                {
                    validator.ErrorMessage = "Duty Type is required when viewing the report.";
                    validator.ToolTip = "Duty Type is required when viewing the report.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that the Effective Date is less than the Ending Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that the Effective Date is less than the Ending Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record to delete in the grid.";
                    validator.ToolTip = "Please select the record to delete in the grid.";
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
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.dtpEffectiveDate.SelectedDate = null;
            this.dtpEndingDate.SelectedDate = null;
            this.cboDutyType.Text = string.Empty;
            this.cboDutyType.SelectedIndex = -1;
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
            this.DutyROTAList.Clear();
            this.CheckedDutyROTAList.Clear();
            this.DutyTypeList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["SelectedDutyROTA"] = null;
            Session.Remove("SelectedDutyROTA");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.DutyROTAInqStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.DutyROTAInqStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.DutyROTAInqStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.DutyROTAInqStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.DutyROTAInqStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.DutyROTAInqStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.DutyROTAInqStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.DutyROTAInqStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.DutyROTAInqStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.DutyROTAInqStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.DutyROTAInqStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.DutyROTAInqStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.DutyROTAInqStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.DutyROTAInqStorage.ContainsKey("DutyROTAList"))
                this.DutyROTAList = this.DutyROTAInqStorage["DutyROTAList"] as List<DutyROTAEntity>;
            else
                this.DutyROTAList = null;

            if (this.DutyROTAInqStorage.ContainsKey("CheckedDutyROTAList"))
                this.CheckedDutyROTAList = this.DutyROTAInqStorage["DutyROTAList"] as List<DutyROTAEntity>;
            else
                this.CheckedDutyROTAList = null;

            if (this.DutyROTAInqStorage.ContainsKey("DutyTypeList"))
                this.DutyTypeList = this.DutyROTAInqStorage["DutyTypeList"] as List<DutyROTAEntity>;
            else
                this.DutyTypeList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.DutyROTAInqStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.DutyROTAInqStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.DutyROTAInqStorage.ContainsKey("dtpEffectiveDate"))
                this.dtpEffectiveDate.SelectedDate = UIHelper.ConvertObjectToDate(this.DutyROTAInqStorage["dtpEffectiveDate"]);
            else
                this.dtpEffectiveDate.SelectedDate = null;

            if (this.DutyROTAInqStorage.ContainsKey("dtpEndingDate"))
                this.dtpEndingDate.SelectedDate = UIHelper.ConvertObjectToDate(this.DutyROTAInqStorage["dtpEndingDate"]);
            else
                this.dtpEndingDate.SelectedDate = null;

            if (this.DutyROTAInqStorage.ContainsKey("cboDutyType"))
                this.cboDutyType.SelectedValue = UIHelper.ConvertObjectToString(this.DutyROTAInqStorage["cboDutyType"]);
            else
            {
                this.cboDutyType.Text = string.Empty;
                this.cboDutyType.SelectedIndex = -1;
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
            this.DutyROTAInqStorage.Clear();
            this.DutyROTAInqStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.DutyROTAInqStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.DutyROTAInqStorage.Add("cboDutyType", this.cboDutyType.SelectedValue);
            this.DutyROTAInqStorage.Add("dtpEffectiveDate", this.dtpEffectiveDate.SelectedDate);
            this.DutyROTAInqStorage.Add("dtpEndingDate", this.dtpEndingDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.DutyROTAInqStorage.Add("CallerForm", this.CallerForm);
            this.DutyROTAInqStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.DutyROTAInqStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.DutyROTAInqStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.DutyROTAInqStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.DutyROTAInqStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.DutyROTAInqStorage.Add("DutyROTAList", this.DutyROTAList);
            this.DutyROTAInqStorage.Add("CheckedDutyROTAList", this.CheckedDutyROTAList);
            this.DutyROTAInqStorage.Add("DutyTypeList", this.DutyTypeList);
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
            FillDutyTypeCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetDutyROTA(bool reloadDataFromDB = false)
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

                int autoID = 0;
                string dutyType = this.cboDutyType.SelectedValue;
                if (dutyType == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    dutyType = string.Empty;

                DateTime? effectiveDate = this.dtpEffectiveDate.SelectedDate;
                DateTime? endingDate = this.dtpEndingDate.SelectedDate;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridSearchResults.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<DutyROTAEntity> gridSource = new List<DutyROTAEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.DutyROTAList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetDutyROTAEntry(autoID, empNo, effectiveDate, endingDate, dutyType, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                this.DutyROTAList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.DutyROTAList.Count > 0)
                {
                    int totalRecords = this.DutyROTAList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridSearchResults.VirtualItemCount = totalRecords;
                    else
                        this.gridSearchResults.VirtualItemCount = 1;

                    this.gridSearchResults.DataSource = this.DutyROTAList;
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

        private bool DeleteDutyROTA(List<DutyROTAEntity> recordToDeleteList)
        {
            if (recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteDutyROTA(Convert.ToInt32(UIHelper.SaveType.Delete), recordToDeleteList, ref error, ref innerError);
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

        private void FillDutyTypeCombo(bool reloadFromDB, string defaultValue = "")
        {
            try
            {
                List<DutyROTAEntity> comboSource = new List<DutyROTAEntity>();
                if (this.DutyTypeList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.DutyTypeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    comboSource = proxy.GetDutyROTAType(ref error, ref innerError);
                    if (comboSource != null && comboSource.Count() > 0)
                    {
                        // Add blank item
                        comboSource.Insert(0, new DutyROTAEntity()
                        {
                            AutoID = 0,
                            DutyType = UIHelper.CONST_COMBO_EMTYITEM_ID,
                            DutyDescription = string.Empty,
                            DutyAllowance = 0
                        });
                    }
                }

                // Store to session
                this.DutyTypeList = comboSource;

                #region Bind data to combobox
                this.cboDutyType.DataSource = this.DutyTypeList;
                this.cboDutyType.DataTextField = "DutyDescription";
                this.cboDutyType.DataValueField = "DutyType";
                this.cboDutyType.DataBind();

                if (this.cboDutyType.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboDutyType.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }
        #endregion
                
    }
}