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
    public partial class AssignWorkingCostCenterInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            InvalidDateRange,
            NoRecordToDelete
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

        private Dictionary<string, object> WorkingCostCenterStorage
        {
            get
            {
                Dictionary<string, object> list = Session["WorkingCostCenterStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["WorkingCostCenterStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["WorkingCostCenterStorage"] = value;
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

        private List<EmployeeDetail> WorkingCostCenterList
        {
            get
            {
                List<EmployeeDetail> list = ViewState["WorkingCostCenterList"] as List<EmployeeDetail>;
                if (list == null)
                    ViewState["WorkingCostCenterList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                ViewState["WorkingCostCenterList"] = value;
            }
        }

        private List<EmployeeDetail> CheckedWorkingCostCenterList
        {
            get
            {
                List<EmployeeDetail> list = ViewState["CheckedWorkingCostCenterList"] as List<EmployeeDetail>;
                if (list == null)
                    ViewState["CheckedWorkingCostCenterList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                ViewState["CheckedWorkingCostCenterList"] = value;
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

        private List<UDCEntity> JobCatalogList
        {
            get
            {
                List<UDCEntity> list = ViewState["JobCatalogList"] as List<UDCEntity>;
                if (list == null)
                    ViewState["JobCatalogList"] = list = new List<UDCEntity>();

                return list;
            }
            set
            {
                ViewState["JobCatalogList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.WORKCCINQ.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_WORKING_COSTCENTER_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_WORKING_COSTCENTER_INQUIRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnNew.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.WorkingCostCenterStorage.Count > 0)
                {
                    if (this.WorkingCostCenterStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.WorkingCostCenterStorage["FormFlag"]);
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
                    Session.Remove("WorkingCostCenterStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("WorkingCostCenterStorage");

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
        protected void gridResults_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            GetWorkingCostCenter(true);
        }

        protected void gridResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            GetWorkingCostCenter(true);
        }

        protected void gridResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.WorkingCostCenterList.Count > 0)
            {
                this.gridResults.DataSource = this.WorkingCostCenterList;
                this.gridResults.DataBind();

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
                        sortExpr.SortOrder = this.gridResults.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridResults.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridResults_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    dynamic itemObj = e.CommandSource;
                    string itemText = itemObj.Text;

                    // Initialize variables
                    int empNo = UIHelper.ConvertObjectToInt(item["EmpNo"].Text);
                    string empName = string.Empty;
                    string position = string.Empty;
                    string costCenter = string.Empty;

                    // Get data key value
                    long autoID = UIHelper.ConvertObjectToLong(this.gridResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));

                    // Save session values
                    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region View link is clicked
                        EmployeeDetail selectedRecord = this.WorkingCostCenterList
                               .Where(a => a.AutoID == autoID)
                               .FirstOrDefault();
                        if (selectedRecord != null && autoID > 0)
                        {
                            empName = selectedRecord.EmpName;
                            position = selectedRecord.Position;
                            costCenter = selectedRecord.CostCenterFullName;
                        }

                        Response.Redirect
                       (
                           String.Format(UIHelper.PAGE_WORKING_COSTCENTER_HISTORY + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}",
                           UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                           UIHelper.PAGE_WORKING_COSTCENTER_INQ,
                           "EmpNo",
                           empNo.ToString(),
                           "EmpName",
                           empName,
                           "Position",
                           position,
                           "CostCenter",
                           costCenter
                       ),
                       false);
                        #endregion
                    }
                    else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    {
                        #region Edit link is clicked
                        string formLoadType = Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString();

                        if (this.WorkingCostCenterList.Count > 0)
                        {
                            EmployeeDetail selectedRecord = this.WorkingCostCenterList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null && autoID > 0)
                            {
                                // Save to session
                                Session["SelectedEmployee"] = selectedRecord;
                            }
                        }

                        // Redirect to "Assign Temporary Working Cost Center & Special Job Catalog (Data Entry) " page
                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_WORKING_COSTCENTER_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_WORKING_COSTCENTER_INQ,
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
            else if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                    e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                    e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                    e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
            {
                this.gridResults.AllowPaging = false;
                RebindDataToGrid();

                this.gridResults.ExportSettings.Excel.Format = GridExcelExportFormat.Biff;
                this.gridResults.ExportSettings.IgnorePaging = true;
                this.gridResults.ExportSettings.ExportOnlyData = true;
                this.gridResults.ExportSettings.OpenInNewWindow = true;
                this.gridResults.ExportSettings.UseItemStyles = true;

                this.gridResults.AllowPaging = true;
                this.gridResults.Rebind();
            }
            else if (e.CommandName.Equals(RadGrid.RebindGridCommandName))
            {
                RebindDataToGrid();
            }
        }

        protected void gridResults_ItemDataBound(object sender, GridItemEventArgs e)
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
            if (this.WorkingCostCenterList.Count > 0)
            {
                this.gridResults.DataSource = this.WorkingCostCenterList;
                this.gridResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.WorkingCostCenterList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridResults.DataSource = new List<EmployeeDetail>();
            this.gridResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion
        
        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboJobCatalog.Text = string.Empty;
            this.cboJobCatalog.SelectedIndex = -1;

            // Cler collections
            this.WorkingCostCenterList.Clear();
            this.CheckedWorkingCostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Reset the grid
            this.gridResults.VirtualItemCount = 1;
            this.gridResults.CurrentPageIndex = 0;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // Reset page index
            this.gridResults.CurrentPageIndex = 0;

            GetWorkingCostCenter(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_WORKING_COSTCENTER_INQ
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            // Redirect to data entry page
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_WORKING_COSTCENTER_ENTRY + "?{0}={1}&{2}={3}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_WORKING_COSTCENTER_INQ,
                UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                Convert.ToInt32(UIHelper.DataLoadTypes.CreateNewRecord).ToString()
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
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboJobCatalog.Text = string.Empty;
            this.cboJobCatalog.SelectedIndex = -1;
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
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
            this.WorkingCostCenterList.Clear();
            this.CheckedWorkingCostCenterList.Clear();
            this.JobCatalogList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.WorkingCostCenterStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.WorkingCostCenterStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.WorkingCostCenterStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.WorkingCostCenterStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.WorkingCostCenterStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.WorkingCostCenterStorage.ContainsKey("WorkingCostCenterList"))
                this.WorkingCostCenterList = this.WorkingCostCenterStorage["WorkingCostCenterList"] as List<EmployeeDetail>;
            else
                this.WorkingCostCenterList = null;

            if (this.WorkingCostCenterStorage.ContainsKey("CheckedWorkingCostCenterList"))
                this.CheckedWorkingCostCenterList = this.WorkingCostCenterStorage["WorkingCostCenterList"] as List<EmployeeDetail>;
            else
                this.CheckedWorkingCostCenterList = null;

            if (this.WorkingCostCenterStorage.ContainsKey("JobCatalogList"))
                this.JobCatalogList = this.WorkingCostCenterStorage["JobCatalogList"] as List<UDCEntity>;
            else
                this.JobCatalogList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.WorkingCostCenterStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.WorkingCostCenterStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.WorkingCostCenterStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.WorkingCostCenterStorage.ContainsKey("cboJobCatalog"))
                this.cboJobCatalog.SelectedValue = UIHelper.ConvertObjectToString(this.WorkingCostCenterStorage["cboJobCatalog"]);
            else
            {
                this.cboJobCatalog.Text = string.Empty;
                this.cboJobCatalog.SelectedIndex = -1;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            this.gridResults.CurrentPageIndex = 0;
            this.gridResults.MasterTableView.CurrentPageIndex = 0;
            this.gridResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.WorkingCostCenterStorage.Clear();
            this.WorkingCostCenterStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.WorkingCostCenterStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.WorkingCostCenterStorage.Add("cboJobCatalog", this.cboJobCatalog.SelectedValue);
            this.WorkingCostCenterStorage.Add("cboCostCenter", this.cboCostCenter.Text);
            #endregion

            #region Save Query String values to collection
            this.WorkingCostCenterStorage.Add("CallerForm", this.CallerForm);
            this.WorkingCostCenterStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.WorkingCostCenterStorage.Add("WorkingCostCenterList", this.WorkingCostCenterList);
            this.WorkingCostCenterStorage.Add("CheckedWorkingCostCenterList", this.CheckedWorkingCostCenterList);
            this.WorkingCostCenterStorage.Add("JobCatalogList", this.JobCatalogList);
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
            FillJobCatalogCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetWorkingCostCenter(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables               
                int empNo = 0;
                empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                string costCenter = this.cboCostCenter.Text.Trim();
                string jobCatalog = this.cboJobCatalog.SelectedValue;
                #endregion

                #region Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridResults.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<EmployeeDetail> gridSource = new List<EmployeeDetail>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.WorkingCostCenterList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetWorkingCostCenter(0, empNo, costCenter, jobCatalog, ref error, ref innerError);
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
                this.WorkingCostCenterList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.WorkingCostCenterList.Count > 0)
                {
                    this.gridResults.DataSource = this.WorkingCostCenterList;
                    this.gridResults.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", this.WorkingCostCenterList.Count.ToString("#,###"));
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

        private bool DeleteWorkingCostCenter(List<EmployeeDetail> recordToDeleteList)
        {
            if (recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteWorkingCostCenter(Convert.ToInt32(UIHelper.SaveType.Delete), recordToDeleteList, ref error, ref innerError);
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

        private void FillJobCatalogCombo(bool reloadFromDB)
        {
            try
            {
                List<UDCEntity> comboSource = new List<UDCEntity>();

                if (this.JobCatalogList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.JobCatalogList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetUDCListItem(UIHelper.CONST_SPECIAL_JOB_CATALOG, ref error, ref innerError);
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

                            #region Add blank item
                            comboSource.Insert(0, new UDCEntity()
                            {
                                UDCKey = UIHelper.CONST_SPECIAL_JOB_CATALOG,
                                Code = string.Empty,
                                Description = string.Empty
                            });
                            #endregion
                        }
                    }
                }

                // Store to session
                this.JobCatalogList = comboSource;

                #region Bind data to combobox
                this.cboJobCatalog.DataSource = comboSource;
                this.cboJobCatalog.DataTextField = "Description";
                this.cboJobCatalog.DataValueField = "Code";
                this.cboJobCatalog.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
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
                enableEmpSearch = true;
                #endregion
            }
            else
            {
                #region No filtering for cost center
                //foreach (DataRow rw in source)
                //{
                //    EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                //    row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                //    row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                //    row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                //    row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                //    row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                //    row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                //    row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);

                //    // Add record to the collection
                //    filteredDT.Rows.Add(row);
                //}

                ////Set the flag
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
        #endregion                
    }
}