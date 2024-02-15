using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.UI.Helpers;
//using GARMCO.AMS.TAS.UI.TASWCFProxy;
using GARMCO.Common.DAL.WebCommonSetup;
using Telerik.Web.UI;
using GARMCO.AMS.TAS.BL.Entities;
using System.Collections;
//using Microsoft.TeamFoundation.Client;
//using Microsoft.TeamFoundation.VersionControl.Client;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class TASMaster : System.Web.UI.MasterPage
    {
        #region Properties        
        public ObjectDataSource UserFormDataAccess
        {
            get { return this.objUserFormAccess; }
        }

        public string DefaultButton
        {
            set
            {
                this.form1.DefaultButton = value;
            }
        }

        public bool IsRetrieveUserInfo
        {
            get
            {
                bool viewPage = false;
                if (ViewState["IsRetrieveUserInfo"] != null)
                    viewPage = Convert.ToBoolean(ViewState["IsRetrieveUserInfo"]);

                return viewPage;
            }

            set
            {
                ViewState["IsRetrieveUserInfo"] = value;
            }
        }

        public bool IsToCheckSession
        {
            get
            {
                return !Path.GetFileName(Request.Path).Equals(UIHelper.PAGE_ERROR);
            }
        }

        public bool IsRecordModified
        {
            get
            {
                bool isModified = false;
                if (ViewState["IsRecordModified"] != null)
                    isModified = Convert.ToBoolean(ViewState["IsRecordModified"]);

                return isModified;
            }

            set
            {
                ViewState["IsRecordModified"] = value;
            }
        }

        public string FormAccess
        {
            get
            {
                string userFormAccess = GAPConstants.FORM_ACCESS_DEFAULT;
                if (!String.IsNullOrEmpty(this.HiddenFormAccess))
                    userFormAccess = this.HiddenFormAccess;

                return userFormAccess;
            }

            set
            {
                this.HiddenFormAccess = value;
            }
        }

        public bool IsSessionExpired
        {
            get
            {
                return (Session[GAPConstants.GARMCO_USERID] == null);
            }
        }

        public string FormTitle
        {
            get
            {
                return this.litPageTitle.Text.Trim();
            }

            set
            {
                this.litPageTitle.Text = value;
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

        public bool IsRetrieveAllowed
        {
            get
            {
                #region Common Admin security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Retrieve);
                #endregion
            }
        }

        public bool IsCreateAllowed
        {
            get
            {
                #region Common Admin security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Create);
                #endregion
            }
        }

        public bool IsEditAllowed
        {
            get
            {
                #region Common Admin security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Update);
                #endregion
            }
        }

        public bool IsDeleteAllowed
        {
            get
            {
                #region Common Admin Security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Delete);
                #endregion
            }
        }

        public bool IsPrintAllowed
        {
            get
            {
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Print);
            }
        }

        public string LogOnUser
        {
            get { return litUser.Text.Trim(); }
            set { litUser.Text = value; }
        }

        public string LogOnUserInfo
        {
            get { return litUserInfo.Text.Trim(); }
            set { litUserInfo.Text = value; }
        }               

        public string HiddenFormAccess
        {
            get { return this.hidFormAccess.Value.Trim(); }
            set { this.hidFormAccess.Value = value; }
        }
                
        public string ApplicationEnvironment
        {
            set
            {
                this.litEnvironment.Text = value;
            }
        }

        public List<string> AllowedCostCenterList
        {
            get
            {
                List<string> list = Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string>;
                if (list == null)
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = list = new List<string>();

                return list;
            }
        }

        private List<TASFormEntity> TASFormList
        {
            get
            {
                List<TASFormEntity> list = ViewState["TASFormList"] as List<TASFormEntity>;
                if (list == null)
                    ViewState["TASFormList"] = list = new List<TASFormEntity>();

                return list;
            }
            set
            {
                ViewState["TASFormList"] = value;
            }
        }               

        private bool IsComboLoaded
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(Session["IsComboLoaded"]);
            }
            set
            {
                Session["IsComboLoaded"] = value;
            }
        }

        public List<EmployeeDetail> VisitorPassAdminList
        {
            get
            {
                List<EmployeeDetail> list = Session["VisitorPassAdminList"] as List<EmployeeDetail>;
                if (list == null)
                    Session["VisitorPassAdminList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                Session["VisitorPassAdminList"] = value;
            }
        }

        public bool IsVisitorPassSystemAdmin
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(Session["IsVisitorPassSystemAdmin"]);
            }
            set
            {
                Session["IsVisitorPassSystemAdmin"] = value;
            }
        }

        public bool IsSystemAdmin
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_ADMIN]);
            }
        }

        public int CurrentUserPayGrade
        {
            get
            {
                return UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USER_PAY_GRADE]);
            }
        }

        public bool IsTASAdmin
        {
            get
            {
                bool isAdmin = false;

                try
                {
                    string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                    string[] adminArray = ConfigurationManager.AppSettings["SystemAdministrators"].Split(',');

                    if (adminArray != null)
                    {
                        foreach (string item in adminArray)
                        {
                            if (item == userID)
                            {
                                isAdmin = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }

                return isAdmin;
            }
        }
        #endregion

        #region Page Events
        protected void Page_Init(object sender, EventArgs e)
        {
            #region Check if session is expired
            //if (this.IsSessionExpired)
            //{
            //    // Get the name of the content page
            //    string callerForm = Page.ToString().Replace("ASP.", "").Replace("_", ".");
            //    if (!callerForm.ToUpper().Equals("INDEX.ASPX") &&
            //        !callerForm.ToUpper().Equals("DEFAULT.ASPX"))
            //    {
            //        Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);
            //    }
            //}
            #endregion

            if (!Page.IsPostBack)
            {
                #region Get Allowed Cost Center list
                //if (Session[UIHelper.CONST_ALLOWED_COSTCENTER] == null)
                //{
                    int empNo = UIHelper.ConvertObjectToInt(Session[GAPConstants.GARMCO_USERID]);
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = empNo > 0 ? UIHelper.GetAllowedCostCenterByApp(UIHelper.ApplicationCodes.TAS3.ToString(), empNo) : null;
                //}
                #endregion

                #region Get Visitor Pass System Administrator group members
                if (Session["VisitorPassAdminList"] == null)
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    try
                    {
                        DALProxy dataProxy = new DALProxy();
                        var rawData = dataProxy.GetWorkflowActionMember(0, UIHelper.DistributionGroupCodes.VISITADMIN.ToString(), "ALL", ref error, ref innerError);
                        if (rawData != null)
                        {
                            this.VisitorPassAdminList.AddRange(rawData.ToList());
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                #endregion

                #region Determine if current user is member of the Training Administrators group
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                if (this.VisitorPassAdminList.Count > 0 && 
                    userEmpNo > 0)
                {
                    EmployeeDetail adminEmployee = this.VisitorPassAdminList
                        .Where(a => a.EmpNo == userEmpNo)
                        .FirstOrDefault();
                    if (adminEmployee != null)
                        this.IsVisitorPassSystemAdmin = true;
                }
                #endregion
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Fill data to search box control
            FillDataToSearchBox();
            #endregion

            if (!this.IsPostBack && !this.IsSessionExpired)
            {
                #region Checks if user has permission to view the page
                //if (!GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                //    Response.Redirect(String.Format("{0}?error={1}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage)), false);
                #endregion

                this.Page.Title = UIHelper.MASTER_PAGE_TITLE;

                #region Set the application environment
                try
                {
                    bool isLiveDB = false;
                    string dbConnectionString = UIHelper.ConvertObjectToString(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString);
                    string sqlProdServerName = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["SQLProductionServer"]);

                    if (!string.IsNullOrEmpty(dbConnectionString))
                    {
                        string[] connectionArray = dbConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (connectionArray.Length > 0)
                        {
                            foreach (string item in connectionArray)
                            {
                                int idx = item.IndexOf("=");
                                string searchKey = item.Substring(0, idx);
                                if (searchKey.ToUpper() == "DATA SOURCE")
                                {
                                    string searchValue = item.Substring(idx + 1);
                                    if (searchValue.Trim().ToUpper() == sqlProdServerName.ToUpper())
                                    {
                                        isLiveDB = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (isLiveDB)
                        this.ApplicationEnvironment = "PRODUCTION";
                    else
                        this.ApplicationEnvironment = "TEST";
                }
                catch (Exception)
                {

                }
                #endregion

                #region Determine the homepage to use
                string homePage = UIHelper.PAGE_HOME;

                // Find the Home menu item
                RadMenuItem homeMenuItem = this.mainMenu.FindItemByValue("Home");
                if (homeMenuItem != null)
                {
                    homeMenuItem.NavigateUrl = homePage;
                }
                #endregion

                #region Determine menu render mode
                string currentRenderMode = UIHelper.ConvertObjectToString(Session["CurrentMenuRenderMode"]);
                if (currentRenderMode != string.Empty)
                {
                    this.lnkMenuRenderMode.Text = currentRenderMode;
                    this.lnkMenuRenderMode_Click(this.lnkMenuRenderMode, new EventArgs());
                }
                #endregion

                // Get the TFS changeset number wherein the application was compiled and deployed to production
                int deployChangesetNo = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["TFSDeployChangeset"]);
                //int latestChangetSetNo = GetTFSLatestChangeset();

                // Display the system copyright with the changeset information
                this.tdCopyright.InnerHtml = string.Format("TAS ver. 3.0.{0} - Copyright &copy; 2016 Gulf Aluminium Rolling Mill B.S.C. (c)", deployChangesetNo);
            }
        }
        #endregion                

        #region Page Control Events
        protected void lnkMenuRenderMode_Click(object sender, EventArgs e)
        {
            if (this.lnkMenuRenderMode.Text == "Switch to Mobile Menu")
            {
                this.lnkMenuRenderMode.Text = "Switch to Classic Menu";
                this.mainMenu.RenderMode = RenderMode.Mobile;

                // Save render mode to session
                Session["CurrentMenuRenderMode"] = "Switch to Mobile Menu";
            }
            else
            {
                this.lnkMenuRenderMode.Text = "Switch to Mobile Menu";
                this.mainMenu.RenderMode = RenderMode.Lightweight;

                // Save render mode to session
                Session["CurrentMenuRenderMode"] = "Switch to Classic Menu";
            }
        }

        protected void searchBox_Search(object sender, SearchBoxEventArgs e)
        {
            #region Navigate selected page based on value        
            //if (!string.IsNullOrEmpty(e.Value))
            //{
            //    UIHelper.FormAccessCodes currentPage = UIHelper.FormAccessCodes.EMPSELFSVC;
            //    try
            //    {
            //        currentPage = (UIHelper.FormAccessCodes)Enum.Parse(typeof(UIHelper.FormAccessCodes), e.Value);

            //        switch (currentPage)
            //        {
            //            case UIHelper.FormAccessCodes.OTAPPROVAL:
            //                #region Open the Overtime & Meal Voucher Approval Form
            //                Response.Redirect
            //                (
            //                    String.Format(UIHelper.PAGE_OT_MEALVOUCHER_APPROVAL + "?{0}={1}",
            //                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
            //                    UIHelper.PAGE_HOME
            //                ),
            //                false);

            //                break;
            //                #endregion
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        UIHelper.DisplayJavaScriptMessage(this, ex.Message.ToString());
            //    }
            //}
            #endregion

            if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_SELF_SERVICE)
            {
                #region Open the "Employee Self Service" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_HOME + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_ATTENDANCE_DASHBOARD)
            {
                #region Open the "Employee Attendance Dashboard" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_EMPLOYEE_ATTENDANCE_DASHBOARD + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_VIEW_ATTENDANCE_HISTORY)
            {
                #region Open the "View Attendance History" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_VIEW_ATTENDANCE_HISTORY + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_DUTY_ROTA_ENTRY)
            {
                #region Open the "Duty ROTA Entry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_DUTY_ROTA_INQ + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_OVERTIME_ENTRY)
            {
                #region Open the "Employee Overtime Entry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_OVERTIME_ENTRY + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_VIEW_CURRENT_SHIFT_PATTERN)
            {
                #region Open the "View Current Shift Pattern (Employee)" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_VIEW_SHIFT_PATTERN + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TIMESHEET_EXCEPTIONAL)
            {
                #region Open the "Timesheet Exceptional (By Pay Period)" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_TIMESHEET_EXCEPTIONAL + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TIMESHEET_CORRECTION)
            {
                #region Open the "Timesheet Correction" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_INQUIRY + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_REASON_OF_ABSENCE_ENTRY)
            {
                #region Open the "Reason of Absence Entry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_REASON_ABSENCE_INQ + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_MANUAL_TIMESHEET_ENTRY)
            {
                #region Open the "Manual Timesheet Entry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_MANUAL_TIMESHEET_INQ + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_OT_REQUISITION_INQUIRY)
            {
                #region Open the "Overtime Requisition Inquiry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_OVERTIME_REQUISITION_INQUIRY + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_OTMEAL_VOUCHER_APPROVAL)
            {
                #region Open the "Overtime & Meal Voucher Approval" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_OVERTIME_APPROVAL + "?{0}={1}",
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_HOME
                ),
                false);

                //Response.Redirect
                //(
                //    String.Format(UIHelper.PAGE_OT_MEALVOUCHER_APPROVAL + "?{0}={1}",
                //    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                //    UIHelper.PAGE_HOME
                //),
                //false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_SHIFT_PATTERN_CHANGES_EMPLOYEE)
            {
                #region Open the "Shift Pattern Changes (Employee)" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ + "?{0}={1}&{2}={3}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME,
                        "ShiftPatternType",
                        "1"
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_SHIFT_PATTERN_CHANGES_FIRETEAM)
            {
                #region Open the "Shift Pattern Changes (Fire Team)" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ + "?{0}={1}&{2}={3}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME,
                        "ShiftPatternType",
                        "2"
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_SHIFT_PATTERN_CHANGES_CONTRACTOR)
            {
                #region Open the "Shift Pattern Changes (Contractors)" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_SHIFT_PATTERN_CHANGES_INQ + "?{0}={1}&{2}={3}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME,
                        "ShiftPatternType",
                        "3"
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_ASSIGN_CONTRACTOR_SHIFT_PATTERN)
            {
                #region Open the "Assign Contractors Shift Pattern" page
                Response.Redirect
                (
                     String.Format(UIHelper.PAGE_CONTRACTOR_SHIFT_PATTERN_INQ + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_ASSIGN_TEMP_COST_CENTER)
            {
                #region Open the "Assign Temporary Cost Center and Special Job Catalog" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_WORKING_COSTCENTER_INQ + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_EXCEPTIONAL_INQUIRY)
            {
                #region Open the "Employee Exceptional Inquiry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_EMPLOYEE_EXCEPTIONAL_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_LONG_ABSENCES_INQUIRY)
            {
                #region Open the "Long Absences Inquiry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_LONG_ABSENCES_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_REASSIGNED_BUT_SWIPED)
            {
                #region Open the "Reasigned But Swiped" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_RESIGNED_BUTSWIPED_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TIMESHEET_INTEGRITY)
            {
                #region Open the "Timesheet Integrity by Correction Code" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_TIMESHEET_INTEGRITY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TAS_JDE_COMPARISON_REPORT)
            {
                #region Open the "TAS and JDE Comparison Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_TAS_JDE_COMPARISON_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_DIRECTORY)
            {
                #region Open the "Employee Directory" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_EMPLOYEE_DIRECTORY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_GARMCO_CALENDAR)
            {
                #region Open the "GARMCO Calendar" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_GARMCO_CALENDAR + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_COST_CENTER_MANAGERS)
            {
                #region Open the "Cost Center Managers" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_COST_CENTER_MANAGERS + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_MANUAL_ATTENDANCE)
            {
                #region Open the "Manual Attendance" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_MANUAL_ATTENDANCE + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMERGENCY_RESPONSE_TEAM)
            {
                #region Open the "Emergency Response Team" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_EMERGENCY_RESPONSE_TEAM + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_VISITOR_PASS_INQUIRY)
            {
                #region Open the "Visitor Pass Inquiry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_VISITOR_PASS_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_VISITOR_PASS_ENTRY)
            {
                #region Open the "Visitor Pass Entry" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_VISITOR_PASS_ENTRY + "?FormLoadTypeKey=0&{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_COST_CENTER_SECURITY_SETUP)
            {
                #region Open the "Cost Center Security Setup" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_COST_CENTER_ACCESS_INQ + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_MASTER_SHIFT_PATTERN_SETUP)
            {
                #region Open the "Master Shift Pattern Setup" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_MASTER_SHIFT_PATTERN_SETUP + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_MASTER_TABLE_SETUP)
            {
                #region Open the "Master Table Setup" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_UNDER_CONSTRUCTION + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TIMESHEET_VALIDATION_SETUP)
            {
                #region Open the "Timesheet Validations Setup" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_UNDER_CONSTRUCTION + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_SITE_VISITOR_LOG)
            {
                #region Open the "Site Visitor Log Setup" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_UNDER_CONSTRUCTION + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_SHIFT_PATTERN_UPDATE_LOG)
            {
                #region Open the "Shift Pattern Update Service Log" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_UNDER_CONSTRUCTION + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TIMESHEET_PROCESSING_LOG)
            {
                #region Open the "Timesheet Processing Service Log" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_UNDER_CONSTRUCTION + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_ABSENCE_REASON_REPORT)
            {
                #region Open the "Absence Reason Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_ABSENCE_REASON_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_SHIFT_PROJECTION_REPORT)
            {
                #region Open the "Shift Projection Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_SHIFT_PROJECTION_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_CONTRACTOR_ATTENDANCE_REPORT)
            {
                #region Open the "Contractor Attendance Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_CONTRACTOR_ATTENDANCE_INQUIRY + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_ATTENDANCE_HISTORY_REPORT)
            {
                #region Open the "Employee Attendance History Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_EMPLOYEE_ATTENDANCE_HISTORY_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_EMPLOYEE_DAILY_ATTENDANCE_REPORT)
            {
                #region Open the "Employee Daily Attendance" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_DAILY_ATTENDANCE_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_DAILY_ATTENDANCE_FOR_SALARY_STAFF)
            {
                #region Open the "Daily Attendance for Salary Staff" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_DAILY_ATTENDANCE_SALARY_STAFF + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_PUNCTUALITY_REPORT)
            {
                #region Open the "Punctuality Statisctics Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_PUNCTUALITY_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_WEEKLY_PUNCTUALITY_REPORT)
            {
                #region Open the "Weekly Employee Punctuality Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_WEEKLY_PUNCTUALITY_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_WEEKLY_PUNCTUALITY_REPORT)
            {
                #region Open the "Weekly Employee Punctuality Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_WEEKLY_PUNCTUALITY_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_DAY_IN_LIEU_REPORT)
            {
                #region Open the "Day In Lieu Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_DAYINLIEU_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_DUTY_ROTA_REPORT)
            {
                #region Open the "Duty ROTA Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_DUTY_ROTA_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_DIL_LATE_ENTRY_REPORT)
            {
                #region Open the "DIL Due to Late Entry of Duty ROTA Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_LATE_DUTY_ROTA_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_ASPIRE_PAYROLL_REPORT_REPORT)
            {
                #region Open the "Aspire Employees Payroll Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_ASPIRE_PAYROLL_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_WEEKLY_OVERTIME_REPORT)
            {
                #region Open the "Weekly Overtime Report" page
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_WEEKLY_OVERTIME_REPORT + "?{0}={1}",
                        UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                        UIHelper.PAGE_HOME
                ),
                false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_USER_FORM_ACCESS)
            {
                #region Open the "User Form Access" page
                Response.Redirect
                 (
                     String.Format(UIHelper.PAGE_USER_FORM_ACCESS + "?{0}={1}",
                         UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                         UIHelper.PAGE_HOME
                 ),
                 false);
                #endregion
            }
            else if (this.searchBox.Text == UIHelper.CONST_TIMESHEET_PROCESS_SPU_ANALYSIS)
            {
                #region Open the "Timesheet Process / SPU Service Log Analysis" page
                Response.Redirect
                 (
                     String.Format(UIHelper.PAGE_TIMESHEET_PROCESS_SPU_ANALYSIS + "?{0}={1}",
                         UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                         UIHelper.PAGE_HOME
                 ),
                 false);
                #endregion
            }
        }
        #endregion

        #region Private Methods
        private int GetTFSLatestChangeset()
        {
            int latestChangesetId = 0;

            try
            {
                //string tfsURL = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["TFSUrl"]);   
                //string tfsUserName = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["TFSUsername"]);
                //string tfsPassword = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["TFSPassword"]);

                //TfsTeamProjectCollection proj_coll = new TfsTeamProjectCollection(new Uri(tfsURL), new System.Net.NetworkCredential(tfsUserName, tfsPassword));
                ////TfsTeamProjectCollection proj_coll = new TfsTeamProjectCollection(new Uri(tfsURL));
                //proj_coll.EnsureAuthenticated();
                //VersionControlServer vcs = (VersionControlServer)proj_coll.GetService(typeof(VersionControlServer));

                //// Get the latest changeset number
                ////latestChangesetId = vcs.GetLatestChangesetId();
                //latestChangesetId = vcs.QueryHistory("$/GARMCO TAS Ver. 3.0", VersionSpec.Latest, 0, RecursionType.Full, String.Empty, VersionSpec.Latest, VersionSpec.Latest, 1, false, true) 
                //    .Cast<Changeset>() 
                //    .Single() 
                //    .ChangesetId;

                return latestChangesetId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        #region Public Methods
        public void ShowHideDateTime(bool isShow)
        {
            if (isShow)
            {
                this.currentDateTime.Style[HtmlTextWriterStyle.FontFamily] = "Verdana";
                this.currentDateTime.Style[HtmlTextWriterStyle.FontSize] = "8pt";
                this.currentDateTime.Style[HtmlTextWriterStyle.Display] = string.Empty;
            }
            else
                this.currentDateTime.Style[HtmlTextWriterStyle.Display] = "none";
        }
        #endregion

        #region Database Access
        public void SetPageForm(string formCode)
        {
            #region Retrieves user's access to the form
            this.objUserFormAccess.SelectParameters["userFrmFormCode"].DefaultValue = formCode;
            this.objUserFormAccess.Select();
            #endregion
        }

        protected void objUserFormAccess_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            UserFormAccessDAL.UserFormAccessDataTable dataTable = e.ReturnValue as
                UserFormAccessDAL.UserFormAccessDataTable;
            if (dataTable != null && dataTable.Rows.Count > 0)
                this.FormAccess = (dataTable.Rows[0] as UserFormAccessDAL.UserFormAccessRow).UserFrmCRUDP;
        }

        private List<TASFormEntity> GetTASFormList()
        {
            List<TASFormEntity> result = null;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                List<TASFormEntity> rawData = proxy.GetFormList(string.Empty, ref error, ref innerError);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<TASFormEntity>();

                    result.AddRange(rawData.ToList());
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void FillDataToSearchBox()
        {
            ArrayList itemsList = new ArrayList();

            itemsList.Add(UIHelper.CONST_EMPLOYEE_SELF_SERVICE);
            itemsList.Add(UIHelper.CONST_EMPLOYEE_ATTENDANCE_DASHBOARD);
            itemsList.Add(UIHelper.CONST_VIEW_ATTENDANCE_HISTORY);
            itemsList.Add(UIHelper.CONST_DUTY_ROTA_ENTRY);
            itemsList.Add(UIHelper.CONST_EMPLOYEE_OVERTIME_ENTRY);
            itemsList.Add(UIHelper.CONST_VIEW_CURRENT_SHIFT_PATTERN);
            itemsList.Add(UIHelper.CONST_TIMESHEET_EXCEPTIONAL);
            itemsList.Add(UIHelper.CONST_TIMESHEET_CORRECTION);
            itemsList.Add(UIHelper.CONST_REASON_OF_ABSENCE_ENTRY);
            itemsList.Add(UIHelper.CONST_MANUAL_TIMESHEET_ENTRY);
            itemsList.Add(UIHelper.CONST_OT_REQUISITION_INQUIRY);
            itemsList.Add(UIHelper.CONST_OTMEAL_VOUCHER_APPROVAL);
            itemsList.Add(UIHelper.CONST_SHIFT_PATTERN_CHANGES_EMPLOYEE);
            itemsList.Add(UIHelper.CONST_SHIFT_PATTERN_CHANGES_FIRETEAM);
            //itemsList.Add(UIHelper.CONST_SHIFT_PATTERN_CHANGES_CONTRACTOR);
            itemsList.Add(UIHelper.CONST_ASSIGN_CONTRACTOR_SHIFT_PATTERN);
            itemsList.Add(UIHelper.CONST_ASSIGN_TEMP_COST_CENTER);
            itemsList.Add(UIHelper.CONST_EMPLOYEE_EXCEPTIONAL_INQUIRY);
            itemsList.Add(UIHelper.CONST_LONG_ABSENCES_INQUIRY);
            itemsList.Add(UIHelper.CONST_REASSIGNED_BUT_SWIPED);
            itemsList.Add(UIHelper.CONST_TIMESHEET_INTEGRITY);
            itemsList.Add(UIHelper.CONST_TAS_JDE_COMPARISON_REPORT);
            itemsList.Add(UIHelper.CONST_EMPLOYEE_DIRECTORY);
            itemsList.Add(UIHelper.CONST_GARMCO_CALENDAR);
            itemsList.Add(UIHelper.CONST_COST_CENTER_MANAGERS);
            itemsList.Add(UIHelper.CONST_MANUAL_ATTENDANCE);
            itemsList.Add(UIHelper.CONST_EMERGENCY_RESPONSE_TEAM);
            itemsList.Add(UIHelper.CONST_VISITOR_PASS_INQUIRY);
            itemsList.Add(UIHelper.CONST_VISITOR_PASS_ENTRY);
            itemsList.Add(UIHelper.CONST_COST_CENTER_SECURITY_SETUP);
            //itemsList.Add(UIHelper.CONST_FORM_SECURITY_SETUP);
            itemsList.Add(UIHelper.CONST_MASTER_SHIFT_PATTERN_SETUP);
            //itemsList.Add(UIHelper.CONST_MASTER_TABLE_SETUP);
            //itemsList.Add(UIHelper.CONST_TIMESHEET_VALIDATION_SETUP);
            itemsList.Add(UIHelper.CONST_SITE_VISITOR_LOG);
            itemsList.Add(UIHelper.CONST_SHIFT_PATTERN_UPDATE_LOG);
            //itemsList.Add(UIHelper.CONST_TIMESHEET_PROCESSING_LOG);
            itemsList.Add(UIHelper.CONST_ABSENCE_REASON_REPORT);
            itemsList.Add(UIHelper.CONST_SHIFT_PROJECTION_REPORT);
            itemsList.Add(UIHelper.CONST_CONTRACTOR_ATTENDANCE_REPORT);
            itemsList.Add(UIHelper.CONST_EMPLOYEE_ATTENDANCE_HISTORY_REPORT);
            itemsList.Add(UIHelper.CONST_EMPLOYEE_DAILY_ATTENDANCE_REPORT);
            itemsList.Add(UIHelper.CONST_DAILY_ATTENDANCE_FOR_SALARY_STAFF);            
            itemsList.Add(UIHelper.CONST_PUNCTUALITY_REPORT);
            itemsList.Add(UIHelper.CONST_WEEKLY_PUNCTUALITY_REPORT);
            itemsList.Add(UIHelper.CONST_DAY_IN_LIEU_REPORT);
            itemsList.Add(UIHelper.CONST_DUTY_ROTA_REPORT);
            itemsList.Add(UIHelper.CONST_DIL_LATE_ENTRY_REPORT);
            itemsList.Add(UIHelper.CONST_ASPIRE_PAYROLL_REPORT_REPORT);
            itemsList.Add(UIHelper.CONST_WEEKLY_OVERTIME_REPORT);
            itemsList.Add(UIHelper.CONST_USER_FORM_ACCESS);
            itemsList.Add(UIHelper.CONST_TIMESHEET_PROCESS_SPU_ANALYSIS);
            itemsList.Sort();

            this.searchBox.DataSource = itemsList;
        }
        #endregion               
    }
}