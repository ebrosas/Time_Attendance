﻿using System;
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
using System.IO;
using System.Configuration;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class EmployeeDirectoryInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError
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

        private Dictionary<string, object> EmployeeDirectoryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["EmployeeDirectoryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["EmployeeDirectoryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["EmployeeDirectoryStorage"] = value;
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

        private List<EmployeeDirectoryEntity> EmployeeDirectoryList
        {
            get
            {
                List<EmployeeDirectoryEntity> list = ViewState["EmployeeDirectoryList"] as List<EmployeeDirectoryEntity>;
                if (list == null)
                    ViewState["EmployeeDirectoryList"] = list = new List<EmployeeDirectoryEntity>();

                return list;
            }
            set
            {
                ViewState["EmployeeDirectoryList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.EMPDIRECT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_DIRECTORY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMPLOYEE_DIRECTORY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnNew.Enabled = this.Master.IsCreateAllowed;
                //this.btnDelete.Enabled = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.EmployeeDirectoryStorage.Count > 0)
                {
                    if (this.EmployeeDirectoryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.EmployeeDirectoryStorage["FormFlag"]);
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
                    Session.Remove("EmployeeDirectoryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("EmployeeDirectoryStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    this.chkShowPhoto.Checked = false;
                    this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());
                    #endregion

                    // Fill data to the grid
                    //this.btnSearch_Click(this.btnSearch, new EventArgs());
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
            if (this.EmployeeDirectoryList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.EmployeeDirectoryList;
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
            try
            {
                if (e.CommandName.Equals(RadGrid.SelectCommandName))
                {
                    #region Process View link
                    //GridDataItem item = e.Item as GridDataItem;
                    //if (item != null)
                    //{
                    //    dynamic itemObj = e.CommandSource;
                    //    string itemText = itemObj.Text;

                    //    // Get data key value
                    //    long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    //    if (autoID > 0 && this.EmployeeDirectoryList.Count > 0)
                    //    {
                    //        EmployeeDirectoryEntity selectedRecord = this.EmployeeDirectoryList
                    //            .Where(a => a.AutoID == autoID)
                    //            .FirstOrDefault();
                    //        if (selectedRecord != null && autoID > 0)
                    //        {
                    //            // Save to session
                    //            Session["SelectedEmpShiftPattern"] = selectedRecord;
                    //        }
                    //    }

                    //    if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    //    {
                    //        #region View link is clicked
                    //        // Save session values
                    //        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //        Response.Redirect
                    //       (
                    //           String.Format(UIHelper.PAGE_CURRENT_SHIFT_PATTERN_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //           UIHelper.PAGE_EMPLOYEE_DIRECTORY,
                    //           UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //           autoID,
                    //           UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //           Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString()
                    //       ),
                    //       false);
                    //        #endregion
                    //    }
                    //}
                    #endregion
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
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridSearchResults_PreRender(object sender, EventArgs e)
        {
            try
            {
                #region Show/Hide checkbox selection column
                GridColumn dynamicColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "EmployeeImagePath").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    dynamicColumn.Visible = chkShowPhoto.Checked;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void RebindDataToGrid()
        {
            if (this.EmployeeDirectoryList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.EmployeeDirectoryList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.EmployeeDirectoryList.Count.ToString("#,###"));                                
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<EmployeeDirectoryEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.txtSearchString.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.chkShowPhoto.Checked = false;
            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());

            // Cler collections
            this.EmployeeDirectoryList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Reset the grid
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

            GetEmployeeDirectory(true);

            // Show/hide employee photo
            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());

        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_EMPLOYEE_DIRECTORY
            ),
            false);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                this.gridSearchResults.ExportSettings.Excel.Format = (GridExcelExportFormat)Enum.Parse(typeof(GridExcelExportFormat), "Xlsx");
                this.gridSearchResults.ExportSettings.IgnorePaging = true;
                this.gridSearchResults.ExportSettings.ExportOnlyData = true;
                this.gridSearchResults.ExportSettings.OpenInNewWindow = true;
                this.gridSearchResults.MasterTableView.ExportToExcel();
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

        protected void chkShowPhoto_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkShowPhoto.Checked)
            {
                if (this.EmployeeDirectoryList.Count > 0)
                {
                    ShowEmployeePhoto();

                    // Refresh the grid
                    RebindDataToGrid();                                        
                }
            }

            // Display the Photo column in the grid
            this.gridSearchResults_PreRender(this.gridSearchResults, new EventArgs());
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.txtSearchString.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.chkShowPhoto.Checked = false;
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
        }

        public void KillSessions()
        {
            // Cler collections
            this.EmployeeDirectoryList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.EmployeeDirectoryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.EmployeeDirectoryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.EmployeeDirectoryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.EmployeeDirectoryStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.EmployeeDirectoryStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.EmployeeDirectoryStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.EmployeeDirectoryStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.EmployeeDirectoryStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.EmployeeDirectoryStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.EmployeeDirectoryStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.EmployeeDirectoryStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.EmployeeDirectoryStorage.ContainsKey("EmployeeDirectoryList"))
                this.EmployeeDirectoryList = this.EmployeeDirectoryStorage["EmployeeDirectoryList"] as List<EmployeeDirectoryEntity>;
            else
                this.EmployeeDirectoryList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.EmployeeDirectoryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.EmployeeDirectoryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.EmployeeDirectoryStorage.ContainsKey("txtSearchString"))
                this.txtSearchString.Text = UIHelper.ConvertObjectToString(this.EmployeeDirectoryStorage["txtSearchString"]);
            else
                this.txtSearchString.Text = string.Empty;

            if (this.EmployeeDirectoryStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.EmployeeDirectoryStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.EmployeeDirectoryStorage.ContainsKey("chkShowPhoto"))
                this.chkShowPhoto.Checked = UIHelper.ConvertNumberToBolean(this.EmployeeDirectoryStorage["chkShowPhoto"]);
            else
                this.chkShowPhoto.Checked = false;

            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());
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
            this.EmployeeDirectoryStorage.Clear();
            this.EmployeeDirectoryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.EmployeeDirectoryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.EmployeeDirectoryStorage.Add("txtSearchString", this.txtSearchString.Text.Trim());
            this.EmployeeDirectoryStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.EmployeeDirectoryStorage.Add("chkShowPhoto", this.chkShowPhoto.Checked);
            #endregion

            #region Save Query String values to collection
            this.EmployeeDirectoryStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.EmployeeDirectoryStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.EmployeeDirectoryStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.EmployeeDirectoryStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.EmployeeDirectoryStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.EmployeeDirectoryStorage.Add("EmployeeDirectoryList", this.EmployeeDirectoryList);
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
        private void GetEmployeeDirectory(bool reloadDataFromDB = false)
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
                string searchString = this.txtSearchString.Text.Trim();

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                #endregion

                #region Fill data to the collection
                List<EmployeeDirectoryEntity> gridSource = new List<EmployeeDirectoryEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.EmployeeDirectoryList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetEmployeeDirectory(empNo, costCenter, searchString, ref error, ref innerError);
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
                this.EmployeeDirectoryList = gridSource;
                #endregion

                // Bind data to the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillCostCenterCombo(bool allowFilter = true)
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

            if (allowFilter)
            {
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
                    //enableEmpSearch = true;
                    #endregion
                }
            }
            else
            {
                #region No filtering by cost center
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

        private void ShowEmployeePhoto()
        {
            try
            {
                //string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["ImageRootPath"]);
                string empPhotoFolder = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["EmpPhotoVirtualFolder"]);

                foreach (EmployeeDirectoryEntity item in this.EmployeeDirectoryList)
                {
                    #region Get the employee photo
                    try
                    {
                        bool isPhotoFound = false;
                        //string imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", imageRootPath, item.EmpNo);
                        //string imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", imageRootPath, item.EmpNo);
                        string imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", empPhotoFolder, item.EmpNo);
                        string imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", empPhotoFolder, item.EmpNo);

                        #region Begin searching for bitmap photo                                
                        if (File.Exists(Server.MapPath(imageFullPath_BMP)))
                        {
                            item.EmployeeImagePath = imageFullPath_BMP;
                            isPhotoFound = true;
                        }
                        else
                        {
                            if (item.EmpNo > 10000000)
                            {
                                //imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", imageRootPath, item.EmpNo - 10000000);
                                imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", empPhotoFolder, item.EmpNo - 10000000);
                                if (File.Exists(Server.MapPath(imageFullPath_BMP)))
                                {
                                    item.EmployeeImagePath = imageFullPath_BMP;
                                    isPhotoFound = true;
                                }
                                else
                                {
                                    item.EmployeeImagePath = UIHelper.CONST_NO_EMPLOYEE_PHOTO;
                                    item.EmployeeImageTooltip = UIHelper.CONST_NO_PHOTO_MESSAGE;
                                }
                            }
                            else
                            {
                                item.EmployeeImagePath = UIHelper.CONST_NO_EMPLOYEE_PHOTO;
                                item.EmployeeImageTooltip = UIHelper.CONST_NO_PHOTO_MESSAGE;
                            }
                        }
                        #endregion

                        if (!isPhotoFound)
                        {
                            #region Search for JPEG photo
                            if (File.Exists(Server.MapPath(imageFullPath_JPG)))
                            {
                                item.EmployeeImagePath = imageFullPath_JPG;
                                isPhotoFound = true;
                            }
                            else
                            {
                                if (item.EmpNo > 10000000)
                                {
                                    //imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", imageRootPath, item.EmpNo - 10000000);
                                    imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", empPhotoFolder, item.EmpNo - 10000000);
                                    if (File.Exists(Server.MapPath(imageFullPath_JPG)))
                                    {
                                        item.EmployeeImagePath = imageFullPath_JPG;
                                        isPhotoFound = true;
                                    }
                                    else
                                    {
                                        item.EmployeeImagePath = UIHelper.CONST_NO_EMPLOYEE_PHOTO;
                                        item.EmployeeImageTooltip = UIHelper.CONST_NO_PHOTO_MESSAGE;
                                    }
                                }
                                else
                                {
                                    item.EmployeeImagePath = UIHelper.CONST_NO_EMPLOYEE_PHOTO;
                                    item.EmployeeImageTooltip = UIHelper.CONST_NO_PHOTO_MESSAGE;
                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception)
                    {
                        item.EmployeeImagePath = UIHelper.CONST_NO_EMPLOYEE_PHOTO;
                        item.EmployeeImageTooltip = UIHelper.CONST_NO_PHOTO_MESSAGE;
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
    }
}