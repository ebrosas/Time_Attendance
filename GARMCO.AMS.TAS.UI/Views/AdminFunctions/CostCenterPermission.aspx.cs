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

namespace GARMCO.AMS.TAS.UI.Views.AdminFunctions
{
    public partial class CostCenterPermission : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
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

        private Dictionary<string, object> CostCenterSetupStorage
        {
            get
            {
                Dictionary<string, object> list = Session["CostCenterSetupStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["CostCenterSetupStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["CostCenterSetupStorage"] = value;
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

        private List<CostCenterAccessEntity> CostCenterAccessList
        {
            get
            {
                List<CostCenterAccessEntity> list = ViewState["CostCenterAccessList"] as List<CostCenterAccessEntity>;
                if (list == null)
                    ViewState["CostCenterAccessList"] = list = new List<CostCenterAccessEntity>();

                return list;
            }
            set
            {
                ViewState["CostCenterAccessList"] = value;
            }
        }

        private List<CostCenterAccessEntity> PermittedCostCenterList
        {
            get
            {
                List<CostCenterAccessEntity> list = ViewState["PermittedCostCenterList"] as List<CostCenterAccessEntity>;
                if (list == null)
                    ViewState["PermittedCostCenterList"] = list = new List<CostCenterAccessEntity>();

                return list;
            }
            set
            {
                ViewState["PermittedCostCenterList"] = value;
            }
        }

        private List<CostCenterAccessEntity> CheckedCostCenterList
        {
            get
            {
                List<CostCenterAccessEntity> list = ViewState["CheckedCostCenterList"] as List<CostCenterAccessEntity>;
                if (list == null)
                    ViewState["CheckedCostCenterList"] = list = new List<CostCenterAccessEntity>();

                return list;
            }
            set
            {
                ViewState["CheckedCostCenterList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.CCSECSETUP.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_COST_CENTER_SECURITY_SETUP_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_COST_CENTER_SECURITY_SETUP_TITLE), true);
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
                if (this.CostCenterSetupStorage.Count > 0)
                {
                    if (this.CostCenterSetupStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.CostCenterSetupStorage["FormFlag"]);
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
                    Session.Remove("CostCenterSetupStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("CostCenterSetupStorage");

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
            if (this.CostCenterAccessList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.CostCenterAccessList;
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
                    // Get data key value
                    int empNo = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("EmpNo"));
                    if (empNo > 0 && this.CostCenterAccessList.Count > 0)
                    {
                        Session["SelectedCostCenterAccess"] = this.CostCenterAccessList
                            .Where(a => a.EmpNo == empNo)
                            .FirstOrDefault();
                    }

                    if (UIHelper.ConvertObjectToString(e.CommandArgument) == "EditButton")
                    {
                        #region Edit button
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_COST_CENTER_ACCESS_INQ,
                            UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                            empNo,
                            UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                            Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString()
                        ),
                        false);
                        #endregion
                    }
                    else if (UIHelper.ConvertObjectToString(e.CommandArgument) == "CopyButton")
                    {
                        #region Copy button
                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_COST_CENTER_ACCESS_INQ,                            
                            UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                            Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString(),
                            "CopyMode",
                            true.ToString()
                        ),
                        false);
                        #endregion
                    }
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
                    #region Set Image Button's Tooltip
                    System.Web.UI.WebControls.ImageButton imgEdit = item["EditButton"].Controls[0] as System.Web.UI.WebControls.ImageButton;
                    if (imgEdit != null)
                        imgEdit.ToolTip = "Edit selected record";

                    System.Web.UI.WebControls.ImageButton imgCopy = item["CopyButton"].Controls[0] as System.Web.UI.WebControls.ImageButton;
                    if (imgCopy != null)
                        imgCopy.ToolTip = "Copy permission set to other users";
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.CostCenterAccessList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.CostCenterAccessList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.CostCenterAccessList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<CostCenterAccessEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Details Grid Events                
        protected void gridDetail_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToDetailsGrid();
        }

        protected void gridDetail_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToDetailsGrid();
        }

        protected void gridDetail_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.PermittedCostCenterList.Count > 0)
            {
                this.gridDetail.DataSource = this.PermittedCostCenterList;
                this.gridDetail.DataBind();

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
                        sortExpr.SortOrder = this.gridDetail.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridDetail.Rebind();
            }
            else
                InitializeDataToDetailsGrid();
        }

