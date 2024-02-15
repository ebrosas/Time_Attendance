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
    public partial class ManualTimesheetEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToDelete,
            NoEmpNo,
            NoContractorNo,
            NoSpecifiedEmpNo,
            NoDateIn,
            NoDateOut,
            NoDateInAndOut,
            NoTimeIn,
            NoTimeOut,
            InvalidDateRange,
            InvalidSwipeTime,
            DateDifferenceExceedLimit
        }

        private enum FilterOption
        {
            valEmployee,
            valContractor
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

        private Dictionary<string, object> ManualTSEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ManualTSEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ManualTSEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ManualTSEntryStorage"] = value;
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

        private EmployeeAttendanceEntity CurrentManualTimesheetRecord
        {
            get
            {
                return ViewState["CurrentManualTimesheetRecord"] as EmployeeAttendanceEntity;
            }
            set
            {
                ViewState["CurrentManualTimesheetRecord"] = value;
            }
        }

        private UIHelper.DataLoadTypes CurrentFormLoadType
        {
            get
            {
                UIHelper.DataLoadTypes result = UIHelper.DataLoadTypes.OpenReadonlyRecord;
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

        private int AutoID
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["AutoID"]);
            }
            set
            {
                ViewState["AutoID"] = value;
            }
        }

        private FilterOption CurrentFilterOption
        {
            get
            {
                FilterOption result = FilterOption.valEmployee;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.MANLTSENTY.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_MANUAL_TIMESHEET_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_MANUAL_TIMESHEET_ENTRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Visible = this.Master.IsCreateAllowed;
                this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnGet.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ManualTSEntryStorage.Count > 0)
                {
                    if (this.ManualTSEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY]));
                    }

                    // Clear data storage
                    Session.Remove("ManualTSEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetContractorInfo.ToString())
                {
                    #region Get the contractor's info
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtContractorNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        this.litContractorName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("ManualTSEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("ManualTSEntryStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    InitializeControls(this.CurrentFormLoadType);

                    this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
                    this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

                    #region Check if need to load data in the grid
                    if (this.AutoID > 0)
                    {
                        GetManualTimesheetRecord(this.AutoID);
                    }
                    #endregion   
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Action Buttons        
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
                UIHelper.PAGE_MANUAL_TIMESHEET_ENTRY
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {

        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            // Check if there is selected record to delete
            if (this.CurrentManualTimesheetRecord == null)
            {
                this.txtGeneric.Text = ValidationErrorType.NoRecordToDelete.ToString();
                this.ErrorType = ValidationErrorType.NoRecordToDelete;
                this.cusValButton.Validate();
                return;
            }
            #endregion

            StringBuilder script = new StringBuilder();
            script.Append("ConfirmRecordDeletion('");
            script.Append(string.Concat(this.btnDeleteDummy.ClientID, "','"));
            script.Append(string.Concat(this.btnRebind.ClientID, "','"));
            script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Deletion Confirmation", script.ToString(), true);
        }

        protected void btnDeleteDummy_Click(object sender, EventArgs e)
        {
            try
            {
                #region Delete database record
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteManualTimesheet(Convert.ToInt32(UIHelper.SaveType.Delete), 
                    (new List<EmployeeAttendanceEntity>() { this.CurrentManualTimesheetRecord }), 
                    ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Shift Pattern Change Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_MANUAL_TIMESHEET_INQ + "?{0}={1}",
                        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                        true.ToString()
                    ),
                    false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            if (this.AutoID > 0)
            {
                GetManualTimesheetRecord(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (this.AutoID > 0)
            {
                GetManualTimesheetRecord(this.AutoID);
            }
            else
            {
                #region Reset controls
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                this.litUpdateUser.Text = "Not defined";
                this.litLastUpdateTime.Text = "Not defined";
                this.txtContractorNo.Text = string.Empty;
                this.litContractorName.Text = "Not defined";

                this.dtpDateIn.SelectedDate = null;
                this.dtpDateOut.SelectedDate = null;
                this.dtpTimeIn.SelectedDate = null;
                this.dtpTimeOut.SelectedDate = null;

                this.chkSwipeIn.Checked = false;
                this.chkSwipeIn_CheckedChanged(this.chkSwipeIn, new EventArgs());

                this.chkSwipeOut.Checked = false;
                this.chkSwipeOut_CheckedChanged(this.chkSwipeOut, new EventArgs());

                this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
                this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());
                #endregion

                #region Clear sessions
                this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentManualTimesheetRecord"] = null;
                #endregion
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                UIHelper.SaveType saveType = this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord
                    ? UIHelper.SaveType.Insert
                    : UIHelper.SaveType.Update;

                #region Perform Data Validation

                #region Check selected employee 
                int empNo = 0;
                if (this.rblOption.SelectedValue == FilterOption.valEmployee.ToString())
                {
                    empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                    if (empNo.ToString().Length == 4)
                    {
                        empNo += 10000000;

                        // Display the formatted Emp. No.
                        this.txtEmpNo.Text = empNo.ToString();
                    }

                    if (empNo == 0)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                        this.ErrorType = ValidationErrorType.NoEmpNo;
                        this.cusValEmpNo.Validate();
                        errorCount++;
                    }
                }
                else
                {
                    empNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
                    if (empNo.ToString().Length == 4)
                    {
                        empNo += 10000000;

                        // Display the formatted Emp. No.
                        this.txtContractorNo.Text = empNo.ToString();
                    }

                    if (empNo == 0)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoContractorNo.ToString();
                        this.ErrorType = ValidationErrorType.NoContractorNo;
                        this.cusValContractorNo.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate Date In and Date Out 
                if (this.dtpDateIn.SelectedDate == null &&
                    this.dtpDateOut.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDateInAndOut.ToString();
                    this.ErrorType = ValidationErrorType.NoDateInAndOut;
                    this.cusValSwipeIn.Validate();
                    errorCount++;
                }
                else
                {
                    #region Check Date In
                    if (this.dtpDateIn.SelectedDate != null &&
                        this.dtpTimeIn.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoTimeIn.ToString();
                        this.ErrorType = ValidationErrorType.NoTimeIn;
                        this.cusValSwipeIn.Validate();
                        errorCount++;
                    }
                    else if (this.dtpDateIn.SelectedDate == null &&
                        this.dtpTimeIn.SelectedDate != null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoDateIn.ToString();
                        this.ErrorType = ValidationErrorType.NoDateIn;
                        this.cusValSwipeIn.Validate();
                        errorCount++;
                    }
                    #endregion

                    #region Check Date Out
                    if (this.dtpDateOut.SelectedDate != null &&
                        this.dtpTimeOut.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoTimeOut.ToString();
                        this.ErrorType = ValidationErrorType.NoTimeOut;
                        this.cusValSwipeOut.Validate();
                        errorCount++;
                    }
                    else if (this.dtpDateOut.SelectedDate == null &&
                        this.dtpTimeOut.SelectedDate != null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoDateOut.ToString();
                        this.ErrorType = ValidationErrorType.NoDateOut;
                        this.cusValSwipeOut.Validate();
                        errorCount++;
                    }
                    #endregion

                    #region Check date range
                    if (this.dtpDateIn.SelectedDate != null &&
                        this.dtpDateOut.SelectedDate != null &&
                        this.dtpDateOut.SelectedDate < this.dtpDateIn.SelectedDate)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                        this.ErrorType = ValidationErrorType.InvalidDateRange;
                        this.cusValSwipeOut.Validate();
                        errorCount++;
                    }
                    else if (this.dtpDateIn.SelectedDate != null &&
                        this.dtpDateOut.SelectedDate != null &&
                        (Convert.ToDateTime(this.dtpDateOut.SelectedDate) - Convert.ToDateTime(this.dtpDateIn.SelectedDate)).TotalDays > 1)
                    {
                        this.txtGeneric.Text = ValidationErrorType.DateDifferenceExceedLimit.ToString();
                        this.ErrorType = ValidationErrorType.DateDifferenceExceedLimit;
                        this.cusValSwipeOut.Validate();
                        errorCount++;
                    }
                    #endregion
                }
                #endregion

                #region Validate swipe time
                if (this.dtpTimeIn.SelectedDate != null &&
                    this.dtpTimeOut.SelectedDate != null)
                {
                    DateTime swipeIn = Convert.ToDateTime(this.dtpDateIn.SelectedDate)
                        .AddHours(Convert.ToDateTime(this.dtpTimeIn.SelectedDate).Hour)
                        .AddMinutes(Convert.ToDateTime(this.dtpTimeIn.SelectedDate).Minute);

                    DateTime swipeOut = Convert.ToDateTime(this.dtpDateOut.SelectedDate)
                       .AddHours(Convert.ToDateTime(this.dtpTimeOut.SelectedDate).Hour)
                       .AddMinutes(Convert.ToDateTime(this.dtpTimeOut.SelectedDate).Minute);

                    if (swipeIn > swipeOut)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidSwipeTime.ToString();
                        this.ErrorType = ValidationErrorType.InvalidSwipeTime;
                        this.cusValSwipeIn.Validate();
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

                if (saveType == UIHelper.SaveType.Insert)
                {
                    #region Perform Insert Operation
                    // Initialize collection
                    List<EmployeeAttendanceEntity> recordToInsertList = new List<EmployeeAttendanceEntity>();

                    recordToInsertList.Add(new EmployeeAttendanceEntity()
                    {
                        EmpNo = empNo,
                        dtIN = this.dtpDateIn.SelectedDate,
                        dtOUT = this.dtpDateOut.SelectedDate,
                        TimeIn = this.dtpTimeIn.SelectedDate,
                        TimeOut = this.dtpTimeOut.SelectedDate,
                        CreatedUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                        CreatedTime = DateTime.Now
                    });

                    SaveChanges(saveType, recordToInsertList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Update)
                {
                    #region Perform Update Operation
                    // Update data change 
                    this.CurrentManualTimesheetRecord.dtIN = this.dtpDateIn.SelectedDate;
                    this.CurrentManualTimesheetRecord.dtOUT = this.dtpDateOut.SelectedDate;
                    this.CurrentManualTimesheetRecord.TimeIn = this.dtpTimeIn.SelectedDate;
                    this.CurrentManualTimesheetRecord.TimeOut = this.dtpTimeOut.SelectedDate;
                    this.CurrentManualTimesheetRecord.LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]));
                    this.CurrentManualTimesheetRecord.LastUpdateTime = DateTime.Now;

                    // Initialize collection
                    List<EmployeeAttendanceEntity> recordToUpdateList = new List<EmployeeAttendanceEntity>() { this.CurrentManualTimesheetRecord };

                    SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    // Initialize collection
                    List<EmployeeAttendanceEntity> recordToUpdateList = new List<EmployeeAttendanceEntity>() { this.CurrentManualTimesheetRecord };

                    SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerForm))
                Response.Redirect(this.CallerForm, false);
            else
                Response.Redirect(UIHelper.PAGE_HOME, false);
        }

        protected void btnFindContractor_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetContractorInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_CONTRACTOR_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_MANUAL_TIMESHEET_ENTRY
            ),
            false);
        }

        protected void btnGetContractor_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform data validation
                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtContractorNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoContractorNo.ToString();
                    this.ErrorType = ValidationErrorType.NoContractorNo;
                    this.cusValContractorNo.Validate();
                    return;
                }
                #endregion

                #region Initialize control values and variables
                this.litContractorName.Text = "Not defined";
                
                int empNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtContractorNo.Text = empNo.ToString();
                }

                string error = string.Empty;
                string innerError = string.Empty;
                #endregion

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetContractors(empNo, string.Empty, ref error, ref innerError);
                if (rawData != null)
                {
                    EmployeeDetail contractorRecord = rawData.FirstOrDefault();
                    if (contractorRecord != null)
                    {
                        this.litContractorName.Text = contractorRecord.ContractorEmpName;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
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
                else if (this.ErrorType == ValidationErrorType.NoContractorNo)
                {
                    validator.ErrorMessage = "Contractor No. is required.";
                    validator.ToolTip = "Contractor No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateInAndOut)
                {
                    validator.ErrorMessage = "Both Swipe In and Swipe Out dates cannot be null.";
                    validator.ToolTip = "Both Swipe In and Swipe Out dates cannot be null.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateIn)
                {
                    validator.ErrorMessage = "Date In is required.";
                    validator.ToolTip = "Date In is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateOut)
                {
                    validator.ErrorMessage = "Date In is required.";
                    validator.ToolTip = "Date In is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeIn)
                {
                    validator.ErrorMessage = "Time In is required if Date In is specified.";
                    validator.ToolTip = "Time In is required if Date In is specified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTimeOut)
                {
                    validator.ErrorMessage = "Time Out is required if Date Out is specified.";
                    validator.ToolTip = "Time Out is required if Date Out is specified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "No record has been selected on the grid for deletion.";
                    validator.ToolTip = "No record has been selected on the grid for deletion.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "Date In should be less than Date Out.";
                    validator.ToolTip = "Date In should be less than Date Out.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidSwipeTime)
                {
                    validator.ErrorMessage = "Invalid swipe time. The swipe-in time should be less than the swipe-out time.";
                    validator.ToolTip = "Invalid swipe time. The swipe-in time should be less than the swipe-out time.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.DateDifferenceExceedLimit)
                {
                    validator.ErrorMessage = "The difference between the swipe in and swipe out dates should not exceed 1 day.";
                    validator.ToolTip = "The difference between the swipe in and swipe out dates should not exceed 1 day.";
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

        protected void chkSwipeIn_CheckedChanged(object sender, EventArgs e)
        {
            this.dtpDateIn.Enabled = this.dtpTimeIn.Enabled = this.chkSwipeIn.Checked;

            if (!this.dtpDateIn.Enabled)
                this.dtpDateIn.SelectedDate = null;
            else
                this.dtpDateIn.Focus();

            if (!this.dtpTimeIn.Enabled)
                this.dtpTimeIn.SelectedDate = null;
        }

        protected void chkSwipeOut_CheckedChanged(object sender, EventArgs e)
        {
            this.dtpDateOut.Enabled = this.dtpTimeOut.Enabled = this.chkSwipeOut.Checked;

            if (!this.dtpDateOut.Enabled)
                this.dtpDateOut.SelectedDate = null;
            else
            {
                if (this.dtpDateIn.SelectedDate != null)
                    this.dtpDateOut.SelectedDate = this.dtpDateIn.SelectedDate;

                this.dtpDateOut.Focus();
            }

            if (!this.dtpTimeOut.Enabled)
                this.dtpTimeOut.SelectedDate = null;
        }

        protected void rblOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rblOption.SelectedValue == FilterOption.valEmployee.ToString())
            {
                this.panEmployee.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.panContractor.Style[HtmlTextWriterStyle.Display] = "none";
                this.txtEmpNo.Focus();
            }
            else
            {
                this.panContractor.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.panEmployee.Style[HtmlTextWriterStyle.Display] = "none";
                this.txtContractorNo.Focus();
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = string.Empty;
            this.litPosition.Text = string.Empty;
            this.litCostCenter.Text = string.Empty;
            this.litUpdateUser.Text = string.Empty;
            this.litLastUpdateTime.Text = string.Empty;
            this.txtContractorNo.Text = string.Empty;
            this.litContractorName.Text = string.Empty;

            this.dtpDateIn.SelectedDate = null;
            this.dtpDateOut.SelectedDate = null;
            this.dtpTimeIn.SelectedDate = null;
            this.dtpTimeOut.SelectedDate = null;

            this.chkSwipeIn.Checked = false;
            this.chkSwipeIn_CheckedChanged(this.chkSwipeIn, new EventArgs());

            this.chkSwipeOut.Checked = false;
            this.chkSwipeOut_CheckedChanged(this.chkSwipeOut, new EventArgs());

            this.rblOption.SelectedValue = FilterOption.valEmployee.ToString();
            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.AutoID = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY]);

            #region Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_FORM_LOAD_TYPE]);
            if (formLoadType != string.Empty)
            {
                UIHelper.DataLoadTypes loadType = UIHelper.DataLoadTypes.OpenReadonlyRecord;
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

            #region Determine Filter Option
            bool isContractor = UIHelper.ConvertObjectToBolean(Request.QueryString["IsContractor"]);
            if (isContractor)
                this.CurrentFilterOption = FilterOption.valContractor;
            else
                this.CurrentFilterOption = FilterOption.valEmployee;
            #endregion
        }

        public void KillSessions()
        {
            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentManualTimesheetRecord"] = null;
            ViewState["CurrentFilterOption"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ManualTSEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ManualTSEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.ManualTSEntryStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["CurrentFormLoadType"]);
            if (formLoadType != string.Empty)
            {
                UIHelper.DataLoadTypes loadType = UIHelper.DataLoadTypes.OpenReadonlyRecord;
                try
                {
                    loadType = (UIHelper.DataLoadTypes)Enum.Parse(typeof(UIHelper.DataLoadTypes), formLoadType);
                }
                catch (Exception)
                {
                }
                this.CurrentFormLoadType = loadType;
            }

            string filterOption = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["CurrentFilterOption"]);
            if (filterOption != string.Empty)
            {
                FilterOption loadType = FilterOption.valEmployee;
                try
                {
                    loadType = (FilterOption)Enum.Parse(typeof(FilterOption), filterOption);
                }
                catch (Exception)
                {
                }
                this.CurrentFilterOption = loadType;
            }
            #endregion

            #region Restore session values
            if (this.ManualTSEntryStorage.ContainsKey("CurrentManualTimesheetRecord"))
                this.CurrentManualTimesheetRecord = this.ManualTSEntryStorage["CurrentManualTimesheetRecord"] as EmployeeAttendanceEntity;
            else
                this.CurrentManualTimesheetRecord = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ManualTSEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("txtContractorNo"))
                this.txtContractorNo.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["txtContractorNo"]);
            else
                this.txtContractorNo.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("litContractorName"))
                this.litContractorName.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["litContractorName"]);
            else
                this.litContractorName.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("litUpdateUser"))
                this.litUpdateUser.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["litUpdateUser"]);
            else
                this.litUpdateUser.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.ManualTSEntryStorage.ContainsKey("dtpDateIn"))
                this.dtpDateIn.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualTSEntryStorage["dtpDateIn"]);
            else
                this.dtpDateIn.SelectedDate = null;

            if (this.ManualTSEntryStorage.ContainsKey("dtpDateOut"))
                this.dtpDateOut.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualTSEntryStorage["dtpDateOut"]);
            else
                this.dtpDateOut.SelectedDate = null;

            if (this.ManualTSEntryStorage.ContainsKey("dtpTimeIn"))
                this.dtpTimeIn.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualTSEntryStorage["dtpTimeIn"]);
            else
                this.dtpTimeIn.SelectedDate = null;

            if (this.ManualTSEntryStorage.ContainsKey("dtpTimeOut"))
                this.dtpTimeOut.SelectedDate = UIHelper.ConvertObjectToDate(this.ManualTSEntryStorage["dtpTimeOut"]);
            else
                this.dtpTimeOut.SelectedDate = null;

            if (this.ManualTSEntryStorage.ContainsKey("chkSwipeIn"))
                this.chkSwipeIn.Checked = UIHelper.ConvertObjectToBolean(this.ManualTSEntryStorage["chkSwipeIn"]);
            else
                this.chkSwipeIn.Checked = false;

            this.chkSwipeIn_CheckedChanged(this.chkSwipeIn, new EventArgs());

            if (this.ManualTSEntryStorage.ContainsKey("chkSwipeOut"))
                this.chkSwipeOut.Checked = UIHelper.ConvertObjectToBolean(this.ManualTSEntryStorage["chkSwipeOut"]);
            else
                this.chkSwipeOut.Checked = false;

            this.chkSwipeOut_CheckedChanged(this.chkSwipeOut, new EventArgs());

            if (this.ManualTSEntryStorage.ContainsKey("rblOption"))
                this.rblOption.SelectedValue = UIHelper.ConvertObjectToString(this.ManualTSEntryStorage["rblOption"]);
            else
                this.rblOption.SelectedValue = FilterOption.valEmployee.ToString();

            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ManualTSEntryStorage.Clear();
            this.ManualTSEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ManualTSEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.ManualTSEntryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.ManualTSEntryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.ManualTSEntryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.ManualTSEntryStorage.Add("txtContractorNo", this.txtContractorNo.Text.Trim());
            this.ManualTSEntryStorage.Add("litContractorName", this.litContractorName.Text.Trim());
            this.ManualTSEntryStorage.Add("litUpdateUser", this.litUpdateUser.Text.Trim());
            this.ManualTSEntryStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());
            this.ManualTSEntryStorage.Add("dtpDateIn", this.dtpDateIn.SelectedDate);
            this.ManualTSEntryStorage.Add("dtpDateOut", this.dtpDateOut.SelectedDate);
            this.ManualTSEntryStorage.Add("dtpTimeIn", this.dtpTimeIn.SelectedDate);
            this.ManualTSEntryStorage.Add("dtpTimeOut", this.dtpTimeOut.SelectedDate);
            this.ManualTSEntryStorage.Add("chkSwipeIn", this.chkSwipeIn.Checked);
            this.ManualTSEntryStorage.Add("chkSwipeOut", this.chkSwipeOut.Checked);
            this.ManualTSEntryStorage.Add("rblOption", this.rblOption.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.ManualTSEntryStorage.Add("CallerForm", this.CallerForm);
            this.ManualTSEntryStorage.Add("AutoID", this.AutoID);
            this.ManualTSEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            this.ManualTSEntryStorage.Add("CurrentFilterOption", this.CurrentFilterOption);
            #endregion

            #region Store session data to collection
            this.ManualTSEntryStorage.Add("CurrentManualTimesheetRecord", this.CurrentManualTimesheetRecord);
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

        private void InitializeControls(UIHelper.DataLoadTypes formLoadType)
        {
            this.dtpDateIn.MaxDate = DateTime.Now;
            this.dtpDateOut.MaxDate = DateTime.Now;

            switch (formLoadType)
            {
                case UIHelper.DataLoadTypes.CreateNewRecord:
                    #region Create new record
                    // Setup controls 
                    this.txtEmpNo.Enabled = true;
                    this.dtpDateIn.Enabled = false;
                    this.dtpDateOut.Enabled = false;
                    this.dtpTimeIn.Enabled = false;
                    this.dtpTimeOut.Enabled = false;
                    this.chkSwipeIn.Checked = false;
                    this.chkSwipeOut.Checked = false;
                    this.chkSwipeIn.Enabled = true;
                    this.chkSwipeOut.Enabled = true;
                    this.rblOption.Enabled = true;

                    // Initialize control values
                    this.litEmpName.Text = "Not defined";
                    this.litPosition.Text = "Not defined";
                    this.litCostCenter.Text = "Not defined";
                    this.litUpdateUser.Text = "Not defined";
                    this.litLastUpdateTime.Text = "Not defined";
                    this.litContractorName.Text = "Not defined";

                    // Setup buttons
                    this.btnGet.Enabled = true;
                    this.btnGetContractor.Enabled = true;
                    this.btnFindEmployee.Enabled = true;
                    this.btnFindContractor.Enabled = true;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing record
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.txtContractorNo.Enabled = false;
                    this.dtpDateIn.Enabled = true;
                    this.dtpDateOut.Enabled = true;
                    this.dtpTimeIn.Enabled = true;
                    this.dtpTimeOut.Enabled = true;
                    this.chkSwipeIn.Checked = true;
                    this.chkSwipeOut.Checked = true;
                    this.chkSwipeIn.Enabled = true;
                    this.chkSwipeOut.Enabled = true;
                    this.rblOption.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnGetContractor.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnFindContractor.Enabled = false;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = true;
                    this.btnReset.Enabled = true;
                    
                    break;
                    #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing record (read-only)
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.txtContractorNo.Enabled = false;
                    this.dtpDateIn.Enabled = false;
                    this.dtpDateOut.Enabled = false;
                    this.dtpTimeIn.Enabled = false;
                    this.dtpTimeOut.Enabled = false;
                    this.chkSwipeIn.Checked = false;                    
                    this.chkSwipeOut.Checked = false;
                    this.chkSwipeIn.Enabled = false;
                    this.chkSwipeOut.Enabled = false;
                    this.rblOption.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnGetContractor.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnFindContractor.Enabled = false;
                    this.btnSave.Enabled = false;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion
            }
        }

        private void FillComboData(bool reloadFromDB = true)
        {
        }
        #endregion

        #region Database Access
        private void GetManualTimesheetRecord(int autoID)
        {
            try
            {
                #region Initialize controls
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                this.litUpdateUser.Text = "Not defined";
                this.litLastUpdateTime.Text = "Not defined";
                this.txtContractorNo.Text = string.Empty;
                this.litContractorName.Text = "Not defined";

                this.dtpDateIn.SelectedDate = null;
                this.dtpDateOut.SelectedDate = null;
                this.dtpTimeIn.SelectedDate = null;
                this.dtpTimeOut.SelectedDate = null;

                this.chkSwipeIn.Checked = false;
                this.chkSwipeOut.Checked = false;
                #endregion

                if (Session["SelectedManualTimesheet"] != null)
                {
                    this.CurrentManualTimesheetRecord = Session["SelectedManualTimesheet"] as EmployeeAttendanceEntity;
                }
                else
                {
                    #region Fetch database record
                    List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetManualTimesheetEntry(autoID, 0, string.Empty, null, null, 0, 0, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(error, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (rawData != null)
                        {
                            this.CurrentManualTimesheetRecord = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentManualTimesheetRecord != null)
                {
                    if (this.rblOption.SelectedValue == FilterOption.valEmployee.ToString())
                    {
                        this.txtEmpNo.Value = this.CurrentManualTimesheetRecord.EmpNo;
                        this.litEmpName.Text = this.CurrentManualTimesheetRecord.EmpName;
                        this.litPosition.Text = this.CurrentManualTimesheetRecord.Position;
                        this.litCostCenter.Text = this.CurrentManualTimesheetRecord.CostCenterFullName;
                    }
                    else
                    {
                        this.txtContractorNo.Value = this.CurrentManualTimesheetRecord.EmpNo;
                        this.litContractorName.Text = this.CurrentManualTimesheetRecord.EmpName;
                    }

                    this.litUpdateUser.Text = this.CurrentManualTimesheetRecord.LastUpdateUser;
                    this.litLastUpdateTime.Text = this.CurrentManualTimesheetRecord.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentManualTimesheetRecord.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : string.Empty;
                    this.dtpDateIn.SelectedDate = this.CurrentManualTimesheetRecord.dtIN;
                    this.dtpDateOut.SelectedDate = this.CurrentManualTimesheetRecord.dtOUT;
                    this.dtpTimeIn.SelectedDate = this.CurrentManualTimesheetRecord.TimeIn;
                    this.dtpTimeOut.SelectedDate = this.CurrentManualTimesheetRecord.TimeOut;

                    this.chkSwipeIn.Checked = this.dtpDateIn.SelectedDate != null;
                    this.chkSwipeOut.Checked = this.dtpDateOut.SelectedDate != null;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, List<EmployeeAttendanceEntity> shiftPatternList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteManualTimesheet(Convert.ToInt32(saveType), shiftPatternList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Shift Pattern Changes Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_MANUAL_TIMESHEET_INQ + "?{0}={1}&{2}={3}",
                        UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                        this.AutoID,
                        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                        true.ToString()
                    ),
                    false);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.CurrentManualTimesheetRecord = null;
                throw new Exception(ex.Message.ToString());
            }
        }
        #endregion
    }
}