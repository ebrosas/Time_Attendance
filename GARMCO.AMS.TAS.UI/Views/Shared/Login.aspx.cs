using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class Login : Page, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoUsername,
            NoPassword,
            NoEmployeeNo,
            EmployeeNoNotNumeric,
            InvalidEmployeeNo,
            InvalidCredentials
        }

        private enum LoginOptionValue
        {
            valUsername,
            valEmployeeNo
        }
        #endregion

        #region Properties
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

        private Dictionary<string, object> LoginStorage
        {
            get
            {
                Dictionary<string, object> list = Session["LoginStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["LoginStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["LoginStorage"] = value;
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
        #endregion

        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            //base.IsRetrieveUserInfo = true;
            base.OnInit(e);

            if (!this.IsPostBack)
            {
                //if (this.Master.IsSessionExpired)
                    //Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);

                this.Master.SetPageForm(UIHelper.FormAccessCodes.SYSLOGIN.ToString());
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
                this.Master.FormTitle = UIHelper.PAGE_SYSTEM_LOGIN_TITLE;
                #endregion

                #region Check if user has permission to access the page
                //if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                //{
                //    Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_LOGIN_PAGE_TITLE), true);
                //}
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnSave.Visible = this.Master.IsEditAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnLogin.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.LoginStorage.Count > 0)
                {
                    if (this.LoginStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.LoginStorage["FormFlag"]);
                }
                #endregion

                ClearForm();
                ProcessQueryString();

                // Show/hide Cancel button
                this.btnBack.Visible = !string.IsNullOrEmpty(this.CallerForm);
            }

            AddControlsAttribute();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
        }
        #endregion

        #region Action Buttons
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                int errorCount = 0;
                EmployeeWebService.EmployeeInfo empInfo = null;
                string adUserID = string.Empty;

                // Determine the homepage to use
                string homePage = UIHelper.PAGE_HOME;

                if (this.rblLoginOption.SelectedValue == LoginOptionValue.valUsername.ToString())
                {
                    #region Login using the Username

                    #region Check the Username
                    if (this.txtUsername.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoUsername.ToString();
                        this.ErrorType = ValidationErrorType.NoUsername;
                        this.cusValUser.Validate();
                        errorCount++;
                    }
                    #endregion

                    #region Check Password
                    if (this.txtPassword.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoPassword.ToString();
                        this.ErrorType = ValidationErrorType.NoPassword;
                        this.cusValPwd.Validate();
                        errorCount++;
                    }
                    #endregion

                    if (errorCount == 0)
                    {
                        empInfo = UIHelper.ValidateEmployeeInfo(0, this.txtUsername.Text.Trim(),
                            this.txtPassword.Text.Trim(), UIHelper.UserLoginOption.LoginByUsername);
                    }
                    else
                        return;
                    #endregion
                }
                else
                {
                    #region Login using the Employee No.

                    #region Check Employee No.
                    if (this.txtUsername.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoEmployeeNo.ToString();
                        this.ErrorType = ValidationErrorType.NoEmployeeNo;
                        this.cusValUser.Validate();
                        errorCount++;
                    }
                    else if (UIHelper.ConvertObjectToInt(this.txtUsername.Text) == 0)
                    {
                        this.txtGeneric.Text = ValidationErrorType.EmployeeNoNotNumeric.ToString();
                        this.ErrorType = ValidationErrorType.EmployeeNoNotNumeric;
                        this.cusValUser.Validate();
                        errorCount++;
                    }
                    else
                    {
                        #region Check if Employee is valid
                        //if (UIHelper.ConvertObjectToInt(this.txtUsername.Text) > 0)
                        //{
                        //    EmployeeDetail empDetail = UIHelper.GetEmployeeInfoAdvanced(UIHelper.ConvertObjectToInt(this.txtUsername.Text), 
                        //        string.Empty, UIHelper.EmployeeInfoSearchType.SearchByEmpNo);
                        //    if (empDetail == null)
                        //    {
                        //        this.txtGeneric.Text = ValidationErrorType.InvalidEmployeeNo.ToString();
                        //        this.ErrorType = ValidationErrorType.InvalidEmployeeNo;
                        //        this.cusValUser.Validate();
                        //        errorCount++;
                        //    }
                        //}
                        #endregion
                    }
                    #endregion

                    #region Check Password
                    if (this.txtPassword.Text == string.Empty)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoPassword.ToString();
                        this.ErrorType = ValidationErrorType.NoPassword;
                        this.cusValPwd.Validate();
                        errorCount++;
                    }
                    #endregion

                    if (errorCount == 0)
                    {
                        int empNo = UIHelper.ConvertObjectToInt(this.txtUsername.Text);
                        if (this.txtUsername.Text.Length == 4)
                        {
                            empNo = UIHelper.ConvertObjectToInt(this.txtUsername.Text) + 10000000;
                            this.txtUsername.Text = empNo.ToString();
                        }
                        empInfo = UIHelper.ValidateEmployeeInfo(empNo, string.Empty, this.txtPassword.Text.Trim(), UIHelper.UserLoginOption.LoginByEmpNo);
                    }
                    else
                        return;
                    #endregion
                }

                if (empInfo != null)
                {
                    // Get the Windows User ID
                    adUserID = empInfo.Username;

                    EmployeeDetail empDetail = UIHelper.GetEmployeeInfoAdvanced(UIHelper.ConvertObjectToInt(empInfo.EmployeeNo),
                        string.Empty, UIHelper.EmployeeInfoSearchType.SearchByEmpNo);

                    if (empDetail != null)
                    {
                        #region Set the session variables
                        Session[UIHelper.GARMCO_USERID] = empDetail.EmpNo;
                        Session[UIHelper.GARMCO_USERNAME] = string.IsNullOrEmpty(empDetail.EmpUserID) ? adUserID : empDetail.EmpUserID;
                        Session[UIHelper.GARMCO_FULLNAME] = empDetail.EmpName;
                        Session[UIHelper.GARMCO_USER_COST_CENTER] = empDetail.CostCenter;
                        Session[UIHelper.GARMCO_USER_COST_CENTER_NAME] = empDetail.CostCenterName;
                        Session[UIHelper.GARMCO_USER_EMAIL] = empDetail.EmpEmail;
                        Session[UIHelper.GARMCO_USER_EXT] = empDetail.PhoneExtension;
                        Session[UIHelper.GARMCO_USER_GENDER] = empDetail.Gender;
                        Session[UIHelper.GARMCO_USER_DESTINATION] = empDetail.Destination;
                        Session[UIHelper.GARMCO_USER_PAY_GRADE] = empDetail.PayGrade;
                        Session[UIHelper.GARMCO_USER_POSITION_ID] = empDetail.PositionID;
                        Session[UIHelper.GARMCO_USER_POSITION_DESC] = empDetail.Position;
                        Session[UIHelper.GARMCO_USER_EMP_CLASS] = empDetail.EmployeeClass;
                        Session[UIHelper.GARMCO_USER_TICKET_CLASS] = empDetail.TicketClass;
                        Session[UIHelper.GARMCO_USER_SUPERVISOR_NO] = empDetail.SupervisorEmpNo;
                        Session[UIHelper.GARMCO_USER_SUPERVISOR_NAME] = empDetail.SupervisorEmpName;
                        #endregion
                    }
                    else
                    {
                        #region Set the session variables
                        Session[UIHelper.GARMCO_USERID] = empInfo.EmployeeNo;
                        Session[UIHelper.GARMCO_USERNAME] = string.IsNullOrEmpty(empInfo.Username) ? adUserID : empInfo.Username;
                        Session[UIHelper.GARMCO_FULLNAME] = empInfo.FullName;
                        Session[UIHelper.GARMCO_USER_COST_CENTER] = empInfo.CostCenter;
                        Session[UIHelper.GARMCO_USER_COST_CENTER_NAME] = empInfo.CostCenterName;
                        Session[UIHelper.GARMCO_USER_EMAIL] = empInfo.Email;
                        Session[UIHelper.GARMCO_USER_EXT] = empInfo.ExtensionNo;
                        Session[UIHelper.GARMCO_USER_GENDER] = empInfo.Gender;
                        Session[UIHelper.GARMCO_USER_DESTINATION] = empInfo.Destination;
                        Session[UIHelper.GARMCO_USER_PAY_GRADE] = empInfo.PayGrade.ToString();
                        Session[UIHelper.GARMCO_USER_POSITION_ID] = empInfo.PositionID;
                        Session[UIHelper.GARMCO_USER_POSITION_DESC] = empInfo.PositionDesc;
                        Session[UIHelper.GARMCO_USER_EMP_CLASS] = empInfo.EmployeeClass;
                        Session[UIHelper.GARMCO_USER_TICKET_CLASS] = empInfo.TicketClass;
                        Session[UIHelper.GARMCO_USER_SUPERVISOR_NO] = empInfo.SupervisorEmpNo.ToString();
                        Session[UIHelper.GARMCO_USER_SUPERVISOR_NAME] = empInfo.SupervisorEmpName;
                        #endregion
                    }

                    if (UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]) != string.Empty)
                    {
                        #region Check if current user is member of the Administartors group (Retrieve info from web.config)
                        bool isAdmin = false;
                        try
                        {
                            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SystemAdministrators"]))
                            {
                                string[] userID = ConfigurationManager.AppSettings["SystemAdministrators"].Split(',');
                                if (userID != null && userID.Count() > 0)
                                {
                                    List<string> adminUsers = new List<string>();
                                    adminUsers.AddRange(userID.ToList());
                                    if (adminUsers.Where(a => a.ToUpper().Trim() == UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]).ToUpper().Trim()).FirstOrDefault() != null)
                                    {
                                        isAdmin = true;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            Session[UIHelper.GARMCO_USER_IS_ADMIN] = isAdmin;
                        }
                        #endregion

                        #region Check if current user is member of the SpecialUsers group (Retrieve info from web.config)
                        bool isSpecialUser = false;

                        try
                        {
                            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SpecialUsers"]))
                            {
                                string[] userID = ConfigurationManager.AppSettings["SpecialUsers"].Split(',');
                                if (userID != null && userID.Count() > 0)
                                {
                                    List<string> adminUsers = new List<string>();
                                    adminUsers.AddRange(userID.ToList());
                                    if (adminUsers.Where(a => a.ToUpper().Trim() == UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]).ToUpper().Trim()).FirstOrDefault() != null)
                                    {
                                        isSpecialUser = true;

                                        switch (empDetail.EmpUserID.Trim())
                                        {
                                            case "gatews1":
                                            case "gatews2":
                                                Session[UIHelper.GARMCO_FULLNAME] = "Security User";
                                                break;

                                                //case "jdepe_wt":
                                                //    Session[UIHelper.GARMCO_FULLNAME] = "Water Treatment Terminal";
                                                //    break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            Session[UIHelper.GARMCO_USER_IS_SPECIAL] = isSpecialUser;
                        }
                        #endregion
                    }

                    switch (UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]).ToUpper())
                    {
                        case "GATEWS1":
                        case "GATEWS2":
                            //Response.Redirect(UIHelper.PAGE_SERVICE_ASSIGNED_INQUIRY, false);
                            break;

                        default:
                            Response.Redirect(homePage, false);
                            break;
                    }
                }
                else
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidCredentials.ToString();
                    this.ErrorType = ValidationErrorType.InvalidCredentials;
                    this.cusValButtons.Validate();
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            #region Determine the homepage to use
            string homePage = UIHelper.PAGE_HOME;
            //string homePage = UIHelper.PAGE_EMPLOYEE_SWIPES_HISTORY;
            //try
            //{
            //    List<string> costCenterList = Session[UIHelper.CONST_WORKPLACE_COST_CENTER] as List<string>;
            //    if (costCenterList.Count > 0)
            //    {
            //        if (costCenterList.Where(a => a == UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER])).FirstOrDefault() != null)
            //        {
            //            homePage = UIHelper.PAGE_WORKPLACE_ATTENDANCE_DASHBOARD;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //}
            #endregion

            // Clear storage session
            this.LoginStorage = null;

            switch (UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]).ToUpper())
            {
                case "GATEWS1":
                case "GATEWS2":
                    Response.Redirect(homePage, false);
                    break;

                default:
                    Response.Redirect(homePage, false);
                    break;
            }

            //if (this.CallerForm != string.Empty)
            //    Response.Redirect(this.CallerForm, false);
        }
        #endregion

        #region Page Control Events
        protected void cusGeneric_ServerValidate(object source, ServerValidateEventArgs args)
        {
            #region Display the validation Error
            CustomValidator validator = source as CustomValidator;

            try
            {
                if (this.ErrorType == ValidationErrorType.CustomFormError)
                {
                    validator.ErrorMessage = this.CustomErrorMsg != string.Empty ? this.CustomErrorMsg : "Unhandled Error Occured";
                    validator.ToolTip = this.CustomErrorMsg != string.Empty ? this.CustomErrorMsg : "Unhandled Error Occured";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoUsername)
                {
                    validator.ErrorMessage = "Username is mandatory";
                    validator.ToolTip = "Username is mandatory";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoPassword)
                {
                    validator.ErrorMessage = "Password is mandatory";
                    validator.ToolTip = "Password is mandatory";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmployeeNo)
                {
                    validator.ErrorMessage = "Employee No. is mandatory";
                    validator.ToolTip = "Employee No. is mandatory";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidEmployeeNo)
                {
                    validator.ErrorMessage = "The specified Employee No. is invalid!";
                    validator.ToolTip = "The specified Employee No. is invalid!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.EmployeeNoNotNumeric)
                {
                    validator.ErrorMessage = "Employee No. must be numeric!";
                    validator.ToolTip = "Employee No. must be numeric!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidCredentials)
                {
                    if (this.rblLoginOption.SelectedValue == LoginOptionValue.valUsername.ToString())
                    {
                        validator.ErrorMessage = @"Sorry, the supplied login information is invalid. Please check whether the Username and Password are correct.";
                        validator.ErrorMessage = @"Sorry, the supplied login information is invalid. Please check whether the Username and Password are correct.";
                    }
                    else
                    {
                        validator.ErrorMessage = @"Sorry, the supplied login information is invalid. Please check whether the Employee No. and Password are correct.";
                        validator.ErrorMessage = @"Sorry, the supplied login information is invalid. Please check whether the Employee No. and Password are correct.";
                    }
                    args.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
            finally
            {
                this.txtGeneric.Text = string.Empty;
                this.ErrorType = ValidationErrorType.NoError;
            }
            #endregion
        }

        protected void rblLoginOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rblLoginOption.SelectedValue == LoginOptionValue.valUsername.ToString())
            {
                this.tdUsername.InnerText = "Username";
                this.txtUsername.EmptyMessage = "Enter Username (Ex. ervin)";
                this.txtUsername.MaxLength = 50;
            }
            else
            {
                this.tdUsername.InnerText = "Employee No.";
                this.txtUsername.EmptyMessage = "Enter Employee No. (Ex. 10001234)";
                this.txtUsername.MaxLength = 8;
            }

            this.txtUsername.Text = this.txtPassword.Text = string.Empty;
            this.txtUsername.Focus();
        }
        #endregion

        #region Private Methods
        private void DisplayFormLevelError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return;

            this.CustomErrorMsg = errorMsg;
            this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
            this.ErrorType = ValidationErrorType.CustomFormError;
            this.cusValButtons.Validate();
        }
        #endregion

        #region Interface Implementation
        public void ClearForm()
        {
            #region Clear controls
            this.txtPassword.Text = string.Empty;
            this.txtUsername.Text = string.Empty;

            this.rblLoginOption.SelectedValue = LoginOptionValue.valUsername.ToString();
            this.rblLoginOption_SelectedIndexChanged(this.rblLoginOption, new EventArgs());
            #endregion

            KillSessions();
        }

        public void AddControlsAttribute()
        {
        }

        public void SetButtonsVisibility()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
        }

        public void KillSessions()
        {
            ViewState["ErrorType"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CallerForm"] = null;
        }

        public void FillComboData()
        {

        }
        #endregion
    }
}