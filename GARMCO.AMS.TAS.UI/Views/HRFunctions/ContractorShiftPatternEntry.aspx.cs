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
    public partial class ContractorShiftPatternEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToDelete,
            NoContractorNo,
            NoContractorName,
            NoShiftPatCode,
            NoShiftPointer,
            NoDateStarted,
            InvalidExpirationDate
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

        private Dictionary<string, object> ContractorShiftPatEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ContractorShiftPatEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ContractorShiftPatEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ContractorShiftPatEntryStorage"] = value;
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

        private ContractorEntity CurrentRecord
        {
            get
            {
                return ViewState["CurrentRecord"] as ContractorEntity;
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

        private List<UserDefinedCodes> GroupTypeList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["GroupTypeList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["GroupTypeList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["GroupTypeList"] = value;
            }
        }

        private List<UserDefinedCodes> ReligionList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["ReligionList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["ReligionList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["ReligionList"] = value;
            }
        }

        private List<UserDefinedCodes> ContractorCompanyList
        {
            get
            {
                List<UserDefinedCodes> list = ViewState["ContractorCompanyList"] as List<UserDefinedCodes>;
                if (list == null)
                    ViewState["ContractorCompanyList"] = list = new List<UserDefinedCodes>();

                return list;
            }
            set
            {
                ViewState["ContractorCompanyList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTSHFENT.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_ENTRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Visible = this.Master.IsCreateAllowed;
                this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSave.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ContractorShiftPatEntryStorage.Count > 0)
                {
                    if (this.ContractorShiftPatEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetContractorInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtContractorNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        this.txtContractorName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("ContractorShiftPatEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("ContractorShiftPatEntryStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    InitializeControls(this.CurrentFormLoadType);

                    #region Check if need to load record
                    if (this.AutoID > 0)
                    {
                        GetContractorShiftPatternRecord(this.AutoID);
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
            //try
            //{
            //    #region Perform data validation
            //    // Check Employee No.
            //    if (UIHelper.ConvertObjectToInt(this.txtContractorNo.Text) == 0)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.NoEmployeeNo.ToString();
            //        this.ErrorType = ValidationErrorType.NoEmployeeNo;
            //        this.cusValEmpNo.Validate();
            //        return;
            //    }
            //    #endregion

            //    #region Initialize control values and variables
            //    this.txtContractorName.Text = "Not defined";
            //    this.litPosition.Text = "Not defined";
            //    this.litCostCenter.Text = "Not defined";
            //    #endregion

            //    int empNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
            //    if (empNo.ToString().Length == 4)
            //    {
            //        empNo += 10000000;

            //        // Display the formatted Emp. No.
            //        this.txtContractorNo.Text = empNo.ToString();
            //    }

            //    string error = string.Empty;
            //    string innerError = string.Empty;

            //    EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
            //    if (empInfo != null)
            //    {
            //        if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
            //        {
            //            #region Check if cost center exist in the allowed cost center list
            //            //if (this.Master.AllowedCostCenterList.Count > 0)
            //            //{
            //            //    string allowedCC = this.Master.AllowedCostCenterList
            //            //        .Where(a => a == UIHelper.ConvertObjectToString(empInfo.CostCenter))
            //            //        .FirstOrDefault();
            //            //    if (!string.IsNullOrEmpty(allowedCC))
            //            //    {
            //            this.txtContractorName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
            //            this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
            //            this.litCostCenter.Text = string.Format("{0} - {1}",
            //                empInfo.CostCenter,
            //                empInfo.CostCenterName);
            //            //    }
            //            //    else
            //            //    {
            //            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
            //            //    }
            //            //}
            //            #endregion
            //        }
            //        else
            //        {
            //            #region Get employee info from the employee master
            //            DALProxy proxy = new DALProxy();
            //            var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
            //            if (rawData != null)
            //            {
            //                //if (this.Master.AllowedCostCenterList.Count > 0)
            //                //{
            //                //    string allowedCC = this.Master.AllowedCostCenterList
            //                //        .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
            //                //        .FirstOrDefault();
            //                //    if (!string.IsNullOrEmpty(allowedCC))
            //                //    {
            //                this.txtContractorName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
            //                this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
            //                this.litCostCenter.Text = string.Format("{0} - {1}",
            //                   rawData.CostCenter,
            //                   rawData.CostCenterName);
            //                //    }
            //                //    else
            //                //    {
            //                //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
            //                //    }
            //                //}
            //            }
            //            #endregion
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ShowErrorMessage(ex);
            //}
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
                proxy.InsertUpdateDeleteContractorShiftPattern(Convert.ToInt32(UIHelper.SaveType.Delete), (new List<ContractorEntity>() { this.CurrentRecord }), ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Shift Pattern Change Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ + "?{0}={1}",
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
                GetContractorShiftPatternRecord(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset controls
            this.txtContractorNo.Text = string.Empty;
            this.txtContractorName.Text = string.Empty;
            this.litUpdateUser.Text = string.Empty;
            this.litLastUpdateTime.Text = string.Empty;

            this.dtpDateStarted.SelectedDate = null;
            this.dtpExpirationDate.SelectedDate = null;

            this.cboContractorCompany.Text = string.Empty;
            this.cboContractorCompany.SelectedIndex = -1;
            this.cboGroupType.Text = string.Empty;
            this.cboGroupType.SelectedIndex = -1;
            this.cboReligion.Text = string.Empty;
            this.cboReligion.SelectedIndex = -1;
            this.cboShiftPatCode.Text = string.Empty;
            this.cboShiftPatCode.SelectedIndex = -1;
            this.cboShiftPointer.Text = string.Empty;
            this.cboShiftPointer.SelectedIndex = -1;
            #endregion

            #region Clear sessions
            this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentRecord"] = null;
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
                // Check Contractor No.
                if (UIHelper.ConvertObjectToInt(this.txtContractorNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoContractorNo.ToString();
                    this.ErrorType = ValidationErrorType.NoContractorNo;
                    this.cusValContractorNo.Validate();
                    errorCount++;
                }

                // Check Contractor Name
                if (this.txtContractorName.Text.Trim() == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoContractorName.ToString();
                    this.ErrorType = ValidationErrorType.NoContractorName;
                    this.cusValContractorName.Validate();
                    errorCount++;
                }

                // Check Date Started
                if (this.dtpDateStarted.SelectedDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoDateStarted.ToString();
                    this.ErrorType = ValidationErrorType.NoDateStarted;
                    this.cusValDateStarted.Validate();
                    errorCount++;
                }

                // Check Shift Pattern Code
                if (string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue) ||
                    this.cboShiftPatCode.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftPatCode.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftPatCode;
                    this.cusValShiftPatCode.Validate();
                    errorCount++;
                }

                // Check Shift Pointer
                if (string.IsNullOrEmpty(this.cboShiftPointer.SelectedValue) ||
                    UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftPointer.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftPointer;
                    this.cusValShiftPointer.Validate();
                    errorCount++;
                }

                // Validate Expiration Date
                if (this.dtpDateStarted.SelectedDate != null &&
                    this.dtpExpirationDate.SelectedDate != null)
                {
                    if (this.dtpExpirationDate.SelectedDate < this.dtpDateStarted.SelectedDate)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidExpirationDate.ToString();
                        this.ErrorType = ValidationErrorType.InvalidExpirationDate;
                        this.cusValExpirationDate.Validate();
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
                    List<ContractorEntity> recordToInsertList = new List<ContractorEntity>();

                    recordToInsertList.Add(new ContractorEntity()
                    {
                        ContractorNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text),
                        ContractorName = this.txtContractorName.Text.Trim(),
                        GroupCode = this.cboGroupType.SelectedValue,
                        GroupDesc = this.cboGroupType.Text,
                        ReligionCode = this.cboReligion.SelectedValue,
                        ReligionDesc = this.cboReligion.Text,
                        SupplierNo = UIHelper.ConvertObjectToDouble(this.cboContractorCompany.SelectedValue),
                        SupplierName = this.cboContractorCompany.Text,                        
                        ShiftPatCode = this.cboShiftPatCode.SelectedValue,
                        ShiftPointer = UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue),
                        DateJoined = this.dtpDateStarted.SelectedDate,
                        DateResigned = this.dtpExpirationDate.SelectedDate,
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
                    this.CurrentRecord.ContractorName = this.txtContractorName.Text.Trim();
                    this.CurrentRecord.GroupCode = this.cboGroupType.SelectedValue;
                    this.CurrentRecord.GroupDesc = this.cboGroupType.Text;
                    this.CurrentRecord.ReligionCode = this.cboReligion.SelectedValue;
                    this.CurrentRecord.ReligionDesc = this.cboReligion.Text;
                    this.CurrentRecord.SupplierNo = UIHelper.ConvertObjectToDouble(this.cboContractorCompany.SelectedValue);
                    this.CurrentRecord.SupplierName = this.cboContractorCompany.Text;
                    this.CurrentRecord.ShiftPatCode = this.cboShiftPatCode.SelectedValue;
                    this.CurrentRecord.ShiftPointer = UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue);
                    this.CurrentRecord.DateJoined = this.dtpDateStarted.SelectedDate;
                    this.CurrentRecord.DateResigned = this.dtpExpirationDate.SelectedDate;
                    this.CurrentRecord.LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]));
                    this.CurrentRecord.LastUpdateTime = DateTime.Now;

                    // Initialize collection
                    List<ContractorEntity> recordToUpdateList = new List<ContractorEntity>() { this.CurrentRecord };

                    SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    // Initialize collection
                    List<ContractorEntity> recordToUpdateList = new List<ContractorEntity>() { this.CurrentRecord };

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
                Response.Redirect(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ, false);
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
                else if (this.ErrorType == ValidationErrorType.NoContractorNo)
                {
                    validator.ErrorMessage = "Contractor No. is required.";
                    validator.ToolTip = "Contractor No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoContractorName)
                {
                    validator.ErrorMessage = "Contractor Name is required.";
                    validator.ToolTip = "Contractor Name is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateStarted)
                {
                    validator.ErrorMessage = "Date Started is required.";
                    validator.ToolTip = "Date Started is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftPatCode)
                {
                    validator.ErrorMessage = "Shift Pat. Code is required.";
                    validator.ToolTip = "Shift Pat. Code is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftPointer)
                {
                    validator.ErrorMessage = "Shift Pointer is required.";
                    validator.ToolTip = "Shift Pointer is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidExpirationDate)
                {
                    validator.ErrorMessage = "Expiration Date should be greater than Date Started!";
                    validator.ToolTip = "Expiration Date should be greater than Date Started!";
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
                // Reset Shift Pointer
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;

                FillShiftPointerCombo(this.cboShiftPatCode.SelectedValue);                                
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtContractorNo.Text = string.Empty;
            this.txtContractorName.Text = string.Empty;
            this.litUpdateUser.Text = string.Empty;
            this.litLastUpdateTime.Text = string.Empty;

            this.dtpDateStarted.SelectedDate = null;
            this.dtpExpirationDate.SelectedDate = null;

            this.cboContractorCompany.Text = string.Empty;
            this.cboContractorCompany.SelectedIndex = -1;
            this.cboGroupType.Text = string.Empty;
            this.cboGroupType.SelectedIndex = -1;
            this.cboReligion.Text = string.Empty;
            this.cboReligion.SelectedIndex = -1;
            this.cboShiftPatCode.Text = string.Empty;
            this.cboShiftPatCode.SelectedIndex = -1;
            this.cboShiftPointer.Text = string.Empty;
            this.cboShiftPointer.SelectedIndex = -1;
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
            this.ShiftPatternCodeList.Clear();
            this.ShiftPointerCodeList.Clear();
            this.ContractorCompanyList.Clear();
            this.GroupTypeList.Clear();
            this.ReligionList.Clear();

            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentRecord"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ContractorShiftPatEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ContractorShiftPatEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.ContractorShiftPatEntryStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["CurrentFormLoadType"]);
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
            if (this.ContractorShiftPatEntryStorage.ContainsKey("CurrentRecord"))
                this.CurrentRecord = this.ContractorShiftPatEntryStorage["CurrentRecord"] as ContractorEntity;
            else
                this.CurrentRecord = null;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("ShiftPatternCodeList"))
                this.ShiftPatternCodeList = this.ContractorShiftPatEntryStorage["ShiftPatternCodeList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPatternCodeList = null;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("ShiftPointerCodeList"))
                this.ShiftPointerCodeList = this.ContractorShiftPatEntryStorage["ShiftPointerCodeList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPointerCodeList = null;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("ContractorCompanyList"))
                this.ContractorCompanyList = this.ContractorShiftPatEntryStorage["ContractorCompanyList"] as List<UserDefinedCodes>;
            else
                this.ContractorCompanyList = null;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("GroupTypeList"))
                this.GroupTypeList = this.ContractorShiftPatEntryStorage["GroupTypeList"] as List<UserDefinedCodes>;
            else
                this.GroupTypeList = null;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("ReligionList"))
                this.ReligionList = this.ContractorShiftPatEntryStorage["ReligionList"] as List<UserDefinedCodes>;
            else
                this.ReligionList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ContractorShiftPatEntryStorage.ContainsKey("txtContractorNo"))
                this.txtContractorNo.Text = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["txtContractorNo"]);
            else
                this.txtContractorNo.Text = string.Empty;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("txtContractorName"))
                this.txtContractorName.Text = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["txtContractorName"]);
            else
                this.txtContractorName.Text = string.Empty;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("litUpdateUser"))
                this.litUpdateUser.Text = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["litUpdateUser"]);
            else
                this.litUpdateUser.Text = string.Empty;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("dtpDateStarted"))
                this.dtpDateStarted.SelectedDate = UIHelper.ConvertObjectToDate(this.ContractorShiftPatEntryStorage["dtpDateStarted"]);
            else
                this.dtpDateStarted.SelectedDate = null;

            if (this.ContractorShiftPatEntryStorage.ContainsKey("dtpExpirationDate"))
                this.dtpExpirationDate.SelectedDate = UIHelper.ConvertObjectToDate(this.ContractorShiftPatEntryStorage["dtpExpirationDate"]);
            else
                this.dtpExpirationDate.SelectedDate = null;
                       
            if (this.ContractorShiftPatEntryStorage.ContainsKey("cboContractorCompany"))
                this.cboContractorCompany.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["cboContractorCompany"]);
            else
            {
                this.cboContractorCompany.Text = string.Empty;
                this.cboContractorCompany.SelectedIndex = -1;
            }

            if (this.ContractorShiftPatEntryStorage.ContainsKey("cboGroupType"))
                this.cboGroupType.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["cboGroupType"]);
            else
            {
                this.cboGroupType.Text = string.Empty;
                this.cboGroupType.SelectedIndex = -1;
            }

            if (this.ContractorShiftPatEntryStorage.ContainsKey("cboReligion"))
                this.cboReligion.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["cboReligion"]);
            else
            {
                this.cboReligion.Text = string.Empty;
                this.cboReligion.SelectedIndex = -1;
            }

            if (this.ContractorShiftPatEntryStorage.ContainsKey("cboShiftPatCode"))
                this.cboShiftPatCode.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["cboShiftPatCode"]);
            else
            {
                this.cboShiftPatCode.Text = string.Empty;
                this.cboShiftPatCode.SelectedIndex = -1;
            }

            if (this.ContractorShiftPatEntryStorage.ContainsKey("cboShiftPointer"))
                this.cboShiftPointer.SelectedValue = UIHelper.ConvertObjectToString(this.ContractorShiftPatEntryStorage["cboShiftPointer"]);
            else
            {
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;
            }
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ContractorShiftPatEntryStorage.Clear();
            this.ContractorShiftPatEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ContractorShiftPatEntryStorage.Add("txtContractorNo", this.txtContractorNo.Text.Trim());
            this.ContractorShiftPatEntryStorage.Add("txtContractorName", this.txtContractorName.Text.Trim());
            this.ContractorShiftPatEntryStorage.Add("litUpdateUser", this.litUpdateUser.Text.Trim());
            this.ContractorShiftPatEntryStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());
            this.ContractorShiftPatEntryStorage.Add("dtpDateStarted", this.dtpDateStarted.SelectedDate);
            this.ContractorShiftPatEntryStorage.Add("dtpExpirationDate", this.dtpExpirationDate.SelectedDate);            
            this.ContractorShiftPatEntryStorage.Add("cboContractorCompany", this.cboContractorCompany.SelectedValue);
            this.ContractorShiftPatEntryStorage.Add("cboGroupType", this.cboGroupType.SelectedValue);
            this.ContractorShiftPatEntryStorage.Add("cboReligion", this.cboReligion.SelectedValue);
            this.ContractorShiftPatEntryStorage.Add("cboShiftPatCode", this.cboShiftPatCode.SelectedValue);
            this.ContractorShiftPatEntryStorage.Add("cboShiftPointer", this.cboShiftPointer.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.ContractorShiftPatEntryStorage.Add("CallerForm", this.CallerForm);
            this.ContractorShiftPatEntryStorage.Add("AutoID", this.AutoID);
            this.ContractorShiftPatEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.ContractorShiftPatEntryStorage.Add("CurrentRecord", this.CurrentRecord);
            this.ContractorShiftPatEntryStorage.Add("ShiftPatternCodeList", this.ShiftPatternCodeList);
            this.ContractorShiftPatEntryStorage.Add("ShiftPointerCodeList", this.ShiftPointerCodeList);
            this.ContractorShiftPatEntryStorage.Add("ContractorCompanyList", this.ContractorCompanyList);
            this.ContractorShiftPatEntryStorage.Add("GroupTypeList", this.GroupTypeList);
            this.ContractorShiftPatEntryStorage.Add("ReligionList", this.ReligionList);
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
                    this.txtContractorNo.Enabled = true;
                    this.txtContractorName.Enabled = true;
                    this.dtpDateStarted.Enabled = true;
                    this.dtpExpirationDate.Enabled = true;
                    this.cboContractorCompany.Enabled = true;
                    this.cboGroupType.Enabled = true;
                    this.cboReligion.Enabled = true;
                    this.cboShiftPatCode.Enabled = true;
                    this.cboShiftPointer.Enabled = true;
                    
                    // Initialize control values
                    this.txtContractorName.Text = string.Empty;
                    this.litUpdateUser.Text = "Not defined";
                    this.litLastUpdateTime.Text = "Not defined";
                    this.dtpDateStarted.SelectedDate = null;
                    this.dtpExpirationDate.SelectedDate = null;

                    // Setup buttons
                    this.btnGet.Enabled = true;
                    this.btnFindEmployee.Enabled = true;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    // Set focus to Contractor No.
                    this.txtContractorNo.Focus();

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing training record
                    // Setup controls 
                    this.txtContractorNo.Enabled = false;
                    this.txtContractorName.Enabled = true;
                    this.dtpDateStarted.Enabled = true;
                    this.dtpExpirationDate.Enabled = true;
                    this.cboContractorCompany.Enabled = true;
                    this.cboGroupType.Enabled = true;
                    this.cboReligion.Enabled = true;
                    this.cboShiftPatCode.Enabled = true;
                    this.cboShiftPointer.Enabled = true;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = true;
                    this.btnReset.Enabled = false;

                    // Set focus to Contractor Name
                    this.txtContractorName.Focus();

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing training record (read-only)
                    // Setup controls 
                    this.txtContractorNo.Enabled = false;
                    this.txtContractorName.Enabled = false;
                    this.dtpDateStarted.Enabled = false;
                    this.dtpExpirationDate.Enabled = false;
                    this.cboContractorCompany.Enabled = false;
                    this.cboGroupType.Enabled = false;
                    this.cboReligion.Enabled = false;
                    this.cboShiftPatCode.Enabled = false;
                    this.cboShiftPointer.Enabled = false;

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
            FillContractorCompanyCombo(reloadFromDB);
            FillGroupTypeCombo(reloadFromDB);
            FillReligionCombo(reloadFromDB);
            FillShiftPatternCodeCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetContractorShiftPatternRecord(int autoID)
        {
            try
            {
                #region Initialize controls
                this.txtContractorNo.Text = string.Empty;
                this.txtContractorName.Text = string.Empty;
                this.litUpdateUser.Text = string.Empty;
                this.litLastUpdateTime.Text = string.Empty;

                this.dtpDateStarted.SelectedDate = null;
                this.dtpExpirationDate.SelectedDate = null;
                                
                this.cboContractorCompany.Text = string.Empty;
                this.cboContractorCompany.SelectedIndex = -1;
                this.cboGroupType.Text = string.Empty;
                this.cboGroupType.SelectedIndex = -1;
                this.cboReligion.Text = string.Empty;
                this.cboReligion.SelectedIndex = -1;
                this.cboShiftPatCode.Text = string.Empty;
                this.cboShiftPatCode.SelectedIndex = -1;
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;
                #endregion

                if (Session["SelectedContractorShiftPattern"] != null)
                {
                    this.CurrentRecord = Session["SelectedContractorShiftPattern"] as ContractorEntity;
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
                    var rawData = proxy.GetContractorShiftPattern(autoID, 0, string.Empty, null, null, null, null, ref error, ref innerError);
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
                            this.CurrentRecord = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentRecord != null)
                {
                    this.txtContractorNo.Value =  this.CurrentRecord.ContractorNo;
                    this.txtContractorName.Text = this.CurrentRecord.ContractorName;
                    this.litUpdateUser.Text = this.CurrentRecord.LastUpdateUser;
                    this.litLastUpdateTime.Text = this.CurrentRecord.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentRecord.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : string.Empty;

                    if (this.cboContractorCompany.Items.Count > 0)
                        this.cboContractorCompany.SelectedValue = this.CurrentRecord.SupplierNo.ToString();

                    if (this.cboGroupType.Items.Count > 0)
                        this.cboGroupType.SelectedValue = this.CurrentRecord.GroupCode;

                    if (this.cboReligion.Items.Count > 0)
                        this.cboReligion.SelectedValue = this.CurrentRecord.ReligionCode;

                    if (this.cboShiftPatCode.Items.Count > 0)
                    {
                        this.cboShiftPatCode.SelectedValue = this.CurrentRecord.ShiftPatCode;

                        if (!string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue))
                            this.cboShiftPatCode_SelectedIndexChanged(this.cboShiftPatCode, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPatCode.Text, string.Empty, this.cboShiftPatCode.SelectedValue, string.Empty));
                    }

                    if (this.cboShiftPointer.Items.Count > 0)
                        this.cboShiftPointer.SelectedValue = this.CurrentRecord.ShiftPointer.ToString();

                    this.dtpDateStarted.SelectedDate = this.CurrentRecord.DateJoined;
                    this.dtpExpirationDate.SelectedDate = this.CurrentRecord.DateResigned;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, List<ContractorEntity> shiftPatternList)
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
                proxy.InsertUpdateDeleteContractorShiftPattern(Convert.ToInt32(saveType), shiftPatternList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Redirect to Shift Pattern Changes Inquiry page
                    Response.Redirect
                    (
                        String.Format(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ + "?{0}={1}&{2}={3}",
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
                            throw new Exception(innerError);
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

                if (comboSource.Count > 0)
                {
                    // Add blank item
                    comboSource.Insert(0, new ShiftPatternEntity() { ShiftPatDesc = string.Empty, ShiftPatCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
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
                        throw new Exception(innerError);
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

                if (comboSource.Count > 0)
                {
                    // Add blank item
                    comboSource.Insert(0, new ShiftPatternEntity() { ShiftPointer = 0, ShiftPointerCode = string.Empty });
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

        private void FillGroupTypeCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCDesc1, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.GroupTypeList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.GroupTypeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.GROUP_CODES), ref error, ref innerError);
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
                this.GroupTypeList = comboSource;

                #region Bind data to combobox
                this.cboGroupType.DataSource = comboSource;
                this.cboGroupType.DataTextField = "UDCDesc1";
                this.cboGroupType.DataValueField = "UDCCode";
                this.cboGroupType.DataBind();

                if (this.cboGroupType.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboGroupType.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillReligionCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCDesc1, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.ReligionList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.ReligionList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.RELIGION_CODES), ref error, ref innerError);
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
                this.ReligionList = comboSource;

                #region Bind data to combobox
                this.cboReligion.DataSource = comboSource;
                this.cboReligion.DataTextField = "UDCDesc1";
                this.cboReligion.DataValueField = "UDCCode";
                this.cboReligion.DataBind();

                if (this.cboReligion.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboReligion.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void FillContractorCompanyCombo(bool reloadFromDB, UIHelper.UDCSorterColumn sorter = UIHelper.UDCSorterColumn.UDCDesc1, string defaultValue = "")
        {
            try
            {
                List<UserDefinedCodes> rawData = new List<UserDefinedCodes>();
                List<UserDefinedCodes> comboSource = new List<UserDefinedCodes>();

                if (this.ContractorCompanyList.Count > 0 && !reloadFromDB)
                {
                    rawData = this.ContractorCompanyList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetTimesheetUDCCodes(Convert.ToByte(UIHelper.TimesheetUDCCode.SUPPLIER_LIST), ref error, ref innerError);
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
                    //comboSource.Insert(0, new UserDefinedCodes() { UDCDesc1 = string.Empty, UDCCode = UIHelper.CONST_COMBO_EMTYITEM_ID });
                }
                #endregion

                // Store to session
                this.ContractorCompanyList = comboSource;

                #region Bind data to combobox
                this.cboContractorCompany.DataSource = comboSource;
                this.cboContractorCompany.DataTextField = "UDCDesc1";
                this.cboContractorCompany.DataValueField = "UDCCode";
                this.cboContractorCompany.DataBind();

                if (this.cboContractorCompany.Items.Count > 0
                    && !string.IsNullOrEmpty(defaultValue))
                {
                    this.cboContractorCompany.SelectedValue = defaultValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }
        #endregion
    }
}