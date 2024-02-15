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
    public partial class CostCenterPermissionEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoEmpNo,
            NoCostCenter,
            NoAllowedCostCenter,
            CostCenterAlreadyExist,
            NoRecordToDelete,
            CannotDeleteAllRecords
        }

        private enum TabSelection
        {
            valAllowedCostCenter
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

        private Dictionary<string, object> PermittedCostCenterStorage
        {
            get
            {
                Dictionary<string, object> list = Session["PermittedCostCenterStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["PermittedCostCenterStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["PermittedCostCenterStorage"] = value;
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
                    pageSize = this.gridPermission.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
            }
        }

        private UIHelper.DataLoadTypes CurrentFormLoadType
        {
            get
            {
                UIHelper.DataLoadTypes result = UIHelper.DataLoadTypes.EditExistingRecord;
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

        private CostCenterAccessEntity CurrentPermittedCostCenter
        {
            get
            {
                return Session["SelectedCostCenterAccess"] as CostCenterAccessEntity;
            }
            set
            {
                Session["SelectedCostCenterAccess"] = value;
            }
        }

        private int ParamEmpNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["ParamEmpNo"]);
            }
            set
            {
                ViewState["ParamEmpNo"] = value;
            }
        }

        private int SearchedEmpNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["SearchedEmpNo"]);
            }
            set
            {
                ViewState["SearchedEmpNo"] = value;
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

        private bool ParamCopyMode
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["ParamCopyMode"]);
            }
            set
            {
                ViewState["ParamCopyMode"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.CCSETUPENT.ToString());

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
                this.btnSave.Enabled = this.Master.IsCreateAllowed;
                this.btnDelete.Enabled = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSave.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.PermittedCostCenterStorage.Count > 0)
                {
                    if (this.PermittedCostCenterStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.PermittedCostCenterStorage["FormFlag"]);
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

                        // Save Employee No. to session
                        this.SearchedEmpNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("PermittedCostCenterStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    if (this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord)
                    {
                        this.txtEmpNo.ReadOnly = false;
                        this.btnFindEmployee.Enabled = true;
                    }
                    else
                    {
                        this.txtEmpNo.ReadOnly = true;
                        this.btnFindEmployee.Enabled = false;
                    }
                    #endregion

                    if (this.CurrentPermittedCostCenter != null)
                    {
                        if (!this.ParamCopyMode)
                        {
                            // Get employee details
                            this.txtEmpNo.Value = this.CurrentPermittedCostCenter.EmpNo;
                            this.litEmpName.Text = this.CurrentPermittedCostCenter.EmpName;
                        }
                        else
                        {
                            // Set focus to Employee No.
                            this.txtEmpNo.Focus();
                        }
                        
                        // Fill data in the grid
                        GetPermittedCostCenterList(this.CurrentPermittedCostCenter.EmpNo);
                    }
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events                
        protected void gridPermission_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridPermission_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridPermission_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.PermittedCostCenterList.Count > 0)
            {
                this.gridPermission.DataSource = this.PermittedCostCenterList;
                this.gridPermission.DataBind();

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
                        sortExpr.SortOrder = this.gridPermission.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridPermission.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridPermission_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridPermission_ItemDataBound(object sender, GridItemEventArgs e)
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
            if (this.PermittedCostCenterList.Count > 0)
            {
                this.gridPermission.DataSource = this.PermittedCostCenterList;
                this.gridPermission.DataBind();
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridPermission.DataSource = new List<CostCenterAccessEntity>();
            this.gridPermission.DataBind();
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            //if (this.CurrentFormLoadType == UIHelper.DataLoadTypes.EditExistingRecord)
            //{
            //    #region Clear the form
            //    this.cboCostCenter.SelectedIndex = -1;
            //    this.cboCostCenter.Text = string.Empty;

            //    // Clear collections
            //    this.PermittedCostCenterList.Clear();
            //    this.CheckedCostCenterList.Clear();

            //    // Clear sessions
            //    ViewState["CustomErrorMsg"] = null;
            //    ViewState["CurrentStartRowIndex"] = null;
            //    ViewState["CurrentMaximumRows"] = null;
            //    ViewState["CurrentPageIndex"] = null;
            //    ViewState["CurrentPageSize"] = null;
            //    ViewState["SearchedEmpNo"] = null;

            //    // Reset the grid
            //    this.gridPermission.CurrentPageIndex = 0;
            //    this.CurrentPageIndex = this.gridPermission.CurrentPageIndex + 1;
            //    this.CurrentPageSize = this.gridPermission.PageSize;
            //    #endregion

            //    // Select the default tab
            //    RadTab defaultTab = this.tabMain.Tabs.Where(a => a.Value == TabSelection.valAllowedCostCenter.ToString()).FirstOrDefault();
            //    if (defaultTab != null)
            //    {
            //        this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(defaultTab);
            //        this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(defaultTab);
            //        this.tabMain_TabClick(this.tabMain, new RadTabStripEventArgs(defaultTab));
            //    }
            //}
            //else
            //{
                #region Clear the form
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = "Not defined";
                this.cboCostCenter.SelectedIndex = -1;
                this.cboCostCenter.Text = string.Empty;
                this.txtEmpNo.ReadOnly = false;
                this.txtEmpNo.Focus();

                // Initialize buttons
                this.btnFindEmployee.Enabled = true;
                
                // Clear collections
                this.PermittedCostCenterList.Clear();
                this.CheckedCostCenterList.Clear();

                // Clear sessions
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentStartRowIndex"] = null;
                ViewState["CurrentMaximumRows"] = null;
                ViewState["CurrentPageIndex"] = null;
                ViewState["CurrentPageSize"] = null;
                ViewState["SearchedEmpNo"] = null;
                ViewState["ParamCopyMode"] = null;
                ViewState["ParamEmpNo"] = null;
                this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;

                // Reset the grid
                this.gridPermission.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridPermission.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridPermission.PageSize;

                InitializeDataToGrid();
                #endregion
            //}
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_COST_CENTER_ACCESS_ENTRY
            ),
            false);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerForm))
                Response.Redirect(this.CallerForm, false);
            else
                Response.Redirect(UIHelper.PAGE_COST_CENTER_ACCESS_INQ, false);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);

                #region Perform Data Validation
                // Check Employee No.
                if (empNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                // Check Allowed Cost Centers
                if (this.PermittedCostCenterList.Count == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoAllowedCostCenter.ToString();
                    this.ErrorType = ValidationErrorType.NoAllowedCostCenter;
                    this.cusValButton.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                #region Update the Employee No. if copy mode is true
                if (ParamCopyMode)
                {
                    foreach (CostCenterAccessEntity item in this.PermittedCostCenterList)
                    {
                        item.EmpNo = empNo;
                        item.EmpName = this.litEmpName.Text.Trim();
                        item.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        item.CreatedDate = DateTime.Now;
                    }
                }
                #endregion

                SaveChanges(empNo, this.PermittedCostCenterList);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // Reset collection
            this.CheckedCostCenterList.Clear();

            #region Delete all checked items in the grid
            GridDataItemCollection gridData = this.gridPermission.MasterTableView.Items;
            if (gridData.Count > 0)
            {
                foreach (GridDataItem item in gridData)
                {
                    System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
                    int permitID = UIHelper.ConvertObjectToInt(this.gridPermission.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("PermitID"));

                    if (chkSelectColumn != null)
                    {
                        if (chkSelectColumn.Checked)
                        {
                            if (this.PermittedCostCenterList.Count > 0 && permitID > 0)
                            {
                                CostCenterAccessEntity selectedRecord = this.PermittedCostCenterList
                                    .Where(a => a.PermitID == permitID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    // Check if item already exist in the collection
                                    if (this.CheckedCostCenterList.Count == 0)
                                    {
                                        this.CheckedCostCenterList.Add(selectedRecord);
                                    }
                                    else if (this.CheckedCostCenterList.Count > 0 &&
                                        this.CheckedCostCenterList.Where(a => a.PermitID == selectedRecord.PermitID).FirstOrDefault() == null)
                                    {
                                        this.CheckedCostCenterList.Add(selectedRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Check if record exist in the selected item collection
                            if (permitID > 0)
                            {
                                CostCenterAccessEntity selectedRecord = this.PermittedCostCenterList
                                    .Where(a => a.PermitID == permitID)
                                    .FirstOrDefault();
                                if (selectedRecord != null)
                                {
                                    if (this.CheckedCostCenterList.Count > 0
                                        && this.CheckedCostCenterList.Where(a => a.PermitID == selectedRecord.PermitID).FirstOrDefault() != null)
                                    {
                                        CostCenterAccessEntity itemToDelete = this.CheckedCostCenterList
                                            .Where(a => a.PermitID == selectedRecord.PermitID)
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
                // Check if the number of records to delete equal to the total count of allowed cost center list
                //if (this.CheckedCostCenterList.Count == this.PermittedCostCenterList.Count)
                //{
                //    this.txtGeneric.Text = ValidationErrorType.CannotDeleteAllRecords.ToString();
                //    this.ErrorType = ValidationErrorType.CannotDeleteAllRecords;
                //    this.cusValButton.Validate();
                //}
                //else
                //{
                    StringBuilder script = new StringBuilder();
                    script.Append("ConfirmButtonAction('");
                    script.Append(string.Concat(this.btnDeleteDummy.ClientID, "','"));
                    script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                    script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Delete Confirmation", script.ToString(), true);
                //}
            }
            #endregion
        }

        protected void btnDeleteDummy_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.CheckedCostCenterList == null ||
                this.CheckedCostCenterList.Count == 0)
                    return;

                #region Remove selected items in the collection   
                //foreach (CostCenterAccessEntity item in this.CheckedCostCenterList)
                //{
                //    CostCenterAccessEntity itemToRemove = this.PermittedCostCenterList
                //        .Where(a => a.PermitID == item.PermitID)
                //        .FirstOrDefault();
                //    if (itemToRemove != null)
                //        this.PermittedCostCenterList.Remove(itemToRemove);
                //}

                //// Refresh the grid 
                //RebindDataToGrid();
                #endregion

                #region Delete records in the database                        
                if (DeleteCostCenterPermission(this.CheckedCostCenterList))
                {
                    // Refresh data in the grid
                    GetPermittedCostCenterList(this.CurrentPermittedCostCenter.EmpNo);
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                // Check selected Cost Center
                if (string.IsNullOrEmpty(this.cboCostCenter.SelectedValue))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoCostCenter.ToString();
                    this.ErrorType = ValidationErrorType.NoCostCenter;
                    this.cusValCostCenter.Validate();
                    errorCount++;
                }
                else
                {
                    // Check if cost center already exist in the collection
                    if (this.PermittedCostCenterList.Count > 0)
                    {
                        CostCenterAccessEntity recordExist = this.PermittedCostCenterList
                            .Where(a => a.CostCenter.Trim() == this.cboCostCenter.SelectedValue)
                            .FirstOrDefault();
                        if (recordExist != null)
                        {
                            this.txtGeneric.Text = ValidationErrorType.CostCenterAlreadyExist.ToString();
                            this.ErrorType = ValidationErrorType.CostCenterAlreadyExist;
                            this.cusValCostCenter.Validate();
                            errorCount++;
                        }
                    }
                }

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                CostCenterAccessEntity newItem = new CostCenterAccessEntity()
                {
                    EmpNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text),
                    CostCenter = this.cboCostCenter.SelectedValue,
                    CostCenterName = this.cboCostCenter.Text,
                    CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                    CreatedDate = DateTime.Now
                };

                // Add new item to the collection
                this.PermittedCostCenterList.Add(newItem);

                // Sort the collection
                this.PermittedCostCenterList = this.PermittedCostCenterList.OrderByDescending(a => a.CreatedDate).ToList();

                // Refresh the grid
                RebindDataToGrid();

                // Reset controls
                this.cboCostCenter.SelectedIndex = -1;
                this.cboCostCenter.Text = string.Empty;
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
                else if (this.ErrorType == ValidationErrorType.NoCostCenter)
                {
                    validator.ErrorMessage = "Cost Center is required.)";
                    validator.ToolTip = "Cost Center is required.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoAllowedCostCenter)
                {
                    validator.ErrorMessage = "Unable to save changes because there are no defined permitted cost centers. Please add atleast 1 allowed cost center in the list!";
                    validator.ToolTip = "Unable to save changes because there are no defined permitted cost centers. Please add atleast 1 allowed cost center in the list!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record(s) you wish to delete in the grid!";
                    validator.ToolTip = "Please select the record(s)you wish to delete in the grid!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.CostCenterAlreadyExist)
                {
                    validator.ErrorMessage = "The selected cost center already exists in the list. Please choose another one!";
                    validator.ToolTip = "The selected cost center already exists in the list. Please choose another one!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.CannotDeleteAllRecords)
                {
                    validator.ErrorMessage = "You cannot delete all cost center permissions. Please maintain atleast 1 record!";
                    validator.ToolTip = "You cannot delete all cost center permissions. Please maintain atleast 1 record!";
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

        protected void tabMain_TabClick(object sender, RadTabStripEventArgs e)
        {
            RadTab selected = e.Tab;
            if (selected.Value == TabSelection.valAllowedCostCenter.ToString())
            {
                #region Get permission details from the database
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo > 0)
                    GetPermittedCostCenterList(empNo);
                #endregion
            }
        }

        protected void lnkRemove_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkRemove = sender as LinkButton;
                GridDataItem item = lnkRemove.NamingContainer as GridDataItem;
                if (item != null)
                {
                    // Get data key value
                    int permitID = UIHelper.ConvertObjectToInt(this.gridPermission.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("PermitID"));
                    //if (permitID > 0 &&
                    //    this.PermittedCostCenterList.Count > 0)
                    //{
                    //    this.CurrentPermittedCostCenter = this.PermittedCostCenterList
                    //        .Where(a => a.PermitID == permitID)
                    //        .FirstOrDefault();
                    //    if (this.CurrentPermittedCostCenter != null)
                    //    {
                    //        this.txtTrainingProviderName.Text = this.CurrentTrainingProvider.TrainingProviderName;
                    //        this.txtDescription.Text = this.CurrentTrainingProvider.TrainingProviderDesc;
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void txtEmpNo_TextChanged(object sender, EventArgs e)
        {
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            if (empNo.ToString().Length == 4)
            {
                empNo += 10000000;

                // Display the formatted Emp. No.
                this.txtEmpNo.Text = empNo.ToString();
            }

            if (empNo > 0)
            {
                #region Get the employee information                
                string error = string.Empty;
                string innerError = string.Empty;
                DALProxy proxy = new DALProxy();

                EmployeeDetail empInfo = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                if (empInfo != null)
                {
                    this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.EmpName);

                    // Save Employee No. to session
                    this.SearchedEmpNo = empNo;
                }
                else
                    this.litEmpName.Text = "Employee not found!";
                #endregion
            }
            else
                this.litEmpName.Text = "Employee not found!";
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.cboCostCenter.SelectedIndex = -1;
            this.cboCostCenter.Text = string.Empty;
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridPermission.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridPermission.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridPermission.PageSize;

            InitializeDataToGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.ParamEmpNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY]);
            this.ParamCopyMode = UIHelper.ConvertObjectToBolean(Request.QueryString["CopyMode"]);

            #region Determine the Form Data Load Type
            string formLoadType = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_FORM_LOAD_TYPE]);
            if (formLoadType != string.Empty)
            {
                UIHelper.DataLoadTypes loadType = UIHelper.DataLoadTypes.EditExistingRecord;
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
            // Cler collections
            this.PermittedCostCenterList.Clear();
            this.CheckedCostCenterList.Clear();
            this.CostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["SearchedEmpNo"] = null;
            ViewState["ParamEmpNo"] = null;
            ViewState["ParamCopyMode"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.PermittedCostCenterStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.PermittedCostCenterStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.PermittedCostCenterStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.PermittedCostCenterStorage.ContainsKey("ParamEmpNo"))
                this.ParamEmpNo = UIHelper.ConvertObjectToInt(this.PermittedCostCenterStorage["ParamEmpNo"]);
            else
                this.ParamEmpNo = 0;

            if (this.PermittedCostCenterStorage.ContainsKey("ParamCopyMode"))
                this.ParamCopyMode = UIHelper.ConvertObjectToBolean(this.PermittedCostCenterStorage["ParamCopyMode"]);
            else
                this.ParamCopyMode = false;

            // Determine the Form Load Type
            if (this.PermittedCostCenterStorage.ContainsKey("CurrentFormLoadType"))
            {
                string formLoadType = UIHelper.ConvertObjectToString(this.PermittedCostCenterStorage["CurrentFormLoadType"]);
                if (formLoadType != string.Empty)
                {
                    UIHelper.DataLoadTypes loadType = UIHelper.DataLoadTypes.EditExistingRecord;
                    try
                    {
                        loadType = (UIHelper.DataLoadTypes)Enum.Parse(typeof(UIHelper.DataLoadTypes), formLoadType);
                    }
                    catch (Exception)
                    {
                    }
                    this.CurrentFormLoadType = loadType;
                }
            }
            #endregion

            #region Restore session values
            if (this.PermittedCostCenterStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.PermittedCostCenterStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.PermittedCostCenterStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.PermittedCostCenterStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.PermittedCostCenterStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.PermittedCostCenterStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.PermittedCostCenterStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.PermittedCostCenterStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.PermittedCostCenterStorage.ContainsKey("SearchedEmpNo"))
                this.SearchedEmpNo = UIHelper.ConvertObjectToInt(this.PermittedCostCenterStorage["SearchedEmpNo"]);
            else
                this.SearchedEmpNo = 0;

            if (this.PermittedCostCenterStorage.ContainsKey("PermittedCostCenterList"))
                this.PermittedCostCenterList = this.PermittedCostCenterStorage["PermittedCostCenterList"] as List<CostCenterAccessEntity>;
            else
                this.PermittedCostCenterList = null;

            if (this.PermittedCostCenterStorage.ContainsKey("CheckedCostCenterList"))
                this.CheckedCostCenterList = this.PermittedCostCenterStorage["CheckedCostCenterList"] as List<CostCenterAccessEntity>;
            else
                this.CheckedCostCenterList = null;

            if (this.PermittedCostCenterStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.PermittedCostCenterStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.PermittedCostCenterStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.PermittedCostCenterStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.PermittedCostCenterStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.PermittedCostCenterStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.PermittedCostCenterStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.PermittedCostCenterStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.SelectedIndex = -1;
                this.cboCostCenter.Text = string.Empty;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridPermission.CurrentPageIndex = this.CurrentPageIndex;
            this.gridPermission.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridPermission.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridPermission.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.PermittedCostCenterStorage.Clear();
            this.PermittedCostCenterStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.PermittedCostCenterStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.PermittedCostCenterStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.PermittedCostCenterStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.PermittedCostCenterStorage.Add("CallerForm", this.CallerForm);
            this.PermittedCostCenterStorage.Add("ParamEmpNo", this.ParamEmpNo);
            this.PermittedCostCenterStorage.Add("ParamCopyMode", this.ParamCopyMode);
            #endregion

            #region Store session data to collection
            this.PermittedCostCenterStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.PermittedCostCenterStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.PermittedCostCenterStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.PermittedCostCenterStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.PermittedCostCenterStorage.Add("SearchedEmpNo", this.SearchedEmpNo);
            this.PermittedCostCenterStorage.Add("PermittedCostCenterList", this.PermittedCostCenterList);
            this.PermittedCostCenterStorage.Add("CheckedCostCenterList", this.CheckedCostCenterList);
            this.PermittedCostCenterStorage.Add("CostCenterList", this.CostCenterList);
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
            FillCostCenterCombo(true);
        }
        #endregion

        #region Database Access
        private void FillCostCenterCombo(bool reloadFromDB = true)
        {
            try
            {
                string userCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                List<CostCenterEntity> comboSource = new List<CostCenterEntity>();

                if (this.CostCenterList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.CostCenterList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetCostCenterList(0, ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        comboSource.AddRange(source.ToList());
                    }
                }

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

                #region Bind data to combobox
                this.cboCostCenter.DataSource = comboSource;
                this.cboCostCenter.DataTextField = "CostCenterFullName";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataBind();

                //if (this.cboCostCenter.Items.Count > 0
                //    && !string.IsNullOrEmpty(userCostCenter))
                //{
                //    this.cboCostCenter.SelectedValue = userCostCenter;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
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
                RebindDataToGrid();
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
                    proxy.InsertUpdateDeleteCostCenterPermission(3, item.PermitID, 0, string.Empty, userEmpNo, ref error, ref innerError);
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

        private void SaveChanges(int empNo, List<CostCenterAccessEntity> permissionList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                // Get WCF Instance
                if (permissionList == null)
                    return;

                DALProxy proxy = new DALProxy();
                proxy.InsertAllowedCostCenter(empNo, permissionList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Shift Pattern Changes Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_INQ + "?{0}={1}",
                        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                        true.ToString()
                    ),
                    false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        #endregion                
    }
}