        protected void gridDetail_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    
                }
            }
        }

        protected void gridDetail_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                   
                }
            }
        }

        private void RebindDataToDetailsGrid()
        {
            if (this.PermittedCostCenterList.Count > 0)
            {
                this.gridDetail.DataSource = this.PermittedCostCenterList;
                this.gridDetail.DataBind();
            }
            else
                InitializeDataToDetailsGrid();
        }

        private void InitializeDataToDetailsGrid()
        {
            this.gridDetail.DataSource = new List<CostCenterAccessEntity>();
            this.gridDetail.DataBind();
        }
        #endregion

        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;

            // Cler collections
            this.CostCenterAccessList.Clear();
            this.CheckedCostCenterList.Clear();
            this.PermittedCostCenterList.Clear();

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
            this.panDetails.Style[HtmlTextWriterStyle.Display] = "none";

            InitializeDataToGrid();
            InitializeDataToDetailsGrid();
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

            // Reset details sections
            this.panDetails.Style[HtmlTextWriterStyle.Display] = "none";
            InitializeDataToDetailsGrid();

            GetCostCenterPermission(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_COST_CENTER_ACCESS_INQ
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            Session["SelectedCostCenterAccess"] = null;
            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            // Redirect to Employee Training Entry page
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_ENTRY + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_COST_CENTER_ACCESS_INQ,
                UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString()
            ),
            false);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedCostCenterList.Clear();

            #region Loop through each record in the grid
            GridDataItemCollection gridData = this.gridSearchResults.MasterTableView.Items;
            if (gridData.Count > 0)
            {
                foreach (GridDataItem item in gridData)
                {
                    System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    int empNo = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("EmpNo"));

                    if (chkSelectColumn != null)
                    {
                        if (chkSelectColumn.Checked)
                        {
                            if (this.CostCenterAccessList.Count > 0 && empNo > 0)
                            {
                                CostCenterAccessEntity selectedRecord = this.CostCenterAccessList
                                    .Where(a => a.EmpNo == empNo)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedCostCenterList.Count == 0)
                                    {
                                        this.CheckedCostCenterList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedCostCenterList.Count > 0 &&
                                        this.CheckedCostCenterList.Where(a => a.EmpNo == selectedRecord.EmpNo).FirstOrDefault() == null)
                                    {
                                        this.CheckedCostCenterList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record exist in the selected item collection
                            if (empNo > 0)
                            {
                                CostCenterAccessEntity selectedRecord = this.CostCenterAccessList
                                    .Where(a => a.EmpNo == empNo)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedCostCenterList.Count > 0
                                        && this.CheckedCostCenterList.Where(a => a.EmpNo == selectedRecord.EmpNo).FirstOrDefault() != null)
                                    {
                                        CostCenterAccessEntity itemToDelete = this.CheckedCostCenterList
                                            .Where(a => a.EmpNo == selectedRecord.EmpNo)
                                            .FirstOrDefault();
                                        if (itemToDelete != null)
                                        {
                                            this.CheckedCostCenterList.Remove(itemToDelete);
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
            if (this.CheckedCostCenterList.Count == 0)
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
            if (this.CheckedCostCenterList == null ||
                this.CheckedCostCenterList.Count == 0)
                return;

            if (DeleteCostCenterPermission(this.CheckedCostCenterList))
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
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record(s) you wish to delete in the grid!";
                    validator.ToolTip = "Please select the record(s)you wish to delete in the grid!";
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

        protected void lnkEmpName_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkEmpName = sender as LinkButton;
                GridDataItem item = lnkEmpName.NamingContainer as GridDataItem;
                if (item != null)
                {
                    //// Initialize variables
                    //int createdByEmpNo = UIHelper.ConvertObjectToInt(item["CreatedByEmpNo"].Text);
                    //int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    //string formLoadType = string.Empty;
                    //if (createdByEmpNo == currentUserEmpNo || this.Master.IsTrainingAdmin)
                    //    formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString();
                    //else
                    //    formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString();

                    //// Get data key value
                    //long trainingRecordID = UIHelper.ConvertObjectToLong(this.gridTraining.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("TrainingRecordID"));
                    //if (this.TrainingRecordList.Count > 0)
                    //{
                    //    TrainingRecordEntity selectedTrainingRecord = this.TrainingRecordList
                    //        .Where(a => a.TrainingRecordID == trainingRecordID)
                    //        .FirstOrDefault();

                    //    if (selectedTrainingRecord != null && trainingRecordID > 0)
                    //    {
                    //        // Save to session
                    //        Session["SelectedTrainingRecord"] = selectedTrainingRecord;
                    //    }
                    //}

                    //// Save session values
                    //StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //// Redirect to Employee Training Entry page
                    //Response.Redirect
                    //(
                    //    String.Format(UIHelper.PAGE_EMPLOYEE_TRAINING_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //    UIHelper.PAGE_EMPLOYEE_TRAINING_INQUIRY,
                    //    UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //    trainingRecordID,
                    //    UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //    formLoadType
                    //),
                    //false);
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void lnkViewDetail_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkViewDetail = sender as LinkButton;
                GridDataItem item = lnkViewDetail.NamingContainer as GridDataItem;
                if (item != null)
                {
                    // Get data key value
                    int empNo = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("EmpNo"));
                    if (empNo > 0)
                    {
                        // Show the details panel
                        this.panDetails.Style[HtmlTextWriterStyle.Display] = string.Empty;

                        // Fetch details from the database
                        GetPermittedCostCenterList(empNo);
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
            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridSearchResults.VirtualItemCount = 1;
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;
            this.panDetails.Style[HtmlTextWriterStyle.Display] = "none";

            InitializeDataToGrid();
            InitializeDataToDetailsGrid();
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
            this.CostCenterAccessList.Clear();
            this.CheckedCostCenterList.Clear();
            this.PermittedCostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["SelectedCostCenterAccess"] = null;
            Session.Remove("SelectedCostCenterAccess");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.CostCenterSetupStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.CostCenterSetupStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.CostCenterSetupStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.CostCenterSetupStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.CostCenterSetupStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.CostCenterSetupStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.CostCenterSetupStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.CostCenterSetupStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.CostCenterSetupStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.CostCenterSetupStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.CostCenterSetupStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.CostCenterSetupStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.CostCenterSetupStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.CostCenterSetupStorage.ContainsKey("CostCenterAccessList"))
                this.CostCenterAccessList = this.CostCenterSetupStorage["CostCenterAccessList"] as List<CostCenterAccessEntity>;
            else
                this.CostCenterAccessList = null;

            if (this.CostCenterSetupStorage.ContainsKey("CheckedCostCenterList"))
                this.CheckedCostCenterList = this.CostCenterSetupStorage["CostCenterAccessList"] as List<CostCenterAccessEntity>;
            else
                this.CheckedCostCenterList = null;

            if (this.CostCenterSetupStorage.ContainsKey("PermittedCostCenterList"))
                this.PermittedCostCenterList = this.CostCenterSetupStorage["PermittedCostCenterList"] as List<CostCenterAccessEntity>;
            else
                this.PermittedCostCenterList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.CostCenterSetupStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.CostCenterSetupStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.CostCenterSetupStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.CostCenterSetupStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
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
            this.CostCenterSetupStorage.Clear();
            this.CostCenterSetupStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.CostCenterSetupStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.CostCenterSetupStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.CostCenterSetupStorage.Add("CallerForm", this.CallerForm);
            this.CostCenterSetupStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.CostCenterSetupStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.CostCenterSetupStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.CostCenterSetupStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.CostCenterSetupStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.CostCenterSetupStorage.Add("CostCenterAccessList", this.CostCenterAccessList);
            this.CostCenterSetupStorage.Add("CheckedCostCenterList", this.CheckedCostCenterList);
            this.CostCenterSetupStorage.Add("PermittedCostCenterList", this.PermittedCostCenterList);
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
        private void GetCostCenterPermission(bool reloadDataFromDB = false)
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

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                #endregion

                #region Fill data to the collection
                List<CostCenterAccessEntity> gridSource = new List<CostCenterAccessEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.CostCenterAccessList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetCostCenterPermission(0, empNo, costCenter, ref error, ref innerError);
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
                this.CostCenterAccessList = gridSource;
                #endregion

                // Fill data in the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetPermittedCostCenterList(int empNo)
        {
            try
            {
                #region Fill data to the collection                
                string error = string.Empty;
                string innerError = string.Empty;

                // Clear collection
                this.PermittedCostCenterList.Clear();

                DALProxy proxy = new DALProxy();
                var source = proxy.GetCostCenterPermission(1, empNo, string.Empty, ref error, ref innerError);
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
                        this.PermittedCostCenterList.AddRange(source);
                    }
                }
                #endregion

                // Fill data to the grid
                RebindDataToDetailsGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private bool DeleteCostCenterPermission(List<CostCenterAccessEntity> recordToDeleteList)
        {
            if (recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;
                        
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                foreach (CostCenterAccessEntity item in recordToDeleteList)
                {
                    error = string.Empty;
                    innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    proxy.InsertUpdateDeleteCostCenterPermission(4, 0, item.EmpNo, string.Empty, userEmpNo, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(error, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
                return false;
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
            }

            // Enable/Disable employee search button 
            this.btnFindEmployee.Enabled = enableEmpSearch;
        }
        #endregion                
    }
}