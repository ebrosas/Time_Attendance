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
    public partial class ManagerList : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoCostCenter
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

        private Dictionary<string, object> CostCenterStorage
        {
            get
            {
                Dictionary<string, object> list = Session["CostCenterStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["CostCenterStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["CostCenterStorage"] = value;
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

        private List<CostCenterEntity> CostCenterManagerList
        {
            get
            {
                List<CostCenterEntity> list = ViewState["CostCenterManagerList"] as List<CostCenterEntity>;
                if (list == null)
                    ViewState["CostCenterManagerList"] = list = new List<CostCenterEntity>();

                return list;
            }
            set
            {
                ViewState["CostCenterManagerList"] = value;
            }
        }

        private List<UserDefinedCodes> CompanyList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["CompanyList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["CompanyList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["CompanyList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.MANGRLIST.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_COSTCENTER_MANAGER_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_COSTCENTER_MANAGER_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnSearch.Enabled = this.Master.IsRetrieveAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.CostCenterStorage.Count > 0)
                {
                    if (this.CostCenterStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.CostCenterStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("CostCenterStorage");
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
        protected void gridManager_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindManagerGrid();
        }

        protected void gridManager_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindManagerGrid();
        }

        protected void gridManager_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.CostCenterManagerList.Count > 0)
            {
                this.gridManager.DataSource = this.CostCenterManagerList;
                this.gridManager.DataBind();

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
                        sortExpr.SortOrder = this.gridManager.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridManager.Rebind();
            }
            else
                InitializeManagerGrid();
        }

        protected void gridManager_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridManager_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindManagerGrid()
        {
            if (this.CostCenterManagerList.Count > 0)
            {
                this.gridManager.DataSource = this.CostCenterManagerList;
                this.gridManager.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.CostCenterManagerList.Count.ToString("#,###"));
            }
            else
                InitializeManagerGrid();
        }

        private void InitializeManagerGrid()
        {
            this.gridManager.DataSource = new List<CostCenterEntity>();
            this.gridManager.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.cboCompany.Text = string.Empty;
            this.cboCompany.SelectedIndex = -1;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboCostCenter.Text = string.Empty;
            #endregion

            // Clear sessions and collections
            KillSessions();

            // Reset datagrid and other controls
            InitializeManagerGrid();
            this.gridManager.CurrentPageIndex = 0;
            this.lblRecordCount.Text = "0 record found";
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
            this.CostCenterManagerList.Clear();
            this.CompanyList.Clear();
            this.CostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.CostCenterStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.CostCenterStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.CostCenterStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.CostCenterStorage.ContainsKey("CostCenterManagerList"))
                this.CostCenterManagerList = this.CostCenterStorage["CostCenterManagerList"] as List<CostCenterEntity>;
            else
                this.CostCenterManagerList = null;

            if (this.CostCenterStorage.ContainsKey("CompanyList"))
                this.CompanyList = this.CostCenterStorage["CompanyList"] as List<UserDefinedCodes>;
            else
                this.CompanyList = null;

            if (this.CostCenterStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.CostCenterStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.CostCenterStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.Text = UIHelper.ConvertObjectToString(this.CostCenterStorage["cboCostCenter"]);
            else
                this.cboCostCenter.Text = string.Empty;

            if (this.CostCenterStorage.ContainsKey("cboCompany"))
                this.cboCompany.Text = UIHelper.ConvertObjectToString(this.CostCenterStorage["cboCompany"]);
            else
                this.cboCompany.Text = string.Empty;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.CostCenterStorage.Clear();
            this.CostCenterStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.CostCenterStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.CostCenterStorage.Add("cboCompany", this.cboCompany.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.CostCenterStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.CostCenterStorage.Add("CostCenterManagerList", this.CostCenterManagerList);
            this.CostCenterStorage.Add("CompanyList", this.CompanyList);
            this.CostCenterStorage.Add("CostCenterList", this.CostCenterList);
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
            FillCostCenterList(reloadFromDB);
            FillCompanyCombo(reloadFromDB, UIHelper.UDCSorterColumn.UDCCode, "00100");
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset controls
            this.cboCompany.Text = string.Empty;
            this.cboCompany.SelectedIndex = -1;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboCostCenter.Text = string.Empty;
            #endregion

            // Clear sessions and collections
            this.CostCenterManagerList.Clear();

            // Reset datagrid and other controls
            InitializeManagerGrid();
            this.gridManager.CurrentPageIndex = 0;
            this.lblRecordCount.Text = "0 record found";

            this.cboCostCenter.Focus();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string costCenter = this.cboCostCenter.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID ? string.Empty : this.cboCostCenter.SelectedValue;
                string companyCode = this.cboCompany.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID ? string.Empty : this.cboCompany.SelectedValue;

                GetManagerInfo(costCenter, companyCode);
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

        protected void cboCompany_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            string companyCode = this.cboCompany.SelectedValue;
            List<CostCenterEntity> filteredCostCenterList = new List<CostCenterEntity>();

            if (this.CostCenterList.Count > 0)
            {
                if (!string.IsNullOrEmpty(companyCode) &&
                    companyCode != UIHelper.CONST_COMBO_EMTYITEM_ID)
                {
                    filteredCostCenterList = this.CostCenterList
                        .Where(a => a.CompanyCode == companyCode)
                        .ToList();
                }
                else
                    filteredCostCenterList.AddRange(this.CostCenterList);
            }

            #region Add blank item
            if (filteredCostCenterList.Count > 0)
            {
                filteredCostCenterList.Insert(0, new CostCenterEntity()
                {
                    CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                    CostCenterName = string.Empty,
                    CostCenterFullName = string.Empty
                });
            }
            #endregion

            #region Rebind data to combobox            
            this.cboCostCenter.DataSource = filteredCostCenterList;
            this.cboCostCenter.DataTextField = "CostCenterFullName";
            this.cboCostCenter.DataValueField = "CostCenter";
            this.cboCostCenter.DataBind();

            // Clear the selection
            this.cboCostCenter.SelectedIndex = -1;
            this.cboCostCenter.Text = string.Empty;
            #endregion

            // Refresh data in the grid
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }
        #endregion

        #region Database Access
        private void GetManagerInfo(string costCenter, string companyCode)
        {
            try
            {
                // Initialize session
                this.CostCenterManagerList = null;

                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetCostCenterManagerInfo(costCenter, companyCode, ref error, ref innerError);
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
                        // Save to session
                        this.CostCenterManagerList.AddRange(rawData.ToList());                                                
                    }
                }

                // Bind data to the grid
                RebindManagerGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillCostCenterComboOld()
        {
            //DataView dv = this.objCostCenter.Select() as DataView;
            //if (dv == null || dv.Count == 0)
            //    return;

            //DataRow[] source = new DataRow[dv.Count];
            //dv.Table.Rows.CopyTo(source, 0);
            //EmployeeDAL.CostCenterDataTable filteredDT = new EmployeeDAL.CostCenterDataTable();
            //bool enableEmpSearch = false;

            //#region Add default selection item
            //EmployeeDAL.CostCenterRow defaultRow = filteredDT.NewCostCenterRow();
            //defaultRow.CostCenter = String.Empty;
            //defaultRow.CostCenterName = "Please select a Cost Center...";
            //defaultRow.Company = String.Empty;
            //defaultRow.SuperintendentNo = 0;
            //defaultRow.SuperintendentName = String.Empty;
            //defaultRow.ManagerNo = 0;
            //defaultRow.ManagerName = String.Empty;

            //// Add record to the collection
            //filteredDT.Rows.Add(defaultRow);
            //#endregion

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
            //#region No filtering for cost center
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
            //#endregion
            ////}

            //if (filteredDT.Rows.Count > 0)
            //{
            //    this.cboCostCenter.DataTextField = "CostCenter";
            //    this.cboCostCenter.DataValueField = "CostCenter";
            //    this.cboCostCenter.DataSource = filteredDT;
            //    this.cboCostCenter.DataBind();
            //}
        }

        private void FillCompanyCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.CompanyList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.CompanyList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.COMPANY_CODES), ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        rawData.AddRange(source.ToList());
                    }
                }

                #region Sort the list
                if (rawData != null && rawData.Count() > 0)
                {
                    switch (sorter)
                    {
                        case UIHelper.UDCSorterColumn.UDCCode:
                            comboSource.AddRange(rawData.OrderBy(a => a.UDCCode).ToList());
                            break;

                        case UIHelper.UDCSorterColumn.UDCDesc1:
                            comboSource.AddRange(rawData.OrderBy(o => o.UDCDesc1).ToList());
                            break;

                        case UIHelper.UDCSorterColumn.UDCDesc2:
                            comboSource.AddRange(rawData.OrderBy(o => o.UDCDesc2).ToList());
                            break;

                        default:
                            comboSource.AddRange(rawData.ToList());
                            break;
                    }

                    // Add blank item
                    comboSource.Insert(0, new UserDefinedCodes() { UDCDesc1 = string.Empty, UDCCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
                }
                #endregion

                // Store to session
                this.CompanyList = comboSource;

                #region Bind data to combobox
                this.cboCompany.DataSource = comboSource;
                this.cboCompany.DataTextField = "UDCDesc1";
                this.cboCompany.DataValueField = "UDCCode";
                this.cboCompany.DataBind();

                if (this.cboCompany.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboCompany.SelectedValue = defaultValue;
                    if (!string.IsNullOrEmpty(this.cboCompany.SelectedValue))
                    {
                        this.cboCompany_SelectedIndexChanged(this.cboCompany, new RadComboBoxSelectedIndexChangedEventArgs(this.cboCompany.Text, string.Empty, this.cboCompany.SelectedValue, string.Empty));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillCostCenterCombo(bool reloadFromDB = true)
        {
            try
            {
                string userCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                List<CostCenterEntity> comboSource = new List<CostCenterEntity>();
                List<CostCenterEntity> filteredList = new List<CostCenterEntity>();

                if (this.CostCenterList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.CostCenterList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetCostCenterList(Convert.ToByte(UIHelper.CostCenterLoadType.AllWithManagerDefined), ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        comboSource.AddRange(source.ToList());
                    }
                }

                #region Filter cost center based on company code
                //if (!string.IsNullOrEmpty(this.cboCompany.SelectedValue))
                //{
                //    filteredList = comboSource
                //        .Where(a => a.CompanyCode == this.cboCompany.SelectedValue)
                //        .ToList();
                //}
                //else
                    filteredList.AddRange(comboSource.ToList());
                #endregion

                #region Add blank item
                filteredList.Insert(0, new CostCenterEntity()
                {
                    CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                    CostCenterName = string.Empty,
                    CostCenterFullName = string.Empty
                });
                #endregion

                // Store to session
                this.CostCenterList = filteredList;

                #region Bind data to combobox
                this.cboCostCenter.DataSource = filteredList;
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

        private void FillCostCenterList(bool reloadFromDB = true)
        {
            try
            {
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
                    var source = proxy.GetCostCenterList(Convert.ToByte(UIHelper.CostCenterLoadType.AllWithManagerDefined), ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        comboSource.AddRange(source.ToList());
                    }
                }

                #region Add blank item
                //comboSource.Insert(0, new CostCenterEntity()
                //{
                //    CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                //    CostCenterName = string.Empty,
                //    CostCenterFullName = string.Empty
                //});
                #endregion

                // Store to session
                this.CostCenterList = comboSource;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion                
    }
}