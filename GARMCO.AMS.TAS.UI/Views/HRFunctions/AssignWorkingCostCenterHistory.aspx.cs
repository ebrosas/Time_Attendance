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

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class AssignWorkingCostCenterHistory : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoSpecifiedEmpNo,
            NoEmpNo,
            NoRecordToDelete
        }

        private enum TabSelection
        {
            valTimesheetHistory,
            valShiftPatternHistory,
            valAbsenceHistory,
            valLeaveHistory
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

        private Dictionary<string, object> WorkingCCHistoryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["WorkingCCHistoryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["WorkingCCHistoryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["WorkingCCHistoryStorage"] = value;
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

        private List<EmployeeDetail> ChangeHistoryList
        {
            get
            {
                List<EmployeeDetail> list = ViewState["ChangeHistoryList"] as List<EmployeeDetail>;
                if (list == null)
                    ViewState["ChangeHistoryList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                ViewState["ChangeHistoryList"] = value;
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
                    pageSize = this.gridHistory.MasterTableView.PageSize;

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

        private int EmployeeNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["EmployeeNo"]);
            }
            set
            {
                ViewState["EmployeeNo"] = value;
            }
        }

        private string EmployeeName
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["EmployeeName"]);
            }
            set
            {
                ViewState["EmployeeName"] = value;
            }
        }

        private string Position
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["Position"]);
            }
            set
            {
                ViewState["Position"] = value;
            }
        }

        private string CostCenter
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CostCenter"]);
            }
            set
            {
                ViewState["CostCenter"] = value;
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
                this.Master.FormTitle = UIHelper.PAGE_WORKING_COSTCENTER_HISTORY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_WORKING_COSTCENTER_HISTORY_TITLE), true);
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
                if (this.WorkingCCHistoryStorage.Count > 0)
                {
                    if (this.WorkingCCHistoryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["FormFlag"]);
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
                        this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY]));

                        // Save Employee No. to session
                        this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("WorkingCCHistoryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("WorkingCCHistoryStorage");

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

                    #region Initialize controls
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSearch.Visible = false;
                    this.txtEmpNo.ReadOnly = true;

                    this.txtEmpNo.Text = this.EmployeeNo > 0 ? this.EmployeeNo.ToString() : string.Empty;
                    this.litEmpName.Text = this.EmployeeName;
                    this.litPosition.Text = this.Position;
                    this.litCostCenter.Text = this.CostCenter;

                    GetWorkingCostCenterHistory(this.EmployeeNo);
                    #endregion
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region History Grid Events
        protected void gridHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            GetWorkingCostCenterHistory(this.EmployeeNo);
        }

        protected void gridHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            GetWorkingCostCenterHistory(this.EmployeeNo);
        }

        protected void gridHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ChangeHistoryList.Count > 0)
            {
                this.gridHistory.DataSource = this.ChangeHistoryList;
                this.gridHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridHistory.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set font color to red if CorrectionCode is not null
                    //string correctionCode = UIHelper.ConvertObjectToString(item["CorrectionCode"].Text.Replace("&nbsp;", string.Empty));
                    //if (!string.IsNullOrEmpty(correctionCode))
                    //{
                    //    item["CorrectionCode"].ForeColor = System.Drawing.Color.Red;
                    //    item["CorrectionCode"].Font.Bold = true;
                    //}
                    #endregion
                }
            }
        }

        private void RebindDataToGrid()
        {
            if (this.ChangeHistoryList.Count > 0)
            {
                this.gridHistory.DataSource = this.ChangeHistoryList;
                this.gridHistory.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.ChangeHistoryList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridHistory.DataSource = new List<EmployeeDetail>();
            this.gridHistory.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            #region Check selected employee 
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            if (empNo.ToString().Length == 4)
            {
                empNo += 10000000;

                // Display the formatted Emp. No.
                this.txtEmpNo.Text = empNo.ToString();

                if (this.EmployeeNo == 0)
                    this.btnGet_Click(this.btnGet, new EventArgs());
            }

            if (empNo == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                this.ErrorType = ValidationErrorType.NoEmpNo;
                this.cusValEmpNo.Validate();
                errorCount++;

                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
            }
            #endregion

            if (errorCount > 0)
            {
                InitializeDataToGrid();

                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }

            #endregion

            // Reset page index
            this.gridHistory.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridHistory.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridHistory.PageSize;

            GetWorkingCostCenterHistory(empNo);
        }

        protected void btnGet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform data validation
                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSpecifiedEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoSpecifiedEmpNo;
                    this.cusValEmpNo.Validate();
                    return;
                }
                #endregion

                #region Initialize control values and variables
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                #endregion

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                string error = string.Empty;
                string innerError = string.Empty;

                EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
                if (empInfo != null)
                {
                    if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                    {
                        #region Check if cost center exist in the allowed cost center list
                        //if (this.Master.AllowedCostCenterList.Count > 0)
                        //{
                        //    string allowedCC = this.Master.AllowedCostCenterList
                        //        .Where(a => a == UIHelper.ConvertObjectToString(empInfo.CostCenter))
                        //        .FirstOrDefault();
                        //    if (!string.IsNullOrEmpty(allowedCC))
                        //    {
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            empInfo.CostCenter,
                            empInfo.CostCenterName);
                        //    }
                        //    else
                        //    {
                        //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                        //    }
                        //}
                        #endregion
                    }
                    else
                    {
                        #region Get employee info from the employee master
                        DALProxy proxy = new DALProxy();
                        var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                        if (rawData != null)
                        {
                            //if (this.Master.AllowedCostCenterList.Count > 0)
                            //{
                            //    string allowedCC = this.Master.AllowedCostCenterList
                            //        .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
                            //        .FirstOrDefault();
                            //    if (!string.IsNullOrEmpty(allowedCC))
                            //    {
                            this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                            this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                            this.litCostCenter.Text = string.Format("{0} - {1}",
                               rawData.CostCenter,
                               rawData.CostCenterName);
                            //    }
                            //    else
                            //    {
                            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                            //    }
                            //}
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY
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
                Response.Redirect(UIHelper.PAGE_HOME, false);
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
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedEmpNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
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

        protected void tabMain_TabClick(object sender, RadTabStripEventArgs e)
        {
            RadTab selected = e.Tab;
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridHistory.VirtualItemCount = 1;
            this.gridHistory.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridHistory.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridHistory.PageSize;

            InitializeDataToGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);
            this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString["EmpNo"]);
            this.EmployeeName = UIHelper.ConvertObjectToString(Request.QueryString["EmpName"]);
            this.Position = UIHelper.ConvertObjectToString(Request.QueryString["Position"]);
            this.CostCenter = UIHelper.ConvertObjectToString(Request.QueryString["CostCenter"]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.ChangeHistoryList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["EmployeeNo"] = null;
            ViewState["CallerForm"] = null;
            ViewState["EmployeeName"] = null;
            ViewState["Position"] = null;
            ViewState["CostCenter"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.WorkingCCHistoryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.WorkingCCHistoryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.WorkingCCHistoryStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.WorkingCCHistoryStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;

            if (this.WorkingCCHistoryStorage.ContainsKey("EmployeeName"))
                this.EmployeeName = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["EmployeeName"]);
            else
                this.EmployeeName = string.Empty;

            if (this.WorkingCCHistoryStorage.ContainsKey("Position"))
                this.Position = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["Position"]);
            else
                this.Position = string.Empty;

            if (this.WorkingCCHistoryStorage.ContainsKey("CostCenter"))
                this.CostCenter = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["CostCenter"]);
            else
                this.CostCenter = string.Empty;
            #endregion

            #region Restore session values
            if (this.WorkingCCHistoryStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.WorkingCCHistoryStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.WorkingCCHistoryStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.WorkingCCHistoryStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.WorkingCCHistoryStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.WorkingCCHistoryStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.WorkingCCHistoryStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.WorkingCCHistoryStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.WorkingCCHistoryStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.WorkingCCHistoryStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            if (this.WorkingCCHistoryStorage.ContainsKey("ChangeHistoryList"))
                this.ChangeHistoryList = this.WorkingCCHistoryStorage["ChangeHistoryList"] as List<EmployeeDetail>;
            else
                this.ChangeHistoryList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.WorkingCCHistoryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.WorkingCCHistoryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.WorkingCCHistoryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.WorkingCCHistoryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.WorkingCCHistoryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridHistory.CurrentPageIndex = this.CurrentPageIndex;
            this.gridHistory.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridHistory.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridHistory.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.WorkingCCHistoryStorage.Clear();
            this.WorkingCCHistoryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.WorkingCCHistoryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.WorkingCCHistoryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.WorkingCCHistoryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.WorkingCCHistoryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            #endregion

            #region Save Query String values to collection
            this.WorkingCCHistoryStorage.Add("CallerForm", this.CallerForm);
            this.WorkingCCHistoryStorage.Add("ReloadGridData", this.ReloadGridData);
            this.WorkingCCHistoryStorage.Add("EmployeeName", this.EmployeeName);
            this.WorkingCCHistoryStorage.Add("Position", this.Position);
            this.WorkingCCHistoryStorage.Add("CostCenter", this.CostCenter);
            #endregion

            #region Store session data to collection
            this.WorkingCCHistoryStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.WorkingCCHistoryStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.WorkingCCHistoryStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.WorkingCCHistoryStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.WorkingCCHistoryStorage.Add("EmployeeNo", this.EmployeeNo);
            this.WorkingCCHistoryStorage.Add("ChangeHistoryList", this.ChangeHistoryList);
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
        private void GetWorkingCostCenterHistory(int empNo, bool reloadDataFromDB = true)
        {
            try
            {
                // Initialize record count
                this.lblRecordCount.Text = "0 record found";

                #region Fill data to the collection
                List<EmployeeDetail> gridSource = new List<EmployeeDetail>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ChangeHistoryList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetWorkingCostCenterHistory(empNo, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
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
                        }
                    }
                }

                // Store collection to session
                this.ChangeHistoryList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.ChangeHistoryList.Count > 0)
                {
                    this.gridHistory.DataSource = this.ChangeHistoryList;
                    this.gridHistory.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", this.ChangeHistoryList.Count.ToString("#,###"));
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
        #endregion
    }
}