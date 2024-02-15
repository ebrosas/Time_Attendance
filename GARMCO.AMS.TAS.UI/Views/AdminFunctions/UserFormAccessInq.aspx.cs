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
    public partial class UserFormAccessInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToSave
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

        private Dictionary<string, object> UserFormAccessStorage
        {
            get
            {
                Dictionary<string, object> list = Session["UserFormAccessStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["UserFormAccessStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["UserFormAccessStorage"] = value;
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

        private List<UserFormAccessEntity> UserFormAccessList
        {
            get
            {
                List<UserFormAccessEntity> list = ViewState["UserFormAccessList"] as List<UserFormAccessEntity>;
                if (list == null)
                    ViewState["UserFormAccessList"] = list = new List<UserFormAccessEntity>();

                return list;
            }
            set
            {
                ViewState["UserFormAccessList"] = value;
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
                    pageSize = this.gridUserFormAccess.MasterTableView.PageSize;

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

        private List<UserFormAccessEntity> ApplicationList
        {
            get
            {
                List<UserFormAccessEntity> list = ViewState["ApplicationList"] as List<UserFormAccessEntity>;
                if (list == null)
                    ViewState["ApplicationList"] = list = new List<UserFormAccessEntity>();

                return list;
            }
            set
            {
                ViewState["ApplicationList"] = value;
            }
        }

        private List<UserFormAccessEntity> FormList
        {
            get
            {
                List<UserFormAccessEntity> list = ViewState["FormList"] as List<UserFormAccessEntity>;
                if (list == null)
                    ViewState["FormList"] = list = new List<UserFormAccessEntity>();

                return list;
            }
            set
            {
                ViewState["FormList"] = value;
            }
        }

        private bool HasViewAccess
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["HasViewAccess"]);
            }
            set
            {
                ViewState["HasViewAccess"] = value;
            }
        }

        private bool HasCreateAccess
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["HasCreateAccess"]);
            }
            set
            {
                ViewState["HasCreateAccess"] = value;
            }
        }

        private bool HasUpdateAccess
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["HasUpdateAccess"]);
            }
            set
            {
                ViewState["HasUpdateAccess"] = value;
            }
        }

        private bool HasDeleteAccess
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["HasDeleteAccess"]);
            }
            set
            {
                ViewState["HasDeleteAccess"] = value;
            }
        }

        private bool HasPrintAccess
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["HasPrintAccess"]);
            }
            set
            {
                ViewState["HasPrintAccess"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.FORMACCESS.ToString());

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
                //FillCostCenterCombo();
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
                this.Master.FormTitle = UIHelper.PAGE_USER_FORM_ACCESS_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_USER_FORM_ACCESS_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnUpdateAll.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.UserFormAccessStorage.Count > 0)
                {
                    if (this.UserFormAccessStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.UserFormAccessStorage["FormFlag"]);
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
                    Session.Remove("UserFormAccessStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("UserFormAccessStorage");

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

                    // Initialize controls
                    this.cboApplication.Enabled = Master.IsTASAdmin;

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());

                    // Set focus to employee no.
                    this.txtEmpNo.Focus();
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridUserFormAccess_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            RebindDataToGrid();
        }

        protected void gridUserFormAccess_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            RebindDataToGrid();
        }

        protected void gridUserFormAccess_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.UserFormAccessList.Count > 0)
            {
                this.gridUserFormAccess.DataSource = this.UserFormAccessList;
                this.gridUserFormAccess.DataBind();

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
                        sortExpr.SortOrder = this.gridUserFormAccess.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridUserFormAccess.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridUserFormAccess_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    // Get data key value
                    string FormCode = UIHelper.ConvertObjectToString(this.gridUserFormAccess.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("FormCode"));
                    //if (empNo > 0 && this.UserFormAccessList.Count > 0)
                    //{
                    //    Session["SelectedCostCenterAccess"] = this.UserFormAccessList
                    //        .Where(a => a.EmpNo == empNo)
                    //        .FirstOrDefault();
                    //}

                    //if (UIHelper.ConvertObjectToString(e.CommandArgument) == "EditButton")
                    //{
                    //    #region Edit button
                    //    // Save session values
                    //    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //    Response.Redirect
                    //    (
                    //        String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //        UIHelper.PAGE_COST_CENTER_ACCESS_INQ,
                    //        UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //        empNo,
                    //        UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //        Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString()
                    //    ),
                    //    false);
                    //    #endregion
                    //}
                    //else if (UIHelper.ConvertObjectToString(e.CommandArgument) == "CopyButton")
                    //{
                    //    #region Copy button
                    //    // Save session values
                    //    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //    Response.Redirect
                    //    (
                    //        String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //        UIHelper.PAGE_COST_CENTER_ACCESS_INQ,                            
                    //        UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //        Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString(),
                    //        "CopyMode",
                    //        true.ToString()
                    //    ),
                    //    false);
                    //    #endregion
                    //}
                }
            }
        }

        protected void gridUserFormAccess_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    bool isDirty = UIHelper.ConvertObjectToBolean(item["IsDirty"].Text);
                    bool formPublic = UIHelper.ConvertObjectToBolean(item["FormPublic"].Text);

                    #region Enable/disable Update link button 
                    LinkButton lnkUpdate = (LinkButton)item["UpdateLink"].FindControl("lnkUpdate");
                    if (lnkUpdate != null)
                    {
                        lnkUpdate.Enabled = isDirty;

                        if (lnkUpdate.Enabled)
                        {
                            lnkUpdate.ForeColor = System.Drawing.Color.Blue;
                            lnkUpdate.Font.Bold = true;
                        }
                        else
                        {
                            lnkUpdate.ForeColor = System.Drawing.Color.Gray;
                            lnkUpdate.Font.Bold = false;
                        }
                    }
                    #endregion

                    #region Process Header columns                                        
                    foreach (GridHeaderItem headerItem in this.gridUserFormAccess.MasterTableView.GetItems(GridItemType.Header))
                    {
                        #region Process "View Access" Header
                        CheckBox chkViewAccessHeader = (CheckBox)headerItem["HasViewAccess"].Controls[1]; // Get the header checkbox 
                        if (chkViewAccessHeader != null)
                        {
                            chkViewAccessHeader.Checked = this.HasViewAccess;
                        }
                        #endregion

                        #region Process "Create Access" Header
                        CheckBox chkCreateAccessHeader = (CheckBox)headerItem["HasCreateAccess"].Controls[1]; // Get the header checkbox 
                        if (chkCreateAccessHeader != null)
                        {
                            chkCreateAccessHeader.Checked = this.HasCreateAccess;
                        }
                        #endregion

                        #region Process "Update Access" Header
                        CheckBox chkUpdateAccessHeader = (CheckBox)headerItem["HasUpdateAccess"].Controls[1]; // Get the header checkbox 
                        if (chkUpdateAccessHeader != null)
                        {
                            chkUpdateAccessHeader.Checked = this.HasUpdateAccess;
                        }
                        #endregion

                        #region Process "Delete Access" Header
                        CheckBox chkDeleteAccessHeader = (CheckBox)headerItem["HasDeleteAccess"].Controls[1]; // Get the header checkbox 
                        if (chkDeleteAccessHeader != null)
                        {
                            chkDeleteAccessHeader.Checked = this.HasDeleteAccess;
                        }
                        #endregion

                        #region Process "Print Access" Header
                        CheckBox chkPrintAccessHeader = (CheckBox)headerItem["HasPrintAccess"].Controls[1]; // Get the header checkbox 
                        if (chkPrintAccessHeader != null)
                        {
                            chkPrintAccessHeader.Checked = this.HasPrintAccess;
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.UserFormAccessList.Count > 0)
            {
                this.gridUserFormAccess.DataSource = this.UserFormAccessList;
                this.gridUserFormAccess.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.UserFormAccessList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridUserFormAccess.DataSource = new List<CostCenterAccessEntity>();
            this.gridUserFormAccess.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = UIHelper.CONST_NOT_DEFINED_TEXT;

            this.cboApplication.SelectedValue = UIHelper.ApplicationCodes.TAS3.ToString();
            this.cboApplication_SelectedIndexChanged(this.cboApplication, new RadComboBoxSelectedIndexChangedEventArgs(this.cboApplication.Text, string.Empty, cboApplication.SelectedValue, string.Empty));

            this.cboFormName.SelectedIndex = -1;
            this.cboFormName.Text = string.Empty;

            // Cler collections
            this.UserFormAccessList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["HasViewAccess"] = null;
            ViewState["HasCreateAccess"] = null;
            ViewState["HasUpdateAccess"] = null;
            ViewState["HasDeleteAccess"] = null;
            ViewState["HasPrintAccess"] = null;

            // Reset the grid
            this.gridUserFormAccess.VirtualItemCount = 1;
            this.gridUserFormAccess.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridUserFormAccess.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridUserFormAccess.PageSize;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());

            // Set focus to employee no.
            this.txtEmpNo.Focus();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Reset session variables
            ViewState["HasViewAccess"] = null;
            ViewState["HasCreateAccess"] = null;
            ViewState["HasUpdateAccess"] = null;
            ViewState["HasDeleteAccess"] = null;
            ViewState["HasPrintAccess"] = null;
            #endregion

            // Reset page index
            this.gridUserFormAccess.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridUserFormAccess.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridUserFormAccess.PageSize;

            GetUserFormAccess(true);
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

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnUpdateAll_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Validation                        
                int errorCount = 0;
                List<UserFormAccessEntity> dirtyUserAccessList = new List<UserFormAccessEntity>();

                if (this.UserFormAccessList.Count > 0)
                {
                    dirtyUserAccessList = this.UserFormAccessList
                        .Where(a => a.IsDirty == true && a.FormPublic == false)
                        .ToList();
                }

                if (dirtyUserAccessList.Count == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoRecordToSave.ToString();
                    this.ErrorType = ValidationErrorType.NoRecordToSave;
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

                #region Loop through each record to determine the permission settings
                foreach (var item in dirtyUserAccessList)
                {
                    if ((item.HasCreateAccess || item.HasUpdateAccess || item.HasDeleteAccess || item.HasPrintAccess) && !item.HasViewAccess)
                        item.HasViewAccess = true;

                    item.UserFrmCRUDP = UIHelper.GetUserFormAccessSetting(item.HasCreateAccess, item.HasViewAccess, item.HasUpdateAccess, item.HasDeleteAccess, item.HasPrintAccess);
                }
                #endregion

                SaveChanges(dirtyUserAccessList);
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
                else if (this.ErrorType == ValidationErrorType.NoRecordToSave)
                {
                    validator.ErrorMessage = "Unable to save because no changes were done to the user's access permission settings.";
                    validator.ToolTip = "Unable to save because no changes were done to the user's access permission settings.";
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

        protected void cboApplication_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            FillDataToFormNameCombo(true, this.cboApplication.SelectedValue);
        }

        protected void chkViewAccessHeader_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkViewAccessHeader = sender as CheckBox;
                if (chkViewAccessHeader != null)
                {
                    // Save to session
                    this.HasViewAccess = chkViewAccessHeader.Checked;

                    if (this.UserFormAccessList.Count > 0)
                    {
                        foreach (UserFormAccessEntity item in this.UserFormAccessList)
                        {
                            item.HasViewAccess = chkViewAccessHeader.Checked;
                            item.IsDirty = true;                            

                            if (string.IsNullOrEmpty(item.UserFrmFormCode))
                            {
                                item.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                item.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.LastUpdatedDate = DateTime.Now;
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
               
        protected void chkCreateAccessHeader_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkCreateAccessHeader = sender as CheckBox;
                if (chkCreateAccessHeader != null)
                {
                    // Save to session
                    this.HasCreateAccess = chkCreateAccessHeader.Checked;

                    if (this.UserFormAccessList.Count > 0)
                    {
                        foreach (UserFormAccessEntity item in this.UserFormAccessList)
                        {
                            item.HasCreateAccess = chkCreateAccessHeader.Checked; ;
                            item.IsDirty = true;

                            if (string.IsNullOrEmpty(item.UserFrmFormCode))
                            {
                                item.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                item.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.LastUpdatedDate = DateTime.Now;
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkUpdateAccessHeader_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkUpdateAccessHeader = sender as CheckBox;
                if (chkUpdateAccessHeader != null)
                {
                    // Save to session
                    this.HasUpdateAccess = chkUpdateAccessHeader.Checked;

                    if (this.UserFormAccessList.Count > 0)
                    {
                        foreach (UserFormAccessEntity item in this.UserFormAccessList)
                        {
                            item.HasUpdateAccess = chkUpdateAccessHeader.Checked;
                            item.IsDirty = true;

                            if (string.IsNullOrEmpty(item.UserFrmFormCode))
                            {
                                item.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                item.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.LastUpdatedDate = DateTime.Now;
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkDeleteAccessHeader_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkDeleteAccessHeader = sender as CheckBox;
                if (chkDeleteAccessHeader != null)
                {
                    // Save to session
                    this.HasDeleteAccess = chkDeleteAccessHeader.Checked;

                    if (this.UserFormAccessList.Count > 0)
                    {
                        foreach (UserFormAccessEntity item in this.UserFormAccessList)
                        {
                            item.HasDeleteAccess = chkDeleteAccessHeader.Checked;
                            item.IsDirty = true;

                            if (string.IsNullOrEmpty(item.UserFrmFormCode))
                            {
                                item.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                item.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.LastUpdatedDate = DateTime.Now;
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkPrintAccessHeader_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkPrintAccessHeader = sender as CheckBox;
                if (chkPrintAccessHeader != null)
                {
                    // Save to session
                    this.HasPrintAccess = chkPrintAccessHeader.Checked;

                    if (this.UserFormAccessList.Count > 0)
                    {
                        foreach (UserFormAccessEntity item in this.UserFormAccessList)
                        {
                            item.HasPrintAccess = chkPrintAccessHeader.Checked;
                            item.IsDirty = true;

                            if (string.IsNullOrEmpty(item.UserFrmFormCode))
                            {
                                item.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                item.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                item.LastUpdatedDate = DateTime.Now;
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkViewAccessItem_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkViewAccessItem = sender as CheckBox;
            if (chkViewAccessItem != null)
            {
                GridDataItem item = chkViewAccessItem.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Get the data key value
                    string formCode = UIHelper.ConvertObjectToString(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("FormCode"));

                    if (!string.IsNullOrEmpty(formCode) &&
                        this.UserFormAccessList.Count > 0)
                    {
                        UserFormAccessEntity selectedRecord = this.UserFormAccessList
                            .Where(a => a.FormCode == formCode)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            selectedRecord.ViewAccessEnable = true;
                            selectedRecord.IsDirty = true;

                            if (string.IsNullOrEmpty(selectedRecord.UserFrmFormCode))
                            {
                                selectedRecord.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                selectedRecord.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.LastUpdatedDate = DateTime.Now;
                            }

                            if (chkViewAccessItem.Checked)
                            {
                                selectedRecord.HasViewAccess = true;
                                selectedRecord.CreateAccessEnable = true;
                                selectedRecord.UpdateAccessEnable = true;
                                selectedRecord.DeleteAccessEnable = true;
                                selectedRecord.PrintAccessEnable = true;
                            }
                            else
                            {
                                selectedRecord.CreateAccessEnable = false;
                                selectedRecord.UpdateAccessEnable = false;
                                selectedRecord.DeleteAccessEnable = false;
                                selectedRecord.PrintAccessEnable = false;

                                selectedRecord.HasViewAccess = false;
                                selectedRecord.HasCreateAccess = false;
                                selectedRecord.HasUpdateAccess = false;
                                selectedRecord.HasDeleteAccess = false;
                                selectedRecord.HasPrintAccess = false;
                            }

                            RebindDataToGrid();
                        }
                    }
                }
            }
        }

        protected void chkCreateAccessItem_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkCreateAccessItem = sender as CheckBox;
            if (chkCreateAccessItem != null)
            {
                GridDataItem item = chkCreateAccessItem.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Get the data key value
                    string formCode = UIHelper.ConvertObjectToString(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("FormCode"));

                    if (!string.IsNullOrEmpty(formCode) &&
                        this.UserFormAccessList.Count > 0)
                    {
                        UserFormAccessEntity selectedRecord = this.UserFormAccessList
                            .Where(a => a.FormCode == formCode)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            selectedRecord.CreateAccessEnable = true;
                            selectedRecord.IsDirty = true;

                            if (string.IsNullOrEmpty(selectedRecord.UserFrmFormCode))
                            {
                                selectedRecord.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                selectedRecord.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.LastUpdatedDate = DateTime.Now;
                            }

                            selectedRecord.HasCreateAccess = chkCreateAccessItem.Checked;
                        }
                    }
                }
            }
        }

        protected void chkUpdateAccessItem_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkUpdateAccessItem = sender as CheckBox;
            if (chkUpdateAccessItem != null)
            {
                GridDataItem item = chkUpdateAccessItem.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Get the data key value
                    string formCode = UIHelper.ConvertObjectToString(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("FormCode"));

                    if (!string.IsNullOrEmpty(formCode) &&
                        this.UserFormAccessList.Count > 0)
                    {
                        UserFormAccessEntity selectedRecord = this.UserFormAccessList
                            .Where(a => a.FormCode == formCode)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            selectedRecord.UpdateAccessEnable = true;
                            selectedRecord.IsDirty = true;

                            if (string.IsNullOrEmpty(selectedRecord.UserFrmFormCode))
                            {
                                selectedRecord.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                selectedRecord.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.LastUpdatedDate = DateTime.Now;
                            }

                            selectedRecord.HasUpdateAccess = chkUpdateAccessItem.Checked;
                        }
                    }
                }
            }
        }

        protected void chkDeleteAccessItem_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkDeleteAccessItem = sender as CheckBox;
            if (chkDeleteAccessItem != null)
            {
                GridDataItem item = chkDeleteAccessItem.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Get the data key value
                    string formCode = UIHelper.ConvertObjectToString(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("FormCode"));

                    if (!string.IsNullOrEmpty(formCode) &&
                        this.UserFormAccessList.Count > 0)
                    {
                        UserFormAccessEntity selectedRecord = this.UserFormAccessList
                            .Where(a => a.FormCode == formCode)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            selectedRecord.DeleteAccessEnable = true;
                            selectedRecord.IsDirty = true;

                            if (string.IsNullOrEmpty(selectedRecord.UserFrmFormCode))
                            {
                                selectedRecord.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                selectedRecord.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.LastUpdatedDate = DateTime.Now;
                            }

                            selectedRecord.HasDeleteAccess = chkDeleteAccessItem.Checked;
                        }
                    }
                }
            }
        }

        protected void chkPrintAccessItem_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkPrintAccessItem = sender as CheckBox;
            if (chkPrintAccessItem != null)
            {
                GridDataItem item = chkPrintAccessItem.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Get the data key value
                    string formCode = UIHelper.ConvertObjectToString(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("FormCode"));

                    if (!string.IsNullOrEmpty(formCode) &&
                        this.UserFormAccessList.Count > 0)
                    {
                        UserFormAccessEntity selectedRecord = this.UserFormAccessList
                            .Where(a => a.FormCode == formCode)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            selectedRecord.PrintAccessEnable = true;
                            selectedRecord.IsDirty = true;

                            if (string.IsNullOrEmpty(selectedRecord.UserFrmFormCode))
                            {
                                selectedRecord.CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                selectedRecord.LastUpdatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                selectedRecord.LastUpdatedDate = DateTime.Now;
                            }

                            selectedRecord.HasPrintAccess = chkPrintAccessItem.Checked;
                        }
                    }
                }
            }
        }

        protected void lnkUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkUpdate = sender as LinkButton;
                GridDataItem item = lnkUpdate.NamingContainer as GridDataItem;
                if (item != null)
                {
                    //// Get data key value
                    //int empNo = UIHelper.ConvertObjectToInt(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("EmpNo"));
                    //if (empNo > 0)
                    //{
                    //    // Show the details panel
                    //    this.panDetails.Style[HtmlTextWriterStyle.Display] = string.Empty;

                    //    // Fetch details from the database
                    //    GetPermittedCostCenterList(empNo);
                    //}
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
            this.litEmpName.Text = UIHelper.CONST_NOT_DEFINED_TEXT;
            this.cboApplication.SelectedIndex = -1;
            this.cboApplication.Text = string.Empty;
            this.cboFormName.SelectedIndex = -1;
            this.cboFormName.Text = string.Empty;
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridUserFormAccess.VirtualItemCount = 1;
            this.gridUserFormAccess.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridUserFormAccess.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridUserFormAccess.PageSize;

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
            this.UserFormAccessList.Clear();
            this.ApplicationList.Clear();
            this.FormList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["HasViewAccess"] = null;
            ViewState["HasCreateAccess"] = null;
            ViewState["HasUpdateAccess"] = null;
            ViewState["HasDeleteAccess"] = null;
            ViewState["HasPrintAccess"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.UserFormAccessStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.UserFormAccessStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.UserFormAccessStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.UserFormAccessStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.UserFormAccessStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.UserFormAccessStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.UserFormAccessStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.UserFormAccessStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.UserFormAccessStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.UserFormAccessStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.UserFormAccessStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.UserFormAccessStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.UserFormAccessStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.UserFormAccessStorage.ContainsKey("UserFormAccessList"))
                this.UserFormAccessList = this.UserFormAccessStorage["UserFormAccessList"] as List<UserFormAccessEntity>;
            else
                this.UserFormAccessList = null;

            if (this.UserFormAccessStorage.ContainsKey("ApplicationList"))
                this.ApplicationList = this.UserFormAccessStorage["ApplicationList"] as List<UserFormAccessEntity>;
            else
                this.ApplicationList = null;

            if (this.UserFormAccessStorage.ContainsKey("FormList"))
                this.FormList = this.UserFormAccessStorage["FormList"] as List<UserFormAccessEntity>;
            else
                this.FormList = null;

            if (this.UserFormAccessStorage.ContainsKey("HasViewAccess"))
                this.HasViewAccess = UIHelper.ConvertObjectToBolean(this.UserFormAccessStorage["HasViewAccess"]);
            else
                this.HasViewAccess = false;

            if (this.UserFormAccessStorage.ContainsKey("HasCreateAccess"))
                this.HasCreateAccess = UIHelper.ConvertObjectToBolean(this.UserFormAccessStorage["HasCreateAccess"]);
            else
                this.HasCreateAccess = false;

            if (this.UserFormAccessStorage.ContainsKey("HasUpdateAccess"))
                this.HasUpdateAccess = UIHelper.ConvertObjectToBolean(this.UserFormAccessStorage["HasUpdateAccess"]);
            else
                this.HasUpdateAccess = false;

            if (this.UserFormAccessStorage.ContainsKey("HasDeleteAccess"))
                this.HasDeleteAccess = UIHelper.ConvertObjectToBolean(this.UserFormAccessStorage["HasDeleteAccess"]);
            else
                this.HasDeleteAccess = false;

            if (this.UserFormAccessStorage.ContainsKey("HasPrintAccess"))
                this.HasPrintAccess = UIHelper.ConvertObjectToBolean(this.UserFormAccessStorage["HasPrintAccess"]);
            else
                this.HasPrintAccess = false;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.UserFormAccessStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.UserFormAccessStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.UserFormAccessStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.UserFormAccessStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.UserFormAccessStorage.ContainsKey("cboApplication"))
                this.cboApplication.SelectedValue = UIHelper.ConvertObjectToString(this.UserFormAccessStorage["cboApplication"]);
            else
            {
                this.cboApplication.Text = string.Empty;
                this.cboApplication.SelectedIndex = -1;
            }

            if (this.UserFormAccessStorage.ContainsKey("cboFormName"))
                this.cboFormName.SelectedValue = UIHelper.ConvertObjectToString(this.UserFormAccessStorage["cboFormName"]);
            else
            {
                this.cboFormName.Text = string.Empty;
                this.cboFormName.SelectedIndex = -1;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridUserFormAccess.CurrentPageIndex = this.CurrentPageIndex;
            this.gridUserFormAccess.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridUserFormAccess.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridUserFormAccess.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.UserFormAccessStorage.Clear();
            this.UserFormAccessStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.UserFormAccessStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.UserFormAccessStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.UserFormAccessStorage.Add("cboApplication", this.cboApplication.SelectedValue);
            this.UserFormAccessStorage.Add("cboFormName", this.cboFormName.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.UserFormAccessStorage.Add("CallerForm", this.CallerForm);
            this.UserFormAccessStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.UserFormAccessStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.UserFormAccessStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.UserFormAccessStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.UserFormAccessStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.UserFormAccessStorage.Add("UserFormAccessList", this.UserFormAccessList);
            this.UserFormAccessStorage.Add("ApplicationList", this.ApplicationList);
            this.UserFormAccessStorage.Add("FormList", this.FormList);
            this.UserFormAccessStorage.Add("HasViewAccess", this.HasViewAccess);
            this.UserFormAccessStorage.Add("HasCreateAccess", this.HasCreateAccess);
            this.UserFormAccessStorage.Add("HasUpdateAccess", this.HasUpdateAccess);
            this.UserFormAccessStorage.Add("HasDeleteAccess", this.HasDeleteAccess);
            this.UserFormAccessStorage.Add("HasPrintAccess", this.HasPrintAccess);
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
            FillDataToApplicationCombo(reloadFromDB, UIHelper.ApplicationCodes.TAS3.ToString());

            if (!string.IsNullOrEmpty(this.cboApplication.SelectedValue))
                FillDataToFormNameCombo(reloadFromDB, this.cboApplication.SelectedValue);
            else
                FillDataToFormNameCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetUserFormAccess(bool reloadDataFromDB = false)
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

                string appCode = this.cboApplication.SelectedValue;
                string formCode = this.cboFormName.SelectedValue;
                if (formCode == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    formCode = string.Empty;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                #endregion

                #region Fill data to the collection
                List<UserFormAccessEntity> gridSource = new List<UserFormAccessEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.UserFormAccessList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetUserFormAccess(appCode, empNo, formCode, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || 
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError);
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (rawData != null && rawData.Count() > 0)
                        {
                            gridSource.AddRange(rawData);

                            if (empNo > 0 &&
                                (this.litEmpName.Text == string.Empty || this.litEmpName.Text == UIHelper.CONST_NOT_DEFINED_TEXT))
                            {
                                this.litEmpName.Text = gridSource.FirstOrDefault().EmpName;
                            }
                        }
                    }
                }

                // Store collection to session
                this.UserFormAccessList = gridSource;
                #endregion

                // Fill data in the grid
                RebindDataToGrid();
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
                //this.cboCostCenter.DataTextField = "CostCenter";
                //this.cboCostCenter.DataValueField = "CostCenter";
                //this.cboCostCenter.DataSource = filteredDT;
                //this.cboCostCenter.DataBind();
            }

            // Enable/Disable employee search button 
            this.btnFindEmployee.Enabled = enableEmpSearch;
        }

        private void FillDataToApplicationCombo(bool reloadFromDB, string defaultValue = "")
        {
            try
            {
                List<UserFormAccessEntity> comboSource = new List<UserFormAccessEntity>();

                if (this.ApplicationList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.ApplicationList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetCommonAdminComboData(0, string.Empty, string.Empty, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                    {
                        comboSource.AddRange(rawData.ToList());

                        // Add blank item
                        //comboSource.Insert(0, new UserFormAccessEntity() { ApplicationID = 0, ApplicationCode = UIHelper.CONST_COMBO_EMTYITEM_ID, ApplicationName = string.Empty });
                    }
                }

                // Store to session
                this.ApplicationList = comboSource;

                #region Bind data to combobox
                this.cboApplication.DataSource = comboSource;
                this.cboApplication.DataTextField = "ApplicationName";
                this.cboApplication.DataValueField = "ApplicationCode";
                this.cboApplication.DataBind();

                if (this.cboApplication.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboApplication.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillDataToFormNameCombo(bool reloadFromDB, string appCode = "", string defaultValue = "")
        {
            try
            {
                List<UserFormAccessEntity> comboSource = new List<UserFormAccessEntity>();

                if (this.FormList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.FormList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetCommonAdminComboData(1, appCode, string.Empty, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                    {
                        comboSource.AddRange(rawData.ToList());

                        // Add blank item
                        comboSource.Insert(0, new UserFormAccessEntity() { FormAppID = 0, FormCode = UIHelper.CONST_COMBO_EMTYITEM_ID, FormName = string.Empty });
                    }
                }

                // Store to session
                this.FormList = comboSource;

                #region Bind data to combobox
                this.cboFormName.DataSource = this.FormList;
                this.cboFormName.DataTextField = "FormName";
                this.cboFormName.DataValueField = "FormCode";
                this.cboFormName.DataBind();

                if (this.cboFormName.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboFormName.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void SaveChanges(List<UserFormAccessEntity> dirtyUserAccessList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteUserFormAccess(dirtyUserAccessList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || 
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    UIHelper.DisplayJavaScriptMessage(this, "Changes have been saved successfully!");
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
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