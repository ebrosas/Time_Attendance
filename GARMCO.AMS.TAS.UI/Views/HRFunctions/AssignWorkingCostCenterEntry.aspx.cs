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
    public partial class AssignWorkingCostCenterEntry : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoEmployeeNo,
            NoSelectedEmpNo,
            NoWorkingCostCenter,
            NoCatalogStartDate,
            InvalidCatalogDateRange
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

        private Dictionary<string, object> WorkingCostCenterEntryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["WorkingCostCenterEntryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["WorkingCostCenterEntryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["WorkingCostCenterEntryStorage"] = value;
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

        private EmployeeDetail CurrentEmployee
        {
            get
            {
                return ViewState["CurrentEmployee"] as EmployeeDetail;
            }
            set
            {
                ViewState["CurrentEmployee"] = value;
            }
        }

        private List<UDCEntity> JobCatalogList
        {
            get
            {
                List<UDCEntity> list = ViewState["JobCatalogList"] as List<UDCEntity>;
                if (list == null)
                    ViewState["JobCatalogList"] = list = new List<UDCEntity>();

                return list;
            }
            set
            {
                ViewState["JobCatalogList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.WORKCCENTY.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_WORKING_COSTCENTER_ENTRY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_WORKING_COSTCENTER_ENTRY_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Visible = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnGet.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.WorkingCostCenterEntryStorage.Count > 0)
                {
                    if (this.WorkingCostCenterEntryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["FormFlag"]);
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
                    Session.Remove("WorkingCostCenterEntryStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    InitializeControls(this.CurrentFormLoadType);

                    // Clear data storage
                    Session.Remove("WorkingCostCenterEntryStorage");
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
                        GetWorkingCostCenter(this.AutoID);
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
                UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY
            ),
            false);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {

        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            if (this.AutoID > 0)
            {
                GetWorkingCostCenter(this.AutoID);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (this.CurrentFormLoadType == UIHelper.DataLoadTypes.CreateNewRecord)
            {
                #region Reset controls
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = string.Empty;
                this.litPosition.Text = string.Empty;
                this.litCostCenter.Text = string.Empty;
                this.litUpdateUser.Text = string.Empty;
                this.litLastUpdateTime.Text = string.Empty;

                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
                this.cboJobCatalog.Text = string.Empty;
                this.cboJobCatalog.SelectedIndex = -1;
                #endregion

                #region Clear sessions
                this.CurrentFormLoadType = UIHelper.DataLoadTypes.CreateNewRecord;
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentEmployee"] = null;
                #endregion
            }
            else
            {
                GetWorkingCostCenter(this.AutoID);
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
                // Check Employee No.
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                if (empNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSelectedEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoSelectedEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                // Check Working Cost Center
                string costCenter = this.cboCostCenter.Text;
                //if (string.IsNullOrEmpty(costCenter))
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoWorkingCostCenter.ToString();
                //    this.ErrorType = ValidationErrorType.NoWorkingCostCenter;
                //    this.cusValWorkingCC.Validate();
                //    errorCount++;
                //}

                // Check Catalog date range
                DateTime? catalogStartDate = this.dtpStartDate.SelectedDate;
                DateTime? catalogEndDate = this.dtpEndDate.SelectedDate;

                if (catalogStartDate != null &&
                    catalogEndDate != null)
                {
                    if (catalogStartDate > catalogEndDate)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidCatalogDateRange.ToString();
                        this.ErrorType = ValidationErrorType.InvalidCatalogDateRange;
                        this.cusValCatgStartDate.Validate();
                        errorCount++;
                    }
                }
                else
                {
                    if (catalogStartDate == null &&
                        catalogEndDate != null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoCatalogStartDate.ToString();
                        this.ErrorType = ValidationErrorType.NoCatalogStartDate;
                        this.cusValCatgStartDate.Validate();
                        errorCount++;
                    }
                    else if (catalogStartDate != null &&
                        catalogEndDate == null)
                    {
                        catalogEndDate = catalogStartDate;
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
                    List<EmployeeDetail> recordToInsertList = new List<EmployeeDetail>();

                    recordToInsertList.Add(new EmployeeDetail()
                    {
                        EmpNo = empNo,
                        WorkingCostCenter = costCenter,
                        SpecialJobCatg = this.cboJobCatalog.SelectedValue,
                        CatgEffectiveDate = catalogStartDate,
                        CatgEndingDate = catalogEndDate,
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
                    this.CurrentEmployee.WorkingCostCenter = costCenter;
                    this.CurrentEmployee.SpecialJobCatg = this.cboJobCatalog.SelectedValue;
                    this.CurrentEmployee.CatgEffectiveDate = catalogStartDate;
                    this.CurrentEmployee.CatgEndingDate = catalogEndDate;
                    this.CurrentEmployee.LastUpdateUser = string.Format(@"GARMCO\{0}", UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]));
                    this.CurrentEmployee.LastUpdateTime = DateTime.Now;

                    // Initialize collection
                    List<EmployeeDetail> recordToUpdateList = new List<EmployeeDetail>() { this.CurrentEmployee };

                    SaveChanges(saveType, recordToUpdateList);
                    #endregion
                }
                else if (saveType == UIHelper.SaveType.Delete)
                {
                    #region Perform Delete Operation
                    // Initialize collection
                    List<EmployeeDetail> recordToUpdateList = new List<EmployeeDetail>() { this.CurrentEmployee };

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
                else if (this.ErrorType == ValidationErrorType.NoEmployeeNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoWorkingCostCenter)
                {
                    validator.ErrorMessage = "Working Cost Center is mandatory and should not be left blank.";
                    validator.ToolTip = "Working Cost Center is mandatory and should not be left blank.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSelectedEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is mandatory and should not be left blank.";
                    validator.ToolTip = "Employee No. is mandatory and should not be left blank.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoCatalogStartDate)
                {
                    validator.ErrorMessage = "The Catalog Effective Date should not be left blank if the ending date is specified.";
                    validator.ToolTip = "The Catalog Effective Date should not be left blank if the ending date is specified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidCatalogDateRange)
                {
                    validator.ErrorMessage = "The specified catalog date range is ivalid. Please make sure that the catalog effective date is greater than the ending date.";
                    validator.ToolTip = "The specified catalog date range is ivalid. Please make sure that the catalog effective date is greater than the ending date.";
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

        protected void cboJobCatalog_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.cboJobCatalog.SelectedValue))
            {
                this.dtpStartDate.SelectedDate = null;
                this.dtpEndDate.SelectedDate = null;
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

            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboJobCatalog.Text = string.Empty;
            this.cboJobCatalog.SelectedIndex = -1;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
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
            this.JobCatalogList.Clear();

            // Clear sessions
            ViewState["AutoID"] = null;
            ViewState["CurrentFormLoadType"] = null;
            ViewState["CallerForm"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentEmployee"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.WorkingCostCenterEntryStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.WorkingCostCenterEntryStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("AutoID"))
                this.AutoID = UIHelper.ConvertObjectToInt(this.WorkingCostCenterEntryStorage["AutoID"]);
            else
                this.AutoID = 0;

            // Determine the Form Load Type
            string formLoadType = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["CurrentFormLoadType"]);
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
            if (this.WorkingCostCenterEntryStorage.ContainsKey("CurrentEmployee"))
                this.CurrentEmployee = this.WorkingCostCenterEntryStorage["CurrentEmployee"] as EmployeeDetail;
            else
                this.CurrentEmployee = null;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("JobCatalogList"))
                this.JobCatalogList = this.WorkingCostCenterEntryStorage["JobCatalogList"] as List<UDCEntity>;
            else
                this.JobCatalogList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.WorkingCostCenterEntryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("litUpdateUser"))
                this.litUpdateUser.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["litUpdateUser"]);
            else
                this.litUpdateUser.Text = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("litLastUpdateTime"))
                this.litLastUpdateTime.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["litLastUpdateTime"]);
            else
                this.litLastUpdateTime.Text = string.Empty;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.Text = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.WorkingCostCenterEntryStorage.ContainsKey("cboJobCatalog"))
                this.cboJobCatalog.SelectedValue = UIHelper.ConvertObjectToString(this.WorkingCostCenterEntryStorage["cboJobCatalog"]);
            else
            {
                this.cboJobCatalog.Text = string.Empty;
                this.cboJobCatalog.SelectedIndex = -1;
            }

            if (this.WorkingCostCenterEntryStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.WorkingCostCenterEntryStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.WorkingCostCenterEntryStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.WorkingCostCenterEntryStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;
            #endregion            
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.WorkingCostCenterEntryStorage.Clear();
            this.WorkingCostCenterEntryStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.WorkingCostCenterEntryStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.WorkingCostCenterEntryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.WorkingCostCenterEntryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.WorkingCostCenterEntryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.WorkingCostCenterEntryStorage.Add("litUpdateUser", this.litUpdateUser.Text.Trim());
            this.WorkingCostCenterEntryStorage.Add("litLastUpdateTime", this.litLastUpdateTime.Text.Trim());
            this.WorkingCostCenterEntryStorage.Add("cboCostCenter", this.cboCostCenter.Text);
            this.WorkingCostCenterEntryStorage.Add("cboJobCatalog", this.cboJobCatalog.SelectedValue);
            this.WorkingCostCenterEntryStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.WorkingCostCenterEntryStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.WorkingCostCenterEntryStorage.Add("CallerForm", this.CallerForm);
            this.WorkingCostCenterEntryStorage.Add("AutoID", this.AutoID);
            this.WorkingCostCenterEntryStorage.Add("CurrentFormLoadType", this.CurrentFormLoadType);
            #endregion

            #region Store session data to collection
            this.WorkingCostCenterEntryStorage.Add("CurrentEmployee", this.CurrentEmployee);
            this.WorkingCostCenterEntryStorage.Add("JobCatalogList", this.JobCatalogList);
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
                    this.cboCostCenter.Enabled = true;
                    this.cboJobCatalog.Enabled = true;
                    this.dtpStartDate.Enabled = true;
                    this.dtpEndDate.Enabled = true;

                    // Set the minimum date equals to tomorrow's date
                    //this.dtpStartDate.MinDate = DateTime.Now.AddDays(1);
                    //this.dtpEndDate.MinDate = DateTime.Now.AddDays(1);

                    // Initialize control values
                    this.litEmpName.Text = "Not defined";
                    this.litPosition.Text = "Not defined";
                    this.litCostCenter.Text = "Not defined";
                    this.litUpdateUser.Text = "Not defined";
                    this.litLastUpdateTime.Text = "Not defined";

                    // Setup buttons
                    this.btnGet.Enabled = true;
                    this.btnFindEmployee.Enabled = true;
                    this.btnSave.Enabled = true;
                    this.btnReset.Enabled = true;

                    break;
                    #endregion

                case UIHelper.DataLoadTypes.EditExistingRecord:
                    #region Edit existing training record
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.cboCostCenter.Enabled = true;
                    this.cboJobCatalog.Enabled = true;
                    this.dtpStartDate.Enabled = true;
                    this.dtpEndDate.Enabled = true;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = true;
                    this.btnReset.Enabled = true;
                    
                    break;
                    #endregion

                case UIHelper.DataLoadTypes.OpenReadonlyRecord:
                    #region Open existing training record (read-only)
                    // Setup controls 
                    this.txtEmpNo.Enabled = false;
                    this.cboCostCenter.Enabled = false;
                    this.cboJobCatalog.Enabled = false;
                    this.dtpStartDate.Enabled = false;
                    this.dtpEndDate.Enabled = false;

                    // Setup buttons
                    this.btnGet.Enabled = false;
                    this.btnFindEmployee.Enabled = false;
                    this.btnSave.Enabled = false;
                    this.btnReset.Enabled = false;

                    break;
                    #endregion
            }                        
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            FillJobCatalogCombo(reloadFromDB);
        }
        #endregion

        #region Database Access
        private void GetWorkingCostCenter(int autoID)
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

                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
                this.cboJobCatalog.Text = string.Empty;
                this.cboJobCatalog.SelectedIndex = -1;
                this.dtpStartDate.SelectedDate = null;
                this.dtpEndDate.SelectedDate = null;
                #endregion

                if (Session["SelectedShiftPatternChange"] != null)
                {
                    this.CurrentEmployee = Session["SelectedEmployee"] as EmployeeDetail;
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
                    var rawData = proxy.GetWorkingCostCenter(autoID, 0, string.Empty, string.Empty, ref error, ref innerError);
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
                            this.CurrentEmployee = rawData.FirstOrDefault();
                        }
                    }
                    #endregion
                }

                #region Bind data to controls
                if (this.CurrentEmployee != null)
                {
                    this.txtEmpNo.Value =  this.CurrentEmployee.EmpNo;
                    this.litEmpName.Text = this.CurrentEmployee.EmpName;
                    this.litPosition.Text = this.CurrentEmployee.Position;
                    this.litCostCenter.Text = this.CurrentEmployee.CostCenterFullName;
                    this.litUpdateUser.Text = this.CurrentEmployee.LastUpdateUser;
                    this.litLastUpdateTime.Text = this.CurrentEmployee.LastUpdateTime.HasValue
                        ? Convert.ToDateTime(this.CurrentEmployee.LastUpdateTime).ToString("dd-MMM-yyyy HH:mm:ss")
                        : string.Empty;

                    this.cboCostCenter.Text = this.CurrentEmployee.WorkingCostCenter;
                    this.cboJobCatalog.SelectedValue = this.CurrentEmployee.SpecialJobCatg;
                    this.dtpStartDate.SelectedDate = this.CurrentEmployee.CatgEffectiveDate;
                    this.dtpEndDate.SelectedDate = this.CurrentEmployee.CatgEndingDate;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveChanges(UIHelper.SaveType saveType, List<EmployeeDetail> empDetailList)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                #region Save data to database
                // Get WCF Instance
                if (empDetailList == null)
                    return;

                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteWorkingCostCenter(Convert.ToInt32(saveType), empDetailList, ref error, ref innerError);
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
                        String.Format(UIHelper.PAGE_WORKING_COSTCENTER_INQ + "?{0}={1}&{2}={3}",
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
                this.CurrentEmployee = null;
                throw new Exception(ex.Message.ToString());
            }
        }

        private void FillJobCatalogCombo(bool reloadFromDB)
        {
            try
            {
                List<UDCEntity> comboSource = new List<UDCEntity>();

                if (this.JobCatalogList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.JobCatalogList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetUDCListItem(UIHelper.CONST_SPECIAL_JOB_CATALOG, ref error, ref innerError);
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

                            #region Add blank item
                            comboSource.Insert(0, new UDCEntity()
                            {
                                UDCKey = UIHelper.CONST_SPECIAL_JOB_CATALOG,
                                Code = string.Empty,
                                Description = string.Empty
                            });
                            #endregion
                        }
                    }
                }

                // Store to session
                this.JobCatalogList = comboSource;

                #region Bind data to combobox
                this.cboJobCatalog.DataSource = comboSource;
                this.cboJobCatalog.DataTextField = "Description";
                this.cboJobCatalog.DataValueField = "Code";
                this.cboJobCatalog.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
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
            #region No filtering for cost center
            foreach (DataRow rw in source)
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

            //Set the flag
            enableEmpSearch = true;
            #endregion
            //}

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();
            }
        }
        #endregion               
    }
}