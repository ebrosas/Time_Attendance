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

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class ShiftPatternChanges : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            InvalidDateRange,
            NoRecordToDelete,
            CannotViewFireTeamMember,
            NoCostCenterPermission
        }

        private enum FilterOption
        {
            valAll,
            valEmployee,
            valFireTeamMember            
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

        private Dictionary<string, object> ShiftPatternStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ShiftPatternStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ShiftPatternStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ShiftPatternStorage"] = value;
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

        private List<ShiftPatternEntity> ShiftPatternList
        {
            get
            {
                List<ShiftPatternEntity> list = ViewState["ShiftPatternList"] as List<ShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPatternList"] = list = new List<ShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPatternList"] = value;
            }
        }

        private List<ShiftPatternEntity> CheckedShiftPatternList
        {
            get
            {
                List<ShiftPatternEntity> list = ViewState["CheckedShiftPatternList"] as List<ShiftPatternEntity>;
                if (list == null)
                    ViewState["CheckedShiftPatternList"] = list = new List<ShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedShiftPatternList"] = value;
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
                    pageSize = this.gridShiftPattern.MasterTableView.PageSize;

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

        private List<EmployeeDetail> FireTeamMemberList
        {
            get
            {
                List<EmployeeDetail> list = ViewState["FireTeamMemberList"] as List<EmployeeDetail>;
                if (list == null)
                    ViewState["FireTeamMemberList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                ViewState["FireTeamMemberList"] = value;
            }
        }

        private FilterOption CurrentFilterOption
        {
            get
            {
                FilterOption result = FilterOption.valAll;
                if (ViewState["CurrentFilterOption"] != null)
                {
                    try
                    {
                        result = (FilterOption)Enum.Parse(typeof(FilterOption), UIHelper.ConvertObjectToString(ViewState["CurrentFilterOption"]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            set
            {
                ViewState["CurrentFilterOption"] = value;
            }
        }

        private string PageTitle
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["PageTitle"]);
            }
            set
            {
                ViewState["PageTitle"] = value;
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

                #region Determine Filter Option
                FilterOption filterOption = FilterOption.valAll;
                string shiftPatternType = UIHelper.ConvertObjectToString(Request.QueryString["ShiftPatternType"]);

                if (shiftPatternType != string.Empty)
                {
                    try
                    {
                        filterOption = (FilterOption)Enum.Parse(typeof(FilterOption), shiftPatternType);
                    }
                    catch (Exception)
                    {
                    }
                }
                #endregion

                if (filterOption == FilterOption.valFireTeamMember)
                {
                    this.PageTitle = UIHelper.PAGE_SHIFT_PATTERN_CHANGE_FIRETEAM_INQUIRY_TITLE;
                    this.Master.SetPageForm(UIHelper.FormAccessCodes.SHFPATFIRE.ToString());
                }
                else
                {
                    this.PageTitle = UIHelper.PAGE_SHIFT_PATTERN_CHANGE_INQUIRY_TITLE;
                    this.Master.SetPageForm(UIHelper.FormAccessCodes.SHFTPATINQ.ToString());
                }

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
                this.Master.FormTitle = this.PageTitle;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), this.PageTitle), true);
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
                if (this.ShiftPatternStorage.Count > 0)
                {
                    if (this.ShiftPatternStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["FormFlag"]);
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
                    Session.Remove("ShiftPatternStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("ShiftPatternStorage");

                    this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
                    this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

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

                    this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
                    this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridShiftPattern_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetShiftPatternChangeRecords(true);
        }

        protected void gridShiftPattern_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetShiftPatternChangeRecords(true);
        }

        protected void gridShiftPattern_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ShiftPatternList.Count > 0)
            {
                this.gridShiftPattern.DataSource = this.ShiftPatternList;
                this.gridShiftPattern.DataBind();

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
                        sortExpr.SortOrder = this.gridShiftPattern.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridShiftPattern.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridShiftPattern_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Open the Shift Pattern Entry page
                    // Save session values
                    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    // Initialize variables
                    DateTime? effectiveDate = UIHelper.ConvertObjectToDate(item["EffectiveDate"].Text);
                    string formLoadType = string.Empty;

                    if (effectiveDate.HasValue && Convert.ToDateTime(effectiveDate) > DateTime.Now)
                        formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString();
                    else
                        formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString();

                    // Get data key value
                    long autoID = UIHelper.ConvertObjectToLong(this.gridShiftPattern.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (this.ShiftPatternList.Count > 0)
                    {
                        ShiftPatternEntity selectedRecord = this.ShiftPatternList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null && autoID > 0)
                        {
                            // Save to session
                            Session["SelectedShiftPatternChange"] = selectedRecord;
                        }
                    }

                    // Redirect to Employee Training Entry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ,
                        UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                        autoID,
                        UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                        formLoadType
                    ),
                    false);
                    #endregion
                }
            }
        }

        protected void gridShiftPattern_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Enable/Disable selection checkbox
                    DateTime? effectiveDate = UIHelper.ConvertObjectToDate(item["EffectiveDate"].Text);
                    CheckBox chkSelect = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;

                    if (chkSelect != null)
                    {
                        if (effectiveDate.HasValue)
                        {
                            chkSelect.Enabled = Convert.ToDateTime(effectiveDate) > DateTime.Now.Date;
                        }
                    }
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.ShiftPatternList.Count > 0)
            {
                int totalRecords = this.ShiftPatternList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridShiftPattern.VirtualItemCount = totalRecords;
                else
                    this.gridShiftPattern.VirtualItemCount = 1;

                this.gridShiftPattern.DataSource = this.ShiftPatternList;
                this.gridShiftPattern.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridShiftPattern.DataSource = new List<ShiftPatternEntity>();
            this.gridShiftPattern.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion
        
        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;

            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;

            this.cboChangeType.Text = string.Empty;
            this.cboChangeType.SelectedIndex = -1;
            this.cboFireTeamMeber.Text = string.Empty;
            this.cboFireTeamMeber.SelectedIndex = -1;
                        
            this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

            // Cler collections
            this.ShiftPatternList.Clear();
            this.CheckedShiftPatternList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Reset the grid
            this.gridShiftPattern.VirtualItemCount = 1;
            this.gridShiftPattern.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridShiftPattern.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridShiftPattern.PageSize;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check date duration
            if (this.dtpStartDate.SelectedDate != null &&
                this.dtpEndDate.SelectedDate != null)
            {
                if (this.dtpStartDate.SelectedDate > this.dtpEndDate.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateRange;
                    this.cusValEffectiveDate.Validate();
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

            // Reset page index
            this.gridShiftPattern.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridShiftPattern.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridShiftPattern.PageSize;

            GetShiftPatternChangeRecords(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            // Identity the Shift Pattern Display Type
            FilterOption displayType = FilterOption.valEmployee;
            if (!string.IsNullOrEmpty(this.rblOption.SelectedValue))
            {
                try
                {
                    displayType = (FilterOption)Enum.Parse(typeof(FilterOption), this.rblOption.SelectedValue);
                }
                catch (Exception)
                {
                }
            }

            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            // Redirect to Employee Training Entry page
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ,
                UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString(),
                "ShiftPatternType",
                Convert.ToInt16(displayType).ToString()
            ),
            false);            
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedShiftPatternList.Clear();

            #region Loop through each record in the grid
            GridDataItemCollection gridData = this.gridShiftPattern.MasterTableView.Items;
            if (gridData.Count > 0)
            {
                foreach (GridDataItem item in gridData)
                {
                    System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    int autoID = UIHelper.ConvertObjectToInt(this.gridShiftPattern.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                    if (chkSelectColumn != null)
                    {
                        if (chkSelectColumn.Checked)
                        {
                            if (this.ShiftPatternList.Count > 0 && autoID > 0)
                            {
                                ShiftPatternEntity selectedRecord = this.ShiftPatternList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedShiftPatternList.Count == 0)
                                    {
                                        this.CheckedShiftPatternList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedShiftPatternList.Count > 0 &&
                                        this.CheckedShiftPatternList.Where(a => a.AutoID == selectedRecord.AutoID).FirstOrDefault() == null)
                                    {
                                        this.CheckedShiftPatternList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record exist in the selected item collection
                            if (autoID > 0)
                            {
                                ShiftPatternEntity selectedRecord = this.ShiftPatternList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedShiftPatternList.Count > 0
                                        && this.CheckedShiftPatternList.Where(a => a.AutoID == selectedRecord.AutoID).FirstOrDefault() != null)
                                    {
                                        ShiftPatternEntity itemToDelete = this.CheckedShiftPatternList
                                            .Where(a => a.AutoID == selectedRecord.AutoID)
                                            .FirstOrDefault();
                                        if (itemToDelete != null)
                                        {
                                            this.CheckedShiftPatternList.Remove(itemToDelete);
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
            if (this.CheckedShiftPatternList.Count == 0)
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
            if (this.CheckedShiftPatternList == null ||
                this.CheckedShiftPatternList.Count == 0)
                return;

            if (DeleteShiftPatternChanges(this.CheckedShiftPatternList))
            {
                // Refresh data in the grid
                this.btnSearch_Click(this.btnSearch, new EventArgs());
            }
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
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record to delete in the grid.";
                    validator.ToolTip = "Please select the record to delete in the grid.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.CannotViewFireTeamMember)
                {
                    validator.ErrorMessage = "Sorry, you cannot view the shift pattern records of the specified employee because he is a Fire Team Member. Please go to Shift Pattern Changes (Fire Team) page!";
                    validator.ToolTip = "Sorry, you cannot view the shift pattern records of the specified employee because he is a Fire Team Member. Please go to Shift Pattern Changes (Fire Team) page!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoCostCenterPermission)
                {
                    validator.ErrorMessage = "Sorry, you don't have access permission to view the shift pattern information of the specified employee. Please contact ICT or create a Helpdesk request to grant you cost center permission!";
                    validator.ToolTip = "Sorry, you don't have access permission to view the shift pattern information of the specified employee. Please contact ICT or create a Helpdesk request to grant you cost center permission!";
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

        protected void rblOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rblOption.SelectedValue == FilterOption.valEmployee.ToString())
            {
                this.tdEmployee.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.tdFireTeamMember.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdEmployeeTitle.InnerText = "Enter Emp. No.";
                this.tdPageTitle.InnerText = "Shift Pattern Changes (Employee)";
            }
            else if (this.rblOption.SelectedValue == FilterOption.valFireTeamMember.ToString())
            {
                this.tdEmployee.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdFireTeamMember.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.tdEmployeeTitle.InnerText = "Select Employee";
                this.tdPageTitle.InnerText = "Shift Pattern Changes (Fire Team)";
            }
            else
            {
                this.tdEmployee.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdFireTeamMember.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdEmployeeTitle.InnerText = string.Empty;
                this.tdPageTitle.InnerText = "Shift Pattern Changes (Inquiry)";
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;

            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;

            this.cboChangeType.Text = string.Empty;
            this.cboChangeType.SelectedIndex = -1;
            this.cboFireTeamMeber.Text = string.Empty;
            this.cboFireTeamMeber.SelectedIndex = -1;

            this.rblOption.SelectedValue = FilterOption.valEmployee.ToString();
            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridShiftPattern.VirtualItemCount = 1;
            this.gridShiftPattern.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridShiftPattern.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridShiftPattern.PageSize;

            InitializeDataToGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);

            #region Determine Filter Option
            string shiftPatternType = UIHelper.ConvertObjectToString(Request.QueryString["ShiftPatternType"]);
            if (shiftPatternType != string.Empty)
            {
                FilterOption filterOption = FilterOption.valAll;
                try
                {
                    filterOption = (FilterOption)Enum.Parse(typeof(FilterOption), shiftPatternType);
                }
                catch (Exception)
                {
                }
                this.CurrentFilterOption = filterOption;
            }
            #endregion
        }

        public void KillSessions()
        {
            // Cler collections
            this.ShiftPatternList.Clear();
            this.CheckedShiftPatternList.Clear();
            this.FireTeamMemberList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CurrentFilterOption"] = null;
            ViewState["PageTitle"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["SelectedShiftPatternChange"] = null;
            Session.Remove("SelectedShiftPatternChange");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ShiftPatternStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ShiftPatternStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ShiftPatternStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.ShiftPatternStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;

            string filterOption = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["CurrentFilterOption"]);
            if (filterOption != string.Empty)
            {
                FilterOption loadType = FilterOption.valAll;
                try
                {
                    loadType = (FilterOption)Enum.Parse(typeof(FilterOption), filterOption);
                }
                catch (Exception)
                {
                }
                this.CurrentFilterOption = loadType;
            }
            #endregion

            #region Restore session values
            if (this.ShiftPatternStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.ShiftPatternStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.ShiftPatternStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.ShiftPatternStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.ShiftPatternStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.ShiftPatternStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.ShiftPatternStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.ShiftPatternStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.ShiftPatternStorage.ContainsKey("ShiftPatternList"))
                this.ShiftPatternList = this.ShiftPatternStorage["ShiftPatternList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPatternList = null;

            if (this.ShiftPatternStorage.ContainsKey("CheckedShiftPatternList"))
                this.CheckedShiftPatternList = this.ShiftPatternStorage["ShiftPatternList"] as List<ShiftPatternEntity>;
            else
                this.CheckedShiftPatternList = null;

            if (this.ShiftPatternStorage.ContainsKey("FireTeamMemberList"))
                this.FireTeamMemberList = this.ShiftPatternStorage["FireTeamMemberList"] as List<EmployeeDetail>;
            else
                this.FireTeamMemberList = null;

            if (this.ShiftPatternStorage.ContainsKey("PageTitle"))
                this.PageTitle = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["PageTitle"]);
            else
                this.PageTitle = string.Empty;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ShiftPatternStorage.ContainsKey("rblOption"))
                this.rblOption.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["rblOption"]);
            else
                this.rblOption.SelectedValue = FilterOption.valEmployee.ToString();

            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

            if (this.ShiftPatternStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.ShiftPatternStorage.ContainsKey("cboChangeType"))
                this.cboChangeType.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternStorage["cboChangeType"]);
            else
            {
                this.cboChangeType.Text = string.Empty;
                this.cboChangeType.SelectedIndex = -1;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridShiftPattern.CurrentPageIndex = this.CurrentPageIndex;
            this.gridShiftPattern.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridShiftPattern.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridShiftPattern.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ShiftPatternStorage.Clear();
            this.ShiftPatternStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ShiftPatternStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.ShiftPatternStorage.Add("cboChangeType", this.cboChangeType.SelectedValue);
            this.ShiftPatternStorage.Add("cboFireTeamMeber", this.cboFireTeamMeber.SelectedValue);
            this.ShiftPatternStorage.Add("rblOption", this.rblOption.SelectedValue);
            this.ShiftPatternStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.ShiftPatternStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.ShiftPatternStorage.Add("CallerForm", this.CallerForm);
            this.ShiftPatternStorage.Add("ReloadGridData", this.ReloadGridData);
            this.ShiftPatternStorage.Add("CurrentFilterOption", this.CurrentFilterOption);
            #endregion

            #region Store session data to collection
            this.ShiftPatternStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.ShiftPatternStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.ShiftPatternStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.ShiftPatternStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.ShiftPatternStorage.Add("ShiftPatternList", this.ShiftPatternList);
            this.ShiftPatternStorage.Add("CheckedShiftPatternList", this.CheckedShiftPatternList);
            this.ShiftPatternStorage.Add("FireTeamMemberList", this.FireTeamMemberList);
            this.ShiftPatternStorage.Add("PageTitle", this.PageTitle);
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
            FillFireTeamMemberCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetShiftPatternChangeRecords(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables and sessions
                this.ShiftPatternList.Clear();

                int empNo = 0;
                if (this.rblOption.SelectedValue == FilterOption.valEmployee.ToString())
                {
                    empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                    if (empNo.ToString().Length == 4)
                    {
                        empNo += 10000000;

                        // Display Emp. No.
                        this.txtEmpNo.Text = empNo.ToString();
                    }
                }
                else if (this.rblOption.SelectedValue == FilterOption.valFireTeamMember.ToString())
                {
                    empNo = UIHelper.ConvertObjectToInt(this.cboFireTeamMeber.SelectedValue);
                }

                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                string changeType = this.cboChangeType.SelectedValue;
                FilterOption selectedOption = FilterOption.valEmployee;
                try
                {
                    selectedOption = (FilterOption)Enum.Parse(typeof(FilterOption), this.rblOption.SelectedValue);
                }
                catch (Exception)
                {

                }
                #endregion

                #region Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridShiftPattern.VirtualItemCount = 1;
                #endregion

                #region Check if user has permission to the cost center of the specified employee
                if (this.Master.AllowedCostCenterList.Count > 0)
                {
                    string error = string.Empty;
                    string innerError = string.Empty;
                    DALProxy proxy = new DALProxy();
                    EmployeeDetail empInfo = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                    if (empInfo != null)
                    {
                        string allowedCC = this.Master.AllowedCostCenterList
                          .Where(a => a == empInfo.CostCenter)
                          .FirstOrDefault();
                        if (string.IsNullOrEmpty(allowedCC))
                        {
                            this.txtGeneric.Text = ValidationErrorType.NoCostCenterPermission.ToString();
                            this.ErrorType = ValidationErrorType.NoCostCenterPermission;
                            this.cusValSearchButton.Validate();

                            InitializeDataToGrid();
                            return;
                        }
                    }
                }
                #endregion

                #region Check if Fire Team Member
                if (this.CurrentFilterOption == FilterOption.valEmployee &&
                    this.FireTeamMemberList.Count > 0)
                {
                    EmployeeDetail fireTeamMember = this.FireTeamMemberList
                        .Where(a => a.EmpNo == empNo)
                        .FirstOrDefault();
                    if (fireTeamMember != null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.CannotViewFireTeamMember.ToString();
                        this.ErrorType = ValidationErrorType.CannotViewFireTeamMember;
                        this.cusValSearchButton.Validate();

                        InitializeDataToGrid();
                        return;
                    }
                }
                #endregion

                #region Fill data to the collection
                List<ShiftPatternEntity> gridSource = new List<ShiftPatternEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ShiftPatternList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetShiftPatternChanges(0, Convert.ToByte(selectedOption), empNo, changeType, startDate, endDate, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                this.ShiftPatternList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.ShiftPatternList.Count > 0)
                {
                    int totalRecords = this.ShiftPatternList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridShiftPattern.VirtualItemCount = totalRecords;
                    else
                        this.gridShiftPattern.VirtualItemCount = 1;

                    this.gridShiftPattern.DataSource = this.ShiftPatternList;
                    this.gridShiftPattern.DataBind();

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

        private bool DeleteShiftPatternChanges(List<ShiftPatternEntity> recordToDeleteList)
        {
            if (recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteShiftPattern(Convert.ToInt32(UIHelper.SaveType.Delete), recordToDeleteList, ref error, ref innerError);
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

        private void FillFireTeamMemberCombo(bool reloadFromDB)
        {
            try
            {
                List<EmployeeDetail> comboSource = new List<EmployeeDetail>();

                if (this.FireTeamMemberList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.FireTeamMemberList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetFireTeamMember(ref error, ref innerError);
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
                            comboSource.AddRange(rawData);
                        }
                    }
                }

                // Store to session
                this.FireTeamMemberList = comboSource;

                #region Bind data to combobox
                this.cboFireTeamMeber.DataSource = comboSource;
                this.cboFireTeamMeber.DataTextField = "EmpName";
                this.cboFireTeamMeber.DataValueField = "EmpNo";
                this.cboFireTeamMeber.DataBind();
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