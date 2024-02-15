using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class EmployeeSelfService : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            NoRecord,
            CustomFormError,
            InvalidYear,
            NoSwipeStartDate,
            NoSwipeEndDate,
            NoSwipeStartEndDate,
            InvalidSwipeDuration,
            NoCurrentEmpNo
        }

        private enum PanelBarMenuItem
        {
            SwipeHistory,
            AttendanceHistory,
            AbsenceHistory,
            LeaveHistory,
            LeaveBalance,
            DILEntitlement,
            ShiftPatternInfo,
            DependentsInfo,
            PersonalLegalDocument,
            EmployeeLeavePlanner,
            LeaveRequisition,
            DILRequisition,
            PlantSwipeAccessSystem,
            TrainingHistory,
            EPayslip
        }

        private enum DILType
        {
            InactiveDIL = 1,
            ApprovedDIL 
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

        private Dictionary<string, object> EmpSelfServiceStorage
        {
            get
            {
                Dictionary<string, object> list = Session["EmpSelfServiceStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["EmpSelfServiceStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["EmpSelfServiceStorage"] = value;
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

        private int CurrentStartRowIndex
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentStartRowIndex"]);
            }
            set
            {
                ViewState["CurrentStartRowIndex"] = value;
            }
        }

        private int CurrentMaximumRows
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentMaximumRows"]);
            }
            set
            {
                ViewState["CurrentMaximumRows"] = value;
            }
        }

        private int CurrentPageIndex
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentPageIndex"]);
            }
            set
            {
                ViewState["CurrentPageIndex"] = value;
            }
        }

        private int CurrentPageSize
        {
            get
            {
                int pageSize = UIHelper.ConvertObjectToInt(ViewState["CurrentPageSize"]);
                //if (pageSize == 0)
                //    pageSize = this.gridSwipes.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
            }
        }

        private List<string> AllowedCostCenterList
        {
            get
            {
                List<string> list = Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string>;
                if (list == null)
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = list = new List<string>();

                return list;
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

        private List<EmployeeAttendanceEntity> SwipeHistoryList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["SwipeHistoryList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["SwipeHistoryList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["SwipeHistoryList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> AbsenceHistoryList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["AbsenceHistoryList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["AbsenceHistoryList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["AbsenceHistoryList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> LeaveHistoryList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["LeaveHistoryList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["LeaveHistoryList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["LeaveHistoryList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> AttendanceHistoryList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["AttendanceHistoryList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["AttendanceHistoryList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["AttendanceHistoryList"] = value;
            }
        }

        private List<LeaveEntity> LeaveDetailList
        {
            get
            {
                List<LeaveEntity> list = ViewState["LeaveDetailList"] as List<LeaveEntity>;
                if (list == null)
                    ViewState["LeaveDetailList"] = list = new List<LeaveEntity>();

                return list;
            }
            set
            {
                ViewState["LeaveDetailList"] = value;
            }
        }

        private List<DILEntity> ApprovedDILList
        {
            get
            {
                List<DILEntity> list = ViewState["ApprovedDILList"] as List<DILEntity>;
                if (list == null)
                    ViewState["ApprovedDILList"] = list = new List<DILEntity>();

                return list;
            }
            set
            {
                ViewState["ApprovedDILList"] = value;
            }
        }

        private List<DILEntity> InactiveDILList
        {
            get
            {
                List<DILEntity> list = ViewState["InactiveDILList"] as List<DILEntity>;
                if (list == null)
                    ViewState["InactiveDILList"] = list = new List<DILEntity>();

                return list;
            }
            set
            {
                ViewState["InactiveDILList"] = value;
            }
        }

        private EmployeeDetail CurrentUserEmployeeInfo
        {
            get
            {
                return ViewState["CurrentUserEmployeeInfo"] as EmployeeDetail;
            }
            set
            {
                ViewState["CurrentUserEmployeeInfo"] = value;
            }
        }

        private List<AccessReaderEntity> AccessReaderList
        {
            get
            {
                List<AccessReaderEntity> list = ViewState["AccessReaderList"] as List<AccessReaderEntity>;
                if (list == null)
                    ViewState["AccessReaderList"] = list = new List<AccessReaderEntity>();

                return list;
            }
            set
            {
                ViewState["AccessReaderList"] = value;
            }
        }

        private List<ShiftPatternEntity> ShiftPatternList
        {
            get
            {
                List<ShiftPatternEntity> list = ViewState["ShiftPatternList"] as List<ShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPatternList"] = list = new List<ShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPatternList"] = value;
            }
        }

        private int CurrentEmployeeNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentEmployeeNo"]);
            }
            set
            {
                ViewState["CurrentEmployeeNo"] = value;
            }
        }

        private List<DependentEntity> DependentList
        {
            get
            {
                List<DependentEntity> list = ViewState["DependentList"] as List<DependentEntity>;
                if (list == null)
                    ViewState["DependentList"] = list = new List<DependentEntity>();

                return list;
            }
            set
            {
                ViewState["DependentList"] = value;
            }
        }

        private List<TrainingRecordEntity> TrainingRecordList
        {
            get
            {
                List<TrainingRecordEntity> list = ViewState["TrainingRecordList"] as List<TrainingRecordEntity>;
                if (list == null)
                    ViewState["TrainingRecordList"] = list = new List<TrainingRecordEntity>();

                return list;
            }
            set
            {
                ViewState["TrainingRecordList"] = value;
            }
        }

        private bool CanAccessDependentInfo
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["CanAccessDependentInfo"]);
            }
            set
            {
                ViewState["CanAccessDependentInfo"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.EMPSELFSVC.ToString());

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
                //FillCostCenterCombo();
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
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_SELF_SERVICE_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMPLOYEE_SELF_SERVICE_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnSearch.Visible = this.Master.IsRetrieveAllowed;
                //this.btnPrint.Visible = this.Master.IsPrintAllowed;
                //this.btnSubmitApproval.Visible = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnGet.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.EmpSelfServiceStorage.Count > 0)
                {
                    if (this.EmpSelfServiceStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty 
                    && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    ClearForm();
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);

                        // Save value to session variable
                        this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);

                        #region Check if user has permission to display employee photos
                        bool showPhoto = false;
                        try
                        {
                            #region Check if current user is member of the allowed cost center who can view employee photos
                            string[] costCenterArray = ConfigurationManager.AppSettings["EnablePhotoCostCenters"].Split(',');
                            if (costCenterArray != null)
                            {
                                foreach (string item in costCenterArray)
                                {
                                    if (item == costCenter)
                                    {
                                        showPhoto = true;
                                        break;
                                    }
                                }
                            }
                            #endregion

                            #region Check if current user is member of the System Administrators group
                            if (!showPhoto)
                            {
                                string[] adminArray = ConfigurationManager.AppSettings["SystemAdministrators"].Split(',');
                                if (adminArray != null)
                                {
                                    foreach (string item in adminArray)
                                    {
                                        if (item == userID)
                                        {
                                            showPhoto = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        catch (Exception)
                        {
                        }

                        if (!showPhoto)
                        {
                            this.chkShowPhoto.Visible = false;
                            this.tdShowPhoto.InnerText = string.Empty;
                        }
                        else
                        {
                            this.chkShowPhoto.Visible = true;
                            this.tdShowPhoto.InnerText = "Show Photo";
                        }
                        #endregion

                        #region Load Employee Photo
                        LoadEmployeeInformation(this.CurrentEmployeeNo);
                        #endregion

                        #region Load Swipes History                                        
                        RadPanelItem attendanceItem = new RadPanelItem();
                        attendanceItem = this.panBarMain.Items[0];
                        if (attendanceItem != null)
                        {
                            RadPanelItem swipeHistoryItem = attendanceItem.Items[Convert.ToInt32(PanelBarMenuItem.SwipeHistory)];
                            if (swipeHistoryItem != null)
                            {
                                swipeHistoryItem.Selected = true;
                                this.panBarMain_ItemClick(this.panBarMain, new RadPanelBarEventArgs(swipeHistoryItem));
                            }
                        }
                        #endregion

                        Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG] = null;
                    }

                    // Clear session storage
                    //this.EmpSelfServiceStorage.Clear();
                    Session.Remove("EmpSelfServiceStorage");
                    #endregion
                }
                else if (formFlag != string.Empty &&
                    (formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString() || formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString()))
                {
                    #region Show last inquiry data
                    RestoreDataFromCollection();

                    // Clear data storage
                    //this.EmpSelfServiceStorage.Clear();
                    Session.Remove("EmpSelfServiceStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    KillSessions();
                    ProcessQueryString();
                    FillComboData();

                    #region Check if user has permission to display employee photos
                    bool showPhoto = false;
                    try
                    {
                        #region Check if current user is member of the allowed cost center who can view employee photos
                        string[] costCenterArray = ConfigurationManager.AppSettings["EnablePhotoCostCenters"].Split(',');
                        if (costCenterArray != null)
                        {
                            foreach (string item in costCenterArray)
                            {
                                if (item == costCenter)
                                {
                                    showPhoto = true;
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region Check if current user is member of the System Administrators group
                        if (!showPhoto)
                        {
                            string[] adminArray = ConfigurationManager.AppSettings["SystemAdministrators"].Split(',');
                            if (adminArray != null)
                            {
                                foreach (string item in adminArray)
                                {
                                    if (item == userID)
                                    {
                                        showPhoto = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                    }

                    if (!showPhoto)
                    {
                        this.chkShowPhoto.Visible = false;
                        this.tdShowPhoto.InnerText = string.Empty;
                    }
                    else
                    {
                        this.chkShowPhoto.Visible = true;
                        this.tdShowPhoto.InnerText = "Show Photo";
                    }
                    #endregion

                    #region Load Employee Photo
                    this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                    LoadEmployeeInformation(this.CurrentEmployeeNo);

                    this.chkShowPhoto.Checked = true;
                    this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());
                    #endregion

                    #region Load Swipes History                                        
                    RadPanelItem attendanceItem = new RadPanelItem();
                    attendanceItem = this.panBarMain.Items[0];
                    if (attendanceItem != null)
                    {
                        RadPanelItem swipeHistoryItem = attendanceItem.Items[Convert.ToInt32(PanelBarMenuItem.SwipeHistory)];
                        if (swipeHistoryItem != null)
                        {
                            swipeHistoryItem.Selected = true;
                            this.panBarMain_ItemClick(this.panBarMain, new RadPanelBarEventArgs(swipeHistoryItem));
                        }
                    }
                    #endregion

                    #region Check if current user has cost center permission
                    if (this.AllowedCostCenterList.Count > 0)
                    {
                        this.txtEmpNo.Enabled = true;
                        this.btnGet.Enabled = true;
                        this.btnFindEmp.Enabled = true;
                    }
                    else
                    {
                        this.txtEmpNo.Enabled = false;
                        this.btnGet.Enabled = false;
                        this.btnFindEmp.Enabled = false;
                    }
                    #endregion
                }

                #region Check if current user has access to this form
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                this.CanAccessDependentInfo = proxy.CheckIfCanAccessDependentInfo(currentUserEmpNo, ref error, ref innerError);
                #endregion

                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
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
                else if (this.ErrorType == ValidationErrorType.NoSwipeStartEndDate)
                {
                    validator.ErrorMessage = "Start and end date of duration are both required.";
                    validator.ToolTip = "Start and end date of duration are both required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSwipeStartDate)
                {
                    validator.ErrorMessage = "Start date of duration is required.";
                    validator.ToolTip = "Start date of duration is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoSwipeEndDate)
                {
                    validator.ErrorMessage = "End date of duration is required.";
                    validator.ToolTip = "End date of duration is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidSwipeDuration)
                {
                    validator.ErrorMessage = "Date duration is invalid. Start Date must be less than End Date.";
                    validator.ToolTip = "Date duration is invalid. Start Date must be less than End Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidYear)
                {
                    validator.ErrorMessage = "The specified payroll year should not be greater than the current year.";
                    validator.ToolTip = "The specified payroll year should not be greater than the current year.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoCurrentEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is not defined!";
                    validator.ToolTip = "Employee No. is not defined!";
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

        protected void tabMain_TabClick(object sender, RadTabStripEventArgs e)
        {

        }

        protected void tabDIL_TabClick(object sender, RadTabStripEventArgs e)
        {            
            DILType dilType = DILType.ApprovedDIL;
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            int tabIndex = this.tabDIL.SelectedIndex;

            // Initialize panels
            this.panApprovedDIL.Visible = false;
            this.panInactiveDIL.Visible = false;

            switch (tabIndex)
            {
                case 0:     // Approved DIL
                    GetDILEntitlements(dilType, empNo, null, null, false);

                    // Show the panel
                    this.panApprovedDIL.Visible = true;
                    break;

                case 1:     // Inactive DIL
                    dilType = DILType.InactiveDIL;
                    GetDILEntitlements(dilType, empNo, null, null, false);

                    // Show the panel
                    this.panInactiveDIL.Visible = true;
                    break;
            }
        }

        protected void cboMonth_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpSwipeHistorySDate.SelectedDate = this.dtpSwipeHistoryEDate.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYear.Text == string.Empty)
            {
                this.txtYear.Text = DateTime.Now.Year.ToString();
            }
            //else
            //{
            //    // Check if less than year 2000
            //    if (this.txtYear.Value > DateTime.Today.Year)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidYear.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidYear;
            //        this.cusValSwipeDuration.Validate();
            //        this.txtYear.Focus();
            //        return;
            //    }
            //}

            int month = UIHelper.ConvertObjectToInt(this.cboMonth.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYear.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpSwipeHistorySDate.SelectedDate = startDate;
            this.dtpSwipeHistoryEDate.SelectedDate = endDate;
        }
        protected void panBarMain_ItemClick(object sender, RadPanelBarEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Item.Value))
                return;

            // Check the employee no.
            if (this.CurrentEmployeeNo == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoCurrentEmpNo.ToString();
                this.ErrorType = ValidationErrorType.NoCurrentEmpNo;
                this.cusValEmpNo.Validate();
                return;
            }

            try
            {
                RadTab currentSelectedTab = this.tabMain.SelectedTab;
                RadTab newSelectedTab = null;
                PanelBarMenuItem selectedMenu = (PanelBarMenuItem)Enum.Parse(typeof(PanelBarMenuItem), e.Item.Value);
                DateTime? startDate = null;
                DateTime? endDate = null;
                bool reloadDataFromDB = true; //UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1 ? true : false;

                #region Initialize controls and variables
                this.multiPageDIL.Visible = false;
                this.gridSwipeHistory.CurrentPageIndex = 0;
                this.gridSwipeHistory.CurrentPageIndex = 0;
                this.gridLeaveDetails.CurrentPageIndex = 0;
                this.gridShiftPattern.CurrentPageIndex = 0;
                this.gridDependentInfo.CurrentPageIndex = 0;
                this.gridApprovedDIL.CurrentPageIndex = 0;
                this.gridInactiveDIL.CurrentPageIndex = 0;
                this.gridAttendanceHistory.CurrentPageIndex = 0;
                this.gridAttendanceHistory.VirtualItemCount = 1;
                this.gridTraining.CurrentPageIndex = 0;
                this.gridTraining.VirtualItemCount = 1;
                this.gridLeaveHistory.CurrentPageIndex = 0;
                this.gridLeaveHistory.VirtualItemCount = 1;
                this.gridAbsenceHistory.CurrentPageIndex = 0;
                this.gridAbsenceHistory.VirtualItemCount = 1;

                this.CurrentPageIndex = 1;
                this.CurrentPageSize = 10;
                #endregion

                switch (selectedMenu)
                {
                    case PanelBarMenuItem.SwipeHistory:
                        #region Swipes History

                        #region Set the Search filter description
                        if (this.dtpSwipeHistorySDate.SelectedDate != null &&
                            this.dtpSwipeHistoryEDate.SelectedDate != null)
                        {
                            this.lblSwipeHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                                this.dtpSwipeHistorySDate.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                                this.dtpSwipeHistoryEDate.SelectedDate.Value.ToString("dd-MMM-yyyy"));
                        }
                        else
                            this.lblSwipeHistorySearchString.Text = string.Empty;
                        #endregion

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.SwipeHistory.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get the swipe records from the database
                        startDate = this.dtpSwipeHistorySDate.SelectedDate;
                        endDate = this.dtpSwipeHistoryEDate.SelectedDate;

                        string costCenter = this.litCostCenterCode.Text.Trim(); 
                        string locationName = string.Empty;
                        string readerName = string.Empty;

                        if (!string.IsNullOrEmpty(this.cboLocation.SelectedValue) && this.AccessReaderList.Count > 0)
                        {
                            #region Identify the locationName and readerName
                            AccessReaderEntity selectedReader = this.AccessReaderList
                                .Where(a => a.AutoID == UIHelper.ConvertObjectToInt(this.cboLocation.SelectedValue))
                                .FirstOrDefault();
                            if (selectedReader != null)
                            {
                                locationName = selectedReader.LocationName;
                                readerName = selectedReader.ReaderName;
                            }
                            #endregion
                        }

                        // Fill the Swipes History grid
                        GetSwipeHistory(startDate, endDate, this.CurrentEmployeeNo, costCenter, locationName, readerName, reloadDataFromDB);
                        #endregion

                        break;
                        #endregion
                    
                    case PanelBarMenuItem.AbsenceHistory:
                        #region Absences History
                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.AbsenceHistory.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get the Absences History records from database                        
                        //this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        startDate = null;
                        endDate = null;

                        // Fill the Swipes History grid
                        //GetAbsenceHistory(this.CurrentEmployeeNo, startDate, endDate, false);
                        GetAbsenceHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);
                        #endregion

                        break;
                        #endregion

                    case PanelBarMenuItem.LeaveHistory:
                        #region Leave History

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.LeaveHistory.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get the Leave History records from database                        
                        //this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        startDate = null;
                        endDate = null;

                        // Fill data to Leave Balance and Leave History grid
                        GetLeaveDetails(this.CurrentEmployeeNo, reloadDataFromDB);
                        GetLeaveHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);
                        #endregion

                        break;
                        #endregion

                    case PanelBarMenuItem.LeaveBalance:
                        #region Leave Balance

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.LeaveBalance.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get the Leave Details from database                        
                        //this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                        // Fill the Swipes History grid
                        GetLeaveDetails(this.CurrentEmployeeNo, reloadDataFromDB);
                        #endregion

                        break;
                        #endregion

                    case PanelBarMenuItem.AttendanceHistory:
                        #region Attendance History

                        #region Set the Search filter description
                        if (this.dtpStartDateAttendanceHistory.SelectedDate != null &&
                            this.dtpEndDateAttendanceHistory.SelectedDate != null)
                        {
                            this.lblAttendanceHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                                this.dtpStartDateAttendanceHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                                this.dtpEndDateAttendanceHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"));
                        }
                        else
                            this.lblAttendanceHistorySearchString.Text = string.Empty;
                        #endregion

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.AttendanceHistory.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get the Attendance History records from database                        
                        //this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        startDate = this.dtpStartDateAttendanceHistory.SelectedDate;
                        endDate = this.dtpEndDateAttendanceHistory.SelectedDate;

                        // Fill Attendance History grid
                        //GetAttendanceHistory(this.CurrentEmployeeNo, startDate, endDate, false);
                        GetAttendanceHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);
                        #endregion

                        break;
                    #endregion

                    case PanelBarMenuItem.TrainingHistory:
                        #region Training History

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.TrainingHistory.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get training records from the database                        
                        GetTrainingHistory(this.CurrentEmployeeNo, reloadDataFromDB);
                        #endregion

                        break;
                    #endregion

                    case PanelBarMenuItem.DILEntitlement:
                        #region DIL Entitlements

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.DILEntitlement.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            this.multiPageDIL.Visible = true;
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        // Get the Approved DIL Entitlements
                        this.tabDIL_TabClick(this.tabDIL, new RadTabStripEventArgs(this.tabDIL.Tabs[0]));

                        break;
                        #endregion

                    case PanelBarMenuItem.ShiftPatternInfo:
                        #region Shift Pattern Information

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.ShiftPatternInfo.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get shift pattern info from database                        
                        // Fill data to the grid
                        GetShiftPatternInformation(this.CurrentEmployeeNo, reloadDataFromDB);
                        #endregion

                        break;
                        #endregion

                    case PanelBarMenuItem.DependentsInfo:
                        #region Dependents Information

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.DependentsInfo.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        #region Get Dependent info from database     
                        if (UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]) != UIHelper.ConvertObjectToInt(this.txtEmpNo.Text))
                        {
                            if (this.CanAccessDependentInfo ||
                                this.Master.IsSystemAdmin)
                            {
                                // Fill data to the grid
                                GetDependentInfo(this.CurrentEmployeeNo, reloadDataFromDB);
                            }
                            else
                                InitializeDependentInfoGrid();
                        }
                        else
                        {
                            GetDependentInfo(this.CurrentEmployeeNo, reloadDataFromDB);
                        }
                        #endregion

                        break;
                        #endregion

                    case PanelBarMenuItem.PersonalLegalDocument:
                        #region Personal Legal Documents

                        #region Set the current tab
                        newSelectedTab = this.tabMain.Tabs.Where(a => a.Value == PanelBarMenuItem.PersonalLegalDocument.ToString()).FirstOrDefault();
                        if (newSelectedTab != null)
                        {
                            currentSelectedTab.Visible = false;
                            newSelectedTab.Visible = true;

                            this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                            this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(newSelectedTab);
                        }
                        #endregion

                        break;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkSwipeHistoryFilter_CheckedChanged(object sender, EventArgs e)
        {
            this.panSwipeHistoryFilter.Style[HtmlTextWriterStyle.Display] = this.chkSwipeHistoryFilter.Checked ? string.Empty : "none";
        }

        protected void cboMonthAbsence_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpStartDateAbsence.SelectedDate = this.dtpEndDateAbsence.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYearAbsence.Text == string.Empty)
            {
                this.txtYearAbsence.Text = DateTime.Now.Year.ToString();
            }
            //else
            //{
            //    if (this.txtYearAbsence.Value > DateTime.Today.Year)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidYear.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidYear;
            //        this.cusValDurationAbsence.Validate();
            //        this.txtYearAbsence.Focus();
            //        return;
            //    }
            //}

            int month = UIHelper.ConvertObjectToInt(this.cboMonthAbsence.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYearAbsence.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpStartDateAbsence.SelectedDate = startDate;
            this.dtpEndDateAbsence.SelectedDate = endDate;
        }
                
        protected void chkAbsenceHistoryFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.chkAbsenceHistoryFilter.Checked)
            {
                this.lblAbsenceHistorySearchString.Text = string.Empty;
                this.panAbsenceHistoryFilter.Style[HtmlTextWriterStyle.Display] = "none";
            }
            else
            {
                this.panAbsenceHistoryFilter.Style[HtmlTextWriterStyle.Display] = string.Empty;
            }
        }

        protected void chkPayPeriodAbsence_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriodAbsence.Checked)
            {
                this.cboMonthAbsence.Enabled = true;
                this.txtYearAbsence.Enabled = true;
                this.dtpStartDateAbsence.Enabled = false;
                this.dtpEndDateAbsence.Enabled = false;

                #region Set the current pay period
                int month = DateTime.Now.Month;
                if (DateTime.Now.Day >= 16)
                    month = month + 1;

                this.txtYearAbsence.Text = DateTime.Now.Year.ToString();

                if (month > 12)
                {
                    month = 1;
                    this.txtYearAbsence.Text = (DateTime.Now.Year + 1).ToString();
                }

                this.cboMonthAbsence.SelectedValue = month.ToString();
                this.cboMonthAbsence_SelectedIndexChanged(this.cboMonthAbsence, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAbsence.Text, string.Empty, this.cboMonthAbsence.SelectedValue, string.Empty));
                this.cboMonthAbsence.Focus();
                #endregion
            }
            else
            {
                this.cboMonthAbsence.Enabled = false;
                this.txtYearAbsence.Enabled = false;
                this.dtpStartDateAbsence.Enabled = true;
                this.dtpEndDateAbsence.Enabled = true;
            }
        }

        protected void chkPayPeriodSwipeHistory_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriodSwipeHistory.Checked)
            {
                this.cboMonth.Enabled = true;
                this.txtYear.Enabled = true;
                this.dtpSwipeHistorySDate.Enabled = false;
                this.dtpSwipeHistoryEDate.Enabled = false;

                #region Set the current pay period
                int month = DateTime.Now.Month;
                if (DateTime.Now.Day >= 16)
                    month = month + 1;

                this.txtYear.Text = DateTime.Now.Year.ToString();

                if (month > 12)
                {
                    month = 1;
                    this.txtYear.Text = (DateTime.Now.Year + 1).ToString();
                }

                this.cboMonth.SelectedValue = month.ToString();
                this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                this.cboMonth.Focus();
                #endregion
            }
            else
            {
                this.cboMonth.Enabled = false;
                this.txtYear.Enabled = false;
                this.dtpSwipeHistorySDate.Enabled = true;
                this.dtpSwipeHistoryEDate.Enabled = true;
            }
        }

        protected void chkLeaveHistoryFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.chkLeaveHistoryFilter.Checked)
            {
                this.lblLeaveHistorySearchString.Text = string.Empty;
                this.panLeaveHistoryFilter.Style[HtmlTextWriterStyle.Display] = "none";
            }
            else
                this.panLeaveHistoryFilter.Style[HtmlTextWriterStyle.Display] = string.Empty;
        }

        protected void chkPayPeriodLeaveHistory_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriodLeaveHistory.Checked)
            {
                this.cboMonthLeaveHistory.Enabled = true;
                this.txtYearLeaveHistory.Enabled = true;
                this.dtpStartDateLeaveHistory.Enabled = false;
                this.dtpEndDateLeaveHistory.Enabled = false;

                #region Set the current pay period
                int month = DateTime.Now.Month;
                if (DateTime.Now.Day >= 16)
                    month = month + 1;

                this.txtYearLeaveHistory.Text = DateTime.Now.Year.ToString();

                if (month > 12)
                {
                    month = 1;
                    this.txtYearLeaveHistory.Text = (DateTime.Now.Year + 1).ToString();
                }

                this.cboMonthLeaveHistory.SelectedValue = month.ToString();
                this.cboMonthLeaveHistory_SelectedIndexChanged(this.cboMonthLeaveHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthLeaveHistory.Text, string.Empty, this.cboMonthLeaveHistory.SelectedValue, string.Empty));
                this.cboMonthLeaveHistory.Focus();
                #endregion
            }
            else
            {
                this.cboMonthLeaveHistory.Enabled = false;
                this.txtYearLeaveHistory.Enabled = false;
                this.dtpStartDateLeaveHistory.Enabled = true;
                this.dtpEndDateLeaveHistory.Enabled = true;
            }
        }

        protected void cboMonthLeaveHistory_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpStartDateLeaveHistory.SelectedDate = this.dtpEndDateLeaveHistory.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYearLeaveHistory.Text == string.Empty)
            {
                this.txtYearLeaveHistory.Text = DateTime.Now.Year.ToString();
            }
            //else
            //{
            //    if (this.txtYearLeaveHistory.Value > DateTime.Today.Year)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidYear.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidYear;
            //        this.cusValDurationLeaveHistory.Validate();
            //        this.txtYearLeaveHistory.Focus();
            //        return;
            //    }
            //}

            int month = UIHelper.ConvertObjectToInt(this.cboMonthLeaveHistory.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYearLeaveHistory.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpStartDateLeaveHistory.SelectedDate = startDate;
            this.dtpEndDateLeaveHistory.SelectedDate = endDate;
        }

        protected void cboMonthAttendanceHistory_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpStartDateAttendanceHistory.SelectedDate = this.dtpEndDateAttendanceHistory.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYearAttendanceHistory.Text == string.Empty)
            {
                this.txtYearAttendanceHistory.Text = DateTime.Now.Year.ToString();
            }
            //else
            //{
            //    if (this.txtYearAttendanceHistory.Value > DateTime.Today.Year)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidYear.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidYear;
            //        this.cusValDurationAttendanceHistory.Validate();
            //        this.txtYearAttendanceHistory.Focus();
            //        return;
            //    }
            //}

            int month = UIHelper.ConvertObjectToInt(this.cboMonthAttendanceHistory.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYearAttendanceHistory.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpStartDateAttendanceHistory.SelectedDate = startDate;
            this.dtpEndDateAttendanceHistory.SelectedDate = endDate;
        }

        protected void chkPayPeriodAttendanceHistory_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriodAttendanceHistory.Checked)
            {
                this.cboMonthAttendanceHistory.Enabled = true;
                this.txtYearAttendanceHistory.Enabled = true;
                this.dtpStartDateAttendanceHistory.Enabled = false;
                this.dtpEndDateAttendanceHistory.Enabled = false;

                #region Set the current pay period
                int month = DateTime.Now.Month;
                if (DateTime.Now.Day >= 16)
                    month = month + 1;

                this.txtYearAttendanceHistory.Text = DateTime.Now.Year.ToString();

                if (month > 12)
                {
                    month = 1;
                    this.txtYearAttendanceHistory.Text = (DateTime.Now.Year + 1).ToString();
                }
               
                this.cboMonthAttendanceHistory.SelectedValue = month.ToString();
                this.cboMonthAttendanceHistory_SelectedIndexChanged(this.cboMonthAttendanceHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAttendanceHistory.Text, string.Empty, this.cboMonthAttendanceHistory.SelectedValue, string.Empty));
                this.cboMonthAttendanceHistory.Focus();
                #endregion
            }
            else
            {
                this.cboMonthAttendanceHistory.Enabled = false;
                this.txtYearAttendanceHistory.Enabled = false;
                this.dtpStartDateAttendanceHistory.Enabled = true;
                this.dtpEndDateAttendanceHistory.Enabled = true;
            }
        }

        protected void chkAttendanceHistoryFilter_CheckedChanged(object sender, EventArgs e)
        {
            this.panAttendanceHistoryFilter.Style[HtmlTextWriterStyle.Display] = this.chkAttendanceHistoryFilter.Checked ? string.Empty : "none";
        }

        protected void chkShowPhoto_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkShowPhoto.Checked)
                this.tdEmpPhoto.Style[HtmlTextWriterStyle.Display] = string.Empty;
            else
                this.tdEmpPhoto.Style[HtmlTextWriterStyle.Display] = "none";
        }

        protected void lnkReset_Click(object sender, EventArgs e)
        {
            ClearForm();

            #region Load Employee Photo
            this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
            LoadEmployeeInformation(this.CurrentEmployeeNo);

            this.chkShowPhoto.Checked = true;
            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());
            #endregion

            #region Load Swipes History                                        
            RadPanelItem attendanceItem = new RadPanelItem();
            attendanceItem = this.panBarMain.Items[0];
            if (attendanceItem != null)
            {
                RadPanelItem swipeHistoryItem = attendanceItem.Items[Convert.ToInt32(PanelBarMenuItem.SwipeHistory)];
                if (swipeHistoryItem != null)
                {
                    swipeHistoryItem.Selected = true;
                    this.panBarMain_ItemClick(this.panBarMain, new RadPanelBarEventArgs(swipeHistoryItem));
                }
            }
            #endregion

            #region Check if current user has cost center permission
            if (this.AllowedCostCenterList.Count > 0)
            {
                this.txtEmpNo.Enabled = true;
                this.btnGet.Enabled = true;
                this.btnFindEmp.Enabled = true;
            }
            else
            {
                this.txtEmpNo.Enabled = false;
                this.btnGet.Enabled = false;
                this.btnFindEmp.Enabled = false;
            }
            #endregion
        }

        protected void txtYear_TextChanged(object sender, EventArgs e)
        {
            this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
        }

        protected void txtYearAbsence_TextChanged(object sender, EventArgs e)
        {
            this.cboMonthAbsence_SelectedIndexChanged(this.cboMonthAbsence, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAbsence.Text, string.Empty, this.cboMonthAbsence.SelectedValue, string.Empty));
        }

        protected void txtYearAttendanceHistory_TextChanged(object sender, EventArgs e)
        {
            this.cboMonthAttendanceHistory_SelectedIndexChanged(this.cboMonthAttendanceHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAttendanceHistory.Text, string.Empty, this.cboMonthAttendanceHistory.SelectedValue, string.Empty));
        }

        protected void txtYearLeaveHistory_TextChanged(object sender, EventArgs e)
        {
            this.cboMonthLeaveHistory_SelectedIndexChanged(this.cboMonthLeaveHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthLeaveHistory.Text, string.Empty, this.cboMonthLeaveHistory.SelectedValue, string.Empty));
        }
        #endregion

        #region Grid Events

        #region Swipe History Grid 
        protected void gridSwipeHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindSwipeHistoryGrid();
        }

        protected void gridSwipeHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindSwipeHistoryGrid();
        }

        protected void gridSwipeHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.SwipeHistoryList.Count > 0)
            {
                gridSwipeHistory.DataSource = this.SwipeHistoryList;
                gridSwipeHistory.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = gridSwipeHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                gridSwipeHistory.Rebind();
            }
            else
                InitializeSwipeHistoryGrid();
        }

        protected void gridSwipeHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridSwipeHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindSwipeHistoryGrid()
        {
            if (this.SwipeHistoryList.Count > 0)
            {
                this.gridSwipeHistory.DataSource = this.SwipeHistoryList;
                this.gridSwipeHistory.DataBind();
            }
            else
                InitializeSwipeHistoryGrid();

            #region Set the Search filter description
            if (this.dtpSwipeHistorySDate.SelectedDate != null &&
                this.dtpSwipeHistoryEDate.SelectedDate != null)
            {
                this.lblSwipeHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                    this.dtpSwipeHistorySDate.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                    this.dtpSwipeHistoryEDate.SelectedDate.Value.ToString("dd-MMM-yyyy"));
            }
            else
                this.lblSwipeHistorySearchString.Text = string.Empty;
            #endregion
        }

        private void InitializeSwipeHistoryGrid()
        {
            this.gridSwipeHistory.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridSwipeHistory.DataBind();
        }
        #endregion

        #region Absences History Grid 
        protected void gridAbsenceHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (this.chkAbsenceHistoryFilter.Checked)
            {
                startDate = this.dtpStartDateAbsence.SelectedDate;
                endDate = this.dtpEndDateAbsence.SelectedDate;
            }

            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetAbsenceHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridAbsenceHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (this.chkAbsenceHistoryFilter.Checked)
            {
                startDate = this.dtpStartDateAbsence.SelectedDate;
                endDate = this.dtpEndDateAbsence.SelectedDate;
            }

            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetAbsenceHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridAbsenceHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.AbsenceHistoryList.Count > 0)
            {
                gridAbsenceHistory.DataSource = this.AbsenceHistoryList;
                gridAbsenceHistory.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = gridAbsenceHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                gridAbsenceHistory.Rebind();
            }
            else
                InitializeAbsenceHistoryGrid();
        }

        protected void gridAbsenceHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridAbsenceHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindAbsenceHistoryGrid()
        {
            if (this.AbsenceHistoryList.Count > 0)
            {
                int totalRecords = this.AbsenceHistoryList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridAbsenceHistory.VirtualItemCount = totalRecords;
                else
                    this.gridAbsenceHistory.VirtualItemCount = 1;

                this.gridAbsenceHistory.DataSource = this.AbsenceHistoryList;
                this.gridAbsenceHistory.DataBind();                                
            }
            else
                InitializeAbsenceHistoryGrid();

            #region Set the Search filter description
            if (this.dtpStartDateAbsence.SelectedDate != null &&
                this.dtpEndDateAbsence.SelectedDate != null)
            {
                this.lblAbsenceHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                    this.dtpStartDateAbsence.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                    this.dtpEndDateAbsence.SelectedDate.Value.ToString("dd-MMM-yyyy"));
            }
            else
                this.lblAbsenceHistorySearchString.Text = string.Empty;
            #endregion
        }

        private void InitializeAbsenceHistoryGrid()
        {
            this.gridAbsenceHistory.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridAbsenceHistory.DataBind();
        }
        #endregion

        #region Leave History Grid 
        protected void gridLeaveHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (this.chkLeaveHistoryFilter.Checked)
            {
                startDate = this.dtpStartDateLeaveHistory.SelectedDate;
                endDate = this.dtpEndDateLeaveHistory.SelectedDate;
            }
                
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetLeaveHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridLeaveHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (this.chkLeaveHistoryFilter.Checked)
            {
                startDate = this.dtpStartDateLeaveHistory.SelectedDate;
                endDate = this.dtpEndDateLeaveHistory.SelectedDate;
            }

            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetLeaveHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridLeaveHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.LeaveHistoryList.Count > 0)
            {
                this.gridLeaveHistory.DataSource = this.LeaveHistoryList;
                this.gridLeaveHistory.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridLeaveHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridLeaveHistory.Rebind();
            }
            else
                InitializeLeaveHistoryGrid();
        }

        protected void gridLeaveHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridLeaveHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindLeaveHistoryGrid()
        {
            if (this.LeaveHistoryList.Count > 0)
            {
                int totalRecords = this.LeaveHistoryList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridLeaveHistory.VirtualItemCount = totalRecords;
                else
                    this.gridLeaveHistory.VirtualItemCount = 1;

                this.gridLeaveHistory.DataSource = this.LeaveHistoryList;
                this.gridLeaveHistory.DataBind();
            }
            else
                InitializeLeaveHistoryGrid();

            #region Set the Search filter description
            if (this.dtpStartDateLeaveHistory.SelectedDate != null &&
                this.dtpEndDateLeaveHistory.SelectedDate != null)
            {
                this.lblLeaveHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                    this.dtpStartDateLeaveHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                    this.dtpEndDateLeaveHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"));
            }
            else
                this.lblLeaveHistorySearchString.Text = string.Empty;
            #endregion
        }

        private void InitializeLeaveHistoryGrid()
        {
            this.gridLeaveHistory.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridLeaveHistory.DataBind();
        }
        #endregion
                
        #region Leave Balance Grid 
        protected void gridLeaveDetails_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindLeaveDetailGrid();
        }

        protected void gridLeaveDetails_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindLeaveDetailGrid();
        }

        protected void gridLeaveDetails_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.LeaveDetailList.Count > 0)
            {
                this.gridLeaveDetails.DataSource = this.LeaveDetailList;
                this.gridLeaveDetails.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridLeaveDetails.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridLeaveDetails.Rebind();
            }
            else
                InitializeLeaveDetailGrid();
        }

        protected void gridLeaveDetails_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridLeaveDetails_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindLeaveDetailGrid()
        {
            if (this.LeaveDetailList.Count > 0)
            {
                this.gridLeaveDetails.DataSource = this.LeaveDetailList;
                this.gridLeaveDetails.DataBind();
            }
            else
                InitializeLeaveDetailGrid();
        }

        private void InitializeLeaveDetailGrid()
        {
            this.gridLeaveDetails.DataSource = new List<LeaveEntity>();
            this.gridLeaveDetails.DataBind();
        }
        #endregion

        #region Attendance History Grid 
        protected void gridAttendanceHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetAttendanceHistoryV2(this.CurrentEmployeeNo, this.dtpStartDateAttendanceHistory.SelectedDate, this.dtpEndDateAttendanceHistory.SelectedDate, true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridAttendanceHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetAttendanceHistoryV2(this.CurrentEmployeeNo, this.dtpStartDateAttendanceHistory.SelectedDate, this.dtpEndDateAttendanceHistory.SelectedDate, true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridAttendanceHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.AttendanceHistoryList.Count > 0)
            {
                this.gridAttendanceHistory.DataSource = this.AttendanceHistoryList;
                this.gridAttendanceHistory.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridAttendanceHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridAttendanceHistory.Rebind();
            }
            else
                InitializeAttendanceHistoryGrid();
        }

        protected void gridAttendanceHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridAttendanceHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindAttendanceHistoryGrid()
        {
            if (this.AttendanceHistoryList.Count > 0)
            {
                int totalRecords = this.AttendanceHistoryList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridAttendanceHistory.VirtualItemCount = totalRecords;
                else
                    this.gridAttendanceHistory.VirtualItemCount = 1;

                this.gridAttendanceHistory.DataSource = this.AttendanceHistoryList;
                this.gridAttendanceHistory.DataBind();
            }
            else
                InitializeAttendanceHistoryGrid();

            #region Set the Search filter description
            if (this.dtpStartDateAttendanceHistory.SelectedDate != null &&
                this.dtpEndDateAttendanceHistory.SelectedDate != null)
            {
                this.lblAttendanceHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                    this.dtpStartDateAttendanceHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                    this.dtpEndDateAttendanceHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"));
            }
            else
                this.lblAttendanceHistorySearchString.Text = string.Empty;
            #endregion
        }

        private void InitializeAttendanceHistoryGrid()
        {
            this.gridAttendanceHistory.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridAttendanceHistory.DataBind();
        }
        #endregion

        #region Training History Grid 
        protected void gridTraining_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetTrainingHistory(this.CurrentEmployeeNo);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridTraining_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetTrainingHistory(this.CurrentEmployeeNo);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridTraining_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.TrainingRecordList.Count > 0)
            {
                gridTraining.DataSource = this.TrainingRecordList;
                gridTraining.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = gridTraining.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridTraining.Rebind();
            }
            else
                InitializeTrainingRecordGrid();
        }

        protected void gridTraining_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                   
                }
            }
        }

        protected void gridTraining_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindTrainingRecordGrid()
        {
            if (this.TrainingRecordList.Count > 0)
            {
                int totalRecords = this.TrainingRecordList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridTraining.VirtualItemCount = totalRecords;
                else
                    this.gridTraining.VirtualItemCount = 1;

                this.gridTraining.DataSource = this.TrainingRecordList;
                this.gridTraining.DataBind();
            }
            else
                InitializeTrainingRecordGrid();
        }

        private void InitializeTrainingRecordGrid()
        {
            this.gridTraining.DataSource = new List<TrainingRecordEntity>();
            this.gridTraining.DataBind();
        }
        #endregion

        #region Shift Pattern Information Grid 
        protected void gridShiftPattern_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindShiftPatternGrid();
        }

        protected void gridShiftPattern_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindShiftPatternGrid();
        }

        protected void gridShiftPattern_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ShiftPatternList.Count > 0)
            {
                this.gridShiftPattern.DataSource = this.ShiftPatternList;
                this.gridShiftPattern.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridShiftPattern.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridShiftPattern.Rebind();
            }
            else
                InitializeShiftPatternGrid();
        }

        protected void gridShiftPattern_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridShiftPattern_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindShiftPatternGrid()
        {
            if (this.ShiftPatternList.Count > 0)
            {
                this.gridShiftPattern.DataSource = this.ShiftPatternList;
                this.gridShiftPattern.DataBind();
            }
            else
                InitializeShiftPatternGrid();
        }

        private void InitializeShiftPatternGrid()
        {
            this.gridShiftPattern.DataSource = new List<LeaveEntity>();
            this.gridShiftPattern.DataBind();
        }
        #endregion

        #region Dependent Information Grid 
        protected void gridDependentInfo_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDependentInfoGrid();
        }

        protected void gridDependentInfo_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDependentInfoGrid();
        }

        protected void gridDependentInfo_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ShiftPatternList.Count > 0)
            {
                this.gridDependentInfo.DataSource = this.ShiftPatternList;
                this.gridDependentInfo.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridDependentInfo.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridDependentInfo.Rebind();
            }
            else
                InitializeDependentInfoGrid();
        }

        protected void gridDependentInfo_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridDependentInfo_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDependentInfoGrid()
        {
            if (this.DependentList.Count > 0)
            {
                this.gridDependentInfo.DataSource = this.DependentList;
                this.gridDependentInfo.DataBind();
            }
            else
                InitializeDependentInfoGrid();
        }

        private void InitializeDependentInfoGrid()
        {
            this.gridDependentInfo.DataSource = new List<DependentEntity>();
            this.gridDependentInfo.DataBind();
        }
        #endregion

        #region Approved DIL Grid 
        protected void gridApprovedDIL_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindApprovedDILGrid();
        }

        protected void gridApprovedDIL_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindApprovedDILGrid();
        }

        protected void gridApprovedDIL_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ApprovedDILList.Count > 0)
            {
                this.gridApprovedDIL.DataSource = this.ApprovedDILList;
                this.gridApprovedDIL.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridApprovedDIL.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridApprovedDIL.Rebind();
            }
            else
                InitializeApprovedDILGrid();
        }

        protected void gridApprovedDIL_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridApprovedDIL_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindApprovedDILGrid()
        {
            if (this.ApprovedDILList.Count > 0)
            {
                this.gridApprovedDIL.DataSource = this.ApprovedDILList;
                this.gridApprovedDIL.DataBind();
            }
            else
                InitializeApprovedDILGrid();
        }

        private void InitializeApprovedDILGrid()
        {
            this.gridApprovedDIL.DataSource = new List<DILEntity>();
            this.gridApprovedDIL.DataBind();
        }
        #endregion

        #region Inactive DIL Grid 
        protected void gridInactiveDIL_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindInactiveDILGrid();
        }

        protected void gridInactiveDIL_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindInactiveDILGrid();
        }

        protected void gridInactiveDIL_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.InactiveDILList.Count > 0)
            {
                this.gridInactiveDIL.DataSource = this.InactiveDILList;
                this.gridInactiveDIL.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridInactiveDIL.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridInactiveDIL.Rebind();
            }
            else
                InitializeInactiveDILGrid();
        }

        protected void gridInactiveDIL_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridInactiveDIL_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindInactiveDILGrid()
        {
            if (this.InactiveDILList.Count > 0)
            {
                this.gridInactiveDIL.DataSource = this.InactiveDILList;
                this.gridInactiveDIL.DataBind();
            }
            else
                InitializeInactiveDILGrid();
        }

        private void InitializeInactiveDILGrid()
        {
            this.gridInactiveDIL.DataSource = new List<DILEntity>();
            this.gridInactiveDIL.DataBind();
        }
        #endregion

        #endregion

        #region Action Buttons
        protected void btnSearchSwipeHistory_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            DateTime? startDate = this.dtpSwipeHistorySDate.SelectedDate;
            DateTime? endDate = this.dtpSwipeHistoryEDate.SelectedDate;
            //int empNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
            string costCenter = this.litCostCenterCode.Text.Trim(); 
            string locationName = string.Empty;
            string readerName = string.Empty;

            try
            {
                #region Perform data validation
                // Check the employee no.
                if (this.CurrentEmployeeNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoCurrentEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoCurrentEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                if (startDate == null && endDate == null)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSwipeStartEndDate.ToString();
                    this.ErrorType = ValidationErrorType.NoSwipeStartEndDate;
                    this.cusValSwipeDuration.Validate();
                    errorCount++;
                }
                else
                { 
                    if (startDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeStartDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeStartDate;
                        this.cusValSwipeDuration.Validate();
                        errorCount++;
                    }
                    else if (endDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeEndDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeEndDate;
                        this.cusValSwipeDuration.Validate();
                        errorCount++;
                    }
                    else
                    {
                        // Check if duration is valid
                        if (startDate > endDate)
                        {
                            this.txtGeneric.Text = ValidationErrorType.InvalidSwipeDuration.ToString();
                            this.ErrorType = ValidationErrorType.InvalidSwipeDuration;
                            this.cusValSwipeDuration.Validate();
                            errorCount++;
                        }
                    }
                }
                

                if (errorCount > 0)
                    return;
                #endregion

                if (!string.IsNullOrEmpty(this.cboLocation.SelectedValue) && this.AccessReaderList.Count > 0)
                {
                    #region Identify the locationName and readerName
                    AccessReaderEntity selectedReader = this.AccessReaderList
                        .Where(a => a.AutoID == UIHelper.ConvertObjectToInt(this.cboLocation.SelectedValue))
                        .FirstOrDefault();
                    if (selectedReader != null)
                    {
                        locationName = selectedReader.LocationName;
                        readerName = selectedReader.ReaderName;
                    }
                    #endregion
                }

                GetSwipeHistory(startDate, endDate, this.CurrentEmployeeNo, costCenter, locationName, readerName);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnResetSwipeHistory_Click(object sender, EventArgs e)
        {
            // Reset controls
            this.chkPayPeriodSwipeHistory.Checked = false;
            this.chkPayPeriodSwipeHistory_CheckedChanged(this.chkPayPeriodSwipeHistory, new EventArgs());

            this.cboMonth.SelectedIndex = -1;
            this.cboMonth.Text = string.Empty;
            this.cboLocation.SelectedIndex = -1;
            this.cboLocation.Text = string.Empty;
            this.dtpSwipeHistorySDate.SelectedDate = null;
            this.dtpSwipeHistoryEDate.SelectedDate = null;
            this.txtYear.Text = string.Empty;
            this.lblSwipeHistorySearchString.Text = string.Empty;

            // Clear collections
            this.SwipeHistoryList = null;

            // Reset grid
            InitializeSwipeHistoryGrid();
        }

        protected void btnSearchAbsence_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            DateTime? startDate = this.dtpStartDateAbsence.SelectedDate;
            DateTime? endDate = this.dtpEndDateAbsence.SelectedDate;
            //int empNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

            try
            {
                #region Perform data validation
                // Check the employee no.
                if (this.CurrentEmployeeNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoCurrentEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoCurrentEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                if (startDate == null && endDate == null)
                {
                    //this.txtGeneric.Text = ValidationErrorType.NoSwipeStartEndDate.ToString();
                    //this.ErrorType = ValidationErrorType.NoSwipeStartEndDate;
                    //this.cusValDurationAbsence.Validate();
                    //errorCount++;
                }
                else
                {
                    if (startDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeStartDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeStartDate;
                        this.cusValDurationAbsence.Validate();
                        errorCount++;
                    }
                    else if (endDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeEndDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeEndDate;
                        this.cusValDurationAbsence.Validate();
                        errorCount++;
                    }
                    else
                    {
                        // Check if duration is valid
                        if (startDate > endDate)
                        {
                            this.txtGeneric.Text = ValidationErrorType.InvalidSwipeDuration.ToString();
                            this.ErrorType = ValidationErrorType.InvalidSwipeDuration;
                            this.cusValDurationAbsence.Validate();
                            errorCount++;
                        }
                    }
                }


                if (errorCount > 0)
                    return;
                #endregion

                GetAbsenceHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnResetAbsence_Click(object sender, EventArgs e)
        {
            // Reset controls
            this.chkPayPeriodAbsence.Checked = false;
            this.chkPayPeriodAbsence_CheckedChanged(this.chkPayPeriodAbsence, new EventArgs());

            this.cboMonthAbsence.SelectedIndex = -1;
            this.cboMonthAbsence.Text = string.Empty;
            this.dtpStartDateAbsence.SelectedDate = null;
            this.dtpEndDateAbsence.SelectedDate = null;
            this.txtYearAbsence.Text = string.Empty;
            this.lblAbsenceHistorySearchString.Text = string.Empty;

            // Clear collections
            this.AbsenceHistoryList = null;

            // Reset grid
            this.gridAbsenceHistory.VirtualItemCount = 1;
            this.CurrentPageIndex = 1;
            this.CurrentPageSize = 10;
            InitializeAbsenceHistoryGrid();
        }

        protected void btnSearchLeaveHistory_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            DateTime? startDate = this.dtpStartDateLeaveHistory.SelectedDate;
            DateTime? endDate = this.dtpEndDateLeaveHistory.SelectedDate;
            //int empNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

            try
            {
                #region Perform data validation
                // Check the employee no.
                if (this.CurrentEmployeeNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoCurrentEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoCurrentEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                if (startDate == null && endDate == null)
                {
                    //this.txtGeneric.Text = ValidationErrorType.NoSwipeStartEndDate.ToString();
                    //this.ErrorType = ValidationErrorType.NoSwipeStartEndDate;
                    //this.cusValDurationLeaveHistory.Validate();
                    //errorCount++;
                }
                else
                {
                    if (startDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeStartDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeStartDate;
                        this.cusValDurationLeaveHistory.Validate();
                        errorCount++;
                    }
                    else if (endDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeEndDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeEndDate;
                        this.cusValDurationLeaveHistory.Validate();
                        errorCount++;
                    }
                    else
                    {
                        // Check if duration is valid
                        if (startDate > endDate)
                        {
                            this.txtGeneric.Text = ValidationErrorType.InvalidSwipeDuration.ToString();
                            this.ErrorType = ValidationErrorType.InvalidSwipeDuration;
                            this.cusValDurationLeaveHistory.Validate();
                            errorCount++;
                        }
                    }
                }


                if (errorCount > 0)
                    return;
                #endregion

                GetLeaveHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnResetLeaveHistory_Click(object sender, EventArgs e)
        {
            // Reset controls
            this.chkPayPeriodLeaveHistory.Checked = false;
            this.chkPayPeriodLeaveHistory_CheckedChanged(this.chkPayPeriodLeaveHistory, new EventArgs());

            this.cboMonthLeaveHistory.SelectedIndex = -1;
            this.cboMonthLeaveHistory.Text = string.Empty;
            this.dtpStartDateLeaveHistory.SelectedDate = null;
            this.dtpEndDateLeaveHistory.SelectedDate = null;
            this.txtYearLeaveHistory.Text = string.Empty;

            // Clear collections
            this.LeaveHistoryList = null;

            // Reset grid
            this.gridLeaveHistory.VirtualItemCount = 1;
            this.CurrentPageIndex = 1;
            this.CurrentPageSize = 10;
            InitializeLeaveHistoryGrid();
        }

        protected void btnSearchAttendanceHistory_Click(object sender, EventArgs e)
        {
            int errorCount = 0;
            DateTime? startDate = this.dtpStartDateAttendanceHistory.SelectedDate;
            DateTime? endDate = this.dtpEndDateAttendanceHistory.SelectedDate;
            //int empNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

            try
            {
                #region Perform data validation
                // Check the employee no.
                if (this.CurrentEmployeeNo == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoCurrentEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoCurrentEmpNo;
                    this.cusValEmpNo.Validate();
                    errorCount++;
                }

                if (startDate == null && endDate == null)
                {
                    //this.txtGeneric.Text = ValidationErrorType.NoSwipeStartEndDate.ToString();
                    //this.ErrorType = ValidationErrorType.NoSwipeStartEndDate;
                    //this.cusValDurationAttendanceHistory.Validate();
                    //errorCount++;
                }
                else
                {
                    if (startDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeStartDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeStartDate;
                        this.cusValDurationAttendanceHistory.Validate();
                        errorCount++;
                    }
                    else if (endDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoSwipeEndDate.ToString();
                        this.ErrorType = ValidationErrorType.NoSwipeEndDate;
                        this.cusValDurationAttendanceHistory.Validate();
                        errorCount++;
                    }
                    else
                    {
                        // Check if duration is valid
                        if (startDate > endDate)
                        {
                            this.txtGeneric.Text = ValidationErrorType.InvalidSwipeDuration.ToString();
                            this.ErrorType = ValidationErrorType.InvalidSwipeDuration;
                            this.cusValDurationAttendanceHistory.Validate();
                            errorCount++;
                        }
                    }
                }


                if (errorCount > 0)
                    return;
                #endregion

                GetAttendanceHistoryV2(this.CurrentEmployeeNo, startDate, endDate, true);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnResetAttendanceHistory_Click(object sender, EventArgs e)
        {
            // Reset controls
            this.chkPayPeriodAttendanceHistory.Checked = false;
            this.chkPayPeriodAttendanceHistory_CheckedChanged(this.chkPayPeriodAttendanceHistory, new EventArgs());

            this.cboMonthAttendanceHistory.SelectedIndex = -1;
            this.cboMonthAttendanceHistory.Text = string.Empty;
            this.dtpStartDateAttendanceHistory.SelectedDate = null;
            this.dtpEndDateAttendanceHistory.SelectedDate = null;
            this.txtYearAttendanceHistory.Text = string.Empty;
            this.lblAttendanceHistorySearchString.Text = string.Empty;

            // Clear collections
            this.AttendanceHistoryList = null;

            // Reset grid
            this.gridAttendanceHistory.VirtualItemCount = 1;
            this.CurrentPageIndex = 1;
            this.CurrentPageSize = 10;
            InitializeAttendanceHistoryGrid();
        }

        protected void btnGet_Click(object sender, EventArgs e)
        {
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            if (empNo == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoCurrentEmpNo.ToString();
                this.ErrorType = ValidationErrorType.NoCurrentEmpNo;
                this.cusValEmpNo.Validate();
                return;
            }

            if (empNo.ToString().Length == 4)
            {
                empNo += 10000000;
            }

            ClearForm();

            // Save to session variable
            this.CurrentEmployeeNo = empNo;

            // Get Employee photo
            if (LoadEmployeeInformation(empNo))
            {
                #region Load Swipes History                                        
                RadPanelItem attendanceItem = new RadPanelItem();
                attendanceItem = this.panBarMain.Items[0];
                if (attendanceItem != null)
                {
                    RadPanelItem swipeHistoryItem = attendanceItem.Items[Convert.ToInt32(PanelBarMenuItem.SwipeHistory)];
                    if (swipeHistoryItem != null)
                    {
                        swipeHistoryItem.Selected = true;
                        this.panBarMain_ItemClick(this.panBarMain, new RadPanelBarEventArgs(swipeHistoryItem));
                    }
                }
                #endregion
            }
        }

        protected void btnFindEmp_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_HOME
            ),
            false);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();

            #region Load Employee Photo
            this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
            LoadEmployeeInformation(this.CurrentEmployeeNo);

            this.chkShowPhoto.Checked = true;
            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());
            #endregion

            #region Load Swipes History                                        
            RadPanelItem attendanceItem = new RadPanelItem();
            attendanceItem = this.panBarMain.Items[0];
            if (attendanceItem != null)
            {
                RadPanelItem swipeHistoryItem = attendanceItem.Items[Convert.ToInt32(PanelBarMenuItem.SwipeHistory)];
                if (swipeHistoryItem != null)
                {
                    swipeHistoryItem.Selected = true;
                    this.panBarMain_ItemClick(this.panBarMain, new RadPanelBarEventArgs(swipeHistoryItem));
                }
            }
            #endregion

            #region Check if current user has cost center permission
            if (this.AllowedCostCenterList.Count > 0)
            {
                this.txtEmpNo.Enabled = true;
                this.btnGet.Enabled = true;
                this.btnFindEmp.Enabled = true;
            }
            else
            {
                this.txtEmpNo.Enabled = false;
                this.btnGet.Enabled = false;
                this.btnFindEmp.Enabled = false;
            }
            #endregion
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.EmpSelfServiceStorage.Count == 0)
                return;

            #region Restore query string values
            if (this.EmpSelfServiceStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.EmpSelfServiceStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.EmpSelfServiceStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.EmpSelfServiceStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.EmpSelfServiceStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.EmpSelfServiceStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.EmpSelfServiceStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.EmpSelfServiceStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.EmpSelfServiceStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.EmpSelfServiceStorage.ContainsKey("SwipeHistoryList"))
                this.SwipeHistoryList = this.EmpSelfServiceStorage["SwipeHistoryList"] as List<EmployeeAttendanceEntity>;
            else
                this.SwipeHistoryList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("CurrentUserEmployeeInfo"))
                this.CurrentUserEmployeeInfo = this.EmpSelfServiceStorage["CurrentUserEmployeeInfo"] as EmployeeDetail;
            else
                this.CurrentUserEmployeeInfo = null;

            if (this.EmpSelfServiceStorage.ContainsKey("AccessReaderList"))
            {
                this.AccessReaderList = this.EmpSelfServiceStorage["AccessReaderList"] as List<AccessReaderEntity>;
                FillDataToLocationCombo(false);
            }
            else
                this.AccessReaderList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("AbsenceHistoryList"))
                this.AbsenceHistoryList = this.EmpSelfServiceStorage["AbsenceHistoryList"] as List<EmployeeAttendanceEntity>;
            else
                this.AbsenceHistoryList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("LeaveHistoryList"))
                this.LeaveHistoryList = this.EmpSelfServiceStorage["LeaveHistoryList"] as List<EmployeeAttendanceEntity>;
            else
                this.LeaveHistoryList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("AttendanceHistoryList"))
                this.AttendanceHistoryList = this.EmpSelfServiceStorage["AttendanceHistoryList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceHistoryList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("LeaveDetailList"))
                this.LeaveDetailList = this.EmpSelfServiceStorage["LeaveDetailList"] as List<LeaveEntity>;
            else
                this.LeaveDetailList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("ApprovedDILList"))
                this.ApprovedDILList = this.EmpSelfServiceStorage["ApprovedDILList"] as List<DILEntity>;
            else
                this.ApprovedDILList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("InactiveDILList"))
                this.InactiveDILList = this.EmpSelfServiceStorage["InactiveDILList"] as List<DILEntity>;
            else
                this.InactiveDILList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("CurrentEmployeeNo"))
                this.CurrentEmployeeNo = UIHelper.ConvertObjectToInt(this.EmpSelfServiceStorage["CurrentEmployeeNo"]);
            else
                this.CurrentEmployeeNo = 0;

            if (this.EmpSelfServiceStorage.ContainsKey("ShiftPatternList"))
                this.ShiftPatternList = this.EmpSelfServiceStorage["ShiftPatternList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPatternList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("DependentList"))
                this.DependentList = this.EmpSelfServiceStorage["DependentList"] as List<DependentEntity>;
            else
                this.DependentList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("TrainingRecordList"))
                this.TrainingRecordList = this.EmpSelfServiceStorage["TrainingRecordList"] as List<TrainingRecordEntity>;
            else
                this.TrainingRecordList = null;

            if (this.EmpSelfServiceStorage.ContainsKey("CanAccessDependentInfo"))
                this.CanAccessDependentInfo = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["CanAccessDependentInfo"]);
            else
                this.CanAccessDependentInfo = false;

            // Reload combo data
            FillComboData(false);
            #endregion

            #region Restore control values

            #region Employee Details
            if (this.EmpSelfServiceStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("litEmployeeName"))
                this.litEmployeeName.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["litEmployeeName"]);
            else
                this.litEmployeeName.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("litCostCenterCode"))
                this.litCostCenterCode.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["litCostCenterCode"]);
            else
                this.litCostCenterCode.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("litJoiningDate"))
                this.litJoiningDate.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["litJoiningDate"]);
            else
                this.litJoiningDate.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("litServiceYear"))
                this.litServiceYear.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["litServiceYear"]);
            else
                this.litServiceYear.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("imgPhoto"))
                this.imgPhoto.ImageUrl = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["imgPhoto"]);
            else
                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;

            if (this.EmpSelfServiceStorage.ContainsKey("chkShowPhoto"))
                this.chkShowPhoto.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkShowPhoto"]);
            else
                this.chkShowPhoto.Checked = false;

            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());
            #endregion

            #region Swipes History
            if (this.EmpSelfServiceStorage.ContainsKey("cboMonth") && this.AccessReaderList.Count > 0)
            {
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["cboMonth"]);
                if (!string.IsNullOrEmpty(this.cboMonth.SelectedValue))
                {
                    this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                }
            }
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.EmpSelfServiceStorage.ContainsKey("cboLocation") && this.AccessReaderList.Count > 0)
                this.cboLocation.SelectedValue = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["cboLocation"]);
            else
            {
                this.cboLocation.Text = string.Empty;
                this.cboLocation.SelectedIndex = -1;
            }

            if (this.EmpSelfServiceStorage.ContainsKey("dtpStartDate"))
                this.dtpSwipeHistorySDate.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpStartDate"]);
            else
                this.dtpSwipeHistorySDate.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("dtpStartDate"))
                this.dtpSwipeHistoryEDate.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpStartDate"]);
            else
                this.dtpSwipeHistoryEDate.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("chkPayPeriodSwipeHistory"))
                this.chkPayPeriodSwipeHistory.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkPayPeriodSwipeHistory"]);
            else
                this.chkPayPeriodSwipeHistory.Checked = false;

            this.chkPayPeriodSwipeHistory_CheckedChanged(this.chkPayPeriodSwipeHistory, new EventArgs());
            #endregion

            #region Absence  History
            if (this.EmpSelfServiceStorage.ContainsKey("cboMonthAbsence") && this.AccessReaderList.Count > 0)
            {
                this.cboMonthAbsence.SelectedValue = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["cboMonthAbsence"]);
                if (!string.IsNullOrEmpty(this.cboMonthAbsence.SelectedValue))
                {
                    this.cboMonthAbsence_SelectedIndexChanged(this.cboMonthAbsence, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAbsence.Text, string.Empty, this.cboMonthAbsence.SelectedValue, string.Empty));
                }
            }
            else
            {
                this.cboMonthAbsence.Text = string.Empty;
                this.cboMonthAbsence.SelectedIndex = -1;
            }

            if (this.EmpSelfServiceStorage.ContainsKey("dtpStartDateAbsence"))
                this.dtpStartDateAbsence.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpStartDateAbsence"]);
            else
                this.dtpStartDateAbsence.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("dtpEndDateAbsence"))
                this.dtpEndDateAbsence.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpEndDateAbsence"]);
            else
                this.dtpEndDateAbsence.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("txtYearAbsence"))
                this.txtYearAbsence.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["txtYearAbsence"]);
            else
                this.txtYearAbsence.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("chkPayPeriodAbsence"))
                this.chkPayPeriodAbsence.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkPayPeriodAbsence"]);
            else
                this.chkPayPeriodAbsence.Checked = false;

            this.chkPayPeriodAbsence_CheckedChanged(this.chkPayPeriodAbsence, new EventArgs());

            if (this.EmpSelfServiceStorage.ContainsKey("chkAbsenceHistoryFilter"))
                this.chkAbsenceHistoryFilter.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkAbsenceHistoryFilter"]);
            else
                this.chkAbsenceHistoryFilter.Checked = false;

            this.chkAbsenceHistoryFilter_CheckedChanged(this.chkAbsenceHistoryFilter, new EventArgs());
            #endregion

            #region Leave History
            if (this.EmpSelfServiceStorage.ContainsKey("cboMonthLeaveHistory") && this.AccessReaderList.Count > 0)
            {
                this.cboMonthLeaveHistory.SelectedValue = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["cboMonthLeaveHistory"]);
                if (!string.IsNullOrEmpty(this.cboMonthLeaveHistory.SelectedValue))
                {
                    this.cboMonthLeaveHistory_SelectedIndexChanged(this.cboMonthLeaveHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthLeaveHistory.Text, string.Empty, this.cboMonthLeaveHistory.SelectedValue, string.Empty));
                }
            }
            else
            {
                this.cboMonthLeaveHistory.Text = string.Empty;
                this.cboMonthLeaveHistory.SelectedIndex = -1;
            }

            if (this.EmpSelfServiceStorage.ContainsKey("dtpStartDateLeaveHistory"))
                this.dtpStartDateLeaveHistory.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpStartDateLeaveHistory"]);
            else
                this.dtpStartDateLeaveHistory.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("dtpEndDateLeaveHistory"))
                this.dtpEndDateLeaveHistory.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpEndDateLeaveHistory"]);
            else
                this.dtpEndDateLeaveHistory.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("txtYearLeaveHistory"))
                this.txtYearLeaveHistory.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["txtYearLeaveHistory"]);
            else
                this.txtYearLeaveHistory.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("chkPayPeriodLeaveHistory"))
                this.chkPayPeriodLeaveHistory.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkPayPeriodLeaveHistory"]);
            else
                this.chkPayPeriodLeaveHistory.Checked = false;

            this.chkPayPeriodLeaveHistory_CheckedChanged(this.chkPayPeriodLeaveHistory, new EventArgs());

            if (this.EmpSelfServiceStorage.ContainsKey("chkLeaveHistoryFilter"))
                this.chkLeaveHistoryFilter.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkLeaveHistoryFilter"]);
            else
                this.chkLeaveHistoryFilter.Checked = false;

            this.chkLeaveHistoryFilter_CheckedChanged(this.chkLeaveHistoryFilter, new EventArgs());
            #endregion

            #region Attendance History
            if (this.EmpSelfServiceStorage.ContainsKey("cboMonthAttendanceHistory") && this.AccessReaderList.Count > 0)
            {
                this.cboMonthAttendanceHistory.SelectedValue = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["cboMonthAttendanceHistory"]);
                if (!string.IsNullOrEmpty(this.cboMonthAttendanceHistory.SelectedValue))
                {
                    this.cboMonthAttendanceHistory_SelectedIndexChanged(this.cboMonthAttendanceHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAttendanceHistory.Text, string.Empty, this.cboMonthAttendanceHistory.SelectedValue, string.Empty));
                }
            }
            else
            {
                this.cboMonthAttendanceHistory.Text = string.Empty;
                this.cboMonthAttendanceHistory.SelectedIndex = -1;
            }

            if (this.EmpSelfServiceStorage.ContainsKey("dtpStartDateAttendanceHistory"))
                this.dtpStartDateAttendanceHistory.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpStartDateAttendanceHistory"]);
            else
                this.dtpStartDateAttendanceHistory.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("dtpEndDateAttendanceHistory"))
                this.dtpEndDateAttendanceHistory.SelectedDate = UIHelper.ConvertObjectToDate(this.EmpSelfServiceStorage["dtpEndDateAttendanceHistory"]);
            else
                this.dtpEndDateAttendanceHistory.SelectedDate = null;

            if (this.EmpSelfServiceStorage.ContainsKey("txtYearAttendanceHistory"))
                this.txtYearAttendanceHistory.Text = UIHelper.ConvertObjectToString(this.EmpSelfServiceStorage["txtYearAttendanceHistory"]);
            else
                this.txtYearAttendanceHistory.Text = string.Empty;

            if (this.EmpSelfServiceStorage.ContainsKey("chkPayPeriodAttendanceHistory"))
                this.chkPayPeriodAttendanceHistory.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkPayPeriodAttendanceHistory"]);
            else
                this.chkPayPeriodAttendanceHistory.Checked = false;

            this.chkPayPeriodAttendanceHistory_CheckedChanged(this.chkPayPeriodAttendanceHistory, new EventArgs());

            if (this.EmpSelfServiceStorage.ContainsKey("chkSwipeHistoryFilter"))
                this.chkSwipeHistoryFilter.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkSwipeHistoryFilter"]);
            else
                this.chkSwipeHistoryFilter.Checked = false;

            this.chkSwipeHistoryFilter_CheckedChanged(this.chkSwipeHistoryFilter, new EventArgs());

            if (this.EmpSelfServiceStorage.ContainsKey("chkAttendanceHistoryFilter"))
                this.chkAttendanceHistoryFilter.Checked = UIHelper.ConvertObjectToBolean(this.EmpSelfServiceStorage["chkAttendanceHistoryFilter"]);
            else
                this.chkAttendanceHistoryFilter.Checked = false;

            this.chkAttendanceHistoryFilter_CheckedChanged(this.chkLeaveHistoryFilter, new EventArgs());
            #endregion

            #endregion

            #region Reset the grids
            RebindAttendanceHistoryGrid();

            // Set grid attributes
            this.gridAttendanceHistory.CurrentPageIndex = this.CurrentPageIndex;
            this.gridAttendanceHistory.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridAttendanceHistory.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridAttendanceHistory.MasterTableView.DataBind();

            this.gridLeaveHistory.CurrentPageIndex = this.CurrentPageIndex;
            this.gridLeaveHistory.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridLeaveHistory.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridLeaveHistory.MasterTableView.DataBind();

            this.gridAbsenceHistory.CurrentPageIndex = this.CurrentPageIndex;
            this.gridAbsenceHistory.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridAbsenceHistory.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridAbsenceHistory.MasterTableView.DataBind();
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.EmpSelfServiceStorage.Clear();
            this.EmpSelfServiceStorage.Add("FormFlag", formFlag.ToString());

            #region Store control values to the collection
            // Employee Details
            this.EmpSelfServiceStorage.Add("txtEmpNo", this.txtEmpNo.Text);
            this.EmpSelfServiceStorage.Add("litEmployeeName", this.litEmployeeName.Text);
            this.EmpSelfServiceStorage.Add("litPosition", this.litPosition.Text);
            this.EmpSelfServiceStorage.Add("litCostCenter", this.litCostCenter.Text);
            this.EmpSelfServiceStorage.Add("litCostCenterCode", this.litCostCenterCode.Text);
            this.EmpSelfServiceStorage.Add("litJoiningDate", this.litJoiningDate.Text);
            this.EmpSelfServiceStorage.Add("litServiceYear", this.litServiceYear.Text);
            this.EmpSelfServiceStorage.Add("imgPhoto", this.imgPhoto.ImageUrl);
            this.EmpSelfServiceStorage.Add("chkShowPhoto", this.chkShowPhoto.Checked);

            // Swipes History
            this.EmpSelfServiceStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.EmpSelfServiceStorage.Add("cboLocation", this.cboLocation.SelectedValue);
            this.EmpSelfServiceStorage.Add("dtpSwipeHistorySDate", this.dtpSwipeHistorySDate.SelectedDate);
            this.EmpSelfServiceStorage.Add("dtpSwipeHistoryEDate", this.dtpSwipeHistoryEDate.SelectedDate);
            this.EmpSelfServiceStorage.Add("txtYear", this.txtYear.Text);
            this.EmpSelfServiceStorage.Add("chkPayPeriodSwipeHistory", this.chkPayPeriodSwipeHistory.Checked);
            this.EmpSelfServiceStorage.Add("chkSwipeHistoryFilter", this.chkSwipeHistoryFilter.Checked);

            // Absence History
            this.EmpSelfServiceStorage.Add("cboMonthAbsence", this.cboMonthAbsence.SelectedValue);
            this.EmpSelfServiceStorage.Add("dtpStartDateAbsence", this.dtpStartDateAbsence.SelectedDate);
            this.EmpSelfServiceStorage.Add("dtpEndDateAbsence", this.dtpEndDateAbsence.SelectedDate);
            this.EmpSelfServiceStorage.Add("txtYearAbsence", this.txtYearAbsence.Text);
            this.EmpSelfServiceStorage.Add("chkPayPeriodAbsence", this.chkPayPeriodAbsence.Checked);
            this.EmpSelfServiceStorage.Add("chkAbsenceHistoryFilter", this.chkAbsenceHistoryFilter.Checked);

            // Leave History
            this.EmpSelfServiceStorage.Add("cboMonthLeaveHistory", this.cboMonthLeaveHistory.SelectedValue);
            this.EmpSelfServiceStorage.Add("dtpStartDateLeaveHistory", this.dtpStartDateLeaveHistory.SelectedDate);
            this.EmpSelfServiceStorage.Add("dtpEndDateLeaveHistory", this.dtpEndDateLeaveHistory.SelectedDate);
            this.EmpSelfServiceStorage.Add("txtYearLeaveHistory", this.txtYearLeaveHistory.Text);
            this.EmpSelfServiceStorage.Add("chkPayPeriodLeaveHistory", this.chkPayPeriodLeaveHistory.Checked);
            this.EmpSelfServiceStorage.Add("chkLeaveHistoryFilter", this.chkLeaveHistoryFilter.Checked);

            // Attendance History
            this.EmpSelfServiceStorage.Add("cboMonthAttendanceHistory", this.cboMonthAttendanceHistory.SelectedValue);
            this.EmpSelfServiceStorage.Add("dtpStartDateAttendanceHistory", this.dtpStartDateAttendanceHistory.SelectedDate);
            this.EmpSelfServiceStorage.Add("dtpEndDateAttendanceHistory", this.dtpEndDateAttendanceHistory.SelectedDate);
            this.EmpSelfServiceStorage.Add("txtYearAttendanceHistory", this.txtYearAttendanceHistory.Text);
            this.EmpSelfServiceStorage.Add("chkPayPeriodAttendanceHistory", this.chkPayPeriodAttendanceHistory.Checked);
            this.EmpSelfServiceStorage.Add("chkAttendanceHistoryFilter", this.chkAttendanceHistoryFilter.Checked);
            #endregion

            #region Store query string values to session 
            this.EmpSelfServiceStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to the collection            
            this.EmpSelfServiceStorage.Add("CurrentUserEmployeeInfo", this.CurrentUserEmployeeInfo);
            this.EmpSelfServiceStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.EmpSelfServiceStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.EmpSelfServiceStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.EmpSelfServiceStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.EmpSelfServiceStorage.Add("SwipeHistoryList", this.SwipeHistoryList);
            this.EmpSelfServiceStorage.Add("AccessReaderList", this.AccessReaderList);
            this.EmpSelfServiceStorage.Add("AbsenceHistoryList", this.AbsenceHistoryList);
            this.EmpSelfServiceStorage.Add("LeaveHistoryList", this.LeaveHistoryList);
            this.EmpSelfServiceStorage.Add("AttendanceHistoryList", this.AttendanceHistoryList);
            this.EmpSelfServiceStorage.Add("LeaveDetailList", this.LeaveDetailList);
            this.EmpSelfServiceStorage.Add("ApprovedDILList", this.ApprovedDILList);
            this.EmpSelfServiceStorage.Add("InactiveDILList", this.InactiveDILList);
            this.EmpSelfServiceStorage.Add("CurrentEmployeeNo", this.CurrentEmployeeNo);
            this.EmpSelfServiceStorage.Add("ShiftPatternList", this.ShiftPatternList);
            this.EmpSelfServiceStorage.Add("DependentList", this.DependentList);
            this.EmpSelfServiceStorage.Add("TrainingRecordList", this.TrainingRecordList);
            this.EmpSelfServiceStorage.Add("CanAccessDependentInfo", this.CanAccessDependentInfo);
            #endregion
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            FillDataToLocationCombo(reloadFromDB);
        }

        private void LoadEmployeePhoto(string fileName, ref Image imgPhoto)
        {
            try
            {
                //BitmapImage bitmapImg = new BitmapImage();

                //using (FileStream photoStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                //{
                //    bitmapImg.BeginInit();

                //    using (BinaryReader reader = new BinaryReader(photoStream))
                //    {
                //        // Copy the content of the file into a memory stream
                //        MemoryStream memoryStream = new MemoryStream(reader.ReadBytes(Convert.ToInt32(photoStream.Length)));

                //        // Make a new Bitmap object the owner of the MemoryStream
                //        bitmapImg.StreamSource = memoryStream;
                //    };

                //    bitmapImg.EndInit();
                //    imgPhoto.Source = bitmapImg;
                //    photoStream.Dispose();
                //};
            }
            catch (Exception ex)
            {
            }
        }

        private bool LoadEmployeeInformation(int empNo)
        {
            bool result = true;

            try
            {
                #region Get the employee information
                GetEmployeeDetails(empNo);
                #endregion

                #region Get the employee photo                                
                string empPhotoPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["EmployeePhotoPath"]);                
                if (!string.IsNullOrEmpty(empPhotoPath))
                {
                    //FetchEmployeePhoto(empNo, empPhotoPath);
                    FetchEmployeePhotoURL(empNo);
                }
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
                return false;
            }
        }

        private void FetchEmployeePhotoOld(int empNo, string empPhotoPath)
        {
            try
            {
                int currentEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                if (this.chkShowPhoto.Visible ||
                    empNo == currentEmpNo)
                {
                    #region Get employee's photo if current user is allowed to do
                    bool isPhotoFound = false;
                    string imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", empPhotoPath, empNo);
                    string imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", empPhotoPath, empNo);

                    #region Begin searching for bitmap photo                                
                    if (File.Exists(imageFullPath_BMP))
                    {
                        this.imgPhoto.ImageUrl = imageFullPath_BMP;
                        isPhotoFound = true;
                    }
                    else
                    {
                        if (empNo > 10000000)
                        {
                            imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", empPhotoPath, empNo - 10000000);
                            if (File.Exists(imageFullPath_BMP))
                            {
                                this.imgPhoto.ImageUrl = imageFullPath_BMP;
                                isPhotoFound = true;
                            }
                            else
                            {
                                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                            }
                        }
                        else
                        {
                            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                        }
                    }
                    #endregion

                    if (!isPhotoFound)
                    {
                        #region Search for JPEG photo
                        if (File.Exists(imageFullPath_JPG))
                        {
                            this.imgPhoto.ImageUrl = imageFullPath_JPG;
                            isPhotoFound = true;
                        }
                        else
                        {
                            if (empNo > 10000000)
                            {
                                imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", empPhotoPath, empNo - 10000000);
                                if (File.Exists(imageFullPath_JPG))
                                {
                                    this.imgPhoto.ImageUrl = imageFullPath_JPG;
                                    isPhotoFound = true;
                                }
                                else
                                {
                                    this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                                }
                            }
                            else
                            {
                                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                    this.imgPhoto.ToolTip = UIHelper.CONST_NO_PHOTO_MESSAGE;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FetchEmployeePhotoURL(int empNo)
        {
            try
            {
                string empPhotoFolder = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["EmpPhotoVirtualFolder"]);
                int currentEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                if (this.chkShowPhoto.Visible ||
                    empNo == currentEmpNo)
                {
                    #region Get employee's photo if current user is allowed to do
                    bool isPhotoFound = false;
                    string imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", empPhotoFolder, empNo);
                    string imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", empPhotoFolder, empNo);

                    #region Begin searching for bitmap photo                                
                    if (File.Exists(Server.MapPath(imageFullPath_BMP)))
                    {
                        this.imgPhoto.ImageUrl = imageFullPath_BMP;
                        isPhotoFound = true;
                    }
                    else
                    {
                        if (empNo > 10000000)
                        {
                            imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", empPhotoFolder, empNo - 10000000);
                            if (File.Exists(Server.MapPath(imageFullPath_BMP)))
                            {
                                this.imgPhoto.ImageUrl = imageFullPath_BMP;
                                isPhotoFound = true;
                            }
                            else
                            {
                                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                            }
                        }
                        else
                        {
                            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                        }
                    }
                    #endregion

                    if (!isPhotoFound)
                    {
                        #region Search for JPEG photo
                        if (File.Exists(Server.MapPath(imageFullPath_JPG)))
                        {
                            this.imgPhoto.ImageUrl = imageFullPath_JPG;
                            isPhotoFound = true;
                        }
                        else
                        {
                            if (empNo > 10000000)
                            {
                                imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", empPhotoFolder, empNo - 10000000);
                                if (File.Exists(Server.MapPath(imageFullPath_JPG)))
                                {
                                    this.imgPhoto.ImageUrl = imageFullPath_JPG;
                                    isPhotoFound = true;
                                }
                                else
                                {
                                    this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                                }
                            }
                            else
                            {
                                this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
                    this.imgPhoto.ToolTip = UIHelper.CONST_NO_PHOTO_MESSAGE;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DisplayFormLevelError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return;

            this.CustomErrorMsg = errorMsg;
            this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
            this.ErrorType = ValidationErrorType.CustomFormError;
            this.cusValEmpNo.Validate();
        }

        private void GetPayPeriod(int year, int month, ref DateTime? startDate, ref DateTime? endDate)
        {
            try
            {
                switch (month)
                {
                    case 1:     // January
                        startDate = UIHelper.ConvertObjectToDate(string.Format("16/{0}/{1}", month + 11, year - 1));
                        endDate = UIHelper.ConvertObjectToDate(string.Format("15/{0}/{1}", month, year));
                        break;

                    case 2:     // February
                    case 3:     // March
                    case 4:     // April
                    case 5:     // May
                    case 6:     // June
                    case 7:     // July
                    case 8:     // August
                    case 9:     // September
                    case 10:    // October
                    case 11:    // November
                    case 12:    // December
                        startDate = UIHelper.ConvertObjectToDate(string.Format("16/{0}/{1}", month - 1, year));
                        endDate = UIHelper.ConvertObjectToDate(string.Format("15/{0}/{1}", month, year));
                        break;
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
            #region Reset Controls

            #region Employee Details
            this.txtEmpNo.Text = string.Empty;
            this.litEmployeeName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.litCostCenterCode.Text = string.Empty;
            this.litJoiningDate.Text = "Not defined";
            this.litServiceYear.Text = "Not defined";
            this.imgPhoto.ImageUrl = UIHelper.CONST_DEFAULT_EMP_PHOTO;
            //this.chkShowPhoto.Checked = false;
            #endregion

            #region Swipe History
            this.cboMonth.SelectedIndex = -1;
            this.cboMonth.Text = string.Empty;
            this.cboLocation.SelectedIndex = -1;
            this.cboLocation.Text = string.Empty;
            this.dtpSwipeHistorySDate.SelectedDate = null;
            this.dtpSwipeHistoryEDate.SelectedDate = null;
            this.txtYear.Text = string.Empty;

            this.chkPayPeriodSwipeHistory.Checked = true;
            this.chkPayPeriodSwipeHistory_CheckedChanged(this.chkPayPeriodSwipeHistory, new EventArgs());

            this.chkSwipeHistoryFilter.Checked = false;
            this.chkSwipeHistoryFilter_CheckedChanged(this.chkSwipeHistoryFilter, new EventArgs());
            #endregion

            #region Absence History
            this.cboMonthAbsence.SelectedIndex = -1;
            this.cboMonthAbsence.Text = string.Empty;
            this.dtpStartDateAbsence.SelectedDate = null;
            this.dtpEndDateAbsence.SelectedDate = null;
            this.txtYearAbsence.Text = string.Empty;

            this.chkPayPeriodAbsence.Checked = true;
            this.chkPayPeriodAbsence_CheckedChanged(this.chkPayPeriodAbsence, new EventArgs());

            this.chkAttendanceHistoryFilter.Checked = false;
            this.chkAttendanceHistoryFilter_CheckedChanged(this.chkAttendanceHistoryFilter, new EventArgs());
            #endregion

            #region Leave History
            this.cboMonthLeaveHistory.SelectedIndex = -1;
            this.cboMonthLeaveHistory.Text = string.Empty;
            this.dtpStartDateLeaveHistory.SelectedDate = null;
            this.dtpEndDateLeaveHistory.SelectedDate = null;
            this.txtYearLeaveHistory.Text = string.Empty;

            this.chkPayPeriodLeaveHistory.Checked = true;
            this.chkPayPeriodLeaveHistory_CheckedChanged(this.chkPayPeriodLeaveHistory, new EventArgs());

            this.chkLeaveHistoryFilter.Checked = false;
            this.chkLeaveHistoryFilter_CheckedChanged(this.chkLeaveHistoryFilter, new EventArgs());
            #endregion

            #region Attendance History
            this.cboMonthAttendanceHistory.SelectedIndex = -1;
            this.cboMonthAttendanceHistory.Text = string.Empty;
            this.dtpStartDateAttendanceHistory.SelectedDate = null;
            this.dtpEndDateAttendanceHistory.SelectedDate = null;
            this.txtYearAttendanceHistory.Text = string.Empty;

            this.chkPayPeriodAttendanceHistory.Checked = true;
            this.chkPayPeriodAttendanceHistory_CheckedChanged(this.chkPayPeriodAttendanceHistory, new EventArgs());

            this.chkAttendanceHistoryFilter.Checked = false;
            this.chkAttendanceHistoryFilter_CheckedChanged(this.chkAttendanceHistoryFilter, new EventArgs());
            #endregion

            #region Other controls
            this.panBarMain.ClearSelectedItems();
            #endregion

            #endregion

            #region Reset panels
            this.panSwipeHistoryFilter.Style[HtmlTextWriterStyle.Display] = "none";
            this.panAbsenceHistoryFilter.Style[HtmlTextWriterStyle.Display] = "none";
            this.panLeaveHistoryFilter.Style[HtmlTextWriterStyle.Display] = "none";
            this.panAttendanceHistoryFilter.Style[HtmlTextWriterStyle.Display] = "none";
            #endregion

            #region Reset datagrids
            InitializeSwipeHistoryGrid();
            InitializeAbsenceHistoryGrid();
            InitializeLeaveHistoryGrid();
            InitializeLeaveDetailGrid();
            InitializeAttendanceHistoryGrid();
            InitializeTrainingRecordGrid();
            InitializeShiftPatternGrid();
            InitializeApprovedDILGrid();
            InitializeInactiveDILGrid();
            InitializeDependentInfoGrid();

            this.gridSwipeHistory.CurrentPageIndex = 0;
            this.gridSwipeHistory.CurrentPageIndex = 0;
            this.gridLeaveDetails.CurrentPageIndex = 0;
            this.gridShiftPattern.CurrentPageIndex = 0;
            this.gridDependentInfo.CurrentPageIndex = 0;
            this.gridApprovedDIL.CurrentPageIndex = 0;
            this.gridInactiveDIL.CurrentPageIndex = 0;

            this.gridAttendanceHistory.CurrentPageIndex = 0;
            this.gridAttendanceHistory.VirtualItemCount = 1;
            this.gridTraining.CurrentPageIndex = 0;
            this.gridTraining.VirtualItemCount = 1;
            this.gridLeaveHistory.CurrentPageIndex = 0;
            this.gridLeaveHistory.VirtualItemCount = 1;
            this.gridAbsenceHistory.CurrentPageIndex = 0;
            this.gridAbsenceHistory.VirtualItemCount = 1;

            this.CurrentPageIndex = 1;
            this.CurrentPageSize = 10;
            #endregion

            #region Initialize filter criteria controls
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (DateTime.Now.Day >= 16)
            {
                month = month + 1;
            }

            if (month > 12)
            {
                month = 1;
                year = year + 1;
            }

            // Swipes History 
            this.txtYear.Text = year.ToString();
            this.cboMonth.SelectedValue = month.ToString();
            this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
            this.cboMonth.Focus();

            // Absence History 
            this.txtYearAbsence.Text = year.ToString();
            this.cboMonthAbsence.SelectedValue = month.ToString();
            this.cboMonthAbsence_SelectedIndexChanged(this.cboMonthAbsence, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAbsence.Text, string.Empty, this.cboMonthAbsence.SelectedValue, string.Empty));

            // Leave History 
            this.txtYearLeaveHistory.Text = year.ToString();
            this.cboMonthLeaveHistory.SelectedValue = month.ToString();
            this.cboMonthLeaveHistory_SelectedIndexChanged(this.cboMonthLeaveHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthLeaveHistory.Text, string.Empty, this.cboMonthLeaveHistory.SelectedValue, string.Empty));

            // Attendance History 
            this.txtYearAttendanceHistory.Text = year.ToString();
            this.cboMonthAttendanceHistory.SelectedValue = month.ToString();
            this.cboMonthAttendanceHistory_SelectedIndexChanged(this.cboMonthAttendanceHistory, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonthAttendanceHistory.Text, string.Empty, this.cboMonthAttendanceHistory.SelectedValue, string.Empty));
            #endregion

            #region Clear search filter strings
            this.lblSwipeHistorySearchString.Text = string.Empty;
            this.lblAbsenceHistorySearchString.Text = string.Empty;
            this.lblLeaveHistorySearchString.Text = string.Empty;
            this.lblAttendanceHistorySearchString.Text = string.Empty;
            #endregion
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
        }

        public void KillSessions()
        {
            // Clear collections
            this.SwipeHistoryList = null;
            this.AccessReaderList = null;
            this.AbsenceHistoryList = null;
            this.LeaveHistoryList = null;
            this.AttendanceHistoryList = null;
            this.LeaveDetailList = null;
            this.ApprovedDILList = null;
            this.InactiveDILList = null;
            this.ShiftPatternList = null;
            this.DependentList = null;
            this.TrainingRecordList = null;

            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentUserEmployeeInfo"] = null;
            ViewState["CurrentEmployeeNo"] = null;
            ViewState["CanAccessDependentInfo"] = null;

            ViewState.Clear();
        }
        #endregion

        #region Database Access
        private void GetEmployeeDetails(int empNo)
        {
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;
                int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                DALProxy proxy = new DALProxy();

                var rawData = proxy.GetEmployeeInfoFromJDE(empNo, string.Empty, null, ref error, ref innerError);
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
                        this.CurrentUserEmployeeInfo = rawData.FirstOrDefault();

                        if (empNo != currentUserEmpNo)
                        {
                            #region Check if the employee's cost center exists in the allowed cost center list
                            if (this.CurrentUserEmployeeInfo != null &&
                                this.Master.AllowedCostCenterList.Count > 0)
                            {
                                string allowedCC = this.Master.AllowedCostCenterList
                                    .Where(a => a == this.CurrentUserEmployeeInfo.CostCenter)
                                    .FirstOrDefault();
                                if (string.IsNullOrEmpty(allowedCC))
                                {
                                    // Clear session
                                    this.CurrentEmployeeNo = 0;
                                    this.CurrentUserEmployeeInfo = null;

                                    this.litEmployeeName.Text = "Access denied";
                                    this.litPosition.Text = "Access denied";
                                    this.litCostCenter.Text = "Access denied";
                                    this.litJoiningDate.Text = "Access denied";
                                    this.litServiceYear.Text = "Access denied";

                                    // Throw error
                                    throw new Exception("Sorry, you don't have access permission to view the information of the specified employee. Please contact ICT or create a Helpdesk Request to grant you cost center permission!");
                                }
                            }
                            #endregion
                        }

                        #region Bind data to controls
                        this.txtEmpNo.Text = this.CurrentUserEmployeeInfo.EmpNo.ToString();
                        this.litEmployeeName.Text = this.CurrentUserEmployeeInfo.EmpName;
                        this.litPosition.Text = this.CurrentUserEmployeeInfo.Position;
                        this.litCostCenter.Text = this.CurrentUserEmployeeInfo.CostCenterFullName;
                        this.litCostCenterCode.Text = this.CurrentUserEmployeeInfo.CostCenter;
                        this.litJoiningDate.Text = this.CurrentUserEmployeeInfo.DateJoined.HasValue
                            ? Convert.ToDateTime(this.CurrentUserEmployeeInfo.DateJoined).ToString("dd-MMM-yyyy")
                            : string.Empty;
                        this.litServiceYear.Text = string.Format("{0} year(s)", this.CurrentUserEmployeeInfo.YearsOfService.ToString());
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetSwipeHistory(DateTime? startDate, DateTime? endDate, int empNo, string costCenter = "", string locationName = "", string readerName = "", bool reloadData = true)
        {
            try
            {
                if (reloadData || this.SwipeHistoryList.Count == 0)
                {
                    // Initialize session
                    this.SwipeHistoryList = null;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    if (empNo > 0)
                    {
                        DALProxy proxy = new DALProxy();
                        var rawData = proxy.GetSwipeHistory(startDate, endDate, empNo, costCenter, locationName, readerName, ref error, ref innerError);
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
                                // Save to session
                                this.SwipeHistoryList.AddRange(rawData.OrderByDescending(a => a.SwipeDate));
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindSwipeHistoryGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillDataToLocationCombo(bool reloadFromDB = true)
        {
            try
            {
                List<AccessReaderEntity> comboSource = new List<AccessReaderEntity>();

                if (this.AccessReaderList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.AccessReaderList;
                }
                else
                {
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    var rawData = dataProxy.GetAccessReaders(0, 0, 0, ref error, ref innerError);
                    if (rawData != null && rawData.Count() > 0)
                    {
                        comboSource.AddRange(rawData.ToList());

                        #region Add blank item
                        comboSource.Insert(0, new AccessReaderEntity()
                        {
                            AutoID = 0,
                            LocationName = string.Empty,
                            ReaderName = string.Empty,
                            LocationFullName = string.Empty
                        });
                        #endregion
                    }
                }

                // Store to session
                this.AccessReaderList = comboSource;

                #region Bind data to combobox
                this.cboLocation.DataSource = this.AccessReaderList;
                this.cboLocation.DataTextField = "LocationFullName";
                this.cboLocation.DataValueField = "AutoID";
                this.cboLocation.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void GetAbsenceHistory(int empNo, DateTime? startDate = null, DateTime? endDate = null, bool reloadData = true)
        {
            try
            {
                if (reloadData || this.AbsenceHistoryList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.AbsenceHistoryList = null;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    if (empNo > 0)
                    {
                        var rawData = dataProxy.GetAbsencesHistory(empNo, startDate, endDate, ref error, ref innerError);
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
                                // Save to session
                                this.AbsenceHistoryList.AddRange(rawData.OrderByDescending(a => a.SwipeDate));
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindAbsenceHistoryGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetAbsenceHistoryV2(int empNo, DateTime? startDate = null, DateTime? endDate = null, bool reloadDataFromDB = false)
        {
            try
            {
                // Initialize record count
                this.gridAbsenceHistory.VirtualItemCount = 1;

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.AbsenceHistoryList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    var source = dataProxy.GetAbsencesHistoryv2(empNo, startDate, endDate, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError);
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (source != null && source.Count() > 0)
                        {
                            gridSource.AddRange(source);
                        }
                    }
                }

                // Store collection to session
                this.AbsenceHistoryList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.AbsenceHistoryList.Count > 0)
                {
                    int totalRecords = this.AbsenceHistoryList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridAbsenceHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridAbsenceHistory.VirtualItemCount = 1;

                    this.gridAbsenceHistory.DataSource = this.AbsenceHistoryList;
                    this.gridAbsenceHistory.DataBind();
                }
                else
                    InitializeAbsenceHistoryGrid();
                #endregion

                if (this.chkAbsenceHistoryFilter.Checked)
                {
                    #region Set the Search filter description
                    if (this.dtpStartDateAbsence.SelectedDate != null &&
                        this.dtpEndDateAbsence.SelectedDate != null)
                    {
                        this.lblAbsenceHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                            this.dtpStartDateAbsence.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                            this.dtpEndDateAbsence.SelectedDate.Value.ToString("dd-MMM-yyyy"));
                    }
                    else
                        this.lblAbsenceHistorySearchString.Text = string.Empty;
                    #endregion
                }
                else
                    this.lblAbsenceHistorySearchString.Text = string.Empty;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetLeaveHistory(int empNo, DateTime? startDate = null, DateTime? endDate = null, bool reloadData = true)
        {
            try
            {
                if (reloadData || this.LeaveHistoryList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.LeaveHistoryList = null;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    if (empNo > 0)
                    {
                        var rawData = dataProxy.GetLeaveHistory(empNo, startDate, endDate, ref error, ref innerError);
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
                                // Save to session
                                this.LeaveHistoryList.AddRange(rawData.OrderByDescending(a => a.SwipeDate));
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindLeaveHistoryGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetLeaveHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, bool reloadDataFromDB = false)
        {
            try
            {
                // Initialize record count
                this.gridLeaveHistory.VirtualItemCount = 1;

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.LeaveHistoryList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    var source = dataProxy.GetLeaveHistoryV2(empNo, startDate, endDate, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError);
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (source != null && source.Count() > 0)
                        {
                            gridSource.AddRange(source);
                        }
                    }
                }

                // Store collection to session
                this.LeaveHistoryList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.LeaveHistoryList.Count > 0)
                {
                    int totalRecords = this.LeaveHistoryList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridLeaveHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridLeaveHistory.VirtualItemCount = 1;

                    this.gridLeaveHistory.DataSource = this.LeaveHistoryList;
                    this.gridLeaveHistory.DataBind();
                }
                else
                    InitializeLeaveHistoryGrid();
                #endregion

                #region Set the Search filter description
                if (this.chkLeaveHistoryFilter.Checked)
                {
                    if (this.dtpStartDateLeaveHistory.SelectedDate != null &&
                        this.dtpEndDateLeaveHistory.SelectedDate != null)
                    {
                        this.lblLeaveHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                            this.dtpStartDateLeaveHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                            this.dtpEndDateLeaveHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"));
                    }
                    else
                        this.lblLeaveHistorySearchString.Text = string.Empty;
                }
                else
                    this.lblLeaveHistorySearchString.Text = string.Empty;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetAttendanceHistory(int empNo, DateTime? startDate = null, DateTime? endDate = null, bool reloadData = true)
        {
            try
            {
                if (reloadData || this.AttendanceHistoryList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.AttendanceHistoryList = null;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    if (empNo > 0)
                    {
                        var rawData = dataProxy.GetAttendanceHistory(empNo, startDate, endDate, ref error, ref innerError);
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
                                // Save to session
                                this.AttendanceHistoryList.AddRange(rawData.OrderByDescending(a => a.SwipeDate));
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindAttendanceHistoryGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetAttendanceHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, bool reloadDataFromDB = false)
        {
            try
            {
                // Initialize record count
                this.gridAttendanceHistory.VirtualItemCount = 1;

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.AttendanceHistoryList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    var source = dataProxy.GetAttendanceHistoryV2(empNo, startDate, endDate, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError);
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (source != null && source.Count() > 0)
                        {
                            gridSource.AddRange(source);
                        }
                    }
                }

                // Store collection to session
                this.AttendanceHistoryList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.AttendanceHistoryList.Count > 0)
                {
                    int totalRecords = this.AttendanceHistoryList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridAttendanceHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridAttendanceHistory.VirtualItemCount = 1;

                    this.gridAttendanceHistory.DataSource = this.AttendanceHistoryList;
                    this.gridAttendanceHistory.DataBind();
                }
                else
                    InitializeAttendanceHistoryGrid();
                #endregion

                #region Set the Search filter description
                if (this.dtpStartDateAttendanceHistory.SelectedDate != null &&
                    this.dtpEndDateAttendanceHistory.SelectedDate != null)
                {
                    this.lblAttendanceHistorySearchString.Text = string.Format("Period Covered: {0} to {1}",
                        this.dtpStartDateAttendanceHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"),
                        this.dtpEndDateAttendanceHistory.SelectedDate.Value.ToString("dd-MMM-yyyy"));
                }
                else
                    this.lblAttendanceHistorySearchString.Text = string.Empty;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetLeaveDetails(int empNo, bool reloadData = true)
        {
            try
            {
                if (reloadData || this.LeaveDetailList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.LeaveDetailList = null;

                    if (empNo > 0)
                    {
                        string error = string.Empty;
                        string innerError = string.Empty;

                        var rawData = dataProxy.GetLeaveDetails(empNo, ref error, ref innerError);
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
                                // Save to session
                                this.LeaveDetailList.AddRange(rawData.ToList());
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindLeaveDetailGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetDILEntitlements(DILType dilType, int empNo, DateTime? startDate, DateTime? endDate, bool reloadData = true)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                if (dilType == DILType.ApprovedDIL)
                {
                    #region Fetch Approved DIL Entitlements
                    if (reloadData || this.ApprovedDILList.Count == 0)
                    {
                        // Get WCF Instance
                        if (dataProxy == null)
                            return;

                        // Initialize session
                        this.ApprovedDILList = null;

                        if (empNo > 0)
                        {
                            var rawData = dataProxy.GetDILEntitlements(Convert.ToByte(dilType), empNo, startDate, endDate, ref error, ref innerError);
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
                                    // Save to session
                                    this.ApprovedDILList.AddRange(rawData.ToList());
                                }
                            }
                        }
                    }

                    // Bind data to the grid
                    RebindApprovedDILGrid();
                    #endregion
                }
                else
                {
                    #region Fetch Inactive DIL Entitlements
                    if (reloadData || this.InactiveDILList.Count == 0)
                    {
                        // Get WCF Instance
                        if (dataProxy == null)
                            return;

                        // Initialize session
                        this.InactiveDILList = null;

                        if (empNo > 0)
                        {
                            var rawData = dataProxy.GetDILEntitlements(Convert.ToByte(dilType), empNo, startDate, endDate, ref error, ref innerError);
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
                                    // Save to session
                                    this.InactiveDILList.AddRange(rawData.ToList());
                                }
                            }
                        }
                    }

                    // Bind data to the grid
                    RebindInactiveDILGrid();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetShiftPatternInformation(int empNo, bool reloadData = true)
        {
            try
            {
                if (reloadData || this.ShiftPatternList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.ShiftPatternList = null;

                    if (empNo > 0)
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
                            if (rawData != null && rawData.Count() > 0)
                            {
                                // Save to session
                                this.ShiftPatternList.AddRange(rawData.ToList());
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindShiftPatternGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetDependentInfo(int empNo, bool reloadData = true)
        {
            try
            {
                if (reloadData || this.DependentList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.DependentList = null;

                    if (empNo > 0)
                    {
                        string error = string.Empty;
                        string innerError = string.Empty;

                        var rawData = dataProxy.GetDependentInfo(empNo, ref error, ref innerError);
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
                                // Save to session
                                this.DependentList.AddRange(rawData.ToList());
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindDependentInfoGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetTrainingHistory(int empNo, bool reloadData = true)
        {
            try
            {                
                if (reloadData || this.TrainingRecordList.Count == 0)
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    // Initialize session
                    this.TrainingRecordList = null;

                    string error = string.Empty;
                    string innerError = string.Empty;
                    int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                    if (empNo > 0)
                    {
                        var rawData = dataProxy.GetTrainingRecord(0, empNo, string.Empty, 0, 0, string.Empty, string.Empty, string.Empty, null, null, 0, userEmpNo, 0, 
                            null, null, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                                // Save to session
                                this.TrainingRecordList.AddRange(rawData.ToList());
                            }
                        }
                    }
                }

                // Bind data to the grid
                RebindTrainingRecordGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
                
    }
}