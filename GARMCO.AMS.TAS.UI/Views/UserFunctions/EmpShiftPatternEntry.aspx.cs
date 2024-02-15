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
    public partial class EmpShiftPatternEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToDelete
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

        private Dictionary<string, object> ShiftPatternDetailStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ShiftPatternDetailStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ShiftPatternDetailStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ShiftPatternDetailStorage"] = value;
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

        private ShiftPatternEntity CurrentShiftPattern
        {
            get
            {
                return ViewState["CurrentShiftPattern"] as ShiftPatternEntity;
            }
            set
            {
                ViewState["CurrentShiftPattern"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.SHFTPATDET.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_VIEW_CURRENT_SHIFT_PATTERN_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_VIEW_CURRENT_SHIFT_PATTERN_ENTRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnSave.Visible = this.Master.IsCreateAllowed;
                //this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnBack.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ShiftPatternDetailStorage.Count > 0)
                {
                    if (this.ShiftPatternDetailStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY]));
                    }

                    // Clear data storage
                    Session.Remove("ShiftPatternDetailStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("ShiftPatternDetailStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    InitializeControls(this.CurrentFormLoadType);

                    #region Check if need to fetch Shift Pattern record
                    if (this.AutoID > 0)
                    {
                        GetEmployeeShiftPattern(this.AutoID);
                    }
                    #endregion   
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Action Buttons        
        protected void btnNew_Click(object sender, EventArgs e)
        {

        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            #region Perform data validation
            // Check if there is selected record to delete
            if (this.CurrentShiftPattern == null)
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
                    (new List<ShiftPatternEntity>() { this.CurrentShiftPattern }),
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
                GetEmployeeShiftPattern(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Reset controls
            this.litEmpName.Text = string.Empty;
            this.litPosition.Text = string.Empty;
            this.litCostCenter.Text = string.Empty;
            this.litUpdateUser.Text = string.Empty;
            this.litLastUpdateTime.Text = string.Empty;

            this.cboShiftPatCode.Text = string.Empty;
            this.cboShiftPatCode.SelectedIndex = -1;
            this.cboShiftPointer.Text = string.Empty;
            this.cboShiftPointer.SelectedIndex = -1;
            #endregion

            #region Clear sessions
            this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentShiftPattern"] = null;
            #endregion
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            string error = string.Empty;
            string innerError = string.Empty;
            int empNo = 0;

            try
            {
                UIHelper.SaveType saveType = this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord
                    ? UIHelper.SaveType.Insert
                    : UIHelper.SaveType.Update;

                #region Perform Data Validation
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

                    //recordToInsertList.Add(new ShiftPatternEntity()
                    //{
                    //    EmpNo = empNo,
                    //    ShiftPatCode = this.cboShiftPatCode.SelectedValue,
                    //    ShiftPointer = UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue),
                    //    LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME])),
                    //    LastUpdateTime = DateTime.Now
                    //});

                    //SaveChanges(saveType, recordToInsertList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Update)
                {
                    #region Perform Update Operation
                    // Update data change 
                    //this.CurrentShiftPattern.ShiftPatCode = this.cboShiftPatCode.SelectedValue;
                    //this.CurrentShiftPattern.ShiftPointer = UIHelper.ConvertObjectToInt(this.cboShiftPointer.SelectedValue);
                    //this.CurrentShiftPattern.LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]));
                    //this.CurrentShiftPattern.LastUpdateTime = DateTime.Now;

                    //// Initialize collection
                    //List<ShiftPatternEntity> recordToUpdateList = new List<ShiftPatternEntity>() { this.CurrentShiftPattern };

                    //SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    // Initialize collection
                    //List<ShiftPatternEntity> recordToUpdateList = new List<ShiftPatternEntity>() { this.CurrentShiftPattern };

                    //SaveChanges(saveType, recordToUpdateList);
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
            this.litEmpName.Text = string.Empty;
            this.litPosition.Text = string.Empty;
            this.litCostCenter.Text = string.Empty;
            this.litUpdateUser.Text = string.Empty;
            this.litLastUpdateTime.Text = string.Empty;

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

            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentShiftPattern"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ShiftPatternDetailStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ShiftPatternDetailStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ShiftPatternDetailStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.ShiftPatternDetailStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["CurrentFormLoadType"]);
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
            if (this.ShiftPatternDetailStorage.ContainsKey("CurrentShiftPattern"))
                this.CurrentShiftPattern = this.ShiftPatternDetailStorage["CurrentShiftPattern"] as ShiftPatternEntity;
            else
                this.CurrentShiftPattern = null;

            if (this.ShiftPatternDetailStorage.ContainsKey("ShiftPatternCodeList"))
                this.ShiftPatternCodeList = this.ShiftPatternDetailStorage["ShiftPatternCodeList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPatternCodeList = null;

            if (this.ShiftPatternDetailStorage.ContainsKey("ShiftPointerCodeList"))
                this.ShiftPointerCodeList = this.ShiftPatternDetailStorage["ShiftPointerCodeList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPointerCodeList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.ShiftPatternDetailStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.ShiftPatternDetailStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.ShiftPatternDetailStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.ShiftPatternDetailStorage.ContainsKey("litUpdateUser"))
                this.litUpdateUser.Text = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["litUpdateUser"]);
            else
                this.litUpdateUser.Text = string.Empty;

            if (this.ShiftPatternDetailStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.ShiftPatternDetailStorage.ContainsKey("cboShiftPatCode"))
                this.cboShiftPatCode.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["cboShiftPatCode"]);
            else
            {
                this.cboShiftPatCode.Text = string.Empty;
                this.cboShiftPatCode.SelectedIndex = -1;
            }

            if (this.ShiftPatternDetailStorage.ContainsKey("cboShiftPointer"))
                this.cboShiftPointer.SelectedValue = UIHelper.ConvertObjectToString(this.ShiftPatternDetailStorage["cboShiftPointer"]);
            else
            {
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;
            }
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ShiftPatternDetailStorage.Clear();
            this.ShiftPatternDetailStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ShiftPatternDetailStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.ShiftPatternDetailStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.ShiftPatternDetailStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.ShiftPatternDetailStorage.Add("litUpdateUser", this.litUpdateUser.Text.Trim());
            this.ShiftPatternDetailStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());
            this.ShiftPatternDetailStorage.Add("cboShiftPatCode", this.cboShiftPatCode.SelectedValue);
            this.ShiftPatternDetailStorage.Add("cboShiftPointer", this.cboShiftPointer.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.ShiftPatternDetailStorage.Add("CallerForm", this.CallerForm);
            this.ShiftPatternDetailStorage.Add("AutoID", this.AutoID);
            this.ShiftPatternDetailStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.ShiftPatternDetailStorage.Add("CurrentShiftPattern", this.CurrentShiftPattern);
            this.ShiftPatternDetailStorage.Add("ShiftPatternCodeList", this.ShiftPatternCodeList);
            this.ShiftPatternDetailStorage.Add("ShiftPointerCodeList", this.ShiftPointerCodeList);
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
                    this.cboShiftPatCode.Enabled = true;
                    this.cboShiftPointer.Enabled = true;

                    // Initialize control values
                    this.litEmpName.Text = "Not defined";
                    this.litPosition.Text = "Not defined";
                    this.litCostCenter.Text = "Not defined";
                    this.litUpdateUser.Text = "Not defined";
                    this.litLastUpdateTime.Text = "Not defined";

                    // Setup buttons
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = false;
                    this.btnReset.Enabled = true;

                    break;
                #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing training record
                    // Setup controls 
                    this.cboShiftPatCode.Enabled = true;
                    this.cboShiftPointer.Enabled = true;

                    // Setup buttons
                    this.btnSave.Enabled = true;
                    this.btnDelete.Enabled = true;
                    this.btnReset.Enabled = false;

                    break;
                #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing training record (read-only)
                    // Setup controls 
                    this.cboShiftPatCode.Enabled = false;
                    this.cboShiftPointer.Enabled = false;

                    // Setup buttons
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
        }
        #endregion

        #region Database Access
        private void GetEmployeeShiftPattern(int autoID)
        {
            try
            {
                #region Initialize controls
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                this.litUpdateUser.Text = "Not defined";
                this.litLastUpdateTime.Text = "Not defined";

                this.cboShiftPatCode.Text = string.Empty;
                this.cboShiftPatCode.SelectedIndex = -1;
                this.cboShiftPointer.Text = string.Empty;
                this.cboShiftPointer.SelectedIndex = -1;
                #endregion

                if (Session["SelectedEmpShiftPattern"] != null)
                {
                    this.CurrentShiftPattern = Session["SelectedEmpShiftPattern"] as ShiftPatternEntity;
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
                    var rawData = proxy.GetEmployeeShiftPattern(autoID, 0, string.Empty, 0, 0, ref error, ref innerError);
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
                            this.CurrentShiftPattern = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentShiftPattern != null)
                {
                    this.litEmpName.Text = this.CurrentShiftPattern.EmpFullName;
                    this.litPosition.Text = this.CurrentShiftPattern.Position;
                    this.litCostCenter.Text = this.CurrentShiftPattern.CostCenterFullName;
                    this.litUpdateUser.Text = this.CurrentShiftPattern.LastUpdateUser;
                    this.litLastUpdateTime.Text = this.CurrentShiftPattern.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentShiftPattern.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : string.Empty;

                    this.cboShiftPatCode.SelectedValue = this.CurrentShiftPattern.ShiftPatCode;
                    if (!string.IsNullOrEmpty(this.cboShiftPatCode.SelectedValue))
                        this.cboShiftPatCode_SelectedIndexChanged(this.cboShiftPatCode, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPatCode.Text, string.Empty, this.cboShiftPatCode.SelectedValue, string.Empty));

                    if (this.cboShiftPointer.Items.Count > 0)
                        this.cboShiftPointer.SelectedValue = this.CurrentShiftPattern.ShiftPointer.ToString();
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

                //DALProxy proxy = new DALProxy();
                //proxy.InsertUpdateDeleteShiftPattern(Convert.ToInt32(saveType), shiftPatternList, ref error, ref innerError);
                //if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                //{
                //    if (!string.IsNullOrEmpty(innerError))
                //        throw new Exception(error, new Exception(innerError));
                //    else
                //        throw new Exception(error);
                //}
                //else
                //{
                //    // Redirect to Shift Pattern Changes Inquiry page
                //    Response.Redirect
                //    (
                //        String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ + "?{0}={1}&{2}={3}",
                //        UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                //        this.AutoID,
                //        UIHelper.QUERY_STRING_RELOAD_DATA_KEY,  // Flag that determines whether to invoke the Search button
                //        true.ToString()
                //    ),
                //    false);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                this.CurrentShiftPattern = null;
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
        #endregion
    }
}