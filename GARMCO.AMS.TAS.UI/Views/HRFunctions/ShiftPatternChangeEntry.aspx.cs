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
    public partial class ShiftPatternChangeEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            InvalidDateRange,
            NoRecordToDelete,
            NoEmpNo,
            NoEffectiveDate,
            NoEndingDate,
            NoShiftPatCode,
            NoShiftPointer,
            NoChangeType,
            NoEmployeeNo,
            NoSelectedFireTeam,
            NoCostCenterPermission
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

        private Dictionary<string, object> ShiftPatternEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ShiftPatternEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ShiftPatternEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ShiftPatternEntryStorage"] = value;
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

        private ShiftPatternEntity CurrentShiftPatternChange
        {
            get
            {
                return ViewState["CurrentShiftPatternChange"] as ShiftPatternEntity;
            }
            set
            {
                ViewState["CurrentShiftPatternChange"] = value;
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

        private List<ShiftPatternEntity> ShiftPatternCodeList
        {
            get
            {
                List<ShiftPatternEntity> list = ViewState["ShiftPatternCodeList"] as List<ShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPatternCodeList"] = list = new List<ShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPatternCodeList"] = value;
            }
        }

        private List<ShiftPatternEntity> ShiftPointerCodeList
        {
            get
            {
                List<ShiftPatternEntity> list = ViewState["ShiftPointerCodeList"] as List<ShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPointerCodeList"] = list = new List<ShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPointerCodeList"] = value;
            }
        }

        private List<EmployeeDetail> FireTeamMemberList
        {
            get
            {
                List<EmployeeDetail> list = ViewState["FireTeamMemberList"] as List<EmployeeDetail>;
                if (list == null)
                    ViewState["FireTeamMemberList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                ViewState["FireTeamMemberList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.SHFTPATENT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY_TITLE), true);
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
                if (this.ShiftPatternEntryStorage.Count > 0)
                {
                    if (this.ShiftPatternEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        int empNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        bool isFireTeamMember = false;

                        #region Check if the selected employee is a Fire Team Member
                        if (this.CurrentFilterOption == FilterOption.valEmployee &&
                            this.FireTeamMemberList.Count > 0)
                        {
                            EmployeeDetail fireTeamMember = this.FireTeamMemberList
                                .Where(a => a.EmpNo == empNo)
                                .FirstOrDefault();
                            isFireTeamMember = fireTeamMember != null;
                        }
                        #endregion

                        if (!isFireTeamMember)
                        {
                            #region Get the employee information if not a Fire Team Member
                            this.txtEmpNo.Value = empNo;
                            this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                            this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                            this.litCostCenter.Text = string.Format("{0} - {1}",
                                UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                                UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY]));
                            this.litSupervisor.Text = string.Format("{0} - {1}",
                                UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_SUPERVISOR_NO_KEY]),
                                UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_SUPERVISOR_NAME_KEY]));

                            #region Get the shift pattern information
                            string shiftPatternInfo = GetShiftPatternInformation(UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]));
                            if (!string.IsNullOrEmpty(shiftPatternInfo))
                            {
                                this.litCurrentShiftPattern.Text = shiftPatternInfo;
                            }
                            #endregion

                            #endregion
                        }
                        else
                        {
                            // Disable the Save button
                            this.btnSave.Enabled = false;

                            DisplayFormLevelError("Sorry, you cannot change the Shift Pattern of the selected employee because he is a Fire Team Member. Please go to Shift Pattern Changes (Fire Team Member) page to change the shift pattern.");
                        }
                    }

                    // Clear data storage
                    Session.Remove("ShiftPatternEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("ShiftPatternEntryStorage");
                    #endregion
                }
                else if (formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("ShiftPatternEntryStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
                    this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

                    #region Check if need to load Shift Pattern Change record
                    if (this.AutoID > 0)
                    {
                        GetShiftPatternChange(this.AutoID);
                    }
                    #endregion

                    InitializeControls(this.CurrentFormLoadType);
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
                this.litSupervisor.Text = "Not defined";
                this.litCurrentShiftPattern.Text = "Unknown";
                #endregion

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                #region Check if the selected employee is a Fire Team Member
                bool isFireTeamMember = false;
                if (this.CurrentFilterOption == FilterOption.valEmployee &&
                    this.FireTeamMemberList.Count > 0)
                {
                    EmployeeDetail fireTeamMember = this.FireTeamMemberList
                        .Where(a => a.EmpNo == empNo)
                        .FirstOrDefault();
                    isFireTeamMember = fireTeamMember != null;
                }
                #endregion

                if (!isFireTeamMember)
                {
                    #region Get employee info from the employee master
                    string error = string.Empty;
                    string innerError = string.Empty;
                    DALProxy proxy = new DALProxy();

                    var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                    if (rawData != null)
                    {
                        #region Check if user has permission to the cost center of the specified employee
                        if (this.Master.AllowedCostCenterList.Count > 0)
                        {
                            string allowedCC = this.Master.AllowedCostCenterList
                                  .Where(a => a == rawData.CostCenter)
                                  .FirstOrDefault();
                            if (string.IsNullOrEmpty(allowedCC))
                            {
                                this.txtGeneric.Text = ValidationErrorType.NoCostCenterPermission.ToString();
                                this.ErrorType = ValidationErrorType.NoCostCenterPermission;
                                this.cusValEmpNo.Validate();

                                this.txtEmpNo.Text = string.Empty;
                                return;
                            }
                        }
                        #endregion

                        this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                           rawData.CostCenter,
                           rawData.CostCenterName);
                        this.litSupervisor.Text = string.Format("{0} - {1}",
                            rawData.SupervisorEmpNo,
                            rawData.SupervisorEmpName);

                        #region Get the shift pattern information
                        string shiftPatternInfo = GetShiftPatternInformation(rawData.EmpNo);
                        if (!string.IsNullOrEmpty(shiftPatternInfo))
                        {
                            this.litCurrentShiftPattern.Text = shiftPatternInfo;
                        }
                        #endregion
                    }

                    // Enable the Save button
                    this.btnSave.Enabled = true;
                    #endregion
                }
                else
                {
                    // Disable the Save button
                    this.btnSave.Enabled = false;

                    DisplayFormLevelError("Sorry, you cannot change the Shift Pattern of the selected employee because he is a Fire Team Member. Please go to Shift Pattern Changes (Fire Team Member) page to change the shift pattern.");
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
                UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY
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
            if (this.CurrentShiftPatternChange == null)
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
                proxy.InsertUpdateDeleteShiftPattern(Convert.ToInt32(UIHelper.SaveType.Delete), 
                    (new List<ShiftPatternEntity>() { this.CurrentShiftPatternChange }), 
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
                        String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ + "?{0}={1}",
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
                GetShiftPatternChange(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = string.Empty;
            this.litPosition.Text = string.Empty;
            this.litCostCenter.Text = string.Empty;
            this.litUpdateUser.Text = string.Empty;
            this.litLastUpdateTime.Text = string.Empty;
            this.litSupervisor.Text = string.Empty;
            this.litCurrentShiftPattern.Text = string.Empty;

            this.dtpEffectiveDate.SelectedDate = null;
            this.dtpEndingDate.SelectedDate = null;

            this.cboFireTeamMeber.Text = string.Empty;
            this.cboFireTeamMeber.SelectedIndex = -1;
            this.cboShiftPatCode.Text = string.Empty;
            this.cboShiftPatCode.SelectedIndex = -1;
            this.cboShiftPointer.Text = string.Empty;
            this.cboShiftPointer.SelectedIndex = -1;
            this.cboChangeType.Text = string.Empty;
            this.cboChangeType.SelectedIndex = -1;
            this.cboChangeType_SelectedIndexChanged(this.cboChangeType, new RadComboBoxSelectedIndexChangedEventArgs(this.cboChangeType.Text, string.Empty, cboChangeType.SelectedValue, string.Empty));

            this.rblOption.SelectedValue = this.CurrentFilterOption.ToString();
            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());

            // Enable the Save button
            this.btnSave.Enabled = true;
            #endregion

            #region Clear sessions
            this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentShiftPatternChange"] = null;
            #endregion
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

                int empNo = 0;

                #region Perform Data Validation

                #region Check selected employee 
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
                    empNo = UIHelper.ConvertObjectToInt(this.cboFireTeamMeber.SelectedValue);
                    if (empNo == 0)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSelectedFireTeam.ToString();
                        this.ErrorType = ValidationErrorType.NoSelectedFireTeam;
                        this.cusValFireTeamMember.Validate();
                        errorCount++;
                    }
                }
                #endregion

                // Check Effective Date
                if (this.dtpEffectiveDate.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEffectiveDate.ToString();
                    this.ErrorType = ValidationErrorType.NoEffectiveDate;
                    this.cusValEffectiveDate.Validate();
                    errorCount++;
                }

                // Check Shift Pattern Code
                if (string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftPatCode.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftPatCode;
                    this.cusValShiftPatCode.Validate();
                    errorCount++;
                }

                // Check Shift Pointer
                if (string.IsNullOrEmpty(this.cboShiftPointer.SelectedValue))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftPointer.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftPointer;
                    this.cusValShiftPointer.Validate();
                    errorCount++;
                }

                // Check Change Type
                if (string.IsNullOrEmpty(this.cboChangeType.SelectedValue))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoChangeType.ToString();
                    this.ErrorType = ValidationErrorType.NoChangeType;
                    this.cusValChangeType.Validate();
                    errorCount++;
                }
                else
                {
                    // Check if Ending Date is specified
                    if (this.cboChangeType.SelectedValue == "T")
                    {
                        if (this.dtpEndingDate.SelectedDate == null)
                        {
                            this.txtGeneric.Text = ValidationErrorType.NoEndingDate.ToString();
                            this.ErrorType = ValidationErrorType.NoEndingDate;
                            this.cusValEndingDate.Validate();
                            errorCount++;
                        }
                    }
                }

                // Check date duration
                if (this.dtpEffectiveDate.SelectedDate != null &&
                    this.dtpEndingDate.SelectedDate != null)
                {
                    if (this.dtpEffectiveDate.SelectedDate > this.dtpEndingDate.SelectedDate)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                        this.ErrorType = ValidationErrorType.InvalidDateRange;
                        this.cusValEffectiveDate.Validate();
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

                if (saveType == UIHelper.SaveType.Insert)
                {
                    #region Perform Insert Operation
                    // Initialize collection
                    List<ShiftPatternEntity> recordToInsertList = new List<ShiftPatternEntity>();

                    recordToInsertList.Add(new ShiftPatternEntity()
                    {
                        EmpNo = empNo,
                        ShiftPatCode = this.cboShiftPatCode.SelectedValue,
                        ShiftPointer = UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue),
                        ChangeType = this.cboChangeType.SelectedValue,
                        EffectiveDate = this.dtpEffectiveDate.SelectedDate,
                        EndingDate = this.dtpEndingDate.SelectedDate,
                        LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                        LastUpdateTime = DateTime.Now
                    });

                    SaveChanges(saveType, recordToInsertList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Update)
                {
                    #region Perform Update Operation
                    // Update data change 
                    this.CurrentShiftPatternChange.ShiftPatCode = this.cboShiftPatCode.SelectedValue;
                    this.CurrentShiftPatternChange.ShiftPointer = UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue);
                    this.CurrentShiftPatternChange.ChangeType = this.cboChangeType.SelectedValue;
                    this.CurrentShiftPatternChange.EffectiveDate = this.dtpEffectiveDate.SelectedDate;
                    this.CurrentShiftPatternChange.EndingDate = this.dtpEndingDate.SelectedDate;
                    this.CurrentShiftPatternChange.LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]));
                    this.CurrentShiftPatternChange.LastUpdateTime = DateTime.Now;

                    // Initialize collection
                    List<ShiftPatternEntity> recordToUpdateList = new List<ShiftPatternEntity>() { this.CurrentShiftPatternChange };

                    SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    // Initialize collection
                    List<ShiftPatternEntity> recordToUpdateList = new List<ShiftPatternEntity>() { this.CurrentShiftPatternChange };

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
                else if (this.ErrorType == ValidationErrorType.NoEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEffectiveDate)
                {
                    validator.ErrorMessage = "Effective Date is required.";
                    validator.ToolTip = "Effective Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEndingDate)
                {
                    validator.ErrorMessage = "Ending Date is required if Change Type is set to temporary.";
                    validator.ToolTip = "Ending Date is required if Change Type is set to temporary.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftPatCode)
                {
                    validator.ErrorMessage = "Shift Pattern Code is required.";
                    validator.ToolTip = "Shift Pattern Code is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftPointer)
                {
                    validator.ErrorMessage = "Shift Pointer is required.";
                    validator.ToolTip = "Shift Pointer is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoChangeType)
                {
                    validator.ErrorMessage = "Change Type is required.";
                    validator.ToolTip = "Change Type is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that Effective Date is less than Ending Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that Effective Date is less than Ending Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record to delete from the grid!";
                    validator.ToolTip = "Please select the record to delete from the grid!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmployeeNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSelectedFireTeam)
                {
                    validator.ErrorMessage = "Please select the Fire Team Member from the list!";
                    validator.ToolTip = "Please select the Fire Team Member from the list!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoCostCenterPermission)
                {
                    validator.ErrorMessage = "Sorry, you don't have access permission to set the shift pattern of the specified employee. Please contact ICT or create a Helpdesk request to grant you cost center permission!";
                    validator.ToolTip = "Sorry, you don't have access permission to set the shift pattern of the specified employee. Please contact ICT or create a Helpdesk request to grant you cost center permission!";
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

        protected void cboShiftPatCode_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue))
            {
                int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string currentUserCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                string[] empNoArray = null;
                string[] costCenterArray = null;

                // Clear Shift Pointer combobox
                this.cboShiftPointer.Items.Clear();
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;

                #region Check if the selected employee is a Fire Team Member
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;
                }

                bool isFireTeamMember = false;
                if (this.CurrentFilterOption == FilterOption.valEmployee &&
                    this.FireTeamMemberList.Count > 0)
                {
                    EmployeeDetail fireTeamMember = this.FireTeamMemberList
                        .Where(a => a.EmpNo == empNo)
                        .FirstOrDefault();
                    isFireTeamMember = fireTeamMember != null;
                }

                if (isFireTeamMember)
                {
                    DisplayFormLevelError("Sorry, you cannot change the Shift Pattern of the selected employee because he is a Fire Team Member. Please go to Shift Pattern Changes (Fire Team Member) page to change the shift pattern.");
                }
                #endregion

                #region Check if the selected Shift Pattern has access restriction
                string shiftPatCode = this.cboShiftPatCode.SelectedValue;

                ShiftPatternEntity shiftPatEntity = this.ShiftPatternCodeList
                    .Where(a => a.ShiftPatCode == shiftPatCode)
                    .FirstOrDefault();
                if (shiftPatEntity != null)
                {
                    if (shiftPatEntity.RestrictionType > 0)
                    {
                        if (shiftPatEntity.RestrictionType == 1 &&
                            !string.IsNullOrEmpty(shiftPatEntity.RestrictedEmpNoArray))
                        {
                            #region Access restricted to specific employee
                            empNoArray = shiftPatEntity.RestrictedEmpNoArray.Split(',');
                            if (empNoArray != null)
                            {
                                if (empNoArray.Where(a => a.Trim() == currentUserEmpNo.ToString()).FirstOrDefault() == null)
                                {
                                    if (!string.IsNullOrEmpty(shiftPatEntity.RestrictionMessage))
                                    {
                                        DisplayFormLevelError(shiftPatEntity.RestrictionMessage);
                                        this.cboShiftPatCode.ClearSelection();
                                        return;
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (shiftPatEntity.RestrictionType == 2 &&
                            !string.IsNullOrEmpty(shiftPatEntity.RestrictedCostCenterArray))
                        {
                            #region Access restricted to specific cost center
                            costCenterArray = shiftPatEntity.RestrictedCostCenterArray.Split(',');
                            if (costCenterArray != null)
                            {
                                if (costCenterArray.Where(a => a.Trim() == currentUserCostCenter.ToString()).FirstOrDefault() == null)
                                {
                                    if (!string.IsNullOrEmpty(shiftPatEntity.RestrictionMessage))
                                    {
                                        DisplayFormLevelError(shiftPatEntity.RestrictionMessage);
                                        this.cboShiftPatCode.ClearSelection();
                                        return;
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (shiftPatEntity.RestrictionType == 3)
                        {
                            bool hasAccess = false;

                            #region Check if current user has access to the selected Shift Pattern Code
                            if (!string.IsNullOrEmpty(shiftPatEntity.RestrictedEmpNoArray))
                            {
                                empNoArray = shiftPatEntity.RestrictedEmpNoArray.Split(',');
                                if (empNoArray != null)
                                {
                                    if (empNoArray.Where(a => a.Trim() == currentUserEmpNo.ToString()).FirstOrDefault() != null)
                                        hasAccess = true;
                                }
                            }
                            #endregion

                            #region Check if current user has access  to the selected Shift Pattern Code based on his cost center
                            if (!hasAccess)
                            {
                                costCenterArray = shiftPatEntity.RestrictedCostCenterArray.Split(',');
                                if (costCenterArray != null)
                                {
                                    if (costCenterArray.Where(a => a.Trim() == currentUserCostCenter.ToString()).FirstOrDefault() != null)
                                        hasAccess = true;
                                }
                            }
                            #endregion

                            if (!hasAccess &&
                                !string.IsNullOrEmpty(shiftPatEntity.RestrictionMessage))
                            {
                                DisplayFormLevelError(shiftPatEntity.RestrictionMessage);
                                this.cboShiftPatCode.ClearSelection();
                                return;
                            }
                        }
                    }
                }
                #endregion

                FillShiftPointerCombo(shiftPatCode);                                
            }
        }

        protected void cboChangeType_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Enable/disable Ending Date
            this.dtpEndingDate.Enabled = this.cboChangeType.SelectedValue == "T";
        }

        protected void rblOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rblOption.SelectedValue == FilterOption.valEmployee.ToString())
            {
                this.panEmployee.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.panFireTeamMember.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdPageTitle.InnerText = "Shift Pattern Changes (Employee)";
            }
            else if (this.rblOption.SelectedValue == FilterOption.valFireTeamMember.ToString())
            {
                this.panEmployee.Style[HtmlTextWriterStyle.Display] = "none";
                this.panFireTeamMember.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.tdPageTitle.InnerText = "Shift Pattern Changes (Fire Team)";
            }
            else
            {
                this.panEmployee.Style[HtmlTextWriterStyle.Display] = "none";
                this.panFireTeamMember.Style[HtmlTextWriterStyle.Display] = "none";
                this.tdPageTitle.InnerText = "Shift Pattern Changes (Data Entry)";
            }
        }

        protected void txtEmpNo_TextChanged(object sender, EventArgs e)
        {
            this.btnGet_Click(this.btnGet, new EventArgs());
        }

        protected void lnkViewShiftPatDetail_Click(object sender, EventArgs e)
        {
            // Check Shift Pattern Code
            if (string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue))
            {
                this.txtGeneric.Text = ValidationErrorType.NoShiftPatCode.ToString();
                this.ErrorType = ValidationErrorType.NoShiftPatCode;
                this.cusValShiftPatCode.Validate();
                return;
            }

            StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_MASTER_SHIFT_PATTERN_SETUP + "?{0}={1}&{2}={3}&{4}={5}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY,
                "ShiftPatCode",
                this.cboShiftPatCode.SelectedValue,
                "IsReadonlyView",
                true.ToString()
            ),
            false);
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
            this.litSupervisor.Text = string.Empty;
            this.litCurrentShiftPattern.Text = string.Empty;

            this.dtpEffectiveDate.SelectedDate = null;
            this.dtpEndingDate.SelectedDate = null;

            this.cboFireTeamMeber.Text = string.Empty;
            this.cboFireTeamMeber.SelectedIndex = -1;
            this.cboShiftPatCode.Text = string.Empty;
            this.cboShiftPatCode.SelectedIndex = -1;
            this.cboShiftPointer.Text = string.Empty;
            this.cboShiftPointer.SelectedIndex = -1;
            this.cboChangeType.Text = string.Empty;
            this.cboChangeType.SelectedIndex = -1;
            this.cboChangeType_SelectedIndexChanged(this.cboChangeType, new RadComboBoxSelectedIndexChangedEventArgs(this.cboChangeType.Text, string.Empty, cboChangeType.SelectedValue, string.Empty));

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
            string shiftPatternType = UIHelper.ConvertObjectToString(Request.QueryString["ShiftPatternType"]);
            if (shiftPatternType != string.Empty)
            {
                FilterOption filterOption = FilterOption.valAll;
                try
                {
                    filterOption = (FilterOption)Enum.Parse(typeof(FilterOption), shiftPatternType);
                }
                catch (Exception)
                {
                }
                this.CurrentFilterOption = filterOption;
            }
            #endregion
        }

        public void KillSessions()
        {
            // Clear collections
            this.ShiftPatternCodeList.Clear();
            this.ShiftPointerCodeList.Clear();
            this.FireTeamMemberList.Clear();

            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentShiftPatternChange"] = null;
            ViewState["CurrentFilterOption"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ShiftPatternEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ShiftPatternEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.ShiftPatternEntryStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["CurrentFormLoadType"]);
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

            // Determine the Shift Pattern Type
            string filterOption = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["CurrentFilterOption"]);
            if (filterOption != string.Empty)
            {
                FilterOption loadType = FilterOption.valAll;
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
            if (this.ShiftPatternEntryStorage.ContainsKey("CurrentShiftPatternChange"))
                this.CurrentShiftPatternChange = this.ShiftPatternEntryStorage["CurrentShiftPatternChange"] as ShiftPatternEntity;
            else
                this.CurrentShiftPatternChange = null;

            if (this.ShiftPatternEntryStorage.ContainsKey("ShiftPatternCodeList"))
                this.ShiftPatternCodeList = this.ShiftPatternEntryStorage["ShiftPatternCodeList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPatternCodeList = null;

            if (this.ShiftPatternEntryStorage.ContainsKey("ShiftPointerCodeList"))
                this.ShiftPointerCodeList = this.ShiftPatternEntryStorage["ShiftPointerCodeList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPointerCodeList = null;

            if (this.ShiftPatternEntryStorage.ContainsKey("FireTeamMemberList"))
                this.FireTeamMemberList = this.ShiftPatternEntryStorage["FireTeamMemberList"] as List<EmployeeDetail>;
            else
                this.FireTeamMemberList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ShiftPatternEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litSupervisor"))
                this.litSupervisor.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litSupervisor"]);
            else
                this.litSupervisor.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litCurrentShiftPattern"))
                this.litCurrentShiftPattern.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litCurrentShiftPattern"]);
            else
                this.litCurrentShiftPattern.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litUpdateUser"))
                this.litUpdateUser.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litUpdateUser"]);
            else
                this.litUpdateUser.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.ShiftPatternEntryStorage.ContainsKey("dtpEffectiveDate"))
                this.dtpEffectiveDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ShiftPatternEntryStorage["dtpEffectiveDate"]);
            else
                this.dtpEffectiveDate.SelectedDate = null;

            if (this.ShiftPatternEntryStorage.ContainsKey("dtpEndingDate"))
                this.dtpEndingDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ShiftPatternEntryStorage["dtpEndingDate"]);
            else
                this.dtpEndingDate.SelectedDate = null;

            if (this.ShiftPatternEntryStorage.ContainsKey("cboShiftPatCode"))
                this.cboShiftPatCode.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["cboShiftPatCode"]);
            else
            {
                this.cboShiftPatCode.Text = string.Empty;
                this.cboShiftPatCode.SelectedIndex = -1;
            }

            if (this.ShiftPatternEntryStorage.ContainsKey("cboShiftPointer"))
                this.cboShiftPointer.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["cboShiftPointer"]);
            else
            {
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;
            }

            if (this.ShiftPatternEntryStorage.ContainsKey("cboChangeType"))
                this.cboChangeType.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["cboChangeType"]);
            else
            {
                this.cboChangeType.Text = string.Empty;
                this.cboChangeType.SelectedIndex = -1;
            }
            this.cboChangeType_SelectedIndexChanged(this.cboChangeType, new RadComboBoxSelectedIndexChangedEventArgs(this.cboChangeType.Text, string.Empty, cboChangeType.SelectedValue, string.Empty));

            if (this.ShiftPatternEntryStorage.ContainsKey("cboFireTeamMeber"))
                this.cboFireTeamMeber.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["cboFireTeamMeber"]);
            else
            {
                this.cboFireTeamMeber.Text = string.Empty;
                this.cboFireTeamMeber.SelectedIndex = -1;
            }

            if (this.ShiftPatternEntryStorage.ContainsKey("rblOption"))
                this.rblOption.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternEntryStorage["rblOption"]);
            else
                this.rblOption.SelectedValue = FilterOption.valEmployee.ToString();

            this.rblOption_SelectedIndexChanged(this.rblOption, new EventArgs());
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ShiftPatternEntryStorage.Clear();
            this.ShiftPatternEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ShiftPatternEntryStorage.Add("rblOption", this.rblOption.SelectedValue);
            this.ShiftPatternEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litSupervisor", this.litSupervisor.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litCurrentShiftPattern", this.litCurrentShiftPattern.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litUpdateUser", this.litUpdateUser.Text.Trim());
            this.ShiftPatternEntryStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());
            this.ShiftPatternEntryStorage.Add("dtpEffectiveDate", this.dtpEffectiveDate.SelectedDate);
            this.ShiftPatternEntryStorage.Add("dtpEndingDate", this.dtpEndingDate.SelectedDate);
            this.ShiftPatternEntryStorage.Add("cboShiftPatCode", this.cboShiftPatCode.SelectedValue);
            this.ShiftPatternEntryStorage.Add("cboShiftPointer", this.cboShiftPointer.SelectedValue);
            this.ShiftPatternEntryStorage.Add("cboChangeType", this.cboChangeType.SelectedValue);
            this.ShiftPatternEntryStorage.Add("cboFireTeamMeber", this.cboFireTeamMeber.SelectedValue);
            this.ShiftPatternEntryStorage.Add("CurrentFilterOption", this.CurrentFilterOption);
            #endregion

            #region Save Query String values to collection
            this.ShiftPatternEntryStorage.Add("CallerForm", this.CallerForm);
            this.ShiftPatternEntryStorage.Add("AutoID", this.AutoID);
            this.ShiftPatternEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.ShiftPatternEntryStorage.Add("CurrentShiftPatternChange", this.CurrentShiftPatternChange);
            this.ShiftPatternEntryStorage.Add("ShiftPatternCodeList", this.ShiftPatternCodeList);
            this.ShiftPatternEntryStorage.Add("ShiftPointerCodeList", this.ShiftPointerCodeList);
            this.ShiftPatternEntryStorage.Add("FireTeamMemberList", this.FireTeamMemberList);
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
            switch (formLoadType)
            {
                case UIHelper.DataLoadTypes.CreateNewRecord:
                    #region Create new record
                    // Setup controls 
                    this.txtEmpNo.Enabled = true;
                    this.dtpEffectiveDate.Enabled = true;
                    this.dtpEndingDate.Enabled = true;
                    this.cboShiftPatCode.Enabled = true;
                    this.cboShiftPointer.Enabled = true;
                    this.cboChangeType.Enabled = true;

                    // Initialize control values
                    this.litEmpName.Text = "Not defined";
                    this.litPosition.Text = "Not defined";
                    this.litCostCenter.Text = "Not defined";
                    this.litUpdateUser.Text = "Not defined";
                    this.litLastUpdateTime.Text = "Not defined";
                    this.litSupervisor.Text = "Not defined";
                    this.litCurrentShiftPattern.Text = "Unknown";
                    this.dtpEffectiveDate.MinDate = DateTime.Now.AddDays(1);                    
                    this.dtpEndingDate.MinDate = DateTime.Now.AddDays(1);
                    this.dtpEffectiveDate.SelectedDate = this.dtpEffectiveDate.MinDate;
                    this.cboChangeType.SelectedValue = "D";

                    // Setup buttons
                    this.btnGet.Enabled = true;
                    this.btnFindEmployee.Enabled = true;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing training record
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.dtpEffectiveDate.Enabled = true;
                    this.dtpEndingDate.Enabled = true;
                    this.cboShiftPatCode.Enabled = true;
                    this.cboShiftPointer.Enabled = true;
                    this.cboChangeType.Enabled = true;

                    // Initialize control values
                    this.dtpEffectiveDate.MinDate = DateTime.MinValue;
                    this.dtpEndingDate.MinDate = DateTime.MinValue;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = true;
                    this.btnReset.Enabled = false;
                    
                    break;
                    #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing training record (read-only)
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.dtpEffectiveDate.Enabled = false;
                    this.dtpEndingDate.Enabled = false;
                    this.cboShiftPatCode.Enabled = false;
                    this.cboShiftPointer.Enabled = false;
                    this.cboChangeType.Enabled = false;

                    // Initialize control values
                    this.dtpEffectiveDate.MinDate = DateTime.MinValue;
                    this.dtpEndingDate.MinDate = DateTime.MinValue;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = false;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = false;

                    break;
                    #endregion
            }
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            FillShiftPatternCodeCombo(reloadFromDB);
            FillFireTeamMemberCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetShiftPatternChange(int autoID)
        {
            try
            {
                #region Initialize controls
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = string.Empty;
                this.litPosition.Text = string.Empty;
                this.litCostCenter.Text = string.Empty;
                this.litUpdateUser.Text = string.Empty;
                this.litLastUpdateTime.Text = string.Empty;
                this.litSupervisor.Text = string.Empty;
                this.litCurrentShiftPattern.Text = string.Empty;

                this.dtpEffectiveDate.SelectedDate = null;
                this.dtpEndingDate.SelectedDate = null;

                this.cboShiftPatCode.Text = string.Empty;
                this.cboShiftPatCode.SelectedIndex = -1;
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;
                this.cboChangeType.Text = string.Empty;
                this.cboChangeType.SelectedIndex = -1;
                this.cboFireTeamMeber.Text = string.Empty;
                this.cboFireTeamMeber.SelectedIndex = -1;
                #endregion

                if (Session["SelectedShiftPatternChange"] != null)
                {
                    this.CurrentShiftPatternChange = Session["SelectedShiftPatternChange"] as ShiftPatternEntity;
                }
                else
                {
                    #region Fetch database record
                    if (autoID == 0)
                        return;

                    List<ShiftPatternEntity> gridSource = new List<ShiftPatternEntity>();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetShiftPatternChanges(autoID, 0, 0, string.Empty, null, null, 0, 0, ref error, ref innerError);
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
                            this.CurrentShiftPatternChange = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentShiftPatternChange != null)
                {
                    this.txtEmpNo.Value =  this.CurrentShiftPatternChange.EmpNo;
                    this.litEmpName.Text = this.CurrentShiftPatternChange.EmpName;
                    this.litPosition.Text = this.CurrentShiftPatternChange.Position;
                    this.litCostCenter.Text = this.CurrentShiftPatternChange.CostCenterFullName;
                    this.litUpdateUser.Text = this.CurrentShiftPatternChange.LastUpdateUser;
                    this.litLastUpdateTime.Text = this.CurrentShiftPatternChange.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentShiftPatternChange.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : string.Empty;
                    this.litSupervisor.Text = this.CurrentShiftPatternChange.SupervisorFullName;

                    this.cboChangeType.SelectedValue = this.CurrentShiftPatternChange.ChangeType;
                    this.cboChangeType_SelectedIndexChanged(this.cboChangeType, new RadComboBoxSelectedIndexChangedEventArgs(this.cboChangeType.Text, string.Empty, cboChangeType.SelectedValue, string.Empty));

                    this.cboShiftPatCode.SelectedValue = this.CurrentShiftPatternChange.ShiftPatCode;
                    if (!string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue))
                    {
                        FillShiftPointerCombo(this.cboShiftPatCode.SelectedValue);
                        //this.cboShiftPatCode_SelectedIndexChanged(this.cboShiftPatCode, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPatCode.Text, string.Empty, this.cboShiftPatCode.SelectedValue, string.Empty));
                    }

                    if (this.cboShiftPointer.Items.Count > 0)
                        this.cboShiftPointer.SelectedValue = this.CurrentShiftPatternChange.ShiftPointer.ToString();

                    this.dtpEffectiveDate.SelectedDate = this.CurrentShiftPatternChange.EffectiveDate;
                    this.dtpEndingDate.SelectedDate = this.CurrentShiftPatternChange.EndingDate;

                    #region Get the shift pattern information
                    string shiftPatternInfo = GetShiftPatternInformation(this.CurrentShiftPatternChange.EmpNo);
                    if (!string.IsNullOrEmpty(shiftPatternInfo))
                    {
                        this.litCurrentShiftPattern.Text = shiftPatternInfo;
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, List<ShiftPatternEntity> shiftPatternList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                // Get WCF Instance
                if (shiftPatternList == null)
                    return;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteShiftPattern(Convert.ToInt32(saveType), shiftPatternList, ref error, ref innerError);
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
                        String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ + "?{0}={1}&{2}={3}",
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
                this.CurrentShiftPatternChange = null;
                throw new Exception(ex.Message.ToString());
            }
        }

        private void FillShiftPatternCodeCombo(bool reloadFromDB, string shiftPatCode = "")
        {
            try
            {
                List<ShiftPatternEntity> comboSource = new List<ShiftPatternEntity>();

                if (this.ShiftPatternCodeList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.ShiftPatternCodeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetShiftPatternCodes(shiftPatCode, ref error, ref innerError);
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
                        }
                    }
                }

                // Store to session
                this.ShiftPatternCodeList = comboSource;

                #region Bind data to combobox
                this.cboShiftPatCode.DataSource = comboSource;
                this.cboShiftPatCode.DataTextField = "ShiftPatDesc";
                this.cboShiftPatCode.DataValueField = "ShiftPatCode";
                this.cboShiftPatCode.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillShiftPointerCombo(string shiftPatCode)
        {
            try
            {                
                string error = string.Empty;
                string innerError = string.Empty;
                List<ShiftPatternEntity> comboSource = new List<ShiftPatternEntity>();

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetShiftPointerCodes(shiftPatCode, ref error, ref innerError);
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
                    }
                }

                // Store to session
                this.ShiftPointerCodeList = comboSource;

                #region Bind data to combobox
                this.cboShiftPointer.DataSource = comboSource;
                this.cboShiftPointer.DataTextField = "ShiftPointerCode";
                this.cboShiftPointer.DataValueField = "ShiftPointer";
                this.cboShiftPointer.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillFireTeamMemberCombo(bool reloadFromDB)
        {
            try
            {
                List<EmployeeDetail> comboSource = new List<EmployeeDetail>();

                if (this.FireTeamMemberList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.FireTeamMemberList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetFireTeamMember(ref error, ref innerError);
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
                        }
                    }
                }

                // Store to session
                this.FireTeamMemberList = comboSource;

                #region Bind data to combobox
                this.cboFireTeamMeber.DataSource = comboSource;
                this.cboFireTeamMeber.DataTextField = "EmpName";
                this.cboFireTeamMeber.DataValueField = "EmpNo";
                this.cboFireTeamMeber.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private string GetShiftPatternInformation(int empNo)
        {
            string result = string.Empty;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                var rawData = dataProxy.GetShiftPatternInfo(empNo, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (rawData != null)
                    {
                        ShiftPatternEntity shiftPatternInfo = rawData.FirstOrDefault();
                        if (shiftPatternInfo != null)
                        {
                            result = string.Format("{0}_{1} = {2} ({3})",
                                shiftPatternInfo.ShiftPatCode,
                                shiftPatternInfo.ShiftPointer,
                                shiftPatternInfo.ShiftCode,
                                DateTime.Now.ToString("dd-MMM-yyyy"));
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        #endregion
                
    }
}