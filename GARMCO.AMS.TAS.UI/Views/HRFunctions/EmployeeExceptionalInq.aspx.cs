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
using GARMCO.Common.Object;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class EmployeeExceptionalInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoSpecifiedEmpNo,
            NoSelectedCriteria,
            NoDatePeriod,
            NoEmployeeNo,
            InvalidDateRange
        }

        private enum FilterOption
        {            
            valAbsence,
            valSickLeave,
            valNPH,
            valInjuryLeave,
            valDIL,
            valOvertime,
            valAll
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

        private Dictionary<string, object> EmpExceptionalStorage
        {
            get
            {
                Dictionary<string, object> list = Session["EmpExceptionalStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["EmpExceptionalStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["EmpExceptionalStorage"] = value;
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

        private List<EmployeeAttendanceEntity> EmpExceptionalList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["EmpExceptionalList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["EmpExceptionalList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["EmpExceptionalList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.EMPEXCPINQ.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_EXCEPTIONAL_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMPLOYEE_EXCEPTIONAL_INQUIRY_TITLE), true);
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
                if (this.EmpExceptionalStorage.Count > 0)
                {
                    if (this.EmpExceptionalStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.EmpExceptionalStorage["FormFlag"]);
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
                    Session.Remove("EmpExceptionalStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("EmpExceptionalStorage");

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
                    this.dtpStartDate.SelectedDate = DateTime.Now.AddMonths(-1);
                    this.dtpEndDate.SelectedDate = DateTime.Now;
                    this.cblOptions.Items[0].Selected = true;
                    this.cblOptions.Items[1].Selected = true;
                    this.cblOptions.Items[2].Selected = true;
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
        protected void gridResults_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.EmpExceptionalList.Count > 0)
            {
                this.gridResults.DataSource = this.EmpExceptionalList;
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
                }
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
            if (this.EmpExceptionalList.Count > 0)
            {
                this.gridResults.DataSource = this.EmpExceptionalList;
                this.gridResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.EmpExceptionalList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridResults.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion
        
        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.cblOptions.ClearSelection();

            // Initialize controls
            //this.dtpStartDate.SelectedDate = DateTime.Now.AddMonths(-1);
            //this.dtpEndDate.SelectedDate = DateTime.Now;

            // Cler collections
            this.EmpExceptionalList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["EmployeeNo"] = null;

            // Reset the grid
            this.gridResults.CurrentPageIndex = 0;
            InitializeDataToGrid();
            #endregion

            // Reload the data
            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check Employee No.
            if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoSpecifiedEmpNo.ToString();
                this.ErrorType = ValidationErrorType.NoSpecifiedEmpNo;
                this.cusValEmpNo.Validate();
                errorCount++;
            }

            // Check selected criteria
            if (string.IsNullOrEmpty(this.cblOptions.SelectedValue))
            {
                this.txtGeneric.Text = ValidationErrorType.NoSelectedCriteria.ToString();
                this.ErrorType = ValidationErrorType.NoSelectedCriteria;
                this.cusValOptions.Validate();
                errorCount++;
            }

            // Check Date Period
            if (this.dtpStartDate.SelectedDate == null &&
                this.dtpEndDate.SelectedDate == null)
            {
                //this.txtGeneric.Text = ValidationErrorType.NoDatePeriod.ToString();
                //this.ErrorType = ValidationErrorType.NoDatePeriod;
                //this.cusValDateFrom.Validate();
                //errorCount++;
            }
            else
            {
                if (this.dtpStartDate.SelectedDate > this.dtpEndDate.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateRange;
                    this.cusValDateFrom.Validate();
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
            this.gridResults.CurrentPageIndex = 0;
            GetEmployeeExceptional();
        }

        protected void btnGet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform data validation
                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEmployeeNo.ToString();
                    this.ErrorType = ValidationErrorType.NoEmployeeNo;
                    this.cusValEmpNo.Validate();
                    return;
                }
                #endregion

                #region Initialize control values and variables
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";

                // Reset session variables
                this.EmployeeNo = 0;
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

                #region Get employee info from the employee master
                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                if (rawData != null)
                {
                    this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                    this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                    this.litCostCenter.Text = string.Format("{0} - {1}", rawData.CostCenter, rawData.CostCenterName);

                    // Save Employee No. to session
                    this.EmployeeNo = empNo;
                }
                #endregion

                #region Old code in fetching employee information     
                //EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
                //if (empInfo != null)
                //{
                //    if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                //    {
                //        #region Check if cost center exist in the allowed cost center list
                //        //if (this.Master.AllowedCostCenterList.Count > 0)
                //        //{
                //        //    string allowedCC = this.Master.AllowedCostCenterList
                //        //        .Where(a => a == UIHelper.ConvertObjectToString(empInfo.CostCenter))
                //        //        .FirstOrDefault();
                //        //    if (!string.IsNullOrEmpty(allowedCC))
                //        //    {
                //        this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
                //        this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
                //        this.litCostCenter.Text = string.Format("{0} - {1}",
                //            empInfo.CostCenter,
                //            empInfo.CostCenterName);
                //        //    }
                //        //    else
                //        //    {
                //        //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                //        //    }
                //        //}
                //        #endregion
                //    }
                //    else
                //    {
                //        #region Get employee info from the employee master
                //        DALProxy proxy = new DALProxy();
                //        var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                //        if (rawData != null)
                //        {
                //            //if (this.Master.AllowedCostCenterList.Count > 0)
                //            //{
                //            //    string allowedCC = this.Master.AllowedCostCenterList
                //            //        .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
                //            //        .FirstOrDefault();
                //            //    if (!string.IsNullOrEmpty(allowedCC))
                //            //    {
                //            this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                //            this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                //            this.litCostCenter.Text = string.Format("{0} - {1}",
                //               rawData.CostCenter,
                //               rawData.CostCenterName);
                //            //    }
                //            //    else
                //            //    {
                //            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                //            //    }
                //            //}
                //        }
                //        #endregion
                //    }
                //}
                #endregion
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
                UIHelper.PAGE_EMPLOYEE_EXCEPTIONAL_INQUIRY
            ),
            false);
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
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSelectedCriteria)
                {
                    validator.ErrorMessage = "Criteria selection is required.";
                    validator.ToolTip = "Criteria selection is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDatePeriod)
                {
                    validator.ErrorMessage = "Date Period is required.";
                    validator.ToolTip = "Date Period is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmployeeNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
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
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.cblOptions.ClearSelection();
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridResults.CurrentPageIndex = 0;
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
            this.EmpExceptionalList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["EmployeeNo"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.EmpExceptionalStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.EmpExceptionalStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.EmpExceptionalStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.EmpExceptionalStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.EmpExceptionalStorage.ContainsKey("EmpExceptionalList"))
                this.EmpExceptionalList = this.EmpExceptionalStorage["EmpExceptionalList"] as List<EmployeeAttendanceEntity>;
            else
                this.EmpExceptionalList = null;

            if (this.EmpExceptionalStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.EmpExceptionalStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.EmpExceptionalStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.EmpExceptionalStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.EmpExceptionalStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.EmpExceptionalStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.EmpExceptionalStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.EmpExceptionalStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.EmpExceptionalStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.EmpExceptionalStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.EmpExceptionalStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpExceptionalStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.EmpExceptionalStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpExceptionalStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.EmpExceptionalStorage.ContainsKey("isAbsence"))
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valAbsence)].Selected = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["isAbsence"]);
            else
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valAbsence)].Selected = false;

            if (this.EmpExceptionalStorage.ContainsKey("isSickLeave"))
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valSickLeave)].Selected = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["isSickLeave"]);
            else
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valSickLeave)].Selected = false;

            if (this.EmpExceptionalStorage.ContainsKey("isNPH"))
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valNPH)].Selected = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["isNPH"]);
            else
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valNPH)].Selected = false;

            if (this.EmpExceptionalStorage.ContainsKey("isInjuryLeave"))
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valInjuryLeave)].Selected = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["isInjuryLeave"]);
            else
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valInjuryLeave)].Selected = false;

            if (this.EmpExceptionalStorage.ContainsKey("isDIL"))
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valDIL)].Selected = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["isDIL"]);
            else
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valDIL)].Selected = false;

            if (this.EmpExceptionalStorage.ContainsKey("isOvertime"))
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valOvertime)].Selected = UIHelper.ConvertObjectToBolean(this.EmpExceptionalStorage["isOvertime"]);
            else
                this.cblOptions.Items[Convert.ToInt32(FilterOption.valOvertime)].Selected = false;
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridResults.CurrentPageIndex = 0;
            this.gridResults.MasterTableView.CurrentPageIndex = 0;
            this.gridResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.EmpExceptionalStorage.Clear();
            this.EmpExceptionalStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.EmpExceptionalStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.EmpExceptionalStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.EmpExceptionalStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.EmpExceptionalStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.EmpExceptionalStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.EmpExceptionalStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            this.EmpExceptionalStorage.Add("isAbsence", this.cblOptions.Items[Convert.ToInt32(FilterOption.valAbsence)].Selected);
            this.EmpExceptionalStorage.Add("isSickLeave", this.cblOptions.Items[Convert.ToInt32(FilterOption.valSickLeave)].Selected);
            this.EmpExceptionalStorage.Add("isNPH", this.cblOptions.Items[Convert.ToInt32(FilterOption.valNPH)].Selected);
            this.EmpExceptionalStorage.Add("isInjuryLeave", this.cblOptions.Items[Convert.ToInt32(FilterOption.valInjuryLeave)].Selected);
            this.EmpExceptionalStorage.Add("isDIL", this.cblOptions.Items[Convert.ToInt32(FilterOption.valDIL)].Selected);
            this.EmpExceptionalStorage.Add("isOvertime", this.cblOptions.Items[Convert.ToInt32(FilterOption.valOvertime)].Selected);
            #endregion

            #region Save Query String values to collection
            this.EmpExceptionalStorage.Add("CallerForm", this.CallerForm);
            this.EmpExceptionalStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.EmpExceptionalStorage.Add("EmpExceptionalList", this.EmpExceptionalList);
            this.EmpExceptionalStorage.Add("EmployeeNo", this.EmployeeNo);
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
        private void GetEmployeeExceptional()
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

                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;
                bool isAbsence = this.cblOptions.Items[Convert.ToInt32(FilterOption.valAbsence)].Selected;
                bool isSickLeave = this.cblOptions.Items[Convert.ToInt32(FilterOption.valSickLeave)].Selected;
                bool isNPH = this.cblOptions.Items[Convert.ToInt32(FilterOption.valNPH)].Selected;
                bool isInjuryLeave = this.cblOptions.Items[Convert.ToInt32(FilterOption.valInjuryLeave)].Selected;
                bool isDIL = this.cblOptions.Items[Convert.ToInt32(FilterOption.valDIL)].Selected;
                bool isOvertime = this.cblOptions.Items[Convert.ToInt32(FilterOption.valOvertime)].Selected;
                
                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridResults.VirtualItemCount = 1;
                #endregion

                if (this.EmployeeNo == 0 ||
                    (this.EmployeeNo > 0 && this.EmployeeNo != this.txtEmpNo.Value))
                {
                    this.btnGet_Click(this.btnGet, new EventArgs());
                }

                #region Load data to the grid
                // Initialize session
                this.EmpExceptionalList = null;

                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetEmployeeExceptional(startDate, endDate, empNo, isAbsence, isSickLeave, isNPH,
                    isInjuryLeave, isDIL, isOvertime, ref error, ref innerError);
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
                        this.EmpExceptionalList.AddRange(rawData.ToList());

                        //Display the record count
                        this.lblRecordCount.Text = string.Format("{0} record(s) found", this.EmpExceptionalList.Count.ToString("#,###"));
                    }
                }

                // Bind data to the grid
                RebindDataToGrid();
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