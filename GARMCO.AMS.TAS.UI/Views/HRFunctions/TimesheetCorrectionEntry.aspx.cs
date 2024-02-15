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
using System.Configuration;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class TimesheetCorrectionEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoEmpNo,
            NoSpecifiedEmpNo,
            NoCorrectionCode,
            NoShiftCode,
            NoNPH,
            NoDILEntitlement,
            NoRemarkCode,
            NoOTType,
            NoOTStartTime,
            NoOTEndTime,
            InvalidTimeRange,
            NoRecordToDelete,
            NoTypeOfRelative,
            NoOtherRelativeType
        }

        private enum DeathCorrectionCode
        {
            /// <summary>
            /// Remove Absent Death Others
            /// </summary>
            RAD0,
            /// <summary>
            /// Remove Absent Death 1st Degree
            /// </summary>
            RAD1,
            /// <summary>
            /// Remove Absent Death 2nd Degree
            /// </summary>
            RAD2,
            /// <summary>
            /// Remove Absent Death 3rd Degree
            /// </summary>
            RAD3,
            /// <summary>
            /// Remove Absent Death 4th Degree      
            /// </summary>
            RAD4
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

        private Dictionary<string, object> TimesheetCorrectionEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["TimesheetCorrectionEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["TimesheetCorrectionEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["TimesheetCorrectionEntryStorage"] = value;
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

        private EmployeeAttendanceEntity CurrentAttendanceRecord
        {
            get
            {
                return ViewState["CurrentAttendanceRecord"] as EmployeeAttendanceEntity;
            }
            set
            {
                ViewState["CurrentAttendanceRecord"] = value;
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

        private List<UserDefinedCodes> CorrectionCodeList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["CorrectionCodeList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["CorrectionCodeList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["CorrectionCodeList"] = value;
            }
        }

        private List<UserDefinedCodes> OvertimeTypeList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["OvertimeTypeList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["OvertimeTypeList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["OvertimeTypeList"] = value;
            }
        }

        private List<UserDefinedCodes> ShiftCodeList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["ShiftCodeList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["ShiftCodeList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["ShiftCodeList"] = value;
            }
        }

        private List<RelativeType> RelativeTypeList
        {
            get
            {
                List<RelativeType> list = ViewState["RelativeTypeList"] as List<RelativeType>;
                if (list == null)
                    ViewState["RelativeTypeList"] = list = new List<RelativeType>();

                return list;
            }
            set
            {
                ViewState["RelativeTypeList"] = value;
            }
        }

        private List<RelativeType> FilteredRelativeTypeList
        {
            get
            {
                List<RelativeType> list = ViewState["FilteredRelativeTypeList"] as List<RelativeType>;
                if (list == null)
                    ViewState["FilteredRelativeTypeList"] = list = new List<RelativeType>();

                return list;
            }
            set
            {
                ViewState["FilteredRelativeTypeList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.TSCOREKENT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_TIMESHEET_CORRECTION_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_TIMESHEET_CORRECTION_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Visible = this.Master.IsCreateAllowed;
                //this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSave.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.TimesheetCorrectionEntryStorage.Count > 0)
                {
                    if (this.TimesheetCorrectionEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["FormFlag"]);
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
                    Session.Remove("TimesheetCorrectionEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("TimesheetCorrectionEntryStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    InitializeControls(this.CurrentFormLoadType);

                    #region Check if need to load data in the form
                    if (this.AutoID > 0)
                    {
                        GetTimesheetCorrection(this.AutoID);
                    }
                    #endregion   

                    FillCorrectionCodeCombo(true, UIHelper.UDCSorterColumn.UDCDesc1);
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
                this.litEmpName.Text = "-Not defined-";
                this.litPosition.Text = "-Not defined-";
                this.litCostCenter.Text = "-Not defined-";
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
                UIHelper.PAGE_TIMESHEET_CORRECTION_ENTRY
            ),
            false);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            // Check if there is selected record to delete
            if (this.CurrentAttendanceRecord == null)
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
                //proxy.InsertUpdateDeleteTimesheet(Convert.ToInt32(UIHelper.SaveType.Delete), 
                //    (new List<EmployeeAttendanceEntity>() { this.CurrentAttendanceRecord }).ToArray(), 
                //    ref error, ref innerError);
                //if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                //{
                //    if (!string.IsNullOrEmpty(innerError))
                //        throw new Exception(error, new Exception(innerError));
                //    else
                //        throw new Exception(error);
                //}
                //else
                //{
                //    // Redirect to Shift Pattern Change Inquiry page
                //    Response.Redirect
                //    (
                //        String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_INQUIRY + "?{0}={1}",
                //        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                //        true.ToString()
                //    ),
                //    false);
                //}
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
                GetTimesheetCorrection(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (this.AutoID > 0)
            {
                GetTimesheetCorrection(this.AutoID);
            }
            else
            {
                #region Reset controls
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = "-Not defined-";
                this.litPosition.Text = "-Not defined-";
                this.litCostCenter.Text = "-Not defined-";
                this.litAttendanceDate.Text = string.Empty;
                this.litTimeIn.Text = string.Empty;
                this.litTimeOut.Text = string.Empty;
                this.litLastUpdateUser.Text = string.Empty;
                this.litLastUpdateTime.Text = string.Empty;

                this.txtNPH.Text = string.Empty;
                this.txtShiftCode.Text = string.Empty;
                this.litActualShiftCode.Text = string.Empty;
                this.chkShiftAllowance.Checked = false;
                this.dtpStartTime.SelectedDate = null;
                this.dtpStartTimeMirror.SelectedDate = null;
                this.dtpEndTime.SelectedDate = null;
                this.dtpEndTimeMirror.SelectedDate = null;
                this.dtpNPH.SelectedDate = null;

                this.cboCorrectionCode.SelectedIndex = -1;
                this.cboCorrectionCode.Text = string.Empty;
                this.cboCorrectionCode.Enabled = true;
                this.cboOTType.SelectedIndex = -1;
                this.cboOTType.Text = string.Empty;
                this.cboDILEntitlement.SelectedIndex = -1;
                this.cboDILEntitlement.Text = string.Empty;
                this.cboRemarkCode.SelectedIndex = -1;
                this.cboRemarkCode.Text = string.Empty;
                this.cboShiftCode.SelectedIndex = -1;
                this.cboShiftCode.Text = string.Empty;

                #region Initialive Remove Family Death related controls
                this.cboRelativeType.SelectedIndex = -1;
                this.cboRelativeType.Text = string.Empty;
                this.cboRelativeType.Items.Clear();
                this.FilteredRelativeTypeList.Clear();
                //this.txtRemarks.BackColor = System.Drawing.Color.White;
                this.txtRemarks.Text = string.Empty;
                this.trRelativeType.Style[HtmlTextWriterStyle.Display] = "none";
                #endregion
                
                #endregion

                #region Clear sessions
                this.CurrentFormLoadType = UIHelper.DataLoadTypes.EditExistingRecord;
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentAttendanceRecord"] = null;
                ViewState["EmployeeNo"] = null;
                #endregion
            }                       

            InitializeControls(this.CurrentFormLoadType);

            #region Reset control's backcolor
            this.cboOTType.BackColor = System.Drawing.Color.Transparent;
            this.cboDILEntitlement.BackColor = System.Drawing.Color.Transparent;
            this.cboRemarkCode.BackColor = System.Drawing.Color.Transparent;
            this.dtpStartTime.BackColor = System.Drawing.Color.Transparent;
            this.dtpEndTime.BackColor = System.Drawing.Color.Transparent;
            this.dtpNPH.BackColor = System.Drawing.Color.Transparent;
            this.txtNPH.BackColor = System.Drawing.Color.Transparent;
            this.txtShiftCode.BackColor = System.Drawing.Color.Transparent;
            this.chkShiftAllowance.BackColor = System.Drawing.Color.Transparent;
            this.cboShiftCode.BackColor = System.Drawing.Color.Transparent;
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

                #region Perform Data Validation
                               
                #region Check Employee No. if specified
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
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
                #endregion

                #region Check Correction Code if specified
                if (string.IsNullOrEmpty(this.cboCorrectionCode.SelectedValue))
                {
                    this.txtGeneric.Text = ValidationErrorType.NoCorrectionCode.ToString();
                    this.ErrorType = ValidationErrorType.NoCorrectionCode;
                    this.cusValCorrectionCode.Validate();
                    errorCount++;
                }
                #endregion

                #region Check value of all mandatory fields

                #region Check if OT Type is mandatory
                if (this.cboOTType.BackColor.Name == System.Drawing.Color.Yellow.Name &&
                    (string.IsNullOrEmpty(this.cboOTType.SelectedValue) || this.cboOTType.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID) &&
                    this.cboOTType.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoOTType.ToString();
                    this.ErrorType = ValidationErrorType.NoOTType;
                    this.cusValOTType.Validate();
                    errorCount++;
                }
                #endregion

                #region Check if OT Start Time is mandatory
                if (this.dtpStartTime.ToolTip == "REQUIRED" &&
                    this.dtpStartTime.SelectedDate == null &&
                    this.dtpStartTime.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoOTStartTime.ToString();
                    this.ErrorType = ValidationErrorType.NoOTStartTime;
                    this.cusValStartTime.Validate();
                    errorCount++;
                }
                #endregion

                #region Check if OT End Time is mandatory
                if (this.dtpEndTime.ToolTip == "REQUIRED" &&
                    this.dtpEndTime.SelectedDate == null &&
                    this.dtpEndTime.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoOTEndTime.ToString();
                    this.ErrorType = ValidationErrorType.NoOTEndTime;
                    this.cusValEndTime.Validate();
                    errorCount++;
                }
                #endregion

                #region Check if No Pay Hours is mandatory
                if (this.dtpNPH.ToolTip == "REQUIRED" &&
                   this.dtpNPH.SelectedDate == null &&
                   this.dtpNPH.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoNPH.ToString();
                    this.ErrorType = ValidationErrorType.NoNPH;
                    this.cusValNPH.Validate();
                    errorCount++;
                }

                //if (this.txtNPH.ToolTip == "REQUIRED" &&
                //    UIHelper.ConvertObjectToInt(this.txtNPH.Text) == 0 &&
                //    this.txtNPH.Enabled)
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoNPH.ToString();
                //    this.ErrorType = ValidationErrorType.NoNPH;
                //    this.cusValNPH.Validate();
                //    errorCount++;
                //}
                #endregion

                #region Check if Shift Code is mandatory
                //if (this.txtShiftCode.BackColor.Name == System.Drawing.Color.Yellow.Name &&
                //   this.txtShiftCode.Text == string.Empty &&
                //   this.txtShiftCode.Enabled)
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoShiftCode.ToString();
                //    this.ErrorType = ValidationErrorType.NoShiftCode;
                //    this.cusValShiftCode.Validate();
                //    errorCount++;
                //}

                if (this.cboShiftCode.BackColor.Name == System.Drawing.Color.Yellow.Name &&
                   (string.IsNullOrEmpty(this.cboShiftCode.SelectedValue) || this.cboShiftCode.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID) &&
                   this.cboShiftCode.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftCode.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftCode;
                    this.cusValShiftCode.Validate();
                    errorCount++;
                }
                #endregion

                #region Check if DIL Entitlement is mandatory
                if (this.cboDILEntitlement.BackColor.Name == System.Drawing.Color.Yellow.Name &&
                   string.IsNullOrEmpty(this.cboDILEntitlement.SelectedValue) &&
                   this.cboDILEntitlement.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDILEntitlement.ToString();
                    this.ErrorType = ValidationErrorType.NoDILEntitlement;
                    this.cusvalDILEntitlement.Validate();
                    errorCount++;
                }
                #endregion

                #region Check if Remark Code is mandatory
                if (this.cboRemarkCode.BackColor.Name == System.Drawing.Color.Yellow.Name &&
                   string.IsNullOrEmpty(this.cboRemarkCode.SelectedValue) &&
                   this.cboRemarkCode.Enabled)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoRemarkCode.ToString();
                    this.ErrorType = ValidationErrorType.NoRemarkCode;
                    this.cusValRemarkCode.Validate();
                    errorCount++;
                }
                #endregion

                #endregion

                #region Validate data input

                #region Validate overtime related correction code
                if (this.cboCorrectionCode.SelectedValue.Trim() == "AOMA" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COCA" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COCS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "AOBT" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "AOCS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "ACS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "AL" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "BD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CAL" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CBD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CCS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CDF" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COEW" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COMS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CSR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "DF" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "EW" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "MA" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "MS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "PD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "PH" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "PM" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "SD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "SR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "TR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "ROT")
                {
                    if (this.CurrentAttendanceRecord.RemarkCode == "A")
                    {
                        this.CustomErrorMsg = "Cannot add overtime if employee is absent.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    else if (this.CurrentAttendanceRecord.NoPayHours > 0)
                    {
                        this.CustomErrorMsg = "Cannot add overtime if employee has no pay hours.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    //else if (!this.CurrentAttendanceRecord.IsLastRow)
                    //{
                    //    this.CustomErrorMsg = "Please note that overtime can only be added in the last record!";
                    //    this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                    //    this.ErrorType = ValidationErrorType.CustomFormError;
                    //    this.cusValButton.Validate();
                    //    errorCount++;
                    //}
                }
                #endregion

                #region Validate No Pay Hour related correction codes
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ANAD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CNWP")
                {
                    if (this.CurrentAttendanceRecord.RemarkCode == "A")
                    {
                        this.CustomErrorMsg = "Cannot add No Pay Hour if employee is absent.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    else if (this.CurrentAttendanceRecord.OTStartTime != null && this.CurrentAttendanceRecord.OTEndTime != null)
                    {
                        this.CustomErrorMsg = "Cannot add No Pay Hour if employee has overtime.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    //else if (this.CurrentAttendanceRecord.OTStartTimeTE != null && this.CurrentAttendanceRecord.OTEndTimeTE != null)
                    //{
                    //    this.CustomErrorMsg = "Cannot add No Pay Hour if employee has overtime pending for approval.";
                    //    this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                    //    this.ErrorType = ValidationErrorType.CustomFormError;
                    //    this.cusValButton.Validate();
                    //    errorCount++;
                    //}
                }
                #endregion

                #region Validate Shift Allowance related correction codes
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ASES" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "ASNS")
                {
                    if (this.CurrentAttendanceRecord.RemarkCode == "A")
                    {
                        this.CustomErrorMsg = "Cannot add Shift Allowance if employee is absent.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    else if (!this.CurrentAttendanceRecord.IsLastRow)
                    {
                        this.CustomErrorMsg = "Please note that shift allowance can only be added in the last record!";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate absent marking correction codes
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "MACL" ||   // Mark Absent Leave Cancelled
                    this.cboCorrectionCode.SelectedValue.Trim() == "MACS" ||        // Mark Absent-Change Shift
                    this.cboCorrectionCode.SelectedValue.Trim() == "MADA" ||        // Mark Absent-Disciplinary Action
                    this.cboCorrectionCode.SelectedValue.Trim() == "MARO" ||        // Mark Absent - Remove Dayoff
                    this.cboCorrectionCode.SelectedValue.Trim() == "MAGS")          // Mark Absent During Gen. Strike
                {
                    if (this.CurrentAttendanceRecord.NoPayHours > 0)
                    {
                        this.CustomErrorMsg = "Cannot mark absent if employee has no pay hours.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    else if (this.CurrentAttendanceRecord.OTStartTime != null &&
                        this.CurrentAttendanceRecord.OTEndTime != null)
                    {
                        this.CustomErrorMsg = "Cannot mark absent if employee has overtime.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    else if (this.CurrentAttendanceRecord.ShiftAllowance == true)
                    {
                        this.CustomErrorMsg = "Cannot mark absent if employee has shift allowance.";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                    //else if (!this.CurrentAttendanceRecord.IsLastRow)
                    //{
                    //    this.CustomErrorMsg = "Please note that the attendance correction can only be applied in the last row!";
                    //    this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                    //    this.ErrorType = ValidationErrorType.CustomFormError;
                    //    this.cusValButton.Validate();
                    //    errorCount++;
                    //}
                }
                #endregion

                #region Validate absent removal correction codes
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAAP"      // Remove Absent Access Problem 
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RABT"        // Remove Absent Business Trip
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RACB"        // Remove Absent-Child Birth
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RACS"        // Remove Absent-Change Shift
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADF"        // Remove Absent-Death of Family
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADL"        // Remove Absent DIL
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADO"        // Remove Absent-Day Off
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADP"        // Remove Absent Deducted Payroll
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAEA"        // Remove Absent-Excused
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAGD"        // Remove Absent-Give DIL
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAJC"        // Remove Absent-Attend Trade U.
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RALE"        // Remove Absent Leave Entered
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAMT"        // Remove Absent-Manual Timesheet
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAPH"        // Remove absent - Public Holiday
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASA"        // Remove Absent Special Assignmt
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASL"        // Remove Absent Sick Leave
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASP"        // Remove Absent - Change Shift P
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASR"        // Remove Absent Sec. Restriction
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAST")       // Remove Absent Sport Team)
                {
                    //if (!this.CurrentAttendanceRecord.IsLastRow)
                    //{
                    //    this.CustomErrorMsg = "Please note that attendance correction can only be applied in the last row!";
                    //    this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                    //    this.ErrorType = ValidationErrorType.CustomFormError;
                    //    this.cusValButton.Validate();
                    //    errorCount++;
                    //}
                }
                #endregion

                #region Validate "Half Day Leave" correction code
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "HD")        
                {
                    if (!this.CurrentAttendanceRecord.IsLastRow)
                    {
                        this.CustomErrorMsg = "Please note that attendance correction can only be applied in the last row!";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate "Mark DIL-Entitled by Admin" correction code
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "MDEA")        
                {
                    if (!this.CurrentAttendanceRecord.IsLastRow)
                    {
                        this.CustomErrorMsg = "Please note that DIL Entitlement can only be applied in the last row!";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate "Add Meal Voucher" correction code
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ADDM")
                {
                    if (!this.CurrentAttendanceRecord.IsLastRow)
                    {
                        this.CustomErrorMsg = "Please note that meal moucher eligibility can only be applied in the last row!";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate "Local Seminar/Exhibition" correction code
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ALSE")
                {
                    if (!this.CurrentAttendanceRecord.IsLastRow)
                    {
                        this.CustomErrorMsg = "Please note that attendance correction can only be applied in the last row!";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate "Add Extra Pay-Adj last month" correction code
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAAD")
                {
                    if (!this.CurrentAttendanceRecord.IsLastRow)
                    {
                        this.CustomErrorMsg = "Please note that attendance correction can only be applied in the last row!";
                        this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
                        this.ErrorType = ValidationErrorType.CustomFormError;
                        this.cusValButton.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate "Remove Absent Death 1st Degree", "Remove Absent Death 2nd Degree", "Remove Absent Death 3rd Degree", "Remove Absent Death 4th Degree" correction codes
                else if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD1.ToString() ||
                    this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD2.ToString() ||
                    this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD3.ToString() ||
                    this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD4.ToString())
                {
                    if (string.IsNullOrEmpty(this.cboRelativeType.SelectedValue) ||
                        this.cboRelativeType.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoTypeOfRelative.ToString();
                        this.ErrorType = ValidationErrorType.NoTypeOfRelative;
                        this.cusValRelativeType.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Validate "Remove Absent Death Others" correction code
                else if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD0.ToString())
                {
                    if (this.txtOtherRelative.Text.Trim() == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoOtherRelativeType.ToString();
                        this.ErrorType = ValidationErrorType.NoOtherRelativeType;
                        this.cusValRelativeType.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #endregion

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                DateTime? combinedOTStartTime = null;
                DateTime? combinedOTEndTime = null;

                // Save the coorection code to the collection
                this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;

                if (saveType == UIHelper.SaveType.Insert)
                {
                    #region Perform Insert Operation
                    // Initialize collection
                    List<EmployeeAttendanceEntity> recordToInsertList = new List<EmployeeAttendanceEntity>();

                    //recordToInsertList.Add(new EmployeeAttendanceEntity()
                    //{
                    //    EmpNo = empNo,
                    //    EffectiveDate = this.dtpEffectiveDate.SelectedDate,
                    //    EndingDate = this.dtpEndingDate.SelectedDate,
                    //    StartTime = this.dtpStartTime.SelectedDate,
                    //    EndTime = this.dtpEndTime.SelectedDate,
                    //    DayOfWeek = string.IsNullOrEmpty(this.cboOTType.SelectedValue) ? null : this.cboOTType.SelectedValue.Trim(),
                    //    AbsenceReasonCode = this.cboCorrectionCode.SelectedValue,
                    //    XID_TS_DIL_ENT = null,
                    //    XID_TS_DIL_USD = null,
                    //    DIL_ENT_CODE = null,
                    //    LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                    //    LastUpdateTime = DateTime.Now
                    //});

                    //SaveChanges(saveType, recordToInsertList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Update)
                {
                    #region Perform Update Operation

                    #region Update the entity                                        
                    if (this.cboCorrectionCode.SelectedValue.Trim() == "AOBT" ||    // Add OT Busines Trip during Eid
                        this.cboCorrectionCode.SelectedValue.Trim() == "AOCS" ||    // Add Overtime-Change Shift
                        this.cboCorrectionCode.SelectedValue.Trim() == "AOMA" ||    // Add OT manager approved
                        this.cboCorrectionCode.SelectedValue.Trim() == "COCA" ||    // Change Overtime-Call Out
                        this.cboCorrectionCode.SelectedValue.Trim() == "COCS")      // Change Overtime-Change Shift
                    {
                        #region Overtime Correction Codes
                        
                        #region Calculate the OT Start Time
                        if (this.dtpStartTime.SelectedDate.HasValue &&
                            this.dtpStartTimeMirror.SelectedDate.HasValue)
                        {
                            DateTime otDateTimePart = this.dtpStartTime.SelectedDate.Value;
                            DateTime otDatePart = this.dtpStartTimeMirror.SelectedDate.Value.Date;

                            combinedOTStartTime = otDatePart.Add(new TimeSpan(otDateTimePart.Hour, otDateTimePart.Minute, otDateTimePart.Second));
                        }
                        else
                        {                            
                            combinedOTStartTime = this.dtpStartTime.SelectedDate;
                        }
                        #endregion

                        #region Calculate the OT End Time
                        if (this.dtpEndTime.SelectedDate.HasValue &&
                            this.dtpEndTimeMirror.SelectedDate.HasValue)
                        {
                            DateTime otDateTimePart = this.dtpEndTime.SelectedDate.Value;
                            DateTime endDateMirror = this.dtpEndTimeMirror.SelectedDate.Value.Date;

                            combinedOTEndTime = endDateMirror.Add(new TimeSpan(otDateTimePart.Hour, otDateTimePart.Minute, otDateTimePart.Second));
                        }
                        else
                        {
                            combinedOTEndTime = this.dtpEndTime.SelectedDate;
                        }
                        #endregion

                        #region Validate the OT start and end edate values 
                        DateTime? attendanceDate = UIHelper.ConvertObjectToDate(this.litAttendanceDate.Text);
                        if (attendanceDate.HasValue && 
                            combinedOTStartTime.HasValue &&
                            combinedOTEndTime.HasValue)
                        {
                            #region Check if the difference between the attendance date and OT start date is more than 1 day
                            double dayDifference = Math.Abs((Convert.ToDateTime(combinedOTStartTime).Date - Convert.ToDateTime(attendanceDate)).TotalDays);
                            if (dayDifference > 1)
                            {
                                // Check if overtime is for night shift
                                TimeSpan otStartTimePart = Convert.ToDateTime(combinedOTStartTime).TimeOfDay;
                                TimeSpan otEndTimePart = Convert.ToDateTime(combinedOTEndTime).TimeOfDay;
                                if (otStartTimePart > otEndTimePart)
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).AddDays(-1).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                                else
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                            }
                            #endregion

                            #region Check if the difference between the OT Start Time and OT End Time is more than 1 day
                            double dayDifference2 = Math.Abs((Convert.ToDateTime(combinedOTStartTime).Date - Convert.ToDateTime(combinedOTEndTime)).TotalDays);
                            if (dayDifference2 > 1)
                            {
                                // Check if overtime is for night shift
                                TimeSpan otStartTimePart = Convert.ToDateTime(combinedOTStartTime).TimeOfDay;
                                TimeSpan otEndTimePart = Convert.ToDateTime(combinedOTEndTime).TimeOfDay;
                                if (otStartTimePart > otEndTimePart)
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).AddDays(-1).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                                else
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                            }
                            #endregion
                        }
                        #endregion

                        if (combinedOTStartTime != null &&
                            combinedOTEndTime != null)
                        {
                            double otDuration = (new DateTime(combinedOTEndTime.Value.Year, combinedOTEndTime.Value.Month, combinedOTEndTime.Value.Day, combinedOTEndTime.Value.Hour, combinedOTEndTime.Value.Minute, 0) - new DateTime(combinedOTStartTime.Value.Year, combinedOTStartTime.Value.Month, combinedOTStartTime.Value.Day, combinedOTStartTime.Value.Hour, combinedOTStartTime.Value.Minute, 0)).TotalMinutes;

                            #region Check if overtime duration exceeds the limit
                            int maxOTMinutes = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["MaxOTMinutes"]);
                            if (maxOTMinutes > 0)
                            {                                
                                if (otDuration >= maxOTMinutes)
                                {
                                    throw new Exception(string.Format("The specified overtime duration should not be equal to or greater than the maximum limit which is set to {0} hours.", maxOTMinutes / 60));
                                }
                            }
                            #endregion

                            #region Check if overtime duration is greater than the total work duration
                            //if (this.CurrentAttendanceRecord.Duration_Worked_Cumulative > 0 &&
                            //    otDuration > 0 &&
                            //    otDuration > this.CurrentAttendanceRecord.Duration_Worked_Cumulative)
                            //{
                            //    if (this.CurrentAttendanceRecord.IsDriver ||
                            //        this.CurrentAttendanceRecord.IsLiasonOfficer ||
                            //        this.CurrentAttendanceRecord.IsHedger)
                            //    {
                            //        // Bypass checking of overtime duration
                            //    }
                            //    else
                            //        throw new Exception("The specified overtime duration should not be greater than the total work duration.");
                            //}
                            #endregion
                        }

                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.OTType = this.cboOTType.SelectedValue;
                        this.CurrentAttendanceRecord.OTStartTime = combinedOTStartTime; 
                        this.CurrentAttendanceRecord.OTEndTime = combinedOTEndTime; 
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ACS" ||    // Add OT Change Shift
                        this.cboCorrectionCode.SelectedValue.Trim() == "AL" ||          // Annual Leave
                        this.cboCorrectionCode.SelectedValue.Trim() == "BD" ||          // Break Down
                        this.cboCorrectionCode.SelectedValue.Trim() == "CAL" ||         // Call out Annual Leave
                        this.cboCorrectionCode.SelectedValue.Trim() == "CBD" ||         // Call out Break Down
                        this.cboCorrectionCode.SelectedValue.Trim() == "CCS" ||         // Change OT Change Shift
                        this.cboCorrectionCode.SelectedValue.Trim() == "CDF" ||         // Call out Family Death
                        this.cboCorrectionCode.SelectedValue.Trim() == "COEW" ||        // Call Out Extra Work
                        this.cboCorrectionCode.SelectedValue.Trim() == "COMS" ||        // Call Out Manpower Shortage
                        this.cboCorrectionCode.SelectedValue.Trim() == "CSR" ||         // Call out Sick
                        this.cboCorrectionCode.SelectedValue.Trim() == "DF" ||          // Family Death
                        this.cboCorrectionCode.SelectedValue.Trim() == "EW" ||          // Extra Work/ Special Task
                        this.cboCorrectionCode.SelectedValue.Trim() == "MA" ||          // Add OT Manager Approval
                        this.cboCorrectionCode.SelectedValue.Trim() == "MS" ||          // Manpower Shortage
                        this.cboCorrectionCode.SelectedValue.Trim() == "PD" ||          // Project / Development
                        this.cboCorrectionCode.SelectedValue.Trim() == "PH" ||          // Public Holiday
                        this.cboCorrectionCode.SelectedValue.Trim() == "PM" ||          // Planned Maintenance
                        this.cboCorrectionCode.SelectedValue.Trim() == "SD" ||          // Shutdown
                        this.cboCorrectionCode.SelectedValue.Trim() == "SR" ||          // Leave (Sick,Injury,Light Duty)
                        this.cboCorrectionCode.SelectedValue.Trim() == "TR" ||          // Training
                        this.cboCorrectionCode.SelectedValue.Trim() == "ROT")            // OT for Ramadan
                    {
                        #region New Overtime Reason Codes

                        #region Calculate the OT Start Time
                        if (this.dtpStartTime.SelectedDate.HasValue &&
                            this.dtpStartTimeMirror.SelectedDate.HasValue)
                        {
                            DateTime otDateTimePart = this.dtpStartTime.SelectedDate.Value;
                            DateTime otDatePart = this.dtpStartTimeMirror.SelectedDate.Value.Date;

                            combinedOTStartTime = otDatePart.Add(new TimeSpan(otDateTimePart.Hour, otDateTimePart.Minute, otDateTimePart.Second));
                        }
                        else
                        {
                            combinedOTStartTime = this.dtpStartTime.SelectedDate;
                        }
                        #endregion

                        #region Calculate the OT End Time
                        if (this.dtpEndTime.SelectedDate.HasValue &&
                            this.dtpEndTimeMirror.SelectedDate.HasValue)
                        {
                            DateTime otDateTimePart = this.dtpEndTime.SelectedDate.Value;
                            DateTime endDateMirror = this.dtpEndTimeMirror.SelectedDate.Value.Date;

                            combinedOTEndTime = endDateMirror.Add(new TimeSpan(otDateTimePart.Hour, otDateTimePart.Minute, otDateTimePart.Second));
                        }
                        else
                        {
                            combinedOTEndTime = this.dtpEndTime.SelectedDate;
                        }
                        #endregion

                        #region Validate the OT start and end edate values 
                        DateTime? attendanceDate = UIHelper.ConvertObjectToDate(this.litAttendanceDate.Text);
                        if (attendanceDate.HasValue &&
                            combinedOTStartTime.HasValue &&
                            combinedOTEndTime.HasValue)
                        {
                            #region Check if the difference between the attendance date and OT start date is more than 1 day
                            double dayDifference = Math.Abs((Convert.ToDateTime(combinedOTStartTime).Date - Convert.ToDateTime(attendanceDate)).TotalDays);
                            if (dayDifference > 1)
                            {
                                // Check if overtime is for night shift
                                TimeSpan otStartTimePart = Convert.ToDateTime(combinedOTStartTime).TimeOfDay;
                                TimeSpan otEndTimePart = Convert.ToDateTime(combinedOTEndTime).TimeOfDay;
                                if (otStartTimePart > otEndTimePart)
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).AddDays(-1).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                                else
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                            }
                            #endregion

                            #region Check if the difference between the OT Start Time and OT End Time is more than 1 day
                            double dayDifference2 = Math.Abs((Convert.ToDateTime(combinedOTStartTime).Date - Convert.ToDateTime(combinedOTEndTime)).TotalDays);
                            if (dayDifference2 > 1)
                            {
                                // Check if overtime is for night shift
                                TimeSpan otStartTimePart = Convert.ToDateTime(combinedOTStartTime).TimeOfDay;
                                TimeSpan otEndTimePart = Convert.ToDateTime(combinedOTEndTime).TimeOfDay;
                                if (otStartTimePart > otEndTimePart)
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).AddDays(-1).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                                else
                                {
                                    combinedOTStartTime = Convert.ToDateTime(attendanceDate).Add(otStartTimePart);
                                    combinedOTEndTime = Convert.ToDateTime(attendanceDate).Add(otEndTimePart);
                                }
                            }
                            #endregion
                        }
                        #endregion

                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.OTType = this.cboOTType.SelectedValue;
                        this.CurrentAttendanceRecord.OTStartTime = combinedOTStartTime; // this.dtpStartTime.SelectedDate;
                        this.CurrentAttendanceRecord.OTEndTime = combinedOTEndTime; // this.dtpEndTime.SelectedDate;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ROAL" ||   // Remove OT-against last month
                        this.cboCorrectionCode.SelectedValue.Trim() == "ROCS" ||        // Remove OT-Change Shift
                        this.cboCorrectionCode.SelectedValue.Trim() == "RODO" ||        // Remove OT-Day Off
                        this.cboCorrectionCode.SelectedValue.Trim() == "ROMA")          // Remove OT Manager approval
                    {
                        #region Overtime removal correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.OTType = null;
                        this.CurrentAttendanceRecord.OTStartTime = null;
                        this.CurrentAttendanceRecord.OTEndTime = null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ANAD" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "CNWP" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNAP" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNCB" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNCS" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNDF" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNDP" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNLE" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNMR" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNOP" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNSL" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNSO" ||
                        this.cboCorrectionCode.SelectedValue.Trim() == "RNST")
                    {
                        #region No Pay Hour related correction codes
                        int nphMinutes = 0;
                        if (this.dtpNPH.SelectedDate != null)
                            nphMinutes = ReportHelper.ConvertHourStringToMinutes(this.dtpNPH.SelectedDate.Value.TimeOfDay.ToString());

                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.NoPayHours = nphMinutes;   // UIHelper.ConvertObjectToInt(this.txtNPH.Text);
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ASES")     
                    {
                        #region Add Sh Allw Evening-Chng Shift                                                
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftAllowance = true;
                        this.CurrentAttendanceRecord.DurationShiftAllowanceEvening = 250;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ASNS")     
                    {
                        #region Add Sh Allw Night-Chng Shift
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftAllowance = true;
                        this.CurrentAttendanceRecord.DurationShiftAllowanceNight = 250;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RSES")     
                    {
                        #region Remove Shift Allow-evening shf
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftAllowance = false;
                        this.CurrentAttendanceRecord.DurationShiftAllowanceEvening = 0;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RSNS")     
                    {
                        #region Remove Shift Allow-night shift
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftAllowance = false;
                        this.CurrentAttendanceRecord.DurationShiftAllowanceNight = 0;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RSNE")     
                    {
                        #region Remove Shift Allo-not entitled
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftAllowance = false;
                        this.CurrentAttendanceRecord.DurationShiftAllowanceEvening = 0;
                        this.CurrentAttendanceRecord.DurationShiftAllowanceNight = 0;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "MACL" ||   // Mark Absent Leave Cancelled
                       this.cboCorrectionCode.SelectedValue.Trim() == "MACS" ||         // Mark Absent-Change Shift
                       this.cboCorrectionCode.SelectedValue.Trim() == "MADA" ||         // Mark Absent-Disciplinary Action
                       this.cboCorrectionCode.SelectedValue.Trim() == "MARO" ||         // Mark Absent - Remove Dayoff
                       this.cboCorrectionCode.SelectedValue.Trim() == "MAGS")           // Mark Absent During Gen. Strike
                    {
                        #region Process all absent marking correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.RemarkCode = this.cboRemarkCode.SelectedValue;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAAP"      // Remove Absent Access Problem 
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RABT"        // Remove Absent Business Trip
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RACB"        // Remove Absent-Child Birth
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RACS"        // Remove Absent-Change Shift
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RADF"        // Remove Absent-Death of Family
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RADL"        // Remove Absent DIL
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RADO"        // Remove Absent-Day Off
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RADP"        // Remove Absent Deducted Payroll
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RAEA"        // Remove Absent-Excused
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RAGD"        // Remove Absent-Give DIL
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RAJC"        // Remove Absent-Attend Trade U.
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RALE"        // Remove Absent Leave Entered
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RAMT"        // Remove Absent-Manual Timesheet
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RAPH"        // Remove absent - Public Holiday
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RASA"        // Remove Absent Special Assignmt
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RASL"        // Remove Absent Sick Leave
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RASP"        // Remove Absent - Change Shift P
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RASR"        // Remove Absent Sec. Restriction
                        || this.cboCorrectionCode.SelectedValue.Trim() == "RAST")       // Remove Absent Sport Team)
                    {
                        #region Process all absent removal correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.RemarkCode = null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "HD")        
                    {
                        #region Process "Half Day Leave" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.RemarkCode = null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RWLC")       
                    {
                        #region Process "Leave Cancelled" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "MDEA" ||   // Mark DIL-Entitled by Admin
                        this.cboCorrectionCode.SelectedValue.Trim() == "RDEA")          // Remove DIL-Entitled by Admin
                    {
                        #region Process DIL related correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.DILEntitlement = this.cboDILEntitlement.SelectedValue != UIHelper.CONST_COMBO_EMTYITEM_ID 
                            ? this.cboDILEntitlement.SelectedValue
                            : null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ADDM" ||   // Add Meal Voucher
                        this.cboCorrectionCode.SelectedValue.Trim() == "RMVD")          // Remove Meal Voucher Duplicate
                    {
                        #region Process Meal Voucher related correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "MOCS") 
                    {
                        #region Process "Mark Off Change Shift" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftCode = this.cboShiftCode.SelectedValue; //this.txtShiftCode.Text.Trim();
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "ALSE")
                    {
                        #region Process "Local Seminar/Exhibition" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.RemarkCode = null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAAD")
                    {
                        #region Process "Add Extra Pay-Adj last month" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.RemarkCode = null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "CSS")
                    {
                        #region Process "Change Scheduled Shift" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.ShiftCode = this.cboShiftCode.SelectedValue;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue.Trim() == "RRAB")
                    {
                        #region Process "Remove Reason of Absence" correction code
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                     else if (this.cboCorrectionCode.SelectedValue.Trim() == "RDUL" ||  // Remove Dayoff - Mark Unpaid Leave
                       this.cboCorrectionCode.SelectedValue.Trim() == "RDSL" ||         // Remove Dayoff - Mark Unpaid Sick Leave
                       this.cboCorrectionCode.SelectedValue.Trim() == "RDIL")           // Remove Dayoff - Mark Unpaid Injury Leave
                    {
                        #region Process all removal of day-off and unpaid leave marking correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    else if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD0.ToString() ||     // Remove Absent Death Others 
                        this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD1.ToString() ||          // Remove Absent Death 1st Degree
                        this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD2.ToString() ||          // Remove Absent Death 2nd Degree
                        this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD3.ToString() ||          // Remove Absent Death 3rd Degree
                        this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD4.ToString())            // Remove Absent Death 4th Degree
                    {
                        #region Process death related absent removal correction codes
                        this.CurrentAttendanceRecord.CorrectionCode = this.cboCorrectionCode.SelectedValue;
                        this.CurrentAttendanceRecord.RemarkCode = null;
                        this.CurrentAttendanceRecord.LastUpdateUser = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        #endregion
                    }
                    #endregion

                    // Commit changes in the database
                    SaveChanges(saveType, this.CurrentAttendanceRecord);

                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    SaveChanges(saveType, this.CurrentAttendanceRecord);
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
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedEmpNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoCorrectionCode)
                {
                    validator.ErrorMessage = "Correction Code is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Correction Code is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoOTType)
                {
                    validator.ErrorMessage = "Overtime Type is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Overtime Type is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoOTStartTime)
                {
                    validator.ErrorMessage = "Overtime Start Time is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Overtime Start Time is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoOTEndTime)
                {
                    validator.ErrorMessage = "Overtime End Time is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Overtime End Time is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoNPH)
                {
                    validator.ErrorMessage = "No Pay Hour should be greater than zero.";
                    validator.ToolTip = "No Pay Hour should be greater than zero.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftCode)
                {
                    validator.ErrorMessage = "Shift Code is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Shift Code is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDILEntitlement)
                {
                    validator.ErrorMessage = "DIL Entitlement is a required field which should not be left blank or empty.";
                    validator.ToolTip = "DIL Entitlement is a required field which should not be left blank or empty..";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRemarkCode)
                {
                    validator.ErrorMessage = "Remark Code is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Remark Code is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "No record has been selected for deletion!";
                    validator.ToolTip = "No record has been selected for deletion!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidTimeRange)
                {
                    validator.ErrorMessage = "The specified time range is invalid. Please make sure that OT Start Time is less than OT End Time!";
                    validator.ToolTip = "The specified time range is invalid. Please make sure that OT Start Time is less than OT End Time!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoTypeOfRelative)
                {
                    validator.ErrorMessage = "Relative Type is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Relative Type is a required field which should not be left blank or empty.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoOtherRelativeType)
                {
                    validator.ErrorMessage = "Other Relative is a required field which should not be left blank or empty.";
                    validator.ToolTip = "Other Relative is a required field which should not be left blank or empty.";
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

        protected void cboCorrectionCode_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                // Disable the correction code combobox
                if (!string.IsNullOrEmpty(this.cboCorrectionCode.SelectedValue))
                    this.cboCorrectionCode.Enabled = false;

                this.dtpStartTime.ToolTip = string.Empty;
                this.dtpEndTime.ToolTip = string.Empty;

                #region Initialize Remove Family Death related controls
                this.cboRelativeType.SelectedIndex = -1;
                this.cboRelativeType.Text = string.Empty;
                this.cboRelativeType.Items.Clear();
                this.txtRemarks.Text = string.Empty;
                this.txtOtherRelative.Text = string.Empty;
                this.tdRelativeTitle.InnerText = "Relative Type";
                this.trRelativeType.Style[HtmlTextWriterStyle.Display] = "none";
                this.trOtherRelative.Style[HtmlTextWriterStyle.Display] = "none";
                this.trRelativeTypeCombo.Style[HtmlTextWriterStyle.Display] = string.Empty;

                // Clear session
                this.FilteredRelativeTypeList.Clear();
                #endregion

                if (this.cboCorrectionCode.SelectedValue.Trim() == "ACS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "AL" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "BD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CAL" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CBD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CCS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CDF" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COEW" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COMS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "CSR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "DF" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "EW" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "MA" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "MS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "PD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "PH" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "PM" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "SD" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "SR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "TR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "ROT")
                {
                    #region New overtime correction codes
                    this.dtpStartTime.Enabled = false;
                    this.dtpEndTime.Enabled = false;
                    this.cboOTType.Enabled = false;

                    this.dtpStartTime.BackColor = System.Drawing.Color.Yellow;
                    this.dtpEndTime.BackColor = System.Drawing.Color.Yellow;
                    this.cboOTType.BackColor = System.Drawing.Color.Yellow;

                    //this.dtpStartTime.ToolTip = "REQUIRED";
                    //this.dtpEndTime.ToolTip = "REQUIRED";
                    //this.cboOTType.ToolTip = "REQUIRED";

                    //this.dtpStartTime.SelectedDate = this.CurrentAttendanceRecord.OTStartTimeTE;
                    //this.dtpStartTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTStartTimeTE;
                    //this.dtpEndTime.SelectedDate = this.CurrentAttendanceRecord.OTEndTimeTE;
                    //this.dtpEndTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTEndTimeTE;
                    //this.cboOTType.SelectedValue = this.CurrentAttendanceRecord.OTTypeTE;

                    this.dtpStartTime.SelectedDate = this.CurrentAttendanceRecord.OTStartTime;
                    this.dtpStartTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTStartTime;
                    this.dtpEndTime.SelectedDate = this.CurrentAttendanceRecord.OTEndTime;
                    this.dtpEndTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTEndTime;
                    this.cboOTType.SelectedValue = this.CurrentAttendanceRecord.OTType;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "AOMA" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "AOBT" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "AOCS")
                {
                    #region "Add OT Manager Approved", "Add OT Busines Trip during Eid", "Add Overtime-Change Shift"
                    this.cboOTType.Enabled = true;
                    this.dtpStartTime.Enabled = true;
                    this.dtpEndTime.Enabled = true;

                    this.dtpStartTime.BackColor = System.Drawing.Color.Yellow;
                    this.dtpEndTime.BackColor = System.Drawing.Color.Yellow;
                    this.cboOTType.BackColor = System.Drawing.Color.Yellow;

                    this.dtpStartTime.ToolTip = "REQUIRED";
                    this.dtpEndTime.ToolTip = "REQUIRED";
                    //this.cboOTType.ToolTip = "REQUIRED";

                    this.cboOTType.SelectedValue = "R";  // OT type Regular
                    this.dtpStartTime.SelectedDate = null;
                    //this.dtpStartTimeMirror.SelectedDate = null;
                    this.dtpEndTime.SelectedDate = null;
                    //this.dtpEndTimeMirror.SelectedDate = null;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "COCA" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "COCS")
                {
                    #region "Change Overtime-Call Out" and "Change Overtime-Change Shift"
                    this.cboOTType.Enabled = true;
                    this.dtpStartTime.Enabled = true;
                    this.dtpEndTime.Enabled = true;

                    this.dtpStartTime.BackColor = System.Drawing.Color.Yellow;
                    this.dtpEndTime.BackColor = System.Drawing.Color.Yellow;
                    this.cboOTType.BackColor = System.Drawing.Color.Yellow;

                    this.dtpStartTime.ToolTip = "REQUIRED";
                    this.dtpEndTime.ToolTip = "REQUIRED";
                    //this.cboOTType.ToolTip = "REQUIRED";

                    this.dtpStartTime.SelectedDate = this.CurrentAttendanceRecord.OTStartTime.HasValue ? this.CurrentAttendanceRecord.OTStartTime : this.CurrentAttendanceRecord.OTStartTimeTE;
                    this.dtpStartTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTStartTime.HasValue ? this.CurrentAttendanceRecord.OTStartTime : this.CurrentAttendanceRecord.OTStartTimeTE;
                    this.dtpEndTime.SelectedDate = this.CurrentAttendanceRecord.OTEndTime.HasValue ? this.CurrentAttendanceRecord.OTEndTime : this.CurrentAttendanceRecord.OTEndTimeTE;
                    this.dtpEndTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTEndTime.HasValue ? this.CurrentAttendanceRecord.OTEndTime : this.CurrentAttendanceRecord.OTEndTimeTE;
                    this.cboOTType.SelectedValue = !string.IsNullOrEmpty(this.CurrentAttendanceRecord.OTType) ? this.CurrentAttendanceRecord.OTType : this.CurrentAttendanceRecord.OTTypeTE;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ROAL" ||
                   this.cboCorrectionCode.SelectedValue.Trim() == "ROCS" ||
                   this.cboCorrectionCode.SelectedValue.Trim() == "RODO" ||
                   this.cboCorrectionCode.SelectedValue.Trim() == "ROMA")
                {
                    #region "Remove OT-against last month", "Remove OT-Change Shift", "Remove OT-Day Off", "Remove OT Manager approval"
                    this.dtpStartTime.SelectedDate = null;
                    this.dtpStartTimeMirror.SelectedDate = null;
                    this.dtpEndTime.SelectedDate = null;
                    this.dtpEndTimeMirror.SelectedDate = null;
                    this.cboOTType.SelectedIndex = -1;
                    this.cboOTType.Text = string.Empty;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ANAD")
                {
                    #region "Add No Pay Hour-Adjustment"
                    this.txtNPH.Enabled = true;
                    this.txtNPH.BackColor = System.Drawing.Color.Yellow;
                    this.txtNPH.ToolTip = "REQUIRED";
                    this.txtNPH.Text = "0";

                    this.dtpNPH.Enabled = true;
                    this.dtpNPH.BackColor = System.Drawing.Color.Yellow;
                    this.dtpNPH.ToolTip = "REQUIRED";
                    this.dtpNPH.SelectedDate = null;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "CNWP")
                {
                    #region "Change NPH-With permission"
                    this.txtNPH.Enabled = true;
                    this.txtNPH.BackColor = System.Drawing.Color.Yellow;

                    this.dtpNPH.Enabled = true;
                    this.dtpNPH.BackColor = System.Drawing.Color.Yellow;
                    this.dtpNPH.ToolTip = "REQUIRED";
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RNAP" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNCB" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNCS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNDF" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNDP" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNLE" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNMR" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNOP" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNSL" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNSO" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RNST")
                {
                    #region "Remove NoPay Access problem", "Remove NoPayHour-Co.Business", "Remove NoPayHour-Change Shift", "Remove NoPayHour - Death Family", "Remove NoPayHour - w / permission", "Remove No Pay Leave Entered", "Remove NoPayHour - Medical reason", "Remove NoPayHour - Baby born", "Remove No Pay SL", "Remove NoPay / Special Occasion", "Remove NoPay sport team"
                    this.txtNPH.BackColor = System.Drawing.Color.Yellow;
                    this.txtNPH.Text = "0";

                    this.dtpNPH.BackColor = System.Drawing.Color.Yellow;
                    this.dtpNPH.SelectedDate = null;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ASES" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "ASNS")
                {
                    #region Process "Add Sh Allw Evening-Chng Shift", "Add Sh Allw Night-Chng Shift" correction codes
                    this.chkShiftAllowance.Checked = true;
                    this.chkShiftAllowance.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RSES" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RSNS" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RSNE")
                {
                    #region Process "Remove Shift Allow-evening shf", "Remove Shift Allow-night shift", "Remove Shift Allo-not entitled" correction codes
                    this.chkShiftAllowance.Checked = false;
                    this.chkShiftAllowance.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "MACL" ||   // Mark Absent Leave Cancelled
                    this.cboCorrectionCode.SelectedValue.Trim() == "MACS" ||        // Mark Absent-Change Shift
                    this.cboCorrectionCode.SelectedValue.Trim() == "MADA" ||        // Mark Absent-Disciplinary Action
                    this.cboCorrectionCode.SelectedValue.Trim() == "MARO" ||        // Mark Absent - Remove Dayoff
                    this.cboCorrectionCode.SelectedValue.Trim() == "MAGS")          // Mark Absent During Gen. Strike
                {
                    #region Process all absent marking correction codes
                    this.cboRemarkCode.SelectedValue = "A";
                    this.cboRemarkCode.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAAP"      // Remove Absent Access Problem 
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RABT"        // Remove Absent Business Trip
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RACB"        // Remove Absent-Child Birth
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RACS"        // Remove Absent-Change Shift
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADF"        // Remove Absent-Death of Family
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADL"        // Remove Absent DIL
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADO"        // Remove Absent-Day Off
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RADP"        // Remove Absent Deducted Payroll
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAEA"        // Remove Absent-Excused
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAGD"        // Remove Absent-Give DIL
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAJC"        // Remove Absent-Attend Trade U.
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RALE"        // Remove Absent Leave Entered
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAMT"        // Remove Absent-Manual Timesheet
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAPH"        // Remove absent - Public Holiday
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASA"        // Remove Absent Special Assignmt
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASL"        // Remove Absent Sick Leave
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASP"        // Remove Absent - Change Shift P
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RASR"        // Remove Absent Sec. Restriction
                    || this.cboCorrectionCode.SelectedValue.Trim() == "RAST")       // Remove Absent Sport Team)
                {
                    #region Process all absent removal correction codes
                    this.cboRemarkCode.SelectedIndex = -1;
                    this.cboRemarkCode.Text = string.Empty;
                    this.cboRemarkCode.BackColor = System.Drawing.Color.Transparent;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "HD")       
                {
                    #region Process "Half Day Leave" correction code
                    this.cboRemarkCode.SelectedIndex = -1;
                    this.cboRemarkCode.Text = string.Empty;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "MDEA")     
                {
                    #region Process "Mark DIL-Entitled by Admin" correction code
                    if (this.cboDILEntitlement.Items.Count > 0)
                        this.cboDILEntitlement.SelectedValue = "EA";

                    //this.cboDILEntitlement.Enabled = true;
                    this.cboDILEntitlement.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RDEA")
                {
                    #region Process "Remove DIL-Entitled by Admin" correction code
                    this.cboDILEntitlement.SelectedIndex = -1;
                    this.cboDILEntitlement.Text = string.Empty;
                    this.cboDILEntitlement.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "MOCS")
                {
                    #region Process "Mark Off Change Shift" correction code
                    this.txtShiftCode.Text = "O";
                    this.txtShiftCode.BackColor = System.Drawing.Color.Yellow;

                    this.cboShiftCode.SelectedValue = "O";
                    this.cboShiftCode.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "ALSE")
                {
                    #region Process "Local Seminar/Exhibition" correction code
                    this.cboRemarkCode.SelectedIndex = -1;
                    this.cboRemarkCode.Text = string.Empty;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAAD")
                {
                    #region Process "Add Extra Pay-Adj last month" correction code
                    this.cboRemarkCode.SelectedIndex = -1;
                    this.cboRemarkCode.Text = string.Empty;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "CSS")
                {
                    #region Process "Change Schedule Shift" correction code
                    this.cboShiftCode.Enabled = true;
                    this.cboShiftCode.BackColor = System.Drawing.Color.Yellow;
                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAD1" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RAD2" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RAD3" ||
                    this.cboCorrectionCode.SelectedValue.Trim() == "RAD4")
                {
                    #region Process "Remove Absent Death 1st Degree", "Remove Absent Death 2nd Degree", "Remove Absent Death 3rd Degree", "Remove Absent Death 4th Degree" correction codes
                    this.trRelativeType.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    this.trOtherRelative.Style[HtmlTextWriterStyle.Display] = "none";
                    this.trRelativeTypeCombo.Style[HtmlTextWriterStyle.Display] = string.Empty;                    
                    this.tdRelativeTitle.InnerText = "Relative Type";

                    this.cboRemarkCode.SelectedIndex = -1;
                    this.cboRemarkCode.Text = string.Empty;
                    this.cboRemarkCode.BackColor = System.Drawing.Color.Transparent;

                    #region Filter the Relative Type combobox
                    if (this.RelativeTypeList.Count > 0)
                    {
                        if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD1.ToString())
                        {
                            #region Remove Absent Death 1st Degree
                            this.FilteredRelativeTypeList = this.RelativeTypeList
                                .Where(a => a.DegreeLevel == 1)
                                .OrderBy(a => a.RelativeTypeName)
                                .ToList();                                                        
                            #endregion
                        }
                        else if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD2.ToString())
                        {
                            #region Remove Absent Death 2nd Degree 
                            this.FilteredRelativeTypeList = this.RelativeTypeList
                                .Where(a => a.DegreeLevel == 2)
                                .OrderBy(a => a.RelativeTypeName)
                                .ToList();
                            #endregion
                        }
                        else if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD3.ToString())
                        {
                            #region Remove Absent Death 3rd Degree 
                            this.FilteredRelativeTypeList = this.RelativeTypeList
                                .Where(a => a.DegreeLevel == 3)
                                .OrderBy(a => a.RelativeTypeName)
                                .ToList();
                            #endregion
                        }
                        else if (this.cboCorrectionCode.SelectedValue == DeathCorrectionCode.RAD4.ToString())
                        {
                            #region Remove Absent Death 4th Degree 
                            this.FilteredRelativeTypeList = this.RelativeTypeList
                                .Where(a => a.DegreeLevel == 4)
                                .OrderBy(a => a.RelativeTypeName)
                                .ToList();
                            #endregion
                        }

                        if (this.FilteredRelativeTypeList.Count > 0)
                        {
                            // Add blank item
                            this.FilteredRelativeTypeList.Insert(0, new RelativeType() { RelativeTypeName = string.Empty, RelativeTypeCode = UIHelper.CONST_COMBO_EMTYITEM_ID });

                            #region Bind data to combobox
                            this.cboRelativeType.DataSource = this.FilteredRelativeTypeList;
                            this.cboRelativeType.DataTextField = "RelativeTypeName";
                            this.cboRelativeType.DataValueField = "RelativeTypeCode";
                            this.cboRelativeType.DataBind();
                            #endregion
                        }
                    }
                    #endregion

                    #endregion
                }
                else if (this.cboCorrectionCode.SelectedValue.Trim() == "RAD0")
                {
                    #region Process "Remove Absent Death Others" correction code
                    this.trRelativeType.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    this.trOtherRelative.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    this.trRelativeTypeCombo.Style[HtmlTextWriterStyle.Display] = "none";
                    this.tdRelativeTitle.InnerText = "Other Relative";
                    this.cboRemarkCode.SelectedIndex = -1;
                    this.cboRemarkCode.Text = string.Empty;
                    this.cboRemarkCode.BackColor = System.Drawing.Color.Transparent;
                    #endregion
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
            this.litEmpName.Text = "-Not defined-";
            this.litPosition.Text = "-Not defined-";
            this.litCostCenter.Text = "-Not defined-";
            this.litAttendanceDate.Text = "-Not defined-";
            this.litTimeIn.Text = "-Not defined-";
            this.litTimeOut.Text = "-Not defined-";
            this.litLastUpdateUser.Text = "-Not defined-";
            this.litLastUpdateTime.Text = "-Not defined-";

            this.txtNPH.Text = string.Empty;
            this.txtShiftCode.Text = string.Empty;
            this.litActualShiftCode.Text = string.Empty;
            this.chkShiftAllowance.Checked = false;
            this.dtpStartTime.SelectedDate = null;
            this.dtpStartTimeMirror.SelectedDate = null;
            this.dtpEndTime.SelectedDate = null;
            this.dtpEndTimeMirror.SelectedDate = null;
            this.dtpNPH.SelectedDate = null;

            this.cboCorrectionCode.SelectedIndex = -1;
            this.cboCorrectionCode.Text = string.Empty;
            this.cboCorrectionCode.Enabled = true;
            this.cboOTType.SelectedIndex = -1;
            this.cboOTType.Text = string.Empty;
            this.cboDILEntitlement.SelectedIndex = -1;
            this.cboDILEntitlement.Text = string.Empty;
            this.cboRemarkCode.SelectedIndex = -1;
            this.cboRemarkCode.Text = string.Empty;
            this.cboShiftCode.SelectedIndex = -1;
            this.cboShiftCode.Text = string.Empty;

            #region Initialive Remove Family Death related controls
            this.cboRelativeType.SelectedIndex = -1;
            this.cboRelativeType.Text = string.Empty;
            this.cboRelativeType.Items.Clear();
            this.txtRemarks.Text = string.Empty;
            this.txtOtherRelative.Text = string.Empty;
            this.tdRelativeTitle.InnerText = "Relative Type";
            this.trRelativeType.Style[HtmlTextWriterStyle.Display] = "none";
            this.trOtherRelative.Style[HtmlTextWriterStyle.Display] = "none";
            this.trRelativeTypeCombo.Style[HtmlTextWriterStyle.Display] = string.Empty;
            #endregion

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
        }

        public void KillSessions()
        {
            // Clear collections
            this.CorrectionCodeList.Clear();
            this.OvertimeTypeList.Clear();
            this.ShiftCodeList.Clear();
            this.RelativeTypeList.Clear();
            this.FilteredRelativeTypeList.Clear();

            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentAttendanceRecord"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.TimesheetCorrectionEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.TimesheetCorrectionEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionEntryStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["CurrentFormLoadType"]);
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

            #region Restore session values
            if (this.TimesheetCorrectionEntryStorage.ContainsKey("CurrentAttendanceRecord"))
                this.CurrentAttendanceRecord = this.TimesheetCorrectionEntryStorage["CurrentAttendanceRecord"] as EmployeeAttendanceEntity;
            else
                this.CurrentAttendanceRecord = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("CorrectionCodeList"))
                this.CorrectionCodeList = this.TimesheetCorrectionEntryStorage["CorrectionCodeList"] as List<UserDefinedCodes>;
            else
                this.CorrectionCodeList = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("OvertimeTypeList"))
                this.OvertimeTypeList = this.TimesheetCorrectionEntryStorage["OvertimeTypeList"] as List<UserDefinedCodes>;
            else
                this.OvertimeTypeList = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("ShiftCodeList"))
                this.ShiftCodeList = this.TimesheetCorrectionEntryStorage["ShiftCodeList"] as List<UserDefinedCodes>;
            else
                this.ShiftCodeList = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("RelativeTypeList"))
                this.RelativeTypeList = this.TimesheetCorrectionEntryStorage["RelativeTypeList"] as List<RelativeType>;
            else
                this.RelativeTypeList = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("FilteredRelativeTypeList"))
                this.FilteredRelativeTypeList = this.TimesheetCorrectionEntryStorage["FilteredRelativeTypeList"] as List<RelativeType>;
            else
                this.FilteredRelativeTypeList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.TimesheetCorrectionEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("txtNPH"))
                this.txtNPH.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["txtNPH"]);
            else
                this.txtNPH.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("txtShiftCode"))
                this.txtShiftCode.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["txtShiftCode"]);
            else
                this.txtShiftCode.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litActualShiftCode"))
                this.litActualShiftCode.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litActualShiftCode"]);
            else
                this.litActualShiftCode.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litAttendanceDate"))
                this.litAttendanceDate.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litAttendanceDate"]);
            else
                this.litAttendanceDate.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litTimeIn"))
                this.litTimeIn.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litTimeIn"]);
            else
                this.litTimeIn.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litTimeOut"))
                this.litTimeOut.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litTimeOut"]);
            else
                this.litTimeOut.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litLastUpdateUser"))
                this.litLastUpdateUser.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litLastUpdateUser"]);
            else
                this.litLastUpdateUser.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("chkShiftAllowance"))
                this.chkShiftAllowance.Checked = UIHelper.ConvertObjectToBolean(this.TimesheetCorrectionEntryStorage["chkShiftAllowance"]);
            else
                this.chkShiftAllowance.Checked = false;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("dtpStartTime"))
                this.dtpStartTime.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetCorrectionEntryStorage["dtpStartTime"]);
            else
                this.dtpStartTime.SelectedDate = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("dtpStartTimeMirror"))
                this.dtpStartTimeMirror.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetCorrectionEntryStorage["dtpStartTimeMirror"]);
            else
                this.dtpStartTimeMirror.SelectedDate = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("dtpEndTime"))
                this.dtpEndTime.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetCorrectionEntryStorage["dtpEndTime"]);
            else
                this.dtpEndTime.SelectedDate = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("dtpEndTimeMirror"))
                this.dtpEndTimeMirror.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetCorrectionEntryStorage["dtpEndTimeMirror"]);
            else
                this.dtpEndTimeMirror.SelectedDate = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("dtpNPH"))
                this.dtpNPH.SelectedDate = UIHelper.ConvertObjectToDate(this.TimesheetCorrectionEntryStorage["dtpNPH"]);
            else
                this.dtpNPH.SelectedDate = null;

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("cboCorrectionCode"))
                this.cboCorrectionCode.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["cboCorrectionCode"]);
            else
            {
                this.cboCorrectionCode.SelectedIndex = -1;
                this.cboCorrectionCode.Text = string.Empty;
            }

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("cboOTType"))
                this.cboOTType.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["cboOTType"]);
            else
            {
                this.cboOTType.SelectedIndex = -1;
                this.cboOTType.Text = string.Empty;
            }

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("cboDILEntitlement"))
                this.cboDILEntitlement.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["cboDILEntitlement"]);
            else
            {
                this.cboDILEntitlement.SelectedIndex = -1;
                this.cboDILEntitlement.Text = string.Empty;
            }

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("cboRemarkCode"))
                this.cboRemarkCode.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["cboRemarkCode"]);
            else
            {
                this.cboRemarkCode.SelectedIndex = -1;
                this.cboRemarkCode.Text = string.Empty;
            }

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("cboShiftCode"))
                this.cboShiftCode.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["cboShiftCode"]);
            else
            {
                this.cboShiftCode.SelectedIndex = -1;
                this.cboShiftCode.Text = string.Empty;
            }

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("cboRelativeType"))
                this.cboRelativeType.SelectedValue = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["cboRelativeType"]);
            else
            {
                this.cboRelativeType.SelectedIndex = -1;
                this.cboRelativeType.Text = string.Empty;
            }

            if (this.TimesheetCorrectionEntryStorage.ContainsKey("txtRemarks"))
                this.txtRemarks.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionEntryStorage["txtRemarks"]);
            else
                this.txtRemarks.Text = string.Empty;
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.TimesheetCorrectionEntryStorage.Clear();
            this.TimesheetCorrectionEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.TimesheetCorrectionEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("txtNPH", this.txtNPH.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("txtShiftCode", this.txtShiftCode.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litActualShiftCode", this.litActualShiftCode.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litAttendanceDate", this.litAttendanceDate.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litTimeIn", this.litTimeIn.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litTimeOut", this.litTimeOut.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litLastUpdateUser", this.litLastUpdateUser.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());
            this.TimesheetCorrectionEntryStorage.Add("chkShiftAllowance", this.chkShiftAllowance.Checked);
            this.TimesheetCorrectionEntryStorage.Add("dtpStartTime", this.dtpStartTime.SelectedDate);
            this.TimesheetCorrectionEntryStorage.Add("dtpStartTimeMirror", this.dtpStartTimeMirror.SelectedDate);
            this.TimesheetCorrectionEntryStorage.Add("dtpEndTime", this.dtpEndTime.SelectedDate);
            this.TimesheetCorrectionEntryStorage.Add("dtpEndTimeMirror", this.dtpEndTimeMirror.SelectedDate);
            this.TimesheetCorrectionEntryStorage.Add("dtpNPH", this.dtpNPH.SelectedDate);
            this.TimesheetCorrectionEntryStorage.Add("cboCorrectionCode", this.cboCorrectionCode.SelectedValue);
            this.TimesheetCorrectionEntryStorage.Add("cboOTType", this.cboOTType.SelectedValue);
            this.TimesheetCorrectionEntryStorage.Add("cboDILEntitlement", this.cboDILEntitlement.SelectedValue);
            this.TimesheetCorrectionEntryStorage.Add("cboRemarkCode", this.cboRemarkCode.SelectedValue);
            this.TimesheetCorrectionEntryStorage.Add("cboShiftCode", this.cboShiftCode.SelectedValue);
            this.TimesheetCorrectionEntryStorage.Add("cboRelativeType", this.cboRelativeType.SelectedValue);
            this.TimesheetCorrectionEntryStorage.Add("txtRemarks", this.txtRemarks.Text);
            #endregion

            #region Save Query String values to collection
            this.TimesheetCorrectionEntryStorage.Add("CallerForm", this.CallerForm);
            this.TimesheetCorrectionEntryStorage.Add("AutoID", this.AutoID);
            this.TimesheetCorrectionEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.TimesheetCorrectionEntryStorage.Add("CurrentAttendanceRecord", this.CurrentAttendanceRecord);
            this.TimesheetCorrectionEntryStorage.Add("CorrectionCodeList", this.CorrectionCodeList);
            this.TimesheetCorrectionEntryStorage.Add("OvertimeTypeList", this.OvertimeTypeList);
            this.TimesheetCorrectionEntryStorage.Add("ShiftCodeList", this.ShiftCodeList);
            this.TimesheetCorrectionEntryStorage.Add("RelativeTypeList", this.RelativeTypeList);
            this.TimesheetCorrectionEntryStorage.Add("FilteredRelativeTypeList", this.FilteredRelativeTypeList);
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
                    this.txtEmpNo.Enabled = false;
                    this.txtShiftCode.Enabled = false;
                    this.txtNPH.Enabled = false;
                    this.chkShiftAllowance.Enabled = false;
                    this.cboDILEntitlement.Enabled = false;
                    this.cboRemarkCode.Enabled = false;
                    this.cboOTType.Enabled = false;
                    this.dtpStartTime.Enabled = false;
                    this.dtpEndTime.Enabled = false;
                    this.dtpNPH.Enabled = false;
                    this.cboCorrectionCode.Enabled = false;
                    this.cboShiftCode.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = true;
                    this.btnFindEmployee.Enabled = true;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing record
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.txtShiftCode.Enabled = false;
                    this.txtNPH.Enabled = false;
                    this.chkShiftAllowance.Enabled = false;
                    this.cboDILEntitlement.Enabled = false;
                    this.cboRemarkCode.Enabled = false;
                    this.cboOTType.Enabled = false;
                    this.dtpStartTime.Enabled = false;
                    this.dtpEndTime.Enabled = false;
                    this.dtpNPH.Enabled = false;
                    this.cboCorrectionCode.Enabled = true;
                    this.cboShiftCode.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = true;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing record (read-only)
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.txtShiftCode.Enabled = false;
                    this.txtNPH.Enabled = false;
                    this.chkShiftAllowance.Enabled = false;
                    this.cboDILEntitlement.Enabled = false;
                    this.cboRemarkCode.Enabled = false;
                    this.cboOTType.Enabled = false;
                    this.dtpStartTime.Enabled = false;
                    this.dtpEndTime.Enabled = false;
                    this.dtpNPH.Enabled = false;
                    this.cboCorrectionCode.Enabled = false;
                    this.cboShiftCode.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = false;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion
            }
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            //FillCorrectionCodeCombo(reloadFromDB, UIHelper.UDCSorterColumn.UDCDesc1);
            FillOvertimeTypeCombo(reloadFromDB, UIHelper.UDCSorterColumn.UDCDesc1);
            FillDILCombo(reloadFromDB, UIHelper.UDCSorterColumn.UDCDesc1);
            FillShiftCodeCombo(reloadFromDB);
            GetRelativeTypeList(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void FillCorrectionCodeCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.CorrectionCodeList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.CorrectionCodeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.CORRECTION_CODE), ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        rawData.AddRange(source.ToList());
                    }
                }

                #region Filter the list
                if (rawData != null && rawData.Count() > 0)
                {
                    List<UserDefinedCodes> filteredList = new List<UserDefinedCodes>();

                    if (this.CurrentAttendanceRecord != null)
                    {
                        #region Hide all new overtime reason codes only if OT is not yet posted in the Timesheet
                        if (this.CurrentAttendanceRecord.OTStartTime == null || 
                            this.CurrentAttendanceRecord.OTEndTime == null)
                        {                            
                            foreach (UserDefinedCodes item in rawData)
                            {
                                if (item.UDCCode.Trim() == "ACS" ||         // Add OT Change Shift
                                    item.UDCCode.Trim() == "AL" ||          // Annual Leave
                                    item.UDCCode.Trim() == "BD" ||          // Break Down
                                    item.UDCCode.Trim() == "CAL" ||         // Call out Annual Leave
                                    item.UDCCode.Trim() == "CBD" ||         // Call out Break Down
                                    item.UDCCode.Trim() == "CCS" ||         // Change OT Change Shift
                                    item.UDCCode.Trim() == "CDF" ||         // Call out Family Death
                                    item.UDCCode.Trim() == "COEW" ||        // Call Out Extra Work
                                    item.UDCCode.Trim() == "COMS" ||        // Call Out Manpower Shortage
                                    item.UDCCode.Trim() == "CSR" ||         // Call out Sick
                                    item.UDCCode.Trim() == "DF" ||          // Family Death
                                    item.UDCCode.Trim() == "EW" ||          // Extra Work/ Special Task
                                    item.UDCCode.Trim() == "MA" ||          // Add OT Manager Approval
                                    item.UDCCode.Trim() == "MS" ||          // Manpower Shortage
                                    item.UDCCode.Trim() == "PD" ||          // Project / Development
                                    item.UDCCode.Trim() == "PH" ||          // Public Holiday
                                    item.UDCCode.Trim() == "PM" ||          // Planned Maintenance
                                    item.UDCCode.Trim() == "SD" ||          // Shutdown
                                    item.UDCCode.Trim() == "SR" ||          // Leave (Sick,Injury,Light Duty)
                                    item.UDCCode.Trim() == "TR" ||          // Training
                                    item.UDCCode.Trim() == "ROT")            // OT for Ramadan
                                {
                                    filteredList.Add(item);
                                }
                            }

                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }                            
                        }
                        #endregion

                        #region Check if no overtime in Timesheet table, then hide "Change Overtime-Call Out", "Change Overtime-Change Shift", "Remove OT-against last month", "Remove OT-Change Shift", "Remove OT-Day Off", "Remove OT Manager approval" correction codes
                        if (this.CurrentAttendanceRecord.OTStartTime == null &&
                            this.CurrentAttendanceRecord.OTEndTime == null)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "COCA" || a.UDCCode == "COCS" || a.UDCCode == "ROAL" || a.UDCCode == "ROCS" || a.UDCCode == "RODO" || a.UDCCode == "ROMA")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }                            
                        }
                        #endregion

                        #region Check if overtime is already posted in the Timesheet, then hide "Add OT Busines Trip during Eid", "Add Overtime-Change Shift", "Add OT manager approved" correction codes
                        if (this.CurrentAttendanceRecord.OTStartTime != null &&
                            this.CurrentAttendanceRecord.OTEndTime != null)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "AOBT" || a.UDCCode == "AOCS" || a.UDCCode == "AOMA")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Add No Pay Hour-Adjustment" correction code if "NoPayHour" is greater than zero
                        if (this.CurrentAttendanceRecord.NoPayHours > 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "ANAD")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide all NPH removal correction codes if "NoPayHour" is zero
                        if (UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.NoPayHours) == 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "RNAP" || a.UDCCode == "RNCB" || a.UDCCode == "RNCS" || a.UDCCode == "RNDF" || a.UDCCode == "RNDP" || a.UDCCode == "RNLE" 
                                    || a.UDCCode == "RNMR" || a.UDCCode == "RNOP" || a.UDCCode == "RNSL" || a.UDCCode == "RNSO" || a.UDCCode == "RNST")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Add Sh Allw Evening-Chng Shift" correction codes if "Duration_ShiftAllowance_Evening" is greater than zero
                        if (UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.DurationShiftAllowanceEvening) > 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "ASES")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Add Sh Allw Night-Chng Shift" correction codes if "DurationShiftAllowanceNight" is greater than zero
                        if (UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.DurationShiftAllowanceNight) > 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "ASNS")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove Shift Allow-evening shf" correction code if "Duration_ShiftAllowance_Evening" is zero
                        if (UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.DurationShiftAllowanceEvening) == 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "RSES")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove Shift Allow-night shift" correction code if "Duration_ShiftAllowance_Night" is zero
                        if (UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.DurationShiftAllowanceNight) == 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "RSNS")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove Shift Allo-not entitled" correction code in the ff conditions: 1) Duration_ShiftAllowance_Evening = 0; 2) Duration_ShiftAllowance_Night = 0; 3) ShiftAllowance = 0
                        if (UIHelper.ConvertNumberToBolean(this.CurrentAttendanceRecord.ShiftAllowance) == false &&
                            UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.DurationShiftAllowanceEvening) == 0 &&
                            UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.DurationShiftAllowanceNight) == 0)
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "RSNE")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Mark Absent Leave Cancelled", "Mark Absent-Change Shift", "Mark Absent-Disciplinary Action", "Mark Absent During Gen. Strike" correction codes if the ff conditions are met: 1)RemarkCode = 'A'; 2) ShiftCode = 'O'; 3) LeaveType <> null
                        if (this.CurrentAttendanceRecord.RemarkCode == "A" ||
                            this.CurrentAttendanceRecord.ShiftCode == "O" ||
                            !string.IsNullOrEmpty(this.CurrentAttendanceRecord.LeaveType))
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "MACL" || a.UDCCode == "MACS" || a.UDCCode == "MADA" || a.UDCCode == "MAGS")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide all absent removal correction codes if the ff conditions are met: 1) RemarkCode != 'A'
                        if (this.CurrentAttendanceRecord.RemarkCode != "A")
                        {
                                filteredList = rawData.Where(a => a.UDCCode == "RAAP"   // Remove Absent Access Problem 
                                || a.UDCCode == "RABT"      // Remove Absent Business Trip
                                || a.UDCCode == "RACB"      // Remove Absent-Child Birth
                                || a.UDCCode == "RACS"      // Remove Absent-Change Shift
                                || a.UDCCode == "RADF"      // Remove Absent-Death of Family
                                || a.UDCCode == "RADL"      // Remove Absent DIL
                                || a.UDCCode == "RADO"      // Remove Absent-Day Off
                                || a.UDCCode == "RADP"      // Remove Absent Deducted Payroll
                                || a.UDCCode == "RAEA"      // Remove Absent-Excused
                                || a.UDCCode == "RAGD"      // Remove Absent-Give DIL
                                || a.UDCCode == "RAJC"      // Remove Absent-Attend Trade U.
                                || a.UDCCode == "RALE"      // Remove Absent Leave Entered
                                || a.UDCCode == "RAMT"      // Remove Absent-Manual Timesheet
                                || a.UDCCode == "RAPH"      // Remove absent - Public Holiday
                                || a.UDCCode == "RASA"      // Remove Absent Special Assignmt
                                || a.UDCCode == "RASL"      // Remove Absent Sick Leave
                                || a.UDCCode == "RASP"      // Remove Absent - Change Shift P
                                || a.UDCCode == "RASR"      // Remove Absent Sec. Restriction
                                || a.UDCCode == "RAST"      // Remove Absent Sport Team
                                || a.UDCCode == "RAD1"      // Remove Absent Death 1st Degree
                                || a.UDCCode == "RAD2"      // Remove Absent Death 2nd Degree
                                || a.UDCCode == "RAD3"      // Remove Absent Death 3rd Degree
                                || a.UDCCode == "RAD4"      // Remove Absent Death 4th Degree
                                || a.UDCCode == "RAD0")      // Remove Absent Death Others
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Leave Cancelled" correction code if "LeaveType" = null
                        //if (string.IsNullOrEmpty(this.CurrentAttendanceRecord.LeaveType))
                        //{
                        //    filteredList = rawData.Where(a => a.UDCCode == "RWLC")   // Leave Cancelled
                        //        .ToList();
                        //    if (filteredList.Count > 0)
                        //    {
                        //        foreach (UserDefinedCodes item in filteredList)
                        //        {
                        //            rawData.Remove(item);
                        //        }
                        //    }
                        //}
                        #endregion

                        #region Hide "Mark DIL-Entitled by Admin" correction code if the ff conditions are met: 1) DIL_Entitlement != null; 2) RemarkCode = 'A'; 3) LeaveType != null; 4) IsLastRow = 0
                        if (!string.IsNullOrEmpty(this.CurrentAttendanceRecord.DILEntitlement) ||
                            this.CurrentAttendanceRecord.RemarkCode == "A" ||
                            !string.IsNullOrEmpty(this.CurrentAttendanceRecord.LeaveType) ||
                            !this.CurrentAttendanceRecord.IsLastRow)
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "MDEA")   // Mark DIL-Entitled by Admin
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove DIL-Entitled by Admin" correction code if the ff conditions are met: 1) DIL_Entitlement = null
                        if (string.IsNullOrEmpty(this.CurrentAttendanceRecord.DILEntitlement))
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "RDEA")   // Remove DIL-Entitled by Admin
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Add Meal Voucher" correction code if the ff conditions are met: 1) MealVoucherEligibility = 'YA'; 2) IsLastRow = 0
                        if (this.CurrentAttendanceRecord.MealVoucherEligibilityCode == "YA" ||
                            !this.CurrentAttendanceRecord.IsLastRow)
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "ADDM")   // Add Meal Voucher
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove Meal Voucher Duplicate" correction code if the ff conditions are met: 1) MealVoucherEligibility != 'YA'
                        if (this.CurrentAttendanceRecord.MealVoucherEligibilityCode != "YA")
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "RMVD")   // Remove Meal Voucher Duplicate
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Mark Off Change Shift" correction code if the ff conditions are met: 1) ShiftCode = 'O'
                        if (this.CurrentAttendanceRecord.ShiftCode == "O")
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "MOCS")   // Mark Off Change Shift
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Local Seminar/Exhibition" correction code if the ff conditions are met: 1) CorrectionCode = 'ALSE'
                        if (this.CurrentAttendanceRecord.CorrectionCode == "ALSE")
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "ALSE")   
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Add Extra Pay-Adj last month" correction code if the ff conditions are met: 1) CorrectionCode = 'RAAD'
                        if (this.CurrentAttendanceRecord.CorrectionCode == "RAAD")
                        {
                            filteredList = rawData.Where(a => a.UDCCode == "RAAD")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Mark Absent - Remove Dayoff" correction codes if ShiftCode != 'O'
                        if (!(this.CurrentAttendanceRecord.ShiftCode == "O" &&
                            this.CurrentAttendanceRecord.RemarkCode != "A"))
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "MARO")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove Dayoff - Mark Unpaid Leave", "Remove Dayoff - Mark Unpaid Sick Leave", "Remove Dayoff - Mark Unpaid Injury Leave" correction codes if ShiftCode != "O"
                        if (this.CurrentAttendanceRecord.ShiftCode != "O")
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "RDUL" || a.UDCCode == "RDSL" || a.UDCCode == "RDIL")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion

                        #region Hide "Remove Absent-Death of Family " correction codes if the ff conditions are met: 1)RemarkCode = 'A'
                        if (this.CurrentAttendanceRecord.RemarkCode == "A")
                        {
                            filteredList = rawData
                                .Where(a => a.UDCCode == "RADF")
                                .ToList();
                            if (filteredList.Count > 0)
                            {
                                foreach (UserDefinedCodes item in filteredList)
                                {
                                    rawData.Remove(item);
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion

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
                    }

                    // Add blank item
                    comboSource.Insert(0, new UserDefinedCodes() { UDCDesc1 = string.Empty, UDCCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
                }
                #endregion

                // Store to session
                this.CorrectionCodeList = comboSource;

                #region Bind data to combobox
                this.cboCorrectionCode.DataSource = comboSource;
                this.cboCorrectionCode.DataTextField = "UDCDesc1";
                this.cboCorrectionCode.DataValueField = "UDCCode";
                this.cboCorrectionCode.DataBind();

                if (this.cboCorrectionCode.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboCorrectionCode.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillOvertimeTypeCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.CorrectionCodeList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.CorrectionCodeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.OVERTIME_TYPE), ref error, ref innerError);
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
                    }

                    // Add blank item
                    comboSource.Insert(0, new UserDefinedCodes() { UDCDesc1 = string.Empty, UDCCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
                }
                #endregion

                // Store to session
                this.CorrectionCodeList = comboSource;

                #region Bind data to combobox
                this.cboOTType.DataSource = comboSource;
                this.cboOTType.DataTextField = "UDCDesc1";
                this.cboOTType.DataValueField = "UDCCode";
                this.cboOTType.DataBind();

                if (this.cboOTType.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboOTType.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillShiftCodeCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.ShiftCodeList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.ShiftCodeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.SHIFT_CODES), ref error, ref innerError);
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
                this.ShiftCodeList = comboSource;

                #region Bind data to combobox
                this.cboShiftCode.DataSource = comboSource;
                this.cboShiftCode.DataTextField = "UDCFullName";
                this.cboShiftCode.DataValueField = "UDCCode";
                this.cboShiftCode.DataBind();

                if (this.cboShiftCode.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboShiftCode.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetTimesheetCorrection(int autoID)
        {
            try
            {
                #region Initialize controls
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = "-Not defined-";
                this.litPosition.Text = "-Not defined-";
                this.litCostCenter.Text = "-Not defined-";
                this.litAttendanceDate.Text = "-Not defined-";
                this.litTimeIn.Text = "-Not defined-";
                this.litTimeOut.Text = "-Not defined-";
                this.litLastUpdateUser.Text = "-Not defined-";
                this.litLastUpdateTime.Text = "-Not defined-";

                this.txtNPH.Text = string.Empty;
                this.txtShiftCode.Text = string.Empty;
                this.litActualShiftCode.Text = string.Empty;
                this.chkShiftAllowance.Checked = false;
                this.dtpStartTime.SelectedDate = null;
                this.dtpStartTimeMirror.SelectedDate = null;
                this.dtpEndTime.SelectedDate = null;
                this.dtpEndTimeMirror.SelectedDate = null;
                this.dtpNPH.SelectedDate = null;

                this.cboCorrectionCode.SelectedIndex = -1;
                this.cboCorrectionCode.Text = string.Empty;
                this.cboCorrectionCode.Enabled = true;
                this.cboOTType.SelectedIndex = -1;
                this.cboOTType.Text = string.Empty;
                this.cboDILEntitlement.SelectedIndex = -1;
                this.cboDILEntitlement.Text = string.Empty;
                this.cboRemarkCode.SelectedIndex = -1;
                this.cboRemarkCode.Text = string.Empty;
                this.cboShiftCode.SelectedIndex = -1;
                this.cboShiftCode.Text = string.Empty;

                #region Initialive Remove Family Death related controls
                this.cboRelativeType.SelectedIndex = -1;
                this.cboRelativeType.Text = string.Empty;
                this.cboRelativeType.Items.Clear();
                this.FilteredRelativeTypeList.Clear();
                //this.txtRemarks.BackColor = System.Drawing.Color.White;
                this.txtRemarks.Text = string.Empty;
                this.trRelativeType.Style[HtmlTextWriterStyle.Display] = "none";
                #endregion

                #endregion

                if (Session["SelectedTimesheetRecord"] != null)
                {
                    this.CurrentAttendanceRecord = Session["SelectedTimesheetRecord"] as EmployeeAttendanceEntity;
                }
                else
                {
                    #region Fetch database record
                    if (autoID == 0)
                        return;

                    List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetTimesheetCorrection(string.Empty, 0, null, null, autoID, 0, 0, ref error, ref innerError);
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
                            this.CurrentAttendanceRecord = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentAttendanceRecord != null)
                {
                    this.txtEmpNo.Value = this.CurrentAttendanceRecord.EmpNo;
                    this.litEmpName.Text = this.CurrentAttendanceRecord.EmpName;
                    this.litPosition.Text = this.CurrentAttendanceRecord.Position;
                    this.litCostCenter.Text = this.CurrentAttendanceRecord.CostCenterFullName;
                    this.litAttendanceDate.Text = this.CurrentAttendanceRecord.DT.HasValue ? Convert.ToDateTime(this.CurrentAttendanceRecord.DT).ToString("dd-MMM-yyyy") : "-";
                    this.litTimeIn.Text = this.CurrentAttendanceRecord.dtIN.HasValue ? Convert.ToDateTime(this.CurrentAttendanceRecord.dtIN).ToString("dd-MMM-yyyy HH:mm") : "-";
                    this.litTimeOut.Text = this.CurrentAttendanceRecord.dtOUT.HasValue ? Convert.ToDateTime(this.CurrentAttendanceRecord.dtOUT).ToString("dd-MMM-yyyy HH:mm") : "-";
                    this.litLastUpdateUser.Text = !string.IsNullOrEmpty(this.CurrentAttendanceRecord.LastUpdateUser) ? this.CurrentAttendanceRecord.LastUpdateUser.Trim() : "-Not defined-";
                    this.litLastUpdateTime.Text = this.CurrentAttendanceRecord.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentAttendanceRecord.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : "-Not defined-";

                    //this.cboCorrectionCode.SelectedValue = this.CurrentAttendanceRecord.CorrectionCode;

                    if (!string.IsNullOrEmpty(this.CurrentAttendanceRecord.OTType))
                        this.cboOTType.SelectedValue = this.CurrentAttendanceRecord.OTType;

                    this.dtpStartTime.SelectedDate = this.CurrentAttendanceRecord.OTStartTime;
                    this.dtpStartTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTStartTime.HasValue ? this.CurrentAttendanceRecord.OTStartTime : this.CurrentAttendanceRecord.OTStartTimeTE;
                    this.dtpEndTime.SelectedDate = this.CurrentAttendanceRecord.OTEndTime;
                    this.dtpEndTimeMirror.SelectedDate = this.CurrentAttendanceRecord.OTEndTime.HasValue ? this.CurrentAttendanceRecord.OTEndTime : this.CurrentAttendanceRecord.OTEndTimeTE;

                    this.txtNPH.Text = UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.NoPayHours).ToString();
                    this.dtpNPH.SelectedDate = ReportHelper.ConvertMinuteToDateTime(UIHelper.ConvertObjectToInt(this.CurrentAttendanceRecord.NoPayHours));
                    
                    this.txtShiftCode.Text = this.CurrentAttendanceRecord.ShiftCode;
                    if (this.cboShiftCode.Items.Count > 0)
                        this.cboShiftCode.SelectedValue = this.CurrentAttendanceRecord.ShiftCode;

                    this.litActualShiftCode.Text = this.CurrentAttendanceRecord.ActualShiftCode;

                    this.chkShiftAllowance.Checked = UIHelper.ConvertObjectToBolean(this.CurrentAttendanceRecord.ShiftAllowance);
                    if (!string.IsNullOrEmpty(this.CurrentAttendanceRecord.DILEntitlement))
                        this.cboDILEntitlement.SelectedValue = this.CurrentAttendanceRecord.DILEntitlement;

                    if (!string.IsNullOrEmpty(this.CurrentAttendanceRecord.RemarkCode))
                        this.cboRemarkCode.SelectedValue = this.CurrentAttendanceRecord.RemarkCode;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillDILCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCSequenceNo, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.CorrectionCodeList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.CorrectionCodeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.DIL_TYPES), ref error, ref innerError);
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
                    }

                    // Add blank item
                    comboSource.Insert(0, new UserDefinedCodes() { UDCDesc1 = string.Empty, UDCCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
                }
                #endregion

                // Store to session
                this.CorrectionCodeList = comboSource;

                #region Bind data to combobox
                this.cboDILEntitlement.DataSource = comboSource;
                this.cboDILEntitlement.DataTextField = "UDCDesc1";
                this.cboDILEntitlement.DataValueField = "UDCCode";
                this.cboDILEntitlement.DataBind();

                if (this.cboDILEntitlement.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboDILEntitlement.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, EmployeeAttendanceEntity attendanceRecord)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                // Get WCF Instance
                if (attendanceRecord == null)
                    return;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteTimesheet(Convert.ToByte(saveType), attendanceRecord.AutoID, attendanceRecord.CorrectionCode, attendanceRecord.OTType,
                    attendanceRecord.OTStartTime, attendanceRecord.OTEndTime, UIHelper.ConvertObjectToInt(attendanceRecord.NoPayHours), attendanceRecord.ShiftCode, 
                    attendanceRecord.ShiftAllowance, attendanceRecord.DurationShiftAllowanceEvening, attendanceRecord.DurationShiftAllowanceNight,
                    attendanceRecord.DILEntitlement, attendanceRecord.RemarkCode, attendanceRecord.LastUpdateUser, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || 
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (attendanceRecord.CorrectionCode == DeathCorrectionCode.RAD0.ToString() ||
                        attendanceRecord.CorrectionCode == DeathCorrectionCode.RAD1.ToString() ||
                        attendanceRecord.CorrectionCode == DeathCorrectionCode.RAD2.ToString() ||
                        attendanceRecord.CorrectionCode == DeathCorrectionCode.RAD3.ToString() ||
                        attendanceRecord.CorrectionCode == DeathCorrectionCode.RAD4.ToString())
                    {
                        #region Save other details for death-related timesheet correction codes
                        DeathReasonOfAbsenceEntity deathEntity = new DeathReasonOfAbsenceEntity()
                        {
                            EmpNo = attendanceRecord.EmpNo,
                            DT = attendanceRecord.DT,
                            CostCenter = attendanceRecord.CostCenter,
                            CorrectionCode = attendanceRecord.CorrectionCode,
                            ShiftPatCode = attendanceRecord.ShiftPatCode,
                            ShiftCode = attendanceRecord.ShiftCode,
                            RelativeTypeCode = this.cboRelativeType.SelectedValue,
                            OtherRelativeType = this.txtOtherRelative.Text.Trim(),
                            Remarks = this.txtRemarks.Text.Trim(),
                            CreatedDate = DateTime.Now,
                            CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                            CreatedByEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                            CreatedByUserID = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]))
                        };

                        // Save to database
                        proxy.InsertUpdateDeleteDeathReasonOfAbsence(Convert.ToInt32(UIHelper.SaveType.Insert), deathEntity, ref error, ref innerError);
                        if (!string.IsNullOrEmpty(error) || 
                            !string.IsNullOrEmpty(innerError))
                        {
                            if (!string.IsNullOrEmpty(innerError))
                                throw new Exception(error, new Exception(innerError));
                            else
                                throw new Exception(error);
                        }
                        #endregion
                    }

                    // Redirect to the inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_INQUIRY + "?{0}={1}&{2}={3}",
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
                this.CurrentAttendanceRecord = null;
                throw new Exception(ex.Message.ToString());
            }
        }

        private void GetRelativeTypeList(bool reloadFromDB)
        {
            try
            {
                List<RelativeType> comboSource = new List<RelativeType>();

                if (this.RelativeTypeList.Count > 0 && 
                    !reloadFromDB)
                {
                    comboSource = this.RelativeTypeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetFamilyRelativeTypes(0, string.Empty, ref error, ref innerError);
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
                        if (rawData != null)
                        {
                            comboSource.AddRange(rawData.ToList());
                        }
                    }
                }

                // Store to session
                this.RelativeTypeList = comboSource;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion
    }
}