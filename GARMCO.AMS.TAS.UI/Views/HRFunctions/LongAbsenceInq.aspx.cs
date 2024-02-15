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
    public partial class LongAbsenceInq : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoSpecifiedDate,
            NoSpecifiedEmpNo,
            NoSelectedFilterOption
        }

        private enum FilterOption
        {
            valSickLeave,
            valUnpaidLeave,
            valAbsent
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

        private Dictionary<string, object> LongAbsenceStorage
        {
            get
            {
                Dictionary<string, object> list = Session["LongAbsenceStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["LongAbsenceStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["LongAbsenceStorage"] = value;
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

        private List<EmployeeAttendanceEntity> LongAbsencesList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["LongAbsencesList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["LongAbsencesList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["LongAbsencesList"] = value;
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

        private string CurrentAttendanceRemarks
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CurrentAttendanceRemarks"]);
            }
            set
            {
                ViewState["CurrentAttendanceRemarks"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.LONGABSENT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_LONG_ABSENCES_INQUIRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_LONG_ABSENCES_INQUIRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnNew.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.LongAbsenceStorage.Count > 0)
                {
                    if (this.LongAbsenceStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.LongAbsenceStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        //this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]);

                        // Save Employee No. to session
                        this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("LongAbsenceStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("LongAbsenceStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    // Initialize controls
                    this.dtpProcessDate.SelectedDate = DateTime.Now;
                    this.CurrentAttendanceRemarks = "Attendance History";

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
            if (this.LongAbsencesList.Count > 0)
            {
                this.gridResults.DataSource = this.LongAbsencesList;
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

        protected void gridResults_PreRender(object sender, EventArgs e)
        {
            try
            {
                GridColumn dynamicColumn = this.gridResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "AttendanceHistoryValue").FirstOrDefault();
                if (dynamicColumn != null)
                {
                    dynamicColumn.HeaderText = this.CurrentAttendanceRemarks;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }
        
        private void RebindDataToGrid()
        {
            if (this.LongAbsencesList.Count > 0)
            {
                this.gridResults.DataSource = this.LongAbsencesList;
                this.gridResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.LongAbsencesList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridResults.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridResults.DataBind();

            this.CurrentAttendanceRemarks = "Attendance History";
            this.gridResults_PreRender(this.gridResults, new EventArgs());
            this.lblRecordCount.Text = "0 record found";
            this.lblDateDuration.Text = string.Empty;
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.txtEmpNo.Text = string.Empty;            
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.dtpProcessDate.SelectedDate = null;
            this.cblOptions.ClearSelection();

            // Cler collections
            this.LongAbsencesList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["EmployeeNo"] = null;

            // Reset the grid
            this.gridResults.VirtualItemCount = 1;
            this.gridResults.CurrentPageIndex = 0;

            InitializeDataToGrid();
            #endregion

            // Initialize controls
            //this.dtpProcessDate.SelectedDate = DateTime.Now;            
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check Date 
            if (this.dtpProcessDate.SelectedDate == null)
            {
                this.txtGeneric.Text = ValidationErrorType.NoSpecifiedDate.ToString();
                this.ErrorType = ValidationErrorType.NoSpecifiedDate;
                this.cusValDate.Validate();
                errorCount++;
            }

            // Check selected criteria
            if (string.IsNullOrEmpty(this.cblOptions.SelectedValue))
            {
                this.txtGeneric.Text = ValidationErrorType.NoSelectedFilterOption.ToString();
                this.ErrorType = ValidationErrorType.NoSelectedFilterOption;
                this.cusValFilterOption.Validate();
                errorCount++;
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
            GetLongAbsences();
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
                        this.cboCostCenter.SelectedValue = empInfo.CostCenter;
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
                            this.cboCostCenter.SelectedValue = rawData.CostCenter;
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
                UIHelper.PAGE_LONG_ABSENCES_INQUIRY
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
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
                    args.IsValid = false;
                }                
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedDate)
                {
                    validator.ErrorMessage = "Date is required.";
                    validator.ToolTip = "Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSelectedFilterOption)
                {
                    validator.ErrorMessage = "Data Selection is required.";
                    validator.ToolTip = "Data Selection is required.";
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
            this.dtpProcessDate.SelectedDate = null;            
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
        }

        public void KillSessions()
        {
            // Cler collections
            this.LongAbsencesList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;            
            ViewState["CallerForm"] = null;
            ViewState["EmployeeNo"] = null;
            ViewState["CurrentAttendanceRemarks"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.LongAbsenceStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.LongAbsenceStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.LongAbsenceStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.LongAbsenceStorage.ContainsKey("LongAbsencesList"))
                this.LongAbsencesList = this.LongAbsenceStorage["LongAbsencesList"] as List<EmployeeAttendanceEntity>;
            else
                this.LongAbsencesList = null;

            if (this.LongAbsenceStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.LongAbsenceStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            if (this.LongAbsenceStorage.ContainsKey("CurrentAttendanceRemarks"))
                this.CurrentAttendanceRemarks = UIHelper.ConvertObjectToString(this.LongAbsenceStorage["CurrentAttendanceRemarks"]);
            else
                this.CurrentAttendanceRemarks = string.Empty;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.LongAbsenceStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.LongAbsenceStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.LongAbsenceStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.LongAbsenceStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.LongAbsenceStorage.ContainsKey("dtpProcessDate"))
                this.dtpProcessDate.SelectedDate = UIHelper.ConvertObjectToDate(this.LongAbsenceStorage["dtpProcessDate"]);
            else
                this.dtpProcessDate.SelectedDate = null;
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            this.gridResults.CurrentPageIndex = 0;
            this.gridResults.MasterTableView.CurrentPageIndex = 0;
            this.gridResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.LongAbsenceStorage.Clear();
            this.LongAbsenceStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.LongAbsenceStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.LongAbsenceStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.LongAbsenceStorage.Add("dtpProcessDate", this.dtpProcessDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.LongAbsenceStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.LongAbsenceStorage.Add("LongAbsencesList", this.LongAbsencesList);
            this.LongAbsenceStorage.Add("EmployeeNo", this.EmployeeNo);
            this.LongAbsenceStorage.Add("CurrentAttendanceRemarks", this.CurrentAttendanceRemarks);
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
        private void GetLongAbsences()
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

                    //if (this.EmployeeNo == 0)
                    //    this.btnGet_Click(this.btnGet, new EventArgs());
                }

                DateTime? processDate = this.dtpProcessDate.SelectedDate;
                if (processDate == DateTime.Now)
                    processDate = DateTime.Now.AddDays(-1);

                string costCenter = this.cboCostCenter.SelectedValue;
                bool showSLP = this.cblOptions.Items[Convert.ToInt32(FilterOption.valSickLeave)].Selected;
                bool showUL = this.cblOptions.Items[Convert.ToInt32(FilterOption.valUnpaidLeave)].Selected;
                bool showAbsent = this.cblOptions.Items[Convert.ToInt32(FilterOption.valAbsent)].Selected;
                DateTime? startDate = null;
                DateTime? endDate = null;
                string attendanceHistoryTitle = string.Empty;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.lblDateDuration.Text = string.Empty;
                #endregion

                #region Load data to the grid
                // Initialize session
                this.LongAbsencesList = null;

                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetLongAbsences(processDate, showSLP, showUL, showAbsent, ref startDate, ref endDate, 
                    ref attendanceHistoryTitle, ref error, ref innerError);
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
                        if (empNo > 0 || !string.IsNullOrEmpty(costCenter))
                        {
                            List<EmployeeAttendanceEntity> filteredList = new List<EmployeeAttendanceEntity>();
                            if (empNo > 0 && !string.IsNullOrEmpty(costCenter))
                            {
                                filteredList.AddRange(rawData.Where(a => a.EmpNo == empNo
                                    && a.CostCenter == costCenter));
                            }
                            else
                            {
                                if (empNo > 0)
                                    filteredList.AddRange(rawData.Where(a => a.EmpNo == empNo));
                                else
                                    filteredList.AddRange(rawData.Where(a => a.CostCenter == costCenter));
                            }

                            // Save to session
                            this.LongAbsencesList.AddRange(filteredList.ToList());
                        }
                        else
                        {
                            // Save to session
                            this.LongAbsencesList.AddRange(rawData.ToList());
                        }

                        // Set the Attendance History
                        this.CurrentAttendanceRemarks = attendanceHistoryTitle;
                        this.gridResults_PreRender(this.gridResults, new EventArgs());

                        // Display the date duration
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            this.lblDateDuration.Text = string.Format("Start Date: {0}; End Date: {1}",
                                Convert.ToDateTime(startDate).ToString("dd-MMM-yyyy"),
                                Convert.ToDateTime(endDate).ToString("dd-MMM-yyyy"));
                        }

                        //Display the record count
                        this.lblRecordCount.Text = string.Format("{0} record(s) found", this.LongAbsencesList.Count.ToString("#,###"));
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