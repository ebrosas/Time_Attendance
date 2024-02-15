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

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class DutyROTAEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToDelete,
            NoSpecifiedEmpNo,
            NoEmpNo,
            NoEffectiveDate,
            NoEndingDate,
            NoStartTime,
            NoEndTime,
            NoDutyType,
            InvalidDateRange,
            InvalidTimeRange
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

        private Dictionary<string, object> DutyROTAEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["DutyROTAEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["DutyROTAEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["DutyROTAEntryStorage"] = value;
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

        private DutyROTAEntity CurrentRecord
        {
            get
            {
                return ViewState["CurrentRecord"] as DutyROTAEntity;
            }
            set
            {
                ViewState["CurrentRecord"] = value;
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

        private List<DutyROTAEntity> DutyTypeList
        {
            get
            {
                List<DutyROTAEntity> list = ViewState["DutyTypeList"] as List<DutyROTAEntity>;
                if (list == null)
                    ViewState["DutyTypeList"] = list = new List<DutyROTAEntity>();

                return list;
            }
            set
            {
                ViewState["DutyTypeList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.DROTAENTRY.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_DUTY_ROTA_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_DUTY_ROTA_ENTRY_TITLE), true);
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
                if (this.DutyROTAEntryStorage.Count > 0)
                {
                    if (this.DutyROTAEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["FormFlag"]);
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
                        this.litCostCenterCode.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]);

                        // Save Employee No. to session
                        this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);

                        #region Set the default Duty Type selection
                        if (!string.IsNullOrEmpty(this.litCostCenterCode.Text) &&
                            this.cboDutyType.Items.Count > 0)
                        {
                            if (this.litCostCenterCode.Text.Trim() == "7600")
                            {
                                // Set default Duty type to "ICT Duty Rota"
                                this.cboDutyType.SelectedValue = "2";
                            }
                            else if (this.litCostCenterCode.Text.Trim() == "7400" ||
                                this.litCostCenterCode.Text.Trim() == "7300")
                            {
                                // Set default Duty type to "Purchasing Duty Rota"
                                this.cboDutyType.SelectedValue = "4";
                            }
                            else if (this.litCostCenterCode.Text.Trim() == "5300" ||
                                this.litCostCenterCode.Text.Trim() == "5200" ||
                                this.litCostCenterCode.Text.Trim() == "5400" ||
                                this.litCostCenterCode.Text.Trim() == "3250")
                            {
                                // Set default Duty type to "Engineering Duty Rota"
                                this.cboDutyType.SelectedValue = "3";
                            }
                            else
                            {
                                this.cboDutyType.SelectedIndex = -1;
                                this.cboDutyType.Text = string.Empty;
                            }
                        }
                        #endregion
                    }

                    // Clear data storage
                    Session.Remove("DutyROTAEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("DutyROTAEntryStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    InitializeControls(this.CurrentFormLoadType);

                    #region Check if need to load data in the grid
                    if (this.AutoID > 0)
                    {
                        GetDutyROTA(this.AutoID);
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
                this.litCostCenterCode.Text = string.Empty;
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
                        this.litCostCenterCode.Text = UIHelper.ConvertObjectToString(empInfo.CostCenter);
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
                            this.litCostCenterCode.Text = UIHelper.ConvertObjectToString(rawData.CostCenter);
                            //    }
                            //    else
                            //    {
                            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                            //    }
                            //}
                        }
                        #endregion
                    }

                    #region Set the default Duty Type selection
                    if (!string.IsNullOrEmpty(this.litCostCenterCode.Text) &&
                        this.cboDutyType.Items.Count > 0)
                    {
                        if (this.litCostCenterCode.Text.Trim() == "7600")
                        {
                            // Set default Duty type to "ICT Duty Rota"
                            this.cboDutyType.SelectedValue = "2";
                        }
                        else if (this.litCostCenterCode.Text.Trim() == "7400" ||
                            this.litCostCenterCode.Text.Trim() == "7300")
                        {
                            // Set default Duty type to "Purchasing Duty Rota"
                            this.cboDutyType.SelectedValue = "4";
                        }
                        else if (this.litCostCenterCode.Text.Trim() == "5300" ||
                            this.litCostCenterCode.Text.Trim() == "5200" ||
                            this.litCostCenterCode.Text.Trim() == "5400" ||
                            this.litCostCenterCode.Text.Trim() == "3250")
                        {
                            // Set default Duty type to "Engineering Duty Rota"
                            this.cboDutyType.SelectedValue = "3";
                        }
                        else
                        {
                            this.cboDutyType.SelectedIndex = -1;
                            this.cboDutyType.Text = string.Empty;
                        }
                    }
                    #endregion
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
                UIHelper.PAGE_DUTY_ROTA_ENTRY
            ),
            false);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            // Check if there is selected record to delete
            if (this.CurrentRecord == null)
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
                proxy.InsertUpdateDeleteDutyROTA(Convert.ToInt32(UIHelper.SaveType.Delete),
                    (new List<DutyROTAEntity>() { this.CurrentRecord }),
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
                        String.Format(UIHelper.PAGE_DUTY_ROTA_INQ + "?{0}={1}",
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
                GetDutyROTA(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (this.AutoID > 0)
            {
                GetDutyROTA(this.AutoID);
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
                this.litCostCenterCode.Text = string.Empty;

                this.dtpEffectiveDate.SelectedDate = null;
                this.dtpEndingDate.SelectedDate = null;

                this.cboDutyType.SelectedIndex = -1;
                this.cboDutyType.Text = string.Empty;
                #endregion

                #region Clear sessions
                this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentRecord"] = null;
                ViewState["EmployeeNo"] = null;
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
                }
                #endregion

                #region Validate Effective Date and Ending Date
                if (this.dtpEffectiveDate.SelectedDate != null &&
                    this.dtpEndingDate.SelectedDate != null &&
                    this.dtpEffectiveDate.SelectedDate > this.dtpEndingDate.SelectedDate)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                    this.ErrorType = ValidationErrorType.InvalidDateRange;
                    this.cusValEffectiveDate.Validate();
                    errorCount++;
                }
                else
                {
                    // Check Effective Date
                    if (this.dtpEffectiveDate.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoEffectiveDate.ToString();
                        this.ErrorType = ValidationErrorType.NoEffectiveDate;
                        this.cusValEffectiveDate.Validate();
                        errorCount++;
                    }

                    // Check Ending Date
                    if (this.dtpEndingDate.SelectedDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoEndingDate.ToString();
                        this.ErrorType = ValidationErrorType.NoEndingDate;
                        this.cusValEndingDate.Validate();
                        errorCount++;
                    }
                }
                #endregion

                #region Check Duty Type
                if (string.IsNullOrEmpty(this.cboDutyType.SelectedValue) ||
                    this.cboDutyType.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDutyType.ToString();
                    this.ErrorType = ValidationErrorType.NoDutyType;
                    this.cusValDutyType.Validate();
                    errorCount++;
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
                    List<DutyROTAEntity> recordToInsertList = new List<DutyROTAEntity>();

                    recordToInsertList.Add(new DutyROTAEntity()
                    {
                        EmpNo = empNo,
                        EffectiveDate = this.dtpEffectiveDate.SelectedDate,
                        EndingDate = this.dtpEndingDate.SelectedDate,
                        DutyType = this.cboDutyType.SelectedValue,
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
                    this.CurrentRecord.EffectiveDate = this.dtpEffectiveDate.SelectedDate;
                    this.CurrentRecord.EndingDate = this.dtpEndingDate.SelectedDate;
                    this.CurrentRecord.DutyType = this.cboDutyType.SelectedValue;
                    this.CurrentRecord.LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]));
                    this.CurrentRecord.LastUpdateTime = DateTime.Now;

                    // Initialize collection
                    List<DutyROTAEntity> recordToUpdateList = new List<DutyROTAEntity>() { this.CurrentRecord };

                    SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    // Initialize collection
                    List<DutyROTAEntity> recordToUpdateList = new List<DutyROTAEntity>() { this.CurrentRecord };

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
                else if (this.ErrorType == ValidationErrorType.NoEffectiveDate)
                {
                    validator.ErrorMessage = "Effective Date is required.";
                    validator.ToolTip = "Effective Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEndingDate)
                {
                    validator.ErrorMessage = "Ending Date is required.";
                    validator.ToolTip = "Ending Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDutyType)
                {
                    validator.ErrorMessage = "Duty Type is required.";
                    validator.ToolTip = "Duty Type is required.";
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
                    validator.ErrorMessage = "The specified date range is invalid. Please ensure that Effective Date is less than Ending Date!";
                    validator.ToolTip = "The specified date range is invalid. Please ensure that Effective Date is less than Ending Date!";
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

        protected void txtEmpNo_TextChanged(object sender, EventArgs e)
        {
            this.btnGet_Click(this.btnGet, new EventArgs());
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
            this.litUpdateUser.Text = "Not defined";
            this.litLastUpdateTime.Text = "Not defined";
            this.litCostCenterCode.Text = string.Empty;

            this.dtpEffectiveDate.SelectedDate = null;
            this.dtpEndingDate.SelectedDate = null;

            this.cboDutyType.SelectedIndex = -1;
            this.cboDutyType.Text = string.Empty;
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
            this.DutyTypeList.Clear();

            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentRecord"] = null;
            ViewState["CurrentFilterOption"] = null;
            ViewState["EmployeeNo"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.DutyROTAEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.DutyROTAEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.DutyROTAEntryStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["CurrentFormLoadType"]);
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
            if (this.DutyROTAEntryStorage.ContainsKey("CurrentRecord"))
                this.CurrentRecord = this.DutyROTAEntryStorage["CurrentRecord"] as DutyROTAEntity;
            else
                this.CurrentRecord = null;

            if (this.DutyROTAEntryStorage.ContainsKey("DutyTypeList"))
                this.DutyTypeList = this.DutyROTAEntryStorage["DutyTypeList"] as List<DutyROTAEntity>;
            else
                this.DutyTypeList = null;

            if (this.DutyROTAEntryStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.DutyROTAEntryStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.DutyROTAEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("litCostCenterCode"))
                this.litCostCenterCode.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["litCostCenterCode"]);
            else
                this.litCostCenterCode.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("litUpdateUser"))
                this.litUpdateUser.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["litUpdateUser"]);
            else
                this.litUpdateUser.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.DutyROTAEntryStorage.ContainsKey("dtpEffectiveDate"))
                this.dtpEffectiveDate.SelectedDate = UIHelper.ConvertObjectToDate(this.DutyROTAEntryStorage["dtpEffectiveDate"]);
            else
                this.dtpEffectiveDate.SelectedDate = null;

            if (this.DutyROTAEntryStorage.ContainsKey("dtpEndingDate"))
                this.dtpEndingDate.SelectedDate = UIHelper.ConvertObjectToDate(this.DutyROTAEntryStorage["dtpEndingDate"]);
            else
                this.dtpEndingDate.SelectedDate = null;

            if (this.DutyROTAEntryStorage.ContainsKey("cboDutyType"))
                this.cboDutyType.SelectedValue = UIHelper.ConvertObjectToString(this.DutyROTAEntryStorage["cboDutyType"]);
            else
            {
                this.cboDutyType.SelectedIndex = -1;
                this.cboDutyType.Text = string.Empty;
            }
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.DutyROTAEntryStorage.Clear();
            this.DutyROTAEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.DutyROTAEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.DutyROTAEntryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.DutyROTAEntryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.DutyROTAEntryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.DutyROTAEntryStorage.Add("litCostCenterCode", this.litCostCenterCode.Text.Trim());
            this.DutyROTAEntryStorage.Add("litUpdateUser", this.litUpdateUser.Text.Trim());
            this.DutyROTAEntryStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());

            this.DutyROTAEntryStorage.Add("dtpEffectiveDate", this.dtpEffectiveDate.SelectedDate);
            this.DutyROTAEntryStorage.Add("dtpEndingDate", this.dtpEndingDate.SelectedDate);
            this.DutyROTAEntryStorage.Add("cboDutyType", this.cboDutyType.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.DutyROTAEntryStorage.Add("CallerForm", this.CallerForm);
            this.DutyROTAEntryStorage.Add("AutoID", this.AutoID);
            this.DutyROTAEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.DutyROTAEntryStorage.Add("CurrentRecord", this.CurrentRecord);
            this.DutyROTAEntryStorage.Add("DutyTypeList", this.DutyTypeList);
            this.DutyROTAEntryStorage.Add("EmployeeNo", this.EmployeeNo);
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
                    this.cboDutyType.Enabled = true;

                    // Initialize control values
                    this.litEmpName.Text = "Not defined";
                    this.litPosition.Text = "Not defined";
                    this.litCostCenter.Text = "Not defined";
                    this.litUpdateUser.Text = "Not defined";
                    this.litLastUpdateTime.Text = "Not defined";
                    this.litCostCenterCode.Text = string.Empty;

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
                    this.dtpEffectiveDate.Enabled = true;
                    this.dtpEndingDate.Enabled = true;
                    this.cboDutyType.Enabled = true;

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
                    this.dtpEffectiveDate.Enabled = false;
                    this.dtpEndingDate.Enabled = false;
                    this.cboDutyType.Enabled = false;

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
            FillDutyTypeCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void FillDutyTypeCombo(bool reloadFromDB, string defaultValue = "")
        {
            try
            {
                List<DutyROTAEntity> comboSource = new List<DutyROTAEntity>();
                if (this.DutyTypeList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.DutyTypeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    comboSource = proxy.GetDutyROTAType(ref error, ref innerError);
                    if (comboSource != null && comboSource.Count() > 0)
                    {
                        // Add blank item
                        comboSource.Insert(0, new DutyROTAEntity()
                        {
                            AutoID = 0,
                            DutyType = UIHelper.CONST_COMBO_EMTYITEM_ID,
                            DutyDescription = string.Empty,
                            DutyAllowance = 0
                        });
                    }
                }

                // Store to session
                this.DutyTypeList = comboSource;

                #region Bind data to combobox
                this.cboDutyType.DataSource = this.DutyTypeList;
                this.cboDutyType.DataTextField = "DutyDescription";
                this.cboDutyType.DataValueField = "DutyType";
                this.cboDutyType.DataBind();

                if (this.cboDutyType.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboDutyType.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void GetDutyROTA(int autoID)
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
                this.litCostCenterCode.Text = string.Empty;

                this.dtpEffectiveDate.SelectedDate = null;
                this.dtpEndingDate.SelectedDate = null;

                this.cboDutyType.SelectedIndex = -1;
                this.cboDutyType.Text = string.Empty;
                #endregion

                if (Session["SelectedDutyROTA"] != null)
                {
                    this.CurrentRecord = Session["SelectedDutyROTA"] as DutyROTAEntity;
                }
                else
                {
                    #region Fetch database record
                    if (autoID == 0)
                        return;

                    List<DutyROTAEntity> gridSource = new List<DutyROTAEntity>();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetDutyROTAEntry(autoID, 0, null, null, string.Empty, 0, 0, ref error, ref innerError);
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
                            this.CurrentRecord = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentRecord != null)
                {
                    this.txtEmpNo.Value = this.CurrentRecord.EmpNo;
                    this.litEmpName.Text = this.CurrentRecord.EmpName;
                    this.litPosition.Text = this.CurrentRecord.Position;
                    this.litCostCenter.Text = this.CurrentRecord.CostCenterFullName;
                    this.litCostCenterCode.Text = this.CurrentRecord.CostCenter;
                    this.litUpdateUser.Text = this.CurrentRecord.LastUpdateUser;
                    this.litLastUpdateTime.Text = this.CurrentRecord.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentRecord.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : string.Empty;
                    this.dtpEffectiveDate.SelectedDate = this.CurrentRecord.EffectiveDate;
                    this.dtpEndingDate.SelectedDate = this.CurrentRecord.EndingDate;
                    this.cboDutyType.SelectedValue = this.CurrentRecord.DutyType;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, List<DutyROTAEntity> dataList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                if (dataList == null)
                    return;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteDutyROTA(Convert.ToInt32(saveType), dataList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to the inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_DUTY_ROTA_INQ + "?{0}={1}&{2}={3}",
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
                this.CurrentRecord = null;
                throw new Exception(ex.Message.ToString());
            }
        }
        #endregion                
    }
}