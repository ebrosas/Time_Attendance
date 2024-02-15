using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.Common.DAL.WebCommonSetup;
using GARMCO.Common.Object;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class ReassignmentForm : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            EmployeeNotExist,
            InvalidReasonLenght,
            CannotReassignToSameApprover,
            NoJustification,
            NoSelectedRequisitions
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

        private Dictionary<string, object> ReassignStorageList
        {
            get
            {
                Dictionary<string, object> list = Session["ReassignStorageList"] as Dictionary<string, object>;
                if (list == null)
                    Session["ReassignStorageList"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ReassignStorageList"] = value;
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

        private List<EmployeeAttendanceEntity> OTRequestReassignList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = Session["OTRequestReassignList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    Session["OTRequestReassignList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
        }

        private int CurrentApproverEmpNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentApproverEmpNo"]);
            }
            set
            {
                ViewState["CurrentApproverEmpNo"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.REASIGNFRM.ToString());

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

                if (!string.IsNullOrEmpty(position))
                {
                    sb.Append(string.Format("Position: {0} <br />", position));
                }
                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Concat("Welcome, ",
                    UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                    UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));
                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_APPROVAL_REASSIGNMENT_FORM_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_APPROVAL_REASSIGNMENT_FORM_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnCreate.Visible = this.Master.IsCreateAllowed;
                //this.btnCancel.Visible = this.Master.IsEditAllowed;
                //this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                //this.btnPrint.Visible = this.Master.IsPrintAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnReassign.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ReassignStorageList.Count > 0)
                {
                    if (this.ReassignStorageList.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ReassignStorageList["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get User's Info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtReassignEmpNo.Text = string.Format("({0}) {1}",
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]),
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]));
                    }

                    // Clear storage collection
                    this.ReassignStorageList.Clear();
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Action Buttons
        protected void btnBack_Click(object sender, EventArgs e)
        {
            //Set search flag status
            Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG] = 0;

            // Clear collections
            this.ReassignStorageList.Clear();
            this.OTRequestReassignList.Clear();

            if (this.CallerForm != string.Empty)
                Response.Redirect(this.CallerForm, false);
            else
                Response.Redirect(UIHelper.PAGE_OVERTIME_APPROVAL, false);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void btnReassign_Click(object sender, EventArgs e)
        {
            try
            {
                #region Initialize variables
                int errorCount = 0;
                string assigneeEmpName = string.Empty;
                string assigneeEmpEmail = string.Empty;
                string assigneeUserID = string.Empty;                
                string reassignReason = this.txtReassignReason.Text.Trim();

                int assigneeEmpNo = UIHelper.ConvertObjectToInt(this.txtReassignEmpNo.Text);
                if (assigneeEmpNo.ToString().Length == 4)
                {
                    assigneeEmpNo += 10000000;

                    // Display Emp. No.
                    this.txtReassignEmpNo.Text = assigneeEmpNo.ToString();
                }
                else
                {
                    assigneeEmpNo = GetEmployeeNumber(this.txtReassignEmpNo.Text, ref assigneeEmpName);
                }
                #endregion

                #region Perform Validation
                if (assigneeEmpNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.EmployeeNotExist.ToString();
                    this.ErrorType = ValidationErrorType.EmployeeNotExist;
                    this.cusReassignEmpNo.Validate();
                    errorCount++;
                }
                else
                {
                    if (assigneeEmpName == string.Empty || 
                        assigneeEmpEmail == string.Empty)
                    {
                        EmployeeDetail empInfo = UIHelper.GetEmployeeEmailInfo(assigneeEmpNo);
                        if (empInfo != null)
                        {
                            assigneeEmpName = empInfo.EmpName;
                            assigneeEmpEmail = empInfo.EmpEmail;
                            assigneeUserID = empInfo.EmpUserID;
                        }
                    }
                }

                // Check if Employee is valid
                if (assigneeEmpName == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.EmployeeNotExist.ToString();
                    this.ErrorType = ValidationErrorType.EmployeeNotExist;
                    this.cusReassignEmpNo.Validate();
                    errorCount++;
                }                               

                // Check Justification
                if (this.txtReassignReason.Text == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoJustification.ToString();
                    this.ErrorType = ValidationErrorType.NoJustification;
                    this.cusReason.Validate();
                    errorCount++;
                }

                // Check the selected requisitions
                if (this.OTRequestReassignList.Count == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSelectedRequisitions.ToString();
                    this.ErrorType = ValidationErrorType.NoSelectedRequisitions;
                    this.cusValButton.Validate();
                    errorCount++;
                }
                else
                {
                    // Check for records with the same approver 
                    List<EmployeeAttendanceEntity> otWithSameApproverList = this.OTRequestReassignList
                        .Where(a => a.CurrentlyAssignedEmpNo == assigneeEmpNo)
                        .ToList();
                    if (otWithSameApproverList != null &&
                        otWithSameApproverList.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (EmployeeAttendanceEntity item in otWithSameApproverList)
                        {
                            if (sb.Length == 0)
                                sb.Append(item.EmpNo.ToString());
                            else
                                sb.Append(string.Concat(", ", item.EmpNo.ToString()));
                        }

                        DisplayFormLevelError(string.Format("Cannot reassign requisition to the same approver for the following employees: {0}", sb.ToString().Trim()));
                        errorCount++;
                    }
                }

                if (errorCount > 0)
                    return;
                #endregion

                // Process the reassignment
                ProcessReassignToOtherAction(assigneeEmpNo, assigneeEmpName, assigneeEmpEmail, reassignReason, this.OTRequestReassignList);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnReassignEmpNo_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_REASSIGNMENT_FORM
            ),
            false);
        }
        #endregion

        #region Page Control Events
        protected void tooltipMan_AjaxUpdate(object sender, Telerik.Web.UI.ToolTipUpdateEventArgs e)
        {
            e.UpdatePanel.ContentTemplateContainer.Controls.Add(new LiteralControl(e.Value));
        }

        protected void cusGenericValidator_ServerValidate(object source, ServerValidateEventArgs args)
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
                else if (this.ErrorType == ValidationErrorType.NoSelectedRequisitions)
                {
                    validator.ErrorMessage = "No overtime requisitions have been selected for reassignment.";
                    validator.ToolTip = "No overtime requisitions have been selected for reassignment.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.EmployeeNotExist)
                {
                    validator.ErrorMessage = "The specified Employee No. does not exists.";
                    validator.ToolTip = "The specified Employee No. does not exists.";
                    args.IsValid = false;
                    this.hdnErrorFlag.Value = "1";
                }
                else if (this.ErrorType == ValidationErrorType.NoJustification)
                {
                    int textLenght = this.txtReassignReason.Text.Trim().Length;
                    validator.ErrorMessage = "Justification cannot be empty";
                    validator.ToolTip = "Justification cannot be empty";
                    args.IsValid = false;
                    this.hdnErrorFlag.Value = "1";
                }
                else if (this.ErrorType == ValidationErrorType.CannotReassignToSameApprover)
                {
                    validator.ErrorMessage = "Cannot reassign requisition to the same approver.";
                    validator.ToolTip = "Cannot reassign requisition to the same approver.";
                    args.IsValid = false;
                    this.hdnErrorFlag.Value = "1";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
            finally
            {
                this.txtGeneric.Text = string.Empty;
                this.ErrorType = ValidationErrorType.NoError;
            }
            #endregion
        }
        #endregion

        #region Private Methods       
        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ReassignStorageList.Clear();
            this.ReassignStorageList.Add("FormFlag", formFlag.ToString());

            #region Save control values 
            this.ReassignStorageList.Add("txtReassignEmpNo", this.txtReassignEmpNo.Text.Trim());
            this.ReassignStorageList.Add("ReassignReason", this.txtReassignReason.Text.Trim());
            this.ReassignStorageList.Add("ReassignToMe", this.chkReassignSendBack.Checked);
            #endregion

            #region Save session values
            this.ReassignStorageList.Add("CallerForm", this.CallerForm);
            this.ReassignStorageList.Add("CurrentApproverEmpNo", this.CurrentApproverEmpNo);
            #endregion
        }

        private void RestoreDataFromCollection()
        {
            if (this.ReassignStorageList.Count == 0)
                return;

            #region Restore session values
            if (this.ReassignStorageList.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ReassignStorageList["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.ReassignStorageList.ContainsKey("CurrentApproverEmpNo"))
                this.CurrentApproverEmpNo = UIHelper.ConvertObjectToInt(this.ReassignStorageList["CurrentApproverEmpNo"]);
            else
                this.CurrentApproverEmpNo = 0;
            #endregion

            #region Restore control values
            if (this.ReassignStorageList.ContainsKey("txtReassignEmpNo"))
                this.txtReassignEmpNo.Text = UIHelper.ConvertObjectToString(this.ReassignStorageList["txtReassignEmpNo"]);
            else
                this.txtReassignEmpNo.Text = string.Empty;

            if (this.ReassignStorageList.ContainsKey("txtReassignReason"))
                this.txtReassignReason.Text = UIHelper.ConvertObjectToString(this.ReassignStorageList["txtReassignReason"]);
            else
                this.txtReassignReason.Text = string.Empty;

            if (this.ReassignStorageList.ContainsKey("chkReassignSendBack"))
                this.chkReassignSendBack.Checked = UIHelper.ConvertObjectToBolean(this.ReassignStorageList["chkReassignSendBack"]);
            else
                this.chkReassignSendBack.Checked = false;
            #endregion
        }

        private int GetEmployeeNumber(string employeeFullName, ref string employeeName)
        {
            if (string.IsNullOrEmpty(employeeFullName))
                return 0;

            try
            {
                int result = 0;
                int idx = employeeFullName.LastIndexOf(")");
                if (idx > 0)
                {
                    result = UIHelper.ConvertObjectToInt(employeeFullName.Substring(1, idx - 1));
                    employeeName = employeeFullName.Substring(idx + 1);
                }
                else
                {
                    int num;
                    if (int.TryParse(employeeFullName, out num))
                        result = num;
                }
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
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

        private void ProcessReassignToOtherAction(int assigneeEmpNo, string assigneeEmpName, string assigneeEmpEmail, string reassignReason, List<EmployeeAttendanceEntity> selectedRequisitionList)
        {
            try
            {
                if (selectedRequisitionList.Count == 0)
                    return;

                #region Initialize variables                                
                string error = string.Empty;
                string innerError = string.Empty;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string userEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                string userEmailAddress = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EMAIL]);
                string otStartTime = string.Empty;
                string otEndTime = string.Empty;
                string otType = string.Empty;
                string isOTApproved = string.Empty;
                string isMealVoucherApproved = string.Empty;
                DALProxy proxy = new DALProxy();
                #endregion

                #region Update the database
                int emailCounter = 0;
                foreach (EmployeeAttendanceEntity item in selectedRequisitionList)
                {                    
                    DatabaseSaveResult workflowResult = proxy.ProcessOvertimeWorflow(Convert.ToByte(UIHelper.WorkflowActionTypes.ReassignToOtherApprover),
                        item.OTRequestNo, item.AutoID, userID, userEmpNo, userEmpName, assigneeEmpNo, assigneeEmpName, null, reassignReason, 
                        item.RequestSubmissionDate, null, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) ||
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }

                    // Increment the counter
                    emailCounter++;
                }
                #endregion

                #region Send email to the reassigned approver
                StringBuilder sb = new StringBuilder();
                string emailBody = string.Empty;
                int counter = 1;

                #region Build the email contents
                foreach (EmployeeAttendanceEntity item in selectedRequisitionList)
                {
                    if (item.OTApprovalCode == "Y") // OT is Approved
                    {
                        isOTApproved = "Yes";
                        otStartTime = UIHelper.ConvertObjectToTimeString(item.OTStartTime);
                        otEndTime = UIHelper.ConvertObjectToTimeString(item.OTEndTime);
                        otType = item.OTType;
                    }
                    else if (item.OTApprovalCode == "N")    // OT is rejected
                    {
                        isOTApproved = "No";
                        otStartTime = @"N/A";
                        otEndTime = @"N/A";
                        otType = @"N/A";
                    }
                    else
                    {
                        isOTApproved = "No action";
                        otStartTime = "-";
                        otEndTime = "-";
                        otType = "-";
                    }

                    if (item.MealVoucherEligibility == "YA")
                        isMealVoucherApproved = "Yes";
                    else if (item.MealVoucherEligibility == "N")
                        isMealVoucherApproved = "No";
                    else
                        isMealVoucherApproved = "No action";

                    if (sb.Length > 0)
                        sb.AppendLine(@"<br />");

                    sb.AppendLine(string.Format(@"{0}. <b>Requisition No.:</b> {1}; " +
                                                    "<b>Employee Name:</b> {2}; " +
                                                    "<b>Position:</b> {3}; " +
                                                    "<b>Cost Center:</b> {4}; " +
                                                    "<b>Pay Grade:</b> {5}; " +
                                                    "<b>Shift Pat.:</b> {6}; " +
                                                    "<b>Sched. Shift:</b> {7}; " +
                                                    "<b>Actual Shift:</b> {8}; " +
                                                    "<b>Date:</b> {9}; " +
                                                    "<b>Meal Voucher Approved:</b> {10}; " +
                                                    "<b>OT Approved:</b> {11}; " +
                                                    "<b>OT Start Time:</b>" + "<font color=" + "red" + ">" + " {12}</font>; " +
                                                    "<b>OT End Time:</b>" + "<font color=" + "red" + ">" + " {13}</font>; " +
                                                    //"<b>OT Type:</b> {14}",
                                                    "<b>Remarks:</b> {14}",
                                                counter,
                                                item.OTRequestNo,
                                                !string.IsNullOrEmpty(item.EmpFullName) ? item.EmpFullName : "Not defined",
                                                !string.IsNullOrEmpty(item.Position) ? item.Position : "Not defined",
                                                !string.IsNullOrEmpty(item.CostCenterFullName) ? item.CostCenterFullName : "Not defined",
                                                item.PayGrade,
                                                !string.IsNullOrEmpty(item.ShiftPatCode) ? item.ShiftPatCode : "Not defined",
                                                !string.IsNullOrEmpty(item.ShiftCode) ? item.ShiftCode : "Not defined",
                                                !string.IsNullOrEmpty(item.ActualShiftCode) ? item.ShiftCode : "Not defined",
                                                UIHelper.ConvertObjectToDateString(item.DT),
                                                isMealVoucherApproved,
                                                isOTApproved,
                                                otStartTime,
                                                otEndTime,
                                                //otType,
                                                item.AttendanceRemarks));

                    
                    sb.AppendLine(@"<br />");

                    // Increment the counter
                    counter++;
                }

                if (sb.Length > 0)
                    emailBody = sb.ToString().Trim();
                #endregion

                if (SendEmailToApprover(userEmpNo, userEmpName, userEmailAddress, assigneeEmpNo, assigneeEmpName, assigneeEmpEmail, emailBody, reassignReason))
                {
                    // Go back to previous page
                    this.btnBack_Click(this.btnBack, new EventArgs());
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Email Communications
        private bool SendEmailToApprover(int currentApproverNo, string currentApproverName, string currentApproverEmail, int reassignApproverNo,
            string reassignApproverName, string reassignApproverEmail, string emailBody, string reassignReason)
        {
            try
            {
                try
                {
                    #region Perform Validation
                    //Check mail server
                    string mailServer = ConfigurationManager.AppSettings["MailServer"];
                    if (string.IsNullOrEmpty(mailServer))
                        return false;
                    #endregion

                    #region Initialize variables
                    int retError = 0;
                    string errorMsg = string.Empty;
                    string error = string.Empty;
                    string innerError = string.Empty;
                    string recipientEmail = string.Empty;
                    string recipientName = string.Empty;
                    EmployeeDetail empInfo = new EmployeeDetail();
                    string distListCode = string.Empty;
                    #endregion

                    #region Set the From, Subject, and primary recipients
                    string adminAlias = ConfigurationManager.AppSettings["AdminEmailAlias"];
                    MailAddress from = new MailAddress(ConfigurationManager.AppSettings["AdminEmail"], !string.IsNullOrEmpty(adminAlias) ? adminAlias : "TAS Admin");
                    string subject = "TAS - Overtime Online Approval";
                    #endregion

                    #region Set the Mail Recipients
                    List<MailAddress> toList = null;
                    List<MailAddress> ccList = null;
                    List<MailAddress> bccList = null;

                    #region Set the To recipients
                    if (!string.IsNullOrEmpty(reassignApproverEmail) &&
                        !string.IsNullOrEmpty(reassignApproverName))
                    {
                        recipientName = UIHelper.ConvertStringToTitleCase(reassignApproverName);
                        recipientEmail = reassignApproverEmail;
                    }
                    else
                    {
                        if (reassignApproverNo > 0)
                        {
                            empInfo = UIHelper.GetEmployeeEmailInfo(reassignApproverNo);
                            if (empInfo != null &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                recipientEmail = UIHelper.ConvertObjectToString(empInfo.EmpEmail);
                                recipientName = UIHelper.ConvertStringToTitleCase(empInfo.EmpName);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(recipientEmail)
                        && !string.IsNullOrEmpty(recipientName))
                    {
                        toList = new List<MailAddress>();
                        toList.Add(new MailAddress(recipientEmail, recipientName));
                    }
                    #endregion

                    #region Set the Cc Recipients
                    ccList = new List<MailAddress>();

                    if (!string.IsNullOrEmpty(currentApproverEmail) &&
                        !string.IsNullOrEmpty(currentApproverName))
                    {
                        ccList.Add(new MailAddress(currentApproverEmail, UIHelper.ConvertStringToTitleCase(currentApproverName)));
                    }
                    else
                    {
                        if (currentApproverNo > 0)
                        {
                            empInfo = UIHelper.GetEmployeeEmailInfo(currentApproverNo);
                            if (empInfo != null &&
                                !string.IsNullOrEmpty(empInfo.EmpEmail))
                            {
                                ccList.Add(new MailAddress(empInfo.EmpEmail, UIHelper.ConvertStringToTitleCase(empInfo.EmpName)));
                            }
                        }
                    }
                    #endregion

                    #region Set the Bcc recipients (For tracking purpose)
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AdminBCCRecipients"]))
                    {
                        string[] recipients = ConfigurationManager.AppSettings["AdminBCCRecipients"].Split(',');
                        if (recipients != null && recipients.Count() > 0)
                        {
                            bccList = new List<MailAddress>();
                            foreach (string recipient in recipients)
                            {
                                if (recipient.Length > 0)
                                    bccList.Add(new MailAddress(recipient, recipient));
                            }
                        }
                    }
                    #endregion

                    #endregion

                    // Exit if Mail-to recipient is null
                    if (toList == null || toList.Count == 0)
                        return false;

                    #region Build URL address
                    string dynamicEndpointAddress = string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url),
                        UIHelper.PAGE_OVERTIME_APPROVAL.Replace("~", string.Empty));

                    string queryString = string.Format("?IsAssignedKey={0}", true.ToString());

                    StringBuilder url = new StringBuilder();
                    url.Append(string.Concat(dynamicEndpointAddress, queryString.Trim()));
                    #endregion

                    #region Set Message Body
                    string body = String.Empty;
                    string htmLBody = string.Empty;
                    string appPath = Server.MapPath(UIHelper.CONST_REASSIGN_EMAIL_TEMPLATE);
                    string adminName = ConfigurationManager.AppSettings["AdminName"];

                    // Build the message body
                    body = String.Format(UIHelper.RetrieveXmlMessage(appPath),
                        recipientName,
                        emailBody,
                        url.ToString().Trim(),
                        adminName,
                        string.Format("<font color=Red>{0}</font>", reassignReason)
                        ).Replace("&lt;", "<").Replace("&gt;", ">");

                    // Format the message contents
                    htmLBody = string.Format("<HTML><BODY><p>{0}</p></BODY></HTML>", body);
                    #endregion

                    #region Create attachment
                    List<Attachment> attachmentList = null;
                    #endregion

                    #region Send the e-mail
                    if (!string.IsNullOrEmpty(htmLBody))
                    {
                        retError = 0;
                        errorMsg = string.Empty;
                        SendEmail(toList, ccList, bccList, from, subject, htmLBody, attachmentList, mailServer, ref errorMsg, ref retError);
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            throw new Exception(errorMsg);
                        }
                    }
                    #endregion

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SendEmail(List<MailAddress> toList, List<MailAddress> ccList, List<MailAddress> bccList, MailAddress from,
            string subject, string body, List<Attachment> attachmentList, string smtpConn, ref string errorMsg, ref int retError)
        {
            errorMsg = String.Empty;
            retError = 0;

            try
            {
                bool isTestMode = UIHelper.ConvertNumberToBolean(ConfigurationManager.AppSettings["EmailTestMode"]);
                int indexLoc = 0;
                string newEmailAddress = string.Empty;

                // Create an email object
                MailMessage email = new MailMessage();

                #region Add all the recipients and originator
                if (toList != null)
                {
                    foreach (MailAddress to in toList)
                    {
                        if (isTestMode)
                        {
                            #region Append underscore to the email address if in test mode
                            if (!string.IsNullOrEmpty(to.Address))
                            {
                                indexLoc = to.Address.IndexOf("@");
                                if (indexLoc > 0)
                                {
                                    newEmailAddress = to.Address.Replace(to.Address.Substring(indexLoc + 1),
                                        string.Concat("_", to.Address.Substring(indexLoc + 1)));

                                    // Add email address
                                    email.To.Add(new MailAddress(newEmailAddress, to.DisplayName));
                                }
                                else
                                    email.To.Add(to);
                            }
                            #endregion
                        }
                        else
                            email.To.Add(to);
                    }
                }

                if (ccList != null)
                {
                    foreach (MailAddress cc in ccList)
                    {
                        if (isTestMode)
                        {
                            #region Append underscore to the email address if in test mode
                            if (!string.IsNullOrEmpty(cc.Address))
                            {
                                indexLoc = cc.Address.IndexOf("@");
                                if (indexLoc > 0)
                                {
                                    newEmailAddress = cc.Address.Replace(cc.Address.Substring(indexLoc + 1),
                                        string.Concat("_", cc.Address.Substring(indexLoc + 1)));

                                    // Add email address
                                    email.CC.Add(new MailAddress(newEmailAddress, cc.DisplayName));
                                }
                                else
                                    email.CC.Add(cc);
                            }
                            #endregion
                        }
                        else
                            email.CC.Add(cc);
                    }
                }

                if (bccList != null)
                {
                    foreach (MailAddress bcc in bccList)
                    {
                        email.Bcc.Add(bcc);
                    }
                }

                email.From = from;
                #endregion

                #region Set the subject and body
                // Deserialize the subject
                RadEditor txtStorage = new RadEditor();
                txtStorage.Content = subject;
                email.Subject = txtStorage.Text.Trim();

                StringBuilder bodyList = new StringBuilder();
                bodyList.Append("<div style='font-family: Tahoma; font-size: 10pt'>");
                bodyList.Append(body);
                bodyList.Append("</div>");
                email.Body = bodyList.ToString();
                email.IsBodyHtml = true;
                #endregion

                #region Add attachments
                if (attachmentList != null)
                {
                    foreach (Attachment attach in attachmentList)
                        email.Attachments.Add(attach);
                }
                #endregion

                // Create an smtp client and send the mail message
                SmtpClient smtpClient = new SmtpClient(smtpConn);
                smtpClient.UseDefaultCredentials = true;

                // Send the mail message
                smtpClient.Send(email);
            }
            catch (Exception error)
            {
                errorMsg = error.Message;
                retError = -1;
            }
        }
        #endregion

        #region Database Access
        protected void objUserFormAccess_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            UserFormAccessDAL.UserFormAccessDataTable dataTable = e.ReturnValue as
                UserFormAccessDAL.UserFormAccessDataTable;
            if (dataTable != null && dataTable.Rows.Count > 0)
                this.FormAccess = (dataTable.Rows[0] as UserFormAccessDAL.UserFormAccessRow).UserFrmCRUDP;
        }
        #endregion 

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            this.txtReassignEmpNo.Text = String.Empty;
            this.txtReassignReason.Text = String.Empty;
            this.chkReassignSendBack.Checked = false;
            this.ReassignStorageList.Clear();
        }


        public void AddControlsAttribute()
        {
        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.CurrentApproverEmpNo = UIHelper.ConvertObjectToInt(Request.QueryString["CurrentApproverEmpNo"]);
        }

        public void KillSessions()
        {
            ViewState["CustomErrorMsg"] = null;
        }
        #endregion
    }
}