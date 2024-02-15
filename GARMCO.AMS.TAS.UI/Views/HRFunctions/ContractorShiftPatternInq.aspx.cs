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

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class ContractorShiftPatternInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            InvalidDateJoinedRange,
            InvalidDateResignedRange,
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

        private Dictionary<string, object> ContractorShiftPatInqStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ContractorShiftPatInqStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ContractorShiftPatInqStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ContractorShiftPatInqStorage"] = value;
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

        private List<ContractorEntity> ContractorShiftPatternList
        {
            get
            {
                List<ContractorEntity> list = ViewState["ContractorShiftPatternList"] as List<ContractorEntity>;
                if (list == null)
                    ViewState["ContractorShiftPatternList"] = list = new List<ContractorEntity>();

                return list;
            }
            set
            {
                ViewState["ContractorShiftPatternList"] = value;
            }
        }

        private List<ContractorEntity> CheckedContractorShiftPatList
        {
            get
            {
                List<ContractorEntity> list = ViewState["CheckedContractorShiftPatList"] as List<ContractorEntity>;
                if (list == null)
                    ViewState["CheckedContractorShiftPatList"] = list = new List<ContractorEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedContractorShiftPatList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTSHFINQ.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQUIRY_TITLE), true);
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
                if (this.ContractorShiftPatInqStorage.Count > 0)
                {
                    if (this.ContractorShiftPatInqStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ContractorShiftPatInqStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetContractorInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("ContractorShiftPatInqStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("ContractorShiftPatInqStorage");

                    // Check if need to refresh data in the grid
                    if (this.ReloadGridData)
                    {
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    }
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("ContractorShiftPatInqStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());

                    // Set focus to Contractor No.
                    this.txtEmpNo.Focus();
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

            GetContractorShiftPattern(true);
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            GetContractorShiftPattern(true);
        }

        protected void gridSearchResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ContractorShiftPatternList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.ContractorShiftPatternList;
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
                    #region Open the Contractor Shift Pattern Entry page
                    dynamic itemObj = e.CommandSource;
                    string itemText = itemObj.Text;

                    // Get data key value
                    long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0 && this.ContractorShiftPatternList.Count > 0)
                    {
                        ContractorEntity selectedRecord = this.ContractorShiftPatternList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null && autoID > 0)
                        {
                            // Save to session
                            Session["SelectedContractorShiftPattern"] = selectedRecord;
                        }
                    }

                    if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region Edit link is clicked
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                       (
                           String.Format(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                           UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ,
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
                           String.Format(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                           UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ,
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
            else if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                   e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                   e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                   e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
            {
                this.gridSearchResults.AllowPaging = false;
                RebindDataToGrid();

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
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Enable Edit link based on user permission
                    LinkButton editLink = item["EditLinkButton"].Controls[0] as LinkButton;
                    //if (editLink != null)
                    //{
                    //    string currentUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                    //    string currentUserCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                    //    string lastUpdateUserID = string.Empty;
                    //    string costCenter = UIHelper.ConvertObjectToString(item["CostCenter"].Text);

                    //    Literal litLastUpdateUser = item["LastUpdateUser"].FindControl("litLastUpdateUser") as Literal;
                    //    if (litLastUpdateUser != null)
                    //    {
                    //        int idx = litLastUpdateUser.Text.Trim().LastIndexOf(@"\");
                    //        if (idx > 0)
                    //            lastUpdateUserID = litLastUpdateUser.Text.Trim().Substring(idx + 1);
                    //    }

                    //    // Enable Edit link if the following conditions are met:
                    //    // The value of "LastUpdateUser" field equals to the current logged-on user id
                    //    // The current logged-on user belongs to the same cost center of the affected employee
                    //    // Current user belongs to HR department 
                    //    if ((!string.IsNullOrEmpty(lastUpdateUserID) && lastUpdateUserID == currentUserID) ||
                    //        (costCenter == currentUserCostCenter) ||
                    //        (currentUserCostCenter == "7500"))
                    //    {
                    //        editLink.Enabled = true;
                    //        editLink.ForeColor = System.Drawing.Color.Blue;
                    //    }
                    //    else
                    //    {
                    //        editLink.Enabled = false;
                    //        editLink.ForeColor = System.Drawing.Color.Gray;
                    //    }
                    //}
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.ContractorShiftPatternList.Count > 0)
            {
                int totalRecords = this.ContractorShiftPatternList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridSearchResults.VirtualItemCount = totalRecords;
                else
                    this.gridSearchResults.VirtualItemCount = 1;

                this.gridSearchResults.DataSource = this.ContractorShiftPatternList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<ContractorEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.txtContractorName.Text = string.Empty;
            this.dtpDateJoinedStart.SelectedDate = null;
            this.dtpDateJoinedEnd.SelectedDate = null;
            this.dtpDateResignedStart.SelectedDate = null;
            this.dtpDateResignedEnd.SelectedDate = null;

            // Cler collections
            this.ContractorShiftPatternList.Clear();
            this.CheckedContractorShiftPatList.Clear();

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
            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            #region Check date range
            if (this.dtpDateJoinedStart.SelectedDate != null &&
                this.dtpDateJoinedEnd.SelectedDate != null)
            {
                if (this.dtpDateJoinedStart.SelectedDate > this.dtpDateJoinedEnd.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateJoinedRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateJoinedRange;
                    this.cusValDateJoined.Validate();
                    errorCount++;
                }
            }

            if (this.dtpDateResignedStart.SelectedDate != null &&
               this.dtpDateResignedEnd.SelectedDate != null)
            {
                if (this.dtpDateResignedStart.SelectedDate > this.dtpDateResignedEnd.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateResignedRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateResignedRange;
                    this.cusValDateResigned.Validate();
                    errorCount++;
                }
            }
            #endregion

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

            GetContractorShiftPattern(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetContractorInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            // Redirect to Employee Training Entry page
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ,
                UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString()
            ),
            false);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedContractorShiftPatList.Clear();

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
                            if (this.ContractorShiftPatternList.Count > 0 && autoID > 0)
                            {
                                ContractorEntity selectedRecord = this.ContractorShiftPatternList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedContractorShiftPatList.Count == 0)
                                    {
                                        this.CheckedContractorShiftPatList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedContractorShiftPatList.Count > 0 &&
                                        this.CheckedContractorShiftPatList.Where(a => a.AutoID == selectedRecord.AutoID).FirstOrDefault() == null)
                                    {
                                        this.CheckedContractorShiftPatList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record exist in the selected item collection
                            if (autoID > 0)
                            {
                                ContractorEntity selectedRecord = this.ContractorShiftPatternList
                                    .Where(a => a.AutoID == autoID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedContractorShiftPatList.Count > 0
                                        && this.CheckedContractorShiftPatList.Where(a => a.AutoID == selectedRecord.AutoID).FirstOrDefault() != null)
                                    {
                                        ContractorEntity itemToDelete = this.CheckedContractorShiftPatList
                                            .Where(a => a.AutoID == selectedRecord.AutoID)
                                            .FirstOrDefault();
                                        if (itemToDelete != null)
                                        {
                                            this.CheckedContractorShiftPatList.Remove(itemToDelete);
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
            if (this.CheckedContractorShiftPatList.Count == 0)
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
            if (this.CheckedContractorShiftPatList == null ||
                this.CheckedContractorShiftPatList.Count == 0)
                return;

            if (DeleteContractorShiftPattern(this.CheckedContractorShiftPatList))
            {
                #region Refresh the entity collection then rebind data to the grid                                
                foreach (ContractorEntity item in this.CheckedContractorShiftPatList)
                {
                    ContractorEntity itemToDelete = this.ContractorShiftPatternList
                        .Where(a => a.ContractorNo == item.ContractorNo)
                        .FirstOrDefault();
                    if (itemToDelete != null)
                    {
                        this.ContractorShiftPatternList.RemoveAt(this.ContractorShiftPatternList.IndexOf(itemToDelete));
                    }
                }
                
                // Reset page index
                this.gridSearchResults.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridSearchResults.PageSize;

                GetContractorShiftPattern(false);
                #endregion
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
                else if (this.ErrorType == ValidationErrorType.InvalidDateJoinedRange)
                {
                    validator.ErrorMessage = "The specified Date Joined range is invalid. Make sure that start date is less than the end date.";
                    validator.ToolTip = "The specified Date Joined range is invalid. Make sure that start date is less than the end date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateResignedRange)
                {
                    validator.ErrorMessage = "The specified Date Resigned range is invalid. Make sure that start date is less than the end date.";
                    validator.ToolTip = "The specified Date Resigned range is invalid. Make sure that start date is less than the end date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select from the grid those records you wish to delete in the database!";
                    validator.ToolTip = "Please select from the grid those records you wish to delete in the database!";
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
            this.txtContractorName.Text = string.Empty;
            this.dtpDateJoinedStart.SelectedDate = null;
            this.dtpDateJoinedEnd.SelectedDate = null;
            this.dtpDateResignedStart.SelectedDate = null;
            this.dtpDateResignedEnd.SelectedDate = null;
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
            this.ContractorShiftPatternList.Clear();
            this.CheckedContractorShiftPatList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["SelectedContractorShiftPattern"] = null;
            Session.Remove("SelectedContractorShiftPattern");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ContractorShiftPatInqStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ContractorShiftPatInqStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ContractorShiftPatInqStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ContractorShiftPatInqStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.ContractorShiftPatInqStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.ContractorShiftPatInqStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.ContractorShiftPatInqStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.ContractorShiftPatInqStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.ContractorShiftPatInqStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.ContractorShiftPatInqStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.ContractorShiftPatInqStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.ContractorShiftPatInqStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.ContractorShiftPatInqStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.ContractorShiftPatInqStorage.ContainsKey("ContractorShiftPatternList"))
                this.ContractorShiftPatternList = this.ContractorShiftPatInqStorage["ContractorShiftPatternList"] as List<ContractorEntity>;
            else
                this.ContractorShiftPatternList = null;

            if (this.ContractorShiftPatInqStorage.ContainsKey("CheckedContractorShiftPatList"))
                this.CheckedContractorShiftPatList = this.ContractorShiftPatInqStorage["ContractorShiftPatternList"] as List<ContractorEntity>;
            else
                this.CheckedContractorShiftPatList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ContractorShiftPatInqStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.ContractorShiftPatInqStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.ContractorShiftPatInqStorage.ContainsKey("txtContractorName"))
                this.txtContractorName.Text = UIHelper.ConvertObjectToString(this.ContractorShiftPatInqStorage["txtContractorName"]);
            else
                this.txtContractorName.Text = string.Empty;

            if (this.ContractorShiftPatInqStorage.ContainsKey("dtpDateJoinedStart"))
                this.dtpDateJoinedStart.SelectedDate = UIHelper.ConvertObjectToDate(this.ContractorShiftPatInqStorage["dtpDateJoinedStart"]);
            else
                this.dtpDateJoinedStart.SelectedDate = null;

            if (this.ContractorShiftPatInqStorage.ContainsKey("dtpDateJoinedEnd"))
                this.dtpDateJoinedEnd.SelectedDate = UIHelper.ConvertObjectToDate(this.ContractorShiftPatInqStorage["dtpDateJoinedEnd"]);
            else
                this.dtpDateJoinedEnd.SelectedDate = null;
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
            this.ContractorShiftPatInqStorage.Clear();
            this.ContractorShiftPatInqStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ContractorShiftPatInqStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.ContractorShiftPatInqStorage.Add("txtContractorName", this.txtContractorName.Text.Trim());
            this.ContractorShiftPatInqStorage.Add("dtpDateJoinedStart", this.dtpDateJoinedStart.SelectedDate);
            this.ContractorShiftPatInqStorage.Add("dtpDateJoinedEnd", this.dtpDateJoinedEnd.SelectedDate);
            this.ContractorShiftPatInqStorage.Add("dtpDateResignedStart", this.dtpDateResignedStart.SelectedDate);
            this.ContractorShiftPatInqStorage.Add("dtpDateResignedEnd", this.dtpDateResignedEnd.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.ContractorShiftPatInqStorage.Add("CallerForm", this.CallerForm);
            this.ContractorShiftPatInqStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.ContractorShiftPatInqStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.ContractorShiftPatInqStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.ContractorShiftPatInqStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.ContractorShiftPatInqStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.ContractorShiftPatInqStorage.Add("ContractorShiftPatternList", this.ContractorShiftPatternList);
            this.ContractorShiftPatInqStorage.Add("CheckedContractorShiftPatList", this.CheckedContractorShiftPatList);
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
        private void GetContractorShiftPattern(bool reloadDataFromDB = false)
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
                string contractorName = this.txtContractorName.Text.Trim();
                DateTime? dateJoinedStart = this.dtpDateJoinedStart.SelectedDate;
                DateTime? dateJoinedEnd = this.dtpDateJoinedEnd.SelectedDate;
                DateTime? dateResignedStart = this.dtpDateResignedStart.SelectedDate;
                DateTime? dateResignedEnd = this.dtpDateResignedEnd.SelectedDate;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                #endregion

                #region Fill data to the collection
                List<ContractorEntity> gridSource = new List<ContractorEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ContractorShiftPatternList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    //var source = proxy.GetContractorShiftPattern(autoID, empNo, contractorName, dateJoinedStart, dateJoinedEnd, dateResignedStart, dateResignedEnd, ref error, ref innerError);
                    var source = proxy.GetContractorShiftPatternV2(autoID, empNo, contractorName, dateJoinedStart, dateJoinedEnd, dateResignedStart, dateResignedEnd, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);

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
                this.ContractorShiftPatternList = gridSource;
                #endregion

                // Bind data to the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private bool DeleteContractorShiftPattern(List<ContractorEntity> recordToDeleteList)
        {
            if (recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteContractorShiftPattern(Convert.ToInt32(UIHelper.SaveType.Delete), recordToDeleteList, ref error, ref innerError);
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